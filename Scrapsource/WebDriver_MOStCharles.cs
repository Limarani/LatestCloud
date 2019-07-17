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
    public class WebDriver_MOStCharles
    {

        string outputPath = "";
        IWebDriver driver;
        DBconnection db = new DBconnection();

        GlobalClass gc = new GlobalClass();
        MySqlParameter[] mParam;
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        public string FTP_StCahrles(string houseno, string sname, string sttype, string account, string parcelNumber, string searchType, string orderNumber, string ownername, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;

            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
            string address = houseno + " " + sname;
            string ParcellNumber = "";

            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            using (driver = new PhantomJSDriver())
            {


                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");
                    if (searchType == "titleflex")
                    {
                        gc.TitleFlexSearch(orderNumber, parcelNumber, "", address, "MO", "St Charles");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_MOStCharles"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }
                    if (searchType == "address")
                    {
                        driver.Navigate().GoToUrl("https://lookups.sccmo.org/assessor");
                        Thread.Sleep(3000);

                        driver.FindElement(By.Id("SitusName")).SendKeys(address);
                        gc.CreatePdf_WOP(orderNumber, "Address search", driver, "MO", "St Charles");

                        driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/form/div/div[7]/div[2]/input")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);
                        gc.CreatePdf_WOP(orderNumber, "Address search Result", driver, "MO", "St Charles");
                        string Parcellcount = driver.FindElement(By.XPath("/html/body/div[1]/div/div[3]")).Text;

                        //multi parcel

                        try
                        {
                            var MultiparcellSplit = Parcellcount.Substring(0, Parcellcount.IndexOf("\r\n"));

                            if (MultiparcellSplit != "Showing 1 to 1 out of 1")
                            {
                                IWebElement Propertytable = driver.FindElement(By.XPath("/html/body/div[1]/div/form/div/table"));
                                IList<IWebElement> propertytableRow = Propertytable.FindElements(By.TagName("tr"));
                                int rowcount = propertytableRow.Count;
                                IList<IWebElement> propertyrowTD;
                                List<string> listurl = new List<string>();
                                foreach (IWebElement rownew in propertytableRow)
                                {
                                    propertyrowTD = rownew.FindElements(By.TagName("a"));
                                    if (propertyrowTD.Count != 0 && rownew.Text.Contains("Details"))
                                    {
                                        string url = propertyrowTD[0].GetAttribute("href");
                                        listurl.Add(url);
                                    }
                                }

                                foreach (string URL in listurl)
                                {

                                    driver.Navigate().GoToUrl(URL);
                                    Thread.Sleep(3000);
                                    //CreatePdf_WOP(orderNumber, "Property search Result");

                                    string ParcellNumberDuplicate = driver.FindElement(By.XPath("/html/body/div[1]/div/table/tbody/tr[1]/td[2]")).Text;
                                    string OwnerNameDuplicate = driver.FindElement(By.XPath("/html/body/div[1]/div/table/tbody/tr[2]/td[1]")).Text;
                                    string AddressDuplicate = driver.FindElement(By.XPath("/html/body/div[1]/div/table/tbody/tr[2]/td[2]")).Text;


                                    var splitted1 = ParcellNumberDuplicate.Split(':');
                                    ParcellNumberDuplicate = splitted1[1];

                                    ParcellNumber = ParcellNumberDuplicate;
                                    //Split 3
                                    string[] stringSeparators = new string[] { "\r\n" };


                                    string result1 = string.Concat(OwnerNameDuplicate.TakeWhile(c => c < '0' || c > '9'));

                                    string[] lines = result1.Split(stringSeparators, StringSplitOptions.None);
                                    string ownerName = "";
                                    foreach (string s in lines)
                                    {
                                        if (s != "Owner(s):")
                                        {

                                            ownerName = s + "" + ownerName;

                                        }

                                    }


                                    string OwnerName = ownerName;
                                    //Split 4


                                    var splitted4 = AddressDuplicate.Split(':');
                                    AddressDuplicate = splitted4[1];

                                    string Address = AddressDuplicate;


                                    string multiparcedata = OwnerName + "~" + Address;
                                    gc.insert_date(orderNumber, ParcellNumber, 96, multiparcedata, 1, DateTime.Now);
                                }
                                if (propertytableRow.Count > 25)
                                {
                                    HttpContext.Current.Session["multiParcel_StCharles_Multicount"] = "Maximum";
                                    driver.Quit();
                                    return "Maximum";
                                }
                                if (propertytableRow.Count <= 25)
                                {
                                    HttpContext.Current.Session["multiParcel_StCharles"] = "Yes";
                                    driver.Quit();
                                    return "MultiParcel";
                                }

                            }
                        }
                        catch { }

                    }

                    else if (searchType == "parcel")
                    {
                        string parcelText1 = "";
                        string parcelText2 = "";
                        string parcelText3 = "";
                        string parcelText4 = "";
                        string parcelText5 = "";
                        string parcelText6 = "";
                        if (parcelNumber.Contains("-") || parcelNumber.Contains("."))
                        {
                            parcelNumber = parcelNumber.Replace("-", "").Trim();
                            parcelNumber = parcelNumber.Replace(".", "").Trim();
                        }

                        parcelText1 = parcelNumber.Substring(0, 1);
                        parcelText2 = parcelNumber.Substring(1, 4);
                        parcelText3 = parcelNumber.Substring(5, 4);
                        parcelText4 = parcelNumber.Substring(9, 2);
                        parcelText5 = parcelNumber.Substring(11, 4);
                        parcelText6 = parcelNumber.Substring(15, 7);





                        driver.Navigate().GoToUrl("https://lookups.sccmo.org/assessor");
                        Thread.Sleep(3000);

                        driver.FindElement(By.Id("searchParcelID1")).SendKeys(parcelText1);
                        driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/form/div/div[6]/div/div/input[2]")).SendKeys(parcelText2);
                        driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/form/div/div[6]/div/div/input[3]")).SendKeys(parcelText3);
                        driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/form/div/div[6]/div/div/input[4]")).SendKeys(parcelText4);
                        driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/form/div/div[6]/div/div/input[5]")).SendKeys(parcelText5);
                        driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/form/div/div[6]/div/div/input[6]")).SendKeys(parcelText6);
                        gc.CreatePdf_WOP(orderNumber, "ParCell search", driver, "MO", "St Charles");
                        driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/form/div/div[7]/div[2]/input")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "ParCell search Result", driver, "MO", "St Charles");

                    }
                    else if (searchType == "block")
                    {
                        driver.Navigate().GoToUrl("https://lookups.sccmo.org/assessor");
                        Thread.Sleep(3000);

                        Thread.Sleep(3000);
                        driver.FindElement(By.XPath("//*[@id='account']")).SendKeys(account);

                        driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/form/div/div[7]/div[2]/input")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);
                        gc.CreatePdf_WOP(orderNumber, "Unit Number search", driver, "MO", "St Charles");
                        Thread.Sleep(3000);
                        string Parcellcount = driver.FindElement(By.XPath("/html/body/div[1]/div/div[3]")).Text;
                        gc.CreatePdf_WOP(orderNumber, "Unit Number search Result", driver, "MO", "St Charles");
                        //multi parcel

                        try
                        {
                            var MultiparcellSplit = Parcellcount.Substring(0, Parcellcount.IndexOf("\r\n"));

                            if (MultiparcellSplit != "Showing 1 to 1 out of 1")
                            {
                                IWebElement Propertytable = driver.FindElement(By.XPath("/html/body/div[1]/div/form/div/table"));
                                IList<IWebElement> propertytableRow = Propertytable.FindElements(By.TagName("tr"));
                                int rowcount = propertytableRow.Count;
                                IList<IWebElement> propertyrowTD;
                                List<string> listurl = new List<string>();
                                foreach (IWebElement rownew in propertytableRow)
                                {
                                    propertyrowTD = rownew.FindElements(By.TagName("a"));
                                    if (propertyrowTD.Count != 0 && rownew.Text.Contains("Details"))
                                    {
                                        string url = propertyrowTD[0].GetAttribute("href");
                                        listurl.Add(url);
                                    }
                                }

                                foreach (string URL in listurl)
                                {

                                    driver.Navigate().GoToUrl(URL);
                                    Thread.Sleep(3000);
                                    gc.CreatePdf_WOP(orderNumber, "Account search Detail", driver, "MO", "St Charles");
                                    string ParcellNumberDuplicate = driver.FindElement(By.XPath("/html/body/div[1]/div/table/tbody/tr[1]/td[2]")).Text;
                                    string OwnerNameDuplicate = driver.FindElement(By.XPath("/html/body/div[1]/div/table/tbody/tr[2]/td[1]")).Text;
                                    string AddressDuplicate = driver.FindElement(By.XPath("/html/body/div[1]/div/table/tbody/tr[2]/td[2]")).Text;

                                    ////Split 2

                                    var splitted1 = ParcellNumberDuplicate.Split(':');
                                    ParcellNumberDuplicate = splitted1[1];

                                    ParcellNumber = ParcellNumberDuplicate;
                                    //Split 3
                                    string[] stringSeparators = new string[] { "\r\n" };




                                    string result1 = string.Concat(OwnerNameDuplicate.TakeWhile(c => c < '0' || c > '9'));

                                    string[] lines = result1.Split(stringSeparators, StringSplitOptions.None);
                                    string ownerName = "";
                                    foreach (string s in lines)
                                    {
                                        if (s != "Owner(s):")
                                        {

                                            ownerName = s + "" + ownerName;

                                        }

                                    }


                                    string OwnerName = ownerName;
                                    //Split 4


                                    var splitted4 = AddressDuplicate.Split(':');
                                    AddressDuplicate = splitted4[1];

                                    string Address = AddressDuplicate;
                                    //Split 6

                                    string multiparcedata = OwnerName + "~" + Address;
                                    gc.insert_date(orderNumber, ParcellNumber, 96, multiparcedata, 1, DateTime.Now);

                                }
                                if (propertytableRow.Count > 25)
                                {
                                    HttpContext.Current.Session["multiParcel_StCharles_Multicount"] = "Maximum";
                                    driver.Quit();
                                    return "Maximum";
                                }
                                if (propertytableRow.Count <= 25)
                                {
                                    HttpContext.Current.Session["multiParcel_StCharles"] = "Yes";
                                    driver.Quit();
                                    return "MultiParcel";
                                }
                            }
                        }
                        catch { }
                    }

                    try
                    {
                        IWebElement INodata = driver.FindElement(By.XPath("//*[@id='main']/div[1]/p")); 
                        if(INodata.Text.Contains("No results were found"))
                        {
                            HttpContext.Current.Session["Nodata_MOStCharles"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }

                    string Accountnumber = "";

                    IWebElement Propertytable1 = driver.FindElement(By.XPath("/html/body/div[1]/div/form/div/table"));
                    IList<IWebElement> propertytableRow1 = Propertytable1.FindElements(By.TagName("tr"));
                    int rowcount1 = propertytableRow1.Count;
                    IList<IWebElement> propertyrowTD1;
                    List<string> listurl1 = new List<string>();
                    foreach (IWebElement rownew in propertytableRow1)
                    {
                        propertyrowTD1 = rownew.FindElements(By.TagName("a"));
                        if (propertyrowTD1.Count != 0 && rownew.Text.Contains("Details"))
                        {
                            string url = propertyrowTD1[0].GetAttribute("href");
                            listurl1.Add(url);
                        }
                    }

                    foreach (string URL in listurl1)
                    {

                        //Property
                        driver.Navigate().GoToUrl(URL);

                        Thread.Sleep(3000);
                        gc.CreatePdf_WOP(orderNumber, "Property Search Result", driver, "MO", "St Charles");
                        string AccountnumberDuplicate = driver.FindElement(By.XPath("/html/body/div[1]/div/table/tbody/tr[1]/td[1]")).Text;
                        string ParcellNumberDuplicate = driver.FindElement(By.XPath("/html/body/div[1]/div/table/tbody/tr[1]/td[2]")).Text;
                        string OwnerNameDuplicate = driver.FindElement(By.XPath("/html/body/div[1]/div/table/tbody/tr[2]/td[1]")).Text;
                        string AddressDuplicate = driver.FindElement(By.XPath("/html/body/div[1]/div/table/tbody/tr[2]/td[2]")).Text;
                        string SchoolDistrictDuplicate = driver.FindElement(By.XPath("/html/body/div[1]/div/table/tbody/tr[3]/td")).Text;
                        string CityDuplicate = driver.FindElement(By.XPath("/html/body/div[1]/div/table/tbody/tr[4]/td")).Text;
                        string FireDistrictDuplicate = driver.FindElement(By.XPath("/html/body/div[1]/div/table/tbody/tr[5]/td")).Text;
                        string NeighborhoodCodeDuplicate = driver.FindElement(By.XPath("/html/body/div[1]/div/table/tbody/tr[6]/td")).Text;
                        string SubDivisionDuplicate = driver.FindElement(By.XPath("/html/body/div[1]/div/table/tbody/tr[7]/td")).Text;
                        string LegalDescriptionDuplicate = driver.FindElement(By.XPath("/html/body/div[1]/div/table/tbody/tr[8]/td")).Text;
                        string YearBuiltDuplicate = driver.FindElement(By.XPath("/html/body/div[1]/div/table/tbody/tr[11]/td[1]")).Text;
                        string PropertyTypeDuplicate = driver.FindElement(By.XPath("/html/body/div[1]/div/table/tbody/tr[11]/td[2]")).Text;
                        string CommercialValueDuplicate = "";
                        string Total_market_valueDuplicate = "";
                        string ResidentialValueDuplicate = "";
                        string LandValueDuplicate = "";
                        string AgricultureValueDuplicate = "";
                        string Ok = "";
                        int Increment = 0;
                        //Assessed
                        //*[@id="main"]/table/tbody/tr[25]/td[2]
                        IWebElement AssessedTable = driver.FindElement(By.XPath("//*[@id='main']/table"));
                        IList<IWebElement> AssessedRow = AssessedTable.FindElements(By.TagName("tr"));
                        IList<IWebElement> TD;

                        foreach (IWebElement row3 in AssessedRow)
                        {
                            TD = row3.FindElements(By.TagName("td"));
                            if (Ok == "Allowed" && Increment == 0)
                            {

                                CommercialValueDuplicate = TD[0].Text.ToString();
                                Total_market_valueDuplicate = TD[1].Text.ToString();
                                Increment++;

                            }
                            else if (Ok == "Allowed" && Increment == 1)
                            {

                                ResidentialValueDuplicate = TD[0].Text.ToString();
                                LandValueDuplicate = TD[1].Text.ToString();
                                Increment++;
                            }
                            else if (Ok == "Allowed" && Increment == 2)
                            {

                                AgricultureValueDuplicate = TD[0].Text.ToString();

                                Increment++;
                            }
                            if (Increment == 3)
                            {



                                break;
                            }
                            if (row3.Text.Contains("Assessed Value"))
                            {
                                Ok = "Allowed";


                            }



                        }



                        //split13
                        var splitted13 = CommercialValueDuplicate.Split(':');
                        CommercialValueDuplicate = splitted13[1];

                        string CommercialValue = CommercialValueDuplicate;
                        //split14
                        var splitted14 = Total_market_valueDuplicate.Split(':');
                        Total_market_valueDuplicate = splitted14[1];

                        string Total_market_value = Total_market_valueDuplicate;
                        //split15
                        var splitted15 = ResidentialValueDuplicate.Split(':');
                        ResidentialValueDuplicate = splitted15[1];

                        string ResidentialValue = ResidentialValueDuplicate;

                        //split16
                        var splitted16 = LandValueDuplicate.Split(':');
                        LandValueDuplicate = splitted16[1];

                        string LandValue = LandValueDuplicate;
                        //split17
                        var splitted17 = AgricultureValueDuplicate.Split(':');
                        AgricultureValueDuplicate = splitted17[1];

                        string AgricultureValue = AgricultureValueDuplicate;
                        //Split 1

                        var splitted = AccountnumberDuplicate.Split(':');
                        AccountnumberDuplicate = splitted[1];

                        Accountnumber = AccountnumberDuplicate;
                        //Split 2

                        var splitted1 = ParcellNumberDuplicate.Split(':');
                        ParcellNumberDuplicate = splitted1[1];

                        ParcellNumber = ParcellNumberDuplicate;
                        //Split 3
                        string[] stringSeparators = new string[] { "\r\n" };




                        string result1 = string.Concat(OwnerNameDuplicate.TakeWhile(c => c < '0' || c > '9'));

                        string[] lines = result1.Split(stringSeparators, StringSplitOptions.None);
                        string ownerName = "";
                        foreach (string s in lines)
                        {
                            if (s != "Owner(s):")
                            {

                                ownerName = ownerName + "" + s;

                            }

                        }


                        string OwnerName = ownerName;
                        //Split 4


                        var splitted4 = AddressDuplicate.Split(':');
                        AddressDuplicate = splitted4[1];

                        string Address = AddressDuplicate;
                        //Split 6
                        var splitted5 = SchoolDistrictDuplicate.Split(':');
                        SchoolDistrictDuplicate = splitted5[1];

                        string SchoolDistrict = SchoolDistrictDuplicate;
                        //Split 7
                        var splitted6 = CityDuplicate.Split(':');
                        CityDuplicate = splitted6[1];

                        string City = CityDuplicate;
                        //Split 8
                        var splitted7 = NeighborhoodCodeDuplicate.Split(':');
                        NeighborhoodCodeDuplicate = splitted7[1];

                        string NeighborhoodCode = NeighborhoodCodeDuplicate;
                        //Split 9
                        var splitted8 = SubDivisionDuplicate.Split(':');
                        SubDivisionDuplicate = splitted8[1];

                        string SubDivision = SubDivisionDuplicate;
                        //Split 10
                        var splitted9 = LegalDescriptionDuplicate.Split(':');
                        LegalDescriptionDuplicate = splitted9[1];

                        string LegalDescription = LegalDescriptionDuplicate;
                        //Split 11
                        var splitted10 = YearBuiltDuplicate.Split(':');
                        YearBuiltDuplicate = splitted10[1];

                        string YearBuilt = YearBuiltDuplicate;
                        //Split 12
                        var splitted11 = PropertyTypeDuplicate.Split(':');
                        PropertyTypeDuplicate = splitted11[1];

                        string PropertyType = PropertyTypeDuplicate;
                        var splitted12 = FireDistrictDuplicate.Split(':');
                        FireDistrictDuplicate = splitted12[1];

                        string FireDistrict = FireDistrictDuplicate;
                        //PropertyInsert
                        string multiparcedata = Accountnumber + "~" + OwnerName + "~" + Address + "~" + SchoolDistrict + "~" + City + "~" + FireDistrict + "~" + NeighborhoodCode + "~" + SubDivision + "~" + LegalDescription + "~" + YearBuilt + "~" + PropertyType;
                        gc.insert_date(orderNumber, ParcellNumber, 97, multiparcedata, 1, DateTime.Now);
                        //AssessedInsert
                        string multiparcedata1 = CommercialValue + "~" + Total_market_value + "~" + ResidentialValue + "~" + LandValue + "~" + AgricultureValue;
                        gc.insert_date(orderNumber, ParcellNumber, 99, multiparcedata1, 1, DateTime.Now);


                    }

                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");

                    string Years = "";
                    driver.Navigate().GoToUrl("https://mo-stcharles-collector.publicaccessnow.com/ContactUs.aspx");
                    Thread.Sleep(1000);
                    string Taxauthoritytable = driver.FindElement(By.XPath("//*[@id='dnn_ctr430_HtmlModule_HtmlModule_lblContent']/table/tbody/tr/td[1]")).Text;
                    string taxAuthority1 = gc.Between(Taxauthoritytable, "Main County Office:", "collector@sccmo.org");
                    string Taxauthoritytable1 = driver.FindElement(By.XPath("//*[@id='dnn_ctr430_HtmlModule_HtmlModule_lblContent']/table/tbody/tr/td[2]/p[1]")).Text;
                    string taxAuthority2 = GlobalClass.Before(Taxauthoritytable1, "Fax:").Replace("\r\n", "");
                    string TaxAuthorityDuplicate = taxAuthority1 + " " + taxAuthority2;
                    //tax Information
                    driver.Navigate().GoToUrl("https://www.stcharlesmocollector.com/#/WildfireSearch");
                    Thread.Sleep(4000);
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div[1]/div/div/div[3]/button[1]")).Click();
                    }
                    catch { }


                    // string[] stringSeparators1 = new string[] { "\r\n" };
                    driver.FindElement(By.Id("searchBox")).SendKeys(Accountnumber.Trim());
                    gc.CreatePdf_WOP(orderNumber, "AccountNumber search", driver, "MO", "St Charles");
                    // driver.FindElement(By.XPath("//*[@id='btnsearch']")).SendKeys(Keys.Enter);

                    Thread.Sleep(3000);
                    gc.CreatePdf_WOP(orderNumber, "AccountNumber search Result", driver, "MO", "St Charles");
                    //Note Pending
                    IWebElement Propertytable2 = null;
                    try
                    {
                        Propertytable2 = driver.FindElement(By.XPath("//*[@id='avalon']/div/div[3]/div[2]/table/tbody"));
                    }
                    catch { }
                    try
                    {
                        if (Propertytable2 == null)
                        {
                            Propertytable2 = driver.FindElement(By.XPath("//*[@id='avalon']/div/div[4]/div[2]/table/tbody"));
                        }
                    }
                    catch { }
                    IList<IWebElement> propertytableRow2 = Propertytable2.FindElements(By.TagName("tr"));
                    IList<IWebElement> propertyrowTD2;
                    foreach (IWebElement rownew in propertytableRow2)
                    {
                        propertyrowTD2 = rownew.FindElements(By.TagName("td"));
                        string sample4 = propertyrowTD2[4].GetAttribute("innerText");
                        if ((propertyrowTD2.Count) != 0 && propertyrowTD2[4].GetAttribute("innerText") == "Real Property")
                        {
                            string taxHistoryresult = propertyrowTD2[0].Text + "~" + propertyrowTD2[1].GetAttribute("innerText") + "~" + propertyrowTD2[2].Text + "~" + propertyrowTD2[3].GetAttribute("innerText") + "~" + propertyrowTD2[4].GetAttribute("innerText") + "~" + propertyrowTD2[5].GetAttribute("innerText") + "~" + propertyrowTD2[6].GetAttribute("innerText");
                            gc.insert_date(orderNumber, ParcellNumber, 1222, taxHistoryresult, 1, DateTime.Now);
                        }

                    }
                    for (int i = 1; i < 4; i++)
                    {
                        try
                        {
                            driver.FindElement(By.XPath("//*[@id='filters']/div[3]/div[" + i + "]/label/input")).Click();
                            Thread.Sleep(2000);
                            gc.CreatePdf_WOP(orderNumber, "AccountNumber Year Result" + i, driver, "MO", "St Charles");
                            //*[@id="avalon"]/div/div[3]/div[2]/table/tbody
                            IWebElement Propertytable = null;
                            try
                            {
                                Propertytable = driver.FindElement(By.XPath("//*[@id='avalon']/div/div[3]/div[2]/table/tbody"));
                            }
                            catch { }
                            try
                            {
                                if (Propertytable == null)
                                {
                                    Propertytable = driver.FindElement(By.XPath("//*[@id='avalon']/div/div[4]/div[2]/table/tbody"));
                                }
                            }
                            catch { }
                            IList<IWebElement> propertytableRow = Propertytable.FindElements(By.TagName("tr"));
                            IList<IWebElement> propertyrowTD;
                            foreach (IWebElement rownew in propertytableRow)
                            {
                                propertyrowTD = rownew.FindElements(By.TagName("td"));
                                if ((propertyrowTD.Count) != 0 && propertyrowTD[4].GetAttribute("innerText").Trim() == "Real Property")
                                {
                                    IWebElement Iviewtax = propertyrowTD[8].FindElement(By.TagName("button"));
                                    IJavaScriptExecutor js = driver as IJavaScriptExecutor;
                                    js.ExecuteScript("arguments[0].click();", Iviewtax);
                                    //propertyrowTD[7].Click();
                                    Thread.Sleep(2000);
                                    break;

                                }

                            }
                            gc.CreatePdf_WOP(orderNumber, "AccountNumber Detail Result" + i, driver, "MO", "St Charles");
                            string owenerinfomation = driver.FindElement(By.XPath("//*[@id='avalon']/div/div/div/div[1]/div[1]/div[1]")).Text;
                            string[] Ownersplit = owenerinfomation.Split('\r');
                            string ownernametax = Ownersplit[1].Replace("\n", "") + " " + Ownersplit[2].Replace("\n", "");
                            string MailingAddress = Ownersplit[3].Replace("\n", "") + " " + Ownersplit[4].Replace("\n", "");
                            string classess = driver.FindElement(By.XPath("//*[@id='avalon']/div/div/div/div[1]/div[2]/div[3]/table/tbody/tr[1]/td")).Text;
                            string Peoprtyinfotable = driver.FindElement(By.XPath("//*[@id='avalon']/div/div/div/div[1]/div[2]/div[1]/table/tbody")).Text;
                            string Taxyear = gc.Between(Peoprtyinfotable, "Tax Year", "Account # / PIN");
                            string Account = gc.Between(Peoprtyinfotable, "Account # / PIN", "Description");
                            string Description = "", Acres = "";
                            if (!Peoprtyinfotable.Contains("Acres"))
                            {
                                Description = gc.Between(Peoprtyinfotable, "Description", "Geo CD");
                            }
                            else
                            {
                                Description = gc.Between(Peoprtyinfotable, "Description", "Acres");
                                Acres = gc.Between(Peoprtyinfotable, "Acres", "Geo CD");
                            }
                            string GeoCD = gc.Between(Peoprtyinfotable, "Geo CD", "Situs Address");
                            string SitusAddress = GlobalClass.After(Peoprtyinfotable, "Situs Address");
                            string Paymentinfotable = driver.FindElement(By.XPath("//*[@id='avalon']/div/div/div/div[1]/div[1]/div[2]/table/tbody")).Text;
                            string Status = gc.Between(Paymentinfotable, "Status", "Last Payment Date");
                            string LastPaymentDate = gc.Between(Paymentinfotable, "Last Payment Date", "Amount Paid");
                            string AmountPaid = gc.Between(Paymentinfotable, "Amount Paid", "Payer Name");
                            string PayerName = gc.Between(Paymentinfotable, "Payer Name", "Receipt Number");
                            string ReceiptNumber = GlobalClass.After(Paymentinfotable, "Receipt Number");
                            string Billinfomation = driver.FindElement(By.XPath("//*[@id='avalon']/div/div/div/div[1]/div[2]/div[2]/table/tbody")).Text;
                            string BillNumber = gc.Between(Billinfomation, "Bill Number", "Base Taxes");
                            string BaseTaxes = gc.Between(Billinfomation, "Base Taxes", "Penalty");
                            string Penalty = gc.Between(Billinfomation, "Penalty", "Interest");
                            string Interest = gc.Between(Billinfomation, "Interest", "Discount");
                            string Discount = gc.Between(Billinfomation, "Discount", "Total Due");
                            string TotalDue = GlobalClass.After(Billinfomation, "Total Due");
                            string TaxBillresult = ownernametax + "~" + MailingAddress + "~" + classess + "~" + Taxyear + "~" + Account + "~" + Description + "~" + Acres + "~" + GeoCD + "~" + SitusAddress + "~" + Status + "~" + LastPaymentDate + "~" + AmountPaid + "~" + PayerName + "~" + ReceiptNumber + "~" + BillNumber + "~" + BaseTaxes + "~" + Penalty + "~" + Interest + "~" + Discount + "~" + TotalDue + "~" + TaxAuthorityDuplicate;
                            gc.insert_date(orderNumber, ParcellNumber, 1224, TaxBillresult, 1, DateTime.Now);


                            IWebElement taxinstalmenttable = driver.FindElement(By.XPath("//*[@id='avalon']/div/div/div/div[1]/div[3]/div/table/tbody"));

                            IList<IWebElement> taxinstalmentrow = taxinstalmenttable.FindElements(By.TagName("tr"));

                            IList<IWebElement> taxinstalmentid;
                            foreach (IWebElement taxinstalment in taxinstalmentrow)
                            {
                                string sample1 = taxinstalment.GetAttribute("innerHTML");
                                taxinstalmentid = taxinstalment.FindElements(By.TagName("td"));
                                if (taxinstalmentid.Count != 0)
                                {
                                    string TaxInstalresult = Taxyear + "~" + taxinstalmentid[0].GetAttribute("innerText") + "~" + taxinstalmentid[1].GetAttribute("innerText") + "~" + taxinstalmentid[2].GetAttribute("innerText") + "~" + taxinstalmentid[3].GetAttribute("innerText") + "~" + taxinstalmentid[4].GetAttribute("innerText") + "~" + taxinstalmentid[5].GetAttribute("innerText") + "~" + taxinstalmentid[6].GetAttribute("innerText");
                                    gc.insert_date(orderNumber, ParcellNumber, 1225, TaxInstalresult, 1, DateTime.Now);
                                }
                            }
                            IWebElement TaxDistributiontable = driver.FindElement(By.XPath("//*[@id='avalon']/div/div/div/div[1]/div[4]/div/table/tbody"));
                            IList<IWebElement> TaxDistributionrow = TaxDistributiontable.FindElements(By.TagName("tr"));
                            IList<IWebElement> TaxDistributionid;
                            foreach (IWebElement TaxDistribution in TaxDistributionrow)
                            {
                                TaxDistributionid = TaxDistribution.FindElements(By.TagName("td"));
                                if (TaxDistributionid.Count != 0)
                                {
                                    string TaxDistributionresult = Taxyear + "~" + GlobalClass.Before(TaxDistributionid[0].GetAttribute("innerText"), ":") + "~" + TaxDistributionid[1].GetAttribute("innerText") + "~" + TaxDistributionid[2].GetAttribute("innerText") + "~" + TaxDistributionid[3].GetAttribute("innerText") + "~" + TaxDistributionid[4].GetAttribute("innerText") + "~" + TaxDistributionid[5].GetAttribute("innerText");
                                    // TaxDistributionresult = "";
                                    gc.insert_date(orderNumber, ParcellNumber, 1226, TaxDistributionresult, 1, DateTime.Now);
                                }
                            }
                            IWebElement TaxDistributiontableTF = driver.FindElement(By.XPath("//*[@id='avalon']/div/div/div/div[1]/div[4]/div/table/tfoot"));
                            IList<IWebElement> TaxDistributionrowTF = TaxDistributiontableTF.FindElements(By.TagName("tr"));
                            IList<IWebElement> TaxDistributionidTF;
                            foreach (IWebElement TaxDistributionTF in TaxDistributionrowTF)
                            {
                                TaxDistributionidTF = TaxDistributionTF.FindElements(By.TagName("th"));
                                if (TaxDistributionidTF.Count != 0)
                                {
                                    string TaxDistributionresult = Taxyear + "~" + TaxDistributionidTF[0].GetAttribute("innerText") + "~" + TaxDistributionidTF[1].GetAttribute("innerText") + "~" + TaxDistributionidTF[2].GetAttribute("innerText") + "~" + TaxDistributionidTF[3].GetAttribute("innerText") + "~" + TaxDistributionidTF[4].GetAttribute("innerText") + "~" + TaxDistributionidTF[5].GetAttribute("innerText");
                                    gc.insert_date(orderNumber, ParcellNumber, 1226, TaxDistributionresult, 1, DateTime.Now);
                                }
                            }
                            driver.Navigate().Back();
                            Thread.Sleep(1000);
                            driver.FindElement(By.XPath("//*[@id='filters']/div[3]/div[" + i + "]/label/input")).Click();
                            Thread.Sleep(2000);
                        }
                        catch { }
                    }


                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "MO", "St Charles", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    gc.mergpdf(orderNumber, "MO", "St Charles");
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