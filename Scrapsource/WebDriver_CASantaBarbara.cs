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

namespace ScrapMaricopa.Scrapsource
{
    public class WebDriver_CASantaBarbara
    {
        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        MySqlParameter[] mParam;
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        string outputpath = "-", outparcelnowoh="";
        string MultiParcelData = "-", Situs_Address = "", strAssess = "-";
        string property_Address = "-", outparcelno = "-", Street_Address = "-", Address = "-", TRA = "-", Transfer_TaxAmount = "-", Use_Description = "-", Jurisdiction = "-", Acreage = "-", Year_Built = "-";
        string assement_details = "-", Land_MineralRights = "-", Improvements = "-", Personal_Property = "-", Home_OwnerExemption = "-", Other_Exemption = "-", Net_AssessedValue = "-", Fund = "-", Amount = "-", taxUrl = "-", Taxrate_Area = "-", NetAssessed_Value = "-", NoteTax = "-", proprtyTax = "-";
        string Property_Type = "-", Current_PropertyAddress = "-", Property_Remarks = "-", TotalDue = "-", Bill_No = "-", Bill_Date = "-", Bill_Year = "-", Bill_PropertyAddress = "-", Bill_FirstInstallment = "-", Bill_SecondInstallment = "-", Tax_Authority = "-", Bill_Tax1 = "-", Bill_Tax = "-", Taxauthority = "-";
        string Tax_Bills = "", Cancelled = "", Bill_Dates = "", Bill_Number = "", Value_Type = "", Bill_Type = "", TaxYear_BillYear = "", Land = "", Improvements1 = "", Trade_Fixtures = "", Personal_Property1 = "", Penalties = "", Gross_Value = "", HomeOwner_Exemp = "", Other_Exemptions = "", Net_Value = "",
                    OnePercent_Tax = "", Fixed_Charges = "", Special_Districts = "", Total_Tax = "", Delinquent_Penalty = "", Cost = "", Additional_Penalty = "", Fees = "", Total_Billed = "", Transfered = "", Refunded = "", Cancelled_Tax = "", Payments = "", Total_Due;
        string Frow1 = "-", Row1 = "-", Frow2 = "-", Row2 = "-", Frow3 = "-", Row3 = "-", Payment_Date = "-", Effective_PaymentDate = "-", BillNumber = "-", Year = "-", TaxBill_Type = "-", Installment = "-", Payment_Batch = "-", Payment_Type = "-", Payment_Amount = "-", Tax_Receipts = "-";

