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
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;


namespace ScrapMaricopa.Scrapsource
{
    public class Webdriver_MohaveAZ
    {

        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();

        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());

        public string FTP_Mohave(string address, string parcelNumber, string ownername, string searchType, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;

            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
            string TaxAuthority = "";
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            var option = new ChromeOptions();
            option.AddArgument("No-Sandbox");
            using (driver = new ChromeDriver(option))
            {
                // driver = new ChromeDriver();

                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");

                    driver.Navigate().GoToUrl("https://www.mohavecounty.us/ContentPage.aspx?id=111&cid=869");
                    Thread.Sleep(7000);

                    if (searchType == "titleflex")
                    {
                        string titleaddress = address;
                        gc.TitleFlexSearch(orderNumber, "", "", titleaddress, "AZ", "Mohave");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_Mohave"] = "Yes";
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
                            driver.FindElement(By.Id("radioAddress")).Click();
                            Thread.Sleep(7000);
                        }
                        catch { }

                        driver.FindElement(By.Id("searchForPattern")).SendKeys(address);
                        Thread.Sleep(4000);
                        gc.CreatePdf_WOP(orderNumber, "Address Search", driver, "AZ", "Mohave");

                        try
                        {
                            driver.FindElement(By.Id("searchParcel-ct-button-span-label")).Click();
                        }
                        catch { }
                        try
                        {
                            driver.FindElement(By.Id("searchParcel")).Click();
                        }
                        catch { }
                        int Max = 0;
                        string RecordCount = "";
                        try
                        {
                            RecordCount = driver.FindElement(By.XPath("//*[@id='gridSearchContainer']/div[1]/div[2]/h5")).Text.Replace("Record Count:", "").Trim();
                        }
                        catch { }
                        try
                        {
                            string strowner = "", strAddress = "", strparcel = "";
                            IWebElement multiaddress = driver.FindElement(By.XPath("//*[@id='gridSearchResults']/tbody"));
                            IList<IWebElement> multiRow = multiaddress.FindElements(By.TagName("tr"));
                            IList<IWebElement> multiTD;
                            foreach (IWebElement multi in multiRow)
                            {
                                multiTD = multi.FindElements(By.TagName("td"));

                                if (multiTD.Count != 0 && multiRow.Count > 25)
                                {
                                    HttpContext.Current.Session["multiparcel_MohaveAZ_Maximum"] = "Maximum";
                                    driver.Quit();
                                    return "Maximum";

                                }
                                if (multiTD.Count != 0 && multiRow.Count > 2 && multiRow.Count <= 25 && multi.Text.Trim() != "")
                                {
                                    strowner = multiTD[1].Text;
                                    strAddress = multiTD[2].Text;
                                    strparcel = multiTD[0].Text;
                                    string multidetails = strowner + "~" + strAddress;
                                    gc.insert_date(orderNumber, strparcel, 1200, multidetails, 1, DateTime.Now);
                                    Max++;
                                }
                            }
                            if (multiRow.Count > 2 && multiRow.Count <= 25)
                            {
                                HttpContext.Current.Session["multiparcel_MohaveAZ"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                            if (Max == 0 && RecordCount == "0")
                            {

                                HttpContext.Current.Session["Nodata_Mohave"] = "Yes";
                                driver.Quit();
                                return "No Data Found";

                            }
                        }
                        catch { }
                    }
                    else if (searchType == "parcel")
                    {
                        try
                        {
                            driver.FindElement(By.Id("radioParcel")).Click();
                            Thread.Sleep(3000);
                        }
                        catch { }
                        driver.FindElement(By.Id("searchForPattern")).SendKeys(parcelNumber);
                        gc.CreatePdf(orderNumber, parcelNumber, "parcel search", driver, "AZ", "Mohave");
                        driver.FindElement(By.Id("searchParcel-ct-button-span-label")).Click();
                        Thread.Sleep(3000);
                        // gc.CreatePdf(orderNumber, parcelNumber, "parcel search Result", driver, "AZ", "Mohave");
                        try
                        {
                            IWebElement Inodata = driver.FindElement(By.Id("deletedParcelText"));
                            if (Inodata.Text.Contains("The following parcel was not found"))
                            {
                                HttpContext.Current.Session["Nodata_Mohave"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }
                    else if (searchType == "ownername")
                    {
                        try
                        {
                            driver.FindElement(By.Id("radioOwner")).Click();
                            Thread.Sleep(5000);
                        }
                        catch { }
                        driver.FindElement(By.Id("searchForPattern")).SendKeys(ownername);
                        gc.CreatePdf_WOP(orderNumber, "Ownername search", driver, "AZ", "Mohave");
                        driver.FindElement(By.Id("searchParcel-ct-button-span-label")).Click();
                        Thread.Sleep(3000);
                        gc.CreatePdf_WOP(orderNumber, "Ownername search Result", driver, "AZ", "Mohave");

                        IWebElement Multiparcel = driver.FindElement(By.XPath("//*[@id='gridSearchResults']/tbody"));

                        int Max = 0;
                        string RecordCount = "";
                        try
                        {
                            RecordCount = driver.FindElement(By.XPath("//*[@id='gridSearchContainer']/div[1]/div[2]/h5")).Text.Replace("Record Count:", "").Trim();
                        }
                        catch { }
                        try
                        {
                            string strowner1 = "", strAddress1 = "", strparcel1 = "";
                            IWebElement multiaddress1 = driver.FindElement(By.XPath("//*[@id='gridSearchResults']/tbody"));
                            IList<IWebElement> multiRow = multiaddress1.FindElements(By.TagName("tr"));
                            IList<IWebElement> multiTD;
                            foreach (IWebElement multi in multiRow)
                            {
                                multiTD = multi.FindElements(By.TagName("td"));

                                if (multiTD.Count != 0 && multiRow.Count > 25)
                                {
                                    HttpContext.Current.Session["multiparcel_MohaveAZ_Maximum"] = "Maximum";
                                    driver.Quit();
                                    return "Maximum";
                                    Max++;
                                }
                                if (multiTD.Count != 0 && multiRow.Count > 2 && multiRow.Count <= 25 && multi.Text.Trim() != "")
                                {
                                    strowner1 = multiTD[1].Text;
                                    strAddress1 = multiTD[2].Text;
                                    strparcel1 = multiTD[0].Text;
                                    string multidetails1 = strowner1 + "~" + strAddress1;
                                    gc.insert_date(orderNumber, strparcel1, 1200, multidetails1, 1, DateTime.Now);
                                    Max++;
                                }
                            }
                            if (multiRow.Count > 2 && multiRow.Count <= 25)
                            {
                                HttpContext.Current.Session["multiparcel_MohaveAZ"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                            if (Max == 0 && RecordCount == "0")
                            {
                                HttpContext.Current.Session["Nodata_Mohave"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }


                        catch { }
                    }

                    //property details

                    string owner_name = "", ownershiptype = "", MailingAddress = "", Multiowner = "", SiteAddress = "", parcelsize = "", township = "", range = "", section = "", Yearbuilt = "";
                    IWebElement iparcelnum = driver.FindElement(By.XPath("//*[@id='parcelNumberResult']"));
                    parcelNumber = iparcelnum.Text;

                    try
                    {
                        ByVisibleElement(driver.FindElement(By.XPath("//*[@id='parcelNumberResult']")));
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Property Details", driver, "AZ", "Mohave");

                        driver.FindElement(By.XPath("//*[@id='dialog-ct-dialog-button-ok']")).Click();
                        Thread.Sleep(4000);
                    }
                    catch { }

                    IWebElement iownername = driver.FindElement(By.XPath("//*[@id='parcelOwnerResult']"));
                    owner_name = iownername.Text;
                    IWebElement iownershiptype = driver.FindElement(By.XPath("//*[@id='parcelOwnershipTypeResult']"));
                    ownershiptype = iownershiptype.Text;
                    IWebElement ImailingAdd = driver.FindElement(By.Id("parcelMailingAddressResult"));
                    MailingAddress = ImailingAdd.Text;
                    IWebElement isiteaddress = driver.FindElement(By.Id("parcelSiteAddressResult"));
                    SiteAddress = isiteaddress.Text;
                    IWebElement Imultiowner = driver.FindElement(By.Id("parcelMultipleOwnersResult"));
                    Multiowner = Imultiowner.Text;
                    string Bulkinfo = driver.FindElement(By.Id("parcelAssessorDescriptionInformationResultContainer")).Text;
                    parcelsize = gc.Between(Bulkinfo, "Parcel Size", "Township");
                    township = gc.Between(Bulkinfo, "Township", "Range");
                    range = gc.Between(Bulkinfo, "Range", "Section");
                    try
                    {
                        IWebElement Isection = driver.FindElement(By.XPath("//*[@id='parcelAssessorDescriptionInformationResultContainer']/div[4]/div[2]/div/h5"));
                        section = Isection.Text;
                    }
                    catch { }
                    try
                    {
                        IWebElement Iyearbuilt = driver.FindElement(By.XPath("//*[@id='gridImprovementDataResult']/tbody/tr[1]/td[4]/h5"));
                        Yearbuilt = Iyearbuilt.Text;
                    }
                    catch { }
                    string propertydetails = owner_name + "~" + ownershiptype + "~" + MailingAddress + "~" + SiteAddress + "~" + Multiowner + "~" + parcelsize + "~" + township + "~" + range + "~" + section + "~" + Yearbuilt;
                    gc.insert_date(orderNumber, parcelNumber, 1184, propertydetails, 1, DateTime.Now);

                    //  Assessment Details
                    string valuetype = "", PreviousYear = "", CurrentYear = "", FutureYear = "";
                    IWebElement Assessmentdetails = driver.FindElement(By.XPath("//*[@id='parcelAssessorParcelInformationContainer']"));
                    IList<IWebElement> TRAssessmentdetails = Assessmentdetails.FindElements(By.TagName("div"));
                    IList<IWebElement> THAssessmentdetails = Assessmentdetails.FindElements(By.TagName("h5"));
                    IList<IWebElement> TDAssessmentdetails;
                    foreach (IWebElement row in TRAssessmentdetails)
                    {
                        TDAssessmentdetails = row.FindElements(By.TagName("h5"));
                        if (TDAssessmentdetails.Count != 0 && !row.Text.Contains("Previous Year") && !row.Text.Contains("Property Class") && row.Text.Trim() != "" && TDAssessmentdetails.Count == 4)
                        {
                            valuetype += TDAssessmentdetails[0].Text + "~";
                            PreviousYear += TDAssessmentdetails[1].Text + "~";
                            CurrentYear += TDAssessmentdetails[2].Text + "~";
                            FutureYear += TDAssessmentdetails[3].Text + "~";
                        }
                    }


                    db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + valuetype.Remove(valuetype.Length - 1, 1) + "' where Id = '" + 1188 + "'");
                    gc.insert_date(orderNumber, parcelNumber, 1188, PreviousYear.Remove(PreviousYear.Length - 1, 1), 1, DateTime.Now);
                    gc.insert_date(orderNumber, parcelNumber, 1188, CurrentYear.Remove(CurrentYear.Length - 1, 1), 1, DateTime.Now);
                    gc.insert_date(orderNumber, parcelNumber, 1188, FutureYear.Remove(FutureYear.Length - 1, 1), 1, DateTime.Now);

                    try
                    {
                        ByVisibleElement(driver.FindElement(By.XPath("//*[@id='parcelAssessorParcelInformationContainer']")));
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Assessment Details", driver, "AZ", "Mohave");
                    }
                    catch { }
                    // Tax Due Date
                    string geturl = driver.Url;
                    string tableassess1 = "", tableassess2 = "", tableassess3 = "";
                    string First_Half = "", Second_Half = "", Full_Half = "";
                    string pdftext = "";
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
                        Thread.Sleep(7000);
                        try
                        {
                            driver1.FindElement(By.Id("searchForPattern")).SendKeys(parcelNumber);
                            driver1.FindElement(By.Id("searchParcel-ct-button-span-label")).Click();
                            Thread.Sleep(3000);
                        }
                        catch { }
                        string fileName = "";

                        IWebElement Receipttable = driver1.FindElement(By.Id("parcelCurentTaxBillResult"));
                        string BillTax2 = Receipttable.GetAttribute("href");
                        fileName = GlobalClass.After(BillTax2, "stream/");
                        Receipttable.Click();
                        Thread.Sleep(7000);
                        gc.AutoDownloadFile(orderNumber, parcelNumber, "Mohave", "AZ", fileName + ".pdf");
                        Thread.Sleep(12000);
                        string FilePath = gc.filePath(orderNumber, parcelNumber) + fileName + ".pdf";
                        PdfReader reader;
                        string pdfData;
                        reader = new PdfReader(FilePath);
                        String textFromPage = PdfTextExtractor.GetTextFromPage(reader, 2);
                        System.Diagnostics.Debug.WriteLine("" + textFromPage);
                        pdftext = textFromPage;
                        try
                        {
                            tableassess1 = gc.Between(pdftext, "1St ha Lf Stu B", "Pyments must be made in a u .s .").Trim();

                            // tableassess3 = gc.Between(pdftext, "or fu LL year", "Pyments must be made in a u .s .").Trim();

                            string[] tableArray1 = tableassess1.Split('\n');
                            for (int c = 0; c < 1; c++)
                            {
                                int count1 = tableArray1.Length;
                                string a1 = tableArray1[3].Replace(" ", "~");
                                string[] rowarray = a1.Split('~');
                                int tdcount = rowarray.Length;
                                if (tdcount < 6)
                                {
                                    First_Half = rowarray[2] + " " + rowarray[3] + " " + rowarray[4];

                                }



                            }
                        }
                        catch { }
                        try
                        {

                            tableassess2 = GlobalClass.After(pdftext, "2nD").Trim();

                            string[] tableArray2 = tableassess2.Split('\n');
                            for (int k = 0; k < 1; k++)
                            {
                                int count1 = tableArray2.Length;
                                string a2 = tableArray2[3].Replace(" ", "~");
                                string[] rowarray2 = a2.Split('~');
                                int tdcount = rowarray2.Length;
                                if (tdcount < 6)
                                {
                                    Second_Half = rowarray2[2] + " " + rowarray2[3] + " " + rowarray2[4];

                                }


                            }
                        }
                        catch { }
                        try
                        {
                            tableassess3 = gc.Between(pdftext, "Re T u R n 	T h IS 	 ST u B 	 f OR 	201 8 s ECond hal F pa YMEnts onl Y", "Re T u R n 	T h IS 	 ST u B 	 f OR 	201 8 FUll YEa R pa YMEnts onl Y").Trim();


                            string[] tableArray3 = tableassess3.Split('\n');

                            for (int d = 0; d < 1; d++)
                            {
                                int count3 = tableArray3.Length;
                                string a3 = tableArray3[5].Replace(" ", "~");
                                string[] rowarray3 = a3.Split('~');
                                int tdcount = rowarray3.Length;
                                if (tdcount < 6)
                                {
                                    Full_Half = rowarray3[2] + " " + rowarray3[3] + " " + rowarray3[4];

                                }
                            }
                        }
                        catch (Exception ex) { }

                        driver1.Quit();
                    }
                    catch (Exception ex) { }



                    // Tax Information
                    driver.Navigate().GoToUrl("https://eagletw.mohavecounty.us/treasurer/treasurerweb/search.jsp");
                    Thread.Sleep(4000);

                    try
                    {
                        driver.FindElement(By.XPath("//*[@id='middle_left']/form/input[1]")).Click();
                        Thread.Sleep(2000);
                    }
                    catch { }
                    driver.FindElement(By.Id("TaxAParcelID")).SendKeys(parcelNumber.Replace("-", ""));
                    gc.CreatePdf(orderNumber, parcelNumber, "Tax search", driver, "AZ", "Mohave");
                    driver.FindElement(By.XPath("//*[@id='middle']/form/table[5]/tbody/tr/td[1]/input")).Click();
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, parcelNumber, "Tax search Result", driver, "AZ", "Mohave");
                    driver.FindElement(By.XPath("//*[@id='searchResultsTable']/tbody/tr/td[2]/a/table/tbody/tr/td[1]/b/font")).Click();
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, parcelNumber, "Tax Info-Second or Full", driver, "AZ", "Mohave");
                    try
                    {
                        driver.FindElement(By.Id("paymentTypeFirst")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Tax Info-First", driver, "AZ", "Mohave");
                    }
                    catch { }
                    // Assessment Value Details

                    string check = "", ValueAmount = "", ValueTitle = "", first = "", Title1 = "", Title2 = "";
                    int valuecount = 0;
                    IWebElement IValue = driver.FindElement(By.XPath("//*[@id='taxAccountValueSummary']/div/table/tbody"));
                    IList<IWebElement> IValueRow = IValue.FindElements(By.TagName("tr"));
                    IList<IWebElement> IValueTD;
                    foreach (IWebElement value in IValueRow)
                    {
                        IValueTD = value.FindElements(By.TagName("td"));
                        if (IValueTD.Count != 0 && value.Text != "")
                        {
                            string AssessValue = IValueTD[0].Text + "~" + IValueTD[1].Text + "~" + IValueTD[2].Text;
                            gc.insert_date(orderNumber, parcelNumber, 1195, AssessValue, 1, DateTime.Now);


                        }
                    }



                    try
                    {
                        ByVisibleElement(driver.FindElement(By.XPath("//*[@id='taxAccountValueSummary']/div/table/tbody")));
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Assessment Value Details", driver, "AZ", "Mohave");
                    }
                    catch { }

                    // Tax Account Details
                    IWebElement IDue; IWebElement IDue2;
                    string duedate = "";
                    string Accountid = "", Owner = "", Situsaddress = "", legal = "", Asof1 = "", Paymenttype1 = "", Asof2 = "", Paymenttype2 = "";
                    string PayTaxDue = "", PayInterestDue = "", PayTotalDue = "", PayMiscDue = "", PayLienDue = "", PayLienInterestDue = "", GoodthroughDate = "";
                    TaxAuthority = "PO Box 712 Kingman, AZ 86402 Phone number: 928-753-0737";
                    string Bulkdata = driver.FindElement(By.XPath("//*[@id='taxAccountSummary']/table/tbody")).Text;
                    Accountid = gc.Between(Bulkdata, "Account Id", "Parcel").Trim();
                    Owner = gc.Between(Bulkdata, "Owners", "Address").Trim();
                    Situsaddress = gc.Between(Bulkdata, "Situs Address", "Legal").Trim();
                    try
                    {
                        driver.FindElement(By.Id("paymentTypeFirst")).Click();
                        Thread.Sleep(1000);

                    }
                    catch { }
                    IWebElement IAsof1 = driver.FindElement(By.Id("paymentDate"));
                    Asof1 = IAsof1.GetAttribute("value");
                    // Good Through Date
                    IDue = driver.FindElement(By.Id("totals"));
                    try
                    {
                        if (!IDue.Text.Contains("$0.00") || (IDue.Text.Contains("Interest") && IDue.Text.Contains("Interest") && IDue.Text.Contains("Total")))

                        {
                            string Fulltype = driver.FindElement(By.XPath("//*[@id='inquiryForm']/table/tbody/tr[2]/td[2]/label[2]")).Text;
                            //TaxDueDate(orderNumber, TaxParcel, strDueDate, Fulltype, TaxAuthority);
                            string currDate = DateTime.Now.ToString("MM/dd/yyyy");
                            string dateChecking = DateTime.Now.ToString("MM") + "/15/" + DateTime.Now.ToString("yyyy");

                            if (Convert.ToDateTime(currDate) > Convert.ToDateTime(dateChecking))
                            {
                                string nextEndOfMonth = "";
                                if ((Convert.ToInt16(Convert.ToInt16(DateTime.Now.ToString("MM"))) < 12))
                                {
                                    nextEndOfMonth = new DateTime(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(Convert.ToInt16(DateTime.Now.ToString("MM")) + 1), DateTime.DaysInMonth(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(DateTime.Now.ToString("MM")) + 1)).ToString("MM/dd/yyyy");
                                }
                                else
                                {
                                    int nxtYr = Convert.ToInt16(DateTime.Now.ToString("yyyy")) + 1;
                                    nextEndOfMonth = nextEndOfMonth = new DateTime(nxtYr, 1, DateTime.DaysInMonth(Convert.ToInt16(DateTime.Now.ToString("yyyy")), 1)).ToString("MM/dd/yyyy");
                                }
                                GoodthroughDate = nextEndOfMonth;
                            }
                            else
                            {
                                string EndOfMonth = new DateTime(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(Convert.ToInt16(DateTime.Now.ToString("MM"))), DateTime.DaysInMonth(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(DateTime.Now.ToString("MM")))).ToString("MM/dd/yyyy");
                                GoodthroughDate = EndOfMonth;
                            }
                        }

                    }
                    catch { }
                    try
                    {
                        if (GoodthroughDate != "")
                        {
                            IWebElement IAsof11 = driver.FindElement(By.Id("paymentDate"));
                            IAsof11.Clear();
                            driver.FindElement(By.Id("paymentDate")).SendKeys(GoodthroughDate);
                            Thread.Sleep(1000);
                            driver.FindElement(By.Id("paymentTypeFirst")).Click();
                            Thread.Sleep(2000);

                        }
                    }
                    catch { }

                    legal = GlobalClass.After(Bulkdata, "Legal").Trim();
                    Paymenttype1 = driver.FindElement(By.XPath("//*[@id='inquiryForm']/table/tbody/tr[2]/td[2]/label[1]")).Text;

                    gc.CreatePdf(orderNumber, parcelNumber, "Tax_Account Details-First", driver, "AZ", "Mohave");
                    IWebElement IDueDetails = driver.FindElement(By.XPath("//*[@id='totals']/table/tbody"));
                    IList<IWebElement> IDueDetailRow = IDueDetails.FindElements(By.TagName("tr"));
                    IList<IWebElement> IDueDetailTd;
                    foreach (IWebElement Due in IDueDetailRow)
                    {
                        IDueDetailTd = Due.FindElements(By.TagName("td"));
                        if (IDueDetailTd.Count != 0)
                        {
                            if (Due.Text.Contains("Taxes Due"))
                            {
                                PayTaxDue = IDueDetailTd[1].Text;
                            }
                            if (Due.Text.Contains("Interest Due") && !Due.Text.Contains("Lien"))
                            {
                                PayInterestDue = IDueDetailTd[1].Text;
                            }

                            if (Due.Text.Contains("Total Due"))
                            {
                                PayTotalDue = IDueDetailTd[1].Text;
                            }
                            if (Due.Text.Contains("Misc Due"))
                            {
                                PayMiscDue = IDueDetailTd[1].Text;
                            }
                            if (Due.Text.Contains("Lien Due"))
                            {
                                PayLienDue = IDueDetailTd[1].Text;
                            }

                            if (Due.Text.Contains("Lien Interest Due"))
                            {
                                PayLienInterestDue = IDueDetailTd[1].Text;
                            }
                        }
                    }


                    string TaxInfodetails = Accountid + "~" + Owner + "~" + Situsaddress + "~" + legal + "~" + Asof1 + "~" + Paymenttype1 + "~" + PayTaxDue + "~" + PayInterestDue + "~" + PayMiscDue + "~" + PayLienDue + "~" + PayLienInterestDue + "~" + PayTotalDue + "~" + First_Half + "~" + TaxAuthority;
                    gc.insert_date(orderNumber, parcelNumber, 1194, TaxInfodetails, 1, DateTime.Now);

                    // GoodthroughDate Calculation County(1)
                    try
                    {
                        string Goodthroughdetails1 = Asof1 + "~" + Paymenttype1 + "~" + PayTaxDue + "~" + PayInterestDue + "~" + PayMiscDue + "~" + PayLienDue + "~" + PayLienInterestDue + "~" + PayTotalDue + "~" + First_Half + "~" + GoodthroughDate;
                        gc.insert_date(orderNumber, parcelNumber, 1334, Goodthroughdetails1, 1, DateTime.Now);
                    }
                    catch { }

                    try
                    {
                        driver.FindElement(By.Id("paymentTypeSecond")).Click();
                        Thread.Sleep(2000);
                        duedate = Second_Half;

                    }
                    catch { }
                    try
                    {
                        driver.FindElement(By.Id("paymentTypeFull")).Click();
                        Thread.Sleep(2000);
                        duedate = Full_Half;
                    }
                    catch { }
                    IWebElement IAsof2 = driver.FindElement(By.Id("paymentDate"));
                    Asof2 = IAsof2.GetAttribute("value");
                    // Good Through Date
                    IDue2 = driver.FindElement(By.Id("totals"));
                    string PayTaxDue2 = "", PayInterestDue2 = "", PayTotalDue2 = "", PayMiscDue2 = "", PayLienDue2 = "", PayLienInterestDue2 = "", GoodthroughDate2 = "";
                    try
                    {
                        if (!IDue2.Text.Contains("$0.00") || (IDue2.Text.Contains("Interest") && IDue2.Text.Contains("Interest") && IDue2.Text.Contains("Total")))

                        {
                            string Fulltype = driver.FindElement(By.XPath("//*[@id='inquiryForm']/table/tbody/tr[2]/td[2]/label[2]")).Text;
                            //TaxDueDate(orderNumber, TaxParcel, strDueDate, Fulltype, TaxAuthority);
                            string currDate = DateTime.Now.ToString("MM/dd/yyyy");
                            string dateChecking = DateTime.Now.ToString("MM") + "/15/" + DateTime.Now.ToString("yyyy");

                            if (Convert.ToDateTime(currDate) > Convert.ToDateTime(dateChecking))
                            {
                                string nextEndOfMonth = "";
                                if ((Convert.ToInt16(Convert.ToInt16(DateTime.Now.ToString("MM"))) < 12))
                                {
                                    nextEndOfMonth = new DateTime(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(Convert.ToInt16(DateTime.Now.ToString("MM")) + 1), DateTime.DaysInMonth(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(DateTime.Now.ToString("MM")) + 1)).ToString("MM/dd/yyyy");
                                }
                                else
                                {
                                    int nxtYr = Convert.ToInt16(DateTime.Now.ToString("yyyy")) + 1;
                                    nextEndOfMonth = nextEndOfMonth = new DateTime(nxtYr, 1, DateTime.DaysInMonth(Convert.ToInt16(DateTime.Now.ToString("yyyy")), 1)).ToString("MM/dd/yyyy");
                                }
                                GoodthroughDate2 = nextEndOfMonth;
                            }
                            else
                            {
                                string EndOfMonth = new DateTime(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(Convert.ToInt16(DateTime.Now.ToString("MM"))), DateTime.DaysInMonth(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(DateTime.Now.ToString("MM")))).ToString("MM/dd/yyyy");
                                GoodthroughDate2 = EndOfMonth;
                            }
                        }

                    }
                    catch { }
                    try
                    {
                        if (GoodthroughDate2 != "")
                        {
                            IWebElement IAsof12 = driver.FindElement(By.Id("paymentDate"));
                            IAsof12.Clear();
                            driver.FindElement(By.Id("paymentDate")).SendKeys(GoodthroughDate2);
                            Thread.Sleep(1000);
                            driver.FindElement(By.Id("paymentTypeSecond")).Click();
                            Thread.Sleep(2000);
                            gc.CreatePdf(orderNumber, parcelNumber, "Tax Account Details- Second", driver, "AZ", "Mohave");
                        }
                    }
                    catch { }
                    try
                    {
                        driver.FindElement(By.Id("paymentTypeFull")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Tax Account Details- Full", driver, "AZ", "Mohave");
                    }
                    catch { }


                    Paymenttype2 = driver.FindElement(By.XPath("//*[@id='inquiryForm']/table/tbody/tr[2]/td[2]/label[2]")).Text;

                    IWebElement IDueDetails2 = driver.FindElement(By.XPath("//*[@id='totals']/table/tbody"));
                    IList<IWebElement> IDueDetailRow2 = IDueDetails2.FindElements(By.TagName("tr"));
                    IList<IWebElement> IDueDetailTd2;
                    foreach (IWebElement Due2 in IDueDetailRow2)
                    {
                        IDueDetailTd2 = Due2.FindElements(By.TagName("td"));
                        if (IDueDetailTd2.Count != 0)
                        {
                            if (Due2.Text.Contains("Taxes Due"))
                            {
                                PayTaxDue2 = IDueDetailTd2[1].Text;
                            }
                            if (Due2.Text.Contains("Interest Due") && !Due2.Text.Contains("Lien"))
                            {
                                PayInterestDue2 = IDueDetailTd2[1].Text;
                            }

                            if (Due2.Text.Contains("Total Due"))
                            {
                                PayTotalDue2 = IDueDetailTd2[1].Text;
                            }
                            if (Due2.Text.Contains("Misc Due"))
                            {
                                PayMiscDue2 = IDueDetailTd2[1].Text;
                            }
                            if (Due2.Text.Contains("Lien Due"))
                            {
                                PayLienDue2 = IDueDetailTd2[1].Text;
                            }

                            if (Due2.Text.Contains("Lien Interest Due"))
                            {
                                PayLienInterestDue2 = IDueDetailTd2[1].Text;
                            }
                        }
                    }

                    string TaxInfodetails2 = Accountid + "~" + Owner + "~" + Situsaddress + "~" + legal + "~" + Asof1 + "~" + Paymenttype2 + "~" + PayTaxDue2 + "~" + PayInterestDue2 + "~" + PayMiscDue2 + "~" + PayLienDue2 + "~" + PayLienInterestDue2 + "~" + PayTotalDue2 + "~" + duedate + "~" + TaxAuthority;
                    gc.insert_date(orderNumber, parcelNumber, 1194, TaxInfodetails2, 1, DateTime.Now);

                    // GoodthroughDate Calculation County
                    try
                    {
                        string Goodthroughdetails2 = Asof1 + "~" + Paymenttype2 + "~" + PayTaxDue2 + "~" + PayInterestDue2 + "~" + PayMiscDue2 + "~" + PayLienDue2 + "~" + PayLienInterestDue2 + "~" + PayTotalDue2 + "~" + duedate + "~" + GoodthroughDate2;
                        gc.insert_date(orderNumber, parcelNumber, 1334, Goodthroughdetails2, 1, DateTime.Now);
                    }
                    catch { }
                    // Tax Area Details
                    driver.FindElement(By.LinkText("Account Value")).Click();
                    Thread.Sleep(2000);

                    string AcNumber = "", Taxbillrate = "", Taxarea = "";
                    IWebElement Iacnum = driver.FindElement(By.XPath("//*[@id='middle']/table[1]/tbody/tr/td[2]"));
                    AcNumber = Iacnum.Text;
                    try
                    {
                        IWebElement Itaxbillrate = driver.FindElement(By.XPath("//*[@id='middle']/h2"));
                        Taxbillrate = Itaxbillrate.Text;
                    }
                    catch { }
                    try
                    {
                        IWebElement Itaxarea = driver.FindElement(By.XPath("//*[@id='middle']/table[2]/tbody/tr/td[2]"));
                        Taxarea = Itaxarea.Text;
                    }
                    catch { }
                    gc.CreatePdf(orderNumber, parcelNumber, "Tax Area Details", driver, "AZ", "Mohave");

                    try
                    {
                        IWebElement taxareadetails = driver.FindElement(By.XPath("//*[@id='middle']/table[3]/tbody"));
                        IList<IWebElement> TRtaxareadetails = taxareadetails.FindElements(By.TagName("tr"));
                        IList<IWebElement> THtaxareadetails = taxareadetails.FindElements(By.TagName("th"));
                        IList<IWebElement> TDtaxareadetails;
                        foreach (IWebElement row in TRtaxareadetails)
                        {
                            TDtaxareadetails = row.FindElements(By.TagName("td"));

                            if (TDtaxareadetails.Count != 0 && !row.Text.Contains("Authority Id"))
                            {
                                string Taxareadetails = AcNumber + "~" + Taxbillrate + "~" + Taxarea + "~" + TDtaxareadetails[0].Text + "~" + TDtaxareadetails[1].Text + "~" + TDtaxareadetails[2].Text + "~" + TDtaxareadetails[3].Text + "~" + TDtaxareadetails[4].Text;
                                gc.insert_date(orderNumber, parcelNumber, 1193, Taxareadetails, 1, DateTime.Now);
                            }

                        }
                    }
                    catch { }

                    // Property Code Details 
                    try
                    {
                        IWebElement Propertycodedetails = driver.FindElement(By.XPath("//*[@id='middle']/table[4]/tbody"));
                        IList<IWebElement> TRPropertycodedetails = Propertycodedetails.FindElements(By.TagName("tr"));
                        IList<IWebElement> THPropertycodedetails = Propertycodedetails.FindElements(By.TagName("th"));
                        IList<IWebElement> TDPropertycodedetails;
                        foreach (IWebElement row in TRPropertycodedetails)
                        {
                            TDPropertycodedetails = row.FindElements(By.TagName("td"));

                            if (TDPropertycodedetails.Count != 0 && !row.Text.Contains("Property Code"))
                            {
                                string propertycodedetails = TDPropertycodedetails[0].Text + "~" + TDPropertycodedetails[1].Text + "~" + TDPropertycodedetails[2].Text + "~" + TDPropertycodedetails[3].Text;
                                gc.insert_date(orderNumber, parcelNumber, 1197, propertycodedetails, 1, DateTime.Now);
                            }

                        }
                    }
                    catch { }

                    try
                    {
                        ByVisibleElement(driver.FindElement(By.XPath("//*[@id='middle']/table[4]/tbody")));
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Property Code Details", driver, "AZ", "Mohave");
                    }
                    catch { }

                    // Tax Summary Details

                    driver.FindElement(By.LinkText("Transaction Detail")).Click();
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, parcelNumber, "Tax Summary", driver, "AZ", "Mohave");
                    IWebElement Taxsummary = driver.FindElement(By.XPath("//*[@id='middle']/table[1]/tbody"));
                    IList<IWebElement> TRTaxsummary = Taxsummary.FindElements(By.TagName("tr"));
                    IList<IWebElement> THTaxsummary = Taxsummary.FindElements(By.TagName("th"));
                    IList<IWebElement> TDTaxsummary;
                    foreach (IWebElement row in TRTaxsummary)
                    {
                        TDTaxsummary = row.FindElements(By.TagName("td"));

                        if (TDTaxsummary.Count != 0)
                        {
                            string Taxsummarydetails = TDTaxsummary[0].Text + "~" + TDTaxsummary[1].Text + "~" + TDTaxsummary[2].Text + "~" + TDTaxsummary[3].Text + "~" + TDTaxsummary[4].Text + "~" + TDTaxsummary[5].Text + "~" + TDTaxsummary[6].Text + "~" + TDTaxsummary[7].Text;
                            gc.insert_date(orderNumber, parcelNumber, 1198, Taxsummarydetails, 1, DateTime.Now);
                        }

                    }

                    // Transaction Details Table:
                    try
                    {
                        IWebElement Transaction = driver.FindElement(By.XPath("//*[@id='middle']/table[2]/tbody"));
                        IList<IWebElement> TRTransaction = Transaction.FindElements(By.TagName("tr"));
                        IList<IWebElement> THTransaction = Transaction.FindElements(By.TagName("th"));
                        IList<IWebElement> TDTransaction;
                        foreach (IWebElement row in TRTransaction)
                        {
                            TDTransaction = row.FindElements(By.TagName("td"));

                            if (TDTransaction.Count != 0)
                            {
                                string Transactiondetails = TDTransaction[0].Text + "~" + TDTransaction[1].Text + "~" + TDTransaction[2].Text + "~" + TDTransaction[3].Text + "~" + TDTransaction[4].Text;
                                gc.insert_date(orderNumber, parcelNumber, 1199, Transactiondetails, 1, DateTime.Now);
                            }

                        }
                    }
                    catch { }
                    try
                    {
                        ByVisibleElement(driver.FindElement(By.XPath("//*[@id='middle']/table[2]/tbody")));
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Transactional Details", driver, "AZ", "Mohave");
                    }
                    catch { }

                    // Account Balance
                    string fileName1 = "";
                    try
                    {
                        driver.FindElement(By.LinkText("Account Balance")).Click();
                        Thread.Sleep(7000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Account Balance", driver, "AZ", "Mohave");
                        driver.FindElement(By.LinkText("Account Balance")).Click();
                        Thread.Sleep(5000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Account Balance Download", driver, "AZ", "Mohave");
                        driver.Navigate().Back();
                        Thread.Sleep(4000);
                        driver.Navigate().Back();
                        Thread.Sleep(4000);

                    }
                    catch { }

                    // Statement Of Taxes Due  

                    try
                    {
                        driver.FindElement(By.LinkText("Statement Of Taxes Due")).Click();
                        Thread.Sleep(5000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Statement Of Taxes Due", driver, "AZ", "Mohave");
                        try
                        {
                            driver.FindElement(By.XPath("//*[@id='myReports']/table/tbody/tr[2]/td[1]/a")).Click();
                            Thread.Sleep(5000);
                            gc.CreatePdf(orderNumber, parcelNumber, "Statement Of Taxes Due Download", driver, "AZ", "Mohave");
                        }
                        catch { }

                        driver.Navigate().Back();
                        Thread.Sleep(4000);
                        driver.Navigate().Back();
                        Thread.Sleep(4000);
                    }
                    catch (Exception ex)
                    { }

                    // Summary of Taxes Due

                    try
                    {
                        driver.FindElement(By.XPath("//*[@id='left']/div[1]/a[3]")).Click();
                        Thread.Sleep(5000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Statement Of Taxes Due-2", driver, "AZ", "Mohave");
                        try
                        {
                            driver.FindElement(By.XPath("//*[@id='myReports']/table/tbody/tr[2]/td[1]/a")).Click();
                            Thread.Sleep(4000);
                            gc.CreatePdf(orderNumber, parcelNumber, "Statement Of Taxes Due-2 Download", driver, "AZ", "Mohave");
                        }
                        catch { }

                        driver.Navigate().Back();
                        Thread.Sleep(2000);
                        driver.Navigate().Back();
                        Thread.Sleep(2000);
                    }
                    catch { }





                    // Payments Receipt
                    int i = 1;
                    string tableassess = "";
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
                        Thread.Sleep(7000);
                        try
                        {
                            driver1.FindElement(By.XPath("//*[@id='middle_left']/form/input[1]")).Click();
                            Thread.Sleep(4000);
                        }
                        catch { }
                        try
                        {
                            driver.FindElement(By.Id("TaxAParcelID")).SendKeys(parcelNumber.Replace("-", ""));

                            driver.FindElement(By.XPath("//*[@id='middle']/form/table[5]/tbody/tr/td[1]/input")).Click();
                            Thread.Sleep(2000);

                            driver.FindElement(By.XPath("//*[@id='searchResultsTable']/tbody/tr/td[2]/a/table/tbody/tr/td[1]/b/font")).Click();
                            Thread.Sleep(2000);
                        }
                        catch { }

                        string fileName = "";
                        IWebElement IParcelAssess = driver1.FindElement(By.XPath("//*[@id='receiptHistory']"));
                        IList<IWebElement> IParcelAssessList = IParcelAssess.FindElements(By.TagName("a"));
                        foreach (IWebElement parcel in IParcelAssessList)
                        {
                            if (!parcel.Text.Contains("Payment"))
                            {
                                if (i < 7)
                                {
                                    parcel.Click();
                                    Thread.Sleep(10000);

                                    IWebElement Receipttable = driver1.FindElement(By.XPath("//*[@id='receiptHistory']/a[" + i + "]"));
                                    string BillTax2 = Receipttable.GetAttribute("href");
                                    fileName = gc.Between(BillTax2, "taxreceipt/", "?id=").Replace("-", "_");
                                    gc.AutoDownloadFile(orderNumber, parcelNumber, "Mohave", "AZ", fileName);
                                    Thread.Sleep(10000);
                                    string FilePathCurrentTaxInfo22 = gc.filePath(orderNumber, parcelNumber) + fileName;
                                    PdfReader reader22;
                                    reader22 = new PdfReader(FilePathCurrentTaxInfo22);
                                    string textFromPage22 = PdfTextExtractor.GetTextFromPage(reader22, 1);
                                    System.Diagnostics.Debug.WriteLine("" + textFromPage22);

                                    string pdftext22 = textFromPage22;


                                    try
                                    {
                                        tableassess = gc.Between(pdftext22, "Receipt Number", "Situs Address").Trim();
                                        string[] tableArray = tableassess.Split('\n');
                                        for (int c = 0; c < 1; c++)
                                        {
                                            int count1 = tableArray.Length;
                                            string a1 = tableArray[c].Replace(" ", "~");
                                            string[] rowarray = a1.Split('~');
                                            int tdcount = rowarray.Length;
                                            if (tdcount < 7)
                                            {
                                                string datepdf = rowarray[0] + "~" + rowarray[2] + "" + rowarray[3] + " " + rowarray[4] + "~" + " " + "~" + rowarray[5];
                                                gc.insert_date(orderNumber, parcelNumber, 1258, datepdf, 1, DateTime.Now);
                                            }
                                            if (tdcount > 6)
                                            {
                                                string datepdf = rowarray[0] + "~" + rowarray[2] + "" + rowarray[3] + " " + rowarray[4] + "~" + rowarray[5] + " " + rowarray[6] + " " + rowarray[7] + "~" + rowarray[8];
                                                gc.insert_date(orderNumber, parcelNumber, 1258, datepdf, 1, DateTime.Now);
                                            }

                                        }
                                    }
                                    catch { }


                                }
                                i++;
                            }
                        }
                    }
                    catch (Exception ex) { }
                    TaxTime = DateTime.Now.ToString("HH:mm:ss");
                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "AZ", "Mohave", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);
                    driver.Quit();
                    gc.mergpdf(orderNumber, "AZ", "Mohave");
                    return "Data Inserted Successfully";
                }


                catch (Exception ex)
                {
                    driver.Quit();
                    throw ex;
                }
            }
        }
        public void ByVisibleElement(IWebElement Element)
        {
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            js.ExecuteScript("arguments[0].scrollIntoView();", Element);
        }

    }
}