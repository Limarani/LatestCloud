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
using OpenQA.Selenium.Firefox;
using System.Diagnostics;
using OpenQA.Selenium.Remote;
using System.Collections.Generic;
using System;

namespace ScrapMaricopa.Scrapsource
{
    public class Webdriver_OKTulsa
    {
        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        private TimeSpan timeOutInSeconds;
        MySqlParameter[] mParam;
        string outputPath = "";

        public string FTP_OKTulsa(string houseno, string sname, string direction, string sttype, string parcelNumber, string searchType, string accountnumber, string orderNumber, string ownername, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;

            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
            sname = sname + sttype;
            var driverService = PhantomJSDriverService.CreateDefaultService();
            IJavaScriptExecutor js = driver as IJavaScriptExecutor;

            driverService.HideCommandPromptWindow = true;
            using (driver = new PhantomJSDriver())
            {
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");
                    if (searchType == "titleflex")
                    {
                        string address = houseno + " " + direction + " " + sname + " " + accountnumber;
                        gc.TitleFlexSearch(orderNumber, parcelNumber, "", address, "OK", "Tulsa");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_OKTulsa"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }
                    driver.Navigate().GoToUrl("http://www.assessor.tulsacounty.org/assessor-property-search.php");
                    try
                    {
                        driver.FindElement(By.XPath("//*[@id='content']/form/button[1]")).SendKeys(Keys.Enter);
                    }
                    catch { }

                    if (searchType == "address")
                    {

                        IWebElement add = driver.FindElement(By.XPath("//*[@id='srchaddr']/label"));
                        add.Click();
                        driver.FindElement(By.Id("streetno")).SendKeys(houseno);
                        driver.FindElement(By.XPath("//*[@id='propertysearches']/div[2]/p[1]/select[1]")).SendKeys(direction);

                        driver.FindElement(By.Id("streetname")).SendKeys(sname);
                        driver.FindElement(By.Id("bttnaddr")).SendKeys(Keys.Enter);
                        gc.CreatePdf_WOP(orderNumber, "Address search result", driver, "OK", "Tulsa");
                        try
                        {
                            string mul = driver.FindElement(By.XPath("//*[@id='pickone_info']")).Text;
                            mul = WebDriverTest.After(mul, "to");
                            mul = WebDriverTest.Before(mul, "of").Trim();

                            if ((mul != "1") && (mul != "0"))
                            {
                                //multi parcel
                                IWebElement tbmulti = driver.FindElement(By.XPath("//*[@id='pickone']/tbody"));
                                IList<IWebElement> TRmulti = tbmulti.FindElements(By.TagName("tr"));

                                IList<IWebElement> TDmulti;
                                foreach (IWebElement row in TRmulti)
                                {
                                    TDmulti = row.FindElements(By.TagName("td"));
                                    if (TDmulti.Count != 0)
                                    {
                                        string multi = TDmulti[0].Text + "~" + TDmulti[1].Text + "~" + TDmulti[2].Text + "~" + TDmulti[3].Text + "~" + TDmulti[4].Text + "~" + TDmulti[5].Text;
                                        gc.insert_date(orderNumber, TDmulti[0].Text, 109, multi, 1, DateTime.Now);
                                    }
                                }

                                gc.CreatePdf_WOP(orderNumber, "Multiparcel Address Search", driver, "OK", "Tulsa");
                                HttpContext.Current.Session["multiparcel_Tulsa"] = "Yes";
                                if (TRmulti.Count > 25)
                                {
                                    HttpContext.Current.Session["multiparcel_Tulsa_count"] = "Maximum";
                                }
                                driver.Quit();
                                return "MultiParcel";
                            }
                        }

                        catch { }
                    }

                    else if (searchType == "parcel")
                    {
                        driver.FindElement(By.XPath("//*[@id='srchprcl']")).Click();
                        driver.FindElement(By.Id("parcel")).SendKeys(parcelNumber);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel search", driver, "OK", "Tulsa");
                        driver.FindElement(By.Id("bttnprcl")).SendKeys(Keys.Enter);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel search result", driver, "OK", "Tulsa");
                    }

                    else if (searchType == "block")
                    {
                        driver.FindElement(By.XPath("//*[@id='srchacct']/label")).Click();
                        driver.FindElement(By.Id("account")).SendKeys(accountnumber);
                        gc.CreatePdf_WOP(orderNumber, "Account Number search", driver, "OK", "Tulsa");
                        driver.FindElement(By.Id("bttnacct")).SendKeys(Keys.Enter);
                        gc.CreatePdf_WOP(orderNumber, "Account Number result search", driver, "OK", "Tulsa");
                    }

                    else if (searchType == "ownername")
                    {
                        string[] on = ownername.Split(' ');
                        string first = on[0];
                        string second = on[1];
                        //ownername = second + " " + first;

                        driver.FindElement(By.XPath("//*[@id='srchprsn']/label")).Click();
                        driver.FindElement(By.Id("ln")).SendKeys(first);
                        driver.FindElement(By.Id("fn")).SendKeys(second);
                        gc.CreatePdf_WOP(orderNumber, "Owner Name search", driver, "OK", "Tulsa");
                        driver.FindElement(By.Id("bttnprsn")).SendKeys(Keys.Enter);
                        gc.CreatePdf_WOP(orderNumber, "Owner Name search result", driver, "OK", "Tulsa");

                        try
                        {
                            string mul = driver.FindElement(By.XPath("//*[@id='pickone_info']")).Text;
                            mul = WebDriverTest.After(mul, "to");
                            mul = WebDriverTest.Before(mul, "of").Trim();

                            if ((mul != "1") && (mul != "0"))
                            {
                                //multi parcel
                                IWebElement tbmulti = driver.FindElement(By.XPath("//*[@id='pickone']/tbody"));
                                IList<IWebElement> TRmulti = tbmulti.FindElements(By.TagName("tr"));

                                IList<IWebElement> TDmulti;
                                foreach (IWebElement row in TRmulti)
                                {
                                    TDmulti = row.FindElements(By.TagName("td"));
                                    if (TDmulti.Count != 0)
                                    {
                                        string multi = TDmulti[0].Text + "~" + TDmulti[1].Text + "~" + TDmulti[2].Text + "~" + TDmulti[3].Text + "~" + TDmulti[4].Text + "~" + TDmulti[5].Text;
                                        gc.insert_date(orderNumber, TDmulti[0].Text, 109, multi, 1, DateTime.Now);
                                    }
                                }
                            }

                            gc.CreatePdf_WOP(orderNumber, " Multi Parcel Owner Name search", driver, "OK", "Tulsa");
                            HttpContext.Current.Session["multiparcel_Tulsa"] = "Yes";

                            driver.Quit();
                            return "MultiParcel";
                        }

                        catch { }

                    }

                    try
                    {
                        IWebElement INodata = driver.FindElement(By.XPath("//*[@id='content']/p[4]"));
                        if (INodata.Text.Contains("No properties were found"))
                        {
                            HttpContext.Current.Session["Nodata_OKTulsa"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }

                    Thread.Sleep(4000);
                    //Assessment Information
                    string Parcel_No = "-", Account_No = "-", Owner_Name = "-", Property_Address = "-", Mailing_Address = "-", Legal_Description = "-", Year_Built = "-";
                    string fullqucikfacts = driver.FindElement(By.XPath("//*[@id='quick']/table/tbody")).Text;
                    //Parcel_No = driver.FindElement(By.XPath("//*[@id='quick']/table/tbody/tr[1]/td[2]")).Text;
                    Parcel_No = gc.Between(fullqucikfacts, "Parcel #", "Situs address").Trim();
                    //Account_No = driver.FindElement(By.XPath("//*[@id='quick']/table/tbody/tr[1]/td[2]")).Text;
                    Account_No = gc.Between(fullqucikfacts, "Account #", "Parcel #").Trim();
                    //Owner_Name = driver.FindElement(By.XPath("//*[@id='quick']/table/tbody/tr[4]/td[2]")).Text;
                    Owner_Name = gc.Between(fullqucikfacts, "Owner name", "Fair cash").Trim();
                    //Property_Address = driver.FindElement(By.XPath("//*[@id='quick']/table/tbody/tr[3]/td[2]/text()")).Text;
                    Property_Address = gc.Between(fullqucikfacts, "Situs address", "Owner name").Trim();
                    Legal_Description = GlobalClass.After(fullqucikfacts, "Legal description");
                    string legalmailtext = driver.FindElement(By.XPath("//*[@id='general']/table/tbody")).Text;
                    // Mailing_Address = driver.FindElement(By.XPath("//*[@id='general']/table/tbody/tr[3]/td[2]/text()[1]")).Text;
                    Mailing_Address = gc.Between(legalmailtext, "Owner mailing address", "Land area");
                    //Legal_Description = driver.FindElement(By.XPath("//*[@id='quick']/table/tbody/tr[7]/td[2]/text()[2]")).Text;

                    try
                    {
                        Year_Built = driver.FindElement(By.XPath("//*[@id='improvements']/table/tbody/tr/td[5]")).Text;
                    }
                    catch { }


                    if (Mailing_Address.Contains("\r\n"))
                    {
                        Mailing_Address = Mailing_Address.Replace("\r\n", "");
                    }

                    if (Legal_Description.Contains("\r\n"))
                    {
                        Legal_Description = Legal_Description.Replace("\r\n", "");
                    }
                    string prop = Account_No + "~" + Owner_Name + "~" + Property_Address + "~" + Mailing_Address + "~" + Legal_Description + "~" + Year_Built;
                    gc.insert_date(orderNumber, Parcel_No, 111, prop, 1, DateTime.Now);


                    //Assessment information
                    //insert year

                    string AssessedYear = "", MarketValue = "", Totaltaxablevalue = "", AssessmentRatio = "", GrossAssessed = "", Exemptions = "", NetAssessed = "", LandValue = "", Improvementsvalue = "", Homestead = "", AdditionalHomestead = "", SeniorValuationLimitation = "", Veteran = "";
                    IWebElement TBTax = driver.FindElement(By.XPath("//*[@id='tax']/table"));
                    IList<IWebElement> TRTax = TBTax.FindElements(By.TagName("tr"));
                    IList<IWebElement> THTax;
                    foreach (IWebElement row1 in TRTax)
                    {
                        THTax = row1.FindElements(By.TagName("th"));
                        if (THTax.Count != 0)
                        {
                            AssessedYear = THTax[1].Text + "~" + THTax[2].Text;
                        }
                        else
                        {
                            THTax = row1.FindElements(By.TagName("td"));
                            if (THTax.Count != 0 && THTax.Count != 1 && THTax.Count != 2)
                            {

                                MarketValue = THTax[1].Text + "~" + THTax[2].Text;
                                Totaltaxablevalue = THTax[1].Text + "~" + THTax[2].Text;
                                AssessmentRatio = THTax[1].Text + "~" + THTax[2].Text;
                                GrossAssessed = THTax[1].Text + "~" + THTax[2].Text;
                                Exemptions = THTax[1].Text + "~" + THTax[2].Text;
                                NetAssessed = THTax[1].Text + "~" + THTax[2].Text;



                            }

                        }


                    }
                    IWebElement TBTax1 = driver.FindElement(By.XPath("//*[@id='value']/table"));
                    IList<IWebElement> TRTax1 = TBTax1.FindElements(By.TagName("tr"));
                    IList<IWebElement> THTax1;
                    foreach (IWebElement row1 in TRTax1)
                    {
                        THTax1 = row1.FindElements(By.TagName("th"));
                        if (THTax1.Count != 0)
                        {
                            AssessedYear = THTax1[1].Text + "~" + THTax1[2].Text;
                        }
                        else
                        {
                            THTax1 = row1.FindElements(By.TagName("td"));
                            if (THTax1.Count != 0)
                            {


                                LandValue = THTax1[1].Text + "~" + THTax1[2].Text;
                                Improvementsvalue = THTax1[1].Text + "~" + THTax1[2].Text;

                            }

                        }


                    }
                    IWebElement TBTax2 = driver.FindElement(By.XPath("//*[@id='adjustments']/table"));
                    IList<IWebElement> TRTax2 = TBTax2.FindElements(By.TagName("tr"));
                    IList<IWebElement> THTax2;
                    foreach (IWebElement row1 in TRTax1)
                    {
                        THTax2 = row1.FindElements(By.TagName("th"));
                        if (THTax2.Count != 0)
                        {
                            AssessedYear = THTax2[1].Text + "~" + THTax2[2].Text;
                        }
                        else
                        {
                            THTax2 = row1.FindElements(By.TagName("td"));
                            if (THTax2.Count != 0)
                            {
                                Homestead = THTax2[1].Text + "~" + THTax2[2].Text;
                                AdditionalHomestead = THTax2[1].Text + "~" + THTax2[2].Text;
                                SeniorValuationLimitation = THTax2[1].Text + "~" + THTax2[2].Text;
                                Veteran = THTax2[1].Text + "~" + THTax2[2].Text;
                            }

                        }


                    }



                    var split1 = AssessedYear.Split('~');
                    var split2 = MarketValue.Split('~');
                    var split3 = Totaltaxablevalue.Split('~');
                    var split4 = AssessmentRatio.Split('~');
                    var split5 = GrossAssessed.Split('~');
                    var split6 = Exemptions.Split('~');
                    var split7 = NetAssessed.Split('~');
                    var split8 = LandValue.Split('~');
                    var split9 = Improvementsvalue.Split('~');
                    var split10 = Homestead.Split('~');
                    var split11 = AdditionalHomestead.Split('~');
                    var split12 = SeniorValuationLimitation.Split('~');
                    var split13 = Veteran.Split('~');
                    for (int I = 0; split1.Length > I; I++)
                    {

                        string AssessDetail = split1[I] + "~" + split2[I] + "~" + split3[I] + "~" + split4[I] + "~" + split5[I] + "~" + split6[I] + "~" + split7[I] + "~" + split8[I] + "~" + split9[I] + "~" + split10[I] + "~" + split11[I] + "~" + split12[I] + "~" + split13[I];
                        gc.insert_date(orderNumber, Parcel_No, 112, AssessDetail, 1, DateTime.Now);

                    }
                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");

                    //insert tax information of assessment
                    //IWebElement TBTax3 = driver.FindElement(By.XPath("//*[@id='tax']/table/tbody"));
                    //IList<IWebElement> TRTax3 = TBTax1.FindElements(By.TagName("tr"));
                    //IList<IWebElement> TDTax3;
                    //foreach (IWebElement row1 in TRTax3)
                    //{
                    //    if (!row1.Text.Contains("Tax rate") && !row1.Text.Contains("Estimated taxes") && !row1.Text.Contains("Most recent"))
                    //    {
                    //        TDTax3 = row1.FindElements(By.TagName("td"));
                    //        string t_val = TDTax3[0].Text + "~" + TDTax3[1].Text + "~" + TDTax3[2].Text;
                    //        gc.insert_date(orderNumber, Parcel_No, 112, t_val, 1, DateTime.Now);
                    //    }

                    //}

                    //Values
                    //IWebElement TBval = driver.FindElement(By.XPath("//*[@id='value']/table/tbody"));
                    //IList<IWebElement> TRVal = TBval.FindElements(By.TagName("tr"));
                    //IList<IWebElement> TDVal;
                    //foreach (IWebElement row1 in TRVal)
                    //{
                    //    TDVal = row1.FindElements(By.TagName("td"));
                    //    string val = TDVal[0].Text + "~" + TDVal[1].Text + "~" + TDVal[2].Text;
                    //    gc.insert_date(orderNumber, Parcel_No, 112, val, 1, DateTime.Now);
                    //}

                    //Exemptions claimed
                    //IWebElement TBexe = driver.FindElement(By.XPath("//*[@id='adjustments']/table/tbody"));
                    //IList<IWebElement> TRexe = TBexe.FindElements(By.TagName("tr"));
                    //IList<IWebElement> TDexe;
                    //foreach (IWebElement row1 in TRexe)
                    //{
                    //    TDexe = row1.FindElements(By.TagName("td"));
                    //    string exe = TDexe[0].Text + "~" + TDexe[1].Text + "~" + TDexe[2].Text;
                    //    gc.insert_date(orderNumber, Parcel_No, 112, exe, 1, DateTime.Now);
                    //}

                    //Tax Account Details
                    driver.Navigate().GoToUrl("http://www.treasurer.tulsacounty.org/");
                    Thread.Sleep(4000);
                    IWebElement iframe = driver.FindElement(By.XPath("/html/body/table/tbody/tr/td/table/tbody/tr/td/iframe"));
                    driver.SwitchTo().Frame(iframe);
                    IWebElement frame1 = driver.FindElement(By.XPath("/html/frameset/frameset/frame[1]"));
                    driver.SwitchTo().Frame(frame1);
                    driver.FindElement(By.XPath("/html/body/div[1]/div[6]")).Click();
                    driver.SwitchTo().DefaultContent();
                    Thread.Sleep(4000);
                    driver.SwitchTo().Frame(iframe);
                    IWebElement frame2 = driver.FindElement(By.XPath("/html/frameset/frameset/frame[2]"));
                    driver.SwitchTo().Frame("main_frame");
                    driver.FindElement(By.XPath("//*[@id='HyperLink1']")).SendKeys(Keys.Enter);
                    Thread.Sleep(4000);


                    if (Parcel_No.Contains("-"))
                    {
                        string[] s = Parcel_No.Split('-');
                        Parcel_No = s[0] + s[1] + s[2] + s[3];
                    }


                    driver.FindElement(By.XPath("//*[@id='tb_PN']")).SendKeys(Parcel_No);
                    gc.CreatePdf(orderNumber, Parcel_No, "ImputPassed TaxSearch", driver, "OK", "Tulsa");
                    driver.FindElement(By.XPath("//*[@id='Button2']")).SendKeys(Keys.Enter);
                    gc.CreatePdf(orderNumber, Parcel_No, "TaxSearch Result", driver, "OK", "Tulsa");
                    string Parcel_ID = "-", t_Owner_Name = "-", T_Property_Address = "-", Tax_Year = "-", Tax_Type = "-", Tax_Amount = "-", Tax_Balance = "-", Interest = "-", Costs = "-", Total_Due = "-", Paid_Date = "-", Tax_Amount_Paid = "-", Interest_Paid = "-", Costs_Paid = "-", Total_Paid = "-", Reference_Number = "-", Paid_By = "-", Taxing_Authority = "-";
                    IWebElement TBtx = driver.FindElement(By.XPath("/html/body/form/div[3]/table[2]/tbody/tr[1]/td/table/tbody"));
                    IList<IWebElement> TRtx = TBtx.FindElements(By.TagName("tr"));
                    int count = TRtx.Count;
                    for (int a = 2; a <= count; a++)
                    {
                        IWebElement tx = driver.FindElement(By.XPath("/html/body/form/div[3]/table[2]/tbody/tr[1]/td/table/tbody/tr[" + a + "]/td[6]/a[1]"));
                        tx.SendKeys(Keys.Enter);
                        Thread.Sleep(3000);

                        Parcel_ID = driver.FindElement(By.XPath("//*[@id='form1']/div[3]/div[2]/table/tbody/tr/td/table[2]/tbody/tr[1]/td/table[1]/tbody/tr/td[2]/span")).Text;
                        t_Owner_Name = driver.FindElement(By.XPath("/html/body/form/div[3]/div[2]/table/tbody/tr/td/table[2]/tbody/tr[3]/td/table/tbody/tr[1]/td[1]/table/tbody/tr[3]/td/span")).Text;
                        T_Property_Address = driver.FindElement(By.XPath("/html/body/form/div[3]/div[2]/table/tbody/tr/td/table[2]/tbody/tr[3]/td/table/tbody/tr[2]/td/table[3]/tbody/tr/td[2]/span")).Text;
                        Tax_Year = driver.FindElement(By.XPath("/html/body/form/div[3]/div[2]/table/tbody/tr/td/table[1]/tbody/tr[1]/td/table/tbody/tr[2]/td/span")).Text;
                        var s = Tax_Year;
                        var firstSpaceIndex = s.IndexOf(" ");
                        var firstString = s.Substring(0, firstSpaceIndex); // INAGX4
                        var secondString = s.Substring(firstSpaceIndex + 1);
                        Tax_Year = firstString;
                        gc.CreatePdf(orderNumber, Parcel_No, "Detailed Tax Information" + Tax_Year, driver, "OK", "Tulsa");


                        try
                        {
                            string taxalert = driver.FindElement(By.XPath("//*[@id='lblmsgtxt']")).Text;
                            if (taxalert.Contains("Please call "))
                            {
                                gc.insert_date(orderNumber, Parcel_No, 1872, taxalert.Replace("/r/n",""), 1, DateTime.Now);
                                HttpContext.Current.Session["Taxalert_Tulsa"] = "Yes";
                                driver.Quit();
                                // return "'" + taxalert + "'";
                                return "Data Inserted Successfully";
                            }
                        }
                        catch
                        {

                        }
                        try
                        {
                            Tax_Type = secondString;
                            Tax_Amount = driver.FindElement(By.XPath("/html/body/form/div[3]/div[2]/table/tbody/tr/td/table[3]/tbody/tr[2]/td/table/tbody/tr[5]/td[2]/span")).Text;
                            Tax_Balance = driver.FindElement(By.XPath("/html/body/form/div[3]/div[2]/table/tbody/tr/td/table[3]/tbody/tr[2]/td/table/tbody/tr[9]/td[2]/span")).Text;
                            Interest = driver.FindElement(By.XPath("/html/body/form/div[3]/div[2]/table/tbody/tr/td/table[3]/tbody/tr[2]/td/table/tbody/tr[10]/td[2]/span")).Text;
                            Costs = driver.FindElement(By.XPath("/html/body/form/div[3]/div[2]/table/tbody/tr/td/table[3]/tbody/tr[2]/td/table/tbody/tr[11]/td[2]")).Text;
                            Total_Due = driver.FindElement(By.XPath("/html/body/form/div[3]/div[2]/table/tbody/tr/td/table[3]/tbody/tr[2]/td/table/tbody/tr[12]/td[2]/span")).Text;
                            string ta = driver.FindElement(By.XPath("/html/body/form/div[3]/div[1]/table/tbody/tr[2]/td")).Text;
                            string ta1 = driver.FindElement(By.XPath("/html/body/form/div[3]/div[1]/table/tbody/tr[3]/td")).Text;
                            string ta2 = driver.FindElement(By.XPath("/html/body/form/div[3]/div[1]/table/tbody/tr[4]")).Text;
                            Taxing_Authority = ta + ta1 + ta2;
                        }
                        catch
                        {

                        }
                        try
                        {
                            IWebElement TBtax = driver.FindElement(By.XPath("/html/body/form/div[3]/div[2]/table/tbody/tr/td/table[4]/tbody/tr[1]/td/table[2]/tbody"));
                            IList<IWebElement> TRtax = TBtax.FindElements(By.TagName("tr"));
                            IList<IWebElement> TDtax;
                            foreach (IWebElement row1 in TRtax)
                            {
                                if (!row1.Text.Contains("Seq#"))
                                {
                                    TDtax = row1.FindElements(By.TagName("td"));
                                    Paid_Date = TDtax[1].Text;
                                    Tax_Amount_Paid = TDtax[2].Text;
                                    Interest_Paid = TDtax[3].Text;
                                    Costs_Paid = TDtax[4].Text;
                                    Total_Paid = TDtax[5].Text;
                                    Reference_Number = TDtax[6].Text;
                                    Paid_By = TDtax[7].Text;
                                    string tax_auth = t_Owner_Name + "~" + T_Property_Address + "~" + Tax_Year + "~" + Tax_Type + "~" + Tax_Amount + "~" + Tax_Balance + "~" + Interest + "~" + Costs + "~" + Total_Due + "~" + Taxing_Authority + "~" + Paid_Date + "~" + Tax_Amount_Paid + "~" + Interest_Paid + "~" + Costs_Paid + "~" + Total_Paid + "~" + Reference_Number + "~" + Paid_By;
                                    gc.insert_date(orderNumber, Parcel_ID, 116, tax_auth, 1, DateTime.Now);
                                }

                            }
                        }
                        catch
                        {

                        }

                        driver.FindElement(By.XPath("/html/body/form/div[3]/div[2]/table/tbody/tr/td/table[2]/tbody/tr[1]/td/table[1]/tbody/tr/td[3]/span/a[1]")).SendKeys(Keys.Enter);
                        Thread.Sleep(4000);
                        gc.CreatePdf(orderNumber, Parcel_No, "Tax History details", driver, "OK", "Tulsa");
                        //Tax History
                        string Tax_Roll_Number = "-", Tax_Amount_T = "-", Tax_Balance_Due = "-", Interest_Due = "-", Cost = "-", Tax_Total_Due = "-";
                        IWebElement TBtax_history = driver.FindElement(By.XPath("/html/body/form/div[3]/table/tbody/tr/td/table[2]/tbody"));
                        IList<IWebElement> TRtax_history = TBtax_history.FindElements(By.TagName("tr"));
                        IList<IWebElement> TDtax_history;
                        foreach (IWebElement row1 in TRtax_history)
                        {
                            if (!row1.Text.Contains("Tax Roll Number"))
                            {
                                TDtax_history = row1.FindElements(By.TagName("td"));
                                Tax_Roll_Number = TDtax_history[0].Text;
                                Tax_Amount_T = TDtax_history[1].Text;
                                Tax_Balance_Due = TDtax_history[2].Text;
                                Interest_Due = TDtax_history[3].Text;
                                Cost = TDtax_history[4].Text;
                                Tax_Total_Due = TDtax_history[5].Text;
                                string tax_history = Tax_Year + "~" + Tax_Roll_Number + "~" + Tax_Amount_T + "~" + Tax_Balance_Due + "~" + Interest_Due + "~" + Cost + "~" + Tax_Total_Due;
                                gc.insert_date(orderNumber, Parcel_ID, 117, tax_history, 1, DateTime.Now);
                            }
                        }

                        driver.FindElement(By.XPath("//*[@id='HyperLink2']")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);

                        driver.FindElement(By.XPath("//*[@id='HyperLink2']")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);
                    }

                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "OK", "Tulsa", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    gc.mergpdf(orderNumber, "OK", "Tulsa");
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