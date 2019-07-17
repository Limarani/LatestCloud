using System;
using System.Collections.Generic;
using OpenQA.Selenium.Chrome;
using System.Linq;
using System.Web;
using System.IO;
using System.Drawing.Imaging;
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
    public class WebDriver_ORClackamas
    {
        string outputPath = "";
        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        MySqlParameter[] mParam;
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());

        public string FTP_ORClackamas(string houseno, string sname, string Dir, string account, string parcelNumber, string searchType, string orderNumber, string ownername, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            string outparcelno = "", address = "", ChkMultiParcel = "", LocationAddress = "", MultiParcelData = "", Authority_Address = "";
            string Property_Address = "", Alternate_Property = "", Property_Description = "", Property_Category = "", Status = "", TaxCode_Area = "", Neighborhood = "", Year_Built = "", Property_Details = "", Assemnt_Details1 = "", Assemnt_Details2 = "";
            string Receipt = "-", Date_Time = "-", Tax_Year = "-", TCA_District = "-", Amount_Applied = "-", Desription = "-", Name = "-", Tender_Type = "-", Receipt_Details = "-", stryearBuilt = "-";
            string authority_Address = "", authority_Mail = "", authority_Details = "";
            string Taxyear = "-", Installment = "-", EarliestDueDate = "-", Principal = "-", InterestCosts = "-", Totaldue = "-", Cummulativedue = "-", TaxBalnce_details = "-";
            string TaxProperty_Address = "-", Taxdetail_Year = "-", Category = "-", TCA_Dist = "-", Charged = "-", Minimum = "-", Balnce_Due = "-", Due_Date = "-", TAXAssement_details, authority_Phone = "";

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

                        gc.TitleFlexSearch(orderNumber, parcelNumber, "", address, "OR", "Clackamas");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_ORClackmas"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }
                    if (searchType == "address")
                    {
                        driver.Navigate().GoToUrl("http://ascendweb.clackamas.us/ascendweb/(S(cisqdyhth2sdnfwpxlheqkqs))/default.aspx");
                        Thread.Sleep(2000);
                        address = houseno + " " + sname;
                        driver.FindElement(By.Id("mStreetAddress")).SendKeys(address);
                        //Screen-Shot
                        gc.CreatePdf_WOP(orderNumber, "AddressSearch", driver, "OR", "Clackamas");
                        driver.FindElement(By.Id("mSubmit")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);

                        try
                        {
                            string norecord = driver.FindElement(By.XPath("//*[@id='mMessage']")).Text;
                            if (norecord.Contains("0 records"))
                            {
                                driver.Navigate().GoToUrl("http://ascendweb.clackamas.us/ascendweb/(S(cisqdyhth2sdnfwpxlheqkqs))/default.aspx");
                                Thread.Sleep(2000);
                                address = houseno + " " + Dir + " " + sname;
                                driver.FindElement(By.Id("mStreetAddress")).SendKeys(address);
                                //Screen-Shot
                                gc.CreatePdf_WOP(orderNumber, "AddressSearch", driver, "OR", "Clackamas");
                                driver.FindElement(By.Id("mSubmit")).SendKeys(Keys.Enter);
                                Thread.Sleep(2000);
                            }

                        }
                        catch
                        { }
                        try
                        {
                            //MultiParcel

                            ChkMultiParcel = driver.FindElement(By.XPath("//*[@id='Table2']/tbody/tr/td[2]")).Text;

                            if (ChkMultiParcel == "1 records returned from your search input.")
                            {
                                driver.FindElement(By.XPath("/html/body/form/table/tbody/tr[2]/td[2]/table/tbody/tr/td/table/tbody/tr[3]/td/div/table/tbody/tr[2]/td[1]/a")).Click();
                                Thread.Sleep(3000);
                                //Screen-Shot                        
                                gc.CreatePdf_WOP(orderNumber, "Single Address Search", driver, "OR", "Clackamas");
                            }

                            else
                            {
                                IWebElement MultiParcelTable = driver.FindElement(By.XPath("/html/body/form/table/tbody/tr[2]/td[2]/table/tbody/tr/td/table/tbody/tr[3]/td/div/table/tbody"));
                                IList<IWebElement> MultiParcelTR = MultiParcelTable.FindElements(By.TagName("tr"));
                                IList<IWebElement> MultiParcelTD;

                                int maxCheck = 0;

                                foreach (IWebElement multi in MultiParcelTR)
                                {
                                    if (maxCheck <= 10)
                                    {
                                        MultiParcelTD = multi.FindElements(By.TagName("td"));
                                        if (MultiParcelTD.Count != 0)
                                        {
                                            parcelNumber = MultiParcelTD[0].Text;
                                            LocationAddress = MultiParcelTD[1].Text;
                                            MultiParcelData = LocationAddress;
                                            gc.insert_date(orderNumber, parcelNumber, 151, MultiParcelData, 1, DateTime.Now);
                                        }
                                        maxCheck++;
                                    }
                                }
                                HttpContext.Current.Session["multiparcel_ORClackamas"] = "Yes";
                                if (MultiParcelTR.Count > 10)
                                {
                                    HttpContext.Current.Session["multiParcel_ORClackamas_count"] = "Maximum";
                                }
                                driver.Quit();
                                return "MultiParcel";
                            }
                        }
                        catch { }
                        try
                        {
                            IWebElement INodata = driver.FindElement(By.Id("Table2"));
                            if (INodata.Text.Contains("0 records returned"))
                            {
                                HttpContext.Current.Session["Nodata_ORClackmas"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }

                    else if (searchType == "parcel")
                    {
                        driver.Navigate().GoToUrl("http://ascendweb.clackamas.us/ascendweb/(S(cisqdyhth2sdnfwpxlheqkqs))/default.aspx");
                        Thread.Sleep(2000);
                        driver.FindElement(By.Id("mParcelID2")).SendKeys(parcelNumber);
                        //Screen-Shot
                        gc.CreatePdf(orderNumber, parcelNumber, "ParcelSearch", driver, "OR", "Clackamas");
                        driver.FindElement(By.Id("mSubmit")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        try
                        {
                            IWebElement INodata = driver.FindElement(By.Id("Table1"));
                            if(INodata.Text.Contains("does not exist"))
                            {
                                HttpContext.Current.Session["Nodata_ORClackmas"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }
                    else if (searchType == "block")
                    {

                        driver.Navigate().GoToUrl("http://ascendweb.clackamas.us/ascendweb/(S(cisqdyhth2sdnfwpxlheqkqs))/default.aspx");
                        Thread.Sleep(2000);
                        driver.FindElement(By.Id("mAlternateParcelID")).SendKeys(account);
                        //Screen-Shot
                        gc.CreatePdf(orderNumber, account, "AccountSearch", driver, "OR", "Clackamas");
                        driver.FindElement(By.Id("mSubmit")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        try
                        {
                            IWebElement INodata = driver.FindElement(By.Id("Table1"));
                            if (INodata.Text.Contains("does not exist"))
                            {
                                HttpContext.Current.Session["Nodata_ORClackmas"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }
                    //Scrapped Data 

                    //Property Deatails
                    outparcelno = driver.FindElement(By.XPath("/html/body/form/table[1]/tbody/tr[2]/td/table[1]/tbody/tr[4]/td/table[1]/tbody/tr/td/table/tbody/tr/td[2]")).Text;
                    Property_Address = driver.FindElement(By.XPath("/html/body/form/table[1]/tbody/tr[2]/td/table[1]/tbody/tr[4]/td/table[1]/tbody/tr/td/table/tbody/tr/td[4]")).Text;
                    Alternate_Property = driver.FindElement(By.XPath("/html/body/form/table[1]/tbody/tr[2]/td/table[1]/tbody/tr[4]/td/table[2]/tbody/tr[2]/td/div/table/tbody/tr[1]/td[2]")).Text;
                    Property_Description = driver.FindElement(By.XPath("/html/body/form/table[1]/tbody/tr[2]/td/table[1]/tbody/tr[4]/td/table[2]/tbody/tr[2]/td/div/table/tbody/tr[2]/td[2]")).Text;
                    Property_Category = driver.FindElement(By.XPath("/html/body/form/table[1]/tbody/tr[2]/td/table[1]/tbody/tr[4]/td/table[2]/tbody/tr[2]/td/div/table/tbody/tr[3]/td[2]")).Text;
                    Status = driver.FindElement(By.XPath("/html/body/form/table[1]/tbody/tr[2]/td/table[1]/tbody/tr[4]/td/table[2]/tbody/tr[2]/td/div/table/tbody/tr[4]/td[2]")).Text;
                    TaxCode_Area = driver.FindElement(By.XPath("/html/body/form/table[1]/tbody/tr[2]/td/table[1]/tbody/tr[4]/td/table[2]/tbody/tr[2]/td/div/table/tbody/tr[5]/td[2]")).Text;
                    Neighborhood = driver.FindElement(By.XPath("/html/body/form/table[1]/tbody/tr[2]/td/table[1]/tbody/tr[4]/td/table[2]/tbody/tr[4]/td/div/table/tbody/tr[1]/td[2]")).Text;
                    try
                    {
                        IWebElement Properttable = driver.FindElement(By.XPath("/html/body/form/table[1]/tbody/tr[2]/td/table[1]/tbody/tr[4]/td/table[2]/tbody/tr[4]/td/div/table/tbody"));
                        IList<IWebElement> PropertTR = Properttable.FindElements(By.TagName("tr"));
                        IList<IWebElement> PropertTD;

                        foreach (IWebElement row2 in PropertTR)
                        {
                            PropertTD = row2.FindElements(By.TagName("td"));
                            if (PropertTD.Count != 0 && !row2.Text.Contains("Change property ratio") && row2.Text.Contains("Year Built"))
                            {
                                Year_Built = PropertTD[1].Text;
                            }
                        }
                    }
                    catch
                    { }
                    //Screen-Shot
                    gc.CreatePdf(orderNumber, outparcelno, "Property_Search", driver, "OR", "Clackamas");

                    Property_Details = Property_Address + "~" + Alternate_Property + "~" + Property_Description + "~" + Property_Category + "~" + Status + "~" + TaxCode_Area + "~" + Neighborhood + "~" + Year_Built;
                    gc.insert_date(orderNumber, outparcelno, 152, Property_Details, 1, DateTime.Now);


                    //Assessment Details
                    IWebElement valuetableElement = driver.FindElement(By.XPath("/html/body/form/table[1]/tbody/tr[2]/td/table[1]/tbody/tr[4]/td/div[4]/table/tbody/tr[2]/td/div/table/tbody"));
                    IList<IWebElement> valuetableRow = valuetableElement.FindElements(By.TagName("tr"));
                    IList<IWebElement> valuerowTD;
                    IList<IWebElement> valuerowTH;

                    List<string> Value_Type = new List<string>();
                    List<string> AVR_Total = new List<string>();
                    List<string> Exempt = new List<string>();
                    List<string> TVR_Total = new List<string>();
                    List<string> Real_Mkt_Land = new List<string>();
                    List<string> Real_Mkt_Bldg = new List<string>();
                    List<string> Real_Mkt_Total = new List<string>();
                    List<string> M5_Mkt_Land = new List<string>();
                    List<string> M5_Mkt_Bldg = new List<string>();
                    List<string> M5_SAV = new List<string>();
                    List<string> SAVL = new List<string>();
                    List<string> MAV = new List<string>();
                    List<string> Mkt_Exception = new List<string>();
                    List<string> AV_Exception = new List<string>();

                    int i = 0;
                    foreach (IWebElement row in valuetableRow)
                    {

                        valuerowTD = row.FindElements(By.TagName("td"));

                        if (i == 0)
                        {
                            valuerowTH = row.FindElements(By.TagName("th"));
                            Value_Type.Add(valuerowTH[1].Text.Trim().Replace("\r\n", ""));
                            Value_Type.Add(valuerowTH[2].Text.Trim().Replace("\r\n", ""));
                        }
                        else if (i == 1)
                        {
                            AVR_Total.Add(valuerowTD[1].Text);
                            AVR_Total.Add(valuerowTD[2].Text);
                        }
                        else if (i == 2)
                        {
                            Exempt.Add(valuerowTD[1].Text);
                            Exempt.Add(valuerowTD[2].Text);
                        }
                        else if (i == 3)
                        {
                            TVR_Total.Add(valuerowTD[1].Text);
                            TVR_Total.Add(valuerowTD[2].Text);
                        }
                        else if (i == 4)
                        {
                            Real_Mkt_Land.Add(valuerowTD[1].Text);
                            Real_Mkt_Land.Add(valuerowTD[2].Text);
                        }
                        else if (i == 5)
                        {
                            Real_Mkt_Bldg.Add(valuerowTD[1].Text);
                            Real_Mkt_Bldg.Add(valuerowTD[2].Text);
                        }
                        else if (i == 6)
                        {
                            Real_Mkt_Total.Add(valuerowTD[1].Text);
                            Real_Mkt_Total.Add(valuerowTD[2].Text);
                        }
                        else if (i == 7)
                        {
                            M5_Mkt_Land.Add(valuerowTD[1].Text);
                            M5_Mkt_Land.Add(valuerowTD[2].Text);
                        }
                        else if (i == 8)
                        {
                            M5_Mkt_Bldg.Add(valuerowTD[1].Text);
                            M5_Mkt_Bldg.Add(valuerowTD[2].Text);
                        }
                        else if (i == 9)
                        {
                            M5_SAV.Add(valuerowTD[1].Text);
                            M5_SAV.Add(valuerowTD[2].Text);
                        }
                        else if (i == 10)
                        {
                            SAVL.Add(valuerowTD[1].Text);
                            SAVL.Add(valuerowTD[2].Text);
                        }
                        else if (i == 11)
                        {
                            MAV.Add(valuerowTD[1].Text);
                            MAV.Add(valuerowTD[2].Text);
                        }
                        else if (i == 12)
                        {
                            Mkt_Exception.Add(valuerowTD[1].Text);
                            Mkt_Exception.Add(valuerowTD[2].Text);
                        }
                        else if (i == 13)
                        {
                            AV_Exception.Add(valuerowTD[1].Text);
                            AV_Exception.Add(valuerowTD[2].Text);
                        }

                        i++;
                    }
                    Assemnt_Details1 = Value_Type[0] + "~" + AVR_Total[0] + "~" + Exempt[0] + "~" + TVR_Total[0] + "~" + Real_Mkt_Land[0] + "~" + Real_Mkt_Bldg[0] + "~" + Real_Mkt_Total[0] + "~" + M5_Mkt_Land[0] + "~" + M5_Mkt_Bldg[0] + "~" + M5_SAV[0] + "~" + SAVL[0] + "~" + MAV[0] + "~" + Mkt_Exception[0] + "~" + AV_Exception[0];
                    Assemnt_Details2 = Value_Type[1] + "~" + AVR_Total[1] + "~" + Exempt[1] + "~" + TVR_Total[1] + "~" + Real_Mkt_Land[1] + "~" + Real_Mkt_Bldg[1] + "~" + Real_Mkt_Total[1] + "~" + M5_Mkt_Land[1] + "~" + M5_Mkt_Bldg[1] + "~" + M5_SAV[1] + "~" + SAVL[1] + "~" + MAV[1] + "~" + Mkt_Exception[1] + "~" + AV_Exception[1];
                    gc.insert_date(orderNumber, outparcelno, 153, Assemnt_Details1, 1, DateTime.Now);
                    gc.insert_date(orderNumber, outparcelno, 153, Assemnt_Details2, 1, DateTime.Now);
                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");

                    //Tax Balnce Details
                    try
                    {
                        IWebElement TBBalnce = driver.FindElement(By.XPath("/html/body/form/table[1]/tbody/tr[2]/td/table[2]/tbody/tr/td/table/tbody/tr[5]/td/div/table/tbody"));
                        IList<IWebElement> TRBalnce = TBBalnce.FindElements(By.TagName("tr"));
                        IList<IWebElement> TDBalnce;

                        foreach (IWebElement assess in TRBalnce)
                        {
                            TDBalnce = assess.FindElements(By.TagName("td"));
                            if (TDBalnce.Count != 0)
                            {
                                Taxyear = TDBalnce[0].Text;
                                Installment = TDBalnce[1].Text;
                                EarliestDueDate = TDBalnce[2].Text;
                                Principal = TDBalnce[3].Text;
                                InterestCosts = TDBalnce[4].Text;
                                Totaldue = TDBalnce[5].Text;
                                Cummulativedue = TDBalnce[6].Text;
                                TaxBalnce_details = Taxyear + "~" + Installment + "~" + EarliestDueDate + "~" + Principal + "~" + InterestCosts + "~" + Totaldue + "~" + Cummulativedue;
                                gc.insert_date(orderNumber, outparcelno, 179, TaxBalnce_details, 1, DateTime.Now);
                            }
                        }


                        //Tax Detailed Statement
                        driver.FindElement(By.XPath("//*[@id='mDetailedStatement']")).Click();
                        Thread.Sleep(2000);

                        TaxProperty_Address = driver.FindElement(By.XPath("/html/body/form/table/tbody/tr[2]/td[2]/table/tbody/tr[2]/td/table/tbody/tr/td/table/tbody/tr[1]/td/table[1]/tbody/tr/td[4]")).Text;

                        IWebElement TBDetails = driver.FindElement(By.XPath("/html/body/form/table/tbody/tr[2]/td[2]/table/tbody/tr[2]/td/table/tbody/tr/td/table/tbody/tr[2]/td/table/tbody/tr/td/div/table/tbody"));
                        IList<IWebElement> TRDetails = TBDetails.FindElements(By.TagName("tr"));
                        IList<IWebElement> TDDetails;

                        int k = 0;
                        foreach (IWebElement Details in TRDetails)
                        {
                            TDDetails = Details.FindElements(By.TagName("td"));
                            if (k != 0)
                            {
                                if (TDDetails.Count != 0)
                                {
                                    Taxdetail_Year = TDDetails[0].Text;
                                    Category = TDDetails[1].Text;
                                    TCA_Dist = TDDetails[2].Text;
                                    Charged = TDDetails[3].Text;
                                    Minimum = TDDetails[4].Text;
                                    Balnce_Due = TDDetails[5].Text;
                                    Due_Date = TDDetails[6].Text;
                                    TAXAssement_details = Taxdetail_Year + "~" + Category + "~" + TCA_Dist + "~" + Charged + "~" + Minimum + "~" + Balnce_Due + "~" + Due_Date;
                                    gc.insert_date(orderNumber, outparcelno, 199, TAXAssement_details, 1, DateTime.Now);
                                }
                            }
                            k++;
                        }

                        driver.Navigate().Back();
                        Thread.Sleep(2000);

                    }
                    catch
                    { }
                    //Tax Payment Details
                    IWebElement Receipttable = driver.FindElement(By.XPath("/html/body/form/div[5]/table/tbody/tr[2]/td/div/table/tbody"));
                    IList<IWebElement> ReceipttableRow = Receipttable.FindElements(By.TagName("tr"));
                    int rowcount = ReceipttableRow.Count;

                    for (int j = 2; j <= rowcount; j++)
                    {
                        try
                        {
                            driver.FindElement(By.XPath("/html/body/form/div[5]/table/tbody/tr[2]/td/div/table/tbody/tr[" + j + "]/td[2]/a")).Click();
                            Thread.Sleep(3000); IWebElement TaxReTB = driver.FindElement(By.XPath("/html/body/form/table/tbody/tr[2]/td[2]/table/tbody/tr[1]/td/table/tbody"));
                            IList<IWebElement> TaxReTR = TaxReTB.FindElements(By.TagName("tr"));
                            IList<IWebElement> TaxReTD;
                            foreach (IWebElement Receis in TaxReTR)
                            {
                                TaxReTD = Receis.FindElements(By.TagName("td"));
                                {
                                    if (TaxReTD.Count != 0)
                                    {
                                        Receipt = TaxReTD[1].Text;
                                        Date_Time = TaxReTD[3].Text;
                                        Receipt_Details = Receipt + "~" + Date_Time + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "";
                                        gc.insert_date(orderNumber, outparcelno, 161, Receipt_Details, 1, DateTime.Now);
                                    }
                                }
                            }

                            IWebElement TaxReceiptTB = driver.FindElement(By.XPath("/html/body/form/table/tbody/tr[2]/td[2]/table/tbody/tr[3]/td/table/tbody/tr[2]/td/div/table/tbody"));
                            IList<IWebElement> TaxReceiptTR = TaxReceiptTB.FindElements(By.TagName("tr"));
                            IList<IWebElement> TaxReceiptTD;

                            foreach (IWebElement Receipts in TaxReceiptTR)
                            {
                                TaxReceiptTD = Receipts.FindElements(By.TagName("td"));
                                {
                                    if (TaxReceiptTD.Count != 0)
                                    {
                                        Tax_Year = TaxReceiptTD[1].Text;
                                        TCA_District = TaxReceiptTD[2].Text;
                                        Amount_Applied = TaxReceiptTD[3].Text;
                                        Desription = TaxReceiptTD[4].Text;
                                        Receipt_Details = "" + "~" + "" + "~" + Tax_Year + "~" + TCA_District + "~" + Amount_Applied + "~" + Desription + "~" + "" + "~" + "" + "~" + "";
                                        gc.insert_date(orderNumber, outparcelno, 161, Receipt_Details, 1, DateTime.Now);
                                    }
                                }
                            }

                            //Payer Details

                            IWebElement TaxTB = driver.FindElement(By.XPath("/html/body/form/table/tbody/tr[2]/td[2]/table/tbody/tr[5]/td/table/tbody/tr[2]/td/div/table/tbody"));
                            IList<IWebElement> TaxTR = TaxTB.FindElements(By.TagName("tr"));
                            IList<IWebElement> TaxTD;

                            foreach (IWebElement Res in TaxTR)
                            {
                                TaxTD = Res.FindElements(By.TagName("td"));
                                {
                                    if (TaxTD.Count != 0 && !Res.Text.Contains("Name"))
                                    {
                                        Name = TaxTD[0].Text;
                                        Tender_Type = TaxTD[1].Text;
                                        string TotalAmount_Applied = TaxTD[2].Text;
                                        Receipt_Details = "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + Name + "~" + Tender_Type + "~" + TotalAmount_Applied;
                                        gc.insert_date(orderNumber, outparcelno, 161, Receipt_Details, 1, DateTime.Now);
                                    }
                                }
                            }

                            driver.Navigate().GoToUrl("http://ascendweb.clackamas.us/ascendweb/(S(rbsmpxkc0vynv1vyuqc13ri4))/ParcelInfo.aspx");
                            Thread.Sleep(3000);
                        }
                        catch { }
                    }

                    try
                    {
                        driver.FindElement(By.XPath("//*[@id='mReceipts']/tbody/tr[2]/td[2]/a")).Click();
                        Thread.Sleep(3000);
                        gc.CreatePdf(orderNumber, outparcelno, "Tax Payment Details", driver, "OR", "Clackamas");
                        driver.Navigate().GoToUrl("http://ascendweb.clackamas.us/ascendweb/(S(rbsmpxkc0vynv1vyuqc13ri4))/ParcelInfo.aspx");
                        Thread.Sleep(3000);
                    }
                    catch
                    { }

                    //Tax information
                    driver.Navigate().GoToUrl("https://web3.clackamas.us/taxstatements/");
                    driver.FindElement(By.Id("pn")).SendKeys(outparcelno);
                    driver.FindElement(By.XPath("/html/body/div[3]/form/fieldset/table/tbody/tr[4]/td[2]/input[5]")).Click();
                    Thread.Sleep(3000);
                    //Screen-Shot
                    gc.CreatePdf(orderNumber, outparcelno, "City Tax Details", driver, "OR", "Clackamas");

                    //Download the pdf for Tax Details 
                    String Parent_Window = driver.CurrentWindowHandle;

                    IJavaScriptExecutor js1 = driver as IJavaScriptExecutor;

                    IWebElement ButtonLinkSearch = driver.FindElement(By.XPath("//*[@id='st1']/a"));
                    string script = ButtonLinkSearch.GetAttribute("href");
                    driver.ExecuteJavaScript<object>(script);
                    IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                    var newScrollHeight = (long)js.ExecuteScript("window.scrollTo(0, document.body.scrollHeight); return document.body.scrollHeight;");
                    Thread.Sleep(2000);
                    //gc.downloadfileHeader(script, orderNumber, outparcelno, "Clacmas_Tax", "OR", "Clackamas", driver);

                    System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    driver.SwitchTo().Window(driver.WindowHandles.Last());
                    Thread.Sleep(2000);
                    gc.CreatePdf_Chrome(orderNumber, outparcelno, "Clacmas_Tax.pdf", driver, "OR", "Clackamas");

                    driver.SwitchTo().Window(Parent_Window);
                    Thread.Sleep(2000);

                    //Tax Authority
                    try
                    { 
                    driver.Navigate().GoToUrl("https://clackamas.us/at");
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, outparcelno, "Authority Search", driver, "OR", "Clackamas");
                    authority_Address = driver.FindElement(By.XPath("//*[@id='block-views-block-ccts-locations-ccts-locations-view']/div/div/div/div/div/span/div[2]/p[2]/a")).Text.Replace("\r\n", " ");
                    authority_Phone = driver.FindElement(By.XPath("//*[@id='block-views-block-ccts-locations-ccts-locations-view']/div/div/div/div/div/span/div[2]/p[1]/a[1]")).Text;
                    Authority_Address = authority_Address + " " + authority_Phone;
                    authority_Mail = driver.FindElement(By.XPath("//*[@id='block-views-block-ccts-locations-ccts-locations-view']/div/div/div/div/div/span/div[2]/p[1]/a[2]")).Text;
                    authority_Details = Authority_Address + "~" + authority_Mail;
                    gc.insert_date(orderNumber, outparcelno, 163, authority_Details, 1, DateTime.Now);
                    }
                    catch
                    {

                    }
                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "OR", "Clackamas", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    //megrge pdf files
                    gc.mergpdf(orderNumber, "OR", "Clackamas");
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