        IWebElement Tax_Receipt;
        public string FTP_CASantaBarbara(string houseno, string sname, string sttype, string parcelNumber,string unitnumber, string searchType, string orderNumber, string ownername, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;

            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";

            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            // driver = new PhantomJSDriver();
            // driver = new ChromeDriver();
            using (driver = new PhantomJSDriver())
            {
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");

                    if (searchType == "titleflex")
                    {
                        gc.TitleFlexSearch(orderNumber, parcelNumber, ownername, "", "CA", "Santa Barbara");
                        if (HttpContext.Current.Session["titleparcel"] != null && ownername.Trim() != "")
                        {
                            string[] strowner = ownername.Split(' ');
                            gc.TitleFlexSearch(orderNumber, parcelNumber, strowner[0], "", "CA", "Santa Barbara");
                        }
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_CASantaBarbara"] = "Zero";
                            driver.Quit();
                            return "No Data Found";
                        }
                        searchType = "parcel";
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                    }

                    if (searchType == "address")
                    {
                        driver.Navigate().GoToUrl("http://www.sbcvote.com/assessor/search.aspx");
                        Thread.Sleep(2000);
                        driver.FindElement(By.Id("HouseNumberTextBox")).SendKeys(houseno);
                        driver.FindElement(By.Id("StreetNameTextBox")).SendKeys(sname);
                        driver.FindElement(By.Id("UnitNumberTextBox")).SendKeys(unitnumber);
                        //Screen-Shot
                        gc.CreatePdf_WOP(orderNumber, "AddressSearch", driver, "CA", "Santa Barbara");
                        driver.FindElement(By.Id("SearchButton")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);

                        //MultiParcel
                        try
                        {
                            IWebElement MultiParcelTable = driver.FindElement(By.XPath("/html/body/form/table/tbody/tr[2]/td[2]/table/tbody/tr[2]/td/table/tbody/tr[2]/td[2]/table/tbody/tr[3]/td[2]/table/tbody/tr[5]/td/table"));
                            IList<IWebElement> MultiParcelTR = MultiParcelTable.FindElements(By.TagName("tr"));
                            IList<IWebElement> MultiParcelTD;

                            foreach (IWebElement multi in MultiParcelTR)
                            {
                                MultiParcelTD = multi.FindElements(By.TagName("td"));
                                if (multi.Text.Contains("Assessor Parcel Number (APN)"))
                                {
                                    strAssess = "MultiParcel";
                                }
                                if (MultiParcelTD.Count != 0 && strAssess.Trim() == "MultiParcel" && !multi.Text.Contains("Assessor Parcel Number (APN)") && MultiParcelTD.Count != 1 && MultiParcelTD[1].Text != "")
                                {
                                    HttpContext.Current.Session["multiParcel_CASantaBarbara"] = "Yes";
                                    parcelNumber = MultiParcelTD[1].Text;
                                    Situs_Address = MultiParcelTD[2].Text;
                                    MultiParcelData = Situs_Address;
                                    gc.insert_date(orderNumber, parcelNumber, 220, MultiParcelData, 1, DateTime.Now);
                                }
                            }
                            if (strAssess == "MultiParcel")
                            {
                                driver.Quit();
                                return "MultiParcel";
                            }
                        }
                        catch
                        { }
                    }

                    if (searchType == "parcel")
                    {
                        driver.Navigate().GoToUrl("http://www.sbcvote.com/assessor/search.aspx");
                        Thread.Sleep(2000);

                        driver.FindElement(By.Id("APNTextBox")).SendKeys(parcelNumber);
                        gc.CreatePdf(orderNumber, parcelNumber, "ParcelSearch", driver, "CA", "Santa Barbara");
                        driver.FindElement(By.Id("SearchButton")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                    }
                    try
                    {
                        IWebElement Inodata = driver.FindElement(By.Id("ErrorLabel"));
                        if(Inodata.Text.Contains("There were no properties matching your query"))
                        {
                            HttpContext.Current.Session["Nodata_CASantaBarbara"] = "Zero";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }
                    //Scrapped Data 

                    //Property Deatails 
                    string propdet = driver.FindElement(By.XPath("//*[@id='form1']/table/tbody/tr[2]/td[2]/table/tbody/tr[2]/td/table/tbody/tr[2]/td[2]/table/tbody/tr[3]/td[2]/table/tbody/tr[5]/td/table/tbody/tr[2]/td/table/tbody")).Text.Replace("\r\n", " ");
                    outparcelno = gc.Between(propdet, "Parcel Number:", "Address:").Trim().Replace("Value Notice", "");
                    outparcelnowoh = outparcelno.Replace("-", "").Trim();
                    Street_Address = gc.Between(propdet, "Address:", "Transfer Date:").Trim().Replace("Value Notice", "");                    
                    TRA = gc.Between(propdet, "TRA:", "Document").Trim();
                    Transfer_TaxAmount = GlobalClass.After(propdet, "Transfer Tax Amount:").Trim();

                    string propdet1 = driver.FindElement(By.XPath("//*[@id='form1']/table/tbody/tr[2]/td[2]/table/tbody/tr[2]/td/table/tbody/tr[2]/td[2]/table/tbody/tr[3]/td[2]/table/tbody/tr[5]/td/table/tbody/tr[3]/td/table")).Text.Replace("\r\n", " ");
                    Use_Description = gc.Between(propdet1, "Use Description:", "Jurisdiction:").Trim();
                    Jurisdiction = gc.Between(propdet1, "Jurisdiction:", "Acreage:").Trim();
                    Acreage = gc.Between(propdet1,  "Acreage:", "Square Feet:").Trim();
                    Year_Built = gc.Between(propdet1, "Year Built:", "Bedrooms:").Trim();

                    gc.CreatePdf(orderNumber, outparcelnowoh, "Assement", driver, "CA", "Santa Barbara");
                    property_Address = Street_Address +  "~" + TRA + "~" + Transfer_TaxAmount + "~" + Use_Description + "~" + Jurisdiction + "~" + Acreage + "~" + Year_Built;
                    gc.insert_date(orderNumber, outparcelno, 221, property_Address, 1, DateTime.Now);

                    //Assessment Details
                    Land_MineralRights = driver.FindElement(By.XPath("/html/body/form/table/tbody/tr[2]/td[2]/table/tbody/tr[2]/td/table/tbody/tr[2]/td[2]/table/tbody/tr[3]/td[2]/table/tbody/tr[5]/td/table/tbody/tr[6]/td/table/tbody/tr[2]/td[3]")).Text;
                    Improvements = driver.FindElement(By.XPath("/html/body/form/table/tbody/tr[2]/td[2]/table/tbody/tr[2]/td/table/tbody/tr[2]/td[2]/table/tbody/tr[3]/td[2]/table/tbody/tr[5]/td/table/tbody/tr[6]/td/table/tbody/tr[4]/td[3]")).Text;
                    Personal_Property = driver.FindElement(By.XPath("/html/body/form/table/tbody/tr[2]/td[2]/table/tbody/tr[2]/td/table/tbody/tr[2]/td[2]/table/tbody/tr[3]/td[2]/table/tbody/tr[5]/td/table/tbody/tr[6]/td/table/tbody/tr[6]/td[3]")).Text;
                    Home_OwnerExemption = driver.FindElement(By.XPath("/html/body/form/table/tbody/tr[2]/td[2]/table/tbody/tr[2]/td/table/tbody/tr[2]/td[2]/table/tbody/tr[3]/td[2]/table/tbody/tr[5]/td/table/tbody/tr[6]/td/table/tbody/tr[8]/td[3]")).Text;
                    Other_Exemption = driver.FindElement(By.XPath("/html/body/form/table/tbody/tr[2]/td[2]/table/tbody/tr[2]/td/table/tbody/tr[2]/td[2]/table/tbody/tr[3]/td[2]/table/tbody/tr[5]/td/table/tbody/tr[6]/td/table/tbody/tr[10]/td[3]")).Text;
                    Net_AssessedValue = driver.FindElement(By.XPath("/html/body/form/table/tbody/tr[2]/td[2]/table/tbody/tr[2]/td/table/tbody/tr[2]/td[2]/table/tbody/tr[3]/td[2]/table/tbody/tr[5]/td/table/tbody/tr[6]/td/table/tbody/tr[12]/td[3]")).Text;

                    assement_details = Land_MineralRights + "~" + Improvements + "~" + Personal_Property + "~" + Home_OwnerExemption + "~" + Other_Exemption + "~" + Net_AssessedValue;
                    gc.insert_date(orderNumber, outparcelno, 222, assement_details, 1, DateTime.Now);
                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");

                    //Download the pdf for Tax Details 
                    String Parent_Window = driver.CurrentWindowHandle;
                    string cururl = driver.Url;

                    //Value_Notice
                    IWebElement ValueNotice = driver.FindElement(By.XPath("/html/body/form/table/tbody/tr[2]/td[2]/table/tbody/tr[2]/td/table/tbody/tr[2]/td[2]/table/tbody/tr[3]/td[2]/table/tbody/tr[5]/td/table/tbody/tr[2]/td/table/tbody/tr[2]/td[3]/table/tbody/tr/td[2]/span/strong/a"));
                    IJavaScriptExecutor js1 = driver as IJavaScriptExecutor;
                    js1.ExecuteScript("arguments[0].click();", ValueNotice);
                    Thread.Sleep(2000);
                    System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    driver.SwitchTo().Window(driver.WindowHandles.Last());
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, outparcelnowoh, "Value_Tax.pdf", driver, "CA", "Santa Barbara");
                    // driver.SwitchTo().Window(Parent_Window);
                    driver.Navigate().GoToUrl(cururl);
                    Thread.Sleep(2000);

                    //Jurisdiction Details
                    try
                    {
                        taxUrl = driver.FindElement(By.XPath("//*[@id='lblAuditorLink']/a")).GetAttribute("href").ToString();

                        driver.Navigate().GoToUrl(taxUrl);


                        IWebElement SelectOption = driver.FindElement(By.Id("Criteria#001"));
                        IList<IWebElement> Select = SelectOption.FindElements(By.TagName("option"));
                        List<string> option = new List<string>();
                        string opt = "";
                        int Check = 0;
                        foreach (IWebElement Op in Select)
                        {
                            if (Check == 1)
                            {
                                //option.Add(Op.Text);
                                opt = Op.Text;
                                break;
                            }
                            Check++;
                        }
                        //foreach (string item in option)
                        //{
                        var SelectAddress = driver.FindElement(By.Id("Criteria#001"));
                        var SelectAddressTax = new SelectElement(SelectAddress);
                        SelectAddressTax.SelectByText(opt);
                        driver.FindElement(By.Name("Submit")).Click();
                        Thread.Sleep(2000);
                        IWebElement asstable = driver.FindElement(By.XPath("/html/body/table[5]/tbody/tr/td[2]/table[1]/tbody/tr[2]/td[2]/h1/table[1]/tbody"));
                        IList<IWebElement> asstr = asstable.FindElements(By.TagName("tr"));
                        IList<IWebElement> asstd;
                        foreach (IWebElement tax in asstr)
                        {
                            asstd = tax.FindElements(By.TagName("td"));
                            if (asstd.Count != 0 && asstd.Count == 5 && tax.Text.Trim() != "" && !tax.Text.Contains("Parcel Number") || (tax.Text.Contains("Basic") || tax.Text.Contains("Total") || tax.Text.Contains("Fixed") || tax.Text.Contains("Situs")))
                            {
                                Taxrate_Area = asstd[2].Text.Trim();
                                NetAssessed_Value = asstd[4].Text.Trim();
                            }
                        }



                        gc.CreatePdf(orderNumber, outparcelnowoh, "Property_Tax.pdf", driver, "CA", "Santa Barbara");
                        NoteTax = Taxrate_Area + "~" + NetAssessed_Value + "~" + "-" + "~" + "-";
                        gc.insert_date(orderNumber, outparcelno, 239, NoteTax, 1, DateTime.Now);
                        //}

                        IWebElement TBTax = driver.FindElement(By.XPath("/html/body/table[5]/tbody/tr/td[2]/table[1]/tbody/tr[2]/td[2]/h1/table[2]/tbody"));
                        IList<IWebElement> TRTax = TBTax.FindElements(By.TagName("tr"));
                        IList<IWebElement> TDTax;

                        foreach (IWebElement tax in TRTax)
                        {
                            TDTax = tax.FindElements(By.TagName("td"));
                            if (TDTax.Count != 0 && TDTax.Count != 1 && tax.Text.Trim() != "" && !tax.Text.Contains("Fund") || (tax.Text.Contains("Basic") || tax.Text.Contains("Total") || tax.Text.Contains("Fixed") || tax.Text.Contains("Bonds") || tax.Text.Contains("9801 - County School Srvc Fund")))
                            {
                                try
                                {
                                    Fund = TDTax[0].Text;
                                    Amount = TDTax[2].Text;
                                }
                                catch { Amount = "-"; }

                                proprtyTax = "-" + "~" + "-" + "~" + Fund + "~" + Amount;
                                gc.insert_date(orderNumber, parcelNumber, 239, proprtyTax, 1, DateTime.Now);

                            }
                        }
                    }
                    catch { }

                    driver.Navigate().GoToUrl("https://Mytaxes.sbtaxes.org/WebPages/Assistance_ContactUs.aspx");
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, outparcelnowoh, "Current_Authority.pdf", driver, "CA", "Santa Barbara");
                    Tax_Authority = driver.FindElement(By.XPath("//*[@id='ctl00_PageContentBody_ASPxPageControl1_C0']/div/div[1]/div[2]/div/h4")).Text + " " + driver.FindElement(By.XPath("/html/body/form/div[3]/div[2]/div[2]/div/div/div[2]/div[1]/div/div[1]/div/div[1]/div[2]/div/ul")).Text;

                    //Tax Information

                    driver.Navigate().GoToUrl("http://www.countyofsb.org/ttcpapg/index.aspx");
                    Thread.Sleep(2000);

                    try
                    {
                        IWebElement TaxInfo = driver.FindElement(By.XPath("/html/body/form/div[3]/div[2]/div[2]/div[2]/div/a"));
                        IJavaScriptExecutor TI = driver as IJavaScriptExecutor;
                        TI.ExecuteScript("arguments[0].click();", TaxInfo);
                        Thread.Sleep(2000);

                        WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                        var element = wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("/html/body/form/div[3]/div[1]/div[2]/div[2]/div[1]/table/tbody/tr/td[2]")));
                        Actions action = new Actions(driver);
                        action.MoveToElement(element).Perform();
                        Thread.Sleep(2000);

