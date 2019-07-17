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
    public class WebDriver_FayetteKy
    {
        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        public string FTP_FayetteKy(string streetno, string direction, string streetname, string streettype, string unitno, string ownername, string parcelNumber, string searchType, string orderNumber)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            List<string> multiparcel = new List<string>();
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";


            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            using (driver = new PhantomJSDriver())
            {
                StartTime = DateTime.Now.ToString("HH:mm:ss");
                driver.Navigate().GoToUrl("http://qpublic9.qpublic.net/ky_fayette_search2.php");
                Thread.Sleep(2000);
                driver.FindElement(By.XPath("//*[@id='xr_xri']/div[1]/span[9]/a")).SendKeys(Keys.Enter);
                Thread.Sleep(2000);
                try
                {
                    if (searchType == "titleflex")
                    {
                        string address = streetno + " " + direction + " " + streetname + " " + streettype + "" + unitno;
                        gc.TitleFlexSearch(orderNumber, "", ownername, address, "KY", "Fayette");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_FayetteKy"] = "Zero";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }

                    if (searchType == "address")
                    {
                        driver.FindElement(By.XPath("/html/body/table[1]/tbody/tr/td[1]/ul/li[1]/b/a")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);

                        driver.FindElement(By.XPath("/html/body/form/table/tbody/tr/td[2]/table/tbody/tr[1]/td[2]/input")).SendKeys(streetno);
                        driver.FindElement(By.XPath("//*[@id='streetName']")).SendKeys(streetname);
                        gc.CreatePdf_WOP(orderNumber, "SearchAddressBefore", driver, "KY", "Fayette");
                        driver.FindElement(By.XPath("/html/body/form/table/tbody/tr/td[2]/table/tbody/tr[6]/td[2]/input")).SendKeys(Keys.Enter);
                        gc.CreatePdf_WOP(orderNumber, "SearchAddressAfter", driver, "KY", "Fayette");
                        Thread.Sleep(2000);
                        try
                        {
                            gc.CreatePdf_WOP(orderNumber, "Multiparcel Result", driver, "KY", "Fayette");
                            Thread.Sleep(2000);
                            string Owner = "", Address = "", MultiParcelNumber = "";
                            int Mcount = 0;
                            IWebElement multy = driver.FindElement(By.XPath("/html/body/table/tbody"));
                            IList<IWebElement> muladdress = multy.FindElements(By.TagName("tr"));
                            IList<IWebElement> mulid;
                            foreach (IWebElement addressrow in muladdress)
                            {
                                mulid = addressrow.FindElements(By.TagName("td"));
                                if (mulid.Count != 0 && !addressrow.Text.Contains("Return to Main Search") && !addressrow.Text.Contains("Search Criteria:") && !addressrow.Text.Contains("Search produced") && !addressrow.Text.Contains("Parcel Number") && !addressrow.Text.Contains("To create") && !addressrow.Text.Contains("No warranties") && !addressrow.Text.Contains(" Return to Main Search "))
                                {
                                    if (Mcount <= 29)
                                    {
                                        IWebElement Iparcel = mulid[0].FindElement(By.TagName("a"));
                                        MultiParcelNumber = Iparcel.Text;
                                        Owner = mulid[1].Text;
                                        Address = mulid[2].Text;
                                        string MultyInst = Address + "~" + Owner;
                                        gc.insert_date(orderNumber, MultiParcelNumber, 690, MultyInst, 1, DateTime.Now);
                                    }
                                    Mcount++;
                                }
                            }
                            if (Mcount > 29)
                            {
                                HttpContext.Current.Session["multiParcel_FayetteKy_Maximum"] = "Maximum";
                                return "Maximum";
                            }
                            else
                            {
                                HttpContext.Current.Session["multiparcel_FayetteKy"] = "Yes";
                            }
                            driver.Quit();
                            return "MultiParcel";
                        }
                        catch
                        {

                        }
                        gc.CreatePdf_WOP(orderNumber, "SearchAddressAfter", driver, "KY", "Fayette");
                        Thread.Sleep(2000);
                        try
                        {
                            //No Data Found
                            string nodata = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/font")).Text;
                            if (nodata.Contains("0 of 0 Results Returned"))
                            {
                                HttpContext.Current.Session["Nodata_FayetteKy"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }
                    if (searchType == "parcel")
                    {
                        driver.FindElement(By.XPath("html/body/table[1]/tbody/tr/td[1]/ul/li[3]/b/a")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);

                        driver.FindElement(By.XPath("html/body/form/table/tbody/tr/td/input[2]")).SendKeys(parcelNumber);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search", driver, "KY", "Fayette");
                        driver.FindElement(By.XPath("/html/body/form/table/tbody/tr/td/input[5]")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        try
                        {
                            //No Data Found
                            string nodata = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/font")).Text;
                            if (nodata.Contains("0 of 0 Results Returned"))
                            {
                                HttpContext.Current.Session["Nodata_FayetteKy"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }
                    //Property Details

                    string ParcelID = "", OwnerName = "", MailingAddress = "", Address1 = "", PropertyAddress = "", LegalDescription = "", PropertyClass = "", LandUseCode = "", MapBlock = "", Lot = "", SubDivision = "", TaxDistrict = "", TaxRate = "";

                    IWebElement tbmulti11 = driver.FindElement(By.XPath("/html/body/center[2]/table[2]/tbody"));
                    IList<IWebElement> TRmulti11 = tbmulti11.FindElements(By.TagName("tr"));
                    IList<IWebElement> TDmulti11;
                    foreach (IWebElement row in TRmulti11)
                    {
                        TDmulti11 = row.FindElements(By.TagName("td"));
                        if (TDmulti11.Count != 0)
                        {
                            if (row.Text.Contains("Owner Name"))
                            {
                                OwnerName = TDmulti11[1].Text;
                            }
                            if (row.Text.Contains("Parcel Number"))
                            {
                                ParcelID = TDmulti11[3].Text.Trim();
                            }
                            if (row.Text.Contains("Mailing Address"))
                            {
                                MailingAddress = TDmulti11[1].Text;
                            }
                            if (row.Text.Contains("Tax District"))
                            {
                                TaxDistrict = TDmulti11[3].Text;
                                Address1 = TDmulti11[1].Text;
                            }
                            if (row.Text.Contains("Location Address"))
                            {
                                PropertyAddress = TDmulti11[1].Text;
                            }
                            if (row.Text.Contains("Legal Description"))
                            {
                                LegalDescription = TDmulti11[1].Text;
                            }
                            if (row.Text.Contains("Property Class"))
                            {
                                PropertyClass = TDmulti11[1].Text;
                            }
                            if (row.Text.Contains("Land Use Code"))
                            {
                                LandUseCode = TDmulti11[1].Text;
                            }
                            if (row.Text.Contains("Map Block"))
                            {
                                MapBlock = TDmulti11[1].Text;
                            }
                            if (row.Text.Contains("Lot"))
                            {
                                Lot = TDmulti11[1].Text;
                            }
                            if (row.Text.Contains("Subdivision"))
                            {
                                SubDivision = TDmulti11[1].Text;
                            }
                            if (row.Text.Contains("Tax Rate"))
                            {
                                TaxRate = TDmulti11[3].Text;
                            }
                        }
                    }
                    string YearBuilt = "";
                    try
                    {
                        YearBuilt = driver.FindElement(By.XPath("/html/body/center[2]/table[4]/tbody/tr[3]/td[5]")).Text;

                    }
                    catch { }
                    string propertydetails = ParcelID + "~" + OwnerName + "~" + MailingAddress + " " + Address1 + "~" + PropertyAddress + "~" + LegalDescription + "~" + PropertyClass + "~" + LandUseCode + "~" + MapBlock + "~" + Lot + "~" + SubDivision + "~" + TaxDistrict + "~" + TaxRate + "~" + YearBuilt;
                    gc.insert_date(orderNumber, ParcelID, 649, propertydetails, 1, DateTime.Now);
                    //********Tax Assessment Details*******
                    int i = 0;
                    string assesstitle = "", assessvalue = "";
                    gc.CreatePdf(orderNumber, ParcelID, "Assessment Details", driver, "KY", "Fayette");
                    Thread.Sleep(3000);
                    IWebElement IassessmentTable = driver.FindElement(By.XPath("/html/body/center[2]/table[3]/tbody"));
                    IList<IWebElement> IassessmentRow = IassessmentTable.FindElements(By.TagName("tr"));
                    IList<IWebElement> Iassessmenttd;
                    foreach (IWebElement assess in IassessmentRow)
                    {
                        Iassessmenttd = assess.FindElements(By.TagName("td"));
                        if (Iassessmenttd.Count != 0 && !assess.Text.Contains("Assessment Information"))
                        {
                            for (int j = 0; j < Iassessmenttd.Count; j++)
                            {
                                assessvalue += Iassessmenttd[j].Text + "~";
                            }
                            if (i == 0)
                            {
                                db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + assessvalue.Remove(assessvalue.Length - 1, 1) + "' where Id = '" + 655 + "'");
                                assessvalue = "";
                            }
                            if (i == 1)
                            {
                                gc.insert_date(orderNumber, ParcelID, 655, assessvalue.Remove(assessvalue.Length - 1, 1), 1, DateTime.Now);
                            }
                            i++;
                        }
                    }
                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");
                    //*******Tax information Details*******
                    string Delinquent = "";
                    try
                    {
                        string currentyear = "";
                        int year = DateTime.Now.Year;
                        int increase = 0;
                        for (int l = 0; l < 7; l++)
                        {
                            if (increase < 3)
                            {
                                try
                                {

                                    currentyear = Convert.ToString(year);
                                    driver.Navigate().GoToUrl("http://fayettesheriff.com/property_taxes_lookup.php");
                                    Thread.Sleep(2000);
                                    gc.CreatePdf(orderNumber, ParcelID, "Tax Year Choosed", driver, "KY", "Fayette");
                                    IWebElement ISelect = driver.FindElement(By.XPath("/html/body/div/div/div[1]/p/select"));
                                    SelectElement sSelect = new SelectElement(ISelect);
                                    sSelect.SelectByText(currentyear);
                                    driver.FindElement(By.XPath("//*[@id='table2']/tbody/tr[3]/td[2]/blockquote/form[2]/p[1]/input")).SendKeys(ParcelID);
                                    gc.CreatePdf(orderNumber, ParcelID, "Tax Search  Input Passed", driver, "KY", "Fayette");
                                    driver.FindElement(By.XPath("//*[@id='table2']/tbody/tr[3]/td[2]/blockquote/form[2]/p[2]/input")).SendKeys(Keys.Enter);
                                    Thread.Sleep(2000);
                                    gc.CreatePdf(orderNumber, ParcelID, "Tax Search Result" + currentyear, driver, "KY", "Fayette");
                                    try
                                    {
                                        string taxinfotitle = "", taxinfovalue = "";
                                        IWebElement ITaxinfoTable = driver.FindElement(By.XPath("//*[@id='table2']/tbody/tr[3]/td[2]/table/tbody/tr/td/p[1]/table/tbody"));
                                        IList<IWebElement> ItaxinfoRow = ITaxinfoTable.FindElements(By.TagName("tr"));
                                        IList<IWebElement> ItaxinfoTD;
                                        IList<IWebElement> ItaxinfoTH;
                                        foreach (IWebElement rowid in ItaxinfoRow)
                                        {
                                            ItaxinfoTH = rowid.FindElements(By.TagName("th"));
                                            ItaxinfoTD = rowid.FindElements(By.TagName("td"));
                                            if (ItaxinfoTH.Count != 0 && rowid.Text != "")
                                            {
                                                for (int m = 0; m < ItaxinfoTH.Count; m++)
                                                {
                                                    if (ItaxinfoTH[m].Text != "")
                                                    {
                                                        taxinfotitle += ItaxinfoTH[m].Text.Replace("\r\n", " & ").Trim() + "~";
                                                    }
                                                }
                                            }
                                            if (ItaxinfoTD.Count != 0 && rowid.Text != "")
                                            {
                                                for (int n = 0; n < ItaxinfoTD.Count; n++)
                                                {
                                                    if (ItaxinfoTD[n].Text != "")
                                                    {
                                                        taxinfovalue += ItaxinfoTD[n].Text.Replace("\r\n", " & ").Trim() + "~";
                                                    }
                                                }
                                            }
                                        }
                                        string Taxyear1 = "", Taxyear = "", TaxingAuthority = "";

                                        string bulktext = driver.FindElement(By.XPath("//*[@id='table2']/tbody/tr[3]/td[2]/table/tbody/tr/td/span[2]")).Text;
                                        Taxyear1 = GlobalClass.Before(bulktext, "- ").Trim();
                                        Taxyear = GlobalClass.After(Taxyear1, ",").Trim();

                                        if (taxinfovalue.Contains("Due") && !taxinfovalue.Contains("Paid"))
                                        {
                                            Delinquent = "Yes";
                                        }

                                        TaxingAuthority = driver.FindElement(By.XPath("//*[@id='table2']/tbody/tr[3]/td[1]/font")).Text;
                                        db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + taxinfotitle + "Taxyear" + "~" + "TaxingAuthority" + "' where Id = '" + 657 + "'");
                                        gc.insert_date(orderNumber, ParcelID, 657, taxinfovalue + Taxyear + "~" + TaxingAuthority, 1, DateTime.Now);
                                    }
                                    catch
                                    {

                                    }
                                    increase++;
                                }
                                catch { }
                            }
                            year--;
                        }

                        try
                        {
                            driver.Navigate().GoToUrl("https://www.fayettecountyclerk.com/web/landrecords/delinquenttaxes/delinquentTaxCalc.htm");
                            Thread.Sleep(2000);
                            driver.FindElement(By.XPath("//*[@id='form1']/table/tbody/tr[1]/td/input")).SendKeys(ParcelID);
                            Thread.Sleep(2000);
                            gc.CreatePdf(orderNumber, ParcelID, "Delinquent details ", driver, "KY", "Fayette");
                            driver.FindElement(By.XPath("//*[@id='form1']/table/tbody/tr[2]/td/input[4]")).SendKeys(Keys.Enter);
                            Thread.Sleep(2000);
                            gc.CreatePdf(orderNumber, ParcelID, "Delinquent Details InputPassedPdf", driver, "KY", "Fayette");
                        }
                        catch { }

                        //Delinquent Tax information Details
                        if (Delinquent == "Yes")
                        {
                            try
                            {
                                driver.Navigate().GoToUrl("https://www.fayettecountyclerk.com/web/landrecords/delinquenttaxes/delinquentTaxCalc.htm");
                                Thread.Sleep(2000);
                                driver.FindElement(By.XPath("//*[@id='form1']/table/tbody/tr[1]/td/input")).SendKeys(ParcelID);
                                Thread.Sleep(2000);
                                gc.CreatePdf(orderNumber, ParcelID, "Delinquent site InputPassed", driver, "KY", "Fayette");
                                driver.FindElement(By.XPath("//*[@id='form1']/table/tbody/tr[2]/td/input[4]")).SendKeys(Keys.Enter);
                                Thread.Sleep(2000);
                                gc.CreatePdf(orderNumber, ParcelID, "Delinquent Details Displayed", driver, "KY", "Fayette");

                                string Year = "", BillNumber = "", Account = "", Paid = "", Assigned = "", Owner = "";
                                IWebElement tbmulti12 = driver.FindElement(By.XPath("//*[@id='subfile']/tbody"));
                                IList<IWebElement> TRmulti12 = tbmulti12.FindElements(By.TagName("tr"));
                                IList<IWebElement> TDmulti12;
                                foreach (IWebElement row in TRmulti12)
                                {
                                    TDmulti12 = row.FindElements(By.TagName("td"));
                                    if (TDmulti12.Count != 0 && row.Text.Contains(ParcelID))
                                    {
                                        Year = TDmulti12[1].Text;
                                        BillNumber = TDmulti12[2].Text;
                                        Account = TDmulti12[3].Text;
                                        Paid = TDmulti12[4].Text;
                                        Assigned = TDmulti12[5].Text;
                                        Owner = TDmulti12[6].Text;

                                        string DelinquentTaxinformationDetails = Year + "~" + BillNumber + "~" + Account + "~" + Paid + "~" + Assigned + "~" + Owner;
                                        gc.insert_date(orderNumber, ParcelID, 674, DelinquentTaxinformationDetails, 1, DateTime.Now);
                                    }
                                }
                            }
                            catch
                            {

                            }
                        }
                    }
                    catch { }

                    TaxTime = DateTime.Now.ToString("HH:mm:ss");
                    driver.Quit();
                    gc.mergpdf(orderNumber, "KY", "Fayette");
                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "KY", "Fayette", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);
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