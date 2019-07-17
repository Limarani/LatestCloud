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
using iTextSharp.text.pdf.parser;
using iTextSharp.text.pdf;

namespace ScrapMaricopa.Scrapsource
{
    public class WebDriver_CowlitzWA
    {
        string propertydetails = "", Parcel_Id = "", Pro_Id = "", Jurisdiction = "", Acres = "", Township = "", Pro_Use = "", Neighberhood = "", Tax_Code = "", Exemptions = "", Leavy_Rate = "", Primary_Owner = "", Address1 = "", Address2 = "", Address = "", Year_Built = "";
        string Assessment_details = "", Asse_Year = "", TaxPay_year = "", Land_Value = "", Impr_Value = "", Totl_AsseValue = "", Notice_Value = "";
        string TaxPayment_details = "", Tax_Year = "", Stmt_Id = "", Taxes = "", Assesments = "", Totl_Charges = "", Totl_Paid = "", Totl_Due = "";
        string fulladdress = "", multi_details = "", parcelno = "", Pro_Id1 = "", ChkMultiParcel = "", Tax_Authority = "", Taxauthority_Details = "";

        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();

        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());

        public string FTP_Cowlitz(string streetno, string direction, string streetname, string streettype, string parcelNumber, string ownername, string searchType, string orderNumber, string directParcel, string unitnumber)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            List<string> mcheck = new List<string>();

            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";

            //driver = new c();
            //driver = new PhantomJSDriver();

