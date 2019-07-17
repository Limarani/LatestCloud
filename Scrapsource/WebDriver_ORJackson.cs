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
    public class WebDriver_ORJackson
    {
        string Currentowner = "", Account = "", Situs_Address = "", Curnt_Own = "", Curnt_Accn = "", Curnt_Situs = "";
        string Parcel_ID = "", Map_Taxlot = "", Owner1 = "", Owner2 = "", Owner3 = "", Owner_Name = "", Pro_Address = "", Mail_Address = "", Tax_Code = "", Acrage = "", Pro_Class = "", Stat_Class = "", Acct_Sts = "", Tax_Sts = "", Property_Details = "", Year_Built = "";
        string Assessment3_details = "", AV11 = "", MAV11 = "", M511 = "", RMV11 = "", Type1 = "", Assessment2_details = "", AV1 = "", MAV1 = "", M51 = "", RMV1 = "", Acrage1 = "", Type = "", Assessment1_details = "", Max_Av = "", Pre_Max_SAV = "", Pre_Max_AV = "", Excemption_Value = "", M5 = "", RMV = "", Value_Source = "", Tax_Year = "";
        string Taxdistribution_details1 = "", Dis_Total1 = "", Non_Limited1 = "", Limited1 = "", District_id1 = "", Taxdistribution_details = "", Dis_Total = "", Non_Limited = "", Limited = "", Value = "", Tax_Rate = "", District = "", M5_Type = "", District_id = "";
        string CurrentTaxInfo = "", FilePathCurrentTaxInfo = "", textFromPage = "", pdftext = "", tableassess = "", a1 = "", newrow = "", newrow1 = "", newrow3 = "", newrow4 = "";
        string SupplementalURL = "", FilePathCurrentTaxInfo1 = "", pdftext1 = "", tableassess1 = "", a2 = "", newrow111 = "", newrow112 = "", tableassess123 = "", a123 = "", newrow123 = "", Pro_Address1 = "", Mail_Address1 = "";
        string TaxAithority_Details = "", Tax_Authority = "", Taxinfo_Details111 = "", TaxyTotalDue11 = "", TaxyAmount_Due11 = "", taxy_Year11 = "", pdftext2211 = "", FilePathCurrentTaxInfo2211 = "", CurrentTaxInfo2211 = "", Taxinfo_Details11 = "", TaxyTotalDue1 = "", TaxyAmount_Due1 = "", taxy_Year1 = "", pdftext221 = "", FilePathCurrentTaxInfo221 = "", CurrentTaxInfo221 = "", Taxinfo_Details1 = "", TaxyTotalDue = "", TaxyAmount_Due = "", taxy_Year = "", pdftext22 = "", FilePathCurrentTaxInfo22 = "", CurrentTaxInfo22 = "", ChkMulti = "", tableassess17 = "", newrow1127 = "";
        string Deliquent_Taxes16 = "", Deliquent_Taxes17 = "", Deliquent_Taxes1 = "", Deliquent_Taxes18 = "", Deliquent_Taxes2 = "";

        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());


        public string FTP_ORJackson(string streetno, string direction, string streetname, string city, string streettype, string unitnumber, string ownernm, string parcelNumber, string searchType, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            GlobalClass.sname = "OR";
            GlobalClass.cname = "Jackson";
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";

            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            using (driver = new PhantomJSDriver())
            {

                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");

                    if (searchType == "titleflex")
                    {
                        gc.TitleFlexSearch(orderNumber, "", ownernm, "", "OR", "Jackson");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_ORJackson"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }

                    if (searchType == "address")
                    {
                        driver.Navigate().GoToUrl("http://web.jacksoncounty.org/PDO/");
                        Thread.Sleep(3000);

                        IWebElement iframeElement = driver.FindElement(By.XPath("//*[@id='zzz']/frame[1]"));
                        driver.SwitchTo().Frame(iframeElement);
                        Thread.Sleep(2000);

                        IJavaScriptExecutor js = driver as IJavaScriptExecutor;
                        IWebElement AddressLinkSearch = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td[14]/span/a"));
                        js.ExecuteScript("arguments[0].click();", AddressLinkSearch);
                        Thread.Sleep(2000);

                        driver.SwitchTo().DefaultContent();
                        Thread.Sleep(1000);

                        IWebElement iframeElement1 = driver.FindElement(By.Name("tabMenuFrame"));
                        driver.SwitchTo().Frame(iframeElement1);
                        Thread.Sleep(2000);

                        IJavaScriptExecutor js1 = driver as IJavaScriptExecutor;
                        IWebElement AddressLinkSearch1 = driver.FindElement(By.XPath("/html/body/a"));
                        js1.ExecuteScript("arguments[0].click();", AddressLinkSearch1);
                        Thread.Sleep(2000);

                        driver.SwitchTo().DefaultContent();
                        Thread.Sleep(1000);

                        IWebElement iframeElement2 = driver.FindElement(By.Name("TextFrame"));
                        driver.SwitchTo().Frame(iframeElement2);
                        Thread.Sleep(2000);

                        IJavaScriptExecutor js2 = driver as IJavaScriptExecutor;
                        IWebElement AddressLinkSearch2 = driver.FindElement(By.XPath("/html/body/form/table[2]/tbody/tr[2]/td[1]/input"));
                        js2.ExecuteScript("arguments[0].click();", AddressLinkSearch2);
                        Thread.Sleep(2000);

                        var SelectAddress = driver.FindElement(By.XPath("/html/body/form/table[3]/tbody/tr[5]/td/select"));
                        var SelectAddressSearch = new SelectElement(SelectAddress);
                        SelectAddressSearch.SelectByIndex(0);
                        Thread.Sleep(1000);

                        driver.FindElement(By.XPath("/html/body/form/table[4]/tbody/tr/td[2]/input")).Clear();
                        driver.FindElement(By.XPath("/html/body/form/table[4]/tbody/tr[1]/td[2]/input")).SendKeys(streetno);
                        driver.FindElement(By.XPath("/html/body/form/table[4]/tbody/tr[2]/td[2]/input")).SendKeys(streetname);
                        gc.CreatePdf_WOP(orderNumber, "Address search", driver, "OR", "Jackson");
                        driver.FindElement(By.XPath("/html/body/form/table[5]/tbody/tr[6]/td/input[1]")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);

                        try
                        {
                            //MultiParcel
                            IList<IWebElement> HiddenElements = driver.FindElements(By.CssSelector("#headerTable"));
                            ChkMulti = HiddenElements[1].GetAttribute("textContent").Replace("\r\n\t\t\t\r\n\t\t\t\t\r\n\t\t\t\t\t", "");
                            ChkMulti = WebDriverTest.Before(ChkMulti, " found for ");
                        }
                        catch { }

                        if (ChkMulti == "1 Record")
                        {
                            try
                            {
                                IJavaScriptExecutor js3 = driver as IJavaScriptExecutor;
                                IWebElement AddressLinkSearch3 = driver.FindElement(By.XPath("//*[@id='tblLargeAccount']/tbody/tr[5]/td/a"));
                                js3.ExecuteScript("arguments[0].click();", AddressLinkSearch3);
                                Thread.Sleep(2000);
                            }
                            catch
                            { }

                            try
                            {
                                IJavaScriptExecutor js5 = driver as IJavaScriptExecutor;
                                IWebElement AddressLinkSearch5 = driver.FindElement(By.XPath("//*[@id='tblLargeAccount']/tbody/tr[8]/td[2]/a"));
                                js5.ExecuteScript("arguments[0].click();", AddressLinkSearch5);
                                Thread.Sleep(2000);
                            }
                            catch
                            { }
                        }

                        else
                        {
                            try
                            {
                                IWebElement MultiTable = driver.FindElement(By.XPath("//*[@id='tblLargeAccount']/tbody"));
                                gc.CreatePdf_Chrome(orderNumber, parcelNumber, "Multi Address Search", driver, "OR", "Jackson");
                                IList<IWebElement> MultiTR = MultiTable.FindElements(By.TagName("tr"));
                                IList<IWebElement> MultiTD;

                                foreach (IWebElement row2 in MultiTR)
                                {
                                    MultiTD = row2.FindElements(By.TagName("td"));

                                    if (MultiTD.Count != 0 && row2.Text.Contains("Owner") || row2.Text.Contains("Account #") || row2.Text.Contains("Situs Address"))
                                    {
                                        Curnt_Own = MultiTD[0].Text;
                                        if (Curnt_Own.Contains("Owner"))
                                        {
                                            Currentowner = MultiTD[1].Text;
                                        }

                                        Curnt_Accn = MultiTD[0].Text;
                                        if (Curnt_Accn.Contains("Account #"))
                                        {
                                            Account = MultiTD[1].Text;
                                        }

                                        Curnt_Situs = MultiTD[0].Text;
                                        if (Curnt_Situs.Contains("Situs Address"))
                                        {
                                            Situs_Address = MultiTD[1].Text;
                                        }
                                    }
                                    if (Currentowner != "" && Account != "" && Situs_Address != "")
                                    {
                                        string MultiData = Currentowner + "~" + Situs_Address;
                                        gc.insert_date(orderNumber, Account, 993, MultiData, 1, DateTime.Now);
                                        Currentowner = ""; Situs_Address = ""; Account = "";
                                    }

                                }

                                if (MultiTR.Count > 25)
                                {
                                    HttpContext.Current.Session["multiParcel_ORjackson_Multicount"] = "Maximum";
                                    driver.Quit();
                                    return "Maximum";
                                }
                                else if (MultiTR.Count <= 25)
                                {
                                    HttpContext.Current.Session["multiParcel_ORjackson"] = "Yes";
                                    driver.Quit();
                                    return "MultiParcel";
                                }

                            }
                            catch { }
                        }
                    }


                    if (searchType == "parcel")
                    {
                        driver.Navigate().GoToUrl("http://web.jacksoncounty.org/PDO/");
                        Thread.Sleep(2000);

                        IWebElement iframeElement = driver.FindElement(By.XPath("//*[@id='zzz']/frame[1]"));
                        driver.SwitchTo().Frame(iframeElement);
                        Thread.Sleep(2000);

                        IJavaScriptExecutor js = driver as IJavaScriptExecutor;
                        IWebElement AddressLinkSearch = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td[14]/span/a"));
                        js.ExecuteScript("arguments[0].click();", AddressLinkSearch);
                        Thread.Sleep(2000);

                        driver.SwitchTo().DefaultContent();
                        Thread.Sleep(1000);

                        IWebElement iframeElement1 = driver.FindElement(By.Name("tabMenuFrame"));
                        driver.SwitchTo().Frame(iframeElement1);
                        Thread.Sleep(2000);

                        IJavaScriptExecutor js1 = driver as IJavaScriptExecutor;
                        IWebElement AddressLinkSearch1 = driver.FindElement(By.XPath("/html/body/a"));
                        js1.ExecuteScript("arguments[0].click();", AddressLinkSearch1);
                        Thread.Sleep(2000);

                        driver.SwitchTo().DefaultContent();
                        Thread.Sleep(1000);

                        IWebElement iframeElement2 = driver.FindElement(By.Name("TextFrame"));
                        driver.SwitchTo().Frame(iframeElement2);
                        Thread.Sleep(2000);

                        IJavaScriptExecutor js2 = driver as IJavaScriptExecutor;
                        IWebElement AddressLinkSearch2 = driver.FindElement(By.XPath("/html/body/form/table[2]/tbody/tr[2]/td[2]/input"));
                        js2.ExecuteScript("arguments[0].click();", AddressLinkSearch2);
                        Thread.Sleep(2000);

                        var SelectParcel = driver.FindElement(By.XPath("/html/body/form/table[3]/tbody/tr[5]/td/select"));
                        var SelectParcelSearch = new SelectElement(SelectParcel);
                        SelectParcelSearch.SelectByIndex(0);
                        Thread.Sleep(1000);
                        driver.FindElement(By.XPath("/html/body/form/table[4]/tbody/tr/td[2]/input")).Clear();
                        driver.FindElement(By.XPath("/html/body/form/table[4]/tbody/tr/td[2]/input")).SendKeys(parcelNumber);

                        gc.CreatePdf(orderNumber, parcelNumber, "ParcelSearch", driver, "OR", "Jackson");
                        driver.FindElement(By.XPath("/html/body/form/table[5]/tbody/tr[6]/td/input[1]")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);

                        try
                        {
                            IJavaScriptExecutor js3 = driver as IJavaScriptExecutor;
                            IWebElement AddressLinkSearch3 = driver.FindElement(By.XPath("//*[@id='tblLargeAccount']/tbody/tr[5]/td/a"));
                            js3.ExecuteScript("arguments[0].click();", AddressLinkSearch3);
                            Thread.Sleep(2000);
                        }
                        catch
                        { }

                        try
                        {
                            IJavaScriptExecutor js4 = driver as IJavaScriptExecutor;
                            IWebElement AddressLinkSearch4 = driver.FindElement(By.XPath("//*[@id='tblLargeAccount']/tbody/tr[8]/td[2]/a"));
                            js4.ExecuteScript("arguments[0].click();", AddressLinkSearch4);
                            Thread.Sleep(2000);
                        }
                        catch
                        { }

                    }

                    try
                    {
                        IWebElement INodata = driver.FindElement(By.Id("tblBottomResults"));
                        if (INodata.Text.Contains("0 records found"))
                        {
                            HttpContext.Current.Session["Nodata_ORJackson"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }
                    Thread.Sleep(2000);
                    driver.SwitchTo().Window(driver.WindowHandles.Last());
                    Thread.Sleep(2000);

                    //Property Details
                    try
                    {
                        Parcel_ID = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[2]/td[1]/table/tbody/tr[2]/td[2]")).Text;
                        Map_Taxlot = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[2]/td[1]/table/tbody/tr[3]/td[2]")).Text;
                        Owner1 = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[2]/td[1]/table/tbody/tr[4]/td[2]")).Text;
                        try
                        {
                            Owner2 = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[2]/td[1]/table/tbody/tr[5]/td[2]")).Text;
                            Owner3 = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[2]/td[1]/table/tbody/tr[6]/td[2]")).Text;
                        }
                        catch
                        { }
                        Owner_Name = Owner1 + " , " + Owner2 + " , " + Owner3;
                        try
                        {
                            Pro_Address = driver.FindElement(By.XPath("//*[@id='showSitusTable10984491']/tbody/tr/td[1]")).Text;
                        }
                        catch
                        { }
                        try
                        {
                            Pro_Address1 = driver.FindElement(By.XPath("//*[@id='showSitusTable10417984']/tbody/tr/td[1]")).Text;
                        }
                        catch
                        { }
                        try
                        {
                            Pro_Address = driver.FindElement(By.XPath("//*[@id='showSitusTable10003137']/tbody/tr/td[1]")).Text;
                        }
                        catch
                        { }

                        try
                        {
                            Mail_Address = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[2]/td[1]/table/tbody/tr[9]/td[2]")).Text;
                        }
                        catch
                        { }
                        try
                        {
                            Mail_Address1 = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[2]/td[1]/table/tbody/tr[7]/td[2]")).Text;
                        }
                        catch
                        { }
                        try
                        {
                            Mail_Address1 = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[2]/td[1]/table/tbody/tr[8]/td[2]")).Text;
                        }
                        catch
                        { }
                        Tax_Code = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[2]/td[3]/table/tbody/tr[2]/td[2]")).Text;
                        Acrage = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[2]/td[3]/table/tbody/tr[3]/td[2]")).Text;
                        try
                        {
                            Pro_Class = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[2]/td[3]/table/tbody/tr[8]/td[2]")).Text;
                        }
                        catch
                        { }

                        try
                        {
                            Stat_Class = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[2]/td[3]/table/tbody/tr[9]/td[2]")).Text;
                        }
                        catch
                        { }

                        try
                        {
                            Acct_Sts = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[2]/td[3]/table/tbody/tr[14]/td[2]")).Text;
                        }
                        catch
                        { }

                        try
                        {
                            Tax_Sts = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[2]/td[3]/table/tbody/tr[15]/td[2]")).Text;
                        }
                        catch
                        { }

                        try
                        {
                            Year_Built = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[3]/td/table/tbody/tr[10]/td[3]")).Text;
                        }
                        catch
                        { }

                        Property_Details = Map_Taxlot + "~" + Owner_Name + "~" + Pro_Address + "~" + Pro_Address1 + "~" + Mail_Address + "~" + Mail_Address1 + "~" + Tax_Code + "~" + Acrage + "~" + Pro_Class + "~" + Stat_Class + "~" + Acct_Sts + "~" + Tax_Sts + "~" + Year_Built;
                        gc.CreatePdf(orderNumber, Parcel_ID, "Property Details", driver, "OR", "Jackson");
                        gc.insert_date(orderNumber, Parcel_ID, 891, Property_Details, 1, DateTime.Now);
                    }
                    catch
                    { }

                    //Assement Details1
                    IWebElement SelectOption = driver.FindElement(By.XPath("/html/body/table/tbody/tr[1]/td/table/tbody/tr/td[3]/select"));
                    IList<IWebElement> Select = SelectOption.FindElements(By.TagName("option"));
                    List<string> option = new List<string>();
                    int Check = 0;
                    foreach (IWebElement Op in Select)
                    {

                        if (Check <= 2)
                        {
                            option.Add(Op.Text.Replace("\r\n", "").Replace(" ", ""));
                            Check++;
                        }

                    }

                    foreach (string item in option)
                    {
                        var SelectAddress = driver.FindElement(By.XPath("/html/body/table/tbody/tr[1]/td/table/tbody/tr/td[3]/select"));
                        var SelectAddressTax = new SelectElement(SelectAddress);
                        SelectAddressTax.SelectByText(item);
                        Thread.Sleep(4000);

                        try
                        {
                            Tax_Year = driver.FindElement(By.XPath("//*[@id='toggleTableHref1']/span")).Text;
                            Tax_Year = WebDriverTest.After(Tax_Year, "( For Assessment Year ").Replace(" - Subject To Change )", "").Replace(" )", "");
                        }
                        catch
                        { }

                        driver.FindElement(By.XPath("//*[@id='toggleTableHref1']")).Click();
                        Thread.Sleep(2000);

                        try
                        {
                            IWebElement Assessment1TB = driver.FindElement(By.XPath("//*[@id='DetailTable']/tbody"));
                            IList<IWebElement> Assessment1TR = Assessment1TB.FindElements(By.TagName("tr"));
                            IList<IWebElement> Assessment1TD;

                            foreach (IWebElement Assessment1 in Assessment1TR)
                            {
                                Assessment1TD = Assessment1.FindElements(By.TagName("td"));
                                if (Assessment1TD.Count != 0 && !Assessment1.Text.Contains("Code Area") && !Assessment1.Text.Contains("49-01") && Assessment1TD[0].Text != "" && !Assessment1.Text.Contains("RMV") && !Assessment1.Text.Contains("49-03") && !Assessment1.Text.Contains("1-01"))
                                {
                                    Value_Source = Assessment1TD[1].Text;
                                    RMV = Assessment1TD[2].Text;
                                    M5 = Assessment1TD[3].Text;
                                    Excemption_Value = Assessment1TD[4].Text;
                                    Pre_Max_AV = Assessment1TD[5].Text;
                                    Pre_Max_SAV = Assessment1TD[6].Text;
                                    Max_Av = Assessment1TD[7].Text;

                                    Assessment1_details = Tax_Year + "~" + Value_Source + "~" + RMV + "~" + M5 + "~" + Excemption_Value + "~" + Pre_Max_AV + "~" + Pre_Max_SAV + "~" + Max_Av;
                                    gc.CreatePdf(orderNumber, Parcel_ID, "Assessment Details" + Tax_Year, driver, "OR", "Jackson");
                                    gc.insert_date(orderNumber, Parcel_ID, 963, Assessment1_details, 1, DateTime.Now);
                                }
                            }
                        }
                        catch
                        { }

                        try
                        {
                            IWebElement Assessment2TB = driver.FindElement(By.XPath("//*[@id='MarketTable']/tbody"));
                            IList<IWebElement> Assessment2TR = Assessment2TB.FindElements(By.TagName("tr"));
                            IList<IWebElement> Assessment2TD;

                            foreach (IWebElement Assessment2 in Assessment2TR)
                            {
                                Assessment2TD = Assessment2.FindElements(By.TagName("td"));
                                if (Assessment2TD.Count != 0 && !Assessment2.Text.Contains("Type") && !Assessment2.Text.Contains("Total:") && Assessment2TD[0].Text != "" && !Assessment2.Text.Contains("Value Summary Details") && !Assessment2.Text.Contains("Value History"))
                                {
                                    Type = Assessment2TD[1].Text;
                                    Acrage1 = Assessment2TD[2].Text;
                                    RMV1 = Assessment2TD[3].Text;
                                    M51 = Assessment2TD[4].Text;
                                    MAV1 = Assessment2TD[5].Text;
                                    AV1 = Assessment2TD[6].Text;

                                    Assessment2_details = Tax_Year + "~" + Type + "~" + Acrage1 + "~" + RMV1 + "~" + M51 + "~" + MAV1 + "~" + AV1;
                                    gc.insert_date(orderNumber, Parcel_ID, 964, Assessment2_details, 1, DateTime.Now);
                                }

                                if (Assessment2.Text.Contains("Total:"))
                                {
                                    Type1 = Assessment2TD[1].Text;
                                    RMV11 = Assessment2TD[2].Text;
                                    M511 = Assessment2TD[3].Text;
                                    MAV11 = Assessment2TD[4].Text;
                                    AV11 = Assessment2TD[5].Text;

                                    Assessment3_details = Tax_Year + "~" + Type1 + "~" + "" + "~" + RMV11 + "~" + M511 + "~" + MAV11 + "~" + AV11;
                                    gc.insert_date(orderNumber, Parcel_ID, 964, Assessment3_details, 1, DateTime.Now);
                                }
                            }
                        }
                        catch
                        { }
                        AssessmentTime = DateTime.Now.ToString("HH:mm:ss");


                        //try
                        //{
                        //    try
                        //    {                               
                        //        driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[2]/td[2]/table/tbody/tr[7]/td[2]/a")).Click();
                        //        Thread.Sleep(2000);
                        //    }
                        //    catch
                        //    { }

                        //    try
                        //    {
                        //        driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[2]/td[2]/table/tbody/tr[3]/td[2]/a")).Click();
                        //        Thread.Sleep(2000);

                        //        driver.SwitchTo().Window(driver.WindowHandles.Last());
                        //        Thread.Sleep(2000);

                        //        driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr/td/form/input[1]")).Click();
                        //        Thread.Sleep(3000);
                        //    }
                        //    catch
                        //    { }

                        //    try
                        //    {
                        //        driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[2]/td[2]/table/tbody/tr[16]/td[2]/a")).Click();
                        //        Thread.Sleep(4000);
                        //    }
                        //    catch
                        //    { }
                        //    try
                        //    {
                        //        driver.SwitchTo().Window(driver.WindowHandles.Last());
                        //        Thread.Sleep(2000);

                        //        driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[2]/td[2]/table/tbody/tr[13]/td[2]/a")).Click();
                        //        Thread.Sleep(4000);
                        //    }
                        //    catch
                        //    { }
                        //    driver.SwitchTo().Window(driver.WindowHandles.Last());
                        //    Thread.Sleep(2000);

                        //    try
                        //    {
                        //        IWebElement TaxdistributionTB = driver.FindElement(By.XPath("/html/body/table/tbody/tr[1]/td/table[2]/tbody"));
                        //        IList<IWebElement> TaxdistributionTR = TaxdistributionTB.FindElements(By.TagName("tr"));
                        //        IList<IWebElement> TaxdistributionTD;

                        //        foreach (IWebElement Taxdistribution in TaxdistributionTR)
                        //        {
                        //            TaxdistributionTD = Taxdistribution.FindElements(By.TagName("td"));
                        //            if (TaxdistributionTD.Count != 0 && !Taxdistribution.Text.Contains("Account") && !Taxdistribution.Text.Contains("DISTRICT ID") && TaxdistributionTD[0].Text != "" && !Taxdistribution.Text.Contains("TOTALS"))
                        //            {
                        //                District_id = TaxdistributionTD[0].Text;
                        //                M5_Type = TaxdistributionTD[1].Text;
                        //                District = TaxdistributionTD[2].Text;
                        //                Tax_Rate = TaxdistributionTD[3].Text;
                        //                Value = TaxdistributionTD[4].Text;
                        //                Limited = TaxdistributionTD[5].Text;
                        //                Non_Limited = TaxdistributionTD[6].Text;
                        //                Dis_Total = TaxdistributionTD[7].Text;

                        //                Taxdistribution_details = Tax_Year + "~" + District_id + "~" + M5_Type + "~" + District + "~" + Tax_Rate + "~" + Value + "~" + Limited + "~" + Non_Limited + "~" + Dis_Total;
                        //                gc.insert_date(orderNumber, Parcel_ID, 967, Taxdistribution_details, 1, DateTime.Now);
                        //            }

                        //            if (Taxdistribution.Text.Contains("TOTALS"))
                        //            {
                        //                District_id1 = TaxdistributionTD[0].Text;
                        //                Limited1 = TaxdistributionTD[1].Text;
                        //                Non_Limited1 = TaxdistributionTD[2].Text;
                        //                Dis_Total1 = TaxdistributionTD[3].Text;

                        //                Taxdistribution_details1 = Tax_Year + "~" + District_id1 + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + Limited1 + "~" + Non_Limited1 + "~" + Dis_Total1;
                        //                gc.CreatePdf(orderNumber, Parcel_ID, "Tax Distribution Details" + Tax_Year, driver, "OR", "Jackson");
                        //                gc.insert_date(orderNumber, Parcel_ID, 967, Taxdistribution_details1, 1, DateTime.Now);
                        //            }
                        //        }


                        //    }
                        //    catch
                        //    { }
                        //}
                        //catch
                        //{ }

                        //try
                        //{
                        //    driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr/td/table/tbody/tr/td/form/input[1]")).Click();
                        //    Thread.Sleep(2000);
                        //}
                        //catch
                        //{ }

                        //try
                        //{
                        //    driver.FindElement(By.XPath("/html/body/table[1]/tbody/tr/td/form/input[1]")).Click();
                        //    Thread.Sleep(2000);
                        //}
                        //catch
                        //{ }

                        //Tax Distribution Details
                        try
                        {
                            IWebElement ITaxDistribution = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[2]/td[2]/table/tbody/tr[6]/td[2]/a"));
                            string strTaxDistribution = ITaxDistribution.GetAttribute("href");
                            gc.downloadfile(strTaxDistribution, orderNumber, Parcel_ID, "ViewTaxBill", "OR", "Jackson");
                            Thread.Sleep(2000);

                            FilePathCurrentTaxInfo22 = gc.filePath(orderNumber, Parcel_ID) + "ViewTaxBill.pdf";
                            PdfReader reader22;
                            reader22 = new PdfReader(FilePathCurrentTaxInfo22);
                            String textFromPage22 = PdfTextExtractor.GetTextFromPage(reader22, 1);
                            System.Diagnostics.Debug.WriteLine("" + textFromPage22);

                            pdftext22 = textFromPage22;

                            tableassess123 = gc.Between(pdftext22, "Bonds", "Page").Trim();
                            string TaxYear = gc.Between(pdftext22, "Year", "Code Area").Trim();
                            string Exemption = gc.Between(pdftext22, "Exemption", "Dist").Trim();
                            if(Exemption !="")
                            {
                                newrow123 = TaxYear + "~" + Exemption + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "";
                                gc.insert_date(orderNumber, Parcel_ID, 967, newrow123, 1, DateTime.Now);
                            }
                           
                            string[] tableArray123 = tableassess123.Split('\n');
                            List<string> rowarray123 = new List<string>();
                            int m = 0, n = 0, o = 0, p = 0, q = 0;
                            int count123 = tableArray123.Length;
                            for (m = 0; m < count123; m++)
                            {
                                a123 = tableArray123[m].Replace(" ", "~");
                                string[] rowarray213 = a123.Split('~');
                                rowarray123.AddRange(rowarray213);
                                if (rowarray123.Count() > 5)
                                {
                                    string strDistCode = "", strDistNme = "", strRate = "", strGovt = "", strEducation = "", strBonds = "";
                                    for (int i = 0; i < rowarray123.Count(); i++)
                                    {
                                        if (i == 0)
                                        {
                                            strDistCode = rowarray123[i];
                                        }
                                        else if (i == rowarray123.Count() - 1)
                                        {
                                            strBonds = rowarray123[i];
                                        }
                                        else if (i == rowarray123.Count() - 2)
                                        {
                                            strEducation = rowarray123[i];
                                        }
                                        else if (i == rowarray123.Count() - 3)
                                        {
                                            strGovt = rowarray123[i];
                                        }
                                        else if (i == rowarray123.Count() - 4)
                                        {
                                            strRate = rowarray123[i];
                                        }
                                        else if (i > 0 && i < rowarray123.Count() - 4)
                                        {
                                            strDistNme += rowarray123[i] + " ";
                                        }
                                    }
                                    newrow123 = TaxYear + "~" + "" + "~" + strDistCode + "~" + strDistNme + "~" + strRate + "~" + strGovt + "~" + strEducation + "~" + strBonds;
                                    gc.insert_date(orderNumber, Parcel_ID, 967, newrow123, 1, DateTime.Now);
                                }
                                if (rowarray123.Count() == 5 && rowarray123[0].Contains("Totals"))
                                {
                                    newrow123 = TaxYear + "~" + "~" + "~" + rowarray123[0] + "~" + rowarray123[1] + "~" + rowarray123[2] + "~" + rowarray123[3] + "~" + rowarray123[4];
                                    gc.insert_date(orderNumber, Parcel_ID, 967, newrow123, 1, DateTime.Now);
                                }
                                rowarray123.Clear();
                            }

                        }
                        catch
                        { }
                        try
                        {
                            driver.SwitchTo().Window(driver.WindowHandles.Last());
                            Thread.Sleep(2000);
                        }
                        catch
                        { }
                        //Tax Info2 Details
                        try
                        {
                            IWebElement TaxInfo22 = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[2]/td[2]/table/tbody/tr[4]/td[2]/a"));
                            CurrentTaxInfo22 = TaxInfo22.GetAttribute("href").Replace("javascript: newWinNoVar('", "").Replace("','asmtInfo','770','500','400','0','yes','no','no')", "");
                            gc.downloadfileHeader(CurrentTaxInfo22, orderNumber, Parcel_ID, "Tax_Info2", "OR", "Jackson", driver);

                            FilePathCurrentTaxInfo22 = gc.filePath(orderNumber, Parcel_ID) + "Tax_Info2.pdf";
                            PdfReader reader22;
                            reader22 = new PdfReader(FilePathCurrentTaxInfo22);
                            String textFromPage22 = PdfTextExtractor.GetTextFromPage(reader22, 1);
                            System.Diagnostics.Debug.WriteLine("" + textFromPage22);

                            pdftext22 = textFromPage22;

                            tableassess123 = gc.Between(pdftext22, "Trimester Option", "Total TOTAL DUE").Trim();
                            string[] tableArray123 = tableassess123.Split('\n');
                            List<string> rowarray123 = new List<string>();
                            int m = 0, n = 0, o = 0, p = 0, q = 0;
                            int count123 = tableArray123.Length;
                            for (m = 0; m < count123; m++)
                            {
                                a123 = tableArray123[m].Replace(" ", "~");
                                string[] rowarray213 = a123.Split('~');
                                rowarray123.AddRange(rowarray213);
                                if (rowarray123.Count == 4)
                                {
                                    newrow123 = rowarray123[p] + "~" + rowarray123[p + 1] + "~" + rowarray123[p + 2] + "~" + rowarray123[p + 3];
                                    gc.insert_date(orderNumber, Parcel_ID, 991, newrow123, 1, DateTime.Now);
                                }
                                if (rowarray123.Count == 2)
                                {
                                    newrow123 = rowarray123[n] + "~" + "" + "~" + "" + "~" + rowarray123[n + 1];
                                    gc.insert_date(orderNumber, Parcel_ID, 991, newrow123, 1, DateTime.Now);
                                }
                                if (rowarray123.Count == 3 && !rowarray123.Contains("2,660.64") && !rowarray123.Contains("3,782.05"))
                                {
                                    newrow123 = rowarray123[o] + "~" + "" + "~" + rowarray123[o + 1] + "~" + rowarray123[o + 2];
                                    gc.insert_date(orderNumber, Parcel_ID, 991, newrow123, 1, DateTime.Now);
                                }
                                if (rowarray123.Count == 3 && !rowarray123.Contains("886.88") && !rowarray123.Contains("462.13"))
                                {
                                    newrow123 = "Total" + "~" + rowarray123[q] + "~" + rowarray123[q + 1] + "~" + rowarray123[q + 2];
                                    gc.insert_date(orderNumber, Parcel_ID, 991, newrow123, 1, DateTime.Now);
                                }
                                rowarray123.Clear();
                            }

                            taxy_Year = gc.Between(pdftext22, "TAXATION OFFICE", " TAX ( ").Trim();
                            TaxyAmount_Due = gc.Between(pdftext22, "Before Discount ) ", "PAYMENT OPTIONS").Replace("DELINQUENT TAXES:", "").Trim();
                            TaxyTotalDue = gc.Between(pdftext22, "Pre-payments)", "Tear Here").Trim();

                            try
                            {
                                Deliquent_Taxes1 = gc.Between(pdftext22, "PAYMENT OPTIONS", "Date Due 3%").Trim();
                                try
                                {
                                    Deliquent_Taxes2 = gc.Between(pdftext22, "Trimester Option", "11/15/18").Trim();
                                }
                                catch
                                { }

                                Deliquent_Taxes18 = Deliquent_Taxes1 + " / " + Deliquent_Taxes2;
                            }
                            catch
                            { }

                            Taxinfo_Details1 = taxy_Year + "~" + TaxyAmount_Due + "~" + TaxyTotalDue + "~" + Deliquent_Taxes18;
                            gc.insert_date(orderNumber, Parcel_ID, 978, Taxinfo_Details1, 1, DateTime.Now);
                        }
                        catch
                        { }
                    }

                    try
                    {
                        var SelectAddress32 = driver.FindElement(By.XPath("/html/body/table/tbody/tr[1]/td/table/tbody/tr/td[3]/select"));
                        var SelectAddressTax32 = new SelectElement(SelectAddress32);
                        SelectAddressTax32.SelectByIndex(1);

                        IWebElement TaxInfo221 = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[2]/td[2]/table/tbody/tr[4]/td[2]/a"));
                        CurrentTaxInfo221 = TaxInfo221.GetAttribute("href").Replace("javascript: newWinNoVar('", "").Replace("','asmtInfo','770','500','400','0','yes','no','no')", "");
                        gc.downloadfileHeader(CurrentTaxInfo221, orderNumber, Parcel_ID, "Tax_Info21", "OR", "Jackson", driver);

                        FilePathCurrentTaxInfo221 = gc.filePath(orderNumber, Parcel_ID) + "Tax_Info21.pdf";
                        PdfReader reader221;
                        reader221 = new PdfReader(FilePathCurrentTaxInfo221);
                        String textFromPage221 = PdfTextExtractor.GetTextFromPage(reader221, 1);
                        System.Diagnostics.Debug.WriteLine("" + textFromPage221);

                        pdftext221 = textFromPage221;

                        taxy_Year1 = gc.Between(pdftext221, "TAXATION OFFICE", " TAX ( ").Trim();
                        TaxyAmount_Due1 = gc.Between(pdftext221, "Before Discount ) ", "PAYMENT OPTIONS").Replace("DELINQUENT TAXES:", "").Trim();
                        TaxyTotalDue1 = gc.Between(pdftext221, "Pre-payments)", "Tear Here").Trim();

                        try
                        {
                            Deliquent_Taxes17 = gc.Between(pdftext221, "PAYMENT OPTIONS", "Date Due 3%").Trim();
                        }
                        catch
                        { }

                        Taxinfo_Details11 = taxy_Year1 + "~" + TaxyAmount_Due1 + "~" + TaxyTotalDue1 + "~" + Deliquent_Taxes17;
                        gc.insert_date(orderNumber, Parcel_ID, 978, Taxinfo_Details11, 1, DateTime.Now);
                    }
                    catch
                    { }

                    try
                    {
                        var SelectAddress321 = driver.FindElement(By.XPath("/html/body/table/tbody/tr[1]/td/table/tbody/tr/td[3]/select"));
                        var SelectAddressTax321 = new SelectElement(SelectAddress321);
                        SelectAddressTax321.SelectByIndex(2);

                        IWebElement TaxInfo2211 = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[2]/td[2]/table/tbody/tr[4]/td[2]/a"));
                        CurrentTaxInfo2211 = TaxInfo2211.GetAttribute("href").Replace("javascript: newWinNoVar('", "").Replace("','asmtInfo','770','500','400','0','yes','no','no')", "");
                        gc.downloadfileHeader(CurrentTaxInfo2211, orderNumber, Parcel_ID, "Tax_Info211", "OR", "Jackson", driver);

                        FilePathCurrentTaxInfo2211 = gc.filePath(orderNumber, Parcel_ID) + "Tax_Info211.pdf";
                        PdfReader reader2211;
                        reader2211 = new PdfReader(FilePathCurrentTaxInfo2211);
                        String textFromPage2211 = PdfTextExtractor.GetTextFromPage(reader2211, 1);
                        System.Diagnostics.Debug.WriteLine("" + textFromPage2211);

                        pdftext2211 = textFromPage2211;

                        taxy_Year11 = gc.Between(pdftext2211, "TAXATION OFFICE", " TAX ( ").Trim();
                        TaxyAmount_Due11 = gc.Between(pdftext2211, "Before Discount ) ", "PAYMENT OPTIONS").Trim().Replace("DELINQUENT TAXES:", "").Trim();
                        TaxyTotalDue11 = gc.Between(pdftext2211, "Pre-payments)", "Tear Here").Trim();
                        try
                        {
                            Deliquent_Taxes16 = gc.Between(pdftext2211, "PAYMENT OPTIONS", "Date Due 3%").Trim();
                        }
                        catch
                        { }

                        Taxinfo_Details111 = taxy_Year11 + "~" + TaxyAmount_Due11 + "~" + TaxyTotalDue11 + "~" + Deliquent_Taxes16;
                        gc.insert_date(orderNumber, Parcel_ID, 978, Taxinfo_Details111, 1, DateTime.Now);
                    }
                    catch
                    { }

                    try
                    {
                        var SerachTax31 = driver.FindElement(By.XPath("/html/body/table/tbody/tr[1]/td/table/tbody/tr/td[3]/select"));
                        var selectElement311 = new SelectElement(SerachTax31);
                        selectElement311.SelectByIndex(0);

                        SupplementalURL = "https://apps.jacksoncounty.org/OIS/taxsummary/R/" + Parcel_ID.Replace("-", "");
                        gc.downloadfileHeader(SupplementalURL, orderNumber, Parcel_ID, "Tax_Info1", "OR", "Jackson", driver);

                        try
                        {
                            //Tax Info Details1
                            FilePathCurrentTaxInfo1 = gc.filePath(orderNumber, Parcel_ID) + "Tax_Info1.pdf";
                            PdfReader reader1;
                            reader1 = new PdfReader(FilePathCurrentTaxInfo1);
                            String textFromPage11 = PdfTextExtractor.GetTextFromPage(reader1, 1);
                            System.Diagnostics.Debug.WriteLine("" + textFromPage11);

                            pdftext1 = textFromPage11;

                            try
                            {
                                tableassess17 = gc.Between(pdftext1, "Interest To ", "Tax Summary").Trim();

                                newrow1127 = "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + tableassess17;
                                gc.insert_date(orderNumber, Parcel_ID, 977, newrow1127, 1, DateTime.Now);
                            }
                            catch
                            { }

                            tableassess1 = GlobalClass.After(pdftext1, "Due Date").Trim();
                            string[] tableArray1 = tableassess1.Split('\n');
                            List<string> rowarray11 = new List<string>();
                            int a = 0, b = 0, c = 0;
                            int count11 = tableArray1.Length;
                            for (a = 0; a < count11; a++)
                            {
                                a2 = tableArray1[a].Replace(" ", "~");
                                string[] rowarray2 = a2.Split('~');
                                rowarray11.AddRange(rowarray2);
                                if (rowarray11.Count == 10)
                                {
                                    newrow111 = rowarray11[b] + "~" + rowarray11[b + 1] + "~" + rowarray11[b + 2] + "~" + rowarray11[b + 3] + "~" + rowarray11[b + 4] + "~" + rowarray11[b + 5] + "~" + rowarray11[b + 6] + "~" + rowarray11[b + 7] + " " + rowarray11[b + 8] + " " + rowarray11[b + 9] + "~" + "";
                                    gc.insert_date(orderNumber, Parcel_ID, 977, newrow111, 1, DateTime.Now);
                                }
                                if (rowarray11.Contains("Total"))
                                {
                                    newrow112 = "" + "~" + rowarray11[c] + "~" + rowarray11[c + 1] + "~" + rowarray11[c + 2] + "~" + rowarray11[c + 3] + "~" + rowarray11[c + 4] + "~" + rowarray11[c + 5] + "~" + "" + "~" + "";
                                    gc.insert_date(orderNumber, Parcel_ID, 977, newrow112, 1, DateTime.Now);
                                }
                                rowarray11.Clear();
                            }

                        }
                        catch
                        { }

                        try
                        {
                            //Tax Info Deatils3
                            IWebElement TaxInfo = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[2]/td[2]/table/tbody/tr[5]/td[2]/a"));
                            CurrentTaxInfo = TaxInfo.GetAttribute("href").Replace("javascript: newWinNoVar('", "").Replace("','asmtInfo','770','500','400','0','yes','no','no')", "");
                            gc.downloadfileHeader(CurrentTaxInfo, orderNumber, Parcel_ID, "Tax_Info", "OR", "Jackson", driver);
                            String textFromPage4 = "", textFromPage5 = "";
                            FilePathCurrentTaxInfo = gc.filePath(orderNumber, Parcel_ID) + "Tax_Info.pdf";
                            PdfReader reader;
                            reader = new PdfReader(FilePathCurrentTaxInfo);
                            String textFromPage1 = PdfTextExtractor.GetTextFromPage(reader, 1);
                            String textFromPage2 = PdfTextExtractor.GetTextFromPage(reader, 2);
                            String textFromPage3 = PdfTextExtractor.GetTextFromPage(reader, 3);
                            try
                            {
                                textFromPage4 = PdfTextExtractor.GetTextFromPage(reader, 4);
                            }
                            catch
                            { }

                            try
                            {
                                textFromPage5 = PdfTextExtractor.GetTextFromPage(reader, 5);
                            }
                            catch
                            { }


                            textFromPage = textFromPage1 + " " + textFromPage2 + " " + textFromPage3 + " " + textFromPage4 + " " + textFromPage5;
                            System.Diagnostics.Debug.WriteLine("" + textFromPage);

                            pdftext = textFromPage;

                            try
                            {
                                tableassess = gc.Between(pdftext, "Due Amount", "Page 3 of 3").Trim();
                            }
                            catch
                            { }

                            try
                            {
                                tableassess = gc.Between(pdftext, "Due Amount", "Page 4 of 4").Trim();
                            }
                            catch
                            { }

                            try
                            {
                                tableassess = gc.Between(pdftext, "Due Amount", "Page 5 of 5").Trim();
                            }
                            catch
                            { }

                            string[] tableArray = tableassess.Split('\n');
                            List<string> rowarray1 = new List<string>();
                            int i = 0, j = 0, k = 0, y = 0, w = 0, z = 0;
                            int count1 = tableArray.Length;
                            for (i = 0; i < count1; i++)
                            {
                                a1 = tableArray[i].Replace(" ", "~");
                                string[] rowarray = a1.Split('~');
                                rowarray1.AddRange(rowarray);
                                if (rowarray1.Count == 17 && !rowarray1.Contains("Payor"))
                                {
                                    newrow = rowarray1[j] + "~" + rowarray1[j + 1] + "~" + rowarray1[j + 2] + "~" + rowarray1[j + 3] + "~" + rowarray1[j + 4] + " " + rowarray1[j + 5] + " " + rowarray1[j + 6] + " " + rowarray1[j + 7] + " " + rowarray1[j + 8] + " " + rowarray1[j + 9] + " " + rowarray1[j + 10] + "~" + rowarray1[j + 11] + "~" + rowarray1[j + 12] + "~" + rowarray1[j + 13] + "~" + rowarray1[j + 14] + "~" + rowarray1[j + 15] + "~" + rowarray1[j + 16];
                                    gc.insert_date(orderNumber, Parcel_ID, 975, newrow, 1, DateTime.Now);
                                }
                                if (rowarray1.Count == 1 && !rowarray1.Contains("Payor") && !rowarray1.Contains("OFF"))
                                {
                                    newrow1 = "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + rowarray1[y] + "~" + "" + "~" + "" + "~" + "" + "~" + "";
                                    gc.insert_date(orderNumber, Parcel_ID, 975, newrow1, 1, DateTime.Now);
                                }
                                if (rowarray1.Contains("OFF") && !rowarray1.Contains("ORS"))
                                {
                                    newrow1 = "" + "~" + "" + "~" + "" + "~" + rowarray1[y] + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "";
                                    gc.insert_date(orderNumber, Parcel_ID, 975, newrow1, 1, DateTime.Now);
                                }
                                if (rowarray1.Contains("CA"))
                                {
                                    newrow1 = "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "~" + "" + rowarray1[k] + " " + rowarray1[k + 1] + " " + rowarray1[k + 2] + " " + rowarray1[k + 3] + " " + rowarray1[k + 4] + " " + rowarray1[k + 5] + " " + rowarray1[k + 6] + " " + rowarray1[k + 7] + " " + rowarray1[k + 8] + " " + rowarray1[k + 9] + " " + rowarray1[k + 10];
                                    gc.insert_date(orderNumber, Parcel_ID, 975, newrow1, 1, DateTime.Now);
                                }
                                if (rowarray1.Count == 11 && !rowarray1.Contains("CA"))
                                {
                                    newrow3 = rowarray1[y] + "~" + rowarray1[y + 1] + "~" + rowarray1[y + 2] + "~" + rowarray1[y + 3] + "~" + rowarray1[y + 4] + "~" + rowarray1[y + 5] + "~" + rowarray1[y + 6] + "~" + rowarray1[y + 7] + "~" + rowarray1[y + 8] + "~" + rowarray1[y + 9] + "~" + rowarray1[y + 10] + "~" + "";
                                    gc.insert_date(orderNumber, Parcel_ID, 975, newrow3, 1, DateTime.Now);
                                }
                                if (rowarray1.Count == 12 && !rowarray1.Contains("Payor"))
                                {
                                    newrow4 = rowarray1[w] + "~" + rowarray1[w + 1] + "~" + rowarray1[w + 2] + "~" + rowarray1[w + 3] + "~" + rowarray1[w + 4] + " " + rowarray1[w + 5] + "~" + rowarray1[w + 6] + "~" + rowarray1[w + 7] + "~" + rowarray1[w + 8] + "~" + rowarray1[w + 9] + "~" + rowarray1[w + 10] + "~" + rowarray1[w + 11] + "~" + "";
                                    gc.insert_date(orderNumber, Parcel_ID, 975, newrow4, 1, DateTime.Now);
                                }
                                if (rowarray1.Count == 3)
                                {
                                    newrow1 = "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "~" + "" + rowarray1[z] + " " + rowarray1[z + 1] + " " + rowarray1[z + 2];
                                    gc.insert_date(orderNumber, Parcel_ID, 975, newrow1, 1, DateTime.Now);
                                }
                                if (rowarray1.Count == 4)
                                {
                                    newrow1 = "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "~" + "" + rowarray1[z] + " " + rowarray1[z + 1] + " " + rowarray1[z + 2] + " " + rowarray1[z + 3];
                                    gc.insert_date(orderNumber, Parcel_ID, 975, newrow1, 1, DateTime.Now);
                                }
                                if (rowarray1.Count == 13 && !rowarray1.Contains("Page"))
                                {
                                    newrow4 = rowarray1[w] + "~" + rowarray1[w + 1] + "~" + rowarray1[w + 2] + "~" + rowarray1[w + 3] + "~" + rowarray1[w + 4] + " " + rowarray1[w + 5] + " " + rowarray1[w + 6] + "~" + rowarray1[w + 7] + "~" + rowarray1[w + 8] + "~" + rowarray1[w + 9] + "~" + rowarray1[w + 10] + "~" + rowarray1[w + 11] + "~" + rowarray1[w + 12] + "~" + "";
                                    gc.insert_date(orderNumber, Parcel_ID, 975, newrow4, 1, DateTime.Now);
                                }
                                rowarray1.Clear();
                            }
                        }
                        catch
                        { }

                    }
                    catch
                    { }

                    driver.SwitchTo().Window(driver.WindowHandles.Last());
                    Thread.Sleep(2000);

                    try
                    {
                        //Tax Authority
                        Tax_Authority = "JACKSON COUNTY TAXATION OFFICE, P.O. BOX 1569, EDFORD, OR 97501";

                        TaxAithority_Details = Tax_Authority;
                        gc.insert_date(orderNumber, Parcel_ID, 981, TaxAithority_Details, 1, DateTime.Now);
                    }
                    catch
                    { }

                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "OR", "Jackson", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    gc.mergpdf(orderNumber, "OR", "Jackson");
                    return "Data Inserted Successfully";
                }
                catch (Exception ex)
                {
                    driver.Quit();
                    GlobalClass.LogError(ex, orderNumber);
                    throw;
                }
            }

        }
    }
}