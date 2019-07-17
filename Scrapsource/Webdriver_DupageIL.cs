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
    public class Webdriver_DupageIL
    {
        IWebDriver driver;
        DBconnection db = new DBconnection();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        GlobalClass gc = new GlobalClass();
        IWebElement Address1;
        public string Ftp_Dupage(string streetno, string streetname, string direction, string streettype, string unitnumber, string parcelNumber, string ownernm, string searchType, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
            string Parcel_number = "", Tax_Authority = "", Year = "", Property = "", Propertyresult = "", Valuvationresult = "", Multiaddressadd = "", Addresshrf = "";
            // driver = new ChromeDriver();
            // driver = new PhantomJSDriver();
            //rdp
            using (driver = new PhantomJSDriver())
            {
                if (searchType == "titleflex")
                {

                    gc.TitleFlexSearch(orderNumber, "", ownernm, "", "IL", "Dupage");
                    if (HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes")
                    {
                        driver.Quit();
                        return "MultiParcel";
                    }
                    else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                    {
                        HttpContext.Current.Session["Zero_Dupage"] = "Zero";
                        driver.Quit();
                        return "No Data Found";
                    }
                    parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                    searchType = "parcel";
                }
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");
                    driver.Navigate().GoToUrl("http://www.dupageco.org/Treasurer/PayingTaxes/");
                    Thread.Sleep(2000);
                    try
                    {//*[@id="prefix-overlay-header"]/button
                        driver.FindElement(By.XPath("//*[@id='prefix-overlay-header']/button")).Click();
                        Thread.Sleep(2000);
                    }
                    catch { }
                    gc.CreatePdf_WOP(orderNumber, "Tax Authority", driver, "IL", "Dupage");
                    string Tax_Authority1 = driver.FindElement(By.XPath("//*[@id='Main']/p[10]")).Text;
                    Tax_Authority = "DuPage County Collector,P.O.Box 4203, Carol Stream, IL 60197 - 4203";

                }
                catch { }
                try
                {
                    driver.Navigate().GoToUrl("http://www.dupageco.org/PropertyInfo/PropertyLookUp.aspx");
                    try
                    {
                        driver.FindElement(By.Id("prefix-dismissButton")).Click();
                        Thread.Sleep(2000);
                    }
                    catch { }
                    if (searchType == "address")
                    {
                        driver.FindElement(By.Id("ctl00_pageContent_ctl00_txtStreetNumber")).SendKeys(streetno);
                        if (direction != "")
                        {
                            IWebElement PropertyInformation = driver.FindElement(By.Id("ctl00_pageContent_ctl00_ddlDir"));
                            SelectElement PropertyInformationSelect = new SelectElement(PropertyInformation);
                            PropertyInformationSelect.SelectByValue(direction.ToUpper().Trim());
                        }
                        driver.FindElement(By.Id("ctl00_pageContent_ctl00_txtStreet")).SendKeys(streetname);
                        driver.FindElement(By.Id("ctl00_pageContent_ctl00_txtNumber")).SendKeys(unitnumber);
                        gc.CreatePdf_WOP(orderNumber, "SearchBefore", driver, "IL", "Dupage");
                        driver.FindElement(By.Id("ctl00_pageContent_ctl00_btnSearch")).Click();
                        Thread.Sleep(2000);

                        try
                        {
                            try
                            {
                                Property = driver.FindElement(By.Id("Main")).Text;
                            }
                            catch { }
                            int Max = 0;
                            if (!Property.Contains("Property Information"))
                            {
                                //*[@id="ContentPlaceHolder1_gvSearchResults"]/tbody
                                gc.CreatePdf_WOP(orderNumber, "SearchAfter", driver, "IL", "Dupage");
                                IWebElement Multiparceladdress = driver.FindElement(By.Id("ctl00_pageContent_ctl00_gvList"));
                                IList<IWebElement> Multiparcelrow = Multiparceladdress.FindElements(By.TagName("tr"));
                                IList<IWebElement> Multiparcelid;
                                foreach (IWebElement multiparcel in Multiparcelrow)
                                {
                                    Multiparcelid = multiparcel.FindElements(By.TagName("td"));
                                    if (Multiparcelid.Count != 0 && streetno.Trim() == Multiparcelid[1].Text.Trim() && Multiparcelid[3].Text.Trim().Contains(streetname.ToUpper()))
                                    {
                                        Address1 = Multiparcelid[0].FindElement(By.TagName("a"));
                                        Addresshrf = Address1.GetAttribute("href");
                                        string Stnumber = Multiparcelid[1].Text;
                                        string Dir = Multiparcelid[2].Text;
                                        string street = Multiparcelid[3].Text;
                                        string Unit = Multiparcelid[4].Text;
                                        string City = Multiparcelid[5].Text;
                                        string Zip = Multiparcelid[6].Text;
                                        string Addressst = Stnumber.Trim() + " " + Dir.Trim() + " " + street.Trim() + " " + Unit.Trim() + " " + City + " " + Zip;
                                        // string Owner = Multiparcelid[1].Text;
                                        string Pin = Multiparcelid[0].Text;
                                        string Multiparcel = Addressst;
                                        gc.insert_date(orderNumber, Pin, 1705, Multiparcel, 1, DateTime.Now);
                                        Max++;
                                    }
                                }
                                if (Max == 1)
                                {
                                    Address1.Click();
                                    Thread.Sleep(2000);
                                }
                                if (Max > 1 && Max < 26)
                                {
                                    HttpContext.Current.Session["multiParcel_Dupage"] = "Maximum";
                                    gc.CreatePdf_WOP(orderNumber, "No Data Found", driver, "IL", "Dupage");
                                    driver.Quit();
                                    return "MultiParcel";
                                }
                                if (Max > 25)
                                {
                                    HttpContext.Current.Session["multiParcel_Dupage_Multicount"] = "Maximum";
                                    driver.Quit();
                                    return "Maximum";
                                }
                                if (Max == 0)
                                {
                                    HttpContext.Current.Session["Zero_Dupage"] = "Zero";
                                    driver.Quit();
                                    return "No Data Found";
                                }
                            }
                        }
                        catch { }

                    }
                    if (searchType == "parcel")
                    {
                        driver.FindElement(By.Id("ctl00_pageContent_ctl00_txtParcel")).SendKeys(parcelNumber.Replace("-", ""));
                        gc.CreatePdf(orderNumber, Parcel_number, "Search Before", driver, "IL", "Dupage");
                        driver.FindElement(By.Id("ctl00_pageContent_ctl00_btnSearch")).Click();
                        Thread.Sleep(2000);
                    }

                    try
                    {
                        string nodata = driver.FindElement(By.XPath("//*[@id='ctl00_pageContent_ctl00_pnlSearchResults']")).Text;
                        if (nodata.Contains("no records that match the search criteria"))
                        {
                            HttpContext.Current.Session["Zero_Dupage"] = "Zero";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }

                    Parcel_number = driver.FindElement(By.XPath("//*[@id='tabid01']/div[2]/div[2]")).Text;
                    string[] Parcelsplit = Parcel_number.Split('-');
                    string firstsplit = Parcelsplit[0].Trim();
                    string secondsplit = Parcelsplit[1];
                    string threesplit = Parcelsplit[2];
                    string foursplit = Parcelsplit[3];
                    string propertyaddress = driver.FindElement(By.XPath("//*[@id='tabid01']/div[3]/div[2]")).Text;
                    string ownersplit = driver.FindElement(By.XPath("//*[@id='tabid01']/div[4]/div[2]")).Text;
                    string[] ownerarray = ownersplit.Split('\n');
                    string Ownername = ownerarray[0];
                    string mailaddress = ownerarray[1] + " " + ownerarray[2];
                    string taxinfo = propertyaddress + "~" + mailaddress + "~" + Ownername + "~" + Tax_Authority;
                    gc.insert_date(orderNumber, Parcel_number, 1670, taxinfo, 1, DateTime.Now);
                    gc.CreatePdf(orderNumber, Parcel_number, "Tax Site Enter", driver, "IL", "Dupage");
                    //Installment

                    IWebElement Installmenttable = driver.FindElement(By.Id("ctl00_pageContent_ctl00_tblResults"));
                    IList<IWebElement> Installmentrow = Installmenttable.FindElements(By.TagName("tr"));
                    IList<IWebElement> Installmentid;
                    foreach (IWebElement Installment in Installmentrow)
                    {
                        Installmentid = Installment.FindElements(By.TagName("td"));
                        if (Installmentid.Count > 1 && Installmentid.Count== 5)
                        {
                            string[] instalsplit = Installmentid[0].Text.Split(':');
                            string Installmentresult = "~" + instalsplit[0] + "~" + instalsplit[1] + "~" + Installmentid[1].Text + "~" + Installmentid[2].Text + "~" + Installmentid[3].Text + "~" + Installmentid[4].Text;
                            gc.insert_date(orderNumber, Parcel_number, 1671, Installmentresult, 1, DateTime.Now);
                        }
                        if (Installmentid.Count ==3)
                        {
                            string Installmentresult1 = "~" + Installmentid[0].Text + "~" + "" + "~" + Installmentid[1].Text + "~" + Installmentid[2].Text + "~" + "" + "~" + "";
                            gc.insert_date(orderNumber, Parcel_number, 1671, Installmentresult1, 1, DateTime.Now);

                        }
                    }

                    try
                    {
                        //Prior Year 1
                        string prioryear1split = driver.FindElement(By.XPath("//*[@id='tabid01']")).Text;
                        string prioryear1tax = gc.Between(prioryear1split, "Year", "Taxes");
                        IWebElement PriorYear1table = driver.FindElement(By.XPath("//*[@id='tabid01']"));
                        IList<IWebElement> PriorYear1Div = PriorYear1table.FindElements(By.TagName("div"));
                        IList<IWebElement> PriorYear1Row;
                        IList<IWebElement> PriorYear1Td;
                        foreach (IWebElement PriorYear1 in PriorYear1Div)
                        {
                            PriorYear1Row = PriorYear1.FindElements(By.TagName("tr"));
                            if (PriorYear1Row.Count != 0 && !PriorYear1.Text.Contains("Due") && PriorYear1.Text.Contains("Prior Year"))
                            {
                                prioryear1tax = gc.Between(PriorYear1.FindElement(By.TagName("h2")).Text, "Year", "Taxes").Trim();
                                foreach (IWebElement Prior in PriorYear1Row)
                                {
                                    PriorYear1Td = Prior.FindElements(By.TagName("td"));
                                    if (PriorYear1Td.Count != 0 && PriorYear1Td.Count == 3 && !Prior.Text.Contains("Installment"))
                                    {
                                        string PriorYear1Result = prioryear1tax + "~" + PriorYear1Td[0].Text + "~" + "" + "~" + PriorYear1Td[1].Text + "~" + "" + "~" + "" + "~" + PriorYear1Td[2].Text;
                                        gc.insert_date(orderNumber, Parcel_number, 1671, PriorYear1Result, 1, DateTime.Now);
                                    }
                                }
                            }
                        }
                    }
                    catch { }

                    ////Prior Year 1
                    //string prioryear1split = driver.FindElement(By.XPath("//*[@id='tabid01']/div[8]/h2")).Text;
                    //string prioryear1tax = gc.Between(prioryear1split, "Year", "Taxes");
                    //IWebElement PriorYear1table = driver.FindElement(By.XPath("//*[@id='tabid01']/div[8]/table/tbody"));
                    //IList<IWebElement> PriorYear1row = PriorYear1table.FindElements(By.TagName("tr"));
                    //IList<IWebElement> PriorYear1id;
                    //foreach (IWebElement PriorYear1 in PriorYear1row)
                    //{
                    //    PriorYear1id = PriorYear1.FindElements(By.TagName("td"));
                    //    if (!PriorYear1.Text.Contains("Installment"))
                    //    {
                    //        string PriorYear1Result = prioryear1tax + "~" + PriorYear1id[0].Text + "~" + "" + "~" + PriorYear1id[1].Text + "~" + "" + "~" + "" + "~" + PriorYear1id[2].Text;
                    //        gc.insert_date(orderNumber, Parcel_number, 1671, PriorYear1Result, 1, DateTime.Now);
                    //    }
                    //}
                    //string prioryear2split = driver.FindElement(By.XPath("//*[@id='tabid01']/div[9]/h2")).Text;
                    //string prioryear2tax = gc.Between(prioryear2split, "Year", "Taxes");

                    //IWebElement PriorYear2table = driver.FindElement(By.XPath("//*[@id='tabid01']/div[9]/table/tbody"));
                    //IList<IWebElement> PriorYear2row = PriorYear2table.FindElements(By.TagName("tr"));
                    //IList<IWebElement> PriorYear2id;
                    //foreach (IWebElement PriorYear2 in PriorYear2row)
                    //{
                    //    PriorYear2id = PriorYear2.FindElements(By.TagName("td"));
                    //    if (!PriorYear2.Text.Contains("Installment"))
                    //    {
                    //        string PriorYear2Result = prioryear2tax + "~" + PriorYear2id[0].Text + "~" + "" + "~" + PriorYear2id[1].Text + "~" + "" + "~" + "" + "~" + PriorYear2id[2].Text;
                    //        gc.insert_date(orderNumber, Parcel_number, 1671, PriorYear2Result, 1, DateTime.Now);
                    //    }
                    //}
                    //string prioryear3split = driver.FindElement(By.XPath("//*[@id='tabid01']/div[10]/h2")).Text;
                    //string prioryear3tax = gc.Between(prioryear3split, "Year", "Taxes");
                    //IWebElement PriorYear3table = driver.FindElement(By.XPath("//*[@id='tabid01']/div[10]/table/tbody"));
                    //IList<IWebElement> PriorYear3row = PriorYear3table.FindElements(By.TagName("tr"));
                    //IList<IWebElement> PriorYear3id;
                    //foreach (IWebElement PriorYear3 in PriorYear3row)
                    //{
                    //    PriorYear3id = PriorYear3.FindElements(By.TagName("td"));
                    //    if (!PriorYear3.Text.Contains("Installment"))
                    //    {
                    //        string PriorYear3Result = prioryear3tax + "~" + PriorYear3id[0].Text + "~" + "" + "~" + PriorYear3id[1].Text + "~" + "" + "~" + "" + "~" + PriorYear3id[2].Text;
                    //        gc.insert_date(orderNumber, Parcel_number, 1671, PriorYear3Result, 1, DateTime.Now);
                    //    }
                    //}
                    //TaxDistribution
                    string TaxDistributionResult = "", Heading = "";
                    driver.FindElement(By.LinkText("Property Tax Distribution")).Click();
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, Parcel_number, "Property Tax Distribution", driver, "IL", "Dupage");
                    //                                //*[@id="ctl00_pageContent_ctl00_gvList4"]/tbody
                    IWebElement TaxDistributiontable = driver.FindElement(By.Id("ctl00_pageContent_ctl00_gvList4"));
                    IList<IWebElement> TaxDistributionrow = TaxDistributiontable.FindElements(By.TagName("tr"));
                    IList<IWebElement> TaxDistributionid;
                    IList<IWebElement> TaxDistributionth;
                    foreach (IWebElement TaxDistribution in TaxDistributionrow)
                    {
                        TaxDistributionid = TaxDistribution.FindElements(By.TagName("td"));
                        TaxDistributionth = TaxDistribution.FindElements(By.TagName("th"));
                        if (!TaxDistribution.Text.Contains("Tax Body"))
                        {
                            TaxDistributionResult = TaxDistributionid[0].Text + "~" + TaxDistributionid[1].Text + "~" + TaxDistributionid[2].Text + "~" + TaxDistributionid[3].Text;
                            gc.insert_date(orderNumber, Parcel_number, 1672, TaxDistributionResult, 1, DateTime.Now);
                        }
                        if (TaxDistribution.Text.Contains("Tax Body"))
                        {
                            Heading = TaxDistributionth[0].Text + "~" + TaxDistributionth[1].Text + "~" + TaxDistributionth[2].Text + "~" + TaxDistributionth[3].Text;
                            db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + Heading + "' where Id = '" + 1672 + "'");
                        }
                    }
                    //TaxHistory
                    driver.FindElement(By.LinkText("Property Tax History")).Click();
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, Parcel_number, "Property Tax History", driver, "IL", "Dupage");
                    IWebElement TaxHistorytable = driver.FindElement(By.Id("ctl00_pageContent_ctl00_gvList5"));
                    IList<IWebElement> TaxHistoryrow = TaxHistorytable.FindElements(By.TagName("tr"));
                    IList<IWebElement> TaxHistoryid;
                    IList<IWebElement> TaxHistoryth;
                    foreach (IWebElement TaxHistory in TaxHistoryrow)
                    {
                        TaxHistoryid = TaxHistory.FindElements(By.TagName("td"));
                        TaxHistoryth = TaxHistory.FindElements(By.TagName("th"));
                        if (TaxHistoryth.Count==0)
                        {
                            string Taxhistoryresult = TaxHistoryid[0].Text + "~" + TaxHistoryid[1].Text + "~" + TaxHistoryid[2].Text + "~" + TaxHistoryid[3].Text + "~" + TaxHistoryid[4].Text;
                            gc.insert_date(orderNumber, Parcel_number, 1673, Taxhistoryresult, 1, DateTime.Now);
                        }
                        if (TaxHistoryid.Count ==0)
                        {
                            string TaxhistoryHead = "Tax History" + "~" + TaxHistoryth[1].Text + "~" + TaxHistoryth[2].Text + "~" + TaxHistoryth[3].Text + "~" + TaxHistoryth[4].Text;
                            db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + TaxhistoryHead + "' where Id = '" + 1673 + "'");
                        }
                    }
                    driver.FindElement(By.LinkText("Assessment Information")).Click();
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, Parcel_number, "Assessment Detail", driver, "IL", "Dupage");
                    //Assessment 1
                    //Wayne
                    if (firstsplit == "01")
                    {
                        driver.Navigate().GoToUrl("http://www.waynetownshipassessor.com/public/parcel_search.aspx");
                        driver.FindElement(By.Id("ctl00_BC_lbSearchPin")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, Parcel_number, "Pin" + firstsplit, driver, "IL", "Dupage");
                        driver.FindElement(By.Id("ctl00_BC_txP2")).SendKeys(secondsplit);
                        driver.FindElement(By.Id("ctl00_BC_txP3")).SendKeys(threesplit);
                        driver.FindElement(By.Id("ctl00_BC_txP4")).SendKeys(foursplit);
                        gc.CreatePdf(orderNumber, Parcel_number, "Pin Split" + firstsplit, driver, "IL", "Dupage");
                        driver.FindElement(By.Id("ctl00_BC_btnSearchPin")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, Parcel_number, "Pin Split After" + firstsplit, driver, "IL", "Dupage");
                        string propertyparcel = driver.FindElement(By.Id("ctl00_BC_lbPIN")).Text;
                        string Addresspro1 = driver.FindElement(By.Id("ctl00_BC_lbAddress")).Text;
                        string Addresspro2 = driver.FindElement(By.Id("ctl00_BC_lbCityStateZip")).Text;
                        string Addresspro = Addresspro1 + " " + Addresspro2;
                        string YearBuilt = driver.FindElement(By.Id("ctl00_BC_lbYearBuilt")).Text;
                        string Assmentyear = driver.FindElement(By.Id("ctl00_BC_lbASMYear")).Text;
                        //string yearass = GlobalClass.Before(Assmentyear, "Assmentyear");
                        string Propertyresult1 = propertyparcel + "~" + Addresspro + "~" + YearBuilt + "~" + Assmentyear;
                        //1
                        string Propertyhead = "Parcel Number" + "~" + "Property Address" + "~" + "Year Built" + "~" + "Assessment";
                        db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + Propertyhead + "' where Id = '" + 1683 + "'");
                        gc.insert_date(orderNumber, Parcel_number, 1683, Propertyresult1, 1, DateTime.Now);
                        string Land = driver.FindElement(By.Id("ctl00_BC_lbASMLand")).Text;
                        string Building = driver.FindElement(By.Id("ctl00_BC_lbASMBuilding")).Text;
                        string Total = driver.FindElement(By.Id("ctl00_BC_lbASMTotal")).Text;
                        string BuildingArea = driver.FindElement(By.Id("ctl00_BC_lbBuildArea")).Text;
                        string Assesment1 = Land + "~" + Building + "~" + Total + "~" + BuildingArea;
                        string Headingass = "Land" + "~" + "Building" + "~" + "Total" + "~" + "Building Area";
                        //2
                        db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + Headingass + "' where Id = '" + 1690 + "'");
                        gc.insert_date(orderNumber, Parcel_number, 1690, Assesment1, 1, DateTime.Now);
                    }
                    //Bloomingdale
                    if (firstsplit == "02")
                    {
                        driver.Navigate().GoToUrl("http://www.bloomingdaletownshipassessor.com/SD/BT/AssessorDB/Search.aspx");
                        driver.FindElement(By.Id("contentPageContent_txtParcelNumber")).SendKeys(Parcel_number);
                        gc.CreatePdf(orderNumber, Parcel_number, "Pin" + firstsplit, driver, "IL", "Dupage");
                        driver.FindElement(By.Id("contentPageContent_cmdSearch")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, Parcel_number, "Pin After" + firstsplit, driver, "IL", "Dupage");
                        string Assessparcel = driver.FindElement(By.Id("lblParcelNumberValue")).Text;
                        string proaddress = driver.FindElement(By.Id("lblAddressValue")).Text;
                        string proyearbuilt = driver.FindElement(By.Id("contentPageContent_lblYearBuiltValue")).Text;
                        string Subdevision = driver.FindElement(By.Id("contentPageContent_lblSubdivisionValue")).Text;
                        string Useparcel = driver.FindElement(By.Id("contentPageContent_lblPropertyUseValue")).Text;
                        string otherparcel = driver.FindElement(By.Id("contentPageContent_lblRelatedParcelsValue")).Text;
                        string propertyresult = Assessparcel + "~" + proaddress + "~" + proyearbuilt + "~" + Subdevision + "~" + Useparcel + "~" + otherparcel;
                        string Propertyhead = "pin Number" + "~" + "Property Address" + "~" + "Year Built" + "~" + "Subdivision" + "~" + "Parcel Use" + "~" + "Use with Other Parcels";
                        //1
                        db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + Propertyhead + "' where Id = '" + 1683 + "'");
                        gc.insert_date(orderNumber, Parcel_number, 1683, propertyresult, 1, DateTime.Now);
                        IWebElement Taxbillink = driver.FindElement(By.Id("menuSiteNav_m4"));
                        string Taxlink = Taxbillink.GetAttribute("href");
                        driver.Navigate().GoToUrl(Taxlink);
                        gc.CreatePdf(orderNumber, Parcel_number, "Tax Bill" + firstsplit, driver, "IL", "Dupage");
                        string AssessmentHead = "", Row1 = "", Row2 = "", Row3 = "", Head1 = "", Head2 = "", Head3 = "";
                        //IWebElement Assessmenttable1 = driver.FindElement(By.Id("contentPageContent_taxbillrowheaders"));
                        //IList<IWebElement> Assessmentrow1 = Assessmenttable1.FindElements(By.TagName("tr"));
                        //IList<IWebElement> Assessmentid;
                        //IList<IWebElement> Assessmentth;
                        //foreach (IWebElement Assessment in Assessmentrow1)
                        //{
                        //    Assessmentid = Assessment.FindElements(By.TagName("td"));
                        //    Assessmentth = Assessment.FindElements(By.TagName("th"));
                        //    if (!Assessment.Text.Contains("ASSESSMENT YEAR") && !Assessment.Text.Contains("TAXES PAYABLE YEAR"))
                        //    {
                        //        AssessmentHead += Assessmentid[0].Text + "~";
                        //    }
                        //}
                        IWebElement Assessmenttable2 = driver.FindElement(By.Id("contentPageContent_TaxBillDataGridView"));
                        IList<IWebElement> Assessmentrow2 = Assessmenttable2.FindElements(By.TagName("tr"));
                        IList<IWebElement> Assessmentid2;
                        IList<IWebElement> Assessmentth2;
                        foreach (IWebElement Assessment2 in Assessmentrow2)
                        {
                            Assessmentid2 = Assessment2.FindElements(By.TagName("td"));
                            Assessmentth2 = Assessment2.FindElements(By.TagName("th"));
                            if (Assessmentid2.Count != 0 && Assessmentid2[0].Text.Trim() != "")
                            {
                                Row1 += Assessmentid2[0].Text + "~";
                                Row2 += Assessmentid2[1].Text + "~";
                                Row3 += Assessmentid2[2].Text + "~";
                            }
                            if (Assessmentth2.Count != 0)
                            {
                                Head1 = Assessmentth2[0].Text;
                                Head2 = Assessmentth2[1].Text;
                                Head3 = Assessmentth2[2].Text;
                            }
                        }
                        string AssessmentHead1 = "Assessment Year" + "~" + "Assessor Value - Total" + "~" + "Times County Equalizer1" + "~" + "Total Assessed Value -LAND" + "~" + "BUILDING1" + "~" + "TOTAL1" + "~" + "Less HIE Exemption" + "~" + "Equalized Value - LAND" + "~" + "BUILDING2" + "~" + "TOTAL2" + "~" +
                        "Board Of Review/Assessor Value - LAND" + "~" + "BUILDING3" + "~" + "TOTAL3" + "~" + "Times State Equalizer" + "~" + "State Equalized Value - LAND" + "~" + "BUILDING4" + "~" + "TOTAL4" + "~" + "Less Exemptions(Excluding HIE)" + "~" + "Final Billing Valuation" + "~" + "Tax Code" + "~" + "Local Tax Rate" + "~" + "Property Tax Amount" + "~" + "TAXES PAYABLE YEAR ";
                        db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + AssessmentHead1 + "' where Id = '" + 1690 + "'");
                        string Row01 = Head1 + "~" + Row1.Remove(Row1.Length - 1);
                        string Row02 = Head2 + "~" + Row2.Remove(Row2.Length - 1);
                        string Row03 = Head3 + "~" + Row3.Remove(Row3.Length - 1);
                        gc.insert_date(orderNumber, Parcel_number, 1690, Row01, 1, DateTime.Now);
                        gc.insert_date(orderNumber, Parcel_number, 1690, Row02, 1, DateTime.Now);
                        gc.insert_date(orderNumber, Parcel_number, 1690, Row03, 1, DateTime.Now);
                    }
                    //Addison Township
                    if (firstsplit == "03")
                    {
                        driver.Navigate().GoToUrl("http://www.addisontownship.com/webdb/sd/addison/assessordb/search.aspx");
                        driver.FindElement(By.Id("contentPageContent_txtParcelNumber")).SendKeys(Parcel_number);
                        gc.CreatePdf(orderNumber, Parcel_number, "Pin" + firstsplit, driver, "IL", "Dupage");
                        driver.FindElement(By.Id("contentPageContent_cmdSearch")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, Parcel_number, "Pin After" + firstsplit, driver, "IL", "Dupage");
                        string Parcelpro = driver.FindElement(By.Id("contentPageContent_lblParcelNumberValue")).Text;
                        string ProAddress = driver.FindElement(By.Id("contentPageContent_lblAddressValue")).Text;
                        string City = driver.FindElement(By.Id("contentPageContent_lblCityValue")).Text;
                        string Neighborhood = driver.FindElement(By.Id("contentPageContent_lblNBHDCodeValue")).Text;
                        string Proyearbuilt = driver.FindElement(By.Id("contentPageContent_lblYearBuiltValue")).Text;
                        string propertyresult = Parcelpro + "~" + ProAddress + "~" + City + "~" + Neighborhood + "~" + Proyearbuilt;
                        string Propertyhead = "Parcel ID" + "~" + "Property Address" + "~" + "City" + "~" + "Neighborhood" + "~" + "Year Built";
                        db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + Propertyhead + "' where Id = '" + 1683 + "'");
                        gc.insert_date(orderNumber, Parcel_number, 1683, propertyresult, 1, DateTime.Now);
                        //02
                        IWebElement Assessmenttable3 = driver.FindElement(By.Id("contentPageContent_grdAssessments"));
                        IList<IWebElement> Assessmentrow3 = Assessmenttable3.FindElements(By.TagName("tr"));
                        IList<IWebElement> Assessmentid;
                        IList<IWebElement> Assessmentth;
                        foreach (IWebElement Assessment in Assessmentrow3)
                        {
                            Assessmentid = Assessment.FindElements(By.TagName("td"));
                            Assessmentth = Assessment.FindElements(By.TagName("th"));
                            if (Assessmentth.Count != 0 & Assessment.Text.Contains("Year"))
                            {
                                string AssessmentHead = Assessmentth[0].Text + "~" + Assessmentth[1].Text + "~" + Assessmentth[2].Text + "~" + Assessmentth[3].Text + "~" + Assessmentth[4].Text + "~" + Assessmentth[5].Text + "~" + Assessmentth[6].Text + "~" + Assessmentth[7].Text + "~" + Assessmentth[8].Text;
                                db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + AssessmentHead + "' where Id = '" + 1690 + "'");
                            }
                            if (Assessmentid.Count != 0 & !Assessment.Text.Contains("Year"))
                            {
                                string Assessmentres = Assessmentid[0].Text + "~" + Assessmentid[1].Text + "~" + Assessmentid[2].Text + "~" + Assessmentid[3].Text + "~" + Assessmentid[4].Text + "~" + Assessmentid[5].Text + "~" + Assessmentid[6].Text + "~" + Assessmentid[7].Text + "~" + Assessmentid[8].Text;
                                gc.insert_date(orderNumber, Parcel_number, 1690, Assessmentres, 1, DateTime.Now);
                            }

                        }
                    }
                    //Winfield Township
                    if (firstsplit == "04")
                    {
                        driver.Navigate().GoToUrl("http://www.winfieldtownship.com/ParcelSearch/SD/Winfield/AssessorDB/SearchStop.aspx");
                        driver.FindElement(By.Id("contentPageContent_txtParcelNumber")).SendKeys(Parcel_number);
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, Parcel_number, "Pin" + firstsplit, driver, "IL", "Dupage");
                        driver.FindElement(By.Id("contentPageContent_cmdSearch")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, Parcel_number, "Pin After" + firstsplit, driver, "IL", "Dupage");
                        string ProParcel = driver.FindElement(By.Id("contentPageContent_lblParcelNumberValue")).Text;
                        string Proaddress = driver.FindElement(By.Id("contentPageContent_lblAddressValue")).Text;
                        string city = driver.FindElement(By.Id("contentPageContent_lblCityValue")).Text;
                        string Nbh = driver.FindElement(By.Id("contentPageContent_lblNBHDCodeValue")).Text;
                        string Propertyclass = driver.FindElement(By.Id("contentPageContent_lblPropertyClassValue")).Text;
                        string subdivision = driver.FindElement(By.Id("contentPageContent_lblSubdivisionValue")).Text;
                        string Yearbuilt = driver.FindElement(By.Id("contentPageContent_lblYearBuiltValue")).Text;
                        string propertyresult = ProParcel + "~" + Proaddress + "~" + city + "~" + Nbh + "~" + Propertyclass + "~" + subdivision + "~" + Yearbuilt;
                        string Propertyhead = "Parcel Number" + "~" + "Property Address" + "~" + "City" + "~" + "Neighborhood" + "~" + "Property Class" + "~" + "Subdivision" + "~" + "Year Built";
                        db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + Propertyhead + "' where Id = '" + 1683 + "'");
                        gc.insert_date(orderNumber, Parcel_number, 1683, propertyresult, 1, DateTime.Now);
                        //02
                        IWebElement Assessmenttable3 = driver.FindElement(By.Id("contentPageContent_GridView1"));
                        IList<IWebElement> Assessmentrow3 = Assessmenttable3.FindElements(By.TagName("tr"));
                        IList<IWebElement> Assessmentid;
                        IList<IWebElement> Assessmentth;
                        foreach (IWebElement Assessment in Assessmentrow3)
                        {
                            Assessmentid = Assessment.FindElements(By.TagName("td"));
                            Assessmentth = Assessment.FindElements(By.TagName("th"));
                            if (Assessmentth.Count != 0 & Assessment.Text.Contains("Year"))
                            {
                                string AssessmentHead = Assessmentth[0].Text + "~" + Assessmentth[1].Text + "~" + Assessmentth[2].Text + "~" + Assessmentth[3].Text + "~" + Assessmentth[4].Text + "~" + Assessmentth[5].Text + "~" + Assessmentth[6].Text;
                                db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + AssessmentHead + "' where Id = '" + 1690 + "'");
                            }
                            if (Assessmentid.Count != 0 & !Assessment.Text.Contains("Year"))
                            {
                                string Assessmentres = Assessmentid[0].Text + "~" + Assessmentid[1].Text + "~" + Assessmentid[2].Text + "~" + Assessmentid[3].Text + "~" + Assessmentid[4].Text + "~" + Assessmentid[5].Text + "~" + Assessmentid[6].Text;
                                gc.insert_date(orderNumber, Parcel_number, 1690, Assessmentres, 1, DateTime.Now);
                            }

                        }
                    }
                    //05 - Milton Township
                    if (firstsplit == "05")
                    {
                        string subdivision = "";
                        driver.Navigate().GoToUrl("http://www.miltontownshipassessor.com/parcelsearch/sd/milton/assessordb/search.aspx");
                        driver.FindElement(By.Id("contentPageContent_txtParcelNumber")).SendKeys(Parcel_number);
                        gc.CreatePdf(orderNumber, Parcel_number, "Pin" + firstsplit, driver, "IL", "Dupage");
                        driver.FindElement(By.Id("contentPageContent_cmdSearch")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, Parcel_number, "Pin After" + firstsplit, driver, "IL", "Dupage");
                        string ProParcel = driver.FindElement(By.Id("contentPageContent_lblParcelNumberValue")).Text;
                        string Proaddress = driver.FindElement(By.Id("contentPageContent_lblAddressValue")).Text;
                        string city = driver.FindElement(By.Id("contentPageContent_lblCityValue")).Text;
                        string Nbh = driver.FindElement(By.Id("contentPageContent_lblNBHDCodeValue")).Text;
                        string Propertyclass = driver.FindElement(By.Id("contentPageContent_lblPropertyClassValue")).Text;
                        try
                        {
                            subdivision = driver.FindElement(By.Id("contentPageContent_lblSubdivisionValue")).Text;
                        }
                        catch { }
                        string Yearbuilt = driver.FindElement(By.Id("contentPageContent_lblYearBuiltValue")).Text;
                        string propertyresult = ProParcel + "~" + Proaddress + "~" + city + "~" + Nbh + "~" + Propertyclass + "~" + subdivision + "~" + Yearbuilt;
                        string Propertyhead = "Parcel Number" + "~" + "Property Address" + "~" + "City" + "~" + "Neighborhood" + "~" + "Property Class" + "~" + "Subdivision" + "~" + "Year Built";
                        db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + Propertyhead + "' where Id = '" + 1683 + "'");
                        gc.insert_date(orderNumber, Parcel_number, 1683, propertyresult, 1, DateTime.Now);
                        //02
                        IWebElement Assessmenttable3 = driver.FindElement(By.Id("contentPageContent_GridView1"));
                        IList<IWebElement> Assessmentrow3 = Assessmenttable3.FindElements(By.TagName("tr"));
                        IList<IWebElement> Assessmentid;
                        IList<IWebElement> Assessmentth;
                        foreach (IWebElement Assessment in Assessmentrow3)
                        {
                            Assessmentid = Assessment.FindElements(By.TagName("td"));
                            Assessmentth = Assessment.FindElements(By.TagName("th"));
                            if (Assessmentth.Count != 0 & Assessment.Text.Contains("Year"))
                            {
                                string AssessmentHead = Assessmentth[0].Text + "~" + Assessmentth[1].Text + "~" + Assessmentth[2].Text + "~" + Assessmentth[3].Text + "~" + Assessmentth[4].Text + "~" + Assessmentth[5].Text + "~" + Assessmentth[6].Text + "~" + Assessmentth[7].Text;
                                db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + AssessmentHead + "' where Id = '" + 1690 + "'");
                            }
                            if (Assessmentid.Count != 0 & !Assessment.Text.Contains("Year"))
                            {
                                string Assessmentres = Assessmentid[0].Text + "~" + Assessmentid[1].Text + "~" + Assessmentid[2].Text + "~" + Assessmentid[3].Text + "~" + Assessmentid[4].Text + "~" + Assessmentid[5].Text + "~" + Assessmentid[6].Text + "~" + Assessmentid[7].Text;
                                gc.insert_date(orderNumber, Parcel_number, 1690, Assessmentres, 1, DateTime.Now);
                            }

                        }
                    }
                    //York Township
                    if (firstsplit == "06")
                    {
                        driver.Navigate().GoToUrl("http://www.yorkassessor.com/York/ParcelSearch/SD/York/AssessorDB/Search.aspx");
                        driver.FindElement(By.Id("contentPageContent_txtParcelNumber")).SendKeys(Parcel_number);
                        gc.CreatePdf(orderNumber, Parcel_number, "Pin" + firstsplit, driver, "IL", "Dupage");
                        driver.FindElement(By.Id("contentPageContent_cmdSearch")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, Parcel_number, "Pin After" + firstsplit, driver, "IL", "Dupage");
                        string ProParcel = driver.FindElement(By.Id("contentPageContent_lblParcelNumberValue")).Text;
                        string Proaddress = driver.FindElement(By.Id("contentPageContent_lblAddressValue")).Text;
                        string city = driver.FindElement(By.Id("contentPageContent_lblCityValue")).Text;
                        string Nbh = driver.FindElement(By.Id("contentPageContent_lblNBHDCodeValue")).Text;
                        //string Propertyclass = driver.FindElement(By.Id("contentPageContent_lblPropertyClassValue")).Text;
                        // string subdivision = driver.FindElement(By.Id("contentPageContent_lblSubdivisionValue")).Text;
                        string Yearbuilt = driver.FindElement(By.Id("contentPageContent_lblYearBuiltValue")).Text;
                        string propertyresult = ProParcel + "~" + Proaddress + "~" + city + "~" + Nbh + "~" + Yearbuilt;
                        string Propertyhead = "Parcel Number" + "~" + "Property Address" + "~" + "City" + "~" + "Neighborhood" + "~" + "Year Built";
                        db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + Propertyhead + "' where Id = '" + 1683 + "'");
                        gc.insert_date(orderNumber, Parcel_number, 1683, propertyresult, 1, DateTime.Now);
                        //02
                        IWebElement Assessmenttable3 = driver.FindElement(By.Id("contentPageContent_GridView1"));
                        IList<IWebElement> Assessmentrow3 = Assessmenttable3.FindElements(By.TagName("tr"));
                        IList<IWebElement> Assessmentid;
                        IList<IWebElement> Assessmentth;
                        foreach (IWebElement Assessment in Assessmentrow3)
                        {
                            Assessmentid = Assessment.FindElements(By.TagName("td"));
                            Assessmentth = Assessment.FindElements(By.TagName("th"));
                            if (Assessmentth.Count != 0 & Assessment.Text.Contains("Year"))
                            {
                                string AssessmentHead = Assessmentth[0].Text + "~" + Assessmentth[1].Text + "~" + Assessmentth[2].Text + "~" + Assessmentth[3].Text + "~" + Assessmentth[4].Text + "~" + Assessmentth[5].Text + "~" + Assessmentth[6].Text;
                                db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + AssessmentHead + "' where Id = '" + 1690 + "'");
                            }
                            if (Assessmentid.Count != 0 & !Assessment.Text.Contains("Year"))
                            {
                                string Assessmentres = Assessmentid[0].Text + "~" + Assessmentid[1].Text + "~" + Assessmentid[2].Text + "~" + Assessmentid[3].Text + "~" + Assessmentid[4].Text + "~" + Assessmentid[5].Text + "~" + Assessmentid[6].Text;
                                gc.insert_date(orderNumber, Parcel_number, 1690, Assessmentres, 1, DateTime.Now);
                            }
                        }
                    }
                    //07 - Naperville Township
                    if (firstsplit == "07")
                    {
                        driver.Navigate().GoToUrl("http://www.napervilletownship.com/SD/Naperville/assessordb/search.aspx");
                        driver.FindElement(By.Id("contentPageContent_txtParcelNumber")).SendKeys(Parcel_number);
                        gc.CreatePdf(orderNumber, Parcel_number, "Pin" + firstsplit, driver, "IL", "Dupage");
                        driver.FindElement(By.Id("contentPageContent_cmdSearch")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, Parcel_number, "Pin After" + firstsplit, driver, "IL", "Dupage");
                        string ProParcel = driver.FindElement(By.Id("contentPageContent_lblParcelNumberValue")).Text;
                        string Proaddress = driver.FindElement(By.Id("contentPageContent_lblAddressValue")).Text;
                        string city = driver.FindElement(By.Id("contentPageContent_lblCityValue")).Text;
                        string Nbh = driver.FindElement(By.Id("contentPageContent_lblNBHDCodeValue")).Text;
                        string Propertyclass = driver.FindElement(By.Id("contentPageContent_lblPropertyClassValue")).Text;
                        string subdivision = driver.FindElement(By.Id("contentPageContent_lblSubdivisionValue")).Text;
                        string Yearbuilt = driver.FindElement(By.Id("contentPageContent_lblYearBuiltValue")).Text;
                        string propertyresult = ProParcel + "~" + Proaddress + "~" + city + "~" + Nbh + "~" + Propertyclass + "~" + subdivision + "~" + Yearbuilt;
                        string Propertyhead = "Parcel Number" + "~" + "Property Address" + "~" + "City" + "~" + "Neighborhood" + "~" + "Property Class" + "~" + "Subdivision" + "~" + "Year Built";
                        db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + Propertyhead + "' where Id = '" + 1683 + "'");
                        gc.insert_date(orderNumber, Parcel_number, 1683, propertyresult, 1, DateTime.Now);
                        //02
                        IWebElement Assessmenttable3 = driver.FindElement(By.Id("contentPageContent_GridView1"));
                        IList<IWebElement> Assessmentrow3 = Assessmenttable3.FindElements(By.TagName("tr"));
                        IList<IWebElement> Assessmentid;
                        IList<IWebElement> Assessmentth;
                        foreach (IWebElement Assessment in Assessmentrow3)
                        {
                            Assessmentid = Assessment.FindElements(By.TagName("td"));
                            Assessmentth = Assessment.FindElements(By.TagName("th"));
                            if (Assessmentth.Count != 0 & Assessment.Text.Contains("Year"))
                            {
                                string AssessmentHead = Assessmentth[0].Text + "~" + Assessmentth[1].Text + "~" + Assessmentth[2].Text + "~" + Assessmentth[3].Text + "~" + Assessmentth[4].Text + "~" + Assessmentth[5].Text + "~" + Assessmentth[6].Text;
                                db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + AssessmentHead + "' where Id = '" + 1690 + "'");
                            }
                            if (Assessmentid.Count != 0 & !Assessment.Text.Contains("Year"))
                            {
                                string Assessmentres = Assessmentid[0].Text + "~" + Assessmentid[1].Text + "~" + Assessmentid[2].Text + "~" + Assessmentid[3].Text + "~" + Assessmentid[4].Text + "~" + Assessmentid[5].Text + "~" + Assessmentid[6].Text;
                                gc.insert_date(orderNumber, Parcel_number, 1690, Assessmentres, 1, DateTime.Now);
                            }
                        }
                    }
                    //08 - Lisle Township
                    if (firstsplit == "08")
                    {
                        driver.Navigate().GoToUrl("http://www.lisletownshipassessor.com/sd/lisle/assessordb/Search.aspx");
                        driver.FindElement(By.Id("ctl00_contentPageContent_txtParcelNumber")).SendKeys(Parcel_number);
                        gc.CreatePdf(orderNumber, Parcel_number, "Pin" + firstsplit, driver, "IL", "Dupage");
                        driver.FindElement(By.Id("ctl00_contentPageContent_cmdSearch")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, Parcel_number, "Pin After" + firstsplit, driver, "IL", "Dupage");
                        string ProParcel = driver.FindElement(By.Id("ctl00_contentPageContent_lblParcelNumberValue")).Text;
                        string Proaddress = driver.FindElement(By.Id("ctl00_contentPageContent_lblAddressValue")).Text;
                        string city = driver.FindElement(By.Id("ctl00_contentPageContent_lblCityValue")).Text;
                        //string Nbh = driver.FindElement(By.Id("contentPageContent_lblNBHDCodeValue")).Text;
                        //string Propertyclass = driver.FindElement(By.Id("contentPageContent_lblPropertyClassValue")).Text;
                        // string subdivision = driver.FindElement(By.Id("contentPageContent_lblSubdivisionValue")).Text;
                        string Yearbuilt = driver.FindElement(By.Id("ctl00_contentPageContent_lblYearBuiltValue")).Text;
                        string propertyresult = ProParcel + "~" + Proaddress + "~" + city + "~" + Yearbuilt;
                        string Propertyhead = "Parcel Number" + "~" + "Property Address" + "~" + "City" + "~" + "Year Built";
                        db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + Propertyhead + "' where Id = '" + 1683 + "'");
                        gc.insert_date(orderNumber, Parcel_number, 1683, propertyresult, 1, DateTime.Now);
                        //02
                        IWebElement Assessmenttable3 = driver.FindElement(By.Id("ctl00_contentPageContent_grdAssessmentInformation"));
                        IList<IWebElement> Assessmentrow3 = Assessmenttable3.FindElements(By.TagName("tr"));
                        IList<IWebElement> Assessmentid;
                        IList<IWebElement> Assessmentth;
                        foreach (IWebElement Assessment in Assessmentrow3)
                        {
                            Assessmentid = Assessment.FindElements(By.TagName("td"));
                            Assessmentth = Assessment.FindElements(By.TagName("th"));
                            if (Assessmentth.Count != 0 & Assessment.Text.Contains("Year"))
                            {
                                string AssessmentHead = Assessmentth[0].Text + "~" + Assessmentth[1].Text + "~" + Assessmentth[2].Text + "~" + Assessmentth[3].Text;
                                db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + AssessmentHead + "' where Id = '" + 1690 + "'");
                            }
                            if (Assessmentid.Count != 0 & !Assessment.Text.Contains("Year"))
                            {
                                string Assessmentres = Assessmentid[0].Text + "~" + Assessmentid[1].Text + "~" + Assessmentid[2].Text + "~" + Assessmentid[3].Text;
                                gc.insert_date(orderNumber, Parcel_number, 1690, Assessmentres, 1, DateTime.Now);
                            }
                        }
                    }
                    //09 & 10 - Downers Grove Township
                    if (firstsplit == "09" || firstsplit == "10")
                    {
                        driver.Navigate().GoToUrl("http://www.dgtownship.com/search.php");
                        driver.FindElement(By.XPath("/html/body/form/table/tbody/tr[2]/td/input[1]")).Click();
                        gc.CreatePdf(orderNumber, Parcel_number, "Pin" + firstsplit, driver, "IL", "Dupage");
                        driver.FindElement(By.Name("resParcel")).SendKeys(Parcel_number);
                        gc.CreatePdf(orderNumber, Parcel_number, "Pin1" + firstsplit, driver, "IL", "Dupage");
                        try
                        {
                            driver.FindElement(By.XPath("//*[@id='residentialDiv']/table/tbody/tr[2]/td/table/tbody/tr[3]/td/input")).Click();
                            Thread.Sleep(2000);
                            gc.CreatePdf(orderNumber, Parcel_number, "Pin After" + firstsplit, driver, "IL", "Dupage");
                        }
                        catch { }
                        try
                        {
                            string Proparcel = driver.FindElement(By.XPath("/html/body/table/tbody/tr[6]/td[3]")).Text;
                            string PRoaddress = driver.FindElement(By.XPath("/html/body/table/tbody/tr[6]/td[5]")).Text;
                            string Neighborhood = driver.FindElement(By.XPath("/html/body/table/tbody/tr[6]/td[4]")).Text;
                            string YearBuilt = driver.FindElement(By.XPath("/html/body/table/tbody/tr[6]/td[22]")).Text;
                            string propertyresult = Proparcel + "~" + PRoaddress + "~" + Neighborhood + "~" + YearBuilt;
                            string Propertyhead = "Parcel ID" + "~" + "Property Address" + "~" + "Neighborhood" + "~" + "Year Built";
                            //01
                            db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + Propertyhead + "' where Id = '" + 1683 + "'");
                            gc.insert_date(orderNumber, Parcel_number, 1683, propertyresult, 1, DateTime.Now);
                        }
                        catch { }
                        try
                        {
                            string Land = driver.FindElement(By.XPath("/html/body/table/tbody/tr[6]/td[6]")).Text;
                            string Building = driver.FindElement(By.XPath("/html/body/table/tbody/tr[6]/td[7]")).Text;
                            string TotalAssessed = driver.FindElement(By.XPath("/html/body/table/tbody/tr[6]/td[8]")).Text;
                            string AssessmentHead = "Land Value" + "~" + "Building Value" + "~" + "Total Assessed Value";
                            db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + AssessmentHead + "' where Id = '" + 1690 + "'");
                            string Assessmentres = Land + "~" + Building + "~" + TotalAssessed;
                            gc.insert_date(orderNumber, Parcel_number, 1690, Assessmentres, 1, DateTime.Now);
                        }
                        catch { }
                    }
                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "IL", "Dupage", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);
                    driver.Quit();
                    gc.mergpdf(orderNumber, "IL", "Dupage");
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
