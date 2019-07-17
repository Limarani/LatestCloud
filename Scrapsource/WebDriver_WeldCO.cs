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
    public class WebDriver_WeldCO
    {

        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();

        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());

        public string FTP_WeldCO(string address, string account, string parcelNumber, string ownername, string searchType, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;

            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
            string Address = "", lastName = "", firstName = "", Acnumber = "", PropertyAdd = "", Strownername = "", subdivision="", Pin = "", AccountNo = "", TaxAuth="";

            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;

                 //                        using (driver = new ChromeDriver())
                                           using (driver = new PhantomJSDriver())
            {


                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");
                    
                    try
                    {
                        driver.Navigate().GoToUrl("https://www.co.weld.co.us/apps1/propertyportal/");
                        Thread.Sleep(4000);
                    }
                    catch { }

                    if (searchType == "titleflex")
                    {
                        address = address.Trim();
                        string titleaddress = address;
                        gc.TitleFlexSearch(orderNumber, "", "", titleaddress, "CO", "Weld");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Zero_Weld"] = "Zero";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }

                    if (searchType == "address")
                    {

                        driver.FindElement(By.XPath("//*[@id='searchBar']/div[2]/form/input[1]")).SendKeys(address);
                        driver.FindElement(By.XPath("//*[@id='searchBar']/div[2]/form/input[2]")).SendKeys(Keys.Enter);

                        Thread.Sleep(4000);
                        gc.CreatePdf_WOP(orderNumber, "Address search", driver, "CO", "Weld");
                        string check = "";
                        try { 
                        check = driver.FindElement(By.XPath("//*[@id='mainContent']/div/div/p")).Text;
                        if (check.Contains("found 0 records"))
                        {
                            HttpContext.Current.Session["Zero_Weld"] = "Zero";
                            driver.Quit();
                            return "No Data Found";
                        }
                        }
                        catch
                        {

                        }
                    
                        try
                        {
                            int Max = 0;
                            string strowner = "", strAddress = "", strCity = "";
                            string Record = "";
                            try
                            {
                                IWebElement Irecord = driver.FindElement(By.XPath("//*[@id='mainContent']/div/div/ul/li/a/h4"));
                                Record = Irecord.Text;
                                Record = gc.Between(Record, "Address Search:", "record").Trim();
                            }
                            catch { }
                            if (Record != "1")
                            {

                                IWebElement multiaddress = driver.FindElement(By.XPath("//*[@id='tab_address']/div/table"));
                                IList<IWebElement> multiRow = multiaddress.FindElements(By.TagName("tr"));
                                IList<IWebElement> multiTD;
                                foreach (IWebElement multi in multiRow)
                                {
                                    multiTD = multi.FindElements(By.TagName("td"));
                                    if (multiTD.Count != 0 && multiRow.Count >= 3 && multiRow.Count <= 25 && !multi.Text.Contains("Subdivision") && multi.Text.Trim() != "")
                                    {
                                        PropertyAdd = multiTD[2].Text;
                                        Acnumber = multiTD[0].Text;
                                        parcelNumber = multiTD[1].Text;
                                        subdivision = multiTD[3].Text;

                                        string multidetails = Acnumber + "~" + PropertyAdd + "~" + subdivision;
                                        gc.insert_date(orderNumber, parcelNumber, 1616, multidetails, 1, DateTime.Now);
                                        Max++;
                                    }
                                    if (multiTD.Count != 0 && multiRow.Count > 25)
                                    {
                                        HttpContext.Current.Session["multiparcel_Weld_Maximum"] = "Maximum";
                                        driver.Quit();
                                        return "Maximum";
                                    }

                                }
                                if (Max > 1 && Max < 26)
                                {
                                    HttpContext.Current.Session["multiparcel_Weld"] = "Yes";
                                    driver.Quit();
                                    return "MultiParcel";
                                }
                                if (Max == 0)
                                {
                                    HttpContext.Current.Session["Zero_Weld"] = "Zero";
                                    driver.Quit();
                                    return "No Data Found";
                                }
                            }
                        }
                        catch { }
                    }

                    if (searchType == "account")
                    {


                        driver.FindElement(By.Id("pin")).SendKeys(account);
                        Thread.Sleep(4000);
                        gc.CreatePdf_WOP(orderNumber, "Pin or Schedule search", driver, "CO", "Weld");
                        try
                        {
                            int Max = 0;
                            string strowner = "", strAddress = "", strCity = "";
                            string Record = "";
                            try
                            {
                                IWebElement Irecord = driver.FindElement(By.XPath("//*[@id='mainContent']/div/div/ul/li/a/h4"));
                                Record = Irecord.Text;
                                Record = gc.Between(Record, "Address Search:", "record").Trim();
                            }
                            catch { }
                            if (Record != "1")
                            {

                                IWebElement multiaddress = driver.FindElement(By.XPath("//*[@id='tab_address']/div/table"));
                                IList<IWebElement> multiRow = multiaddress.FindElements(By.TagName("tr"));
                                IList<IWebElement> multiTD;
                                foreach (IWebElement multi in multiRow)
                                {
                                    multiTD = multi.FindElements(By.TagName("td"));
                                    if (multiTD.Count != 0 && multiRow.Count >= 3 && multiRow.Count <= 25 && !multi.Text.Contains("Subdivision") && multi.Text.Trim() != "")
                                    {
                                        PropertyAdd = multiTD[2].Text;
                                        Acnumber = multiTD[0].Text;
                                        parcelNumber = multiTD[1].Text;
                                        subdivision = multiTD[3].Text;

                                        string multidetails = Acnumber + "~" + PropertyAdd + "~" + subdivision;
                                        gc.insert_date(orderNumber, parcelNumber, 1616, multidetails, 1, DateTime.Now);
                                        Max++;
                                    }
                                    if (multiTD.Count != 0 && multiRow.Count > 25)
                                    {
                                        HttpContext.Current.Session["multiparcel_Weld_Maximum"] = "Maximum";
                                        driver.Quit();
                                        return "Maximum";
                                    }
                                }
                                if (Max > 1 && Max < 26)
                                {
                                    HttpContext.Current.Session["multiparcel_Weld"] = "Yes";
                                    driver.Quit();
                                    return "MultiParcel";
                                }
                                if (Max == 0)
                                {
                                    HttpContext.Current.Session["Zero_Weld"] = "Zero";
                                    driver.Quit();
                                    return "No Data Found";
                                }
                            }
                        }
                        catch { }
                    }


                    else if (searchType == "parcel")
                    {

                        driver.FindElement(By.XPath("//*[@id='searchBar']/div[2]/form/input[1]")).SendKeys(parcelNumber);
                        driver.FindElement(By.XPath("//*[@id='searchBar']/div[2]/form/input[2]")).SendKeys(Keys.Enter);

                        Thread.Sleep(4000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel search", driver, "CO", "Weld");
                        try
                        {           
                            //driver.FindElement(By.XPath("//*[@id='mainContent']/div/div/div[2]/div[3]/a")).SendKeys(Keys.Enter);
                            driver.FindElement(By.Id("fullRpt")).SendKeys(Keys.Enter);
                            Thread.Sleep(3000);
                        }
                        catch { }

                        try
                        {
                            int Max = 0;
                            string strowner = "", strAddress = "", strCity = "";
                            string Record = "";
                            try
                            {
                                IWebElement Irecord = driver.FindElement(By.XPath("//*[@id='mainContent']/div/div/ul/li/a/h4"));
                                Record = Irecord.Text;
                                Record = gc.Between(Record, "Address Search:", "record").Trim();
                            }
                            catch { }
                            if (Record != "1")
                            {

                                IWebElement multiaddress = driver.FindElement(By.XPath("//*[@id='tab_address']/div/table"));
                                IList<IWebElement> multiRow = multiaddress.FindElements(By.TagName("tr"));
                                IList<IWebElement> multiTD;
                                foreach (IWebElement multi in multiRow)
                                {
                                    multiTD = multi.FindElements(By.TagName("td"));
                                    if (multiTD.Count != 0 && multiRow.Count >= 3 && multiRow.Count <= 25 && !multi.Text.Contains("Subdivision") && multi.Text.Trim() != "")
                                    {
                                        PropertyAdd = multiTD[2].Text;
                                        Acnumber = multiTD[0].Text;
                                        parcelNumber = multiTD[1].Text;
                                        subdivision = multiTD[3].Text;

                                        string multidetails = Acnumber + "~" + PropertyAdd + "~" + subdivision;
                                        gc.insert_date(orderNumber, parcelNumber, 1616, multidetails, 1, DateTime.Now);
                                        Max++;
                                    }
                                    if (multiTD.Count != 0 && multiRow.Count > 25)
                                    {
                                        HttpContext.Current.Session["multiparcel_Weld_Maximum"] = "Maximum";
                                        driver.Quit();
                                        return "Maximum";
                                    }

                                }
                                if (Max > 1 && Max < 26)
                                {
                                    HttpContext.Current.Session["multiparcel_Weld"] = "Yes";
                                    driver.Quit();
                                    return "MultiParcel";
                                }
                                if (Max == 0)
                                {
                                    HttpContext.Current.Session["Zero_Weld"] = "Zero";
                                    driver.Quit();
                                    return "No Data Found";
                                }
                            }
                        }
                        catch { }
                    }

                    if (searchType == "ownername")
                    {

                        driver.FindElement(By.XPath("//*[@id='searchBar']/div[2]/form/input[1]")).SendKeys(ownername);
                        Thread.Sleep(5000);

                        gc.CreatePdf_WOP(orderNumber, "OwnerName search", driver, "CO", "Weld");
                        driver.FindElement(By.XPath("//*[@id='searchBar']/div[2]/form/input[2]")).SendKeys(Keys.Enter);
                        Thread.Sleep(5000);                       

                        gc.CreatePdf_WOP(orderNumber, "OwnerName search Result", driver, "CO", "Weld");
                        try
                        {
                            int Max = 0;
                            string strowner = "", strAddress = "", strCity = "";
                            string Record = "";
                            try
                            {
                                IWebElement Irecord = driver.FindElement(By.XPath("//*[@id='mainContent']/div/div/ul/li/a/h4"));
                                Record = Irecord.Text;
                                Record = gc.Between(Record, "Name Search:", "record").Trim();
                            }
                            catch { }
                            if (Record != "1")
                            {

                                IWebElement multiaddress = driver.FindElement(By.XPath("//*[@id='tab_name']/div/table"));
                                IList<IWebElement> multiRow = multiaddress.FindElements(By.TagName("tr"));
                                IList<IWebElement> multiTD;
                                foreach (IWebElement multi in multiRow)
                                {
                                    multiTD = multi.FindElements(By.TagName("td"));
                                    if (multiTD.Count != 0 && multiRow.Count >= 3 && multiRow.Count <= 25 && !multi.Text.Contains("Subdivision") && multi.Text.Trim() != "")
                                    {
                                        PropertyAdd = multiTD[2].Text +" "+ multiTD[3].Text;
                                        Acnumber = multiTD[0].Text;
                                        parcelNumber = multiTD[1].Text;
                                        subdivision = multiTD[4].Text;

                                        string multidetails = Acnumber + "~" + PropertyAdd + "~" + subdivision;
                                        gc.insert_date(orderNumber, parcelNumber, 1616, multidetails, 1, DateTime.Now);
                                        Max++;
                                    }
                                    if (multiTD.Count != 0 && multiRow.Count > 25)
                                    {
                                        HttpContext.Current.Session["multiparcel_Weld_Maximum"] = "Maximum";
                                        driver.Quit();
                                        return "Maximum";
                                    }

                                }
                                if (Max > 1 && Max < 26)
                                {
                                    HttpContext.Current.Session["multiparcel_Weld"] = "Yes";
                                    driver.Quit();
                                    return "MultiParcel";
                                }
                                if (Max == 0)
                                {
                                    HttpContext.Current.Session["Zero_Weld"] = "Zero";
                                    driver.Quit();
                                    return "No Data Found";
                                }
                            }
                        }
                        catch { }
                    }
                    try
                    {
                        //No Data Found
                        string nodata = driver.FindElement(By.Id("mainContent")).Text;
                        if (nodata.Contains("returned: 0 records"))
                        {
                            HttpContext.Current.Session["Zero_Weld"] = "Zero";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }

                    //property details

                    string space = "", PropertyType = "", Tax_Year = "", OwnerName = "", PropertyCity = "", Zip = "", MailingAddress = "";
                    string Proadd1 = "", Proadd2 = "", PropertyAddress = "", YearBuilt = "", LegalDesc = "", OwnerName1 = "", OwnerName2 = "";

                    //IWebElement Iframe = driver.FindElement(By.XPath("/html/body/noscript/text()"));
                    //driver.SwitchTo().Window(driver.WindowHandles.Last());

                   
                    try
                    {
                        driver.FindElement(By.Id("fullRpt")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "search Result", driver, "CO", "Weld");
                    }
                    catch
                    { }
                    driver.SwitchTo().Window(driver.WindowHandles.Last());

                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div[3]/button")).Click();
                        Thread.Sleep(4000);
                    }
                    catch { }
                                     
                    parcelNumber = driver.FindElement(By.XPath("/html/body/div[5]/table[1]/tbody/tr/td[2]")).Text;
                    AccountNo = driver.FindElement(By.XPath("/html/body/div[5]/table[1]/tbody/tr/td[1]")).Text;
                    space = driver.FindElement(By.XPath("/html/body/div[5]/table[1]/tbody/tr/td[3]")).Text;
                    PropertyType = driver.FindElement(By.XPath("/html/body/div[5]/table[1]/tbody/tr/td[4]")).Text;
                    Tax_Year = driver.FindElement(By.XPath("/html/body/div[5]/table[1]/tbody/tr/td[5]")).Text;

                    try
                    {
                        LegalDesc = driver.FindElement(By.XPath("/html/body/div[5]/table[2]/tbody")).Text;
                    }
                    catch { }
                    //try
                    //{
                    //    driver.FindElement(By.Id("owner")).SendKeys(Keys.Enter);
                    //    Thread.Sleep(4000);                       
                    //}

                    //catch { }
                    try
                    {
                        OwnerName1 = driver.FindElement(By.XPath("/html/body/div[6]/table[2]/tbody/tr[1]/td[2]")).Text;
                        OwnerName2 = driver.FindElement(By.XPath("/html/body/div[6]/table[2]/tbody/tr[2]/td[2]")).Text;
                    }
                    catch { }
                    try
                    {
                        if(OwnerName1=="")
                        {
                            OwnerName1 = driver.FindElement(By.XPath("/html/body/div[6]/table[2]/tbody/tr/td[2]")).Text;
                        }                      
                       
                    }
                    catch { }
                    OwnerName = OwnerName1 + " " + OwnerName2;
                    PropertyAddress = driver.FindElement(By.XPath("/html/body/div[5]/table[4]/tbody/tr/td[1]")).Text;
                    PropertyCity = driver.FindElement(By.XPath("/html/body/div[5]/table[4]/tbody/tr/td[2]")).Text;
                    Zip = driver.FindElement(By.XPath("/html/body/div[5]/table[4]/tbody/tr/td[3]")).Text;
                    MailingAddress = driver.FindElement(By.XPath("/html/body/div[6]/table[2]/tbody/tr[1]/td[3]")).Text;
                    try
                    {
                        if (MailingAddress=="")
                        {
                            MailingAddress = driver.FindElement(By.XPath("/html/body/div[6]/table[2]/tbody/tr/td[3]")).Text;
                        }
                    }
                    catch { }
                    
                    //try
                    //{
                    //    driver.FindElement(By.Id("buildings")).SendKeys(Keys.Enter);
                    //    Thread.Sleep(2000);                     
                    //}
                    //catch { }

                    try
                    {
                        YearBuilt = driver.FindElement(By.XPath("/html/body/div[8]/table[6]/tbody/tr/td[4]")).Text.Trim();
                        if(LegalDesc == "")
                        {
                            LegalDesc = driver.FindElement(By.XPath("/html/body/div[5]/table[2]/tbody")).Text;
                        }
                        
                    }
                    catch { }



                    string propertydetails = AccountNo + "~" + space + "~" + PropertyType + "~" + Tax_Year + "~" + OwnerName + "~" + PropertyAddress + "~" + PropertyCity + "~" + Zip + "~" + MailingAddress + "~" + YearBuilt + "~" + LegalDesc;
                    gc.insert_date(orderNumber, parcelNumber, 1614, propertydetails, 1, DateTime.Now);



                    // Assessment Details

                    //try
                    //{
                    //    driver.FindElement(By.Id("valuation")).SendKeys(Keys.Enter);
                    //    Thread.Sleep(2000);                        
                    //}
                    //catch { }

                    try
                    {
                        IWebElement Assessment = driver.FindElement(By.XPath("/html/body/div[9]/table[2]"));
                        IList<IWebElement> TRAssessment = Assessment.FindElements(By.TagName("tr"));
                        IList<IWebElement> THAssessment = Assessment.FindElements(By.TagName("th"));
                        IList<IWebElement> TDAssessment;
                        foreach (IWebElement row in TRAssessment)
                        {
                            TDAssessment = row.FindElements(By.TagName("td"));
                            if (TDAssessment.Count != 0 && !row.Text.Contains("Assessed Value"))
                            {
                                string Assessmentdetails = TDAssessment[0].Text + "~" + TDAssessment[1].Text + "~" + TDAssessment[2].Text + "~" + TDAssessment[3].Text + "~" + TDAssessment[4].Text;
                                gc.insert_date(orderNumber, parcelNumber, 1615, Assessmentdetails, 1, DateTime.Now);
                            }
                        }
                    }
                    catch { }

                    gc.CreatePdf(orderNumber, parcelNumber, "Property Details", driver, "CO", "Weld");

                    // Tax Information Details
                   
                    try
                    {
                        driver.Navigate().GoToUrl("https://www.weldtax.com/treasurer/web/");
                        Thread.Sleep(4000);
                        string ta1 = "", ta2 = "", ta3 = "";
                        ta1 = driver.FindElement(By.Id("left")).Text.Replace("John R. Lefebvre", "");
                        ta2 = GlobalClass.Before(ta1, "Mailing Address:").Replace("\r\n", " ").Replace("\r\n", " ").Trim();
                        ta3 = gc.Between(ta1, "Phone:", "Fax:");
                        TaxAuth = ta2 + " " + "Phone: " + ta3;
                        gc.CreatePdf(orderNumber, parcelNumber, "Tax Authority", driver, "CO", "Weld");

                    }
                    catch { }

                    driver.Navigate().GoToUrl("https://www.weldtax.com/treasurer/web/login.jsp?submit=I+Have+Read+The+Above+Statement+");
                    Thread.Sleep(5000);
                    try
                    {
                        driver.FindElement(By.XPath("//*[@id='middle_left']/form/input[1]")).Click();
                        Thread.Sleep(2000);
                    }
                    catch { }

                    driver.FindElement(By.Id("AccountId")).SendKeys(AccountNo);
                    gc.CreatePdf(orderNumber, parcelNumber, "Tax Search", driver, "CO", "Weld");
                    driver.FindElement(By.XPath("//*[@id='middle']/b/form/table[4]/tbody/tr/td[1]/input")).Click();
                    Thread.Sleep(4000);
                    gc.CreatePdf(orderNumber, parcelNumber, "Tax Search Result", driver, "CO", "Weld");
                    try
                    {
                        driver.FindElement(By.XPath("//*[@id='searchResultsTable']/tbody/tr/td[1]/strong/a")).Click();
                        Thread.Sleep(4000);
                    }
                    catch { }

                    string bulkdata = "", strAccNo = "", strownername = "", strProAdd = "", strMailingAdd = "", Installment1 = "";
                    string TaxesDue1 = "", TotalDue1 = "", TaxesDue2 = "", TaxesDue3 = "", TotalDue2 = "", Installment2 = "", DueDate1 = "", DueDate2 = "", DueDate3 = "", Installment3 = "";
                    string Installment_Full = "", TotalDue_Full = "", Good_through_date = "";
                    string InterestDue1 = "", InterestDue2 = "", InterestDue3 = "";
                    bulkdata = driver.FindElement(By.XPath("//*[@id='taxAccountSummary']/table/tbody")).Text;
                    strAccNo = gc.Between(bulkdata, "Account Id", "Parcel Number").Trim();
                    strownername = gc.Between(bulkdata, "Owners", "Address").Trim();
                    strProAdd = gc.Between(bulkdata, "Situs Address", "Legal").Trim();
                    strMailingAdd = gc.Between(bulkdata, "Address", "Situs Address").Trim();

                    string MiscDue1 = "", LienDue1 = "", LienInterestDue1 = "", MiscDue2 = "", LienDue2 = "", LienInterestDue2 = "", MiscDue3 = "", LienDue3 = "", LienInterestDue3 = "";

                    //try
                    //{
                    //    IWebElement TaxInfo = driver.FindElement(By.XPath("//*[@id='totals']/table/tbody"));
                    //    IList<IWebElement> TRTaxInfo = TaxInfo.FindElements(By.TagName("tr"));
                    //    IList<IWebElement> THTaxInfo = TaxInfo.FindElements(By.TagName("th"));
                    //    IList<IWebElement> TDTaxInfo;
                    //    foreach (IWebElement row in TRTaxInfo)
                    //    {
                    //        TDTaxInfo = row.FindElements(By.TagName("td"));
                    //        if (TRTaxInfo.Count != 0 && row.Text.Trim() != "")
                    //        {
                    //            if (row.Text.Contains("Taxes Due"))
                    //            {
                    //                TaxesDue3 = TDTaxInfo[1].Text;
                    //            }
                    //            if (row.Text.Contains("Interest Due") && !row.Text.Contains("Lien"))
                    //            {
                    //                InterestDue3 = TDTaxInfo[1].Text;
                    //            }
                    //            if (row.Text.Contains("Misc Due"))
                    //            {
                    //                MiscDue3 = TDTaxInfo[1].Text;
                    //            }
                    //            if (row.Text.Contains("Lien Due"))
                    //            {
                    //                LienDue3 = TDTaxInfo[1].Text;
                    //            }
                    //            if (row.Text.Contains("Lien Interest Due"))
                    //            {
                    //                LienInterestDue3 = TDTaxInfo[1].Text;
                    //            }
                    //            if (row.Text.Contains("Total Due"))
                    //            {
                    //                TotalDue_Full = TDTaxInfo[1].Text;
                    //            }

                    //        }
                    //    }
                    //}
                    //catch { }

                    try
                    {
                        driver.FindElement(By.Id("paymentTypeFirst")).Click();
                        Thread.Sleep(4000);
                        gc.CreatePdf(orderNumber, parcelNumber, "First Payment", driver, "CO", "Weld");
                        Installment1 = driver.FindElement(By.XPath("//*[@id='inquiryForm']/table/tbody/tr[2]/td[2]/label[1]")).Text;
                        // TotalDue1 = driver.FindElement(By.XPath("//*[@id='totals']/table/tbody/tr/td[2]")).Text;

                    }
                    catch { }



                    //Good Through Details

                    try
                    {

                        InterestDue1 = driver.FindElement(By.XPath("//*[@id='totals']/table/tbody/tr[2]/td[2]")).Text;
                        if (InterestDue1 != "$0.00")
                        {
                            IWebElement good_date = driver.FindElement(By.Id("paymentDate"));
                            Good_through_date = good_date.GetAttribute("value");
                            if (Good_through_date.Contains("Select A Date"))
                            {
                                Good_through_date = "-";
                            }
                            else
                            {

                                DateTime G_Date = Convert.ToDateTime(Good_through_date);
                                string dateChecking = DateTime.Now.ToString("MM") + "/15/" + DateTime.Now.ToString("yyyy");

                                if (G_Date < Convert.ToDateTime(dateChecking))
                                {
                                    //end of the month
                                    Good_through_date = new DateTime(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(Convert.ToInt16(DateTime.Now.ToString("MM"))), DateTime.DaysInMonth(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(DateTime.Now.ToString("MM")))).ToString("MM/dd/yyyy");

                                }
                                else if (G_Date > Convert.ToDateTime(dateChecking))
                                {
                                    // nextEndOfMonth 
                                    if ((Convert.ToInt16(Convert.ToInt16(DateTime.Now.ToString("MM"))) < 12))
                                    {
                                        Good_through_date = new DateTime(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(Convert.ToInt16(DateTime.Now.ToString("MM")) + 1), DateTime.DaysInMonth(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(DateTime.Now.ToString("MM")) + 1)).ToString("MM/dd/yyyy");
                                    }
                                    else
                                    {
                                        int nxtYr = Convert.ToInt16(DateTime.Now.ToString("yyyy")) + 1;
                                        Good_through_date = new DateTime(nxtYr, 1, DateTime.DaysInMonth(Convert.ToInt16(DateTime.Now.ToString("yyyy")), 1)).ToString("MM/dd/yyyy");
                                    }
                                }
                                driver.FindElement(By.Id("paymentDate")).Clear();
                                Thread.Sleep(1000);

                                string[] daysplit = Good_through_date.Split('/');
                                try
                                {
                                    driver.FindElement(By.Id("paymentDate")).SendKeys(Good_through_date);
                                    Thread.Sleep(5000);
                                }
                                catch { }

                                try
                                {
                                    driver.FindElement(By.XPath("//*[@id='inquiryForm']/table/tbody/tr[1]/td[2]/img")).Click();
                                    Thread.Sleep(3000);
                                }
                                catch { }
                                IWebElement Iday = driver.FindElement(By.Id("calendarContainer"));
                                IList<IWebElement> IdayRow = Iday.FindElements(By.TagName("button"));
                                foreach (IWebElement day in IdayRow)
                                {
                                    if (day.Text != "" && day.Text == daysplit[1])
                                    {
                                        day.SendKeys(Keys.Enter);
                                        Thread.Sleep(4000);
                                        gc.CreatePdf(orderNumber, parcelNumber, "Good Through Date", driver, "CO", "Weld");
                                    }
                                }

                            }
                        }
                    }
                    catch
                    { }
                    string bulkdata3 = "";

                    if (Good_through_date != "" || Installment1 != "")
                    {
                        try
                        {
                            IWebElement TaxInfo = driver.FindElement(By.XPath("//*[@id='totals']/table/tbody"));
                            IList<IWebElement> TRTaxInfo = TaxInfo.FindElements(By.TagName("tr"));
                            IList<IWebElement> THTaxInfo = TaxInfo.FindElements(By.TagName("th"));
                            IList<IWebElement> TDTaxInfo;
                            foreach (IWebElement row in TRTaxInfo)
                            {
                                TDTaxInfo = row.FindElements(By.TagName("td"));
                                if (TRTaxInfo.Count != 0 && row.Text.Trim() != "")
                                {
                                    if (row.Text.Contains("Taxes Due"))
                                    {
                                        TaxesDue1 = TDTaxInfo[1].Text;
                                    }
                                    if (row.Text.Contains("Interest Due") && !row.Text.Contains("Lien"))
                                    {
                                        InterestDue1 = TDTaxInfo[1].Text;
                                    }
                                    if (row.Text.Contains("Misc Due"))
                                    {
                                        MiscDue1 = TDTaxInfo[1].Text;
                                    }
                                    if (row.Text.Contains("Lien Due"))
                                    {
                                        LienDue1 = TDTaxInfo[1].Text;
                                    }
                                    if (row.Text.Contains("Lien Interest Due"))
                                    {
                                        LienInterestDue1 = TDTaxInfo[1].Text;
                                    }
                                    if (row.Text.Contains("Total Due"))
                                    {
                                        TotalDue1 = TDTaxInfo[1].Text;
                                    }

                                }
                            }
                        }
                        catch { }

                    }
                    else
                    { }
                    try
                    {
                        driver.FindElement(By.Id("paymentTypeSecond")).Click();
                        Thread.Sleep(4000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Second Payment", driver, "CO", "Weld");
                        Installment2 = driver.FindElement(By.XPath("//*[@id='inquiryForm']/table/tbody/tr[2]/td[2]/label[2]")).Text;
                        //TaxesDue2 = driver.FindElement(By.XPath("//*[@id='totals']/table/tbody/tr[1]/td[2]")).Text;
                        // TotalDue2 = driver.FindElement(By.XPath("//*[@id='totals']/table/tbody/tr[2]/td[2]")).Text;
                    }
                    catch { }
                    if (Good_through_date != "" || Installment2 != "")
                    {
                        try
                        {
                            IWebElement TaxInfo = driver.FindElement(By.XPath("//*[@id='totals']/table/tbody"));
                            IList<IWebElement> TRTaxInfo = TaxInfo.FindElements(By.TagName("tr"));
                            IList<IWebElement> THTaxInfo = TaxInfo.FindElements(By.TagName("th"));
                            IList<IWebElement> TDTaxInfo;
                            foreach (IWebElement row in TRTaxInfo)
                            {
                                TDTaxInfo = row.FindElements(By.TagName("td"));
                                if (TRTaxInfo.Count != 0 && row.Text.Trim() != "")
                                {
                                    if (row.Text.Contains("Taxes Due"))
                                    {
                                        TaxesDue2 = TDTaxInfo[1].Text;
                                    }
                                    if (row.Text.Contains("Interest Due") && !row.Text.Contains("Lien"))
                                    {
                                        InterestDue2 = TDTaxInfo[1].Text;
                                    }
                                    if (row.Text.Contains("Misc Due"))
                                    {
                                        MiscDue2 = TDTaxInfo[1].Text;
                                    }
                                    if (row.Text.Contains("Lien Due"))
                                    {
                                        LienDue2 = TDTaxInfo[1].Text;
                                    }
                                    if (row.Text.Contains("Lien Interest Due"))
                                    {
                                        LienInterestDue2 = TDTaxInfo[1].Text;
                                    }
                                    if (row.Text.Contains("Total Due"))
                                    {
                                        TotalDue2 = TDTaxInfo[1].Text;
                                    }

                                }
                            }
                        }
                        catch { }
                    }
                    else
                    { }

                    try
                    {
                        driver.FindElement(By.Id("paymentTypeFull")).Click();
                        Thread.Sleep(7000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Full Payment", driver, "CO", "Weld");
                        Installment_Full = driver.FindElement(By.XPath("//*[@id='inquiryForm']/table/tbody/tr[2]/td[2]/label[2]")).Text;
                        //TotalDue_Full = driver.FindElement(By.XPath("//*[@id='totals']/table/tbody/tr/td[2]")).Text;
                    }
                    catch { }

                    if (Good_through_date != "" || Installment_Full != "")
                    {
                        try
                        {
                            IWebElement TaxInfo = driver.FindElement(By.XPath("//*[@id='totals']/table/tbody"));
                            IList<IWebElement> TRTaxInfo = TaxInfo.FindElements(By.TagName("tr"));
                            IList<IWebElement> THTaxInfo = TaxInfo.FindElements(By.TagName("th"));
                            IList<IWebElement> TDTaxInfo;
                            foreach (IWebElement row in TRTaxInfo)
                            {
                                TDTaxInfo = row.FindElements(By.TagName("td"));
                                if (TRTaxInfo.Count != 0 && row.Text.Trim() != "")
                                {
                                    if (row.Text.Contains("Taxes Due"))
                                    {
                                        TaxesDue3 = TDTaxInfo[1].Text;
                                    }
                                    if (row.Text.Contains("Interest Due") && !row.Text.Contains("Lien"))
                                    {
                                        InterestDue3 = TDTaxInfo[1].Text;
                                    }
                                    if (row.Text.Contains("Misc Due"))
                                    {
                                        MiscDue3 = TDTaxInfo[1].Text;
                                    }
                                    if (row.Text.Contains("Lien Due"))
                                    {
                                        LienDue3 = TDTaxInfo[1].Text;
                                    }
                                    if (row.Text.Contains("Lien Interest Due"))
                                    {
                                        LienInterestDue3 = TDTaxInfo[1].Text;
                                    }
                                    if (row.Text.Contains("Total Due"))
                                    {
                                        TotalDue_Full = TDTaxInfo[1].Text;
                                    }

                                }
                            }
                        }
                        catch { }

                    }
                    else
                    { }
                    try
                    {
                        driver.FindElement(By.Id("paymentTypeFirst")).Click();
                        Thread.Sleep(5000);
                        // gc.CreatePdf(orderNumber, parcelNumber, "First Payment", driver, "CO", "Weld");
                        //Installment1 = driver.FindElement(By.XPath("//*[@id='inquiryForm']/table/tbody/tr[2]/td[2]/label[1]")).Text;
                        //    TotalDue1 = driver.FindElement(By.XPath("//*[@id='totals']/table/tbody/tr/td[2]")).Text;

                    }
                    catch { }
                    if (Good_through_date != "" || Installment1 != "")
                    {
                        IWebElement TaxInfo = driver.FindElement(By.XPath("//*[@id='totals']/table/tbody"));
                        IList<IWebElement> TRTaxInfo = TaxInfo.FindElements(By.TagName("tr"));
                        IList<IWebElement> THTaxInfo = TaxInfo.FindElements(By.TagName("th"));
                        IList<IWebElement> TDTaxInfo;
                        foreach (IWebElement row in TRTaxInfo)
                        {
                            TDTaxInfo = row.FindElements(By.TagName("td"));
                            if (TRTaxInfo.Count != 0 && row.Text.Trim() != "")
                            {
                                if (row.Text.Contains("Taxes Due"))
                                {
                                    TaxesDue1 = TDTaxInfo[1].Text;
                                }
                                if (row.Text.Contains("Interest Due") && !row.Text.Contains("Lien"))
                                {
                                    InterestDue1 = TDTaxInfo[1].Text;
                                }
                                if (row.Text.Contains("Misc Due"))
                                {
                                    MiscDue1 = TDTaxInfo[1].Text;
                                }
                                if (row.Text.Contains("Lien Due"))
                                {
                                    LienDue1 = TDTaxInfo[1].Text;
                                }
                                if (row.Text.Contains("Lien Interest Due"))
                                {
                                    LienInterestDue1 = TDTaxInfo[1].Text;
                                }
                                if (row.Text.Contains("Total Due"))
                                {
                                    TotalDue1 = TDTaxInfo[1].Text;
                                }

                            }
                        }
                    }

                    DueDate1 = driver.FindElement(By.XPath("//*[@id='left']/b/p[2]")).Text;
                    DueDate1 = GlobalClass.After(DueDate1, "Payment").Replace("-", "").Trim();
                    DueDate2 = driver.FindElement(By.XPath("//*[@id='left']/b/p[3]")).Text;
                    DueDate2 = GlobalClass.After(DueDate2, "Payment").Replace("-", "").Trim();
                    DueDate3 = driver.FindElement(By.XPath("//*[@id='left']/b/p[5]")).Text;
                    Installment3 = GlobalClass.Before(DueDate3, "Payment").Replace("-", "").Trim();
                    DueDate3 = GlobalClass.After(DueDate3, "Payment").Replace("-", "").Trim();

                    if (Installment1 != "")
                    {
                        string TaxInfo1 = strAccNo + "~" + strownername + "~" + strProAdd + "~" + strMailingAdd + "~" + Installment1 + "~" + TaxesDue1 + "~" + InterestDue1 + "~" + MiscDue1 + "~" + LienDue1 + "~" + LienInterestDue1 + "~" + TotalDue1 + "~" + DueDate1 + "~" + Good_through_date;
                        gc.insert_date(orderNumber, parcelNumber, 1617, TaxInfo1, 1, DateTime.Now);
                    }
                    if (Installment2 != "")
                    {
                        string TaxInfo2 = strAccNo + "~" + strownername + "~" + strProAdd + "~" + strMailingAdd + "~" + Installment2 + "~" + TaxesDue2 + "~" + InterestDue2 + "~" + MiscDue2 + "~" + LienDue2 + "~" + LienInterestDue2 + "~" + TotalDue2 + "~" + DueDate2 + "~" + Good_through_date;
                        gc.insert_date(orderNumber, parcelNumber, 1617, TaxInfo2, 1, DateTime.Now);
                    }
                    if (Installment_Full != "")
                    {
                        if (InterestDue3 == "$0.00")
                        {
                            Good_through_date = DateTime.Now.ToShortDateString();
                        }
                        string TaxInfo3 = strAccNo + "~" + strownername + "~" + strProAdd + "~" + strMailingAdd + "~" + Installment3 + "~" + TaxesDue3 + "~" + InterestDue3 + "~" + MiscDue3 + "~" + LienDue3 + "~" + LienInterestDue3 + "~" + TotalDue_Full + "~" + DueDate3 + "~" + Good_through_date;
                        gc.insert_date(orderNumber, parcelNumber, 1617, TaxInfo3, 1, DateTime.Now);
                    }
                    try
                    {
                        driver.FindElement(By.LinkText("Account value")).SendKeys(Keys.Enter);
                        Thread.Sleep(4000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Tax Distribution Details", driver, "CO", "Weld");
                    }
                    catch { }

                    // Tax Distribution Details

                    try
                    {
                        IWebElement TaxDistribution = driver.FindElement(By.XPath("//*[@id='middle']/table[3]/tbody"));
                        IList<IWebElement> TRTaxDistribution = TaxDistribution.FindElements(By.TagName("tr"));
                        IList<IWebElement> THTaxDistribution = TaxDistribution.FindElements(By.TagName("th"));
                        IList<IWebElement> TDTaxDistribution;
                        foreach (IWebElement row in TRTaxDistribution)
                        {
                            TDTaxDistribution = row.FindElements(By.TagName("td"));
                            if (TDTaxDistribution.Count != 0 && !row.Text.Contains("Tax Rate") && !row.Text.Contains("Credit Levy"))
                            {
                                string TaxDistributiondetails = TDTaxDistribution[0].Text + "~" + TDTaxDistribution[1].Text + "~" + TDTaxDistribution[2].Text + "~" + TDTaxDistribution[3].Text + "~" + TaxAuth;
                                gc.insert_date(orderNumber, parcelNumber, 1619, TaxDistributiondetails, 1, DateTime.Now);
                            }
                        }
                    }
                    catch { }

                    // Tax Due Details

                    try
                    {
                        driver.FindElement(By.LinkText("Transaction Detail")).SendKeys(Keys.Enter);
                        Thread.Sleep(4000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Tax Due Details", driver, "CO", "Weld");
                    }
                    catch { }

                    try
                    {
                        IWebElement TaxDue = driver.FindElement(By.XPath("//*[@id='middle']/table[1]"));
                        IList<IWebElement> TRTaxDue = TaxDue.FindElements(By.TagName("tr"));
                        IList<IWebElement> THTaxDue = TaxDue.FindElements(By.TagName("th"));
                        IList<IWebElement> TDTaxDue;
                        foreach (IWebElement row in TRTaxDue)
                        {
                            TDTaxDue = row.FindElements(By.TagName("td"));
                            if (TDTaxDue.Count != 0 && !row.Text.Contains("Total Due"))
                            {
                                string TDTaxDuedetails = TDTaxDue[0].Text + "~" + TDTaxDue[1].Text + "~" + TDTaxDue[2].Text + "~" + TDTaxDue[3].Text + "~" + TDTaxDue[4].Text + "~" + TDTaxDue[5].Text + "~" + TDTaxDue[6].Text + "~" + TDTaxDue[7].Text;
                                gc.insert_date(orderNumber, parcelNumber, 1620, TDTaxDuedetails, 1, DateTime.Now);
                            }
                        }
                    }
                    catch { }


                    //  Tax History Details

                    try
                    {
                        IWebElement TaxHistory = driver.FindElement(By.XPath("//*[@id='middle']/table[2]/tbody"));
                        IList<IWebElement> TRTaxHistory = TaxHistory.FindElements(By.TagName("tr"));
                        IList<IWebElement> THTaxHistory = TaxHistory.FindElements(By.TagName("th"));
                        IList<IWebElement> TDTaxHistory;
                        foreach (IWebElement row in TRTaxHistory)
                        {
                            TDTaxHistory = row.FindElements(By.TagName("td"));
                            if (TDTaxHistory.Count != 0 && !row.Text.Contains("Effective Date"))
                            {
                                string TDTaxDuedetails = TDTaxHistory[0].Text + "~" + TDTaxHistory[1].Text + "~" + TDTaxHistory[2].Text + "~" + TDTaxHistory[3].Text + "~" + TDTaxHistory[4].Text;
                                gc.insert_date(orderNumber, parcelNumber, 1621, TDTaxDuedetails, 1, DateTime.Now);
                            }
                        }
                    }
                    catch { }

                    // Tax Notice Download
                    try
                    {
                        driver.FindElement(By.LinkText("Web Tax Notice")).Click();
                        Thread.Sleep(10000);
                       // gc.CreatePdf(orderNumber, parcelNumber, "Web Tax Notice Page", driver, "CO", "Weld");
                    }
                    catch { }
                    try
                    {
                        IWebElement assessme = driver.FindElement(By.LinkText("Web Tax Notice"));
                        assessme.Click();
                        Thread.Sleep(7000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Tax Notice", driver, "CO", "Weld");
                        // string download = assessme.GetAttribute("href");
                        // driver.Navigate().GoToUrl(download);
                       // Thread.Sleep(4000);
                      //  driver.SwitchTo().Window(driver.WindowHandles.Last());
                       // Thread.Sleep(2000);
                       
                       // gc.downloadfile(download, orderNumber, parcelNumber, "Tax Download", "CO", "Weld");
                    }
                    catch { }

                  //  // Tax Notice Download
                  //  var chromeOptions = new ChromeOptions();

                  //  var downloadDirectory = ConfigurationManager.AppSettings["AutoPdf"];

                  //  chromeOptions.AddUserProfilePreference("download.default_directory", downloadDirectory);
                  //  chromeOptions.AddUserProfilePreference("download.prompt_for_download", false);
                  //  chromeOptions.AddUserProfilePreference("disable-popup-blocking", "true");
                  //  chromeOptions.AddUserProfilePreference("plugins.always_open_pdf_externally", true);
                  //  var driver1 = new ChromeDriver(chromeOptions);
                  //  driver1.Navigate().GoToUrl(driver.Url);
                    
                  //  try
                  //  {
                  //      driver1.FindElement(By.XPath("//*[@id='middle_left']/form/input[1]")).Click();
                  //      Thread.Sleep(4000);
                  //  }
                  //  catch { }

                  //  string fileName = "";
                  //  try
                  //  {
                  //      driver1.FindElement(By.LinkText("Web Tax Notice")).Click();
                  //      Thread.Sleep(7000);
                  //      gc.CreatePdf(orderNumber, parcelNumber, "Web Tax Notice Page", driver1, "CO", "Weld");
                  //  }
                  //  catch { }
                  // // driver1.SwitchTo().Window(driver1.WindowHandles.Last());
                  ////  Thread.Sleep(2000);
                  //  try
                  //  {
                  //      IWebElement TaxBody = driver1.FindElement(By.Id("myReports"));
                  //      IList<IWebElement> TRTaxBody = TaxBody.FindElements(By.TagName("tr"));
                  //      IList<IWebElement> THTaxBody = TaxBody.FindElements(By.TagName("th"));
                  //      IList<IWebElement> TDTaxBody;
                  //      foreach (IWebElement row in TRTaxBody)
                  //      {
                  //          TDTaxBody = row.FindElements(By.TagName("td"));
                  //          if (TDTaxBody.Count != 0 && !row.Text.Contains("Report Name") && row.Text.Trim() != "")
                  //          {
                  //              IWebElement ItaxReceipt = TDTaxBody[0].FindElement(By.TagName("a"));
                  //              string strTaxtURL = ItaxReceipt.GetAttribute("href");
                  //              driver1.Navigate().GoToUrl(strTaxtURL);
                  //              Thread.Sleep(7000);
                  //              gc.CreatePdf(orderNumber, parcelNumber, "Tax Notice", driver1, "CO", "Weld");

                  //          }
                  //      }
                      
                  //  }
                  //  catch { }
                  //  fileName = "Web Tax Notice.pdf";
                  //  try
                  //  {
                  //      IWebElement Iclick = driver1.FindElement(By.LinkText("Web Tax Notice"));

                  //      gc.AutoDownloadFile(orderNumber, parcelNumber, "Weld", "CO", fileName);
                  //      Thread.Sleep(5000);
                       
                  //  }
                  //  catch (Exception ex)
                  //  {
                  //      driver1.Quit();
                  //  }

                   


                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "CO", "Weld", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    //driver1.Quit();
                    driver.Quit();
                    gc.mergpdf(orderNumber, "CO", "Weld");
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