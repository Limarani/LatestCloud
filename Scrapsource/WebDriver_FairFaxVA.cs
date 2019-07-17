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
using System.ComponentModel;
using System.Text;
using HtmlAgilityPack;
using iTextSharp.text;
using System.Text.RegularExpressions;
using OpenQA.Selenium.PhantomJS;
using OpenQA.Selenium.Support.Extensions;
using System.Net;
using OpenQA.Selenium.Firefox;
using System.Diagnostics;
using OpenQA.Selenium.Remote;


namespace ScrapMaricopa.Scrapsource
{
    public class WebDriver_FairFaxVA
    {
        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        MySqlParameter[] mParam;
        public string FTP_FairFaxVA(string houseno, string Direction, string sname, string stype, string Unit, string parcelNumber, string ownername, string searchType, string orderNumber)
        {
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
                        string address = "";
                        if (Direction != "")
                        {
                            address = houseno + " " + Direction + " " + sname + " " + stype + " " + Unit;
                        }
                        if (Direction == "")
                        {
                            address = houseno + " " + sname + " " + stype + " " + Unit;
                        }
                        gc.TitleFlexSearch(orderNumber, "", ownername, "", "VA", "Fairfax");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["nodata_FairFaxVA"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                        parcelNumber = parcelNumber.Replace("-", "").Trim();
                    }
                    if (searchType == "address")
                    {
                        driver.Navigate().GoToUrl("https://icare.fairfaxcounty.gov/ffxcare/search/commonsearch.aspx?mode=address");
                        Thread.Sleep(1000);
                        driver.FindElement(By.Id("inpNumber")).SendKeys(houseno);
                        try
                        {
                            driver.FindElement(By.Id("inpUnit")).SendKeys(Unit.Trim());
                        }
                        catch { }

                        driver.FindElement(By.Id("inpStreet")).SendKeys(sname);
                        try
                        {
                            IWebElement IStreettype = driver.FindElement(By.Id("Select1"));
                            SelectElement sSTreetType = new SelectElement(IStreettype);
                            sSTreetType.SelectByText(stype.ToUpper());
                        }
                        catch { }
                        try
                        {
                            IWebElement IResultCount = driver.FindElement(By.Id("selPageSize"));
                            SelectElement sResultCount = new SelectElement(IResultCount);
                            sResultCount.SelectByText("25");
                        }
                        catch { }
                        gc.CreatePdf_WOP(orderNumber, "Address Search", driver, "VA", "Fairfax");
                        driver.FindElement(By.Id("btSearch")).SendKeys(Keys.Enter);
                        gc.CreatePdf_WOP(orderNumber, "Address Search Input", driver, "VA", "Fairfax");

                        try
                        {
                            IWebElement multiaddress = driver.FindElement(By.Id("searchResults"));
                            IList<IWebElement> TRmultiaddress = multiaddress.FindElements(By.TagName("tr"));
                            IList<IWebElement> TDmultiaddress;
                            foreach (IWebElement row in TRmultiaddress)
                            {
                                TDmultiaddress = row.FindElements(By.TagName("td"));
                                if (TDmultiaddress.Count != 0 && !row.Text.Contains("Property Address") && row.Text.Trim() != "")
                                {
                                    string strParcelID = TDmultiaddress[0].Text;
                                    string strOwner = TDmultiaddress[1].Text;
                                    string strAddress = TDmultiaddress[2].Text;

                                    gc.insert_date(orderNumber, strParcelID, 2136, strOwner + "~" + strAddress, 1, DateTime.Now);
                                }
                            }
                            if (TRmultiaddress.Count <= 2)
                            {
                                IWebElement parceldata = driver.FindElement(By.XPath("//*[@id='searchResults']/tbody/tr[3]"));
                                parceldata.Click();
                                Thread.Sleep(1000);
                            }
                            if (TRmultiaddress.Count > 27)
                            {
                                HttpContext.Current.Session["multiParcel_Fairfax_Maximum"] = "Maximum";
                                driver.Quit();
                                return "Maximum";
                            }
                            if (TRmultiaddress.Count > 2 && TRmultiaddress.Count < 28)
                            {
                                HttpContext.Current.Session["multiParcel_Fairfax"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                            gc.CreatePdf_WOP(orderNumber, "Multi Address Search Input", driver, "VA", "Fairfax");
                        }
                        catch { }
                    }

                    if (searchType == "parcel")
                    {
                        driver.Navigate().GoToUrl("https://icare.fairfaxcounty.gov/ffxcare/search/commonsearch.aspx?mode=parid");
                        Thread.Sleep(1000);
                        driver.FindElement(By.Id("inpParid")).SendKeys(parcelNumber);
                        try
                        {
                            IWebElement IResultCount = driver.FindElement(By.Id("selPageSize"));
                            SelectElement sResultCount = new SelectElement(IResultCount);
                            sResultCount.SelectByText("25");
                        }
                        catch { }
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search", driver, "VA", "Fairfax");
                        driver.FindElement(By.Id("btSearch")).SendKeys(Keys.Enter);
                        Thread.Sleep(1000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search Result", driver, "VA", "Fairfax");

                        try
                        {
                            int taxcount = 2;
                            IWebElement multiaddress = driver.FindElement(By.Id("searchResults"));
                            IList<IWebElement> TRmultiaddress = multiaddress.FindElements(By.TagName("tr"));
                            IList<IWebElement> TDmultiaddress;
                            foreach (IWebElement row in TRmultiaddress)
                            {
                                TDmultiaddress = row.FindElements(By.TagName("td"));
                                if (TDmultiaddress.Count != 0 && !row.Text.Contains("Property Address") && row.Text.Trim() != "")
                                {
                                    taxcount++;
                                    string strParcelID = TDmultiaddress[0].Text;
                                    if (strParcelID.Trim().ToUpper() == parcelNumber.Trim().ToUpper())
                                    {
                                        IWebElement parceldata = driver.FindElement(By.XPath("//*[@id='searchResults']/tbody/tr[" + taxcount + "]"));
                                        parceldata.Click();
                                        Thread.Sleep(1000);
                                    }
                                    string strOwner = TDmultiaddress[1].Text;
                                    string strAddress = TDmultiaddress[2].Text;

                                    gc.insert_date(orderNumber, strParcelID, 2136, strOwner + "~" + strAddress, 1, DateTime.Now);
                                }
                            }
                            if (TRmultiaddress.Count <= 2)
                            {
                                IWebElement parceldata = driver.FindElement(By.XPath("//*[@id='searchResults']/tbody/tr[3]"));
                                parceldata.Click();
                                Thread.Sleep(1000);
                            }
                            if (TRmultiaddress.Count > 27)
                            {
                                HttpContext.Current.Session["multiParcel_Fairfax_Maximum"] = "Maximum";
                                driver.Quit();
                                return "Maximum";
                            }
                            if (TRmultiaddress.Count > 2 && TRmultiaddress.Count < 28)
                            {
                                HttpContext.Current.Session["multiParcel_Fairfax"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                            gc.CreatePdf_WOP(orderNumber, "Multi Address Search Input", driver, "VA", "Fairfax");
                        }
                        catch { }
                        try
                        {
                            string nodata = driver.FindElement(By.XPath("//*[@id='frmMain']/table/tbody/tr/td/div/div/table[2]")).Text;
                            if (nodata.Contains("search did not find any records"))
                            {
                                HttpContext.Current.Session["nodata_FairFaxVA"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }

                    ////Parcel Id, Owner, Address
                    //IWebElement Iparcel = driver.FindElement(By.XPath("//*[@id='datalet_header_row']/td/table"));
                    //IList<IWebElement> IparcelRow = Iparcel.FindElements(By.TagName("tr"));
                    //IList<IWebElement> IparcelTD;
                    //foreach(IWebElement parcel in IparcelRow)
                    //{
                    //    IparcelTD = parcel.FindElements(By.TagName("td"));
                    //    if (IparcelTD.Count !=0 && parcel.Text.Contains("MAP"))
                    //    {

                    //    }
                    //    if (IparcelTD.Count != 0 && !parcel.Text.Contains("MAP") && parcel.Text.Trim() !="")
                    //    {

                    //    }
                    //}

                    string strOwnerName = "", PropertyAddress = "", TaxDistrict = "", DistrictName = "", LandUseCode = "", LandAcerage = "", LandSoft = "", ZoiningDescription = "", Utilities = "", CountyInvertory = "", CountyHistoric = "", LegalDescription = "";
                    //Owner
                    IWebElement IOwner = driver.FindElement(By.XPath("//*[@id='Owner']/tbody"));
                    IList<IWebElement> IOwnerRow = IOwner.FindElements(By.TagName("tr"));
                    IList<IWebElement> IOwnerTD;
                    foreach (IWebElement Owner in IOwnerRow)
                    {
                        IOwnerTD = Owner.FindElements(By.TagName("td"));
                        if (IOwnerTD.Count != 0 && Owner.Text.Contains("Name"))
                        {
                            strOwnerName = IOwnerTD[1].Text;
                        }
                    }

                    if (searchType == "parcel")
                    {
                        IWebElement IAddressSplit = driver.FindElement(By.XPath("//*[@id='datalet_header_row']/td/table/tbody/tr[3]/td[2]"));
                        string[] strSplit = IAddressSplit.Text.Trim().Split(' ');
                        try
                        {
                            houseno = strSplit[0];
                            sname = strSplit[1];
                            stype = strSplit[2];
                            Direction = strSplit[3];
                        }
                        catch { }
                    }

                    //Basic Details
                    IWebElement IparcelDetails = driver.FindElement(By.XPath("//*[@id='Parcel']/tbody"));
                    IList<IWebElement> IparcelDetailsRow = IparcelDetails.FindElements(By.TagName("tr"));
                    IList<IWebElement> IparcelDetailsTD;
                    foreach (IWebElement parcel in IparcelDetailsRow)
                    {
                        IparcelDetailsTD = parcel.FindElements(By.TagName("td"));
                        if (IparcelDetailsTD.Count != 0 && parcel.Text.Contains("Property Location"))
                        {
                            PropertyAddress = IparcelDetailsTD[1].Text;
                        }
                        if (IparcelDetailsTD.Count != 0 && parcel.Text.Contains("Map"))
                        {
                            parcelNumber = IparcelDetailsTD[1].Text;
                        }
                        if (IparcelDetailsTD.Count != 0 && parcel.Text.Contains("Tax District"))
                        {
                            TaxDistrict = IparcelDetailsTD[1].Text;
                        }
                        if (IparcelDetailsTD.Count != 0 && parcel.Text.Contains("District Name"))
                        {
                            DistrictName = IparcelDetailsTD[1].Text;
                        }
                        if (IparcelDetailsTD.Count != 0 && parcel.Text.Contains("Land Use Code"))
                        {
                            LandUseCode = IparcelDetailsTD[1].Text;
                        }
                        if (IparcelDetailsTD.Count != 0 && parcel.Text.Contains("Land Area (acreage)"))
                        {
                            LandAcerage = IparcelDetailsTD[1].Text;
                        }
                        if (IparcelDetailsTD.Count != 0 && parcel.Text.Contains("Land Area (SQFT)"))
                        {
                            LandSoft = IparcelDetailsTD[1].Text;
                        }
                        if (IparcelDetailsTD.Count != 0 && parcel.Text.Contains("Zoning Description"))
                        {
                            ZoiningDescription = IparcelDetailsTD[1].Text;
                        }
                        if (IparcelDetailsTD.Count != 0 && parcel.Text.Contains("County Inventory of Historic Sites"))
                        {
                            CountyInvertory = IparcelDetailsTD[1].Text;
                        }
                        if (IparcelDetailsTD.Count != 0 && parcel.Text.Contains("County Historic Overlay District") && !parcel.Text.Contains("CLICK HERE") && !parcel.Text.Contains("For further information"))
                        {
                            CountyHistoric = IparcelDetailsTD[1].Text;
                        }
                        if (IparcelDetailsTD.Count != 0 && parcel.Text.Contains("Utilities"))
                        {
                            try
                            {
                                Utilities = gc.Between(IparcelDetails.Text, "Utilities", "County Inventory of Historic Sites");
                            }
                            catch { }
                        }
                    }

                    //Legal Description
                    IWebElement Ilegal = driver.FindElement(By.XPath("//*[@id='Legal Description']/tbody"));
                    LegalDescription = GlobalClass.After(Ilegal.Text, "Legal Description").Replace("\r\n", "").Trim();

                    string strPropertyDetails = PropertyAddress + "~" + strOwnerName + "~" + TaxDistrict + "~" + DistrictName + "~" + LandUseCode + "~" + LandAcerage + "~" + LandSoft + "~" + ZoiningDescription + "~" + Utilities + "~" + CountyInvertory + "~" + CountyHistoric + "~" + LegalDescription;
                    gc.insert_date(orderNumber, parcelNumber, 2130, strPropertyDetails, 1, DateTime.Now);
                    gc.CreatePdf(orderNumber, parcelNumber, "Property Result", driver, "VA", "Fairfax");
                    gc.insert_date(orderNumber, parcelNumber, 2139, "Property Tax", 1, DateTime.Now);
                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");

                    //Sales
                    driver.FindElement(By.LinkText("Sales")).Click();
                    gc.CreatePdf(orderNumber, parcelNumber, "Sales Result", driver, "VA", "Fairfax");
                    try
                    {
                        IWebElement ISalesHistory = driver.FindElement(By.XPath("//*[@id='Sales History']/tbody"));
                        IList<IWebElement> ISalesHistoryRow = ISalesHistory.FindElements(By.TagName("tr"));
                        IList<IWebElement> ISalesHistoryTD;
                        foreach (IWebElement SalesHistory in ISalesHistoryRow)
                        {
                            ISalesHistoryTD = SalesHistory.FindElements(By.TagName("td"));
                            if (ISalesHistoryTD.Count != 0 && !SalesHistory.Text.Contains("Date") && SalesHistory.Text.Trim() != "" && !SalesHistory.Text.Contains("No Data"))
                            {
                                string strSalesDetails = ISalesHistoryTD[0].Text + "~" + ISalesHistoryTD[1].Text + "~" + ISalesHistoryTD[2].Text + "~" + ISalesHistoryTD[3].Text;
                                gc.insert_date(orderNumber, parcelNumber, 2131, strSalesDetails, 1, DateTime.Now);
                            }
                        }
                    }
                    catch { }
                    try
                    {
                        string SalesTitle = "", SalesValues = "";
                        IWebElement ISales = driver.FindElement(By.XPath("//*[@id='Sales']/tbody"));
                        IList<IWebElement> ISalesRow = ISales.FindElements(By.TagName("tr"));
                        IList<IWebElement> ISalesTD;
                        foreach (IWebElement Sales in ISalesRow)
                        {
                            ISalesTD = Sales.FindElements(By.TagName("td"));
                            if (ISalesTD.Count != 0 && Sales.Text.Trim() != "" && !Sales.Text.Contains("No Data"))
                            {
                                SalesTitle += ISalesTD[0].Text + "~";
                                SalesValues += ISalesTD[1].Text + "~";
                            }
                        }
                        db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + SalesTitle.Remove(SalesTitle.Length - 1, 1) + "' where Id = '" + 2132 + "'");
                        gc.insert_date(orderNumber, parcelNumber, 2132, SalesValues.Remove(SalesValues.Length - 1, 1), 1, DateTime.Now);
                    }
                    catch { }

                    //Values
                    driver.FindElement(By.LinkText("Values")).Click();
                    gc.CreatePdf(orderNumber, parcelNumber, "Values Result", driver, "VA", "Fairfax");
                    try
                    {
                        string ValuesTitle = "", ValuesValues = "";
                        IWebElement IValues = driver.FindElement(By.XPath("//*[@id='Values']/tbody"));
                        IList<IWebElement> IValuesRow = IValues.FindElements(By.TagName("tr"));
                        IList<IWebElement> IValuesTD;
                        foreach (IWebElement values in IValuesRow)
                        {
                            IValuesTD = values.FindElements(By.TagName("td"));
                            if (IValuesTD.Count != 0 && values.Text.Trim() != "" && !values.Text.Contains("No Data"))
                            {
                                ValuesTitle += IValuesTD[0].Text + "~";
                                ValuesValues += IValuesTD[1].Text + "~";
                            }
                        }
                        db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + ValuesTitle.Remove(ValuesTitle.Length - 1, 1) + "' where Id = '" + 2133 + "'");
                        gc.insert_date(orderNumber, parcelNumber, 2133, ValuesValues.Remove(ValuesValues.Length - 1, 1), 1, DateTime.Now);
                    }
                    catch { }
                    try
                    {
                        IWebElement IValuesHistory = driver.FindElement(By.XPath("//*[@id='Values History']/tbody"));
                        IList<IWebElement> IValuesHistoryRow = IValuesHistory.FindElements(By.TagName("tr"));
                        IList<IWebElement> IValuesHistoryTD;
                        foreach (IWebElement ValuesHistory in IValuesHistoryRow)
                        {
                            IValuesHistoryTD = ValuesHistory.FindElements(By.TagName("td"));
                            if (IValuesHistoryTD.Count != 0 && !ValuesHistory.Text.Contains("Tax Year") && ValuesHistory.Text.Trim() != "" && !ValuesHistory.Text.Contains("No Data"))
                            {
                                string strValueDetails = IValuesHistoryTD[0].Text + "~" + IValuesHistoryTD[1].Text + "~" + IValuesHistoryTD[2].Text + "~" + IValuesHistoryTD[3].Text + "~" + IValuesHistoryTD[4].Text;
                                gc.insert_date(orderNumber, parcelNumber, 2134, strValueDetails, 1, DateTime.Now);
                            }
                        }
                    }
                    catch { }

                    //Tax Details
                    driver.FindElement(By.LinkText("Tax Details")).Click();
                    gc.CreatePdf(orderNumber, parcelNumber, "Tax Details Result", driver, "VA", "Fairfax");
                    try
                    {
                        IWebElement IPrevoiusTaxDetails = driver.FindElement(By.XPath("//*[@id='Tax and Payment History']/tbody"));
                        IList<IWebElement> IPrevoiusTaxDetailsRow = IPrevoiusTaxDetails.FindElements(By.TagName("tr"));
                        IList<IWebElement> IPrevoiusTaxDetailsTD;
                        foreach (IWebElement Prevoius in IPrevoiusTaxDetailsRow)
                        {
                            IPrevoiusTaxDetailsTD = Prevoius.FindElements(By.TagName("td"));
                            if (IPrevoiusTaxDetailsTD.Count != 0 && !Prevoius.Text.Contains("Year") && Prevoius.Text.Trim() != "" && !Prevoius.Text.Contains("No Data"))
                            {
                                string strPrevoiusDetails = IPrevoiusTaxDetailsTD[0].Text + "~" + "~" + IPrevoiusTaxDetailsTD[1].Text + "~" + IPrevoiusTaxDetailsTD[2].Text + "~" + IPrevoiusTaxDetailsTD[3].Text + "~" + IPrevoiusTaxDetailsTD[4].Text + "~" + IPrevoiusTaxDetailsTD[5].Text + "~" + IPrevoiusTaxDetailsTD[6].Text + "~" + IPrevoiusTaxDetailsTD[7].Text;
                                gc.insert_date(orderNumber, parcelNumber, 2135, strPrevoiusDetails, 1, DateTime.Now);
                            }
                        }
                    }
                    catch { }
                    try
                    {
                        IWebElement ITax = driver.FindElement(By.XPath("//*[@id='frmMain']/div[3]/div/div/table/tbody/tr/td/table/tbody/tr[2]/td/div"));
                        IList<IWebElement> ITaxDetailsList = ITax.FindElements(By.TagName("table"));
                        foreach (IWebElement taxdetail in ITaxDetailsList)
                        {
                            if (taxdetail.Text.Contains("General Fund Net Taxes") && taxdetail.Text.Contains("Amount Paid") && !taxdetail.Text.Contains("Prepays") && !taxdetail.Text.Contains("Refunds") && !taxdetail.Text.Contains("No Data"))
                            {
                                IList<IWebElement> ITaxDetailsRow = taxdetail.FindElements(By.TagName("tr"));
                                IList<IWebElement> ITaxDetailsTD;
                                foreach (IWebElement TaxDetails in ITaxDetailsRow)
                                {
                                    ITaxDetailsTD = TaxDetails.FindElements(By.TagName("td"));
                                    if (ITaxDetailsTD.Count != 0 && !TaxDetails.Text.Contains("Year") && TaxDetails.Text.Trim() != "")
                                    {
                                        string strValueDetails = ITaxDetailsTD[0].Text + "~" + ITaxDetailsTD[1].Text + "~" + ITaxDetailsTD[2].Text + "~" + ITaxDetailsTD[3].Text + "~" + ITaxDetailsTD[4].Text + "~" + ITaxDetailsTD[5].Text + "~" + ITaxDetailsTD[6].Text + "~" + ITaxDetailsTD[7].Text + "~" + ITaxDetailsTD[8].Text + "~" + ITaxDetailsTD[9].Text;
                                        gc.insert_date(orderNumber, parcelNumber, 2135, strValueDetails, 1, DateTime.Now);
                                    }
                                }
                            }
                        }
                    }
                    catch { }
                    try
                    {
                        string IPayRefundTitle = "", IPayRefundValues = "";
                        IWebElement IPayRefund = driver.FindElement(By.XPath("//*[@id='Prepays and Refunds']/tbody"));
                        IList<IWebElement> IPayRefundRow = IPayRefund.FindElements(By.TagName("tr"));
                        IList<IWebElement> IPayRefundTD;
                        foreach (IWebElement values in IPayRefundRow)
                        {
                            IPayRefundTD = values.FindElements(By.TagName("td"));
                            if (IPayRefundTD.Count != 0 && values.Text.Trim() != "" && !values.Text.Contains("No Data"))
                            {
                                IPayRefundTitle += IPayRefundTD[0].Text + "~";
                                IPayRefundValues += IPayRefundTD[1].Text + "~";
                            }
                        }
                        db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + IPayRefundTitle.Remove(IPayRefundTitle.Length - 1, 1) + "' where Id = '" + 2137 + "'");
                        gc.insert_date(orderNumber, parcelNumber, 2137, IPayRefundValues.Remove(IPayRefundValues.Length - 1, 1), 1, DateTime.Now);
                    }
                    catch { }

                    //Residential
                    driver.FindElement(By.LinkText("Residential")).Click();
                    gc.CreatePdf(orderNumber, parcelNumber, "Residential Result", driver, "VA", "Fairfax");
                    string IResidentialTitle = "", IResidentialValues = "";
                    try
                    {
                        IWebElement IResidential = driver.FindElement(By.XPath("//*[@id='Primary Building']/tbody"));
                        IList<IWebElement> IResidentialRow = IResidential.FindElements(By.TagName("tr"));
                        IList<IWebElement> IResidentialTD;
                        foreach (IWebElement Residential in IResidentialRow)
                        {
                            IResidentialTD = Residential.FindElements(By.TagName("td"));
                            if (IResidentialTD.Count != 0 && Residential.Text.Trim() != "" && !Residential.Text.Contains("No Data"))
                            {
                                IResidentialTitle += IResidentialTD[0].Text + "~";
                                IResidentialValues += IResidentialTD[1].Text + "~";
                            }
                        }
                        db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + IResidentialTitle.Remove(IResidentialTitle.Length - 1, 1) + "' where Id = '" + 2138 + "'");
                        gc.insert_date(orderNumber, parcelNumber, 2138, IResidentialValues.Remove(IResidentialValues.Length - 1, 1), 1, DateTime.Now);
                    }
                    catch { }

                    string TaxAddress = "";
                    if (Direction != "")
                    {
                        TaxAddress = sname + " " + stype + " " + Direction;
                    }
                    if (Direction == "")
                    {
                        TaxAddress = sname + " " + stype;
                    }

                    //Township of Vienna
                    if (DistrictName.Contains("Town of Vienna") || DistrictName.Contains("TOWN OF VIENNA"))
                    {
                        gc.insert_date(orderNumber, parcelNumber, 2139, "County Tax Town of Vienna", 1, DateTime.Now);
                        IJavaScriptExecutor js = driver as IJavaScriptExecutor;

                        driver.Navigate().GoToUrl("https://mss.viennava.gov/MSS/citizens/RealEstate/Default.aspx?mode=new");
                        driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_Control_AddressSearchFieldLayout_ctl01_StreetNumberTextBox")).SendKeys(houseno.Trim());
                        driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_Control_StreetNameSearchFieldLayout_ctl01_AddressTextBox")).SendKeys(sname.Trim());
                        string CurrentYear = (DateTime.Now.Year).ToString();
                        IWebElement ICYearClick = driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_Control_FiscalYearLayoutItem_ctl01_YearSearchTextBox"));
                        js.ExecuteScript("arguments[0].setAttribute('value', '" + CurrentYear + "')", ICYearClick);
                        driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_Control_FormLayoutItem7_ctl01_Button1")).SendKeys(Keys.Enter);
                        Thread.Sleep(4000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Tax Information", driver, "VA", "Fairfax");
                        string TaxDetails = "", TaxCheck = "", multino = "0";
                        try
                        {
                            TaxDetails = driver.FindElement(By.Id("molContentMainDiv")).Text;
                        }
                        catch { }
                        try
                        {
                            if (TaxDetails == "")
                            {
                                TaxDetails = driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_ViewBill1_ViewBillControlPanel")).Text;
                            }
                        }
                        catch { }
                        if ((TaxDetails.Contains("House number") && TaxDetails.Contains("Street name")) || TaxDetails.Contains("currently restricted"))
                        {
                            driver.Navigate().GoToUrl("https://mss.viennava.gov/MSS/citizens/RealEstate/Default.aspx?mode=new");
                            driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_Control_AddressSearchFieldLayout_ctl01_StreetNumberTextBox")).SendKeys(houseno.Trim());
                            driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_Control_StreetNameSearchFieldLayout_ctl01_AddressTextBox")).SendKeys(sname.Trim());
                            string PreviousYear = (DateTime.Now.Year - 1).ToString();
                            IWebElement IPYearClick = driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_Control_FiscalYearLayoutItem_ctl01_YearSearchTextBox"));
                            js.ExecuteScript("arguments[0].setAttribute('value', '" + PreviousYear + "')", IPYearClick);
                            driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_Control_FormLayoutItem7_ctl01_Button1")).SendKeys(Keys.Enter);
                            Thread.Sleep(2000);
                            gc.CreatePdf(orderNumber, parcelNumber, "Tax Information", driver, "VA", "Fairfax");
                        }

                        try
                        {
                            TaxCheck = driver.FindElement(By.Id("molContentMainDiv")).Text;
                        }
                        catch { }
                        try
                        {
                            if (TaxCheck == "")
                            {
                                TaxCheck = driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_ViewBill1_ViewBillControlPanel")).Text;
                            }
                        }
                        catch { }
                        if (((TaxCheck.Contains("View bill image") && TaxCheck.Contains("View payments/adjustments")) || (TaxCheck.Contains("Bill Type") && TaxCheck.Contains("View Bill"))) && ((!TaxCheck.Contains("House number") && !TaxCheck.Contains("Street name")) && !TaxCheck.Contains("currently restricted")))
                        {
                            try
                            {
                                multino = gc.Between(driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_BillsGridView")).Text, "excluded)\r\n", " Found").Trim();
                            }
                            catch { }
                            if (Convert.ToInt32(multino) > 1)
                            {
                                try
                                {
                                    IWebElement ITaxMultiAddress = driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_BillsGridView"));
                                    IList<IWebElement> ITaxMultiAddressRow = ITaxMultiAddress.FindElements(By.TagName("tr"));
                                    IList<IWebElement> ITaxMultiAddressTD;
                                    foreach (IWebElement multi in ITaxMultiAddressRow)
                                    {
                                        ITaxMultiAddressTD = multi.FindElements(By.TagName("td"));
                                        if (ITaxMultiAddressTD.Count != 0 && !multi.Text.Contains("Address"))
                                        {
                                            if (ITaxMultiAddressTD[0].Text.Trim() == houseno.Trim().ToUpper() + " " + TaxAddress.ToUpper().Trim() && ITaxMultiAddressTD[1].Text.Trim() == Unit.ToUpper().Trim())
                                            {
                                                IWebElement IClick = ITaxMultiAddressTD[6].FindElement(By.TagName("a"));
                                                string idClick = IClick.GetAttribute("id");
                                                driver.FindElement(By.Id(idClick)).Click();
                                                Thread.Sleep(3000);
                                            }
                                        }
                                    }
                                }
                                catch { }
                            }

                            string taxbill = driver.CurrentWindowHandle;
                            //Good Through Date
                            IWebElement IasofDateSearch;
                            string asofGood = "", parcelid1 = "", billyearGood = "", billGood = "", ownerGood = "", parcelidGood = "";
                            // Deliquent
                            try
                            {
                                IWebElement Delinquent1 = driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_ViewBill1_BillDetailsUpdatePanel']/table/tbody/tr[2]/td[6]"));
                                if (Delinquent1.Text != "$0.00")
                                {
                                    IasofDateSearch = driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_ViewBill1_AsOfDateTextBox']"));
                                    IasofDateSearch.Clear();
                                    string currDate = DateTime.Now.ToString("MM/dd/yyyy");
                                    string dateChecking = DateTime.Now.ToString("MM") + "/15/" + DateTime.Now.ToString("yyyy");
                                    if (Convert.ToDateTime(currDate) > Convert.ToDateTime(dateChecking))
                                    {
                                        string nextEndOfMonth = "";
                                        if ((Convert.ToInt16(Convert.ToInt16(DateTime.Now.ToString("MM"))) < 12))
                                        {
                                            nextEndOfMonth = new DateTime(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(Convert.ToInt16(DateTime.Now.ToString("MM")) + 1), DateTime.DaysInMonth(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(DateTime.Now.ToString("MM")) + 1)).ToString("MM/dd/yyyy");
                                        }
                                        else
                                        {
                                            int nxtYr = Convert.ToInt16(DateTime.Now.ToString("yyyy")) + 1;
                                            nextEndOfMonth = nextEndOfMonth = new DateTime(nxtYr, 1, DateTime.DaysInMonth(Convert.ToInt16(DateTime.Now.ToString("yyyy")), 1)).ToString("MM/dd/yyyy");
                                        }

                                        IasofDateSearch.SendKeys(nextEndOfMonth);
                                        asofGood = nextEndOfMonth;
                                    }
                                    else
                                    {
                                        string EndOfMonth = new DateTime(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(Convert.ToInt16(DateTime.Now.ToString("MM"))), DateTime.DaysInMonth(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(DateTime.Now.ToString("MM")))).ToString("MM/dd/yyyy");
                                        IasofDateSearch.SendKeys(EndOfMonth);
                                        asofGood = EndOfMonth;
                                    }

                                    driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_ViewBill1_AsOfDateTextBox")).Clear();
                                    string[] daysplit = asofGood.Split('/');
                                    try
                                    {
                                        IWebElement gclick = driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_ViewBill1_AsOfDateTextBox"));
                                        gclick.Click();
                                    }
                                    catch { }

                                    IWebElement Iday = driver.FindElement(By.XPath("//*[@id='ui-datepicker-div']/table"));
                                    IList<IWebElement> IdayRow = Iday.FindElements(By.TagName("a"));
                                    foreach (IWebElement day in IdayRow)
                                    {
                                        if (day.Text != "" && day.Text == daysplit[1])
                                        {
                                            day.SendKeys(Keys.Enter);
                                            Thread.Sleep(2000);
                                            //gc.CreatePdf(orderNumber, ParcelNumber, "Good Through Date", driver, "VA", "Fairfax");
                                        }
                                    }

                                    gc.CreatePdf(orderNumber, parcelNumber, "View Bill Yearwise", driver, "VA", "Fairfax");
                                    string asof = "", billyear = "", bill = "", viewbillowner = "";
                                    IWebElement IasofDate = driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_ViewBill1_AsOfDateTextBox"));
                                    asof = IasofDate.GetAttribute("value");

                                    string bulktxt = driver.FindElement(By.XPath("//*[@id='BillDetailTable']/tbody")).Text;
                                    string strbillyear = gc.Between(bulktxt, "Bill Year", "Owner");
                                    billyear = GlobalClass.Before(strbillyear, "Bill");
                                    bill = GlobalClass.After(strbillyear, "Bill");
                                    viewbillowner = gc.Between(bulktxt, "Owner", "Parcel ID");
                                    parcelid1 = GlobalClass.After(bulktxt, "Parcel ID");

                                    try
                                    {
                                        IWebElement Bigdata = driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_ViewBill1_BillDetailsUpdatePanel"));
                                        IList<IWebElement> TRBigdata = Bigdata.FindElements(By.TagName("tr"));
                                        IList<IWebElement> THBigdata = Bigdata.FindElements(By.TagName("th"));
                                        IList<IWebElement> TDBigdata;
                                        foreach (IWebElement row in TRBigdata)
                                        {
                                            TDBigdata = row.FindElements(By.TagName("td"));
                                            if (TDBigdata.Count == 7 && TDBigdata.Count != 0)
                                            {
                                                string taxdetails1 = asofGood + "~" + asof + "~" + billyear + "~" + bill + "~" + viewbillowner + "~" + parcelid1 + "~" + TDBigdata[0].Text + "~" + TDBigdata[1].Text + "~" + TDBigdata[2].Text + "~" + TDBigdata[3].Text + "~" + TDBigdata[4].Text + "~" + TDBigdata[5].Text + "~" + TDBigdata[6].Text + "~" + "Town of Vienna 127 Center St. S.Vienna, VA  22180";

                                                gc.insert_date(orderNumber, parcelid1, 2252, taxdetails1, 1, DateTime.Now);
                                            }
                                            if (TDBigdata.Count == 6 && TDBigdata.Count != 0)
                                            {
                                                string taxdetails1 = asofGood + "~" + asof + "~" + billyear + "~" + bill + "~" + viewbillowner + "~" + parcelid1 + "~" + TDBigdata[0].Text + "~" + "" + "~" + TDBigdata[1].Text + "~" + TDBigdata[2].Text + "~" + TDBigdata[3].Text + "~" + TDBigdata[4].Text + "~" + TDBigdata[5].Text + "~" + "Town of Vienna 127 Center St. S.Vienna, VA  22180";

                                                gc.insert_date(orderNumber, parcelid1, 2252, taxdetails1, 1, DateTime.Now);
                                            }

                                        }
                                    }
                                    catch (Exception ex) { }
                                }
                            }
                            catch { }

                            //Tax Information
                            IWebElement Delinquent = driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_ViewBill1_BillDetailsUpdatePanel']/table/tbody/tr[2]/td[6]"));
                            if (Delinquent.Text == "$0.00")
                            {
                                gc.CreatePdf(orderNumber, parcelNumber, "View Bill Yearwise", driver, "VA", "Fairfax");
                                string asof = "", billyear = "", bill = "", viewbillowner = "";
                                IWebElement IasofDate = driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_ViewBill1_AsOfDateTextBox"));
                                asof = IasofDate.GetAttribute("value");
                                Thread.Sleep(3000);
                                //asof = driver.FindElement(By.XPath("//*[@id='BillDetailTable']/tbody/tr[1]/td")).Text.Trim();
                                string bulktxt = driver.FindElement(By.XPath("//*[@id='BillDetailTable']/tbody")).Text;
                                string strbillyear = gc.Between(bulktxt, "Bill Year", "Owner");
                                billyear = GlobalClass.Before(strbillyear, "Bill");
                                bill = GlobalClass.After(strbillyear, "Bill");
                                viewbillowner = gc.Between(bulktxt, "Owner", "Parcel ID");
                                parcelid1 = GlobalClass.After(bulktxt, "Parcel ID");


                                try
                                {
                                    IWebElement Bigdata = driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_ViewBill1_BillDetailsUpdatePanel"));
                                    IList<IWebElement> TRBigdata = Bigdata.FindElements(By.TagName("tr"));
                                    IList<IWebElement> THBigdata = Bigdata.FindElements(By.TagName("th"));
                                    IList<IWebElement> TDBigdata;
                                    foreach (IWebElement row in TRBigdata)
                                    {
                                        TDBigdata = row.FindElements(By.TagName("td"));
                                        if (TDBigdata.Count == 7 && TDBigdata.Count != 0)
                                        {
                                            string taxdetails = "" + "~" + asof + "~" + billyear + "~" + bill + "~" + viewbillowner + "~" + parcelid1 + "~" + TDBigdata[0].Text + "~" + TDBigdata[1].Text + "~" + TDBigdata[2].Text + "~" + TDBigdata[3].Text + "~" + TDBigdata[4].Text + "~" + TDBigdata[5].Text + "~" + TDBigdata[6].Text + "~" + "Town of Vienna 127 Center St. S.Vienna, VA  22180";

                                            gc.insert_date(orderNumber, parcelid1, 2252, taxdetails, 1, DateTime.Now);
                                        }
                                        if (TDBigdata.Count == 6 && TDBigdata.Count != 0)
                                        {
                                            string taxdetails = "" + "~" + asof + "~" + billyear + "~" + bill + "~" + viewbillowner + "~" + parcelid1 + "~" + TDBigdata[0].Text + "~" + "" + "~" + TDBigdata[1].Text + "~" + TDBigdata[2].Text + "~" + TDBigdata[3].Text + "~" + TDBigdata[4].Text + "~" + TDBigdata[5].Text + "~" + "Town of Vienna 127 Center St. S. Vienna, VA  22180";

                                            gc.insert_date(orderNumber, parcelid1, 2252, taxdetails, 1, DateTime.Now);
                                        }

                                    }
                                }
                                catch (Exception ex) { }
                            }

                            //Tax Payment Details
                            string strall_billyear = "", all_billyear = "", all_bill = "", all_billyear1 = "", all_bill1 = "";
                            try
                            {
                                driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_ViewBill1_ViewPaymentsLinkButton")).SendKeys(Keys.Enter);
                                Thread.Sleep(5000);
                                string allbills = driver.FindElement(By.XPath("//*[@id='BillActivityTable']/tbody")).Text;

                                strall_billyear = GlobalClass.After(allbills, "Year");
                                all_bill = GlobalClass.After(strall_billyear, "Bill");
                                all_billyear = GlobalClass.Before(strall_billyear, "Bill").Replace("\r\n", "");
                                gc.CreatePdf(orderNumber, parcelNumber, "Tax Payment Bill" + all_billyear, driver, "VA", "Fairfax");
                                IWebElement allbill1 = driver.FindElement(By.Id("molContentContainer"));
                                IList<IWebElement> TRallbill1 = allbill1.FindElements(By.TagName("tr"));
                                IList<IWebElement> THallbill1 = allbill1.FindElements(By.TagName("th"));
                                IList<IWebElement> TDallbill1;
                                foreach (IWebElement text in TRallbill1)
                                {
                                    TDallbill1 = text.FindElements(By.TagName("td"));
                                    THallbill1 = text.FindElements(By.TagName("th"));

                                    if (TDallbill1.Count != 0 && TDallbill1.Count == 1 && !text.Text.Contains("Paid By/Reference") && text.Text.Contains("Year"))
                                    {
                                        all_billyear1 = TDallbill1[0].Text;
                                    }
                                    if (TDallbill1.Count != 0 && TDallbill1.Count == 1 && !text.Text.Contains("Paid By/Reference") && !text.Text.Contains("Year"))
                                    {
                                        all_bill1 = TDallbill1[0].Text;
                                    }
                                    if (TDallbill1.Count != 0 && TDallbill1.Count == 4 && !text.Text.Contains("Paid By/Reference") && !text.Text.Contains("Bill Year"))
                                    {
                                        string Allbilldetails = all_billyear1 + "~" + all_bill + "~" + TDallbill1[0].Text + "~" + TDallbill1[1].Text + "~" + TDallbill1[2].Text + "~" + TDallbill1[3].Text;
                                        gc.insert_date(orderNumber, parcelNumber, 2251, Allbilldetails, 1, DateTime.Now);
                                    }

                                }
                            }
                            catch { }

                            //Tax Distribution Details (Charges Click)
                            IWebElement ICharges = driver.FindElement(By.Id("primarynav"));
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
                                        if (strcharges.Contains("Charges"))
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
                            //driver.FindElement(By.LinkText("Charges")).Click();
                            //Thread.Sleep(2000);
                            gc.CreatePdf(orderNumber, parcelNumber, "Tax Charges", driver, "VA", "Fairfax");

                            string TownOwner = "", TownLegal = "", TownLocation = "", TownCustomerId = "", Jurisdiction = "", Deed = "", Exemption = "", Book = "", Units = "", AssessValue = "", Charges = "";
                            //Property Details(Click Property Details)
                            IWebElement IProperty = driver.FindElement(By.Id("primarynav"));
                            IList<IWebElement> IPropertyRow = IProperty.FindElements(By.TagName("li"));
                            IList<IWebElement> IPropertyTD;
                            foreach (IWebElement proper in IPropertyRow)
                            {
                                IPropertyTD = proper.FindElements(By.TagName("a"));
                                if (IPropertyTD.Count != 0)
                                {
                                    try
                                    {
                                        string strproper = IPropertyTD[0].GetAttribute("innerText");
                                        if (strproper.Contains("Property Detail"))
                                        {
                                            IWebElement IProperSearch = IPropertyTD[0];
                                            js.ExecuteScript("arguments[0].click();", IProperSearch);
                                            Thread.Sleep(3000);
                                            break;
                                        }
                                    }
                                    catch { }
                                }
                            }
                            //IWebElement IProperty = driver.FindElement(By.XPath("//*[@id='primarynav']/li[5]/ul/li[3]/a"));
                            //IProperty.Click();
                            //Thread.Sleep(2000);
                            gc.CreatePdf(orderNumber, parcelNumber, "Tax Property Details", driver, "VA", "Fairfax");

                            IWebElement Bigdata21 = driver.FindElement(By.Id("ParcelTable"));
                            IList<IWebElement> TRBigdata21 = Bigdata21.FindElements(By.TagName("tr"));
                            IList<IWebElement> THBigdata21 = Bigdata21.FindElements(By.TagName("th"));
                            IList<IWebElement> TDBigdata21;
                            foreach (IWebElement rows1 in TRBigdata21)
                            {
                                TDBigdata21 = rows1.FindElements(By.TagName("td"));
                                THBigdata21 = rows1.FindElements(By.TagName("th"));
                                if (TDBigdata21.Count != 0 && rows1.Text.Contains("Legal Description"))
                                {
                                    TownLegal = TDBigdata21[0].Text;
                                }
                                if (TDBigdata21.Count != 0 && rows1.Text.Contains("Owner"))
                                {
                                    TownOwner = TDBigdata21[0].Text;
                                }
                                if (TDBigdata21.Count != 0 && rows1.Text.Contains("Location"))
                                {
                                    TownLocation = TDBigdata21[0].Text;
                                }
                                if (TDBigdata21.Count != 0 && rows1.Text.Contains("Customer ID"))
                                {
                                    TownCustomerId = TDBigdata21[0].Text;
                                }
                                if (TDBigdata21.Count != 0 && rows1.Text.Contains("Jurisdiction"))
                                {
                                    Jurisdiction = TDBigdata21[0].Text;
                                }
                                if (TDBigdata21.Count != 0 && rows1.Text.Contains("Deed Recorded"))
                                {
                                    Deed = TDBigdata21[0].Text;
                                }
                                if (TDBigdata21.Count != 0 && rows1.Text.Contains("Book/Page"))
                                {
                                    Book = TDBigdata21[0].Text;
                                }
                                if (TDBigdata21.Count != 0 && rows1.Text.Contains("Units"))
                                {
                                    Units = TDBigdata21[0].Text;
                                }
                                if (TDBigdata21.Count != 0 && rows1.Text.Contains("Assessed Value"))
                                {
                                    AssessValue = TDBigdata21[0].Text;
                                }
                                if (TDBigdata21.Count != 0 && rows1.Text.Contains("Exemptions Value"))
                                {
                                    Exemption = TDBigdata21[0].Text;
                                }
                                if (TDBigdata21.Count != 0 && rows1.Text.Contains("Charges"))
                                {
                                    Charges = TDBigdata21[0].Text;
                                }
                            }

                            string AssessmentDetails = TownOwner + "~" + TownLegal + "~" + TownLocation + "~" + TownCustomerId + "~" + Jurisdiction + "~" + Deed + "~" + Book + "~" + Units + "~" + AssessValue + "~" + Exemption + "~" + Charges;
                            gc.insert_date(orderNumber, parcelNumber, 2250, AssessmentDetails, 1, DateTime.Now);


                            //Assessment Details (Click Assessment Details)(Pending)
                            //IWebElement IAssessment = driver.FindElement(By.XPath("//*[@id='primarynav']/li[5]/ul/li[5]/a"));
                            //IAssessment.Click();
                            //Thread.Sleep(9000);
                            IWebElement IAssessmentDetails = driver.FindElement(By.Id("primarynav"));
                            IList<IWebElement> IAssessmentRow = IAssessmentDetails.FindElements(By.TagName("li"));
                            IList<IWebElement> IAssessmentTD;
                            foreach (IWebElement assess in IAssessmentRow)
                            {
                                IAssessmentTD = assess.FindElements(By.TagName("a"));
                                if (IAssessmentTD.Count != 0)
                                {
                                    try
                                    {
                                        string strassess = IAssessmentTD[0].GetAttribute("innerText");
                                        if (strassess.Contains("Assessment"))
                                        {
                                            IWebElement IassessClick = IAssessmentTD[0];
                                            js.ExecuteScript("arguments[0].click();", IassessClick);
                                            Thread.Sleep(3000);
                                            break;
                                        }
                                    }
                                    catch { }
                                }
                            }

                            //js.ExecuteScript("arguments[0].click();", IAssessment);
                            gc.CreatePdf(orderNumber, parcelNumber, "Tax Assessment", driver, "VA", "Fairfax");

                            IWebElement bulkavd = driver.FindElement(By.XPath("//*[@id='molContentContainer']/div/table[2]/tbody"));
                            IList<IWebElement> TRbulkavd = bulkavd.FindElements(By.TagName("tr"));
                            IList<IWebElement> THbulkavd = bulkavd.FindElements(By.TagName("th"));
                            IList<IWebElement> TDbulkavd;
                            foreach (IWebElement txt in TRbulkavd)
                            {
                                TDbulkavd = txt.FindElements(By.TagName("td"));
                                THbulkavd = txt.FindElements(By.TagName("th"));
                                if (TDbulkavd.Count != 0 && !txt.Text.Contains("Gross Assessment"))
                                {
                                    string Assessvaluedetails = all_billyear.Trim() + "~" + THbulkavd[0].Text.Trim() + "~" + TDbulkavd[0].Text.Trim();
                                    gc.insert_date(orderNumber, parcelNumber, 2249, Assessvaluedetails, 1, DateTime.Now);
                                }
                            }
                            //Tax Assessment History Details
                            //IWebElement IAssessmenthistory = driver.FindElement(By.XPath("//*[@id='primarynav']/li[5]/ul/li[6]/a"));
                            //IAssessmenthistory.Click();
                            //Thread.Sleep(5000);
                            IWebElement IAssessHistory = driver.FindElement(By.Id("primarynav"));
                            IList<IWebElement> IAssessHistoryRow = IAssessHistory.FindElements(By.TagName("li"));
                            IList<IWebElement> IAssessHistoryTD;
                            foreach (IWebElement history in IAssessHistoryRow)
                            {
                                IAssessHistoryTD = history.FindElements(By.TagName("a"));
                                if (IAssessHistoryTD.Count != 0)
                                {
                                    try
                                    {
                                        string strassess = IAssessHistoryTD[0].GetAttribute("innerText");
                                        if (strassess.Contains("Assessment History"))
                                        {
                                            IWebElement IhistoryClick = IAssessHistoryTD[0];
                                            js.ExecuteScript("arguments[0].click();", IhistoryClick);
                                            Thread.Sleep(3000);
                                            break;
                                        }
                                    }
                                    catch { }
                                }
                            }

                            //js.ExecuteScript("arguments[0].click();", IAssessment);
                            gc.CreatePdf(orderNumber, parcelNumber, "Tax Assessment History", driver, "VA", "Fairfax");

                            //Tax History Bill Details
                            try
                            {
                                try
                                {
                                    IWebElement IAddressSearch1 = driver.FindElement(By.LinkText("All Bills"));
                                    IJavaScriptExecutor js1 = driver as IJavaScriptExecutor;
                                    js1.ExecuteScript("arguments[0].click();", IAddressSearch1);
                                    Thread.Sleep(9000);
                                }
                                catch { }

                                IWebElement IBill = driver.FindElement(By.Id("primarynav"));
                                IList<IWebElement> IBillRow = IBill.FindElements(By.TagName("li"));
                                IList<IWebElement> IBillTD;
                                foreach (IWebElement bill in IBillRow)
                                {
                                    IBillTD = bill.FindElements(By.TagName("a"));
                                    if (IBillTD.Count != 0)
                                    {
                                        try
                                        {
                                            string strbill = IBillTD[0].GetAttribute("innerText");
                                            if (strbill.Contains("All Bills"))
                                            {
                                                IWebElement IbillClick = IBillTD[0];
                                                js.ExecuteScript("arguments[0].click();", IbillClick);
                                                Thread.Sleep(3000);
                                                break;
                                            }
                                        }
                                        catch { }
                                    }
                                }

                                gc.CreatePdf(orderNumber, parcelNumber, "Tax History Details All Bills", driver, "VA", "Fairfax");
                            }
                            catch { }
                        }
                    }
                    else if (DistrictName.Contains("Town of Herndon") || DistrictName.Contains("TOWN OF HERNDON"))
                    {
                        gc.insert_date(orderNumber, parcelNumber, 2139, "County Tax Town of Herndon", 1, DateTime.Now);
                    }


                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "VA", "Fairfax", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    gc.mergpdf(orderNumber, "VA", "Fairfax");
                    return "Data Inserted Successfully";
                }
                catch (Exception ex)
                {
                    driver.Quit();
                    throw ex;
                }
            }
        }
        public void ByVisibleElement(IWebElement Element)
        {
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            js.ExecuteScript("arguments[0].scrollIntoView();", Element);
        }

    }
}