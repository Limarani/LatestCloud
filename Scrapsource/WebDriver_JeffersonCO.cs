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
    public class WebDriver_JeffersonCO
    {

        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();

        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());

        public string FTP_JeffersonCO(string houseno, string Direction, string sname, string stype, string account, string parcelNumber, string ownername, string searchType, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;

            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
            string address = "", lastName="", firstName="", Pinnumber="", PropertyAdd="", Strownername="", Pin="";

            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            using (driver = new PhantomJSDriver())
           // using (driver = new ChromeDriver())
            {


                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");


                    if (searchType == "titleflex")
                    {
                        if (Direction != "")
                        {
                            address = houseno + " " + Direction + " " + sname + " " + stype + " " + account;
                            address = address.Trim();
                        }
                        if (Direction == "")
                        {
                            address = houseno + " " + sname + " " + stype + " " + account;
                            address = address.Trim();
                        }
                        string titleaddress = address;
                        gc.TitleFlexSearch(orderNumber, "", "", titleaddress, "CO", "Jefferson");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_JeffersonCO"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }

                    if (searchType == "address")
                    {
                        try
                        {
                            driver.Navigate().GoToUrl("https://propertysearch.jeffco.us/propertyrecordssearch/address");
                            Thread.Sleep(4000);
                        }
                        catch { }

                        driver.FindElement(By.Id("addressNumber")).SendKeys(houseno);
                        driver.FindElement(By.Id("predirectionalType")).SendKeys(Direction);
                        driver.FindElement(By.Id("streetName")).SendKeys(sname);
                        driver.FindElement(By.Id("streetType")).SendKeys(stype);
                        driver.FindElement(By.Id("unitNumber")).SendKeys(account);
                        Thread.Sleep(3000);
                        gc.CreatePdf_WOP(orderNumber, "Address search", driver, "CO", "Jefferson");

                        try
                        {
                            int Max = 0;
                            string strowner = "", strAddress = "", strCity = "";
                            string Record = "";
                            IWebElement Irecord = driver.FindElement(By.XPath("//*[@id='propertyRecordsSearchMiniSpaModule']/div/div[2]/div/div[2]/div/div/div[2]/div/div[2]/div/span"));
                            Record = Irecord.Text;
                            Record = gc.Between(Record, "to", "of").Trim();
                            if (Record != "1")
                            {
                                gc.CreatePdf_WOP(orderNumber, "Address search Result", driver, "CO", "Jefferson");
                                IWebElement multiaddress = driver.FindElement(By.XPath("//*[@id='propertyRecordsSearchMiniSpaModule']/div/div[2]/div/div[2]/div/div/div[2]/div/div[3]/div[1]/table/tbody"));
                                IList<IWebElement> multiRow = multiaddress.FindElements(By.TagName("tr"));
                                IList<IWebElement> multiTD;
                                foreach (IWebElement multi in multiRow)
                                {
                                    multiTD = multi.FindElements(By.TagName("td"));
                                    if (multiTD.Count != 0 && multiRow.Count >= 2 && multiRow.Count <= 25 && multi.Text.Trim() != "")
                                    {
                                        Strownername = multiTD[6].Text;
                                        Pinnumber = multiTD[3].Text;
                                        parcelNumber = multiTD[4].Text;
                                        PropertyAdd = multiTD[0].Text + " " + multiTD[1].Text;

                                        string multidetails = Strownername + "~" + Pinnumber + "~" + PropertyAdd;
                                        gc.insert_date(orderNumber, parcelNumber, 1607, multidetails, 1, DateTime.Now);
                                        Max++;
                                    }

                                    if (multiTD.Count != 0 && multiRow.Count > 25)
                                    {
                                        HttpContext.Current.Session["multiparcel_Richland_Maximum"] = "Maximum";
                                        driver.Quit();
                                        return "Maximum";
                                    }

                                }
                                if (Max > 1 && Max < 26)
                                {
                                    HttpContext.Current.Session["multiparcel_Jefferson"] = "Yes";
                                    driver.Quit();
                                    return "MultiParcel";
                                }
                                if (Max == 0)
                                {
                                    HttpContext.Current.Session["Nodata_JeffersonCO"] = "Yes";
                                    driver.Quit();
                                    return "No Data Found";
                                }
                            }
                        }
                        catch { }
                    }

                    if (searchType == "account")
                    {
                        try
                        {
                            driver.Navigate().GoToUrl("https://propertysearch.jeffco.us/propertyrecordssearch/pin");
                            Thread.Sleep(4000);
                        }
                        catch { }


                        driver.FindElement(By.Id("pin")).SendKeys(account);
                        Thread.Sleep(3000);
                        gc.CreatePdf_WOP(orderNumber, "Pin or Schedule search", driver, "CO", "Jefferson");

                        try
                        {
                            int Max = 0;
                            string strowner = "", strAddress = "", strCity = "";
                            string Record = "";
                            IWebElement Irecord = driver.FindElement(By.XPath("//*[@id='propertyRecordsSearchMiniSpaModule']/div/div[2]/div/div[2]/div/div/div[2]/div/div[2]/div/span"));
                            Record = Irecord.Text;
                            Record = gc.Between(Record, "to", "of").Trim();
                            if (Record != "1")
                            {
                                gc.CreatePdf_WOP(orderNumber, "Pin or Schedule search Result", driver, "CO", "Jefferson");
                                IWebElement multiaddress = driver.FindElement(By.XPath("//*[@id='propertyRecordsSearchMiniSpaModule']/div/div[2]/div/div[2]/div/div/div[2]/div/div[3]/div[1]/table/tbody"));
                                IList<IWebElement> multiRow = multiaddress.FindElements(By.TagName("tr"));
                                IList<IWebElement> multiTD;
                                foreach (IWebElement multi in multiRow)
                                {
                                    multiTD = multi.FindElements(By.TagName("td"));
                                    if (multiTD.Count != 0 && multiRow.Count >= 2 && multiRow.Count <= 25 && multi.Text.Trim() != "")
                                    {
                                        Strownername = multiTD[0].Text;
                                        Pinnumber = multiTD[1].Text;
                                        parcelNumber = multiTD[2].Text;
                                        PropertyAdd = multiTD[4].Text + " " + multiTD[5].Text;

                                        string multidetails = Strownername + "~" + Pinnumber + "~" + PropertyAdd;
                                        gc.insert_date(orderNumber, parcelNumber, 1607, multidetails, 1, DateTime.Now);
                                        Max++;
                                    }
                                    if (multiTD.Count != 0 && multiRow.Count > 25)
                                    {
                                        HttpContext.Current.Session["multiparcel_Richland_Maximum"] = "Maximum";
                                        driver.Quit();
                                        return "Maximum";
                                    }

                                }
                                if (Max > 1 && Max < 26)
                                {
                                    HttpContext.Current.Session["multiparcel_Jefferson"] = "Yes";
                                    driver.Quit();
                                    return "MultiParcel";
                                }
                                if (Max == 0)
                                {
                                    HttpContext.Current.Session["Nodata_JeffersonCO"] = "Yes";
                                    driver.Quit();
                                    return "No Data Found";
                                }
                            }
                        }
                        catch { }
                    }


                    else if (searchType == "parcel")
                    {
                        try
                        {
                            driver.Navigate().GoToUrl("https://propertysearch.jeffco.us/propertyrecordssearch/ain");
                            Thread.Sleep(4000);
                        }
                        catch { }


                        driver.FindElement(By.Id("ain")).SendKeys(parcelNumber);
                        Thread.Sleep(3000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel search", driver, "CO", "Jefferson");


                        try
                        {
                            int Max = 0;
                            string strowner = "", strAddress = "", strCity = "";
                            string Record = "";
                            IWebElement Irecord = driver.FindElement(By.XPath("//*[@id='propertyRecordsSearchMiniSpaModule']/div/div[2]/div/div[2]/div/div/div[2]/div/div[2]/div/span"));
                            Record = Irecord.Text;
                            Record = gc.Between(Record, "to", "of").Trim();
                            if (Record != "1")
                            {
                                gc.CreatePdf(orderNumber, parcelNumber, "Parcel search Result", driver, "CO", "Jefferson");
                                IWebElement multiaddress = driver.FindElement(By.XPath("//*[@id='propertyRecordsSearchMiniSpaModule']/div/div[2]/div/div[2]/div/div/div[2]/div/div[3]/div[1]/table/tbody"));
                                IList<IWebElement> multiRow = multiaddress.FindElements(By.TagName("tr"));
                                IList<IWebElement> multiTD;
                                foreach (IWebElement multi in multiRow)
                                {
                                    multiTD = multi.FindElements(By.TagName("td"));
                                    if (multiTD.Count != 0 && multiRow.Count >= 2 && multiRow.Count <= 25 && multi.Text.Trim() != "")
                                    {
                                        Strownername = multiTD[0].Text;
                                        Pinnumber = multiTD[1].Text;
                                        parcelNumber = multiTD[2].Text;
                                        PropertyAdd = multiTD[4].Text + " " + multiTD[5].Text;

                                        string multidetails = Strownername + "~" + Pinnumber + "~" + PropertyAdd;
                                        gc.insert_date(orderNumber, parcelNumber, 1607, multidetails, 1, DateTime.Now);
                                        Max++;
                                    }
                                    if (multiTD.Count != 0 && multiRow.Count > 25)
                                    {
                                        HttpContext.Current.Session["multiparcel_Richland_Maximum"] = "Maximum";
                                        driver.Quit();
                                        return "Maximum";
                                    }

                                }
                                if (Max > 1 && Max < 26)
                                {
                                    HttpContext.Current.Session["multiparcel_Jefferson"] = "Yes";
                                    driver.Quit();
                                    return "MultiParcel";
                                }
                                if (Max == 0)
                                {
                                    HttpContext.Current.Session["Nodata_JeffersonCO"] = "Yes";
                                    driver.Quit();
                                    return "No Data Found";
                                }
                            }
                        }
                        catch { }
                    }

                    if (searchType == "ownername")
                    {
                        try
                        {
                            driver.Navigate().GoToUrl("https://propertysearch.jeffco.us/propertyrecordssearch/owner");
                            Thread.Sleep(4000);
                        }
                        catch { }

                        if (ownername.Contains(" "))
                        {
                            string[] owner = ownername.Split(' ');
                            lastName = owner[0];
                            firstName = owner[1];
                        }
                        else
                        {
                            string[] owner = ownername.Split(',');
                            lastName = owner[0];
                            firstName = owner[1];
                        }
                        driver.FindElement(By.Id("lastName")).SendKeys(lastName);
                        driver.FindElement(By.Id("firstName")).SendKeys(firstName);
                        Thread.Sleep(3000);

                        gc.CreatePdf_WOP(orderNumber, "OwnerName search", driver, "CO", "Jefferson");


                        try
                        {
                            int Max = 0;
                            string strowner = "", strAddress = "", strCity = "", strbulkdata = "";
                            string Record = "";
                            IWebElement Irecord = driver.FindElement(By.XPath("//*[@id='propertyRecordsSearchMiniSpaModule']/div/div[2]/div/div[2]/div/div/div[2]/div/div[2]/div/span"));
                            Record = Irecord.Text;
                            Record = gc.Between(Record, "to", "of").Trim();
                            if (Record != "1")
                            {

                                IWebElement multiaddress = driver.FindElement(By.XPath("//*[@id='propertyRecordsSearchMiniSpaModule']/div/div[2]/div/div[2]/div/div/div[2]/div/div[3]/div[1]/table/tbody"));
                                IList<IWebElement> multiRow = multiaddress.FindElements(By.TagName("tr"));
                                IList<IWebElement> multiTD;
                                foreach (IWebElement multi in multiRow)
                                {
                                    multiTD = multi.FindElements(By.TagName("td"));
                                    if (multiTD.Count != 0 && multiRow.Count >= 2 && multiRow.Count <= 25 && multi.Text.Trim() != "")
                                    {
                                        Strownername = multiTD[0].Text;
                                        Pinnumber = multiTD[1].Text;
                                        parcelNumber = multiTD[2].Text;
                                        PropertyAdd = multiTD[4].Text + " " + multiTD[5].Text;

                                        string multidetails = Strownername + "~" + Pinnumber + "~" + PropertyAdd;
                                        gc.insert_date(orderNumber, parcelNumber, 1607, multidetails, 1, DateTime.Now);
                                        Max++;
                                    }
                                    if (multiTD.Count != 0 && multiRow.Count > 25)
                                    {
                                        HttpContext.Current.Session["multiparcel_Richland_Maximum"] = "Maximum";
                                        driver.Quit();
                                        return "Maximum";
                                    }

                                }
                                if (Max > 1 && Max < 26)
                                {
                                    HttpContext.Current.Session["multiparcel_Jefferson"] = "Yes";
                                    driver.Quit();
                                    return "MultiParcel";
                                }
                                if (Max == 0)
                                {
                                    HttpContext.Current.Session["Nodata_JeffersonCO"] = "Yes";
                                    driver.Quit();
                                    return "No Data Found";
                                }
                            }
                        }
                        catch { }
                    }


                    //property details
                    string Bulkdata = "";
                    try
                    {
                        IWebElement IBulkdata = driver.FindElement(By.XPath("//*[@id='propertyRecordsSearchMiniSpaModule']/div/div[2]/div/div[2]/div/div/div[2]/label"));
                        Bulkdata = IBulkdata.Text;
                        if (Bulkdata.Contains("No records found"))
                        {
                            HttpContext.Current.Session["Zero_Jefferson"] = "Zero";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }
                    try
                    {
                        driver.FindElement(By.XPath("//*[@id='propertyRecordsSearchMiniSpaModule']/div/div[2]/div/div[2]/div/div/div[2]/div/div[3]/div[1]/table/tbody/tr/td[1]/span")).Click();
                        Thread.Sleep(5000);
                    }
                    catch { }
                    string PropertyClass = "", MailAdd1 = "", MailAdd2 = "", MailingAddress = "", Subdivision = "", Block = "", Lot = "", Track = "", Section = "", Township = "";
                    string OwnerName = "", Proadd1 = "", Proadd2 = "", PropertyAddress = "", Range = "", Qsection = "", Acres = "", Neighbourhood = "", YearBuilt = "";

                    IWebElement Ipin = driver.FindElement(By.XPath("//*[@id='propertyRecordsSearchMiniSpaModule']/div/div[2]/div/div[3]/div[2]/div/div/div/div[1]/dl/dd[1]"));
                    Pin = Ipin.Text;

                    IWebElement parcelNo = driver.FindElement(By.XPath("//*[@id='propertyRecordsSearchMiniSpaModule']/div/div[2]/div/div[3]/div[2]/div/div/div/div[2]/dl/dd[1]"));
                    parcelNumber = parcelNo.Text;


                    IWebElement IPropertyClass = driver.FindElement(By.XPath("//*[@id='propertyRecordsSearchMiniSpaModule']/div/div[2]/div/div[3]/div[2]/div/div/div/div[3]/dl/dd[1]"));
                    PropertyClass = IPropertyClass.Text;
                    IWebElement StrOwnerName = driver.FindElement(By.XPath("//*[@id='propertyRecordsSearchMiniSpaModule']/div/div[2]/div/div[3]/div[2]/div/div/div/div[1]/dl/dd[2]/span[1]"));
                    OwnerName = StrOwnerName.Text;

                    IWebElement IProadd1 = driver.FindElement(By.XPath("//*[@id='propertyRecordsSearchMiniSpaModule']/div/div[2]/div/div[3]/div[2]/div/div/div/div[2]/dl/dd[2]"));
                    Proadd1 = IProadd1.Text;

                    IWebElement IProadd2 = driver.FindElement(By.XPath("//*[@id='propertyRecordsSearchMiniSpaModule']/div/div[2]/div/div[3]/div[2]/div/div/div/div[2]/dl/dd[3]"));
                    Proadd2 = IProadd2.Text;
                    PropertyAddress = Proadd1 + " " + Proadd2;

                    IWebElement IMailAdd1 = driver.FindElement(By.XPath("//*[@id='propertyRecordsSearchMiniSpaModule']/div/div[2]/div/div[3]/div[2]/div/div/div/div[3]/dl/dd[3]"));
                    MailAdd1 = IMailAdd1.Text;
                    IWebElement IMailAdd2 = driver.FindElement(By.XPath("//*[@id='propertyRecordsSearchMiniSpaModule']/div/div[2]/div/div[3]/div[2]/div/div/div/div[3]/dl/dd[4]"));
                    MailAdd2 = IMailAdd2.Text;
                    MailingAddress = MailAdd1 + " " + MailAdd2;

                    IWebElement ISubdivision = driver.FindElement(By.XPath("//*[@id='propertyRecordsSearchMiniSpaModule']/div/div[2]/div/div[4]/div/div/div[2]/div[1]/div/div/div[1]/dl/dd[1]/span"));
                    Subdivision = ISubdivision.Text;
                    IWebElement IBlock = driver.FindElement(By.XPath("//*[@id='propertyRecordsSearchMiniSpaModule']/div/div[2]/div/div[4]/div/div/div[2]/div[2]/div/table/tbody/tr/td[1]/span"));
                    Block = IBlock.Text;
                    IWebElement ILot = driver.FindElement(By.XPath("//*[@id='propertyRecordsSearchMiniSpaModule']/div/div[2]/div/div[4]/div/div/div[2]/div[2]/div/table/tbody/tr/td[2]/span"));
                    Lot = ILot.Text;
                    Track = driver.FindElement(By.XPath("//*[@id='propertyRecordsSearchMiniSpaModule']/div/div[2]/div/div[4]/div/div/div[2]/div[2]/div/table/tbody/tr/td[3]")).Text;
                    Section = driver.FindElement(By.XPath("//*[@id='propertyRecordsSearchMiniSpaModule']/div/div[2]/div/div[4]/div/div/div[2]/div[2]/div/table/tbody/tr/td[4]")).Text;
                    Township = driver.FindElement(By.XPath("//*[@id='propertyRecordsSearchMiniSpaModule']/div/div[2]/div/div[4]/div/div/div[2]/div[2]/div/table/tbody/tr/td[5]")).Text;
                    Range = driver.FindElement(By.XPath("//*[@id='propertyRecordsSearchMiniSpaModule']/div/div[2]/div/div[4]/div/div/div[2]/div[2]/div/table/tbody/tr/td[6]")).Text;
                    Qsection = driver.FindElement(By.XPath("//*[@id='propertyRecordsSearchMiniSpaModule']/div/div[2]/div/div[4]/div/div/div[2]/div[2]/div/table/tbody/tr/td[7]")).Text;
                    Acres = driver.FindElement(By.XPath("//*[@id='propertyRecordsSearchMiniSpaModule']/div/div[2]/div/div[4]/div/div/div[2]/div[2]/div/table/tbody/tr/td[9]/span")).Text;
                    Neighbourhood = driver.FindElement(By.XPath("//*[@id='propertyRecordsSearchMiniSpaModule']/div/div[2]/div/div[8]/div/div/div[2]/div/div/div/div[1]/dl/dd")).Text;
                    try
                    {
                        YearBuilt = driver.FindElement(By.XPath("//*[@id='propertyRecordsSearchMiniSpaModule']/div/div[2]/div/div[10]/div/div/div[2]/div[1]/div/table/tbody/tr/td[7]/span")).Text;
                    }
                    catch { }
                    string propertydetails = Pin + "~" + PropertyClass + "~" + OwnerName + "~" + PropertyAddress + "~" + MailingAddress + "~" + Subdivision + "~" + Block + "~" + Lot + "~" + Track + "~" + Section + "~" + Township + "~" + Range + "~" + Qsection + "~" + Acres + "~" + Neighbourhood + "~" + YearBuilt;
                    gc.insert_date(orderNumber, parcelNumber, 1604, propertydetails, 1, DateTime.Now);

                    gc.CreatePdf(orderNumber, parcelNumber, "Property Details", driver, "CO", "Jefferson");

                    // Assessment Details



                    try
                    {
                        IWebElement Assessment = driver.FindElement(By.XPath("//*[@id='propertyRecordsSearchMiniSpaModule']/div/div[2]/div/div[6]/div/div/div[2]/div[3]/div/table"));
                        IList<IWebElement> TRAssessment = Assessment.FindElements(By.TagName("tr"));
                        IList<IWebElement> THAssessment = Assessment.FindElements(By.TagName("th"));
                        IList<IWebElement> TDAssessment;
                        foreach (IWebElement row in TRAssessment)
                        {
                            TDAssessment = row.FindElements(By.TagName("td"));
                            if (TDAssessment.Count != 0 && !row.Text.Contains("Assessed Total Value"))
                            {
                                string Assessmentdetails = TDAssessment[0].Text + "~" + TDAssessment[1].Text + "~" + TDAssessment[2].Text + "~" + TDAssessment[3].Text + "~" + TDAssessment[4].Text + "~" + TDAssessment[5].Text + "~" + TDAssessment[6].Text;
                                gc.insert_date(orderNumber, parcelNumber, 1605, Assessmentdetails, 1, DateTime.Now);
                            }
                        }
                    }
                    catch { }

                    // Entity Detail Table 1
                    int i = 0;
                    try
                    {
                        IWebElement Entity = driver.FindElement(By.XPath("//*[@id='propertyRecordsSearchMiniSpaModule']/div/div[2]/div/div[7]/div/div/div[2]/div[3]/div/table"));
                        IList<IWebElement> TREntity = Entity.FindElements(By.TagName("tr"));
                        IList<IWebElement> TDEntity;

                        foreach (IWebElement row in TREntity)
                        {
                            TDEntity = row.FindElements(By.TagName("td"));

                            if (TDEntity.Count != 0 && !row.Text.Contains("Authority") && TDEntity.Count >= 11)
                            {
                                if (i == 0)
                                {
                                    string[] Auth1 = TDEntity[3].Text.Split('\n');
                                    List<string[]> Auth = new List<string[]>();
                                    Auth.Add(Auth1);

                                    int s = Auth1.Length;
                                    string[] MillLevy1 = new string[0];
                                    List<string[]> MillLevy = new List<string[]>();
                                    int r = 0;

                                    MillLevy1 = TDEntity[15].Text.Split('\n');
                                    MillLevy.Add(MillLevy1);
                                    r = MillLevy1.Length;

                                    if (r <= 2)
                                    {
                                        MillLevy1 = TDEntity[16].Text.Split('\n');
                                        MillLevy.Add(MillLevy1);
                                        r = MillLevy1.Length;
                                    }
                                    if (r <= 2)
                                    {
                                        MillLevy1 = TDEntity[13].Text.Split('\n');
                                        MillLevy.Add(MillLevy1);
                                        r = MillLevy1.Length;
                                    }
                                    if (r <= 2)
                                    {
                                        MillLevy1 = TDEntity[14].Text.Split('\n');
                                        MillLevy.Add(MillLevy1);
                                        r = MillLevy1.Length;
                                    }
                                    if (r <= 2)
                                    {
                                        MillLevy1 = TDEntity[17].Text.Split('\n');
                                        MillLevy.Add(MillLevy1);
                                        r = MillLevy1.Length;
                                    }
                                    for (int j = 0; j < s; j++)
                                    {
                                        if (j == 0)
                                        {
                                            string Entitydetails = TDEntity[0].Text + "~" + TDEntity[2].Text + "~" + Auth1[j] + "~" + MillLevy1[j];
                                            gc.insert_date(orderNumber, parcelNumber, 1606, Entitydetails, 1, DateTime.Now);
                                        }
                                        else
                                        {
                                            try
                                            {
                                                if (!Auth1[j].Contains("Total Mill Levy"))
                                                {
                                                    string Entitydetails1 = "" + "~" + "" + "~" + Auth1[j] + "~" + MillLevy1[j];
                                                    gc.insert_date(orderNumber, parcelNumber, 1606, Entitydetails1, 1, DateTime.Now);
                                                }

                                            }
                                            catch { }
                                        }



                                    }

                                    i++;
                                }
                            }
                        }
                    }
                    catch { }
                    string strmilllevy1 = "", strmilllevyval1 = "", strmilllevy2 = "", strmilllevyval2 = "";
                    // Tfooter 1
                    try
                    {
                        IWebElement Entity = driver.FindElement(By.XPath("//*[@id='propertyRecordsSearchMiniSpaModule']/div/div[2]/div/div[7]/div/div/div[2]/div[3]/div/table/tbody/tr[1]/td[3]/table/tfoot"));
                        IList<IWebElement> TREntity = Entity.FindElements(By.TagName("tr"));
                        IList<IWebElement> TDEntity;
                        foreach (IWebElement row in TREntity)
                        {
                            TDEntity = row.FindElements(By.TagName("td"));

                            if (TDEntity.Count != 0)
                            {
                                strmilllevy1 = TDEntity[0].Text;
                            }
                        }
                    }
                    catch { }

                    // Tfooter 2
                    try
                    {
                        IWebElement Entity = driver.FindElement(By.XPath(" //*[@id='propertyRecordsSearchMiniSpaModule']/div/div[2]/div/div[7]/div/div/div[2]/div[3]/div/table/tbody/tr[1]/td[4]/table/tfoot"));
                        IList<IWebElement> TREntity = Entity.FindElements(By.TagName("tr"));
                        IList<IWebElement> TDEntity;
                        foreach (IWebElement row in TREntity)
                        {
                            TDEntity = row.FindElements(By.TagName("td"));

                            if (TDEntity.Count != 0)
                            {
                                strmilllevyval1 = TDEntity[0].Text;
                            }
                        }
                    }
                    catch { }
                    try
                    {

                        string Entitydetails1 = "" + "~" + "" + "~" + strmilllevy1 + "~" + strmilllevyval1;
                        gc.insert_date(orderNumber, parcelNumber, 1606, Entitydetails1, 1, DateTime.Now);

                    }
                    catch { }

                    // Entity Detail Table 2
                    int k = 0;
                    try
                    {
                        IWebElement Entity = driver.FindElement(By.XPath("//*[@id='propertyRecordsSearchMiniSpaModule']/div/div[2]/div/div[7]/div/div/div[2]/div[3]/div/table"));
                        IList<IWebElement> TREntity = Entity.FindElements(By.TagName("tr"));
                        IList<IWebElement> TDEntity;
                        foreach (IWebElement row in TREntity)
                        {
                            TDEntity = row.FindElements(By.TagName("td"));

                            if (TDEntity.Count != 0 && !row.Text.Contains("Authority") && TDEntity.Count > 11)
                            {
                                if (k == 0 || k == 1)
                                {
                                    string[] Auth1 = TDEntity[3].Text.Split('\n');
                                    List<string[]> Auth = new List<string[]>();
                                    Auth.Add(Auth1);
                                    int s = Auth1.Length;

                                    string[] MillLevy1 = new string[0];
                                    List<string[]> MillLevy = new List<string[]>();
                                    int r = 0;
                                    try
                                    {
                                        MillLevy1 = TDEntity[15].Text.Split('\n');
                                        MillLevy.Add(MillLevy1);
                                        r = MillLevy1.Length;
                                    }
                                    catch { }

                                    if (r <= 2)
                                    {
                                        MillLevy1 = TDEntity[16].Text.Split('\n');
                                        MillLevy.Add(MillLevy1);
                                        r = MillLevy1.Length;
                                    }
                                    if (r <= 2)
                                    {
                                        MillLevy1 = TDEntity[13].Text.Split('\n');
                                        MillLevy.Add(MillLevy1);
                                        r = MillLevy1.Length;
                                    }
                                    if (r <= 2)
                                    {
                                        MillLevy1 = TDEntity[14].Text.Split('\n');
                                        MillLevy.Add(MillLevy1);
                                        r = MillLevy1.Length;
                                    }
                                    if (r <= 2)
                                    {
                                        MillLevy1 = TDEntity[17].Text.Split('\n');
                                        MillLevy.Add(MillLevy1);
                                        r = MillLevy1.Length;
                                    }
                                    if (k == 1)
                                    {
                                        for (int j = 0; j < s; j++)
                                        {
                                            if (j == 0)
                                            {
                                                string Entitydetails = TDEntity[0].Text + "~" + TDEntity[2].Text + "~" + Auth1[j] + "~" + MillLevy1[j];
                                                gc.insert_date(orderNumber, parcelNumber, 1606, Entitydetails, 1, DateTime.Now);
                                            }
                                            else
                                            {
                                                try
                                                {
                                                    if (!Auth1[j].Contains("Total Mill Levy"))
                                                    {
                                                        string Entitydetails1 = "" + "~" + "" + "~" + Auth1[j] + "~" + MillLevy1[j];
                                                        gc.insert_date(orderNumber, parcelNumber, 1606, Entitydetails1, 1, DateTime.Now);
                                                    }
                                                }
                                                catch { }
                                            }



                                        }
                                    }
                                    k++;
                                }
                            }
                        }
                    }
                    catch { }

                    // Tfooter 2.1
                    try
                    {
                        IWebElement Entity = driver.FindElement(By.XPath("//*[@id='propertyRecordsSearchMiniSpaModule']/div/div[2]/div/div[7]/div/div/div[2]/div[3]/div/table/tbody/tr[2]/td[3]/table/tfoot"));
                        IList<IWebElement> TREntity = Entity.FindElements(By.TagName("tr"));
                        IList<IWebElement> TDEntity;
                        foreach (IWebElement row in TREntity)
                        {
                            TDEntity = row.FindElements(By.TagName("td"));

                            if (TDEntity.Count != 0)
                            {
                                strmilllevy2 = TDEntity[0].Text;
                            }
                        }
                    }
                    catch { }

                    // Tfooter 2.2
                    try
                    {
                        IWebElement Entity = driver.FindElement(By.XPath("//*[@id='propertyRecordsSearchMiniSpaModule']/div/div[2]/div/div[7]/div/div/div[2]/div[3]/div/table/tbody/tr[2]/td[4]/table/tfoot"));
                        IList<IWebElement> TREntity = Entity.FindElements(By.TagName("tr"));
                        IList<IWebElement> TDEntity;
                        foreach (IWebElement row in TREntity)
                        {
                            TDEntity = row.FindElements(By.TagName("td"));

                            if (TDEntity.Count != 0)
                            {
                                strmilllevyval2 = TDEntity[0].Text;
                            }
                        }
                    }
                    catch { }
                    try
                    {

                        string Entitydetails2 = "" + "~" + "" + "~" + strmilllevy2 + "~" + strmilllevyval2;
                        gc.insert_date(orderNumber, parcelNumber, 1606, Entitydetails2, 1, DateTime.Now);

                    }
                    catch { }

                    // Tax Information Details
                    string taxAuth = "", taxauth1 = "", taxauth2 = "";
                    driver.Navigate().GoToUrl("https://ttps.jeffco.us/ASPX/ScheduleSearch");
                    Thread.Sleep(5000);
                    try
                    {
                        taxauth1 = driver.FindElement(By.XPath("//*[@id='sidebar-wrapper']/ul[2]")).Text.Replace("\r\n"," ");
                        taxAuth = gc.Between(taxauth1, "Contact Us", "Release").Trim();

                        gc.CreatePdf(orderNumber, parcelNumber, "Tax Authority", driver, "CO", "Jefferson");
                    }
                    catch { }

                    try
                    {
                        driver.FindElement(By.LinkText("Search by AIN/Parcel ID")).Click();
                        Thread.Sleep(4000);
                    }
                    catch { }
                    driver.FindElement(By.Id("MainContent_txtAINToSearch")).SendKeys(parcelNumber);
                    gc.CreatePdf(orderNumber, parcelNumber, "Tax Search", driver, "CO", "Jefferson");
                    driver.FindElement(By.Id("MainContent_btnAINSubmit")).Click();
                    Thread.Sleep(4000);
                    gc.CreatePdf(orderNumber, parcelNumber, "Tax Search Result", driver, "CO", "Jefferson");
                    try
                    {
                        driver.FindElement(By.XPath("//*[@id='MainContent_gvAINList']/tbody/tr[2]/td[1]/a")).Click();
                        Thread.Sleep(4000);
                    }
                    catch { }

                    string strpin = "", strownername = "", strProAdd = "", strTaxDistrict = "", strMillLevy = "";

                    strpin = driver.FindElement(By.Id("MainContent_ctl00_lblSchCurPayTax")).Text;
                    strownername = driver.FindElement(By.Id("MainContent_ctl00_tbxOwnerCurPayTax")).Text;
                    strProAdd = driver.FindElement(By.Id("MainContent_ctl00_lblAddressCurPayTax")).Text;
                    strTaxDistrict = driver.FindElement(By.Id("MainContent_lblTaxDistrictCurPay")).Text;
                    strMillLevy = driver.FindElement(By.Id("MainContent_lblTaxMillLevy")).Text;

                    gc.CreatePdf(orderNumber, parcelNumber, "Tax Information", driver, "CO", "Jefferson");
                    // Tax Due Details

                    try
                    {
                        IWebElement TaxDue = driver.FindElement(By.XPath("//*[@id='MainContent_gvCurPayTax_PayableInfo']/tbody"));
                        IList<IWebElement> TRTaxDue = TaxDue.FindElements(By.TagName("tr"));
                        IList<IWebElement> THTaxDue = TaxDue.FindElements(By.TagName("th"));
                        IList<IWebElement> TDTaxDue;
                        foreach (IWebElement row in TRTaxDue)
                        {
                            TDTaxDue = row.FindElements(By.TagName("td"));
                            if (TDTaxDue.Count != 0 && !row.Text.Contains("Half Amount"))
                            {
                                string TaxDuedetails = TDTaxDue[0].Text + "~" + TDTaxDue[1].Text + "~" + TDTaxDue[2].Text;
                                gc.insert_date(orderNumber, parcelNumber, 1609, TaxDuedetails, 1, DateTime.Now);
                            }
                        }
                    }
                    catch { }

                    // Tax Receipt Details
                    try
                    {
                        IWebElement TaxReceipt = driver.FindElement(By.XPath("//*[@id='MainContent_gvCurPayTax_PmtInfo']/tbody"));
                        IList<IWebElement> TRTaxReceipt = TaxReceipt.FindElements(By.TagName("tr"));
                        IList<IWebElement> THTaxReceipt = TaxReceipt.FindElements(By.TagName("th"));
                        IList<IWebElement> TDTaxReceipt;
                        foreach (IWebElement row in TRTaxReceipt)
                        {
                            TDTaxReceipt = row.FindElements(By.TagName("td"));
                            if (TDTaxReceipt.Count != 0 && !row.Text.Contains("Payment Type"))
                            {
                                string TaxReceiptdetails = TDTaxReceipt[0].Text + "~" + TDTaxReceipt[1].Text + "~" + TDTaxReceipt[2].Text + "~" + TDTaxReceipt[3].Text;
                                gc.insert_date(orderNumber, parcelNumber, 1610, TaxReceiptdetails, 1, DateTime.Now);
                            }
                        }
                    }
                    catch { }

                    try
                    {
                        driver.FindElement(By.LinkText("Payment History")).Click();
                        Thread.Sleep(5000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Tax Payment Details", driver, "CO", "Jefferson");
                    }
                    catch { }
                    string Comments = "";
                    try
                    {
                        Comments = driver.FindElement(By.Id("MainContent_lblRedemptionExists")).Text;
                    }
                    catch { }


                    try
                    {
                        string TaxInfodetails = strpin + "~" + strownername + "~" + strProAdd + "~" + strTaxDistrict + "~" + strMillLevy + "~" + taxAuth + "~" + Comments;
                        gc.insert_date(orderNumber, parcelNumber, 1608, TaxInfodetails, 1, DateTime.Now);

                    }
                    catch { }
                    // Tax Payment History Details
                    try
                    {
                        IWebElement TaxPayment = driver.FindElement(By.XPath("//*[@id='MainContent_gvPmtHstry']/tbody"));
                        IList<IWebElement> TRTaxPayment = TaxPayment.FindElements(By.TagName("tr"));
                        IList<IWebElement> THTaxPayment = TaxPayment.FindElements(By.TagName("th"));
                        IList<IWebElement> TDTaxPayment;
                        foreach (IWebElement row in TRTaxPayment)
                        {
                            TDTaxPayment = row.FindElements(By.TagName("td"));
                            if (TDTaxPayment.Count != 0 && !row.Text.Contains("Payment Type") && row.Text.Trim() != "")
                            {
                                string TaxPaymentdetails = TDTaxPayment[0].Text + "~" + TDTaxPayment[1].Text + "~" + TDTaxPayment[2].Text + "~" + TDTaxPayment[3].Text + "~" + TDTaxPayment[4].Text + "~" + TDTaxPayment[5].Text + "~" + TDTaxPayment[6].Text;
                                gc.insert_date(orderNumber, parcelNumber, 1611, TaxPaymentdetails, 1, DateTime.Now);
                            }
                        }
                    }
                    catch { }

                    try
                    {
                        driver.FindElement(By.LinkText("Tax Allocation")).Click();
                        Thread.Sleep(4000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Tax Distribution Details", driver, "CO", "Jefferson");
                    }
                    catch { }
                    // Tax Distribution Details
                    try
                    {
                        IWebElement TaxDistribution = driver.FindElement(By.XPath("//*[@id='MainContent_gvTaxAlloc']/tbody"));
                        IList<IWebElement> TRTaxDistribution = TaxDistribution.FindElements(By.TagName("tr"));
                        IList<IWebElement> THTaxDistribution = TaxDistribution.FindElements(By.TagName("th"));
                        IList<IWebElement> TDTaxDistribution;
                        foreach (IWebElement row in TRTaxDistribution)
                        {
                            TDTaxDistribution = row.FindElements(By.TagName("td"));
                            if (TDTaxDistribution.Count != 0 && !row.Text.Contains("Tax Authority") && row.Text.Trim() != "")
                            {
                                string TaxDistributiondetails = TDTaxDistribution[0].Text + "~" + TDTaxDistribution[1].Text + "~" + TDTaxDistribution[2].Text;
                                gc.insert_date(orderNumber, parcelNumber, 1612, TaxDistributiondetails, 1, DateTime.Now);
                            }
                        }
                    }
                    catch { }
                    string fileName = "";
                    try
                    {

                        var chromeOptions = new ChromeOptions();
                        var downloadDirectory = ConfigurationManager.AppSettings["AutoPdf"];
                        chromeOptions.AddUserProfilePreference("download.default_directory", downloadDirectory);
                        chromeOptions.AddUserProfilePreference("download.prompt_for_download", false);
                        chromeOptions.AddUserProfilePreference("disable-popup-blocking", "true");
                        chromeOptions.AddUserProfilePreference("plugins.always_open_pdf_externally", true);
                        var driver1 = new ChromeDriver(chromeOptions);
                        driver1.Navigate().GoToUrl("https://ttps.jeffco.us/ASPX/ScheduleSearch");
                        Thread.Sleep(5000);

                        try
                        {
                            driver1.FindElement(By.LinkText("Search by AIN/Parcel ID")).Click();
                            Thread.Sleep(4000);
                        }
                        catch { }
                        driver1.FindElement(By.Id("MainContent_txtAINToSearch")).SendKeys(parcelNumber);
                        driver1.FindElement(By.Id("MainContent_btnAINSubmit")).Click();
                        Thread.Sleep(4000);

                        try
                        {
                            driver1.FindElement(By.XPath("//*[@id='MainContent_gvAINList']/tbody/tr[2]/td[1]/a")).Click();
                            Thread.Sleep(7000);
                        }
                        catch { }

                        try
                        {
                            driver1.FindElement(By.LinkText("Tax Statement")).Click();
                            Thread.Sleep(5000);
                            gc.CreatePdf(orderNumber, parcelNumber, "Tax Statement", driver1, "CO", "Jefferson");
                        }
                        catch { }

                        IWebElement Iclick = driver1.FindElement(By.Id("MainContent_btnDsplyTaxStmt"));
                        Iclick.Click();
                        Thread.Sleep(7000);
                        fileName = "DsplyTaxStatement" + ".pdf";
                        gc.AutoDownloadFile(orderNumber, parcelNumber, "Jefferson", "CO", fileName);
                        Thread.Sleep(2000);
                        driver1.Quit();
                    }
                    catch { }



                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "CO", "Jefferson", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    gc.mergpdf(orderNumber, "CO", "Jefferson");
                    return "Data Inserted Successfully";
                }

                catch (Exception ex)
                {
                    driver.Quit();
                    throw ex;
                }
            }
        }
    }
}