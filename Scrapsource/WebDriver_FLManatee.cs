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
using System.Web.UI;

namespace ScrapMaricopa.Scrapsource
{
    public class WebDriver_FLManatee
    {
        string outputPath = "", Outparcel = ""; IWebElement element;
        string OwnerName = "-", Parcel_Address = "-", Roll = "-", Multiparcel = "-", check = "-";
        string T_R_S = "-", Property_Address = "-", Excemption = "-", DOR_Desciption = "-", Map_ID = "-", Legal_Description = "-", Tax_District = "-", Owner_Name = "-", Mailing_Address = "-", Property_Details = "-", Mailing_Address1 = "-", Mailing_Address2 = "-", Mailing_Address3 = "-", Mailing_Address4 = "-", owner1 = "-", owner2 = "-", owner3 = "-", Year_Built = "-";
        string Assessment_Details = "-", Output = "-", Non_Ad_Valorem_Assessments = "-";
        string TaxHistory_Details = "-", Due = "-", Paid_Date = "-", Paid_Amount = "-", Status = "-", Owner_Name_Location = "-", TaxHistory_Year = "-", Delinquent_Due = "-", Deliquent_Details = "-";
        string Tax_Status = "-", TaxInfo_Details = "-", Prior_Years_Due = "-", Exemptions = "-", EI_Correction = "-", TaxPaid_Date = "-", Receipt_Number = "-", Paid_By = "-", TaxPaid_Amount = "-";
        string Taxing_Authority = "-", Assessed_Value = "-", Taxing_Exemptions = "-", Taxable_Value = "-", Millage = "-", Taxes_Levied = "-", TaxDist_Details = "-", Tax_Type = "-", Code = "-";
        string Certificate_Number = "", Certificate_Buyer = "", Buyer1 = "", Buyer2 = "", Buyer_Address = "", Bid_Interest = "", Total = "";
        List<string> strParcelNo = new List<string>();
        List<string> link = new List<string>();
        List<string> Year = new List<string>();
        int billcount;
        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        MySqlParameter[] mParam;
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        IWebElement incrementmultiadrs; IWebElement clickcheckbox;
        public string FTP_FLManatee(string houseno, string sname, string stype, string city, string parcelNumber, string searchType, string orderNumber, string ownername, string unitno)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;

            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";

            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            //driver = new PhantomJSDriver();
            //  driver = new ChromeDriver();
            using (driver = new PhantomJSDriver())
            {
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");

                    if (searchType == "titleflex")
                    {
                        string address = houseno + " " + sname;
                        gc.TitleFlexSearch(orderNumber, parcelNumber, "", address, "FL", "Manatee");

                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Zero_manatee"] = "Zero";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }

                    if (searchType == "address")
                    {
                        driver.Navigate().GoToUrl("http://www.manateepao.com/ManateeFL/Search/Disclaimer.aspx?FromUrl=../search/commonsearch.aspx?mode=owner");
                        Thread.Sleep(2000);

                        driver.FindElement(By.Id("btAgree")).Click();
                        Thread.Sleep(2000);

                        driver.FindElement(By.XPath("/html/body/div/div[2]/div/nav/div[2]/ul/li[2]/a")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);

                        driver.FindElement(By.Id("inpNumber")).SendKeys(houseno);
                        driver.FindElement(By.Id("inpStreet")).SendKeys(sname);
                        try
                        {
                            driver.FindElement(By.Id("inpUnit")).SendKeys(unitno);
                        }
                        catch { }
                        try
                        {
                            var SelectStreetType = driver.FindElement(By.Name("inpSuffix1"));
                            var SelectAddressStreetType = new SelectElement(SelectStreetType);
                            SelectAddressStreetType.SelectByText(stype.ToUpper().ToString().Trim());
                        }
                        catch
                        { }
                        gc.CreatePdf_WOP(orderNumber, "Address search", driver, "FL", "Manatee");
                        driver.FindElement(By.Id("btSearch")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);

                        //MultiParcel
                        int Max = 0;
                        try
                        {
                            IWebElement INodata = driver.FindElement(By.XPath("//*[@id='frmMain']/table/tbody/tr/td/div/div/table[2]/tbody/tr/td/table/tbody/tr[3]/td/center/table[1]/tbody/tr[1]/td/center/div"));
                            if (INodata.Text.Contains("search did not find any records"))
                            {
                                HttpContext.Current.Session["Zero_manatee"] = "Zero";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                        string Nodata = driver.FindElement(By.XPath("//*[@id='frmMain']/table/tbody/tr/td/div/div/table[2]/tbody/tr/td[1]/table/tbody/tr[3]/td/center/table[1]/tbody/tr/td[2]/font/b")).Text;
                        check = driver.FindElement(By.XPath("/html/body/div/div[3]/section/div/form/table/tbody/tr/td/div/div/table[2]/tbody/tr/td[1]/table/tbody/tr[3]/td/center/table[1]/tbody/tr/td[3]")).Text;
                        if (check == "Displaying 1 - 1 of 1")
                        {
                            IWebElement singlesearch = driver.FindElement(By.XPath("//*[@id='searchResults']/tbody/tr[3]/td[2]"));
                            IJavaScriptExecutor js1 = driver as IJavaScriptExecutor;
                            js1.ExecuteScript("arguments[0].click();", singlesearch);
                            Thread.Sleep(2000);
                            Max++;
                        }
                        else
                        {
                            Max++;
                            multiparceladdree(orderNumber);
                            if (HttpContext.Current.Session["multiParcel_FLManatee"] != null && HttpContext.Current.Session["multiParcel_FLManatee"].ToString() == "Maximum")
                            {
                                gc.CreatePdf_WOP(orderNumber, "Multi Address search", driver, "FL", "Manatee");
                                driver.Quit();
                                return "MultiParcel";
                            }
                            if (HttpContext.Current.Session["multiParcel_FLManatee_Multicount"] != null && HttpContext.Current.Session["multiParcel_FLManatee_Multicount"].ToString() == "Maximum")
                            {
                                gc.CreatePdf_WOP(orderNumber, "Multi Address search", driver, "FL", "Manatee");
                                driver.Quit();
                                return "Maximum";
                                Max++;
                            }
                            if (Max == 0 && Nodata != "Click rows to view property details")
                            {
                                HttpContext.Current.Session["Zero_manatee"] = "Zero";
                                driver.Quit();
                                return "Zero";
                            }
                            //        IWebElement MultiTable = driver.FindElement(By.XPath("/html/body/div/div[3]/section/div/form/table/tbody/tr/td/div/div/table[2]/tbody/tr/td[1]/table/tbody/tr[3]/td/center/table[2]/tbody"));
                            //        IList<IWebElement> MultiTR = MultiTable.FindElements(By.TagName("tr"));
                            //        IList<IWebElement> MultiTD;

                            //        gc.CreatePdf_WOP(orderNumber, "Multi Address search", driver, "FL", "Manatee");
                            //        int maxCheck = 0;
                            //        foreach (IWebElement Multi in MultiTR)
                            //        {
                            //            if (maxCheck <= 25)
                            //            {
                            //                MultiTD = Multi.FindElements(By.TagName("td"));
                            //                if (MultiTD.Count != 0 && !Multi.Text.Contains("Parcel ID▲") && Multi.Text.Trim() != "")
                            //                {
                            //                    parcelNumber = MultiTD[1].Text.Replace("\r\n", " ").Trim();
                            //                    OwnerName = MultiTD[2].Text.Replace("\r\n", " ").Trim();
                            //                    Parcel_Address = MultiTD[3].Text.Replace("\r\n", " ").Trim();
                            //                    Roll = MultiTD[4].Text.Replace("\r\n", " ").Trim();

                            //                    Multiparcel = OwnerName + "~" + Parcel_Address + "~" + Roll;
                            //                    gc.insert_date(orderNumber, parcelNumber, 427, Multiparcel, 1, DateTime.Now);
                            //                }
                            //                maxCheck++;
                            //            }
                            //        }
                            //        HttpContext.Current.Session["multiParcel_FLManatee"] = "Yes";
                            //        if (MultiTR.Count > 25)
                            //        {
                            //            HttpContext.Current.Session["multiParcel_FLManatee_Multicount"] = "Maximum";
                            //        }
                            //        driver.Quit();
                            //        return "MultiParcel";
                            //    }
                            //}
                        }
                    }
                    else if (searchType == "parcel")
                    {
                        driver.Navigate().GoToUrl("http://www.manateepao.com/ManateeFL/Search/Disclaimer.aspx?FromUrl=../search/commonsearch.aspx?mode=owner");
                        Thread.Sleep(2000);

                        driver.FindElement(By.Id("btAgree")).Click();
                        Thread.Sleep(2000);

                        driver.FindElement(By.XPath("/html/body/div/div[2]/div/nav/div[2]/ul/li[3]/a")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);

                        driver.FindElement(By.Id("inpParid")).SendKeys(parcelNumber);

                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search", driver, "FL", "Manatee");
                        driver.FindElement(By.Id("btSearch")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);

                        //MultiParcel  
                        int Max = 0;
                        try
                        {
                            IWebElement INodata = driver.FindElement(By.XPath("//*[@id='frmMain']/table/tbody/tr/td/div/div/table[2]/tbody/tr/td/table/tbody/tr[3]/td/center/table[1]/tbody/tr[1]/td/center/div"));
                            if (INodata.Text.Contains("search did not find any records"))
                            {
                                HttpContext.Current.Session["Zero_manatee"] = "Zero";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                        check = driver.FindElement(By.XPath("/html/body/div/div[3]/section/div/form/table/tbody/tr/td/div/div/table[2]/tbody/tr/td[1]/table/tbody/tr[3]/td/center/table[1]/tbody/tr/td[3]")).Text;
                        if (check == "Displaying 1 - 1 of 1")
                        {
                            IWebElement singlesearch = driver.FindElement(By.XPath("//*[@id='searchResults']/tbody/tr[3]/td[2]"));
                            IJavaScriptExecutor js1 = driver as IJavaScriptExecutor;
                            js1.ExecuteScript("arguments[0].click();", singlesearch);
                            Thread.Sleep(8000);
                            Max++;
                        }
                        else
                        {
                            IWebElement MultiTable = driver.FindElement(By.XPath("/html/body/div/div[3]/section/div/form/table/tbody/tr/td/div/div/table[2]/tbody/tr/td[1]/table/tbody/tr[3]/td/center/table[2]/tbody"));
                            IList<IWebElement> MultiTR = MultiTable.FindElements(By.TagName("tr"));
                            IList<IWebElement> MultiTD;
                            gc.CreatePdf(orderNumber, parcelNumber, "Multi Parcel Search", driver, "FL", "Manatee");
                            int maxCheck = 0;
                            foreach (IWebElement Multi in MultiTR)
                            {
                                if (maxCheck <= 25)
                                {
                                    MultiTD = Multi.FindElements(By.TagName("td"));
                                    if (MultiTD.Count != 0 && !Multi.Text.Contains("Parcel ID▲") && Multi.Text.Trim() != "")
                                    {
                                        parcelNumber = MultiTD[1].Text.Replace("\r\n", " ").Trim();
                                        if (parcelNumber != "" && MultiTR.Count == 4)
                                        {
                                            strParcelNo.Add(parcelNumber);
                                        }

                                        OwnerName = MultiTD[2].Text.Replace("\r\n", " ").Trim();
                                        Parcel_Address = MultiTD[3].Text.Replace("\r\n", " ").Trim();
                                        Roll = MultiTD[4].Text.Replace("\r\n", " ").Trim();

                                        Multiparcel = OwnerName + "~" + Parcel_Address + "~" + Roll;
                                        gc.insert_date(orderNumber, parcelNumber, 427, Multiparcel, 1, DateTime.Now);
                                        maxCheck++;
                                        Max++;
                                    }
                                }

                            }
                            strParcelNo = strParcelNo.Distinct().ToList();
                            if (strParcelNo.Count == 1 && strParcelNo.Count != 0)
                            {
                                HttpContext.Current.Session["multiParcel_FLManatee"] = "";
                                IWebElement singlesearch = driver.FindElement(By.XPath("//*[@id='searchResults']/tbody/tr[3]/td[2]"));
                                IJavaScriptExecutor js1 = driver as IJavaScriptExecutor;
                                js1.ExecuteScript("arguments[0].click();", singlesearch);
                                Thread.Sleep(4000);
                                Max++;
                            }
                            else
                            {
                                driver.Quit();
                                HttpContext.Current.Session["multiParcel_FLManatee"] = "Yes";
                                if (MultiTR.Count > 25)
                                {
                                    HttpContext.Current.Session["multiParcel_FLManatee_Multicount"] = "Maximum";
                                }
                                return "MultiParcel";
                            }
                            if (Max == 0)
                            {
                                HttpContext.Current.Session["Zero_manatee"] = "Zero";
                                driver.Quit();
                                return "Zero";
                            }

                        }
                    }

                    else if (searchType == "ownername")
                    {
                        driver.Navigate().GoToUrl("http://www.manateepao.com/ManateeFL/Search/Disclaimer.aspx?FromUrl=../search/commonsearch.aspx?mode=owner");
                        Thread.Sleep(2000);


                        driver.FindElement(By.Id("btAgree")).Click();
                        Thread.Sleep(2000);

                        driver.FindElement(By.Id("inpOwner")).SendKeys(ownername);

                        gc.CreatePdf(orderNumber, parcelNumber, "Owner Search", driver, "FL", "Manatee");
                        driver.FindElement(By.Id("btSearch")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);

                        //MultiParcel
                        int Max = 0;
                        try
                        {
                            IWebElement INodata = driver.FindElement(By.XPath("//*[@id='frmMain']/table/tbody/tr/td/div/div/table[2]/tbody/tr/td/table/tbody/tr[3]/td/center/table[1]/tbody/tr[1]/td/center/div"));
                            if (INodata.Text.Contains("search did not find any records"))
                            {
                                HttpContext.Current.Session["Zero_manatee"] = "Zero";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                        check = driver.FindElement(By.XPath("/html/body/div/div[3]/section/div/form/table/tbody/tr/td/div/div/table[2]/tbody/tr/td[1]/table/tbody/tr[3]/td/center/table[1]/tbody/tr/td[3]")).Text;
                        if (check == "Displaying 1 - 1 of 1")
                        {
                            IWebElement singlesearch = driver.FindElement(By.XPath("//*[@id='searchResults']/tbody/tr[3]/td[2]"));
                            IJavaScriptExecutor js1 = driver as IJavaScriptExecutor;
                            js1.ExecuteScript("arguments[0].click();", singlesearch);
                            Thread.Sleep(2000);
                            Max++;
                        }
                        else
                        {
                            IWebElement MultiTable = driver.FindElement(By.XPath("/html/body/div/div[3]/section/div/form/table/tbody/tr/td/div/div/table[2]/tbody/tr/td[1]/table/tbody/tr[3]/td/center/table[2]/tbody"));
                            IList<IWebElement> MultiTR = MultiTable.FindElements(By.TagName("tr"));
                            IList<IWebElement> MultiTD;

                            gc.CreatePdf(orderNumber, parcelNumber, "Multi Owner Search", driver, "FL", "Manatee");
                            int maxCheck = 0;
                            foreach (IWebElement Multi in MultiTR)
                            {
                                if (maxCheck <= 25)
                                {
                                    MultiTD = Multi.FindElements(By.TagName("td"));
                                    if (MultiTD.Count != 0 && !Multi.Text.Contains("Parcel ID▲") && Multi.Text.Trim() != "")
                                    {
                                        parcelNumber = MultiTD[1].Text.Replace("\r\n", " ").Trim();
                                        OwnerName = MultiTD[2].Text.Replace("\r\n", " ").Trim();
                                        Parcel_Address = MultiTD[3].Text.Replace("\r\n", " ").Trim();
                                        Roll = MultiTD[4].Text.Replace("\r\n", " ").Trim();

                                        Multiparcel = OwnerName + "~" + Parcel_Address + "~" + Roll;
                                        gc.insert_date(orderNumber, parcelNumber, 427, Multiparcel, 1, DateTime.Now);
                                    }
                                    maxCheck++;
                                    Max++;
                                }
                            }
                            if (Max == 0)
                            {
                                HttpContext.Current.Session["Zero_manatee"] = "Zero";
                                driver.Quit();
                                return "No Data Found";
                            }
                            HttpContext.Current.Session["multiParcel_FLManatee"] = "Yes";
                            if (MultiTR.Count > 25)
                            {
                                HttpContext.Current.Session["multiParcel_FLManatee_Multicount"] = "Maximum";
                            }
                            driver.Quit();
                            return "MultiParcel";
                        }

                    }

                    //Property Details
                    Outparcel = driver.FindElement(By.XPath("//*[@id='ID Block']/tbody/tr[1]/td[2]")).Text;
                    T_R_S = driver.FindElement(By.XPath("//*[@id='ID Block']/tbody/tr[2]/td[2]")).Text;
                    Property_Address = driver.FindElement(By.XPath("//*[@id='ID Block']/tbody/tr[3]/td[2]")).Text;
                    Excemption = driver.FindElement(By.XPath("//*[@id='ID Block']/tbody/tr[5]/td[2]")).Text;
                    DOR_Desciption = driver.FindElement(By.XPath("//*[@id='ID Block']/tbody/tr[7]/td[2]")).Text;
                    try
                    {
                        Map_ID = driver.FindElement(By.XPath("//*[@id='ID Block']/tbody/tr[15]/td[2]")).Text;
                    }
                    catch
                    { }

                    try
                    {
                        Legal_Description = driver.FindElement(By.XPath("//*[@id='ID Block']/tbody/tr[22]/td[2]")).Text;
                    }
                    catch
                    { }

                    try
                    {
                        Tax_District = driver.FindElement(By.XPath("//*[@id='ID Block']/tbody/tr[29]/td[2]")).Text;
                    }
                    catch
                    { }

                    owner1 = driver.FindElement(By.XPath("/html/body/div/div[3]/section/div/form/div[3]/div/div/table/tbody/tr/td/table/tbody/tr[2]/td/div/div[2]/table[2]/tbody/tr[1]/td[2]")).Text;
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div/div[3]/section/div/form/div[3]/div/div/table/tbody/tr/td/table/tbody/tr[2]/td/div/div[2]/table[1]/tbody/tr/td[2]/table/tbody/tr/td/table/tbody/tr/td[3]/a")).Click();
                        Thread.Sleep(1000);
                        owner2 = driver.FindElement(By.XPath("/html/body/div/div[3]/section/div/form/div[3]/div/div/table/tbody/tr/td/table/tbody/tr[2]/td/div/table[4]/tbody/tr[1]/td[2]")).Text;
                        driver.FindElement(By.XPath("/html/body/div/div[3]/section/div/form/div[3]/div/div/table/tbody/tr/td/table/tbody/tr[2]/td/div/table[3]/tbody/tr/td[2]/table/tbody/tr/td/table/tbody/tr/td[1]/a")).Click();
                        Thread.Sleep(1000);
                    }
                    catch
                    { }
                    owner3 = driver.FindElement(By.XPath("//*[@id='Owners']/tbody/tr[3]/td[2]")).Text;
                    Owner_Name = owner1 + " " + "&" + " " + owner2 + "," + owner3;

                    Mailing_Address1 = driver.FindElement(By.XPath("//*[@id='Owners']/tbody/tr[4]/td[2]")).Text;
                    Mailing_Address2 = driver.FindElement(By.XPath("//*[@id='Owners']/tbody/tr[7]/td[2]")).Text;
                    Mailing_Address3 = driver.FindElement(By.XPath("//*[@id='Owners']/tbody/tr[8]/td[2]")).Text;
                    Mailing_Address4 = driver.FindElement(By.XPath("//*[@id='Owners']/tbody/tr[9]/td[2]")).Text;
                    Mailing_Address = Mailing_Address1 + "," + " " + Mailing_Address2 + "," + " " + Mailing_Address3 + " " + Mailing_Address4;

                    gc.CreatePdf(orderNumber, Outparcel, "Property Details", driver, "FL", "Manatee");
                    driver.FindElement(By.XPath("//*[@id='sidemenu']/li[4]/a")).Click();
                    Thread.Sleep(2000);
                    //*[@id="Total Value"]/tbody
                    try
                    {
                        IWebElement Yeartable = driver.FindElement(By.XPath("//*[@id='Residential']/tbody"));
                        IList<IWebElement> YearTR = Yeartable.FindElements(By.TagName("tr"));
                        IList<IWebElement> YearTD;

                        foreach (IWebElement Year in YearTR)
                        {
                            YearTD = Year.FindElements(By.TagName("td"));
                            if (YearTD.Count != 0 && Year.Text.Contains("Year Built"))
                            {
                                Year_Built = YearTD[1].Text;
                            }
                        }
                    }
                    catch { }
                    Property_Details = T_R_S + "~" + Property_Address + "~" + Excemption + "~" + DOR_Desciption + "~" + Map_ID + "~" + Legal_Description + "~" + Tax_District + "~" + Owner_Name + "~" + Mailing_Address + "~" + Year_Built;
                    gc.insert_date(orderNumber, Outparcel, 430, Property_Details, 1, DateTime.Now);


                    //Assement Details
                    driver.FindElement(By.XPath("//*[@id='sidemenu']/li[2]/a")).Click();
                    Thread.Sleep(2000);

                    gc.CreatePdf(orderNumber, Outparcel, "Assessment Details", driver, "FL", "Manatee");
                    IWebElement AssessmentTable = driver.FindElement(By.XPath("//*[@id='Total Value']/tbody"));
                    IList<IWebElement> AssessmentTR = AssessmentTable.FindElements(By.TagName("tr"));
                    IList<IWebElement> AssessmentTD;

                    List<string> Tax_Year = new List<string>();
                    List<string> Just_Land_Value = new List<string>();
                    List<string> Just_Improvement_Value = new List<string>();
                    List<string> Total_Just_Value = new List<string>();
                    List<string> New_Construction = new List<string>();
                    List<string> Addition_Value = new List<string>();
                    List<string> Demolition_Value = new List<string>();
                    List<string> Save_Our_Homes_Savings = new List<string>();
                    List<string> Non_Homestead_Cap_Savings = new List<string>();
                    List<string> Market_Value_of_Classified_Use_Land = new List<string>();
                    List<string> Classified_Use_Value = new List<string>();
                    List<string> Total_Assessed_Value = new List<string>();
                    List<string> Previous_Year_Just_Value = new List<string>();
                    List<string> Previous_Year_Assessed_Value = new List<string>();
                    List<string> Previous_Year_Cap_Value = new List<string>();

                    int i = 0;
                    foreach (IWebElement Assessment in AssessmentTR)
                    {
                        AssessmentTD = Assessment.FindElements(By.TagName("td"));
                        if (AssessmentTD.Count != 0 && !Assessment.Text.Contains("Latest Certified Values") && !Assessment.Text.Contains("Previous Year Values") && !Assessment.Text.Contains("**Values are not warranted and are subject to change until TRIM notices are mailed in August.") && Assessment.Text.Trim() != "")
                        {
                            if (i == 0)
                            {
                                Tax_Year.Add(AssessmentTD[1].Text);
                            }
                            else if (i == 1)
                            {
                                Just_Land_Value.Add(AssessmentTD[1].Text);
                            }
                            else if (i == 2)
                            {
                                Just_Improvement_Value.Add(AssessmentTD[1].Text);
                            }
                            else if (i == 3)
                            {
                                Total_Just_Value.Add(AssessmentTD[1].Text);
                            }
                            else if (i == 4)
                            {
                                New_Construction.Add(AssessmentTD[1].Text);
                            }
                            else if (i == 5)
                            {
                                Addition_Value.Add(AssessmentTD[1].Text);
                            }
                            else if (i == 6)
                            {
                                Demolition_Value.Add(AssessmentTD[1].Text);
                            }
                            else if (i == 7)
                            {
                                Save_Our_Homes_Savings.Add(AssessmentTD[1].Text);
                            }
                            else if (i == 8)
                            {
                                Non_Homestead_Cap_Savings.Add(AssessmentTD[1].Text);
                            }
                            else if (i == 9)
                            {
                                Market_Value_of_Classified_Use_Land.Add(AssessmentTD[1].Text);
                            }
                            else if (i == 10)
                            {
                                Classified_Use_Value.Add(AssessmentTD[1].Text);
                            }
                            else if (i == 11)
                            {
                                Total_Assessed_Value.Add(AssessmentTD[1].Text);
                            }
                            else if (i == 12)
                            {
                                Previous_Year_Just_Value.Add(AssessmentTD[1].Text);
                            }
                            else if (i == 13)
                            {
                                Previous_Year_Assessed_Value.Add(AssessmentTD[1].Text);
                            }
                            else if (i == 14)
                            {
                                Previous_Year_Cap_Value.Add(AssessmentTD[1].Text);
                            }
                            i++;
                        }
                    }

                    IWebElement AssessmentValoremTable = driver.FindElement(By.XPath("//*[@id='Total Value']/tbody"));
                    IList<IWebElement> AssessmentValoremTR = AssessmentValoremTable.FindElements(By.TagName("tr"));
                    IList<IWebElement> AssessmentValoremTD;

                    foreach (IWebElement AssessmentValorem in AssessmentValoremTR)
                    {
                        AssessmentValoremTD = AssessmentValorem.FindElements(By.TagName("td"));
                        if (AssessmentValoremTD.Count != 0 && !AssessmentValorem.Text.Contains("Levying Authority") && AssessmentValorem.Text.Trim() != "")
                        {
                            Non_Ad_Valorem_Assessments = AssessmentValoremTD[0].Text;
                            Output = AssessmentValoremTD[1].Text;
                        }
                    }
                    Assessment_Details = Tax_Year[0] + "~" + Just_Land_Value[0] + "~" + Just_Improvement_Value[0] + "~" + Total_Just_Value[0] + "~" + New_Construction[0] + "~" + Addition_Value[0] + "~" + Demolition_Value[0] + "~" + Save_Our_Homes_Savings[0] + "~" + Non_Homestead_Cap_Savings[0] + "~" + Market_Value_of_Classified_Use_Land[0] + "~" + Classified_Use_Value[0] + "~" + Total_Assessed_Value[0] + "~" + Previous_Year_Just_Value[0] + "~" + Previous_Year_Assessed_Value[0] + "~" + Previous_Year_Cap_Value[0] + "~" + Non_Ad_Valorem_Assessments + "~" + Output;
                    gc.insert_date(orderNumber, Outparcel, 434, Assessment_Details, 1, DateTime.Now);
                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");

                    //Tax Details
                    driver.Navigate().GoToUrl("https://secure.taxcollector.com/ptaxweb/?CFID=4847111&CFTOKEN=b9a8b911ad731be3-D0FED435-BD8C-6D9C-5F24C9B6CE24860E");
                    Thread.Sleep(2000);
                    driver.FindElement(By.Id("iAgree")).Click();
                    driver.FindElement(By.XPath("//*[@id='accountNumber']")).Click();

                    driver.FindElement(By.XPath("//*[@id='propertyidchoice']/div[3]/div[1]/div/input")).SendKeys(Outparcel);

                    gc.CreatePdf(orderNumber, Outparcel, "Tax Details", driver, "FL", "Manatee");
                    driver.FindElement(By.Id("propertySearchButtonId")).SendKeys(Keys.Enter);
                    Thread.Sleep(2000);
                    int K = 0;

                    //Tax History Details
                    try
                    {
                        IWebElement TaxHistoryTable = driver.FindElement(By.XPath("//*[@id='currentTableObject']/tbody"));
                        IList<IWebElement> TaxHistoryTR = TaxHistoryTable.FindElements(By.TagName("tr"));
                        IList<IWebElement> TaxHistoryTD;
                        IList<IWebElement> TaxHistoryTA;

                        gc.CreatePdf(orderNumber, Outparcel, "Tax Year Details", driver, "FL", "Manatee");
                        foreach (IWebElement TaxHistory in TaxHistoryTR)
                        {
                            TaxHistoryTD = TaxHistory.FindElements(By.TagName("td"));
                            TaxHistoryTA = TaxHistory.FindElements(By.TagName("a"));
                            if (TaxHistoryTD.Count != 0)
                            {
                                TaxHistory_Year = TaxHistoryTD[2].Text;
                                if (K <= 2)
                                {
                                    link.Add(TaxHistoryTA[2].GetAttribute("href"));
                                    K++;
                                }

                                Owner_Name_Location = TaxHistoryTD[4].Text;
                                Status = TaxHistoryTD[5].Text;
                                Paid_Amount = TaxHistoryTD[6].Text;
                                Paid_Date = TaxHistoryTD[7].Text;
                                Due = TaxHistoryTD[8].Text;
                            }
                            if (Status.Trim() != "Unpaid")
                            {
                                Delinquent_Due = "";
                            }
                            if (Due != "0.00" && Due != "Tax Deed Issued" && Due != "Tax Deed Redeemed")
                            {
                                Delinquent_Due = Due;
                                Due = "";
                                Deliquent_Details = TaxHistory_Year + "~" + Owner_Name_Location + "~" + Status + "~" + Paid_Amount + "~" + Paid_Date + "~" + Due + "~" + Delinquent_Due;
                                gc.insert_date(orderNumber, Outparcel, 436, Deliquent_Details, 1, DateTime.Now);
                            }
                            else
                            {
                                TaxHistory_Details = TaxHistory_Year + "~" + Owner_Name_Location + "~" + Status + "~" + Paid_Amount + "~" + Paid_Date + "~" + Due + "~" + Delinquent_Due;
                                gc.insert_date(orderNumber, Outparcel, 436, TaxHistory_Details, 1, DateTime.Now);
                            }
                        }
                    }
                    catch { }
                    //Tax Info details
                    int count = 0;
                    foreach (string url in link)
                    {
                        driver.Navigate().GoToUrl(url);
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, Outparcel, "Tax Info Details", driver, "FL", "Manatee");

                        IWebElement SelectOption = driver.FindElement(By.XPath("/html/body/div[1]/div[2]/form[1]/div[2]/div[2]/div[2]/span/span/select"));
                        IList<IWebElement> Select = SelectOption.FindElements(By.TagName("option"));

                        int Check = 0;
                        foreach (IWebElement Op in Select)
                        {

                            if (Check <= 2)
                            {
                                Year.Add(Op.Text);
                                Check++;
                            }
                        }

                        try
                        {
                            Certificate_Number = driver.FindElement(By.XPath("/html/body/div[1]/div[2]/form[1]/div[2]/div[2]/div[2]/span/table/tbody/tr[1]/td[2]")).Text;
                            Certificate_Buyer = driver.FindElement(By.XPath("/html/body/div[1]/div[2]/form[1]/div[2]/div[2]/div[2]/span/table/tbody/tr[2]/td[2]")).Text;
                            Buyer1 = driver.FindElement(By.XPath("/html/body/div[1]/div[2]/form[1]/div[2]/div[2]/div[2]/span/table/tbody/tr[4]/td")).Text;
                            Buyer2 = driver.FindElement(By.XPath("/html/body/div[1]/div[2]/form[1]/div[2]/div[2]/div[2]/span/table/tbody/tr[5]/td")).Text;

                            Buyer_Address = Buyer1 + Buyer2;

                            Bid_Interest = driver.FindElement(By.XPath("/html/body/div[1]/div[2]/form[1]/div[2]/div[2]/div[2]/span/table/tbody/tr[6]/td")).Text;
                            Bid_Interest = WebDriverTest.After(Bid_Interest, ": ");
                        }
                        catch
                        { }

                        Tax_Status = driver.FindElement(By.XPath("/html/body/div[1]/div[2]/form[1]/div[2]/div[3]/div[2]")).Text;
                        Tax_Status = WebDriverTest.After(Tax_Status, "STATUS:").Trim();

                        Prior_Years_Due = driver.FindElement(By.XPath("/html/body/div[1]/div[2]/form[1]/div[2]/div[4]/div[2]")).Text;
                        Prior_Years_Due = WebDriverTest.After(Prior_Years_Due, "PRIOR YEARS DUE:").Trim();

                        Exemptions = driver.FindElement(By.XPath("/html/body/div[1]/div[2]/form[1]/div[2]/div[5]/div[2]")).Text;
                        Exemptions = WebDriverTest.After(Exemptions, "EXEMPTIONS:").Trim();

                        EI_Correction = driver.FindElement(By.XPath("/html/body/div[1]/div[2]/form[1]/div[2]/div[5]/div[3]")).Text;
                        EI_Correction = WebDriverTest.After(EI_Correction, "EI CORRECTION:").Trim();


                        try
                        {
                            Tax_Type = driver.FindElement(By.XPath("/html/body/div[1]/div[2]/form[1]/div[2]/div[6]/div")).Text;

                            //Tax Distribution  
                            IWebElement TaxDistTable = driver.FindElement(By.XPath("/html/body/div[1]/div[2]/form[1]/div[2]/div[7]/div/table/tbody"));
                            IList<IWebElement> TaxDistTR = TaxDistTable.FindElements(By.TagName("tr"));
                            IList<IWebElement> TaxDistTD;

                            foreach (IWebElement TaxDist in TaxDistTR)
                            {

                                if (Tax_Type == "AD VALOREM TAX:")
                                {

                                    Tax_Type = "Ad Valorem Taxes";
                                }
                                TaxDistTD = TaxDist.FindElements(By.TagName("td"));
                                if (TaxDistTD.Count != 0)
                                {
                                    Taxing_Authority = TaxDistTD[0].Text;
                                    Assessed_Value = TaxDistTD[1].Text;
                                    Taxing_Exemptions = TaxDistTD[2].Text;
                                    Taxable_Value = TaxDistTD[3].Text;
                                    Millage = TaxDistTD[4].Text;
                                    Taxes_Levied = TaxDistTD[5].Text;

                                    TaxDist_Details = Year[count] + "~" + Taxing_Authority + "~" + Tax_Type + "~" + "-" + "~" + Assessed_Value + "~" + Taxing_Exemptions + "~" + Taxable_Value + "~" + Millage + "~" + Taxes_Levied;
                                    gc.insert_date(orderNumber, Outparcel, 447, TaxDist_Details, 1, DateTime.Now);
                                }
                            }

                            IWebElement TaxAdvaloremTable = driver.FindElement(By.XPath("/html/body/div[1]/div[2]/form[1]/div[2]/div[8]"));
                            IList<IWebElement> TaxAdvaloremTR = TaxAdvaloremTable.FindElements(By.TagName("div"));

                            foreach (IWebElement TaxAdvalorem in TaxAdvaloremTR)
                            {
                                if (TaxAdvaloremTR.Count != 0)
                                {
                                    Taxing_Authority = TaxAdvaloremTR[0].Text;
                                    Millage = TaxAdvaloremTR[1].Text;
                                    Taxes_Levied = TaxAdvaloremTR[2].Text;
                                }
                            }
                            TaxDist_Details = Year[count] + "~" + Taxing_Authority + "~" + Tax_Type + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + Millage + "~" + Taxes_Levied;
                            gc.insert_date(orderNumber, Outparcel, 447, TaxDist_Details, 1, DateTime.Now);

                            Tax_Type = driver.FindElement(By.XPath("/html/body/div[1]/div[2]/form[1]/div[2]/div[9]/div")).Text;

                            IWebElement TaxNonValoremTable = driver.FindElement(By.XPath("/html/body/div[1]/div[2]/form[1]/div[2]/div[10]/div/table/tbody"));
                            IList<IWebElement> TaxNonValoremTR = TaxNonValoremTable.FindElements(By.TagName("tr"));
                            IList<IWebElement> TaxNonValoremTD;

                            foreach (IWebElement TaxNonValorem in TaxNonValoremTR)
                            {

                                if (Tax_Type == "NON AD VALOREM TAX:")
                                {

                                    Tax_Type = "Non-Ad Valorem Assessments";
                                }

                                TaxNonValoremTD = TaxNonValorem.FindElements(By.TagName("td"));
                                if (TaxNonValoremTD.Count != 0)
                                {
                                    Code = TaxNonValoremTD[0].Text;
                                    Taxing_Authority = TaxNonValoremTD[1].Text;
                                    Taxes_Levied = TaxNonValoremTD[2].Text;

                                    TaxDist_Details = Year[count] + "~" + Taxing_Authority + "~" + Tax_Type + "~" + Code + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + Taxes_Levied;
                                    gc.insert_date(orderNumber, Outparcel, 447, TaxDist_Details, 1, DateTime.Now);
                                }
                            }

                            IWebElement TaxTotalTable = driver.FindElement(By.XPath("/html/body/div[1]/div[2]/form[1]/div[2]/div[11]"));
                            IList<IWebElement> TaxTotalTR = TaxTotalTable.FindElements(By.TagName("div"));

                            foreach (IWebElement TaxTotal in TaxTotalTR)
                            {
                                if (TaxTotalTR.Count != 0)
                                {
                                    Taxing_Authority = TaxTotalTR[0].Text;
                                    Taxes_Levied = TaxTotalTR[1].Text;
                                }
                            }
                            TaxDist_Details = Year[count] + "~" + Taxing_Authority + "~" + Tax_Type + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + Taxes_Levied;
                            gc.insert_date(orderNumber, Outparcel, 447, TaxDist_Details, 1, DateTime.Now);

                            IWebElement TaxToTable = driver.FindElement(By.XPath("/html/body/div[1]/div[2]/form[1]/div[2]/div[12]"));
                            IList<IWebElement> TaxToTR = TaxToTable.FindElements(By.TagName("div"));

                            foreach (IWebElement TaxTo in TaxToTR)
                            {
                                if (TaxToTR.Count != 0)
                                {
                                    Taxing_Authority = TaxToTR[0].Text;
                                    Taxes_Levied = TaxToTR[1].Text;
                                }
                            }
                            TaxDist_Details = Year[count] + "~" + Taxing_Authority + "~" + Tax_Type + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + Taxes_Levied;
                            gc.insert_date(orderNumber, Outparcel, 447, TaxDist_Details, 1, DateTime.Now);

                            IWebElement TaxTTable = driver.FindElement(By.XPath("/html/body/div[1]/div[2]/form[1]/div[2]/div[14]"));
                            IList<IWebElement> TaxTTR = TaxTTable.FindElements(By.TagName("div"));

                            foreach (IWebElement TaxT in TaxTTR)
                            {
                                if (TaxTTR.Count != 0)
                                {
                                    Taxing_Authority = TaxTTR[0].Text;
                                    Taxes_Levied = TaxTTR[1].Text;
                                }
                            }
                            TaxDist_Details = Year[count] + "~" + Taxing_Authority + "~" + Tax_Type + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + Taxes_Levied;
                            gc.insert_date(orderNumber, Outparcel, 447, TaxDist_Details, 1, DateTime.Now);

                        }
                        catch
                        { }

                        try
                        {
                            IWebElement DelTable = driver.FindElement(By.XPath("/html/body/div[1]/div[2]/form[1]/div[2]/div[13]/div/table/tbody"));
                            IList<IWebElement> DelTR = DelTable.FindElements(By.TagName("tr"));
                            IList<IWebElement> DelTD;

                            List<string> Interest = new List<string>();
                            List<string> Commission = new List<string>();
                            List<string> Advertising_Fee = new List<string>();
                            List<string> Auction_Fee = new List<string>();
                            List<string> Certificate_Interest = new List<string>();
                            List<string> Collector_Fee = new List<string>();

                            int j = 0;
                            foreach (IWebElement Del in DelTR)
                            {
                                DelTD = Del.FindElements(By.TagName("td"));
                                if (DelTD.Count != 0 && !Del.Text.Contains("post-purchase"))
                                {
                                    if (Del.Text.Contains("Interest") && !Del.Text.Contains("Certificate Interest"))
                                    {
                                        Interest.Add(DelTD[1].Text);
                                    }
                                    else if (Del.Text.Contains("Commission"))
                                    {
                                        Commission.Add(DelTD[1].Text);
                                    }
                                    else if (Del.Text.Contains("Advertising Fee"))
                                    {
                                        Advertising_Fee.Add(DelTD[1].Text);
                                    }
                                    else if (Del.Text.Contains("Auction Fee"))
                                    {
                                        Auction_Fee.Add(DelTD[1].Text);
                                    }
                                    else if (Del.Text.Contains("Certificate Interest") && !Del.Text.Contains("post-purchase"))
                                    {
                                        Certificate_Interest.Add(DelTD[1].Text);
                                    }
                                    else if (Del.Text.Contains("Collector Fee") && !Del.Text.Contains("post-purchase"))
                                    {
                                        Collector_Fee.Add(DelTD[1].Text);
                                    }
                                    j++;
                                }


                            }

                            Total = driver.FindElement(By.XPath("/html/body/div[1]/div[2]/form[1]/div[2]/div[14]/div[2]")).Text;

                            string Del_TaxInfo_Details = Year[count] + "~" + Tax_Status + "~" + Prior_Years_Due + "~" + Exemptions + "~" + EI_Correction + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + Certificate_Number + "~" + Certificate_Buyer + "~" + Buyer_Address + "~" + Bid_Interest + "~" + Interest[0] + "~" + Commission[0] + "~" + Advertising_Fee[0] + "~" + Auction_Fee[0] + "~" + Certificate_Interest[0] + "~" + Collector_Fee[0] + "~" + Total;
                            gc.insert_date(orderNumber, Outparcel, 445, Del_TaxInfo_Details, 1, DateTime.Now);
                        }
                        catch
                        { }
                        try
                        {
                            IWebElement TaxInfoTable = driver.FindElement(By.XPath("//*[@id='currentTableObject4']/tbody"));
                            IList<IWebElement> TaxInfoTR = TaxInfoTable.FindElements(By.TagName("tr"));
                            IList<IWebElement> TaxInfoTD;

                            gc.CreatePdf(orderNumber, Outparcel, "Tax Paid Details", driver, "FL", "Manatee");
                            foreach (IWebElement TaxInfo in TaxInfoTR)
                            {
                                TaxInfoTD = TaxInfo.FindElements(By.TagName("td"));
                                if (TaxInfoTD.Count != 0 && !TaxInfo.Text.Contains("Nothing found to display."))
                                {
                                    TaxPaid_Date = TaxInfoTD[0].Text;
                                    Receipt_Number = TaxInfoTD[1].Text;
                                    Paid_By = TaxInfoTD[2].Text;
                                    TaxPaid_Amount = TaxInfoTD[3].Text;

                                    TaxInfo_Details = Year[count] + "~" + Tax_Status + "~" + Prior_Years_Due + "~" + Exemptions + "~" + EI_Correction + "~" + TaxPaid_Date + "~" + Receipt_Number + "~" + Paid_By + "~" + TaxPaid_Amount + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-";
                                    gc.insert_date(orderNumber, Outparcel, 445, TaxInfo_Details, 1, DateTime.Now);
                                }
                            }
                        }
                        catch
                        { }


                        //Bill Download                  
                        try
                        {
                            String Parent_Window2 = driver.CurrentWindowHandle;

                            //Receipt
                            IWebElement Receipt = driver.FindElement(By.XPath("//*[@id='currentTableObject4']/tbody/tr/td[5]/a[1]"));
                            string URL = Receipt.GetAttribute("href");

                            if (billcount <= 2)
                            {
                                billcount++;
                                gc.downloadfile(URL, orderNumber, Outparcel, Year[count] + " Receipt", "FL", "Manatee");

                            }

                            //Summary
                            IWebElement Summary = driver.FindElement(By.XPath("//*[@id='currentTableObject4']/tbody/tr/td[5]/a[2]"));
                            string URL_Summary = Summary.GetAttribute("href");
                            if (billcount <= 2)
                            {
                                billcount++;
                                gc.downloadfile(URL_Summary, orderNumber, Outparcel, Year[count] + " Summary", "FL", "Manatee");
                            }

                            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                            driver.SwitchTo().Window(driver.WindowHandles.Last());
                            Thread.Sleep(2000);

                            driver.SwitchTo().Window(Parent_Window2);
                            Thread.Sleep(2000);
                        }

                        catch
                        { }

                        try
                        {
                            String Parent_Window1 = driver.CurrentWindowHandle;
                            try
                            {
                                if (Status.Trim() == "Paid")
                                {
                                    driver.FindElement(By.XPath("/html/body/div[1]/div[2]/form[1]/input[11]")).Click();
                                    Thread.Sleep(4000);
                                }
                            }
                            catch
                            { }

                            try
                            {
                                if (Status.Trim() == "Unpaid")
                                {
                                    driver.FindElement(By.XPath("/html/body/div[1]/div[2]/form[1]/input[12]")).Click();
                                    Thread.Sleep(4000);
                                }
                            }
                            catch
                            { }

                            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                            driver.SwitchTo().Window(driver.WindowHandles.Last());

                            if (billcount <= 2)
                            {
                                billcount++;
                                gc.CreatePdf(orderNumber, Outparcel, Year[count] + " Print Bill", driver, "FL", "Manatee");
                            }

                            driver.SwitchTo().Window(Parent_Window1);
                            Thread.Sleep(2000);
                        }
                        catch
                        { }
                        count++;
                    }

                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "FL", "Manatee", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    //megrge pdf files
                    gc.mergpdf(orderNumber, "FL", "Manatee");
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
        public void multiparceladdree(string orderNumber)
        {
            int p = 0;
           
            string parcel_number = "";
            IWebElement afteradsmultitable = driver.FindElement(By.XPath("//*[@id='searchResults']/tbody"));
            IList<IWebElement> afteradsmultirow = afteradsmultitable.FindElements(By.TagName("tr"));
            IList<IWebElement> afteradsmultiid;
            foreach (IWebElement aftermutiads in afteradsmultirow)
            {
                afteradsmultiid = aftermutiads.FindElements(By.TagName("td"));

                if (!aftermutiads.Text.Contains("Parcel ID▲") && afteradsmultiid.Count != 0 && aftermutiads.Text.Trim() != ""&& parcel_number.Trim()!= afteradsmultiid[1].Text.Trim())
                {
                    if (p == 0)
                    {
                        clickcheckbox = afteradsmultiid[0];
                        incrementmultiadrs = afteradsmultiid[1];
                    }
                    if (aftermutiads.Text.Contains(incrementmultiadrs.Text))
                    {
                        p++;
                    }
                    element = afteradsmultiid[1];
                   parcel_number = afteradsmultiid[1].Text;
                    string multiparcelresult = afteradsmultiid[2].Text + "~" + afteradsmultiid[3].Text;
                    gc.insert_date(orderNumber, parcel_number, 427, multiparcelresult, 1, DateTime.Now);
                }
            }
            if (p == 1 && afteradsmultirow.Count >= 4 && afteradsmultirow.Count < 27)
            {
                element.Click();
                Thread.Sleep(2000);
            }
            if (p >= 2 && p < 26)
            {

                IJavaScriptExecutor js1 = driver as IJavaScriptExecutor;
                js1.ExecuteScript("arguments[0].click();", incrementmultiadrs);
                Thread.Sleep(2000);
                //incrementmultiadrs.Click();
            }
            if (afteradsmultirow.Count > 26)
            {
                HttpContext.Current.Session["multiParcel_FLManatee_Multicount"] = "Maximum";
            }


        }
    }
}