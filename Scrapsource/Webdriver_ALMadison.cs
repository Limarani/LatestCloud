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
    public class Webdriver_ALMadison
    {
        IWebDriver driver;
        DBconnection db = new DBconnection();

        GlobalClass gc = new GlobalClass();
        MySqlParameter[] mParam;


        public string FTP_Madison(string houseno, string sname, string account, string parcelNumber, string orderNumber, string ownername, string searchType, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            string address = houseno + " " + sname;
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "", AssessTakenTime = "", TaxTakentime = "", CityTaxtakentime = "";
            string TotaltakenTime = "";
            string TaxAuthority = "";
            if (houseno.Trim() != "" && sname.Trim() != "" && searchType != "parcel")
            { searchType = "address"; }
            string Address = houseno.Trim() + " " + sname.Trim();
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            // driver = new ChromeDriver();
            using (driver = new PhantomJSDriver())
            {
                int Count = 0;
                string PropertyAddress = "", owner = ""; string URL = "";
                string[] stringSeparators1 = new string[] { "\r\n" };
                string Pa1 = "", Pa2 = "", Pa3 = "", Pa4 = "", Pa5 = "", Pa6 = "", Pa7 = "", Pa8 = "";
                List<string> listurl = new List<string>();
                List<string> Limulti = new List<string>();
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");


                    if (searchType == "titleflex")
                    {

                        gc.TitleFlexSearch(orderNumber, parcelNumber, "", address, "AL", "Madison");

                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_ALMadison"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }


                    if (searchType == "address")
                    {
                        driver.Navigate().GoToUrl("https://www.madisoncountyal.gov/departments/tax-collector");
                        Thread.Sleep(2000);
                        TaxAuthority = driver.FindElement(By.XPath("//*[@id='widget_9_259_446']/div/div/p[1]")).Text.Trim() + " " + driver.FindElement(By.XPath("//*[@id='widget_9_259_446']/div/div/p[2]")).Text.Trim() + " " + driver.FindElement(By.XPath("//*[@id='widget_9_259_446']/div/div/p[4]")).Text.Trim();

                        driver.Navigate().GoToUrl("http://www.deltacomputersystems.com/AL/AL47/pappraisala.html");
                        Thread.Sleep(2000);
                        driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/form/div/table/tbody/tr[2]/td[2]/input[1]")).SendKeys(houseno);
                        driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/form/div/table/tbody/tr[2]/td[2]/input[2]")).SendKeys(sname);


                        // driver.FindElement(By.ClassName("form-control add-suite ng-pristine ng-valid")).SendKeys(account);
                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "Address search", driver, "AL", "Madison");

                        driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/form/div/table/tbody/tr[6]/td[2]/input[1]")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);
                        //gc.CreatePdf_WOP(orderNumber, "Address search Result", driver, "FL", "Miami Dade");
                        Thread.Sleep(1000);
                        gc.CreatePdf_WOP(orderNumber, "Address search Result", driver, "AL", "Madison");
                        int G = 0;
                        try
                        {
                            IWebElement MultiOwnerTable = driver.FindElement(By.XPath("/html/body/center/table/tbody/tr/td/table[4]/tbody"));
                            IList<IWebElement> MultiOwnerRow = MultiOwnerTable.FindElements(By.TagName("tr"));
                            IList<IWebElement> MultiOwnerTD;
                            IList<IWebElement> MultiOwnerTA;

                            //string AlterNateID = "", PropertyAddress = "", LegalDescriptoin = "", YearBuilt = "";
                            foreach (IWebElement row1 in MultiOwnerRow)
                            {
                                MultiOwnerTD = row1.FindElements(By.TagName("td"));
                                if (MultiOwnerTD.Count != 0 && !row1.Text.Contains("PARCEL"))
                                {
                                    PropertyAddress = MultiOwnerTD[0].Text;
                                    parcelNumber = MultiOwnerTD[1].Text;
                                    ownername = MultiOwnerTD[2].Text;
                                    if (PropertyAddress.Contains(Address.ToUpper()))
                                    {
                                        MultiOwnerTA = row1.FindElements(By.TagName("a"));
                                        URL = MultiOwnerTA[0].GetAttribute("href");
                                        Limulti.Add(URL);
                                        //driver.Navigate().GoToUrl(URL);
                                        //Thread.Sleep(2000);                                        
                                        //G++;
                                        //break;


                                        string Multi = PropertyAddress + "~" + ownername;
                                        gc.insert_date(orderNumber, parcelNumber, 391, Multi, 1, DateTime.Now);
                                    }


                                }

                            }
                            if (Limulti.Count > 1)
                            {
                                HttpContext.Current.Session["multiparcel_ALMadison"] = "Yes";
                                gc.CreatePdf_WOP(orderNumber, "Address MultiParcel", driver, "AL", "Madison");
                                driver.Quit();
                                return "MultiParcel";
                            }
                            else
                            {
                                driver.Navigate().GoToUrl(URL);
                                Thread.Sleep(2000);
                            }

                            //if (G == 0)
                            //{

                            //    driver.Quit();
                            //    return "MultiParcel";
                            //}

                        }
                        catch { }

                    }




                    else if (searchType == "parcel")
                    {
                        driver.Navigate().GoToUrl("https://www.madisoncountyal.gov/departments/tax-collector");
                        Thread.Sleep(2000);
                        TaxAuthority = driver.FindElement(By.XPath("//*[@id='widget_9_259_446']/div/div/p[1]")).Text.Trim() + " " + driver.FindElement(By.XPath("//*[@id='widget_9_259_446']/div/div/p[2]")).Text.Trim() + " " + driver.FindElement(By.XPath("//*[@id='widget_9_259_446']/div/div/p[4]")).Text.Trim();


                        driver.Navigate().GoToUrl("http://www.deltacomputersystems.com/AL/AL47/pappraisala.html");
                        Thread.Sleep(2000);
                        parcelNumber = parcelNumber.Replace("-", "").Replace(".", "").Trim();

                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search", driver, "AL", "Madison");

                        Count = parcelNumber.Count();
                        try
                        {
                            if (Count == 16)
                            {
                                Pa1 = parcelNumber.Substring(0, 2);
                                Pa2 = parcelNumber.Substring(2, 2);
                                Pa3 = parcelNumber.Substring(4, 2);
                                Pa4 = parcelNumber.Substring(6, 1);
                                Pa5 = parcelNumber.Substring(7, 3);
                                Pa6 = parcelNumber.Substring(10, 3);
                                Pa7 = parcelNumber.Substring(13, 3);
                                driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/form/div/table/tbody/tr[3]/td[2]/input[1]")).SendKeys(Pa1);
                                driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/form/div/table/tbody/tr[3]/td[2]/input[2]")).SendKeys(Pa2);
                                driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/form/div/table/tbody/tr[3]/td[2]/input[3]")).SendKeys(Pa3);
                                driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/form/div/table/tbody/tr[3]/td[2]/input[4]")).SendKeys(Pa4);
                                driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/form/div/table/tbody/tr[3]/td[2]/input[5]")).SendKeys(Pa5);
                                driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/form/div/table/tbody/tr[3]/td[2]/input[6]")).SendKeys(Pa6);
                                driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/form/div/table/tbody/tr[3]/td[2]/input[7]")).SendKeys(Pa7);

                            }
                            else
                            {

                                Pa1 = parcelNumber.Substring(0, 2);
                                Pa2 = parcelNumber.Substring(2, 2);
                                Pa3 = parcelNumber.Substring(4, 2);
                                Pa4 = parcelNumber.Substring(6, 1);
                                Pa5 = parcelNumber.Substring(7, 3);
                                Pa6 = parcelNumber.Substring(10, 3);
                                Pa7 = parcelNumber.Substring(13, 3);
                                Pa8 = parcelNumber.Substring(16, 6);
                                driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/form/div/table/tbody/tr[3]/td[2]/input[1]")).SendKeys(Pa1);
                                driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/form/div/table/tbody/tr[3]/td[2]/input[2]")).SendKeys(Pa2);
                                driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/form/div/table/tbody/tr[3]/td[2]/input[3]")).SendKeys(Pa3);
                                driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/form/div/table/tbody/tr[3]/td[2]/input[4]")).SendKeys(Pa4);
                                driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/form/div/table/tbody/tr[3]/td[2]/input[5]")).SendKeys(Pa5);
                                driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/form/div/table/tbody/tr[3]/td[2]/input[6]")).SendKeys(Pa6);
                                driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/form/div/table/tbody/tr[3]/td[2]/input[7]")).SendKeys(Pa7);
                                driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/form/div/table/tbody/tr[3]/td[2]/input[8]")).SendKeys(Pa8);
                            }
                        }
                        catch
                        {
                            Pa8 = parcelNumber.Substring(16, 5);
                            driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/form/div/table/tbody/tr[3]/td[2]/input[1]")).SendKeys(Pa1);
                            driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/form/div/table/tbody/tr[3]/td[2]/input[2]")).SendKeys(Pa2);
                            driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/form/div/table/tbody/tr[3]/td[2]/input[3]")).SendKeys(Pa3);
                            driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/form/div/table/tbody/tr[3]/td[2]/input[4]")).SendKeys(Pa4);
                            driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/form/div/table/tbody/tr[3]/td[2]/input[5]")).SendKeys(Pa5);
                            driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/form/div/table/tbody/tr[3]/td[2]/input[6]")).SendKeys(Pa6);
                            driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/form/div/table/tbody/tr[3]/td[2]/input[7]")).SendKeys(Pa7);
                            driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/form/div/table/tbody/tr[3]/td[2]/input[8]")).SendKeys(Pa8);
                        }


                        Thread.Sleep(2000);
                        string ChkParcel = "";
                        driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/form/div/table/tbody/tr[6]/td[2]/input[1]")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        try
                        {
                            IWebElement MultiOwnerTable = driver.FindElement(By.XPath("/html/body/center/table/tbody/tr/td/table[4]/tbody"));
                            IList<IWebElement> MultiOwnerRow = MultiOwnerTable.FindElements(By.TagName("tr"));
                            IList<IWebElement> MultiOwnerTD;
                            IList<IWebElement> MultiOwnerTA;
                            //string AlterNateID = "", PropertyAddress = "", LegalDescriptoin = "", YearBuilt = "";
                            foreach (IWebElement row1 in MultiOwnerRow)
                            {
                                MultiOwnerTD = row1.FindElements(By.TagName("td"));
                                if (MultiOwnerTD.Count != 0 && !row1.Text.Contains("PARCEL"))
                                {

                                    ChkParcel = MultiOwnerTD[0].Text;

                                    if (ChkParcel.Replace("-", "").Replace(".", "").Trim() == parcelNumber.Replace("-", "").Replace(".", "").Trim())
                                    {
                                        MultiOwnerTA = row1.FindElements(By.TagName("a"));


                                        string Url = MultiOwnerTA[0].GetAttribute("href");
                                        driver.Navigate().GoToUrl(Url);
                                        break;

                                    }

                                }



                            }



                        }
                        catch { }

                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search Result", driver, "AL", "Madison");


                    }
                    else if (searchType == "ownername")
                    {
                        driver.Navigate().GoToUrl("https://www.madisoncountyal.gov/departments/tax-collector");
                        Thread.Sleep(2000);
                        TaxAuthority = driver.FindElement(By.XPath("//*[@id='widget_9_259_446']/div/div/p[1]")).Text.Trim() + " " + driver.FindElement(By.XPath("//*[@id='widget_9_259_446']/div/div/p[2]")).Text.Trim() + " " + driver.FindElement(By.XPath("//*[@id='widget_9_259_446']/div/div/p[4]")).Text.Trim();

                        driver.Navigate().GoToUrl("http://www.deltacomputersystems.com/AL/AL47/pappraisala.html");
                        Thread.Sleep(2000);
                        driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/form/div/table/tbody/tr[1]/td[2]/input[4]")).SendKeys(ownername);

                        gc.CreatePdf_WOP(orderNumber, "Owner Name search", driver, "AL", "Madison");

                        // driver.FindElement(By.ClassName("form-control add-suite ng-pristine ng-valid")).SendKeys(account);
                        Thread.Sleep(2000);


                        driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/form/div/table/tbody/tr[6]/td[2]/input[1]")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);
                        //gc.CreatePdf_WOP(orderNumber, "Address search Result", driver, "FL", "Miami Dade");
                        Thread.Sleep(1000);
                        gc.CreatePdf_WOP(orderNumber, "Owner Name search Result", driver, "AL", "Madison");
                        try
                        {
                            IWebElement MultiOwnerTable = driver.FindElement(By.XPath("/html/body/center/table/tbody/tr/td/table[4]/tbody"));
                            IList<IWebElement> MultiOwnerRow = MultiOwnerTable.FindElements(By.TagName("tr"));
                            IList<IWebElement> MultiOwnerTD;

                            //string AlterNateID = "", PropertyAddress = "", LegalDescriptoin = "", YearBuilt = "";
                            foreach (IWebElement row1 in MultiOwnerRow)
                            {
                                MultiOwnerTD = row1.FindElements(By.TagName("td"));
                                if (MultiOwnerTD.Count != 0 && !row1.Text.Contains("PARCEL"))
                                {
                                    ownername = MultiOwnerTD[0].Text;
                                    PropertyAddress = MultiOwnerTD[1].Text + " " + MultiOwnerTD[2].Text;
                                    parcelNumber = MultiOwnerTD[3].Text;

                                    string Multi = PropertyAddress + "~" + ownername;
                                    gc.insert_date(orderNumber, parcelNumber, 391, Multi, 1, DateTime.Now);

                                }


                            }


                            GlobalClass.multiparcel_Madison = "Yes";
                            driver.Quit();
                            return "MultiParcel";
                        }
                        catch { }

                    }

                    Thread.Sleep(2000);
                    string CHK = "";
                    string Land = "", Building = "", TotalPriceValue = "";
                    parcelNumber = parcelNumber.Replace("-", "").Replace(".", "").Trim();
                    gc.CreatePdf(orderNumber, parcelNumber, "Property & Assessement Info", driver, "AL", "Madison");
                    string PPIN = "", TAXDIST = "", ACCOUNT = "", TAXABLEVALUE = "", ASSESSMENTVALUE = "", DESCRIPTION = "", PROPERTYADDRESS = "", NEIGHBORHOOD = "", PROPERTYCLASS = "", SUBDIVISION = "", SUBDESC = "", SECTION = "";
                    IWebElement MultiOwnerProperty = driver.FindElement(By.XPath("/html/body/center/table/tbody/tr/td/table[4]/tbody"));

                    IList<IWebElement> MultiOwnerPropertyTR = MultiOwnerProperty.FindElements(By.TagName("tr"));
                    IList<IWebElement> MultiOwnerPropertyTD;

                    //string AlterNateID = "", PropertyAddress = "", LegalDescriptoin = "", YearBuilt = "";
                    foreach (IWebElement row1 in MultiOwnerPropertyTR)
                    {
                        MultiOwnerPropertyTD = row1.FindElements(By.TagName("td"));
                        if (MultiOwnerPropertyTD.Count != 0)
                        {
                            if (row1.Text.Contains("PARCEL") && !row1.Text.Contains("OLD PARCEL"))
                            {
                                parcelNumber = MultiOwnerPropertyTD[1].Text;
                                PPIN = MultiOwnerPropertyTD[2].Text;
                            }
                            else if (row1.Text.Contains("NAME"))
                            {
                                ownername = MultiOwnerPropertyTD[1].Text;
                            }
                            else if (row1.Text.Contains("ADDRESS"))
                            {
                                PROPERTYADDRESS = MultiOwnerPropertyTD[1].Text;
                            }
                            else if (row1.Text.Contains("ACCOUNT "))
                            {
                                ACCOUNT = MultiOwnerPropertyTD[1].Text;
                            }
                            else if (row1.Text.Contains("TAXABLE VALUE "))
                            {
                                TAXABLEVALUE = MultiOwnerPropertyTD[1].Text;
                                ASSESSMENTVALUE = MultiOwnerPropertyTD[2].Text.Replace("ASSESSMENT VALUE ", ""); ;
                            }
                            else if (CHK == "OK")
                            {
                                try
                                {
                                    DESCRIPTION = DESCRIPTION + " " + MultiOwnerPropertyTD[1].Text;
                                }
                                catch { }

                                //CHK = "";
                            }
                            else if (row1.Text.Contains("DESCRIPTION"))
                            {
                                CHK = "OK";
                            }



                        }

                    }

                    MultiOwnerProperty = driver.FindElement(By.XPath("/html/body/center/table/tbody/tr/td/table[5]/tbody"));
                    MultiOwnerPropertyTR = MultiOwnerProperty.FindElements(By.TagName("tr"));

                    foreach (IWebElement row5 in MultiOwnerPropertyTR)
                    {
                        MultiOwnerPropertyTD = row5.FindElements(By.TagName("td"));
                        if (MultiOwnerPropertyTD.Count != 0)
                        {
                            if (row5.Text.Contains("PROPERTY ADDRESS"))
                            {
                                PROPERTYADDRESS = MultiOwnerPropertyTD[1].Text;
                            }
                            else if (row5.Text.Contains("NEIGHBORHOOD"))
                            {
                                NEIGHBORHOOD = MultiOwnerPropertyTD[1].Text;
                            }
                            else if (row5.Text.Contains("PROPERTY CLASS"))
                            {
                                PROPERTYCLASS = MultiOwnerPropertyTD[1].Text;
                            }
                            else if (row5.Text.Contains("SUBDIVISION"))
                            {
                                SUBDIVISION = MultiOwnerPropertyTD[1].Text;
                                SUBDESC = MultiOwnerPropertyTD[3].Text;
                            }
                            else if (row5.Text.Contains("SECTION/TOWNSHIP/RANGE"))
                            {
                                SECTION = MultiOwnerPropertyTD[1].Text;

                            }
                        }

                    }
                    MultiOwnerProperty = driver.FindElement(By.XPath("/html/body/center/table/tbody/tr/td/table[6]/tbody"));
                    MultiOwnerPropertyTR = MultiOwnerProperty.FindElements(By.TagName("tr"));

                    foreach (IWebElement row5 in MultiOwnerPropertyTR)
                    {
                        MultiOwnerPropertyTD = row5.FindElements(By.TagName("td"));
                        if (MultiOwnerPropertyTD.Count != 0)
                        {
                            if (row5.Text.Contains("LAND:"))
                            {
                                Land = MultiOwnerPropertyTD[1].Text;

                            }
                            else if (row5.Text.Contains("BUILDING:"))
                            {
                                Building = MultiOwnerPropertyTD[1].Text;

                            }
                            else if (row5.Text.Contains("TOTAL PARCEL VALUE:"))
                            {
                                TotalPriceValue = MultiOwnerPropertyTD[1].Text;

                            }
                        }

                    }

                    TAXDIST = GlobalClass.After(PPIN, "TAX DIST ").Trim();
                    PPIN = gc.Between(PPIN, "PPIN", "TAX DIST").Trim();

                    string Property = ownername + "~" + PPIN + "~" + TAXDIST + "~" + ACCOUNT + "~" + TAXABLEVALUE + "~" + ASSESSMENTVALUE + "~" + DESCRIPTION + "~" + PROPERTYADDRESS + "~" + NEIGHBORHOOD + "~" + PROPERTYCLASS + "~" + SUBDIVISION + "~" + SUBDESC + "~" + SECTION;
                    gc.insert_date(orderNumber, parcelNumber, 392, Property, 1, DateTime.Now);




                    string Assessment = Land + "~" + Building + "~" + TotalPriceValue;
                    gc.insert_date(orderNumber, parcelNumber, 393, Assessment, 1, DateTime.Now);
                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");
                    string Href = driver.FindElement(By.LinkText("View Collection Record")).GetAttribute("href");

                    driver.Navigate().GoToUrl(Href);

                    Thread.Sleep(5000);
                    // gc.CreatePdf(orderNumber, parcelNumber.Trim(), "Tax search", driver, "AL", "Madison");


                    try
                    {
                        IWebElement MultiOwnerTable = driver.FindElement(By.XPath("/html/body/center/table/tbody/tr/td/table[4]/tbody"));
                        IList<IWebElement> MultiOwnerRow = MultiOwnerTable.FindElements(By.TagName("tr"));
                        IList<IWebElement> MultiOwnerTD;
                        IList<IWebElement> MultiOwnerTA;
                        string ChkParcel = "";
                        //string AlterNateID = "", PropertyAddress = "", LegalDescriptoin = "", YearBuilt = "";
                        foreach (IWebElement row1 in MultiOwnerRow)
                        {
                            MultiOwnerTD = row1.FindElements(By.TagName("td"));
                            if (MultiOwnerTD.Count != 0 && !row1.Text.Contains("PARCEL"))
                            {

                                ChkParcel = MultiOwnerTD[0].Text;
                                if (ChkParcel.Replace("-", "").Replace(".", "").Trim() == parcelNumber.Replace("-", "").Replace(".", "").Trim())
                                {
                                    MultiOwnerTA = row1.FindElements(By.TagName("a"));
                                    string Url = MultiOwnerTA[0].GetAttribute("href");
                                    driver.Navigate().GoToUrl(Url);
                                    break;
                                }
                            }
                        }
                    }
                    catch { }
                    Thread.Sleep(1000);
                    gc.CreatePdf(orderNumber, parcelNumber.Trim(), "Tax search Result", driver, "AL", "Madison");
                    //driver.FindElement(By.XPath("/html/body/center/table/tbody/tr/td/table[4]/tbody/tr[2]/td[1]/a")).Click();

                    string TaxYear = "", Acres = "", Assessed = "", ExemptCodes = "", Description = "", TaxDistrict = "", AccountNumber = "", TaxDue = "", Paid = "", Balance = "", LastPayMentDate = "";
                    gc.CreatePdf(orderNumber, parcelNumber.Trim(), "Tax Info", driver, "AL", "Madison");
                    TAXABLEVALUE = driver.FindElement(By.XPath("/html/body/center/table/tbody/tr/td/table[4]/tbody/tr[2]/td/table/tbody/tr[4]/td[4]/font")).Text.Trim();
                    TaxYear = driver.FindElement(By.XPath("/html/body/center/table/tbody/tr/td/table[2]/tbody/tr[3]/td[2]/font[1]/b")).Text.Trim().Replace("Tax Year", "");
                    Acres = driver.FindElement(By.XPath("/html/body/center/table/tbody/tr/td/table[4]/tbody/tr[2]/td/table/tbody/tr[1]/td[4]/font")).Text.Trim();
                    Assessed = driver.FindElement(By.XPath("/html/body/center/table/tbody/tr/td/table[4]/tbody/tr[2]/td/table/tbody/tr[5]/td[4]/font")).Text.Trim();
                    ExemptCodes = driver.FindElement(By.XPath("/html/body/center/table/tbody/tr/td/table[4]/tbody/tr[6]/td/table[1]/tbody/tr[1]/td[2]/font")).Text.Trim();
                    try
                    {

                        Description = driver.FindElement(By.XPath("/html/body/center/table/tbody/tr/td/table[4]/tbody/tr[6]/td/table[1]/tbody/tr[1]/td[4]/font")).Text.Trim() + " " + driver.FindElement(By.XPath("/html/body/center/table/tbody/tr/td/table[4]/tbody/tr[6]/td/table[1]/tbody/tr[2]/td[4]/font")).Text.Trim();
                    }
                    catch
                    { }

                    try
                    {

                        Description = driver.FindElement(By.XPath("/html/body/center/table/tbody/tr/td/table[4]/tbody/tr[6]/td/table[1]/tbody/tr[1]/td[4]/font")).Text.Trim() + " " + driver.FindElement(By.XPath("/html/body/center/table/tbody/tr/td/table[4]/tbody/tr[6]/td/table[1]/tbody/tr[2]/td[4]/font")).Text.Trim() + " " + driver.FindElement(By.XPath("/html/body/center/table/tbody/tr/td/table[4]/tbody/tr[15]")).Text.Trim() + " " + driver.FindElement(By.XPath("/html/body/center/table/tbody/tr/td/table[4]/tbody/tr[16]")).Text.Trim();
                    }
                    catch
                    { }

                    TaxDistrict = driver.FindElement(By.XPath("/html/body/center/table/tbody/tr/td/table[4]/tbody/tr[6]/td/table[1]/tbody/tr[3]/td[2]/font")).Text.Trim();
                    AccountNumber = driver.FindElement(By.XPath("/html/body/center/table/tbody/tr/td/table[4]/tbody/tr[6]/td/table[1]/tbody/tr[6]/td[2]/font")).Text.Trim();
                    TaxDue = driver.FindElement(By.XPath("/html/body/center/table/tbody/tr/td/table[4]/tbody/tr[4]/td/table/tbody/tr[2]/td[2]/font")).Text.Trim();
                    Paid = driver.FindElement(By.XPath("/html/body/center/table/tbody/tr/td/table[4]/tbody/tr[4]/td/table/tbody/tr[2]/td[3]/font")).Text.Trim();
                    Balance = driver.FindElement(By.XPath("/html/body/center/table/tbody/tr/td/table[4]/tbody/tr[4]/td/table/tbody/tr[2]/td[4]/font")).Text.Trim();
                    LastPayMentDate = driver.FindElement(By.XPath("/html/body/center/table/tbody/tr/td/table[4]/tbody/tr[4]/td/table/tbody/tr[4]/td[2]/font")).Text.Trim();




                    string TaxInfo = TaxYear + "~" + ownername + "~" + Acres + "~" + TAXABLEVALUE + "~" + Assessed + "~" + ExemptCodes + "~" + DESCRIPTION + "~" + TaxDistrict + "~" + PPIN + "~" + AccountNumber + "~" + TaxDue + "~" + Paid + "~" + Balance + "~" + LastPayMentDate + "~" + TaxAuthority;
                    gc.insert_date(orderNumber, parcelNumber, 397, TaxInfo, 1, DateTime.Now);


                    string Totaltax = "", Appraised = "";
                    IWebElement TaxDisTB = driver.FindElement(By.XPath("/html/body/center/table/tbody/tr/td/table[4]/tbody/tr[6]/td/table[2]/tbody"));

                    IList<IWebElement> TaxDisTR = TaxDisTB.FindElements(By.TagName("tr"));
                    IList<IWebElement> TaxDisTD;

                    foreach (IWebElement row1 in TaxDisTR)
                    {

                        TaxDisTD = row1.FindElements(By.TagName("td"));
                        if (!row1.Text.Contains("Appraised"))
                        {
                            if (TaxDisTD.Count != 0 && TaxDisTD.Count != 1)
                            {
                                TaxYear = TaxDisTD[0].Text;
                                ownername = TaxDisTD[1].Text;
                                Totaltax = TaxDisTD[2].Text;
                                Paid = TaxDisTD[3].Text;
                                Appraised = TaxDisTD[4].Text;
                                Assessed = TaxDisTD[4].Text;

                                string TaxDisDetail = TaxYear + "~" + ownername + "~" + Totaltax + "~" + Paid + "~" + Appraised + "~" + Assessed;
                                gc.insert_date(orderNumber, parcelNumber, 398, TaxDisDetail, 1, DateTime.Now);

                            }
                        }

                    }
                    TaxTime = DateTime.Now.ToString("HH:mm:ss");



                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "AL", "Madison", StartTime, AssessmentTime, TaxTime, CityTaxtakentime, LastEndTime);
                    driver.Quit();
                    gc.mergpdf(orderNumber, "AL", "Madison");
                    //gc.MMREM_Template(orderNumber, parcelNumber, "", driver, "AL", "Madison", "116","4");
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