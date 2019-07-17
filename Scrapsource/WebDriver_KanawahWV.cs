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
    public class WebDriver_KanawhaWV
    {

        string Tax_Authority = "", Account_Number = "", Pro_District = "", Owner_Name = "", Pro_Address = "", Legel_Desp = "", Pro_Map = "", Property_Details = "";
        string Home_Exp = "-", Back_Tax = "-", Exoneration = "-", Propr_del = "-";
        string Tax_Year = "", TaxoffYear = "", Ticket_Numbet = "", Tax_Class = "", Spl_Desp = "-", First_Half = "", Second_Half = "", Total_Due = "", Tax_Details = "";
        string Installment = "", FirstHlf = "", Scndhlf = "", Payment_details = "";
        string Assessment = "", Gross = "", Net = "";
        string address = "", strMulticount = "", StrSingleAddress = "";
        string Type = "", TaxPyrName = "", Address = "", Legel_Desp1 = "", Legel_Desp2 = "";

        List<string> SingleAddress = new List<string>();
        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        public string FTP_KanawhaWV(string houseno, string streetname, string stype, string unitno, string direction, string ownername, string parcelNumber, string searchType, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            List<string> multiparcel = new List<string>();
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";

            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            //driver = new PhantomJSDriver();
            // driver = new ChromeDriver();
            using (driver = new PhantomJSDriver()) //ChromeDriver
            {
                StartTime = DateTime.Now.ToString("HH:mm:ss");

                driver.Navigate().GoToUrl("http://kanawha.digitalassessor.com/portal/search_all.php");
                Thread.Sleep(2000);
                try
                {

                    if (searchType == "titleflex")
                    {
                        string titleaddress = houseno + " " + direction + " " + streetname + " " + stype + " " + unitno;
                        gc.TitleFlexSearch(orderNumber, "", "", titleaddress, "WV", "Kanawha");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_KanawhaWV"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }

                    if (searchType == "address")
                    {
                        driver.FindElement(By.LinkText("Parcel Physical Location")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);

                        driver.FindElement(By.XPath("//*[@id='PHYSICAL_LOCATION']/form/table/tbody/tr[3]/td[1]/table/tbody/tr/td[2]/input")).SendKeys(houseno);
                        driver.FindElement(By.XPath("//*[@id='PHYSICAL_LOCATION']/form/table/tbody/tr[3]/td[1]/table/tbody/tr/td[4]/input")).SendKeys(houseno);
                        driver.FindElement(By.XPath("//*[@id='PHYSICAL_LOCATION']/form/table/tbody/tr[6]/td[1]/input")).SendKeys(streetname.ToUpper().Trim());
                        driver.FindElement(By.XPath("//*[@id='PHYSICAL_LOCATION']/form/table/tbody/tr[7]/td[1]/input")).SendKeys(stype.ToUpper().Trim());
                        gc.CreatePdf_WOP(orderNumber, "AddressSearchinputpassedBefore", driver, "WV", "Kanawha");

                        IWebElement IAddressClick = driver.FindElement(By.XPath("//*[@id='PHYSICAL_LOCATION']/form/table/tbody/tr[13]/td"));
                        IList<IWebElement> IAddressList = IAddressClick.FindElements(By.XPath("input"));
                        foreach (IWebElement par in IAddressList)
                        {
                            string strParcel = par.GetAttribute("value");
                            if (strParcel != "" && strParcel.Contains("Search"))
                            {
                                par.SendKeys(Keys.Enter);
                                break;
                            }
                        }
                        int Multicount = 0;
                        try
                        {
                            try
                            {
                                IWebElement Iowner = driver.FindElement(By.XPath("//*[@id='contents']/table/tbody/tr[1]/td/table/tbody"));
                                IList<IWebElement> IowerRow = Iowner.FindElements(By.TagName("tr"));
                                Multicount = IowerRow.Count;
                            }
                            catch { }

                            if (Convert.ToInt32(Multicount) <= 25)
                            {
                                try
                                {
                                    for (int i = 0; i <= Convert.ToInt32(Multicount) / 10; i++)
                                    {
                                        afterClick(houseno, streetname.ToUpper().Trim(), stype.ToUpper().Trim(), orderNumber);
                                        driver.FindElement(By.XPath("//*[@id='contents']/table/tbody/tr[2]/td/table/tbody/tr[1]/td/div/table/tbody/tr/td[2]/a")).Click();
                                    }
                                }
                                catch { }
                            }

                            if (Convert.ToInt32(Multicount) > 25)
                            {
                                HttpContext.Current.Session["multiparcel_Kanawah_Count"] = "Maximum";
                                driver.Quit();
                                return "Maximum";
                            }
                            if (SingleAddress.Count() > 1)
                            {
                                HttpContext.Current.Session["multiparcel_Kanawah"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                            if (SingleAddress.Count == 1)
                            {
                                gc.CreatePdf_WOP(orderNumber, "Multiparcel Result", driver, "WV", "Kanawha");
                                foreach (string strAddressURL in SingleAddress)
                                {
                                    driver.Navigate().GoToUrl(strAddressURL);
                                }
                            }
                        }
                        catch { }

                        try
                        {
                            IWebElement IAddressmatchclick = driver.FindElement(By.XPath("//*[@id='contents']/table/tbody/tr[1]/td/table"));
                            IList<IWebElement> TRmulti11 = IAddressmatchclick.FindElements(By.TagName("tr"));
                            IList<IWebElement> TDmulti11;
                            foreach (IWebElement row in TRmulti11)
                            {
                                TDmulti11 = row.FindElements(By.TagName("td"));
                                if (TDmulti11.Count != 0 && TRmulti11.Count() == 2 && !row.Text.Contains("Parcel ID(Account") && row.Text.Contains(houseno) && row.Text.Contains(streetname.ToUpper()))
                                {
                                    IWebElement Address = TDmulti11[0].FindElement(By.TagName("a"));
                                    Address.Click();
                                }
                            }
                        }
                        catch { }
                        try
                        {
                            //No Data Found
                            string nodata = driver.FindElement(By.XPath("//*[@id='contents']/table/tbody/tr[1]/td/table/tbody/tr/th/h4")).Text;
                            if (nodata.Contains("No records were found matching this criteria"))
                            {
                                HttpContext.Current.Session["Nodata_KanawhaWV"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }

                    }
                    string District = "", Map = "", subparcel = "", parcelsplit = "", parcel = "-", parcelNo = "", parcel1 = "-", parcel2 = "-", parcel3 = "-", ParcelId = "-", parcel4 = "", parcel5 = "", parcel6 = "";
                    if (searchType == "parcel")
                    {
                        driver.FindElement(By.LinkText("Parcel")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);

                        parcelNo = parcelNumber.Replace("-", "").Replace(" ", "").Trim();
                        if (parcelNo.Count() == 17 || parcelNo.Count() == 16 || parcelNo.Count() == 15)
                        {
                            District = parcelNo.Substring(0, 2);
                            parcelsplit = parcelNo.Substring(2, parcelNo.Length - 2).Trim();
                            if (parcelsplit.Count() == 14)
                            {
                                Map = parcelsplit.Substring(0, 2).Trim();
                                parcel = parcelsplit.Substring(3, 5).TrimStart('0').TrimEnd('0').Trim();
                                subparcel = parcelsplit.Substring(9, 5).TrimStart('0').TrimEnd('0').Trim();
                            }

                            if (parcelsplit.Count() == 13)
                            {
                                Map = parcelsplit.Substring(0, 1).TrimStart('0').TrimEnd('0').Trim();
                                parcel = parcelsplit.Substring(2, 3).TrimStart('0').TrimEnd('0').Trim();
                                subparcel = parcelsplit.Substring(5, 4).TrimStart('0').TrimEnd('0').Trim();
                                try
                                {
                                    if (subparcel == "" && parcelsplit.Substring(10, 4).Trim() != "0000")
                                    {
                                        driver.FindElement(By.XPath("//*[@id='PARCEL']/form/table/tbody/tr[11]/td[1]/input")).SendKeys(parcelsplit.Substring(11, 4).TrimStart('0').TrimEnd('0').Trim());
                                    }
                                }
                                catch { }
                            }
                            if (parcelsplit.Count() == 15)
                            {
                                Map = parcelsplit.Substring(0, 3).TrimStart('0').TrimEnd('0').Trim();
                                parcel = parcelsplit.Substring(4, 5).TrimStart('0').TrimEnd('0').Trim();
                                subparcel = parcelsplit.Substring(7, 4).TrimStart('0').TrimEnd('0').Trim();
                                try
                                {
                                    if (subparcel == "" && parcelsplit.Substring(11, 4).Trim() != "0000")
                                    {
                                        driver.FindElement(By.XPath("//*[@id='PARCEL']/form/table/tbody/tr[11]/td[1]/input")).SendKeys(parcelsplit.Substring(11, 4).Trim());
                                    }

                                }
                                catch { }
                            }
                        }

                        driver.FindElement(By.Id("UID1")).SendKeys(District);
                        driver.FindElement(By.XPath("//*[@id='PARCEL']/form/table/tbody/tr[8]/td[1]/input")).SendKeys(Map);
                        driver.FindElement(By.XPath("//*[@id='PARCEL']/form/table/tbody/tr[9]/td[1]/input")).SendKeys(parcel);
                        driver.FindElement(By.XPath("//*[@id='PARCEL']/form/table/tbody/tr[10]/td[1]/input")).SendKeys(subparcel);

                        gc.CreatePdf(orderNumber, parcelNumber, "Search Parcel", driver, "WV", "Kanawha");
                        IWebElement IParcelClick = driver.FindElement(By.XPath("//*[@id='PARCEL']/form/table/tbody/tr[15]/td"));
                        IList<IWebElement> IParcelList = IParcelClick.FindElements(By.XPath("input"));
                        foreach (IWebElement par in IParcelList)
                        {
                            string strParcel = par.GetAttribute("value");
                            if (strParcel != "" && strParcel.Contains("Search"))
                            {
                                par.SendKeys(Keys.Enter);
                                Thread.Sleep(2000);
                                gc.CreatePdf(orderNumber, parcelNumber, "Search Parcel Inputpassed After", driver, "WV", "Kanawha");
                                break;

                            }
                        }
                        try
                        {
                            //No Data Found
                            string nodata = driver.FindElement(By.XPath("//*[@id='contents']/table/tbody/tr[1]/td/table/tbody/tr/th/h4")).Text;
                            if (nodata.Contains("No records were found matching this criteria"))
                            {
                                HttpContext.Current.Session["Nodata_KanawhaWV"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }
                    if (searchType == "ownername")
                    {
                        driver.FindElement(By.XPath("//*[@id='OWNER']/form/table/tbody/tr[2]/td[1]/input")).SendKeys(ownername);
                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "IpuntPassedBefore", driver, "WV", "Kanawha");
                        driver.FindElement(By.Id("Search")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcelNumber, "InputPassed OwnerAfter", driver, "WV", "Kanawha");
                        int Multicount = 0;
                        try
                        {
                            try
                            {
                                IWebElement Iowner = driver.FindElement(By.XPath("//*[@id='contents']/table/tbody/tr[1]/td/table/tbody"));
                                IList<IWebElement> IowerRow = Iowner.FindElements(By.TagName("tr"));
                                Multicount = IowerRow.Count;
                            }
                            catch { }

                            if (Convert.ToInt32(Multicount) <= 25)
                            {
                                try
                                {
                                    for (int i = 0; i <= Convert.ToInt32(Multicount) / 10; i++)
                                    {
                                        afterClick(houseno, streetname,stype, orderNumber);
                                        driver.FindElement(By.XPath("//*[@id='contents']/table/tbody/tr[2]/td/table/tbody/tr[1]/td/div/table/tbody/tr/td[2]/a")).Click();
                                    }
                                }
                                catch { }
                            }

                            if (Convert.ToInt32(Multicount) > 25)
                            {
                                HttpContext.Current.Session["multiparcel_Kanawah_Count"] = "Maximum";
                                driver.Quit();
                                return "Maximum";
                            }
                            if (SingleAddress.Count() > 1)
                            {
                                HttpContext.Current.Session["multiparcel_Kanawah"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                            if (SingleAddress.Count == 1)
                            {
                                gc.CreatePdf_WOP(orderNumber, "Multiparcel Result", driver, "WV", "Kanawha");
                                driver.Navigate().GoToUrl(StrSingleAddress);
                            }

                        }
                        catch { }
                        try
                        {
                            //No Data Found
                            string nodata = driver.FindElement(By.XPath("//*[@id='contents']/table/tbody/tr[1]/td/table/tbody/tr/th/h4")).Text;
                            if (nodata.Contains("No records were found matching this criteria"))
                            {
                                HttpContext.Current.Session["Nodata_KanawhaWV"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }
                    //Property Details
                    string ParcelID = "", TaxYear = "", Neighborhood = "", ProDistrict = "", ProMap = "", Parcel = "", SubParcel = "", SpecialId = "", TaxClass = "", LandUse = "", PropertyClass = "", OwnersName = "", OwnersMailAddress = "", Location = "", LegalDescription1 = "", YearBuilt = "", City = "", TotalAcres = "";

                    try
                    {
                        driver.FindElement(By.XPath("//*[@id='contents']/table/tbody/tr[1]/td/table/tbody/tr/td[1]/a")).SendKeys(Keys.Enter);
                        gc.CreatePdf(orderNumber, parcelNumber, "InputPassedParcelnumber", driver, "WV", "Kanawha");

                    }
                    catch { }
                    try
                    {
                        string taxurl = driver.FindElement(By.XPath("//*[@id='tables']/a[4]")).GetAttribute("href");
                        driver.Navigate().GoToUrl(taxurl);
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcelNumber, "ParcelPagePdf", driver, "WV", "Kanawha");
                    }
                    catch { }
                    try
                    {
                        string taxurl = driver.FindElement(By.XPath("//*[@id='tables']/a[2]")).GetAttribute("href");
                        driver.Navigate().GoToUrl(taxurl);
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcelNumber, "AssessmentTaxPagePdf", driver, "WV", "Kanawha");
                    }
                    catch { }
                    try
                    {
                        string taxurl = driver.FindElement(By.XPath("//*[@id='tables']/a[3]")).GetAttribute("href");
                        driver.Navigate().GoToUrl(taxurl);
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcelNumber, "AssessmentPagePdf", driver, "WV", "Kanawha");
                    }
                    catch { }
                    try
                    {
                        string taxurl = driver.FindElement(By.XPath("//*[@id='tables']/a[6]")).GetAttribute("href");
                        driver.Navigate().GoToUrl(taxurl);
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcelNumber, "OwnershipPagePdf", driver, "WV", "Kanawha");
                    }
                    catch { }
                    try
                    {
                        string taxurl = driver.FindElement(By.XPath("//*[@id='tables']/a[7]")).GetAttribute("href");
                        driver.Navigate().GoToUrl(taxurl);
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcelNumber, "LegalDescription PagePdf", driver, "WV", "Kanawha");
                    }
                    catch { }
                    try
                    {
                        string taxurl = driver.FindElement(By.XPath("//*[@id='tables']/a[1]")).GetAttribute("href");
                        driver.Navigate().GoToUrl(taxurl);
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcelNumber, "SummaryPdf", driver, "WV", "Kanawha");
                    }
                    catch { }
                    try
                    {
                        IWebElement tbprop11 = driver.FindElement(By.XPath("//*[@id='contents']/table/tbody/tr/td/table/tbody/tr/td[1]/table"));
                        IList<IWebElement> TRprop11 = tbprop11.FindElements(By.TagName("tr"));
                        IList<IWebElement> TDprop11;
                        foreach (IWebElement row in TRprop11)
                        {
                            TDprop11 = row.FindElements(By.TagName("td"));
                            if (TDprop11.Count != 0)
                            {
                                if (row.Text.Contains("(Account)"))
                                {
                                    ParcelID = TDprop11[0].Text;
                                }
                                if (row.Text.Contains("Tax Year"))
                                {
                                    TaxYear = TDprop11[0].Text.Trim();
                                }
                                if (row.Text.Contains("Neighborhood"))
                                {
                                    Neighborhood = TDprop11[0].Text;
                                }
                                if (row.Text.Contains("District"))
                                {
                                    ProDistrict = TDprop11[0].Text;
                                }
                                if (row.Text.Contains("Map"))
                                {
                                    ProMap = TDprop11[0].Text;
                                }
                                if (row.Text.Contains("Parcel") && !row.Text.Contains("Sub") && !row.Text.Contains("ID"))
                                {
                                    Parcel = TDprop11[0].Text;
                                }
                                if (row.Text.Contains("Sub"))
                                {
                                    SubParcel = TDprop11[0].Text;
                                }
                                if (row.Text.Contains("Special Id"))
                                {
                                    SpecialId = TDprop11[0].Text;
                                }
                                if (row.Text.Contains("Tax Class"))
                                {
                                    TaxClass = TDprop11[0].Text;
                                }
                                if (row.Text.Contains("Land Use"))
                                {
                                    LandUse = TDprop11[0].Text;
                                }
                                if (row.Text.Contains("Property Class"))
                                {
                                    PropertyClass = TDprop11[0].Text;
                                }
                                if (row.Text.Contains("Owner's Name"))
                                {
                                    OwnersName = TDprop11[0].Text;
                                }
                                if (row.Text.Contains("Mail Address") && row.Text != "")
                                {
                                    if (TDprop11[0].Text.Trim() != "")
                                    {
                                        OwnersMailAddress = TDprop11[0].Text;
                                    }
                                }
                                if (row.Text.Contains("Location") && row.Text != "")
                                {
                                    if (TDprop11[0].Text.Trim() != "")
                                    {
                                        Location = TDprop11[0].Text;
                                    }
                                }
                                if (row.Text.Contains("Legal Description 1"))
                                {
                                    LegalDescription1 = TDprop11[0].Text;
                                }
                            }
                        }
                    }
                    catch { }
                    try
                    {
                        IWebElement tbprop12 = driver.FindElement(By.XPath("//*[@id='contents']/table/tbody/tr/td/table/tbody/tr/td[2]/table/tbody"));
                        IList<IWebElement> TRprop12 = tbprop12.FindElements(By.TagName("tr"));
                        IList<IWebElement> TDprop12;
                        foreach (IWebElement row in TRprop12)
                        {
                            TDprop12 = row.FindElements(By.TagName("td"));
                            if (TDprop12.Count != 0)
                            {
                                if (row.Text.Contains("Year Built"))
                                {
                                    YearBuilt = TDprop12[0].Text;
                                }
                                if (row.Text.Contains("City"))
                                {
                                    City = TDprop12[0].Text.Trim();
                                }
                                if (row.Text.Contains("Total Acres"))
                                {
                                    TotalAcres = TDprop12[0].Text;
                                }
                            }
                        }
                        string PropertyDetails = ParcelID.Trim() + "~" + TaxYear.Trim() + "~" + Neighborhood.Trim() + "~" + ProDistrict.Trim() + "~" + ProMap.Trim() + "~" + Parcel.Trim() + "~" + SubParcel.Trim() + "~" + SpecialId.Trim() + "~" + TaxClass.Trim() + "~" + LandUse.Trim() + "~" + PropertyClass.Trim() + "~" + OwnersName.Trim() + "~" + OwnersMailAddress.Trim() + "~" + Location.Trim() + "~" + LegalDescription1.Trim() + "~" + YearBuilt.Trim() + "~" + City.Trim() + "~" + TotalAcres.Trim();
                        gc.insert_date(orderNumber, ParcelID, 774, PropertyDetails, 1, DateTime.Now);
                    }
                    catch { }
                    //Assessments Details
                    try
                    {
                        string AssessedLand = "", AssessedBuilding = "", MineralValue = "", HomesteadValue = "", TotalAssessedValue = "";
                        IWebElement tbassesment11 = driver.FindElement(By.XPath("//*[@id='contents']/table/tbody/tr/td/table/tbody/tr/td[1]/table"));
                        IList<IWebElement> TRassessment11 = tbassesment11.FindElements(By.TagName("tr"));
                        IList<IWebElement> TDassessment11;
                        foreach (IWebElement row in TRassessment11)
                        {
                            TDassessment11 = row.FindElements(By.TagName("td"));
                            if (TDassessment11.Count != 0)
                            {
                                if (row.Text.Contains("Assessed Land"))
                                {
                                    AssessedLand = TDassessment11[0].Text;
                                }
                                if (row.Text.Contains("Assessed Building"))
                                {
                                    AssessedBuilding = TDassessment11[0].Text.Trim();
                                }
                                if (row.Text.Contains("Mineral Value"))
                                {
                                    MineralValue = TDassessment11[0].Text;
                                }
                                if (row.Text.Contains("Homestead Value"))
                                {
                                    HomesteadValue = TDassessment11[0].Text;
                                }
                                if (row.Text.Contains("Total Assessed Value"))
                                {
                                    TotalAssessedValue = TDassessment11[0].Text;
                                }
                            }
                        }
                        string AssessmentsDetails = AssessedLand.Trim() + "~" + AssessedBuilding.Trim() + "~" + MineralValue.Trim() + "~" + HomesteadValue.Trim() + "~" + TotalAssessedValue.Trim();
                        gc.insert_date(orderNumber, ParcelID, 779, AssessmentsDetails, 1, DateTime.Now);
                    }
                    catch { }

                    //Appraised Value Details
                    try
                    {
                        string taxurl = driver.FindElement(By.XPath("//*[@id='tables']/a[5]")).GetAttribute("href");
                        driver.Navigate().GoToUrl(taxurl);
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcelNumber, "AppraisedValuePdf", driver, "WV", "Kanawha");

                    }
                    catch { }
                    try
                    {
                        string AppraisedLand = "", AppraisedBuilding = "", LandValue = "", BuildingValue = "", CommonLand = "", CommonBuilding = "", CostValue = "", ExemptLand = "", ExemptBuilding = "", TotalAppraisedValue = "";
                        IWebElement tbappraised11 = driver.FindElement(By.XPath("//*[@id='contents']/table/tbody/tr/td/table/tbody"));
                        IList<IWebElement> TRappraised11 = tbappraised11.FindElements(By.TagName("tr"));
                        IList<IWebElement> TDappraised11;
                        foreach (IWebElement row in TRappraised11)
                        {
                            TDappraised11 = row.FindElements(By.TagName("td"));
                            if (TDappraised11.Count != 0)
                            {
                                if (row.Text.Contains("Appraised Land"))
                                {
                                    AppraisedLand = TDappraised11[6].Text;
                                }
                                if (row.Text.Contains("Appraised Building"))
                                {
                                    AppraisedBuilding = TDappraised11[7].Text.Trim();
                                }
                                if (row.Text.Contains("Land Value"))
                                {
                                    LandValue = TDappraised11[8].Text;
                                }
                                if (row.Text.Contains("Building Value"))
                                {
                                    BuildingValue = TDappraised11[9].Text;
                                }
                                if (row.Text.Contains("Common Land"))
                                {
                                    CommonLand = TDappraised11[10].Text;
                                }
                                if (row.Text.Contains("Common Building"))
                                {
                                    CommonBuilding = TDappraised11[11].Text;
                                }
                                if (row.Text.Contains("Cost Value"))
                                {
                                    CostValue = TDappraised11[12].Text;
                                }
                                if (row.Text.Contains("Exempt Land"))
                                {
                                    ExemptLand = TDappraised11[29].Text;
                                }
                                if (row.Text.Contains("Exempt Building"))
                                {
                                    ExemptBuilding = TDappraised11[30].Text;
                                }
                                if (row.Text.Contains("Total Appraised Value"))
                                {
                                    TotalAppraisedValue = TDappraised11[31].Text;
                                }
                                break;
                            }
                        }
                        string AppraisedValueDetails = AppraisedLand.Trim() + "~" + AppraisedBuilding.Trim() + "~" + LandValue.Trim() + "~" + BuildingValue.Trim() + "~" + CommonLand.Trim() + "~" + CommonBuilding.Trim() + "~" + CostValue.Trim() + "~" + ExemptLand.Trim() + "~" + ExemptBuilding.Trim() + "~" + TotalAppraisedValue.Trim();
                        gc.insert_date(orderNumber, ParcelID, 781, AppraisedValueDetails, 1, DateTime.Now);
                    }
                    catch { }


                    try
                    {
                        driver.Navigate().GoToUrl("http://kanawha.softwaresystems.com/index.html");
                        Thread.Sleep(2000);
                        Tax_Authority = driver.FindElement(By.XPath("/html/body/center/table[1]/tbody/tr[4]/td/table/tbody/tr/td")).Text;

                    }
                    catch { }

                    //Tax Bill Details
                    try
                    {
                        string District1 = "", Map1 = "", Parcel1 = "", SubParcel1 = "", Delinquent = "";
                        driver.Navigate().GoToUrl("http://129.71.205.110/index.html");
                        Thread.Sleep(2000);

                        if (ParcelID.Contains("-") || ParcelID.Contains(" ") || ParcelID != "")
                        {
                            parcelNo = ParcelID.Replace("-", "").Replace(" ", "").Replace(".", "");
                        }

                        string CommonParcel = parcelNo;

                        if (CommonParcel.Length == 11)
                        {
                            District1 = parcelNo.Substring(0, 2);
                            Map1 = parcelNo.Substring(2, 1);
                            Parcel1 = parcelNo.Substring(3, 4);
                            SubParcel1 = parcelNo.Substring(7, 4);
                        }
                        if (CommonParcel.Length == 12)
                        {
                            District1 = parcelNo.Substring(0, 2);
                            Map1 = parcelNo.Substring(2, 2);
                            Parcel1 = parcelNo.Substring(4, 4);
                            SubParcel1 = parcelNo.Substring(8, 4);
                        }
                        if (CommonParcel.Length == 13)
                        {
                            District1 = parcelNo.Substring(0, 2);
                            Map1 = parcelNo.Substring(2, 3);
                            Parcel1 = parcelNo.Substring(5, 4);
                            SubParcel1 = parcelNo.Substring(9, 4);
                        }
                        if (CommonParcel.Length == 14)
                        {
                            District1 = parcelNo.Substring(0, 2);
                            Map1 = parcelNo.Substring(2, 4);
                            Parcel1 = parcelNo.Substring(6, 4);
                            SubParcel1 = parcelNo.Substring(10, 4);
                        }
                        if (CommonParcel.Length == 15)
                        {
                            District1 = parcelNo.Substring(0, 2);
                            Map1 = parcelNo.Substring(2, 1);
                            Parcel1 = parcelNo.Substring(3, 4);
                            SubParcel1 = parcelNo.Substring(7, 4);
                        }
                        if (CommonParcel.Length == 16)
                        {
                            District1 = parcelNo.Substring(0, 2);
                            Map1 = parcelNo.Substring(2, 2);
                            Parcel1 = parcelNo.Substring(4, 4);
                            SubParcel1 = parcelNo.Substring(8, 4);
                        }
                        if (CommonParcel.Length == 17)
                        {
                            District1 = parcelNo.Substring(0, 2);
                            Map1 = parcelNo.Substring(2, 3);
                            Parcel1 = parcelNo.Substring(5, 4);
                            SubParcel1 = parcelNo.Substring(9, 4);
                        }

                        var SelectDistrict = driver.FindElement(By.Name("DIST"));
                        var SelectDistrict1 = new SelectElement(SelectDistrict);
                        SelectDistrict1.SelectByValue(District1);

                        var SelectMap = driver.FindElement(By.Name("MAP"));
                        var SelectMap1 = new SelectElement(SelectMap);
                        SelectMap1.SelectByText(Map1);

                        var SelectParcel = driver.FindElement(By.Name("PARC"));
                        var SelectParcel1 = new SelectElement(SelectParcel);
                        SelectParcel1.SelectByText(Parcel1);

                        var SelectSubParcel = driver.FindElement(By.Name("SPAR"));
                        var SelectSubParcel1 = new SelectElement(SelectSubParcel);
                        SelectSubParcel1.SelectByText(SubParcel1);
                        gc.CreatePdf(orderNumber, parcelNumber, "Tax Information Details Inputpassed", driver, "WV", "Kanawha");

                        driver.FindElement(By.XPath("/html/body/center/table[2]/tbody/tr[4]/td[2]/form/input[4]")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Tax Information Details Inputpassed After", driver, "WV", "Kanawha");

                        List<string> ParcelSearch = new List<string>();

                        IWebElement ParcelTB = driver.FindElement(By.XPath("//*[@id='inside']/div/div[1]/table/tbody"));
                        IList<IWebElement> ParcelTR = ParcelTB.FindElements(By.TagName("tr"));
                        ParcelTR.Reverse();
                        int rows_count = ParcelTR.Count;

                        for (int row = 0; row < rows_count; row++)
                        {
                            if (row == rows_count - 1 || row == rows_count - 2 || row == rows_count - 3)
                            {
                                IList<IWebElement> Columns_row = ParcelTR[row].FindElements(By.TagName("td"));

                                int columns_count = Columns_row.Count;

                                for (int column = 0; column < columns_count; column++)
                                {
                                    if (column == columns_count - 2)
                                    {
                                        IWebElement ParcelBill_link = Columns_row[0].FindElement(By.TagName("a"));
                                        string Parcelurl = ParcelBill_link.GetAttribute("href");
                                        ParcelSearch.Add(Parcelurl);

                                    }
                                }
                            }
                        }

                        foreach (string Parcelbill in ParcelSearch)
                        {

                            driver.Navigate().GoToUrl(Parcelbill);
                            Thread.Sleep(3000);

                            try
                            {

                                Tax_Year = driver.FindElement(By.XPath("//*[@id='body']/div[1]/div")).Text;
                                Ticket_Numbet = driver.FindElement(By.XPath("//*[@id='body']/table[1]/tbody/tr[1]/td[2]")).Text;
                                try
                                {
                                    Tax_Class = driver.FindElement(By.XPath("//*[@id='body']/table[2]/tbody/tr[3]/td/table/tbody/tr/td[2]")).Text;
                                    Spl_Desp = driver.FindElement(By.XPath("//*[@id='body']/table[2]/tbody/tr[3]/td/table/tbody/tr/td[12]")).Text;


                                    if (Spl_Desp == "" || Spl_Desp == "Delinquent")
                                    {
                                        string priordelinq = "", HomesteadExemptions = "";
                                        string check = driver.FindElement(By.XPath("//*[@id='body']/table[2]/tbody/tr[3]/td/table/tbody/tr/td[10]/img")).GetAttribute("src").ToString();
                                        if (check == "http://129.71.205.110/images/check_mark.gif")
                                        {
                                            Propr_del = "YES";
                                        }
                                        else if (check == "http://129.71.205.110/images/spacer.gif")
                                        {
                                            Propr_del = "NO";
                                        }

                                        string check1 = driver.FindElement(By.XPath("//*[@id='body']/table[2]/tbody/tr[3]/td/table/tbody/tr/td[4]/img")).GetAttribute("src").ToString();
                                        if (check1 == "http://129.71.205.110/images/check_mark.gif")
                                        {
                                            Home_Exp = "YES";
                                        }
                                        else if (check == "http://129.71.205.110/images/spacer.gif")
                                        {
                                            Home_Exp = "NO";
                                        }

                                        if (Propr_del == "YES" || Home_Exp == "YES")
                                        {
                                            HttpContext.Current.Session["Kanawahmsg"] = "YES";
                                        }
                                    }
                                }
                                catch
                                { }

                                Back_Tax = driver.FindElement(By.XPath("//*[@id='body']/table[2]/tbody/tr[3]/td/table/tbody/tr/td[6]")).Text;
                                Exoneration = driver.FindElement(By.XPath("//*[@id='body']/table[2]/tbody/tr[3]/td/table/tbody/tr/td[8]")).Text;
                                First_Half = driver.FindElement(By.XPath("//*[@id='body']/div[2]/table/tbody/tr/td[3]")).Text.Replace("\r\n", "");
                                Second_Half = driver.FindElement(By.XPath("//*[@id='body']/div[2]/table/tbody/tr/td[5]")).Text.Replace("\r\n", "");
                                Total_Due = driver.FindElement(By.XPath("//*[@id='body']/div[2]/table/tbody/tr/td[7]")).Text.Replace("\r\n", "");

                                Tax_Details = Tax_Year + "~" + Ticket_Numbet + "~" + Tax_Class + "~" + Home_Exp + "~" + Back_Tax + "~" + Exoneration + "~" + Propr_del + "~" + Spl_Desp + "~" + First_Half + "~" + Second_Half + "~" + Total_Due + "~" + Tax_Authority;
                                gc.insert_date(orderNumber, ParcelID, 788, Tax_Details, 1, DateTime.Now);
                            }
                            catch
                            { }
                            //Payment Details
                            try
                            {
                                IWebElement PaymentTB = driver.FindElement(By.XPath("//*[@id='body']/table[3]/tbody/tr/td[3]/table/tbody"));
                                IList<IWebElement> PaymentTR = PaymentTB.FindElements(By.TagName("tr"));
                                IList<IWebElement> PaymentTD;

                                foreach (IWebElement Payment in PaymentTR)
                                {
                                    PaymentTD = Payment.FindElements(By.TagName("td"));
                                    if (PaymentTD.Count != 0 && !Payment.Text.Contains("First Half"))
                                    {
                                        Installment = PaymentTD[0].Text;
                                        FirstHlf = PaymentTD[1].Text;
                                        Scndhlf = PaymentTD[2].Text;

                                        Payment_details = Installment + "~" + FirstHlf + "~" + Scndhlf + "~" + Tax_Year;
                                        gc.insert_date(orderNumber, ParcelID, 789, Payment_details, 1, DateTime.Now);
                                    }
                                }
                            }
                            catch
                            { }
                            //Tax & Assessment Details
                            try
                            {
                                string Assessmentdetails1 = "";
                                IWebElement PropertyTB = driver.FindElement(By.XPath("//*[@id='body']/table[3]/tbody/tr/td[1]/table/tbody"));
                                IList<IWebElement> PropertyTR = PropertyTB.FindElements(By.TagName("tr"));
                                IList<IWebElement> PropertyTD;

                                foreach (IWebElement Property in PropertyTR)
                                {
                                    PropertyTD = Property.FindElements(By.TagName("td"));
                                    if (PropertyTD.Count != 0 && !Property.Text.Contains("Assessment"))
                                    {
                                        Assessment = PropertyTD[0].Text;
                                        Gross = PropertyTD[1].Text;
                                        Net = PropertyTD[2].Text;
                                        TaxoffYear = PropertyTD[3].Text;

                                        Assessmentdetails1 = Assessment + "~" + Gross + "~" + Net + "~" + TaxoffYear + "~" + Tax_Year;
                                        gc.insert_date(orderNumber, ParcelID, 790, Assessmentdetails1, 1, DateTime.Now);
                                    }
                                }
                            }
                            catch
                            { }
                            try
                            {
                                string taxurl = driver.FindElement(By.XPath("//*[@id='body']/a")).GetAttribute("href");
                                driver.Navigate().GoToUrl(taxurl);
                                Thread.Sleep(2000);
                                gc.CreatePdf(orderNumber, parcelNumber, Tax_Year + "County Real Property Tax information ", driver, "WV", "Kanawha");

                            }
                            catch { }
                            AssessmentTime = DateTime.Now.ToString("HH:mm:ss");
                        }
                    }
                    catch { }

                    TaxTime = DateTime.Now.ToString("HH:mm:ss");
                    driver.Quit();
                    gc.mergpdf(orderNumber, "WV", "Kanawha");
                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "WV", "Kanawha", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);
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
        public void afterClick(string houseno, string streetname,string stype, string orderNumber)
        {
            IWebElement IAddress;
            string Owner = "", strMulticount = "", Address = "", MultiParcelNumber = "", strSingleAddress = "";
            try
            {
                IWebElement IAddressmatchclick = driver.FindElement(By.XPath("//*[@id='contents']/table/tbody/tr[1]/td/table"));
                IList<IWebElement> TRmulti11 = IAddressmatchclick.FindElements(By.TagName("tr"));
                IList<IWebElement> TDmulti11;
                foreach (IWebElement row in TRmulti11)
                {
                    TDmulti11 = row.FindElements(By.TagName("td"));
                    if (TDmulti11.Count != 0 && !row.Text.Contains("Parcel ID(Account") && row.Text.Contains(houseno) && row.Text.Contains(streetname.ToUpper()) && row.Text.Contains(stype.ToUpper()))
                    {
                        IAddress = TDmulti11[0].FindElement(By.TagName("a"));
                        strSingleAddress = IAddress.GetAttribute("href");
                        SingleAddress.Add(strSingleAddress);
                    }
                    if (TDmulti11.Count != 0 && !row.Text.Contains("Parcel ID(Account") && row.Text.Contains(houseno) && row.Text.Contains(streetname.ToUpper()) && row.Text.Contains(stype.ToUpper()))
                    {
                        MultiParcelNumber = TDmulti11[0].Text;
                        houseno = TDmulti11[3].Text;
                        streetname = TDmulti11[6].Text;
                        string MultyInst = houseno + "~" + streetname.ToUpper().Trim();
                        gc.insert_date(orderNumber, MultiParcelNumber, 816, MultyInst, 1, DateTime.Now);
                    }
                }
                gc.CreatePdf(orderNumber, MultiParcelNumber, "Multiparcel Result", driver, "WV", "Kanawha");

            }
            catch { }
        
        }
    }
}



