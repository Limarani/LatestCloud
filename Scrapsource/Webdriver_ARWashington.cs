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
    public class Webdriver_ARWashington
    {
        IWebDriver driver;
        DBconnection db = new DBconnection();

        GlobalClass gc = new GlobalClass();
        MySqlParameter[] mParam;
        IWebElement MultiAssessTB;
        string strAssess = "";
        public string FTP_Washington(string houseno, string sname, string direction, string account, string parcelNumber, string searchType, string orderNumber, string ownername, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
            string address = houseno + " " + sname;
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            // driver = new ChromeDriver();
            using (driver = new PhantomJSDriver())
            {
                string AlterNateID = "", PropertyAddress = "", owner = "", strowner = "", straddress = "", strproperty = "";
                string[] stringSeparators1 = new string[] { "\r\n" };

                StartTime = DateTime.Now.ToString("HH:mm:ss");
                List<string> listurl = new List<string>();
                if (sname.Trim() != "" && houseno.Trim() != "")
                {
                    searchType = "address";
                }
                if (parcelNumber.Trim() != "")
                {
                    searchType = "parcel";
                }
                try
                {

                    if (searchType == "titleflex")
                    {

                        gc.TitleFlexSearch(orderNumber, parcelNumber, "", address, "AR", "Washington");

                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_ARWashington"] = "Zero";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }


                    if (searchType == "address")
                    {

                        driver.Navigate().GoToUrl("https://www.actdatascout.com/RealProperty/Arkansas/Washington");
                        Thread.Sleep(3000);


                        driver.FindElement(By.XPath("//*[@id='StreetNumber']")).SendKeys(houseno);


                        driver.FindElement(By.XPath("//*[@id='StreetName']")).SendKeys(sname);
                        gc.CreatePdf_WOP(orderNumber, "Address search", driver, "AR", "Washington");
                        IWebElement Iaddress = driver.FindElement(By.XPath("//*[@id='SearchBottom']"));
                        IJavaScriptExecutor js = driver as IJavaScriptExecutor;
                        js.ExecuteScript("arguments[0].click();", Iaddress);
                        Thread.Sleep(4000);

                        gc.CreatePdf_WOP(orderNumber, "Address search Result", driver, "AR", "Washington");

                        try
                        {
                            string Multi = driver.FindElement(By.XPath("//*[@id='SearchFeedback']/div[1]/div/div[1]")).Text;
                            string strMultiCount = GlobalClass.Before(Multi, " Result(s)");
                            gc.CreatePdf_WOP(orderNumber, "Owner search Result", driver, "AR", "Washington");

                            if ((Convert.ToInt32(strMultiCount)) > 25)
                            {
                                HttpContext.Current.Session["multiParcel_Washington_Count"] = "Maximum";
                                return "Maximum";
                            }

                            if (Multi.Trim() != "1 Result(s)")
                            {
                                IWebElement MultiOwnerTable = driver.FindElement(By.XPath("//*[@id='SearchFeedback']/div[3]"));
                                IList<IWebElement> MultiOwnerRow = MultiOwnerTable.FindElements(By.TagName("table"));
                                //IList<IWebElement> MultiOwnerRow1 = MultiOwnerTable.FindElements(By.ClassName("dl-horizontal"));
                                IList<IWebElement> MultiOwnerTD;
                                //IList<IWebElement> MultiOwnerTD1;

                                //string AlterNateID = "", PropertyAddress = "", LegalDescriptoin = "", YearBuilt = "";
                                foreach (IWebElement row1 in MultiOwnerRow)
                                {
                                    MultiOwnerTD = row1.FindElements(By.TagName("td"));

                                    if (MultiOwnerTD.Count != 0 && MultiOwnerTD.Count != 2 && MultiOwnerTD.Count != 1)
                                    {

                                        parcelNumber = MultiOwnerTD[1].Text.Replace("Parcel:", "");
                                        string Pa = MultiOwnerTD[3].Text.Replace("Prev.Parcel:", "");
                                        try
                                        {
                                            strproperty = driver.FindElement(By.XPath("//*[@id='SearchFeedback']/div[3]/div[1]")).Text;
                                            strowner = gc.Between(strproperty, "Owner: ", "\r\nAddress:");
                                            straddress = gc.Between(strproperty, "Address: ", "\r\nMail Address:");
                                        }
                                        catch { }
                                        try
                                        {
                                            if (strowner == "" || straddress == "")
                                            {
                                                strproperty = driver.FindElement(By.XPath("//*[@id='SearchFeedback']/div[3]/div[1]")).Text;
                                                strowner = gc.Between(strproperty, "Owner: ", "Address:");
                                                straddress = gc.Between(strproperty, "Address: ", "Mail Address:");
                                            }
                                        }
                                        catch { }
                                        //ownername = gc.Between(ownername, "Owner:", "Address:");
                                        gc.insert_date(orderNumber, parcelNumber, 507, Pa + "~" + strowner + "~" + straddress, 1, DateTime.Now);

                                    }
                                }



                                //GlobalClass.multiParcel_MiamiDade = "Yes";
                                HttpContext.Current.Session["multiParcel_Washington"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";

                            }
                            IWebElement button = driver.FindElement(By.XPath("//*[@id='SearchFeedback']/div[3]/table/thead/tr/td[1]"));
                            IList<IWebElement> MultiOwnerbtn = button.FindElements(By.TagName("button"));
                            foreach (IWebElement row1 in MultiOwnerbtn)
                            {
                                row1.Click();
                                break;

                            }
                        }
                        catch { }
                    }

                    else if (searchType == "ownername")
                    {
                        driver.Navigate().GoToUrl("https://www.actdatascout.com/RealProperty/Arkansas/Washington");
                        Thread.Sleep(3000);
                        var Name = ownername.Replace(",", "").Split(' ');
                        if (Name.Length == 2)
                        {
                            driver.FindElement(By.XPath("//*[@id='FirstName']")).SendKeys(Name[1].ToUpper());
                            driver.FindElement(By.XPath("//*[@id='LastName']")).SendKeys(Name[0].ToUpper());
                            IWebElement Iowner1 = driver.FindElement(By.XPath("//*[@id='SearchBottom']"));
                            IJavaScriptExecutor js1 = driver as IJavaScriptExecutor;
                            js1.ExecuteScript("arguments[0].click();", Iowner1); ;
                            try
                            {
                                string ownerResult = driver.FindElement(By.XPath("//*[@id='RPSearch']/div[2]")).Text;
                                if (ownerResult.Contains("No results. Please broaden your search criteria"))
                                {
                                    driver.FindElement(By.XPath("//*[@id='LastName']")).SendKeys(Name[1].ToUpper());
                                    driver.FindElement(By.XPath("//*[@id='FirstName']")).SendKeys(Name[0].ToUpper());

                                    IWebElement Iowner = driver.FindElement(By.XPath("//*[@id='SearchBottom']"));
                                    IJavaScriptExecutor js = driver as IJavaScriptExecutor;
                                    js.ExecuteScript("arguments[0].click();", Iowner);
                                }
                            }
                            catch { }
                        }
                        if (Name.Length == 1)
                        {
                            driver.FindElement(By.XPath("//*[@id='LastName']")).SendKeys(Name[0].ToUpper());
                        }
                        gc.CreatePdf_WOP(orderNumber, "Owner search", driver, "AR", "Washington");
                        Thread.Sleep(2000);


                        try
                        {
                            string Multi = driver.FindElement(By.XPath("//*[@id='SearchFeedback']/div[1]/div/div[1]")).Text;
                            string strMultiCount = GlobalClass.Before(Multi, " Result(s)");
                            gc.CreatePdf_WOP(orderNumber, "Owner search Result", driver, "AR", "Washington");

                            if ((Convert.ToInt32(strMultiCount)) > 25)
                            {
                                HttpContext.Current.Session["multiParcel_Washington_Count"] = "Maximum";
                                return "Maximum";
                            }

                            if (Multi.Trim() != "1 Result(s)")
                            {

                                IWebElement MultiOwnerTable = driver.FindElement(By.XPath("//*[@id='SearchFeedback']/div[3]"));
                                IList<IWebElement> MultiOwnerRow = MultiOwnerTable.FindElements(By.TagName("table"));
                                //IList<IWebElement> MultiOwnerRow1 = MultiOwnerTable.FindElements(By.ClassName("dl-horizontal"));
                                IList<IWebElement> MultiOwnerTD;
                                //IList<IWebElement> MultiOwnerTD1;

                                //string AlterNateID = "", PropertyAddress = "", LegalDescriptoin = "", YearBuilt = "";
                                foreach (IWebElement row1 in MultiOwnerRow)
                                {
                                    MultiOwnerTD = row1.FindElements(By.TagName("td"));

                                    if (MultiOwnerTD.Count != 0 && MultiOwnerTD.Count != 2 && MultiOwnerTD.Count != 1)
                                    {

                                        parcelNumber = MultiOwnerTD[1].Text.Replace("Parcel:", "");
                                        string Pa = MultiOwnerTD[3].Text.Replace("Prev.Parcel:", "");
                                        try
                                        {
                                            strproperty = driver.FindElement(By.XPath("//*[@id='SearchFeedback']/div[3]/div[1]")).Text;
                                            strowner = gc.Between(strproperty, "Owner:", "\r\nAddress:");
                                            straddress = gc.Between(strproperty, "Address:", "\r\nMail Address:");
                                        }
                                        catch { }
                                        try
                                        {
                                            if (strowner == "" || straddress == "")
                                            {
                                                strproperty = driver.FindElement(By.XPath("//*[@id='SearchFeedback']/div[3]/div[1]")).Text;
                                                strowner = gc.Between(strproperty, "Owner:", "Address:");
                                                straddress = gc.Between(strproperty, "Address:", "Mail Address:");
                                            }
                                        }
                                        catch { }
                                        //ownername = gc.Between(ownername, "Owner:", "Address:");
                                        gc.insert_date(orderNumber, parcelNumber, 507, Pa + "~" + strowner + "~" + straddress, 1, DateTime.Now);

                                    }
                                }

                                //GlobalClass.multiParcel_MiamiDade = "Yes";
                                HttpContext.Current.Session["multiParcel_Washington"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";

                            }
                            IWebElement button = driver.FindElement(By.XPath("//*[@id='SearchFeedback']/div[3]/table/thead/tr/td[1]"));
                            IList<IWebElement> MultiOwnerbtn = button.FindElements(By.TagName("button"));
                            foreach (IWebElement row1 in MultiOwnerbtn)
                            {
                                row1.Click();
                                break;
                            }
                        }
                        catch { }
                    }

                    else if (searchType == "parcel")
                    {

                        driver.Navigate().GoToUrl("https://www.actdatascout.com/RealProperty/Arkansas/Washington");
                        Thread.Sleep(3000);


                        driver.FindElement(By.XPath("//*[@id='ParcelNumber']")).SendKeys(parcelNumber.Trim());

                        IWebElement IParcel = driver.FindElement(By.XPath("//*[@id='SearchBottom']"));
                        IJavaScriptExecutor js = driver as IJavaScriptExecutor;
                        js.ExecuteScript("arguments[0].click();", IParcel);
                        Thread.Sleep(2000);

                        try
                        {
                            //*[@id="SearchFeedback"]/div[3]/table/thead/tr/td[1]

                            IWebElement button = driver.FindElement(By.XPath("//*[@id='SearchFeedback']/div[3]/table/thead/tr/td[1]"));
                            IList<IWebElement> MultiOwnerbtn = button.FindElements(By.TagName("button"));
                            foreach (IWebElement row1 in MultiOwnerbtn)
                            {
                                row1.Click();
                                break;

                            }
                        }
                        catch { }

                    }

                    try
                    {
                        //No Data Found
                        string nodata = driver.FindElement(By.Id("RPSearch")).Text;
                        if (nodata.Contains("No results"))
                        {
                            HttpContext.Current.Session["Nodata_ARWashington"] = "Zero";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }


                    string PreviousParcel = "", MailingAddress = "", LegalDescription = "", PropertyType = "", TaxDistrict = "", MillageRate = "", YearBuilt = "";

                    parcelNumber = driver.FindElement(By.XPath("//*[@id='printArea']/div[1]/div/span[1]")).Text.Trim();
                    ownername = driver.FindElement(By.XPath("//*[@id='printArea']/div[3]/div/div[2]/table/tbody/tr[1]/td[2]")).Text;
                    PropertyAddress = driver.FindElement(By.XPath("//*[@id='printArea']/div[4]/div/div[2]/table/tbody/tr[1]/td[2]")).Text;


                    string[] linesName = parcelNumber.Split(stringSeparators1, StringSplitOptions.None);
                    PreviousParcel = linesName[1].Replace("Previous Parcel:", "");
                    parcelNumber = linesName[0].Replace("Parcel:", "");



                    MailingAddress = driver.FindElement(By.XPath("//*[@id='printArea']/div[3]/div/div[2]/table/tbody/tr[2]/td[2]")).Text;
                    LegalDescription = driver.FindElement(By.XPath("//*[@id='printArea']/div[4]/div/div[2]/table/tbody/tr[6]/td[2]")).Text;
                    PropertyType = driver.FindElement(By.XPath("//*[@id='printArea']/div[3]/div/div[2]/table/tbody/tr[3]/td[2]")).Text;
                    TaxDistrict = driver.FindElement(By.XPath("//*[@id='printArea']/div[3]/div/div[2]/table/tbody/tr[4]/td[2]")).Text;
                    MillageRate = driver.FindElement(By.XPath("//*[@id='printArea']/div[3]/div/div[2]/table/tbody/tr[5]/td[2]")).Text;
                    try
                    {
                        for (int i = 12; i < 20; i++)
                        {
                            if (YearBuilt != "")
                            {
                                YearBuilt = driver.FindElement(By.XPath("//*[@id='printArea']/div[12]/div[1]/div[2]/table/tbody/tr/td[7]")).Text;
                            }
                        }
                    }
                    catch { }
                    string Iyearbuilt = "";
                    try
                    {
                        Iyearbuilt = driver.FindElement(By.XPath("//*[@id='printArea']/div[13]/div[1]/div[2]/table")).Text;
                        if (Iyearbuilt.Contains("Year Built"))
                        {
                            IWebElement strYearBuilt = driver.FindElement(By.XPath("//*[@id='printArea']/div[13]/div[1]/div[2]/table"));
                            IList<IWebElement> TRstrYearBuilt = strYearBuilt.FindElements(By.TagName("tr"));
                            IList<IWebElement> THstrYearBuilt = strYearBuilt.FindElements(By.TagName("th"));
                            IList<IWebElement> TDstrYearBuilt;
                            foreach (IWebElement row in TRstrYearBuilt)
                            {
                                TDstrYearBuilt = row.FindElements(By.TagName("td"));
                                if (TDstrYearBuilt.Count != 0 && !row.Text.Contains("Year Built") && row.Text.Trim() != "")
                                {
                                    YearBuilt = TDstrYearBuilt[6].Text;
                                }
                            }
                        }
                    }
                    catch { }
                    try
                    {
                        Iyearbuilt = driver.FindElement(By.XPath("//*[@id='printArea']/div[12]/div[1]/div[2]/table")).Text;
                        if (Iyearbuilt.Contains("Year Built"))
                        {
                            IWebElement strYearBuilt = driver.FindElement(By.XPath("//*[@id='printArea']/div[12]/div[1]/div[2]/table"));
                            IList<IWebElement> TRstrYearBuilt = strYearBuilt.FindElements(By.TagName("tr"));
                            IList<IWebElement> THstrYearBuilt = strYearBuilt.FindElements(By.TagName("th"));
                            IList<IWebElement> TDstrYearBuilt;
                            foreach (IWebElement row in TRstrYearBuilt)
                            {
                                TDstrYearBuilt = row.FindElements(By.TagName("td"));
                                if (TDstrYearBuilt.Count != 0 && !row.Text.Contains("Year Built") && row.Text.Trim() != "")
                                {
                                    YearBuilt = TDstrYearBuilt[6].Text;
                                }
                            }
                        }
                    }
                    catch { }


                    gc.CreatePdf(orderNumber, parcelNumber, "Assessment Details", driver, "AR", "Washington");

                    string Property = PreviousParcel + "~" + ownername + "~" + PropertyAddress + "~" + MailingAddress + "~" + LegalDescription + "~" + PropertyType + "~" + TaxDistrict + "~" + MillageRate + "~" + YearBuilt;
                    gc.insert_date(orderNumber, parcelNumber, 497, Property, 1, DateTime.Now);

                    string Land = "", Building = "", Total = "", Type = "";//*[@id="printArea"]/div[7]/div/div[2]/table/tbody
                    try
                    {
                        IWebElement YearB = driver.FindElement(By.XPath("//*[@id='printArea']/div[6]/div/div[2]/table/tbody"));
                        IList<IWebElement> YearTR = YearB.FindElements(By.TagName("tr"));
                        IList<IWebElement> YearTD;

                        foreach (IWebElement row1 in YearTR)
                        {
                            YearTD = row1.FindElements(By.TagName("td"));

                            if (YearTD.Count != 0)
                            {
                                if (row1.Text.Contains("Land"))
                                {
                                    Land = YearTD[1].Text;
                                    Building = YearTD[2].Text;
                                    Total = YearTD[3].Text;
                                }
                                else if (row1.Text.Contains("Building"))
                                {
                                    Land = Land + "~" + YearTD[1].Text;
                                    Building = Building + "~" + YearTD[2].Text;
                                    Total = Total + "~" + YearTD[3].Text;
                                }
                                else if (row1.Text.Contains("Totals"))
                                {
                                    Land = Land + "~" + YearTD[1].Text;
                                    Building = Building + "~" + YearTD[2].Text;
                                    Total = Total + "~" + YearTD[3].Text;
                                }

                                //Land = YearTD[1].Text + "~" + Land;
                                //Building = YearTD[2].Text + "~" + Building;
                                //Total = YearTD[3].Text + "~" + Total;
                            }


                        }
                    }
                    catch
                    {
                        IWebElement YearB = driver.FindElement(By.XPath("//*[@id='printArea']/div[7]/div/div[2]/table/tbody"));
                        IList<IWebElement> YearTR = YearB.FindElements(By.TagName("tr"));
                        IList<IWebElement> YearTD;

                        foreach (IWebElement row1 in YearTR)
                        {
                            YearTD = row1.FindElements(By.TagName("td"));

                            if (YearTD.Count != 0)
                            {
                                if (row1.Text.Contains("Land"))
                                {
                                    Land = YearTD[1].Text;
                                    Building = YearTD[2].Text;
                                    Total = YearTD[3].Text;
                                }
                                else if (row1.Text.Contains("Building"))
                                {
                                    Land = Land + "~" + YearTD[1].Text;
                                    Building = Building + "~" + YearTD[2].Text;
                                    Total = Total + "~" + YearTD[3].Text;
                                }
                                else if (row1.Text.Contains("Totals"))
                                {
                                    Land = Land + "~" + YearTD[1].Text;
                                    Building = Building + "~" + YearTD[2].Text;
                                    Total = Total + "~" + YearTD[3].Text;
                                }
                                //Land = YearTD[1].Text + "~" + Land;
                                //Building = YearTD[2].Text + "~" + Building;
                                //Total = YearTD[3].Text + "~" + Total;
                            }


                        }
                    }

                    //Land = Land + "bg";
                    //Building = Building + "bg";
                    //Total = Total + "bg";

                    //Land = Land.Replace("~bg","");
                    //Building = Building.Replace("~bg", "");
                    //Total = Total.Replace("~bg", "");

                    gc.insert_date(orderNumber, parcelNumber, 498, "Market value" + "~" + Land, 1, DateTime.Now);
                    gc.insert_date(orderNumber, parcelNumber, 498, "Assessed Value" + "~" + Building, 1, DateTime.Now);
                    gc.insert_date(orderNumber, parcelNumber, 498, "Taxable Value" + "~" + Total, 1, DateTime.Now);

                    string Assessment = "", TaxAmount = "";
                    try
                    {
                        try
                        {
                            MultiAssessTB = driver.FindElement(By.XPath("//*[@id='printArea']/div[9]/div/div[2]/table/tbody"));
                            strAssess = MultiAssessTB.Text;
                        }
                        catch { }
                        try
                        {
                            if (strAssess == "")
                            {
                                MultiAssessTB = driver.FindElement(By.XPath("//*[@id='printArea']/div[10]/div/div[2]/table/tbody"));
                            }
                        }
                        catch { }

                        IList<IWebElement> MultiAssessTR = MultiAssessTB.FindElements(By.TagName("tr"));
                        IList<IWebElement> MultiAssessTD;

                        foreach (IWebElement row1 in MultiAssessTR)
                        {
                            MultiAssessTD = row1.FindElements(By.TagName("td"));
                            if (MultiAssessTD.Count != 0)
                            {
                                if (MultiAssessTD.Count == 1)
                                {
                                    Assessment = "Total";
                                    TaxAmount = MultiAssessTD[0].Text.Replace("Total", "");
                                }
                                else
                                {
                                    Assessment = MultiAssessTD[0].Text;
                                    TaxAmount = MultiAssessTD[1].Text;
                                }


                                string Assess = Assessment + "~" + TaxAmount;
                                gc.insert_date(orderNumber, parcelNumber, 499, Assess, 1, DateTime.Now);


                            }

                        }
                        MultiAssessTB.Clear();
                    }
                    catch { }

                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");

                    driver.Navigate().GoToUrl("https://www.arcountydata.com/propsearch.asp?county=Washington&s=T");
                    Thread.Sleep(3000);
                   // var SerachCategory = driver.FindElement(By.XPath("//*[@id='carouselOverlayDiv']/div/div[2]/ul/li[62]/a")).GetAttribute("href");
                    //var selectElement1 = new SelectElement(SerachCategory);
                    //selectElement1.SelectByText("Washington");
                    //driver.Navigate().GoToUrl(SerachCategory);
                    // driver.FindElement(By.XPath("//*[@id='StreetNumber']")).SendKeys(houseno);
                    //if (direction.Trim() != "")
                    //{



                    //}                   
                    //driver.FindElement(By.XPath("//*[@id='StreetName']")).SendKeys(sname);
                    //gc.CreatePdf_WOP(orderNumber, "Address search", driver, "FL", "Collier");
                    //driver.FindElement(By.XPath("//*[@id='Collector']/div/div[2]/a")).Click();
                    Thread.Sleep(2000);//*[@id="SearchPanel"]/div/form/div[2]/div/input[1]
                    driver.FindElement(By.Name("ppan")).SendKeys(parcelNumber);
                    driver.FindElement(By.XPath("//*[@id='searchy']")).Click();
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, parcelNumber, "Tax Details", driver, "AR", "Washington");
                    string Year = "", Book = "", Location = "", TaxpayerName = "", URL = "", Status = "";

                    IWebElement SelectOption = driver.FindElement(By.XPath("/html/body/div[2]/table/tbody"));
                    IList<IWebElement> Select = SelectOption.FindElements(By.TagName("tr"));
                    IList<IWebElement> TD;
                    IList<IWebElement> CurrentTaxHistoryTA;
                    int J = 0;
                    foreach (IWebElement Op in Select)
                    {
                        TD = Op.FindElements(By.TagName("td"));
                        foreach (IWebElement Ro in TD)
                        {
                            CurrentTaxHistoryTA = Ro.FindElements(By.TagName("a"));
                            if (J == 0 && CurrentTaxHistoryTA.Count != 0)
                            {
                                URL = CurrentTaxHistoryTA[0].GetAttribute("href");
                                break;
                            }
                        }
                        if (TD.Count != 0 && !Op.Text.Contains("Parcel #"))
                        {
                            parcelNumber = TD[0].Text;
                            Year = TD[1].Text;
                            ownername = TD[2].Text;
                            Book = TD[3].Text;
                            PropertyAddress = TD[4].Text;
                            Location = TD[5].Text;
                            TaxpayerName = TD[6].Text;

                            if (J == 0)
                            {
                                Status = Book;
                                J++;
                            }

                            string History = Year + "~" + ownername + "~" + Book + "~" + PropertyAddress + "~" + Location + "~" + TaxpayerName;
                            gc.insert_date(orderNumber, parcelNumber, 500, History, 1, DateTime.Now);

                        }

                    }

                    //driver.Navigate().GoToUrl(URL);

                    string TaxYear = "", PaidBy = "", Exemption = "", TotalTax = "", PaidAmount = "", TaxDue = "";
                    gc.CreatePdf(orderNumber, parcelNumber, "Tax History", driver, "AR", "Washington");

                    // TaxYear = driver.FindElement(By.XPath("/html/body/div[2]/table[1]/tbody/tr[3]/td[2]")).Text;
                    string CurrentYear = DateTime.Now.Year.ToString();


                    IWebElement CurrenttaxTB = driver.FindElement(By.XPath("/html/body/div[2]/table/tbody"));
                    IList<IWebElement> CurrenttaxTR = CurrenttaxTB.FindElements(By.TagName("tr"));
                    IList<IWebElement> CurrenttaxTD;

                    foreach (IWebElement row1 in CurrenttaxTR)
                    {
                        CurrenttaxTD = row1.FindElements(By.TagName("td"));
                        if (CurrenttaxTD.Count != 0 && !row1.Text.Contains("Taxpayer Name"))
                        {
                            TaxYear = CurrenttaxTD[1].Text;
                            if (CurrentYear == TaxYear)
                            {
                                CurrenttaxTD[0].Click();
                                break;
                            }
                            else if ((Convert.ToInt32(CurrentYear) - 1) == Convert.ToInt32(TaxYear))
                            {
                                CurrenttaxTD[0].Click();
                                break;
                            }

                            else if ((Convert.ToInt32(CurrentYear) - 2) == Convert.ToInt32(TaxYear))
                            {
                                CurrenttaxTD[0].Click();
                                break;
                            }


                        }
                    }
                    gc.CreatePdf(orderNumber, parcelNumber, "Current Tax History ", driver, "AR", "Washington");
                    string[] linesName1 = TaxYear.Split(stringSeparators1, StringSplitOptions.None);
                    TaxYear = linesName1[0];


                    PaidBy = driver.FindElement(By.XPath("/html/body/div[2]/table[1]/tbody/tr[7]/td[2]")).Text.Replace("\r\n", " ");
                    Exemption = driver.FindElement(By.XPath("/html/body/div[2]/table[1]/tbody/tr[13]/td[2]")).Text;
                    try
                    {
                        TotalTax = driver.FindElement(By.XPath("/html/body/div[2]/table[1]/tbody/tr[14]/td[2]")).Text;
                        PaidAmount = driver.FindElement(By.XPath("/html/body/div[2]/table[1]/tbody/tr[15]/td[2]")).Text;
                        TaxDue = driver.FindElement(By.XPath("/html/body/div[2]/table[1]/tbody/tr[16]/td[2]/strong")).Text;
                    }
                    catch { }

                    PropertyType = driver.FindElement(By.XPath("/html/body/div[2]/table[1]/tbody/tr[5]/td[2]")).Text;
                    PropertyAddress = driver.FindElement(By.XPath("/html/body/div[2]/table[1]/tbody/tr[8]/td[2]")).Text;
                    ownername = driver.FindElement(By.XPath("/html/body/div[2]/table[1]/tbody/tr[6]/td[2]")).Text;

                    string TaxInfo = TaxYear + "~" + Status + "~" + PropertyType + "~" + ownername + "~" + PaidBy + "~" + PropertyAddress + "~" + Exemption + "~" + TotalTax + "~" + PaidAmount + "~" + TaxDue;
                    gc.insert_date(orderNumber, parcelNumber, 501, TaxInfo, 1, DateTime.Now);

                    string TaxType = "", TaxDescription = "", District = "", Exempt = "", AssessedValue = "", TaxOwed = "", TaxPaid = "", Balance = "";


                    IWebElement CurrentTaxHistoryTB1 = driver.FindElement(By.XPath("/html/body/div[2]/table[3]/tbody"));
                    IList<IWebElement> CurrentTaxHistoryTR1 = CurrentTaxHistoryTB1.FindElements(By.TagName("tr"));
                    IList<IWebElement> CurrentTaxHistoryTD1;

                    foreach (IWebElement row1 in CurrentTaxHistoryTR1)
                    {


                        CurrentTaxHistoryTD1 = row1.FindElements(By.TagName("td"));

                        if (CurrentTaxHistoryTD1.Count != 0 && !row1.Text.Contains("Tax Type") && CurrentTaxHistoryTD1.Count == 8)
                        {
                            TaxType = CurrentTaxHistoryTD1[0].Text;
                            TaxDescription = CurrentTaxHistoryTD1[1].Text;
                            District = CurrentTaxHistoryTD1[2].Text;
                            Exempt = CurrentTaxHistoryTD1[3].Text;
                            AssessedValue = CurrentTaxHistoryTD1[4].Text;
                            TaxOwed = CurrentTaxHistoryTD1[5].Text;
                            TaxPaid = CurrentTaxHistoryTD1[6].Text;
                            Balance = CurrentTaxHistoryTD1[7].Text;


                            string TaxDis = TaxType + "~" + TaxDescription + "~" + District + "~" + Exempt + "~" + AssessedValue + "~" + TaxOwed + "~" + TaxPaid + "~" + Balance;
                            gc.insert_date(orderNumber, parcelNumber, 503, TaxDis, 1, DateTime.Now);

                        }
                        else if (CurrentTaxHistoryTD1.Count == 4)
                        {
                            TaxType = CurrentTaxHistoryTD1[0].Text;
                            TaxDescription = "";
                            District = "";
                            Exempt = "";
                            AssessedValue = "";
                            TaxOwed = CurrentTaxHistoryTD1[1].Text;
                            TaxPaid = CurrentTaxHistoryTD1[2].Text;
                            Balance = CurrentTaxHistoryTD1[3].Text;


                            string TaxDis = TaxType + "~" + TaxDescription + "~" + District + "~" + Exempt + "~" + AssessedValue + "~" + TaxOwed + "~" + TaxPaid + "~" + Balance;
                            gc.insert_date(orderNumber, parcelNumber, 503, TaxDis, 1, DateTime.Now);
                        }
                    }

                    string Receipt = "", ReceiptDate = "", CashAmt = "", CheckAmt = "", CreditAmt = "";


                    IWebElement CurrentTaxHistoryTB = driver.FindElement(By.XPath("/html/body/div[2]/table[2]/tbody"));
                    IList<IWebElement> CurrentTaxHistoryTR = CurrentTaxHistoryTB.FindElements(By.TagName("tr"));
                    IList<IWebElement> CurrentTaxHistoryTD;


                    foreach (IWebElement row1 in CurrentTaxHistoryTR)
                    {
                        CurrentTaxHistoryTD = row1.FindElements(By.TagName("td"));
                        if (CurrentTaxHistoryTD.Count != 0 && !row1.Text.Contains("Book"))
                        {
                            Receipt = CurrentTaxHistoryTD[0].Text;
                            Status = CurrentTaxHistoryTD[1].Text;
                            TaxYear = CurrentTaxHistoryTD[2].Text;
                            ReceiptDate = CurrentTaxHistoryTD[3].Text;
                            CashAmt = CurrentTaxHistoryTD[4].Text;
                            CheckAmt = CurrentTaxHistoryTD[5].Text;
                            CreditAmt = CurrentTaxHistoryTD[6].Text;
                            Total = CurrentTaxHistoryTD[7].Text;

                            string TaxPay = Receipt + "~" + Status + "~" + TaxYear + "~" + ReceiptDate + "~" + CashAmt + "~" + CheckAmt + "~" + CreditAmt + "~" + Total;
                            gc.insert_date(orderNumber, parcelNumber, 504, TaxPay, 1, DateTime.Now);

                        }
                    }

                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");

                    gc.insert_TakenTime(orderNumber, "AR", "Washington", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);


                    driver.Quit();
                    gc.mergpdf(orderNumber, "AR", "Washington");
                    // gc.MMREM_Template(orderNumber, parcelNumber, "", driver, "AR", "Washington", "149", "");
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