            //var option = new ChromeOptions();
            //option.AddArgument("No-Sandbox");

            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
          //  using (driver = new ChromeDriver())
              using (driver = new PhantomJSDriver())
            {
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");

                    driver.Navigate().GoToUrl("http://www.cowlitzinfo.net/applications/cowlitzassessorparcelsearch/Default.aspx");
                    Thread.Sleep(3000);


                    if (searchType == "titleflex")
                    {
                        string address = "";
                        if (direction != "")
                        {
                            address = streetno + " " + streetname + " " + direction;
                        }
                        else
                        {
                            address = streetno + " " + streetname;
                        }
                        gc.TitleFlexSearch(orderNumber,parcelNumber, ownername, address.Trim(), "WA", "Cowlitz");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Cowlitz_Zero"] = "Zero";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString().Replace("-", "");
                        searchType = "parcel";
                    }


                    if (searchType == "address")
                    {
                        driver.FindElement(By.XPath("//*[@id='address_input_street_number']")).Click();
                        driver.FindElement(By.XPath("//*[@id='address_input_street_number']")).SendKeys(streetno);

                        driver.FindElement(By.XPath("//*[@id='address_input_street_name']")).Click();
                        driver.FindElement(By.XPath("//*[@id='address_input_street_name']")).SendKeys(streetname);

                        try
                        {
                            driver.FindElement(By.XPath("//*[@id='address_input_street_direction']")).SendKeys(direction);
                        }
                        catch
                        { }

                        gc.CreatePdf_WOP(orderNumber, "Address Search", driver, "WA", "Cowlitz");
                        driver.FindElement(By.Id("submit_input")).SendKeys(Keys.Enter);
                        Thread.Sleep(12000);
                        gc.CreatePdf_WOP(orderNumber, "Address Search Result", driver, "WA", "Cowlitz");

                        ChkMultiParcel = driver.FindElement(By.XPath("//*[@id='search-filter']/figure")).Text.Replace("\r\n", "");
                        
                        if (ChkMultiParcel == "Search Results:1Sort By ")
                        {
                            driver.FindElement(By.XPath("//*[@id='properties_section']/div/a/div/div/div[2]/div/div[2]")).Click();
                            Thread.Sleep(10000);
                        }
                        else
                        {
                            try
                            {
                                int AddressmaxCheck = 0;

                                IWebElement add_search = driver.FindElement(By.XPath("//*[@id='properties_section']/div"));
                                IList<IWebElement> TRadd_search = add_search.FindElements(By.TagName("div"));
                                IList<IWebElement> TDadd_search;

                                foreach (IWebElement row in TRadd_search)
                                {
                                    if (AddressmaxCheck <= 25)
                                    {
                                        string addrerss1 = row.GetAttribute("class");
                                        if (addrerss1 == "info")
                                        {
                                            TDadd_search = row.FindElements(By.TagName("div"));
                                            if (TDadd_search.Count != 0)
                                            {
                                                parcelno = TDadd_search[0].Text;
                                                Pro_Id1 = TDadd_search[1].Text;
                                                Pro_Id1 = WebDriverTest.After(Pro_Id1, "Prop ID: ");
                                            }

                                            TDadd_search = row.FindElements(By.TagName("h3"));
                                            if (TDadd_search.Count != 0)
                                            {
                                                fulladdress = TDadd_search[0].Text;
                                            }

                                            multi_details = Pro_Id1 + "~" + fulladdress;
                                            gc.insert_date(orderNumber, parcelno, 815, multi_details, 1, DateTime.Now);
                                        }
                                        AddressmaxCheck++;
                                    }
                                }

                                if (TRadd_search.Count < 27)
                                {
                                    gc.CreatePdf_WOP(orderNumber, "Multi Address search result", driver, "WA", "Cowlitz");
                                    HttpContext.Current.Session["multiparcel_CowlitzWA"] = "Yes";
                                    driver.Quit();
                                    return "MultiParcel";
                                }
                                if (TRadd_search.Count >= 27)
                                {
                                    HttpContext.Current.Session["multiParcel_CowlitzWA_Multicount"] = "Maximum";
                                    driver.Quit();
                                    return "Maximum";
                                }
                            }
                            catch
                            { }
                            
                        }

                        try
                        {
                            string Nodata = driver.FindElement(By.XPath("//*[@id='search-filter']/figure/h3")).Text;
                            if (Nodata.Contains("Search Results:"))
                            {
                                HttpContext.Current.Session["Cowlitz_Zero"] = "Zero";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }

                        //IWebElement add_search = driver.FindElement(By.XPath("//*[@id='ctl00_cphParcelSearch_gvSearchResults']/tbody"));
                        //IList<IWebElement> TRadd_search = add_search.FindElements(By.TagName("tr"));

                        //IList<IWebElement> TDadd_search;
                        //foreach (IWebElement row in TRadd_search)
                        //{
                        //    TDadd_search = row.FindElements(By.TagName("td"));
                        //    if (TRadd_search.Count > 2 && TDadd_search.Count != 0)
                        //    {
                        //        string straccount_no = TDadd_search[1].Text;
                        //        string parcel_no = TDadd_search[9].Text;
                        //        string Address_Details = TDadd_search[2].Text + " " + TDadd_search[3].Text + " " + TDadd_search[4].Text + " " + TDadd_search[5].Text + " " + TDadd_search[6].Text + " " + TDadd_search[7].Text;

                        //        gc.insert_date(orderNumber, parcel_no, 815, straccount_no + "~" + Address_Details, 1, DateTime.Now);
                        //    }
                        //    if (TRadd_search.Count == 2)
                        //    {
                        //        driver.FindElement(By.XPath("//*[@id='ctl00_cphParcelSearch_gvSearchResults']/tbody/tr[2]/td[1]/a")).SendKeys(Keys.Enter);
                        //        Thread.Sleep(5000);
                        //        break;
                        //    }
                        //}
                        //if (TRadd_search.Count < 27 && TRadd_search.Count > 2)
                        //{
                        //    gc.CreatePdf_WOP(orderNumber, "Multi Address search result", driver, "WA", "Cowlitz");
                        //    HttpContext.Current.Session["multiparcel_CowlitzWA"] = "Yes";
                        //    driver.Quit();
                        //    return "MultiParcel";
                        //}
                        //if (TRadd_search.Count >= 27 && TRadd_search.Count > 2)
                        //{
                        //    HttpContext.Current.Session["multiParcel_CowlitzWA_Multicount"] = "Maximum";
                        //    driver.Quit();
                        //    return "Maximum";
                        //}
                    }

                    if (searchType == "parcel")
                    {
                        driver.FindElement(By.Id("radSearchType_1")).Click();
                        Thread.Sleep(2000);

                        driver.FindElement(By.XPath("//*[@id='parcel_input']")).Click();
                        driver.FindElement(By.XPath("//*[@id='parcel_input']")).SendKeys(parcelNumber);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel search", driver, "WA", "Cowlitz");
                        driver.FindElement(By.XPath("//*[@id='submit_input']")).SendKeys(Keys.Enter);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel search result", driver, "WA", "Cowlitz");
                        Thread.Sleep(2000);

                        driver.FindElement(By.XPath("//*[@id='properties_section']/div/div/div/div[1]/a")).SendKeys(Keys.Enter);
                        Thread.Sleep(12000);

                        try
                        {
                            string Nodata = driver.FindElement(By.XPath("//*[@id='search-filter']/figure/h3")).Text;
                            if (Nodata.Contains("Search Results:"))
                            {
                                HttpContext.Current.Session["Cowlitz_Zero"] = "Zero";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }

                    try
                    {
                        // Property Details
                        Parcel_Id = driver.FindElement(By.XPath("//*[@id='general_property_info']/dl/dd[1]")).Text;
                        Pro_Id = driver.FindElement(By.XPath("//*[@id='general_property_info']/dl/dd[2]")).Text;
                        Jurisdiction = driver.FindElement(By.XPath("//*[@id='general_property_info']/dl/dd[3]")).Text;
                        Acres = driver.FindElement(By.XPath("//*[@id='general_property_info']/dl/dd[4]")).Text;
                        Township = driver.FindElement(By.XPath("//*[@id='general_property_info']/dl/dd[7]")).Text;
                        Pro_Use = driver.FindElement(By.XPath("//*[@id='general_property_info']/dl/dd[8]")).Text;
                        Neighberhood = driver.FindElement(By.XPath("//*[@id='general_property_info']/dl/dd[9]")).Text;
                        Tax_Code = driver.FindElement(By.XPath("//*[@id='general_property_info']/dl/dd[10]")).Text;
                        Exemptions = driver.FindElement(By.XPath("//*[@id='general_property_info']/dl/dd[11]")).Text;
                        Leavy_Rate = driver.FindElement(By.XPath("//*[@id='general_property_info']/dl/dt[12]")).Text;
                        Leavy_Rate = WebDriverTest.Between(Leavy_Rate, "Rate = ", ")");
                        Primary_Owner = driver.FindElement(By.XPath("//*[@id='owner_info']/dl/dd[1]")).Text;
                        Address1 = driver.FindElement(By.XPath("//*[@id='owner_info']/dl/dd[2]")).Text;
                        Address2 = driver.FindElement(By.XPath("//*[@id='owner_info']/dl/dd[3]")).Text;

                        Address = Address1 + " " + Address2;
                        Year_Built = driver.FindElement(By.XPath("//*[@id='property_detail_info']/dl/dd[1]")).Text;

                        gc.CreatePdf(orderNumber, parcelNumber, "Property Details", driver, "WA", "Cowlitz");
                        propertydetails = Primary_Owner + "~" + Address + "~" + Pro_Id + "~" + Jurisdiction + "~" + Acres + "~" + Township + "~" + Pro_Use + "~" + Neighberhood + "~" + Tax_Code + "~" + Exemptions + "~" + Leavy_Rate + "~" + Year_Built;
                        gc.insert_date(orderNumber, Parcel_Id, 763, propertydetails, 1, DateTime.Now);

                        //Assessment Details

                        IWebElement AssessmentTB = driver.FindElement(By.XPath("//*[@id='assess_value_table']/table/tbody"));
                        IList<IWebElement> AssessmentTR = AssessmentTB.FindElements(By.TagName("tr"));
                        IList<IWebElement> AssessmentTD;

                        foreach (IWebElement Assessment in AssessmentTR)
                        {
                            AssessmentTD = Assessment.FindElements(By.TagName("td"));
                            if (AssessmentTD.Count != 0)
                            {
                                Asse_Year = AssessmentTD[0].Text;
                                TaxPay_year = AssessmentTD[1].Text;
                                Land_Value = AssessmentTD[2].Text;
                                Impr_Value = AssessmentTD[3].Text;
                                Totl_AsseValue = AssessmentTD[4].Text;
                                Notice_Value = AssessmentTD[5].Text;

                                Assessment_details = Asse_Year + "~" + TaxPay_year + "~" + Land_Value + "~" + Impr_Value + "~" + Totl_AsseValue + "~" + Notice_Value;
                                gc.insert_date(orderNumber, Parcel_Id, 765, Assessment_details, 1, DateTime.Now);
                            }
                        }

                        gc.CreatePdf(orderNumber, parcelNumber, "Assessment Details", driver, "WA", "Cowlitz");

                        //Tax Payment Details

                        IWebElement TaxPaymentTB = driver.FindElement(By.XPath("//*[@id='tax_value_table']/table/tbody"));
                        IList<IWebElement> TaxPaymentTR = TaxPaymentTB.FindElements(By.TagName("tr"));
                        IList<IWebElement> TaxPaymentTD;

                        foreach (IWebElement TaxPayment in TaxPaymentTR)
                        {
                            TaxPaymentTD = TaxPayment.FindElements(By.TagName("td"));
                            if (TaxPaymentTD.Count != 0)
                            {
                                Tax_Year = TaxPaymentTD[0].Text;
                                Stmt_Id = TaxPaymentTD[1].Text;
                                Taxes = TaxPaymentTD[2].Text;
                                Assesments = TaxPaymentTD[3].Text;
                                Totl_Charges = TaxPaymentTD[4].Text;
                                Totl_Paid = TaxPaymentTD[5].Text;
                                Totl_Due = TaxPaymentTD[6].Text;

                                TaxPayment_details = Tax_Year + "~" + Stmt_Id + "~" + Taxes + "~" + Assesments + "~" + Totl_Charges + "~" + Totl_Paid + "~" + Totl_Due;
                                gc.insert_date(orderNumber, Parcel_Id, 773, TaxPayment_details, 1, DateTime.Now);
                            }
                        }

                        //Pdf download
                        IWebElement Receipttable = driver.FindElement(By.XPath("//*[@id='tax_value_table']/table/tbody"));
                        IList<IWebElement> ReceipttableRow = Receipttable.FindElements(By.TagName("tr"));
                        int rowcount = ReceipttableRow.Count;

                        for (int p = 1; p <= rowcount; p++)
                        {
                            if (p < 4)
                            {
                                string Parent_Window1 = driver.CurrentWindowHandle;

                                driver.FindElement(By.XPath("//*[@id='tax_value_table']/table/tbody/tr[" + p + "]/td[10]/a")).Click();
                                Thread.Sleep(10000);
                               

                                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                                driver.SwitchTo().Window(driver.WindowHandles.Last());
                                Thread.Sleep(4000);

                            

                                try
                                {
                                    var chromeOptions = new ChromeOptions();
                                    var downloadDirectory = ConfigurationManager.AppSettings["AutoPdf"];                                  
                                    chromeOptions.AddUserProfilePreference("download.default_directory", downloadDirectory);
                                    chromeOptions.AddUserProfilePreference("download.prompt_for_download", false);
                                    chromeOptions.AddUserProfilePreference("disable-popup-blocking", true);
                                    chromeOptions.AddUserProfilePreference("plugins.always_open_pdf_externally", true);
                                    var chDriver = new ChromeDriver(chromeOptions);
                                    Array.ForEach(Directory.GetFiles(@downloadDirectory), File.Delete);
                                    chDriver.Navigate().GoToUrl(driver.Url);
                                    Thread.Sleep(8000);

                                    //PopUp
                                    try
                                    {
                                        chDriver.SwitchTo().Alert().Accept();
                                        Thread.Sleep(1000);
                                    }
                                    catch
                                    { }
                                    IWebElement ISpan12 = chDriver.FindElement(By.Id("PdfDialog_PdfDownloadLink"));
                                    IJavaScriptExecutor js12 = chDriver as IJavaScriptExecutor;
                                    js12.ExecuteScript("arguments[0].click();", ISpan12);
                                    Thread.Sleep(5000);

                                    chDriver.FindElement(By.Id("PdfDialog_download")).Click();
                                    Thread.Sleep(20000);

                                    string fileName1 = latestfilename();
                                    Thread.Sleep(2000);
                                    gc.AutoDownloadFile(orderNumber, Parcel_Id, "Cowlitz", "WA", fileName1);
                                    chDriver.Quit();
                                }
                                catch { }

                                driver.SwitchTo().Window(Parent_Window1);
                                Thread.Sleep(2000);
                            }
                        }

                        //Tax Authority
                        driver.FindElement(By.XPath("//*[@id='top']/nav/ul/li[2]/a")).Click();
                        Thread.Sleep(2000);

                        //driver.SwitchTo().Window(driver.WindowHandles.Last());
                        //Thread.Sleep(2000);

                        Tax_Authority = driver.FindElement(By.XPath("//*[@id='address']")).Text;
                        Tax_Authority = WebDriverTest.Between(Tax_Authority, "Address", "Option 1").Replace("\r\n","");

                        Taxauthority_Details = Tax_Authority;
                        gc.insert_date(orderNumber, Parcel_Id, 780, Taxauthority_Details, 1, DateTime.Now);
                    }
                    catch
                    { }

                    //IWebElement parcel_id = driver.FindElement(By.Id("ctl00_cphParcelSearch_txtParcel"));
                    //string Parcel_Id = parcel_id.GetAttribute("value");
                    //gc.CreatePdf(orderNumber, Parcel_Id, "Property Details", driver, "WA", "Cowlitz");
                    //IWebElement account_no = driver.FindElement(By.XPath("//*[@id='ctl00_cphParcelSearch_lnkAccount']"));
                    //string Account_No = account_no.Text;
                    //IWebElement jurisdiction = driver.FindElement(By.XPath("//*[@id='ctl00_cphParcelSearch_txtJurisdiction']"));
                    //string Jurisdiction = jurisdiction.GetAttribute("value");
                    //IWebElement owner_name = driver.FindElement(By.XPath("//*[@id='ctl00_cphParcelSearch_txtOwner']"));
                    //string Owner_Name = owner_name.GetAttribute("value");
                    //IWebElement mailing_address1 = driver.FindElement(By.XPath("//*[@id='ctl00_cphParcelSearch_txtAddress1']"));
                    //string Mailing_Address1 = mailing_address1.GetAttribute("value");
                    //IWebElement mailing_address2 = driver.FindElement(By.XPath("//*[@id='ctl00_cphParcelSearch_txtAddress2']"));
                    //string Mailing_Address2 = mailing_address2.GetAttribute("value");
                    //IWebElement tax_district = driver.FindElement(By.XPath("//*[@id='ctl00_cphParcelSearch_lnkTaxDistrict']"));
                    //string Tax_District = tax_district.Text;
                    //IWebElement neighbor_hood = driver.FindElement(By.XPath("//*[@id='ctl00_cphParcelSearch_txtNeighborhood']"));
                    //string Neighbor_Hood = neighbor_hood.GetAttribute("value");
                    //IWebElement levy_rate = driver.FindElement(By.XPath("//*[@id='ctl00_cphParcelSearch_txtLevyRate']"));
                    //string Levy_Rate = levy_rate.GetAttribute("value");
                    //IWebElement legal_desc = driver.FindElement(By.XPath("//*[@id='ctl00_cphParcelSearch_txtLegalDescr']"));
                    //string Legal_Desc = legal_desc.GetAttribute("value");
                    //IWebElement property_address = driver.FindElement(By.XPath("//*[@id='ctl00_cphParcelSearch_txtSitus']"));
                    //string Property_Address = property_address.GetAttribute("value");
                    //string propertydetails = Account_No + "~" + Jurisdiction + "~" + Owner_Name + "~" + Mailing_Address1 + Mailing_Address2 + "~" + Legal_Desc + "~" + Property_Address + "~" + Tax_District + "~" + Neighbor_Hood + "~" + Levy_Rate;
                    //gc.insert_date(orderNumber, Parcel_Id, 763, propertydetails, 1, DateTime.Now);

                    // Assessment Details table

                    //IWebElement tbmulti = driver.FindElement(By.XPath("//*[@id='ctl00_cphParcelSearch_tblAssessmentInformation']/tbody"));
                    //IList<IWebElement> TRmulti = tbmulti.FindElements(By.TagName("tr"));

                    //IList<IWebElement> TDmulti;
                    //foreach (IWebElement row in TRmulti)
                    //{

                    //    TDmulti = row.FindElements(By.TagName("td"));
                    //    if (TDmulti.Count == 7 && !row.Text.Contains("Assessment\r\nYear"))
                    //    {
                    //        string multi1 = TDmulti[0].Text + "~" + TDmulti[1].Text + "~" + TDmulti[2].Text + "~" + TDmulti[3].Text + "~" + TDmulti[4].Text + "~" + TDmulti[5].Text;
                    //        gc.insert_date(orderNumber, Parcel_Id, 765, multi1, 1, DateTime.Now);
                    //    }
                    //}

                    //  Payment History Table
                    //string currenturl = "";
                    //driver.FindElement(By.XPath("//*[@id='ctl00_cphParcelSearch_lnkbtnTransactionHistoricalValues']")).SendKeys(Keys.Enter);
                    //Thread.Sleep(5000);
                    //gc.CreatePdf(orderNumber, Parcel_Id, "TaxPaymentHistory", driver, "WA", "Cowlitz");
                    //try
                    //{
                    //    currenturl = driver.CurrentWindowHandle;
                    //    driver.FindElement(By.XPath("//*[@id='ctl00_cphParcelSearch_tblTransactionValues']/tbody/tr[3]/td[9]/a")).SendKeys(Keys.Enter);
                    //    Thread.Sleep(5000);

                    //    driver.SwitchTo().Window(driver.WindowHandles.Last());

                    //    IWebElement tbDelinq = driver.FindElement(By.XPath("//*[@id='ctl00_cphParcelSearch_tblTrxDetail']/tbody"));
                    //    IList<IWebElement> TRDelinq = tbDelinq.FindElements(By.TagName("tr"));

                    //    IList<IWebElement> TDDelinq;
                    //    foreach (IWebElement row in TRDelinq)
                    //    {

                    //        TDDelinq = row.FindElements(By.TagName("td"));
                    //        if (TDDelinq.Count == 5 && !row.Text.Contains("Assess\r\nYear") && !row.Text.Contains("Total Tax"))
                    //        {
                    //            string Delinq = TDDelinq[0].Text + "~" + TDDelinq[1].Text + "~" + TDDelinq[2].Text + "~" + TDDelinq[3].Text + "~" + TDDelinq[4].Text;
                    //            gc.insert_date(orderNumber, Parcel_Id, 817, Delinq, 1, DateTime.Now);
                    //        }

                    //        if (TDDelinq.Count == 5 && row.Text.Contains("Total Tax") && !row.Text.Contains("Assess\r\nYear"))
                    //        {
                    //            string Delinq = TDDelinq[3].Text + "~" + "~" + "~" + "~" + TDDelinq[4].Text;
                    //            gc.insert_date(orderNumber, Parcel_Id, 817, Delinq, 1, DateTime.Now);
                    //        }
                    //    }


                    //}
                    //catch (Exception ex)
                    //{

                    //}
                    //driver.SwitchTo().Window(currenturl);
                    //List<string> TaxYearDetails = new List<string>();
                    //List<string> TaxBillDetails = new List<string>();
                    //IWebElement tbmulti1 = driver.FindElement(By.XPath("//*[@id='ctl00_cphParcelSearch_tblTransactionValues']/tbody"));
                    //IList<IWebElement> TRmulti1 = tbmulti1.FindElements(By.TagName("tr"));
                    //IList<IWebElement> TDmulti1;
                    //foreach (IWebElement row in TRmulti1)
                    //{
                    //    TDmulti1 = row.FindElements(By.TagName("td"));
                    //    if ((TDmulti1.Count == 9 || TDmulti1.Count == 8) && !(row.Text.Contains("Assessment\r\nYear")))
                    //    {
                    //        string multi2 = TDmulti1[0].Text + "~" + TDmulti1[1].Text + "~" + TDmulti1[2].Text + "~" + TDmulti1[3].Text + "~" + TDmulti1[4].Text + "~" + TDmulti1[5].Text + "~" + TDmulti1[6].Text;

                    //        gc.insert_date(orderNumber, Parcel_Id, 773, multi2, 1, DateTime.Now);

                    //        if (TDmulti1.Count != 0 && TaxYearDetails.Count < 3)
                    //        {
                    //            IWebElement IYear = TDmulti1[1].FindElement(By.TagName("a"));
                    //            string strYear = IYear.GetAttribute("href");
                    //            TaxYearDetails.Add(strYear);
                    //        }
                    //        if (TDmulti1.Count != 0 && TaxBillDetails.Count < 3)
                    //        {
                    //            IWebElement ITaxBill = TDmulti1[7].FindElement(By.TagName("a"));
                    //            string strTaxBill = ITaxBill.GetAttribute("href");
                    //            TaxBillDetails.Add(strTaxBill);
                    //        }

                    //    }
                    //}

                    //int k = 0, l = 0;
                    //foreach (string yearURL in TaxYearDetails)
                    //{
                    //    driver.Navigate().GoToUrl(yearURL);

                    //    gc.CreatePdf(orderNumber, Parcel_Id, "Tax Detail" + k, driver, "WA", "Cowlitz");
                    //    k++;
                    //    try
                    //    {
                    //        IWebElement tbmulti2 = driver.FindElement(By.XPath("//*[@id='ctl00_cphParcelSearch_tblTrxDetail']/tbody"));
                    //        IList<IWebElement> TRmulti2 = tbmulti2.FindElements(By.TagName("tr"));
                    //        IList<IWebElement> TDmulti2;
                    //        foreach (IWebElement Tax in TRmulti2)
                    //        {
                    //            TDmulti2 = Tax.FindElements(By.TagName("td"));
                    //            if ((TDmulti2.Count == 6) && !Tax.Text.Contains("Tax Detail") && !Tax.Text.Contains("Account"))
                    //            {
                    //                string taxing_authority = "Cowlitz County Treasurer 207 4th Ave. N.Kelso, WA 98626";
                    //                string multi3 = TDmulti2[0].Text + "~" + TDmulti2[1].Text + "~" + TDmulti2[2].Text + "~" + TDmulti2[3].Text + "~" + TDmulti2[4].Text + "~" + TDmulti2[5].Text;
                    //                string multi4 = multi3 + "~" + taxing_authority;
                    //                gc.insert_date(orderNumber, Parcel_Id, 780, multi4, 1, DateTime.Now);
                    //            }
                    //        }
                    //    }
                    //    catch { }
                    //}
                    //int j = 0;
                    //foreach (string billURL in TaxBillDetails)
                    //{
                    //    driver.Navigate().GoToUrl(billURL);
                    //    IWebElement Ibill = driver.FindElement(By.XPath("//*[@id='PageToolbar']"));
                    //    IList<IWebElement> TRmulti5 = Ibill.FindElements(By.TagName("a"));
                    //    IList<IWebElement> TDmulti5;
                    //    foreach (IWebElement Tax in TRmulti5)
                    //    {
                    //        TDmulti5 = Tax.FindElements(By.TagName("img"));
                    //        if (TDmulti5.Count != 0)
                    //        {
                    //            IWebElement itaxbill = TDmulti5[0];
                    //            string strtaxbill = itaxbill.GetAttribute("id");
                    //            if (strtaxbill.Contains("FullScreenButton"))
                    //            {

                    //                IJavaScriptExecutor js = driver as IJavaScriptExecutor;
                    //                js.ExecuteScript("arguments[0].click();", itaxbill);
                    //                Thread.Sleep(6000);
                    //            }

                    //        }
                    //    }
                    //    gc.CreatePdf(orderNumber, Parcel_Id, "TaxBill Download" + j, driver, "WA", "Cowlitz");
                    //    try
                    //    {
                    //        //gc.downloadfile(billURL, orderNumber, Parcel_Id, "Paid Bill" + j, "WA", "Cowlitz");
                    //    }
                    //    catch { }
                    //    j++;
                    //}

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "WA", "Cowlitz", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    gc.mergpdf(orderNumber, "WA", "Cowlitz");
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