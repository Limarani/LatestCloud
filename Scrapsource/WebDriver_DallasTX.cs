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
using System.Collections.ObjectModel;

namespace ScrapMaricopa
{

    public class WebDriver_DallasTX
    {
        IWebDriver driver;
        DBconnection db = new DBconnection();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        GlobalClass gc = new GlobalClass();

        public string FTP_DallasTX(string sno, string sname, string direction, string sttype, string unino, string parcelNumber, string ownername, string searchType, string orderNumber, string directparcel)
        {
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "", AssessTakenTime = "", TaxTakentime = "", CityTaxtakentime = "";
            string straddress = "";
            string multiParcelnumber = "";
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            // driver = new PhantomJSDriver();
            //driver = new ChromeDriver();           
            using (driver = new PhantomJSDriver())
            {
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");
                    if (searchType == "titleflex")
                    {

                        if (direction != "")
                        {
                            straddress = sno + " " + direction + " " + sname + " " + sttype + " " + unino;
                        }
                        else
                        {
                            straddress = sno + " " + sname + " " + sttype + " " + unino;
                        }
                        gc.TitleFlexSearch(orderNumber, "", "", straddress, "TX", "Dallas");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_DallasTX"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }


                    driver.Navigate().GoToUrl("http://www.dallascad.org/SearchOwner.aspx");
                    Thread.Sleep(1000);


                    if (searchType == "address")
                    {

                        IWebElement IAddressSearch1 = driver.FindElement(By.XPath("//*[@id='Form1']/table[2]/tbody/tr[1]/td[2]/p[2]/span/a[3]"));
                        IJavaScriptExecutor js1 = driver as IJavaScriptExecutor;
                        js1.ExecuteScript("arguments[0].click();", IAddressSearch1);
                        Thread.Sleep(3000);

                        try
                        {
                            driver.FindElement(By.Id("AcctTypeCheckList1_chkAcctType_1")).Click();
                            Thread.Sleep(2000);
                        }
                        catch { }
                        try
                        {
                            driver.FindElement(By.Id("AcctTypeCheckList1_chkAcctType_2")).Click();
                            Thread.Sleep(2000);
                        }
                        catch { }

                        driver.FindElement(By.Id("txtAddrNum")).SendKeys(sno);
                        driver.FindElement(By.Id("listStDir")).SendKeys(direction);
                        driver.FindElement(By.Id("txtStName")).SendKeys(sname);
                        gc.CreatePdf_WOP(orderNumber, "Address search Input ", driver, "TX", "Dallas");
                        driver.FindElement(By.Id("cmdSubmit")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "Address search Output ", driver, "TX", "Dallas");
                        try
                        {
                            //int Count = 0;   
                            List<string> Multiinfo = new List<string>();
                            IWebElement Multiaddresstable = driver.FindElement(By.Id("SearchResults1_dgResults"));
                            IList<IWebElement> multiaddressrow = Multiaddresstable.FindElements(By.TagName("tr"));
                            IList<IWebElement> Multiaddressid;
                            foreach (IWebElement Multiaddress in multiaddressrow)
                            {
                                Multiaddressid = Multiaddress.FindElements(By.TagName("td"));
                                if (multiaddressrow.Count > 4 && Multiaddressid.Count != 0 && Multiaddressid.Count == 6 && !Multiaddress.Text.Contains("Property Address") && Multiaddress.Text.Contains(straddress))
                                {
                                    string Propertyadd = Multiaddressid[1].Text;
                                    string Cit = Multiaddressid[2].Text;
                                    string OWnername = Multiaddressid[3].Text;
                                    string Totalvl = Multiaddressid[4].Text;
                                    string Type = Multiaddressid[5].Text;
                                    IWebElement value1 = Multiaddressid[1].FindElement(By.Id("Hyperlink1"));
                                    multiParcelnumber = value1.GetAttribute("href");
                                    multiParcelnumber = GlobalClass.After(multiParcelnumber, "http://www.dallascad.org/AcctDetailRes.aspx?ID=");

                                    string multiaddressresult = OWnername.Trim() + "~" + Propertyadd.Trim() + "~" + Cit.Trim() + "~" + Type.Trim() + "~" + Totalvl.Trim();
                                    gc.insert_date(orderNumber, multiParcelnumber, 2155, multiaddressresult, 1, DateTime.Now);
                                    //Count++;
                                }
                            }
                            if (multiaddressrow.Count == 4)
                            {
                                driver.FindElement(By.XPath("//*[@id='Hyperlink1']")).Click();
                                Thread.Sleep(2000);
                            }
                            if (multiaddressrow.Count > 4)
                            {
                                HttpContext.Current.Session["multiparcel_DallasTX"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                            if (multiaddressrow.Count > 35)
                            {
                                HttpContext.Current.Session["multiParcel_DallasTX_Maximum"] = "Maximum";
                                driver.Quit();
                                return "Maximum";
                            }
                        }
                        catch { }
                        try
                        {
                            //No Data Found
                            string nodata = driver.FindElement(By.Id("SearchResults1_lblMessage")).Text;
                            if (nodata.Contains("No Records Found."))
                            {
                                HttpContext.Current.Session["Nodata_DallasTX"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }
                    if (searchType == "parcel")
                    {
                        IWebElement IAddressSearch1 = driver.FindElement(By.XPath("//*[@id='Form1']/table[2]/tbody/tr[1]/td[2]/p[2]/span/a[2]"));
                        IJavaScriptExecutor js1 = driver as IJavaScriptExecutor;
                        js1.ExecuteScript("arguments[0].click();", IAddressSearch1);
                        Thread.Sleep(3000);

                        //parcelNumber = parcelNumber.Replace(" ", "").Replace(".", "").Replace("-", "").Trim();
                        driver.FindElement(By.Id("txtAcctNum")).SendKeys(parcelNumber);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel search Input ", driver, "TX", "Dallas");
                        driver.FindElement(By.Id("Button1")).Click();
                        Thread.Sleep(3000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel search Output ", driver, "TX", "Dallas");

                        try
                        {
                            //No Data Found
                            string nodata = driver.FindElement(By.Id("SearchResults1_lblMessage")).Text;
                            if (nodata.Contains("No Records Found."))
                            {
                                HttpContext.Current.Session["Nodata_DallasTX"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }
                    if (searchType == "ownername")
                    {
                        driver.FindElement(By.Id("txtOwnerName")).SendKeys(ownername);
                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "Owner search Input ", driver, "TX", "Dallas");


                        try
                        {
                            driver.FindElement(By.Id("AcctTypeCheckList1_chkAcctType_1")).Click();
                            Thread.Sleep(2000);
                        }
                        catch { }
                        try
                        {
                            driver.FindElement(By.Id("AcctTypeCheckList1_chkAcctType_2")).Click();
                            Thread.Sleep(2000);
                        }
                        catch { }

                        IWebElement IAddressSearch1 = driver.FindElement(By.Id("cmdSubmit"));
                        IJavaScriptExecutor js1 = driver as IJavaScriptExecutor;
                        js1.ExecuteScript("arguments[0].click();", IAddressSearch1);
                        Thread.Sleep(3000);



                        gc.CreatePdf_WOP(orderNumber, "Owner search Result ", driver, "TX", "Dallas");
                        ////Multiparcel

                        try
                        {
                            //int Count = 0;   
                            List<string> Multiinfo = new List<string>();
                            IWebElement Multiaddresstable = driver.FindElement(By.Id("SearchResults1_dgResults"));
                            IList<IWebElement> multiaddressrow = Multiaddresstable.FindElements(By.TagName("tr"));
                            IList<IWebElement> Multiaddressid;
                            foreach (IWebElement Multiaddress in multiaddressrow)
                            {
                                Multiaddressid = Multiaddress.FindElements(By.TagName("td"));
                                if (multiaddressrow.Count > 4 && Multiaddressid.Count != 0 && Multiaddressid.Count == 6 && !Multiaddress.Text.Contains("Property Address") && Multiaddress.Text.Contains(straddress))
                                {
                                    string Propertyadd = Multiaddressid[1].Text;
                                    string Cit = Multiaddressid[2].Text;
                                    string OWnername = Multiaddressid[3].Text;
                                    string Totalvl = Multiaddressid[4].Text;
                                    string Type = Multiaddressid[5].Text;
                                    IWebElement value1 = Multiaddressid[1].FindElement(By.Id("Hyperlink1"));
                                    multiParcelnumber = value1.GetAttribute("href");
                                    multiParcelnumber = GlobalClass.After(multiParcelnumber, "http://www.dallascad.org/AcctDetailRes.aspx?ID=");

                                    string multiaddressresult = OWnername.Trim() + "~" + Propertyadd.Trim() + "~" + Cit.Trim() + "~" + Type.Trim() + "~" + Totalvl.Trim();
                                    gc.insert_date(orderNumber, multiParcelnumber, 2155, multiaddressresult, 1, DateTime.Now);
                                    //Count++;
                                }

                            }
                            if (multiaddressrow.Count == 4)
                            {
                                driver.FindElement(By.XPath("//*[@id='Hyperlink1']")).Click();
                                Thread.Sleep(2000);
                            }
                            if (multiaddressrow.Count > 4)
                            {
                                HttpContext.Current.Session["multiparcel_DallasTX"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                            if (multiaddressrow.Count > 35)
                            {
                                HttpContext.Current.Session["multiParcel_DallasTX_Maximum"] = "Maximum";
                                driver.Quit();
                                return "Maximum";
                            }
                        }
                        catch { }
                        try
                        {
                            //No Data Found
                            string nodata = driver.FindElement(By.Id("SearchResults1_lblMessage")).Text;
                            if (nodata.Contains("No Records Found."))
                            {
                                HttpContext.Current.Session["Nodata_DallasTX"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }
                    //Property Details
                    try
                    {
                        IWebElement IAddressSearch111 = driver.FindElement(By.XPath("//*[@id='Hyperlink1']"));
                        IJavaScriptExecutor js111 = driver as IJavaScriptExecutor;
                        js111.ExecuteScript("arguments[0].click();", IAddressSearch111);
                        Thread.Sleep(3000);

                    }
                    catch { }

                    string Parcelnumber = "", Ownername = "", Address = "", Addressmail = "", Neighborhood = "", Legaldesc = "", Exemption = "", Yearbuilt = "";
                    ////*[@id="MultiOwner1_dgmultiOwner"]/tbody/tr[2]/td[1]
                    Parcelnumber = driver.FindElement(By.XPath("//*[@id='Form1']/table[2]/tbody/tr[1]/td[2]/p[1]")).Text.Replace("Residential Account #", "").Trim();
                    gc.CreatePdf(orderNumber, Parcelnumber, "Property Details First click", driver, "TX", "Dallas");
                    Ownername = driver.FindElement(By.XPath("//*[@id='MultiOwner1_dgmultiOwner']/tbody/tr[2]/td[1]")).Text;
                    Address = driver.FindElement(By.XPath("//*[@id='PropAddr1_lblPropAddr']")).Text;
                    Neighborhood = driver.FindElement(By.XPath("//*[@id='lblNbhd']")).Text;
                    Legaldesc = driver.FindElement(By.Id("Table8")).Text.Replace("\r\n", "");
                    try
                    {
                        Exemption = driver.FindElement(By.XPath("//*[@id='Exempt1_lblMessage']")).Text.Replace("\r\n", "");
                    }
                    catch { }
                    Yearbuilt = driver.FindElement(By.XPath("//*[@id='table5']/tbody/tr[2]/td[1]")).Text;

                    string Propertydetails = Ownername + "~" + Address + "~" + Neighborhood + "~" + Legaldesc + "~" + Exemption + "~" + Yearbuilt;
                    gc.insert_date(orderNumber, Parcelnumber, 2146, Propertydetails, 1, DateTime.Now);
                    //Assessment Details
                    string Improvements = "", Land = "", Marketvalue1 = "", Revaluationyear = "", Previousval = "";
                    Improvements = driver.FindElement(By.XPath("//*[@id='ValueSummary1_lblImpVal']")).Text;
                    Land = driver.FindElement(By.XPath("//*[@id='ValueSummary1_pnlValue_lblLandVal']")).Text;
                    Marketvalue1 = driver.FindElement(By.XPath("//*[@id='ValueSummary1_pnlValue_lblTotalVal']")).Text;
                    try
                    {
                        Revaluationyear = driver.FindElement(By.XPath("//*[@id='tblValueSum']/tbody/tr[3]/td[2]")).Text;
                    }
                    catch { }
                    Previousval = driver.FindElement(By.XPath("//*[@id='tblValueSum']/tbody/tr[4]/td[2]")).Text;

                    string Assessmentdetails = Improvements + "~" + Land + "~" + Marketvalue1 + "~" + Revaluationyear + "~" + Previousval;
                    gc.insert_date(orderNumber, Parcelnumber, 2147, Assessmentdetails, 1, DateTime.Now);
                    gc.CreatePdf(orderNumber, Parcelnumber, "Property Details", driver, "TX", "Dallas");

                    //Tax Information Details
                    List<string> entitylink1 = new List<string>();

                    entitylink1.AddRange(new string[] { "DALLAS COUNTY", "DALLAS CO COMMUNITY COLLEGE", "PARKLAND HOSPITAL", "DALLAS", "ADDISON", "BALCH SPRINGS", "CARROLLTON", "CEDAR HILL", "COCKRELL HILL", "COPPELL", "DESOTO", "DUNCANVILLE", "FARMERS BRANCH", "GLENN HEIGHTS", "GRAND PRAIRIE", "HIGHLAND PARK", "HUTCHINS", "IRVING", "LANCASTER", "RICHARDSON", "ROWLETT", "SACHSE", "SEAGOVILLE", "SUNNYVALE", "UNIVERSITY PARK", "WILMER", "CEDAR HILL ISD", "COPPELL ISD", "DALLAS ISD", "DESOTO ISD", "DUNCANVILLE ISD", "GRAND PRAIRIE ISD", "HIGHLAND PARK ISD", "LANCASTER ISD", "SUNNYVALE ISD", "IRVING FCD, SECTION I", "IRVING FCD, SECTION III",
                       "COMBINE" ,"FERRIS","OVILLA","FERRIS ISD","IRVING ISD",
                    "GARLAND","GRAPEVINE","CARROLLTON-FARMERS BRANCH ISD","GARLAND ISD","GRAPEVINE-COLLEYVILLE ISD","RICHARDSON ISD","LEWISVILLE","MESQUITE","MESQUITE ISD","WYLIE","DALLAS COUNTY URD",
                    "DENTON CO LEVEE IMPR DIST1","DENTON CO LID1 AND RUD1","LANCASTER MUD1","GRAND PRAIRIE METROPOLITAN URD","NORTHWEST DALLAS COUNTY FCD","VALWOOD IMPROVEMENT AUTHORITY"});

                    string Jurisdiction = "", entityname = "", a1 = "", a2 = "", a3 = "", a4 = "", a5 = "", a6 = "";

                    string title = "", Citytax = "", City = "", school = "", Countyschool = "", College = "", Hospital = "", SpecialDistrict = "";
                    List<string> scenario = new List<string>();
                    IWebElement Taxinfo1 = driver.FindElement(By.XPath("//*[@id='TaxEst1_pnlTaxEst']/table/tbody"));
                    IList<IWebElement> TRBillsinfo2 = Taxinfo1.FindElements(By.TagName("tr"));
                    IList<IWebElement> Aherftax;
                    IList<IWebElement> Aherftax1;
                    int i = 0;

                    foreach (IWebElement row in TRBillsinfo2)
                    {
                        Aherftax = row.FindElements(By.TagName("td"));
                        Aherftax1 = row.FindElements(By.TagName("th"));
                        if (Aherftax1.Count != 0 && Aherftax1.Count == 1 && !row.Text.Contains("City"))
                        {
                            title = Aherftax1[0].Text;
                        }
                        if (Aherftax1.Count != 0 && Aherftax1.Count == 2 && !row.Text.Contains("City"))
                        {
                            City = Aherftax1[0].Text;
                            SpecialDistrict = Aherftax1[1].Text;

                            string taxhisdetails = title + "~" + City.Trim() + "~" + school.Trim() + "~" + Countyschool.Trim() + "~" + College.Trim() + "~" + Hospital.Trim() + "~" + SpecialDistrict.Trim();
                            gc.insert_date(orderNumber, Parcelnumber, 2148, taxhisdetails, 1, DateTime.Now);
                        }

                        if (Aherftax.Count != 0 && Aherftax.Count == 6 && !row.Text.Contains("City"))
                        {
                            City = Aherftax[0].Text;
                            school = Aherftax[1].Text;
                            Countyschool = Aherftax[2].Text;
                            College = Aherftax[3].Text;
                            Hospital = Aherftax[4].Text;
                            SpecialDistrict = Aherftax[5].Text;

                            string taxhisdetails = title + "~" + City.Trim() + "~" + school.Trim() + "~" + Countyschool.Trim() + "~" + College.Trim() + "~" + Hospital.Trim() + "~" + SpecialDistrict.Trim();
                            gc.insert_date(orderNumber, Parcelnumber, 2148, taxhisdetails, 1, DateTime.Now);

                        }
                        if (row.Text.Trim() != "" & Aherftax.Count != 0 && !row.Text.Contains("City") && !row.Text.Contains("Tax Rate per $100") && !row.Text.Contains("Taxable Value") && !row.Text.Contains("Estimated Taxes") && !row.Text.Contains("Tax Ceiling"))
                        {
                            a1 = Aherftax[0].Text;
                            a2 = Aherftax[1].Text;
                            a3 = Aherftax[2].Text;
                            a4 = Aherftax[3].Text;
                            a5 = Aherftax[4].Text;
                            a6 = Aherftax[5].Text;

                            if (entitylink1.Any(str => str.Contains(a1)))
                            {
                                scenario.Add(a1);

                            }
                            if (entitylink1.Any(str => str.Contains(a2)))
                            {
                                scenario.Add(a2);

                            }
                            if (entitylink1.Any(str => str.Contains(a3)))
                            {
                                scenario.Add(a3);

                            }
                            if (entitylink1.Any(str => str.Contains(a4)))
                            {
                                scenario.Add(a4);

                            }
                            if (entitylink1.Any(str => str.Contains(a5)))
                            {
                                scenario.Add(a5);

                            }
                            if (entitylink1.Any(str => str.Contains(a6)))
                            {
                                scenario.Add(a6);

                            }
                        }
                        a1 = ""; a2 = ""; a3 = ""; a4 = ""; a5 = ""; a6 = "";
                    }
                    //City of Garland Tax Information Details
                    int SameLink0 = 0;
                    int SameLink1 = 0;
                    int SameLink2 = 0;
                    int SameLink3 = 0;
                    int SameLink4 = 0;
                    int SameLink5 = 0;
                    string Link0 = "", Link1 = "", Link2 = "", Link4 = "";
                    var chromeOptionsdriver = new ChromeOptions();
                    var chDriver = new ChromeDriver(chromeOptionsdriver);
                    for (int k = 0; k < scenario.Count; k++)
                    {
                        Jurisdiction = scenario[k];
                        //Gridview show details                        
                        if (Jurisdiction == "DALLAS COUNTY" || Jurisdiction == "DALLAS CO COMMUNITY COLLEGE" || Jurisdiction == "PARKLAND HOSPITAL" || Jurisdiction == "DALLAS" || Jurisdiction == "ADDISON" || Jurisdiction == "BALCH SPRINGS" || Jurisdiction == "CARROLLTON" || Jurisdiction == "CEDAR HILL" || Jurisdiction == "COCKRELL HILL" || Jurisdiction == "COPPELL" || Jurisdiction == "DESOTO" || Jurisdiction == "DUNCANVILLE" || Jurisdiction == "FARMERS BRANCH" || Jurisdiction == "GLENN HEIGHTS" || Jurisdiction == "GRAND PRAIRIE" || Jurisdiction == "HIGHLAND PARK" || Jurisdiction == "HUTCHINS" || Jurisdiction == "IRVING" || Jurisdiction == "LANCASTER" || Jurisdiction == "RICHARDSON" || Jurisdiction == "ROWLETT" || Jurisdiction == "SACHSE" || Jurisdiction == "SEAGOVILLE" || Jurisdiction == "SUNNYVALE" || Jurisdiction == "UNIVERSITY PARK" || Jurisdiction == "WILMER" || Jurisdiction == "CEDAR HILL ISD" || Jurisdiction == "COPPELL ISD" || Jurisdiction == "DALLAS ISD" || Jurisdiction == "DESOTO ISD" || Jurisdiction == "DUNCANVILLE ISD" || Jurisdiction == "GRAND PRAIRIE ISD" || Jurisdiction == "HIGHLAND PARK ISD" || Jurisdiction == "LANCASTER ISD" || Jurisdiction == "SUNNYVALE ISD" || Jurisdiction == "IRVING FCD, SECTION I" || Jurisdiction == "IRVING FCD, SECTION III")
                        {
                            Link0 += Jurisdiction + ",";
                        }
                        if (Jurisdiction == "COMBINE" || Jurisdiction == "FERRIS" || Jurisdiction == "OVILLA" || Jurisdiction == "FERRIS ISD" || Jurisdiction == "IRVING ISD")
                        {
                            Link1 += Jurisdiction + ",";
                        }
                        if (Jurisdiction == "GARLAND" || Jurisdiction == "GRAPEVINE" || Jurisdiction == "CARROLLTON-FARMERS BRANCH ISD" || Jurisdiction == "GARLAND ISD" || Jurisdiction == "GRAPEVINE-COLLEYVILLE ISD" || Jurisdiction == "RICHARDSON ISD")
                        {
                            Link2 += Jurisdiction + ",";
                        }
                        if (Jurisdiction == "DENTON CO LEVEE IMPR DIST1" || Jurisdiction == "DENTON CO LID1 AND RUD1" || Jurisdiction == "LANCASTER MUD1")
                        {
                            Link4 += Jurisdiction + ",";
                        }
                        //Link 0
                        if (SameLink0 < 1 && (Jurisdiction == "DALLAS COUNTY" || Jurisdiction == "DALLAS CO COMMUNITY COLLEGE" || Jurisdiction == "PARKLAND HOSPITAL" || Jurisdiction == "DALLAS" || Jurisdiction == "ADDISON" || Jurisdiction == "BALCH SPRINGS" || Jurisdiction == "CARROLLTON" || Jurisdiction == "CEDAR HILL" || Jurisdiction == "COCKRELL HILL" || Jurisdiction == "COPPELL" || Jurisdiction == "DESOTO" || Jurisdiction == "DUNCANVILLE" || Jurisdiction == "FARMERS BRANCH" || Jurisdiction == "GLENN HEIGHTS" || Jurisdiction == "GRAND PRAIRIE" || Jurisdiction == "HIGHLAND PARK" || Jurisdiction == "HUTCHINS" || Jurisdiction == "IRVING" || Jurisdiction == "LANCASTER" || Jurisdiction == "RICHARDSON" || Jurisdiction == "ROWLETT" || Jurisdiction == "SACHSE" || Jurisdiction == "SEAGOVILLE" || Jurisdiction == "SUNNYVALE" || Jurisdiction == "UNIVERSITY PARK" || Jurisdiction == "WILMER" || Jurisdiction == "CEDAR HILL ISD" || Jurisdiction == "COPPELL ISD" || Jurisdiction == "DALLAS ISD" || Jurisdiction == "DESOTO ISD" || Jurisdiction == "DUNCANVILLE ISD" || Jurisdiction == "GRAND PRAIRIE ISD" || Jurisdiction == "HIGHLAND PARK ISD" || Jurisdiction == "LANCASTER ISD" || Jurisdiction == "SUNNYVALE ISD" || Jurisdiction == "IRVING FCD, SECTION I" || Jurisdiction == "IRVING FCD, SECTION III"))
                        {
                            driver.Navigate().GoToUrl("https://www.dallasact.com/act_webdev/dallas/index.jsp");
                            gc.CreatePdf(orderNumber, Parcelnumber, "Property tax search", driver, "TX", "Dallas");
                            driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[1]/td/div[1]/a[3]")).Click();
                            Thread.Sleep(4000);
                            driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[1]/td/table/tbody/tr/td/center/form/table/tbody/tr[2]/td[2]/h3/input")).SendKeys(Parcelnumber);
                            gc.CreatePdf(orderNumber, Parcelnumber, "Tax Account search", driver, "TX", "Dallas");
                            driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[1]/td/table/tbody/tr/td/center/form/table/tbody/tr[3]/td/center/input")).Click();
                            Thread.Sleep(4000);
                            gc.CreatePdf(orderNumber, Parcelnumber, "Tax Account search Result", driver, "TX", "Dallas");
                            driver.FindElement(By.XPath("//*[@id='flextable']/tbody/tr/td[1]/a")).Click();
                            Thread.Sleep(4000);
                            gc.CreatePdf(orderNumber, Parcelnumber, "Property Tax Details", driver, "TX", "Dallas");

                            string AcNo = "", PropertyAdd = "", ProSiteAdd = "", LegalDesc = "", Currenttaxlevy = "", CurrentAmountDue = "", PrioryearDue = "", TotalAmountDue = "";
                            string Marketval = "", landvalue = "", Improval = "", Cappedval = "", Agrival = "", Exemptions = "";

                            string taxdata = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[2]/td/table[2]/tbody/tr/td[1]/h3")).Text.Replace("\r\n", " ");
                            AcNo = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[2]/td/table[2]/tbody/tr/td[1]/h3/b[1]")).Text.Replace("Account Number:", "").Trim();
                            PropertyAdd = gc.Between(taxdata, "Address:", "Property Site Address:").Trim();
                            ProSiteAdd = gc.Between(taxdata, "Property Site Address:", "Legal Description:").Trim();
                            LegalDesc = gc.Between(taxdata, "Legal Description:", "Current Tax Levy:").Trim();
                            Currenttaxlevy = gc.Between(taxdata, "Current Tax Levy:", "Current Amount Due:").Trim();
                            CurrentAmountDue = gc.Between(taxdata, "Current Amount Due:", "Prior Year Amount Due:").Trim();
                            PrioryearDue = gc.Between(taxdata, "Prior Year Amount Due:", "Total Amount Due:").Trim();
                            TotalAmountDue = GlobalClass.After(taxdata, "Total Amount Due:").Trim();

                            string taxdata2 = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[2]/td/table[2]/tbody/tr/td[2]/h3[2]")).Text.Replace("\r\n", " ");
                            Marketval = gc.Between(taxdata2, "Market Value:", "Land Value:").Trim();
                            landvalue = gc.Between(taxdata2, "Land Value:", "Improvement Value:").Trim();
                            Improval = gc.Between(taxdata2, "Improvement Value:", "Capped Value:").Trim();
                            Cappedval = gc.Between(taxdata2, "Capped Value:", "Agricultural Value:").Trim();
                            Agrival = gc.Between(taxdata2, "Agricultural Value:", "Exemptions:").Trim();
                            Exemptions = gc.Between(taxdata2, "Exemptions:", "Current Tax Statement").Trim();

                            string propertytaxdetails = PropertyAdd + "~" + ProSiteAdd + "~" + LegalDesc + "~" + Currenttaxlevy + "~" + CurrentAmountDue + "~" + PrioryearDue + "~" + TotalAmountDue + "~" + Marketval + "~" + landvalue + "~" + Improval + "~" + Cappedval + "~" + Agrival + "~" + Exemptions;
                            gc.insert_date(orderNumber, Parcelnumber, 2156, propertytaxdetails, 1, DateTime.Now);


                            // Taxes Due Details by Year and Jurisdiction

                            driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[2]/td/table[2]/tbody/tr/td[2]/h3[2]/a[3]")).Click();
                            Thread.Sleep(4000);
                            gc.CreatePdf(orderNumber, Parcelnumber, "Taxes Due Details", driver, "TX", "Dallas");

                            IWebElement TaxDue = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[1]/td/center/table/tbody/tr/td/table/tbody"));
                            IList<IWebElement> TRTaxDue = TaxDue.FindElements(By.TagName("tr"));
                            IList<IWebElement> THTaxDue = TaxDue.FindElements(By.TagName("th"));
                            IList<IWebElement> TDTaxDue;
                            foreach (IWebElement row in TRTaxDue)
                            {
                                TDTaxDue = row.FindElements(By.TagName("td"));
                                if (TDTaxDue.Count != 0 && !row.Text.Contains("by end of") && !row.Text.Contains("Base Tax Due") && !row.Text.Contains("No taxes due"))
                                {
                                    string TDTaxInfodetails = TDTaxDue[0].Text + "~" + TDTaxDue[1].Text + "~" + TDTaxDue[2].Text + "~" + TDTaxDue[3].Text + "~" + TDTaxDue[4].Text + "~" + TDTaxDue[5].Text + "~" + TDTaxDue[6].Text + "~" + TDTaxDue[7].Text;
                                    gc.insert_date(orderNumber, parcelNumber, 2157, TDTaxInfodetails, 1, DateTime.Now);
                                }
                                if (TDTaxDue.Count != 0 && !row.Text.Contains("by end of") && !row.Text.Contains("Base Tax Due") && row.Text.Contains("No taxes due"))
                                {
                                    string TDTaxInfodetails = "" + "~" + TDTaxDue[0].Text + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "";
                                    gc.insert_date(orderNumber, Parcelnumber, 2157, TDTaxInfodetails, 1, DateTime.Now);
                                }
                            }

                            driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[1]/td/h3[2]/a[1]")).Click();
                            Thread.Sleep(4000);

                            // payment Information

                            driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[2]/td/table[2]/tbody/tr/td[2]/h3[2]/a[4]")).Click();
                            Thread.Sleep(4000);
                            gc.CreatePdf(orderNumber, Parcelnumber, "Payment Information", driver, "TX", "Dallas");

                            IWebElement payinfo = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[1]/td/table"));
                            IList<IWebElement> TRpayinfo = payinfo.FindElements(By.TagName("tr"));
                            IList<IWebElement> THpayinfo = payinfo.FindElements(By.TagName("th"));
                            IList<IWebElement> TDpayinfo;
                            foreach (IWebElement row in TRpayinfo)
                            {
                                TDpayinfo = row.FindElements(By.TagName("td"));
                                if (TDpayinfo.Count != 0 && !row.Text.Contains("Receipt Date"))
                                {
                                    string payInfodetails = TDpayinfo[0].Text + "~" + TDpayinfo[1].Text + "~" + TDpayinfo[2].Text + "~" + TDpayinfo[3].Text;
                                    gc.insert_date(orderNumber, Parcelnumber, 2158, payInfodetails, 1, DateTime.Now);
                                }
                                //if (TDpayinfo.Count != 0 && !row.Text.Contains("Receipt Date"))
                                //{
                                //    string payInfodetails = "" + "~" + TDpayinfo[0].Text + "~" + "" + "~" + "" ;
                                //    gc.insert_date(orderNumber, parcelNumber, 2158, payInfodetails, 1, DateTime.Now);
                                //}
                            }

                            driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[2]/td/h3[2]/a")).Click();
                            Thread.Sleep(4000);

                            // Composite Receipt
                            try
                            {
                                driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[2]/td/table[2]/tbody/tr/td[2]/h3[2]/a[5]")).Click();
                                Thread.Sleep(4000);
                                gc.CreatePdf(orderNumber, Parcelnumber, "Composite Receipt", driver, "TX", "Dallas");
                                var dd = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[1]/td/form/div[3]/select"));
                                IWebElement SelectOption = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[1]/td/form/div[3]/select"));
                                IList<IWebElement> Select = SelectOption.FindElements(By.TagName("option"));
                                List<string> option = new List<string>();
                                int count = 0, Check = 0;
                                foreach (IWebElement Op in Select)
                                {

                                    if (Select.Count - 1 == Check)
                                    {
                                        var SelectAddress = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[1]/td/form/div[3]/select"));
                                        var SelectAddressTax = new SelectElement(SelectAddress);
                                        SelectAddressTax.SelectByText(Op.Text);
                                        Thread.Sleep(4000);
                                        driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[1]/td/form/div[3]/input")).Click();
                                        Thread.Sleep(7000);
                                        driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[1]/td/div/h3/a")).Click();
                                        Thread.Sleep(4000);
                                        string currentwindow = driver.CurrentWindowHandle;
                                        driver.SwitchTo().Window(driver.WindowHandles.Last());
                                        string url = driver.Url;
                                        gc.downloadfile(url, orderNumber, Parcelnumber, "PaymentRecord", "TX", "Dallas");
                                        //  gc.CreatePdf(orderNumber, parcelNumber, "Payment Record", driver, "TX", "Dallas");
                                        driver.SwitchTo().Window(currentwindow);
                                        Thread.Sleep(4000);

                                    }
                                    Check++;
                                }

                                driver.Navigate().Back();
                                Thread.Sleep(4000);
                                driver.Navigate().Back();
                                Thread.Sleep(4000);

                                // Request an address Correction

                                driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[2]/td/table[2]/tbody/tr/td[2]/h3[2]/a[6]")).Click();
                                Thread.Sleep(4000);
                                gc.CreatePdf(orderNumber, Parcelnumber, "Request address correction", driver, "TX", "Dallas");
                                string certifiedAddress = "", AlternateAddress = "";
                                certifiedAddress = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[1]/td/center/table/tbody/tr[2]/td[1]/h5")).Text.Replace("\r\n", " ");
                                AlternateAddress = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[1]/td/center/table/tbody/tr[2]/td[2]/h5")).Text.Replace("\r\n", " ");

                                string RACdetails = certifiedAddress + "~" + AlternateAddress;
                                gc.insert_date(orderNumber, Parcelnumber, 2159, RACdetails, 1, DateTime.Now);
                                driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[1]/td/h3[4]/a")).Click();
                                Thread.Sleep(4000);

                                // summary tax statement

                                driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[2]/td/table[2]/tbody/tr/td[2]/h3[2]/a[2]")).Click();
                                Thread.Sleep(4000);
                            }
                            catch { }
                            try
                            {
                                var chromeOptions = new ChromeOptions();

                                var downloadDirectory = ConfigurationManager.AppSettings["AutoPdf"];

                                chromeOptions.AddUserProfilePreference("download.default_directory", downloadDirectory);
                                chromeOptions.AddUserProfilePreference("download.prompt_for_download", false);
                                chromeOptions.AddUserProfilePreference("disable-popup-blocking", "true");
                                chromeOptions.AddUserProfilePreference("plugins.always_open_pdf_externally", true);
                                var driver1 = new ChromeDriver(chromeOptions);
                                driver1.Navigate().GoToUrl(driver.Url);
                                Thread.Sleep(2000);

                                try
                                {
                                    driver1.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[1]/td/h3/a")).Click();
                                    Thread.Sleep(4000);
                                    driver1.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[1]/td/div[1]/a[3]")).Click();
                                    Thread.Sleep(4000);
                                    driver1.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[1]/td/table/tbody/tr/td/center/form/table/tbody/tr[2]/td[2]/h3/input")).SendKeys(Parcelnumber);
                                    driver1.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[1]/td/table/tbody/tr/td/center/form/table/tbody/tr[3]/td/center/input")).Click();
                                    Thread.Sleep(4000);
                                    driver1.FindElement(By.XPath("//*[@id='flextable']/tbody/tr/td[1]/a")).Click();
                                    Thread.Sleep(4000);
                                    driver1.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[2]/td/table[2]/tbody/tr/td[2]/h3[2]/a[2]")).Click();
                                    Thread.Sleep(4000);
                                }
                                catch { }

                                driver1.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[2]/td/div/h3/a")).Click();
                                Thread.Sleep(4000);

                                string fileName = "";
                                fileName = latestfilename();
                                Thread.Sleep(3000);
                                gc.AutoDownloadFile(orderNumber, Parcelnumber, "Dallas", "TX", fileName);
                                Thread.Sleep(4000);
                                string currentwindow2 = driver1.Url;
                                driver1.SwitchTo().Window(driver1.WindowHandles.Last());
                                Thread.Sleep(4000);
                                //string url2 = driver.Url;
                                //gc.downloadfile(url2, orderNumber, parcelNumber, "Tax Statement", "TX", "Dallas");
                                //gc.CreatePdf(orderNumber, parcelNumber, "Tax Statement", driver1, "TX", "Dallas");
                                //driver1.SwitchTo().Window(currentwindow2);
                                driver1.Navigate().Back();
                                Thread.Sleep(4000);

                                // current Tax Statement
                                try
                                {
                                    driver1.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[1]/td/h3/a")).Click();
                                    Thread.Sleep(4000);
                                    driver1.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[1]/td/div[1]/a[3]")).Click();
                                    Thread.Sleep(4000);
                                    driver1.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[1]/td/table/tbody/tr/td/center/form/table/tbody/tr[2]/td[2]/h3/input")).SendKeys(Parcelnumber);
                                    driver1.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[1]/td/table/tbody/tr/td/center/form/table/tbody/tr[3]/td/center/input")).Click();
                                    Thread.Sleep(4000);
                                    driver1.FindElement(By.XPath("//*[@id='flextable']/tbody/tr/td[1]/a")).Click();
                                    Thread.Sleep(4000);
                                    driver1.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[2]/td/table[2]/tbody/tr/td[2]/h3[2]/a[1]")).Click();
                                    Thread.Sleep(4000);
                                }
                                catch { }

                                string currentwindow1 = driver1.Url;
                                driver1.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[1]/td/div/h3/a")).Click();
                                Thread.Sleep(4000);
                                string fileName1 = "";
                                fileName1 = latestfilename();
                                Thread.Sleep(3000);
                                gc.AutoDownloadFile(orderNumber, Parcelnumber, "Dallas", "TX", fileName1);
                                Thread.Sleep(4000);
                                driver1.Quit();
                                try
                                {
                                    driver1.Navigate().Back();
                                    Thread.Sleep(4000);
                                }
                                catch { }
                            }
                            catch { }
                            SameLink0++;
                        }
                        //Deva Link 1      
                        //Chrome Driver
                        
                        if (SameLink1 < 1 && (Jurisdiction == "COMBINE" || Jurisdiction == "FERRIS" || Jurisdiction == "OVILLA" || Jurisdiction == "FERRIS ISD" || Jurisdiction == "IRVING ISD"))
                        {
                            IWebElement Itaxstmt1 = null;
                            string dstmt11 = "", stmt11 = "";
                            string Taxauthority1 = "", Account = "";
                            try
                            {
                                if (Jurisdiction == "IRVING ISD")
                                {
                                    chDriver.Navigate().GoToUrl("https://actweb.acttax.com/act_webdev/irving/index.jsp");
                                }
                                if (Jurisdiction == "COMBINE")
                                {
                                    chDriver.Navigate().GoToUrl("https://actweb.acttax.com/act_webdev/kaufman/index.jsp");
                                }
                                if (SameLink1 < 1 && (Jurisdiction == "FERRIS" || Jurisdiction == "OVILLA" || Jurisdiction == "FERRIS ISD"))
                                {
                                    chDriver.Navigate().GoToUrl("https://actweb.acttax.com/act_webdev/ellis/index.jsp");
                                }
                                chDriver.FindElement(By.Id("sc4")).Click();
                                Thread.Sleep(2000);
                                Account = Parcelnumber;
                                chDriver.FindElement(By.Id("criteria")).SendKeys(Account);
                                gc.CreatePdf(orderNumber, Account, "Parcel Number" + Jurisdiction, chDriver, "TX", "Dallas");
                                chDriver.FindElement(By.Name("submit")).Click();
                                Thread.Sleep(2000);
                                gc.CreatePdf(orderNumber, Account, "Parcel search result" + Jurisdiction, chDriver, "TX", "Dallas");
                                //*[@id="data-block"]/table/tbody/tr/td/table/tbody/tr/td[1]
                                // /html/body
                                // /html/body

                                //*[@id="content"]/table/tbody/tr[1]/td/table[2]/tbody/tr/td/table/tbody
                                //*[@id="content"]/table/tbody/tr[1]/td/table[2]/tbody/tr/td/table/tbody/tr[2]/td[1]
                                IWebElement Acountlink = chDriver.FindElement(By.XPath("/html/body"));
                                IList<IWebElement> Acountread = Acountlink.FindElements(By.TagName("a"));
                                foreach (IWebElement Acount in Acountread)
                                {
                                    if (Acount.Text.Trim() == Account.Trim())
                                    {
                                        Acount.Click();
                                        break;
                                    }
                                }
                                Thread.Sleep(2000);
                                //*[@id="pageContent"]/table/tbody/tr[2]/td/table[2]/tbody/tr/td[1]
                                String taxinformation1 = chDriver.FindElement(By.XPath("/html/body")).Text;
                                string AccountNumber = gc.Between(taxinformation1, "Account Number:", "Address:").Trim();
                                string OwnernameT = gc.Between(taxinformation1, "Address:", "Property Site Address:");
                                string LegalDescription = gc.Between(taxinformation1, "Legal Description:", "Current Tax Levy:");
                                string CurrentTaxLevy = gc.Between(taxinformation1, "Current Tax Levy:", "Current Amount Due:");
                                string CurrentAmountDue = gc.Between(taxinformation1, "Current Amount Due:", "Prior Year Amount Due:");
                                string PriorYearAmountDue = gc.Between(taxinformation1, "Prior Year Amount Due:", "Total Amount Due:");
                                string TotalAmountDue = gc.Between(taxinformation1, "Total Amount Due:", "Last Payment Amount for Current Year Taxes:");
                                string LastPaymentAmount = gc.Between(taxinformation1, "Last Payment Amount for Current Year Taxes:", "Last Payer for Current Year Taxes:");
                                string LastPayer = gc.Between(taxinformation1, "Last Payer for Current Year Taxes:", "Last Payment Date for Current Year Taxes:");
                                string LastPaymentDate = gc.Between(taxinformation1, "Last Payment Date for Current Year Taxes:", "Active Lawsuits:");
                                //*[@id="pageContent"]/table/tbody/tr[2]/td/table[2]/tbody/tr/td[2]
                                string Taxinformation2 = chDriver.FindElement(By.XPath("/html/body")).Text;
                                string GrossValue = gc.Between(Taxinformation2, "Gross Value:", "Land Value:");
                                string LandValue = gc.Between(Taxinformation2, "Land Value:", "Improvement Value:");
                                string ImprovementValue = gc.Between(Taxinformation2, "Improvement Value:", "Capped Value:");
                                string CappedValue = gc.Between(Taxinformation2, "Capped Value:", "Agricultural Value:");
                                string AgriculturalValue = gc.Between(Taxinformation2, "Agricultural Value:", "Exemptions:");
                                string Exemptions = gc.Between(Taxinformation2, "Exemptions:", "Exemption and Tax Rate Information");

                                string Taxheading = "Account Number" + "~" + "Address & Ownername" + "~" + "Legal Description" + "~" + "Current Tax Levy" + "~" + "Current Amount Due" + "~" + "Prior Year Amount Due" + "~" + "Total Amount Due" + "~" + "Last Payment Amount for Current Year Taxes" + "~" + "Last Payer for Current Year Taxes" + "~" + "Last Payment Date for Current Year Taxes" + "~" + "Gross Value" + "~" + "Land Value" + "~" + "Improvement Value" + "~" + "Capped Value" + "~" + "Agricultural Value" + "~" + "Exemptions";
                                db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + Taxheading + "' where Id = '2257'");
                                string Taxinfirmationresult = AccountNumber + "~" + OwnernameT + "~" + LegalDescription + "~" + CurrentTaxLevy + "~" + CurrentAmountDue + "~" + PriorYearAmountDue + "~" + TotalAmountDue + "~" + LastPaymentAmount + "~" + LastPayer + "~" + LastPaymentDate + "~" + GrossValue + "~" + LandValue + "~" + ImprovementValue + "~" + CappedValue + "~" + AgriculturalValue + "~" + Exemptions;
                                gc.insert_date(orderNumber, Parcelnumber, 2257, Taxinfirmationresult, 1, DateTime.Now);


                                chDriver.FindElement(By.LinkText("Exemption and Tax Rate Information")).Click();
                                Thread.Sleep(2000);
                                gc.CreatePdf(orderNumber, Account, "Exemption and Tax Rate Information" + Jurisdiction, chDriver, "TX", "Dallas");
                                chDriver.FindElement(By.LinkText("Return to the Previous Page")).Click();
                                Thread.Sleep(2000);
                                chDriver.FindElement(By.LinkText("Taxes Due Detail by Year and Jurisdiction")).Click();
                                Thread.Sleep(2000);
                                gc.CreatePdf(orderNumber, Account, "Taxes Due Detail by Year and Jurisdiction" + Jurisdiction, chDriver, "TX", "Dallas");
                                chDriver.FindElement(By.LinkText("Return to the Previous Page")).Click();
                                Thread.Sleep(2000);

                                //Tax Payment Details Table: 

                                IWebElement clickaddress = chDriver.FindElement(By.XPath("/html/body"));
                                IList<IWebElement> tableread = clickaddress.FindElements(By.TagName("a"));
                                foreach (IWebElement tablerow in tableread)
                                {
                                    if (tablerow.Text.Contains("Payment Information"))
                                    {
                                        tablerow.Click();
                                        break;
                                    }
                                }
                                Thread.Sleep(2000);

                                gc.CreatePdf(orderNumber, Account, "Payment Information" + Jurisdiction, chDriver, "TX", "Dallas");
                                //Account Number~Paid Date~Amount~Tax Year~Description~Paid By
                                // /html/body/table[2]/tbody/tr/td[2]/table/tbody/tr[1]/td/h3[3]
                                // string accountnumber2 = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td[2]/table/tbody/tr[1]/td/h3[3]")).Text.Replace("Account No.:", "");
                                //*[@id="pageContent"]/table/tbody/tr[1]/td/table/tbody
                                IWebElement multitableElement32 = chDriver.FindElement(By.XPath("/html/body"));
                                IList<IWebElement> multitableRow32 = multitableElement32.FindElements(By.TagName("tr"));
                                IList<IWebElement> multirowTD32;
                                foreach (IWebElement row in multitableRow32)
                                {
                                    multirowTD32 = row.FindElements(By.TagName("td"));
                                    if (multirowTD32.Count == 5 && row.Text.Contains("Receipt Date"))
                                    {
                                        string Paymentheading = multirowTD32[0].Text.Trim() + "~" + multirowTD32[1].Text.Trim() + "~" + multirowTD32[2].Text.Trim() + "~" + multirowTD32[3].Text.Trim() + "~" + multirowTD32[4].Text.Trim();
                                        db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + Paymentheading + "' where Id = '2258'");
                                    }
                                    if (multirowTD32.Count == 5 && !row.Text.Contains("Receipt Date"))
                                    {
                                        string TaxesDue = multirowTD32[0].Text.Trim() + "~" + multirowTD32[1].Text.Trim() + "~" + multirowTD32[2].Text.Trim() + "~" + multirowTD32[3].Text.Trim() + "~" + multirowTD32[4].Text.Trim();
                                        gc.insert_date(orderNumber, Account, 2258, TaxesDue, 1, DateTime.Now);
                                    }
                                }
                            }
                            catch { }
                            try
                            {
                                chDriver.FindElement(By.LinkText("Return to the Previous Page")).Click();
                                Thread.Sleep(2000);
                            }
                            catch { }
                            //Tax Statement Pdf Download
                            try
                            {

                                Itaxstmt1 = chDriver.FindElement(By.LinkText("Current Tax Statement"));
                                Thread.Sleep(2000);
                                gc.CreatePdf(orderNumber, Account, "Current Tax Statement" + Jurisdiction, chDriver, "TX", "Dallas");
                                stmt11 = Itaxstmt1.GetAttribute("href");
                                try
                                {
                                    IWebElement Itaxdstmt1 = chDriver.FindElement(By.LinkText("Delinquent Tax Statement"));
                                    Thread.Sleep(2000);
                                    gc.CreatePdf(orderNumber, Account, "Delinquent Tax Statement" + Jurisdiction, chDriver, "TX", "Dallas");
                                    dstmt11 = Itaxdstmt1.GetAttribute("href");
                                }
                                catch { }

                                chDriver.Navigate().GoToUrl(stmt11);
                                Thread.Sleep(4000);
                                gc.CreatePdf(orderNumber, Account, "Current Tax Statement Pdf" + Jurisdiction, chDriver, "TX", "Dallas");
                                IWebElement clickaddress = chDriver.FindElement(By.XPath("/html/body"));
                                IList<IWebElement> tableread = clickaddress.FindElements(By.TagName("a"));
                                foreach (IWebElement tablerow in tableread)
                                {
                                    if (tablerow.Text.Contains("here"))
                                    {
                                        string stmt1 = tablerow.GetAttribute("href");
                                        gc.downloadfile(stmt1, orderNumber, Parcelnumber, "Tax statement" + Jurisdiction, "TX", "Dallas");
                                        Thread.Sleep(2000);
                                        break;
                                    }
                                }
                                try
                                {
                                    chDriver.Navigate().GoToUrl(dstmt11);
                                    Thread.Sleep(4000);
                                    gc.CreatePdf(orderNumber, Account, "Delinquent Tax Statement Pdf" + Jurisdiction, chDriver, "TX", "Dallas");
                                    IWebElement clickaddress1 = chDriver.FindElement(By.XPath("/html/body"));
                                    IList<IWebElement> tableread1 = clickaddress1.FindElements(By.TagName("a"));
                                    foreach (IWebElement tablerow1 in tableread1)
                                    {
                                        if (tablerow1.Text.Contains("here"))
                                        {
                                            string dstmt1 = tablerow1.GetAttribute("href");
                                            gc.downloadfile(dstmt1, orderNumber, Parcelnumber, "Tax Delinquent statement" + Jurisdiction, "TX", "Dallas");
                                            Thread.Sleep(2000);
                                            break;
                                        }
                                    }
                                }
                                catch { }

                            }
                            catch { }
                            SameLink1++;
                        }
                        //Link 2 
                        if (SameLink2 < 1 && (Jurisdiction == "GARLAND" || Jurisdiction == "GRAPEVINE" || Jurisdiction == "CARROLLTON-FARMERS BRANCH ISD" || Jurisdiction == "GARLAND ISD" || Jurisdiction == "GRAPEVINE-COLLEYVILLE ISD" || Jurisdiction == "RICHARDSON ISD"))
                        {
                            if (Jurisdiction == "GARLAND")
                            {
                                chDriver.Navigate().GoToUrl("https://www.texaspayments.com/057120");
                                Thread.Sleep(3000);
                            }
                            if (Jurisdiction == "GRAPEVINE")
                            {
                                chDriver.Navigate().GoToUrl("https://www.texaspayments.com/220906");
                                Thread.Sleep(3000);
                            }
                            if (Jurisdiction == "CARROLLTON-FARMERS BRANCH ISD")
                            {
                                chDriver.Navigate().GoToUrl("https://www.texaspayments.com/057903");
                                Thread.Sleep(3000);
                            }
                            if (Jurisdiction == "GARLAND ISD")
                            {
                                chDriver.Navigate().GoToUrl("https://www.texaspayments.com/057909");
                                Thread.Sleep(3000);
                            }
                            if (Jurisdiction == "GRAPEVINE-COLLEYVILLE ISD")
                            {
                                chDriver.Navigate().GoToUrl("https://www.texaspayments.com/220906");
                                Thread.Sleep(3000);
                            }
                            if (Jurisdiction == "RICHARDSON ISD")
                            {
                                chDriver.Navigate().GoToUrl("https://www.texaspayments.com/057916");
                                Thread.Sleep(6000);
                            }

                            //driver.FindElement(By.XPath("//*[@id='ddlSearch']/span/span/span[2]/span")).Click();
                            //Thread.Sleep(1000);
                            //IWebElement IParcelClick = chDriver.FindElement(By.XPath("//*[@id='ddlSearch']/span/span/span[2]"));
                            //IParcelClick.Click();
                            //Thread.Sleep(6000);
                            //IParcelClick.SendKeys("Search by CAD Number");
                            //Thread.Sleep(6000);
                            try
                            {
                                IWebElement IAddressSearchCad1 = chDriver.FindElement(By.XPath("//*[@id='ddl_accountsearch_listbox']/li[4]"));
                                IJavaScriptExecutor jscad1 = chDriver as IJavaScriptExecutor;
                                jscad1.ExecuteScript("arguments[0].click();", IAddressSearchCad1);
                                Thread.Sleep(2000);
                                gc.CreatePdf(orderNumber, Parcelnumber, "Tax Details Click1", chDriver, "TX", "Dallas");
                            }
                            catch { }

                           

                            IWebElement IAddressSearchCad = chDriver.FindElement(By.Id("ddl_accountsearch_listbox"));
                            IJavaScriptExecutor jscad = chDriver as IJavaScriptExecutor;
                            jscad.ExecuteScript("arguments[0].click();", IAddressSearchCad);
                            Thread.Sleep(2000);
                            chDriver.FindElement(By.Id("searchValue")).SendKeys(Parcelnumber);
                            chDriver.FindElement(By.Id("searchBtn")).Click();
                            Thread.Sleep(15000);
                            gc.CreatePdf(orderNumber, Parcelnumber, "Tax Details Click2", chDriver, "TX", "Dallas");

                            //Pay Tax Details
                            try
                            {
                                List<string> billinfo = new List<string>();
                                IWebElement Billsinfo2 = chDriver.FindElement(By.Id("AccountInfoGrid"));
                                IList<IWebElement> TRBillsinfo22 = Billsinfo2.FindElements(By.TagName("tr"));
                                IList<IWebElement> Aherftax2;
                                // int i = 0;
                                foreach (IWebElement row in TRBillsinfo22)
                                {
                                    Aherftax2 = row.FindElements(By.TagName("td"));

                                    if (Aherftax2.Count != 0 && Aherftax2.Count == 4 && !row.Text.Contains("Show Detail") /*&& !row.Text.Contains("1st Installment 2nd Installment") && !row.Text.Contains("Bill Type")*/)
                                    {
                                        string Year = Aherftax2[1].Text;
                                        string CurrentLevy1 = Aherftax2[2].Text;
                                        string Amountdue = Aherftax2[3].Text;

                                        string Paytaxdetails = Year.Trim() + "~" + CurrentLevy1.Trim() + "~" + Amountdue.Trim();
                                        gc.insert_date(orderNumber, Parcelnumber, 2149, Paytaxdetails, 1, DateTime.Now);
                                    }
                                }
                            }
                            catch { }
                            //Pay Taxes Information Details
                            string Year1 = "", Taxyear = "", Totalmarketval = "", Homesteadcap = "", Totalappraised = "", Homesteadexemption = "", DisabledPersonOverExemption = "", DisabledVeteransExemption = "", OtherExemptionDeferrals = "", TaxableValue = "", FrozenTaxInformation = "", TaxRate = "", CertifiedLevy = "", CurrentLevy2 = "", TaxesDue = "", PenaltyandInterest = "", AttorneyFees = "", OtherDue = "", AmountDue = "", TotalAmountPaid = "", LastCheckNumber = "", LastPayDate = "";
                            int Cityofgary = 0;
                            chDriver.FindElement(By.Id("lblShowAllYearDetail")).Click();
                            Thread.Sleep(5000);
                            gc.CreatePdf(orderNumber, Parcelnumber, "Tax Details click3", chDriver, "TX", "Dallas");
                            IWebElement Paytaxinfo = chDriver.FindElement(By.XPath("//*[@id='AccountBalance']/div[1]/div[5]"));
                            IList<IWebElement> PaytaxinfoRow = Paytaxinfo.FindElements(By.TagName("div"));
                            int rowcount = PaytaxinfoRow.Count;
                            for (int p = 0; p <= rowcount; p++)
                            {
                                if (Cityofgary < 7 && PaytaxinfoRow[p].Text.Contains("CITY OF GARLAND"))
                                {
                                    if (p == 0)
                                    {
                                        Year1 = PaytaxinfoRow[0].Text.Trim();
                                        Totalmarketval = gc.Between(Year1, "Total Market Value:", "10% Homestead Cap/Ag Deferral:");
                                        Homesteadcap = gc.Between(Year1, "10% Homestead Cap/Ag Deferral:", "Total Appraised Value:");
                                        Totalappraised = gc.Between(Year1, "Total Appraised Value:", "Homestead Exemption:");
                                        Homesteadexemption = gc.Between(Year1, "Homestead Exemption:", "Disabled Person/Over 65 Exemption:");
                                        DisabledPersonOverExemption = gc.Between(Year1, "Disabled Person/Over 65 Exemption:", "Disabled Veterans Exemption:");
                                        DisabledVeteransExemption = gc.Between(Year1, "Disabled Veterans Exemption:", "Other Exemption/Deferrals:");
                                        OtherExemptionDeferrals = gc.Between(Year1, "Other Exemption/Deferrals:", "Taxable Value:");
                                        TaxableValue = gc.Between(Year1, "Taxable Value:", "Frozen Tax Information:");
                                        FrozenTaxInformation = gc.Between(Year1, "Frozen Tax Information:", "Tax Rate:");
                                        TaxRate = gc.Between(Year1, "Tax Rate:", "Certified Levy:");
                                        CertifiedLevy = gc.Between(Year1, "Certified Levy:", "Current Levy:");
                                        CurrentLevy2 = gc.Between(Year1, "Current Levy:", "Taxes Due:");
                                        TaxesDue = gc.Between(Year1, "Taxes Due:", "Penalty and Interest Due:");
                                        PenaltyandInterest = gc.Between(Year1, "Penalty and Interest Due:", "Attorney Fees Due:");
                                        AttorneyFees = gc.Between(Year1, "Attorney Fees Due:", "Other Due:");
                                        OtherDue = gc.Between(Year1, "Other Due:", "Amount Due:");
                                        AmountDue = gc.Between(Year1, "Amount Due:", "Total Amount Paid:");
                                        TotalAmountPaid = gc.Between(Year1, "Total Amount Paid:", "Last Check Number:");
                                        LastCheckNumber = gc.Between(Year1, "Last Check Number:", "Last Pay Date:");
                                        LastPayDate = GlobalClass.After(Year1, "Last Pay Date:").Replace("\r\n", "").Trim();

                                    }
                                    if (p == 2)
                                    {
                                        Taxyear = PaytaxinfoRow[2].Text.Trim();
                                        string Paytaxinformadetails = Taxyear.Trim() + "~" + Totalmarketval.Trim() + "~" + Homesteadcap.Trim() + "~" + Totalappraised.Trim() + "~" + Homesteadexemption.Trim() + "~" + DisabledPersonOverExemption.Trim() + "~" + DisabledVeteransExemption.Trim() + "~" + OtherExemptionDeferrals.Trim() + "~" + TaxableValue.Trim() + "~" + FrozenTaxInformation.Trim() + "~" + TaxRate.Trim() + "~" + CertifiedLevy.Trim() + "~" + CurrentLevy2.Trim() + "~" + TaxesDue.Trim() + "~" + PenaltyandInterest.Trim() + "~" + AttorneyFees.Trim() + "~" + OtherDue.Trim() + "~" + AmountDue.Trim() + "~" + TotalAmountPaid.Trim() + "~" + LastCheckNumber.Trim() + "~" + LastPayDate.Trim();
                                        gc.insert_date(orderNumber, Parcelnumber, 2150, Paytaxinformadetails, 1, DateTime.Now);
                                        Year1 = ""; Taxyear = ""; Totalmarketval = ""; Homesteadcap = ""; Totalappraised = ""; Homesteadexemption = ""; DisabledPersonOverExemption = ""; DisabledVeteransExemption = ""; OtherExemptionDeferrals = ""; TaxableValue = ""; FrozenTaxInformation = ""; TaxRate = ""; CertifiedLevy = ""; CurrentLevy2 = ""; TaxesDue = ""; PenaltyandInterest = ""; AttorneyFees = ""; OtherDue = ""; AmountDue = ""; TotalAmountPaid = ""; LastCheckNumber = ""; LastPayDate = "";
                                    }
                                    if (p == 66)
                                    {
                                        Year1 = PaytaxinfoRow[66].Text.Trim();
                                        Totalmarketval = gc.Between(Year1, "Total Market Value:", "10% Homestead Cap/Ag Deferral:");
                                        Homesteadcap = gc.Between(Year1, "10% Homestead Cap/Ag Deferral:", "Total Appraised Value:");
                                        Totalappraised = gc.Between(Year1, "Total Appraised Value:", "Homestead Exemption:");
                                        Homesteadexemption = gc.Between(Year1, "Homestead Exemption:", "Disabled Person/Over 65 Exemption:");
                                        DisabledPersonOverExemption = gc.Between(Year1, "Disabled Person/Over 65 Exemption:", "Disabled Veterans Exemption:");
                                        DisabledVeteransExemption = gc.Between(Year1, "Disabled Veterans Exemption:", "Other Exemption/Deferrals:");
                                        OtherExemptionDeferrals = gc.Between(Year1, "Other Exemption/Deferrals:", "Taxable Value:");
                                        TaxableValue = gc.Between(Year1, "Taxable Value:", "Frozen Tax Information:");
                                        FrozenTaxInformation = gc.Between(Year1, "Frozen Tax Information:", "Tax Rate:");
                                        TaxRate = gc.Between(Year1, "Tax Rate:", "Certified Levy:");
                                        CertifiedLevy = gc.Between(Year1, "Certified Levy:", "Current Levy:");
                                        CurrentLevy2 = gc.Between(Year1, "Current Levy:", "Taxes Due:");
                                        TaxesDue = gc.Between(Year1, "Taxes Due:", "Penalty and Interest Due:");
                                        PenaltyandInterest = gc.Between(Year1, "Penalty and Interest Due:", "Attorney Fees Due:");
                                        AttorneyFees = gc.Between(Year1, "Attorney Fees Due:", "Other Due:");
                                        OtherDue = gc.Between(Year1, "Other Due:", "Amount Due:");
                                        AmountDue = gc.Between(Year1, "Amount Due:", "Total Amount Paid:");
                                        TotalAmountPaid = gc.Between(Year1, "Total Amount Paid:", "Last Check Number:");
                                        LastCheckNumber = gc.Between(Year1, "Last Check Number:", "Last Pay Date:");
                                        LastPayDate = GlobalClass.After(Year1, "Last Pay Date:").Replace("\r\n", "").Trim();
                                    }
                                    if (p == 68)
                                    {

                                        Taxyear = PaytaxinfoRow[68].Text.Trim();
                                        string Paytaxinformadetails = Taxyear.Trim() + "~" + Totalmarketval.Trim() + "~" + Homesteadcap.Trim() + "~" + Totalappraised.Trim() + "~" + Homesteadexemption.Trim() + "~" + DisabledPersonOverExemption.Trim() + "~" + DisabledVeteransExemption.Trim() + "~" + OtherExemptionDeferrals.Trim() + "~" + TaxableValue.Trim() + "~" + FrozenTaxInformation.Trim() + "~" + TaxRate.Trim() + "~" + CertifiedLevy.Trim() + "~" + CurrentLevy2.Trim() + "~" + TaxesDue.Trim() + "~" + PenaltyandInterest.Trim() + "~" + AttorneyFees.Trim() + "~" + OtherDue.Trim() + "~" + AmountDue.Trim() + "~" + TotalAmountPaid.Trim() + "~" + LastCheckNumber.Trim() + "~" + LastPayDate.Trim();
                                        gc.insert_date(orderNumber, Parcelnumber, 2150, Paytaxinformadetails, 1, DateTime.Now);
                                        Year1 = ""; Taxyear = ""; Totalmarketval = ""; Homesteadcap = ""; Totalappraised = ""; Homesteadexemption = ""; DisabledPersonOverExemption = ""; DisabledVeteransExemption = ""; OtherExemptionDeferrals = ""; TaxableValue = ""; FrozenTaxInformation = ""; TaxRate = ""; CertifiedLevy = ""; CurrentLevy2 = ""; TaxesDue = ""; PenaltyandInterest = ""; AttorneyFees = ""; OtherDue = ""; AmountDue = ""; TotalAmountPaid = ""; LastCheckNumber = ""; LastPayDate = "";
                                    }

                                    if (p == 132)
                                    {
                                        Year1 = PaytaxinfoRow[132].Text.Trim();
                                        Totalmarketval = gc.Between(Year1, "Total Market Value:", "10% Homestead Cap/Ag Deferral:");
                                        Homesteadcap = gc.Between(Year1, "10% Homestead Cap/Ag Deferral:", "Total Appraised Value:");
                                        Totalappraised = gc.Between(Year1, "Total Appraised Value:", "Homestead Exemption:");
                                        Homesteadexemption = gc.Between(Year1, "Homestead Exemption:", "Disabled Person/Over 65 Exemption:");
                                        DisabledPersonOverExemption = gc.Between(Year1, "Disabled Person/Over 65 Exemption:", "Disabled Veterans Exemption:");
                                        DisabledVeteransExemption = gc.Between(Year1, "Disabled Veterans Exemption:", "Other Exemption/Deferrals:");
                                        OtherExemptionDeferrals = gc.Between(Year1, "Other Exemption/Deferrals:", "Taxable Value:");
                                        TaxableValue = gc.Between(Year1, "Taxable Value:", "Frozen Tax Information:");
                                        FrozenTaxInformation = gc.Between(Year1, "Frozen Tax Information:", "Tax Rate:");
                                        TaxRate = gc.Between(Year1, "Tax Rate:", "Certified Levy:");
                                        CertifiedLevy = gc.Between(Year1, "Certified Levy:", "Current Levy:");
                                        CurrentLevy2 = gc.Between(Year1, "Current Levy:", "Taxes Due:");
                                        TaxesDue = gc.Between(Year1, "Taxes Due:", "Penalty and Interest Due:");
                                        PenaltyandInterest = gc.Between(Year1, "Penalty and Interest Due:", "Attorney Fees Due:");
                                        AttorneyFees = gc.Between(Year1, "Attorney Fees Due:", "Other Due:");
                                        OtherDue = gc.Between(Year1, "Other Due:", "Amount Due:");
                                        AmountDue = gc.Between(Year1, "Amount Due:", "Total Amount Paid:");
                                        TotalAmountPaid = gc.Between(Year1, "Total Amount Paid:", "Last Check Number:");
                                        LastCheckNumber = gc.Between(Year1, "Last Check Number:", "Last Pay Date:");
                                        LastPayDate = GlobalClass.After(Year1, "Last Pay Date:").Replace("\r\n", "").Trim();

                                    }
                                    if (p == 134)
                                    {
                                        Taxyear = PaytaxinfoRow[134].Text.Trim();
                                        string Paytaxinformadetails = Taxyear.Trim() + "~" + Totalmarketval.Trim() + "~" + Homesteadcap.Trim() + "~" + Totalappraised.Trim() + "~" + Homesteadexemption.Trim() + "~" + DisabledPersonOverExemption.Trim() + "~" + DisabledVeteransExemption.Trim() + "~" + OtherExemptionDeferrals.Trim() + "~" + TaxableValue.Trim() + "~" + FrozenTaxInformation.Trim() + "~" + TaxRate.Trim() + "~" + CertifiedLevy.Trim() + "~" + CurrentLevy2.Trim() + "~" + TaxesDue.Trim() + "~" + PenaltyandInterest.Trim() + "~" + AttorneyFees.Trim() + "~" + OtherDue.Trim() + "~" + AmountDue.Trim() + "~" + TotalAmountPaid.Trim() + "~" + LastCheckNumber.Trim() + "~" + LastPayDate.Trim();
                                        gc.insert_date(orderNumber, Parcelnumber, 2150, Paytaxinformadetails, 1, DateTime.Now);
                                        Year1 = ""; Taxyear = ""; Totalmarketval = ""; Homesteadcap = ""; Totalappraised = ""; Homesteadexemption = ""; DisabledPersonOverExemption = ""; DisabledVeteransExemption = ""; OtherExemptionDeferrals = ""; TaxableValue = ""; FrozenTaxInformation = ""; TaxRate = ""; CertifiedLevy = ""; CurrentLevy2 = ""; TaxesDue = ""; PenaltyandInterest = ""; AttorneyFees = ""; OtherDue = ""; AmountDue = ""; TotalAmountPaid = ""; LastCheckNumber = ""; LastPayDate = "";
                                    }
                                    Cityofgary++;
                                }
                            }
                            SameLink2++;
                        }
                        if (SameLink3 < 1 && (Jurisdiction == "MESQUITE" || Jurisdiction == "MESQUITE ISD"))
                        {
                            driver.Navigate().GoToUrl("http://propertytax.cityofmesquite.com/MesquiteTax/");
                            Thread.Sleep(4000);
                            driver.FindElement(By.XPath("//*[@id='search']/table/tbody/tr/td[2]/input")).SendKeys(Parcelnumber);
                            gc.CreatePdf(orderNumber, Parcelnumber, "Tax search", driver, "TX", "Dallas");
                            driver.FindElement(By.Id("submit")).Click();
                            Thread.Sleep(4000);
                            gc.CreatePdf(orderNumber, Parcelnumber, "Tax search Result", driver, "TX", "Dallas");
                            driver.FindElement(By.XPath("/html/body/table[1]/tbody/tr/td[2]/table/tbody/tr[2]/td/div/table[3]/tbody/tr[2]/td[1]/a/font")).Click();
                            Thread.Sleep(4000);
                            gc.CreatePdf(orderNumber, parcelNumber, "Tax Assessment Details", driver, "TX", "Dallas");

                            string Accountno = "", Apd = "", Location = "", Legal = "", Owner = "", Acres = "", YearBuilt = "", Sqfeet = "", Defstart = "";
                            string DefEnd = "", Roll = "", UDI = "", Improvement = "", land = "";

                            Accountno = driver.FindElement(By.XPath("/html/body/table[1]/tbody/tr/td[2]/table/tbody/tr[2]/td/table/tbody/tr[2]/td/table[1]/tbody/tr/td[1]/table/tbody/tr[1]/td[2]/font")).Text;
                            Apd = driver.FindElement(By.XPath("/html/body/table[1]/tbody/tr/td[2]/table/tbody/tr[2]/td/table/tbody/tr[2]/td/table[1]/tbody/tr/td[1]/table/tbody/tr[2]/td[2]/font")).Text;
                            Location = driver.FindElement(By.XPath("/html/body/table[1]/tbody/tr/td[2]/table/tbody/tr[2]/td/table/tbody/tr[2]/td/table[1]/tbody/tr/td[1]/table/tbody/tr[3]/td[2]/font")).Text;
                            Legal = driver.FindElement(By.XPath("/html/body/table[1]/tbody/tr/td[2]/table/tbody/tr[2]/td/table/tbody/tr[2]/td/table[1]/tbody/tr/td[1]/table/tbody/tr[4]/td[2]/font")).Text;
                            Owner = driver.FindElement(By.XPath("/html/body/table[1]/tbody/tr/td[2]/table/tbody/tr[2]/td/table/tbody/tr[2]/td/table[1]/tbody/tr/td[1]/table/tbody/tr[5]/td[2]")).Text;
                            Acres = driver.FindElement(By.XPath("/html/body/table[1]/tbody/tr/td[2]/table/tbody/tr[2]/td/table/tbody/tr[2]/td/table[1]/tbody/tr/td[2]/table/tbody/tr[1]/td[2]/font")).Text;
                            YearBuilt = driver.FindElement(By.XPath("/html/body/table[1]/tbody/tr/td[2]/table/tbody/tr[2]/td/table/tbody/tr[2]/td/table[1]/tbody/tr/td[2]/table/tbody/tr[2]/td[2]/font")).Text;
                            Sqfeet = driver.FindElement(By.XPath("/html/body/table[1]/tbody/tr/td[2]/table/tbody/tr[2]/td/table/tbody/tr[2]/td/table[1]/tbody/tr/td[2]/table/tbody/tr[3]/td[2]/font")).Text;
                            Defstart = driver.FindElement(By.XPath("/html/body/table[1]/tbody/tr/td[2]/table/tbody/tr[2]/td/table/tbody/tr[2]/td/table[1]/tbody/tr/td[2]/table/tbody/tr[4]/td[2]/font")).Text;
                            DefEnd = driver.FindElement(By.XPath("/html/body/table[1]/tbody/tr/td[2]/table/tbody/tr[2]/td/table/tbody/tr[2]/td/table[1]/tbody/tr/td[2]/table/tbody/tr[5]/td[2]/font")).Text;
                            Roll = driver.FindElement(By.XPath("/html/body/table[1]/tbody/tr/td[2]/table/tbody/tr[2]/td/table/tbody/tr[2]/td/table[1]/tbody/tr/td[2]/table/tbody/tr[6]/td[2]/font")).Text;
                            UDI = driver.FindElement(By.XPath("/html/body/table[1]/tbody/tr/td[2]/table/tbody/tr[2]/td/table/tbody/tr[2]/td/table[1]/tbody/tr/td[2]/table/tbody/tr[7]/td[2]/font")).Text;
                            Improvement = driver.FindElement(By.XPath("/html/body/table[1]/tbody/tr/td[2]/table/tbody/tr[2]/td/table/tbody/tr[2]/td/table[1]/tbody/tr/td[3]/table/tbody/tr[2]/td[2]/font")).Text;
                            land = driver.FindElement(By.XPath("/html/body/table[1]/tbody/tr/td[2]/table/tbody/tr[2]/td/table/tbody/tr[2]/td/table[1]/tbody/tr/td[3]/table/tbody/tr[3]/td[2]/font")).Text;
                            //Parcelnumber
                            string propertytaxdetails = Accountno + "~" + Apd + "~" + Location + "~" + Legal + "~" + Owner + "~" + Acres + "~" + YearBuilt + "~" + Sqfeet + "~" + Defstart + "~" + DefEnd + "~" + Roll + "~" + UDI + "~" + Improvement + "~" + land;
                            gc.insert_date(orderNumber, Parcelnumber, 2151, propertytaxdetails, 1, DateTime.Now);

                            // Tax Details

                            IWebElement TaxInfo = driver.FindElement(By.XPath("/html/body/table[1]/tbody/tr/td[2]/table/tbody/tr[2]/td/table/tbody/tr[2]/td/div[2]/table/tbody"));
                            IList<IWebElement> TRTaxInfo = TaxInfo.FindElements(By.TagName("tr"));
                            IList<IWebElement> THTaxInfo = TaxInfo.FindElements(By.TagName("th"));
                            IList<IWebElement> TDTaxInfo;
                            foreach (IWebElement row in TRTaxInfo)
                            {
                                TDTaxInfo = row.FindElements(By.TagName("td"));
                                if (TDTaxInfo.Count != 0 && !row.Text.Contains("Penalty"))
                                {
                                    string TDTaxInfodetails = TDTaxInfo[0].Text + "~" + TDTaxInfo[1].Text + "~" + TDTaxInfo[2].Text + "~" + TDTaxInfo[3].Text + "~" + TDTaxInfo[4].Text + "~" + TDTaxInfo[5].Text + "~" + TDTaxInfo[6].Text + "~" + TDTaxInfo[7].Text + "~" + TDTaxInfo[8].Text + "~" + TDTaxInfo[9].Text;
                                    gc.insert_date(orderNumber, Parcelnumber, 2152, TDTaxInfodetails, 1, DateTime.Now);
                                }
                            }
                            SameLink3++;
                        }

                        //Link 7 dominic
                        //Jurisdiction = "DENTON CO LEVEE IMPR DIST1";
                        if (SameLink4 > 1 && (Jurisdiction == "DENTON CO LEVEE IMPR DIST1" || Jurisdiction == "DENTON CO LID1 AND RUD1" || Jurisdiction == "LANCASTER MUD1"))
                        {
                            List<string> Downloadstring = new List<string>();
                            try
                            {
                                driver.Navigate().GoToUrl("http://bli-tax.com/records/");
                                //Account = Account.Substring(0, 16).Trim();
                                string NumberAccount = "6780010020180";
                                driver.FindElement(By.Id("cadnumber")).SendKeys(NumberAccount);
                                driver.FindElement(By.XPath("//*[@id='cadno']/p[3]/input")).Click();
                                Thread.Sleep(2000);
                                gc.CreatePdf(orderNumber, Parcelnumber, "Mud Scenario Details", driver, "TX", "Dallas");
                                //IWebElement Multyaddresstable1 = driver.FindElement(By.XPath("/html/body/div[1]/iframe"));
                                //driver.SwitchTo().Frame(Multyaddresstable1);
                                IWebElement Parcelclicktable = driver.FindElement(By.XPath("//*[@id='post-2168']/table/tbody"));
                                IList<IWebElement> Parcelclickrow = Parcelclicktable.FindElements(By.TagName("tr"));
                                IList<IWebElement> parcelclickid;
                                foreach (IWebElement parcelclick in Parcelclickrow)
                                {
                                    parcelclickid = parcelclick.FindElements(By.TagName("td"));
                                    if (parcelclickid.Count != 0 && !parcelclick.Text.Contains("CAD Number"))
                                    {
                                        IWebElement carnumberclcik = parcelclickid[0].FindElement(By.TagName("a"));
                                        string cardnumberhref = carnumberclcik.GetAttribute("href");
                                        driver.Navigate().GoToUrl(cardnumberhref);
                                        Thread.Sleep(2000);
                                        break;
                                    }
                                }
                                //string year = driver.FindElement(By.Id("years")).Text;

                                for (int p = 0; p < 3; p++)
                                {
                                    string appricelresult = "", Exemptionhead = "", QualifiedExemptionsResult = "", Current_AsOf = "";
                                    string appricelresult1 = "", appricelhead = "", ExemptValuesResult = "", ExemptValuesHead = "";

                                    IWebElement mySelectElement = driver.FindElement(By.Id("years"));
                                    SelectElement dropdown = new SelectElement(mySelectElement);
                                    dropdown.SelectByIndex(p);
                                    Thread.Sleep(2000);
                                    IWebElement slelectyear = driver.FindElement(By.Id("years"));
                                    SelectElement dropdown1 = new SelectElement(slelectyear);
                                    string year = dropdown1.SelectedOption.Text;
                                    string propertydetail = driver.FindElement(By.XPath("//*[@id='taxform']/tbody/tr[1]/td/table/tbody")).Text;
                                    string ownernamepro = GlobalClass.Before(propertydetail, "Make Checks Payable To:");
                                    string[] ownerarray = ownernamepro.Split('\r');
                                    string ownername1 = ownerarray[0];
                                    string owneraddress = ownerarray[1].Replace("\n", "") + " " + ownerarray[2].Replace("\n", "");
                                    if (propertydetail.Contains("Current As Of"))
                                    {
                                        Current_AsOf = gc.Between(propertydetail, "Current As Of", "Account Number");
                                    }
                                    else
                                    {
                                        Current_AsOf = "";
                                    }
                                    string AccountNumber = gc.Between(propertydetail, "Account Number", "CAD Number");
                                    string CADNumber = GlobalClass.After(propertydetail, "CAD Number").Trim();
                                    string checktable = driver.FindElement(By.XPath("//*[@id='taxform']/tbody/tr[1]/td/table/tbody/tr/td[1]/p[3]")).Text;
                                    string Checkpayble = GlobalClass.After(checktable.Replace("\r\n", ""), "Make Checks Payable To:");
                                    string Legaltable = driver.FindElement(By.XPath("//*[@id='taxform']/tbody/tr[2]/td[1]/table[1]/tbody")).Text;
                                    string legaldescription = GlobalClass.After(Legaltable, "Property Description");

                                    try
                                    {
                                        IWebElement appricelvaluetable = driver.FindElement(By.XPath("//*[@id='taxform']/tbody/tr[2]/td[1]/table[2]/tbody"));
                                        IList<IWebElement> appricelvaluerow = appricelvaluetable.FindElements(By.TagName("tr"));
                                        IList<IWebElement> appricelvalueid;
                                        foreach (IWebElement appricelvalue in appricelvaluerow)
                                        {
                                            appricelvalueid = appricelvalue.FindElements(By.TagName("td"));
                                            if (appricelvalueid.Count != 0 && !appricelvalue.Text.Contains("Appraised Values"))
                                            {
                                                appricelhead += appricelvalueid[0].Text + "~";
                                                appricelresult += appricelvalueid[1].Text + "~";
                                            }
                                        }
                                    }
                                    catch { }
                                    try
                                    {
                                        IWebElement QualifiedExemptionsTable = driver.FindElement(By.XPath("//*[@id='taxform']/tbody/tr[2]/td[1]/table[3]/tbody"));
                                        IList<IWebElement> QualifiedExemptionsrow = QualifiedExemptionsTable.FindElements(By.TagName("tr"));
                                        IList<IWebElement> QualifiedExemptionsid;
                                        foreach (IWebElement QualifiedExemptions in QualifiedExemptionsrow)
                                        {
                                            QualifiedExemptionsid = QualifiedExemptions.FindElements(By.TagName("td"));

                                            if (QualifiedExemptionsid.Count != 0 && !QualifiedExemptions.Text.Contains("Qualified"))
                                            {
                                                QualifiedExemptionsResult = QualifiedExemptionsid[0].Text;
                                            }
                                        }
                                    }
                                    catch { }
                                    try
                                    {
                                        IWebElement ExemptValueTable = driver.FindElement(By.XPath("//*[@id='taxform']/tbody/tr[2]/td[1]/table[4]/tbody"));
                                        IList<IWebElement> ExemptValuesrow = ExemptValueTable.FindElements(By.TagName("tr"));
                                        IList<IWebElement> ExemptValuesid;
                                        foreach (IWebElement ExemptValues in ExemptValuesrow)
                                        {
                                            ExemptValuesid = ExemptValues.FindElements(By.TagName("td"));
                                            if (ExemptValuesid.Count != 0 && ExemptValues.Text.Contains("Tax Rate"))
                                            {
                                                ExemptValuesHead = ExemptValuesid[0].Text + "~" + ExemptValuesid[1].Text + "~" + ExemptValuesid[2].Text + "~" + ExemptValuesid[3].Text;
                                            }
                                            if (ExemptValuesid.Count != 0 && !ExemptValues.Text.Contains("Exempt Value"))
                                            {
                                                ExemptValuesResult = ExemptValuesid[0].Text + "~" + ExemptValuesid[1].Text + "~" + ExemptValuesid[2].Text + "~" + ExemptValuesid[3].Text;
                                            }
                                        }
                                    }
                                    catch { }
                                    try
                                    {
                                        IWebElement Exemptionstable = driver.FindElement(By.XPath("//*[@id='taxform']/tbody/tr[2]/td[1]/table[5]/tbody"));
                                        IList<IWebElement> Exemptionsrow = Exemptionstable.FindElements(By.TagName("tr"));
                                        IList<IWebElement> Exemptionsid;
                                        IList<IWebElement> Exemptionshead;
                                        foreach (IWebElement Exemptions1 in Exemptionsrow)
                                        {
                                            Exemptionsid = Exemptions1.FindElements(By.TagName("td"));
                                            Exemptionshead = Exemptions1.FindElements(By.TagName("th"));
                                            if (Exemptions1.Text.Contains("Homestead"))
                                            {
                                                Exemptionhead = Exemptionshead[0].Text + "~" + Exemptionshead[1].Text + "~" + Exemptionshead[2].Text;
                                            }
                                            if (Exemptionsid.Count != 0 && !Exemptions1.Text.Contains("Homestead"))
                                            {
                                                appricelresult1 = Exemptionsid[0].Text + "~" + Exemptionsid[1].Text + "~" + Exemptionsid[2].Text;
                                            }
                                        }
                                    }
                                    catch { }

                                    if (p == 0)
                                    {
                                        string Propertyfullhead = "Year" + "~" + "Owner Name" + "~" + "Owner Address" + "~" + "Current As Of" + "~" + "Account Number" + "~" + "CAD Number" + "~" + "Property Description" + "~" + appricelhead + Exemptionhead + "~" + "Qualified Exemptions" + "~" + "Exempt Value" + "~" + "Taxable Value" + "~" + "Tax Rate" + "~" + "Taxes" + "~" + "Make Checks Payable To";
                                        db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + Propertyfullhead + "' where Id = '" + 2258 + "'");
                                    }
                                    string propertyfullresult = year + "~" + ownername1 + "~" + owneraddress + "~" + Current_AsOf + "~" + AccountNumber + "~" + CADNumber + "~" + legaldescription + "~" + appricelresult + appricelresult1 + "~" + QualifiedExemptionsResult + "~" + ExemptValuesResult + "~" + Checkpayble;
                                    gc.insert_date(orderNumber, AccountNumber, 2259, propertyfullresult, 1, DateTime.Now);
                                    gc.CreatePdf(orderNumber, Parcelnumber, "Mud Scenario Details Table" + p, driver, "TX", "Dallas");

                                    //7th Scenario For MUD

                                    //Tax summary
                                    string Taxsummaryresult = "";
                                    IWebElement Taxsummarytable = driver.FindElement(By.XPath("//*[@id='taxform']/tbody/tr[2]/td[2]/table[1]/tbody"));
                                    IList<IWebElement> Taxsummaryrow = Taxsummarytable.FindElements(By.TagName("tr"));
                                    IList<IWebElement> taxsummaryid;
                                    foreach (IWebElement Taxsummary in Taxsummaryrow)
                                    {
                                        taxsummaryid = Taxsummary.FindElements(By.TagName("td"));
                                        if (taxsummaryid.Count != 0 && !Taxsummary.Text.Contains("Tax Summary"))
                                        {

                                            Taxsummaryresult = year + "~" + taxsummaryid[0].Text + "~" + taxsummaryid[1].Text;
                                            gc.insert_date(orderNumber, AccountNumber, 2260, Taxsummaryresult, 1, DateTime.Now);
                                        }

                                    }
                                    Taxsummaryresult = "";
                                    try
                                    {
                                        IWebElement Payingtable = driver.FindElement(By.XPath("//*[@id='taxform']/tbody/tr[2]/td[2]/table[2]/tbody"));
                                        IList<IWebElement> Payingrow = Payingtable.FindElements(By.TagName("tr"));
                                        IList<IWebElement> payingid;
                                        foreach (IWebElement Paying in Payingrow)
                                        {
                                            payingid = Paying.FindElements(By.TagName("td"));
                                            if (payingid.Count != 0)
                                            {
                                                string Payingresult = year + "~" + payingid[0].Text + "~" + payingid[1].Text + "~" + payingid[2].Text + "~" + payingid[3].Text + "~" + payingid[4].Text;
                                                gc.insert_date(orderNumber, AccountNumber, 2261, Payingresult, 1, DateTime.Now);
                                                Payingresult = "";
                                            }
                                        }
                                    }
                                    catch { }
                                    try
                                    {
                                        string Callalert = driver.FindElement(By.XPath("//*[@id='taxform']/tbody/tr[2]/td[2]/p[1]")).Text;
                                        if (Callalert.Contains("account information"))
                                        {
                                            string Alertmessage = "For tax amount due, you must call the MUD tax Collector's Office.";
                                            gc.insert_date(orderNumber, AccountNumber, 2262, Alertmessage, 1, DateTime.Now);
                                            Alertmessage = "";
                                        }

                                    }
                                    catch { }

                                    IWebElement Parceldownload = driver.FindElement(By.XPath("//*[@id='taxform-header']/div[3]/div[2]/a"));
                                    string Parcelhref = Parceldownload.GetAttribute("href");
                                    //Parceldownload.Click();
                                    //Thread.Sleep(2000);
                                    Downloadstring.Add(Parcelhref);

                                    // gc.downloadfile(Parcelhref, orderNumber, ParcelID, "Recept M.U.D 7" + p, "TX", "Dallas");
                                }
                                int Re = 0;
                                foreach (string receipt in Downloadstring)
                                {
                                    driver.Navigate().GoToUrl(receipt);
                                    Thread.Sleep(2000);
                                    gc.CreatePdf(orderNumber, Parcelnumber, "Recept M.U.D 7" + Re, driver, "TX", "Dallas");
                                    Re++;
                                }
                            }
                            catch { }

                            SameLink4++;
                        }
                        //Link 8 Dominic                        
                        if (Jurisdiction == "GRAND PRAIRIE METROPOLITAN URD")
                        {
                            string Account = "1144400020020";

                            driver.Navigate().GoToUrl("https://www.wheelerassoc.com/search");
                            string CadNo = "";
                            try
                            {
                                driver.FindElement(By.Id("MainContent_AccountTabContainer_TabPanelCAD_CadTextBox")).SendKeys(Account.Replace("-", "").Trim());
                                gc.CreatePdf_WOP(orderNumber, "AddressSearch", driver, "TX", "Dallas");
                                driver.FindElement(By.Id("MainContent_AccountTabContainer_TabPanelCAD_CadButton")).Click();
                                Thread.Sleep(2000);
                                try
                                {
                                    gc.CreatePdf_WOP(orderNumber, "AddressSearch Result", driver, "TX", "Dallas");
                                    IWebElement Mudtable = driver.FindElement(By.XPath("//*[@id='MainContent_AcctListGridView']/tbody"));
                                    IList<IWebElement> Mudrow = Mudtable.FindElements(By.TagName("tr"));
                                    IList<IWebElement> Mudid;
                                    foreach (IWebElement MudidMudid in Mudrow)
                                    {
                                        Mudid = MudidMudid.FindElements(By.TagName("td"));
                                        if (Mudid.Count != 0 && !MudidMudid.Text.Contains("CAD Account #"))
                                        {
                                            IWebElement accountmUd = Mudid[0].FindElement(By.TagName("a"));
                                            string Mudhref = accountmUd.GetAttribute("href");
                                            driver.Navigate().GoToUrl(Mudhref);
                                            Thread.Sleep(2000);
                                            break;
                                        }
                                    }
                                }
                                catch { }
                                for (int check = 0; check < 3; check++)
                                {
                                    //string current = driver.CurrentWindowHandle;
                                    if (check > 0)
                                    {
                                        //driver.SwitchTo().Window(current);
                                        IWebElement PropertyInformation = driver.FindElement(By.Id("MainContent_TaxYearDropDown"));
                                        SelectElement PropertyInformationSelect = new SelectElement(PropertyInformation);
                                        PropertyInformationSelect.SelectByIndex(i);
                                        Thread.Sleep(3000);
                                    }
                                    string Taxrate = "", PaymentsApplied = "", HomesteadExemption = "", lane = "", TaxLevied = "", Improvementstax = "", TaxableValue = "", Tax_Year_Balance = "";
                                    string Jurisdiction1 = driver.FindElement(By.Id("MainContent_DistrictNameTxt")).Text;
                                    string Tax_AuthorityMud4 = driver.FindElement(By.XPath("//*[@id='PrintArea']/div[1]")).Text;
                                    string Tax_Authority = GlobalClass.After(Tax_AuthorityMud4, "Jurisdiction").Trim();
                                    string Owner_Name = driver.FindElement(By.Id("MainContent_OwnerNameTxt")).Text;
                                    string OwnerAddress1 = driver.FindElement(By.Id("MainContent_OwnerAdd1Txt")).Text;
                                    string OwnerAddress2 = driver.FindElement(By.Id("MainContent_OwnerAdd2Txt")).Text;
                                    string FullOwnerAddress = OwnerAddress1 + " " + OwnerAddress2;
                                    string InquiryDate = driver.FindElement(By.Id("MainContent_DateTxt")).Text;
                                    string DelinquentDate = driver.FindElement(By.Id("MainContent_DelinquentTxt")).Text;
                                    CadNo = driver.FindElement(By.Id("MainContent_CadNoTxt")).Text;
                                    string TaxYear = driver.FindElement(By.Id("MainContent_TaxYearTxt")).Text;
                                    string JurisdictionCode = driver.FindElement(By.Id("MainContent_JurNoTxt")).Text;
                                    string Acreage = driver.FindElement(By.Id("MainContent_AcreageTxt")).Text;
                                    string strLegalDescription = driver.FindElement(By.Id("MainContent_LegalAddTxt")).Text;
                                    string FullPropertyAddress = driver.FindElement(By.Id("MainContent_SitusAddTxt")).Text;

                                    IWebElement Apprasialvaluetable = driver.FindElement(By.XPath("//*[@id='MainContent_RollGridView']/tbody"));
                                    IList<IWebElement> Appricelvaluerow = Apprasialvaluetable.FindElements(By.TagName("tr"));
                                    IList<IWebElement> Appricelvalueid;
                                    foreach (IWebElement appricelvalue in Appricelvaluerow)
                                    {
                                        Appricelvalueid = appricelvalue.FindElements(By.TagName("td"));
                                        if (appricelvalue.Text.Contains("Land"))
                                        {
                                            lane = Appricelvalueid[1].Text;
                                        }
                                        if (appricelvalue.Text.Contains("Improvements"))
                                        {
                                            Improvementstax = Appricelvalueid[1].Text;
                                        }
                                        if (appricelvalue.Text.Contains("Homestead Exemption"))
                                        {
                                            HomesteadExemption = Appricelvalueid[1].Text;
                                        }
                                        if (appricelvalue.Text.Contains("Taxable Value"))
                                        {
                                            TaxableValue = Appricelvalueid[1].Text;
                                        }
                                    }
                                    IWebElement taxratetable = driver.FindElement(By.XPath("//*[@id='MainContent_TaxGridView']/tbody"));
                                    IList<IWebElement> Taxratrow = taxratetable.FindElements(By.TagName("tr"));
                                    IList<IWebElement> taxratrowid;
                                    foreach (IWebElement Taxtar in Taxratrow)
                                    {
                                        taxratrowid = Taxtar.FindElements(By.TagName("td"));
                                        if (Taxtar.Text.Contains("Tax Levied"))
                                        {
                                            TaxLevied = taxratrowid[1].Text;
                                        }
                                        if (Taxtar.Text.Contains("Payments Applied To Taxes"))
                                        {
                                            PaymentsApplied = taxratrowid[1].Text;
                                        }
                                        if (Taxtar.Text.Contains("Tax Year"))
                                        {
                                            Tax_Year_Balance = taxratrowid[1].Text;
                                        }
                                    }
                                    Taxrate = gc.Between(taxratetable.Text, "Tax Rate", "Tax Levied").Trim();
                                    string taxmudresult = Jurisdiction1 + "~" + Owner_Name + "~" + FullOwnerAddress + "~" + InquiryDate + "~" + DelinquentDate + "~" + CadNo + "~" + TaxYear + "~" + JurisdictionCode + "~" + Acreage + "~" + strLegalDescription + "~" + FullPropertyAddress + "~" + lane + "~" + Improvementstax + "~" + HomesteadExemption + "~" + TaxableValue + "~" + Taxrate + "~" + TaxLevied + "~" + PaymentsApplied + "~" + Tax_Year_Balance + "~" + Tax_Authority;
                                    gc.insert_date(orderNumber, Account, 2263, taxmudresult, 1, DateTime.Now);
                                    gc.CreatePdf(orderNumber, Account, "Property detail MUD4" + TaxYear, driver, "TX", "Dallas");
                                    IWebElement Currenttaxduetable = driver.FindElement(By.XPath("//*[@id='MainContent_DueGridView']/tbody"));
                                    IList<IWebElement> currenttaxduerow = Currenttaxduetable.FindElements(By.TagName("tr"));
                                    IList<IWebElement> Currenttaxdueid;
                                    foreach (IWebElement currenttaxdue in currenttaxduerow)
                                    {
                                        Currenttaxdueid = currenttaxdue.FindElements(By.TagName("td"));
                                        if (Currenttaxdueid.Count != 0 && !currenttaxdue.Text.Contains("Tax Year"))
                                        {
                                            string CurrentresultMud = Currenttaxdueid[0].Text + "~" + Currenttaxdueid[1].Text + "~" + Currenttaxdueid[2].Text;
                                            gc.insert_date(orderNumber, Account, 2264, CurrentresultMud, 1, DateTime.Now);
                                        }
                                    }
                                    try
                                    {
                                        IWebElement Taxrecipt = driver.FindElement(By.Id("MainContent_TaxReceiptHyperLink"));
                                        string Taxrecipthref = Taxrecipt.GetAttribute("href");
                                        driver.Navigate().GoToUrl(Taxrecipthref);
                                        Thread.Sleep(5000);
                                        gc.CreatePdf(orderNumber, Account, "Tax Recipt" + TaxYear, driver, "TX", "Dallas");
                                        driver.Navigate().Back();
                                        Thread.Sleep(1000);
                                    }
                                    catch { }
                                    //Download
                                    try
                                    {
                                        IWebElement downloadMud = driver.FindElement(By.Id("MainContent_TaxStatementHyperLink"));
                                        string Downloadhref = downloadMud.GetAttribute("href");
                                        string fileName = "Statement.pdf";
                                        var chromeOptions = new ChromeOptions();
                                        var downloadDirectory = ConfigurationManager.AppSettings["AutoPdf"];
                                        chromeOptions.AddUserProfilePreference("download.default_directory", downloadDirectory);
                                        chromeOptions.AddUserProfilePreference("download.prompt_for_download", false);
                                        chromeOptions.AddUserProfilePreference("disable-popup-blocking", "true");
                                        var chDriver1 = new ChromeDriver(chromeOptions);
                                        try
                                        {
                                            chDriver1.Navigate().GoToUrl(Downloadhref);
                                            Thread.Sleep(4000);
                                            chDriver1.FindElement(By.Id("imagebutton2")).Click();
                                            Thread.Sleep(9000);
                                            gc.AutoDownloadFileSpokane(orderNumber, Account, "Dallas", "TX", fileName);
                                        }
                                        catch { }
                                        chDriver1.Quit();
                                    }
                                    catch { }
                                }
                            }
                            catch { }
                        }
                        //Link 5,6,9,10 Thillai
                        //  Jurisdiction = "WYLIE";
                        //    Jurisdiction = "DALLAS COUNTY URD";
                        //  Jurisdiction = "NORTHWEST DALLAS COUNTY FCD";
                        //   Jurisdiction = "VALWOOD IMPROVEMENT AUTHORITY";

                        if (Jurisdiction == "WYLIE" || Jurisdiction == "DALLAS COUNTY URD" || Jurisdiction == "GRAND PRAIRIE METROPOLITAN URD" || Jurisdiction == "NORTHWEST DALLAS COUNTY FCD" || Jurisdiction == "VALWOOD IMPROVEMENT AUTHORITY")
                        {
                            if (Jurisdiction == "WYLIE")
                            {
                                driver.Navigate().GoToUrl("http://taxpublic.collincountytx.gov/webcollincounty/accountsearch.htm");
                            }
                            if (Jurisdiction == "DALLAS COUNTY URD")
                            {
                                driver.Navigate().GoToUrl("http://taxsearch.dcurd.org/webtax/");
                            }
                            if (Jurisdiction == "NORTHWEST DALLAS COUNTY FCD")
                            {
                                driver.Navigate().GoToUrl("https://www.nwdallasfcd.com/accountSearch.asp");
                            }
                            if (Jurisdiction == "VALWOOD IMPROVEMENT AUTHORITY")
                            {
                                driver.Navigate().GoToUrl("https://www.valwood.com/accountSearch.asp");
                            }
                            //  string Accountno = "R013302300701";
                            //  string Accountno = "322595900K0040000";
                            //   string Accountno = "18006270000210000";
                            //    string Accountno = "99L05160400000000";
                            string Accountno = Parcelnumber;
                            try
                            {
                                IWebElement iframe = driver.FindElement(By.XPath("//*[@id='iframe1']"));
                                driver.SwitchTo().Frame(iframe);
                            }
                            catch { }
                            try
                            {
                                driver.FindElement(By.XPath("//*[@id='form1']/input[1]")).SendKeys(Accountno);
                                driver.FindElement(By.Id("submit")).Click();
                                Thread.Sleep(2000);
                            }
                            catch { }
                            try
                            {
                                IWebElement ProDetails = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td/div/table[3]/tbody"));
                                IList<IWebElement> TRProDetails = ProDetails.FindElements(By.TagName("tr"));
                                IList<IWebElement> THProDetails = ProDetails.FindElements(By.TagName("th"));
                                IList<IWebElement> TDProDetails;
                                foreach (IWebElement row1 in TRProDetails)
                                {
                                    TDProDetails = row1.FindElements(By.TagName("td"));
                                    if (TDProDetails.Count != 0 && row1.Text.Trim() != "" && !row1.Text.Contains("Location") && !row1.Text.Contains("Owner") && TDProDetails.Count == 5)
                                    {
                                        IWebElement IClick = TDProDetails[0].FindElement(By.TagName("a"));
                                        IClick.Click();
                                        Thread.Sleep(4000);
                                    }
                                }
                            }
                            catch { }
                            try
                            {
                                IWebElement ProDetails1 = driver.FindElement(By.XPath("/html/body/table[1]/tbody/tr/td[2]/table/tbody/tr[2]/td/div/table[3]/tbody"));
                                IList<IWebElement> TRProDetails1 = ProDetails1.FindElements(By.TagName("tr"));
                                IList<IWebElement> THProDetails1 = ProDetails1.FindElements(By.TagName("th"));
                                IList<IWebElement> TDProDetails1;
                                foreach (IWebElement row1 in TRProDetails1)
                                {
                                    TDProDetails1 = row1.FindElements(By.TagName("td"));
                                    if (TDProDetails1.Count != 0 && row1.Text.Trim() != "" && !row1.Text.Contains("Location") && !row1.Text.Contains("Owner") && TDProDetails1.Count == 5)
                                    {
                                        IWebElement IClick = TDProDetails1[0].FindElement(By.TagName("a"));
                                        IClick.Click();
                                        Thread.Sleep(4000);
                                    }
                                }
                            }
                            catch { }
                            try
                            {
                                IWebElement ProDetails2 = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td[2]/table/tbody/tr[2]/td/div/table[3]/tbody"));
                                IList<IWebElement> TRProDetails2 = ProDetails2.FindElements(By.TagName("tr"));
                                IList<IWebElement> THProDetails2 = ProDetails2.FindElements(By.TagName("th"));
                                IList<IWebElement> TDProDetails2;
                                foreach (IWebElement row1 in TRProDetails2)
                                {
                                    TDProDetails2 = row1.FindElements(By.TagName("td"));
                                    if (TDProDetails2.Count != 0 && row1.Text.Trim() != "" && !row1.Text.Contains("Location") && !row1.Text.Contains("Owner") && TDProDetails2.Count == 5)
                                    {
                                        IWebElement IClick = TDProDetails2[0].FindElement(By.TagName("a"));
                                        IClick.Click();
                                        Thread.Sleep(4000);
                                    }
                                }
                            }
                            catch { }
                            string bulkdata = "";
                            if (bulkdata == "")
                            {
                                try
                                {
                                    bulkdata = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td/table/tbody/tr[2]/td/table[1]/tbody")).Text;
                                }
                                catch { }
                            }
                            if (bulkdata == "")
                            {
                                try
                                {
                                    bulkdata = driver.FindElement(By.XPath("/html/body/table[1]/tbody/tr/td[2]/table/tbody/tr[2]/td/div/table/tbody/tr[2]/td/table[1]/tbody")).Text;
                                }
                                catch { }
                            }
                            if (bulkdata == "")
                            {
                                try
                                {
                                    bulkdata = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td[2]/table/tbody/tr[3]/td/table/tbody/tr[2]/td/table[1]/tbody")).Text;
                                }
                                catch { }
                            }
                            string Account = "", APD = "", Location = "", Legal = "", Owner = "", Acres = "", YearBuilt = "", Sq_Ft = "", DefStart = "", DefEnd = "", Roll = "", UDI = "", Improvement = "", StrLand = "", Exemption1 = "", Exemption2 = "";
                            Account = gc.Between(bulkdata, "Account", "APD").Replace(":", "").Trim();
                            Parcelnumber = Account;
                            APD = gc.Between(bulkdata, "APD", "Location").Replace(":", "").Trim();
                            Location = gc.Between(bulkdata, "Location", "Legal").Replace(":", "").Trim();
                            Legal = gc.Between(bulkdata, "Legal", "Owner").Replace(":", "").Trim();
                            Owner = gc.Between(bulkdata, "Owner", "Acres").Replace(":", "").Trim();
                            Acres = gc.Between(bulkdata, "Acres", "Yr Built").Replace(":", "").Trim();
                            YearBuilt = gc.Between(bulkdata, "Yr Built", "Sq Ft").Replace(":", "").Trim();
                            Sq_Ft = gc.Between(bulkdata, "Sq Ft", "Def. Start").Replace(":", "").Trim();
                            DefStart = gc.Between(bulkdata, "Def. Start", "Def. End").Replace(":", "").Trim();
                            DefEnd = gc.Between(bulkdata, "Def. End", "Roll").Replace(":", "").Trim();
                            Roll = gc.Between(bulkdata, "Roll", "UDI").Replace(":", "").Trim();
                            try
                            {
                                UDI = gc.Between(bulkdata, "UDI", "Improvement").Replace(":", "").Replace("2018 Values", "").Trim();
                            }
                            catch { }
                            if (UDI == "")
                            {
                                try
                                {
                                    UDI = gc.Between(bulkdata, "UDI", "2018 Values").Replace(":", "").Trim();
                                }
                                catch { }
                            }
                            try
                            {
                                Improvement = gc.Between(bulkdata, "Improvement", "Land").Replace(":", "").Trim();
                                StrLand = gc.Between(bulkdata, "Land", "2018 Exemptions").Replace(":", "").Trim();
                            }
                            catch { }
                            try
                            {
                                Exemption1 = gc.Between(bulkdata, "CAP", "HS001").Replace(":", "").Trim();
                                Exemption2 = GlobalClass.After(bulkdata, "HS001").Replace(":", "").Trim();
                            }
                            catch { }
                            try
                            {
                                Exemption2 = GlobalClass.After(bulkdata, "AB001").Replace(":", "").Trim();
                            }
                            catch { }
                            try
                            {
                                Exemption1 = gc.Between(bulkdata, "HS001", "OV003").Replace(":", "").Trim();
                                Exemption2 = GlobalClass.After(bulkdata, "OV003").Replace(":", "").Trim();
                            }
                            catch { }

                            string PropertyDetails = APD + "~" + Location + "~" + Legal + "~" + Owner + "~" + Acres + "~" + YearBuilt + "~" + Sq_Ft + "~" + DefStart + "~" + DefEnd + "~" + Roll + "~" + UDI + "~" + Improvement + "~" + StrLand + "~" + Exemption1 + "~" + Exemption2;
                            gc.insert_date(orderNumber, Parcelnumber, 2265, PropertyDetails, 1, DateTime.Now);

                            try
                            {
                                driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td/table/tbody/tr[2]/td/table[2]/tbody/tr/td[1]/a/img")).Click();
                                Thread.Sleep(4000);
                            }
                            catch { }
                            try
                            {
                                driver.FindElement(By.XPath("/html/body/table[1]/tbody/tr/td[2]/table/tbody/tr[2]/td/div/table/tbody/tr[2]/td/table[2]/tbody/tr/td[1]/a/img")).Click();
                                Thread.Sleep(4000);
                            }
                            catch { }
                            try
                            {
                                driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td[2]/table/tbody/tr[3]/td/table/tbody/tr[2]/td/table[2]/tbody/tr/td[1]/img")).Click();
                                Thread.Sleep(4000);
                            }
                            catch { }

                            try
                            {
                                driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td[2]/table/tbody/tr[3]/td/table/tbody/tr[2]/td/table[2]/tbody/tr/td[1]/a/img")).Click();
                                Thread.Sleep(4000);
                            }
                            catch { }

                            try
                            {
                                IWebElement TaxDetails = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td/table/tbody/tr[2]/td/div[3]/table/tbody"));
                                IList<IWebElement> TRTaxDetails = TaxDetails.FindElements(By.TagName("tr"));
                                IList<IWebElement> THTaxDetails = TaxDetails.FindElements(By.TagName("th"));
                                IList<IWebElement> TDTaxDetails;
                                foreach (IWebElement row in TRTaxDetails)
                                {
                                    TDTaxDetails = row.FindElements(By.TagName("td"));
                                    if (TDTaxDetails.Count != 0 && row.Text.Trim() != "" && !row.Text.Contains("Penalty") && !row.Text.Contains("Amount Paid") && TDTaxDetails.Count == 10)
                                    {
                                        string TaxDetailInfo = TDTaxDetails[0].Text + "~" + TDTaxDetails[1].Text + "~" + TDTaxDetails[2].Text + "~" + TDTaxDetails[3].Text + "~" + TDTaxDetails[4].Text + "~" + TDTaxDetails[5].Text + "~" + TDTaxDetails[6].Text + "~" + TDTaxDetails[7].Text + "~" + TDTaxDetails[8].Text + "~" + TDTaxDetails[9].Text;
                                        gc.insert_date(orderNumber, Parcelnumber, 2266, TaxDetailInfo, 1, DateTime.Now);
                                    }
                                }
                            }
                            catch { }
                            try
                            {
                                IWebElement TaxDetails1 = driver.FindElement(By.XPath("/html/body/table[1]/tbody/tr/td[2]/table/tbody/tr[2]/td/div/table/tbody/tr[2]/td/table[3]/tbody"));
                                IList<IWebElement> TRTaxDetails1 = TaxDetails1.FindElements(By.TagName("tr"));
                                IList<IWebElement> THTaxDetails1 = TaxDetails1.FindElements(By.TagName("th"));
                                IList<IWebElement> TDTaxDetails1;
                                foreach (IWebElement row in TRTaxDetails1)
                                {
                                    TDTaxDetails1 = row.FindElements(By.TagName("td"));
                                    if (TDTaxDetails1.Count != 0 && row.Text.Trim() != "" && !row.Text.Contains("Penalty") && !row.Text.Contains("Amount Paid") && TDTaxDetails1.Count == 10)
                                    {
                                        string TaxDetailInfo = TDTaxDetails1[0].Text + "~" + TDTaxDetails1[1].Text + "~" + TDTaxDetails1[2].Text + "~" + TDTaxDetails1[3].Text + "~" + TDTaxDetails1[4].Text + "~" + TDTaxDetails1[5].Text + "~" + TDTaxDetails1[6].Text + "~" + TDTaxDetails1[7].Text + "~" + TDTaxDetails1[8].Text + "~" + TDTaxDetails1[9].Text;
                                        gc.insert_date(orderNumber, Parcelnumber, 2266, TaxDetailInfo, 1, DateTime.Now);

                                    }
                                }
                            }
                            catch { }
                            try
                            {
                                IWebElement TaxDetails2 = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td[2]/table/tbody/tr[3]/td/table/tbody/tr[2]/td/div[2]/table/tbody"));
                                IList<IWebElement> TRTaxDetails2 = TaxDetails2.FindElements(By.TagName("tr"));
                                IList<IWebElement> THTaxDetails2 = TaxDetails2.FindElements(By.TagName("th"));
                                IList<IWebElement> TDTaxDetails2;
                                foreach (IWebElement row in TRTaxDetails2)
                                {
                                    TDTaxDetails2 = row.FindElements(By.TagName("td"));
                                    if (TDTaxDetails2.Count != 0 && row.Text.Trim() != "" && !row.Text.Contains("Penalty") && !row.Text.Contains("Amount Paid") && TDTaxDetails2.Count == 10)
                                    {
                                        string TaxDetailInfo = TDTaxDetails2[0].Text + "~" + TDTaxDetails2[1].Text + "~" + TDTaxDetails2[2].Text + "~" + TDTaxDetails2[3].Text + "~" + TDTaxDetails2[4].Text + "~" + TDTaxDetails2[5].Text + "~" + TDTaxDetails2[6].Text + "~" + TDTaxDetails2[7].Text + "~" + TDTaxDetails2[8].Text + "~" + TDTaxDetails2[9].Text;
                                        gc.insert_date(orderNumber, Parcelnumber, 2266, TaxDetailInfo, 1, DateTime.Now);
                                    }
                                }
                            }
                            catch { }
                            try
                            {
                                IWebElement TaxDetails3 = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td[2]/table/tbody/tr[3]/td/table/tbody/tr[2]/td/div/table"));
                                IList<IWebElement> TRTaxDetails3 = TaxDetails3.FindElements(By.TagName("tr"));
                                IList<IWebElement> THTaxDetails3 = TaxDetails3.FindElements(By.TagName("th"));
                                IList<IWebElement> TDTaxDetails3;
                                foreach (IWebElement row in TRTaxDetails3)
                                {
                                    TDTaxDetails3 = row.FindElements(By.TagName("td"));
                                    if (TDTaxDetails3.Count != 0 && row.Text.Trim() != "" && !row.Text.Contains("Penalty") && !row.Text.Contains("Amount Paid") && TDTaxDetails3.Count == 10)
                                    {
                                        string TaxDetailInfo = TDTaxDetails3[0].Text + "~" + TDTaxDetails3[1].Text + "~" + TDTaxDetails3[2].Text + "~" + TDTaxDetails3[3].Text + "~" + TDTaxDetails3[4].Text + "~" + TDTaxDetails3[5].Text + "~" + TDTaxDetails3[6].Text + "~" + TDTaxDetails3[7].Text + "~" + TDTaxDetails3[8].Text + "~" + TDTaxDetails3[9].Text;
                                        gc.insert_date(orderNumber, Parcelnumber, 2266, TaxDetailInfo, 1, DateTime.Now);

                                    }
                                }
                            }
                            catch { }
                        }
                    }
                    HttpContext.Current.Session["Link0"] = Link0;
                    HttpContext.Current.Session["Link1"] = Link1;
                    HttpContext.Current.Session["Link2"] = Link2;
                    HttpContext.Current.Session["Link4"] = Link4;

                    TaxTime = DateTime.Now.ToString("HH:mm:ss");
                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "TX", "Dallas", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);
                    driver.Quit();
                    gc.mergpdf(orderNumber, "TX", "Dallas");
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