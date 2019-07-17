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

namespace ScrapMaricopa.Scrapsource
{
    public class WebDriver_HorrySC
    {
        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        MySqlParameter[] mParam;

        public string FTP_HorrySC(string Address, string ownerName, string unitnumber, string parcelNumber, string searchType, string orderNumber)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            IJavaScriptExecutor js = driver as IJavaScriptExecutor;
            string strmulti = "", Taxy = "";
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "", accountnumber = "";
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            //driver = new PhantomJSDriver();
            //driver = new ChromeDriver();
            using (driver = new PhantomJSDriver())
            {
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");
                    driver.Navigate().GoToUrl("http://www.horrycounty.org/apps/LandRecords");

                    //Thread.Sleep(3000);
                    if (searchType == "titleflex")
                    {
                        // string address = streetNo + " " + direction + " " + streetName + " " + streetType + " " + accountNo;
                        gc.TitleFlexSearch(orderNumber, "", "", Address.Trim(), "SC", "Horry");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_HorrySC"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }
                    if (searchType == "address")
                    {
                        driver.FindElement(By.XPath("//*[@id='appContainer']/div/div/div/div[1]/input")).SendKeys(Address);
                        gc.CreatePdf_WOP(orderNumber, "Address Search Before", driver, "SC", "Horry");
                        driver.FindElement(By.XPath("//*[@id='appContainer']/div/div/div/div[2]")).Click();
                        Thread.Sleep(5000);
                        try
                        {
                            driver.FindElement(By.XPath("//*[@id='appContainer']/div/div/div[2]/ul/li[2]/h3")).Click();
                            Thread.Sleep(7000);                           

                            IWebElement temp = driver.FindElement(By.XPath("//*[@id='graphicsLayer3_layer']"));
                            temp.Click();                         
                            //IJavaScriptExecutor js23 = driver as IJavaScriptExecutor;
                            //js23.ExecuteScript("arguments[0].click();", temp);
                            //Thread.Sleep(3000);
                        }
                        catch { }
                        //driver.FindElement(By.Id("graphicsLayer3_layer")).Click();
                        gc.CreatePdf_WOP(orderNumber, "Address Search After", driver, "SC", "Horry");
                        //Multiparcel
                        try
                        {
                            strmulti = driver.FindElement(By.XPath("//*[@id='appContainer']/div/div/div[2]/ul/li[1]/span")).Text.Trim();
                            if (Convert.ToInt32(strmulti) > 25)
                            {
                                HttpContext.Current.Session["multiParcel_Paulding_Maximum"] = "Maximum";
                                driver.Quit();
                                return "Maximum";
                            }
                            if (Convert.ToInt32(strmulti) == 1)
                            {
                                driver.FindElement(By.XPath("//*[@id='appContainer']/div/div/div[2]/ul/li[2]")).Click();
                                gc.CreatePdf_WOP(orderNumber, "Search After", driver, "SC", "Horry");
                            }
                            IWebElement Imultitable = driver.FindElement(By.XPath("//*[@id='appContainer']/div/div/div[2]/ul"));
                            IList<IWebElement> ImutiRow = Imultitable.FindElements(By.TagName("li"));
                            IList<IWebElement> ImultiTD;
                            IList<IWebElement> ImultiTD1;
                            foreach (IWebElement multi in ImutiRow)
                            {
                                ImultiTD = multi.FindElements(By.TagName("h3"));
                                ImultiTD1 = multi.FindElements(By.TagName("h4"));
                                if (ImultiTD.Count == 1 && ImultiTD1.Count == 1)
                                {
                                    string strmultiDetails = ImultiTD[0].Text.Trim() + "~" + ImultiTD1[0].Text.Replace(";", "").Trim();
                                    gc.insert_date(orderNumber, parcelNumber, 1821, strmultiDetails, 1, DateTime.Now);
                                }
                            }
                            HttpContext.Current.Session["multiParcel_HorrySC"] = "Yes";
                            driver.Quit();
                            return "Multiparcel";
                        }
                        catch { }
                        try
                        {
                            //No Data Found
                            string nodata = driver.FindElement(By.XPath("//*[@id='appContainer']/div/div/div[2]/div")).Text;
                            if (nodata.Contains("No Results Found"))
                            {
                                HttpContext.Current.Session["Nodata_HorrySC"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }
                    if (searchType == "parcel")
                    {
                        driver.FindElement(By.XPath("//*[@id='appContainer']/div/div/div/div[1]/input")).SendKeys(parcelNumber);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Before Search", driver, "SC", "Horry");
                        driver.FindElement(By.XPath("//*[@id='appContainer']/div/div/div/div[2]")).Click();
                        Thread.Sleep(2000);
                        try
                        {
                            driver.FindElement(By.XPath("//*[@id='appContainer']/div/div/div[2]/ul/li[2]")).Click();
                        }
                        catch { }
                        gc.CreatePdf_WOP(orderNumber, "Parcel Search After", driver, "SC", "Horry");
                        try
                        {
                            //No Data Found
                            string nodata = driver.FindElement(By.XPath("//*[@id='appContainer']/div/div/div[2]/div")).Text;
                            if (nodata.Contains("No Results Found"))
                            {
                                HttpContext.Current.Session["Nodata_HorrySC"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }

                    else if (searchType == "ownername")
                    {
                        driver.FindElement(By.XPath("//*[@id='appContainer']/div/div/div/div[1]/input")).SendKeys(ownerName);
                        gc.CreatePdf(orderNumber, parcelNumber, "Ownername Before Search", driver, "SC", "Horry");
                        driver.FindElement(By.XPath("//*[@id='appContainer']/div/div/div/div[2]")).Click();
                        Thread.Sleep(2000);
                       //Multiparcel
                        try
                        {
                            strmulti = driver.FindElement(By.XPath("//*[@id='appContainer']/div/div/div[2]/ul/li[1]/span")).Text.Trim();
                            if (Convert.ToInt32(strmulti) > 25)
                            {
                                HttpContext.Current.Session["multiParcel_Paulding_Maximum"] = "Maximum";
                                driver.Quit();
                                return "Maximum";
                            }
                            if (Convert.ToInt32(strmulti) == 1)
                            {
                                driver.FindElement(By.XPath("//*[@id='appContainer']/div/div/div[2]/ul/li[2]")).Click();
                                gc.CreatePdf_WOP(orderNumber, "Search After", driver, "SC", "Horry");
                            }
                            IWebElement Imultitable = driver.FindElement(By.XPath("//*[@id='appContainer']/div/div/div[2]/ul"));
                            IList<IWebElement> ImutiRow = Imultitable.FindElements(By.TagName("li"));
                            IList<IWebElement> ImultiTD;
                            IList<IWebElement> ImultiTD1;
                            foreach (IWebElement multi in ImutiRow)
                            {
                                ImultiTD = multi.FindElements(By.TagName("h3"));
                                ImultiTD1 = multi.FindElements(By.TagName("h4"));
                                if (ImultiTD.Count == 1 && ImultiTD1.Count == 1)
                                {
                                    string strmultiDetails = ImultiTD[0].Text.Trim() + "~" + ImultiTD1[0].Text.Replace(";","").Trim();
                                    gc.insert_date(orderNumber, parcelNumber, 1821, strmultiDetails, 1, DateTime.Now);
                                }
                            }
                            HttpContext.Current.Session["multiParcel_HorrySC"] = "Yes";
                            driver.Quit();
                            return "Multiparcel";
                        }
                        catch { }
                        try
                        {
                            //No Data Found
                            string nodata = driver.FindElement(By.XPath("//*[@id='appContainer']/div/div/div[2]/div")).Text;
                            if (nodata.Contains("No Results Found"))
                            {
                                HttpContext.Current.Session["Nodata_HorrySC"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }
                    //Property Details
                    try
                    {
                        driver.FindElement(By.XPath("//*[@id='appContainer']/div/div/div[2]/ul/li[2]")).Click();
                        gc.CreatePdf_WOP(orderNumber, "Search After", driver, "SC", "Horry");
                    }
                    catch { }

                    string Parcel = "", Tms = "", Owner = "", Propertyadd = "", District = "", Estimatedacres = "", Estimatedyearbuilt = "", Legal = "";

                    Owner = driver.FindElement(By.XPath("//*[@id='Land']/div/div[1]/h2")).Text.Trim();
                    Propertyadd = driver.FindElement(By.XPath("//*[@id='Land']/div/div[1]/h3")).Text.Trim();
                    IWebElement pinns = driver.FindElement(By.XPath("//*[@id='appContainer']/div/div/div[2]/div[1]/h2"));

                    string[] splitpinnns = pinns.Text.Split('/');

                    Parcel = splitpinnns[0].Replace("PIN:", "").Trim();
                    Tms = splitpinnns[1].Replace("TMS:", "").Trim();

                    IWebElement tbProper12 = driver.FindElement(By.XPath("//*[@id='Land']/div/div[2]/table[1]/tbody"));
                    IList<IWebElement> TRProper2 = tbProper12.FindElements(By.TagName("tr"));
                    IList<IWebElement> TDmulti12;
                    foreach (IWebElement row in TRProper2)
                    {
                        TDmulti12 = row.FindElements(By.TagName("td"));
                        if (TDmulti12.Count == 2 && row.Text.Contains("District"))
                        {
                            District = TDmulti12[1].Text.Trim();
                        }
                        if (TDmulti12.Count == 2 && row.Text.Contains("Estimated Acres"))
                        {
                            Estimatedacres = TDmulti12[1].Text.Trim();
                        }
                    }

                    IWebElement tbProper13 = driver.FindElement(By.XPath("//*[@id='Land']/div/div[2]/table[2]/tbody"));
                    IList<IWebElement> TRProper3 = tbProper13.FindElements(By.TagName("tr"));
                    IList<IWebElement> TDmulti13;
                    foreach (IWebElement row1 in TRProper3)
                    {
                        TDmulti13 = row1.FindElements(By.TagName("td"));
                        if (TDmulti13.Count == 2 && row1.Text.Contains("Estimated Year Built"))
                        {
                            Estimatedyearbuilt = TDmulti13[1].Text.Trim();

                        }
                    }

                    string PropertyDetails = Tms.Trim() + "~" + Owner.Trim() + "~" + Propertyadd.Trim() + "~" + District.Trim() + "~" + Estimatedacres.Trim() + "~" + Estimatedyearbuilt + "~" + Legal.Trim();
                    gc.insert_date(orderNumber, Parcel, 1799, PropertyDetails, 1, DateTime.Now);


                    //Assessment Details
                    //Taxable Values
                    string ResidentialLand = "", ResidentialImproved = "", FarmLand = "", FarmImproved = "", FarmUse = "", OtherLand = "", OtherImproved = "", Taxabletotal = "";

                    IWebElement tbAssess13 = driver.FindElement(By.XPath("//*[@id='Land']/div/div[3]/table/tbody"));
                    IList<IWebElement> TRAssess3 = tbAssess13.FindElements(By.TagName("tr"));
                    IList<IWebElement> TDAssess13;
                    foreach (IWebElement Assess in TRAssess3)
                    {
                        TDAssess13 = Assess.FindElements(By.TagName("td"));
                        if (TDAssess13.Count == 2 && Assess.Text.Contains("Residential Land"))
                        {
                            ResidentialLand = TDAssess13[1].Text.Trim();
                        }
                        if (TDAssess13.Count == 2 && Assess.Text.Contains("Residential Impr."))
                        {
                            ResidentialImproved = TDAssess13[1].Text.Trim();
                        }
                        if (TDAssess13.Count == 2 && Assess.Text.Contains("Farm Land"))
                        {
                            FarmLand = TDAssess13[1].Text.Trim();
                        }
                        if (TDAssess13.Count == 2 && Assess.Text.Contains("Farm Improved"))
                        {
                            FarmImproved = TDAssess13[1].Text.Trim();
                        }
                        if (TDAssess13.Count == 2 && Assess.Text.Contains("Farm Use"))
                        {
                            FarmUse = TDAssess13[1].Text.Trim();
                        }
                        if (TDAssess13.Count == 2 && Assess.Text.Contains("Other Land"))
                        {
                            OtherLand = TDAssess13[1].Text.Trim();
                        }
                        if (TDAssess13.Count == 2 && Assess.Text.Contains("Other Improved"))
                        {
                            OtherImproved = TDAssess13[1].Text.Trim();
                        }
                        if (TDAssess13.Count == 2 && Assess.Text.Contains("Taxable Total"))
                        {
                            Taxabletotal = TDAssess13[1].Text.Trim();
                        }
                    }

                    string Assessmenttaxabledetails = ResidentialLand.Trim() + "~" + ResidentialImproved.Trim() + "~" + FarmLand.Trim() + "~" + FarmImproved.Trim() + "~" + FarmUse.Trim() + "~" + OtherLand.Trim() + "~" + OtherImproved.Trim() + "~" + Taxabletotal.Trim();
                    gc.insert_date(orderNumber, Parcel, 1809, Assessmenttaxabledetails, 1, DateTime.Now);

                    //Market Values
                    string ResidentialMarket = "", ResidentialBlvg = "", FarmLand1 = "", FarmBuilding = "", OtherLand1 = "", Otherbuilding = "", Totalmarketvalue = "";

                    IWebElement tbAssess14 = driver.FindElement(By.XPath("//*[@id='Land']/div/div[4]/table/tbody"));
                    IList<IWebElement> TRAssess4 = tbAssess14.FindElements(By.TagName("tr"));
                    IList<IWebElement> TDAssess14;
                    foreach (IWebElement Assess4 in TRAssess4)
                    {
                        TDAssess14 = Assess4.FindElements(By.TagName("td"));
                        if (TDAssess14.Count == 2 && Assess4.Text.Contains("Residential Land"))
                        {
                            ResidentialMarket = TDAssess14[1].Text.Trim();
                        }
                        if (TDAssess14.Count == 2 && Assess4.Text.Contains("Residential Bldg."))
                        {
                            ResidentialBlvg = TDAssess14[1].Text.Trim();
                        }
                        if (TDAssess14.Count == 2 && Assess4.Text.Contains("Farm Land"))
                        {
                            FarmLand1 = TDAssess14[1].Text.Trim();
                        }
                        if (TDAssess14.Count == 2 && Assess4.Text.Contains("Farm Building"))
                        {
                            FarmBuilding = TDAssess14[1].Text.Trim();
                        }
                        if (TDAssess14.Count == 2 && Assess4.Text.Contains("Other Land"))
                        {
                            OtherLand1 = TDAssess14[1].Text.Trim();
                        }
                        if (TDAssess14.Count == 2 && Assess4.Text.Contains("Other Building"))
                        {
                            Otherbuilding = TDAssess14[1].Text.Trim();
                        }
                        if (TDAssess14.Count == 2 && Assess4.Text.Contains("Total Market Value"))
                        {
                            Totalmarketvalue = TDAssess14[1].Text.Trim();
                        }
                    }
                    string Assessmentmarketdetails = ResidentialMarket.Trim() + "~" + ResidentialBlvg.Trim() + "~" + FarmLand1.Trim() + "~" + FarmBuilding.Trim() + "~" + OtherLand1.Trim() + "~" + Otherbuilding.Trim() + "~" + Totalmarketvalue.Trim();
                    gc.insert_date(orderNumber, Parcel, 1810, Assessmentmarketdetails, 1, DateTime.Now);


                    //Permits Details    
                    //Insert 1811
                    try
                    {
                        driver.FindElement(By.XPath("//*[@id='appContainer']/div/div/div[2]/div[2]/ul/li[4]")).Click();
                        Thread.Sleep(2000);
                        driver.FindElement(By.XPath("//*[@id='Permits']/div/div/i")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, Parcel, "Permits Details", driver, "SC", "Horry");
                    }
                    catch { }
                    string Singlefamily = "", Issued = "", Applicationdate = "", Value = "", Squarefootage = "", Description = "";

                    Singlefamily = driver.FindElement(By.XPath("//*[@id='Permits']/div/div/h2[1]")).Text.Replace("Single Family -", "");
                    Issued = driver.FindElement(By.XPath("//*[@id='Permits']/div/div/h2[2]")).Text.Replace("Issued", "");

                    IWebElement tbPermit14 = driver.FindElement(By.XPath("//*[@id='Permits']/div/div/table/tbody"));
                    IList<IWebElement> TRPermit4 = tbPermit14.FindElements(By.TagName("tr"));
                    IList<IWebElement> TDPermit14;
                    foreach (IWebElement Permit in TRPermit4)
                    {
                        TDPermit14 = Permit.FindElements(By.TagName("td"));
                        if (TDPermit14.Count == 2 && Permit.Text.Contains("Application Date"))
                        {
                            Applicationdate = TDPermit14[1].Text.Trim();
                        }
                        if (TDPermit14.Count == 2 && Permit.Text.Contains("Value"))
                        {
                            Value = TDPermit14[1].Text.Trim();
                        }
                        if (TDPermit14.Count == 2 && Permit.Text.Contains("Square Footage"))
                        {
                            Squarefootage = TDPermit14[1].Text.Trim();
                        }
                        if (TDPermit14.Count == 2 && Permit.Text.Contains("Description"))
                        {
                            Description = TDPermit14[1].Text.Trim();
                        }

                    }
                    string Permitdetails = Singlefamily.Trim() + "~" + Issued.Trim() + "~" + Applicationdate.Trim() + "~" + Value.Trim() + "~" + Squarefootage.Trim() + "~" + Description;
                    gc.insert_date(orderNumber, Parcel, 1811, Permitdetails, 1, DateTime.Now);

                    //Parcel Details          

                    string Parcel1 = "", Tms1 = "", owner = "", NeighborhoodName = "", NeighborhoodNumber = "", Jurisdiction = "", Area = "", District1 = "", CensusTract = "", LegalAcres = "";

                    IWebElement IAddressSearch1 = driver.FindElement(By.XPath("//*[@id='appContainer']/div/div/div[2]/div[4]/div[4]/div[2]/div[1]"));
                    IJavaScriptExecutor js1 = driver as IJavaScriptExecutor;
                    js1.ExecuteScript("arguments[0].click();", IAddressSearch1);
                    Thread.Sleep(9000);
                    driver.SwitchTo().Window(driver.WindowHandles.Last());

                    owner = driver.FindElement(By.XPath("/html/body/div/div[2]/div/div[1]/div[1]/h2")).Text.Trim();
                    Propertyadd = driver.FindElement(By.XPath("/html/body/div/div[2]/div/div[1]/div[1]/h3")).Text.Trim();

                    IWebElement Pin11 = driver.FindElement(By.XPath("/html/body/div/div[1]/h2"));

                    string[] splitpinnns1 = Pin11.Text.Split('-');

                    Parcel1 = splitpinnns[0].Replace("PIN:", "").Trim();
                    Tms1 = splitpinnns[1].Replace("TMS:", "").Trim();

                    gc.CreatePdf(orderNumber, Parcel1, "Parcel Details", driver, "SC", "Horry");

                    IWebElement tbParcel = driver.FindElement(By.XPath("/html/body/div/div[2]/div/div[1]/div[2]/table/tbody"));
                    IList<IWebElement> TRParcel = tbParcel.FindElements(By.TagName("tr"));
                    IList<IWebElement> TDParcel;
                    foreach (IWebElement Parcelrow in TRParcel)
                    {
                        TDParcel = Parcelrow.FindElements(By.TagName("td"));
                        if (TDParcel.Count == 2 && Parcelrow.Text.Contains("Neighborhood Name"))
                        {
                            NeighborhoodName = TDParcel[1].Text.Trim();
                        }
                        if (TDParcel.Count == 2 && Parcelrow.Text.Contains("Neighborhood Number"))
                        {
                            NeighborhoodNumber = TDParcel[1].Text.Trim();
                        }
                        if (TDParcel.Count == 2 && Parcelrow.Text.Contains("Jurisdiction"))
                        {
                            Jurisdiction = TDParcel[1].Text.Trim();
                        }
                        if (TDParcel.Count == 2 && Parcelrow.Text.Contains("Area"))
                        {
                            Area = TDParcel[1].Text.Trim();
                        }
                        if (TDParcel.Count == 2 && Parcelrow.Text.Contains("District"))
                        {
                            District1 = TDParcel[1].Text.Trim();
                        }
                        if (TDParcel.Count == 2 && Parcelrow.Text.Contains("Census Tract"))
                        {
                            CensusTract = TDParcel[1].Text.Trim();
                        }
                        if (TDParcel.Count == 2 && Parcelrow.Text.Contains("Legal Acres"))
                        {
                            LegalAcres = TDParcel[1].Text.Trim();
                        }
                    }
                    string Parceldetails = Tms.Trim() + "~" + Owner.Trim() + "~" + Propertyadd.Trim() + "~" + NeighborhoodName.Trim() + "~" + NeighborhoodNumber.Trim() + "~" + Jurisdiction + "~" + Area.Trim() + "~" + District1.Trim() + "~" + CensusTract.Trim() + "~" + LegalAcres.Trim();
                    gc.insert_date(orderNumber, Parcel, 1812, Parceldetails, 1, DateTime.Now);

                    //Assessment Details
                    string Assessmentyear = "", Reasonforchange = "", MarketvalLand = "", MarketvalImprovement = "", MarketvalTotal = "", LanduseLand = "", LandImprovement = "", Landusetotal = "";
                    IWebElement tbAssess = driver.FindElement(By.XPath("/html/body/div/div[4]/div/table/tbody"));
                    IList<IWebElement> TRAssess = tbAssess.FindElements(By.TagName("tr"));
                    IList<IWebElement> TDAssess;
                    foreach (IWebElement Assess in TRAssess)
                    {
                        TDAssess = Assess.FindElements(By.TagName("td"));
                        if (TDAssess.Count == 8)
                        {
                            Assessmentyear = TDAssess[0].Text.Trim();
                            Reasonforchange = TDAssess[1].Text.Trim();
                            MarketvalLand = TDAssess[2].Text.Trim();
                            MarketvalImprovement = TDAssess[3].Text.Trim();
                            MarketvalTotal = TDAssess[4].Text.Trim();
                            LanduseLand = TDAssess[5].Text.Trim();
                            LandImprovement = TDAssess[6].Text.Trim();
                            Landusetotal = TDAssess[7].Text.Trim();

                            string Assessdetails = Assessmentyear.Trim() + "~" + Reasonforchange.Trim() + "~" + MarketvalLand.Trim() + "~" + MarketvalImprovement.Trim() + "~" + MarketvalTotal.Trim() + "~" + LanduseLand + "~" + LandImprovement.Trim() + "~" + Landusetotal.Trim();
                            gc.insert_date(orderNumber, Parcel, 1813, Assessdetails, 1, DateTime.Now);
                        }
                    }
                    //Transfer of Ownership
                    string Owner1 = "", Consideration = "", Transferdate = "", Deedbook = "", Deedtype = "";
                    IWebElement Transfer = driver.FindElement(By.XPath("/html/body/div/div[3]/div/table/tbody"));
                    IList<IWebElement> TRTransfer = Transfer.FindElements(By.TagName("tr"));
                    IList<IWebElement> TDTransfer;
                    foreach (IWebElement Transfer1 in TRTransfer)
                    {
                        TDTransfer = Transfer1.FindElements(By.TagName("td"));
                        if (TDTransfer.Count == 5)
                        {
                            Owner1 = TDTransfer[0].Text.Trim();
                            Consideration = TDTransfer[1].Text.Trim();
                            Transferdate = TDTransfer[2].Text.Trim();
                            Deedbook = TDTransfer[3].Text.Trim();
                            Deedtype = TDTransfer[4].Text.Trim();

                            string Transferof = Owner1.Trim() + "~" + Consideration.Trim() + "~" + Transferdate.Trim() + "~" + Deedbook.Trim() + "~" + Deedtype.Trim();
                            gc.insert_date(orderNumber, Parcel, 1823, Transferof, 1, DateTime.Now);
                        }
                    }
                    //Tax Information Details
                    driver.Navigate().GoToUrl("https://horrycountytreasurer.qpaybill.com/Taxes/TaxesDefaultType4.aspx");


                    IWebElement ISelect1 = driver.FindElement(By.XPath("//*[@id='ctl00_MainContent_ddlCriteriaList']"));
                    SelectElement sSelect1 = new SelectElement(ISelect1);
                    sSelect1.SelectByText("PIN");

                    driver.FindElement(By.XPath("//*[@id='ctl00_MainContent_txtCriteriaBox']")).SendKeys(Parcel.Trim());
                    gc.CreatePdf_WOP(orderNumber, "Tax Info Page", driver, "SC", "Horry");

                    IWebElement IAddressSearch2 = driver.FindElement(By.XPath("//*[@id='ctl00_MainContent_btnSearch']"));
                    IJavaScriptExecutor js2 = driver as IJavaScriptExecutor;
                    js1.ExecuteScript("arguments[0].click();", IAddressSearch2);
                    Thread.Sleep(9000);

                    //Tax Information Details
                    string Noticenumber = "", Name = "", Year = "", Descriptions = "", Type = "", Status = "", Paymentdate = "", Amount = "";
                    IWebElement Taxinfo1 = driver.FindElement(By.XPath("//*[@id='ctl00_MainContent_gvSearchResults']/tbody"));
                    IList<IWebElement> TRTaxinfo = Taxinfo1.FindElements(By.TagName("tr"));
                    IList<IWebElement> TDTaxinfo;
                    foreach (IWebElement Taxinfo in TRTaxinfo)
                    {
                        TDTaxinfo = Taxinfo.FindElements(By.TagName("td"));
                        if (TDTaxinfo.Count == 11)
                        {
                            Noticenumber = TDTaxinfo[0].Text.Trim();
                            Name = TDTaxinfo[1].Text.Trim();
                            Year = TDTaxinfo[2].Text.Trim();
                            Descriptions = TDTaxinfo[3].Text.Trim();
                            Type = TDTaxinfo[5].Text.Trim();
                            Status = TDTaxinfo[6].Text.Trim();
                            Paymentdate = TDTaxinfo[7].Text.Trim();
                            Amount = TDTaxinfo[8].Text.Trim();

                            string TaxHistorydetails = Noticenumber.Trim() + "~" + Name.Trim() + "~" + Year.Trim() + "~" + Descriptions.Trim() + "~" + Type.Trim() + "~" + Status.Trim() + "~" + Paymentdate.Trim() + "~" + Amount.Trim();
                            gc.insert_date(orderNumber, Parcel, 1838, TaxHistorydetails, 1, DateTime.Now);
                        }
                    }

                    for (int i = 0; i < 5; i++)
                    {
                        try
                        {////*[@id="ctl00_MainContent_gvSearchResults"]/tbody/tr[2]/td[10]
                            IWebElement Receipttable1 = driver.FindElement(By.XPath("//*[@id='ctl00_MainContent_gvSearchResults']/tbody/tr[" + i + "]/td[10]"));
                            Receipttable1.Click();
                            Thread.Sleep(5000);
                            gc.CreatePdf(orderNumber, Parcel, "Tax "+i, driver, "SC", "Horry");
                            //
                            string Recordtype = "", Mapnumber = "", Pin = "", Acres = "", Description1 = "", TaxownerName = "", Address1 = "", Taxyear = "", Districtlevy = "", Citylevy = "", Totalappraised = "", Totalassessd = "", Assessmentratio = "", Landappraisal = "", Buildingappraisal = "", Countytax = "", Citytax = "", Fees = "", Residential = "", Homestead = "", Otherexemption = "", Localoption = "", Totaltax = "",Totalpaid="";

                            //Property Information
                            Recordtype = driver.FindElement(By.Id("ctl00_MainContent_lblRecordType")).Text.Trim();
                            Mapnumber = driver.FindElement(By.Id("ctl00_MainContent_lblMapNo")).Text.Trim();
                            Acres = driver.FindElement(By.Id("ctl00_MainContent_lblAcres")).Text.Trim();
                            Description1 = driver.FindElement(By.Id("ctl00_MainContent_lblDesc2")).Text.Trim();
                            //Tax Information
                            TaxownerName = driver.FindElement(By.Id("ctl00_MainContent_lblName")).Text.Trim();
                            Address1 = driver.FindElement(By.Id("ctl00_MainContent_LabelAddress")).Text.Trim();
                            TaxownerName = driver.FindElement(By.Id("ctl00_MainContent_LabelAddress")).Text.Trim();
                            Taxyear = driver.FindElement(By.Id("ctl00_MainContent_lblTaxYr")).Text.Trim();
                            Districtlevy = driver.FindElement(By.Id("ctl00_MainContent_lblDistrict")).Text.Trim();
                            Citylevy = driver.FindElement(By.Id("ctl00_MainContent_lblCity")).Text.Trim();
                            Totalappraised = driver.FindElement(By.Id("ctl00_MainContent_lblMarketVal")).Text.Trim();
                            Totalassessd = driver.FindElement(By.Id("ctl00_MainContent_lblAssmt")).Text.Trim();
                            Assessmentratio = driver.FindElement(By.XPath("//*[@id='ctl00_MainContent_PanelAppraisal']/li/table/tbody/tr[2]/td[1]")).Text.Trim();
                            Landappraisal = driver.FindElement(By.XPath("//*[@id='ctl00_MainContent_PanelAppraisal']/li/table/tbody/tr[2]/td[2]")).Text.Trim();
                            Buildingappraisal = driver.FindElement(By.XPath("//*[@id='ctl00_MainContent_PanelAppraisal']/li/table/tbody/tr[2]/td[3]")).Text.Trim();
                            //Taxes
                            Countytax = driver.FindElement(By.Id("ctl00_MainContent_lblCountyTax")).Text.Trim();
                            Citytax = driver.FindElement(By.Id("ctl00_MainContent_lblCityTax")).Text.Trim();
                            Fees = driver.FindElement(By.Id("ctl00_MainContent_lblFees")).Text.Trim();
                            Residential = driver.FindElement(By.Id("ctl00_MainContent_lblResidential")).Text.Trim();
                            Homestead = driver.FindElement(By.Id("ctl00_MainContent_lblHomestead")).Text.Trim();
                            Otherexemption = driver.FindElement(By.Id("ctl00_MainContent_lblOther")).Text.Trim();
                            Localoption = driver.FindElement(By.Id("ctl00_MainContent_lblLocalOpts")).Text.Trim();
                            Totaltax = driver.FindElement(By.Id("ctl00_MainContent_lblTotalTaxes")).Text.Trim();
                            Totalpaid = driver.FindElement(By.Id("ctl00_MainContent_lblTotalPd")).Text.Trim();

                            string Taxinformationsdetals = Taxyear + "~" + Recordtype.Trim() + "~" + Mapnumber.Trim() + "~" + Acres.Trim() + "~" + Description1.Trim() + "~" + TaxownerName.Trim() + "~" + Districtlevy.Trim() + "~" + Citylevy.Trim() + "~" + Totalappraised.Trim() + "~" + Totalassessd.Trim() + "~" + Assessmentratio.Trim() + "~" + Landappraisal.Trim() + "~" + Buildingappraisal.Trim() + "~" + Countytax.Trim() + "~" + Citytax.Trim() + "~" + Fees.Trim() + "~" + Residential.Trim() + "~" + Homestead.Trim() + "~" + Otherexemption.Trim() + "~" + Localoption.Trim() + "~" + Totaltax.Trim()+"~"+ Totalpaid.Trim();
                            gc.insert_date(orderNumber, Parcel, 1839, Taxinformationsdetals, 1, DateTime.Now);
                            driver.Navigate().Back();
                        }
                        catch { }
                    }

                    TaxTime = DateTime.Now.ToString("HH:mm:ss");
                    CitytaxTime = DateTime.Now.ToString("HH:mm:ss");
                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "SC", "Horry", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    gc.mergpdf(orderNumber, "SC", "Horry");
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
