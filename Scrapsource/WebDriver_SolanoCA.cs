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
    public class WebDriver_SolanoCA
    {

        //IWebDriver driver;
        IWebDriver driver;
        DBconnection db = new DBconnection();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        GlobalClass gc = new GlobalClass();


        public string FTP_SolanoCA(string houseno, string streetname, string direction, string streettype, string ownername, string parcelNumber, string searchType, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";


            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            driver = new ChromeDriver();
            //driver = new PhantomJSDriver();
            //driver = new ChromeDriver();
            var option = new ChromeOptions();
            option.AddArgument("No-Sandbox");
            using (driver = new ChromeDriver(option)) //ChromeDriver
            {
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");
                    driver.Navigate().GoToUrl("http://www.solanocounty.com/depts/ar/viewpropertyinfo.asp");

                    if (searchType == "titleflex")
                    {
                        //string Address = houseno + " " + direction + " " + streetname + " " + streettype;
                        gc.TitleFlexSearch(orderNumber, "", ownername, "", "CA", "Saolano");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_SolanoCA"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }
                    if (searchType == "address")
                    {
                        IWebElement Multyaddresstable1 = driver.FindElement(By.Id("cvIframe"));
                        driver.SwitchTo().Frame(Multyaddresstable1);
                        driver.FindElement(By.XPath("//*[@id='Table9']/tbody/tr/td[2]/table/tbody/tr[1]/td/table/tbody/tr[3]/td/table/tbody/tr/td[4]/table/tbody/tr[1]/td/table/tbody/tr[1]/td[3]/input")).SendKeys(houseno);
                        driver.FindElement(By.XPath("//*[@id='Table9']/tbody/tr/td[2]/table/tbody/tr[1]/td/table/tbody/tr[3]/td/table/tbody/tr/td[4]/table/tbody/tr[1]/td/table/tbody/tr[2]/td[3]/input")).SendKeys(streetname);
                        // driver.FindElement(By.XPath("//*[@id='Table9']/tbody/tr/td[2]/table/tbody/tr[1]/td/table/tbody/tr[3]/td/table/tbody/tr/td[4]/table/tbody/tr[1]/td/table/tbody/tr[5]/td/table/tbody/tr/td[2]/input")).SendKeys(Keys.Enter);

                        try
                        {
                            int count = 0;
                            string Multiparcelnumber = "", Singlerowclick = "";
                            IWebElement Multiaddresstable1add = driver.FindElement(By.XPath("//*[@id='Table9']/tbody/tr/td[2]/table/tbody/tr[1]/td/table/tbody/tr[4]/td/table/tbody/tr[2]/td/table/tbody"));
                            IList<IWebElement> multiaddressrows = Multiaddresstable1add.FindElements(By.TagName("tr"));
                            IList<IWebElement> Multiaddressid;
                            foreach (IWebElement Multiaddress in multiaddressrows)
                            {
                                Multiaddressid = Multiaddress.FindElements(By.TagName("td"));
                                if (Multiaddressid.Count == 4 && !Multiaddress.Text.Contains("Select") && Multiparcelnumber != Multiaddressid[0].Text)
                                {
                                    Multiparcelnumber = Multiaddressid[0].Text;
                                    IWebElement Singleclick = Multiaddressid[0].FindElement(By.TagName("a"));
                                    Singlerowclick = Singleclick.GetAttribute("href");
                                    string Owneraddress = Multiaddressid[1].Text;
                                    string multiaddressresult = Multiparcelnumber + "~" + Owneraddress;
                                    gc.insert_date(orderNumber, Multiparcelnumber, 1296, multiaddressresult, 1, DateTime.Now);
                                    count++;
                                }
                            }

                            if (count < 2)
                            {
                                driver.Navigate().GoToUrl(Singlerowclick);
                            }
                            if (count > 1 && count < 26)
                            {
                                HttpContext.Current.Session["multiParcel_Solano"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                            if (count > 25)
                            {
                                HttpContext.Current.Session["multiparcel_Solano_Maximum"] = "Maximum";
                                driver.Quit();
                                return "Maximum";
                            }                           
                        }
                        catch { }
                        //try
                        //{
                        //    //No Data Found
                        //    string nodata = driver.FindElement(By.Id("AddressErrorMsg")).Text;
                        //    if (nodata.Contains("No Records were found that matched your input"))
                        //    {
                        //        HttpContext.Current.Session["Nodata_SolanoCA"] = "Yes";
                        //        driver.Quit();
                        //        return "No Data Found";
                        //    }
                        //}
                        //catch { }
                    }

                    if (searchType == "parcel")
                    {
                        IWebElement Multyaddresstable1 = driver.FindElement(By.Id("cvIframe"));
                        driver.SwitchTo().Frame(Multyaddresstable1);
                        driver.FindElement(By.XPath("//*[@id='Table9']/tbody/tr/td[2]/table/tbody/tr[1]/td/table/tbody/tr[5]/td/table/tbody/tr/td[4]/table/tbody/tr[1]/td/table/tbody/tr[1]/td/table/tbody/tr[2]/td[3]/input")).SendKeys(parcelNumber);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel search", driver, "CA", "Solano");

                    }
                    //Property Details  
                    try
                    {
                        driver.FindElement(By.XPath("//*[@id='Table9']/tbody/tr/td[2]/table/tbody/tr[1]/td/table/tbody/tr[5]/td/table/tbody/tr/td[4]/table/tbody/tr[1]/td/table/tbody/tr[3]/td/table/tbody/tr/td[2]")).Click();
                        Thread.Sleep(9000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel search result1", driver, "CA", "Solano");
                    }
                    catch { }

                    try
                    {
                        //No Data Found
                        string nodata = driver.FindElement(By.XPath("//*[@id='Table9']/tbody/tr/td[2]/table/tbody/tr[1]/td/table/tbody")).Text;
                        if (nodata.Contains("No Situs record on file") || nodata.Contains("No Records were found"))
                        {
                            HttpContext.Current.Session["Nodata_SolanoCA"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }
                    try
                    {
                        IWebElement IAddressSearch1 = driver.FindElement(By.LinkText("Property Values, Details and Information"));
                        IJavaScriptExecutor js1 = driver as IJavaScriptExecutor;
                        js1.ExecuteScript("arguments[0].click();", IAddressSearch1);
                        Thread.Sleep(15000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Propertyclick search result1", driver, "CA", "Solano");
                        ByVisibleElement(driver.FindElement(By.XPath("//*[@id='Table9']/tbody/tr/td[2]/table/tbody/tr[1]/td/table/tbody/tr[4]/td/table/tbody/tr[2]/td/table/tbody/tr[5]/td[3]")));
                        gc.CreatePdf(orderNumber, parcelNumber, "Propertyclick search result2", driver, "CA", "Solano");
                    }
                    catch { }
                    //driver.Quit();               

                    string ParcelNumber = "", Usecode1 = "", Exemption = "", Subdivision = "", Yearbuilt = "", Unit = "", Lot = "", Block = "", Sublot = "";

                    ParcelNumber = driver.FindElement(By.XPath("//*[@id='Table9']/tbody/tr/td[2]/table/tbody/tr[1]/td/table/tbody/tr[1]/td/table/tbody/tr[2]/td[1]")).Text;
                    try
                    {
                        Yearbuilt = driver.FindElement(By.XPath("//*[@id='Table9']/tbody/tr/td[2]/table/tbody/tr[1]/td/table/tbody/tr[3]/td/table/tbody/tr/td/table/tbody/tr[2]/td/table/tbody/tr[1]/td[5]")).Text;
                    }
                    catch { }
                    IWebElement Bigdata3 = driver.FindElement(By.XPath("//*[@id='Table9']/tbody/tr/td[2]/table/tbody/tr[1]/td/table/tbody/tr[2]/td/table/tbody"));
                    IList<IWebElement> TRBigdata3 = Bigdata3.FindElements(By.TagName("tr"));
                    IList<IWebElement> TDBigdata3;
                    foreach (IWebElement row3 in TRBigdata3)
                    {
                        TDBigdata3 = row3.FindElements(By.TagName("td"));

                        if (TDBigdata3.Count != 0 && TDBigdata3.Count == 5 && !row3.Text.Contains("Property Information") && row3.Text.Contains("Acres"))
                        {
                            Exemption = TDBigdata3[4].Text;
                            //Subdivision = TDBigdata3[29].Text;
                            //Yearbuilt = TDBigdata3[34].Text;
                        }
                        if (TDBigdata3.Count != 0 && TDBigdata3.Count == 5 && !row3.Text.Contains("Property Information") && row3.Text.Contains("Use Code"))
                        {
                            Usecode1 = TDBigdata3[2].Text.Trim();
                            string unit1 = TDBigdata3[4].Text;
                            Unit = gc.Between(unit1, "Unit", "Lot").Trim().Replace("-", " ");
                            Lot = gc.Between(unit1, "Lot", "Block").Trim().Replace("-", " ");
                            Block = gc.Between(unit1, "Block", "Sublot").Trim().Replace("-", " ");
                            Sublot = GlobalClass.After(unit1, "Sublot").Trim().Replace("-", " ");

                        }
                        if (TDBigdata3.Count != 0 && TDBigdata3.Count == 4 && !row3.Text.Contains("Property Information") && row3.Text.Contains("By"))
                        {
                            //Exemption = TDBigdata3[5].Text;

                            string Subdivision1 = TDBigdata3[3].Text;
                            string[] splitval2 = Subdivision1.Split();

                            try
                            {
                                string Subdivision2 = splitval2[2];
                                string Subdivision3 = splitval2[3];
                                Subdivision = Subdivision2 + " " + Subdivision3;
                            }
                            catch { }
                            try
                            {
                                Subdivision = splitval2[2];

                            }
                            catch { }

                            //Yearbuilt = TDBigdata3[34].Text;
                        }
                    }
                    string PropertyDetails = Exemption.Trim() + "~" + Subdivision.Trim() + "~" + Usecode1.Trim() + "~" + Unit.Trim() + "~" + Lot.Trim() + "~" + Block.Trim() + "~" + Sublot.Trim() + "~" + Yearbuilt.Trim();
                    gc.insert_date(orderNumber, ParcelNumber, 1280, PropertyDetails, 1, DateTime.Now);
                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");

                    //Assesment Details
                    string Year = "", status = "", TaxAreacode = "", TacLastYear = "", Usecode = "", Exemstatus = "";

                    IWebElement Bigdata4 = driver.FindElement(By.XPath("//*[@id='Table9']/tbody/tr/td[2]/table/tbody/tr[1]/td/table/tbody/tr[4]/td/table/tbody/tr[2]/td/table/tbody"));
                    IList<IWebElement> TRBigdata4 = Bigdata4.FindElements(By.TagName("tr"));
                    IList<IWebElement> TDBigdata4;
                    foreach (IWebElement row4 in TRBigdata4)
                    {
                        TDBigdata4 = row4.FindElements(By.TagName("td"));

                        if (TDBigdata4[0].Text.Trim() == "" & !row4.Text.Contains("Full Values"))
                        {
                            status += TDBigdata4[1].Text + "~";
                            TaxAreacode += TDBigdata4[2].Text + "~";
                            TacLastYear += TDBigdata4[3].Text + "~";
                            Usecode += TDBigdata4[4].Text + "~";
                            Exemstatus += TDBigdata4[5].Text + "~";
                        }
                        if (TDBigdata4.Count != 0 && TDBigdata4.Count == 6 && TDBigdata4[0].Text.Trim() != "" & !row4.Text.Contains("Full Values"))
                        {
                            Year += TDBigdata4[0].Text + "~";
                            status += TDBigdata4[1].Text + "~";
                            TaxAreacode += TDBigdata4[2].Text + "~";
                            TacLastYear += TDBigdata4[3].Text + "~";
                            Usecode += TDBigdata4[4].Text + "~";
                            Exemstatus += TDBigdata4[5].Text + "~";
                        }
                    }
                    db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + "Tax Year~" + Year.Remove(Year.Length - 1, 1) + "' where Id = '" + 1281 + "'");
                    string Assessmentdetails = status + "~" + TaxAreacode + "~" + TacLastYear + "~" + Usecode + "~" + Exemstatus;

                    gc.insert_date(orderNumber, ParcelNumber, 1281, status.Remove(status.Length - 1, 1), 1, DateTime.Now);
                    gc.insert_date(orderNumber, ParcelNumber, 1281, TaxAreacode.Remove(TaxAreacode.Length - 1, 1), 1, DateTime.Now);
                    gc.insert_date(orderNumber, ParcelNumber, 1281, TacLastYear.Remove(TacLastYear.Length - 1, 1), 1, DateTime.Now);
                    gc.insert_date(orderNumber, ParcelNumber, 1281, Usecode.Remove(Usecode.Length - 1, 1), 1, DateTime.Now);
                    gc.insert_date(orderNumber, ParcelNumber, 1281, Exemstatus.Remove(Exemstatus.Length - 1, 1), 1, DateTime.Now);

                    //title = title.TrimEnd('~');
                    //value = value.TrimEnd('~');
                    //db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + title.Remove(title.Length - 1).Trim() + "' where Id = '" + 1444 + "'");
                    //gc.insert_date(orderNumber, ParcelNumber, 1281, value.Remove(value.Length - 1).Trim(), 1, DateTime.Now);
                    //AssessmentTime = DateTime.Now.ToString("HH:mm:ss");


                    // driver = new PhantomJSDriver();
                    //Tax Information Details
                    driver.Navigate().GoToUrl("http://mpay.solanocounty.com/parcelSearch.asp");
                    //driver.FindElement(By.XPath("//*[@id='l1_2']/a")).Click();
                    //Thread.Sleep(5000);
                    //gc.CreatePdf(orderNumber, ParcelNumber, "Tax Page Open1", driver, "CA", "Solano");
                    //driver.FindElement(By.XPath("//*[@id='welcomediv']/table/tbody/tr[3]/td/table/tbody/tr[2]/td[1]/table/tbody/tr[4]/td")).Click();
                    //Thread.Sleep(9000);
                    //gc.CreatePdf(orderNumber, ParcelNumber, "Tax Page Open2", driver, "CA", "Solano");
                    //try
                    //{

                    //    IWebElement IAddressSearch1 = driver.FindElement(By.XPath("//*[@id='welcomediv']/table/tbody/tr[3]/td/table/tbody/tr[2]/td[1]/table/tbody/tr[4]/td"));
                    //    IJavaScriptExecutor js1 = driver as IJavaScriptExecutor;
                    //    js1.ExecuteScript("arguments[0].click();", IAddressSearch1);
                    //    Thread.Sleep(9000);
                    //    gc.CreatePdf(orderNumber, ParcelNumber, "Tax Page Open3", driver, "CA", "Solano");
                    //}
                    //catch { }
                    driver.FindElement(By.XPath("//*[@id='detaildiv']/div[1]/table/tbody/tr[3]/td[2]/input")).SendKeys(ParcelNumber);
                    gc.CreatePdf(orderNumber, parcelNumber, "Assessment search1", driver, "CA", "Solano");
                    driver.FindElement(By.XPath("//*[@id='detaildiv']/div[1]/table/tbody/tr[4]/td/input[1]")).Click();
                    Thread.Sleep(9000);
                    gc.CreatePdf(orderNumber, parcelNumber, "Secured and Supplement Data1", driver, "CA", "Solano");
                    ByVisibleElement(driver.FindElement(By.XPath("//*[@id='mobilebuttontable']/tbody/tr[2]/td[2]")));
                    gc.CreatePdf(orderNumber, parcelNumber, "Secured and Supplement Data2", driver, "CA", "Solano");
                    try
                    {

                        IWebElement IAddressSearch1 = driver.FindElement(By.XPath("//*[@id='detaildiv']/div[1]/table/tbody/tr[4]/td/input[1]"));
                        IJavaScriptExecutor js1 = driver as IJavaScriptExecutor;
                        js1.ExecuteScript("arguments[0].click();", IAddressSearch1);
                        Thread.Sleep(9000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Assessment search", driver, "CA", "Solano");
                    }
                    catch { }
                    //gc.CreatePdf(orderNumber, parcelNumber, "Assessment search result", driver, "CA", "Solano");


                    List<string> billinfo = new List<string>();
                    IWebElement Billsinfo2 = driver.FindElement(By.Id("sectable"));
                    IList<IWebElement> TRBillsinfo2 = Billsinfo2.FindElements(By.TagName("tr"));
                    IList<IWebElement> Aherftax;
                    int i = 0;
                    foreach (IWebElement row in TRBillsinfo2)
                    {
                        Aherftax = row.FindElements(By.TagName("td"));

                        if (Aherftax.Count != 0 && Aherftax.Count == 10 && !row.Text.Contains("Secured and Supplemental") && !row.Text.Contains("1st Installment 2nd Installment") && !row.Text.Contains("Bill Type"))
                        {
                            string SSBillType1 = Aherftax[3].Text;
                            string SSStatus1 = Aherftax[4].Text;
                            string SSDuedate1 = Aherftax[5].Text;
                            string SSDue1 = Aherftax[6].Text;
                            string SSStatus2 = Aherftax[7].Text;
                            string SSDuedate2 = Aherftax[8].Text;
                            string SSDue2 = Aherftax[9].Text;

                            string SecuredandsupplementDetails = SSBillType1.Trim() + "~" + SSStatus1.Trim() + "~" + SSDuedate1.Trim() + "~" + SSDue1.Trim() + "~" + SSStatus2.Trim() + "~" + SSDuedate2.Trim() + "~" + SSDue2.Trim();
                            gc.insert_date(orderNumber, ParcelNumber, 1314, SecuredandsupplementDetails, 1, DateTime.Now);
                        }
                        if (Aherftax.Count != 0 && !row.Text.Contains("Secured and Supplemental") && !row.Text.Contains("1st Installment 2nd Installment") && !row.Text.Contains("Bill Type"))
                        {
                            //gc.CreatePdf(orderNumber, parcelNumber, "Test search result2", driver, "CA", "Solano");
                            IWebElement value1 = Aherftax[0].FindElement(By.Id("radio"));
                            string addview = value1.GetAttribute("value");
                            billinfo.Add(addview);
                        }
                    }

                    foreach (string assessmentclick in billinfo)
                    {
                        try
                        {

                            driver.Navigate().GoToUrl("https://www.solanocounty.com/depts/ttcc/tax_collector/info.asp");
                            driver.FindElement(By.XPath("//*[@id='l1_2']/a")).Click();
                            Thread.Sleep(2000);
                            driver.FindElement(By.XPath("//*[@id='welcomediv']/table/tbody/tr[3]/td/table/tbody/tr[2]/td[1]/table/tbody/tr[4]/td")).Click();
                            Thread.Sleep(2000);

                            driver.FindElement(By.XPath("//*[@id='detaildiv']/div[1]/table/tbody/tr[3]/td[2]/input")).SendKeys(ParcelNumber);
                            driver.FindElement(By.XPath("//*[@id='detaildiv']/div[1]/table/tbody/tr[4]/td/input[1]")).Click();
                            //gc.CreatePdf(orderNumber, parcelNumber, "Assessment search result", driver, "CA", "Solano");
                            Thread.Sleep(2000);


                            IWebElement Billsinfo3 = driver.FindElement(By.Id("sectable"));
                            IList<IWebElement> TRBillsinfo3 = Billsinfo3.FindElements(By.TagName("tr"));
                            IList<IWebElement> Aherftax1;

                            foreach (IWebElement row1 in TRBillsinfo3)
                            {
                                Aherftax1 = row1.FindElements(By.TagName("td"));
                                if (Aherftax1.Count != 0 && !row1.Text.Contains("Secured and Supplemental") && !row1.Text.Contains("1st Installment 2nd Installment") && !row1.Text.Contains("Bill Type"))
                                {
                                    IWebElement value12 = Aherftax1[0].FindElement(By.Id("radio"));
                                    string addview1 = value12.GetAttribute("value");
                                    //*[@id="radio"]
                                    if (assessmentclick == addview1)
                                    {
                                        //gc.CreatePdf(orderNumber, parcelNumber, "Test search result1", driver, "CA", "Solano");
                                        // IWebElement IAddressSearch1 = driver.FindElement(By.XPath("//*[@id='welcomediv']/table/tbody/tr[3]/td/table/tbody/tr[2]/td[1]/table/tbody/tr[4]/td"));
                                        IJavaScriptExecutor js1 = driver as IJavaScriptExecutor;
                                        js1.ExecuteScript("arguments[0].click();", value12);
                                        //value12.Click();
                                        Thread.Sleep(2000);
                                    }
                                }
                            }
                            try
                            {//*[@id="mobilebuttontable"]/tbody/tr[2]/td[1]
                             //driver.FindElement(By.XPath("//*[@id='mobilebuttontable']/tbody/tr[2]/td[1]")).Click();
                                IWebElement IAddressSearch1 = driver.FindElement(By.XPath("//*[@id='mobilebuttontable']/tbody/tr[2]/td[1]/input"));
                                IJavaScriptExecutor js1 = driver as IJavaScriptExecutor;
                                js1.ExecuteScript("arguments[0].click();", IAddressSearch1);
                                Thread.Sleep(9000);
                                gc.CreatePdf(orderNumber, parcelNumber, "Tax Bill Details For Assessment1" + i, driver, "CA", "Solano");
                                ByVisibleElement(driver.FindElement(By.Id("buttondiv")));
                                gc.CreatePdf(orderNumber, parcelNumber, "Tax Bill Details For Assessment2" + i, driver, "CA", "Solano");
                                //Tax Information Details && Tax Bill Details
                                string Parcel_numbertax = "", PropertyLocation = "", PropertyType = "", Billed = "", Occurence = "", AssessmentYear = "", BillOrigin = "", TaxRollYear = "", SupEvent = "";
                                string Installments = "", Due = "", AmountPaid = "", BalanceDue = "", Status = "", DelinquentDate = "", Title = "";

                                Parcel_numbertax = driver.FindElement(By.XPath("//*[@id='titlediv']/div/div/font/b")).Text.Trim();

                                IWebElement TaxBillsvalueinfo2 = driver.FindElement(By.XPath("//*[@id='detaildiv']/div/table[1]"));
                                IList<IWebElement> TaxTRBillsvalueinfo2 = TaxBillsvalueinfo2.FindElements(By.TagName("tr"));
                                IList<IWebElement> TaxTDBillsvalueinfo2;
                                foreach (IWebElement Taxrow in TaxTRBillsvalueinfo2)
                                {
                                    TaxTDBillsvalueinfo2 = Taxrow.FindElements(By.TagName("td"));
                                    if (TaxTDBillsvalueinfo2.Count != 0 && TaxTDBillsvalueinfo2.Count == 2 && !Taxrow.Text.Contains("Property Location"))
                                    {
                                        PropertyLocation = TaxTDBillsvalueinfo2[0].Text;
                                        PropertyType = TaxTDBillsvalueinfo2[1].Text;
                                    }
                                }

                                IWebElement TaxBillsvalueinfo3 = driver.FindElement(By.XPath("//*[@id='detaildiv']/div/table[2]"));
                                IList<IWebElement> TaxTRBillsvalueinfo3 = TaxBillsvalueinfo3.FindElements(By.TagName("tr"));
                                IList<IWebElement> TaxTDBillsvalueinfo3;
                                foreach (IWebElement Taxrow1 in TaxTRBillsvalueinfo3)
                                {
                                    TaxTDBillsvalueinfo3 = Taxrow1.FindElements(By.TagName("td"));
                                    if (TaxTDBillsvalueinfo3.Count != 0 && TaxTDBillsvalueinfo3.Count == 6 && !Taxrow1.Text.Contains("Secured Taxbill Detail") && !Taxrow1.Text.Contains("Occurence"))
                                    {
                                        Billed = TaxTDBillsvalueinfo3[1].Text;
                                        //Occurence = TaxTDBillsvalueinfo3[1].Text;
                                        AssessmentYear = TaxTDBillsvalueinfo3[3].Text;
                                        //BillOrigin = TaxTDBillsvalueinfo3[3].Text;
                                        TaxRollYear = TaxTDBillsvalueinfo3[5].Text;
                                        //SupEvent = TaxTDBillsvalueinfo3[5].Text;
                                        //string TaxBillDetails = Title.Trim() + "~" + PropertyLocation.Trim() + "~" + PropertyType.Trim() + "~" + Billed.Trim() + "~" + "" + "~" + AssessmentYear.Trim() + "~" + "" + "~" + TaxRollYear.Trim() + "~" + "";
                                        //gc.insert_date(orderNumber, ParcelNumber, 1292, TaxBillDetails, 1, DateTime.Now);
                                    }
                                    if (TaxTDBillsvalueinfo3.Count != 0 && TaxTDBillsvalueinfo3.Count == 6 && !Taxrow1.Text.Contains("Secured Taxbill Detail") && Taxrow1.Text.Contains("Occurence"))
                                    {
                                        // Billed = TaxTDBillsvalueinfo3[1].Text;
                                        Occurence = TaxTDBillsvalueinfo3[1].Text;
                                        //AssessmentYear = TaxTDBillsvalueinfo3[3].Text;
                                        BillOrigin = TaxTDBillsvalueinfo3[3].Text;
                                        //TaxRollYear = TaxTDBillsvalueinfo3[5].Text;
                                        SupEvent = TaxTDBillsvalueinfo3[5].Text;
                                        //string TaxBillDetails =Title.Trim()+ "~" + PropertyLocation.Trim() + "~" + PropertyType.Trim() + "~" + "" + "~" + Occurence.Trim() + "~" + "" + "~" + BillOrigin.Trim() + "~" + "" + "~" + SupEvent.Trim();
                                        //gc.insert_date(orderNumber, ParcelNumber, 1292, TaxBillDetails, 1, DateTime.Now);
                                    }
                                    if (TaxTDBillsvalueinfo3.Count != 0 && TaxTDBillsvalueinfo3.Count == 1 && Taxrow1.Text.Contains("Secured Taxbill Detail"))
                                    {

                                        Title = TaxTDBillsvalueinfo3[0].Text.Replace("Taxbill Detail", "").Trim();

                                        //string TaxBillDetails = Title.Trim() + "~" + PropertyLocation.Trim() + "~" + PropertyType.Trim() + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "";
                                        //gc.insert_date(orderNumber, ParcelNumber, 1292, TaxBillDetails, 1, DateTime.Now);
                                    }
                                    if (TaxTDBillsvalueinfo3.Count != 0 && TaxTDBillsvalueinfo3.Count == 1 && Taxrow1.Text.Contains("Supplemental Taxbill Detail"))
                                    {

                                        Title = TaxTDBillsvalueinfo3[0].Text.Replace("Taxbill Detail", "").Trim();

                                        //string TaxBillDetails = Title.Trim() + "~" + PropertyLocation.Trim() + "~" + PropertyType.Trim() + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "";
                                        //gc.insert_date(orderNumber, ParcelNumber, 1292, TaxBillDetails, 1, DateTime.Now);
                                    }

                                }
                                string TaxBillDetails = Title.Trim() + "~" + PropertyLocation.Trim() + "~" + PropertyType.Trim() + "~" + Billed.Trim() + "~" + Occurence.Trim() + "~" + AssessmentYear.Trim() + "~" + BillOrigin.Trim() + "~" + TaxRollYear.Trim() + "~" + SupEvent.Trim();
                                gc.insert_date(orderNumber, ParcelNumber, 1292, TaxBillDetails, 1, DateTime.Now);

                                IWebElement TaxBillsvalueinfo4 = driver.FindElement(By.XPath("//*[@id='detaildiv']/div/table[3]"));
                                IList<IWebElement> TaxTRBillsvalueinfo4 = TaxBillsvalueinfo4.FindElements(By.TagName("tr"));
                                IList<IWebElement> TaxTDBillsvalueinfo4;
                                foreach (IWebElement Taxrow2 in TaxTRBillsvalueinfo4)
                                {
                                    TaxTDBillsvalueinfo4 = Taxrow2.FindElements(By.TagName("td"));
                                    if (TaxTDBillsvalueinfo4.Count != 0 && TaxTDBillsvalueinfo4.Count == 6 && !Taxrow2.Text.Contains("Instl"))
                                    {
                                        Installments = TaxTDBillsvalueinfo4[0].Text;
                                        Due = TaxTDBillsvalueinfo4[1].Text;
                                        AmountPaid = TaxTDBillsvalueinfo4[2].Text;
                                        BalanceDue = TaxTDBillsvalueinfo4[3].Text;
                                        Status = TaxTDBillsvalueinfo4[4].Text;
                                        DelinquentDate = TaxTDBillsvalueinfo4[5].Text;
                                        string TaxInstallmentDetails = Title.Trim() + "~" + Installments.Trim() + "~" + Due.Trim() + "~" + AmountPaid.Trim() + "~" + BalanceDue.Trim() + "~" + Status.Trim() + "~" + DelinquentDate.Trim();
                                        gc.insert_date(orderNumber, ParcelNumber, 1293, TaxInstallmentDetails, 1, DateTime.Now);
                                        Installments = ""; Due = ""; AmountPaid = ""; BalanceDue = ""; Status = ""; DelinquentDate = "";
                                    }
                                    if (TaxTDBillsvalueinfo4.Count != 0 && TaxTDBillsvalueinfo4.Count == 5 && Taxrow2.Text.Contains("Total"))
                                    {
                                        Installments = TaxTDBillsvalueinfo4[0].Text;
                                        Due = TaxTDBillsvalueinfo4[1].Text;
                                        AmountPaid = TaxTDBillsvalueinfo4[2].Text;
                                        BalanceDue = TaxTDBillsvalueinfo4[3].Text;
                                        Status = "";
                                        DelinquentDate = "";
                                        string TaxInstallmentDetails = Title.Trim() + "~" + Installments.Trim() + "~" + Due.Trim() + "~" + AmountPaid.Trim() + "~" + BalanceDue.Trim() + "~" + Status.Trim() + "~" + DelinquentDate.Trim();
                                        gc.insert_date(orderNumber, ParcelNumber, 1293, TaxInstallmentDetails, 1, DateTime.Now);
                                        Installments = ""; Due = ""; AmountPaid = ""; BalanceDue = ""; Status = ""; DelinquentDate = "";
                                    }
                                }

                                IWebElement TaxBillsvalueinfo5 = driver.FindElement(By.XPath("//*[@id='detaildiv']/div/table[4]"));
                                IList<IWebElement> TaxTRBillsvalueinfo5 = TaxBillsvalueinfo5.FindElements(By.TagName("tr"));
                                IList<IWebElement> TaxTDBillsvalueinfo5;
                                foreach (IWebElement Taxrow3 in TaxTRBillsvalueinfo5)
                                {
                                    TaxTDBillsvalueinfo5 = Taxrow3.FindElements(By.TagName("td"));
                                    if (TaxTDBillsvalueinfo5.Count != 0 && TaxTDBillsvalueinfo5.Count == 8 && !Taxrow3.Text.Contains("Tax Charge Detail") && !Taxrow3.Text.Contains("Fund"))
                                    {
                                        string Number = TaxTDBillsvalueinfo5[0].Text;
                                        string Fund = TaxTDBillsvalueinfo5[1].Text;
                                        string Rate = TaxTDBillsvalueinfo5[3].Text;
                                        string FirstInstallStatus = TaxTDBillsvalueinfo5[4].Text;
                                        string FirstInstallAmount = TaxTDBillsvalueinfo5[5].Text;
                                        string SecondInstallStatus = TaxTDBillsvalueinfo5[6].Text;
                                        string SecondInstallAmount = TaxTDBillsvalueinfo5[7].Text;

                                        string TaxChargesDetails = Title.Trim() + "~" + Number.Trim() + "~" + Fund.Trim() + "~" + Rate.Trim() + "~" + FirstInstallStatus.Trim() + "~" + FirstInstallAmount.Trim() + "~" + SecondInstallStatus.Trim() + "~" + SecondInstallAmount.Trim();
                                        gc.insert_date(orderNumber, ParcelNumber, 1294, TaxChargesDetails, 1, DateTime.Now);
                                        Installments = ""; Due = ""; AmountPaid = ""; BalanceDue = ""; Status = ""; DelinquentDate = "";
                                    }
                                    if (TaxTDBillsvalueinfo5.Count != 0 && TaxTDBillsvalueinfo5.Count == 4 && !Taxrow3.Text.Contains("Tax Charge Detail") && !Taxrow3.Text.Contains("Fund"))
                                    {
                                        string Number = "";
                                        string Fund = TaxTDBillsvalueinfo5[0].Text;
                                        string Rate = TaxTDBillsvalueinfo5[1].Text;
                                        string FirstInstallStatus = "";
                                        string FirstInstallAmount = TaxTDBillsvalueinfo5[2].Text;
                                        string SecondInstallStatus = "";
                                        string SecondInstallAmount = TaxTDBillsvalueinfo5[3].Text;
                                        string TaxChargesDetails = Title.Trim() + "~" + Number.Trim() + "~" + Fund.Trim() + "~" + Rate.Trim() + "~" + FirstInstallStatus.Trim() + "~" + FirstInstallAmount.Trim() + "~" + SecondInstallStatus.Trim() + "~" + SecondInstallAmount.Trim();
                                        gc.insert_date(orderNumber, ParcelNumber, 1294, TaxChargesDetails, 1, DateTime.Now);
                                        Installments = ""; Due = ""; AmountPaid = ""; BalanceDue = ""; Status = ""; DelinquentDate = "";
                                    }
                                }
                                IWebElement TaxBillsvalueinfo6 = driver.FindElement(By.Id("paymentDetail"));
                                IList<IWebElement> TaxTRBillsvalueinfo6 = TaxBillsvalueinfo6.FindElements(By.TagName("tr"));
                                IList<IWebElement> TaxTDBillsvalueinfo6;
                                foreach (IWebElement Taxrow4 in TaxTRBillsvalueinfo6)
                                {
                                    TaxTDBillsvalueinfo6 = Taxrow4.FindElements(By.TagName("td"));
                                    if (TaxTDBillsvalueinfo6.Count != 0 && TaxTDBillsvalueinfo6.Count == 4 && !Taxrow4.Text.Contains("Payment Detail") && !Taxrow4.Text.Contains("Payment Type"))
                                    {
                                        string Paymenttype = TaxTDBillsvalueinfo6[0].Text;
                                        string Installment = TaxTDBillsvalueinfo6[1].Text;
                                        string Posted = TaxTDBillsvalueinfo6[2].Text;
                                        string PaymentAmount = TaxTDBillsvalueinfo6[3].Text;
                                        string TaxChargesDetails = Title.Trim() + "~" + Paymenttype.Trim() + "~" + Installment.Trim() + "~" + Posted.Trim() + "~" + PaymentAmount.Trim();
                                        gc.insert_date(orderNumber, ParcelNumber, 1295, TaxChargesDetails, 1, DateTime.Now);
                                        Paymenttype = ""; Due = ""; Installment = ""; Posted = ""; PaymentAmount = "";
                                    }
                                    if (TaxTDBillsvalueinfo6.Count != 0 && TaxTDBillsvalueinfo6.Count == 1 && Taxrow4.Text.Contains("No records found"))
                                    {
                                        string Paymenttype = TaxTDBillsvalueinfo6[0].Text;
                                        string Installment = "";
                                        string Posted = "";
                                        string PaymentAmount = "";
                                        string TaxChargesDetails = Title.Trim() + "~" + Paymenttype.Trim() + "~" + Installment.Trim() + "~" + Posted.Trim() + "~" + PaymentAmount.Trim();
                                        gc.insert_date(orderNumber, ParcelNumber, 1295, TaxChargesDetails, 1, DateTime.Now);
                                        Paymenttype = ""; Due = ""; Installment = ""; Posted = ""; PaymentAmount = "";
                                    }
                                }
                                //Pdf Download

                                driver.FindElement(By.XPath("//*[@id='buttondiv']/div/a[1]")).Click();
                                Thread.Sleep(9000);
                                driver.SwitchTo().Window(driver.WindowHandles.Last());
                                gc.CreatePdf(orderNumber, parcelNumber, "Tax Bill Details PDF" + i, driver, "CA", "Solano");
                                i++;
                            }
                            catch { }

                        }
                        catch { }
                    }

                    //Delinquent Scenario Details
                    try
                    {
                        string Parcel_numbertax1 = "", PropertyLocation1 = "", PropertyType1 = "", Billed1 = "", Occurence1 = "", AssessmentYear1 = "", BillOrigin1 = "", TaxRollYear1 = "", SupEvent1 = "";
                        string TotalDueFor = "", Interest = "", ToReedem = "", Paymentplan = "", Taxtype = "";
                        driver.Navigate().GoToUrl("https://www.solanocounty.com/depts/ttcc/tax_collector/info.asp");
                        driver.FindElement(By.XPath("//*[@id='l1_2']/a")).Click();
                        driver.FindElement(By.XPath("//*[@id='welcomediv']/table/tbody/tr[3]/td/table/tbody/tr[2]/td[1]/table/tbody/tr[4]/td")).Click();
                        Thread.Sleep(2000);

                        driver.FindElement(By.XPath("//*[@id='detaildiv']/div[1]/table/tbody/tr[3]/td[2]/input")).SendKeys(ParcelNumber);
                        driver.FindElement(By.XPath("//*[@id='detaildiv']/div[1]/table/tbody/tr[4]/td/input[1]")).Click();
                        gc.CreatePdf(orderNumber, parcelNumber, "Delinquent Details PDF", driver, "CA", "Solano");
                        ByVisibleElement(driver.FindElement(By.XPath("//*[@id='mobilebuttontable']/tbody/tr[2]/td[1]/input")));
                        gc.CreatePdf(orderNumber, parcelNumber, "Delinquent Details PDF1", driver, "CA", "Solano");
                        Thread.Sleep(2000);
                        List<string> billinfo1 = new List<string>();
                        IWebElement Billsinfo21 = driver.FindElement(By.XPath("//*[@id='detaildiv']/div/form/table[3]"));
                        IList<IWebElement> TRBillsinfo21 = Billsinfo21.FindElements(By.TagName("tr"));
                        IList<IWebElement> Aherftax11;
                        int p = 0;
                        foreach (IWebElement row21 in TRBillsinfo21)
                        {
                            Aherftax11 = row21.FindElements(By.TagName("td"));
                            if (Aherftax11.Count != 0 && !row21.Text.Contains("Prior Year Delinquent Taxes") && !row21.Text.Contains("Bill Type"))
                            {
                                IWebElement value11 = Aherftax11[0].FindElement(By.Id("radio"));
                                string addview31 = value11.GetAttribute("value");
                                billinfo1.Add(addview31);

                            }
                            if (Aherftax11.Count != 0 && row21.Text.Contains("Prior Year Delinquent Taxes"))
                            {
                                string Deliquenttax = "This Property has unpaid prior year taxes.For more information, contact this office at ttccc@solanocounty.com or call";
                                string DelinquentTaxComments = Deliquenttax.Trim();
                                gc.insert_date(orderNumber, ParcelNumber, 1313, DelinquentTaxComments, 1, DateTime.Now);
                            }
                        }
                        foreach (string assessmentclick1 in billinfo1)
                        {
                            try
                            {

                                //driver.Navigate().GoToUrl("https://www.solanocounty.com/depts/ttcc/tax_collector/info.asp");
                                //driver.FindElement(By.XPath("//*[@id='l1_2']/a")).Click();
                                //driver.FindElement(By.XPath("//*[@id='welcomediv']/table/tbody/tr[3]/td/table/tbody/tr[2]/td[1]/table/tbody/tr[4]/td")).Click();
                                //Thread.Sleep(2000);

                                //driver.FindElement(By.XPath("//*[@id='detaildiv']/div[1]/table/tbody/tr[3]/td[2]/input")).SendKeys(ParcelNumber);
                                //driver.FindElement(By.XPath("//*[@id='detaildiv']/div[1]/table/tbody/tr[4]/td/input[1]")).Click();
                                ////gc.CreatePdf(orderNumber, parcelNumber, "Assessment search result", driver, "CA", "Solano");
                                //Thread.Sleep(2000);


                                IWebElement Billsinfo31 = driver.FindElement(By.XPath("//*[@id='detaildiv']/div/form/table[3]"));
                                IList<IWebElement> TRBillsinfo31 = Billsinfo31.FindElements(By.TagName("tr"));
                                IList<IWebElement> Aherftax31;

                                foreach (IWebElement row31 in TRBillsinfo31)
                                {
                                    Aherftax31 = row31.FindElements(By.TagName("td"));
                                    if (Aherftax31.Count != 0 && !row31.Text.Contains("Prior Year Delinquent Taxes") && !row31.Text.Contains("Bill Type"))
                                    {
                                        IWebElement value31 = Aherftax31[0].FindElement(By.Id("radio"));
                                        string addview31 = value31.GetAttribute("value");

                                        if (assessmentclick1 == addview31)
                                        {
                                            value31.Click();
                                        }
                                    }
                                }
                                driver.FindElement(By.XPath("//*[@id='mobilebuttontable']/tbody/tr[2]/td[1]/input")).Click();
                                Thread.Sleep(5000);
                                gc.CreatePdf(orderNumber, parcelNumber, "Tax Bill Details For Delinquent", driver, "CA", "Solano");

                                ByVisibleElement(driver.FindElement(By.XPath("//*[@id='mobilebuttontable']/tbody/tr[2]/td[2]/form/input")));
                                gc.CreatePdf(orderNumber, parcelNumber, "Tax Bill Details For Delinquent1", driver, "CA", "Solano");

                                Parcel_numbertax1 = driver.FindElement(By.XPath("//*[@id='titlediv']/div/div/font/b")).Text.Trim();

                                IWebElement TaxBillsvalueinfo21 = driver.FindElement(By.XPath("//*[@id='detaildiv']/div/table[2]/tbody"));
                                IList<IWebElement> TaxTRBillsvalueinfo21 = TaxBillsvalueinfo21.FindElements(By.TagName("tr"));
                                IList<IWebElement> TaxTDBillsvalueinfo21;
                                foreach (IWebElement Taxrow21 in TaxTRBillsvalueinfo21)
                                {
                                    TaxTDBillsvalueinfo21 = Taxrow21.FindElements(By.TagName("td"));
                                    if (TaxTDBillsvalueinfo21.Count != 0 && TaxTDBillsvalueinfo21.Count == 2 && !Taxrow21.Text.Contains("Property Location"))
                                    {
                                        PropertyLocation1 = TaxTDBillsvalueinfo21[0].Text;
                                        PropertyType1 = TaxTDBillsvalueinfo21[1].Text;
                                    }
                                }
                                //Redemption Pay Off
                                IWebElement TaxBillsvalueinfo41 = driver.FindElement(By.XPath("//*[@id='detaildiv']/div/table[4]/tbody"));
                                IList<IWebElement> TaxTRBillsvalueinfo41 = TaxBillsvalueinfo41.FindElements(By.TagName("tr"));
                                IList<IWebElement> TaxTDBillsvalueinfo41;
                                foreach (IWebElement Taxrow41 in TaxTRBillsvalueinfo41)
                                {
                                    TaxTDBillsvalueinfo41 = Taxrow41.FindElements(By.TagName("td"));

                                    if (TaxTDBillsvalueinfo41.Count != 0 && TaxTDBillsvalueinfo41.Count == 1 && Taxrow41.Text.Contains("Redemption Pay Off") && !Taxrow41.Text.Contains("Total Due For"))
                                    {
                                        Taxtype = TaxTDBillsvalueinfo41[0].Text;
                                    }
                                    if (TaxTDBillsvalueinfo41.Count != 0 && TaxTDBillsvalueinfo41.Count == 4 && !Taxrow41.Text.Contains("Redemption Pay Off") && !Taxrow41.Text.Contains("Total Due For"))
                                    {
                                        TotalDueFor = TaxTDBillsvalueinfo41[0].Text;
                                        Interest = TaxTDBillsvalueinfo41[1].Text;
                                        ToReedem = TaxTDBillsvalueinfo41[2].Text;
                                        Paymentplan = TaxTDBillsvalueinfo41[3].Text;
                                        string TaxBillDelinquentDetails = Taxtype + "~" + PropertyLocation1 + "~" + PropertyType1 + "~" + TotalDueFor.Trim() + "~" + Interest.Trim() + "~" + ToReedem.Trim() + "~" + Paymentplan.Trim();
                                        gc.insert_date(orderNumber, ParcelNumber, 1310, TaxBillDelinquentDetails, 1, DateTime.Now);

                                    }
                                }



                                //Prior Year Delinquent Details
                                string Title2 = "";
                                IWebElement TaxBillsvalueinfo51 = driver.FindElement(By.XPath("//*[@id='detaildiv']/div/table[5]/tbody"));
                                IList<IWebElement> TaxTRBillsvalueinfo51 = TaxBillsvalueinfo51.FindElements(By.TagName("tr"));
                                IList<IWebElement> TaxTDBillsvalueinfo51;
                                foreach (IWebElement Taxrow51 in TaxTRBillsvalueinfo51)
                                {
                                    TaxTDBillsvalueinfo51 = Taxrow51.FindElements(By.TagName("td"));
                                    if (TaxTDBillsvalueinfo51.Count != 0 && TaxTDBillsvalueinfo51.Count == 1 && !Taxrow51.Text.Contains("Bill Type"))
                                    {
                                        Title2 = TaxTDBillsvalueinfo51[0].Text;

                                    }
                                    if (TaxTDBillsvalueinfo51.Count != 0 && TaxTDBillsvalueinfo51.Count == 4 && !Taxrow51.Text.Contains("Bill Type"))
                                    {
                                        string BillType1 = TaxTDBillsvalueinfo51[0].Text;
                                        string Status1 = TaxTDBillsvalueinfo51[1].Text;
                                        string Paidon = TaxTDBillsvalueinfo51[2].Text;
                                        string AmountPaid = TaxTDBillsvalueinfo51[3].Text;

                                        string TaxPriorYearDelinquentDetails = Title2.Trim() + "~" + BillType1.Trim() + "~" + Status1.Trim() + "~" + Paidon.Trim() + "~" + AmountPaid.Trim();
                                        gc.insert_date(orderNumber, ParcelNumber, 1311, TaxPriorYearDelinquentDetails, 1, DateTime.Now);
                                    }
                                }

                                //Delinquent Tax Year Details
                                string title3 = "";
                                driver.FindElement(By.XPath("//*[@id='mobilebuttontable']/tbody/tr[2]/td[2]/form/input")).Click();
                                Thread.Sleep(2000);
                                gc.CreatePdf(orderNumber, parcelNumber, "Delinquent Tax Year Details", driver, "CA", "Solano");
                                IWebElement TaxBillsvalueinfo61 = driver.FindElement(By.XPath("//*[@id='detaildiv']/div/table[3]/tbody"));
                                IList<IWebElement> TaxTRBillsvalueinfo61 = TaxBillsvalueinfo61.FindElements(By.TagName("tr"));
                                IList<IWebElement> TaxTDBillsvalueinfo61;
                                foreach (IWebElement Taxrow61 in TaxTRBillsvalueinfo61)
                                {
                                    TaxTDBillsvalueinfo61 = Taxrow61.FindElements(By.TagName("td"));

                                    if (TaxTDBillsvalueinfo61.Count != 0 && TaxTDBillsvalueinfo61.Count == 1)
                                    {
                                        title3 = TaxTDBillsvalueinfo61[0].Text;

                                    }
                                    if (TaxTDBillsvalueinfo61.Count != 0 && TaxTDBillsvalueinfo61.Count == 7 && !Taxrow61.Text.Contains("Delinquent Tax Year Detail") && !Taxrow61.Text.Contains("Tax Year"))
                                    {
                                        string TaxYear = TaxTDBillsvalueinfo61[0].Text;
                                        string BillType = TaxTDBillsvalueinfo61[1].Text;
                                        string AsmntYear = TaxTDBillsvalueinfo61[2].Text;
                                        string Installment = TaxTDBillsvalueinfo61[3].Text;
                                        string Base = TaxTDBillsvalueinfo61[4].Text;
                                        string Penalty = TaxTDBillsvalueinfo61[5].Text;
                                        string Cost = TaxTDBillsvalueinfo61[6].Text;

                                        string DelinquentTaxYearDetails = title3.Trim() + "~" + TaxYear.Trim() + "~" + BillType.Trim() + "~" + AsmntYear.Trim() + "~" + Installment.Trim() + "~" + Base.Trim() + "~" + Penalty.Trim() + "~" + Cost.Trim();
                                        gc.insert_date(orderNumber, ParcelNumber, 1312, DelinquentTaxYearDetails, 1, DateTime.Now);
                                    }
                                }

                            }
                            catch { }
                        }
                    }
                    catch { }

                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "CA", "Solano", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);
                    driver.Quit();
                    //driver.Quit();
                    gc.mergpdf(orderNumber, "CA", "Solano");
                    return "Data Inserted Successfully";
                }
                catch (Exception ex)
                {
                    driver.Quit();
                    //driver.Quit();
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