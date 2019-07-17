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
    public class WebDriver_CobbGA
    {
        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        int searchcount;
        public string FTP_Cobb(string streetnumber, string direction, string streetname, string streettype, string unitnumber, string ownername, string parcelNumber, string searchType, string orderNumber, string directParcel)
        {
            string address = "";
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            if (direction != "")
            {
                address = streetnumber + " " + direction + " " + streetname + " " + streettype + " " + unitnumber;
            }
            else
            {
                address = streetnumber + " " + streetname + " " + streettype + " " + unitnumber;
            }
            List<string> strTaxRealestate = new List<string>();
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            // driver = new PhantomJSDriver();
            //driver = new ChromeDriver();
            using (driver = new PhantomJSDriver()) //ChromeDriver
            {
                try
                {

                    if (searchType == "titleflex")
                    {
                        address = streetnumber + " " + direction + " " + streetname + " " + streettype;
                        gc.TitleFlexSearch(orderNumber, "", "", address, "GA", "Cobb");
                        if (HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes")
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_CobbGA"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }

                    StartTime = DateTime.Now.ToString("HH:mm:ss");
                    driver.Navigate().GoToUrl("https://www.cobbassessor.org/cobbga/search/commonsearch.aspx?mode=address");


                    if (searchType == "address")
                    {
                        driver.FindElement(By.Id("inpNumber")).SendKeys(streetnumber);
                        driver.FindElement(By.Id("inpStreet")).SendKeys(streetname.ToUpper());
                        driver.FindElement(By.Id("Select1")).SendKeys(streettype);
                        gc.CreatePdf_WOP(orderNumber, "AddressInput Before", driver, "GA", "Cobb");
                        driver.FindElement(By.Id("btSearch")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);
                        gc.CreatePdf_WOP(orderNumber, "AddressInput After", driver, "GA", "Cobb");

                        ////Multiparcel
                        try
                        {
                            //int Count = 0;
                            IWebElement Multiaddresstable = driver.FindElement(By.Id("searchResults"));
                            IList<IWebElement> multiaddressrow = Multiaddresstable.FindElements(By.TagName("tr"));
                            IList<IWebElement> Multiaddressid;
                            List<string> searchlist = new List<string>();
                            foreach (IWebElement Multiaddress in multiaddressrow)
                            {
                                Multiaddressid = Multiaddress.FindElements(By.TagName("td"));
                                if (Multiaddressid.Count != 0 && !Multiaddress.Text.Contains("Parcel") && Multiaddressid.Count == 4 && !Multiaddressid[1].Text.Contains("P") & Multiaddress.Text.Trim().Contains(address.ToUpper().Trim()))
                                {
                                    string Multiparcelnumber = Multiaddressid[1].Text;
                                    string OWnername = Multiaddressid[2].Text;
                                    string Address1 = Multiaddressid[3].Text;
                                    searchlist.Add(Multiaddressid[1].Text);
                                    searchcount = searchlist.Count;
                                }
                            }
                            if (searchcount == 1)
                            {
                                IWebElement element2 = driver.FindElement(By.XPath("//*[@id='searchResults']/tbody/tr[3]/td[2]"));
                                IJavaScriptExecutor js2 = driver as IJavaScriptExecutor;
                                js2.ExecuteScript("arguments[0].click();", element2);
                                Thread.Sleep(3000);
                            }
                            else
                            {
                                if (searchcount > 1 && searchcount < 25)
                                {
                                    multiparcel(orderNumber, address);
                                    gc.CreatePdf_WOP(orderNumber, "Multi Address search Result", driver, "GA", "Cobb");
                                    HttpContext.Current.Session["multiparcel_Cobb"] = "Yes";
                                    driver.Quit();
                                    gc.mergpdf(orderNumber, "GA", "Cobb");
                                    return "MultiParcel";
                                }
                                if (searchcount > 25)
                                {
                                    HttpContext.Current.Session["multiParcel_Cobb_Multicount"] = "Maximum";
                                    driver.Quit();
                                    return "Maximum";
                                }
                            }
                        }
                        catch { }
                        try
                        {
                            //No Data Found
                            string nodata = driver.FindElement(By.XPath("//*[@id='frmMain']/table/tbody/tr/td/div/div/table[2]/tbody/tr/td/table/tbody/tr[3]/td/center/table[1]/tbody/tr[1]/td/center/div/p/large/large")).Text;
                            if (nodata.Contains("Your search did not find any records"))
                            {
                                HttpContext.Current.Session["Nodata_CobbGA"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }
                    if (searchType == "parcel")
                    {
                        driver.FindElement(By.XPath("//*[@id='secondarytopmenu']/ul/li[3]/a")).Click();
                        Thread.Sleep(2000);
                        driver.FindElement(By.Id("inpParid")).SendKeys(parcelNumber);
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcelNumber, "ParcelIpuntPassedBefore", driver, "GA", "Cobb");
                        driver.FindElement(By.Id("btSearch")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcelNumber, "ParcelIpuntPassedAfter", driver, "GA", "Cobb");
                        try
                        {
                            //No Data Found
                            string nodata = driver.FindElement(By.XPath("//*[@id='frmMain']/table/tbody/tr/td/div/div/table[2]/tbody/tr/td/table/tbody/tr[3]/td/center/table[1]/tbody/tr[1]/td/center/div/p/large/large")).Text;
                            if (nodata.Contains("Your search did not find any records"))
                            {
                                HttpContext.Current.Session["Nodata_CobbGA"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }
                    if (searchType == "ownername")
                    {
                        driver.FindElement(By.XPath("//*[@id='secondarytopmenu']/ul/li[1]/a")).Click();
                        Thread.Sleep(2000);
                        driver.FindElement(By.Id("inpOwner")).SendKeys(ownername);
                        gc.CreatePdf_WOP(orderNumber, "OwnernameIpuntPassedBefore", driver, "GA", "Cobb");
                        driver.FindElement(By.Id("btSearch")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "OwnernameIpuntPassedAfter", driver, "GA", "Cobb");

                        ////Multiparcel
                        try
                        {
                            //int Count = 0;
                            IWebElement Multiaddresstable = driver.FindElement(By.Id("searchResults"));
                            IList<IWebElement> multiaddressrow = Multiaddresstable.FindElements(By.TagName("tr"));
                            IList<IWebElement> Multiaddressid;
                            List<string> searchlist = new List<string>();
                            foreach (IWebElement Multiaddress in multiaddressrow)
                            {
                                Multiaddressid = Multiaddress.FindElements(By.TagName("td"));
                                if (Multiaddressid.Count != 0 && !Multiaddress.Text.Contains("Parcel") && Multiaddressid.Count == 4 && !Multiaddressid[1].Text.Contains("P") & Multiaddress.Text.Trim().Contains(address.ToUpper().Trim()))
                                {
                                    string Multiparcelnumber = Multiaddressid[1].Text;
                                    string OWnername = Multiaddressid[2].Text;
                                    string Address1 = Multiaddressid[3].Text;
                                    searchlist.Add(Multiaddressid[1].Text);
                                    searchcount = searchlist.Count;
                                }
                            }
                            if (searchcount == 1)
                            {
                                IWebElement element2 = driver.FindElement(By.XPath("//*[@id='searchResults']/tbody/tr[3]/td[2]"));
                                IJavaScriptExecutor js2 = driver as IJavaScriptExecutor;
                                js2.ExecuteScript("arguments[0].click();", element2);
                                Thread.Sleep(1000);
                            }
                            else
                            {
                                if (searchcount > 1 && searchcount < 25)
                                {
                                    multiparcel(orderNumber, address);
                                    gc.CreatePdf_WOP(orderNumber, "Multi Address search Result", driver, "GA", "Cobb");
                                    HttpContext.Current.Session["multiparcel_Cobb"] = "Yes";
                                    driver.Quit();
                                    gc.mergpdf(orderNumber, "GA", "Cobb");
                                    return "MultiParcel";
                                }
                                if (searchcount > 25)
                                {
                                    HttpContext.Current.Session["multiParcel_Cobb_Multicount"] = "Maximum";
                                    driver.Quit();
                                    return "Maximum";
                                }
                            }
                            //if (multiaddressrow.Count < 4)
                            //{
                            //    driver.FindElement(By.XPath("//*[@id='searchResults']/tbody/tr[3]/td[2]")).Click();
                            //    Thread.Sleep(2000);
                            //}

                            //if (multiaddressrow.Count > 3 && multiaddressrow.Count < 26)
                            //{
                            //    HttpContext.Current.Session["multiparcel_Cobb"] = "Yes";
                            //    driver.Quit();
                            //    return "MultiParcel";
                            //}
                            //if (multiaddressrow.Count > 3 && multiaddressrow.Count > 25)
                            //{
                            //    HttpContext.Current.Session["multiParcel_Cobb_Multicount"] = "Maximum";
                            //    driver.Quit();
                            //    return "Maximum";
                            //}                           
                        }
                        catch { }
                        try
                        {
                            //No Data Found
                            string nodata = driver.FindElement(By.XPath("//*[@id='frmMain']/table/tbody/tr/td/div/div/table[2]/tbody/tr/td/table/tbody/tr[3]/td/center/table[1]/tbody/tr[1]/td/center/div/p/large/large")).Text;
                            if (nodata.Contains("Your search did not find any records"))
                            {
                                HttpContext.Current.Session["Nodata_CobbGA"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }


                    //Property Details


                    string ParcelNumber = "", PropertyClass = "", PropertyAddress = "", Neighborhood = "", Owner = "", TaxDistrict = "", SubdivisionNumber = "", YearBuilt = "";
                    try
                    {
                        driver.FindElement(By.XPath("//*[@id='searchResults']/tbody/tr[3]/td[2]")).Click();
                        Thread.Sleep(2000);
                    }
                    catch
                    { }


                    ParcelNumber = driver.FindElement(By.XPath("//*[@id='datalet_header_row']/td/table/tbody/tr[1]/td[1]")).Text.Replace("PARID:", "");
                    gc.CreatePdf(orderNumber, ParcelNumber, "Property Details Pdf", driver, "GA", "Cobb");
                    IWebElement Bigdata4 = driver.FindElement(By.Id("Parcel"));
                    IList<IWebElement> TRBigdata4 = Bigdata4.FindElements(By.TagName("tr"));
                    IList<IWebElement> TDBigdata4;
                    foreach (IWebElement row4 in TRBigdata4)
                    {
                        TDBigdata4 = row4.FindElements(By.TagName("td"));

                        if (TDBigdata4.Count != 0 && TDBigdata4.Count == 2 && TDBigdata4[0].Text.Trim() != "" && (row4.Text.Contains("Address")))
                        {
                            PropertyAddress = TDBigdata4[1].Text;
                        }
                        if (TDBigdata4.Count != 0 && TDBigdata4.Count == 2 && TDBigdata4[0].Text.Trim() != "" && (row4.Text.Contains("Neighborhood")))
                        {
                            Neighborhood = TDBigdata4[1].Text;
                        }
                    }

                    IWebElement Bigdata5 = driver.FindElement(By.Id("Owner"));
                    IList<IWebElement> TRBigdata5 = Bigdata5.FindElements(By.TagName("tr"));
                    IList<IWebElement> TDBigdata5;
                    foreach (IWebElement row5 in TRBigdata5)
                    {
                        TDBigdata5 = row5.FindElements(By.TagName("td"));

                        if (TDBigdata5.Count != 0 && TDBigdata5.Count == 2 && TDBigdata5[0].Text.Trim() != "" && (row5.Text.Contains("Owner")))
                        {
                            Owner = TDBigdata5[1].Text;
                        }
                    }
                    IWebElement Bigdata6 = driver.FindElement(By.Id("Legal"));
                    IList<IWebElement> TRBigdata6 = Bigdata6.FindElements(By.TagName("tr"));
                    IList<IWebElement> TDBigdata6;
                    foreach (IWebElement row6 in TRBigdata6)
                    {
                        TDBigdata6 = row6.FindElements(By.TagName("td"));

                        if (TDBigdata6.Count != 0 && TDBigdata6.Count == 2 && TDBigdata6[0].Text.Trim() != "" && row6.Text.Contains("Tax District"))
                        {
                            TaxDistrict = TDBigdata6[1].Text;
                        }
                        if (TDBigdata6.Count != 0 && TDBigdata6.Count == 2 && TDBigdata6[0].Text.Trim() != "" && row6.Text.Contains("Subdivision Number"))
                        {
                            SubdivisionNumber = TDBigdata6[1].Text;
                        }

                    }
                    //Residential ,Commercial ,History,Appeals ,Permits,Sales information 

                    driver.FindElement(By.XPath("//*[@id='sidemenu']/li[3]/a")).Click();
                    Thread.Sleep(1000);
                    gc.CreatePdf(orderNumber, ParcelNumber, "Residential", driver, "GA", "Cobb");

                    driver.FindElement(By.XPath("//*[@id='sidemenu']/li[4]/a")).Click();
                    Thread.Sleep(1000);
                    gc.CreatePdf(orderNumber, ParcelNumber, "Commercial", driver, "GA", "Cobb");

                    driver.FindElement(By.XPath("//*[@id='sidemenu']/li[5]/a")).Click();
                    Thread.Sleep(1000);
                    gc.CreatePdf(orderNumber, ParcelNumber, "Historypdf", driver, "GA", "Cobb");

                    driver.FindElement(By.XPath("//*[@id='sidemenu']/li[6]/a")).Click();
                    Thread.Sleep(1000);
                    gc.CreatePdf(orderNumber, ParcelNumber, "Appeals", driver, "GA", "Cobb");

                    driver.FindElement(By.XPath("//*[@id='sidemenu']/li[7]/a")).Click();
                    Thread.Sleep(1000);
                    gc.CreatePdf(orderNumber, ParcelNumber, "Permits", driver, "GA", "Cobb");


                    driver.FindElement(By.XPath("//*[@id='sidemenu']/li[8]/a")).Click();
                    Thread.Sleep(1000);
                    gc.CreatePdf(orderNumber, ParcelNumber, "Sales Information", driver, "GA", "Cobb");
                    ByVisibleElement(driver.FindElement(By.XPath("//*[@id='Sales']/tbody/tr[1]/td[7]")));
                    gc.CreatePdf(orderNumber, ParcelNumber, "Sales Information1", driver, "GA", "Cobb");
                    //Year Built
                    try
                    {
                        driver.FindElement(By.XPath("//*[@id='sidemenu']/li[3]/a")).Click();
                        Thread.Sleep(1000);

                        IWebElement Bigdata8 = driver.FindElement(By.Id("Building"));
                        IList<IWebElement> TRBigdata8 = Bigdata8.FindElements(By.TagName("tr"));
                        IList<IWebElement> TDBigdata8;
                        foreach (IWebElement row8 in TRBigdata8)
                        {
                            TDBigdata8 = row8.FindElements(By.TagName("td"));

                            if (TDBigdata8.Count != 0 && TDBigdata8.Count == 2 && TDBigdata8[0].Text.Trim() != "" && (row8.Text.Contains("Year Built")))
                            {
                                YearBuilt = TDBigdata8[1].Text;
                            }
                        }
                    }
                    catch { }
                    try
                    {
                        driver.FindElement(By.XPath("//*[@id='sidemenu']/li[4]/a")).Click();
                        Thread.Sleep(2000);

                        IWebElement Bigdata9 = driver.FindElement(By.Id("Commercial"));
                        IList<IWebElement> TRBigdata9 = Bigdata9.FindElements(By.TagName("tr"));
                        IList<IWebElement> TDBigdata9;
                        foreach (IWebElement row9 in TRBigdata9)
                        {
                            TDBigdata9 = row9.FindElements(By.TagName("td"));

                            if (TDBigdata9.Count != 0 && TDBigdata9.Count == 2 && TDBigdata9[0].Text.Trim() != "" && (row9.Text.Contains("Year Built")))
                            {
                                YearBuilt = TDBigdata9[1].Text;
                            }
                        }
                    }
                    catch { }
                    //History Click Property class Details
                    try
                    {
                        driver.FindElement(By.XPath("//*[@id='sidemenu']/li[5]/a")).Click();
                        Thread.Sleep(2000);

                        PropertyClass = driver.FindElement(By.XPath("//*[@id='History']/tbody/tr[2]/td[2]")).Text.Trim();
                    }
                    catch { }

                    string PropertyDetails1 = PropertyClass.Trim() + "~" + PropertyAddress.Trim() + "~" + Neighborhood.Trim() + "~" + Owner.Trim() + "~" + TaxDistrict.Trim() + "~" + SubdivisionNumber.Trim() + "~" + YearBuilt.Trim();
                    gc.insert_date(orderNumber, ParcelNumber, 1384, PropertyDetails1, 1, DateTime.Now);


                    //Assessment Details

                    try
                    {
                        driver.FindElement(By.XPath("//*[@id='sidemenu']/li[2]/a")).Click();
                        Thread.Sleep(2000);
                    }
                    catch { }
                    gc.CreatePdf(orderNumber, ParcelNumber, "Assessment Details Pdf", driver, "GA", "Cobb");

                    string Taxyear = "", Landvalue = "", Buildingvalue = "", TotalAssessedvalue = "";

                    Taxyear = driver.FindElement(By.XPath("//*[@id='datalet_header_row']/td/table/tbody/tr[1]/td[3]")).Text.Trim().Replace("TAX YEAR:", "");

                    IWebElement Bigdata7 = driver.FindElement(By.Id("Assessed Value"));
                    IList<IWebElement> TRBigdata7 = Bigdata7.FindElements(By.TagName("tr"));
                    IList<IWebElement> TDBigdata7;
                    foreach (IWebElement row7 in TRBigdata7)
                    {
                        TDBigdata7 = row7.FindElements(By.TagName("td"));

                        if (TDBigdata7.Count != 0 && TDBigdata7.Count == 2 && TDBigdata7[0].Text.Trim() != "" && row7.Text.Contains("Land Value"))
                        {
                            Landvalue = TDBigdata7[1].Text;
                        }
                        if (TDBigdata7.Count != 0 && TDBigdata7.Count == 2 && TDBigdata7[0].Text.Trim() != "" && row7.Text.Contains("Building"))
                        {
                            Buildingvalue = TDBigdata7[1].Text;
                        }
                        if (TDBigdata7.Count != 0 && TDBigdata7.Count == 2 && TDBigdata7[0].Text.Trim() != "" && row7.Text.Contains("Total Assessed"))
                        {
                            TotalAssessedvalue = TDBigdata7[1].Text;
                        }

                    }
                    string Assessmentdetails = Taxyear.Trim() + "~" + Landvalue.Trim() + "~" + Buildingvalue.Trim() + "~" + TotalAssessedvalue.Trim();
                    gc.insert_date(orderNumber, ParcelNumber, 1385, Assessmentdetails, 1, DateTime.Now);

                    //Tax Information Details
                    driver.Navigate().GoToUrl("https://www.cobbtax.org/taxes#/WildfireSearch");
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div[1]/div/div/div[3]/button[2]")).SendKeys(Keys.Enter);
                        Thread.Sleep(5000);
                    }
                    catch { }

                    driver.FindElement(By.XPath("//*[@id='searchBox']")).SendKeys(ParcelNumber);
                    driver.FindElement(By.XPath("//*[@id='searchForm']/div[1]/div/span/button")).Click();
                    Thread.Sleep(1000);
                    gc.CreatePdf(orderNumber, ParcelNumber, "Tax Information Details Pdf", driver, "GA", "Cobb");

                    //Tax Payment Receipt Details Table
                    string Paymentdetails = "", TaxOwnerName = "", TaxYear = "", TaxParcelid = "", Address = "", BillType = "", Paid = "", Due = "";

                    try
                    {
                        IWebElement PaymentTB = driver.FindElement(By.XPath("//*[@id='secContent']/div[2]/div/div[3]/div[2]/table"));
                        IList<IWebElement> PaymentTR = PaymentTB.FindElements(By.TagName("tr"));
                        IList<IWebElement> PaymentTD;

                        foreach (IWebElement Payment in PaymentTR)
                        {
                            PaymentTD = Payment.FindElements(By.TagName("td"));
                            if (PaymentTD.Count != 0 && !Payment.Text.Contains("Owner Name"))
                            {
                                TaxOwnerName = PaymentTD[0].Text;
                                TaxYear = PaymentTD[1].Text;
                                TaxParcelid = PaymentTD[2].Text;
                                Address = PaymentTD[3].Text;
                                BillType = PaymentTD[4].Text;
                                Paid = PaymentTD[5].Text;
                                Due = PaymentTD[6].Text;


                                Paymentdetails = TaxOwnerName + "~" + TaxYear + "~" + TaxParcelid + "~" + Address + "~" + BillType + "~" + Paid + "~" + Due;
                                gc.insert_date(orderNumber, ParcelNumber, 1386, Paymentdetails, 1, DateTime.Now);
                            }
                        }
                    }
                    catch { }
                    //Tax Bill Details
                    string PropertyNumber = "", TaxDistrict1 = "", HomestedExemption = "";
                    try
                    {
                        string name = "", Taxyear1 = "", bill_no = "", amount = "", Del_details = "", delinaccount = "", delinparcelid = "";

                        //Tax Info Details
                        IWebElement Receipttable = driver.FindElement(By.XPath("//*[@id='secContent']/div[2]/div/div[3]/div[2]/table"));
                        IList<IWebElement> ReceipttableRow = Receipttable.FindElements(By.TagName("tr"));
                        int rowcount = ReceipttableRow.Count;

                        for (int p = 1; p <= rowcount; p++)
                        {
                            if (p < 4)
                            {////*[@id="secContent"]/div[2]/div/div[3]/div[2]/table/tbody/tr[" + p + "]/td[10]/button

                                driver.FindElement(By.XPath("//*[@id='secContent']/div[2]/div/div[3]/div[2]/table/tbody/tr[" + p + "]/td[10]/button")).Click();
                                try
                                {
                                    driver.FindElement(By.XPath("/html/body/div[1]/div/div/div[2]/button[2]")).Click();
                                    Thread.Sleep(2000);
                                }
                                catch
                                { }
                                gc.CreatePdf(orderNumber, ParcelNumber, "Overview & Pay Pdf" + p, driver, "GA", "Cobb");

                                Thread.Sleep(3000);
                                //View Delinquent Details
                                try
                                {
                                    driver.FindElement(By.XPath("//*[@id='avalon']/div/div/div/div[1]/div[1]/div[3]/button[4]")).Click();
                                    IWebElement DeliquentTB = driver.FindElement(By.XPath("/html/body/div[1]/div/div/div[1]/table/tbody"));
                                    IList<IWebElement> DeliquentTR = DeliquentTB.FindElements(By.TagName("tr"));
                                    IList<IWebElement> DeliquentTD;

                                    foreach (IWebElement Deliquent in DeliquentTR)
                                    {
                                        DeliquentTD = Deliquent.FindElements(By.TagName("td"));
                                        if (DeliquentTD.Count != 0)
                                        {
                                            name = DeliquentTD[0].Text.Trim();
                                            Taxyear1 = DeliquentTD[1].Text.Trim();
                                            bill_no = DeliquentTD[2].Text.Trim();
                                            delinaccount = DeliquentTD[3].Text.Trim();
                                            delinparcelid = DeliquentTD[4].Text.Trim();
                                            amount = DeliquentTD[6].Text.Trim();

                                            Del_details = name + "~" + Taxyear1 + "~" + bill_no + "~" + delinaccount + "~" + delinparcelid + "~" + amount;
                                            gc.CreatePdf(orderNumber, ParcelNumber, "Deliquent Details" + delinaccount, driver, "GA", "Paulding");
                                            gc.insert_date(orderNumber, ParcelNumber, 895, Del_details, 1, DateTime.Now);
                                            name = ""; Taxyear = ""; bill_no = ""; amount = ""; delinaccount = ""; delinparcelid = "";
                                        }
                                    }
                                }
                                catch
                                { }
                                try
                                {
                                    IWebElement DeliquentfootTB = driver.FindElement(By.XPath("/html/body/div[1]/div/div/div[1]/table/tfoot"));
                                    IList<IWebElement> DeliquentfootTR = DeliquentfootTB.FindElements(By.TagName("tr"));
                                    IList<IWebElement> DeliquentfootTD;

                                    foreach (IWebElement Deliquentfoot in DeliquentfootTR)
                                    {
                                        DeliquentfootTD = Deliquentfoot.FindElements(By.TagName("th"));
                                        if (DeliquentfootTD.Count != 0)
                                        {
                                            string bill_no1 = DeliquentfootTD[0].Text;
                                            string amount1 = DeliquentfootTD[2].Text;

                                            string Del_details1 = "" + "~" + "" + "~" + "" + "~" + "" + "~" + bill_no1 + "~" + amount1;
                                            gc.insert_date(orderNumber, ParcelNumber, 895, Del_details1, 1, DateTime.Now);
                                        }
                                    }
                                }
                                catch
                                { }
                                try
                                {
                                    driver.FindElement(By.XPath("//*[@id='secContent']/div[2]/div/div/ul/li[1]/a/div")).Click();
                                    Thread.Sleep(2000);
                                }
                                catch
                                { }
                                string Taxinfownername = "", Taxaddress1 = "", Account = "", RecordType = "", BillNumber = "", BillTaxYear = "", taxown = "";
                                string TaxYearbill = "", TaxDueDate = "";
                                //Tax information details
                                try
                                {
                                    //Owner information
                                    IWebElement TaxTB = driver.FindElement(By.XPath("//*[@id='secContent']/div[2]/div/div/div/div[1]/div[1]/div[1]"));
                                    string[] splitowner = TaxTB.Text.Split('\r');
                                    Taxinfownername = splitowner[1].Replace("\n", "");
                                    //Payment Information
                                    string DueDate = "", PaymentStatus = "", LastPaymentDate = "", TotalAmountPaid = "", TotalDuePayment = "";
                                    IWebElement TaxTB4 = driver.FindElement(By.XPath("//*[@id='secContent']/div[2]/div/div/div/div[1]/div[1]/div[2]/table"));
                                    IList<IWebElement> TaxTR4 = TaxTB4.FindElements(By.TagName("tr"));
                                    IList<IWebElement> TaxTD4;
                                    foreach (IWebElement Tax in TaxTR4)
                                    {
                                        TaxTD4 = Tax.FindElements(By.TagName("td"));
                                        if (TaxTD4.Count != 0 && Tax.Text != "")
                                        {

                                            if (Tax.Text.Contains("Status"))
                                            {
                                                PaymentStatus = TaxTD4[1].Text + " ";
                                            }
                                            if (Tax.Text.Contains("Last Payment Date"))
                                            {
                                                LastPaymentDate = TaxTD4[1].Text;
                                            }
                                            if (Tax.Text.Contains("Amount Paid"))
                                            {
                                                TotalAmountPaid = TaxTD4[1].Text + " ";
                                            }

                                        }
                                    }

                                    //Property information

                                    IWebElement TaxTB2 = driver.FindElement(By.XPath("//*[@id='secContent']/div[2]/div/div/div/div[1]/div[2]/div[1]/table"));
                                    IList<IWebElement> TaxTR2 = TaxTB2.FindElements(By.TagName("tr"));
                                    IList<IWebElement> TaxTD2;
                                    foreach (IWebElement Tax in TaxTR2)
                                    {
                                        TaxTD2 = Tax.FindElements(By.TagName("td"));
                                        if (TaxTD2.Count != 0 && Tax.Text != "")
                                        {
                                            if (Tax.Text.Contains("Parcel Number"))
                                            {
                                                PropertyNumber = TaxTD2[1].Text;
                                            }
                                            if (Tax.Text.Contains("Tax District"))
                                            {
                                                TaxDistrict1 = TaxTD2[1].Text + " ";
                                            }
                                            if (Tax.Text.Contains("Homestead Exemption"))
                                            {
                                                HomestedExemption = TaxTD2[1].Text + " ";
                                            }
                                        }
                                    }
                                    //Bill Information

                                    IWebElement TaxTB1 = driver.FindElement(By.XPath("//*[@id='secContent']/div[2]/div/div/div/div[1]/div[2]/div[2]/table/tbody"));
                                    IList<IWebElement> TaxTR1 = TaxTB1.FindElements(By.TagName("tr"));
                                    IList<IWebElement> TaxTD1;
                                    foreach (IWebElement Tax1 in TaxTR1)
                                    {
                                        TaxTD1 = Tax1.FindElements(By.TagName("td"));
                                        if (TaxTD1.Count != 0 && Tax1.Text != "")
                                        {
                                            if (Tax1.Text.Contains("Tax Year"))
                                            {
                                                TaxYearbill = TaxTD1[1].Text;
                                            }
                                            if (Tax1.Text.Contains("Due Date"))
                                            {
                                                TaxDueDate = TaxTD1[1].Text + " ";
                                            }

                                        }
                                    }
                                    //Tax Information
                                    string BaseTaxes = "", Penalty = "", Interest = "", Fees = "", GoodThrough = "", BalanceDue = "";
                                    IWebElement TaxTB3 = driver.FindElement(By.XPath("//*[@id='secContent']/div[2]/div/div/div/div[1]/div[2]/div[3]/table/tbody"));
                                    IList<IWebElement> TaxTR3 = TaxTB3.FindElements(By.TagName("tr"));
                                    IList<IWebElement> TaxTD3;
                                    foreach (IWebElement Tax in TaxTR3)
                                    {
                                        TaxTD3 = Tax.FindElements(By.TagName("td"));
                                        if (TaxTD3.Count != 0 && Tax.Text != "")
                                        {
                                            if (Tax.Text.Contains("Base Taxes"))
                                            {
                                                BaseTaxes = TaxTD3[1].Text;
                                            }
                                            if (Tax.Text.Contains("Penalty"))
                                            {
                                                Penalty = TaxTD3[1].Text + " ";
                                            }
                                            if (Tax.Text.Contains("Interest"))
                                            {
                                                Interest = TaxTD3[1].Text;
                                            }
                                            if (Tax.Text.Contains("Fees"))
                                            {
                                                Fees = TaxTD3[1].Text + " ";
                                            }
                                            if (Tax.Text.Contains("Good Through"))
                                            {
                                                GoodThrough = TaxTD3[1].Text + " ";
                                            }
                                            if (Tax.Text.Contains("Balance Due"))
                                            {
                                                BalanceDue = TaxTD3[1].Text + " ";
                                            }
                                        }
                                    }

                                    string Taxinfo_details1 = Taxinfownername + "~" + PropertyNumber.Trim() + "~" + TaxDistrict1.Trim() + "~" + HomestedExemption.Trim() + "~" + TaxYearbill.Trim() + "~" + TaxDueDate.Trim() + "~" + BaseTaxes.Trim() + "~" + Penalty.Trim() + "~" + Interest.Trim() + "~" + Fees.Trim() + "~" + GoodThrough.Trim() + "~" + BalanceDue.Trim() + "~" + PaymentStatus.Trim() + "~" + LastPaymentDate.Trim() + "~" + TotalAmountPaid.Trim() /* + "~" + Taxauthority*/;
                                    gc.insert_date(orderNumber, ParcelNumber, 1388, Taxinfo_details1, 1, DateTime.Now);
                                }
                                catch { }
                                //Jurisdiction Details Table

                                try
                                {
                                    string Taxauthority = "", Assessedvalue = "", LessExemption = "", NetTaxValue = "", Millage = "", Tax = "";
                                    IWebElement TaxTB1 = driver.FindElement(By.XPath("//*[@id='secContent']/div[2]/div/div/div/div[1]/div[4]/div/table"));
                                    IList<IWebElement> TaxTR1 = TaxTB1.FindElements(By.TagName("tr"));
                                    IList<IWebElement> TaxTD1;
                                    foreach (IWebElement Tax1 in TaxTR1)
                                    {
                                        TaxTD1 = Tax1.FindElements(By.TagName("td"));
                                        if (TaxTD1.Count != 0 && TaxTD1.Count == 6 && Tax1.Text != "")
                                        {

                                            Taxauthority = TaxTD1[0].Text;
                                            Assessedvalue = TaxTD1[1].Text;
                                            LessExemption = TaxTD1[2].Text;
                                            NetTaxValue = TaxTD1[3].Text;
                                            Millage = TaxTD1[4].Text;
                                            Tax = TaxTD1[5].Text;

                                            string Jurisdictiondetails1 = Taxauthority + "~" + Assessedvalue.Trim() + "~" + LessExemption.Trim() + "~" + NetTaxValue.Trim() + "~" + Millage.Trim() + "~" + Tax.Trim();
                                            gc.insert_date(orderNumber, ParcelNumber, 1389, Jurisdictiondetails1, 1, DateTime.Now);

                                        }
                                    }

                                }
                                catch { }

                                //Tax Bill
                                try
                                {
                                    driver.FindElement(By.XPath("//*[@id='secContent']/div[2]/div/div/ul/li[2]/a/div")).Click();
                                    Thread.Sleep(9000);
                                    gc.CreatePdf(orderNumber, ParcelNumber, "Tax Bill Details" + TaxYearbill, driver, "GA", "Cobb");
                                }
                                catch { }

                                //View And Print Receipt
                                try
                                {
                                    driver.FindElement(By.XPath("//*[@id='secContent']/div[2]/div/div/ul/li[4]/a/div")).Click();
                                    Thread.Sleep(3000);
                                    gc.CreatePdf(orderNumber, ParcelNumber, "View Print Receipt" + TaxYearbill, driver, "GA", "Cobb");
                                }
                                catch { }
                                Thread.Sleep(3000);
                                driver.Navigate().Back();


                            }
                        }
                    }
                    catch { }


                    // 3) Scenario for City / Township Taxes:
                    //1) City of Smyrna can be obtained through the link

                    try
                    {
                        if (TaxDistrict1.Contains("City of Smyrna"))

                        {
                            driver.Navigate().GoToUrl("https://portal.smyrnaga.gov/MSS/citizens/RealEstate/Default.aspx?mode=new");

                            driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_Control_ParcelIdSearchFieldLayout_ctl01_ParcelIDTextBox")).SendKeys(ParcelNumber);
                            driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_Control_FormLayoutItem7_ctl01_Button1")).SendKeys(Keys.Enter);
                            Thread.Sleep(2000);
                            gc.CreatePdf(orderNumber, ParcelNumber, "Tax Information", driver, "GA", "Cobb");

                            List<string> ParcelSearch = new List<string>();

                            try
                            {
                                IWebElement ParcelTB = driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_BillsGridView"));
                                IList<IWebElement> ParcelTR = ParcelTB.FindElements(By.TagName("tr"));
                                ParcelTR.Reverse();
                                int rows_count = ParcelTR.Count;

                                for (int row = 1; row < rows_count; row++)
                                {
                                    if (row == rows_count - 3 || row == rows_count - 1 || row == rows_count - 2)
                                    {
                                        IList<IWebElement> Columns_row = ParcelTR[row].FindElements(By.TagName("td"));
                                        int columns_count = Columns_row.Count;
                                        if (columns_count != 0)
                                        {
                                            IWebElement ParcelBill_link = Columns_row[6].FindElement(By.TagName("a"));
                                            string Parcelurl = ParcelBill_link.GetAttribute("id");
                                            ParcelSearch.Add(Parcelurl);
                                        }
                                    }
                                }
                            }
                            catch { }


                            //Tax Information after View bill details Scrap
                            ////view details click

                            foreach (string taxlink in ParcelSearch)
                            {

                                try
                                {
                                    driver.Navigate().GoToUrl("https://portal.smyrnaga.gov/MSS/citizens/RealEstate/Default.aspx?mode=new");

                                    driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_Control_ParcelIdSearchFieldLayout_ctl01_ParcelIDTextBox")).SendKeys(ParcelNumber);
                                    driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_Control_FormLayoutItem7_ctl01_Button1")).SendKeys(Keys.Enter);
                                    Thread.Sleep(2000);
                                }
                                catch { }

                                driver.FindElement(By.Id(taxlink)).Click();
                                Thread.Sleep(5000);

                                string taxbill = driver.CurrentWindowHandle;


                                string asofGood = "", asof = "", billyear = "", bill = "", viewbillowner = "", parcelid1 = "";
                                IWebElement IasofDate = driver.FindElement(By.Id("AsOfDateTextBox"));
                                asof = IasofDate.GetAttribute("value");
                                string bulktxt = driver.FindElement(By.XPath("//*[@id='BillDetailTable']/tbody")).Text;
                                string strbillyear = gc.Between(bulktxt, "Bill Year", "Owner");
                                billyear = GlobalClass.Before(strbillyear, "Bill");
                                gc.CreatePdf(orderNumber, ParcelNumber, "View Bill Yearwise" + billyear, driver, "GA", "Cobb");
                                bill = GlobalClass.After(strbillyear, "Bill");
                                viewbillowner = gc.Between(bulktxt, "Owner", "Parcel ID");
                                parcelid1 = GlobalClass.After(bulktxt, "Parcel ID");
                                string TaxingAuthority = "";

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
                                            string taxdetails = asofGood + "~" + asof + "~" + billyear + "~" + bill + "~" + viewbillowner + "~" + parcelid1 + "~" + TDBigdata[0].Text + "~" + TDBigdata[1].Text + "~" + TDBigdata[2].Text + "~" + TDBigdata[3].Text + "~" + TDBigdata[4].Text + "~" + TDBigdata[5].Text + "~" + TDBigdata[6].Text + "~" + TaxingAuthority;

                                            gc.insert_date(orderNumber, parcelid1, 1397, taxdetails, 1, DateTime.Now);
                                        }
                                        if (TDBigdata.Count == 6 && TDBigdata.Count != 0)
                                        {
                                            string taxdetails = asofGood + "~" + asof + "~" + billyear + "~" + bill + "~" + viewbillowner + "~" + parcelid1 + "~" + TDBigdata[0].Text + "~" + "" + "~" + TDBigdata[1].Text + "~" + TDBigdata[2].Text + "~" + TDBigdata[3].Text + "~" + TDBigdata[4].Text + "~" + TDBigdata[5].Text + "~" + TaxingAuthority;

                                            gc.insert_date(orderNumber, parcelid1, 1397, taxdetails, 1, DateTime.Now);
                                        }

                                    }
                                }
                                catch (Exception ex) { }
                                //Tax Payment Details
                                string strall_billyear = "", all_billyear = "", all_bill = "", all_billyear1 = "", all_bill1 = "";
                                try
                                {
                                    int i = 0;
                                    driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_ViewBill1_ViewPaymentsLinkButton")).SendKeys(Keys.Enter);
                                    Thread.Sleep(2000);
                                    string allbills = driver.FindElement(By.XPath("//*[@id='BillActivityTable']/tbody")).Text;

                                    strall_billyear = GlobalClass.After(allbills, "Year");
                                    all_bill = GlobalClass.After(strall_billyear, "Bill");
                                    all_billyear = GlobalClass.Before(strall_billyear, "Bill").Replace("\r\n", "");
                                    gc.CreatePdf(orderNumber, ParcelNumber, "Tax Payment Bill" + all_billyear, driver, "GA", "Cobb");


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
                                        if (TDallbill1.Count != 0 && TDallbill1.Count == 5 && !text.Text.Contains("Paid By/Reference") && !text.Text.Contains("Bill Year"))
                                        {
                                            string Allbilldetails = all_billyear1 + "~" + all_bill + "~" + TDallbill1[0].Text + "~" + TDallbill1[1].Text + "~" + TDallbill1[2].Text + "~" + TDallbill1[3].Text + "~" + TDallbill1[4].Text;
                                            gc.insert_date(orderNumber, ParcelNumber, 1399, Allbilldetails, 1, DateTime.Now);
                                        }
                                    }
                                    i++;
                                    driver.Navigate().Back();

                                    //Chromedrive use
                                    //int l = 0;
                                    string fileName1 = "";
                                    // Pdf download
                                    try
                                    {
                                        var chromeOptions = new ChromeOptions();
                                        var downloadDirectory = ConfigurationManager.AppSettings["AutoPdf"];
                                        chromeOptions.AddUserProfilePreference("download.default_directory", downloadDirectory);
                                        chromeOptions.AddUserProfilePreference("download.prompt_for_download", false);
                                        chromeOptions.AddUserProfilePreference("disable-popup-blocking", "true");
                                        chromeOptions.AddUserProfilePreference("plugins.always_open_pdf_externally", true);
                                        var chDriver = new ChromeDriver(chromeOptions);
                                        Array.ForEach(Directory.GetFiles(@downloadDirectory), File.Delete);
                                        chDriver.Navigate().GoToUrl("https://portal.smyrnaga.gov/MSS/citizens/RealEstate/Default.aspx?mode=new");
                                        chDriver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_Control_ParcelIdSearchFieldLayout_ctl01_ParcelIDTextBox")).SendKeys(ParcelNumber);
                                        chDriver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_Control_FormLayoutItem7_ctl01_Button1")).SendKeys(Keys.Enter);
                                        Thread.Sleep(1000);
                                        chDriver.FindElement(By.XPath("//*[@id='ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_BillsGridView']/tbody/tr[17]/td[7]")).Click();
                                        Thread.Sleep(1000);
                                        chDriver.FindElement(By.XPath("//*[@id='ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_ViewBill1_BillDocumentLink']")).Click();                                        
                                        chDriver.SwitchTo().Window(chDriver.WindowHandles.Last());
                                        //string urdown = chDriver.Url;
                                        //gc.downloadfile(urdown, orderNumber, ParcelNumber, "Check download", "GA", "Cobb");
                                        gc.CreatePdf(orderNumber, ParcelNumber, "lk" + all_billyear1, chDriver, "GA", "Cobb");
                                        fileName1 = latestfilename();
                                        Thread.Sleep(1000);
                                        gc.AutoDownloadFile(orderNumber, ParcelNumber, "Cobb", "GA", fileName1);
                                        chDriver.Quit();
                                    }
                                    catch { }                                                                    
                                }
                                catch { }
                            }
                            try
                            {
                                //Tax History Bill Details

                                try
                                {

                                    IWebElement IAddressSearch1 = driver.FindElement(By.LinkText("All Bills"));
                                    IJavaScriptExecutor js1 = driver as IJavaScriptExecutor;
                                    js1.ExecuteScript("arguments[0].click();", IAddressSearch1);
                                    Thread.Sleep(3000);
                                }
                                catch { }
                                try
                                {

                                    IWebElement IAddressSearch1 = driver.FindElement(By.XPath("//*[@id='submenuselected']"));
                                    IJavaScriptExecutor js1 = driver as IJavaScriptExecutor;
                                    js1.ExecuteScript("arguments[0].click();", IAddressSearch1);
                                    Thread.Sleep(3000);
                                }
                                catch { }
                                try
                                {

                                    IWebElement IAddressSearch1 = driver.FindElement(By.Id("submenuselected"));
                                    IJavaScriptExecutor js1 = driver as IJavaScriptExecutor;
                                    js1.ExecuteScript("arguments[0].click();", IAddressSearch1);
                                    Thread.Sleep(3000);
                                }
                                catch { }
                                try
                                {
                                    IWebElement ISpan13 = driver.FindElement(By.Id("submenuselected"));
                                    ISpan13.Click();
                                    Thread.Sleep(3000);
                                }
                                catch { }
                                try
                                {
                                    IWebElement ISpan13 = driver.FindElement(By.XPath("//*[@id='submenuselected']"));
                                    ISpan13.Click();
                                    Thread.Sleep(3000);
                                }
                                catch { }


                                gc.CreatePdf(orderNumber, ParcelNumber, "Tax History Details All Bills", driver, "GA", "Cobb");
                                IWebElement Bigdata3 = driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_BillsRepeater_ctl00_BillsGrid"));
                                IList<IWebElement> TRBigdata3 = Bigdata3.FindElements(By.TagName("tr"));
                                IList<IWebElement> THBigdata3 = Bigdata3.FindElements(By.TagName("th"));
                                IList<IWebElement> TDBigdata3;
                                foreach (IWebElement row2 in TRBigdata3)
                                {
                                    TDBigdata3 = row2.FindElements(By.TagName("td"));
                                    THBigdata3 = row2.FindElements(By.TagName("th"));
                                    if (TDBigdata3.Count != 0 && TDBigdata3.Count == 6 && row2.Text.Contains("Bill") && !row2.Text.Contains("Total"))
                                    {
                                        string TaxHistoryBillDetails = TDBigdata3[0].Text + "~" + TDBigdata3[1].Text + "~" + TDBigdata3[2].Text + "~" + TDBigdata3[3].Text + "~" + TDBigdata3[4].Text;

                                        gc.insert_date(orderNumber, ParcelNumber, 1400, TaxHistoryBillDetails, 1, DateTime.Now);
                                    }

                                }
                            }
                            catch { }
                            
                        }

                    }
                    catch { }
                    //2) City of Powder Springs

                    if (TaxDistrict1.Contains("City of Powder Springs"))
                    {
                        try
                        {
                            driver.Navigate().GoToUrl("https://wipp.edmundsassoc.com/Wipp/?wippid=19");
                            Thread.Sleep(4000);
                            //driver.set_page_load_timeout(30);

                            //driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/div/table[1]/tbody/tr/td/div/a")).Click();
                            //ParcelNumber = cityparcelnumber;
                            // string id1 = cityparcelnumber.Substring(0, 2);
                            // string id11 = cityparcelnumber.Substring(2, 6);
                            // cityparcelnumber = id1 + "-" + id11;

                            try
                            {
                                driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/div/table[1]/tbody/tr/td/table/tbody/tr[2]/td/table/tbody/tr[1]/td[2]/input")).SendKeys(parcelNumber);
                                gc.CreatePdf(orderNumber, ParcelNumber, "Taxinfo city", driver, "GA", "Cobb");
                                driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/div/table[1]/tbody/tr/td/table/tbody/tr[2]/td/table/tbody/tr[1]/td[3]/button")).SendKeys(Keys.Enter);
                                Thread.Sleep(5000);
                                gc.CreatePdf(orderNumber, ParcelNumber, "Taxinfo city1", driver, "GA", "Cobb");
                                string a = driver.FindElement(By.XPath(" /html/body/div[2]/div/table/tbody/tr[2]/td[2]/div/table/tbody/tr[1]/td/table/tbody/tr[2]/td[1]")).Text;
                            }
                            catch { }
                            //          
                            string strTaxReal = "";
                            try
                            {
                                IWebElement ITaxReal11 = driver.FindElement(By.XPath("/html/body/div[2]/div/table/tbody/tr[2]/td[2]/div/table/tbody/tr[1]/td/table/tbody"));
                                IList<IWebElement> ITaxRealRow11 = ITaxReal11.FindElements(By.TagName("tr"));
                                IList<IWebElement> ITaxRealTd11;

                                foreach (IWebElement ItaxReal11 in ITaxRealRow11)
                                {

                                    //  strTaxRealestate.Clear();
                                    ITaxRealTd11 = ItaxReal11.FindElements(By.TagName("td"));
                                    if (ItaxReal11.Text.Contains(parcelNumber))
                                    {

                                        //  string yearbill = ITaxRealTd[0].Text;
                                        IWebElement ITaxBillCount = ITaxRealTd11[0].FindElement(By.TagName("input"));
                                        strTaxReal = ITaxBillCount.GetAttribute("id");
                                        strTaxRealestate.Add(strTaxReal);

                                    }
                                }
                                IWebElement element1 = driver.FindElement(By.Id(strTaxReal));
                                element1.Click();
                                Thread.Sleep(2000);
                            }
                            catch { }

                            gc.CreatePdf(orderNumber, ParcelNumber, "Taxinfo city2", driver, "GA", "Cobb");



                            //Account Number~Owner Name~Property Address~Land Value~Improvement Value~Exempt Value~Total Assessed Value~Deductions~Last Paid Date

                            string Taxaccountid = "", AccountNumber = "", OwnerName = "", PropertyAddress1 = "", LandValue = "", ImprovementValue = "", ExemptValue = "", TotalAssessedValue = "", Deductions = "", LastPaidDate = "";

                            int i = 0;
                            IWebElement tbmulti11 = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[1]/td/table/tbody/tr/td/table/tbody"));
                            IList<IWebElement> TRmulti11 = tbmulti11.FindElements(By.TagName("tr"));
                            IList<IWebElement> TDmulti11;
                            foreach (IWebElement row in TRmulti11)
                            {

                                TDmulti11 = row.FindElements(By.TagName("td"));
                                if (TDmulti11.Count == 4)
                                {
                                    if (i == 0)
                                    {
                                        AccountNumber = TDmulti11[1].Text;
                                        Taxaccountid = TDmulti11[3].Text;
                                    }

                                    if (i == 1)
                                        PropertyAddress1 = TDmulti11[1].Text;
                                    if (i == 2)
                                    {
                                        OwnerName = TDmulti11[1].Text;
                                        LandValue = TDmulti11[3].Text;
                                    }
                                    if (i == 3)
                                        ImprovementValue = TDmulti11[3].Text;
                                    if (i == 4)
                                        ExemptValue = TDmulti11[3].Text;
                                    if (i == 5)
                                        TotalAssessedValue = TDmulti11[3].Text;
                                    if (i == 6)
                                        Deductions = TDmulti11[3].Text;
                                    i++;
                                }
                            }
                            LastPaidDate = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[3]/td/table/tbody/tr[2]/td/div/div/table/tbody/tr[3]/td/table/tbody/tr/td/div")).Text.Trim();
                            LastPaidDate = WebDriverTest.After(LastPaidDate, "Last Payment:").Trim();
                            string Tax_infoCity = Taxaccountid + "~" + OwnerName + "~" + PropertyAddress1 + "~" + LandValue + "~" + ImprovementValue + "~" + ExemptValue + "~" + TotalAssessedValue + "~" + Deductions + "~" + LastPaidDate;
                            gc.insert_date(orderNumber, AccountNumber, 1401, Tax_infoCity, 1, DateTime.Now);


                            IWebElement multitableElement1 = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[3]/td/table/tbody/tr[2]/td/div/div/table/tbody/tr[2]/td/table/tbody"));
                            IList<IWebElement> multitableRow1 = multitableElement1.FindElements(By.TagName("tr"));
                            IList<IWebElement> multirowTD1;
                            foreach (IWebElement row in multitableRow1)
                            {
                                if (!row.Text.Contains("Year") && !row.Text.Contains("Last Payment:"))
                                {

                                    multirowTD1 = row.FindElements(By.TagName("td"));

                                    if (multirowTD1.Count != 0)
                                    {
                                        string tax_HistoryCity = multirowTD1[0].Text.Trim() + "~" + multirowTD1[1].Text.Trim() + "~" + multirowTD1[2].Text.Trim() + "~" + multirowTD1[3].Text.Trim() + "~" + multirowTD1[5].Text.Trim() + "~" + multirowTD1[6].Text.Trim() + "~" + multirowTD1[7].Text.Trim() + "~" + multirowTD1[8].Text.Trim();
                                        gc.insert_date(orderNumber, ParcelNumber, 1402, tax_HistoryCity, 1, DateTime.Now);
                                    }
                                }
                                //Year~Due Date~Type~Tax Amount~Balance~Interest~Total Due~Status

                            }
                        }
                        catch { }
                    }
                    //3) City of Kennesaw Details                
                    if (TaxDistrict1.Contains("City of Kennesaw"))
                    {

                        driver.Navigate().GoToUrl("https://www.municipalonlinepayments.com/kennesawga/tax/search");
                        Thread.Sleep(4000);
                        //driver.set_page_load_timeout(30);

                        driver.FindElement(By.Id("ParcelNumber")).SendKeys(ParcelNumber);
                        gc.CreatePdf(orderNumber, ParcelNumber, "Taxinfo city3 Before", driver, "GA", "Cobb");
                        driver.FindElement(By.XPath("//*[@id='main']/section/div[1]/form/div[4]/div/button")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, ParcelNumber, "Taxinfo city3 After", driver, "GA", "Cobb");
                        ////*[@id="main"]/section/div[2]/table/tbody/tr/td[1]/ul/li/a
                        driver.FindElement(By.XPath("//*[@id='main']/section/div[2]/table/tbody/tr/td[1]/ul/li/a")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, ParcelNumber, "Taxinfo city3 Parcel click After", driver, "GA", "Cobb");

                        //City of Kennesaw Tax Information Details
                        string Name = "", District = "";
                        IWebElement Taxinfo2 = driver.FindElement(By.XPath("//*[@id='main']/section/table[1]/tbody"));
                        IList<IWebElement> TRTaxinfo2 = Taxinfo2.FindElements(By.TagName("tr"));
                        IList<IWebElement> Aherftax;

                        foreach (IWebElement row in TRTaxinfo2)
                        {
                            Aherftax = row.FindElements(By.TagName("td"));

                            if (Aherftax.Count != 0 && Aherftax.Count == 2 && !row.Text.Contains("Deed Holder") && row.Text.Contains("Name") /*&& !row.Text.Contains("Bill Type")*/)
                            {
                                Name = Aherftax[0].Text;
                            }
                            if (Aherftax.Count != 0 && Aherftax.Count == 2 && !row.Text.Contains("Deed Holder") && row.Text.Contains("District") /*&& !row.Text.Contains("Bill Type")*/)
                            {
                                District = Aherftax[1].Text;
                            }
                        }
                        string Taxinformationsdetails = Name.Trim() + "~" + District.Trim();
                        gc.insert_date(orderNumber, ParcelNumber, 1411, Taxinformationsdetails, 1, DateTime.Now);

                        //City of Kennesaw Valuation and Tax Information Details
                        string ThisYear = "", LastYear = "", title1 = "", FullMarketValue1 = "", Assessed1 = "", FullMarketValue2 = "", Assessed2 = "";
                        IWebElement Taxinfo3 = driver.FindElement(By.XPath("//*[@id='main']/section/table[2]/tbody"));
                        IList<IWebElement> TRTaxinfo3 = Taxinfo3.FindElements(By.TagName("tr"));
                        IList<IWebElement> Aherftax1;
                        IList<IWebElement> Aherftax2;
                        IList<IWebElement> Aherftax3;
                        foreach (IWebElement row1 in TRTaxinfo3)
                        {
                            Aherftax1 = row1.FindElements(By.TagName("tr"));
                            Aherftax2 = row1.FindElements(By.TagName("th"));
                            Aherftax3 = row1.FindElements(By.TagName("td"));

                            if (Aherftax2.Count != 0 && Aherftax2.Count == 3 /*&& !row1.Text.Contains("Deed Holder")*//*&& !row.Text.Contains("Bill Type")*/)
                            {
                                ThisYear = Aherftax2[1].Text.Replace("(This Year)", "").Trim();
                                LastYear = Aherftax2[2].Text.Replace("(Last Year)", "").Trim();

                            }
                            if (Aherftax2.Count != 0 && Aherftax2.Count == 1 /*&& !row1.Text.Contains("Deed Holder")*//*&& !row.Text.Contains("Bill Type")*/)
                            {
                                title1 = Aherftax2[0].Text;
                            }
                            if (Aherftax3.Count != 0 && Aherftax3.Count == 4/* && !row1.Text.Contains("Deed Holder") && row1.Text.Contains("District")*/ /*&& !row.Text.Contains("Bill Type")*/)
                            {
                                FullMarketValue1 = Aherftax3[0].Text;
                                Assessed1 = Aherftax3[1].Text;
                                FullMarketValue2 = Aherftax3[2].Text;
                                Assessed2 = Aherftax3[3].Text;

                                db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + "Title~Year1~Full Market Value1~Assessed Value1~Year2~Full Market Value2~Assessed Value2" + "' where Id = '" + 1412 + "'");
                                string ValuationTaxinformationsdetails = title1 + "~" + ThisYear.Trim() + "~" + FullMarketValue1.Trim() + "~" + Assessed1.Trim() + "~" + LastYear.Trim() + "~" + FullMarketValue2.Trim() + "~" + Assessed2.Trim();
                                gc.insert_date(orderNumber, ParcelNumber, 1412, ValuationTaxinformationsdetails, 1, DateTime.Now);
                            }

                        }

                        //City of Kennesaw Tax History Details
                        string TaxKennesaw = "", Type = "", Amount = "", Interest = "", Penalty = "", Total = "", BillNumber = "", Specialdrainage = "";
                        IWebElement Taxinfo4 = driver.FindElement(By.XPath("//*[@id='tax_history']/tbody"));
                        IList<IWebElement> TRTaxinfo4 = Taxinfo4.FindElements(By.TagName("tr"));
                        IList<IWebElement> Aherftax4;

                        foreach (IWebElement row4 in TRTaxinfo4)
                        {
                            Aherftax4 = row4.FindElements(By.TagName("td"));

                            if (Aherftax4.Count != 0 && Aherftax4.Count == 8 && !row4.Text.Contains("Tax Year") /*&& row4.Text.Contains("Name")*/ /*&& !row.Text.Contains("Bill Type")*/)
                            {
                                TaxKennesaw = Aherftax4[0].Text;
                                Type = Aherftax4[1].Text;
                                Amount = Aherftax4[2].Text;
                                Interest = Aherftax4[3].Text;
                                Penalty = Aherftax4[4].Text;
                                Total = Aherftax4[5].Text;
                                BillNumber = Aherftax4[6].Text;
                                Specialdrainage = Aherftax4[7].Text;
                            }
                            string TaxHistorydetails = TaxKennesaw.Trim() + "~" + Type.Trim() + "~" + Amount.Trim() + "~" + Interest.Trim() + "~" + Penalty.Trim() + "~" + Total.Trim() + "~" + BillNumber.Trim() + "~" + Specialdrainage.Trim();
                            gc.insert_date(orderNumber, ParcelNumber, 1418, TaxHistorydetails, 1, DateTime.Now);
                        }
                        //Tax Bill Download
                        var SelectDistrict3 = driver.FindElement(By.Id("ReportName"));
                        var SelectDistrict2 = new SelectElement(SelectDistrict3);
                        SelectDistrict2.SelectByText("Tax Statement Georgia Multi");
                        driver.FindElement(By.XPath("//*[@id='report_form']/div/span/button")).Click();
                        //driver.SwitchTo().Frame(Multyaddresstable1);
                        IWebElement Multyaddresstable1 = driver.FindElement(By.TagName("iframe"));
                        //driver.SwitchTo().Frame(Multyaddresstable1);
                        //IWebElement downl = driver.FindElement(By.Id("content"));
                        string urldowl = Multyaddresstable1.GetAttribute("src");
                        //driver.Navigate().GoToUrl(urldowl);
                        //Thread.Sleep(15000);
                        //gc.downloadfile(urldowl, orderNumber, ParcelNumber, "ViewTaxBill dominic", "GA", "Cobb");

                        int l = 0;
                        gc.downloadfile(urldowl, orderNumber, ParcelNumber, "ViewTaxBill dominic", "GA", "Cobb");
                        string fileName1 = "";
                        //Pdf download
                        //try
                        //{
                        //    var chromeOptions = new ChromeOptions();
                        //    var downloadDirectory = ConfigurationManager.AppSettings["AutoPdf"];
                        //    chromeOptions.AddUserProfilePreference("download.default_directory", downloadDirectory);
                        //    chromeOptions.AddUserProfilePreference("download.prompt_for_download", false);
                        //    chromeOptions.AddUserProfilePreference("disable-popup-blocking", "true");
                        //    chromeOptions.AddUserProfilePreference("plugins.always_open_pdf_externally", true);
                        //    var chDriver = new ChromeDriver(chromeOptions);
                        //    Array.ForEach(Directory.GetFiles(@downloadDirectory), File.Delete);
                        //    chDriver.Navigate().GoToUrl(urldowl);
                        //    Thread.Sleep(2000);
                        //    gc.downloadfile(urldowl, orderNumber, ParcelNumber, "ViewTaxBill chrome", "GA", "Cobb");
                        //    gc.CreatePdf(orderNumber, ParcelNumber, "Tax Bill Kennasaw1", chDriver, "GA", "Cobb");
                        //    chDriver.SwitchTo().Window(chDriver.WindowHandles.Last());
                        //    Thread.Sleep(5000);
                        //    gc.CreatePdf(orderNumber, ParcelNumber, "Tax Bill1 Kennasaw2", chDriver, "GA", "Cobb");

                        //    Thread.Sleep(5000);
                        //    fileName1 = latestfilename();
                        //    Thread.Sleep(2000);
                        //    gc.AutoDownloadFile(orderNumber, ParcelNumber, "Cobb", "GA", fileName1);
                        //    chDriver.Quit();

                        //}
                        //catch { }
                    }


                    //4) City of Marietta
                    //Waiting for input
                    if (TaxDistrict1.Contains("City of Marietta"))
                    {
                        try
                        {
                            driver.Navigate().GoToUrl("https://www.mariettageorgia.us/Click2GovTX/");
                            Thread.Sleep(2000);
                            var SelectParcelnumber = driver.FindElement(By.Id("searchMethod"));
                            var SelectParcelnumber1 = new SelectElement(SelectParcelnumber);
                            SelectParcelnumber1.SelectByText("Parcel");
                            //driver.set_page_load_timeout(30);

                            //driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/div/table[1]/tbody/tr/td/div/a")).Click();
                            string District1 = ParcelNumber.Substring(1, 2);
                            string Landlot1 = ParcelNumber.Substring(3, 5);
                            string Parcel1 = ParcelNumber.Substring(8, 4);
                            //ParcelNumber = id1 + "-" + id11;
                            driver.FindElement(By.Id("parcel.parcelNumber1")).SendKeys(District1);
                            driver.FindElement(By.Id("parcel.parcelNumber2")).SendKeys(Landlot1);
                            driver.FindElement(By.Id("parcel.parcelNumber3")).SendKeys(Parcel1);
                            gc.CreatePdf(orderNumber, ParcelNumber, "Taxinfo city4 Before", driver, "GA", "Cobb");
                            driver.FindElement(By.Id("parcelSbmtBtn")).SendKeys(Keys.Enter);
                            gc.CreatePdf(orderNumber, ParcelNumber, "Taxinfo city4 After", driver, "GA", "Cobb");
                            driver.FindElement(By.XPath("//*[@id='DataTables_Table_0']/tbody/tr/td[1]/a")).Click();
                            Thread.Sleep(2000);
                            gc.CreatePdf(orderNumber, ParcelNumber, "City Tax Marietta Parcelclick", driver, "GA", "Cobb");
                            //City Tax Billing Summary

                            string Billownername = "", BillAddress = "", BillParceid = "", BillNumber1 = "", LocationID = "", OldBillNumber = "";
                            string SecondOwnerName = "", Acreage = "", PlatBookPage = "", Propertyuse = "", Subdivision = "", Township = "", BillAssessedValue = "", Exemptions = "", TaxableValue = "", RollCode = "";
                            string Billedtaxes = "", Currenttaxdue = "", InterestPenalties = "", BillTotaldue = "";
                            IWebElement BillingSum = driver.FindElement(By.XPath("//*[@id='contentPanel']/div"));
                            //Bill Summary
                            BillNumber1 = gc.Between(BillingSum.Text, "Bill Number:", "Owner Name:").Trim();
                            Billownername = gc.Between(BillingSum.Text, "Owner Name:", "Location ID:").Trim();
                            LocationID = gc.Between(BillingSum.Text, "Location ID:", "Address:").Trim();
                            BillAddress = gc.Between(BillingSum.Text, "Address:", "Old Bill Number:").Trim();
                            OldBillNumber = gc.Between(BillingSum.Text, "Old Bill Number:", "Parcel ID:").Trim();
                            BillParceid = gc.Between(BillingSum.Text, "Parcel ID:", "General Account Information").Trim();
                            //General Account Information
                            SecondOwnerName = gc.Between(BillingSum.Text, "Second Owner Name:", "Acreage:").Trim();
                            Acreage = gc.Between(BillingSum.Text, "Acreage:", "Plat Book Page:").Trim();
                            PlatBookPage = gc.Between(BillingSum.Text, "Plat Book Page:", "Property Use:").Trim();
                            Propertyuse = gc.Between(BillingSum.Text, "Property Use:", "Subdivision:").Trim();
                            Subdivision = gc.Between(BillingSum.Text, "Subdivision:", "Township:").Trim();
                            Township = gc.Between(BillingSum.Text, "Township:", "Assessed Value:").Trim();
                            BillAssessedValue = gc.Between(BillingSum.Text, "Assessed Value:", "Exemptions:").Trim();
                            Exemptions = gc.Between(BillingSum.Text, "Exemptions:", "Taxable Value:").Trim();
                            TaxableValue = gc.Between(BillingSum.Text, "Taxable Value:", "Roll Code:").Trim();
                            RollCode = gc.Between(BillingSum.Text, "Roll Code:", "Tax Information for 2018").Trim();
                            //Tax Information for 2018
                            Billedtaxes = gc.Between(BillingSum.Text, "Billed taxes:", "Current Taxes Due:").Trim();
                            Currenttaxdue = gc.Between(BillingSum.Text, "Current Taxes Due:", "Interest, Penalties, and Collections:").Trim();
                            InterestPenalties = gc.Between(BillingSum.Text, "Interest, Penalties, and Collections:", "Total Due:").Trim();
                            BillTotaldue = GlobalClass.After(BillingSum.Text, "Total Due:").Trim();
                            //Owner Name~Address~Parcel ID~Bill Number1~Location ID~Old Bill Number~Second Owner Name~Acreage~Plat Book Page~Property Use~Subdivision~Township~Bill Assessed Value~Exemptions~Taxable Value~Roll Code~Billed Taxes~Current Taxdue~Interest Penalties and Collections~Totaldue
                            string Billinfodetails = Billownername.Trim() + "~" + BillAddress.Trim() + "~" + BillParceid.Trim() + "~" + BillNumber1.Trim() + "~" + LocationID.Trim() + "~" + OldBillNumber.Trim() + "~" + SecondOwnerName.Trim() + "~" + Acreage.Trim() + "~" + PlatBookPage.Trim() + "~" + Propertyuse.Trim() + "~" + Subdivision.Trim() + "~" + Township.Trim() + "~" + BillAssessedValue.Trim() + "~" + Exemptions.Trim() + "~" + TaxableValue.Trim() + "~" + RollCode.Trim() + "~" + Billedtaxes.Trim() + "~" + Currenttaxdue.Trim() + "~" + InterestPenalties.Trim() + "~" + BillTotaldue.Trim();
                            gc.insert_date(orderNumber, ParcelNumber, 1422, Billinfodetails, 1, DateTime.Now);


                            //ViewBill Click 
                            ////*[@id="contentPanel"]/portlet:defineobjects/div[1]/a
                            driver.FindElement(By.XPath("//*[@id='menuWrapper']/a[5]")).Click();
                            Thread.Sleep(2000);
                            gc.CreatePdf(orderNumber, ParcelNumber, "City Tax Marietta viewbillclick", driver, "GA", "Cobb");
                            string a = driver.CurrentWindowHandle;
                            try
                            {
                                IWebElement downln = driver.FindElement(By.LinkText("View PDF bill"));
                                downln.Click();
                                Thread.Sleep(3000);
                            }
                            catch { }
                            driver.SwitchTo().Window(driver.WindowHandles.Last());
                            gc.CreatePdf(orderNumber, ParcelNumber, "View Bill Tax Marietta ", driver, "GA", "Cobb");
                            driver.SwitchTo().Window(a);

                            //Historyclick

                            driver.FindElement(By.XPath("//*[@id='menuWrapper']/a[6]")).Click();
                            Thread.Sleep(2000);
                            gc.CreatePdf(orderNumber, ParcelNumber, "City Tax Marietta Historyclick", driver, "GA", "Cobb");

                            for (int p = 0; p < 3; p++)
                            {

                                //var year = driver.FindElement(By.Id("TaxYear"));
                                //var selectElement1 = new SelectElement(year);
                                //selectElement1.SelectByValue(p);
                                IWebElement PropertyInformation = driver.FindElement(By.Id("TaxYear"));
                                SelectElement PropertyInformationSelect = new SelectElement(driver.FindElement(By.Name("TaxYear")));
                                PropertyInformationSelect.SelectByIndex(p);
                                Thread.Sleep(3000);

                                IWebElement PropertyInformation1 = driver.FindElement(By.Id("TaxYear"));
                                SelectElement PropertyInformationSelect1 = new SelectElement(driver.FindElement(By.Name("TaxYear")));

                                string HistoryTaxyear = "", Totalamountbilled = "", Totalamountpaid = "", Totalamountunapplied = "", Totalamountdue = "";
                                HistoryTaxyear = PropertyInformationSelect1.SelectedOption.Text;

                                Totalamountbilled = driver.FindElement(By.XPath("//*[@id='frmHistoryForm']/div[2]/div[2]/div/p")).Text.Trim();
                                Totalamountpaid = driver.FindElement(By.XPath("//*[@id='frmHistoryForm']/div[2]/div[3]/div/p")).Text.Trim();
                                Totalamountunapplied = driver.FindElement(By.XPath("//*[@id='frmHistoryForm']/div[2]/div[4]/div/p")).Text.Trim();
                                Totalamountdue = driver.FindElement(By.XPath("//*[@id='frmHistoryForm']/div[2]/div[5]/div/p")).Text.Trim();


                                string Date = "", Period = "", Type = "", Amount = "";
                                IWebElement Historyinfo4 = driver.FindElement(By.XPath("//*[@id='DataTables_Table_0']/tbody"));
                                IList<IWebElement> TRHistoryinfo4 = Historyinfo4.FindElements(By.TagName("tr"));
                                IList<IWebElement> Historytax4;

                                foreach (IWebElement History4 in TRHistoryinfo4)
                                {
                                    Historytax4 = History4.FindElements(By.TagName("td"));

                                    if (Historytax4.Count != 0 && Historytax4.Count == 4 /*&& !History4.Text.Contains("Tax Year")*/ /*&& row4.Text.Contains("Name")*/ /*&& !row.Text.Contains("Bill Type")*/)
                                    {
                                        Date = Historytax4[0].Text;
                                        Period = Historytax4[1].Text;
                                        Type = Historytax4[2].Text;
                                        Amount = Historytax4[3].Text;

                                    }
                                    string TaxHistorydetails = HistoryTaxyear.Trim() + "~" + Totalamountbilled.Trim() + "~" + Totalamountpaid.Trim() + "~" + Totalamountunapplied.Trim() + "~" + Totalamountdue.Trim() + "~" + Date.Trim() + "~" + Period.Trim() + "~" + Type.Trim() + "~" + Amount.Trim();
                                    gc.insert_date(orderNumber, ParcelNumber, 1429, TaxHistorydetails, 1, DateTime.Now);
                                }
                            }
                        }
                        catch (Exception ex) { }

                        //Valuation Screeshot
                        driver.FindElement(By.XPath("//*[@id='menuWrapper']/a[7]")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, ParcelNumber, "Valuation", driver, "GA", "Cobb");

                    }

                    //5) City of Austell
                    if (TaxDistrict1.Contains("City of Austell"))
                    {
                        try
                        {
                            driver.Navigate().GoToUrl("https://wipp.edmundsassoc.com/Wipp/?wippid=ASTL");
                            Thread.Sleep(4000);
                            //driver.set_page_load_timeout(30);

                            try
                            {
                                driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/div/table[1]/tbody/tr/td/div/a")).Click();
                                string id1 = ParcelNumber.Substring(0, 2);
                                string id11 = ParcelNumber.Substring(2, 6);
                                ParcelNumber = id1 + "-" + id11;
                            }
                            catch { }
                            ///html/body/table/tbody/tr[2]/td/div/table[1]/tbody/tr/td/table/tbody/tr[2]/td/table/tbody/tr[1]/td[5]/input
                            driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/div/table[1]/tbody/tr/td/table/tbody/tr[2]/td/table/tbody/tr[2]/td[5]/input")).SendKeys(PropertyAddress);
                            gc.CreatePdf(orderNumber, ParcelNumber, "Taxinfo city5 Before", driver, "GA", "Cobb");
                            driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/div/table[1]/tbody/tr/td/table/tbody/tr[2]/td/table/tbody/tr[2]/td[6]/button")).SendKeys(Keys.Enter);
                            gc.CreatePdf(orderNumber, ParcelNumber, "Taxinfo city5 After", driver, "GA", "Cobb");
                            //string a = driver.FindElement(By.XPath(" /html/body/div[2]/div/table/tbody/tr[2]/td[2]/div/table/tbody/tr[1]/td/table/tbody/tr[2]/td[1]")).Text;
                            //          
                            string strTaxReal = "";
                            IWebElement ITaxReal11 = driver.FindElement(By.XPath("/html/body/div[2]/div/table/tbody/tr[2]/td[2]/div/table/tbody/tr[1]/td/table/tbody"));
                            IList<IWebElement> ITaxRealRow11 = ITaxReal11.FindElements(By.TagName("tr"));
                            IList<IWebElement> ITaxRealTd11;

                            foreach (IWebElement ItaxReal11 in ITaxRealRow11)
                            {

                                //  strTaxRealestate.Clear();
                                ITaxRealTd11 = ItaxReal11.FindElements(By.TagName("td"));
                                if (ItaxReal11.Text.Contains(PropertyAddress))
                                {

                                    //  string yearbill = ITaxRealTd[0].Text;
                                    IWebElement ITaxBillCount = ITaxRealTd11[0].FindElement(By.TagName("input"));
                                    strTaxReal = ITaxBillCount.GetAttribute("id");
                                    strTaxRealestate.Add(strTaxReal);

                                }
                            }
                            IWebElement element1 = driver.FindElement(By.Id(strTaxReal));
                            element1.Click();
                            Thread.Sleep(4000);

                            gc.CreatePdf(orderNumber, ParcelNumber, "Taxinfo city5", driver, "GA", "Cobb");



                            //Account Number~Owner Name~Property Address~Land Value~Improvement Value~Exempt Value~Total Assessed Value~Deductions~Last Paid Date

                            string Taxaccid = "", Block = "", OwnerName = "", PropertyAddress1 = "", LandValue = "", ImprovementValue = "", ExemptValue = "", TotalAssessedValue = "", Deductions = "", LastPaidDate = "";

                            int i = 0;
                            IWebElement tbmulti11 = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[1]/td/table/tbody/tr/td/table/tbody"));
                            IList<IWebElement> TRmulti11 = tbmulti11.FindElements(By.TagName("tr"));
                            IList<IWebElement> TDmulti11;
                            foreach (IWebElement row in TRmulti11)
                            {

                                TDmulti11 = row.FindElements(By.TagName("td"));
                                if (TDmulti11.Count == 4)
                                {
                                    if (i == 0)
                                    {
                                        Block = TDmulti11[1].Text;
                                        Taxaccid = TDmulti11[3].Text;
                                    }

                                    if (i == 1)
                                        PropertyAddress1 = TDmulti11[1].Text;
                                    if (i == 2)
                                    {
                                        OwnerName = TDmulti11[1].Text;
                                        LandValue = TDmulti11[3].Text;
                                    }
                                    if (i == 3)
                                        ImprovementValue = TDmulti11[3].Text;
                                    if (i == 4)
                                        ExemptValue = TDmulti11[3].Text;
                                    if (i == 5)
                                        TotalAssessedValue = TDmulti11[3].Text;
                                    if (i == 6)
                                        Deductions = TDmulti11[3].Text;
                                    i++;
                                }
                            }
                            LastPaidDate = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[3]/td/table/tbody/tr[2]/td/div/div/table/tbody/tr[3]/td/table/tbody/tr/td/div")).Text.Trim();
                            LastPaidDate = WebDriverTest.After(LastPaidDate, "Last Payment:").Trim();
                            string Tax_infoCity = Block + "~" + Taxaccid + "~" + OwnerName + "~" + PropertyAddress1 + "~" + LandValue + "~" + ImprovementValue + "~" + ExemptValue + "~" + TotalAssessedValue + "~" + Deductions + "~" + LastPaidDate;
                            gc.insert_date(orderNumber, ParcelNumber, 1413, Tax_infoCity, 1, DateTime.Now);


                            IWebElement multitableElement1 = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[3]/td/table/tbody/tr[2]/td/div/div/table/tbody/tr[2]/td/table/tbody"));
                            IList<IWebElement> multitableRow1 = multitableElement1.FindElements(By.TagName("tr"));
                            IList<IWebElement> multirowTD1;
                            foreach (IWebElement row in multitableRow1)
                            {
                                if (!row.Text.Contains("Year") && !row.Text.Contains("Last Payment:"))
                                {

                                    multirowTD1 = row.FindElements(By.TagName("td"));

                                    if (multirowTD1.Count != 0)
                                    {
                                        string tax_HistoryCity = multirowTD1[0].Text.Trim() + "~" + multirowTD1[1].Text.Trim() + "~" + multirowTD1[2].Text.Trim() + "~" + multirowTD1[3].Text.Trim() + "~" + multirowTD1[5].Text.Trim() + "~" + multirowTD1[6].Text.Trim() + "~" + multirowTD1[7].Text.Trim() + "~" + multirowTD1[8].Text.Trim();
                                        gc.insert_date(orderNumber, ParcelNumber, 1414, tax_HistoryCity, 1, DateTime.Now);
                                    }
                                }
                                //Year~Due Date~Type~Tax Amount~Balance~Interest~Total Due~Status

                            }
                        }
                        catch { }
                    }
                    //6) City of Acworth
                    if (TaxDistrict1.Contains("City of Acworth"))
                    {
                        try
                        {
                            driver.Navigate().GoToUrl("https://wipp.edmundsassoc.com/Wipp/?wippid=ACWR");
                            Thread.Sleep(2000);
                            //driver.set_page_load_timeout(30);

                            try
                            {
                                driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/div/table[1]/tbody/tr/td/div/a")).Click();
                                string id1 = ParcelNumber.Substring(0, 2);
                                string id11 = ParcelNumber.Substring(2, 6);
                                ParcelNumber = id1 + "-" + id11;
                            }
                            catch { }

                            driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/div/table[2]/tbody/tr/td/table/tbody/tr[2]/td/table/tbody/tr[2]/td[5]/input")).SendKeys(PropertyAddress);
                            gc.CreatePdf(orderNumber, ParcelNumber, "Taxinfo city", driver, "GA", "Cobb");
                            driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/div/table[2]/tbody/tr/td/table/tbody/tr[2]/td/table/tbody/tr[2]/td[6]/button")).SendKeys(Keys.Enter);
                            gc.CreatePdf(orderNumber, ParcelNumber, "Taxinfo city1", driver, "GA", "Cobb");
                            //string a = driver.FindElement(By.XPath(" /html/body/div[2]/div/table/tbody/tr[2]/td[2]/div/table/tbody/tr[1]/td/table/tbody/tr[2]/td[1]")).Text;
                            //          
                            string strTaxReal = "";
                            IWebElement ITaxReal11 = driver.FindElement(By.XPath("/html/body/div[2]/div/table/tbody/tr[2]/td[2]/div/table/tbody/tr[1]/td/table/tbody"));
                            IList<IWebElement> ITaxRealRow11 = ITaxReal11.FindElements(By.TagName("tr"));
                            IList<IWebElement> ITaxRealTd11;

                            foreach (IWebElement ItaxReal11 in ITaxRealRow11)
                            {

                                //  strTaxRealestate.Clear();
                                ITaxRealTd11 = ItaxReal11.FindElements(By.TagName("td"));
                                if (ItaxReal11.Text.Contains(PropertyAddress))
                                {

                                    //  string yearbill = ITaxRealTd[0].Text;
                                    IWebElement ITaxBillCount = ITaxRealTd11[0].FindElement(By.TagName("input"));
                                    strTaxReal = ITaxBillCount.GetAttribute("id");
                                    strTaxRealestate.Add(strTaxReal);

                                }
                            }
                            IWebElement element1 = driver.FindElement(By.Id(strTaxReal));
                            element1.Click();
                            Thread.Sleep(2000);

                            gc.CreatePdf(orderNumber, ParcelNumber, "Taxinfo city2", driver, "GA", "Cobb");



                            //Account Number~Owner Name~Property Address~Land Value~Improvement Value~Exempt Value~Total Assessed Value~Deductions~Last Paid Date

                            string Block = "", Taxidacc = "", OwnerName = "", PropertyAddress1 = "", LandValue = "", ImprovementValue = "", ExemptValue = "", TotalAssessedValue = "", Deductions = "", LastPaidDate = "";

                            int i = 0;
                            IWebElement tbmulti11 = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[1]/td/table/tbody/tr/td/table/tbody"));
                            IList<IWebElement> TRmulti11 = tbmulti11.FindElements(By.TagName("tr"));
                            IList<IWebElement> TDmulti11;
                            foreach (IWebElement row in TRmulti11)
                            {

                                TDmulti11 = row.FindElements(By.TagName("td"));
                                if (TDmulti11.Count == 4)
                                {
                                    if (i == 0)
                                    {
                                        Block = TDmulti11[1].Text;
                                        Taxidacc = TDmulti11[3].Text;
                                    }

                                    if (i == 1)
                                        PropertyAddress1 = TDmulti11[1].Text;
                                    if (i == 2)
                                    {
                                        OwnerName = TDmulti11[1].Text;
                                        LandValue = TDmulti11[3].Text;
                                    }
                                    if (i == 3)
                                        ImprovementValue = TDmulti11[3].Text;
                                    if (i == 4)
                                        ExemptValue = TDmulti11[3].Text;
                                    if (i == 5)
                                        TotalAssessedValue = TDmulti11[3].Text;
                                    if (i == 6)
                                        Deductions = TDmulti11[3].Text;
                                    i++;
                                }
                            }
                            LastPaidDate = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[3]/td/table/tbody/tr[2]/td/div/div/table/tbody/tr[3]/td/table/tbody/tr/td/div")).Text.Trim();
                            LastPaidDate = WebDriverTest.After(LastPaidDate, "Last Payment:").Trim();
                            string Tax_infoCity = Block + "~" + Taxidacc + "~" + OwnerName + "~" + PropertyAddress1 + "~" + LandValue + "~" + ImprovementValue + "~" + ExemptValue + "~" + TotalAssessedValue + "~" + Deductions + "~" + LastPaidDate;
                            gc.insert_date(orderNumber, ParcelNumber, 1416, Tax_infoCity, 1, DateTime.Now);


                            IWebElement multitableElement1 = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[3]/td/table/tbody/tr[2]/td/div/div/table/tbody/tr[2]/td/table/tbody"));
                            IList<IWebElement> multitableRow1 = multitableElement1.FindElements(By.TagName("tr"));
                            IList<IWebElement> multirowTD1;
                            foreach (IWebElement row in multitableRow1)
                            {
                                if (!row.Text.Contains("Year") && !row.Text.Contains("Last Payment:"))
                                {

                                    multirowTD1 = row.FindElements(By.TagName("td"));

                                    if (multirowTD1.Count != 0)
                                    {
                                        string tax_HistoryCity = multirowTD1[0].Text.Trim() + "~" + multirowTD1[1].Text.Trim() + "~" + multirowTD1[2].Text.Trim() + "~" + multirowTD1[3].Text.Trim() + "~" + multirowTD1[5].Text.Trim() + "~" + multirowTD1[6].Text.Trim() + "~" + multirowTD1[7].Text.Trim() + "~" + multirowTD1[8].Text.Trim();
                                        gc.insert_date(orderNumber, ParcelNumber, 1417, tax_HistoryCity, 1, DateTime.Now);
                                    }
                                }
                                //Year~Due Date~Type~Tax Amount~Balance~Interest~Total Due~Status
                            }
                        }
                        catch { }
                    }
                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");
                    TaxTime = DateTime.Now.ToString("HH:mm:ss");
                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "GA", "Cobb", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    GlobalClass.titleparcel = "";
                    gc.mergpdf(orderNumber, "GA", "Cobb");
                    return "Data Inserted Successfully";


                }
                catch (Exception ex)
                {
                    driver.Quit();
                    throw ex;
                }
            }
        }
        //Multiparcel Method
        private void multiparcel(string orderNumber, string address)
        {

            IWebElement Multiaddresstable = driver.FindElement(By.Id("searchResults"));
            IList<IWebElement> multiaddressrow = Multiaddresstable.FindElements(By.TagName("tr"));
            IList<IWebElement> Multiaddressid;
            List<string> searchlist = new List<string>();
            foreach (IWebElement Multiaddress in multiaddressrow)
            {
                Multiaddressid = Multiaddress.FindElements(By.TagName("td"));
                if (Multiaddressid.Count != 0 && !Multiaddress.Text.Contains("Parcel") && Multiaddressid.Count == 4 && !Multiaddressid[1].Text.Contains("P") & Multiaddress.Text.Trim().Contains(address.ToUpper().Trim()))
                {
                    string Multiparcelnumber = Multiaddressid[1].Text;
                    string OWnername = Multiaddressid[2].Text;
                    string Address1 = Multiaddressid[3].Text;
                    searchlist.Add(Multiaddressid[1].Text);
                    searchcount = searchlist.Count;
                    string multiaddressresult = OWnername + "~" + Address1;
                    gc.insert_date(orderNumber, Multiparcelnumber, 1383, multiaddressresult, 1, DateTime.Now);

                }
            }
        }
        //By Visible Method
        public void ByVisibleElement(IWebElement Element)
        {
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            js.ExecuteScript("arguments[0].scrollIntoView();", Element);
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

    }
}