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
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
namespace ScrapMaricopa.Scrapsource
{
    public class Webdriver_NJMiddlesex
    {
        string ChkMultiParcel = "", Block = "", Lot = "", Qual = "", LocationAddress = "", OwnerName = "", MultiParcelData = "", nodata = "", strParcelQual = "", strParcelQual1 = "", strParcelQual2 = "", strParcelLot = "", strParcelBlock = "";
        string Parcel_Id = "", Pro_Details = "", PropertyAlldata = "", Pro_Block = "", Pro_Lot = "", Pro_Qual = "", Pro_Location = "", Pro_Owner = "", Pro_District = "", Pro_Class = "", Pro_LandDesc = "", Pro_BldgDesc = "", Pro_Acrage = "", Pro_Taxes = "", Pro_YearBuilt = "";
        string Sale_Date = "", Sale_Book = "", Sale_Page = "", Sale_Price = "", Sale_Nu = "", Sale_Details = "";
        string Assessment_Year = "", Assessment_Exe = "", Assessment_Ass = "", Assessment_Proclass = "", Assessment_Details = "", Assessment_Land = "", Assessment_Imp = "", Assessment_Tot = "", District = "";
        string Taxing = "", Tax1 = "", Tax_Authority = "", TaxParcelBlock = "", TaxParcelLot = "", TaxParcelQual = "";
        string[] certinumber;
        string[] saledate;
        string[] LienHolder;
        string[] saleAmount;
        string[] chargetypes;
        string[] chargetypes1;
        string[] chargetypes2;
        string[] yearinsales;
        string[] subsequentcharge;
        IList<IWebElement> tables5;
        string HrefMulti = "";
        List<string> listutility = new List<string>();
        IWebDriver driver;
        string urlCurrentBill = "";
        int maxCheck1 = 0;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        string msg;
        IWebElement addclick;
        string countyname = "";
        string Pid;
        int id;
        string districtID = "";
        int addresscount = 0;
        string filename = "";
        string[] ordinance;
        string[] Description;
        string[] LastPayment;
        string[] LevyDate;
        string[] LevyAmount;
        string[] TotalNumber;
        string[] PrincipalBalance;
        string[] PayoffAmount;
        string[] NumberYears;
        string[] NextInstallment;
        string[] NextDue;
        string[] Status;
        string district = "";
        public string FTP_NJMiddlesex(string address, string assessment_id, string parcelNumber, string searchType, string orderNumber, string directParcel, string ownername, string countynameNJ, string statecountyid, string township, string townshipcode)
        {
            var distirctCode_Notax = new List<string> { "0715", "1504", "1505", "1509", "1513", "1514", "1520", "1522", "1523", "1531", "1532", "1402", "1408", "1411", "1428", "1203", "1208", "1213", "1215", "0202", "0206", "0207", "0209", "0211", "0215", "0218", "0221", "0222", "0224", "0225", "0226", "0228", "0232", "0234", "0235", "0237", "0240", "0245", "0249", "0252", "0254", "0262", "0264", "0265", "0314", "0325", "0328", "0336", "0339", "0340", "1815", "1819", "0402", "0407", "0410", "0419", "0420", "0421", "0427", "0429", "0432", "0433", "0437", "0901", "0902", "0903", "0904", "0910", "0911", "1302", "1305", "1306", "1315", "1323", "1346", "1335", "1341", "1345", "1347", "1349", "2004", "2007", "2015", "2021", "1504", "1505", "1509", "1514", "1520", "1522", "1523", "1531", "1532" };
            int countC1 = 0;
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            int duecount = 0;
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            // driver = new ChromeDriver();
            //driver = new PhantomJSDriver()

            using (driver = new PhantomJSDriver())//
            {
                try
                {
                    if (searchType == "titleflex")
                    {

                        gc.TitleFlexSearch(orderNumber, parcelNumber, ownername, "", "NJ", countynameNJ);

                        if (HttpContext.Current.Session["TitleFlex_Search" + countynameNJ] != null && HttpContext.Current.Session["TitleFlex_Search" + countynameNJ].ToString() == "Yes")
                        {
                            return "MultiParcel";
                        }
                        searchType = "parcel";
                        parcelNumber = GlobalClass.global_parcelNo;
                    }

                    if (searchType == "address")
                    {
                        if (statecountyid == "229")//Ocean
                        {
                            driver.Navigate().GoToUrl("https://www.tax.co.ocean.nj.us/TaxBoardTaxListSearch.aspx ");
                            Thread.Sleep(2000);
                            countyname = countynameNJ.ToUpper();
                            driver.FindElement(By.Id("cmbDistrict")).SendKeys(township);
                            Thread.Sleep(2000);
                            driver.FindElement(By.Id("txtStreet")).SendKeys(address.Trim());
                            gc.CreatePdf_WOP(orderNumber, "Address Search Before", driver, "NJ", countynameNJ);
                            driver.FindElement(By.Id("btnSearch")).Click();
                            Thread.Sleep(2000);
                            try
                            {
                                string Nodatafound = driver.FindElement(By.Id("lblMessage")).Text;
                                if (Nodatafound.Contains("No matching records"))
                                {
                                    HttpContext.Current.Session["Zero_NJ" + countynameNJ] = "Zero";
                                    driver.Quit();
                                    return "No Data Found";
                                }
                            }

                            catch { }
                            gc.CreatePdf_WOP(orderNumber, "Address Search After", driver, "NJ", countynameNJ);
                            int Max = 0;
                            Pid = db.ExecuteScalar("SELECT id FROM data_field_master where State_County_ID='" + statecountyid + "'and Category_Id='7'");
                            id = Convert.ToInt32(Pid);
                            IWebElement multiaddress = driver.FindElement(By.Id("m_DataTable"));
                            IList<IWebElement> TRmultiaddress = multiaddress.FindElements(By.TagName("tr"));
                            IList<IWebElement> TDmultiaddress;
                            foreach (IWebElement Multiparcel in TRmultiaddress)
                            {
                                TDmultiaddress = Multiparcel.FindElements(By.TagName("td"));
                                if (TDmultiaddress.Count() > 1 && !Multiparcel.Text.Contains("LOCATION"))
                                {
                                    IWebElement Plink = TDmultiaddress[0].FindElement(By.TagName("a"));
                                    HrefMulti = Plink.GetAttribute("href");
                                    string Muni = TDmultiaddress[1].Text;
                                    string Block = TDmultiaddress[2].Text;
                                    string Lot = TDmultiaddress[3].Text;
                                    string Qual = TDmultiaddress[4].Text;
                                    string Owner = TDmultiaddress[5].Text;
                                    string Location = TDmultiaddress[6].Text;
                                    string Propcls = TDmultiaddress[7].Text;
                                    Max++;
                                    string ParcelMul = Block.Trim() + "/" + Lot.Trim() + "/" + Qual.Trim();
                                    string MultiParcelData = Muni + "~" + Owner + "~" + Location + "~" + Propcls;
                                    gc.insert_date(orderNumber, ParcelMul, id, MultiParcelData, 1, DateTime.Now);
                                }
                            }
                            if (Max == 1)
                            {
                                driver.Navigate().GoToUrl(HrefMulti);
                                Thread.Sleep(2000);
                            }
                            if (Max > 1 && Max < 26)
                            {
                                HttpContext.Current.Session["multiParcel_Multicount_NJ" + countynameNJ] = "Maximum";
                                driver.Quit();
                                return "MultiParcel";
                            }
                            if (Max > 25)
                            {
                                HttpContext.Current.Session["multiparcel_NJ" + countynameNJ] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }

                        }
                        else
                        {
                            driver.Navigate().GoToUrl("http://tax1.co.monmouth.nj.us/cgi-bin/prc6.cgi?district=&ms_user=monm");
                            Thread.Sleep(2000);
                            countyname = countynameNJ.ToUpper();
                            var SelectAddress = driver.FindElement(By.Name("select_cc"));
                            var SelectAddressTax = new SelectElement(SelectAddress);
                            SelectAddressTax.SelectByText(countyname);
                            if (township == "--select--")
                            {
                                var SelectAddressAll = driver.FindElement(By.Name("district"));
                                var SelectAddressTaxAll = new SelectElement(SelectAddressAll);
                                SelectAddressTaxAll.SelectByText("ALL");
                            }
                            else
                            {
                                var SelectAddressAll = driver.FindElement(By.Name("district"));
                                var SelectAddressTaxAll = new SelectElement(SelectAddressAll);
                                SelectAddressTaxAll.SelectByText(township.ToUpper().Trim());
                            }

                            driver.FindElement(By.XPath("//*[@id='normdiv']/form/table[2]/tbody/tr[2]/td[2]/input")).SendKeys(address);

                            gc.CreatePdf_WOP(orderNumber, "Address search", driver, "NJ", countynameNJ);
                            driver.FindElement(By.XPath("//*[@id='normdiv']/form/table[3]/tbody/tr[2]/td[2]/input[1]")).SendKeys(Keys.Enter);
                            Thread.Sleep(2000);
                            gc.CreatePdf_WOP(orderNumber, "Single Address Search", driver, "NJ", countynameNJ);


                            //MultiParcel
                            try
                            {
                                //No Data Found
                                nodata = driver.FindElement(By.XPath("/html/body/form/b")).Text;
                                IWebElement MultiParcelTable = driver.FindElement(By.XPath("/html/body/form/table/tbody"));
                                IList<IWebElement> MultiParcelTR = MultiParcelTable.FindElements(By.TagName("tr"));
                                if (MultiParcelTR.Count() == 1)
                                {
                                    HttpContext.Current.Session["Zero_NJ" + countynameNJ] = "Zero";
                                    driver.Quit();
                                    return "No Data Found";
                                }
                            }
                            catch { }
                            int count = multiparcel(township, address, orderNumber, countynameNJ, statecountyid);
                            if (count > 1)
                            {
                                driver.Quit();
                                return "MultiParcel";
                            }

                        }
                    }

                    if (searchType == "parcel")
                    {
                        if (statecountyid == "229")
                        {
                            driver.Navigate().GoToUrl("https://www.tax.co.ocean.nj.us/TaxBoardTaxListSearch.aspx ");
                            Thread.Sleep(2000);
                            string Parcelsplit3 = "";
                            countyname = countynameNJ.ToUpper();
                            driver.FindElement(By.Id("cmbDistrict")).SendKeys(township);
                            Thread.Sleep(2000);
                            string[] splitparcel = parcelNumber.Split('/');
                            if (splitparcel[2].Trim() == "n")
                            {
                                Parcelsplit3 = "";
                            }
                            else
                            {
                                Parcelsplit3 = splitparcel[2].Trim();
                            }
                            driver.FindElement(By.Id("txtBlock")).SendKeys(splitparcel[0].Trim());
                            driver.FindElement(By.Id("txtLot")).SendKeys(splitparcel[1].Trim());
                            driver.FindElement(By.Id("txtQual")).SendKeys(Parcelsplit3.Trim());
                            driver.FindElement(By.Id("btnSearch")).Click();
                            Thread.Sleep(2000);
                            IWebElement multiaddress = driver.FindElement(By.Id("m_DataTable"));
                            IList<IWebElement> TRmultiaddress = multiaddress.FindElements(By.TagName("tr"));
                            IList<IWebElement> TDmultiaddress;
                            foreach (IWebElement Multiparcel in TRmultiaddress)
                            {
                                TDmultiaddress = Multiparcel.FindElements(By.TagName("td"));
                                if (TDmultiaddress.Count() > 1 && !Multiparcel.Text.Contains("LOCATION"))
                                {
                                    IWebElement Plink = TDmultiaddress[0].FindElement(By.TagName("a"));
                                    HrefMulti = Plink.GetAttribute("href");
                                    driver.Navigate().GoToUrl(HrefMulti);
                                    Thread.Sleep(2000);
                                }
                            }
                        }
                        else
                        {


                            if (townshipcode != "")
                            {
                                township = db.ExecuteScalar("SELECT Township FROM tbl_njtownshipmaster where State_County_Id='" + statecountyid + "'and Township_Code='" + townshipcode + "'");
                            }
                            driver.Navigate().GoToUrl("http://tax1.co.monmouth.nj.us/cgi-bin/prc6.cgi?district=&ms_user=monm");
                            Thread.Sleep(2000);
                            countyname = countynameNJ.ToUpper();
                            var SelectAddress = driver.FindElement(By.Name("select_cc"));
                            var SelectAddressTax = new SelectElement(SelectAddress);
                            SelectAddressTax.SelectByText(countyname);

                            if (township == "--select--")
                            {
                                var SelectAddressAll = driver.FindElement(By.Name("district"));
                                var SelectAddressTaxAll = new SelectElement(SelectAddressAll);
                                SelectAddressTaxAll.SelectByText("ALL");
                            }
                            else
                            {
                                var SelectAddressAll = driver.FindElement(By.Name("district"));
                                var SelectAddressTaxAll = new SelectElement(SelectAddressAll);
                                SelectAddressTaxAll.SelectByText(township.ToUpper().Trim());
                            }

                            string[] parcelSplit = parcelNumber.Split('-');
                            if (parcelSplit.Count() == 2)
                            {
                                strParcelBlock = parcelSplit[0];
                                strParcelLot = parcelSplit[1];
                            }
                            if (parcelSplit.Count() == 3)
                            {
                                strParcelBlock = parcelSplit[0];
                                strParcelLot = parcelSplit[1];
                                strParcelQual = parcelSplit[2];
                            }
                            if (parcelSplit.Count() == 4)
                            {
                                strParcelBlock = parcelSplit[0];
                                strParcelLot = parcelSplit[1];
                                strParcelQual1 = parcelSplit[2];
                                strParcelQual2 = parcelSplit[3];
                                strParcelQual = strParcelQual1.Trim() + "-" + strParcelQual2.Trim();
                            }

                            driver.FindElement(By.XPath("//*[@id='normdiv']/form/table[2]/tbody/tr[4]/td[2]/input")).SendKeys(strParcelBlock);
                            driver.FindElement(By.XPath("//*[@id='normdiv']/form/table[2]/tbody/tr[4]/td[4]/input")).SendKeys(strParcelLot);
                            driver.FindElement(By.XPath("//*[@id='normdiv']/form/table[2]/tbody/tr[4]/td[6]/input")).SendKeys(strParcelQual);

                            gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search", driver, "NJ", countynameNJ);
                            driver.FindElement(By.XPath("//*[@id='normdiv']/form/table[3]/tbody/tr[2]/td[2]/input[1]")).SendKeys(Keys.Enter);
                            Thread.Sleep(2000);
                            gc.CreatePdf(orderNumber, parcelNumber, "Single Parcel Search", driver, "NJ", countynameNJ);
                            try
                            {
                                //No Data Found
                                nodata = driver.FindElement(By.XPath("/html/body/form/b")).Text;
                                IWebElement MultiParcelTable = driver.FindElement(By.XPath("/html/body/form/table/tbody"));
                                IList<IWebElement> MultiParcelTR = MultiParcelTable.FindElements(By.TagName("tr"));
                                if (MultiParcelTR.Count() == 1)
                                {
                                    HttpContext.Current.Session["Zero_NJ" + countynameNJ] = "Zero";
                                    driver.Quit();
                                    return "No Data Found";
                                }
                            }
                            catch { }
                            string district1 = "";
                            ChkMultiParcel = driver.FindElement(By.XPath("/html/body/form/b")).Text.Replace("\r\n", "");
                            if (ChkMultiParcel.Contains("1 Records Found"))
                            {
                                driver.FindElement(By.XPath("/html/body/form/table/tbody/tr[2]/td[1]/a")).Click();
                                Thread.Sleep(2000);
                            }

                            else
                            {
                                int iparcel = 0;
                                if (township == "--select--")
                                {
                                    IWebElement MultiParcelTable = driver.FindElement(By.XPath("/html/body/form/table/tbody"));
                                    IList<IWebElement> MultiParcelTR = MultiParcelTable.FindElements(By.TagName("tr"));
                                    IList<IWebElement> MultiParcelTD;

                                    int maxCheck = 0;

                                    foreach (IWebElement multi in MultiParcelTR)
                                    {

                                        MultiParcelTD = multi.FindElements(By.TagName("td"));
                                        if (MultiParcelTD.Count != 0 && !multi.Text.Contains("Location"))
                                        {

                                            if (!distirctCode_Notax.Exists(x => string.Equals(x, MultiParcelTD[1].Text, StringComparison.OrdinalIgnoreCase)))
                                            {
                                                district1 = MultiParcelTD[1].Text;
                                                Block = MultiParcelTD[2].Text;
                                                Lot = MultiParcelTD[3].Text;
                                                Qual = MultiParcelTD[4].Text;
                                                LocationAddress = MultiParcelTD[5].Text;

                                                OwnerName = MultiParcelTD[6].Text;

                                                if (strParcelBlock == Block && strParcelLot == Lot && strParcelQual == Qual)
                                                {
                                                    addclick = MultiParcelTD[0].FindElement(By.TagName("a"));
                                                    iparcel++;
                                                }
                                                parcelNumber = Block + "-" + Lot + "-" + Qual;
                                                MultiParcelData = district1 + "~" + LocationAddress + "~" + OwnerName;
                                                Pid = db.ExecuteScalar("SELECT id FROM data_field_master where State_County_ID='" + statecountyid + "'and Category_Id='7'");
                                                id = Convert.ToInt32(Pid);
                                                gc.insert_date(orderNumber, parcelNumber, id, MultiParcelData, 1, DateTime.Now);
                                                maxCheck++;
                                            }
                                        }
                                    }
                                    if (iparcel == 1)
                                    {
                                        addclick.Click();
                                    }
                                    else
                                    {
                                        HttpContext.Current.Session["multiparcel_NJ" + countynameNJ] = "Yes";
                                        driver.Quit();
                                        return "MultiParcel";
                                    }
                                }
                                else
                                {
                                    districtID = db.ExecuteScalar("SELECT Township_Code FROM tbl_njtownshipmaster where State_County_Id='" + statecountyid + "'and Township='" + township + "'");

                                    IWebElement MultiParcelTable = driver.FindElement(By.XPath("/html/body/form/table/tbody"));
                                    IList<IWebElement> MultiParcelTR = MultiParcelTable.FindElements(By.TagName("tr"));
                                    IList<IWebElement> MultiParcelTD;
                                    if (MultiParcelTR.Count > 2)
                                    {
                                        db.ExecuteQuery("delete from data_value_master where Data_Field_Text_Id = (select id from data_field_master where Category_Id = 7 and State_County_ID ='" + statecountyid + "') and order_no='" + orderNumber + "'");
                                    }
                                    int maxCheck = 0;

                                    foreach (IWebElement multi in MultiParcelTR)
                                    {

                                        MultiParcelTD = multi.FindElements(By.TagName("td"));
                                        if (MultiParcelTD.Count != 0 && !multi.Text.Contains("Location"))
                                        {

                                            Block = MultiParcelTD[1].Text.Trim();
                                            Lot = MultiParcelTD[2].Text.Trim();
                                            Qual = MultiParcelTD[3].Text.Trim();
                                            LocationAddress = MultiParcelTD[5].Text;

                                            OwnerName = MultiParcelTD[6].Text;
                                            if (strParcelBlock == Block && strParcelLot == Lot && strParcelQual == Qual)
                                            {
                                                addclick = MultiParcelTD[0].FindElement(By.TagName("a"));
                                                iparcel++;
                                            }
                                            parcelNumber = Block + "-" + Lot + "-" + Qual;
                                            MultiParcelData = districtID + "~" + LocationAddress + "~" + OwnerName;
                                            Pid = db.ExecuteScalar("SELECT id FROM data_field_master where State_County_ID='" + statecountyid + "'and Category_Id='7'");
                                            id = Convert.ToInt32(Pid);
                                            gc.insert_date(orderNumber, parcelNumber, id, MultiParcelData, 1, DateTime.Now);
                                            maxCheck++;
                                        }

                                    }
                                    if (iparcel == 1)
                                    {
                                        addclick.Click();
                                    }
                                    else
                                    {
                                        HttpContext.Current.Session["multiparcel_NJ" + countynameNJ] = "Yes";
                                        driver.Quit();
                                        return "MultiParcel";
                                    }
                                }

                            }

                            //multiparcel(township, address, orderNumber, countynameNJ, statecountyid);                                            
                        }
                    }
                    if (searchType == "ownername")
                    {
                        if (statecountyid == "229")//Ocean
                        {
                            //Ownername

                            driver.Navigate().GoToUrl("https://www.tax.co.ocean.nj.us/TaxBoardTaxListSearch.aspx ");
                            Thread.Sleep(2000);
                            countyname = countynameNJ.ToUpper();
                            driver.FindElement(By.Id("cmbDistrict")).SendKeys(township);
                            Thread.Sleep(2000);
                            driver.FindElement(By.Id("txtOwner")).SendKeys(ownername);
                            gc.CreatePdf_WOP(orderNumber, "Owner Search Before", driver, "NJ", countynameNJ);
                            driver.FindElement(By.Id("btnSearch")).Click();
                            Thread.Sleep(2000);
                            try
                            {
                                string Nodatafound = driver.FindElement(By.Id("lblMessage")).Text;
                                if (Nodatafound.Contains("No matching records"))
                                {
                                    HttpContext.Current.Session["Zero_NJ" + countynameNJ] = "Zero";
                                    driver.Quit();
                                    return "No Data Found";
                                }
                            }
                            catch { }
                            gc.CreatePdf_WOP(orderNumber, "Owner Search After", driver, "NJ", countynameNJ);
                            int Max = 0;
                            Pid = db.ExecuteScalar("SELECT id FROM data_field_master where State_County_ID='" + statecountyid + "'and Category_Id='7'");
                            id = Convert.ToInt32(Pid);
                            IWebElement multiaddress = driver.FindElement(By.Id("m_DataTable"));
                            IList<IWebElement> TRmultiaddress = multiaddress.FindElements(By.TagName("tr"));
                            IList<IWebElement> TDmultiaddress;
                            foreach (IWebElement Multiparcel in TRmultiaddress)
                            {
                                TDmultiaddress = Multiparcel.FindElements(By.TagName("td"));
                                if (TDmultiaddress.Count() > 1 && !Multiparcel.Text.Contains("LOCATION"))
                                {
                                    IWebElement Plink = TDmultiaddress[0].FindElement(By.TagName("a"));
                                    HrefMulti = Plink.GetAttribute("href");
                                    string Muni = TDmultiaddress[1].Text;
                                    string Block = TDmultiaddress[2].Text;
                                    string Lot = TDmultiaddress[3].Text;
                                    string Qual = TDmultiaddress[4].Text;
                                    string Owner = TDmultiaddress[5].Text;
                                    string Location = TDmultiaddress[6].Text;
                                    string Propcls = TDmultiaddress[7].Text;
                                    Max++;
                                    if (Qual.Contains("n/a"))
                                    {
                                        Qual = "";
                                    }
                                    string ParcelMul = Block.Trim() + "/" + Lot.Trim() + "/" + Qual.Trim();
                                    string MultiParcelData = Muni + "~" + Owner + "~" + Location + "~" + Propcls;
                                    gc.insert_date(orderNumber, ParcelMul, id, MultiParcelData, 1, DateTime.Now);
                                }
                            }
                            if (Max == 1)
                            {
                                driver.Navigate().GoToUrl(HrefMulti);
                                Thread.Sleep(2000);
                            }
                            if (Max > 1 && Max < 26)
                            {
                                HttpContext.Current.Session["multiParcel_Multicount_NJ" + countynameNJ] = "Maximum";
                                driver.Quit();
                                return "MultiParcel";
                            }
                            if (Max > 25)
                            {
                                HttpContext.Current.Session["multiparcel_NJ" + countynameNJ] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }

                        }
                        else
                        {
                            driver.Navigate().GoToUrl("http://tax1.co.monmouth.nj.us/cgi-bin/prc6.cgi?district=&ms_user=monm");
                            Thread.Sleep(2000);
                            countyname = countynameNJ.ToUpper();
                            var SelectAddress = driver.FindElement(By.Name("select_cc"));
                            var SelectAddressTax = new SelectElement(SelectAddress);
                            SelectAddressTax.SelectByText(countyname);

                            if (township == "--select--")
                            {
                                var SelectAddressAll = driver.FindElement(By.Name("district"));
                                var SelectAddressTaxAll = new SelectElement(SelectAddressAll);
                                SelectAddressTaxAll.SelectByText("ALL");
                            }
                            else
                            {
                                //driver.FindElement(By.Name("district")).SendKeys(district.ToUpper().Trim()+Keys.Enter);
                                var SelectAddressAll = driver.FindElement(By.Name("district"));
                                var SelectAddressTaxAll = new SelectElement(SelectAddressAll);
                                SelectAddressTaxAll.SelectByText(township.ToUpper().Trim());


                            }

                            driver.FindElement(By.XPath("//*[@id='normdiv']/form/table[2]/tbody/tr[3]/td[2]/input")).SendKeys(ownername);

                            gc.CreatePdf_WOP(orderNumber, "Owner search", driver, "NJ", countynameNJ);
                            driver.FindElement(By.XPath("//*[@id='normdiv']/form/table[3]/tbody/tr[2]/td[2]/input[1]")).SendKeys(Keys.Enter);
                            Thread.Sleep(2000);
                            gc.CreatePdf_WOP(orderNumber, "Single Owner Search", driver, "NJ", countynameNJ);
                            //MultiParcel
                            try
                            {
                                //No Data Found
                                nodata = driver.FindElement(By.XPath("/html/body/form/b")).Text;
                                IWebElement MultiParcelTable = driver.FindElement(By.XPath("/html/body/form/table/tbody"));
                                IList<IWebElement> MultiParcelTR = MultiParcelTable.FindElements(By.TagName("tr"));
                                if (MultiParcelTR.Count() == 1)
                                {
                                    HttpContext.Current.Session["Zero_NJ" + countynameNJ] = "Zero";
                                    driver.Quit();
                                    return "No Data Found";
                                }
                            }
                            catch { }

                            ChkMultiParcel = driver.FindElement(By.XPath("/html/body/form/b")).Text.Replace("\r\n", "");
                            int Tdcount = driver.FindElements(By.XPath("/html/body/form/table/tbody/tr")).Count();

                            if (Tdcount <= 2)
                            {
                                driver.FindElement(By.XPath("/html/body/form/table/tbody/tr[2]/td[1]/a")).Click();
                                Thread.Sleep(2000);
                            }
                            //ChkMultiParcel = driver.FindElement(By.XPath("/html/body/form/b")).Text.Replace("\r\n", "");

                            //if (ChkMultiParcel.Contains("1 Records Found"))
                            //{
                            //    driver.FindElement(By.XPath("/html/body/form/table/tbody/tr[2]/td[1]/a")).Click();
                            //    Thread.Sleep(2000);
                            //}

                            else
                            {
                                if (township == "--select--")
                                {
                                    IWebElement MultiParcelTable = driver.FindElement(By.XPath("/html/body/form/table/tbody"));
                                    IList<IWebElement> MultiParcelTR = MultiParcelTable.FindElements(By.TagName("tr"));
                                    IList<IWebElement> MultiParcelTD;

                                    int maxCheck = 0;

                                    foreach (IWebElement multi in MultiParcelTR)
                                    {
                                        if (maxCheck <= 25)
                                        {
                                            MultiParcelTD = multi.FindElements(By.TagName("td"));
                                            if (MultiParcelTD.Count != 0 && !multi.Text.Contains("Location"))
                                            {
                                                string District1 = MultiParcelTD[1].Text;
                                                Block = MultiParcelTD[2].Text;
                                                Lot = MultiParcelTD[3].Text;
                                                Qual = MultiParcelTD[4].Text;
                                                LocationAddress = MultiParcelTD[5].Text;
                                                OwnerName = MultiParcelTD[6].Text;

                                                parcelNumber = Block + "-" + Lot + "-" + Qual;
                                                MultiParcelData = District1 + "~" + LocationAddress + "~" + OwnerName;
                                                Pid = db.ExecuteScalar("SELECT id FROM data_field_master where State_County_ID='" + statecountyid + "'and Category_Id='7'");
                                                id = Convert.ToInt32(Pid);

                                                gc.insert_date(orderNumber, parcelNumber, id, MultiParcelData, 1, DateTime.Now);
                                            }
                                            maxCheck++;
                                        }
                                    }

                                    gc.CreatePdf_WOP(orderNumber, "Multi Owner Search", driver, "NJ", countynameNJ);

                                    if (MultiParcelTR.Count > 25)
                                    {
                                        HttpContext.Current.Session["multiParcel_Multicount_NJ" + countynameNJ] = "Maximum";
                                    }
                                    else
                                    {
                                        HttpContext.Current.Session["multiparcel_NJ" + countynameNJ] = "Yes";
                                    }
                                    driver.Quit();
                                    return "MultiParcel";
                                }
                                else
                                {
                                    districtID = db.ExecuteScalar("SELECT Township_Code FROM tbl_njtownshipmaster where State_County_Id='" + statecountyid + "'and Township='" + township + "'");

                                    IWebElement MultiParcelTable = driver.FindElement(By.XPath("/html/body/form/table/tbody"));
                                    IList<IWebElement> MultiParcelTR = MultiParcelTable.FindElements(By.TagName("tr"));
                                    IList<IWebElement> MultiParcelTD;

                                    int maxCheck = 0;

                                    foreach (IWebElement multi in MultiParcelTR)
                                    {
                                        if (maxCheck <= 25)
                                        {
                                            MultiParcelTD = multi.FindElements(By.TagName("td"));
                                            if (MultiParcelTD.Count != 0 && !multi.Text.Contains("Location"))
                                            {
                                                // string District1 = township;
                                                Block = MultiParcelTD[1].Text;
                                                Lot = MultiParcelTD[2].Text;
                                                Qual = MultiParcelTD[3].Text;
                                                LocationAddress = MultiParcelTD[5].Text;
                                                OwnerName = MultiParcelTD[6].Text;

                                                parcelNumber = Block + "-" + Lot + "-" + Qual;
                                                MultiParcelData = districtID + "~" + LocationAddress + "~" + OwnerName;
                                                Pid = db.ExecuteScalar("SELECT id FROM data_field_master where State_County_ID='" + statecountyid + "'and Category_Id='7'");
                                                id = Convert.ToInt32(Pid);

                                                gc.insert_date(orderNumber, parcelNumber, id, MultiParcelData, 1, DateTime.Now);
                                            }
                                            maxCheck++;
                                        }
                                    }

                                    gc.CreatePdf_WOP(orderNumber, "Multi Owner Search", driver, "NJ", countynameNJ);

                                    if (MultiParcelTR.Count > 25)
                                    {
                                        HttpContext.Current.Session["multiParcel_Multicount_NJ" + countynameNJ] = "Maximum";
                                    }
                                    else
                                    {
                                        HttpContext.Current.Session["multiparcel_NJ" + countynameNJ] = "Yes";
                                    }
                                    driver.Quit();
                                    return "MultiParcel";
                                }
                            }
                        }
                    }

                    //Property Details
                    if (statecountyid == "229")  //ocean
                    {
                        District = db.ExecuteScalar("SELECT Township_Code FROM tbl_njtownshipmaster where State_County_Id='" + statecountyid + "'and Township='" + township + "'");
                        if (distirctCode_Notax.Exists(x => string.Equals(x, District, StringComparison.OrdinalIgnoreCase)))
                        {
                            HttpContext.Current.Session["NoTax_NJ" + countynameNJ] = "No_Tax";
                            driver.Quit();
                            return "Taxes Not Available";
                        }
                        //Property

                        string Propertydetailtable = driver.FindElement(By.Id("MOD4Table")).Text;
                        string Municipality = gc.Between(Propertydetailtable, "Municipality:", "Deed date:");
                        string Ownername = gc.Between(Propertydetailtable, "Owner:", "Block:");
                        Pro_Block = gc.Between(Propertydetailtable, "Block:", "Mailing address:").Trim();
                        string Mailingaddress = gc.Between(Propertydetailtable, "Mailing address:", "Lot:");
                        Pro_Lot = gc.Between(Propertydetailtable, "Lot:", "City/State:").Trim();
                        string City_State = gc.Between(Propertydetailtable, "City/State:", "Qual:");
                        Pro_Qual = gc.Between(Propertydetailtable, "Qual:", "Location:").Trim();
                        if (Pro_Qual.Contains("n/a"))
                        {
                            Pro_Qual = "";
                        }
                        string LocationP = gc.Between(Propertydetailtable, "Location:", "Prop class:").Trim();
                        string Propclass = gc.Between(Propertydetailtable, "Prop class:", "Land val:");
                        string Landval = gc.Between(Propertydetailtable, "Land val:", "Bldg desc:");
                        string Bldgdesc = gc.Between(Propertydetailtable, "Bldg desc:", "Improvement val:");
                        string Improvementval = gc.Between(Propertydetailtable, "Improvement val:", "Land desc:");
                        string Landdesc = gc.Between(Propertydetailtable, "Land desc:", "Exemption 1:");
                        string Exemption1 = gc.Between(Propertydetailtable, "Exemption 1:", "Addtl lots:");
                        string Exemption2 = gc.Between(Propertydetailtable, "Exemption 2:", "Zone:");
                        string Exemption3 = gc.Between(Propertydetailtable, "Exemption 3:", "Map:");
                        string Exemption4 = gc.Between(Propertydetailtable, "Exemption 4:", "Year blt:");
                        string Yearbuilt = gc.Between(Propertydetailtable, "Year blt:", "Net value:");
                        Parcel_Id = Pro_Block + "/" + Pro_Lot + "/" + Pro_Qual;
                        string Propertyresult = Municipality + "~" + Ownername + "~" + Mailingaddress + "~" + City_State + "~" + LocationP + "~" + Propclass + "~" + Bldgdesc + "~" + Yearbuilt + "~" + Exemption1 + "~" + Exemption2 + "~" + Exemption3 + "~" + Exemption4;
                        Pid = db.ExecuteScalar("SELECT id FROM data_field_master where State_County_ID='" + statecountyid + "'and Category_Id='1'");
                        id = Convert.ToInt32(Pid);
                        // gc.insert_date(orderNumber, Parcel_Id, 2249, Propertyresult, 1, DateTime.Now);
                        gc.insert_date(orderNumber, Parcel_Id, id, Propertyresult, 1, DateTime.Now);
                        Pid = db.ExecuteScalar("SELECT id FROM data_field_master where State_County_ID='" + statecountyid + "'and Category_Id='2'");
                        id = Convert.ToInt32(Pid);
                        IWebElement Assessmenttable = driver.FindElement(By.Id("AssmtHistTable"));
                        IList<IWebElement> TRAssessment = Assessmenttable.FindElements(By.TagName("tr"));
                        //IList<IWebElement> THOwnershipHistory = OwnershipHistory.FindElements(By.TagName("th"));
                        IList<IWebElement> TDAssessment;
                        foreach (IWebElement Assessment in TRAssessment)
                        {
                            TDAssessment = Assessment.FindElements(By.TagName("td"));
                            if (TDAssessment.Count > 1 && !Assessment.Text.Contains("Year"))
                            {
                                string AssessmentDetails = TDAssessment[0].Text + "~" + TDAssessment[1].Text + "~" + TDAssessment[2].Text + "~" + TDAssessment[3].Text + "~" + TDAssessment[4].Text;
                                // gc.insert_date(orderNumber, Parcel_Id, 2250, AssessmentDetails, 1, DateTime.Now);
                                gc.insert_date(orderNumber, Parcel_Id, id, AssessmentDetails, 1, DateTime.Now);
                            }
                        }
                    }
                    else
                    {
                        try
                        {


                            //Property Details
                            PropertyAlldata = driver.FindElement(By.XPath("/html/body/table[1]/tbody")).Text;
                            Pro_Block = gc.Between(PropertyAlldata, "Block: ", " Prop Loc:");
                            Pro_Lot = gc.Between(PropertyAlldata, "Lot: ", " District:");
                            Pro_Qual = gc.Between(PropertyAlldata, "Qual:", "Class:").Replace(" ", "");
                            Pro_Location = gc.Between(PropertyAlldata, "Prop Loc: ", "Owner:");
                            Pro_Owner = gc.Between(PropertyAlldata, "Owner: ", "Square Ft:");
                            Pro_District = gc.Between(PropertyAlldata, "District: ", "Street:");
                            Pro_Class = gc.Between(PropertyAlldata, "Class: ", "City State:");
                            Pro_LandDesc = gc.Between(PropertyAlldata, "Land Desc: ", "Statute:");
                            Pro_BldgDesc = gc.Between(PropertyAlldata, "Bldg Desc: ", "Initial:");
                            Pro_Acrage = gc.Between(PropertyAlldata, "Acreage: ", "Taxes:");
                            Pro_Taxes = gc.Between(PropertyAlldata, "Taxes: ", "Sale Information");
                            Pro_YearBuilt = gc.Between(PropertyAlldata, "Year Built:", "Qual:");


                            District = Regex.Match(Pro_District, @"\d+").Value.Trim();
                            if (distirctCode_Notax.Exists(x => string.Equals(x, District, StringComparison.OrdinalIgnoreCase)))
                            {
                                HttpContext.Current.Session["NoTax_NJ" + countynameNJ] = "No_Tax";
                                driver.Quit();
                                return "Taxes Not Available";
                            }


                            Parcel_Id = Pro_Block + "-" + Pro_Lot + "-" + Pro_Qual;
                            Pid = db.ExecuteScalar("SELECT id FROM data_field_master where State_County_ID='" + statecountyid + "'and Category_Id='1'");
                            id = Convert.ToInt32(Pid);

                            Pro_Details = Pro_Location + "~" + Pro_Owner + "~" + Pro_District + "~" + Pro_Class + "~" + Pro_LandDesc + "~" + Pro_BldgDesc + "~" + Pro_Acrage + "~" + Pro_Taxes + "~" + Pro_YearBuilt;
                            gc.insert_date(orderNumber, Parcel_Id, id, Pro_Details, 1, DateTime.Now);

                            //Sale information
                            Sale_Date = driver.FindElement(By.XPath("/html/body/table[1]/tbody/tr[11]/td[2]/font")).Text;
                            Sale_Book = driver.FindElement(By.XPath("/html/body/table[1]/tbody/tr[11]/td[4]/font[1]")).Text;
                            Sale_Page = driver.FindElement(By.XPath("/html/body/table[1]/tbody/tr[11]/td[4]/font[3]")).Text;
                            Sale_Price = driver.FindElement(By.XPath("/html/body/table[1]/tbody/tr[11]/td[6]/font[1]")).Text;
                            Sale_Nu = driver.FindElement(By.XPath("/html/body/table[1]/tbody/tr[11]/td[6]/font[3]")).Text;

                            Sale_Details = Sale_Date + "~" + Sale_Book + "~" + Sale_Page + "~" + Sale_Price + "~" + Sale_Nu;
                            Pid = db.ExecuteScalar("SELECT id FROM data_field_master where State_County_ID='" + statecountyid + "'and Category_Id='2'");
                            id = Convert.ToInt32(Pid);

                            gc.insert_date(orderNumber, Parcel_Id, id, Sale_Details, 1, DateTime.Now);
                            gc.CreatePdf(orderNumber, Parcel_Id, "Property Details", driver, "NJ", countynameNJ);
                        }
                        catch (Exception ex)
                        { }

                        //Assessment Details
                        int Assessed = 0;
                        IWebElement AssessmentTB = driver.FindElement(By.XPath("/html/body/table[3]/tbody"));
                        IList<IWebElement> AssessmentTR = AssessmentTB.FindElements(By.TagName("tr"));
                        IList<IWebElement> AssessmentTD;

                        foreach (IWebElement Assessment in AssessmentTR)
                        {
                            AssessmentTD = Assessment.FindElements(By.TagName("td"));
                            if (AssessmentTD.Count != 0 && !Assessment.Text.Contains("TAX-LIST-HISTORY") && !Assessment.Text.Contains("Year") && AssessmentTD.Count != 1 && !Assessment.Text.Contains("*Click Here for More History"))
                            {
                                if (Assessment.Text.Trim() != "")
                                {
                                    if (AssessmentTD[0].Text.Trim() != "")
                                    {
                                        Assessment_Year = AssessmentTD[0].Text;
                                        Assessment_Land = AssessmentTD[2].Text;
                                        Assessment_Exe = AssessmentTD[3].Text;
                                        Assessment_Ass = AssessmentTD[4].Text;
                                        Assessment_Proclass = AssessmentTD[5].Text;
                                        Assessed = 0;
                                    }
                                    if (AssessmentTD[0].Text.Trim() == "" && Assessed == 1)
                                    {
                                        Assessment_Tot = AssessmentTD[2].Text;
                                        Assessment_Details = Assessment_Year + "~" + Assessment_Land + "~" + Assessment_Imp + "~" + Assessment_Tot + "~" + Assessment_Exe + "~" + Assessment_Ass + "~" + Assessment_Proclass;
                                        Pid = db.ExecuteScalar("SELECT id FROM data_field_master where State_County_ID='" + statecountyid + "'and Category_Id='3'");
                                        id = Convert.ToInt32(Pid);

                                        gc.insert_date(orderNumber, Parcel_Id, id, Assessment_Details, 1, DateTime.Now);
                                    }
                                    if (AssessmentTD[0].Text.Trim() == "" && Assessed == 0)
                                    {
                                        Assessment_Imp = AssessmentTD[2].Text;
                                        Assessed++;
                                    }
                                }
                            }
                        }
                    }
                    //Tax Details
                    string urltown = "", countlink = "", taxCollectorlink = "";

                    NJ_Link linknj = new Scrapsource.NJ_Link();
                    string[] urllink = linknj.link(District, countynameNJ);

                    urltown = urllink[0];
                    countlink = urllink[1];
                    taxCollectorlink = urllink[2];
                    HttpContext.Current.Session["linkNo"] = countlink;
                    #region zero Tax Link


                    if (countlink == "0")
                    {
                        driver.Navigate().GoToUrl(urltown);
                        Thread.Sleep(3000);
                        NJ_TaxAuthority TaxCollectornj = new Scrapsource.NJ_TaxAuthority();
                        string taxAuthority = TaxCollectornj.TaxCollector(driver, District, orderNumber, Parcel_Id, countynameNJ);

                        //Test


                        //Tax_Authority = Tax1;
                        Pid = db.ExecuteScalar("SELECT id FROM data_field_master where State_County_ID='" + statecountyid + "'and Category_Id='4'");
                        id = Convert.ToInt32(Pid);

                        gc.insert_date(orderNumber, Parcel_Id, id, taxAuthority, 1, DateTime.Now);
                        if (Pro_Qual == "HM")
                        {
                            Pro_Qual = "";
                        }
                        try
                        {
                            driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/div/table[1]/tbody/tr/td/table/tbody/tr[2]/td/table/tbody/tr[1]/td[2]/input")).SendKeys(Pro_Block);
                            driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/div/table[1]/tbody/tr/td/table/tbody/tr[2]/td/table/tbody/tr[2]/td[2]/input")).SendKeys(Pro_Lot);
                            driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/div/table[1]/tbody/tr/td/table/tbody/tr[2]/td/table/tbody/tr[3]/td[2]/input")).SendKeys(Pro_Qual);
                            gc.CreatePdf(orderNumber, Parcel_Id, "Tax site", driver, "NJ", countynameNJ);
                            driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/div/table[1]/tbody/tr/td/table/tbody/tr[2]/td/table/tbody/tr[3]/td[3]/button")).SendKeys(Keys.Enter);

                        }
                        catch
                        {
                            driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/div/table[2]/tbody/tr/td/table/tbody/tr[2]/td/table/tbody/tr[1]/td[2]/input")).SendKeys(Pro_Block);
                            driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/div/table[2]/tbody/tr/td/table/tbody/tr[2]/td/table/tbody/tr[2]/td[2]/input")).SendKeys(Pro_Lot);
                            driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/div/table[2]/tbody/tr/td/table/tbody/tr[2]/td/table/tbody/tr[3]/td[2]/input")).SendKeys(Pro_Qual);
                            gc.CreatePdf(orderNumber, Parcel_Id, "Tax site", driver, "NJ", countynameNJ);
                            driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/div/table[2]/tbody/tr/td/table/tbody/tr[2]/td/table/tbody/tr[3]/td[3]/button")).SendKeys(Keys.Enter);

                        }
                        Thread.Sleep(4000);
                        gc.CreatePdf(orderNumber, Parcel_Id, "Tax Parcel Search", driver, "NJ", countynameNJ);

                        // string Parent_Window1 = driver.CurrentWindowHandle;

                        driver.FindElement(By.Name("picklistGroup")).Click();
                        Thread.Sleep(6000);
                        string block_lot = "", taxAccountid = "", propertylocation = "", propertyClass = "", propertyaddress = "", landvalue = "", ImprovementValue = "", ExemptValue = "", TotalAssessedValue = "", additionalLots = "", SpecialTaxing = "", deductions = "";
                        string taxinfo = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[1]/td/table")).Text.Replace("\r\n", "~");
                        string SpecialTaxing1 = "", SpecialTaxing2 = "", TaxLastPayment = "";
                        string propertyaddress1 = "", propertyaddress2 = "", propertyaddress3 = "", propertyaddress4 = "";
                        gc.CreatePdf(orderNumber, Parcel_Id, "Tax details", driver, "NJ", countynameNJ);
                        block_lot = gc.Between(taxinfo, "Block/Lot/Qual:", "Tax Account Id:").Replace("~", "").Trim();
                        taxAccountid = gc.Between(taxinfo, "Tax Account Id:", "Property Location:").Replace("~", "").Trim();
                        propertylocation = gc.Between(taxinfo, "Property Location:", "Property Class:").Replace("~", "").Trim();
                        propertyClass = gc.Between(taxinfo, "Property Class:", "Owner Name/Address:").Replace("~", "").Trim();
                        ExemptValue = gc.Between(taxinfo, "Exempt Value:", "Total Assessed Value:");
                        ExemptValue = GlobalClass.Before(ExemptValue, "~");
                        TotalAssessedValue = gc.Between(taxinfo, "Total Assessed Value:", "Additional Lots:").Replace("~", "").Trim();
                        additionalLots = gc.Between(taxinfo, "Additional Lots:", "Special Taxing Districts:").Replace("~", "").Trim();
                        try
                        {
                            TaxLastPayment = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[3]/td/table/tbody/tr[2]/td/div/div/table/tbody/tr[3]/td")).Text.Replace("Last Payment:", "").Trim();
                        }
                        catch { }
                        try
                        {
                            if (TaxLastPayment == "")
                            {
                                TaxLastPayment = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[3]/td/table/tbody/tr[2]/td/div/div/table/tbody/tr[1]/td/table/tbody/tr/td[3]")).Text.Replace("Last Payment:", "").Trim();
                            }
                        }
                        catch { }
                        int i1 = 0;
                        IWebElement taxInfo = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[1]/td/table/tbody/tr/td/table/tbody"));
                        IList<IWebElement> TRtaxinfo = taxInfo.FindElements(By.TagName("tr"));
                        IList<IWebElement> TDtaxinfo;
                        foreach (IWebElement row in TRtaxinfo)
                        {

                            TDtaxinfo = row.FindElements(By.TagName("td"));
                            if (TDtaxinfo.Count != 0)
                            {

                                if (i1 == 2)
                                {
                                    propertyaddress1 = TDtaxinfo[1].Text;
                                    landvalue = TDtaxinfo[3].Text;
                                }
                                if (i1 == 3)
                                {
                                    propertyaddress2 = TDtaxinfo[1].Text;
                                    ImprovementValue = TDtaxinfo[3].Text;
                                }
                                if (i1 == 4)
                                    propertyaddress3 = TDtaxinfo[1].Text;
                                if (i1 == 5)
                                {
                                    propertyaddress4 = TDtaxinfo[1].Text;
                                    propertyaddress = propertyaddress1 + " " + propertyaddress2 + " " + propertyaddress3 + " " + propertyaddress4;
                                }
                                if (i1 == 7)
                                {
                                    SpecialTaxing = TDtaxinfo[1].Text;
                                    deductions = TDtaxinfo[3].Text;
                                }
                                if (i1 == 9)
                                {
                                    SpecialTaxing2 = TDtaxinfo[1].Text;
                                    SpecialTaxing = SpecialTaxing + " " + SpecialTaxing2;
                                }
                                if (i1 == 8)
                                {
                                    string deduction1 = "";
                                    try
                                    {
                                        deduction1 = TDtaxinfo[3].Text;
                                    }
                                    catch { }

                                    deductions = deductions + " " + deduction1;
                                }
                                i1++;
                            }
                        }
                        //Block/Lot/Qual~Tax Account Id~Property Location~Property Class~Owner Name/Address~Land Value~Improvement Value~Exempt Value~Total Assessed Value~Additional Lots~Special Taxing Districts~Deductions
                        //Tax Info
                        string Tax_info = block_lot + "~" + taxAccountid + "~" + propertylocation + "~" + propertyClass + "~" + propertyaddress + "~" + landvalue + "~" + ImprovementValue + "~" + ExemptValue + "~" + TotalAssessedValue + "~" + additionalLots + "~" + SpecialTaxing + "~" + deductions + "~" + TaxLastPayment;
                        Pid = db.ExecuteScalar("SELECT id FROM data_field_master where State_County_ID='" + statecountyid + "'and Category_Id='5'");
                        id = Convert.ToInt32(Pid);

                        gc.insert_date(orderNumber, Parcel_Id, id, Tax_info, 1, DateTime.Now);
                        string status = "", status1 = "";
                        string taxheader = "", taxheaderinner = "";
                        string innertext = "", innertextMake = "";
                        int make = 0;
                        int counttd = driver.FindElements(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[3]/td/table/tbody/tr[1]/td/table/tbody/tr/td")).Count;
                        for (int k = 1; k <= counttd; k++)
                        {

                            taxheader = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[3]/td/table/tbody/tr[1]/td/table/tbody/tr/td[" + k + "]")).Text;
                            if (taxheader == "Taxes")
                            {
                                driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[3]/td/table/tbody/tr[1]/td/table/tbody/tr/td[" + k + "]")).Click();
                                Thread.Sleep(3000);
                                int countinner = driver.FindElements(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[3]/td/table/tbody/tr[2]/td/div/div[1]/table/tbody/tr[1]/td/table/tbody/tr/td/table/tbody/tr/td")).Count;
                                innertext = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[3]/td/table/tbody/tr[2]/td/div/div[1]/table/tbody/tr[1]/td/table/tbody/tr/td/table/tbody")).Text;
                                if (innertext.Contains("Make a Payment"))
                                {

                                }
                                else
                                {
                                    if (make == 0)
                                    {
                                        innertextMake = "Payment";
                                    }
                                }
                                for (int k1 = 1; k1 <= countinner; k1++)
                                {
                                    taxheaderinner = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[3]/td/table/tbody/tr[2]/td/div/div[1]/table/tbody/tr[1]/td/table/tbody/tr/td/table/tbody/tr/td[" + k1 + "]")).Text;
                                    if ((taxheaderinner == "Make a Payment") || (innertextMake == "Payment"))
                                    {
                                        make++;
                                        innertextMake = "";
                                        var assesscolumn = ""; var assessvalue = "";
                                        IWebElement tbmulti = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[3]/td/table/tbody/tr[2]/td/div/div[1]/table/tbody/tr[2]/td/table/tbody"));
                                        IList<IWebElement> TRmulti = tbmulti.FindElements(By.TagName("tr"));
                                        IList<IWebElement> TDmulti;
                                        Pid = db.ExecuteScalar("SELECT id FROM data_field_master where State_County_ID='" + statecountyid + "'and Category_Id='6'");
                                        id = Convert.ToInt32(Pid);
                                        foreach (IWebElement row in TRmulti)
                                        {
                                            TDmulti = row.FindElements(By.TagName("td"));
                                            if (!row.Text.Contains("Due Date"))
                                            {

                                                if (TDmulti.Count != 0)
                                                {
                                                    //Year~Due Date~Type~Billed~Balance~Interest~Total Due~Status
                                                    status = TDmulti[8].Text;
                                                    if (status == "OPEN")
                                                    {
                                                        status1 = "OPEN";
                                                    }

                                                    //string Tax_history =  TDmulti[0].Text + "~" + TDmulti[1].Text + "~" + TDmulti[2].Text + "~" + TDmulti[3].Text + "~" + TDmulti[5].Text + "~" + TDmulti[6].Text + "~" + TDmulti[7].Text + "~" + TDmulti[8].Text;

                                                    // gc.insert_date(orderNumber, Parcel_Id, id, Tax_history, 1, DateTime.Now);

                                                    for (int i = 0; i < TDmulti.Count; i++)
                                                    {
                                                        string value = TDmulti[i].Text;
                                                        if (value == "")
                                                        {
                                                            value = "-";
                                                        }

                                                        assessvalue += value + "~";


                                                    }
                                                    assessvalue = assessvalue.TrimEnd('~');
                                                    gc.insert_date(orderNumber, Parcel_Id, id, assessvalue, 1, DateTime.Now);
                                                    assessvalue = "";
                                                }
                                            }
                                            else
                                            {
                                                for (int i = 0; i < TDmulti.Count; i++)
                                                {
                                                    assesscolumn += TDmulti[i].Text + "~";
                                                }

                                            }
                                        }
                                        assesscolumn = assesscolumn.TrimEnd('~');


                                        DBconnection dbconn = new DBconnection();
                                        dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + assesscolumn + "' where Id = '" + id + "'");

                                    }


                                    if (taxheaderinner == "View Tax Rates")

                                    {
                                        driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[3]/td/table/tbody/tr[2]/td/div/div[1]/table/tbody/tr[1]/td/table/tbody/tr/td/table/tbody/tr/td[" + k1 + "]/button")).Click();
                                        Thread.Sleep(3000);
                                        string year = driver.FindElement(By.XPath("/html/body/div[2]/div/table/tbody/tr[2]/td[2]/div/table/tbody/tr[1]/td/table/tbody/tr[1]/td/table/tbody/tr")).Text.Trim();
                                        gc.CreatePdf(orderNumber, Parcel_Id, "Tax rate", driver, "NJ", countynameNJ);
                                        IWebElement tbmulti = driver.FindElement(By.XPath("/html/body/div[2]/div/table/tbody/tr[2]/td[2]/div/table/tbody/tr[1]/td/table/tbody/tr[2]/td/div/div/table/tbody"));
                                        IList<IWebElement> TRmulti = tbmulti.FindElements(By.TagName("tr"));
                                        IList<IWebElement> TDmulti;
                                        foreach (IWebElement row in TRmulti)
                                        {

                                            if (!row.Text.Contains("of Valuation"))
                                            {
                                                TDmulti = row.FindElements(By.TagName("td"));
                                                if (TDmulti.Count != 0)
                                                {
                                                    //Tax~Rate per $100 of Valuation
                                                    string Tax_Rate = year + "~" + TDmulti[0].Text + "~" + TDmulti[1].Text;
                                                    Pid = db.ExecuteScalar("SELECT id FROM data_field_master where State_County_ID='" + statecountyid + "'and Category_Id='8'");
                                                    id = Convert.ToInt32(Pid);

                                                    gc.insert_date(orderNumber, Parcel_Id, id, Tax_Rate, 1, DateTime.Now);
                                                }
                                            }
                                        }
                                        driver.FindElement(By.XPath("/html/body/div[2]/div/table/tbody/tr[2]/td[2]/div/table/tbody/tr[2]/td/button")).Click();

                                    }
                                    if (taxheaderinner == "Project Interest")
                                    {
                                        if (status1 == "OPEN")
                                        {
                                            driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[3]/td/table/tbody/tr[2]/td/div/div[1]/table/tbody/tr[1]/td/table/tbody/tr/td/table/tbody/tr/td[" + k1 + "]/button")).Click();
                                            Thread.Sleep(3000);
                                            string todaydate = "";
                                            gc.CreatePdf(orderNumber, Parcel_Id, "Tax project Interest", driver, "NJ", countynameNJ);

                                            string todaydatemonth = DateTime.Now.ToString("MM");
                                            string deliquentdate = "";


                                            DateTime G_Date = DateTime.Today;
                                            string dateChecking = DateTime.Now.ToString("MM") + "/15/" + DateTime.Now.ToString("yyyy");

                                            if (G_Date < Convert.ToDateTime(dateChecking))
                                            {
                                                //end of the month
                                                todaydate = new DateTime(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(Convert.ToInt16(DateTime.Now.ToString("MM"))), DateTime.DaysInMonth(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(DateTime.Now.ToString("MM")))).ToString("MM/dd/yyyy");
                                                deliquentdate = todaydate.Substring(3, 2);
                                                msg = "current";

                                            }

                                            else if (G_Date >= Convert.ToDateTime(dateChecking))
                                            {
                                                // nextEndOfMonth 
                                                if ((Convert.ToInt16(Convert.ToInt16(DateTime.Now.ToString("MM"))) < 12))
                                                {
                                                    todaydate = new DateTime(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(Convert.ToInt16(DateTime.Now.ToString("MM")) + 1), DateTime.DaysInMonth(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(DateTime.Now.ToString("MM")) + 1)).ToString("MM/dd/yyyy");
                                                    deliquentdate = todaydate.Substring(3, 2);
                                                    msg = "next";
                                                }
                                                else
                                                {
                                                    int nxtYr = Convert.ToInt16(DateTime.Now.ToString("yyyy")) + 1;
                                                    todaydate = new DateTime(nxtYr, 1, DateTime.DaysInMonth(Convert.ToInt16(DateTime.Now.ToString("yyyy")), 1)).ToString("MM/dd/yyyy");
                                                    deliquentdate = todaydate.Substring(3, 2);
                                                    msg = "next";
                                                }
                                            }

                                            Thread.Sleep(2000);
                                            // dt.Clear();
                                            string month = "";
                                            if (msg == "next")
                                            {
                                                driver.FindElement(By.XPath("/html/body/div[2]/div/table/tbody/tr[2]/td[2]/div/table/tbody/tr[1]/td/table/tbody/tr[1]/td/table/tbody/tr/td[3]/div")).Click();
                                                Thread.Sleep(3000);
                                            }
                                            int rowcount = 0;
                                            IWebElement tbmulti = driver.FindElement(By.XPath("/html/body/div[2]/div/table/tbody/tr[2]/td[2]/div/table/tbody/tr[1]/td/table/tbody/tr[2]/td/table/tbody"));
                                            IList<IWebElement> TRmulti = tbmulti.FindElements(By.TagName("tr"));
                                            IList<IWebElement> TDmulti;
                                            foreach (IWebElement row in TRmulti)
                                            {
                                                rowcount++;
                                                if (rowcount > 2)
                                                {
                                                    TDmulti = row.FindElements(By.TagName("td"));
                                                    if (TDmulti.Count != 0)
                                                    {
                                                        for (int i9 = 0; i9 < 7; i9++)
                                                        {
                                                            if (TDmulti[i9].Text == deliquentdate)
                                                            {
                                                                month = TDmulti[i9].Text;
                                                                IWebElement clickdate = TDmulti[i9].FindElement(By.TagName("div"));
                                                                clickdate.Click();
                                                                Thread.Sleep(3000);
                                                                break;
                                                            }

                                                        }
                                                    }
                                                    if (month == deliquentdate)
                                                    {
                                                        break;
                                                    }
                                                }
                                            }
                                            Thread.Sleep(3000);
                                            string deliquent = driver.FindElement(By.XPath("/html/body/div[2]/div/table/tbody/tr[2]/td[2]/div/table/tbody/tr[2]/td/table/tbody")).Text;
                                            string interestdue = "", principaldue = "", totaldue = "";
                                            interestdue = gc.Between(deliquent, "Interest Due:", "Principal Due:").Trim();
                                            principaldue = gc.Between(deliquent, "Principal Due:", "Total Due:").Trim();
                                            totaldue = GlobalClass.After(deliquent, "Total Due:").Trim();

                                            string deliquentdet = interestdue + "~" + principaldue + "~" + totaldue;
                                            Pid = db.ExecuteScalar("SELECT id FROM data_field_master where State_County_ID='" + statecountyid + "'and Category_Id='13'");
                                            id = Convert.ToInt32(Pid);

                                            gc.insert_date(orderNumber, Parcel_Id, id, deliquentdet, 1, DateTime.Now);
                                            gc.CreatePdf(orderNumber, Parcel_Id, "Tax deliquent", driver, "NJ", countynameNJ);
                                            //  Interest Due~principal Due~Total Due
                                            driver.FindElement(By.XPath("/html/body/div[2]/div/table/tbody/tr[2]/td[2]/div/table/tbody/tr[3]/td/button")).Click();
                                            Thread.Sleep(3000);
                                        }
                                    }
                                    if (taxheaderinner == "View Current Bill")
                                    {
                                        string currentbill = "yes";
                                        urlCurrentBill = driver.Url;

                                        try
                                        {

                                            var chromeOptions = new ChromeOptions();
                                            var downloadDirectory = ConfigurationManager.AppSettings["AutoPdf"];
                                            chromeOptions.AddUserProfilePreference("download.default_directory", downloadDirectory);
                                            chromeOptions.AddUserProfilePreference("download.prompt_for_download", false);
                                            chromeOptions.AddUserProfilePreference("disable-popup-blocking", "true");
                                            chromeOptions.AddUserProfilePreference("plugins.always_open_pdf_externally", true);
                                            var driver1 = new ChromeDriver(chromeOptions);
                                            Array.ForEach(Directory.GetFiles(@downloadDirectory), File.Delete);
                                            driver1.Navigate().GoToUrl(urlCurrentBill);
                                            Thread.Sleep(3000);
                                            ByVisibleElement(driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[3]/td/table/tbody/tr[2]/td/div/div[1]/table/tbody/tr[3]/td/table/tbody/tr/td/div")));
                                            driver1.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[3]/td/table/tbody/tr[2]/td/div/div[1]/table/tbody/tr[1]/td/table/tbody/tr/td/table/tbody/tr/td[" + k1 + "]/button")).Click();
                                            Thread.Sleep(5000);
                                            try
                                            {
                                                IWebElement IAddressSearch1 = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[3]/td/table/tbody/tr[2]/td/div/div[1]/table/tbody/tr[1]/td/table/tbody/tr/td/table/tbody/tr/td[" + k1 + "]/button"));
                                                IJavaScriptExecutor js1 = driver as IJavaScriptExecutor;
                                                js1.ExecuteScript("arguments[0].click();", IAddressSearch1);
                                                Thread.Sleep(5000);
                                            }
                                            catch { }

                                            filename = latestfilename();
                                            gc.AutoDownloadFile(orderNumber, Parcel_Id, countynameNJ, "NJ", filename);
                                            driver1.Quit();

                                        }
                                        catch
                                        { }
                                    }
                                }

                            }
                            if (taxheader == "Special Assessments")
                            {

                                IWebElement Iowner = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[3]/td/table/tbody/tr[1]/td/table/tbody/tr/td[" + k + "]/div/input"));
                                IJavaScriptExecutor js = driver as IJavaScriptExecutor;
                                js.ExecuteScript("arguments[0].click();", Iowner);
                                //  driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[3]/td/table/tbody/tr[1]/td/table/tbody/tr/td[" + k + "]")).Click();
                                Thread.Sleep(3000);
                                gc.CreatePdf(orderNumber, Parcel_Id, "Tax Special Assessment", driver, "NJ", countynameNJ);
                                // string specialassess = "";
                                // //          
                                // specialassess = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[3]/td/table/tbody/tr[2]/td/div/div[2]/table/tbody/tr/td/table/tbody")).Text.Replace("\r\n", " ");
                                //if(specialassess=="")
                                // {
                                //     specialassess = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[3]/td/table/tbody/tr[2]/td/div/div[3]/table/tbody/tr/td/table/tbody")).Text.Replace("\r\n", " ");
                                // }
                                //Ordinance: 97-19986 Description: Last Payment: 02/27/2014 Levy Date: 05/03/2000 Levy Amount: 11,377.71 Total Number of Years: 10 Principal Balance: 0.00 Payoff Amount: 0.00 Number of Years Remaining: 0 Next Installment Due: 0.00 Next Due Date: Status: PAID
                                gc.CreatePdf(orderNumber, Parcel_Id, "Tax Special assessment", driver, "NJ", countynameNJ);
                                // int countC = Regex.Matches(taxliens, "Certificate Number:").Count;

                                IList<IWebElement> tables = driver.FindElements(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[3]/td/table/tbody/tr[2]/td/div/div/table/tbody"));
                                int count1 = tables.Count;
                                foreach (IWebElement tab in tables)
                                {
                                    if (tab.Text.Contains("Ordinance:"))
                                    {
                                        countC1 = Regex.Matches(tab.Text, "Ordinance:").Count;
                                        //  string Ordinance = "", Description = "", LastPayment = "", LevyDate = "", LevyAmount = "", TotalNumber = "";
                                        //   string PrincipalBalance = "", PayoffAmount = "", NumberYears = "", NextInstallment = "", NextDue = "", Status = "";

                                        ordinance = new string[countC1];
                                        Description = new string[countC1];
                                        LastPayment = new string[countC1];
                                        LevyDate = new string[countC1];
                                        LevyAmount = new string[countC1];
                                        TotalNumber = new string[countC1];
                                        PrincipalBalance = new string[countC1];
                                        PayoffAmount = new string[countC1];
                                        NumberYears = new string[countC1];
                                        NextInstallment = new string[countC1];
                                        NextDue = new string[countC1];
                                        Status = new string[countC1];

                                        IList<IWebElement> utilityTr = tab.FindElements(By.TagName("tr"));
                                        IList<IWebElement> TDmulti1;

                                        int n = 0;
                                        foreach (IWebElement rowU in utilityTr)
                                        {

                                            TDmulti1 = rowU.FindElements(By.TagName("td"));
                                            if (TDmulti1.Count == 6)
                                            {
                                                if (rowU.Text.Contains("Ordinance:"))
                                                {
                                                    ordinance[n] = TDmulti1[1].Text;
                                                    Description[n] = TDmulti1[3].Text;
                                                    LastPayment[n] = TDmulti1[5].Text;
                                                }
                                                if (rowU.Text.Contains("Levy Date:"))
                                                {
                                                    LevyDate[n] = TDmulti1[1].Text;
                                                    LevyAmount[n] = TDmulti1[3].Text;
                                                    TotalNumber[n] = TDmulti1[5].Text;
                                                }
                                                if (rowU.Text.Contains("Principal Balance:"))
                                                {
                                                    PrincipalBalance[n] = TDmulti1[1].Text;
                                                    PayoffAmount[n] = TDmulti1[3].Text;
                                                    NumberYears[n] = TDmulti1[5].Text;
                                                }
                                                if (rowU.Text.Contains("Next Installment Due:"))
                                                {
                                                    NextInstallment[n] = TDmulti1[1].Text;
                                                    NextDue[n] = TDmulti1[3].Text;
                                                    Status[n] = TDmulti1[5].Text;
                                                    n++;
                                                }

                                            }

                                        }
                                    }
                                }
                                for (int p = 0; p < countC1; p++)
                                {
                                    string Tax_Special = ordinance[p] + "~" + Description[p] + "~" + LastPayment[p] + "~" + LevyDate[p] + "~" + LevyAmount[p] + "~" + TotalNumber[p] + "~" + PrincipalBalance[p] + "~" + PayoffAmount[p] + "~" + NumberYears[p] + "~" + NextInstallment[p] + "~" + NextDue[p] + "~" + Status[p];
                                    //Ordinance~Description~Last Payment~Levy Date~Levy Amount~Total Number of Years~Principal Balance~Payoff Amount~Number of Years Remaining~Next Installment Due~Next Due Date~Status
                                    Pid = db.ExecuteScalar("SELECT id FROM data_field_master where State_County_ID='" + statecountyid + "'and Category_Id='41'");
                                    id = Convert.ToInt32(Pid);
                                    gc.insert_date(orderNumber, Parcel_Id, id, Tax_Special, 1, DateTime.Now);
                                }


                            }
                            if (taxheader == "Special Charges")
                            {
                                IWebElement Iowner = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[3]/td/table/tbody/tr[1]/td/table/tbody/tr/td[" + k + "]/div/input"));
                                IJavaScriptExecutor js = driver as IJavaScriptExecutor;
                                js.ExecuteScript("arguments[0].click();", Iowner);
                                // driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[3]/td/table/tbody/tr[1]/td/table/tbody/tr/td[" + k + "]")).Click();
                                Thread.Sleep(3000);

                                gc.CreatePdf(orderNumber, Parcel_Id, "Special Charges", driver, "NJ", countynameNJ);
                            }
                            if (taxheader == "Utilities" || taxheader == "Sewer")
                            {
                                IWebElement Iowner = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[3]/td/table/tbody/tr[1]/td/table/tbody/tr/td[" + k + "]/div/input"));
                                IJavaScriptExecutor js = driver as IJavaScriptExecutor;
                                js.ExecuteScript("arguments[0].click();", Iowner);
                                // driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[3]/td/table/tbody/tr[1]/td/table/tbody/tr/td[" + k + "]")).Click();
                                Thread.Sleep(3000);
                                gc.CreatePdf(orderNumber, Parcel_Id, "Tax utility", driver, "NJ", countynameNJ);

                                IWebElement tbmulti = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[3]/td/table/tbody/tr[2]/td/div/div[2]/table/tbody/tr[2]/td/table/tbody"));
                                IList<IWebElement> TRmulti = tbmulti.FindElements(By.TagName("tr"));
                                int counttr = TRmulti.Count;
                                IList<IWebElement> TDmulti;
                                foreach (IWebElement row in TRmulti)
                                {

                                    if (!row.Text.Contains("Current Bill"))
                                    {
                                        TDmulti = row.FindElements(By.TagName("td"));
                                        if (TDmulti.Count != 0)
                                        {
                                            //
                                            // Account~Service~Due Date~Current Bill~Current Balance~Delinquent Balance~Interest~Total
                                            try
                                            {

                                                //if (counttr == 4)
                                                //{
                                                //    IWebElement taxutilities = TDmulti[0].FindElement(By.TagName("a"));
                                                //    string taxutilitylink = taxutilities.GetAttribute("href");
                                                //    listutility.Add(taxutilitylink);
                                                //}
                                                //else
                                                //{

                                                //if (TDmulti[7].Text != "0.00")
                                                //{
                                                IWebElement taxutilities = TDmulti[0].FindElement(By.TagName("a"));
                                                string taxutilitylink = taxutilities.GetAttribute("href");
                                                listutility.Add(taxutilitylink);
                                                //    }
                                                //}
                                            }
                                            catch { }
                                            string Tax_utility = TDmulti[0].Text + "~" + TDmulti[1].Text + "~" + TDmulti[2].Text + "~" + TDmulti[3].Text + "~" + TDmulti[4].Text + "~" + TDmulti[5].Text + "~" + TDmulti[6].Text + "~" + TDmulti[7].Text;
                                            Pid = db.ExecuteScalar("SELECT id FROM data_field_master where State_County_ID='" + statecountyid + "'and Category_Id='9'");
                                            id = Convert.ToInt32(Pid);

                                            gc.insert_date(orderNumber, Parcel_Id, id, Tax_utility, 1, DateTime.Now);
                                        }
                                    }
                                }
                            }
                            //string certinumber = "", saledate = "", LienHolder = "", saleAmount = "", chargetypes = "", chargetypes1 = "", chargetypes2 = "", yearinsales = "", subsequentcharge = "";
                            if (taxheader == "Liens")
                            {
                                string taxliens = "";

                                IWebElement Iowner = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[3]/td/table/tbody/tr[1]/td/table/tbody/tr/td[" + k + "]/div/input"));
                                IJavaScriptExecutor js = driver as IJavaScriptExecutor;
                                js.ExecuteScript("arguments[0].click();", Iowner);
                                //driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[3]/td/table/tbody/tr[1]/td/table/tbody/tr/td[" + k + "]")).Click();
                                Thread.Sleep(3000);
                                gc.CreatePdf(orderNumber, Parcel_Id, "Tax Lien", driver, "NJ", countynameNJ);
                                int count1 = 0;
                                try
                                {
                                    tables5 = driver.FindElements(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[3]/td/table/tbody/tr[2]/td/div/div[2]/table/tbody/tr[1]/td/table"));
                                    count1 = tables5.Count;
                                }
                                catch { }
                                if (count1 == 0)
                                {
                                    try
                                    {
                                        tables5 = driver.FindElements(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[3]/td/table/tbody/tr[2]/td/div/div[3]/table/tbody/tr[1]/td/table"));
                                        count1 = tables5.Count;
                                    }
                                    catch { }
                                }
                                foreach (IWebElement tab in tables5)
                                {
                                    if (tab.Text.Contains("Certificate Number:"))
                                    {
                                        countC1 = Regex.Matches(tab.Text, "Certificate Number:").Count;
                                        //  string Ordinance = "", Description = "", LastPayment = "", LevyDate = "", LevyAmount = "", TotalNumber = "";
                                        //   string PrincipalBalance = "", PayoffAmount = "", NumberYears = "", NextInstallment = "", NextDue = "", Status = "";

                                        certinumber = new string[countC1];
                                        saledate = new string[countC1];
                                        LienHolder = new string[countC1];
                                        saleAmount = new string[countC1];
                                        chargetypes = new string[countC1];
                                        chargetypes1 = new string[countC1];
                                        chargetypes2 = new string[countC1];
                                        yearinsales = new string[countC1];
                                        subsequentcharge = new string[countC1];
                                        int q = 0;
                                        string chargetypesword = "";
                                        IList<IWebElement> utilityTr = tab.FindElements(By.TagName("tr"));
                                        IList<IWebElement> TDmulti1;

                                        int n = 0;
                                        foreach (IWebElement rowU in utilityTr)
                                        {

                                            TDmulti1 = rowU.FindElements(By.TagName("td"));
                                            if (TDmulti1.Count == 6)
                                            {
                                                if (rowU.Text.Contains("Certificate Number:"))
                                                {
                                                    if (q == 1)
                                                    {
                                                        n++;
                                                    }
                                                    certinumber[n] = TDmulti1[1].Text;
                                                    saledate[n] = TDmulti1[3].Text;
                                                    LienHolder[n] = TDmulti1[5].Text;
                                                }
                                                if (rowU.Text.Contains("Sale Amount:"))
                                                {
                                                    saleAmount[n] = TDmulti1[1].Text;
                                                    chargetypes[n] = TDmulti1[3].Text;
                                                    yearinsales[n] = TDmulti1[5].Text;
                                                }

                                                if (rowU.Text.Contains("Subsequent Charges:"))
                                                {
                                                    subsequentcharge[n] = TDmulti1[1].Text;

                                                    n++;
                                                }

                                            }
                                            if (TDmulti1.Count == 4)
                                            {
                                                if (rowU.Text.Contains("Subsequent Charges:"))
                                                {
                                                    subsequentcharge[n] = TDmulti1[1].Text;
                                                    n++;
                                                    if (rowU.Text.Contains("Sewer") || rowU.Text.Contains("CCMUA") || rowU.Text.Contains("Tax") || rowU.Text.Contains("Water"))
                                                    {
                                                        n--;
                                                    }
                                                }
                                                if (rowU.Text.Contains("Sewer") || rowU.Text.Contains("CCMUA") || rowU.Text.Contains("Tax") || rowU.Text.Contains("Water"))
                                                {
                                                    try
                                                    {
                                                        if (TDmulti1[3].Text != "")
                                                        {
                                                            chargetypesword = chargetypes[n];
                                                            chargetypesword = chargetypesword + " " + TDmulti1[3].Text;
                                                            chargetypes[n] = chargetypesword;
                                                            var resultcharge = string.Join(" ", chargetypes[n].Split(' ').Distinct());
                                                            chargetypes[n] = resultcharge;
                                                            q = 1;
                                                        }
                                                    }
                                                    catch { }

                                                }
                                            }



                                        }
                                    }
                                }


                                for (int p = 0; p < countC1; p++)
                                {
                                    string Tax_Special = certinumber[p] + "~" + saledate[p] + "~" + LienHolder[p] + "~" + saleAmount[p] + "~" + chargetypes[p] + "~" + yearinsales[p] + "~" + subsequentcharge[p];
                                    //Ordinance~Description~Last Payment~Levy Date~Levy Amount~Total Number of Years~Principal Balance~Payoff Amount~Number of Years Remaining~Next Installment Due~Next Due Date~Status
                                    Pid = db.ExecuteScalar("SELECT id FROM data_field_master where State_County_ID='" + statecountyid + "'and Category_Id='10'");
                                    id = Convert.ToInt32(Pid);
                                    gc.insert_date(orderNumber, Parcel_Id, id, Tax_Special, 1, DateTime.Now);
                                }
                            }
                            //Certificate Number~Sale Date~Lien Holder~Sale Amount~Charge Types~Year in Sale~Subsequent Charges

                        }
                        string taxpayment = "";
                        int kU = 1;
                        foreach (string real in listutility)
                        {
                            driver.Navigate().GoToUrl(real);
                            Thread.Sleep(5000);
                            gc.CreatePdf(orderNumber, Parcel_Id, "Tax Utility details" + kU, driver, "NJ", countynameNJ);
                            kU++;
                            string utiltiyAccount = "", blocklot = "", proplocationU = "", servicelocation = "", ownerdetails = "";
                            string taxutility = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[1]/td/table/tbody/tr/td/table/tbody")).Text.Replace("ount:", "");
                            if (taxutility.Contains("Water/Sewer Acc"))
                            {
                                utiltiyAccount = gc.Between(taxutility, "Water/Sewer Acc", "Block/Lot/Qual:");
                            }
                            else
                            {
                                utiltiyAccount = gc.Between(taxutility, "Utility Acc", "Block/Lot/Qual:");
                            }

                            blocklot = gc.Between(taxutility, "Block/Lot/Qual:", "Property Location:");
                            proplocationU = gc.Between(taxutility, "Property Location:", "Service Location:");
                            servicelocation = gc.Between(taxutility, "Service Location:", "Owner Name/Address:");
                            ownerdetails = GlobalClass.After(taxutility, "Owner Name/Address:");
                            ownerdetails = ownerdetails.Replace("\r\n", " ");
                            //Utility Account~Block/Lot/Qual~Property Location~Service Location~Owner Name/Address
                            int divcount = 0;
                            string utilityheader = "", utilityheaderinner = "";
                            innertext = ""; make = 0;
                            int countutility = driver.FindElements(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[3]/td/table/tbody/tr[1]/td/table/tbody/tr/td")).Count;
                            innertext = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[3]/td/table/tbody/tr[2]/td/div/div/table/tbody/tr[1]/td/table/tbody")).Text;
                            if (innertext.Contains("Make a Payment"))
                            {

                            }
                            else
                            {
                                if (make == 0)
                                {
                                    innertextMake = "Payment";
                                }
                            }
                            for (int k = 1; k <= countutility; k++)
                            {

                                taxheader = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[3]/td/table/tbody/tr[1]/td/table/tbody/tr/td[" + k + "]")).Text;
                                if (taxheader == "SEWER" || taxheader == "Water/Sewer" || taxheader == "Water\\Sewer" || taxheader == "Water/Sewer Charge" || taxheader == "Water" || taxheader == "Sewer" || taxheader == "Solid Waste" || taxheader == "Garbage" || taxheader == "Garbage Fee")
                                {
                                    divcount++;
                                    IWebElement Iowner = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[3]/td/table/tbody/tr[1]/td/table/tbody/tr/td[" + k + "]/div/input"));
                                    IJavaScriptExecutor js = driver as IJavaScriptExecutor;
                                    js.ExecuteScript("arguments[0].click();", Iowner);
                                    string result = taxheader.Replace(@"/", "");
                                    result = result.Replace(@"\\", "").Replace(@"\", "");

                                    gc.CreatePdf(orderNumber, Parcel_Id, result, driver, "NJ", countynameNJ);
                                    Thread.Sleep(3000);
                                    int makepay = 0;
                                    int countinner = driver.FindElements(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[3]/td/table/tbody/tr[2]/td/div/div[1]/table/tbody/tr[1]/td/table/tbody/tr/td")).Count;

                                    for (int k1 = 1; k1 <= countinner; k1++)
                                    {

                                        taxheaderinner = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[3]/td/table/tbody/tr[2]/td/div/div[" + divcount + "]/table/tbody/tr[1]/td/table/tbody/tr/td[" + k1 + "]")).Text;

                                        if (((taxheaderinner == "Make a Payment") || (taxheaderinner.Contains("Last Payment")) || (innertextMake == "Payment")) && makepay == 0)
                                        {
                                            makepay++;
                                            make++;
                                            innertextMake = "";
                                            IList<IWebElement> tables = driver.FindElements(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[3]/td/table/tbody/tr[2]/td/div/div/table/tbody/tr/td/table"));
                                            int count1 = tables.Count;
                                            foreach (IWebElement tab in tables)
                                            {
                                                if (tab.Text.Contains("Due Date") || tab.Text.Contains("Bill Date"))
                                                {
                                                    IList<IWebElement> utilityTr = tab.FindElements(By.TagName("tr"));
                                                    IList<IWebElement> TDmulti1;
                                                    Pid = db.ExecuteScalar("SELECT id FROM data_field_master where State_County_ID='" + statecountyid + "'and Category_Id='12'");
                                                    id = Convert.ToInt32(Pid);

                                                    foreach (IWebElement rowU in utilityTr)
                                                    {
                                                        if (!rowU.Text.Contains("Due Date") && !rowU.Text.Contains("Bill Date"))
                                                        {
                                                            TDmulti1 = rowU.FindElements(By.TagName("td"));

                                                            if (TDmulti1.Count == 11)
                                                            {
                                                                string Tax_utility = TDmulti1[0].Text + "~" + TDmulti1[1].Text + "~" + TDmulti1[2].Text + "~" + TDmulti1[3].Text + "~" + TDmulti1[4].Text + "~" + TDmulti1[5].Text + "~" + TDmulti1[6].Text + "~" + TDmulti1[7].Text + "~" + TDmulti1[8].Text + "~" + TDmulti1[9].Text;
                                                                gc.insert_date(orderNumber, Parcel_Id, id, Tax_utility, 1, DateTime.Now);
                                                            }
                                                            if (TDmulti1.Count == 7)
                                                            {
                                                                string Tax_utility = TDmulti1[0].Text + "~" + TDmulti1[1].Text + "~" + TDmulti1[2].Text + "~" + TDmulti1[3].Text + "~" + TDmulti1[4].Text + "~" + TDmulti1[5].Text + "~" + TDmulti1[6].Text + "~" + "" + "~" + "" + "~" + "";
                                                                gc.insert_date(orderNumber, Parcel_Id, id, Tax_utility, 1, DateTime.Now);
                                                            }
                                                            if (TDmulti1.Count == 9)
                                                            {
                                                                string Tax_utility = TDmulti1[0].Text + "~" + TDmulti1[1].Text + "~" + TDmulti1[2].Text + "~" + TDmulti1[3].Text + "~" + TDmulti1[4].Text + "~" + TDmulti1[5].Text + "~" + TDmulti1[6].Text + "~" + "" + "~" + "" + "~" + TDmulti1[7].Text;
                                                                gc.insert_date(orderNumber, Parcel_Id, id, Tax_utility, 1, DateTime.Now);
                                                            }
                                                        }
                                                    }


                                                }
                                            }

                                            //Service~Due Date~Billed~Balance~Interest~Total Due~Status~Reading~Read Date~Usage	

                                        }
                                        if (taxheaderinner == "View Current Bill")
                                        {
                                            var chromeOptions = new ChromeOptions();
                                            var downloadDirectory = ConfigurationManager.AppSettings["AutoPdf"];
                                            chromeOptions.AddUserProfilePreference("download.default_directory", downloadDirectory);
                                            chromeOptions.AddUserProfilePreference("download.prompt_for_download", false);
                                            chromeOptions.AddUserProfilePreference("disable-popup-blocking", "true");
                                            chromeOptions.AddUserProfilePreference("plugins.always_open_pdf_externally", true);
                                            var driver1 = new ChromeDriver(chromeOptions);
                                            try
                                            {
                                                Array.ForEach(Directory.GetFiles(@downloadDirectory), File.Delete);
                                                driver1.Navigate().GoToUrl(real);
                                                Thread.Sleep(3000);
                                                // ByVisibleElement(driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[3]/td/table/tbody/tr[2]/td/div/div[1]/table/tbody/tr[3]/td/table/tbody/tr/td/div")));
                                                driver1.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[3]/td/table/tbody/tr[2]/td/div/div/table/tbody/tr[1]/td/table/tbody/tr/td[" + k1 + "]/button")).Click();
                                                Thread.Sleep(5000);
                                                //try
                                                //{
                                                //    IWebElement IAddressSearch1 = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[3]/td/table/tbody/tr[2]/td/div/div[1]/table/tbody/tr[1]/td/table/tbody/tr/td/table/tbody/tr/td[" + k1 + "]/button"));
                                                //    IJavaScriptExecutor js1 = driver as IJavaScriptExecutor;
                                                //    js1.ExecuteScript("arguments[0].click();", IAddressSearch1);
                                                //    Thread.Sleep(5000);
                                                //}
                                                //catch { }

                                                filename = latestfilename();
                                                gc.AutoDownloadFile(orderNumber, Parcel_Id, countynameNJ, "NJ", filename);
                                                driver1.Quit();
                                            }
                                            catch
                                            {
                                                driver1.Quit();
                                            }
                                            //try
                                            //{
                                            //    driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[3]/td/table/tbody/tr[2]/td/div/div/table/tbody/tr[1]/td/table/tbody/tr/td[" + k1 + "]/button")).Click();
                                            //    Thread.Sleep(7000);
                                            //    string Parent_Window1 = driver.CurrentWindowHandle;
                                            //    driver.SwitchTo().Window(driver.WindowHandles.Last());
                                            //    Thread.Sleep(2000);
                                            //    string url = driver.Url;
                                            //    try
                                            //    {
                                            //        gc.downloadfile(url, orderNumber, Parcel_Id, "Current Utility Bill", "NJ", countynameNJ);
                                            //        Thread.Sleep(4000);
                                            //    }
                                            //    catch { }
                                            //    driver.SwitchTo().Window(Parent_Window1);
                                            //}
                                            //catch { }

                                        }

                                        if (taxheaderinner.Contains("Last Payment"))
                                        {
                                            try
                                            {
                                                taxpayment = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[3]/td/table/tbody/tr[2]/td/div/div/table/tbody/tr[1]/td/table/tbody/tr/td[" + k1 + "]")).Text.Replace("Last Payment:", "");
                                            }
                                            catch { }
                                        }

                                        if (taxpayment == "")
                                        {
                                            try
                                            {
                                                if (taxheaderinner.Contains("Last Payment"))
                                                {
                                                    taxpayment = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[3]/td/table/tbody/tr[2]/td/div/div[2]/table/tbody/tr[1]/td/table/tbody/tr/td[" + k1 + "]")).Text.Replace("Last Payment:", "");
                                                }
                                            }
                                            catch { }
                                        }
                                    }
                                }
                            }
                            string UtilityInfo = utiltiyAccount + "~" + blocklot + "~" + proplocationU + "~" + servicelocation + "~" + ownerdetails + "~" + taxpayment;
                            Pid = db.ExecuteScalar("SELECT id FROM data_field_master where State_County_ID='" + statecountyid + "'and Category_Id='11'");
                            id = Convert.ToInt32(Pid);

                            gc.insert_date(orderNumber, Parcel_Id, id, UtilityInfo, 1, DateTime.Now);
                        }


                    }
                    #endregion
                    string property_loc1 = "";
                    #region one Tax Link
                    if (countlink == "1")
                    {
                        driver.Navigate().GoToUrl(urltown);
                        string block = "", subblock = "", lot = "", sublot = "";
                        string[] blocktax = Pro_Block.Split('.');
                        block = blocktax[0];
                        if (blocktax.Count() >= 2)
                        {
                            subblock = blocktax[1];
                        }
                        string[] lottax = Pro_Lot.Split('.');
                        lot = lottax[0];
                        if (lottax.Count() >= 2)
                        {
                            sublot = lottax[1];
                        }

                        driver.FindElement(By.Name("Block")).SendKeys(block);
                        driver.FindElement(By.Name("SubBlock")).SendKeys(subblock);
                        driver.FindElement(By.Name("Lot")).SendKeys(lot);
                        driver.FindElement(By.Name("SubLot")).SendKeys(sublot);
                        driver.FindElement(By.Name("QualificationCode")).SendKeys(Pro_Qual);
                        driver.FindElement(By.Name("OwnerLast")).SendKeys(Pro_Owner);
                        gc.CreatePdf(orderNumber, Parcel_Id, "Tax input", driver, "NJ", countynameNJ);
                        driver.FindElement(By.Name("continue_step2")).SendKeys(Keys.Enter);
                        Thread.Sleep(4000);
                        gc.CreatePdf(orderNumber, Parcel_Id, "Tax Info", driver, "NJ", countynameNJ);
                        //*[@id="cn_contents"]
                        //*[@id="cn_contents"]/span
                        string norecord = driver.FindElement(By.Id("cn_contents")).Text;
                        if (norecord.Contains("We were unable to retrieve your records."))
                        {

                        }
                        else
                        {
                            string taxinfo1 = driver.FindElement(By.XPath("//*[@id='cn_contents']/table/tbody/tr/td/table[2]/tbody")).Text.Replace("\r\n", " ");
                            string accountno1 = "", blocklot1 = "", propertyzip = "", propertyowner = "", currentQtr1 = "", duedate = "";
                            //taxinfo
                            if (taxinfo1.Contains("Account No.:"))
                            {
                                accountno1 = gc.Between(taxinfo1, "Account No.:", "Block-Lot:").Trim();
                            }
                            else
                            {
                                accountno1 = "";
                            }
                            blocklot1 = gc.Between(taxinfo1, "Block-Lot:", " Property Location:").Trim();
                            property_loc1 = gc.Between(taxinfo1, " Property Location:", "Property Zip:").Trim();
                            propertyzip = gc.Between(taxinfo1, "Property Zip:", "Property Owner:").Trim();
                            propertyowner = gc.Between(taxinfo1, "Property Owner:", "Tax Information:").Replace("Tax History:", "").Replace("View/Print Bill:", "").Trim();
                            currentQtr1 = gc.Between(taxinfo1, "Current Quarter #:", "Current Qtr.").Trim();
                            duedate = gc.Between(taxinfo1, "Current Qtr. Due Date:", "Type").Trim();

                            //Account No~Block-Lot~Property Location~Property Zip~Property Owner~Current Quarter~Current Qtr. Due Date
                            string Tax_info1 = accountno1 + "~" + blocklot1 + "~" + property_loc1 + "~" + propertyzip + "~" + propertyowner + "~" + currentQtr1 + "~" + duedate;
                            Pid = db.ExecuteScalar("SELECT id FROM data_field_master where State_County_ID='" + statecountyid + "'and Category_Id='14'");
                            id = Convert.ToInt32(Pid);

                            gc.insert_date(orderNumber, Parcel_Id, id, Tax_info1, 1, DateTime.Now);

                            // Tax 
                            try
                            {
                                driver.FindElement(By.XPath("//*[@id='cn_contents']/table[2]/tbody/tr/td/table[2]/tbody/tr[7]/td[3]/span/input")).Click();
                                Thread.Sleep(2000);
                                gc.CreatePdf(orderNumber, Parcel_Id, "Tax History", driver, "NJ", countynameNJ);
                            }
                            catch { }
                            // Tax 


                            string MuniCode = "", MuniName = "", strBlock = "", strLot = "", Qualifier = "", AccNo = "", strOwner = "", Location = "", LandValue = "";
                            string Improvement = "", NetValue = "", Interestto = "", Deductions = "", Status = "", PropertyClass = "";
                            string taxdata = "";
                            try
                            {
                                taxdata = driver.FindElement(By.XPath("//*[@id='cn_contents']/div[2]")).Text;
                            }
                            catch { }

                            // Tax Property Information
                            try
                            {
                                MuniCode = gc.Between(taxdata, "Muni. Code:", "Muni. Name:").Trim();
                                MuniName = gc.Between(taxdata, "Muni. Name:", "Block:").Trim();
                                strBlock = gc.Between(taxdata, "Block:", "Lot:").Trim();
                                strLot = gc.Between(taxdata, "Lot:", "Qualifier:").Trim();
                                Qualifier = gc.Between(taxdata, "Qualifier:", "Account No:").Trim();
                                AccNo = gc.Between(taxdata, "Account No:", "Owner Name:").Trim();
                                strOwner = gc.Between(taxdata, "Owner Name:", "Location:").Trim();
                                Location = gc.Between(taxdata, "Location:", "Land Value:").Trim();
                                LandValue = gc.Between(taxdata, "Land Value:", "Improvement:").Trim();
                                Improvement = gc.Between(taxdata, "Improvement:", "Net Value:").Trim();
                                NetValue = gc.Between(taxdata, "Net Value:", "Interest To:").Trim();
                                Interestto = gc.Between(taxdata, "Interest To:", "Deductions:").Trim();
                                Deductions = gc.Between(taxdata, "Deductions:", "Status:").Trim();
                                Status = gc.Between(taxdata, "Status:", "Property Class:").Trim();
                                PropertyClass = GlobalClass.After(taxdata, "Property Class:").Trim();

                                string TaxPropertyInfo = MuniCode + "~" + MuniName + "~" + strBlock + "~" + strLot + "~" + Qualifier + "~" + AccNo + "~" + strOwner + "~" + Location + "~" + LandValue + "~" + Improvement + "~" + NetValue + "~" + Interestto + "~" + Deductions + "~" + Status + "~" + PropertyClass;

                                Pid = db.ExecuteScalar("SELECT id FROM data_field_master where State_County_ID='" + statecountyid + "'and Category_Id='42'");
                                id = Convert.ToInt32(Pid);

                                gc.insert_date(orderNumber, Parcel_Id, id, TaxPropertyInfo, 1, DateTime.Now);
                            }
                            catch { }

                            // Tax History Details
                            try
                            {
                                IWebElement TaxHistory = driver.FindElement(By.XPath("//*[@id='tab-tax']/table/tbody"));
                                IList<IWebElement> TRTaxHistory = TaxHistory.FindElements(By.TagName("tr"));
                                IList<IWebElement> THTaxHistory = TaxHistory.FindElements(By.TagName("th"));
                                IList<IWebElement> TDTaxHistory;
                                foreach (IWebElement row in TRTaxHistory)
                                {
                                    TDTaxHistory = row.FindElements(By.TagName("td"));
                                    if (TDTaxHistory.Count != 0 && !row.Text.Contains("Bill Date") && row.Text.Trim() != "")
                                    {
                                        string TaxHistorydetails = TDTaxHistory[0].Text + "~" + TDTaxHistory[1].Text + "~" + TDTaxHistory[2].Text + "~" + TDTaxHistory[3].Text + "~" + TDTaxHistory[4].Text + "~" + TDTaxHistory[5].Text + "~" + TDTaxHistory[6].Text + "~" + TDTaxHistory[7].Text + "~" + TDTaxHistory[8].Text + "~" + TDTaxHistory[9].Text + "~" + TDTaxHistory[10].Text;

                                        Pid = db.ExecuteScalar("SELECT id FROM data_field_master where State_County_ID='" + statecountyid + "'and Category_Id='43'");
                                        id = Convert.ToInt32(Pid);

                                        gc.insert_date(orderNumber, Parcel_Id, id, TaxHistorydetails, 1, DateTime.Now);
                                    }
                                }
                            }
                            catch { }

                            try
                            {
                                driver.FindElement(By.XPath("//*[@id='tab-tax']/form/p/input[1]")).Click();
                                Thread.Sleep(5000);
                            }
                            catch { }

                            //tax payment
                            try
                            {
                                IWebElement tbmulti9 = driver.FindElement(By.XPath("//*[@id='cn_contents']/table[2]/tbody/tr/td/table[2]/tbody"));
                                IList<IWebElement> TRmulti9 = tbmulti9.FindElements(By.TagName("tr"));
                                IList<IWebElement> TDmulti9;
                                Pid = db.ExecuteScalar("SELECT id FROM data_field_master where State_County_ID='" + statecountyid + "'and Category_Id='15'");
                                id = Convert.ToInt32(Pid);

                                foreach (IWebElement row in TRmulti9)
                                {
                                    TDmulti9 = row.FindElements(By.TagName("td"));
                                    if (TDmulti9.Count >= 2)
                                    {
                                        if (TDmulti9[1].Text.Contains("Original Tax"))
                                        {
                                            IList<IWebElement> ITaxRealRowQ1 = TDmulti9[2].FindElements(By.TagName("tr"));
                                            IList<IWebElement> ITaxRealTdQ1;
                                            foreach (IWebElement ItaxReal1 in ITaxRealRowQ1)
                                            {
                                                ITaxRealTdQ1 = ItaxReal1.FindElements(By.TagName("td"));
                                                if (ITaxRealTdQ1.Count != 0 && ITaxRealTdQ1.Count == 2)
                                                {
                                                    string Tax_payment1 = ITaxRealTdQ1[0].Text + "~" + ITaxRealTdQ1[1].Text;
                                                    gc.insert_date(orderNumber, Parcel_Id, id, Tax_payment1, 1, DateTime.Now);

                                                    //Installment~Amount

                                                }
                                            }

                                        }
                                    }
                                }
                            }
                            catch { }
                            try
                            {
                                IWebElement TaxAmt = driver.FindElement(By.XPath("//*[@id='taxamounttable']/tbody"));
                                IList<IWebElement> TaxTr = TaxAmt.FindElements(By.TagName("tr"));
                                IList<IWebElement> TaxTd;
                                Pid = db.ExecuteScalar("SELECT id FROM data_field_master where State_County_ID='" + statecountyid + "'and Category_Id='16'");
                                id = Convert.ToInt32(Pid);

                                foreach (IWebElement row in TaxTr)
                                {

                                    if (!row.Text.Contains("Amount Due"))
                                    {
                                        TaxTd = row.FindElements(By.TagName("td"));
                                        if (TaxTd.Count == 4)
                                        {
                                            //Type~Amount Due~Status
                                            string Tax_Rate = TaxTd[0].Text + "~" + TaxTd[1].Text + "~" + TaxTd[2].Text + "~" + TaxTd[3].Text;
                                            gc.insert_date(orderNumber, Parcel_Id, id, Tax_Rate, 1, DateTime.Now);
                                        }
                                        if (TaxTd.Count == 3)
                                        {
                                            //Type~Amount Due~Status
                                            string Tax_Rate = TaxTd[0].Text + "~" + "" + "~" + TaxTd[1].Text + "~" + "";
                                            gc.insert_date(orderNumber, Parcel_Id, id, Tax_Rate, 1, DateTime.Now);
                                        }
                                    }
                                }
                            }
                            catch { }
                        }

                        string filename = "";

                        var chromeOptions = new ChromeOptions();
                        var downloadDirectory = ConfigurationManager.AppSettings["AutoPdf"];
                        chromeOptions.AddUserProfilePreference("download.default_directory", downloadDirectory);
                        chromeOptions.AddUserProfilePreference("download.prompt_for_download", false);
                        chromeOptions.AddUserProfilePreference("disable-popup-blocking", "true");
                        chromeOptions.AddUserProfilePreference("plugins.always_open_pdf_externally", true);
                        var driver1 = new ChromeDriver(chromeOptions);
                        Array.ForEach(Directory.GetFiles(@downloadDirectory), File.Delete);
                        try
                        {

                            driver1.Navigate().GoToUrl(urltown);
                            Thread.Sleep(3000);
                            try
                            {
                                driver1.FindElement(By.Name("Block")).SendKeys(block);
                                driver1.FindElement(By.Name("SubBlock")).SendKeys(subblock);
                                driver1.FindElement(By.Name("Lot")).SendKeys(lot);
                                driver1.FindElement(By.Name("SubLot")).SendKeys(sublot);
                                driver1.FindElement(By.Name("QualificationCode")).SendKeys(Pro_Qual);
                                driver1.FindElement(By.Name("OwnerLast")).SendKeys(Pro_Owner);

                                driver1.FindElement(By.Name("continue_step2")).SendKeys(Keys.Enter);
                                Thread.Sleep(4000);
                            }
                            catch { }

                            driver1.FindElement(By.XPath("//*[@id='cn_contents']/table[2]/tbody/tr/td/table[2]/tbody/tr[6]/td[3]/a")).Click();
                            Thread.Sleep(4000);

                            filename = latestfilename();
                            gc.AutoDownloadFile(orderNumber, Parcel_Id, countynameNJ, "NJ", filename);
                            Thread.Sleep(2000);
                            driver1.Quit();
                        }
                        catch
                        {
                            driver1.Quit();
                        }

                        driver.Navigate().GoToUrl(taxCollectorlink);
                        Thread.Sleep(6000);
                        gc.CreatePdf(orderNumber, Parcel_Id, "Tax Authority", driver, "NJ", countynameNJ);
                        NJ_TaxAuthority TaxCollectornj = new Scrapsource.NJ_TaxAuthority();
                        //tax Authority
                        string taxAuthority = TaxCollectornj.TaxCollector(driver, District, orderNumber, Parcel_Id, countynameNJ);

                        Pid = db.ExecuteScalar("SELECT id FROM data_field_master where State_County_ID='" + statecountyid + "'and Category_Id='4'");
                        id = Convert.ToInt32(Pid);

                        gc.insert_date(orderNumber, Parcel_Id, id, taxAuthority, 1, DateTime.Now);

                        if (township == "PARSIPPANY")
                        {
                            driver.Navigate().GoToUrl("https://wipp.edmundsassoc.com/Wipp/?wippid=1429");
                            Thread.Sleep(5000);
                            driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/div/table/tbody/tr/td/table/tbody/tr[2]/td/table/tbody/tr[3]/td[7]/input")).SendKeys(property_loc1);
                            driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/div/table/tbody/tr/td/table/tbody/tr[2]/td/table/tbody/tr[3]/td[8]/button")).SendKeys(Keys.Enter);
                            Thread.Sleep(3000);
                            gc.CreatePdf(orderNumber, Parcel_Id, "Utility Input Passing", driver, "NJ", countynameNJ);
                            driver.FindElement(By.Name("picklistGroup")).Click();
                            Thread.Sleep(6000);
                            gc.CreatePdf(orderNumber, Parcel_Id, "Utility Details", driver, "NJ", countynameNJ);

                            try
                            {

                                var chromeOptions1 = new ChromeOptions();
                                var downloadDirectory1 = ConfigurationManager.AppSettings["AutoPdf"];
                                chromeOptions1.AddUserProfilePreference("download.default_directory", downloadDirectory1);
                                chromeOptions1.AddUserProfilePreference("download.prompt_for_download", false);
                                chromeOptions1.AddUserProfilePreference("disable-popup-blocking", "true");
                                chromeOptions1.AddUserProfilePreference("plugins.always_open_pdf_externally", true);
                                var drivertown = new ChromeDriver(chromeOptions1);
                                try
                                {
                                    Array.ForEach(Directory.GetFiles(@downloadDirectory1), File.Delete);
                                    drivertown.Navigate().GoToUrl("https://wipp.edmundsassoc.com/Wipp/?wippid=1429");
                                    Thread.Sleep(7000);
                                    drivertown.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/div/table/tbody/tr/td/table/tbody/tr[2]/td/table/tbody/tr[3]/td[7]/input")).SendKeys(property_loc1);
                                    drivertown.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/div/table/tbody/tr/td/table/tbody/tr[2]/td/table/tbody/tr[3]/td[8]/button")).SendKeys(Keys.Enter);
                                    Thread.Sleep(5000);
                                    //gc.CreatePdf(orderNumber, Parcel_Id, "Utility Details1", drivertown, "NJ", countynameNJ);
                                    try
                                    {
                                        IWebElement IAddressSearch1 = drivertown.FindElement(By.Name("picklistGroup"));
                                        IJavaScriptExecutor js1 = drivertown as IJavaScriptExecutor;
                                        js1.ExecuteScript("arguments[0].click();", IAddressSearch1);
                                        Thread.Sleep(7000);
                                        //gc.CreatePdf(orderNumber, Parcel_Id, "Picklistgrouping", drivertown, "NJ", countynameNJ);
                                    }
                                    catch
                                    { }
                                    try
                                    {
                                        drivertown.FindElement(By.Name("picklistGroup")).Click();
                                        Thread.Sleep(7000);
                                         gc.CreatePdf(orderNumber, Parcel_Id, "Utility Details2", drivertown, "NJ", countynameNJ);
                                        int towncount = drivertown.FindElements(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[3]/td/table/tbody/tr[1]/td/table/tbody/tr/td")).Count;
                                        string towntax = "";
                                    }
                                    catch { }

                                    int counttr = drivertown.FindElements(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[3]/td/table/tbody/tr")).Count;
                                    IList<IWebElement> tablestr = drivertown.FindElements(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[3]/td/table/tbody/tr"));
                                    int counttr1 = tablestr.Count;
                                    foreach (IWebElement tab in tablestr)
                                    {
                                        if (tab.Text.Contains("View Current Bill"))
                                        {
                                            try
                                            {
                                                IWebElement IAddressSearch1 = drivertown.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[3]/td/table/tbody/tr[2]/td/div/div/table/tbody/tr[1]/td/table/tbody/tr/td[2]/button"));
                                                IJavaScriptExecutor js1 = drivertown as IJavaScriptExecutor;
                                                js1.ExecuteScript("arguments[0].click();", IAddressSearch1);
                                                //Thread.Sleep(7000);
                                                //gc.CreatePdf(orderNumber, Parcel_Id, "Utility Details3", drivertown, "NJ", countynameNJ);

                                            }
                                            catch
                                            {

                                                IWebElement IAddressSearch2 = drivertown.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[3]/td/table/tbody/tr[1]/td/div/div/table/tbody/tr[1]/td/table/tbody/tr/td[2]/button"));
                                                IJavaScriptExecutor js2 = drivertown as IJavaScriptExecutor;
                                                js2.ExecuteScript("arguments[0].click();", IAddressSearch2);
                                                //Thread.Sleep(7000);
                                            }
                                            Thread.Sleep(8000);
                                            //gc.CreatePdf(orderNumber, Parcel_Id, "Utility Details4", drivertown, "NJ", countynameNJ);
                                        }
                                    }

                                    filename = latestfilename();
                                    gc.AutoDownloadFile(orderNumber, Parcel_Id, countynameNJ, "NJ", filename);
                                    drivertown.Quit();
                                }
                                catch
                                {
                                    drivertown.Quit();
                                }

                            }
                            catch
                            { }
                        }
                    }
                    #endregion
                    #region two Tax Link  //deva
                    if (countlink == "2")  //
                    {
                        driver.Navigate().GoToUrl(urltown);


                        string BlockTax = "", Lottax = "";
                        int CountPro_Block = Pro_Block.Count();
                        int Count_Lot = Pro_Lot.Count();
                        if (CountPro_Block == 1)
                        {
                            BlockTax = "00" + Pro_Block;
                        }
                        if (CountPro_Block == 2)
                        {
                            BlockTax = "0" + Pro_Block;
                        }
                        if (CountPro_Block == 3)
                        {
                            BlockTax = Pro_Block;
                        }
                        if (Count_Lot == 1)
                        {
                            Lottax = "00" + Pro_Lot;
                        }
                        if (Count_Lot == 2)
                        {
                            Lottax = "0" + Pro_Lot;
                        }
                        if (Count_Lot == 3)
                        {
                            Lottax = Pro_Lot;
                        }
                        driver.FindElement(By.Id("Block")).SendKeys(BlockTax);
                        driver.FindElement(By.Id("Lot")).SendKeys(Lottax);
                        driver.FindElement(By.Id("Qualification")).SendKeys(Pro_Qual);
                        gc.CreatePdf(orderNumber, Parcel_Id, "Tax input", driver, "NJ", countynameNJ);
                        driver.FindElement(By.Name("search_block")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, Parcel_Id, "Tax Info1", driver, "NJ", countynameNJ);
                        IWebElement Taxcountlink2table = driver.FindElement(By.XPath("//*[@id='cn_contents']/table/tbody"));
                        IList<IWebElement> Taxcountlinktr = Taxcountlink2table.FindElements(By.TagName("tr"));
                        IList<IWebElement> Taxcountlinktd;
                        foreach (IWebElement Taxcountlink in Taxcountlinktr)
                        {
                            Taxcountlinktd = Taxcountlink.FindElements(By.TagName("td"));
                            if (!Taxcountlink.Text.Contains("Block"))
                            {
                                IWebElement taxtable = Taxcountlinktd[0].FindElement(By.TagName("a"));
                                string Href = taxtable.GetAttribute("href");
                                driver.Navigate().GoToUrl(Href);
                                Thread.Sleep(2000);
                                break;
                            }
                        }
                        gc.CreatePdf(orderNumber, Parcel_Id, "Tax Info", driver, "NJ", countynameNJ);
                        string TaxProperty2 = driver.FindElement(By.XPath("//*[@id='cn_contents']/div[2]")).Text;
                        string municode = gc.Between(TaxProperty2, "Muni. Code:", "Muni. Name:");
                        string Muniname = gc.Between(TaxProperty2, "Muni. Name:", "Block:");
                        string Block = gc.Between(TaxProperty2, "Block:", "Lot:");
                        string Lot = gc.Between(TaxProperty2, "Lot:", "Qualifier:");
                        string Qualifier = gc.Between(TaxProperty2, "Qualifier:", "Account No:");
                        string Account_No = gc.Between(TaxProperty2, "Account No:", "Owner Name:");
                        string ownername2 = gc.Between(TaxProperty2, "Owner Name:", "Location:");
                        string location = gc.Between(TaxProperty2, "Location:", "Land Value:");
                        string LandValue = gc.Between(TaxProperty2, "Land Value:", "Improvement:");
                        string Improvement = gc.Between(TaxProperty2, "Improvement:", "Net Value:");
                        string NetValue = gc.Between(TaxProperty2, "Net Value:", "Interest To:");
                        string InterestTo = gc.Between(TaxProperty2, "Interest To:", "Deductions:");
                        string Deductions = gc.Between(TaxProperty2, "Deductions:", "Status:");
                        string Status = gc.Between(TaxProperty2, "Status:", "Property Class:");
                        string PropertyClass = GlobalClass.After(TaxProperty2, "Property Class:");
                        string propertyresult = municode + "~" + Muniname + "~" + Block + "~" + Lot + "~" + Qualifier + "~" + Account_No + "~" + ownername2 + "~" + location + "~" + LandValue + "~" + Improvement + "~" + NetValue + "~" + InterestTo + "~" + Deductions + "~" + Status + "~" + PropertyClass;

                        Pid = db.ExecuteScalar("SELECT id FROM data_field_master where State_County_ID='" + statecountyid + "'and Category_Id='17'");
                        id = Convert.ToInt32(Pid);

                        gc.insert_date(orderNumber, Parcel_Id, id, propertyresult, 1, DateTime.Now);
                        try
                        {
                            IWebElement Tapcount1 = driver.FindElement(By.Id("tabs"));
                            IList<IWebElement> Tapcount = Tapcount1.FindElements(By.TagName("li"));
                            for (int I = 1; I <= Tapcount.Count(); I++)
                            {
                                IWebElement Choose = null;
                                string strChoose = "";
                                try
                                {
                                    Choose = driver.FindElement(By.Id("ui-id-" + I + ""));
                                }
                                catch { }
                                if (Choose.Text != "")
                                {
                                    strChoose = Choose.Text.Trim();
                                }
                                if (strChoose == "Tax")
                                {
                                    IWebElement Taxtable = driver.FindElement(By.Id("tab-tax"));
                                    IList<IWebElement> Taxtr = Taxtable.FindElements(By.TagName("tr"));
                                    IList<IWebElement> Taxtd;
                                    Pid = db.ExecuteScalar("SELECT id FROM data_field_master where State_County_ID='" + statecountyid + "'and Category_Id='18'");
                                    id = Convert.ToInt32(Pid);

                                    foreach (IWebElement TaxBeg in Taxtr)
                                    {
                                        Taxtd = TaxBeg.FindElements(By.TagName("td"));
                                        if (!TaxBeg.Text.Contains("Year") && Taxtd.Count > 1)
                                        {
                                            string Taxbegresult = Taxtd[0].Text + "~" + Taxtd[1].Text + "~" + Taxtd[2].Text + "~" + Taxtd[3].Text + "~" + Taxtd[4].Text + "~" + Taxtd[5].Text + "~" + Taxtd[6].Text + "~" + Taxtd[7].Text + "~" + Taxtd[8].Text + "~" + Taxtd[9].Text + "~" + Taxtd[10].Text;
                                            gc.insert_date(orderNumber, Parcel_Id, id, Taxbegresult, 1, DateTime.Now);
                                        }
                                    }
                                }
                                if (strChoose == "REFUSE")
                                {
                                    //IWebElement Refuse = driver.FindElement(By.Id("ui-id-2"));
                                    IWebElement Refuse = driver.FindElement(By.LinkText("REFUSE"));
                                    Refuse.Click();
                                    Thread.Sleep(2000);
                                    gc.CreatePdf(orderNumber, Parcel_Id, "Refuse", driver, "NJ", countynameNJ);
                                    IWebElement Refusetable = driver.FindElement(By.Id("tab-R"));
                                    IList<IWebElement> Refusetr = Refusetable.FindElements(By.TagName("tr"));
                                    IList<IWebElement> Refusetd;
                                    Pid = db.ExecuteScalar("SELECT id FROM data_field_master where State_County_ID='" + statecountyid + "'and Category_Id='19'");
                                    id = Convert.ToInt32(Pid);

                                    foreach (IWebElement RefuseBeg in Refusetr)
                                    {
                                        Refusetd = RefuseBeg.FindElements(By.TagName("td"));
                                        if (!RefuseBeg.Text.Contains("Year") && Refusetd.Count > 1)
                                        {
                                            string RefuseBegresult = Refusetd[0].Text + "~" + Refusetd[1].Text + "~" + Refusetd[2].Text + "~" + Refusetd[3].Text + "~" + Refusetd[4].Text + "~" + Refusetd[5].Text + "~" + Refusetd[6].Text + "~" + Refusetd[7].Text + "~" + Refusetd[8].Text + "~" + Refusetd[9].Text + "~" + Refusetd[10].Text;
                                            gc.insert_date(orderNumber, Parcel_Id, id, RefuseBegresult, 1, DateTime.Now);
                                        }
                                    }
                                }
                                if (strChoose == "SEWER")
                                {

                                    //IWebElement Sewer = driver.FindElement(By.Id("ui-id-3"));
                                    IWebElement Sewer = driver.FindElement(By.LinkText("SEWER"));
                                    Sewer.Click();
                                    Thread.Sleep(2000);
                                    gc.CreatePdf(orderNumber, Parcel_Id, "Sewer", driver, "NJ", countynameNJ);
                                    IWebElement Sewertable = driver.FindElement(By.Id("tab-S"));
                                    IList<IWebElement> Sewertr = Sewertable.FindElements(By.TagName("tr"));
                                    IList<IWebElement> Sewertd;
                                    Pid = db.ExecuteScalar("SELECT id FROM data_field_master where State_County_ID='" + statecountyid + "'and Category_Id='20'");
                                    id = Convert.ToInt32(Pid);

                                    foreach (IWebElement SewerBeg in Sewertr)
                                    {
                                        Sewertd = SewerBeg.FindElements(By.TagName("td"));
                                        if (!SewerBeg.Text.Contains("Year") && Sewertd.Count > 1)
                                        {
                                            string SewerBegresult = Sewertd[0].Text + "~" + Sewertd[1].Text + "~" + Sewertd[2].Text + "~" + Sewertd[3].Text + "~" + Sewertd[4].Text + "~" + Sewertd[5].Text + "~" + Sewertd[6].Text + "~" + Sewertd[7].Text + "~" + Sewertd[8].Text + "~" + Sewertd[9].Text + "~" + Sewertd[10].Text;
                                            gc.insert_date(orderNumber, Parcel_Id, id, SewerBegresult, 1, DateTime.Now);
                                        }
                                    }
                                }
                            }
                        }
                        catch { }
                        try
                        {
                            driver.Navigate().GoToUrl(taxCollectorlink);
                            Thread.Sleep(7000);
                            gc.CreatePdf(orderNumber, Parcel_Id, "Tax Authority", driver, "NJ", countynameNJ);
                            NJ_TaxAuthority TaxCollectornj = new Scrapsource.NJ_TaxAuthority();
                            //tax Authority
                            string taxAuthority = TaxCollectornj.TaxCollector(driver, District, orderNumber, Parcel_Id, countynameNJ);

                            Pid = db.ExecuteScalar("SELECT id FROM data_field_master where State_County_ID='" + statecountyid + "'and Category_Id='4'");
                            id = Convert.ToInt32(Pid);

                            gc.insert_date(orderNumber, Parcel_Id, id, taxAuthority, 1, DateTime.Now);
                        }
                        catch
                        { }
                    }
                    #endregion
                    #region three Tax Link   //Thillai
                    if (countlink == "3")
                    {
                        driver.Navigate().GoToUrl(urltown);

                        driver.FindElement(By.Id("ctl00_MainContentPlaceHolder_searchValue0")).SendKeys(Pro_Block);
                        driver.FindElement(By.Id("ctl00_MainContentPlaceHolder_searchValue1")).SendKeys(Pro_Lot);
                        if (Pro_Qual.Contains("HM"))
                        {
                            Pro_Qual = Pro_Qual.Replace("HM", "");
                            driver.FindElement(By.Id("ctl00_MainContentPlaceHolder_searchValue2")).SendKeys(Pro_Qual);
                        }
                        gc.CreatePdf(orderNumber, Parcel_Id, "Tax Input", driver, "NJ", countynameNJ);
                        driver.FindElement(By.Id("ctl00_MainContentPlaceHolder_search")).SendKeys(Keys.Enter);
                        Thread.Sleep(4000);
                        gc.CreatePdf(orderNumber, Parcel_Id, "Tax Info", driver, "NJ", countynameNJ);
                        try
                        {
                            int MAX = 0;
                            IWebElement Multitaxtable = driver.FindElement(By.Id("ctl00_MainContentPlaceHolder_gvwSearchResults"));
                            IList<IWebElement> MultitaxRow = Multitaxtable.FindElements(By.TagName("tr"));
                            IList<IWebElement> Multitaxid;
                            if (MultitaxRow.Count() > 2)
                            {
                                HttpContext.Current.Session["LinkThree" + countynameNJ] = "Maximum";
                                driver.Quit();
                                return "MultiParcel In Tax Site";
                            }

                        }
                        catch { }
                        try
                        {
                            //No Data Found
                            nodata = driver.FindElement(By.Id("ctl00_MainContentPlaceHolder_gvwSearchResults")).Text;
                            if (nodata.Contains("No records"))
                            {
                                HttpContext.Current.Session["NoTax_NJ" + countynameNJ] = "No_Tax";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }


                        string Block = "", Lot = "", Qualifier = "", Account = "", Name = "", Location = "", Address = "", City = "", Zip = "";
                        string vclass = "", AdditionalLots = "", BuildingDesc = "", LandSize = "", Zoning = "", Mappage = "", BankCode = "", taxdata = "";
                        taxdata = driver.FindElement(By.XPath("//*[@id='ctl00_MainContentPlaceHolder_tblContent']/tbody")).Text.Replace("\r\n", " ");
                        Block = gc.Between(taxdata, "Block", "Lot").Trim();
                        Lot = gc.Between(taxdata, "Lot", "Qualifier").Trim();
                        Qualifier = gc.Between(taxdata, "Qualifier", "Account").Trim();
                        Account = gc.Between(taxdata, "Account", "Name").Trim();
                        Name = gc.Between(taxdata, "Name", "Location").Trim();
                        Location = gc.Between(taxdata, "Location", "Address").Trim();
                        Address = gc.Between(taxdata, "Address", "City/State").Trim();
                        if (Address.Contains("Address"))
                        {
                            Address = GlobalClass.After(Address, "Address");
                        }
                        City = gc.Between(taxdata, "City/State", "Zip").Trim();
                        Zip = gc.Between(taxdata, "Zip", "Class").Trim();
                        vclass = gc.Between(taxdata, "Class", "Additional Lots").Trim();
                        AdditionalLots = gc.Between(taxdata, "Additional Lots", "Building Description").Trim();
                        BuildingDesc = gc.Between(taxdata, "Building Description", "Land Size").Trim();
                        LandSize = gc.Between(taxdata, "Land Size", "Zoning").Trim();
                        Zoning = gc.Between(taxdata, "Zoning", "Map Page").Trim();
                        Mappage = gc.Between(taxdata, "Map Page", "Bank Code").Trim();
                        BankCode = gc.Between(taxdata, "Bank Code", "Year").Trim();
                        string currentamount_due = "";
                        currentamount_due = driver.FindElement(By.Id("ctl00_MainContentPlaceHolder_ucCurrentAmountDue_lblCurrentAmountDue")).Text;

                        string TaxInfodetails = Block + "~" + Lot + "~" + Qualifier + "~" + Account + "~" + Name + "~" + Location + "~" + Address + "~" + City + "~" + Zip + "~" + vclass + "~" + AdditionalLots + "~" + BuildingDesc + "~" + LandSize + "~" + Zoning + "~" + Mappage + "~" + BankCode + "~" + currentamount_due;

                        Pid = db.ExecuteScalar("SELECT id FROM data_field_master where State_County_ID='" + statecountyid + "'and Category_Id='21'");
                        id = Convert.ToInt32(Pid);

                        gc.insert_date(orderNumber, Parcel_Id, id, TaxInfodetails, 1, DateTime.Now);

                        try
                        {
                            IWebElement TaxHistory = driver.FindElement(By.XPath("//*[@id='ucTaxBillSummary_gvwDisplay']/tbody"));
                            IList<IWebElement> TRTaxHistory = TaxHistory.FindElements(By.TagName("tr"));
                            IList<IWebElement> THTaxHistory = TaxHistory.FindElements(By.TagName("th"));
                            IList<IWebElement> TDTaxHistory;
                            foreach (IWebElement row in TRTaxHistory)
                            {
                                TDTaxHistory = row.FindElements(By.TagName("td"));
                                if (TDTaxHistory.Count != 0 && !row.Text.Contains("Due Date") && row.Text.Trim() != "")
                                {
                                    string TaxHistorydetails = TDTaxHistory[0].Text + "~" + TDTaxHistory[1].Text + "~" + TDTaxHistory[2].Text + "~" + TDTaxHistory[3].Text + "~" + TDTaxHistory[4].Text + "~" + TDTaxHistory[5].Text;

                                    Pid = db.ExecuteScalar("SELECT id FROM data_field_master where State_County_ID='" + statecountyid + "'and Category_Id='22'");
                                    id = Convert.ToInt32(Pid);

                                    gc.insert_date(orderNumber, Parcel_Id, id, TaxHistorydetails, 1, DateTime.Now);
                                }
                            }
                        }
                        catch { }
                        driver.Navigate().GoToUrl(taxCollectorlink);
                        Thread.Sleep(6000);
                        gc.CreatePdf(orderNumber, Parcel_Id, "Tax Authority", driver, "NJ", countynameNJ);
                        NJ_TaxAuthority TaxCollectornj = new Scrapsource.NJ_TaxAuthority();
                        //tax Authority
                        string taxAuthority = TaxCollectornj.TaxCollector(driver, District, orderNumber, Parcel_Id, countynameNJ);

                        Pid = db.ExecuteScalar("SELECT id FROM data_field_master where State_County_ID='" + statecountyid + "'and Category_Id='4'");
                        id = Convert.ToInt32(Pid);

                        gc.insert_date(orderNumber, Parcel_Id, id, taxAuthority, 1, DateTime.Now);

                    }
                    #endregion
                    #region four Tax Link  //deva
                    if (countlink == "4")
                    {
                        driver.Navigate().GoToUrl(urltown);
                        if (Pro_Block.Contains("."))
                        {
                            string[] Blok1 = Pro_Block.Split('.');
                            driver.FindElement(By.Id("Block")).SendKeys(Blok1[0]);
                            driver.FindElement(By.Id("SubBlock")).SendKeys(Blok1[1]);
                        }
                        else
                        {
                            driver.FindElement(By.Id("Block")).SendKeys(Pro_Block);
                        }
                        if (Pro_Lot.Contains("."))
                        {
                            string[] lot1 = Pro_Lot.Split('.');
                            driver.FindElement(By.Id("Lot")).SendKeys(lot1[0]);
                            driver.FindElement(By.Id("SubLot")).SendKeys(lot1[1]);
                        }
                        else
                        {
                            driver.FindElement(By.Id("Lot")).SendKeys(Pro_Lot);
                        }
                        driver.FindElement(By.Id("OwnerLast")).SendKeys(Pro_Owner);
                        gc.CreatePdf(orderNumber, Parcel_Id, "Block Search", driver, "NJ", countynameNJ);

                        driver.FindElement(By.Name("continue_step2")).Click();
                        Thread.Sleep(4000);
                        gc.CreatePdf(orderNumber, Parcel_Id, "Block Search After", driver, "NJ", countynameNJ);
                        string TaxInfomation = "";
                        try
                        {                                             //*[@id="page-body"]/div[2]/article/section/div[2]/ul[1]
                            TaxInfomation = driver.FindElement(By.XPath("//*[@id='main-content']/article/section/div[2]/ul[1]")).Text;
                        }
                        catch { }
                        try
                        {
                            TaxInfomation = driver.FindElement(By.XPath("//*[@id='page-body']/div[2]/article/section/div[2]/ul[1]")).Text;
                        }
                        catch { }
                        string AccountNo = "";
                        try
                        {
                            AccountNo = gc.Between(TaxInfomation, "Account No.:", "Block-Lot:");
                        }
                        catch { }

                        string BlockLot = gc.Between(TaxInfomation, "Block-Lot:", "Property Location:");
                        string PropertyLocation = gc.Between(TaxInfomation, "Property Location:", "Property Zip:");
                        string PropertyZip = gc.Between(TaxInfomation, "Property Zip:", "Property Owner:");
                        string PropertyOwner = GlobalClass.After(TaxInfomation, "Property Owner:");
                        string Taxinforesult = AccountNo + "~" + BlockLot + "~" + PropertyLocation + "~" + PropertyZip + "~" + PropertyOwner;
                        Pid = db.ExecuteScalar("SELECT id FROM data_field_master where State_County_ID='" + statecountyid + "'and Category_Id='23'");
                        id = Convert.ToInt32(Pid);
                        gc.insert_date(orderNumber, Parcel_Id, id, Taxinforesult, 1, DateTime.Now);
                        string Originaltax = "";
                        try
                        {
                            Originaltax = driver.FindElement(By.XPath("//*[@id='main-content']/article/section/div[2]/ul[2]/li[1]")).Text;
                        }
                        catch { }
                        try
                        {
                            Originaltax = driver.FindElement(By.XPath("//*[@id='page-body']/div[2]/article/section/div[2]/ul[2]/li[1]")).Text;
                        }
                        catch { }
                        string[] Oringinalarray = Originaltax.Split('\r');
                        Pid = db.ExecuteScalar("SELECT id FROM data_field_master where State_County_ID='" + statecountyid + "'and Category_Id='24'");
                        id = Convert.ToInt32(Pid);
                        string[] first = Oringinalarray[1].Split(':');
                        string Firstqtr1 = first[0];
                        string firstqtr2 = first[1];
                        gc.insert_date(orderNumber, Parcel_Id, id, Firstqtr1 + "~" + firstqtr2, 1, DateTime.Now);
                        string[] Sec = Oringinalarray[2].Split(':');
                        string Secound1 = Sec[0];
                        string Secound2 = Sec[1];
                        string Secound = Secound1 + "~" + Secound2;
                        gc.insert_date(orderNumber, Parcel_Id, id, Secound, 1, DateTime.Now);
                        string[] tre = Oringinalarray[3].Split(':');
                        string three1 = tre[0];
                        string three2 = tre[1];
                        string Three = three1 + "~" + three2;
                        gc.insert_date(orderNumber, Parcel_Id, id, Three, 1, DateTime.Now);
                        string[] fou = Oringinalarray[4].Split(':');
                        string Four1 = fou[0];
                        string Four2 = fou[1];
                        string foure = Four1 + "~" + Four2;
                        gc.insert_date(orderNumber, Parcel_Id, id, foure, 1, DateTime.Now);

                        IWebElement CurrentAmounttable = driver.FindElement(By.Id("taxamounttable"));
                        IList<IWebElement> CurrentAmountRow = CurrentAmounttable.FindElements(By.TagName("tr"));
                        IList<IWebElement> TDCurrentAmount;
                        Pid = db.ExecuteScalar("SELECT id FROM data_field_master where State_County_ID='" + statecountyid + "'and Category_Id='25'");
                        id = Convert.ToInt32(Pid);
                        foreach (IWebElement CurrentAmount in CurrentAmountRow)
                        {
                            TDCurrentAmount = CurrentAmount.FindElements(By.TagName("td"));
                            if (TDCurrentAmount.Count == 4 && !CurrentAmount.Text.Contains("Type") && CurrentAmount.Text.Trim() != "")
                            {
                                string CurrentAmountResult = TDCurrentAmount[0].Text + "~" + TDCurrentAmount[1].Text + "~" + TDCurrentAmount[2].Text + "~" + TDCurrentAmount[3].Text;
                                gc.insert_date(orderNumber, Parcel_Id, id, CurrentAmountResult, 1, DateTime.Now);
                            }
                            if (TDCurrentAmount.Count == 2)
                            {
                                string CurrentAmountResult = TDCurrentAmount[0].Text + "~" + "" + "~" + TDCurrentAmount[1].Text + "~" + "";
                                gc.insert_date(orderNumber, Parcel_Id, id, CurrentAmountResult, 1, DateTime.Now);
                            }
                        }


                        driver.Navigate().GoToUrl(taxCollectorlink);
                        Thread.Sleep(6000);
                        gc.CreatePdf(orderNumber, Parcel_Id, "Tax Authority", driver, "NJ", countynameNJ);
                        NJ_TaxAuthority TaxCollectornj = new Scrapsource.NJ_TaxAuthority();
                        //tax Authority
                        string taxAuthority = TaxCollectornj.TaxCollector(driver, District, orderNumber, Parcel_Id, countynameNJ);

                        Pid = db.ExecuteScalar("SELECT id FROM data_field_master where State_County_ID='" + statecountyid + "'and Category_Id='4'");
                        id = Convert.ToInt32(Pid);

                        gc.insert_date(orderNumber, Parcel_Id, id, taxAuthority, 1, DateTime.Now);
                    }
                    #endregion
                    #region five Tax Link   //thillai
                    if (countlink == "5")
                    {
                        driver.Navigate().GoToUrl(urltown);
                        // driver.Navigate().GoToUrl(urltown);
                        driver.FindElement(By.Id("txtBlock")).SendKeys(Pro_Block);
                        driver.FindElement(By.Id("txtlot")).SendKeys(Pro_Lot);
                        driver.FindElement(By.Id("txtQual")).SendKeys(Pro_Qual);
                        gc.CreatePdf(orderNumber, Parcel_Id, "Tax Input", driver, "NJ", countynameNJ);
                        driver.FindElement(By.XPath("//*[@id='form1']/font/font/font/font/font/p[2]/font/input[1]")).SendKeys(Keys.Enter);
                        Thread.Sleep(4000);
                        gc.CreatePdf(orderNumber, Parcel_Id, "Tax Info", driver, "NJ", countynameNJ);
                        string current = driver.CurrentWindowHandle;
                        driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td[13]/span/a")).SendKeys(Keys.Enter);
                        Thread.Sleep(4000);


                        // driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td[13]/span/a")).SendKeys(Keys.Enter);
                        //  Thread.Sleep(4000);

                        driver.SwitchTo().Window(driver.WindowHandles.Last());
                        gc.CreatePdf(orderNumber, Parcel_Id, "Tax Info Details", driver, "NJ", countynameNJ);


                        driver.SwitchTo().Window(current);
                        Thread.Sleep(2000);
                        string fileName2 = "";
                        var chromeOptions1 = new ChromeOptions();

                        var downloadDirectory1 = ConfigurationManager.AppSettings["AutoPdf"];

                        chromeOptions1.AddUserProfilePreference("download.default_directory", downloadDirectory1);
                        chromeOptions1.AddUserProfilePreference("download.prompt_for_download", false);
                        chromeOptions1.AddUserProfilePreference("disable-popup-blocking", "true");
                        chromeOptions1.AddUserProfilePreference("plugins.always_open_pdf_externally", true);
                        var driver2 = new ChromeDriver(chromeOptions1);
                        driver2.Navigate().GoToUrl(driver.Url);
                        Thread.Sleep(4000);
                        string current2 = driver2.CurrentWindowHandle;
                        fileName2 = "guest";
                        int pcount = 0, count = 0;
                        IWebElement IParcelAssess = driver2.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td[13]"));
                        IList<IWebElement> IParcelAssessList = IParcelAssess.FindElements(By.TagName("a"));
                        foreach (IWebElement parcel in IParcelAssessList)
                        {
                            if (parcel.Text.Contains("Record") && pcount < 2)
                            {
                                parcel.Click();
                                Thread.Sleep(21000);
                                try
                                {
                                    gc.AutoDownloadFileSpokane(orderNumber, Parcel_Id, countynameNJ, "NJ", fileName2 + ".pdf");
                                }
                                catch { }
                                driver2.SwitchTo().Window(driver2.WindowHandles.Last());
                                //gc.CreatePdf(orderNumber, Parcel_Id, "Print Record" + pcount, driver2, "NJ", countynameNJ);
                                pcount++;
                                driver2.SwitchTo().Window(current2);
                                Thread.Sleep(2000);
                            }
                        }
                        driver2.SwitchTo().Window(current2);
                        Thread.Sleep(2000);



                        try
                        {

                            driver2.Quit();
                            Thread.Sleep(3000);
                            string FilePath = gc.filePath(orderNumber, Parcel_Id) + "guest.pdf";
                            PdfReader reader;
                            string pdfData;
                            string pdftext = "", pdftext2 = "";
                            try
                            {
                                reader = new PdfReader(FilePath);
                                String textFromPage = PdfTextExtractor.GetTextFromPage(reader, 1);
                                System.Diagnostics.Debug.WriteLine("" + textFromPage);

                                pdftext = textFromPage;
                            }
                            catch { }
                            try
                            {
                                reader = new PdfReader(FilePath);
                                String textFromPage2 = PdfTextExtractor.GetTextFromPage(reader, 2);
                                System.Diagnostics.Debug.WriteLine("" + textFromPage2);

                                pdftext2 = textFromPage2;
                            }
                            catch { }
                            // Property Information Details
                            try
                            {
                                string tableassess3 = gc.Between(pdftext, "Property Information", "Owner Information").Trim();
                                string[] tableArray3 = tableassess3.Split('\n');
                                Pid = db.ExecuteScalar("SELECT id FROM data_field_master where State_County_ID='" + statecountyid + "'and Category_Id='32'");
                                id = Convert.ToInt32(Pid);

                                string Block = "", Lot = "", Qualification = "", Deduction = "", TaxAccountNum = "", Senior = "", Dimension = "", Vet = "";
                                string PropertyLoc = "", Widow = "", PropertyClass = "", Survivor = "", BankCode = "", Disabled = "", BuildingDescript = "", DecAmount = "", AdditionalLots = "";

                                Block = gc.Between(tableassess3, "Block :", "Lot:");
                                Lot = gc.Between(tableassess3, "Lot:", "Qualification :");
                                Qualification = gc.Between(tableassess3, "Qualification :", "Deductions:");
                                Deduction = gc.Between(tableassess3, "Deductions:", "Tax Account Number :");
                                TaxAccountNum = gc.Between(tableassess3, "Tax Account Number :", "Senior:");
                                Senior = gc.Between(tableassess3, "Senior:", "Dimension :");
                                Dimension = gc.Between(tableassess3, "Dimension :", "Vet :");
                                Vet = gc.Between(tableassess3, "Vet :", "Property location :");
                                PropertyLoc = gc.Between(tableassess3, "Property location :", "Widow :");
                                Widow = gc.Between(tableassess3, "Widow :", "Property Class :");
                                PropertyClass = gc.Between(tableassess3, "Property Class :", "Survivor:");
                                Survivor = gc.Between(tableassess3, "Survivor:", "Bank code :");
                                BankCode = gc.Between(tableassess3, "Bank code :", "Disabled:").Replace("WELLS FARGO", "");
                                Disabled = gc.Between(tableassess3, "Disabled:", "Building Descript :");
                                BuildingDescript = gc.Between(tableassess3, "Building Descript :", "Deduction amount:");
                                DecAmount = gc.Between(tableassess3, "Deduction amount:", "Additional lots :");
                                AdditionalLots = GlobalClass.After(tableassess3, "Additional lots :");

                                string TaxInfodetails = Block + "~" + Lot + "~" + Qualification + "~" + Deduction + "~" + TaxAccountNum + "~" + Senior + "~" + Dimension + "~" + Vet + "~" + PropertyLoc + "~" + Widow + "~" + PropertyClass + "~" + Survivor + "~" + BankCode + "~" + Disabled + "~" + BuildingDescript + "~" + DecAmount + "~" + AdditionalLots;

                                Pid = db.ExecuteScalar("SELECT id FROM data_field_master where State_County_ID='" + statecountyid + "'and Category_Id='32'");
                                id = Convert.ToInt32(Pid);

                                gc.insert_date(orderNumber, Parcel_Id, id, TaxInfodetails, 1, DateTime.Now);
                            }
                            catch { }
                            //  Property Tax Information
                            try
                            {
                                string Net_Tax1 = "", Net_Tax2 = "", Total_Tax2 = "", LandVAlue = "", ImprovementValue = "", NetTaxableValue = "", Special_Tax_Codes = "", Special_Tax_Amount = "";
                                string PropertyTax = gc.Between(pdftext, "Property Tax Information", "Tax Quarter History: 2019").Trim();

                                Net_Tax1 = gc.Between(PropertyTax, "Net Tax :", "Land value:");
                                Net_Tax2 = gc.Between(PropertyTax, "Net Tax :", "Improvement value:");
                                Net_Tax2 = GlobalClass.After(Net_Tax2, "Net Tax :");
                                Total_Tax2 = gc.Between(PropertyTax, "Total Tax:", "Net taxable value:");
                                LandVAlue = gc.Between(PropertyTax, "Land value:", "Improvement value:");
                                LandVAlue = GlobalClass.Before(LandVAlue, "Net Tax :").Replace("2019", "").Trim();
                                ImprovementValue = gc.Between(PropertyTax, "Improvement value:", "Total Tax:");
                                ImprovementValue = GlobalClass.Before(ImprovementValue, "\n");
                                NetTaxableValue = gc.Between(PropertyTax, "Net taxable value:", "Special Tax codes :");
                                Special_Tax_Codes = gc.Between(PropertyTax, "Special Tax codes :", "Special Tax Amount :");
                                Special_Tax_Amount = GlobalClass.After(PropertyTax, "Special Tax Amount :");

                                string PropertyTaxdetails = Net_Tax1 + "~" + Net_Tax2 + "~" + Total_Tax2 + "~" + LandVAlue + "~" + ImprovementValue + "~" + NetTaxableValue + "~" + Special_Tax_Codes + "~" + Special_Tax_Amount;
                                Pid = db.ExecuteScalar("SELECT id FROM data_field_master where State_County_ID='" + statecountyid + "'and Category_Id='44'");
                                id = Convert.ToInt32(Pid);
                                gc.insert_date(orderNumber, Parcel_Id, id, PropertyTaxdetails, 1, DateTime.Now);
                            }
                            catch { }
                            // Tax Quarter History Details- 2019
                            try
                            {
                                string tableassess = gc.Between(pdftext, "Tax Quarter History:", "Balance Summary").Trim();
                                string[] tableArray = tableassess.Split('\n');
                                Pid = db.ExecuteScalar("SELECT id FROM data_field_master where State_County_ID='" + statecountyid + "'and Category_Id='33'");
                                id = Convert.ToInt32(Pid);
                                int count3 = tableArray.Length;
                                for (int i = 0; i < count3; i++)
                                {
                                    // 
                                    string a1 = tableArray[i].Replace(" ", "~");
                                    string[] rowarray = a1.Split('~');
                                    int tdcount = rowarray.Length;
                                    if (tdcount == 12)
                                    {
                                        int j = 0;
                                        string newrow1 = "" + "~" + rowarray[j] + " " + rowarray[j + 1] + " " + rowarray[j + 2] + "~" + rowarray[j + 3] + " " + rowarray[j + 4] + " " + rowarray[j + 5] + "~" + rowarray[j + 6] + " " + rowarray[j + 7] + " " + rowarray[j + 8] + "~" + rowarray[j + 9] + " " + rowarray[j + 10] + " " + rowarray[j + 11] + "~" + "";
                                        gc.insert_date(orderNumber, Parcel_Id, id, newrow1, 1, DateTime.Now);
                                    }
                                    else if (tdcount == 11)
                                    {
                                        int j = 0;
                                        string newrow2 = "" + "~" + rowarray[j] + " " + rowarray[j + 1] + "~" + rowarray[j + 2] + " " + rowarray[j + 3] + "~" + rowarray[j + 4] + " " + rowarray[j + 5] + "~" + rowarray[j + 6] + " " + rowarray[j + 7] + "~" + rowarray[j + 8] + " " + rowarray[j + 9] + " " + rowarray[j + 10];
                                        gc.insert_date(orderNumber, Parcel_Id, id, newrow2, 1, DateTime.Now);
                                    }
                                    else if (tdcount == 7)
                                    {
                                        int j = 0;
                                        string newrow3 = rowarray[j] + " " + rowarray[j + 1] + "~" + rowarray[j + 2] + "~" + rowarray[j + 3] + "~" + rowarray[j + 4] + "~" + rowarray[j + 5] + "~" + rowarray[j + 6];
                                        gc.insert_date(orderNumber, Parcel_Id, id, newrow3, 1, DateTime.Now);
                                    }
                                    else if (tdcount == 6 && !a1.Contains("Transferd"))
                                    {
                                        int j = 0;
                                        string newrow4 = rowarray[j] + "~" + rowarray[j + 1] + "~" + rowarray[j + 2] + "~" + rowarray[j + 3] + "~" + rowarray[j + 4] + "~" + rowarray[j + 5];
                                        gc.insert_date(orderNumber, Parcel_Id, id, newrow4, 1, DateTime.Now);
                                    }
                                    else if (tdcount == 4)
                                    {
                                        int j = 0;
                                        string new_row3 = rowarray[j] + " " + rowarray[j + 1] + "~" + rowarray[j + 2] + "~" + "" + "~" + "" + "~" + rowarray[j + 3] + "~" + "" + "~" + "";
                                        gc.insert_date(orderNumber, Parcel_Id, id, new_row3, 1, DateTime.Now);
                                    }
                                }
                            }
                            catch { }

                            // Over Payment History

                            string TransferdDate = "", OriginalAmount = "", CurrentBalance = "", PayDate = "", PayAmount = "", Checknum = "", Authority = "", TaxId = "", PayBlock = "", PayLot = "", PayQual = "";
                            //try
                            //{
                            //    string OverPayment = gc.Between(pdftext, "Over Payment History", "2019 Balance Summary").Trim();
                            //    string[] tableArray4 = OverPayment.Split('\n');
                            //    Pid = db.ExecuteScalar("SELECT id FROM data_field_master where State_County_ID='" + statecountyid + "'and Category_Id='45'");
                            //    id = Convert.ToInt32(Pid);
                            //    int count4 = tableArray4.Length;
                            //    for (int i = 0; i < count4; i++)
                            //    {
                            //        // 
                            //        string a1 = tableArray4[i].Replace(" ", "~");
                            //        string[] rowarray = a1.Split('~');
                            //        int tdcount = rowarray.Length;
                            //        if (tdcount == 3 && !a1.Contains("Information"))
                            //        {

                            //            int j = 0;
                            //            TransferdDate = rowarray[j];
                            //            OriginalAmount = rowarray[j + 1];
                            //            CurrentBalance = rowarray[j + 2];

                            //        }
                            //        if (tdcount == 9 && !a1.Contains("Information"))
                            //        {
                            //            int j = 0;

                            //            PayDate = rowarray[j + 2];
                            //            PayAmount = rowarray[j + 3];
                            //            Checknum = rowarray[j + 4];
                            //            Authority = rowarray[j + 5];
                            //            TaxId = rowarray[j + 6];
                            //            PayBlock = rowarray[j + 7];
                            //            PayLot = rowarray[j + 8];
                            //            try
                            //            {
                            //                PayQual = rowarray[j + 9];
                            //            }
                            //            catch { }
                            //        }
                            //    }

                            //    string newrow = TransferdDate + "~" + OriginalAmount + "~" + CurrentBalance + "~" + PayDate + "~" + PayAmount + "~" + Checknum + "~" + Authority + "~" + TaxId + "~" + PayBlock + "~" + PayLot + "~" + PayQual;
                            //    gc.insert_date(orderNumber, Parcel_Id, id, newrow, 1, DateTime.Now);

                            //}
                            //catch { }

                            // Balance Summary
                            try
                            {
                                string BalanceSummary = GlobalClass.After(pdftext, "Balance Summary").Trim();

                                string TotalDue = "", Bal_Sum_Paid = "", Adjust = "", Bal_Sum_Bal = "";

                                TotalDue = gc.Between(BalanceSummary, "Totals Due:", "Paid :");
                                Bal_Sum_Paid = gc.Between(BalanceSummary, "Paid :", "Adjust:");
                                Adjust = gc.Between(BalanceSummary, "Adjust:", "Bal:");
                                Bal_Sum_Bal = GlobalClass.After(BalanceSummary, "Bal:");
                                if (Bal_Sum_Bal.Contains("\n"))
                                {
                                    try
                                    {
                                        Bal_Sum_Bal = GlobalClass.Before(Bal_Sum_Bal, "\n").Trim();
                                    }
                                    catch { }
                                }

                                string BalanceSummaryDetails = TotalDue + "~" + Bal_Sum_Paid + "~" + Adjust + "~" + Bal_Sum_Bal;

                                Pid = db.ExecuteScalar("SELECT id FROM data_field_master where State_County_ID='" + statecountyid + "'and Category_Id='46'");
                                id = Convert.ToInt32(Pid);
                                gc.insert_date(orderNumber, Parcel_Id, id, BalanceSummaryDetails, 1, DateTime.Now);
                            }
                            catch { }


                            // Transaction History
                            string tableassess2 = "";
                            try
                            {
                                tableassess2 = gc.Between(pdftext, "Transaction History", "Summary of Transactions").Trim();
                            }
                            catch { }
                            try
                            {
                                if (tableassess2 == "")
                                {
                                    tableassess2 = gc.Between(pdftext2, "Transaction History", "Summary of Transactions").Trim();
                                }
                            }
                            catch { }
                            try
                            {

                                string[] tableArray2 = tableassess2.Split('\n');
                                Pid = db.ExecuteScalar("SELECT id FROM data_field_master where State_County_ID='" + statecountyid + "'and Category_Id='34'");
                                id = Convert.ToInt32(Pid);
                                int count2 = tableArray2.Length;
                                for (int i = 0; i < count2; i++)
                                {
                                    // 
                                    string a1 = tableArray2[i].Replace(" ", "~");
                                    string[] rowarray = a1.Split('~');
                                    int tdcount = rowarray.Length;
                                    if (tdcount == 12 && !a1.Contains("Interest"))
                                    {
                                        int j = 0;
                                        string new_row = rowarray[j] + " " + rowarray[j + 1] + "~" + rowarray[j + 2] + " " + rowarray[j + 3] + "~" + rowarray[j + 4] + " " + rowarray[j + 5] + "~" + rowarray[j + 6] + "~" + rowarray[j + 7] + " " + rowarray[j + 8] + "~" + rowarray[j + 9] + " " + rowarray[j + 10] + "~" + rowarray[j + 11];
                                        gc.insert_date(orderNumber, Parcel_Id, id, new_row, 1, DateTime.Now);
                                    }

                                    else if (tdcount == 8)
                                    {
                                        int j = 0;
                                        string new_row2 = rowarray[j] + " " + rowarray[j + 1] + "~" + rowarray[j + 2] + "~" + rowarray[j + 3] + "~" + rowarray[j + 4] + "~" + rowarray[j + 5] + "~" + rowarray[j + 6] + "~" + rowarray[j + 7];
                                        gc.insert_date(orderNumber, Parcel_Id, id, new_row2, 1, DateTime.Now);
                                    }
                                    else if (tdcount == 7)
                                    {
                                        int j = 0;
                                        string new_row3 = rowarray[j] + " " + rowarray[j + 1] + "~" + rowarray[j + 2] + "~" + rowarray[j + 3] + "~" + rowarray[j + 4] + "~" + rowarray[j + 5] + "~" + rowarray[j + 6] + "~" + "";
                                        gc.insert_date(orderNumber, Parcel_Id, id, new_row3, 1, DateTime.Now);
                                    }
                                    else if (tdcount == 4)
                                    {
                                        int j = 0;
                                        string new_row3 = rowarray[j] + " " + rowarray[j + 1] + "~" + rowarray[j + 2] + "~" + "" + "~" + "" + "~" + rowarray[j + 3] + "~" + "" + "~" + "";
                                        gc.insert_date(orderNumber, Parcel_Id, id, new_row3, 1, DateTime.Now);
                                    }

                                    else if (tdcount == 3)
                                    {
                                        if (a1.Contains("0.00"))
                                        {
                                            int j = 0;
                                            string new_row4 = rowarray[j] + " " + rowarray[j + 1] + "~" + rowarray[j + 2] + "~" + "" + "~" + "" + "~" + "" + "~" + " " + "~" + " ";
                                            gc.insert_date(orderNumber, Parcel_Id, id, new_row4, 1, DateTime.Now);
                                        }
                                        else if (a1.Contains("Tax Year"))
                                        {
                                            int j = 0;
                                            string new_row5 = rowarray[j] + " " + rowarray[j + 1] + " " + rowarray[j + 2] + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + " " + "~" + " ";
                                            gc.insert_date(orderNumber, Parcel_Id, id, new_row5, 1, DateTime.Now);
                                        }
                                        else
                                        {
                                            int j = 0;
                                            string new_row6 = rowarray[j] + " " + rowarray[j + 1] + "~" + rowarray[j + 2] + "~" + "" + "~" + "" + "~" + "" + "~" + " " + "~" + " ";
                                            gc.insert_date(orderNumber, Parcel_Id, id, new_row6, 1, DateTime.Now);
                                        }
                                    }

                                }

                            }
                            catch { }
                            // Summary of Transactions
                            string tableassess5 = "";
                            try
                            {
                                tableassess5 = GlobalClass.After(pdftext, "Summary of Transactions").Trim();
                            }
                            catch { }
                            try
                            {
                                if (tableassess5 == "")
                                {
                                    tableassess5 = GlobalClass.After(pdftext2, "Summary of Transactions").Trim();
                                }
                            }
                            catch { }

                            try
                            {

                                string[] tableArray5 = tableassess5.Split('\n');
                                Pid = db.ExecuteScalar("SELECT id FROM data_field_master where State_County_ID='" + statecountyid + "'and Category_Id='47'");
                                id = Convert.ToInt32(Pid);
                                int count5 = tableArray5.Length;
                                for (int i = 0; i < count5; i++)
                                {
                                    // 
                                    string a1 = tableArray5[i].Replace(" ", "~");
                                    string[] rowarray = a1.Split('~');
                                    int tdcount = rowarray.Length;
                                    if (tdcount == 3)
                                    {
                                        int j = 0;
                                        string newrow11 = "" + "~" + rowarray[j] + "~" + rowarray[j + 1] + "~" + "" + "~" + rowarray[j + 2];
                                        gc.insert_date(orderNumber, Parcel_Id, id, newrow11, 1, DateTime.Now);
                                    }
                                    if (tdcount == 4)
                                    {
                                        try
                                        {
                                            int j = 0;
                                            string newrow12 = "" + "~" + rowarray[j] + "~" + rowarray[j + 1] + "~" + rowarray[j + 2] + "~" + rowarray[j + 3];
                                            gc.insert_date(orderNumber, Parcel_Id, id, newrow12, 1, DateTime.Now);
                                        }
                                        catch { }
                                    }
                                    if (tdcount == 5 && !a1.Contains("paid"))
                                    {
                                        try
                                        {
                                            int j = 0;
                                            string newrow13 = rowarray[j] + "~" + rowarray[j + 1] + "~" + rowarray[j + 2] + "~" + rowarray[j + 3] + "~" + rowarray[j + 4];
                                            gc.insert_date(orderNumber, Parcel_Id, id, newrow13, 1, DateTime.Now);
                                        }
                                        catch { }
                                    }
                                }
                            }
                            catch { }
                            try
                            {
                                string tableassess6 = GlobalClass.After(pdftext2, "Interest calculation").Trim();
                                string[] tableArray6 = tableassess6.Split('\n');
                                Pid = db.ExecuteScalar("SELECT id FROM data_field_master where State_County_ID='" + statecountyid + "'and Category_Id='48'");
                                id = Convert.ToInt32(Pid);
                                int count5 = tableArray6.Length;
                                for (int i = 0; i < count5; i++)
                                {
                                    // 
                                    string a1 = tableArray6[i].Replace(" ", "~");
                                    string[] rowarray = a1.Split('~');
                                    int tdcount = rowarray.Length;
                                    if (tdcount == 8)
                                    {
                                        int j = 0;
                                        string newrow11 = rowarray[j] + "~" + rowarray[j + 1] + "~" + rowarray[j + 2] + "~" + rowarray[j + 3] + "~" + rowarray[j + 4] + "~" + rowarray[j + 5] + "~" + rowarray[j + 6] + "~" + rowarray[j + 7];
                                        gc.insert_date(orderNumber, Parcel_Id, id, newrow11, 1, DateTime.Now);
                                    }
                                    if (tdcount == 9)
                                    {
                                        try
                                        {
                                            if (!a1.Contains("Delinquent"))
                                            {
                                                int j = 0;
                                                string newrow12 = rowarray[j] + " " + rowarray[j + 1] + " " + rowarray[j + 2] + "~" + rowarray[j + 3] + "~" + rowarray[j + 4] + "~" + " " + "~" + rowarray[j + 5] + "~" + rowarray[j + 6] + "~" + rowarray[j + 7] + "~" + rowarray[j + 8];
                                                gc.insert_date(orderNumber, Parcel_Id, id, newrow12, 1, DateTime.Now);
                                            }
                                            else if (a1.Contains("Delinquent"))
                                            {
                                                int j = 0;
                                                string newrow12 = rowarray[j] + " " + rowarray[j + 1] + "~ " + " " + "~ " + rowarray[j + 2] + "~" + " " + "~" + rowarray[j + 3] + "~" + rowarray[j + 4] + "~" + rowarray[j + 5] + "~" + rowarray[j + 6] + " " + rowarray[j + 7] + " " + rowarray[j + 8];
                                                gc.insert_date(orderNumber, Parcel_Id, id, newrow12, 1, DateTime.Now);
                                            }

                                        }
                                        catch { }
                                    }
                                    if (tdcount == 7)
                                    {
                                        try
                                        {
                                            int j = 0;
                                            string newrow13 = rowarray[j] + "~" + rowarray[j + 1] + "~" + rowarray[j + 2] + "~" + rowarray[j + 3] + "~" + " " + "~" + rowarray[j + 4] + "~" + rowarray[j + 5] + "~" + rowarray[j + 6];
                                            gc.insert_date(orderNumber, Parcel_Id, id, newrow13, 1, DateTime.Now);
                                        }
                                        catch { }
                                    }

                                    if (tdcount == 5 && !a1.Contains("Total Due"))
                                    {
                                        try
                                        {
                                            int j = 0;
                                            string newrow13 = rowarray[j] + " " + rowarray[j + 1] + " " + rowarray[j + 2] + "~" + rowarray[j + 3] + " " + rowarray[j + 4];
                                            gc.insert_date(orderNumber, Parcel_Id, id, newrow13, 1, DateTime.Now);
                                        }
                                        catch { }
                                    }

                                }
                            }
                            catch { }

                        }
                        catch { }


                        driver.Navigate().GoToUrl(taxCollectorlink);
                        Thread.Sleep(5000);
                        gc.CreatePdf(orderNumber, Parcel_Id, "Tax Authority", driver, "NJ", countynameNJ);
                        NJ_TaxAuthority TaxCollectornj = new Scrapsource.NJ_TaxAuthority();
                        //tax Authority
                        string taxAuthority = TaxCollectornj.TaxCollector(driver, District, orderNumber, Parcel_Id, countynameNJ);

                        Pid = db.ExecuteScalar("SELECT id FROM data_field_master where State_County_ID='" + statecountyid + "'and Category_Id='4'");
                        id = Convert.ToInt32(Pid);

                        gc.insert_date(orderNumber, Parcel_Id, id, taxAuthority, 1, DateTime.Now);

                    }


                    #endregion
                    #region six Tax Link //thillai
                    if (countlink == "6")
                    {
                        driver.Navigate().GoToUrl(urltown);
                        driver.FindElement(By.Id("Block")).SendKeys(Pro_Block);
                        driver.FindElement(By.Id("Lot")).SendKeys(Pro_Lot);
                        driver.FindElement(By.Id("Qualifier")).SendKeys(Pro_Qual);
                        gc.CreatePdf(orderNumber, Parcel_Id, "Tax Input", driver, "NJ", countynameNJ);
                        driver.FindElement(By.XPath("/html/body/form/div[2]/div/input")).SendKeys(Keys.Enter);
                        Thread.Sleep(4000);
                        gc.CreatePdf(orderNumber, Parcel_Id, "Tax Info", driver, "NJ", countynameNJ);
                        driver.FindElement(By.LinkText("View / Pay")).SendKeys(Keys.Enter);
                        Thread.Sleep(4000);
                        gc.CreatePdf(orderNumber, Parcel_Id, "Tax Info Details", driver, "NJ", countynameNJ);
                        string Account = "", Owner = "", Address = "", City = "", Location = "", BLQ = "", Bankcode = "";
                        string Deductions = "", Intdate = "", Lpaydate = "", Principal = "", Interest = "", Total = "";
                        Account = driver.FindElement(By.XPath("/html/body/div[3]/div[2]/span")).Text;
                        Owner = driver.FindElement(By.XPath("/html/body/div[4]/div[2]/span")).Text;
                        Address = driver.FindElement(By.XPath("/html/body/div[5]/div[2]/span")).Text;
                        City = driver.FindElement(By.XPath("/html/body/div[6]/div[2]/span")).Text;
                        Location = driver.FindElement(By.XPath("/html/body/div[7]/div[2]/span")).Text;
                        BLQ = driver.FindElement(By.XPath("/html/body/div[3]/div[4]/span")).Text;
                        Bankcode = driver.FindElement(By.XPath("/html/body/div[4]/div[4]/span")).Text;
                        Deductions = driver.FindElement(By.XPath("/html/body/div[5]/div[4]/span")).Text;
                        Intdate = driver.FindElement(By.XPath("/html/body/div[6]/div[4]/span")).Text;
                        Lpaydate = driver.FindElement(By.XPath("/html/body/div[7]/div[4]/span")).Text;
                        Principal = driver.FindElement(By.XPath("/html/body/div[3]/div[6]/span")).Text;
                        Interest = driver.FindElement(By.XPath("/html/body/div[4]/div[6]/span")).Text;
                        Total = driver.FindElement(By.XPath("/html/body/div[5]/div[6]/span")).Text;
                        string TaxInfodetails = Account + "~" + Owner + "~" + Address + "~" + City + "~" + Location + "~" + BLQ + "~" + Bankcode + "~" + Deductions + "~" + Intdate + "~" + Lpaydate + "~" + Principal + "~" + Interest + "~" + Total;

                        Pid = db.ExecuteScalar("SELECT id FROM data_field_master where State_County_ID='" + statecountyid + "'and Category_Id='26'");
                        id = Convert.ToInt32(Pid);

                        gc.insert_date(orderNumber, Parcel_Id, id, TaxInfodetails, 1, DateTime.Now);

                        // Tax History Details
                        try
                        {
                            IWebElement TaxHistory = driver.FindElement(By.XPath("/html/body/div[8]/div/table"));
                            IList<IWebElement> TRTaxHistory = TaxHistory.FindElements(By.TagName("tr"));
                            IList<IWebElement> THTaxHistory = TaxHistory.FindElements(By.TagName("th"));
                            IList<IWebElement> TDTaxHistory;
                            foreach (IWebElement row in TRTaxHistory)
                            {
                                TDTaxHistory = row.FindElements(By.TagName("td"));
                                if (TDTaxHistory.Count != 0 && !row.Text.Contains("Description") && row.Text.Trim() != "")
                                {
                                    string TaxHistorydetails = TDTaxHistory[0].Text + "~" + TDTaxHistory[1].Text + "~" + TDTaxHistory[2].Text + "~" + TDTaxHistory[3].Text + "~" + TDTaxHistory[4].Text + "~" + TDTaxHistory[5].Text + "~" + TDTaxHistory[6].Text + "~" + TDTaxHistory[7].Text + "~" + TDTaxHistory[8].Text + "~" + TDTaxHistory[9].Text;

                                    Pid = db.ExecuteScalar("SELECT id FROM data_field_master where State_County_ID='" + statecountyid + "'and Category_Id='27'");
                                    id = Convert.ToInt32(Pid);

                                    gc.insert_date(orderNumber, Parcel_Id, id, TaxHistorydetails, 1, DateTime.Now);
                                }
                            }
                        }
                        catch { }

                        // Tax Due Details

                        //try
                        //{
                        //    IWebElement TaxDue = driver.FindElement(By.XPath("/html/body/div[8]/div/table"));
                        //    IList<IWebElement> TRTaxDue = TaxDue.FindElements(By.TagName("tr"));
                        //    IList<IWebElement> THTaxHistoryTaxDue = TaxDue.FindElements(By.TagName("th"));
                        //    IList<IWebElement> TDTaxDue;
                        //    foreach (IWebElement row in TRTaxDue)
                        //    {
                        //        TDTaxDue = row.FindElements(By.TagName("td"));
                        //        if (TDTaxDue.Count != 0 && !row.Text.Contains("Certificate") && row.Text.Trim() != "")
                        //        {
                        //            string TaxHistorydetails = TDTaxDue[0].Text + "~" + TDTaxDue[1].Text + "~" + TDTaxDue[2].Text + "~" + TDTaxDue[3].Text + "~" + TDTaxDue[4].Text + "~" + TDTaxDue[5].Text + "~" + TDTaxDue[6].Text;

                        //            Pid = db.ExecuteScalar("SELECT id FROM data_field_master where State_County_ID='" + statecountyid + "'and Category_Id='28'");
                        //            id = Convert.ToInt32(Pid);

                        //            gc.insert_date(orderNumber, Parcel_Id, id, TaxHistorydetails, 1, DateTime.Now);
                        //        }
                        //    }
                        //}
                        //catch { }
                        gc.CreatePdf(orderNumber, Parcel_Id, "Tax History Details", driver, "NJ", countynameNJ);



                        driver.Navigate().GoToUrl(taxCollectorlink);
                        Thread.Sleep(5000);
                        gc.CreatePdf(orderNumber, Parcel_Id, "Tax Authority", driver, "NJ", countynameNJ);
                        NJ_TaxAuthority TaxCollectornj = new Scrapsource.NJ_TaxAuthority();
                        //tax Authority
                        string taxAuthority = TaxCollectornj.TaxCollector(driver, District, orderNumber, Parcel_Id, countynameNJ);

                        Pid = db.ExecuteScalar("SELECT id FROM data_field_master where State_County_ID='" + statecountyid + "'and Category_Id='4'");
                        id = Convert.ToInt32(Pid);

                        gc.insert_date(orderNumber, Parcel_Id, id, taxAuthority, 1, DateTime.Now);
                    }
                    #endregion
                    #region seven Tax Link //deva
                    if (countlink == "7")
                    {
                        driver.Navigate().GoToUrl(urltown);
                        driver.FindElement(By.Id("MainContent_txtBlock")).SendKeys(Pro_Block);
                        driver.FindElement(By.Id("MainContent_txtLot")).SendKeys(Pro_Lot);
                        driver.FindElement(By.Id("MainContent_txtQual")).SendKeys(Pro_Qual);
                        gc.CreatePdf(orderNumber, Parcel_Id, "Block Before", driver, "NJ", countynameNJ);
                        driver.FindElement(By.Id("MainContent_btnBLQ")).Click();
                        Thread.Sleep(2000);

                        gc.CreatePdf(orderNumber, Parcel_Id, "Block Search", driver, "NJ", countynameNJ);

                        int Taxmax = 0;
                        string searchresult = driver.FindElement(By.Id("MainContent_lblSearchResult")).Text;
                        string search1 = GlobalClass.After(searchresult, "of");
                        if (search1.Trim() != "1")
                        {
                            string Searchid = "";
                            IWebElement Bedminstertable1 = driver.FindElement(By.Id("MainContent_GridViewRadio"));
                            IList<IWebElement> Bedminstertr1 = Bedminstertable1.FindElements(By.TagName("tr"));
                            IList<IWebElement> Bedminstertd1;
                            foreach (IWebElement Bedminster1 in Bedminstertr1)
                            {
                                Bedminstertd1 = Bedminster1.FindElements(By.TagName("td"));
                                if (Bedminstertd1.Count != 0 && Pro_Owner.Trim().ToUpper().Contains(Bedminstertd1[2].Text.Trim().ToUpper()) && Pro_Location.Trim().ToUpper().Contains(Bedminstertd1[3].Text.Trim().ToUpper()))
                                {
                                    IWebElement getid = Bedminstertd1[0].FindElement(By.TagName("a"));
                                    Searchid = getid.GetAttribute("id");
                                    Taxmax++;
                                }
                            }
                            if (Taxmax == 1)
                            {
                                driver.FindElement(By.Id(Searchid)).Click();
                                Thread.Sleep(2000);
                            }
                            if (Taxmax > 1)
                            {
                                HttpContext.Current.Session["LinkSeven" + countynameNJ] = "Maximum";
                                driver.Quit();
                                return "MultiParcel";
                            }
                        }
                        else
                        {
                            IWebElement Bedminstertable = driver.FindElement(By.Id("MainContent_GridViewRadio"));
                            IList<IWebElement> Bedminstertr = Bedminstertable.FindElements(By.TagName("tr"));
                            IList<IWebElement> Bedminstertd;
                            foreach (IWebElement Bedminster in Bedminstertr)
                            {
                                Bedminstertd = Bedminster.FindElements(By.TagName("td"));
                                if (!Bedminster.Text.Contains("Owner Name") && Bedminstertd.Count != 0)
                                {
                                    IWebElement taxtable = Bedminstertd[0].FindElement(By.TagName("a"));
                                    //string Href = taxtable.GetAttribute("href");
                                    //driver.Navigate().GoToUrl(Href);
                                    taxtable.Click();
                                    Thread.Sleep(2000);

                                }
                            }
                        }

                        gc.CreatePdf(orderNumber, Parcel_Id, "Taxes", driver, "NJ", countynameNJ);
                        string Taxinfo = driver.FindElement(By.XPath("//*[@id='MainContent_FormView1']/tbody/tr[2]/td/table/tbody")).Text;
                        string NJCounty = gc.Between(Taxinfo, "NJ County", "NJ District");
                        string NJDistrict = gc.Between(Taxinfo, "NJ District", "Owner Name");
                        string OwnerName = gc.Between(Taxinfo, "Owner Name", "Block_Lot_Qual").Replace("pdf", "").Trim();
                        string Block_Lot_Qual = gc.Between(Taxinfo, "Block_Lot_Qual", "Owner Street");
                        string OwnerStreet = gc.Between(Taxinfo, "Owner Street", "Property Class");
                        string PropertyClass = gc.Between(Taxinfo, "Property Class", "Owner City");
                        string OwnerCity = gc.Between(Taxinfo, "Owner City", "Tax Map : Bank Code");
                        string BankCode = gc.Between(Taxinfo, "Bank Code", "Location");
                        string Location = gc.Between(Taxinfo, "Location", "Book-Page");
                        string AssessLand = gc.Between(Taxinfo, "Assess Land", "Land Value");
                        string LandValue = gc.Between(Taxinfo, "Land Value", "Assess Impr");
                        string AssessImpr = gc.Between(Taxinfo, "Assess Impr", "Impr Value");
                        string ImprValue = gc.Between(Taxinfo, "Impr Value", "Assess Total");
                        string AssessTotal = gc.Between(Taxinfo, "Assess Total", "Total Value");
                        string TotalValue = gc.Between(Taxinfo, "Total Value", "Add Lots");
                        string YearBuilt = gc.Between(Taxinfo, "Year Built", "Build Desc :");
                        string Exempts = gc.Between(Taxinfo, " Exempts", "Deductions");
                        string Deductions = gc.Between(Taxinfo, "Exempts", "Deductions");
                        string Book_Page = gc.Between(Taxinfo, "Book-Page", "Assess Land");
                        string Addlot = gc.Between(Taxinfo, " Add Lots", "Sale Price");
                        string SalePrice = gc.Between(Taxinfo, "Sale Price", "Land Desc");
                        string LandDesc = gc.Between(Taxinfo, "Land Desc", "Year Built");
                        string BuildDesc = gc.Between(Taxinfo, "Bld Cla", "Last Year Tax");
                        string LastYearTax = gc.Between(Taxinfo, "Last Year Tax", "Exempts");
                        string FloorArea = gc.Between(Taxinfo, "Floor Area", "Zone :");
                        string Zone = gc.Between(Taxinfo, "Cla4", "Refresh Date");
                        string RefreshDate = gc.Between(Taxinfo, "Refresh Date", "Last Update");
                        string LastUpdate = GlobalClass.After(Taxinfo, "Last Update");
                        string Taxinforesult = NJCounty + "~" + NJDistrict + "~" + OwnerName + "~" + Block_Lot_Qual + "~" + OwnerStreet + "~" + PropertyClass + "~" + OwnerCity + "~" + BankCode + "~" + Location + "~" + AssessLand + "~" + LandValue + "~" + AssessImpr + "~" + ImprValue + "~" + AssessTotal + "~" + TotalValue + "~" + YearBuilt + "~" + Exempts + "~" + Deductions + "~" + Book_Page + "~" + Addlot + "~" + SalePrice + "~" + LandDesc + "~" + BuildDesc + "~" + LastYearTax + "~" + FloorArea + "~" + Zone + "~" + RefreshDate + "~" + LastUpdate;

                        Pid = db.ExecuteScalar("SELECT id FROM data_field_master where State_County_ID='" + statecountyid + "'and Category_Id='29'");
                        id = Convert.ToInt32(Pid);
                        gc.insert_date(orderNumber, Parcel_Id, id, Taxinforesult, 1, DateTime.Now);
                        IWebElement taxesFoot = driver.FindElement(By.XPath("//*[@id='MainContent_GridViewBill']/tbody/tr[12]/td/table/tbody"));
                        IList<IWebElement> TaxFoottd = taxesFoot.FindElements(By.TagName("td"));
                        IJavaScriptExecutor js = driver as IJavaScriptExecutor;
                        for (int F = 1; F <= TaxFoottd.Count; F++)
                        {
                            if (F != 1)
                            {
                                IWebElement Taxesfirst = driver.FindElement(By.XPath("//*[@id='MainContent_GridViewBill']/tbody/tr[12]/td/table/tbody/tr/td[" + F + "]")).FindElement(By.TagName("a"));
                                js.ExecuteScript("arguments[0].click();", Taxesfirst);
                                Thread.Sleep(4000);
                            }

                            gc.CreatePdf(orderNumber, Parcel_Id, "Taxes" + F, driver, "NJ", countynameNJ);
                            IWebElement Taxestable = driver.FindElement(By.Id("MainContent_GridViewBill"));
                            IList<IWebElement> Taxesrow = Taxestable.FindElements(By.TagName("tr"));
                            IList<IWebElement> Taxesid;
                            Pid = db.ExecuteScalar("SELECT id FROM data_field_master where State_County_ID='" + statecountyid + "'and Category_Id='30'");
                            id = Convert.ToInt32(Pid);
                            foreach (IWebElement Taxes in Taxesrow)
                            {
                                Taxesid = Taxes.FindElements(By.TagName("td"));
                                if (Taxesid.Count > 5 && !Taxes.Text.Contains("Year"))
                                {
                                    string Taxesresult = Taxesid[0].Text + "~" + Taxesid[1].Text + "~" + Taxesid[2].Text + "~" + Taxesid[3].Text + "~" + Taxesid[4].Text + "~" + Taxesid[5].Text + "~" + Taxesid[6].Text + "~" + Taxesid[7].Text + "~" + Taxesid[8].Text + "~" + Taxesid[9].Text;
                                    gc.insert_date(orderNumber, Parcel_Id, id, Taxesresult, 1, DateTime.Now);
                                }
                            }
                        }

                        try
                        {
                            IWebElement Sewer = driver.FindElement(By.LinkText("Sewer"));
                            Sewer.Click();
                            Thread.Sleep(4000);
                            IWebElement Swerlisttable = driver.FindElement(By.XPath("//*[@id='MainContent_GridViewSewer']/tbody/tr[11]/td/table/tbody/tr"));
                            IList<IWebElement> SewerList = Swerlisttable.FindElements(By.TagName("td"));
                            for (int Sewercount = 1; Sewercount <= SewerList.Count(); Sewercount++)
                            {
                                if (Sewercount != 1)
                                {
                                    IWebElement Sewerclick = driver.FindElement(By.XPath("//*[@id='MainContent_GridViewSewer']/tbody/tr[11]/td/table/tbody/tr/td[" + Sewercount + "]/a"));
                                    js.ExecuteScript("arguments[0].click();", Sewerclick);
                                    Thread.Sleep(4000);
                                }
                                gc.CreatePdf(orderNumber, Parcel_Id, "Sewer" + Sewercount, driver, "NJ", countynameNJ);
                            }
                        }
                        catch { }
                        try
                        {
                            IWebElement OutLiens = driver.FindElement(By.LinkText("Out-Liens"));
                            OutLiens.Click();
                            Thread.Sleep(4000);
                            gc.CreatePdf(orderNumber, Parcel_Id, "Out Liens", driver, "NJ", countynameNJ);
                        }
                        catch { }
                        try
                        {
                            IWebElement MuniLiens = driver.FindElement(By.LinkText("Muni-Liens"));
                            MuniLiens.Click();
                            Thread.Sleep(4000);
                            gc.CreatePdf(orderNumber, Parcel_Id, "Muni Liens", driver, "NJ", countynameNJ);
                        }
                        catch { }
                        try
                        {
                            IWebElement Penalties = driver.FindElement(By.LinkText("Penalties"));
                            Penalties.Click();
                            Thread.Sleep(4000);
                            gc.CreatePdf(orderNumber, Parcel_Id, "Penalties", driver, "NJ", countynameNJ);
                        }
                        catch { }
                        try
                        {
                            IWebElement General = driver.FindElement(By.LinkText("General"));
                            General.Click();
                            Thread.Sleep(4000);
                            gc.CreatePdf(orderNumber, Parcel_Id, "General", driver, "NJ", countynameNJ);
                            string Generalinfo = driver.FindElement(By.Id("MainContent_FormView2")).Text.Replace("\r\n", " ");
                            string NJCountyDistrict = gc.Between(Generalinfo, "NJ County District", "Town Name").Trim();
                            string TownName = gc.Between(Generalinfo, "Town Name", "Collector Name");
                            string CollectorName = gc.Between(Generalinfo, "Collector Name", "Checks Payable To");
                            string ChecksPayableTo = gc.Between(Generalinfo, "Checks Payable To", "Municipal Address");
                            string MunicipalAddress = gc.Between(Generalinfo, "Municipal Address", "Municipal Phone").Trim();
                            string MunicipalPhone = GlobalClass.After(Generalinfo, "Municipal Phone");
                            gc.CreatePdf(orderNumber, Parcel_Id, "General", driver, "NJ", countynameNJ);
                            string Gendralresult = NJCountyDistrict + "~" + TownName + "~" + CollectorName + "~" + ChecksPayableTo + "~" + MunicipalAddress + "~" + MunicipalPhone;
                            Pid = db.ExecuteScalar("SELECT id FROM data_field_master where State_County_ID='" + statecountyid + "'and Category_Id='40'");
                            id = Convert.ToInt32(Pid);
                            gc.insert_date(orderNumber, Parcel_Id, id, Gendralresult, 1, DateTime.Now);
                        }
                        catch { }

                        //
                        try
                        {
                            IWebElement LastPayStub = driver.FindElement(By.LinkText("Last Pay Stub"));
                            LastPayStub.Click();

                            Thread.Sleep(4000);
                            gc.CreatePdf(orderNumber, Parcel_Id, "Last Pay Stub", driver, "NJ", countynameNJ);
                        }
                        catch { }
                        try
                        {
                            IWebElement LastPayStubtable = driver.FindElement(By.Id("MainContent_GridViewLastpay"));
                            IList<IWebElement> LastPayStubrow = LastPayStubtable.FindElements(By.TagName("tr"));
                            IList<IWebElement> LastPayStubid;
                            Pid = db.ExecuteScalar("SELECT id FROM data_field_master where State_County_ID='" + statecountyid + "'and Category_Id='31'");
                            id = Convert.ToInt32(Pid);
                            foreach (IWebElement LastPay in LastPayStubrow)
                            {
                                LastPayStubid = LastPay.FindElements(By.TagName("td"));
                                if (LastPayStubid.Count != 0 && !LastPay.Text.Contains("Pay Date"))
                                {
                                    string LastPayStubresult = LastPayStubid[0].Text + "~" + LastPayStubid[1].Text + "~" + LastPayStubid[2].Text + "~" + LastPayStubid[3].Text + "~" + LastPayStubid[4].Text + "~" + LastPayStubid[5].Text + "~" + LastPayStubid[6].Text;
                                    gc.insert_date(orderNumber, Parcel_Id, id, LastPayStubresult, 1, DateTime.Now);
                                }
                            }

                        }
                        catch { }

                        try
                        {
                            driver.Navigate().GoToUrl(taxCollectorlink);
                            Thread.Sleep(7000);
                            try
                            {
                                gc.CreatePdf(orderNumber, Parcel_Id, "Tax Authority", driver, "NJ", countynameNJ);
                            }
                            catch { }
                            NJ_TaxAuthority TaxCollectornj = new Scrapsource.NJ_TaxAuthority();
                            //tax Authority
                            string taxAuthority = TaxCollectornj.TaxCollector(driver, District, orderNumber, Parcel_Id, countynameNJ);
                            Pid = db.ExecuteScalar("SELECT id FROM data_field_master where State_County_ID='" + statecountyid + "'and Category_Id='4'");
                            id = Convert.ToInt32(Pid);

                            gc.insert_date(orderNumber, Parcel_Id, id, taxAuthority, 1, DateTime.Now);
                        }
                        catch { }
                    }
                    #endregion//
                    #region eight Tax Link
                    if (countlink == "8")//tamil
                    {
                        driver.Navigate().GoToUrl(urltown);
                        driver.FindElement(By.XPath("/html/body/div/div[2]/div[1]/div[1]/div/input")).SendKeys(Parcel_Id);
                        driver.FindElement(By.XPath("/html/body/div/div[2]/div[1]/div[1]/div/span[2]/button")).Click();
                        gc.CreatePdf(orderNumber, Parcel_Id, "Parcel Search", driver, "NJ", countynameNJ);
                        driver.FindElement(By.XPath("//*[@id='searchModal']/div/div/div[2]/div/div[1]/div[1]/div/label[4]/input")).Click();
                        driver.FindElement(By.XPath("//*[@id='searchModal']/div/div/div[2]/div/div[1]/div[2]/div/input")).SendKeys(Pro_Location);
                        gc.CreatePdf(orderNumber, Parcel_Id, "Addres Key", driver, "NJ", countynameNJ);
                        driver.FindElement(By.XPath("//*[@id='searchModal']/div/div/div[2]/div/div[1]/div[2]/div/span/button")).Click();
                        Thread.Sleep(3000);
                        driver.FindElement(By.XPath("//*[@id='searchModal']/div/div/div[2]/div/div[2]/table/tbody/tr/td[1]/button")).Click();
                        gc.CreatePdf(orderNumber, Parcel_Id, "Addres Key Result", driver, "NJ", countynameNJ);
                        string URAccountNumber = "", UROwner = "", URAddress = "", URCity = "", URLocation = "", URParcelId = "", URBank = "", URDeduction = "", URPrincipal = "", URInterest = "", URTotalDue = "", URAmountPay = "";
                        //Basic Information
                        Pid = db.ExecuteScalar("SELECT id FROM data_field_master where State_County_ID='" + statecountyid + "'and Category_Id='35'");
                        id = Convert.ToInt32(Pid);
                        IWebElement IBasic = driver.FindElement(By.XPath("/html/body/div/div[2]/div[4]/form/div[1]/div"));
                        IList<IWebElement> IBasicRow = IBasic.FindElements(By.TagName("div"));
                        IList<IWebElement> IBasicTD;
                        IList<IWebElement> IBasicValue;
                        foreach (IWebElement basicdetails in IBasicRow)
                        {
                            IBasicValue = basicdetails.FindElements(By.TagName("div"));
                            foreach (IWebElement basic in IBasicValue)
                            {
                                IBasicTD = basic.FindElements(By.TagName("div"));
                                if (IBasicTD.Count != 0)
                                {
                                    if (basic.Text.Contains("Account"))
                                    {
                                        URAccountNumber = IBasicTD[1].Text;
                                    }
                                    if (basic.Text.Contains("Owner"))
                                    {
                                        UROwner = IBasicTD[1].Text;
                                    }
                                    if (basic.Text.Contains("Address"))
                                    {
                                        URAddress = IBasicTD[1].Text;
                                    }
                                    if (basic.Text.Contains("City"))
                                    {
                                        URCity = IBasicTD[1].Text;
                                    }
                                    if (basic.Text.Contains("Location"))
                                    {
                                        URLocation = IBasicTD[1].Text;
                                    }
                                    if (basic.Text.Contains("B/L/Q"))
                                    {
                                        URParcelId = IBasicTD[1].Text;
                                    }
                                    if (basic.Text.Contains("Bank"))
                                    {
                                        URBank = IBasicTD[1].Text;
                                    }
                                    if (basic.Text.Contains("Deduction"))
                                    {
                                        URDeduction = IBasicTD[1].Text;
                                    }
                                    if (basic.Text.Contains("Principal"))
                                    {
                                        URPrincipal = IBasicTD[1].Text;
                                    }
                                    if (basic.Text.Contains("Interest"))
                                    {
                                        URInterest = IBasicTD[1].Text;
                                    }
                                    if (basic.Text.Contains("Total Due"))
                                    {
                                        URTotalDue = IBasicTD[1].Text;
                                    }
                                    if (basic.Text.Contains("Amt. to Pay"))
                                    {
                                        try
                                        {
                                            IWebElement IAmountPay = driver.FindElement(By.XPath("/html/body/div/div[2]/div[4]/form/div[1]/div/div[3]/div[4]/div[1]/div[2]/input"));
                                            URAmountPay = IAmountPay.GetAttribute("value");

                                        }
                                        catch { }
                                    }
                                }
                            }
                        }

                        string strBasicDetails = URAccountNumber + "~" + UROwner + "~" + URAddress + " " + URCity + "~" + URLocation + "~" + URParcelId + "~" + URBank + "~" + URDeduction + "~" + URPrincipal + "~" + URInterest + "~" + URTotalDue + "~" + URAmountPay;
                        gc.insert_date(orderNumber, URParcelId, id, strBasicDetails, 1, DateTime.Now);

                        //Yearly Summary
                        driver.FindElement(By.LinkText("Yearly Summary")).Click();
                        gc.CreatePdf(orderNumber, Parcel_Id, "Yearly Summary", driver, "NJ", countynameNJ);
                        Pid = db.ExecuteScalar("SELECT id FROM data_field_master where State_County_ID='" + statecountyid + "'and Category_Id='36'");
                        id = Convert.ToInt32(Pid);
                        IWebElement IYearlySummary = driver.FindElement(By.XPath("//*[@id='pane1']/div/div/table/tbody"));
                        IList<IWebElement> IYearlySummaryRow = IYearlySummary.FindElements(By.TagName("tr"));
                        IList<IWebElement> IYearlySummaryTD;
                        foreach (IWebElement summary in IYearlySummaryRow)
                        {
                            IYearlySummaryTD = summary.FindElements(By.TagName("td"));
                            if (IYearlySummaryTD.Count != 0)
                            {
                                string strYearSummaryDetails = IYearlySummaryTD[0].Text + "~" + IYearlySummaryTD[1].Text + "~" + IYearlySummaryTD[2].Text + "~" + IYearlySummaryTD[3].Text + "~" + IYearlySummaryTD[4].Text + "~" + IYearlySummaryTD[5].Text + "~" + IYearlySummaryTD[6].Text + "~" + IYearlySummaryTD[7].Text + "~" + IYearlySummaryTD[8].Text + "~" + IYearlySummaryTD[9].Text;
                                gc.insert_date(orderNumber, URParcelId, id, strYearSummaryDetails, 1, DateTime.Now);
                            }
                        }

                        //Details
                        driver.FindElement(By.LinkText("Details")).Click();
                        gc.CreatePdf(orderNumber, Parcel_Id, "Details", driver, "NJ", countynameNJ);
                        string URYear = "", URLand = "", URImprovement = "", URExemption = "", URNetTax = "", URDeductions = "", URBilled = "", URAdjusted = "", URPaid = "", UROpenBalance = "";
                        Pid = db.ExecuteScalar("SELECT id FROM data_field_master where State_County_ID='" + statecountyid + "'and Category_Id='37'");
                        id = Convert.ToInt32(Pid);
                        IWebElement IDetails = driver.FindElement(By.XPath("//*[@id='pane2']/div/table"));
                        IList<IWebElement> IDetailsRow = IDetails.FindElements(By.TagName("tr"));
                        IList<IWebElement> IDetailsTD;
                        foreach (IWebElement detail in IDetailsRow)
                        {
                            IDetailsTD = detail.FindElements(By.TagName("td"));
                            if (IDetailsTD.Count != 0)
                            {
                                string strYearSummaryDetails = IDetailsTD[0].Text + "~" + IDetailsTD[1].Text + "~" + IDetailsTD[2].Text + "~" + IDetailsTD[3].Text + "~" + IDetailsTD[4].Text + "~" + IDetailsTD[5].Text + "~" + IDetailsTD[6].Text + "~" + IDetailsTD[7].Text + "~" + IDetailsTD[8].Text + "~" + IDetailsTD[9].Text + "~" + IDetailsTD[10].Text;
                                gc.insert_date(orderNumber, URParcelId, id, strYearSummaryDetails, 1, DateTime.Now);
                            }
                        }

                        //Liens
                        //driver.FindElement(By.LinkText("Liens (0)")).Click();
                        IJavaScriptExecutor js = driver as IJavaScriptExecutor;
                        IWebElement ICharges = driver.FindElement(By.XPath("/html/body/div/div[2]/div[4]/form/div[2]/div/ul"));
                        IList<IWebElement> IChargesRow = ICharges.FindElements(By.TagName("li"));
                        IList<IWebElement> IChargesTD;
                        foreach (IWebElement charge in IChargesRow)
                        {
                            IChargesTD = charge.FindElements(By.TagName("a"));
                            if (IChargesTD.Count != 0)
                            {
                                try
                                {
                                    string strcharges = IChargesTD[0].GetAttribute("innerText");
                                    if (strcharges.Contains("Liens"))
                                    {
                                        IWebElement IChargesSearch = IChargesTD[0];
                                        js.ExecuteScript("arguments[0].click();", IChargesSearch);
                                        Thread.Sleep(3000);
                                        break;
                                    }
                                }
                                catch { }
                            }
                        }
                        gc.CreatePdf(orderNumber, Parcel_Id, "Liens", driver, "NJ", countynameNJ);
                        Pid = db.ExecuteScalar("SELECT id FROM data_field_master where State_County_ID='" + statecountyid + "'and Category_Id='38'");
                        id = Convert.ToInt32(Pid);
                        IWebElement ILiens = driver.FindElement(By.XPath("//*[@id='pane3']/div/div/table/tbody"));
                        IList<IWebElement> ILiensRow = ILiens.FindElements(By.TagName("tr"));
                        IList<IWebElement> ILiensTD;
                        foreach (IWebElement lien in ILiensRow)
                        {
                            ILiensTD = lien.FindElements(By.TagName("td"));
                            if (ILiensTD.Count != 0)
                            {
                                string strYearSummaryDetails = ILiensTD[0].Text + "~" + ILiensTD[1].Text + "~" + ILiensTD[2].Text + "~" + ILiensTD[3].Text + "~" + ILiensTD[4].Text + "~" + ILiensTD[5].Text + "~" + ILiensTD[6].Text + "~" + ILiensTD[7].Text + "~" + ILiensTD[8].Text;
                                gc.insert_date(orderNumber, URParcelId, id, strYearSummaryDetails, 1, DateTime.Now);
                            }
                        }

                        //MOD IV
                        driver.FindElement(By.LinkText("Mod IV")).Click();
                        gc.CreatePdf(orderNumber, Parcel_Id, "ModIV", driver, "NJ", countynameNJ);
                        string URLots = "", URBuildingDescription = "", URLandDescription = "", URClaaCode = "", URDelinquent = "", URSpecialTaxCode = "", URMODLand = "", URMODImprovement = "", URMODNetTax = "", URSeniors = "", URVeterans = "", URDisabled = "", URSurvivingSpouse = "", URWidow = "", URFireDistrict = "", URTaxMapPage = "", URExemptCode = "", URDeedBook = "", URDeedPage = "", URDeedDate = "", URSalePrice = "", URSaleAssessment = "";
                        Pid = db.ExecuteScalar("SELECT id FROM data_field_master where State_County_ID='" + statecountyid + "'and Category_Id='39'");
                        id = Convert.ToInt32(Pid);
                        IWebElement IMOD = driver.FindElement(By.XPath("//*[@id='pane4']/div/div/div"));
                        IList<IWebElement> IMODRow = IMOD.FindElements(By.TagName("div"));
                        IList<IWebElement> IMODTD;
                        IList<IWebElement> IMODValues;
                        foreach (IWebElement modfive in IMODRow)
                        {
                            IMODValues = modfive.FindElements(By.TagName("div"));
                            foreach (IWebElement mod in IMODValues)
                            {
                                IMODTD = mod.FindElements(By.TagName("div"));
                                if (IMODTD.Count != 0)
                                {
                                    if (mod.Text.Contains("Addl' Lots:"))
                                    {
                                        URLots = IMODTD[1].Text;
                                    }
                                    if (mod.Text.Contains("Building Decription:"))
                                    {
                                        URBuildingDescription = IMODTD[1].Text;
                                    }
                                    if (mod.Text.Contains("Land Description:"))
                                    {
                                        URLandDescription = IMODTD[1].Text;
                                    }
                                    if (mod.Text.Contains("Class Code:"))
                                    {
                                        URClaaCode = IMODTD[1].Text;
                                    }
                                    if (mod.Text.Contains("Delinquent:"))
                                    {
                                        URDelinquent = IMODTD[1].Text;
                                    }
                                    if (mod.Text.Contains("Special Tax Codes:"))
                                    {
                                        URSpecialTaxCode = IMODTD[1].Text;
                                    }
                                    if (mod.Text.Contains("Land:"))
                                    {
                                        URMODLand = IMODTD[1].Text;
                                    }
                                    if (mod.Text.Contains("Imrpovement:"))
                                    {
                                        URMODImprovement = IMODTD[1].Text;
                                    }
                                    if (mod.Text.Contains("Net Taxable:"))
                                    {
                                        URMODNetTax = IMODTD[1].Text;
                                    }
                                    if (mod.Text.Contains("Seniors:"))
                                    {
                                        URSeniors = IMODTD[1].Text;
                                    }
                                    if (mod.Text.Contains("Veterans:"))
                                    {
                                        URVeterans = IMODTD[1].Text;
                                    }
                                    if (mod.Text.Contains("Disabled:"))
                                    {
                                        URDisabled = IMODTD[1].Text;
                                    }
                                    if (mod.Text.Contains("Surviving Spouse:"))
                                    {
                                        URSurvivingSpouse = IMODTD[1].Text;
                                    }
                                    if (mod.Text.Contains("Widow:"))
                                    {
                                        URWidow = IMODTD[1].Text;
                                    }
                                    if (mod.Text.Contains("Fire District:"))
                                    {
                                        URFireDistrict = IMODTD[1].Text;
                                    }
                                    if (mod.Text.Contains("Tax Map Page:"))
                                    {
                                        URTaxMapPage = IMODTD[1].Text;
                                    }
                                    if (mod.Text.Contains("Exempt Code/Amt:"))
                                    {
                                        URExemptCode += IMODTD[1].Text + "~";
                                    }
                                    if (mod.Text.Contains("Deed Book:"))
                                    {
                                        URDeedBook = IMODTD[1].Text;
                                    }
                                    if (mod.Text.Contains("Deed Page:"))
                                    {
                                        URDeedPage = IMODTD[1].Text;
                                    }
                                    if (mod.Text.Contains("Deed Date:"))
                                    {
                                        URDeedDate = IMODTD[1].Text;
                                    }
                                    if (mod.Text.Contains("Sale Price:"))
                                    {
                                        URSalePrice = IMODTD[1].Text;
                                    }
                                    if (mod.Text.Contains("Sale Assessment:"))
                                    {
                                        URSaleAssessment = IMODTD[1].Text;
                                    }
                                }
                            }
                        }

                        string strMODIVDetails = URLots + "~" + URBuildingDescription + "~" + URLandDescription + "~" + URClaaCode + "~" + URDelinquent + "~" + URSpecialTaxCode + "~" + URMODLand + "~" + URMODImprovement + "~" + URMODNetTax + "~" + URSeniors + "~" + URVeterans + "~" + URDisabled + "~" + URSurvivingSpouse + "~" + URWidow + "~" + URFireDistrict + "~" + URTaxMapPage + "~" + URExemptCode.Remove(URExemptCode.Length - 1, 1) + "~" + URDeedBook + "~" + URDeedPage + "~" + URDeedDate + "~" + URSalePrice + "~" + URSaleAssessment;
                        gc.insert_date(orderNumber, URParcelId, id, strMODIVDetails, 1, DateTime.Now);


                        driver.Navigate().GoToUrl(taxCollectorlink);
                        Thread.Sleep(3000);
                        gc.CreatePdf(orderNumber, Parcel_Id, "Tax Authority", driver, "NJ", countynameNJ);
                        NJ_TaxAuthority TaxCollectornj = new Scrapsource.NJ_TaxAuthority();
                        //tax Authority
                        string taxAuthority = TaxCollectornj.TaxCollector(driver, District, orderNumber, Parcel_Id, countynameNJ);

                        Pid = db.ExecuteScalar("SELECT id FROM data_field_master where State_County_ID='" + statecountyid + "'and Category_Id='4'");
                        id = Convert.ToInt32(Pid);

                        gc.insert_date(orderNumber, Parcel_Id, id, taxAuthority, 1, DateTime.Now);

                    }
                    #endregion



                    driver.Quit();
                    gc.mergpdf(orderNumber, "NJ", countynameNJ);
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
        private int multiparcel(string township, string address, string orderNumber, string countynameNJ, string statecountyid)
        {
            string parcelNumber = "";
            ChkMultiParcel = driver.FindElement(By.XPath("/html/body/form/b")).Text.Replace("\r\n", "");
            int Tdcount = driver.FindElements(By.XPath("/html/body/form/table/tbody/tr")).Count();

            if (Tdcount <= 2)
            {
                driver.FindElement(By.XPath("/html/body/form/table/tbody/tr[2]/td[1]/a")).Click();
                Thread.Sleep(2000);
            }

            else
            {
                if (township == "--select--")
                {
                    IWebElement MultiParcelTable = driver.FindElement(By.XPath("/html/body/form/table/tbody"));
                    IList<IWebElement> MultiParcelTR = MultiParcelTable.FindElements(By.TagName("tr"));
                    IList<IWebElement> MultiParcelTD;

                    int maxCheck = 0;

                    foreach (IWebElement multi in MultiParcelTR)
                    {
                        if (maxCheck <= 25)
                        {
                            MultiParcelTD = multi.FindElements(By.TagName("td"));

                            if (MultiParcelTD.Count != 0 && !multi.Text.Contains("District"))
                            {
                                districtID = MultiParcelTD[1].Text;
                                Block = MultiParcelTD[2].Text;
                                Lot = MultiParcelTD[3].Text;
                                Qual = MultiParcelTD[4].Text;
                                LocationAddress = MultiParcelTD[5].Text;
                                if (LocationAddress == address)
                                {
                                    addresscount++;
                                    addclick = MultiParcelTD[0].FindElement(By.TagName("a"));
                                    OwnerName = MultiParcelTD[6].Text;

                                    parcelNumber = Block + "-" + Lot + "-" + Qual;
                                    MultiParcelData = districtID + "~" + LocationAddress + "~" + OwnerName;
                                    Pid = db.ExecuteScalar("SELECT id FROM data_field_master where State_County_ID='" + statecountyid + "'and Category_Id='7'");
                                    id = Convert.ToInt32(Pid);
                                    gc.insert_date(orderNumber, parcelNumber, id, MultiParcelData, 1, DateTime.Now);

                                }
                            }
                            maxCheck++;
                        }
                    }
                    if (addresscount == 1)
                    {
                        addclick.Click();
                    }
                    if (addresscount > 1)
                    {
                        gc.CreatePdf_WOP(orderNumber, "Multi Address Search", driver, "NJ", countynameNJ);

                        if (addresscount > 25)
                        {
                            HttpContext.Current.Session["multiParcel_Multicount_NJ" + countynameNJ] = "Maximum";
                        }
                        else
                        {
                            HttpContext.Current.Session["multiparcel_NJ" + countynameNJ] = "Yes";
                        }

                    }
                    else if (addresscount == 0)
                    {
                        IWebElement MultiParcelTable1 = driver.FindElement(By.XPath("/html/body/form/table/tbody"));
                        IList<IWebElement> MultiParcelTR1 = MultiParcelTable1.FindElements(By.TagName("tr"));
                        IList<IWebElement> MultiParcelTD1;

                        int maxCheck1 = 0;

                        foreach (IWebElement multi in MultiParcelTR1)
                        {
                            if (maxCheck1 <= 25)
                            {
                                MultiParcelTD1 = multi.FindElements(By.TagName("td"));
                                if (MultiParcelTD1.Count != 0 && !multi.Text.Contains("District"))
                                {
                                    districtID = MultiParcelTD1[1].Text;
                                    Block = MultiParcelTD1[2].Text;
                                    Lot = MultiParcelTD1[3].Text;
                                    Qual = MultiParcelTD1[4].Text;
                                    LocationAddress = MultiParcelTD1[5].Text;
                                    OwnerName = MultiParcelTD1[6].Text;
                                    parcelNumber = Block + "-" + Lot + "-" + Qual;
                                    MultiParcelData = districtID + "~" + LocationAddress + "~" + OwnerName;
                                    Pid = db.ExecuteScalar("SELECT id FROM data_field_master where State_County_ID='" + statecountyid + "'and Category_Id='7'");
                                    id = Convert.ToInt32(Pid);

                                    gc.insert_date(orderNumber, parcelNumber, id, MultiParcelData, 1, DateTime.Now);
                                    addresscount++;
                                }
                                maxCheck1++;

                            }
                        }
                        gc.CreatePdf_WOP(orderNumber, "Multi Address Search", driver, "NJ", countynameNJ);

                        if (maxCheck1 > 25)
                        {
                            HttpContext.Current.Session["multiParcel_Multicount_NJ" + countynameNJ] = "Maximum";
                        }
                        else
                        {
                            HttpContext.Current.Session["multiparcel_NJ" + countynameNJ] = "Yes";
                        }
                    }

                }
                else
                {

                    districtID = db.ExecuteScalar("SELECT Township_Code FROM tbl_njtownshipmaster where State_County_Id='" + statecountyid + "'and Township='" + township + "'");
                    IWebElement MultiParcelTable = driver.FindElement(By.XPath("/html/body/form/table/tbody"));
                    IList<IWebElement> MultiParcelTR = MultiParcelTable.FindElements(By.TagName("tr"));
                    IList<IWebElement> MultiParcelTD;

                    int maxCheck = 0;

                    foreach (IWebElement multi in MultiParcelTR)
                    {
                        if (maxCheck <= 25)
                        {
                            MultiParcelTD = multi.FindElements(By.TagName("td"));

                            if (MultiParcelTD.Count != 0 && !multi.Text.Contains("Location"))
                            {

                                Block = MultiParcelTD[1].Text;
                                Lot = MultiParcelTD[2].Text;
                                Qual = MultiParcelTD[3].Text;
                                LocationAddress = MultiParcelTD[5].Text;
                                if (LocationAddress == address)
                                {
                                    addresscount++;
                                    addclick = MultiParcelTD[0].FindElement(By.TagName("a"));
                                    OwnerName = MultiParcelTD[6].Text;

                                    parcelNumber = Block + "-" + Lot + "-" + Qual;
                                    MultiParcelData = districtID + "~" + LocationAddress + "~" + OwnerName;

                                    Pid = db.ExecuteScalar("SELECT id FROM data_field_master where State_County_ID='" + statecountyid + "'and Category_Id='7'");
                                    id = Convert.ToInt32(Pid);
                                    gc.insert_date(orderNumber, parcelNumber, id, MultiParcelData, 1, DateTime.Now);
                                    maxCheck++;

                                }
                            }

                        }
                    }
                    if (addresscount == 1)
                    {
                        addclick.Click();
                    }
                    if (addresscount > 1)
                    {
                        gc.CreatePdf_WOP(orderNumber, "Multi Address Search", driver, "NJ", countynameNJ);
                        HttpContext.Current.Session["multiparcel_NJ" + countynameNJ] = "Yes";
                        if (addresscount > 25)
                        {
                            HttpContext.Current.Session["multiParcel_Multicount_NJ" + countynameNJ] = "Maximum";
                        }
                        //driver.Quit();
                        //return "MultiParcel";
                    }
                    else if (addresscount == 0)
                    {
                        districtID = db.ExecuteScalar("SELECT Township_Code FROM tbl_njtownshipmaster where State_County_Id='" + statecountyid + "'and Township='" + township + "'");

                        IWebElement MultiParcelTable1 = driver.FindElement(By.XPath("/html/body/form/table/tbody"));
                        IList<IWebElement> MultiParcelTR1 = MultiParcelTable1.FindElements(By.TagName("tr"));
                        IList<IWebElement> MultiParcelTD1;



                        foreach (IWebElement multi in MultiParcelTR1)
                        {
                            if (maxCheck1 <= 25)
                            {
                                MultiParcelTD1 = multi.FindElements(By.TagName("td"));
                                if (MultiParcelTD1.Count != 0 && !multi.Text.Contains("Location"))
                                {
                                    Block = MultiParcelTD1[1].Text;
                                    Lot = MultiParcelTD1[2].Text;
                                    Qual = MultiParcelTD1[3].Text;
                                    LocationAddress = MultiParcelTD1[5].Text;
                                    OwnerName = MultiParcelTD1[6].Text;

                                    parcelNumber = Block + "-" + Lot + "-" + Qual;
                                    MultiParcelData = districtID + "~" + LocationAddress + "~" + OwnerName;
                                    Pid = db.ExecuteScalar("SELECT id FROM data_field_master where State_County_ID='" + statecountyid + "'and Category_Id='7'");
                                    id = Convert.ToInt32(Pid);

                                    gc.insert_date(orderNumber, parcelNumber, id, MultiParcelData, 1, DateTime.Now);
                                    maxCheck1++;
                                    addresscount++;
                                }

                            }
                        }
                        gc.CreatePdf_WOP(orderNumber, "Multi Address Search", driver, "NJ", countynameNJ);
                        if (maxCheck1 > 25)
                        {
                            HttpContext.Current.Session["multiParcel_Multicount_NJ" + countynameNJ] = "Maximum";
                        }
                        else
                        {
                            HttpContext.Current.Session["multiparcel_NJ" + countynameNJ] = "Yes";
                        }
                        //driver.Quit();
                        //return "MultiParcel";



                    }
                }
            }
            return addresscount;
        }
        public void ByVisibleElement(IWebElement Element)
        {
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            js.ExecuteScript("arguments[0].scrollIntoView();", Element);
        }
    }
}