                        driver.FindElement(By.XPath("/html/body/form/div[3]/div[1]/div[2]/div[2]/div[1]/table/tbody/tr/td[2]/div/div[2]/div/ul/li[1]/div/span")).Click();
                        Thread.Sleep(2000);
                        driver.FindElement(By.Id("ctl00_ASPxCallbackPanel1_combo_Search_I")).SendKeys(outparcelno);
                        gc.CreatePdf(orderNumber, outparcelnowoh, "City_Info.pdf", driver, "CA", "Santa Barbara");
                        driver.FindElement(By.Id("ctl00_ASPxCallbackPanel1_btn_Search")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, outparcelnowoh, "City_Info_tax.pdf", driver, "CA", "Santa Barbara");
                        //Tax Bill

                        //Download the pdf for Tax Details 
                        //String Parent_Window1 = driver.CurrentWindowHandle;

                        //IWebElement taxbill = driver.FindElement(By.XPath("//*[@id='ctl00_PageContentBody_tabControl_Propertys_gridView_BillDetail_cell1_0_BillMenu_DXI0_T']"));
                        //IJavaScriptExecutor TB = driver as IJavaScriptExecutor;
                        //TB.ExecuteScript("arguments[0].click();", taxbill);
                        //Thread.Sleep(2000);

