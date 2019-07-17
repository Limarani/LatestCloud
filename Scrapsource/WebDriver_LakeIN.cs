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
    public class WebDriver_LakeIN
    {

        IWebDriver driver;
        DBconnection db = new DBconnection();

        GlobalClass gc = new GlobalClass();
        MySqlParameter[] mParam;

        public string FTP_Lake(string Address, string unitno, string parcelNumber, string ownername, string searchType, string orderNumber)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;

            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
            string Parcelno = "", Owner = "", parcellocation = "";
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            // driver = new ChromeDriver();
            //driver = new PhantomJSDriver();        

            using (driver = new PhantomJSDriver()) //ChromeDriver
            {
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");

                    if (searchType == "titleflex")
                    {
                        gc.TitleFlexSearch(orderNumber, "", "", Address, "IN", "Lake");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_LakeIN"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }

                    if (searchType == "address")
                    {
                        driver.Navigate().GoToUrl("http://counties.azurewebsites.net/lake/parcelsearch.aspx");
                        Thread.Sleep(4000);
                        driver.FindElement(By.Id("BodyContent_txtbxAddress")).SendKeys(Address.Trim());
                        gc.CreatePdf_WOP(orderNumber, "Address search", driver, "IN", "Lake");
                        driver.FindElement(By.Id("BodyContent_Submit")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);
                        gc.CreatePdf_WOP(orderNumber, "Address search Result", driver, "IN", "Lake");

                        //Multiparcel
                        try
                        {
                            IWebElement multiaddress = driver.FindElement(By.Id("BodyContent_GridViewParcelSearchResults"));
                            IList<IWebElement> TRmultiaddress = multiaddress.FindElements(By.TagName("tr"));
                            IList<IWebElement> THmultiaddress = multiaddress.FindElements(By.TagName("th"));
                            IList<IWebElement> TDmultiaddress;
                            if (TRmultiaddress.Count > 29)
                            {
                                HttpContext.Current.Session["multiParcel_Lucas_Maximum"] = "Maimum";
                                return "Maximum";
                            }
                            if (TRmultiaddress.Count > 2)
                            {
                                foreach (IWebElement row in TRmultiaddress)
                                {
                                    TDmultiaddress = row.FindElements(By.TagName("td"));
                                    if (!row.Text.Contains("Parcel Number") && row.Text.Trim() != "" && row.Text.Trim().Contains(Address.ToUpper().Trim()))
                                    {
                                        try
                                        {
                                            Parcelno = TDmultiaddress[1].Text;
                                            Owner = TDmultiaddress[3].Text;
                                            parcellocation = TDmultiaddress[2].Text;

                                            string Multi = Owner + "~" + parcellocation;
                                            gc.insert_date(orderNumber, Parcelno, 1747, Multi, 1, DateTime.Now);
                                        }
                                        catch { }
                                    }
                                }
                                HttpContext.Current.Session["multiParcel_LakeIN"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                            if (TRmultiaddress.Count == 2)
                            {
                                //TDmultiaddress[0].Click();
                                driver.FindElement(By.XPath("//*[@id='BodyContent_GridViewParcelSearchResults']/tbody/tr[2]/td[1]/a")).Click();
                                Thread.Sleep(1000);
                            }
                        }
                        catch { }
                        try
                        {
                            //No Data Found
                            string nodata = driver.FindElement(By.XPath("//*[@id='content']/p[2]")).Text;
                            if (nodata.Contains("No records returned. Please click here to return to the search page. Possible reasons for 'No records returned'."))
                            {
                                HttpContext.Current.Session["Nodata_LakeIN"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }

                    }
                    if (searchType == "parcel")
                    {
                        driver.Navigate().GoToUrl("http://counties.azurewebsites.net/lake/parcelsearch.aspx");
                        Thread.Sleep(4000);
                        driver.FindElement(By.Id("BodyContent_txtbxParcelNum")).SendKeys(parcelNumber);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search", driver, "IN", "Lake");
                        driver.FindElement(By.Id("BodyContent_Submit")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search Result", driver, "IN", "Lake");
                       
                        try
                        {
                            //No Data Found
                            string nodata = driver.FindElement(By.XPath("//*[@id='content']/p[2]")).Text;
                            if (nodata.Contains("No records returned. Please click here to return to the search page. Possible reasons for 'No records returned'."))
                            {
                                HttpContext.Current.Session["Nodata_LakeIN"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }
                    if (searchType == "ownername")
                    {
                        driver.Navigate().GoToUrl("http://counties.azurewebsites.net/lake/parcelsearch.aspx");
                        Thread.Sleep(4000);
                        driver.FindElement(By.Id("BodyContent_txtbxOwnerName")).SendKeys(ownername);
                        gc.CreatePdf(orderNumber, parcelNumber, "Owner Search", driver, "IN", "Lake");
                        driver.FindElement(By.Id("BodyContent_Submit")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Owner Search Result", driver, "IN", "Lake");
                      
                        //Multiparcel
                        try
                        {
                            IWebElement multiaddress = driver.FindElement(By.Id("BodyContent_GridViewParcelSearchResults"));
                            IList<IWebElement> TRmultiaddress = multiaddress.FindElements(By.TagName("tr"));
                            IList<IWebElement> THmultiaddress = multiaddress.FindElements(By.TagName("th"));
                            IList<IWebElement> TDmultiaddress;
                            if (TRmultiaddress.Count > 29)
                            {
                                HttpContext.Current.Session["multiParcel_Lucas_Maximum"] = "Maimum";
                                return "Maximum";
                            }
                            if (TRmultiaddress.Count > 2)
                            {
                                foreach (IWebElement row in TRmultiaddress)
                                {
                                    TDmultiaddress = row.FindElements(By.TagName("td"));
                                    if (!row.Text.Contains("Parcel Number") && row.Text.Trim() != "" && row.Text.Trim().Contains(Address.ToUpper().Trim()))
                                    {
                                        try
                                        {
                                            Parcelno = TDmultiaddress[1].Text;
                                            Owner = TDmultiaddress[3].Text;
                                            parcellocation = TDmultiaddress[2].Text;

                                            string Multi = Owner + "~" + parcellocation;
                                            gc.insert_date(orderNumber, Parcelno, 1747, Multi, 1, DateTime.Now);
                                        }
                                        catch { }
                                    }
                                }
                                HttpContext.Current.Session["multiParcel_LakeIN"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                            if (TRmultiaddress.Count == 2)
                            {                                
                                driver.FindElement(By.XPath("//*[@id='BodyContent_GridViewParcelSearchResults']/tbody/tr[2]/td[1]/a")).Click();
                                Thread.Sleep(1000);
                            }
                        }
                        catch { }
                        try
                        {
                            //No Data Found
                            string nodata = driver.FindElement(By.XPath("//*[@id='content']/p[2]")).Text;
                            if (nodata.Contains("No records returned. Please click here to return to the search page. Possible reasons for 'No records returned'."))
                            {
                                HttpContext.Current.Session["Nodata_LakeIN"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }
                    try
                    {
                        driver.FindElement(By.XPath("//*[@id='BodyContent_GridViewParcelSearchResults']/tbody/tr[2]/td[1]/a")).Click();
                        Thread.Sleep(4000);
                    }
                    catch { }
                    //Property Details
                    string ParcelID = "", TaxID = "", OwnerName = "", PropertyAddress = "", MailingAddress1 = "", MailingAddress2 = "", MailingAddress = "", Yearbuilt = "", LegalDescription = "", PropertyClass = "";
                    gc.CreatePdf(orderNumber, parcelNumber, "Propety Deatil", driver, "IN", "Lake");

                    IWebElement Propertyvalue = driver.FindElement(By.XPath("//*[@id='content']/div/div[2]/table/tbody"));
                    ParcelID = gc.Between(Propertyvalue.Text, "Parcel Number:", "TaxID:").Trim();
                    TaxID = gc.Between(Propertyvalue.Text, "TaxID:", "Property Address:").Trim();
                    PropertyAddress = gc.Between(Propertyvalue.Text.Replace("\r","").Trim(), "Property Address:", "Neighborhood Name:").Trim();
                    LegalDescription = gc.Between(Propertyvalue.Text, "Legal Description:", "Subdivision / Lot:").Trim();
                    PropertyClass = gc.Between(Propertyvalue.Text, "Property Class:", "Township:").Trim();

                    try
                    {
                        driver.FindElement(By.Id("BodyContent_ownertransferhistoryLb")).Click();
                        Thread.Sleep(4000);
                    }
                    catch { }
                    IWebElement Propertyvalue1 = driver.FindElement(By.XPath("//*[@id='content']/div/div[2]/table/tbody/tr/td[1]/table/tbody"));
                    string[] propersplit = Propertyvalue1.Text.Split('\r');
                    OwnerName = propersplit[1].Trim();
                    MailingAddress1 = propersplit[2].Trim();
                    MailingAddress2 = propersplit[3].Trim();
                    MailingAddress = MailingAddress1 + " " + MailingAddress2;
                    try
                    {
                        driver.FindElement(By.Id("BodyContent_improvementinformationLb")).Click();
                        Thread.Sleep(4000);
                    }
                    catch { }
                    try
                    {////*[@id="BodyContent_GridView10"]/tbody/tr[3]/td[4]
                        Yearbuilt = driver.FindElement(By.XPath("//*[@id='BodyContent_GridView10']/tbody/tr[3]/td[4]")).Text.Trim();
                    }
                    catch { }

                    string Propertydetails = TaxID + "~" + OwnerName + "~" + PropertyAddress + "~" + MailingAddress + "~" + Yearbuilt + "~" + LegalDescription + "~" + PropertyClass;
                    gc.insert_date(orderNumber, ParcelID, 1718, Propertydetails, 1, DateTime.Now);
                    //Assessment Details

                    driver.FindElement(By.Id("BodyContent_valuationhistoryLb")).Click();
                    Thread.Sleep(4000);
                    gc.CreatePdf(orderNumber, parcelNumber, "Assessment Deatil", driver, "IN", "Lake");

                    string title = "", value = "";
                    int s = 0;

                    IWebElement Bigdata2 = driver.FindElement(By.XPath("//*[@id='Valuation']/td/table/tbody"));
                    IList<IWebElement> TRBigdata2 = Bigdata2.FindElements(By.TagName("tr"));
                    IList<IWebElement> TDBigdata2;
                    foreach (IWebElement row2 in TRBigdata2)
                    {
                        TDBigdata2 = row2.FindElements(By.TagName("td"));

                        if (TDBigdata2.Count != 0 && TDBigdata2.Count == 14 && !row2.Text.Contains("Assess. Year"))
                        {
                            value = TDBigdata2[0].Text + "~" + TDBigdata2[1].Text + "~" + TDBigdata2[2].Text + "~" + TDBigdata2[3].Text + "~" + TDBigdata2[4].Text + "~" + TDBigdata2[5].Text + "~" + TDBigdata2[6].Text + "~" + TDBigdata2[7].Text + "~" + TDBigdata2[8].Text + "~" + TDBigdata2[9].Text + "~" + TDBigdata2[10].Text + "~" + TDBigdata2[11].Text + "~" + TDBigdata2[12].Text + "~" + TDBigdata2[13].Text;
                            gc.insert_date(orderNumber, ParcelID, 1719, value, 1, DateTime.Now);                            
                        }
                    }
                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");
                    //Tax Information Details

                    driver.Navigate().GoToUrl("http://in-lake-treasurer2.governmax.com/collectmax/search_collect.asp?l_nm=parcelid&form=searchform&formelement=0&sid=2B7CE89CDAC94FEAAD7482292D4055BA");
                    Thread.Sleep(4000);

                    driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table/tbody/tr/td/table/tbody/tr[4]/td/center/a")).Click();
                    Thread.Sleep(2000);
                    driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[1]/td[1]/table/tbody/tr[2]/td/table/tbody/tr[4]/td/font/a")).Click();
                    Thread.Sleep(2000);
                    driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table/tbody/tr/td/table/tbody/tr[1]/td/table/tbody/tr/td/table/tbody/tr[2]/td/font/input")).SendKeys(ParcelID);
                    gc.CreatePdf(orderNumber, ParcelID, "Tax Parcel Input", driver, "IN", "Lake");
                    driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table/tbody/tr/td/table/tbody/tr[3]/td/input")).Click();
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, ParcelID, "Tax Parcel Result", driver, "IN", "Lake");


                    string Parcel = "", Ownername = "", Propertyadd = "", Mailingadd = "", Propertytype = "", Taxingunit = "", Taxyear = "", Legaldes = "", Grossassessedval = "", GrossAvResidential = "", GrossAvAllpro = "", Totalgrassass = "", Deductions = "", Netassess = "", Localtaxrate = "", Grosstaxliability = "", Propertytaxcredits = "", Propertytaxcap, SixtyfiveYears = "", totalpropertaxliability = "";

                    IWebElement TaxCurrent1 = driver.FindElement(By.XPath("//*[@id='form1']/table[2]/tbody/tr[2]/td/table/tbody"));
                    IList<IWebElement> TRTaxCurrent1 = TaxCurrent1.FindElements(By.TagName("tr"));
                    IList<IWebElement> TDTaxCurrent1;
                    foreach (IWebElement row1 in TRTaxCurrent1)
                    {
                        TDTaxCurrent1 = row1.FindElements(By.TagName("td"));

                        if (TDTaxCurrent1.Count != 0 && row1.Text.Trim() != "" && !row1.Text.Contains("Property Number"))
                        {
                            Parcel = TDTaxCurrent1[0].Text.Trim();
                            Propertytype = TDTaxCurrent1[1].Text.Trim();
                            Taxingunit = TDTaxCurrent1[2].Text.Trim();
                            Taxyear = TDTaxCurrent1[3].Text.Trim();

                        }
                    }

                    IWebElement TaxCurrent2 = driver.FindElement(By.XPath("//*[@id='form1']/table[2]/tbody/tr[3]/td/table/tbody"));
                    IList<IWebElement> TRTaxCurrent2 = TaxCurrent2.FindElements(By.TagName("tr"));
                    IList<IWebElement> TDTaxCurrent2;
                    foreach (IWebElement row2 in TRTaxCurrent2)
                    {
                        TDTaxCurrent2 = row2.FindElements(By.TagName("td"));

                        if (TDTaxCurrent2.Count == 2 && TDTaxCurrent2.Count != 0 && row2.Text.Trim() != "")
                        {
                            string[] splitt = TDTaxCurrent2[0].Text.Split('\r');
                            Ownername = splitt[1].Trim();
                            string Prop1 = splitt[2].Trim();
                            string Prop2 = splitt[3].Trim();
                            Propertyadd = Prop1 + " " + Prop2;
                        }
                        if (TDTaxCurrent2.Count == 1 && TDTaxCurrent2.Count != 0 && row2.Text.Trim() != "" && !row2.Text.Contains("Legal Description:") && !row2.Text.Contains("Our records"))
                        {
                            Mailingadd = TDTaxCurrent2[0].Text.Trim().Replace("Location:", "").Trim();
                        }
                        if (TDTaxCurrent2.Count == 1 && TDTaxCurrent2.Count != 0 && row2.Text.Trim() != "" && !row2.Text.Contains("Location:") && !row2.Text.Contains("Our records"))
                        {
                            Legaldes = TDTaxCurrent2[0].Text.Trim().Replace("Legal Description:", "").Trim();
                        }
                    }

                    IWebElement TaxCurrent3 = driver.FindElement(By.XPath("//*[@id='form1']/table[2]/tbody/tr[4]/td/table/tbody"));
                    Grossassessedval = gc.Between(TaxCurrent3.Text, "1a. Gross Assessed Value (AV) of homestead property (capped at 1%)", "1b. Gross AV of residential property and farmland (capped at 2%)").Trim();
                    GrossAvResidential = gc.Between(TaxCurrent3.Text, "1b. Gross AV of residential property and farmland (capped at 2%)", "1c. Gross AV of all other property, including personal property (capped at 3%)").Trim();
                    GrossAvAllpro = gc.Between(TaxCurrent3.Text, "1c. Gross AV of all other property, including personal property (capped at 3%)", "2. Equals Total Gross Assessed Value of Property").Trim();
                    Totalgrassass = gc.Between(TaxCurrent3.Text, "2. Equals Total Gross Assessed Value of Property", "2a. Minus Deductions (See Table 5 Below)").Trim();
                    Deductions = gc.Between(TaxCurrent3.Text, "2a. Minus Deductions (See Table 5 Below)", "3. Equals Subtotal of Net Assessed Value of Property").Trim();
                    Netassess = gc.Between(TaxCurrent3.Text, "3. Equals Subtotal of Net Assessed Value of Property", "3a. Multiplied by Your Local Tax Rate").Trim();
                    Localtaxrate = gc.Between(TaxCurrent3.Text, "3a. Multiplied by Your Local Tax Rate", "4. Equals Gross Tax Liability (See Table 3 Below)").Trim();
                    Grosstaxliability = gc.Between(TaxCurrent3.Text, "4. Equals Gross Tax Liability (See Table 3 Below)", "4a. Minus Local Property Tax Credits").Trim();
                    Propertytaxcredits = gc.Between(TaxCurrent3.Text, "4a. Minus Local Property Tax Credits", "4b. Minus Savings Due to Property Tax Cap (See Table 2 Below)").Trim();
                    Propertytaxcap = gc.Between(TaxCurrent3.Text, "4b. Minus Savings Due to Property Tax Cap (See Table 2 Below)", "4c. Minus Savings Due to 65 Years & Older Cap").Trim();
                    SixtyfiveYears = gc.Between(TaxCurrent3.Text, "4c. Minus Savings Due to 65 Years & Older Cap", " 5. Total Property Tax Liability").Trim();
                    totalpropertaxliability = gc.Between(TaxCurrent3.Text, "Total Property Tax Liability", "Please See Table 4 for a Summary of Other Charges to This Property").Trim();


                    string Taxinformationdetails = Ownername + "~" + Propertyadd + "~" + Mailingadd + "~" + Propertytype + "~" + Taxingunit + "~" + Taxyear + "~" + Grossassessedval + "~" + GrossAvResidential + "~" + GrossAvAllpro + "~" + Totalgrassass + "~" + Deductions + "~" + Netassess + "~" + Localtaxrate + "~" + Grosstaxliability + "~" + Propertytaxcredits + "~" + Propertytaxcap + "~" + SixtyfiveYears + "~" + totalpropertaxliability;
                    gc.insert_date(orderNumber, ParcelID, 1743, Taxinformationdetails, 1, DateTime.Now);


                    IWebElement TaxCurrent4 = driver.FindElement(By.XPath("//*[@id='form1']/table[2]/tbody/tr[7]/td/table/tbody"));
                    IList<IWebElement> TRTaxCurrent4 = TaxCurrent4.FindElements(By.TagName("tr"));
                    IList<IWebElement> TDTaxCurrent4;
                    foreach (IWebElement row4 in TRTaxCurrent4)
                    {
                        TDTaxCurrent4 = row4.FindElements(By.TagName("td"));

                        if (TDTaxCurrent4.Count == 4 & TDTaxCurrent4.Count != 0 && row4.Text.Trim() != "" && !row4.Text.Contains("TABLE 4: OTHER APPLICABLE CHARGES") && !row4.Text.Contains("Levying Authority"))
                        {
                            string TaxinformationDetails2 = TDTaxCurrent4[0].Text + "~" + TDTaxCurrent4[1].Text + "~" + TDTaxCurrent4[2].Text + "~" + TDTaxCurrent4[3].Text;
                            gc.insert_date(orderNumber, ParcelID, 1744, TaxinformationDetails2, 1, DateTime.Now);
                        }
                    }
                    //Current Year Tax Information Details

                    IWebElement TaxCurrent5 = driver.FindElement(By.XPath("//*[@id='form1']/table[2]/tbody/tr[9]/td/table/tbody"));
                    IList<IWebElement> TRTaxCurrent5 = TaxCurrent5.FindElements(By.TagName("tr"));
                    IList<IWebElement> TDTaxCurrent5;
                    foreach (IWebElement row5 in TRTaxCurrent5)
                    {
                        TDTaxCurrent5 = row5.FindElements(By.TagName("td"));


                        if (TDTaxCurrent5.Count == 2 & TDTaxCurrent5.Count != 0 && row5.Text.Trim() != "" && !row5.Text.Contains("FIRST INSTALLMENT (SPRING)"))
                        {
                            string TaxinformationDetails4 = "Due Date" + "~" + TDTaxCurrent5[0].Text.Replace("Delinquent After", "") + "~" + TDTaxCurrent5[1].Text.Replace("Delinquent After", "");
                            gc.insert_date(orderNumber, ParcelID, 1745, TaxinformationDetails4, 1, DateTime.Now);
                        }

                        if (TDTaxCurrent5.Count == 4 & TDTaxCurrent5.Count != 0 && row5.Text.Trim() != "" && !row5.Text.Contains("FIRST INSTALLMENT (SPRING)"))
                        {
                            string TaxinformationDetails3 = TDTaxCurrent5[0].Text.Replace("(See Table 4)", "").Replace("for SPRING", "").Replace("for FALL", "") + "~" + TDTaxCurrent5[1].Text + "~" + TDTaxCurrent5[3].Text;
                            gc.insert_date(orderNumber, ParcelID, 1745, TaxinformationDetails3, 1, DateTime.Now);
                        }
                    }
                    //Tax Distribution Details

                    IWebElement TaxDistribution = driver.FindElement(By.XPath("//*[@id='form1']/table[2]/tbody/tr[6]/td/table/tbody"));
                    IList<IWebElement> TRDistribution = TaxDistribution.FindElements(By.TagName("tr"));
                    IList<IWebElement> TDDistribution;
                    foreach (IWebElement Distribution in TRDistribution)
                    {
                        TDDistribution = Distribution.FindElements(By.TagName("td"));


                        if (TDDistribution.Count == 2 & TDDistribution.Count != 0 && Distribution.Text.Trim() != "" && !Distribution.Text.Contains("FIRST INSTALLMENT (SPRING)"))
                        {
                            string TaxDistributiondeta = TDDistribution[0].Text + "~" + TDDistribution[1].Text;
                            gc.insert_date(orderNumber, ParcelID, 1746, TaxDistributiondeta, 1, DateTime.Now);
                        }

                    }
                    //Previou Year Tax Information Details
                    IWebElement TaxPrevious = driver.FindElement(By.XPath("//*[@id='form1']/table[2]/tbody/tr[13]/td/table/tbody"));
                    IList<IWebElement> TRPrevious = TaxPrevious.FindElements(By.TagName("tr"));
                    IList<IWebElement> TDPrevious;
                    foreach (IWebElement Previous in TRPrevious)
                    {
                        TDPrevious = Previous.FindElements(By.TagName("td"));

                        if (TDPrevious.Count == 2 & TDPrevious.Count != 0 && Previous.Text.Trim() != "" && !Previous.Text.Contains("FIRST INSTALLMENT (SPRING)"))
                        {
                            string TaxinformationDetails4 = "Due Date" + "~" + TDPrevious[0].Text.Replace("Delinquent After", "") + "~" + TDPrevious[1].Text.Replace("Delinquent After", "");
                            gc.insert_date(orderNumber, ParcelID, 1748, TaxinformationDetails4, 1, DateTime.Now);
                        }

                        if (TDPrevious.Count == 4 & TDPrevious.Count != 0 && Previous.Text.Trim() != "" && !Previous.Text.Contains("FIRST INSTALLMENT (SPRING)"))
                        {
                            string TaxinformationDetails3 = TDPrevious[0].Text.Replace("(See Table 4)", "").Replace("for SPRING", "").Replace("for FALL", "") + "~" + TDPrevious[1].Text + "~" + TDPrevious[3].Text;
                            gc.insert_date(orderNumber, ParcelID, 1748, TaxinformationDetails3, 1, DateTime.Now);
                        }
                    }

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "IN", "Lake", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    gc.mergpdf(orderNumber, "IN", "Lake");
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


