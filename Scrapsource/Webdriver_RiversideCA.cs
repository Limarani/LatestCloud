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
using System.Web;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;

namespace ScrapMaricopa.Scrapsource
{
    public class Webdriver_RiversideCA
    {
        IWebDriver driver;
        DBconnection db = new DBconnection();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        GlobalClass gc = new GlobalClass();
        IWebElement Address1;
        public string FTP_Riverside(string address, string parcelNumber, string ownername, string searchType, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
            List<string> viewbilllist = new List<string>();
            List<string> Taxlistllist = new List<string>();
            List<string> Securitylist = new List<string>();
            int a = 0;
            string Parcel_number = "", Tax_Authority = "", taxyear = "", Securedtype = "", Propertyresult = "", LastYear = "", street1 = "", Addresshrf = "";
            // driver = new ChromeDriver();
            // driver = new PhantomJSDriver();
            using (driver = new PhantomJSDriver())
            {
                if (searchType == "titleflex")
                {
                    //string address = streetno + " " + direction + " " + streetname + " " + streettype;
                    gc.TitleFlexSearch(orderNumber, "", ownername, address.Trim(), "CA", "RiverSide");
                    if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                    {
                        driver.Quit();
                        return "MultiParcel";
                    }
                    else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                    {
                        HttpContext.Current.Session["Nodata_RiversideCA"] = "Yes";
                        driver.Quit();
                        return "No Data Found";
                    }
                    parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                    searchType = "parcel";
                }
                StartTime = DateTime.Now.ToString("HH:mm:ss");
                try
                {
                    if (searchType == "address")
                    {
                        driver.Navigate().GoToUrl("https://ca-riverside-ttc.publicaccessnow.com/Search/PropertyAddressSearch.aspx");
                        //*[@id='ngb-tab-0-panel']/span/tr-search-options/div/div/div/div[1]/div/div/div/input
                      
                        driver.FindElement(By.XPath("//*[@id='ngb-tab-0-panel']/div/span/tr-search-options/div/div/div/div[1]/div/div/div/input")).SendKeys(address);
                        gc.CreatePdf_WOP(orderNumber, "Search Before", driver, "CA", "Riverside");
                        //*[@id='ngb-tab-0-panel']/span/tr-search-options/div/div/div/div[2]/div/button
                        driver.FindElement(By.XPath("//*[@id='ngb-tab-0-panel']/div/span/tr-search-options/div/div/div/div[2]/div/button")).Click();
                        Thread.Sleep(10000);
                        gc.CreatePdf_WOP(orderNumber, "Search after", driver, "CA", "Riverside");
                        try
                        {
                            //*[@id='ngb-tab-0-panel']/span/tr-search-component/div/div
                            string Nodata = driver.FindElement(By.XPath("//*[@id='ngb-tab-0-panel']/div/span/tr-search-component/div/h4")).Text;
                            if (Nodata.Contains("No Result Found"))
                            {
                                HttpContext.Current.Session["Nodata_RiversideCA"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }
                    if (searchType == "parcel")
                    {
                        driver.Navigate().GoToUrl("https://ca-riverside-ttc.publicaccessnow.com/Search/AssessmentNumberSearch.aspx");
                        //*[@id='ngb-tab-0-panel']/span/tr-search-options/div/div/div/div[1]/div/div/div/tr-masked-text-input/input
                        driver.FindElement(By.XPath("  //*[@id='ngb-tab-0-panel']/div/span/tr-search-options/div/div/div/div[1]/div/div/div/tr-masked-text-input/input")).SendKeys(parcelNumber);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel", driver, "CA", "Riverside");
                        //*[@id='ngb-tab-0-panel']/span/tr-search-options/div/div/div/div[2]/div/button
                        driver.FindElement(By.XPath("//*[@id='ngb-tab-0-panel']/div/span/tr-search-options/div/div/div/div[2]/div/button")).Click();
                        Thread.Sleep(10000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel After", driver, "CA", "Riverside");
                        try
                        {
                            //*[@id='ngb-tab-0-panel']/span/tr-search-component/div/div
                            string Nodata = driver.FindElement(By.XPath("//*[@id='ngb-tab-0-panel']/div/span/tr-search-component/div/h4")).Text;
                            if (Nodata.Contains("No Result Found"))
                            {
                                HttpContext.Current.Session["Nodata_RiversideCA"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }
                    
                                                                  //*[@id="lxT413"]/table/tbody/tr[1]/td[1]
                    Parcel_number = driver.FindElement(By.XPath("//*[@id='lxT413']/table/tbody/tr[1]/td[1]")).Text;
                                                                       //*[@id="lxT413"]/table/tbody/tr[1]/td[2]
                    string propertytype = driver.FindElement(By.XPath("//*[@id='lxT413']/table/tbody/tr[1]/td[2]")).Text;
                    string Accountinformationtable = driver.FindElement(By.XPath("//*[@id='lxT413']/table/tbody")).Text;
                    string currentowner = gc.Between(Accountinformationtable, "Current Owner:", "Legal Description");
                    string LegalDescription = GlobalClass.After(Accountinformationtable, "Legal Description");
                    string Taxinfo = propertytype + "~" + currentowner + "~" + LegalDescription;
                    gc.insert_date(orderNumber, Parcel_number, 1739, Taxinfo, 1, DateTime.Now);
                    gc.CreatePdf(orderNumber, parcelNumber, "Secured", driver, "CA", "Riverside");
                    //Secured
                    try
                    {
                        IList<IWebElement> security = driver.FindElement(By.Id("414")).FindElements(By.TagName("table"));
                        for (int i = 1; i <= security.Count; i++)
                        {
                            //IWebElement Viewbill = driver.FindElement(By.XPath("//*[@id='414']/table[" + i + "]/thead/tr[1]/th")).FindElement(By.TagName("a"));
                            //string viewhref = Viewbill.GetAttribute("href");
                            //viewbilllist.Add(viewhref);
                            string Billheader = driver.FindElement(By.XPath("//*[@id='414']/table[" + i + "]/thead")).Text;
                            if (Billheader.Contains("VIEW BILL DETAIL BILL NUMBER:"))
                            {
                                a++;
                                string Billnumber = gc.Between(Billheader, "VIEW BILL DETAIL BILL NUMBER: ", " - ").Replace(":", "").Trim();
                                Securedtype = gc.Between(Billheader, " - ", "TAX YEAR : ").Trim();
                                taxyear = gc.Between(Billheader, "TAX YEAR : ", "TAX OTHER");
                                Taxlistllist.Add(taxyear);
                                Securitylist.Add(Securedtype);
                                IWebElement Viewbill = driver.FindElement(By.XPath("//*[@id='414']/table[" + i + "]/thead/tr[1]/th")).FindElement(By.TagName("a"));
                                string viewhref = Viewbill.GetAttribute("href");
                                viewbilllist.Add(viewhref);
                                IWebElement Installmenttable = driver.FindElement(By.XPath("//*[@id='414']/table[" + i + "]/tbody"));
                                IList<IWebElement> Installmentrow = Installmenttable.FindElements(By.TagName("tr"));
                                IList<IWebElement> Installmentid;
                                foreach (IWebElement Installment in Installmentrow)
                                {
                                    Installmentid = Installment.FindElements(By.TagName("td"));
                                    if (Installmentid.Count != 0)
                                    {
                                        string Instalmentresult = Billnumber + "~" + Securedtype + "~" + taxyear + "~" + Installmentid[0].Text + "~" + Installmentid[1].Text + "~" + Installmentid[2].Text + "~" + Installmentid[3].Text + "~" + Installmentid[4].Text + "~" + Installmentid[5].Text + "~" + Installmentid[6].Text;
                                        gc.insert_date(orderNumber, Parcel_number, 1740, Instalmentresult, 1, DateTime.Now);
                                    }
                                }
                            }
                            else
                            {
                                IWebElement Totaltable = driver.FindElement(By.XPath("//*[@id='414']/table[" + i + "]/tbody"));
                                IList<IWebElement> Totalrow = Totaltable.FindElements(By.TagName("tr"));
                                IList<IWebElement> Totalid;
                                foreach (IWebElement Installment in Totalrow)
                                {
                                    Totalid = Installment.FindElements(By.TagName("td"));
                                    if (Totalid.Count != 0)
                                    {
                                        string Totalresult = "";
                                        gc.insert_date(orderNumber, Parcel_number, 1740, Totalresult, 1, DateTime.Now);
                                    }
                                }
                            }
                        }
                    }
                    catch { }
                    int z = 0;
                    try
                    {
                        driver.FindElement(By.XPath("//*[@id='headingPaid']/button")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Secured Tax", driver, "CA", "Riverside");
                        IList<IWebElement> paidBill = driver.FindElement(By.Id("collapsePaid")).FindElements(By.TagName("table"));
                        // List<string> viewbilllist = new List<string>();
                        for (int i = 1; i <= paidBill.Count; i++)
                        {
                            string Billheader = driver.FindElement(By.XPath("//*[@id='collapsePaid']/div/table[" + i + "]/thead")).Text;
                            string paidowner = GlobalClass.Before(Billheader, "VIEW BILL DETAIL");
                            string Billnumber = gc.Between(Billheader, "BILL NUMBER:", "-").Trim();
                            string Securedtype1 = gc.Between(Billheader, "-", "TAX YEAR :").Trim();
                            string[] Securedarray = Securedtype1.Split('-');
                            string array2 = Securedarray[1].Trim();
                            string taxyear1 = gc.Between(Billheader, "TAX YEAR :", "TAX OTHER").Trim();
                            int cuttentyear = DateTime.Now.Year;
                            int current1 = DateTime.Now.Year - 1;
                            int current2 = DateTime.Now.Year - 2;
                            int Month = DateTime.Now.Month;
                            if (Month < 6)
                            {
                                cuttentyear--;
                                current1--;
                                current2--;
                            }

                            if (a == 0 || !Taxlistllist.Contains(taxyear1) || !Securitylist.Contains(Securedarray[1].Trim()))
                            {
                                if (Billheader.Contains("VIEW BILL DETAIL"))
                                {
                                    if (Convert.ToInt32(taxyear1) == cuttentyear || Convert.ToInt32(taxyear1) == current1 || Convert.ToInt32(taxyear1) == current2)
                                    {
                                        IWebElement Viewbill = driver.FindElement(By.XPath("//*[@id='collapsePaid']/div/table[" + i + "]/thead/tr[2]/th")).FindElement(By.TagName("a"));
                                        string viewhref = Viewbill.GetAttribute("href");
                                        viewbilllist.Add(viewhref);
                                    }
                                }

                                IWebElement Installmenttable = driver.FindElement(By.XPath("//*[@id='collapsePaid']/div/table[" + i + "]/tbody"));
                                IList<IWebElement> Installmentrow = Installmenttable.FindElements(By.TagName("tr"));
                                IList<IWebElement> Installmentid;
                                foreach (IWebElement Installment in Installmentrow)
                                {
                                    Installmentid = Installment.FindElements(By.TagName("td"));
                                    if (Installmentid.Count > 2)
                                    {
                                        string Instalmentresult = Billnumber + "~" + Securedtype1 + "~" + paidowner + "~" + taxyear1 + "~" + Installmentid[0].Text + "~" + Installmentid[1].Text + "~" + Installmentid[2].Text + "~" + Installmentid[3].Text + "~" + Installmentid[4].Text + "~" + Installmentid[5].Text + "~" + Installmentid[6].Text;
                                        gc.insert_date(orderNumber, Parcel_number, 1742, Instalmentresult, 1, DateTime.Now);
                                    }
                                    if (Installmentid.Count == 2)
                                    {
                                        string Instalmentresult = Billnumber + "~" + Securedtype1 + "~" + paidowner + "~" + taxyear1 + "~" + Installmentid[0].Text + "~" + "" + "~" + "" + "~" + Installmentid[1].Text + "~" + "" + "~" + "" + "~" + "";
                                        gc.insert_date(orderNumber, Parcel_number, 1742, Instalmentresult, 1, DateTime.Now);
                                    }
                                }
                            }
                        }
                    }
                    catch { }
                    //Defaulted Secured Tax
                    try
                    {
                        driver.FindElement(By.XPath("//*[@id='headingOne']/button")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Defaulted Secured Tax", driver, "CA", "Riverside");
                        IList<IWebElement> Default = driver.FindElement(By.Id("collapseOne")).FindElements(By.TagName("table"));
                        for (int i = 1; i <= Default.Count; i++)
                        {
                            string Billheader = driver.FindElement(By.XPath("//*[@id='collapseOne']/div/table[" + i + "]/thead")).Text;
                            if (Billheader.Contains("BILL NUMBER:"))
                            {
                                string paidowner = GlobalClass.Before(Billheader, "BILL NUMBER:");
                                string Billnumber = gc.Between(Billheader, "BILL NUMBER:", "-").Trim();
                                string Securedtype1 = gc.Between(Billheader, "-", "TAX YEAR :").Trim();
                                string taxyear1 = gc.Between(Billheader, "TAX YEAR :", "TAX OTHER").Trim();
                                int cuttentyear = DateTime.Now.Year;
                                int current1 = DateTime.Now.Year - 1;
                                int current2 = DateTime.Now.Year - 2;
                                int Month = DateTime.Now.Month;
                                if (Month < 6)
                                {
                                    cuttentyear--;
                                    current1--;
                                    current2--;
                                }
                                if (Billheader.Contains("VIEW BILL DETAIL BILL NUMBER:"))
                                {
                                    if (Convert.ToInt32(taxyear1) == cuttentyear || Convert.ToInt32(taxyear1) == current1 || Convert.ToInt32(taxyear1) == current2)
                                    {
                                        IWebElement Viewbill = driver.FindElement(By.XPath("//*[@id='collapsePaid']/div/table[" + i + "]/thead/tr[2]/th")).FindElement(By.TagName("a"));
                                        string viewhref = Viewbill.GetAttribute("href");
                                        viewbilllist.Add(viewhref);
                                    }
                                }
                                IWebElement Installmenttable = driver.FindElement(By.XPath("//*[@id='collapseOne']/div/table[" + i + "]/tbody"));
                                IList<IWebElement> Installmentrow = Installmenttable.FindElements(By.TagName("tr"));
                                IList<IWebElement> Installmentid;
                                foreach (IWebElement Installment in Installmentrow)
                                {
                                    Installmentid = Installment.FindElements(By.TagName("td"));
                                    if (Installmentid.Count > 2 & !Installment.Text.Contains("Total Bill"))
                                    {
                                        string Instalmentresult = Billnumber + "~" + Securedtype1 + "~" + paidowner + "~" + taxyear1 + "~" + Installmentid[0].Text + "~" + Installmentid[1].Text + "~" + Installmentid[2].Text + "~" + Installmentid[3].Text + "~" + Installmentid[4].Text + "~" + Installmentid[5].Text + "~" + Installmentid[6].Text;
                                        gc.insert_date(orderNumber, Parcel_number, 1750, Instalmentresult, 1, DateTime.Now);
                                    }
                                    if (Installmentid.Count == 2)
                                    {
                                        string Instalmentresult = Billnumber + "~" + Securedtype1 + "~" + paidowner + "~" + taxyear1 + "~" + Installmentid[0].Text + "~" + "" + "~" + "" + "~" + Installmentid[1].Text + "~" + "" + "~" + "" + "~" + "";
                                        gc.insert_date(orderNumber, Parcel_number, 1750, Instalmentresult, 1, DateTime.Now);
                                    }
                                    if (Installment.Text.Contains("Total Bill"))
                                    {
                                        string Instalmentresult = Billnumber + "~" + Securedtype1 + "~" + paidowner + "~" + taxyear1 + "~" + Installmentid[0].Text + "~" + Installmentid[1].Text + "~" + Installmentid[2].Text + "~" + Installmentid[3].Text + "~" + Installmentid[4].Text + "~" + Installmentid[5].Text + "~" + "";
                                        gc.insert_date(orderNumber, Parcel_number, 1750, Instalmentresult, 1, DateTime.Now);
                                    }
                                }
                            }
                            else
                            {
                                IWebElement Totaltable = driver.FindElement(By.XPath("//*[@id='accordionDefault']/table/tbody"));
                                IList<IWebElement> Totalrow = Totaltable.FindElements(By.TagName("tr"));
                                IList<IWebElement> Totalid;
                                foreach (IWebElement Installment in Totalrow)
                                {
                                    Totalid = Installment.FindElements(By.TagName("td"));
                                    if (Totalid.Count != 0)
                                    {
                                        string Totalresult = "" + "~" + "" + "~" + "" + "~" + "" + "~" + Totalid[0].Text + "~" + Totalid[1].Text + "~" + Totalid[2].Text + "~" + Totalid[3].Text + "~" + Totalid[4].Text + "~" + Totalid[5].Text;
                                        gc.insert_date(orderNumber, Parcel_number, 1750, Totalresult, 1, DateTime.Now);
                                    }
                                }
                            }
                        }
                    }
                    catch { }
                    foreach (string View in viewbilllist)
                    {
                        driver.Navigate().GoToUrl(View);
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Viwe Bill" + z, driver, "CA", "Riverside");
                        //Assessment Data
                        IWebElement AssessmentTable = driver.FindElement(By.XPath("//*[@id='lxT420']/table/tbody"));
                        IList<IWebElement> Assessmentrow = AssessmentTable.FindElements(By.TagName("tr"));
                        IList<IWebElement> Assessmentid;
                        foreach (IWebElement assessment in Assessmentrow)
                        {
                            Assessmentid = assessment.FindElements(By.XPath("td"));
                            if (Assessmentid.Count != 0)
                            {
                                string assmentresult = Assessmentid[0].Text + "~" + Assessmentid[1].Text;
                                gc.insert_date(orderNumber, Parcel_number, 1751, assmentresult, 1, DateTime.Now);
                            }
                        }
                        string Exemptiondata = driver.FindElement(By.Id("lxT421")).Text;
                        string exemptionresult = "Exemptions" + "~" + Exemptiondata;
                        gc.insert_date(orderNumber, Parcel_number, 1751, exemptionresult, 1, DateTime.Now);
                        //Taxing Authority
                        IWebElement TaxingTable = driver.FindElement(By.XPath("//*[@id='lxT423']/table/tbody"));
                        IList<IWebElement> Taxingrow = TaxingTable.FindElements(By.TagName("tr"));
                        IList<IWebElement> Taxingid;
                        IList<IWebElement> Taxingth;
                        foreach (IWebElement Taxing in Taxingrow)
                        {
                            Taxingid = Taxing.FindElements(By.XPath("td"));
                            Taxingth = Taxing.FindElements(By.XPath("th"));
                            if (Taxingid.Count > 3)
                            {
                                string Taxingresult = Taxingid[0].Text + "~" + Taxingid[1].Text + "~" + Taxingid[2].Text + "~" + Taxingid[3].Text;
                                gc.insert_date(orderNumber, Parcel_number, 1752, Taxingresult, 1, DateTime.Now);
                            }
                            if (Taxingid.Count == 3)
                            {
                                string Taxingresult = Taxingid[0].Text + "~" + "" + "~" + Taxingid[1].Text + "~" + Taxingid[2].Text;
                                gc.insert_date(orderNumber, Parcel_number, 1752, Taxingresult, 1, DateTime.Now);
                            }
                            if (Taxingth.Count == 1)
                            {
                                string Taxingresult = Taxingth[0].Text + "~" + "" + "~" + "" + "~" + "";
                                gc.insert_date(orderNumber, Parcel_number, 1752, Taxingresult, 1, DateTime.Now);
                            }
                            if (Taxingth.Count == 3)
                            {
                                string Taxingresult = Taxingth[0].Text + "~" + "" + "~" + Taxingth[1].Text + "~" + Taxingth[2].Text;
                                gc.insert_date(orderNumber, Parcel_number, 1752, Taxingresult, 1, DateTime.Now);
                            }
                        }
                        try
                        {
                            string Paybill = "", Paytype = "";
                            IList<IWebElement> Instalmentcount = driver.FindElement(By.Id("424")).FindElements(By.TagName("table"));
                            for (int s = 1; s <= Instalmentcount.Count; s++)
                            {
                                string Installmentresult = "", Installmentresult1 = "";
                                //*[@id="tblInstallment"]/tbody/tr[1]/th
                                string InstalmentTh = driver.FindElement(By.XPath("//*[@id='tblInstallment']/tbody/tr[1]/th")).Text;
                                string[] instalarray = InstalmentTh.Split('|');
                                string Payyear = GlobalClass.After(instalarray[0], "Payable Year:");
                                try
                                {
                                    Paybill = GlobalClass.After(instalarray[1], "Bill Number");
                                    Paytype = instalarray[2];
                                }
                                catch { }

                                gc.insert_date(orderNumber, Parcel_number, 1750, exemptionresult, 1, DateTime.Now);
                                //*[@id="tblInstallment"]/tbody
                                //*[@id="tblInstallment"]/tbody
                                //*[@id="tblInstallment"]/tbody/tr[2]
                                //*[@id="tblInstallment"]/tbody/tr[2]
                                IWebElement InstallmentTable = driver.FindElement(By.Id("tblInstallment"));
                                IList<IWebElement> Installmentrow = InstallmentTable.FindElements(By.TagName("tr"));
                                IList<IWebElement> Installmentid;
                                IList<IWebElement> Installmentth;
                                foreach (IWebElement Installment in Installmentrow)
                                {
                                    Installmentid = Installment.FindElements(By.XPath("td"));
                                    Installmentth = Installment.FindElements(By.XPath("th"));
                                    if (Installmentid.Count != 0)
                                    {
                                        Installmentresult = Installmentresult1 + "~" + Installmentid[0].Text + "~" + Installmentid[1].Text + "~" + Installmentid[2].Text + "~" + Installmentid[3].Text + "~" + Installmentid[4].Text + "~" + Installmentid[5].Text + "~" + Installmentid[6].Text;
                                        gc.insert_date(orderNumber, Parcel_number, 1753, Installmentresult, 1, DateTime.Now);
                                    }
                                    if (Installment.Text.Contains("Installment"))
                                    {
                                        Installmentresult1 = Payyear + "~" + Paybill + "~" + Paytype + "~" + Installmentth[0].Text;
                                        //gc.insert_date(orderNumber, Parcel_number, 1750, Taxingresult, 1, DateTime.Now);
                                    }
                                }
                            }
                        }
                        catch { }
                        z++;
                    }
                    driver.Quit();
                    gc.mergpdf(orderNumber, "CA", "RiverSide");
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