                        //System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                        //driver.SwitchTo().Window(driver.WindowHandles.Last());
                        //Thread.Sleep(4000);
                        //gc.CreatePdf_Chrome(orderNumber, outparcelno, "Tax_Bill.pdf", driver, "CA", "Santa Barbara");
                        //driver.SwitchTo().Window(Parent_Window1);
                        //Thread.Sleep(2000);

                    }
                    catch
                    { }

                    try
                    {
                        //Current Tax Details 
                        Property_Type = driver.FindElement(By.XPath("/html/body/form/div[3]/div[2]/div[2]/div/table/tbody/tr/td/table/tbody/tr[2]/td[2]")).Text;
                        try
                        {
                            Current_PropertyAddress = driver.FindElement(By.Id("ctl00_PageContentBody_gridView_Propertys_cell0_2_ASPxSitusAddressLine1")).Text + " " + driver.FindElement(By.Id("ctl00_PageContentBody_gridView_Propertys_cell0_2_ASPxSitusAddressLine2")).Text;
                        }
                        catch { }
                        Property_Remarks = driver.FindElement(By.XPath("/html/body/form/div[3]/div[2]/div[2]/div/table/tbody/tr/td/table/tbody/tr[2]/td[4]")).Text;
                        TotalDue = driver.FindElement(By.XPath("/html/body/form/div[3]/div[2]/div[2]/div/table/tbody/tr/td/table/tbody/tr[2]/td[6]")).Text;
                        gc.CreatePdf(orderNumber, outparcelnowoh, "Current_Tax.pdf", driver, "CA", "Santa Barbara");
                        Bill_Tax1 = Property_Type + "~" + Current_PropertyAddress + "~" + Property_Remarks + "~" + TotalDue + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-";
                        gc.insert_date(orderNumber, outparcelno, 242, Bill_Tax1, 1, DateTime.Now);

                        IWebElement TBCurrnetTax = driver.FindElement(By.XPath("/html/body/form/div[3]/div[2]/div[2]/div/div[2]/div/div[1]/div/table/tbody/tr/td/table[2]/tbody"));
                        IList<IWebElement> TRCurrnetTax = TBCurrnetTax.FindElements(By.TagName("tr"));
                        IList<IWebElement> TDCurrnetTax;
                        int i = 3;
                        foreach (IWebElement CurrentTax in TRCurrnetTax)
                        {
                            TDCurrnetTax = CurrentTax.FindElements(By.TagName("td"));
                            if (TDCurrnetTax.Count != 0 && !CurrentTax.Text.Contains("Bill Menu") && !CurrentTax.Text.Contains("Paid Bills") && !CurrentTax.Text.Contains("Date") && !CurrentTax.Text.Contains("Year") && !CurrentTax.Text.Contains("Property Address") && !CurrentTax.Text.Contains("First Installment") && !CurrentTax.Text.Contains("Second Installment") && CurrentTax.Text.Trim() != "")
                            {
                                Bill_No = driver.FindElement(By.XPath("/html/body/form/div[3]/div[2]/div[2]/div/div[2]/div/div[1]/div/table/tbody/tr/td/table[2]/tbody/tr[" + i + "]/td[2]/div")).Text;
                                if (Bill_No.Contains("\r\n"))
                                {
                                    Bill_No = Bill_No.Replace("\r\n", " ");
                                }
                                Bill_Date = TDCurrnetTax[2].Text;
                                Bill_Year = TDCurrnetTax[3].Text;
                                Bill_PropertyAddress = TDCurrnetTax[4].Text;
                                Bill_FirstInstallment = TDCurrnetTax[5].Text;
                                Bill_SecondInstallment = TDCurrnetTax[6].Text;

                                Bill_Tax = "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + Bill_No + "~" + Bill_Date + "~" + Bill_Year + "~" + Bill_PropertyAddress + "~" + Bill_FirstInstallment + "~" + Bill_SecondInstallment + "~" + "-";
                                gc.insert_date(orderNumber, outparcelno, 242, Bill_Tax, 1, DateTime.Now);
                                i++;
                            }
                        }

                        try
                        {
                            IWebElement Ilink = driver.FindElement(By.Id("ctl00_PageContentBody_tabControl_Propertys_gridView_BillDetail_cell1_0_BillMenu_DXI0_T"));
                            string strLink = "https://mytaxes.sbtaxes.org/WebPages/" + gc.Between(Ilink.GetAttribute("href"), "javascript: PopUpCenter('", "||");
                            gc.downloadfile(strLink, orderNumber, outparcelnowoh, "TaxBill.pdf", "CA", "Santa Barbara");
                        }
                        catch { }


                        // driver.Navigate().Back();
                        // Thread.Sleep(2000);
                    }
                    catch
                    { }
                    try
                    {
                        Taxauthority = "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + Tax_Authority;
                        gc.insert_date(orderNumber, outparcelnowoh, 242, Taxauthority, 1, DateTime.Now);
                    }
                    catch { }

