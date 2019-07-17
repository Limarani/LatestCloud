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
using System.Text;
using OpenQA.Selenium.Firefox;

namespace ScrapMaricopa.Scrapsource
{
    public class Webdriver_SCBerkeley
    {
        string outputPath = "";
        IWebElement p_no;
        IWebDriver driver;
        DBconnection db = new DBconnection();

        GlobalClass gc = new GlobalClass();
        MySqlParameter[] mParam;
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());

        public string FTP_SCBerkeley(string houseno, string sname, string unitno, string parcelNumber, string searchType, string orderNumber, string ownername, string directParcel)
        {
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "", AssessTakenTime = "", TaxTakentime = "", CityTaxtakentime = "";
            string TotaltakenTime = "";

            List<string> strTaxRealestate = new List<string>();
            List<string> strTaxRealestate1 = new List<string>();
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            //IWebElement iframeElement1;
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            using (driver = new PhantomJSDriver())
            {
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");
                    driver.Navigate().GoToUrl("https://www.berkeleycountysc.gov/propcards/prop_card_search.php");
                    Thread.Sleep(7000);
                    if (searchType == "address")
                    {
                        driver.FindElement(By.Name("streetnum")).SendKeys(houseno);
                        driver.FindElement(By.Name("streetname")).SendKeys(sname);
                        gc.CreatePdf_WOP(orderNumber, "Address search", driver, "SC", "Berkeley");
                        driver.FindElement(By.XPath("/html/body/div/form[2]/center/input[6]")).SendKeys(Keys.Enter);
                        Thread.Sleep(7000);
                        gc.CreatePdf_WOP(orderNumber, "Address1", driver, "SC", "Berkeley");
                        try
                        {


                            string Parcelhref = "";
                            //   gc.CreatePdf_WOP(orderNumber, "Address2", driver, "MD", "Harford");
                            IWebElement multitableElement11 = driver.FindElement(By.XPath("//*[@id='egtable']/tbody"));
                            IList<IWebElement> multitableRow11 = multitableElement11.FindElements(By.TagName("tr"));
                            IList<IWebElement> multirowTD11;
                            int i = 0;

                            foreach (IWebElement row in multitableRow11)
                            {
                                if (i < 25)
                                {
                                    if (!row.Text.Contains("Owner Name") && !row.Text.Contains("Export results"))
                                    {
                                        multirowTD11 = row.FindElements(By.TagName("td"));
                                        if (multirowTD11.Count != 1 && multirowTD11[0].Text.Trim() != "")
                                        {
                                            p_no = multirowTD11[0].FindElement(By.TagName("a"));
                                            string Parceln = multirowTD11[0].Text.Trim();
                                            Parcelhref = p_no.GetAttribute("href");
                                            string multi_parcel = multirowTD11[1].Text.Trim() + "~" + multirowTD11[2].Text.Trim();
                                            gc.insert_date(orderNumber, Parceln, 451, multi_parcel, 1, DateTime.Now);
                                            i++;
                                        }
                                    }
                                }

                            }
                            if (i == 1)
                            {
                                driver.Navigate().GoToUrl(Parcelhref);
                                Thread.Sleep(2000);
                            }
                            if (i < 26 & i > 1)
                            {
                                HttpContext.Current.Session["multiParcel_SCBerkeley_Multicount"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }

                        }
                        catch { }
                    }

                    if (searchType == "titleflex")
                    {
                        string titleaddress = houseno + " " + sname + " " + unitno;
                        gc.TitleFlexSearch(orderNumber, "", "", titleaddress, "SC", "Berkeley");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_SCBerkely"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }

                    if (searchType == "parcel")
                    {
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();

                        }
                        if (parcelNumber.Contains("-"))
                        {
                            parcelNumber = parcelNumber.Replace("-", "");
                        }

                        driver.FindElement(By.XPath("/html/body/div/form[1]/center/input[1]")).SendKeys(parcelNumber);
                        gc.CreatePdf(orderNumber, parcelNumber, "parcel search", driver, "SC", "Berkeley");

                        driver.FindElement(By.XPath("/html/body/div/form[1]/center/input[2]")).SendKeys(Keys.Enter);
                        Thread.Sleep(5000);
                        try
                        {
                            IWebElement Inodata = driver.FindElement(By.XPath("/html/body"));
                            if (Inodata.Text.Contains("not found"))
                            {
                                HttpContext.Current.Session["Nodata_SCBerkely"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }


                    if (searchType == "ownername")
                    {

                        driver.FindElement(By.Name("srchname")).SendKeys(ownername);


                        driver.FindElement(By.XPath("/html/body/table[1]/tbody/tr[3]/td/div/table/tbody/tr[2]/td[2]/table/tbody/tr[2]/td/form/table/tbody/tr/td/table/tbody/tr[8]/td/input")).SendKeys(Keys.Enter);

                        Thread.Sleep(7000);
                        gc.CreatePdf_WOP(orderNumber, "owner name search", driver, "SC", "Berkeley");
                        try
                        {
                            int iRowsCount = driver.FindElements(By.XPath("/html/body/table[1]/tbody/tr[3]/td/div/table/tbody/tr[3]/td/table/tbody/tr")).Count;
                            if (iRowsCount > 3)
                            {

                                //   gc.CreatePdf_WOP(orderNumber, "Address2", driver, "MD", "Harford");
                                IWebElement multitableElement11 = driver.FindElement(By.XPath("/html/body/table[1]/tbody/tr[3]/td/div/table/tbody/tr[3]/td/table/tbody"));
                                IList<IWebElement> multitableRow11 = multitableElement11.FindElements(By.TagName("tr"));
                                IList<IWebElement> multirowTD11;
                                int i = 0;

                                foreach (IWebElement row in multitableRow11)
                                {
                                    if (i < 25)
                                    {
                                        if (!row.Text.Contains("Owner Name") && !row.Text.Contains("Export results"))
                                        {
                                            multirowTD11 = row.FindElements(By.TagName("td"));
                                            if (multirowTD11.Count != 1 && multirowTD11[0].Text.Trim() != "")
                                            {
                                                //string p_no = multirowTD11[1].Text.Trim();

                                                // string multi_parcel = multirowTD11[2].Text.Trim() + "~" + multirowTD11[4].Text.Trim();
                                                //gc.insert_date(orderNumber, p_no, 451, multi_parcel, 1, DateTime.Now);
                                                //             Owner Name~Address
                                            }
                                        }
                                    }
                                    i++;
                                }
                                HttpContext.Current.Session["multiparcel_SCBerkeley"] = "Yes";
                                if (iRowsCount > 25)
                                {
                                    HttpContext.Current.Session["multiParcel_SCBerkeley_Multicount"] = "Maximum";
                                }
                                driver.Quit();
                                // gc.mergpdf(orderNumber);
                                return "MultiParcel";
                            }

                            else
                            {
                                try
                                {
                                    driver.FindElement(By.XPath("/html/body/table[1]/tbody/tr[3]/td/div/table/tbody/tr[3]/td/table/tbody/tr[3]/td[2]/a")).Click();
                                    Thread.Sleep(3000);
                                }
                                catch { }
                            }


                        }
                        catch { }
                    }
                    //407 MOSSY WOOD RD
                    //property_details
                    try
                    {
                        IWebElement Inodata = driver.FindElement(By.XPath("//*[@id='egtable']"));
                        IList<IWebElement> InodataRow = Inodata.FindElements(By.TagName("td"));
                        if (InodataRow.Count == 0)
                        {
                            HttpContext.Current.Session["Nodata_SCBerkely"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }

                    string Parcel_ID = "", Owner_Name = "", Property_Address = "", Year_Built = "";
                    string parceltable = driver.FindElement(By.XPath("/html/body/table[1]")).Text;
                    Parcel_ID = gc.Between(parceltable, "TMS:", "Owner Information:").Trim();
                    gc.CreatePdf(orderNumber, Parcel_ID, "property", driver, "SC", "Berkeley");
                    ByVisibleElement(driver.FindElement(By.XPath("/html/body/h3[2]/center")));
                    gc.CreatePdf(orderNumber, Parcel_ID, "taxinforhalf1", driver, "SC", "Berkeley");

                    string Ownr_table = "";
                    if (parceltable.Contains("Council"))
                    {
                        Ownr_table = gc.Between(parceltable.Replace("\r\n", "~").Replace("~~", "~"), "Owner Information:", "Council").TrimEnd('~').TrimStart('~');
                    }
                    else
                    {
                        Ownr_table = GlobalClass.After(parceltable, "Owner Information:");
                    }
                    string[] Splitowner = Ownr_table.Split('~');
                    if (Splitowner.Count() == 3)
                    {
                        Owner_Name = Splitowner[0];
                        Property_Address = Splitowner[1] + " " + Splitowner[2];
                    }
                    if (Splitowner.Count() == 2)
                    {
                        Owner_Name = Splitowner[0];
                        Property_Address = Splitowner[1];
                    }
                    try
                    {
                        string yeartable = driver.FindElement(By.XPath("/html/body/h3[7]")).Text;
                        Year_Built = gc.Between(yeartable, "Year Built:", "Depreciation").Trim();
                    }
                    catch { }
                    string summaryinfo2 = driver.FindElement(By.XPath("/html/body/table[1]")).Text;
                    string CouncilDistrict = gc.Between(summaryinfo2, "Council District:", "Fire District:");
                    string FireDistrict = gc.Between(summaryinfo2, "Fire District:", "Tax District:");
                    string TaxDistrict = gc.Between(summaryinfo2, "Tax District:", "TIS Zone:");
                    string Jurisdiction = gc.Between(summaryinfo2, "Jurisdiction:", "Acres:");
                    string Acres = gc.Between(summaryinfo2, "Acres:", "Lots:");
                    string Lots = GlobalClass.After(summaryinfo2, "Lots:");
                    string summaryinfo3 = driver.FindElement(By.XPath("/html/body/table[1]")).Text;
                    string HomesteadExempt = gc.Between(summaryinfo3, "Homestead Exempt:", "Parent TMS:");
                    string property_details = Owner_Name + "~" + Property_Address + "~" + CouncilDistrict + "~" + FireDistrict + "~" + TaxDistrict + "~" + Jurisdiction + "~" + Acres + "~" + Lots + "~" + HomesteadExempt + "~" + Year_Built;
                    gc.insert_date(orderNumber, Parcel_ID, 448, property_details, 1, DateTime.Now);
                    //taxinfo
                    //driver.FindElement(By.XPath("/html/body/table[1]/tbody/tr[3]/td/div/table/tbody/tr[2]/td/table/tbody/tr[1]/td[3]/a")).Click();
                    //Thread.Sleep(3000);             

                    IWebElement tbmulti = driver.FindElement(By.XPath("/html/body/div[1]/table/tbody"));
                    IList<IWebElement> TRmulti = tbmulti.FindElements(By.TagName("tr"));
                    IList<IWebElement> TDmulti;
                    foreach (IWebElement row in TRmulti)
                    {
                        if (!row.Text.Contains("Tax Year"))
                        {
                            TDmulti = row.FindElements(By.TagName("th"));
                            if (TDmulti.Count == 8)
                            {
                                string tax_details = TDmulti[0].Text + "~" + TDmulti[1].Text + "~" + TDmulti[2].Text + "~" + TDmulti[3].Text + "~" + TDmulti[4].Text + "~" + TDmulti[5].Text + "~" + TDmulti[6].Text + "~" + TDmulti[7].Text;
                                gc.insert_date(orderNumber, Parcel_ID, 450, tax_details, 1, DateTime.Now);

                                //    Tax Year~Receipt  Number~Assessed Value~Tax Amount~Due Date~Paid Date~Owner Name~Status
                            }
                        }
                    }
                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");

                    //property card 
                    string valucationtable = "";
                    try
                    {
                        try
                        {
                            valucationtable = driver.FindElement(By.XPath("/html/body/table[7]/tbody")).Text;
                        }
                        catch { }
                        if (!valucationtable.Contains("Building Market:"))
                        {
                            valucationtable = driver.FindElement(By.XPath("/html/body/table[8]/tbody")).Text;
                        }
                    }
                    catch { }

                    string BuildingMarket = "", LandMarket = "", BuildingTaxable1 = "", BuildingTaxable2 = "", BuildingTaxable3 = "", BuildingTaxable4 = "", LandTaxable1 = "", LandTaxable2 = "", LandTaxable3 = "", LandTaxable4 = "";

                    BuildingMarket = gc.Between(valucationtable, "Building Market:", "Land Market:");
                    LandMarket = gc.Between(valucationtable, "Land Market:", "Building Taxable (4% Res):");

                    BuildingTaxable1 = gc.Between(valucationtable, "Building Taxable (4% Res):", "Building Taxable (6% Other):");

                    BuildingTaxable2 = gc.Between(valucationtable, "Building Taxable (6% Other):", "Building Taxable (4% Ag):");
                    //TotalAssessedValue = WebDriverTest.After(TotalAssessedValue, "Total Assessment:").Trim();
                    BuildingTaxable3 = gc.Between(valucationtable, "Building Taxable (4% Ag):", "Building Taxable (6% Ag):");
                    BuildingTaxable4 = gc.Between(valucationtable, "Building Taxable (6% Ag):", "Land Taxable (4% Res):");
                    LandTaxable1 = gc.Between(valucationtable, "Land Taxable (4% Res):", "Land Taxable (6% Other):");
                    LandTaxable2 = gc.Between(valucationtable, "Land Taxable (6% Other):", "Land Taxable (4% Ag):");
                    LandTaxable3 = gc.Between(valucationtable, "Land Taxable (4% Ag):", "Land Taxable (6% Ag):");
                    LandTaxable4 = gc.Between(valucationtable, "Land Taxable (6% Ag):", "Total Taxable Value:");

                    string totalvalues = "";
                    try
                    {
                        try
                        {
                            totalvalues = driver.FindElement(By.XPath("/html/body/table[8]/tbody/tr[2]")).Text;
                        }
                        catch { }
                        if (!totalvalues.Contains("Total Taxable Value"))
                        {
                            totalvalues = driver.FindElement(By.XPath("/html/body/table[7]/tbody/tr[2]/td[2]")).Text;
                        }
                    }
                    catch { }
                    string totaltaxable = gc.Between(totalvalues, "Total Taxable Value:", "Total Assessment:");
                    string totalAssessment = GlobalClass.After(totalvalues, "Total Assessment:");
                    string assessment_details = BuildingMarket + "~" + LandMarket + "~" + BuildingTaxable1 + "~" + BuildingTaxable2 + "~" + BuildingTaxable3 + "~" + BuildingTaxable4 + "~" + LandTaxable1 + "~" + LandTaxable2 + "~" + LandTaxable3 + "~" + LandTaxable4 + "~" + totaltaxable + "~" + totalAssessment;
                    gc.insert_date(orderNumber, Parcel_ID, 449, assessment_details, 1, DateTime.Now);
                    // Sales information
                    string salesinformation1 = "";
                    try
                    {
                        try
                        {
                            salesinformation1 = driver.FindElement(By.XPath("/html/body/table[6]/tbody/tr/td[1]")).Text;
                        }
                        catch { }
                        if (salesinformation1.Contains("Choose the tax year of the notice you would like to reprint"))
                        {
                            salesinformation1 = driver.FindElement(By.XPath("/html/body/table[7]/tbody")).Text;
                        }
                    }
                    catch { }
                    string Lastsale = gc.Between(salesinformation1, "Last Sale Date:", "Recording Date:");
                    string recordingdate = gc.Between(salesinformation1, "Recording Date:", "Sale Price:");
                    string saleprice = GlobalClass.After(salesinformation1, "Sale Price:");
                    string salesinformation2 = "";
                    try
                    {
                        try
                        {
                            salesinformation2 = driver.FindElement(By.XPath("/html/body/table[6]/tbody/tr/td[2]")).Text;
                        }
                        catch { }
                        if (!salesinformation2.Contains("Plat Information"))
                        {
                            salesinformation2 = driver.FindElement(By.XPath("/html/body/table[7]/tbody/tr/td[2]")).Text;
                        }
                    }
                    catch { }
                    string PlatInformation = gc.Between(salesinformation2, "Plat Information:", "Deed Book:");
                    string Deedbook = gc.Between(salesinformation2, "Deed Book:", "Deed Page:");
                    string DeedPage = GlobalClass.After(salesinformation2, "Deed Page:");

                    string salesinformation3 = "";
                    try
                    {
                        try
                        {
                            salesinformation3 = driver.FindElement(By.XPath("/html/body/table[6]/tbody/tr/td[3]")).Text;
                        }
                        catch { }
                        if (!salesinformation3.Contains("Sales Validity"))
                        {
                            salesinformation3 = driver.FindElement(By.XPath("/html/body/table[7]/tbody/tr/td[3]")).Text;
                        }
                    }
                    catch { }

                    string SalesValidity = gc.Between(salesinformation3, "Sales Validity:", "Validity Other:");
                    string ValidityOther = GlobalClass.After(salesinformation3, "Validity Other:");
                    string Saleinforesult = Lastsale + "~" + recordingdate + "~" + saleprice + "~" + PlatInformation + "~" + Deedbook + "~" + DeedPage + "~" + SalesValidity + "~" + ValidityOther;
                    gc.insert_date(orderNumber, Parcel_ID, 1387, Saleinforesult, 1, DateTime.Now);
                    TaxTime = DateTime.Now.ToString("HH:mm:ss");
                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "SC", "Berkeley", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);
                    driver.Quit();
                    GlobalClass.titleparcel = "";
                    gc.mergpdf(orderNumber, "SC", "Berkeley");
                    return "Data Inserted Successfully";
                }

                catch (Exception ex)
                {
                    driver.Quit();
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