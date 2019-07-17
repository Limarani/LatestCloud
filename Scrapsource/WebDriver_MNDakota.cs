using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System.Threading;
using OpenQA.Selenium.Interactions;
using MySql.Data.MySqlClient;
using System.Configuration;
using System.Drawing;
using OpenQA.Selenium.PhantomJS;

using System.Windows.Input;
namespace ScrapMaricopa.Scrapsource
{
    public class WebDriver_MNDakota
    {
        string outputPath = "";
        IWebDriver driver;
        DBconnection db = new DBconnection();

        GlobalClass gc = new GlobalClass();
        MySqlParameter[] mParam;
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());

        public string FTP_MNDakota(string houseno, string sname, string unitno, string parcelNumber, string searchType, string orderNumber, string ownername, string directParcel)
        {
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "", AssessTakenTime = "", TaxTakentime = "", CityTaxtakentime = "";
            string TotaltakenTime = "";
            string OwnerName = "", JointOwnerName = "", PropertyAddress = "", MailingAddress = "", Municipality = "", PropertyUse = "", YearBuilt = "", LegalDescription = "", parcel_id = "";

            List<string> strTaxRealestate = new List<string>();
            List<string> strTaxRealestate1 = new List<string>();
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            //IWebElement iframeElement1;
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            //driver = new PhantomJSDriver();
            driver = new ChromeDriver();
            var option = new ChromeOptions();
            option.AddArgument("No-Sandbox");
            using (driver = new ChromeDriver(option))
            {
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");
                    driver.Navigate().GoToUrl("https://gis2.co.dakota.mn.us/WAB/PropertyInformationPublic/index.html");
                    Thread.Sleep(20000);
                    // driver.SwitchTo().Window(driver.WindowHandles.Last());
                    try
                    {

                        IWebElement ISpan12 = driver.FindElement(By.XPath("//*[@id='widgets_Splash_Widget_71']/div[2]/div[2]/div[2]/div[2]"));
                        IJavaScriptExecutor js12 = driver as IJavaScriptExecutor;
                        js12.ExecuteScript("arguments[0].click();", ISpan12);
                        Thread.Sleep(9000);
                    }
                    catch { }
                    if (searchType == "address")
                    {

                        driver.FindElement(By.Id("esri_dijit_Search_0_input")).SendKeys(houseno + " " + sname);
                        gc.CreatePdf_WOP(orderNumber, "Address Search", driver, "MN", "Dakota");
                        driver.FindElement(By.XPath("//*[@id='esri_dijit_Search_0']/div/div[2]")).Click();
                        //   gc.CreatePdf_WOP(orderNumber, "Address Search result", driver, "MN", "Dakota");
                        try
                        {
                            IWebElement Inodata = driver.FindElement(By.XPath("//*[@id='esri_dijit_Search_0']/div/div[4]/div"));
                            if (Inodata.Text.Contains("No results") || Inodata.Text.Contains("no results found"))
                            {
                                HttpContext.Current.Session["Nodata_MNDakota"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                        Thread.Sleep(10000);
                        try
                        {
                            driver.FindElement(By.XPath("//*[@id='widgets_Search_Widget_60']/div[2]/div[2]/ul/li")).Click();
                            Thread.Sleep(2000);
                        }
                        catch { }
                        try
                        {
                            int i, j = 0;
                            int iRowsCount = driver.FindElements(By.XPath("//*[@id='widgets_Search_Widget_60']/div[2]/div[2]/ul/li")).Count;
                            if (iRowsCount > 1)
                            {
                                if (j < 25)
                                {

                                    for (i = 1; i <= iRowsCount; i++)
                                    {
                                        ////*[@id="widgets_Search_Widget_60"]/div[2]/div[2]/ul
                                        string add1 = driver.FindElement(By.XPath("//*[@id='widgets_Search_Widget_60']/div[2]/div[2]/ul/li[" + i + "]")).Text;
                                        driver.FindElement(By.XPath("//*[@id='widgets_Search_Widget_60']/div[2]/div[2]/ul/li[" + i + "]")).Click();
                                        Thread.Sleep(3000);
                                        driver.SwitchTo().Window(driver.WindowHandles.Last());
                                        Thread.Sleep(1000);
                                        string fulltext4 = driver.FindElement(By.XPath("//*[@id='dijit_layout_ContentPane_0']")).Text.Trim().Replace("\r\n", "");
                                        parcel_id = gc.Between(fulltext4, "Parcel ID:", "Property Details").Trim().Replace("-", "");
                                        PropertyAddress = gc.Between(fulltext4, "Parcel ID:", "Property Details").Trim().Replace("-", "");
                                        // string s1 = parcel_id;
                                        string[] words1 = parcel_id.Split(' ');
                                        parcel_id = words1[0];

                                        parcel_id = parcel_id.Substring(0, 12);
                                        PropertyAddress = WebDriverTest.After(PropertyAddress, parcel_id).Trim();
                                        //string parcel = driver.FindElement(By.XPath("/html/body/div[2]/div[1]/div[4]/div[2]/div/div/div[2]/div/div[1]/div[1]")).Text.Trim().Replace("Parcel ID:", "");
                                        //string fulltext1 = driver.FindElement(By.XPath("/html/body/div[2]/div[1]/div[4]/div[2]/div/div/div[2]/div/div[1]/div[3]/font[4]/table/tbody")).Text.Trim().Replace("\r\n", "");

                                        string Owner = gc.Between(fulltext4, "Owner", "Joint Owner").Trim();
                                        string multi = add1 + "~" + Owner;
                                        gc.insert_date(orderNumber, parcel_id, 464, multi, 1, DateTime.Now);
                                        driver.FindElement(By.XPath("//*[@id='esri_dijit_Search_0']/div/div[2]")).Click();
                                        Thread.Sleep(4000);
                                    }
                                    j++;
                                }

                                if (iRowsCount > 25)
                                {
                                    HttpContext.Current.Session["multiParcel_MNDakota_Multicount"] = "Maximum";
                                }
                                else
                                {
                                    HttpContext.Current.Session["multiparcel_MNDakota"] = "Yes";
                                }
                                driver.Quit();

                                return "MultiParcel";
                            }

                            else
                            {
                                try { 
                                driver.FindElement(By.XPath("//*[@id='widgets_Search_Widget_60']/div[2]/div[2]/ul/li")).Click();
                                Thread.Sleep(8000);
                                }
                                catch { }
                            }


                        }
                        catch { }

                    }

                    if (searchType == "titleflex")
                    {
                        string titleaddress = houseno + " " + sname + " " + unitno;
                        gc.TitleFlexSearch(orderNumber, "", "", titleaddress, "MN", "Dakota");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_MNDakota"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }


                    if (searchType == "parcel")
                    {
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();

                        }

                        if (parcelNumber.Contains("-"))
                        {
                            parcel_id = parcelNumber.Replace("-", "");
                        }
                        else
                            parcel_id = parcelNumber;

                        driver.FindElement(By.Id("esri_dijit_Search_0_input")).SendKeys(parcelNumber);
                        gc.CreatePdf(orderNumber, parcel_id, "parcel search", driver, "MN", "Dakota");
                        driver.FindElement(By.XPath("//*[@id='esri_dijit_Search_0']/div/div[2]")).Click();
                        try
                        {
                            IWebElement Inodata = driver.FindElement(By.XPath("//*[@id='esri_dijit_Search_0']/div/div[4]/div"));
                            if (Inodata.Text.Contains("No results") || Inodata.Text.Contains("no results found"))
                            {
                                HttpContext.Current.Session["Nodata_MNDakota"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                        Thread.Sleep(6000);
                    }


                    //property_details
                    driver.SwitchTo().Window(driver.WindowHandles.Last());
                    //Owner Name~Joint Owner Name~Property Address~Mailing Address~Municipality~Property Use~Year Built~Legal Description
                    gc.CreatePdf(orderNumber, parcel_id, "property info", driver, "MN", "Dakota");
                    string fulltext3 = driver.FindElement(By.XPath("//*[@id='dijit_layout_ContentPane_0']")).Text.Trim().Replace("\r\n", "");
                    parcel_id = gc.Between(fulltext3, "Parcel ID:", "Property Details").Trim().Replace("-", "");
                    PropertyAddress = gc.Between(fulltext3, "Parcel ID:", "Property Details").Trim().Replace("-", "");
                    string s = parcel_id;
                    string[] words = parcel_id.Split(' ');
                    parcel_id = words[0];

                    parcel_id = parcel_id.Substring(0, 12);
                    PropertyAddress = WebDriverTest.After(PropertyAddress, parcel_id).Trim();

                    Thread.Sleep(2000);
                    string fulltext = driver.FindElement(By.XPath("//*[@id='dijit_layout_ContentPane_0']")).Text.Trim().Replace("\r\n", "");
                    OwnerName = gc.Between(fulltext, "Owner", "Joint Owner").Trim();
                    JointOwnerName = gc.Between(fulltext, "Joint Owner", "Owner Address").Trim();
                    MailingAddress = gc.Between(fulltext, "Owner Address", "Municipality").Trim();
                    Municipality = gc.Between(fulltext, "Municipality", "Primary Use").Trim();
                    PropertyUse = gc.Between(fulltext, "Primary Use", "Acres").Trim();
                    LegalDescription = gc.Between(fulltext, "Tax Description", "Lot and Block").Trim();
                    YearBuilt = gc.Between(fulltext, "Year Built", "Building Type").Trim();

                    string property_details = OwnerName + "~" + JointOwnerName + "~" + PropertyAddress + "~" + MailingAddress + "~" + Municipality + "~" + PropertyUse + "~" + YearBuilt + "~" + LegalDescription;
                    gc.insert_date(orderNumber, parcel_id, 461, property_details, 1, DateTime.Now);

                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");

                    string strCurrentURL = driver.CurrentWindowHandle;
                    //Tax Year~Estimated Market Value~Homestead Exclusion~Taxable Market Value~New Imp/Expired Excl~New Imp/Expired Excl

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
                        Thread.Sleep(20000);
                        string fileName = "PropertyReport";
                        IWebElement ISpan121 = driver1.FindElement(By.XPath("//*[@id='widgets_Splash_Widget_71']/div[2]/div[2]/div[2]/div[2]"));
                        IJavaScriptExecutor js121 = driver1 as IJavaScriptExecutor;
                        js121.ExecuteScript("arguments[0].click();", ISpan121);
                        Thread.Sleep(15000);
                        driver1.FindElement(By.Id("esri_dijit_Search_0_input")).SendKeys(parcel_id);
                        driver1.FindElement(By.XPath("//*[@id='esri_dijit_Search_0']/div/div[2]")).Click();
                        Thread.Sleep(6000);
                        IWebElement Itaxbill = driver1.FindElement(By.LinkText("Property Details"));
                        Itaxbill.Click();
                        Thread.Sleep(6000);
                        gc.AutoDownloadFile(orderNumber, parcelNumber, "Dakota", "MN", "PropertyReport.pdf");

                        driver1.Quit();
                    }
                    catch (Exception e)
                    { }


                    try
                    {
                        IWebElement Itaxstmt = driver.FindElement(By.LinkText("Tax Statement"));
                        string stmt1 = Itaxstmt.GetAttribute("href");
                        strTaxRealestate1.Add(stmt1);
                    }
                    catch { }
                    try
                    {

                        IWebElement Itaxstmt1 = driver.FindElement(By.LinkText("Tax Payment Stub"));
                        string stmt11 = Itaxstmt1.GetAttribute("href");
                        var chDriver = new ChromeDriver();

                        chDriver.Navigate().GoToUrl(stmt11);
                        Thread.Sleep(5000);
                        chDriver.Manage().Window.Size = new Size(480, 320);
                        gc.CreatePdf(orderNumber, parcel_id, "stub", chDriver, "MN", "Dakota");


                        Actions act = new Actions(chDriver);

                        for (int l = 0; l < 13; l++)
                        {
                            act.SendKeys(Keys.Down).Perform();
                        }
                        gc.CreatePdf(orderNumber, parcel_id, "stub2", chDriver, "MN", "Dakota");

                        Thread.Sleep(4000);
                        chDriver.Quit();
                    }
                    catch (Exception e) { }


                    try
                    {
                        IWebElement Itaxstmt2 = driver.FindElement(By.LinkText("Tax Facts"));
                        string stmt12 = Itaxstmt2.GetAttribute("href");
                        gc.downloadfile(stmt12, orderNumber, parcel_id, "TaxFact", "MN", "Dakota");
                    }
                    catch
                    {
                    }

                    List<string> description = new List<string>();
                    List<string> Output1 = new List<string>();
                    List<string> Output2 = new List<string>();

                    List<string> Output1A = new List<string>();
                    List<string> Output2A = new List<string>();
                    foreach (string real in strTaxRealestate1)
                    {
                        driver.Navigate().GoToUrl(real);
                        Thread.Sleep(4000);
                        try {
                            ByVisibleElement(driver.FindElement(By.Id("hplTaxStmntPDF")));
                        }
                        catch { }
                        if (real.Contains("TaxStatement"))
                        {
                            //Taxinfo_Details
                            gc.CreatePdf(orderNumber, parcel_id, "Tax info", driver, "MN", "Dakota");

                            description.Add(driver.FindElement(By.Id("lblLine3")).Text.Trim());
                            Output1.Add(driver.FindElement(By.Id("txtPrevTotalAidTax")).Text.Trim());
                            Output2.Add(driver.FindElement(By.Id("txtCurrentTotalAidTax")).Text.Trim());

                            description.Add(driver.FindElement(By.XPath("//*[@id='txtTaxStatement']/div[52]")).Text.Trim());
                            Output1.Add(driver.FindElement(By.Id("txtPrevHomesteadCredit")).Text.Trim());
                            Output2.Add(driver.FindElement(By.Id("txtCurrentHomesteadCredit")).Text.Trim());

                            description.Add(driver.FindElement(By.Id("Div31")).Text.Trim());
                            Output1.Add(driver.FindElement(By.Id("txtPrevOtherCredits")).Text.Trim());
                            Output2.Add(driver.FindElement(By.Id("txtCurrentOtherCredits")).Text.Trim());

                            description.Add(driver.FindElement(By.Id("Div48")).Text.Trim());
                            Output1.Add(driver.FindElement(By.Id("txtPrevTotalNetTax")).Text.Trim());
                            Output2.Add(driver.FindElement(By.Id("txtCurrentTotalNetTax")).Text.Trim());

                            description.Add(driver.FindElement(By.Id("Div7")).Text.Trim());
                            Output1.Add(driver.FindElement(By.Id("txtPrevCountyTax")).Text.Trim());
                            Output2.Add(driver.FindElement(By.Id("txtCurrentCountyTax")).Text.Trim());

                            description.Add(driver.FindElement(By.Id("Div10")).Text.Trim());
                            Output1.Add(driver.FindElement(By.Id("txtPrevRail")).Text.Trim());
                            Output2.Add(driver.FindElement(By.Id("txtRail")).Text.Trim());

                            description.Add(driver.FindElement(By.Id("txtDistrictName")).Text.Trim());
                            Output1.Add(driver.FindElement(By.Id("txtPrevCityTax")).Text.Trim());
                            Output2.Add(driver.FindElement(By.Id("txtCurrentCityTax")).Text.Trim());

                            description.Add(driver.FindElement(By.Id("Div51")).Text.Trim());
                            Output1.Add(driver.FindElement(By.Id("txtPrevStateTax")).Text.Trim());
                            Output2.Add(driver.FindElement(By.Id("txtCurrentStateTax")).Text.Trim());

                            description.Add(driver.FindElement(By.Id("txtSchoolDistrict")).Text.Trim());
                            Output1.Add(driver.FindElement(By.Id("txtPrevVoterApprovedLevies")).Text.Trim());
                            Output2.Add(driver.FindElement(By.Id("txtCurrentVoterApprovedLevies")).Text.Trim());

                            description.Add(driver.FindElement(By.Id("Div55")).Text.Trim());
                            Output1.Add(driver.FindElement(By.Id("txtPrevOtherLevies")).Text.Trim());
                            Output2.Add(driver.FindElement(By.Id("txtCurrentOtherLevies")).Text.Trim());

                            description.Add(driver.FindElement(By.Id("Div57")).Text.Trim());
                            Output1.Add(driver.FindElement(By.Id("txtPrevMetroTax")).Text.Trim());
                            Output2.Add(driver.FindElement(By.Id("txtCurrentMetroTax")).Text.Trim());

                            description.Add(driver.FindElement(By.Id("Div58")).Text.Trim());
                            Output1.Add(driver.FindElement(By.Id("txtPrevSpecial")).Text.Trim());
                            Output2.Add(driver.FindElement(By.Id("txtCurrentSpecial")).Text.Trim());

                            description.Add(driver.FindElement(By.Id("Div59")).Text.Trim());
                            Output1.Add(driver.FindElement(By.Id("txtPrevTaxIncrement")).Text.Trim());
                            Output2.Add(driver.FindElement(By.Id("txtCurrentTaxIncrement")).Text.Trim());

                            description.Add(driver.FindElement(By.Id("Div60")).Text.Trim());
                            Output1.Add(driver.FindElement(By.Id("txtPrevFiscalDisparity")).Text.Trim());
                            Output2.Add(driver.FindElement(By.Id("txtCurrentFiscalDisparity")).Text.Trim());

                            description.Add(driver.FindElement(By.Id("Div61")).Text.Trim());
                            Output1.Add(driver.FindElement(By.Id("txtPrevNonSchoolVoterLevy")).Text.Trim());
                            Output2.Add(driver.FindElement(By.Id("txtCurrentNonSchoolVoterLevy")).Text.Trim());

                            description.Add(driver.FindElement(By.Id("Div62")).Text.Trim());
                            Output1.Add(driver.FindElement(By.Id("txtPrevTaxBeforeSA")).Text.Trim());
                            Output2.Add(driver.FindElement(By.Id("txtCurrentTaxBeforeSA")).Text.Trim());

                            description.Add(driver.FindElement(By.Id("Div63")).Text.Trim());
                            Output1.Add(driver.FindElement(By.Id("txtTotalSA")).Text.Trim());
                            Output2.Add(" ");

                            description.Add(driver.FindElement(By.Id("Div64")).Text.Trim());
                            Output1.Add(" ");
                            Output2.Add(driver.FindElement(By.Id("txtSAPrincipal")).Text.Trim());

                            description.Add(driver.FindElement(By.Id("Div65")).Text.Trim());
                            Output1.Add(" ");
                            Output2.Add(driver.FindElement(By.Id("txtSAInterest")).Text.Trim());

                            description.Add(driver.FindElement(By.Id("Div66")).Text.Trim());
                            Output1.Add(driver.FindElement(By.Id("txtPriorTaxAndSA")).Text.Trim());
                            Output2.Add(driver.FindElement(By.Id("txtCurrentTaxAndSA")).Text.Trim());

                            description.Add(driver.FindElement(By.Id("Div67")).Text.Trim());
                            Output1.Add(" ");
                            Output2.Add(driver.FindElement(By.Id("txtFirstHalfTax")).Text.Trim());

                            description.Add(driver.FindElement(By.Id("Div69")).Text.Trim());
                            Output1.Add(" ");
                            Output2.Add(driver.FindElement(By.Id("txtSecondHalfTax")).Text.Trim());
                            Output1.Add(driver.FindElement(By.Id("txtPriorYear")).Text.Trim());
                            Output2.Add(driver.FindElement(By.Id("txtCurrentYear")).Text.Trim());

                            string deli = (driver.FindElement(By.Id("txtDelinquentInd")).Text.Trim());
                            string msg = "";

                            if (deli.Contains(""))
                            {
                                deli = "NO";
                                msg = " ";
                            }

                            else
                            {
                                deli = "Yes";
                                msg = "Taxes are Delinquent. Please contact county for tax information";
                            }

                            string tax_address = driver.FindElement(By.Id("lblDeptHeading")).Text.Trim().Replace("\r\n", "");
                            string descriptionN = "Tax Year" + "~" + description[0] + "~" + "Credits that reduce property taxes " + description[1] + "~" + "Credits that reduce property taxes " + description[2] + "~" + description[3] + "~" + "County: " + description[4] + "~" + description[5] + "~" + "City or Town: " + description[6] + "~" + description[7] + "~" + "School District: " + description[8] + " A.  Voter Approved Levies" + "~" + description[9] + "~" + "Special Taxing Districts  " + description[10] + "~" + description[11] + "~" + description[12] + "~" + description[13] + "~" + description[14] + "~" + description[15] + "~" + description[16] + "~" + description[17] + "~" + description[18] + "~" + description[19] + "~" + description[20] + "~" + description[21] + "~" + "Delinquent Taxes" + "~" + "Comments" + "~" + "Tax Address";
                            string taxInfo1 = Output1[22] + "~" + Output1[0] + "~" + Output1[1] + "~" + Output1[2] + "~" + Output1[3] + "~" + Output1[4] + "~" + Output1[5] + "~" + Output1[6] + "~" + Output1[7] + "~" + Output1[8] + "~" + Output1[9] + "~" + Output1[10] + "~" + Output1[11] + "~" + Output1[12] + "~" + Output1[13] + "~" + Output1[14] + "~" + Output1[15] + "~" + Output1[16] + "~" + Output1[17] + "~" + Output1[18] + "~" + Output1[19] + "~" + Output1[20] + "~" + Output1[21] + "~" + deli + "~" + msg + "~" + tax_address;
                            string taxInfo11 = Output2[22] + "~" + Output2[0] + "~" + Output2[1] + "~" + Output2[2] + "~" + Output2[3] + "~" + Output2[4] + "~" + Output2[5] + "~" + Output2[6] + "~" + Output2[7] + "~" + Output2[8] + "~" + Output2[9] + "~" + Output2[10] + "~" + Output2[11] + "~" + Output2[12] + "~" + Output2[13] + "~" + Output2[14] + "~" + Output2[15] + "~" + Output2[16] + "~" + Output2[17] + "~" + Output2[18] + "~" + Output2[19] + "~" + Output2[20] + "~" + Output2[21] + "~" + deli + "~" + msg + "~" + tax_address;

                            DBconnection dbconn = new DBconnection();
                            dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + descriptionN + "' where Id = '" + 463 + "'");

                            gc.insert_date(orderNumber, parcel_id, 463, taxInfo1, 1, DateTime.Now);
                            gc.insert_date(orderNumber, parcel_id, 463, taxInfo11, 1, DateTime.Now);

                            //Assessment details
                            Output1A.Add(driver.FindElement(By.Id("txtPriorYear")).Text.Trim());
                            Output2A.Add(driver.FindElement(By.Id("txtCurrentYear")).Text.Trim());


                            Output1A.Add(driver.FindElement(By.Id("txtPriorMarketValue")).Text.Trim());
                            Output2A.Add(driver.FindElement(By.Id("txtCurrentEstimatedMarketValue")).Text.Trim());


                            Output1A.Add(driver.FindElement(By.Id("txtPriorHmstdExclusion")).Text.Trim());
                            Output2A.Add(driver.FindElement(By.Id("txtCurrentHmstdExclusion")).Text.Trim());

                            Output1A.Add(driver.FindElement(By.Id("txtPriorTaxableMarketValue")).Text.Trim());
                            Output2A.Add(driver.FindElement(By.Id("txtCurrentTaxableMarketValue")).Text.Trim());


                            Output1A.Add(driver.FindElement(By.Id("txtPriorNewImprovements")).Text.Trim());
                            Output2A.Add(driver.FindElement(By.Id("txtCurrentNewImprovements")).Text.Trim());


                            Output1A.Add(driver.FindElement(By.Id("txtPriorValueClass1")).Text.Trim());
                            Output2A.Add(driver.FindElement(By.Id("txtValueClass1")).Text.Trim());
                            string Assessemt1 = Output1A[0] + "~" + Output1A[1] + "~" + Output1A[2] + "~" + Output1A[3] + "~" + Output1A[4] + "~" + Output1A[5];
                            string Assessemt11 = Output2A[0] + "~" + Output2A[1] + "~" + Output2A[2] + "~" + Output2A[3] + "~" + Output2A[4] + "~" + Output2A[5];

                            gc.insert_date(orderNumber, parcel_id, 462, Assessemt1, 1, DateTime.Now);
                            gc.insert_date(orderNumber, parcel_id, 462, Assessemt11, 1, DateTime.Now);



                            //download taxbill
                            try
                            {
                                IWebElement Itaxbill5 = driver.FindElement(By.Id("hplTaxStmntPDF"));
                                string URL15 = Itaxbill5.GetAttribute("href");
                                gc.downloadfile(URL15, orderNumber, parcel_id, "TaxBill1", "MN", "Dakota");
                            }
                            catch
                            { }

                        }



                    }
                    TaxTime = DateTime.Now.ToString("HH:mm:ss");
                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "MN", "Dakota", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);


                    driver.Quit();
                    HttpContext.Current.Session["TitleFlex_Search"] = "";

                    gc.mergpdf(orderNumber, "MN", "Dakota");
                    return "Data Inserted Successfully";
                }

                catch (Exception ex)
                {
                    driver.Quit();
                    throw ex;
                }
            }
        }
        public void CreatePdf1(string orderno, string parcelno, string pdfName, IWebDriver driver, string sname, string cname)
        {
            string outputPath = gc.ReturnPath(sname, cname);
            outputPath = outputPath + orderno + "\\" + parcelno + "\\";
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }
            string img = outputPath + pdfName + ".png";
            string pdf = outputPath + pdfName + ".pdf";

            driver.Manage().Window.Maximize();
            ((ITakesScreenshot)driver).GetScreenshot().SaveAsFile(img, ScreenshotImageFormat.Png);
            // driver.GetScreenshot().SaveAsFile(img, ScreenshotImageFormat.Png);

            WebDriverTest.ConvertImageToPdf(img, pdf);
            if (File.Exists(img))
            {
                File.Delete(img);
            }

        }
        public void ByVisibleElement(IWebElement Element)
        {
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            js.ExecuteScript("arguments[0].scrollIntoView();", Element);
        }

    }
}