                    try
                    {
                        //Taxes and Values Details
                        string Ok = "";
                        IWebElement Tax_Value = driver.FindElement(By.XPath("//*[@id='ctl00_PageContentBody_tabControl_Propertys_T1T']"));
                        IJavaScriptExecutor TV = driver as IJavaScriptExecutor;
                        TV.ExecuteScript("arguments[0].click();", Tax_Value);
                        Thread.Sleep(2000);

                        List<string> List1 = new List<string>();
                        List<string> List2 = new List<string>();
                        List<string> List3 = new List<string>();
                        List<string> List4 = new List<string>();
                        int K = 0;
                        IWebElement valuetableElement = driver.FindElement(By.XPath("/html/body/form/div[3]/div[2]/div[2]/div/div[2]/div/div[2]/div/div/table/tbody/tr/td/div[1]/table/tbody"));
                        IList<IWebElement> valuetableRow = valuetableElement.FindElements(By.TagName("tr"));
                        IList<IWebElement> valuerowTD;
                        gc.CreatePdf(orderNumber, outparcelnowoh, "Current_Value.pdf", driver, "CA", "Santa Barbara");
                        foreach (IWebElement row in valuetableRow)
                        {
                            valuerowTD = row.FindElements(By.TagName("td"));
                            if (valuerowTD.Count != 0 && valuerowTD.Count != 1 && Ok == "OK")
                            {
                                if (valuerowTD.Count == 2)
                                {
                                    if (valuerowTD[0].Text.Trim() != "")
                                    {
                                        string Taxnumber = valuerowTD[0].Text;
                                        List4.Add(Taxnumber);
                                    }
                                }
                                else
                                {
                                    if (valuerowTD[1].Text.Trim() != "" && valuerowTD[2].Text.Trim() != "" && valuerowTD[3].Text.Trim() != "")
                                    {
                                        string Value1 = valuerowTD[1].Text;
                                        List1.Add(Value1);
                                        string Value2 = valuerowTD[2].Text;
                                        List2.Add(Value2);
                                        string Value3 = valuerowTD[3].Text;
                                        List3.Add(Value3);
                                    }
                                }
                            }
                            if (valuerowTD.Count != 0 && valuerowTD.Count != 1)
                            {

                                if (valuerowTD[0].Text.Contains("Tax Bills"))
                                {

                                    if (K == 1)
                                    {
                                        Ok = "OK";
                                    }
                                    K++;
                                }
                            }
                        }

                        Tax_Bills = List4[0] + "~" + List4[1] + "~" + List4[2];

                        Frow1 = List4[0] + "~" + List1[0] + "~" + List2[0] + "~" + List3[0];
                        Row1 = "";
                        for (int X = List1.Count - 1; X >= 0; X--)
                        {
                            Row1 = List1[X] + "~" + Row1;
                        }
                        Row1 = List4[0] + "~" + Row1 + "Wer";
                        Row1 = Row1.Replace("~Wer", "");
                        gc.insert_date(orderNumber, outparcelno, 259, Row1, 1, DateTime.Now);

                        Frow2 = List4[0] + "~" + List1[0] + "~" + List2[0] + "~" + List3[0];
                        Row2 = "";
                        for (int X = List2.Count - 1; X >= 0; X--)
                        {
                            Row2 = List2[X] + "~" + Row2;
                        }
                        Row2 = List4[1] + "~" + Row2 + "Wer";
                        Row2 = Row2.Replace("~Wer", "");
                        gc.insert_date(orderNumber, outparcelno, 259, Row2, 1, DateTime.Now);

                        Frow3 = List4[0] + "~" + List1[0] + "~" + List2[0] + "~" + List3[0];
                        Row3 = "";
                        for (int X = List3.Count - 1; X >= 0; X--)
                        {
                            Row3 = List3[X] + "~" + Row3;
                        }
                        Row3 = List4[2] + "~" + Row3 + "Wer";
                        Row3 = Row3.Replace("~Wer", "");
                        gc.insert_date(orderNumber, outparcelno, 259, Row3, 1, DateTime.Now);
                    }
                    catch
                    { }

