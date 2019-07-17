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

namespace ScrapMaricopa.Scrapsource
{
    public class WebDriver_SussexDE
    {
        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();

        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());

        public string FTP_Sussex(string houseno, string Direction, string sname, string stype, string account, string parcelNumber, string ownername, string searchType, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            string TaxingAuthority = ""; string address = "";
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
            IJavaScriptExecutor js = driver as IJavaScriptExecutor;
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            using (driver = new PhantomJSDriver())
            {

                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");
                    try
                    {
                        //Taxing Authority
                        driver.Navigate().GoToUrl("https://sussexcountyde.gov/property-tax-information");


                        string bulkTaxauth = driver.FindElement(By.Id("contact-table")).Text.Replace("\r\n", " ");
                        TaxingAuthority = gc.Between(bulkTaxauth, "Address", "Contact Billing Support");

                        gc.CreatePdf_WOP(orderNumber, "Taxing Authority", driver, "DE", "Sussex");
                    }
                    catch (Exception ex) { }

                    if (Direction != "")
                    {
                        address = houseno + " " + Direction + " " + sname + " " + stype + " " + account;
                        address = address.Trim();
                    }
                    if (Direction == "")
                    {
                        address = houseno + " " + sname + " " + stype + " " + account;
                        address = address.Trim();
                    }

                    if (searchType == "address")
                    {
                        driver.Navigate().GoToUrl("https://iframe.sussexcountyde.gov/e-service/propertytaxes/index.cfm?resource=name_search&CFID=5192517&CFTOKEN=32991376");


                        try
                        {
                            driver.FindElement(By.LinkText("Search by Address")).Click();
                            Thread.Sleep(1000);
                        }
                        catch { }
                        try
                        {
                            driver.FindElement(By.Id("btAgree")).SendKeys(Keys.Enter);
                            Thread.Sleep(1000);

                        }
                        catch { }

                        driver.FindElement(By.Id("inpNumber")).SendKeys(houseno);
                        driver.FindElement(By.Id("Select1")).SendKeys(Direction);
                        driver.FindElement(By.Id("inpStreet")).SendKeys(sname);
                        //driver.FindElement(By.Id("Select1")).SendKeys(stype);
                        //driver.FindElement(By.Id("inpOwner")).SendKeys(account);

                        gc.CreatePdf_WOP(orderNumber, "Address Search", driver, "DE", "Sussex");
                        try
                        {
                            driver.FindElement(By.Id("btSearch")).Click();
                        }
                        catch { }
                        string Record = "";
                        try
                        {
                            IWebElement Irecord = driver.FindElement(By.XPath("//*[@id='frmMain']/table/tbody/tr/td/div/div/table[2]/tbody/tr/td/table/tbody/tr[3]/td/center/table[1]/tbody/tr[1]/td/div/p"));
                            Record = Irecord.Text;
                            if (Record.Contains("Your search did not find any records."))
                            {
                                searchType = "titleflex";
                            }
                        }
                        catch { }
                        try
                        {
                            string strowner = "", strAddress = "", strProperty = "", strCity = "";
                            IWebElement multiaddress = driver.FindElement(By.Id("searchResults"));
                            IList<IWebElement> multiRow = multiaddress.FindElements(By.TagName("tr"));
                            IList<IWebElement> multiTD;
                            foreach (IWebElement multi in multiRow)
                            {
                                multiTD = multi.FindElements(By.TagName("td"));
                                if (multiTD.Count != 0 && multiRow.Count < 2)
                                {
                                    IWebElement IsearchClick = multiTD[2].FindElement(By.TagName("a"));
                                    string searchclick = IsearchClick.GetAttribute("href");
                                    driver.Navigate().GoToUrl(searchclick);
                                    break;
                                }
                                if (multiTD.Count != 0 && multiRow.Count > 25)
                                {
                                    HttpContext.Current.Session["multiParcel_SussexDE_Multicount"] = "Maximum";
                                    driver.Quit();
                                    return "Maximum";
                                }
                                if (multiTD.Count != 0 && multiRow.Count > 2 && multiRow.Count <= 25 && !multi.Text.Contains("Address") && multi.Text.Trim() != "")
                                {
                                    strowner = multiTD[2].Text;
                                    strAddress = multiTD[3].Text;
                                    strProperty = multiTD[1].Text;
                                    strCity = multiTD[4].Text;
                                    string multidetails = strowner + "~" + strAddress + " " + strCity;
                                    gc.insert_date(orderNumber, strProperty, 890, multidetails, 1, DateTime.Now);
                                }
                            }
                            if (multiRow.Count > 2 && multiRow.Count <= 25)
                            {
                                HttpContext.Current.Session["multiparcel_SussexDE"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                        }
                        catch { }

                    }

                    if (searchType == "titleflex")
                    {

                        string titleaddress = address;
                        gc.TitleFlexSearch(orderNumber, "", "", titleaddress, "DE", "Sussex");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_SussexDE"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }



                    if (searchType == "ownername")
                    {
                        driver.Navigate().GoToUrl("https://iframe.sussexcountyde.gov/e-service/propertytaxes/index.cfm?resource=name_search&CFID=5192517&CFTOKEN=32991376");

                        try
                        {
                            driver.FindElement(By.LinkText("Search by Owner")).Click();
                            Thread.Sleep(1000);
                        }
                        catch { }
                        try
                        {
                            driver.FindElement(By.Id("btAgree")).SendKeys(Keys.Enter);
                            Thread.Sleep(1000);
                            gc.CreatePdf_WOP(orderNumber, "OwnerName Search", driver, "DE", "Sussex");
                        }
                        catch { }

                        driver.FindElement(By.Id("inpOwner")).SendKeys(ownername);

                        gc.CreatePdf_WOP(orderNumber, "OwnerName Search Input", driver, "DE", "Sussex");
                        try
                        {
                            driver.FindElement(By.Id("btSearch")).Click();
                        }
                        catch { }


                        try
                        {
                            string strowner = "", strAddress = "", strProperty = "", strCity = "";
                            IWebElement multiaddress = driver.FindElement(By.Id("searchResults"));
                            IList<IWebElement> multiRow = multiaddress.FindElements(By.TagName("tr"));
                            IList<IWebElement> multiTD;
                            foreach (IWebElement multi in multiRow)
                            {
                                multiTD = multi.FindElements(By.TagName("td"));
                                if (multiTD.Count != 0 && multiRow.Count < 2)
                                {
                                    IWebElement IsearchClick = multiTD[2].FindElement(By.TagName("a"));
                                    string searchclick = IsearchClick.GetAttribute("href");
                                    driver.Navigate().GoToUrl(searchclick);
                                    break;
                                }
                                if (multiTD.Count != 0 && multiRow.Count > 25)
                                {
                                    HttpContext.Current.Session["multiParcel_SussexDE_Multicount"] = "Maximum";
                                    driver.Quit();
                                    return "Maximum";
                                }
                                if (multiTD.Count != 0 && multiRow.Count > 2 && multiRow.Count <= 25 && !multi.Text.Contains("Address") && multi.Text.Trim() != "")
                                {
                                    strowner = multiTD[2].Text;
                                    strAddress = multiTD[3].Text;
                                    strProperty = multiTD[1].Text;
                                    strCity = multiTD[4].Text;
                                    string multidetails = strowner + "~" + strAddress + " " + strCity;
                                    gc.insert_date(orderNumber, strProperty, 890, multidetails, 1, DateTime.Now);
                                }
                            }
                            if (multiRow.Count > 2 && multiRow.Count <= 25)
                            {
                                HttpContext.Current.Session["multiparcel_SussexDE"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                        }
                        catch { }

                    }
                    if (searchType == "parcel")
                    {
                        string strDistrick = "", strMap = "", strParcel = "", strUnit = "";
                        driver.Navigate().GoToUrl("https://iframe.sussexcountyde.gov/e-service/propertytaxes/index.cfm?resource=map_search");
                        Thread.Sleep(1000);

                        try
                        {
                            driver.FindElement(By.LinkText("Search by Parcel ID")).SendKeys(Keys.Enter);
                            Thread.Sleep(1000);
                        }
                        catch { }
                        try
                        {
                            driver.FindElement(By.Id("btAgree")).SendKeys(Keys.Enter);
                            Thread.Sleep(1000);
                            gc.CreatePdf_WOP(orderNumber, "Parcel search", driver, "DE", "Sussex");
                        }
                        catch { }
                        if (parcelNumber.Contains("-"))
                        {
                            parcelNumber = parcelNumber.Replace("-", " ");
                        }
                        if (!parcelNumber.Contains("Unit"))
                        {
                            string[] parcelSplit = parcelNumber.Split(' ');
                            strDistrick = parcelSplit[0];
                            strMap = parcelSplit[1];
                            if (parcelSplit.Count() == 3)
                            {
                                strParcel = parcelSplit[2];
                            }
                            if (parcelSplit.Count() == 4)
                            {
                                strParcel = parcelSplit[2];
                                strUnit = parcelSplit[3];
                            }

                        }
                        if (parcelNumber.Contains("Unit"))
                        {
                            string[] parcelSplit = parcelNumber.Split(' ');
                            strDistrick = parcelSplit[0];
                            strMap = parcelSplit[1];
                            strParcel = parcelSplit[3];
                            strUnit = parcelSplit[5];
                        }
                        string map = "", parcel = "";


                        map = strMap.Substring(0, 1);
                        if (map == "0")
                        {
                            map = strMap.Remove(0, 1);
                        }
                        else
                        {
                            map = strMap;
                        }
                        parcel = strDistrick + " " + map + " " + strParcel + " " + strUnit;
                        if (parcel.EndsWith(" "))
                            parcel = parcel.Substring(0, parcel.Length - 1);
                        parcel = parcel.TrimEnd();
                        try
                        {
                            driver.FindElement(By.XPath("//*[@id='district']")).SendKeys(strDistrick);
                            driver.FindElement(By.XPath("//*[@id='map']")).SendKeys(map);
                            driver.FindElement(By.XPath("//*[@id='parcel']")).SendKeys(strParcel);
                            driver.FindElement(By.XPath("//*[@id='unit']")).SendKeys(strUnit);
                        }
                        catch { }
                        try
                        {
                            parcelNumber = strDistrick + "-" + map + "-" + strParcel + "-" + strUnit;
                            parcelNumber = parcelNumber.Trim('-');
                            driver.FindElement(By.Id("inpParid")).SendKeys(parcelNumber);
                        }
                        catch { }

                        gc.CreatePdf(orderNumber, parcelNumber, "parcel search Input", driver, "DE", "Sussex");

                        try
                        {
                            driver.FindElement(By.Id("btSearch")).Click();
                        }
                        catch { }



                        parcel = Regex.Replace(parcel, @"\s+", " ");
                        try
                        {
                            string parcelcheckunit = "";
                            IWebElement parcelcheck = driver.FindElement(By.XPath("//*[@id='roundmaincontent']/table/tbody"));
                            IList<IWebElement> TRparcelcheck = parcelcheck.FindElements(By.TagName("tr"));
                            IList<IWebElement> THparcelcheck = parcelcheck.FindElements(By.TagName("th"));
                            IList<IWebElement> TDparcelcheck;
                            foreach (IWebElement row in TRparcelcheck)
                            {
                                TDparcelcheck = row.FindElements(By.TagName("td"));
                                THparcelcheck = row.FindElements(By.TagName("th"));
                                if (TDparcelcheck.Count != 0 && !row.Text.Contains("Property") && !row.Text.Contains("click on property number to view"))
                                {
                                    //  if (parcelNumber.Replace(" 0"," ") == TDparcelcheck[2].Text.Replace("  "," "))
                                    string TDparcelcheck1 = TDparcelcheck[2].Text;
                                    //TDparcelcheck1 = Regex.Match(TDparcelcheck1, @"\d+").Value;
                                    TDparcelcheck1 = Regex.Replace(TDparcelcheck1, @"\s+", " ");
                                    if (TDparcelcheck1.Contains("Unit "))
                                    {
                                        parcelcheckunit = TDparcelcheck1.Replace("Unit ", "");
                                    }

                                    if (parcel == TDparcelcheck1 || parcel == parcelcheckunit)
                                    {
                                        IWebElement IsearchClick = TDparcelcheck[2].FindElement(By.TagName("a"));
                                        string searchclick = IsearchClick.GetAttribute("href");
                                        driver.Navigate().GoToUrl(searchclick);
                                        break;
                                        //driver.FindElement(By.XPath("//*[@id='roundmaincontent']/table/tbody/tr[3]/td[3]/a")).SendKeys(Keys.Enter);
                                        //Thread.Sleep(4000);
                                    }
                                }
                            }
                        }
                        catch { }
                    }
                    gc.CreatePdf(orderNumber, parcelNumber, "Property Details", driver, "DE", "Sussex");
                    try
                    {
                        //No Data Found
                        string nodata = driver.FindElement(By.XPath("//*[@id='frmMain']/table/tbody/tr/td/div/div/table[2]/tbody/tr/td/table/tbody/tr[3]/td/center/table[1]")).Text;
                        if (nodata.Contains("search did not find any records"))
                        {
                            HttpContext.Current.Session["Nodata_SussexDE"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }

                    string bulktext = driver.FindElement(By.XPath("/html/body")).Text;
                    string parcel_Id = "", PropertyAdd = "", owner_name = "", Unit = "", City = "", State = "", Zip = "", Pro_class = "", Use_Code = "", Legal_Desc = "", mailling_address = "", land_use = "", zoning = "";
                    string town = "", Tax_district = "", School_district = "", Council_district = "", Fire_district = "", Acres = "";

                    parcel_Id = gc.Between(bulktext, "PARID:", "ROLL").Trim();
                    PropertyAdd = gc.Between(bulktext, "Property Location:", "Unit:").Trim();
                    Unit = gc.Between(bulktext, "Unit:", "City:").Trim();
                    City = gc.Between(bulktext, "City:", "State:").Trim();
                    State = gc.Between(bulktext, "State:", "Zip:").Trim();
                    Zip = gc.Between(bulktext, "Zip:", "Class:").Trim();
                    Pro_class = gc.Between(bulktext, "Class:", "Use Code (LUC)").Trim();
                    Use_Code = gc.Between(bulktext, "Use Code (LUC):", "Town").Trim();
                    IWebElement strtown1 = driver.FindElement(By.XPath("//*[@id='Property Information']/tbody/tr[9]/td[2]"));
                    town = strtown1.Text;

                    Tax_district = gc.Between(bulktext, "Tax District:", "School District:").Trim();
                    School_district = gc.Between(bulktext, "School District:", "Council District:").Trim();
                    Council_district = gc.Between(bulktext, "Council District:", "Fire District:").Trim();
                    Fire_district = gc.Between(bulktext, "Fire District:", "Deeded Acres:").Trim();
                    Acres = gc.Between(bulktext, "Deeded Acres:", "Frontage:").Trim();

                    IWebElement strOwner1 = driver.FindElement(By.XPath("//*[@id='Owners']/tbody/tr[2]/td[1]"));
                    IWebElement strOwner2 = driver.FindElement(By.XPath("//*[@id='Owners']/tbody/tr[2]/td[2]"));
                    owner_name = strOwner1.Text.Trim() + " & " + strOwner2.Text.Trim();
                    owner_name = owner_name.Trim('&');
                    //IWebElement strLegal = driver.FindElement(By.XPath("//*[@id='Legal']/tbody"));
                    //Legal_Desc = strLegal.Text.Replace("Legal Description", "").Replace("\r\n", " ").Trim();

                    IWebElement strBilladdress1 = driver.FindElement(By.XPath("//*[@id='Owners']/tbody/tr[2]/td[3]"));
                    IWebElement strBilladdress2 = driver.FindElement(By.XPath("//*[@id='Owners']/tbody/tr[2]/td[4]"));
                    IWebElement strBilladdress3 = driver.FindElement(By.XPath("//*[@id='Owners']/tbody/tr[2]/td[5]"));
                    IWebElement strBilladdress4 = driver.FindElement(By.XPath("//*[@id='Owners']/tbody/tr[2]/td[6]"));
                    mailling_address = strBilladdress1.Text.Trim() + " " + strBilladdress2.Text.Trim() + " " + strBilladdress3.Text.Trim() + " " + strBilladdress4.Text.Trim();





                    string propertydetails = PropertyAdd + "~" + Unit + "~" + City + "~" + State + "~" + Zip + "~" + Pro_class + "~" + Use_Code + "~" + town + "~" + Tax_district + "~" + School_district + "~" + Council_district + "~" + Fire_district + "~" + Acres + "~" + owner_name + "~" + mailling_address;
                    gc.insert_date(orderNumber, parcel_Id, 853, propertydetails, 1, DateTime.Now);

                    // Assessment Details
                    string YearBuilt = "";
                    try
                    {
                        driver.FindElement(By.LinkText("Sales")).SendKeys(Keys.Enter);
                        Thread.Sleep(1000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Sales Details", driver, "DE", "Sussex");
                    }
                    catch { }
                    try
                    {
                        driver.FindElement(By.LinkText("Land")).SendKeys(Keys.Enter);
                        Thread.Sleep(1000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Land Details", driver, "DE", "Sussex");
                    }
                    catch { }

                    try
                    {
                        driver.FindElement(By.LinkText("Residential")).SendKeys(Keys.Enter);
                        Thread.Sleep(1000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Residentials Details", driver, "DE", "Sussex");
                        IWebElement StrYear_built = driver.FindElement(By.XPath("//*[@id='Residential']/tbody/tr[4]/td[2]"));
                        YearBuilt = StrYear_built.Text;
                    }
                    catch { }
                    try
                    {
                        driver.FindElement(By.LinkText("Values")).SendKeys(Keys.Enter);
                        Thread.Sleep(1000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Values Details", driver, "DE", "Sussex");
                    }
                    catch { }
                    string landvalue = "", improvvalue = "", totalvalue = "";

                    // 100% values
                    string percentage = "";
                    IWebElement ipercentage = driver.FindElement(By.XPath("//*[@id='datalet_div_0']/table[1]/tbody"));
                    percentage = ipercentage.Text.Replace("Values", "");
                    try
                    {
                        IWebElement valdata = driver.FindElement(By.XPath("//*[@id='100% Values']/tbody"));
                        IList<IWebElement> TRvaldata = valdata.FindElements(By.TagName("tr"));
                        IList<IWebElement> THvaldata = valdata.FindElements(By.TagName("th"));
                        IList<IWebElement> TDvaldata;
                        foreach (IWebElement row in TRvaldata)
                        {
                            TDvaldata = row.FindElements(By.TagName("td"));
                            if (TRvaldata.Count > 1 && row.Text.Trim() != "" && !row.Text.Contains("Improv Value"))
                            {
                                string assessmentdetails = percentage + "~" + TDvaldata[0].Text + "~" + TDvaldata[1].Text + "~" + TDvaldata[2].Text;
                                gc.insert_date(orderNumber, parcel_Id, 855, assessmentdetails, 1, DateTime.Now);
                            }

                        }


                    }
                    catch { }
                    // 50 % values
                    string percentage1 = "";
                    IWebElement ipercentage1 = driver.FindElement(By.XPath("//*[@id='datalet_div_1']/table[1]/tbody"));
                    percentage1 = ipercentage1.Text.Replace("Values", "");
                    try
                    {
                        IWebElement valdata1 = driver.FindElement(By.XPath("//*[@id='50% Values']/tbody"));
                        IList<IWebElement> TRvaldata1 = valdata1.FindElements(By.TagName("tr"));
                        IList<IWebElement> THvaldata1 = valdata1.FindElements(By.TagName("th"));
                        IList<IWebElement> TDvaldata1;
                        foreach (IWebElement row1 in TRvaldata1)
                        {
                            TDvaldata1 = row1.FindElements(By.TagName("td"));
                            if (TRvaldata1.Count > 1 && row1.Text.Trim() != "" && !row1.Text.Contains("Improv Value"))
                            {
                                string assessmentdetails1 = percentage1 + "~" + TDvaldata1[0].Text + "~" + TDvaldata1[1].Text + "~" + TDvaldata1[2].Text;
                                gc.insert_date(orderNumber, parcel_Id, 855, assessmentdetails1, 1, DateTime.Now);
                            }

                        }


                    }
                    catch { }

                    // Tax Information
                    int IYear = 0;
                    int Syear = DateTime.Now.Year;
                    int Smonth = DateTime.Now.Month;
                    if (Smonth >= 9)
                    {
                        IYear = Syear;
                    }
                    else
                    {
                        Syear--;
                        IYear = Syear;

                    }

                    driver.Navigate().GoToUrl("https://munis.sussexcountyde.gov/MSS/citizens/RealEstate/Default.aspx");
                    Thread.Sleep(4000);
                    gc.CreatePdf(orderNumber, parcel_Id, "Tax Information Page", driver, "DE", "Sussex");
                    string strparcel = parcel_Id.Replace(" ", "-").Replace("--", "-").Replace("Unit-", "");
                    driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_Control_ParcelIdSearchFieldLayout_ctl01_ParcelIDTextBox']")).SendKeys(strparcel);
                    driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_Control_FiscalYearLayoutItem_ctl01_YearSearchTextBox']")).SendKeys(IYear.ToString());
                    gc.CreatePdf(orderNumber, parcelNumber, "Tax Information Page Search", driver, "DE", "Sussex");
                    driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_Control_FormLayoutItem7_ctl01_Button1']")).SendKeys(Keys.Enter);
                    Thread.Sleep(5000);
                    try
                    {
                        IWebElement taxMultiparceltable = driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_BillsGridView']/tbody"));
                        IList<IWebElement> TaxMultiparcelrow = taxMultiparceltable.FindElements(By.TagName("tr"));
                        IList<IWebElement> TaxMultiid;
                        foreach (IWebElement TaxMulti in TaxMultiparcelrow)
                        {
                            TaxMultiid = TaxMulti.FindElements(By.TagName("td"));
                            if (TaxMultiid.Count != 0 && !TaxMulti.Text.Contains("Address"))
                            {
                                string Checkparcel = TaxMultiid[3].Text;
                                if (strparcel == Checkparcel)
                                {
                                    driver.FindElement(By.LinkText("View Bill")).Click();
                                    Thread.Sleep(2000);
                                    break;
                                }
                            }
                        }
                    }
                    catch { }
                    gc.CreatePdf(orderNumber, parcel_Id, "View Bill", driver, "DE", "Sussex");
                    //try
                    //{
                    //    IWebElement Delinquent1 = driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_ViewBill1_SpecialConditionsControl1_SpecialConditionMessagesTable']/tbody"));
                    //    if (Delinquent1.Text.Contains("This account is coded for payment"))
                    //    {
                    //        string asof = "", billyear = "", bill = "", owner = "", parcelid = "";
                    //        IWebElement IasofDate = driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_ViewBill1_AsOfDateTextBox']"));
                    //        asof = IasofDate.GetAttribute("value");
                    //        string bulktxt = driver.FindElement(By.XPath("//*[@id='BillDetailTable']/tbody")).Text;
                    //        string strbillyear = gc.Between(bulktxt, "Bill Year", "Owner");
                    //        billyear = GlobalClass.Before(strbillyear, "Bill");
                    //        bill = GlobalClass.After(strbillyear, "Bill");
                    //        owner = gc.Between(bulktxt, "Owner", "Parcel ID");
                    //        parcelid = GlobalClass.After(bulktxt, "Parcel ID");
                    //        try
                    //        {
                    //            IWebElement Bigdata = driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_ViewBill1_BillDetailsUpdatePanel']/table/tbody"));
                    //            IList<IWebElement> TRBigdata = Bigdata.FindElements(By.TagName("tr"));
                    //            IList<IWebElement> THBigdata = Bigdata.FindElements(By.TagName("th"));
                    //            IList<IWebElement> TDBigdata;
                    //            foreach (IWebElement row in TRBigdata)
                    //            {
                    //                TDBigdata = row.FindElements(By.TagName("td"));
                    //                if (TRBigdata.Count > 1 && TDBigdata.Count != 0 && !row.Text.Contains("TOTAL") && !row.Text.Contains("Interest"))
                    //                {
                    //                    string taxdetails = asof + "~" + billyear + "~" + bill + "~" + owner + "~" + parcelid + "~" + TDBigdata[0].Text + "~" + TDBigdata[1].Text + "~" + TDBigdata[2].Text + "~" + TDBigdata[3].Text + "~" + TDBigdata[4].Text + "~" + TDBigdata[5].Text + "~" + TDBigdata[6].Text + "~" + TaxingAuthority;

                    //                    gc.insert_date(orderNumber, parcel_Id, 858, taxdetails, 1, DateTime.Now);
                    //                }
                    //                if (TRBigdata.Count > 2 && TDBigdata.Count != 0 && !row.Text.Contains("TOTAL") && row.Text.Contains("Interest"))
                    //                {

                    //                    string taxdetails1 = "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + TDBigdata[0].Text + "~" + "" + "~" + "" + "~" + TDBigdata[2].Text + "~" + "" + "~" + "" + "~" + TDBigdata[5].Text + "~" + TaxingAuthority;
                    //                    gc.insert_date(orderNumber, parcel_Id, 858, taxdetails1, 1, DateTime.Now);
                    //                }
                    //                if (TRBigdata.Count > 2 && TDBigdata.Count != 0 && row.Text.Contains("TOTAL"))
                    //                {

                    //                    string taxdetails2 = "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + TDBigdata[0].Text + "~" + "" + "~" + TDBigdata[1].Text + "~" + TDBigdata[2].Text + "~" + TDBigdata[3].Text + "~" + TDBigdata[4].Text + "~" + TDBigdata[5].Text + "~" + TaxingAuthority;
                    //                    gc.insert_date(orderNumber, parcel_Id, 858, taxdetails2, 1, DateTime.Now);
                    //                }
                    //            }
                    //            IYear--;
                    //        }
                    //        catch (Exception ex) { }
                    //    }
                    //}
                    //catch { }
                    int IyeaR = Syear;
                    IWebElement IasofDateSearch;
                    string asofGood = "", billyearGood = "", billGood = "", ownerGood = "", parcelidGood = "";
                    // Deliquent
                    //try
                    //{
                    //    IWebElement Delinquent = driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_PaymentBlockMessage_BlockageMessageParagraph']/span"));
                    //    if (Delinquent.Text == "Prior unpaid bill(s) exist for this parcel.")
                    //    {
                    //        HttpContext.Current.Session["Delinquent_SussexDE"] = "Yes";
                    //        Thread.Sleep(3000);
                    //        IasofDateSearch = driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_ViewBill1_AsOfDateTextBox']"));
                    //        IasofDateSearch.Clear();
                    //        string currDate = DateTime.Now.ToString("MM/dd/yyyy");
                    //        string dateChecking = DateTime.Now.ToString("MM") + "/15/" + DateTime.Now.ToString("yyyy");
                    //        if (Convert.ToDateTime(currDate) > Convert.ToDateTime(dateChecking))
                    //        {
                    //            string nextEndOfMonth = "";
                    //            if ((Convert.ToInt16(Convert.ToInt16(DateTime.Now.ToString("MM"))) < 12))
                    //            {
                    //                nextEndOfMonth = new DateTime(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(Convert.ToInt16(DateTime.Now.ToString("MM")) + 1), DateTime.DaysInMonth(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(DateTime.Now.ToString("MM")) + 1)).ToString("MM/dd/yyyy");
                    //                IasofDateSearch.SendKeys(nextEndOfMonth);
                    //            }
                    //            else
                    //            {
                    //                int nxtYr = Convert.ToInt16(DateTime.Now.ToString("yyyy")) + 1;
                    //                nextEndOfMonth = nextEndOfMonth = new DateTime(nxtYr, 1, DateTime.DaysInMonth(Convert.ToInt16(DateTime.Now.ToString("yyyy")), 1)).ToString("MM/dd/yyyy");
                    //                IasofDateSearch.SendKeys(nextEndOfMonth);
                    //            }
                    //            string[] daysplit = nextEndOfMonth.Split('/');
                    //            IWebElement Inextmonth = driver.FindElement(By.XPath("//*[@id='ui-datepicker-div']/div/a[2]"));
                    //            Inextmonth.SendKeys(Keys.Enter);
                    //            IWebElement Iday = driver.FindElement(By.XPath("//*[@id='ui-datepicker-div']/table/tbody"));
                    //            IList<IWebElement> IdayRow = Iday.FindElements(By.TagName("a"));
                    //            foreach (IWebElement day in IdayRow)
                    //            {
                    //                if (day.Text != "" && day.Text == daysplit[1])
                    //                {
                    //                    day.SendKeys(Keys.Enter);
                    //                }
                    //            }

                    //            asofGood = nextEndOfMonth;
                    //        }
                    //        else
                    //        {
                    //            string EndOfMonth = new DateTime(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(Convert.ToInt16(DateTime.Now.ToString("MM"))), DateTime.DaysInMonth(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(DateTime.Now.ToString("MM")))).ToString("MM/dd/yyyy");
                    //            IasofDateSearch.SendKeys(EndOfMonth);
                    //            string[] Edaysplit = EndOfMonth.Split('/');
                    //            IWebElement IEday = driver.FindElement(By.XPath("//*[@id='ui-datepicker-div']/table/tbody"));
                    //            IList<IWebElement> IEdayRow = IEday.FindElements(By.TagName("a"));
                    //            foreach (IWebElement Eday in IEdayRow)
                    //            {
                    //                if (Eday.Text != "" && Eday.Text == Edaysplit[1])
                    //                {
                    //                    Eday.SendKeys(Keys.Enter);
                    //                }
                    //            }

                    //            asofGood = EndOfMonth;
                    //        }
                    //    }
                    //    else
                    //    {
                    //        IasofDateSearch = driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_ViewBill1_AsOfDateTextBox']"));
                    //        asofGood = IasofDateSearch.GetAttribute("value");
                    //    }

                    //    gc.CreatePdf(orderNumber, parcel_Id, "View Bill Good date", driver, "DE", "Sussex");
                    //    string bulktxtGood = driver.FindElement(By.XPath("//*[@id='BillDetailTable']/tbody")).Text;
                    //    string strbillyearGood = gc.Between(bulktxtGood, "Bill Year", "Owner");
                    //    billyearGood = GlobalClass.Before(strbillyearGood, "Bill");
                    //    billGood = GlobalClass.After(strbillyearGood, "Bill");
                    //    ownerGood = gc.Between(bulktxtGood, "Owner", "Parcel ID");
                    //    parcelidGood = GlobalClass.After(bulktxtGood, "Parcel ID");
                    //    try
                    //    {
                    //        IWebElement BigdataGood = driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_ViewBill1_BillDetailsUpdatePanel']/table/tbody"));
                    //        IList<IWebElement> TRBigdataGood = BigdataGood.FindElements(By.TagName("tr"));
                    //        IList<IWebElement> THBigdataGood = BigdataGood.FindElements(By.TagName("th"));
                    //        IList<IWebElement> TDBigdataGood;
                    //        foreach (IWebElement row in TRBigdataGood)
                    //        {
                    //            TDBigdataGood = row.FindElements(By.TagName("td"));
                    //            if (TRBigdataGood.Count > 1 && TDBigdataGood.Count != 0 && !row.Text.Contains("TOTAL") && !row.Text.Contains("Interest"))
                    //            {
                    //                string taxdetailsGood = asofGood + "~" + billyearGood + "~" + billGood + "~" + ownerGood + "~" + parcelidGood + "~" + TDBigdataGood[0].Text + "~" + TDBigdataGood[1].Text + "~" + TDBigdataGood[2].Text + "~" + TDBigdataGood[3].Text + "~" + TDBigdataGood[4].Text + "~" + TDBigdataGood[5].Text + "~" + TDBigdataGood[6].Text + "~" + TaxingAuthority;

                    //                gc.insert_date(orderNumber, parcel_Id, 858, taxdetailsGood, 1, DateTime.Now);
                    //            }
                    //            if (TRBigdataGood.Count > 2 && TDBigdataGood.Count != 0 && !row.Text.Contains("TOTAL") && row.Text.Contains("Interest"))
                    //            {

                    //                string taxdetails1Good = "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + TDBigdataGood[0].Text + "~" + "" + "~" + "" + "~" + TDBigdataGood[2].Text + "~" + "" + "~" + "" + "~" + TDBigdataGood[5].Text + "~" + TaxingAuthority;
                    //                gc.insert_date(orderNumber, parcel_Id, 858, taxdetails1Good, 1, DateTime.Now);
                    //            }
                    //            if (TRBigdataGood.Count > 2 && TDBigdataGood.Count != 0 && row.Text.Contains("TOTAL"))
                    //            {

                    //                string taxdetails2Good = "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + TDBigdataGood[0].Text + "~" + "" + "~" + TDBigdataGood[1].Text + "~" + TDBigdataGood[2].Text + "~" + TDBigdataGood[3].Text + "~" + TDBigdataGood[4].Text + "~" + TDBigdataGood[5].Text + "~" + TaxingAuthority;
                    //                gc.insert_date(orderNumber, parcel_Id, 858, taxdetails2Good, 1, DateTime.Now);
                    //            }
                    //        }

                    //    }
                    //    catch (Exception ex) { }
                    //}
                    //catch { }

                    try
                    {
                        driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_ViewBill1_ViewPaymentsLinkButton']")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcel_Id, "View payments", driver, "DE", "Sussex");
                    }
                    catch { }

                    //try
                    //{
                    //    string bulktext1 = driver.FindElement(By.XPath("//*[@id='BillActivityTable']")).Text;
                    //    string bill_year = "", strtaxbill = "", taxbill = "";
                    //    strtaxbill = GlobalClass.After(bulktext1, "Bill Year");
                    //    bill_year = GlobalClass.Before(strtaxbill, "Bill").Replace("\r\n", "");
                    //    taxbill = GlobalClass.After(strtaxbill, "Bill");
                    //    IWebElement Bigdata1 = driver.FindElement(By.XPath("//*[@id='molContentContainer']/div/table[2]/tbody"));
                    //    IList<IWebElement> TRBigdata1 = Bigdata1.FindElements(By.TagName("tr"));
                    //    IList<IWebElement> THBigdata1 = Bigdata1.FindElements(By.TagName("th"));
                    //    IList<IWebElement> TDBigdata1;
                    //    foreach (IWebElement row in TRBigdata1)
                    //    {
                    //        TDBigdata1 = row.FindElements(By.TagName("td"));
                    //        if (row.Text.Contains("No payment activity could be found for this bill."))
                    //        {
                    //            string paymentdetails = bill_year + "~" + taxbill + "~" + "" + "~" + "" + "~" + "" + "~" + "";

                    //            gc.insert_date(orderNumber, parcel_Id, 877, paymentdetails, 1, DateTime.Now);
                    //        }
                    //        if (TRBigdata1.Count > 1 && TDBigdata1.Count != 0 && !row.Text.Contains("Activity"))
                    //        {
                    //            string paymentdetails1 = bill_year + "~" + taxbill + "~" + TDBigdata1[0].Text + "~" + TDBigdata1[1].Text + "~" + TDBigdata1[2].Text + "~" + TDBigdata1[3].Text;

                    //            gc.insert_date(orderNumber, parcel_Id, 877, paymentdetails1, 1, DateTime.Now);
                    //        }
                    //    }
                    //    IyeaR--;
                    //}
                    //catch (Exception ex)
                    //{ }

                    //Tax Information after View bill details Scrap
                    //string Cyear = DateTime.Now.Year.ToString();
                    //int cyear = Int32.Parse(Syear);
                    //driver.Navigate().GoToUrl("https://munis.sussexcountyde.gov/MSS/citizens/RealEstate/Default.aspx");
                    //Thread.Sleep(4000);
                    //gc.CreatePdf(orderNumber, parcel_Id, "Tax Information Page", driver, "DE", "Sussex");
                    //string strcparcel = parcel_Id.Replace(" ", "-").Replace("--", "-");
                    //driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_Control_ParcelIdSearchFieldLayout_ctl01_ParcelIDTextBox']")).SendKeys(strcparcel);
                    //driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_Control_FiscalYearLayoutItem_ctl01_YearSearchTextBox']")).SendKeys(cyear.ToString());
                    //gc.CreatePdf(orderNumber, parcelNumber, "Tax Information Page Search", driver, "DE", "Sussex");
                    //driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_Control_FormLayoutItem7_ctl01_Button1']")).SendKeys(Keys.Enter);
                    //Thread.Sleep(5000);
                    IWebElement ISpan12 = driver.FindElement(By.XPath("//*[@id='primarynav']/li[4]/ul/li[2]/a"));
                    ISpan12.Click();
                    Thread.Sleep(1000);
                    //js.ExecuteScript("arguments[0].click();", ISpan12);

                    gc.CreatePdf(orderNumber, parcel_Id, "Tax Charges", driver, "DE", "Sussex");
                    try
                    {
                        IWebElement Bigdata2 = driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_TaxChargesTable']"));
                        IList<IWebElement> TRBigdata2 = Bigdata2.FindElements(By.TagName("tr"));
                        IList<IWebElement> THBigdata2 = Bigdata2.FindElements(By.TagName("th"));
                        IList<IWebElement> TDBigdata2;
                        foreach (IWebElement row in TRBigdata2)
                        {
                            TDBigdata2 = row.FindElements(By.TagName("td"));
                            THBigdata2 = row.FindElements(By.TagName("th"));
                            if (TDBigdata2.Count != 0 && !row.Text.Contains("Taxable Value") && !row.Text.Contains("Total"))
                            {
                                string paymentdetails2 = THBigdata2[0].Text + "~" + TDBigdata2[0].Text + "~" + TDBigdata2[1].Text + "~" + TDBigdata2[2].Text;

                                gc.insert_date(orderNumber, parcel_Id, 879, paymentdetails2, 1, DateTime.Now);
                            }
                            if (TDBigdata2.Count != 0 && row.Text.Contains("Total"))
                            {
                                string paymentdetails3 = THBigdata2[0].Text + "~" + "" + "~" + "" + "~" + TDBigdata2[0].Text;

                                gc.insert_date(orderNumber, parcel_Id, 879, paymentdetails3, 1, DateTime.Now);
                            }
                        }
                    }
                    catch { }

                    try
                    {
                        IWebElement IProperty = driver.FindElement(By.XPath("//*[@id='primarynav']/li[4]/ul/li[3]/a"));
                        IProperty.Click();
                        //js.ExecuteScript("arguments[0].click();", IProperty);
                        Thread.Sleep(1000);
                        gc.CreatePdf(orderNumber, parcel_Id, "Tax Property Details", driver, "DE", "Sussex");
                        IWebElement IOwner = driver.FindElement(By.XPath("//*[@id='primarynav']/li[4]/ul/li[4]/a"));
                        IOwner.Click();
                        // js.ExecuteScript("arguments[0].click();", IOwner);
                        Thread.Sleep(1000);
                        gc.CreatePdf(orderNumber, parcel_Id, "Owner Information", driver, "DE", "Sussex");
                        IWebElement IAssessment = driver.FindElement(By.XPath("//*[@id='primarynav']/li[4]/ul/li[5]/a"));
                        IAssessment.Click();
                        Thread.Sleep(1000);
                        //js.ExecuteScript("arguments[0].click();", IAssessment);
                        gc.CreatePdf(orderNumber, parcel_Id, "Tax Assessment", driver, "DE", "Sussex");
                    }
                    catch { }

                    try
                    {
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
                                string Assessvaluedetails = THbulkavd[0].Text + "~" + TDbulkavd[0].Text;

                                gc.insert_date(orderNumber, parcel_Id, 880, Assessvaluedetails, 1, DateTime.Now);
                            }

                        }
                    }
                    catch { }
                    try
                    {
                        IWebElement bulkavd1 = driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_AssessmentsGrid']/tbody"));
                        IList<IWebElement> TRbulkavd1 = bulkavd1.FindElements(By.TagName("tr"));
                        IList<IWebElement> THbulkavd1 = bulkavd1.FindElements(By.TagName("th"));
                        IList<IWebElement> TDbulkavd1;
                        foreach (IWebElement txt in TRbulkavd1)
                        {
                            TDbulkavd1 = txt.FindElements(By.TagName("td"));
                            THbulkavd1 = txt.FindElements(By.TagName("th"));
                            if (TDbulkavd1.Count != 0 && !txt.Text.Contains("Net Assessment") && !txt.Text.Contains("Total"))
                            {
                                string Assessvalue = TDbulkavd1[0].Text + "~" + TDbulkavd1[1].Text + "~" + TDbulkavd1[2].Text + "~" + TDbulkavd1[3].Text + "~" + TDbulkavd1[4].Text + "~" + TDbulkavd1[5].Text;

                                gc.insert_date(orderNumber, parcel_Id, 881, Assessvalue, 1, DateTime.Now);
                            }
                            if (TDbulkavd1.Count != 0 && txt.Text.Contains("Total"))
                            {
                                string Assessvalue1 = THbulkavd1[0].Text + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + TDbulkavd1[0].Text;

                                gc.insert_date(orderNumber, parcel_Id, 881, Assessvalue1, 1, DateTime.Now);
                            }
                        }
                    }
                    catch { }
                    try
                    {
                        IWebElement IAssessmentHistory = driver.FindElement(By.XPath("//*[@id='primarynav']/li[4]/ul/li[6]/a"));
                        IAssessmentHistory.Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcel_Id, "Assessment History", driver, "DE", "Sussex");
                    }
                    catch { }

                    try
                    {
                        IWebElement bulkahd = driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_AssessmentHistoryGrid']/tbody"));
                        IList<IWebElement> TRbulkahd = bulkahd.FindElements(By.TagName("tr"));
                        IList<IWebElement> THbulkahd = bulkahd.FindElements(By.TagName("th"));
                        IList<IWebElement> TDbulkahd;
                        foreach (IWebElement txt in TRbulkahd)
                        {
                            TDbulkahd = txt.FindElements(By.TagName("td"));
                            THbulkahd = txt.FindElements(By.TagName("th"));
                            if (TDbulkahd.Count != 0 && !txt.Text.Contains("Building Value"))
                            {
                                string Assesshistorydetails = TDbulkahd[0].Text + "~" + TDbulkahd[1].Text + "~" + TDbulkahd[2].Text + "~" + TDbulkahd[3].Text + "~" + TDbulkahd[4].Text;

                                gc.insert_date(orderNumber, parcel_Id, 882, Assesshistorydetails, 1, DateTime.Now);
                            }

                        }
                    }
                    catch { }

                    //Tax Bill (All Bill)
                    try
                    {

                        IWebElement ITaxRates = driver.FindElement(By.XPath("//*[@id='primarynav']/li[4]/ul/li[7]/a"));
                        ITaxRates.Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcel_Id, "Tax Rates", driver, "DE", "Sussex");
                        IWebElement IAllTax = driver.FindElement(By.XPath("//*[@id='primarynav']/li[4]/ul/li[8]/a"));
                        IAllTax.Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcel_Id, "All Bills", driver, "DE", "Sussex");
                    }
                    catch { }

                    try
                    {

                        var Driver = new ChromeDriver();
                        //Driver.Navigate().GoToUrl("https://munis.sussexcountyde.gov/MSS/citizens/RealEstate/Default.aspx");
                        //driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_Control_ParcelIdSearchFieldLayout_ctl01_ParcelIDTextBox")).SendKeys(parcelNumber);
                        //driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_Control_FiscalYearLayoutItem_ctl01_YearSearchTextBox']")).SendKeys(iyear.ToString());
                        //gc.CreatePdf(orderNumber, parcelNumber, "Tax Information Page Search", driver, "DE", "Sussex");
                        //driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_Control_FormLayoutItem7_ctl01_Button1']")).SendKeys(Keys.Enter);
                        //Thread.Sleep(5000);
                        IWebElement allbillutility = driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_BillsRepeater_ctl00_BillsGrid']/tbody"));
                        IList<IWebElement> TRallbillutility = allbillutility.FindElements(By.TagName("tr"));
                        ByVisibleElement(driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_BillsRepeater_ctl01_BillsGrid']/tbody/tr[7]")));
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcel_Id, "All Bills-Utility", Driver, "DE", "Sussex");
                        Thread.Sleep(2000);
                        Driver.Quit();
                    }
                    catch (Exception ex)
                    { }

                    try
                    {
                        List<string> TaxBill = new List<string>();
                        IWebElement allbill = driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_BillsRepeater_ctl00_BillsGrid']/tbody"));
                        IList<IWebElement> TRallbill = allbill.FindElements(By.TagName("tr"));
                        IList<IWebElement> THallbill = allbill.FindElements(By.TagName("th"));
                        IList<IWebElement> TDallbill;
                        foreach (IWebElement txt in TRallbill)
                        {
                            TDallbill = txt.FindElements(By.TagName("td"));
                            THallbill = txt.FindElements(By.TagName("th"));


                            if (TDallbill.Count != 0 && !txt.Text.Contains("Owner") && txt.Text.Contains("Paid"))
                            {
                                IWebElement Ibill = TDallbill[5].FindElement(By.TagName("a"));
                                if (Ibill.Text != "" && Ibill.Text.Contains("View Bill"))
                                {
                                    string strBillUrl = Ibill.GetAttribute("id");
                                    TaxBill.Add(strBillUrl);
                                }

                            }
                        }

                        foreach (string TaxBillURLLink in TaxBill)
                        {
                            // driver.Navigate().GoToUrl(TaxBillURLLink);
                            IWebElement element1 = driver.FindElement(By.Id(TaxBillURLLink));
                            element1.Click();
                            Thread.Sleep(2000);
                            try
                            {
                                int i = 0;
                                driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_ViewBill1_ViewPaymentsLinkButton']")).SendKeys(Keys.Enter);
                                Thread.Sleep(1000);
                                string allbills = driver.FindElement(By.XPath("//*[@id='BillActivityTable']/tbody")).Text;
                                string strall_billyear = "", all_billyear = "", all_bill = "";
                                strall_billyear = GlobalClass.After(allbills, "Year");
                                all_bill = GlobalClass.After(strall_billyear, "Bill");
                                all_billyear = GlobalClass.Before(strall_billyear, "Bill").Replace("\r\n", "");
                                gc.CreatePdf(orderNumber, parcel_Id, "Tax Payment Bill" + all_billyear + all_bill, driver, "DE", "Sussex");
                                IWebElement allbill1 = driver.FindElement(By.XPath("//*[@id='molContentContainer']/div/table[2]/tbody"));
                                IList<IWebElement> TRallbill1 = allbill1.FindElements(By.TagName("tr"));
                                IList<IWebElement> THallbill1 = allbill1.FindElements(By.TagName("th"));
                                IList<IWebElement> TDallbill1;
                                foreach (IWebElement text in TRallbill1)
                                {
                                    TDallbill1 = text.FindElements(By.TagName("td"));
                                    THallbill1 = text.FindElements(By.TagName("th"));

                                    if (TDallbill1.Count != 0 && !text.Text.Contains("Paid By/Reference"))
                                    {
                                        string Allbilldetails = all_billyear + "~" + all_bill + "~" + TDallbill1[0].Text + "~" + TDallbill1[1].Text + "~" + TDallbill1[2].Text + "~" + TDallbill1[3].Text;
                                        gc.insert_date(orderNumber, parcel_Id, 884, Allbilldetails, 1, DateTime.Now);
                                    }
                                }

                                i++;
                                driver.Navigate().Back();
                                driver.Navigate().Back();
                            }
                            catch { }
                        }

                    }
                    catch { }
                    // Tax Info Details and Tax Payments


                    string Vasof = "", Vbillyear = "", Vbill = "", Vowner = "", Vparcelid = "";
                    IWebElement IMasofDate;
                    int iyear = Syear;
                    for (int i = 1; i <= 3; i++)
                    {
                        try
                        {
                            string myear = DateTime.Now.Year.ToString();
                            int year = Int32.Parse(myear);
                            //iyear = iyear;
                            driver.Navigate().GoToUrl("https://munis.sussexcountyde.gov/MSS/citizens/RealEstate/Default.aspx");
                            Thread.Sleep(2000);
                            //gc.CreatePdf(orderNumber, parcel_Id, "Tax Information Page", driver, "DE", "Sussex");
                            string parcel = parcel_Id.Replace(" ", "-").Replace("--", "-");
                            driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_Control_ParcelIdSearchFieldLayout_ctl01_ParcelIDTextBox']")).SendKeys(parcel);
                            driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_Control_FiscalYearLayoutItem_ctl01_YearSearchTextBox']")).SendKeys(iyear.ToString());
                            gc.CreatePdf(orderNumber, parcelNumber, "Tax Information Page Search" + iyear, driver, "DE", "Sussex");
                            driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_Control_FormLayoutItem7_ctl01_Button1']")).SendKeys(Keys.Enter);
                            Thread.Sleep(4000);
                            try
                            {
                                IWebElement Delinquent = driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_PaymentBlockMessage_BlockageMessageParagraph']/span"));
                                if (Delinquent.Text.Contains("Prior unpaid bill(s) exist for this parcel") || Delinquent.Text.Contains("Prior and newer unpaid bills exist for this parcel"))
                                {
                                    HttpContext.Current.Session["Delinquent_SussexDE"] = "Yes";
                                    Thread.Sleep(3000);
                                    IMasofDate = driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_ViewBill1_AsOfDateTextBox']"));
                                    IMasofDate.Clear();
                                    string currDate = DateTime.Now.ToString("MM/dd/yyyy");
                                    string dateChecking = DateTime.Now.ToString("MM") + "/15/" + DateTime.Now.ToString("yyyy");
                                    if (Convert.ToDateTime(currDate) > Convert.ToDateTime(dateChecking))
                                    {
                                        string nextEndOfMonth = "";
                                        if ((Convert.ToInt16(Convert.ToInt16(DateTime.Now.ToString("MM"))) < 12))
                                        {
                                            nextEndOfMonth = new DateTime(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(Convert.ToInt16(DateTime.Now.ToString("MM")) + 1), DateTime.DaysInMonth(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(DateTime.Now.ToString("MM")) + 1)).ToString("MM/dd/yyyy");
                                            IMasofDate.SendKeys(nextEndOfMonth);
                                        }
                                        else
                                        {
                                            int nxtYr = Convert.ToInt16(DateTime.Now.ToString("yyyy")) + 1;
                                            nextEndOfMonth = nextEndOfMonth = new DateTime(nxtYr, 1, DateTime.DaysInMonth(Convert.ToInt16(DateTime.Now.ToString("yyyy")), 1)).ToString("MM/dd/yyyy");
                                            IMasofDate.SendKeys(nextEndOfMonth);
                                        }
                                        string[] daysplit = nextEndOfMonth.Split('/');
                                        IWebElement Inextmonth = driver.FindElement(By.XPath("//*[@id='ui-datepicker-div']/div/a[2]"));
                                        Inextmonth.SendKeys(Keys.Enter);
                                        Vasof = nextEndOfMonth;
                                        IWebElement Iday = driver.FindElement(By.XPath("//*[@id='ui-datepicker-div']/table/tbody"));
                                        IList<IWebElement> IdayRow = Iday.FindElements(By.TagName("a"));
                                        foreach (IWebElement day in IdayRow)
                                        {
                                            if (day.Text != "" && day.Text == daysplit[1])
                                            {
                                                day.SendKeys(Keys.Enter);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        string EndOfMonth = new DateTime(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(Convert.ToInt16(DateTime.Now.ToString("MM"))), DateTime.DaysInMonth(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(DateTime.Now.ToString("MM")))).ToString("MM/dd/yyyy");
                                        IMasofDate.SendKeys(EndOfMonth);
                                        string[] Edaysplit = EndOfMonth.Split('/');
                                        Vasof = EndOfMonth;
                                        IWebElement IEday = driver.FindElement(By.XPath("//*[@id='ui-datepicker-div']/table/tbody"));
                                        IList<IWebElement> IEdayRow = IEday.FindElements(By.TagName("a"));
                                        foreach (IWebElement Eday in IEdayRow)
                                        {
                                            if (Eday.Text != "" && Eday.Text == Edaysplit[1])
                                            {
                                                Eday.SendKeys(Keys.Enter);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    IMasofDate = driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_ViewBill1_AsOfDateTextBox']"));
                                    Vasof = IMasofDate.GetAttribute("value");
                                }
                            }
                            catch { }

                            gc.CreatePdf(orderNumber, parcel_Id, "View Bill" + iyear, driver, "DE", "Sussex");
                            try
                            {
                                IWebElement IasofDate = driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_ViewBill1_AsOfDateTextBox']"));
                                Vasof = IasofDate.GetAttribute("value");
                                string Vbulktxt = driver.FindElement(By.XPath("//*[@id='BillDetailTable']/tbody")).Text;
                                string strVbillyear = gc.Between(Vbulktxt, "Bill Year", "Owner");
                                Vbillyear = GlobalClass.Before(strVbillyear, "Bill");
                                Vbill = GlobalClass.After(strVbillyear, "Bill");
                                Vowner = gc.Between(Vbulktxt, "Owner", "Parcel ID");
                                Vparcelid = GlobalClass.After(Vbulktxt, "Parcel ID");
                            }
                            catch { }
                            try
                            {
                                IWebElement Bigview = driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_ViewBill1_BillDetailsUpdatePanel']/table/tbody"));
                                IList<IWebElement> TRBigview = Bigview.FindElements(By.TagName("tr"));
                                IList<IWebElement> THBigview = Bigview.FindElements(By.TagName("th"));
                                IList<IWebElement> TDBigview;
                                foreach (IWebElement row in TRBigview)
                                {
                                    TDBigview = row.FindElements(By.TagName("td"));
                                    if (TRBigview.Count > 1 && TDBigview.Count != 0 && !row.Text.Contains("TOTAL") && !row.Text.Contains("Interest"))
                                    {
                                        string taxdetails = Vasof + "~" + Vbillyear + "~" + Vbill + "~" + Vowner + "~" + Vparcelid + "~" + TDBigview[0].Text + "~" + TDBigview[1].Text + "~" + TDBigview[2].Text + "~" + TDBigview[3].Text + "~" + TDBigview[4].Text + "~" + TDBigview[5].Text + "~" + TDBigview[6].Text + "~" + TaxingAuthority;

                                        gc.insert_date(orderNumber, parcel_Id, 858, taxdetails, 1, DateTime.Now);
                                    }
                                    if (TRBigview.Count > 2 && TDBigview.Count != 0 && !row.Text.Contains("TOTAL") && row.Text.Contains("Interest"))
                                    {

                                        string taxdetails1 = "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + TDBigview[0].Text + "~" + "" + "~" + "" + "~" + TDBigview[2].Text + "~" + "" + "~" + "" + "~" + TDBigview[5].Text + "~" + TaxingAuthority;
                                        gc.insert_date(orderNumber, parcel_Id, 858, taxdetails1, 1, DateTime.Now);
                                    }
                                    if (TRBigview.Count > 2 && TDBigview.Count != 0 && row.Text.Contains("TOTAL"))
                                    {

                                        string taxdetails2 = "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + TDBigview[0].Text + "~" + "" + "~" + TDBigview[1].Text + "~" + TDBigview[2].Text + "~" + TDBigview[3].Text + "~" + TDBigview[4].Text + "~" + TDBigview[5].Text + "~" + TaxingAuthority;
                                        gc.insert_date(orderNumber, parcel_Id, 858, taxdetails2, 1, DateTime.Now);
                                    }
                                }

                            }
                            catch (Exception ex) { }
                            try
                            {

                                driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_ViewBill1_ViewPaymentsLinkButton']")).SendKeys(Keys.Enter);
                                Thread.Sleep(3000);
                                gc.CreatePdf(orderNumber, parcel_Id, "View payments" + iyear, driver, "DE", "Sussex");
                            }
                            catch { }
                            try
                            {
                                string bulktext1 = driver.FindElement(By.XPath("//*[@id='BillActivityTable']")).Text;
                                string bill_year = "", strtaxbill = "", taxbill = "";
                                strtaxbill = GlobalClass.After(bulktext1, "Bill Year");
                                bill_year = GlobalClass.Before(strtaxbill, "Bill").Replace("\r\n", "");
                                taxbill = GlobalClass.After(strtaxbill, "Bill");
                                IWebElement Bigdata1 = driver.FindElement(By.XPath("//*[@id='molContentContainer']/div/table[2]/tbody"));
                                IList<IWebElement> TRBigdata1 = Bigdata1.FindElements(By.TagName("tr"));
                                IList<IWebElement> THBigdata1 = Bigdata1.FindElements(By.TagName("th"));
                                IList<IWebElement> TDBigdata1;
                                foreach (IWebElement row in TRBigdata1)
                                {
                                    TDBigdata1 = row.FindElements(By.TagName("td"));

                                    if (row.Text.Contains("No payment activity could be found for this bill."))
                                    {
                                        string paymentdetails = bill_year + "~" + taxbill + "~" + "" + "~" + "" + "~" + "" + "~" + "";

                                        gc.insert_date(orderNumber, parcel_Id, 877, paymentdetails, 1, DateTime.Now);
                                    }
                                    if (TRBigdata1.Count > 1 && TDBigdata1.Count != 0 && !row.Text.Contains("Activity"))
                                    {
                                        string paymentdetails1 = bill_year + "~" + taxbill + "~" + TDBigdata1[0].Text + "~" + TDBigdata1[1].Text + "~" + TDBigdata1[2].Text + "~" + TDBigdata1[3].Text;

                                        gc.insert_date(orderNumber, parcel_Id, 877, paymentdetails1, 1, DateTime.Now);
                                    }
                                }
                                iyear--;
                            }
                            catch (Exception ex)
                            { }
                            // iyear--;
                        }
                        catch { }
                    }

                    // Tax Bill Download

                    try
                    {
                        var chromeOptions = new ChromeOptions();
                        var chDriver = new ChromeDriver(chromeOptions);
                        int myear = 0;
                        int Stryear = DateTime.Now.Year;
                        int Strmonth = DateTime.Now.Month;
                        if (Strmonth >= 9)
                        {
                            myear = Stryear;
                        }
                        else
                        {
                            Stryear--;
                            myear = Stryear;

                        }
                        chDriver.Navigate().GoToUrl("https://munis.sussexcountyde.gov/MSS/citizens/RealEstate/Default.aspx");
                        Thread.Sleep(2000);
                        // gc.CreatePdf(orderNumber, parcel_Id, "Tax Information Page", chDriver, "DE", "Sussex");
                        string parcel = parcel_Id.Replace(" ", "-").Replace("--", "-");
                        chDriver.FindElement(By.XPath("//*[@id='ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_Control_ParcelIdSearchFieldLayout_ctl01_ParcelIDTextBox']")).SendKeys(parcel);
                        chDriver.FindElement(By.XPath("//*[@id='ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_Control_FiscalYearLayoutItem_ctl01_YearSearchTextBox']")).SendKeys(Convert.ToString(myear));
                        //gc.CreatePdf(orderNumber, parcelNumber, "Tax Information Page Search", chDriver, "DE", "Sussex");
                        chDriver.FindElement(By.XPath("//*[@id='ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_Control_FormLayoutItem7_ctl01_Button1']")).SendKeys(Keys.Enter);

                        try
                        {
                            IWebElement IReal = chDriver.FindElement(By.XPath("//*[@id='primarynav']/li[4]/ul/li[1]/a"));
                            IReal.Click();
                            Thread.Sleep(2000);
                            //js.ExecuteScript("arguments[0].click();", IReal);
                            IWebElement IBillTax = chDriver.FindElement(By.LinkText("View bill image"));
                            IBillTax.Click();
                            Thread.Sleep(2000);
                            //js.ExecuteScript("arguments[0].click();", IBillTax);
                            //IWebElement Ilink = driver.FindElement(By.LinkText("View bill image"));
                            // Ilink.Click();
                            //Thread.Sleep(9000);
                            chDriver.SwitchTo().Window(chDriver.WindowHandles.Last());
                            Thread.Sleep(2000);
                            gc.CreatePdf(orderNumber, parcel_Id, "TaxBill Download", chDriver, "DE", "Sussex");
                            try
                            {
                                gc.downloadfile(chDriver.Url, orderNumber, parcel_Id, "TaxBill Download", "DE", "Sussex");
                            }
                            catch { chDriver.Quit(); }
                        }
                        catch { chDriver.Quit(); }
                        chDriver.Quit();
                    }
                    catch { }


                    TaxTime = DateTime.Now.ToString("HH:mm:ss");
                    //Staford -- City Tax Info
                    try
                    {
                        if (town.Contains("Seaford"))
                        {
                            string City_TaxingAuthority = "";
                            driver.Navigate().GoToUrl("https://wipp.edmundsassoc.com/Wipp/?wippid=SFRD");
                            Thread.Sleep(2000);
                            gc.CreatePdf_WOP(orderNumber, "City Taxing Authority", driver, "DE", "Sussex");
                            string bulkTaxauth = driver.FindElement(By.XPath("/html/body")).Text;
                            City_TaxingAuthority = gc.Between(bulkTaxauth, "City of Seaford", "Office Hours").Trim();
                            gc.CreatePdf(orderNumber, parcel_Id, "CityTax Home Page", driver, "DE", "Sussex");
                            driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/div/table[1]/tbody/tr/td/table/tbody/tr[2]/td/table/tbody/tr[2]/td[5]/input")).SendKeys(PropertyAdd);
                            driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/div/table[1]/tbody/tr/td/table/tbody/tr[2]/td/table/tbody/tr[2]/td[6]/button")).SendKeys(Keys.Enter);

                            gc.CreatePdf(orderNumber, parcel_Id, "CityTax Address search", driver, "DE", "Sussex");
                            try
                            {
                                IWebElement Citytaxtext = driver.FindElement(By.XPath("/html/body/div[2]/div/table/tbody/tr[2]/td[2]/div/table/tbody/tr[1]/td/table/tbody"));
                                IList<IWebElement> TRCitytaxtext = Citytaxtext.FindElements(By.TagName("tr"));
                                IList<IWebElement> THCitytaxtext = Citytaxtext.FindElements(By.TagName("th"));
                                IList<IWebElement> TDCitytaxtext;
                                foreach (IWebElement text in TRCitytaxtext)
                                {
                                    TDCitytaxtext = text.FindElements(By.TagName("td"));
                                    THCitytaxtext = text.FindElements(By.TagName("th"));

                                    if (TDCitytaxtext.Count != 0 && !text.Text.Contains("Property Location") && text.Text.Contains(address.ToUpper()))
                                    {
                                        IWebElement taxclick = TDCitytaxtext[0].FindElement(By.TagName("input"));
                                        taxclick.Click();


                                    }
                                }
                            }

                            catch { }

                            gc.CreatePdf(orderNumber, parcel_Id, "CityTax Address Result", driver, "DE", "Sussex");
                            string citytax = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[1]/td/table/tbody/tr/td/table/tbody")).Text;
                            string pid = "", propertylocation = "", OwnerName = "", OwnerAddress = "", taxaccountid = "", zoning_code = "";
                            string ExemptValue = "", TotalAssessedValue = "", Deduction = "";
                            pid = gc.Between(citytax, "PID", "Tax Account Id").Replace(":", "");
                            propertylocation = gc.Between(citytax, "Property Location", "Zoning Code").Replace(":", "");
                            OwnerName = gc.Between(citytax, "Owner Name/Address", "Land Value").Replace(":", "");
                            IWebElement OwnerAddress1 = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[1]/td/table/tbody/tr/td/table/tbody/tr[4]/td[2]"));
                            IWebElement OwnerAddress2 = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[1]/td/table/tbody/tr/td/table/tbody/tr[5]/td[2]"));
                            OwnerAddress = OwnerAddress1.Text + OwnerAddress2.Text;
                            //OwnerAddress = gc.Between(citytax, "Land Value", "Exempt Value").Replace(":", "");
                            taxaccountid = gc.Between(citytax, "Tax Account Id", "Property Location").Replace(":", "");
                            zoning_code = gc.Between(citytax, "Zoning Code", "Owner Name/Address").Replace(":", "");
                            IWebElement Land_Value = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[1]/td/table/tbody/tr/td/table/tbody/tr[3]/td[4]"));
                            IWebElement ImprovementValue = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[1]/td/table/tbody/tr/td/table/tbody/tr[4]/td[4]"));

                            ExemptValue = gc.Between(citytax, "Exempt Value", "Total Assessed Value").Replace(":", "");
                            TotalAssessedValue = gc.Between(citytax, "Total Assessed Value", "Deductions").Replace(":", "");
                            Deduction = GlobalClass.After(citytax, "Deductions").Replace(":", "");

                            string Citytaxdetails = pid + "~" + propertylocation + "~" + OwnerName + "~" + OwnerAddress + "~" + taxaccountid + "~" + zoning_code + "~" + Land_Value.Text + "~" + ImprovementValue.Text + "~" + ExemptValue + "~" + TotalAssessedValue + "~" + Deduction + "~" + City_TaxingAuthority;
                            gc.insert_date(orderNumber, parcel_Id, 886, Citytaxdetails, 1, DateTime.Now);

                        }

                        try
                        {
                            if (town.Contains("Seaford"))
                            {

                                string lastpay_info = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[3]/td/table/tbody/tr[2]/td/div/div/table/tbody/tr[3]/td/table/tbody")).Text;
                                //string lastpayment = "";
                                //lastpayment = GlobalClass.After(lastpay_info, "Last Payment");
                                IWebElement citytax = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[3]/td/table/tbody/tr[2]/td/div/div/table/tbody/tr[2]/td/table/tbody"));
                                IList<IWebElement> TRcitytax = citytax.FindElements(By.TagName("tr"));
                                IList<IWebElement> THcitytax = citytax.FindElements(By.TagName("th"));
                                IList<IWebElement> TDcitytax;
                                foreach (IWebElement text in TRcitytax)
                                {
                                    TDcitytax = text.FindElements(By.TagName("td"));
                                    THcitytax = text.FindElements(By.TagName("th"));

                                    if (TDcitytax.Count != 0 && !text.Text.Contains("Total Due"))
                                    {
                                        string Citytaxpayment = TDcitytax[0].Text + "~" + TDcitytax[1].Text + "~" + TDcitytax[2].Text + "~" + TDcitytax[3].Text + "~" + TDcitytax[5].Text + "~" + TDcitytax[6].Text + "~" + TDcitytax[7].Text + "~" + TDcitytax[8].Text;
                                        gc.insert_date(orderNumber, parcel_Id, 887, Citytaxpayment + "~" + lastpay_info, 1, DateTime.Now);
                                    }
                                }
                            }
                        }
                        catch { }
                    }
                    catch (Exception ex) { }

                    CitytaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "DE", "Sussex", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    gc.mergpdf(orderNumber, "DE", "Sussex");
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
    }
}

