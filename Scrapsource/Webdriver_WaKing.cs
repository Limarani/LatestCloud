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
using Org.BouncyCastle.Utilities;
namespace ScrapMaricopa.Scrapsource
{
    public class Webdriver_WaKing
    {
        string Parcel_no = "", ownername = "", situs_address = "", Legal = "", year_built = "", taxaccno = "", taxpaynam = "", taxyer = "", payststus = "",
            taxauthority = "", curtax2 = "", Addresss = "", Taxpayer_Name = "", Building_Number = "", Unit_Number = "",
            Property_Name = "", trocher = "", StateSchool_PartOne = "", StateSchool_Two = "", LocalSchool = "", County = "", City = "", Road = "", Port = "",
            SoundTransit = "", Hospital = "", Flood = "", Library = "", EMS = "", Other = "", FeesAnd_Charges = "";
        int Mcount = 0, WCount = 0; string value = "";
        IWebDriver driver;
        IWebElement multy;
        DBconnection db = new DBconnection();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        GlobalClass gc = new GlobalClass();
        string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
        public string FTP_King(string streetno, string streettype, string streetname, string unitnumber, string ownernm, string parcelNumber, string searchType, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            var option = new ChromeOptions();
            option.AddArgument("No-Sandbox");
            //var driverService = PhantomJSDriverService.CreateDefaultService();
            //driverService.HideCommandPromptWindow = true;
            //driver = new ChromeDriver();
            //driver = new PhantomJSDriver();
            using (driver = new ChromeDriver(option))
            {
               
                if (directParcel.ToUpper() == "NORTH")
                {
                    directParcel = "N";
                }
                if (directParcel.ToUpper() == "NORTH EAST")
                {
                    directParcel = "NE";
                }
                if (directParcel.ToUpper() == "SOUTH EAST")
                {
                    directParcel = "SE";
                }
                string Address = "";
                if (directParcel != "")
                {
                    Address = streetno + " " + directParcel + " " + streetname + " " + streettype;
                }
                else
                {
                    Address = streetno + " " + streetname + " " + streettype;
                }
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");

                    if (searchType == "titleflex")
                    {
                        string straddress = "";
                        if (directParcel != "")
                        {
                            straddress = streetno + " " + directParcel + " " + streetname + " " + streettype + " " + unitnumber;
                        }
                        else
                        {
                            straddress = streetno + " " + streetname + " " + streettype + " " + unitnumber;
                        }


                        gc.TitleFlexSearch(orderNumber, "", ownernm, straddress, "WA", "King");
                        if (HttpContext.Current.Session["TitleFlex_Search"] != null)
                        {
                            string result = HttpContext.Current.Session["TitleFlex_Search"].ToString();
                        }

                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_WAKing"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }
                    if (searchType == "address")
                    {
                        Start(orderNumber);
                        gc.CreatePdf_WOP(orderNumber, "AddressSearch before", driver, "WA", "King");
                        Thread.Sleep(8000);
                        driver.FindElement(By.Id("cphContent_txtAddress")).SendKeys(Address);
                        gc.CreatePdf_WOP(orderNumber, "AddressSearch", driver, "WA", "King");
                        driver.FindElement(By.Id("cphContent_btn_SearchAddress")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        try
                        {     try
                            {                                       //  cphContent_GridViewParcelList
                                multy = driver.FindElement(By.Id("kingcounty_gov_cphContent_GridViewParcelList"));
                            }
                            catch { }
                            try
                            {
                                multy = driver.FindElement(By.Id("cphContent_GridViewParcelList"));
                            }
                            catch { }
                            IList<IWebElement> muladdress = multy.FindElements(By.TagName("tr"));
                            IList<IWebElement> mulid;
                            foreach (IWebElement addressrow in muladdress)
                            {
                                mulid = addressrow.FindElements(By.TagName("td"));
                                if (mulid.Count != 0)
                                {
                                    if (Mcount <= 25)
                                    {
                                        Parcel_no = mulid[0].Text;
                                        Addresss = mulid[1].Text;
                                        Taxpayer_Name = mulid[2].Text;
                                        Building_Number = mulid[3].Text;
                                        Unit_Number = mulid[4].Text;
                                        string MultyInst = "" + "~" + Addresss + "~" + Taxpayer_Name + "~" + Building_Number + "~" + Unit_Number;
                                        gc.insert_date(orderNumber, Parcel_no, 552, MultyInst, 1, DateTime.Now);
                                    }
                                    Mcount++;
                                }

                            }
                            if (Mcount > 25)
                            {
                                HttpContext.Current.Session["multiParcel_King_Multicount"] = "Maximum";
                            }
                            else
                            {
                                HttpContext.Current.Session["multiparcel_King"] = "Yes";
                            }
                            driver.Quit();
                            return "MultiParcel";
                        }
                        catch { }

                        try
                        {
                            //No Data Found
                            string nodata = driver.FindElement(By.Id("cphContent_lblAddressMsg")).Text;
                            if (nodata.Contains("Can not find"))
                            {
                                HttpContext.Current.Session["Nodata_WAKing"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }
                    if (searchType == "parcel")
                    {
                        Start(orderNumber);
                        gc.CreatePdf(orderNumber, Parcel_no, "Aggre Click", driver, "WA", "King");
                        Thread.Sleep(2000); //cphContent_txtParcelNbr
                        driver.FindElement(By.Id("cphContent_txtParcelNbr")).SendKeys(parcelNumber);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcelsearch", driver, "WA", "King");
                        // cphContent_btn_Search
                        driver.FindElement(By.Id("cphContent_btn_Search")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        try
                        {
                            IWebElement multy = driver.FindElement(By.Id("kingcounty_gov_cphContent_GridViewComplex"));
                            IList<IWebElement> muladdress = multy.FindElements(By.TagName("tr"));
                            IList<IWebElement> mulid;
                            foreach (IWebElement addressrow in muladdress)
                            {
                                mulid = addressrow.FindElements(By.TagName("td"));
                                if (mulid.Count != 0)
                                {
                                    if (mulid[0].Text.Trim() == parcelNumber.Trim())
                                    {
                                        IWebElement parcelclick = mulid[0];
                                        parcelclick.Click();
                                        Thread.Sleep(2000);
                                        break;
                                    }
                                }
                            }
                        }
                        catch { }

                        try
                        {
                            //No Data Found
                            string nodata = driver.FindElement(By.Id("cphContent_LblMsg")).Text;
                            if (nodata.Contains("Can not find"))
                            {
                                HttpContext.Current.Session["Nodata_WAKing"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }
                    if (searchType == "Property Name")
                    {
                        Start(orderNumber);
                        gc.CreatePdf(orderNumber, Parcel_no, "Aggre Click", driver, "WA", "King");
                        driver.FindElement(By.Id("kingcounty_gov_cphContent_txtPropName")).SendKeys(streettype);
                        gc.CreatePdf(orderNumber, parcelNumber, "Owner Name search", driver, "WA", "King");
                        driver.FindElement(By.Id("kingcounty_gov_cphContent_btn_SearchPropName")).SendKeys(Keys.Enter);
                        try
                        {
                            IWebElement ownarnameTable = driver.FindElement(By.Id("kingcounty_gov_cphContent_GridViewPropName"));
                            IList<IWebElement> ownarnameRow = ownarnameTable.FindElements(By.TagName("tr"));
                            IList<IWebElement> Ownarid;
                            foreach (IWebElement ownar in ownarnameRow)
                            {
                                Ownarid = ownar.FindElements(By.TagName("td"));
                                if (Ownarid.Count != 0)
                                {
                                    if (WCount <= 25)
                                    {
                                        Parcel_no = Ownarid[0].Text;
                                        Property_Name = Ownarid[1].Text;
                                        Addresss = Ownarid[2].Text;
                                        Taxpayer_Name = Ownarid[3].Text;
                                        Building_Number = Ownarid[4].Text;
                                        Unit_Number = Ownarid[5].Text;
                                        string Mownername = Property_Name + "~" + Addresss + "~" + Taxpayer_Name + "~" + Building_Number + "~" + Unit_Number;
                                        gc.insert_date(orderNumber, Parcel_no, 552, Mownername, 1, DateTime.Now);
                                    }
                                    WCount++;
                                }
                            }
                            if (WCount > 25)
                            {
                                HttpContext.Current.Session["multiParcel_King_Multicount"] = "Maximum";
                            }
                            else
                            {
                                HttpContext.Current.Session["multiparcel_King"] = "Yes";
                            }
                            driver.Quit();
                            return "MultiParcel";
                        }

                        catch { }

                        try
                        {
                            //No Data Found
                            string nodata = driver.FindElement(By.Id("cphContent_lblPropNameMsg")).Text;
                            if (nodata.Contains("Can not find"))
                            {
                                HttpContext.Current.Session["Nodata_WAKing"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }

                    //Property Details      
                    gc.CreatePdf(orderNumber, Parcel_no, "Propety Result", driver, "WA", "King");
                    string fulltext = driver.FindElement(By.Id("cphContent_DetailsViewDashboardHeader")).Text;
                    Parcel_no = gc.Between(fulltext, "Parcel Number", "Name");
                    ownername = gc.Between(fulltext, "Name", "Site Address");
                    situs_address = gc.Between(fulltext, "Site Address", "Legal");
                    Legal = GlobalClass.After(fulltext, "Legal");
                    try
                    {
                        string buildingfulltext = driver.FindElement(By.Id("cphContent_DetailsViewPropTypeR")).Text;
                        year_built = gc.Between(buildingfulltext, "Year Built", "Total Square Footage");
                    }
                    catch { }
                    try
                    {
                        string buildingfulltext = driver.FindElement(By.Id("kingcounty_gov_cphContent_DetailsViewPropTypeK")).Text;
                        year_built = gc.Between(buildingfulltext, "Year Built", "Construction Class");
                    }
                    catch { }
                    string propdetails = ownername + "~" + situs_address + "~" + Legal + "~" + year_built;
                    gc.insert_date(orderNumber, Parcel_no, 537, propdetails, 1, DateTime.Now);


                    //Assessment Details

                    int Count = 0;
                    try
                    {
                        IWebElement taxrolltable = driver.FindElement(By.XPath("//*[@id='cphContent_GridViewDBTaxRoll']/tbody"));
                        IList<IWebElement> taxrow = taxrolltable.FindElements(By.TagName("tr"));
                        IList<IWebElement> taxrollTd;
                        foreach (IWebElement Taxrow in taxrow)
                        {
                            taxrollTd = Taxrow.FindElements(By.TagName("td"));
                            if (taxrollTd.Count != 0)
                            {
                                if (Count < 3)
                                {
                                    string taxroll = taxrollTd[0].Text + "~" + taxrollTd[1].Text + "~" + taxrollTd[2].Text + "~" + taxrollTd[3].Text + "~" + taxrollTd[4].Text + "~" + taxrollTd[5].Text + "~" + taxrollTd[6].Text + "~" + taxrollTd[7].Text;
                                    gc.insert_date(orderNumber, Parcel_no, 538, taxroll, 1, DateTime.Now);
                                }
                                Count++;
                            }
                        }
                    }
                    catch { }
                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");
                    //Tax Details
                    driver.Navigate().GoToUrl("http://info.kingcounty.gov/finance/treasury/propertytax/");
                    Thread.Sleep(2000);
                    string parcelwoh = Parcel_no.Replace("-", "");
                    //*[@id="searchParcel"] searchParcel
                    driver.FindElement(By.Id("searchParcel")).SendKeys(parcelwoh.Trim());
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, Parcel_no, "Tax Search", driver, "WA", "King");

                    //*[@id="ec-tenant-app"]/div/div[2]/div[2]/div[1]/div/form/div/span/button
                    //IWebElement Taxview = driver.FindElement(By.XPath("//*[@id='search - real - property - submi']/span"));
                    try
                    {                               //*[@id="ec-tenant-app"]/div/div[2]/div[2]/div[1]/div/form/div/span
                        driver.FindElement(By.XPath("//*[@id='ec-tenant-app']/div/div[2]/div[2]/div[1]/div/form/div/span/button")).Click();
                        Thread.Sleep(2000);
                    }
                    catch { }
                    try
                    {
                        IWebElement Taxview = driver.FindElement(By.XPath("//*[@id='ec-tenant-app']/div/div[2]/div[2]/div[1]/div/form/div/span/button"));
                        IJavaScriptExecutor js1 = driver as IJavaScriptExecutor;
                        js1.ExecuteScript("arguments[0].click();", Taxview);
                    }
                    catch { }
                    try
                    {
                        driver.FindElement(By.XPath("//*[@id='ec-tenant-app']/div/div[2]/div[2]/div[1]/div/form/div/span/button")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                    }
                    catch { }
                    //IJavaScriptExecutor js1 = driver as IJavaScriptExecutor;
                    //js1.ExecuteScript("arguments[0].click();", Taxview);
                    //Taxview.SendKeys(Keys.Enter);
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, Parcel_no, "Tax Result", driver, "WA", "King");
                    IWebElement acctorcher = driver.FindElement(By.XPath("//*[@id='parcelPanels']/div"));
                    string torcher1 = acctorcher.Text.Replace("\r\n", "");
                    trocher = gc.Between(torcher1, "Tax account number:", "Parcel number:").Trim();
                    // trocher = GlobalClass.After(torcher1, "Tax account number: ");
                    string taxaccno1 = driver.FindElement(By.XPath("//*[@id='parcelAccount" + trocher + "']/h4/a/span/span")).Text;
                    taxaccno = GlobalClass.After(taxaccno1, "Tax account number:").Trim();
                    string taxpayname = driver.FindElement(By.XPath("//*[@id='parcelAccount" + trocher + "']/h4/a/p/strong[1]")).Text;
                    taxpaynam = GlobalClass.After(taxpayname, "Tax payer name:").Trim();
                    payststus = driver.FindElement(By.XPath("//*[@id='payment-details-panel" + trocher + "']/div/div/div/div[1]")).Text;
                    driver.FindElement(By.XPath("//*[@id='details-parcelAccount" + trocher + "']/div[3]/h4/a")).SendKeys(Keys.Enter);


                    driver.FindElement(By.XPath("//*[@id='details-parcelAccount" + trocher + "']/div[5]/h4/a")).SendKeys(Keys.Enter);
                    gc.CreatePdf(orderNumber, Parcel_no, "Recepit", driver, "WA", "King");
                    try
                    {
                        IWebElement taxpaytabel = driver.FindElement(By.XPath("//*[@id='receipts-panel" + trocher + "']/div/div/table/tbody"));
                        IList<IWebElement> taxpayrow = taxpaytabel.FindElements(By.TagName("tr"));
                        IList<IWebElement> taxpayID;
                        foreach (IWebElement trow in taxpayrow)
                        {
                            taxpayID = trow.FindElements(By.TagName("td"));
                            if (taxpayID.Count != 0 && !trow.Text.Contains("Date"))
                            {
                                string payhis = taxpayID[0].Text + "~" + taxpayID[1].Text + "~" + taxpayID[2].Text;
                                gc.insert_date(orderNumber, Parcel_no, 543, payhis, 1, DateTime.Now);
                            }

                        }
                    }
                    catch { }
                    driver.FindElement(By.XPath("//*[@id='details-parcelAccount" + trocher + "']/div[7]/h4/a")).SendKeys(Keys.Enter);
                    gc.CreatePdf(orderNumber, Parcel_no, "Tax Distribution", driver, "WA", "King");
                    try
                    {
                        IWebElement Distributiontable = driver.FindElement(By.XPath("//*[@id='distribution-panel" + trocher + "']/div/div/div/table/tbody"));
                        IList<IWebElement> distributionrow = Distributiontable.FindElements(By.TagName("tr"));
                        IList<IWebElement> distributionID;
                        foreach (IWebElement Distribution in distributionrow)
                        {
                            distributionID = Distribution.FindElements(By.TagName("td"));
                            if (distributionID.Count != 0 && !Distribution.Text.Contains("Distribution information"))
                            {
                                curtax2 = distributionID[0].Text + "~" + distributionID[1].Text + "~" + distributionID[2].Text;
                                gc.insert_date(orderNumber, Parcel_no, 568, curtax2, 1, DateTime.Now);
                            }
                        }
                    }
                    catch { }
                    int a = 0;
                    try
                    {
                        IWebElement taxyeardetail = driver.FindElement(By.XPath("//*[@id='tax-year-details-panel" + trocher + "']/div/div/fieldset/div/div/table"));
                        IList<IWebElement> taxyear = taxyeardetail.FindElements(By.TagName("tr"));
                        IList<IWebElement> taxyeartd;
                        IList<IWebElement> taxyeartd1;
                        foreach (IWebElement year in taxyear)
                        {
                            taxyeartd = year.FindElements(By.TagName("th"));
                            taxyeartd1 = year.FindElements(By.TagName("td"));
                            if (taxyeartd.Count != 0 && !year.Text.Contains("Taxable"))
                            {
                                if (taxyeartd[0].Text.Contains("Tax Information"))
                                {
                                    taxyer = taxyeartd[1].Text + "~" + taxyeartd[2].Text + "~" + taxyeartd[3].Text;
                                    DBconnection dbconn = new DBconnection();
                                    string header = "Tax Information" + "~" + taxyer;
                                    dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + header + "' where Id = '" + 539 + "'");
                                }
                            }
                            else
                            {
                                if (taxyeartd1.Count != 0 && !year.Text.Contains("Charges"))
                                {
                                    value = taxyeartd1[0].Text + "~" + taxyeartd1[1].Text + "~" + taxyeartd1[2].Text + "~" + taxyeartd1[3].Text;
                                    gc.insert_date(orderNumber, Parcel_no, 539, value, 1, DateTime.Now);
                                }

                            }
                        }
                    }

                    catch
                    {
                        try
                        {
                            IWebElement taxyeardetail = driver.FindElement(By.XPath("//*[@id='tax-year-details-panel" + trocher + "']/div/div/fieldset/div/div/table"));
                            IList<IWebElement> taxyear = taxyeardetail.FindElements(By.TagName("tr"));
                            IList<IWebElement> taxyeartd;
                            IList<IWebElement> taxyeartd1;
                            foreach (IWebElement year in taxyear)
                            {
                                taxyeartd = year.FindElements(By.TagName("th"));
                                taxyeartd1 = year.FindElements(By.TagName("td"));
                                if (taxyeartd.Count != 0 && !year.Text.Contains("Taxable"))
                                {
                                    if (taxyeartd[0].Text.Contains("Tax Information"))
                                    {
                                        taxyer = taxyeartd[1].Text + "~" + taxyeartd[2].Text;
                                        DBconnection dbconn = new DBconnection();
                                        string header = "Tax Information" + "~" + taxyer;
                                        dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + header + "' where Id = '" + 539 + "'");
                                    }
                                }
                                else
                                {
                                    if (taxyeartd1.Count != 0 && !year.Text.Contains("Charges"))
                                    {
                                        value = taxyeartd1[0].Text + "~" + taxyeartd1[1].Text + "~" + taxyeartd1[2].Text;
                                        gc.insert_date(orderNumber, Parcel_no, 539, value, 1, DateTime.Now);
                                    }
                                }
                            }

                        }
                        catch
                        { }
                    }

                    //taxing authority
                    try
                    {
                        driver.Navigate().GoToUrl("http://blue.kingcounty.gov/about/contact/");
                        driver.FindElement(By.XPath("//*[@id='footer-nav']/div/div/div[3]/ul/li[1]/a")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        taxauthority = driver.FindElement(By.XPath("//*[@id='kcMasterPagePlaceHolder_pnlForm']/div/div/div/p[3]/strong")).Text;
                        taxauthority = taxauthority.Replace("\r\n", " ");
                    }
                    catch { }


                    string curtax1 = taxaccno + "~" + payststus + "~" + taxauthority;
                    gc.insert_date(orderNumber, Parcel_no, 575, curtax1, 1, DateTime.Now);

                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "WA", "King", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();

                    gc.mergpdf(orderNumber, "WA", "King");
                    return "Data Inserted Successfully";
                }

                catch (Exception ex)
                {
                    driver.Quit();
                    throw ex;
                }
            }
        }
        public void Start(String orderNumber)
        {
            try
            {
                driver.Navigate().GoToUrl("http://blue.kingcounty.com/Assessor/eRealProperty/default.aspx");
                gc.CreatePdf(orderNumber, Parcel_no, "Aggre Click", driver, "WA", "King");
                //driver.FindElement(By.Id("cphContent_checkbox_acknowledge")).SendKeys(Keys.Enter);
                //gc.CreatePdf(orderNumber, Parcel_no, "Aggre Click 1", driver, "WA", "King");
                //driver.FindElement(By.Id("cphContent_checkbox_acknowledge")).Click();
                //gc.CreatePdf(orderNumber, Parcel_no, "Aggre Click 2", driver, "WA", "King");
                IWebElement Iviewtax = driver.FindElement(By.Id("cphContent_checkbox_acknowledge"));
                IJavaScriptExecutor js = driver as IJavaScriptExecutor;
                js.ExecuteScript("arguments[0].click();", Iviewtax);
                gc.CreatePdf(orderNumber, Parcel_no, "Aggre Click 3", driver, "WA", "King");
            }
            catch { }
        }
    }
}