                    //Tax Payment Receipts
                    try
                    {
                        try
                        {
                            Tax_Receipt = driver.FindElement(By.XPath("//*[@id='ctl00_PageContentBody_tabControl_Propertys_T2T']"));
                        }
                        catch { }
                        gc.CreatePdf(orderNumber, outparcelnowoh, "Current_Payment.pdf", driver, "CA", "Santa Barbara");
                        IJavaScriptExecutor TR = driver as IJavaScriptExecutor;
                        TR.ExecuteScript("arguments[0].click();", Tax_Receipt);
                        Thread.Sleep(2000);


                        int i = 0;
                        IWebElement TBTaxReceipts = driver.FindElement(By.XPath("/html/body/form/div[3]/div[2]/div[2]/div/div[2]/div/div[3]/div/table/tbody/tr/td/table[2]/tbody"));
                        IList<IWebElement> TRTaxReceipts = TBTaxReceipts.FindElements(By.TagName("tr"));
                        IList<IWebElement> TDTaxReceipts;
                        gc.CreatePdf(orderNumber, outparcelno, "Current_Payment.pdf", driver, "CA", "Santa Barbara");
                        foreach (IWebElement TaxReceipt in TRTaxReceipts)
                        {
                            TDTaxReceipts = TaxReceipt.FindElements(By.TagName("td"));
                            if (TDTaxReceipts.Count == 9)
                            {

                                Payment_Date = TDTaxReceipts[0].Text;
                                Effective_PaymentDate = TDTaxReceipts[1].Text;
                                BillNumber = TDTaxReceipts[2].Text;
                                Year = TDTaxReceipts[3].Text;
                                TaxBill_Type = TDTaxReceipts[4].Text;
                                Installment = TDTaxReceipts[5].Text;
                                Payment_Batch = TDTaxReceipts[6].Text;
                                Payment_Type = TDTaxReceipts[7].Text;
                                Payment_Amount = TDTaxReceipts[8].Text;

                                Tax_Receipts = Payment_Date + "~" + Effective_PaymentDate + "~" + BillNumber + "~" + Year + "~" + TaxBill_Type + "~" + Installment + "~" + Payment_Batch + "~" + Payment_Type + "~" + Payment_Amount;
                                gc.insert_date(orderNumber, outparcelno, 244, Tax_Receipts, 1, DateTime.Now);
                                i++;
                            }
                        }
                    }
                    catch
                    { }
                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "CA", "Santa Barbara", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    //megrge pdf files
                    gc.mergpdf(orderNumber, "CA", "Santa Barbara");
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