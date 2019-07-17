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
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;

namespace ScrapMaricopa.Scrapsource
{
    public class WebDriver_DonaAnaNM
    {
        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        MySqlParameter[] mParam;
        public string FTP_DonaAnaNM(string houseno, string Direction, string sname, string stype, string account, string parcelNumber, string ownername, string searchType, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            GlobalClass.sname = "NM";
            GlobalClass.cname = "Dona Ana";
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
            string multi = "";
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            using (driver = new PhantomJSDriver())
            {
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");
                    driver.Navigate().GoToUrl("http://assessor.donaanacounty.org/assessor/taxweb/search.jsp");
                    driver.FindElement(By.XPath("//*[@id='middle_left']/form/input[1]")).SendKeys(Keys.Enter);
                    if (searchType == "address")
                    {
                        driver.FindElement(By.Id("SitusIDHouseNumber")).SendKeys(houseno);
                        driver.FindElement(By.Id("SitusIDStreetName")).SendKeys(sname);
                        try
                        {
                            IWebElement IDirection = driver.FindElement(By.Id("SitusIDDirectionSuffix"));
                            SelectElement SDirection = new SelectElement(IDirection);
                            SDirection.SelectByText(Direction);
                        }
                        catch { }
                        try
                        {
                            IWebElement IStreetType = driver.FindElement(By.Id("SitusIDDesignation"));
                            SelectElement SStreetType = new SelectElement(IStreetType);
                            SStreetType.SelectByValue(stype.ToUpper());
                        }
                        catch { }
                        try
                        {
                            string[] unit = new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "S", "T", "U", "V", "W", "X", "Y", "Z" };
                            for (int i = 0; i < 10; i++)
                            {
                                if (account.Contains(Convert.ToString(i)))
                                {
                                    driver.FindElement(By.Id("SitusIDUnitNumber")).SendKeys("UNIT " + account);
                                }
                            }
                            for (int j = 0; j < unit.Length; j++)
                            {
                                if (account.Contains(unit[j]))
                                {
                                    driver.FindElement(By.Id("SitusIDUnitNumber")).SendKeys(account);
                                }
                            }
                        }
                        catch { }
                        gc.CreatePdf_WOP(orderNumber, "Address Search", driver, "NM", "Dona Ana");
                        driver.FindElement(By.XPath("//*[@id='middle']/form/font/font/table/tbody/tr/td[1]/input")).SendKeys(Keys.Enter);

                        try
                        {
                            string straccount = "", strparcel = "", strname = "", straddress = "";
                            string strmulti = driver.FindElement(By.XPath("//*[@id='middle']/p[3]")).Text;
                            multi = gc.Between(strmulti, "Showing ", " results").Trim();
                            if (Convert.ToInt32(multi) > 25)
                            {
                                HttpContext.Current.Session["multiParcel_DonaAna_Maximum"] = "Maximum";
                                driver.Quit();
                                return "Maximum";
                            }
                            string strmultip = "";
                            List<string> checksingle = new List<string>();
                            if (Convert.ToInt32(multi) < 25 && Convert.ToInt32(multi) != 0)
                            {
                                gc.CreatePdf_WOP(orderNumber, "Multi Name Search", driver, "NM", "Dona Ana");
                                IWebElement IMulti = driver.FindElement(By.XPath("//*[@id='searchResultsTable']/tbody"));
                                IList<IWebElement> IMultiRow = IMulti.FindElements(By.TagName("tr"));
                                IList<IWebElement> IMultiTD;
                                IList<IWebElement> IMultiSummary;
                                IList<IWebElement> IMultiSummaryTD;
                                foreach (IWebElement multiparcel in IMultiRow)
                                {
                                    IMultiTD = multiparcel.FindElements(By.TagName("td"));
                                    IMultiSummary = multiparcel.FindElements(By.TagName("table"));
                                    if (IMultiTD.Count != 0 && IMultiTD.Count > 5)
                                    {
                                        straccount = IMultiTD[0].Text;
                                        if ((straccount.Contains("R") || straccount.Contains("M")) && !straccount.Contains("P"))
                                        {//*[@id="searchResultsTable"]/tbody/tr[2]/td[1]
                                            foreach (IWebElement summary in IMultiSummary)
                                            {
                                                IMultiSummaryTD = summary.FindElements(By.TagName("td"));
                                                if (IMultiSummaryTD.Count != 0)
                                                {
                                                    IWebElement multiparcellink = IMultiTD[0].FindElement(By.TagName("a"));
                                                    strmultip = multiparcellink.GetAttribute("href");
                                                    strparcel = GlobalClass.Before(IMultiSummaryTD[0].Text, "\r\n");
                                                    strname = IMultiSummaryTD[1].Text;
                                                    straddress = IMultiSummaryTD[2].Text.Replace("\r\n", "");
                                                    checksingle.Add(strmultip);
                                                }
                                            }

                                            string multiDetails = straccount + "~" + strname + "~" + straddress;
                                            gc.insert_date(orderNumber, strparcel, 670, multiDetails, 1, DateTime.Now);
                                        }
                                    }
                                }

                                if (checksingle.Count == 1)
                                {
                                    driver.Navigate().GoToUrl(strmultip);
                                }

                                else
                                {
                                    HttpContext.Current.Session["multiParcel_DonaAna"] = "Yes";
                                    driver.Quit();
                                    return "MultiParcel";
                                }
                            }
                        }
                        catch { }
                    }

                    if (searchType == "parcel")
                    {
                        driver.FindElement(By.Id("ParcelNumberID")).SendKeys(parcelNumber.Replace("-", ""));
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search", driver, "NM", "Dona Ana");
                        driver.FindElement(By.XPath("//*[@id='middle']/form/font/font/table/tbody/tr/td[1]/input")).SendKeys(Keys.Enter);
                    }
                    if (searchType == "ownername")
                    {
                        driver.FindElement(By.Id("OwnerIDSearchString")).SendKeys(ownername);
                        gc.CreatePdf_WOP(orderNumber, "Owner Name Search", driver, "NM", "Dona Ana");
                        driver.FindElement(By.XPath("//*[@id='middle']/form/font/font/table/tbody/tr/td[1]/input")).SendKeys(Keys.Enter);

                        try
                        {
                            string straccount = "", strparcel = "", strname = "", straddress = "";
                            string strmulti = driver.FindElement(By.XPath("//*[@id='middle']/p[3]")).Text;
                            multi = gc.Between(strmulti, "Showing ", " results").Trim();
                            if (Convert.ToInt32(multi) > 25)
                            {
                                HttpContext.Current.Session["multiParcel_DonaAna_Maximum"] = "Maximum";
                                driver.Quit();
                                return "Maximum";
                            }
                            if (Convert.ToInt32(multi) < 25 && Convert.ToInt32(multi) != 0)
                            {
                                gc.CreatePdf_WOP(orderNumber, "Multi Name Search", driver, "NM", "Dona Ana");
                                IWebElement IMulti = driver.FindElement(By.XPath("//*[@id='searchResultsTable']/tbody"));
                                IList<IWebElement> IMultiRow = IMulti.FindElements(By.TagName("tr"));
                                IList<IWebElement> IMultiTD;
                                IList<IWebElement> IMultiSummary;
                                IList<IWebElement> IMultiSummaryTD;
                                foreach (IWebElement multiparcel in IMultiRow)
                                {
                                    IMultiTD = multiparcel.FindElements(By.TagName("td"));
                                    IMultiSummary = multiparcel.FindElements(By.TagName("table"));
                                    if (IMultiTD.Count != 0 && IMultiTD.Count > 5)
                                    {
                                        straccount = IMultiTD[0].Text;
                                        if ((straccount.Contains("R") || straccount.Contains("M")) && !straccount.Contains("P"))
                                        {
                                            foreach (IWebElement summary in IMultiSummary)
                                            {
                                                IMultiSummaryTD = summary.FindElements(By.TagName("td"));
                                                if (IMultiSummaryTD.Count != 0)
                                                {
                                                    if (IMultiSummaryTD[0].Text.Contains("\r\n"))
                                                    {
                                                        strparcel = GlobalClass.Before(IMultiSummaryTD[0].Text, "\r\n");
                                                    }
                                                    else
                                                    {
                                                        strparcel = IMultiSummaryTD[0].Text;
                                                    }
                                                    strname = IMultiSummaryTD[1].Text;
                                                    straddress = IMultiSummaryTD[2].Text.Replace("\r\n", "");
                                                }
                                            }

                                            string multiDetails = straccount + "~" + strname + "~" + straddress;
                                            gc.insert_date(orderNumber, strparcel, 670, multiDetails, 1, DateTime.Now);
                                        }
                                    }
                                }
                                HttpContext.Current.Session["multiParcel_DonaAna"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                        }
                        catch { }
                    }
                    if (searchType == "account")
                    {
                        if ((account.Contains("R") || account.Contains("M")) && !account.Contains("P"))
                        {
                            driver.FindElement(By.Id("AccountNumID")).SendKeys(account);
                            gc.CreatePdf_WOP(orderNumber, "Account Search", driver, "NM", "Dona Ana");
                            driver.FindElement(By.XPath("//*[@id='middle']/form/font/font/table/tbody/tr/td[1]/input")).SendKeys(Keys.Enter);
                        }
                    }

                    try
                    {
                        IWebElement Inodata = driver.FindElement(By.Id("middle"));
                        if(Inodata.Text.Contains("No results found"))
                        {
                            HttpContext.Current.Session["Nodata_DonaAnaNM"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }
                    //Property Details
                    string AccountNo = "", SitusAddress = "", TaxArea = "", LegalSummary = "", strOwnerName = "", strAddress = "", ValueSummary = "", MValueSummary = "", PropertyStatus = "";
                    try
                    {
                        driver.FindElement(By.XPath("//*[@id='searchResultsTable']/tbody/tr[2]/td[1]/a")).SendKeys(Keys.Enter);
                    }
                    catch
                    {

                    }
                    AccountNo = GlobalClass.After(driver.FindElement(By.XPath("//*[@id='middle']/h1[1]")).Text, "Account: ").Trim();
                    IWebElement Iaddress = driver.FindElement(By.XPath("//*[@id='middle']/table/tbody/tr[2]/td[1]/table"));
                    SitusAddress = gc.Between(Iaddress.Text, "Situs Address ", "\r\nTax Area");
                    TaxArea = gc.Between(Iaddress.Text, "Tax Area ", "\r\nParcel Number");
                    parcelNumber = gc.Between(Iaddress.Text, "Parcel Number ", "\r\nLegal Summary");
                    gc.CreatePdf(orderNumber, parcelNumber, "Property Details", driver, "NM", "Dona Ana");
                    LegalSummary = gc.Between(Iaddress.Text, "Legal Summary ", "Deed Holder");
                    IWebElement IownerAddress = driver.FindElement(By.XPath("//*[@id='middle']/table/tbody/tr[2]/td[2]/table"));
                    strOwnerName = gc.Between(IownerAddress.Text, "Owner Name ", "\r\n");
                    strAddress = GlobalClass.After(IownerAddress.Text, "Owner Address ").Replace("\r\n", " ");

                    string strHistoryHead = "", strHistoryValue = "";
                    IWebElement IAssessmentHistory = driver.FindElement(By.XPath("//*[@id='middle']/table/tbody/tr[2]/td[3]/table[1]"));
                    IList<IWebElement> IAssessmentRow = IAssessmentHistory.FindElements(By.TagName("tr"));
                    IList<IWebElement> IAssessmentTd;
                    foreach (IWebElement Assess in IAssessmentRow)
                    {
                        IAssessmentTd = Assess.FindElements(By.TagName("td"));
                        if (IAssessmentTd.Count != 0 && IAssessmentTd.Count > 1)
                        {
                            strHistoryHead += IAssessmentTd[0].Text + "~";
                            strHistoryValue += IAssessmentTd[1].Text + "~";
                        }
                        if (IAssessmentTd.Count != 0 && IAssessmentTd.Count == 1)
                        {
                            strHistoryHead += IAssessmentTd[0].Text + "~";
                            strHistoryValue += " " + "~";
                        }
                    }
                    string Year = gc.Between(IAssessmentHistory.Text, "Actual (", ")");
                    IWebElement IMillLevy = driver.FindElement(By.XPath("//*[@id='middle']/table/tbody/tr[2]/td[3]/table[2]/caption"));
                    string strMillLevy = GlobalClass.After(IMillLevy.Text, "Mill Levy: ");
                    db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + " Year" + "~" + strHistoryHead + "Mill Levy" + "' where Id = '" + 673 + "'");
                    gc.insert_date(orderNumber, parcelNumber, 673, Year + "~" + strHistoryValue + strMillLevy, 1, DateTime.Now);

                    //Assessmnet History Details
                    driver.FindElement(By.LinkText("Assessment History")).SendKeys(Keys.Enter);
                    gc.CreatePdf(orderNumber, parcelNumber, "Assessment History Details", driver, "NM", "Dona Ana");
                    string strType = "", firstYear = "", secondYear = "", thirdYear = "", fourthYear = "";
                    string[] strsummary = new string[] { "", "", "", "", "", "", "" };
                    IWebElement IValueSummary = driver.FindElement(By.XPath("//*[@id='middle']/div/fieldset/table"));
                    IList<IWebElement> IValueSummaryRow = IValueSummary.FindElements(By.TagName("tr"));
                    IList<IWebElement> IValueSummaryTd;
                    IList<IWebElement> IValueSummaryTh;
                    foreach (IWebElement summary in IValueSummaryRow)
                    {
                        IValueSummaryTd = summary.FindElements(By.TagName("td"));
                        IValueSummaryTh = summary.FindElements(By.TagName("th"));
                        if (IValueSummaryTh.Count != 0 && summary.Text.Contains("Type"))
                        {
                            for (int i = 0; i < IValueSummaryTh.Count; i++)
                            {
                                strsummary[i] = IValueSummaryTh[i].Text;
                            }
                        }
                        if (IValueSummaryTd.Count != 0)
                        {
                            try
                            {
                                strType += IValueSummaryTd[0].Text + "~";
                                firstYear += IValueSummaryTd[1].Text + "~";
                                secondYear += IValueSummaryTd[2].Text + "~";
                                thirdYear += IValueSummaryTd[3].Text + "~";
                                fourthYear += IValueSummaryTd[4].Text + "~";
                            }
                            catch { }
                        }
                    }
                    try
                    {
                        db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + "Year" + "~" + strType.Remove(strType.Length - 1, 1) + "' where Id = '" + 675 + "'");
                        gc.insert_date(orderNumber, parcelNumber, 675, strsummary[1] + "~" + firstYear.Remove(firstYear.Length - 1, 1), 1, DateTime.Now);
                        gc.insert_date(orderNumber, parcelNumber, 675, strsummary[2] + "~" + secondYear.Remove(secondYear.Length - 1, 1), 1, DateTime.Now);
                        gc.insert_date(orderNumber, parcelNumber, 675, strsummary[3] + "~" + thirdYear.Remove(thirdYear.Length - 1, 1), 1, DateTime.Now);
                        gc.insert_date(orderNumber, parcelNumber, 675, strsummary[4] + "~" + fourthYear.Remove(fourthYear.Length - 1, 1), 1, DateTime.Now);
                    }
                    catch { }
                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");
                    try
                    {
                        //Residential Details
                        driver.FindElement(By.LinkText("Residential")).SendKeys(Keys.Enter);
                        gc.CreatePdf(orderNumber, parcelNumber, "Residential Details", driver, "NM", "Dona Ana");
                        string PropertyCode = "", Acres = "", YearBuilt = "", EffectiveYear = "";
                        IWebElement IPropertyCode = driver.FindElement(By.Id("tabcontentcontainer"));
                        PropertyCode = gc.Between(IPropertyCode.Text, "Property Code", "Percent");
                        Acres = gc.Between(IPropertyCode.Text, "Acres", "SQFT");
                        try
                        {
                            YearBuilt = gc.Between(IPropertyCode.Text, "Actual Year Built", "Effective Year Built");
                        }
                        catch { }
                        try
                        {
                            EffectiveYear = gc.Between(IPropertyCode.Text, "Effective Year Built", "Heating Fuel");
                        }
                        catch { }

                        ValueSummary = AccountNo + "~" + SitusAddress + "~" + TaxArea + "~" + LegalSummary + "~" + strOwnerName + "~" + strAddress + "~" + PropertyCode + "~" + Acres + "~" + YearBuilt + "~" + EffectiveYear;
                        gc.insert_date(orderNumber, parcelNumber, 671, ValueSummary, 1, DateTime.Now);
                        PropertyStatus = "Residential";
                    }
                    catch { }

                    try
                    {
                        //Mobile details
                        string MPropertyCode = "", MAcres = "", MYearBuilt = "", MEffectiveYear = "";
                        driver.FindElement(By.LinkText("Mobile")).SendKeys(Keys.Enter);
                        gc.CreatePdf(orderNumber, parcelNumber, "Mobile Details", driver, "NM", "Dona Ana");
                        IWebElement IMPropertyCode = driver.FindElement(By.Id("tabcontentcontainer"));
                        MPropertyCode = gc.Between(IMPropertyCode.Text, "Property Code", "Percent");
                        MAcres = gc.Between(IMPropertyCode.Text, "Acres", "SQFT");
                        try
                        {
                            MYearBuilt = gc.Between(IMPropertyCode.Text, "Actual Year Built", "Effective Year Built");
                        }
                        catch { }
                        try
                        {
                            MEffectiveYear = gc.Between(IMPropertyCode.Text, "Effective Year Built", "Heating Fuel");
                        }
                        catch { }

                        MValueSummary = AccountNo + "~" + SitusAddress + "~" + TaxArea + "~" + LegalSummary + "~" + strOwnerName + "~" + strAddress + "~" + MPropertyCode + "~" + MAcres + "~" + MYearBuilt + "~" + MEffectiveYear;
                        gc.insert_date(orderNumber, parcelNumber, 671, MValueSummary, 1, DateTime.Now);
                        PropertyStatus = "Mobile";
                    }
                    catch { }
                    if (PropertyStatus != "Mobile" && PropertyStatus != "Residential")
                    {
                        ValueSummary = AccountNo + "~" + SitusAddress + "~" + TaxArea + "~" + LegalSummary + "~" + strOwnerName + "~" + strAddress + "~" + "" + "~" + "" + "~" + "" + "~" + "";
                        gc.insert_date(orderNumber, parcelNumber, 671, ValueSummary, 1, DateTime.Now);
                    }

                    driver.Navigate().GoToUrl("https://treasurer.donaanacounty.org/treasurer/web/");
                    gc.CreatePdf(orderNumber, parcelNumber, "TaxAuthority Details", driver, "NM", "Dona Ana");
                    string strtaxauthority = driver.FindElement(By.Id("left")).Text;
                    string TaxAuthority = GlobalClass.After(strtaxauthority, "Dona Ana County Treasurer").Replace("\r\n", " ").Trim();

                    string check = "", AreaID = "", Title = "", MillLevy = "", Actual = "", Assessed = "", Taxes = "", TActual = "", TAssessed = "";
                    string ValueTitle = "", ValueAmount = "", Title1 = "", Title2 = "";
                    int k = 0;
                    driver.Navigate().GoToUrl("https://treasurer.donaanacounty.org/treasurer/treasurerweb/search.jsp");
                    driver.FindElement(By.XPath("//*[@id='middle_left']/form/input[1]")).SendKeys(Keys.Enter);
                    driver.FindElement(By.Id("TaxAParcelID")).SendKeys(parcelNumber.Replace("-", "").Trim());
                    gc.CreatePdf(orderNumber, parcelNumber, "Tax Details Search", driver, "NM", "Dona Ana");
                    driver.FindElement(By.XPath("//*[@id='middle']/form/table[4]/tbody/tr/td[1]/input")).SendKeys(Keys.Enter);
                    gc.CreatePdf(orderNumber, parcelNumber, "Tax Details Search Result", driver, "NM", "Dona Ana");

                    //*[@id="middle"]/p[2]/span[1]
                    string resultcount = driver.FindElement(By.XPath("//*[@id='middle']/p[2]/span[1]")).Text;
                    if (resultcount.Contains("One item found"))
                    {
                        driver.FindElement(By.XPath("//*[@id='searchResultsTable']/tbody/tr/td[1]/strong/a")).SendKeys(Keys.Enter);
                    }

                    else
                    {

                        IWebElement taxbilltable = driver.FindElement(By.XPath("//*[@id='searchResultsTable']/tbody"));
                        IList<IWebElement> taxbilltableRow = taxbilltable.FindElements(By.TagName("tr"));
                        int rowcount = taxbilltableRow.Count;
                        IList<IWebElement> taxbillrowTD;
                        int w = 0;
                        foreach (IWebElement rowid in taxbilltableRow)
                        {
                            taxbillrowTD = rowid.FindElements(By.TagName("td"));
                            if (taxbillrowTD.Count != 0 && !rowid.Text.Contains("Description"))
                            {
                                if (taxbillrowTD[0].Text.Contains("R"))
                                {
                                    driver.FindElement(By.XPath("//*[@id='searchResultsTable']/tbody/tr[" + w + "]/td[1]/strong/a")).SendKeys(Keys.Enter);
                                    break;
                                }
                                w++;
                            }
                        }
                    }

                    gc.CreatePdf(orderNumber, parcelNumber, "Tax Details", driver, "NM", "Dona Ana");
                    List<string> strBill = new List<string>();
                    try
                    {
                        //Reciept Bill Download
                        IWebElement IReciept = driver.FindElement(By.Id("receiptHistory"));
                        IList<IWebElement> IRecieptBill = IReciept.FindElements(By.TagName("a"));
                        foreach (IWebElement Bill in IRecieptBill)
                        {
                            if (Bill.Text != "" && Bill.Text.Contains("Receipt"))
                            {
                                string BillLink = Bill.GetAttribute("href");
                                string billno = Bill.Text;
                                string strCurrent = driver.CurrentWindowHandle;
                                try
                                {
                                    var chromeOptions = new ChromeOptions();
                                    var chDriver = new ChromeDriver(chromeOptions);

                                    chDriver.Navigate().GoToUrl("https://treasurer.donaanacounty.org/treasurer/treasurerweb/search.jsp");
                                    chDriver.FindElement(By.XPath("//*[@id='middle_left']/form/input[1]")).SendKeys(Keys.Enter);
                                    chDriver.FindElement(By.Id("TaxAParcelID")).SendKeys(parcelNumber.Replace("-", "").Trim());
                                    chDriver.FindElement(By.XPath("//*[@id='middle']/form/table[4]/tbody/tr/td[1]/input")).SendKeys(Keys.Enter);
                                    //   chDriver.FindElement(By.XPath("//*[@id='searchResultsTable']/tbody/tr/td[1]/strong/a")).SendKeys(Keys.Enter);
                                    string resultcount1 = chDriver.FindElement(By.XPath("//*[@id='middle']/p[2]/span[1]")).Text;
                                    if (resultcount1.Contains("One item found"))
                                    {
                                        chDriver.FindElement(By.XPath("//*[@id='searchResultsTable']/tbody/tr/td[1]/strong/a")).SendKeys(Keys.Enter);
                                    }

                                    else
                                    {

                                        IWebElement taxbilltable = chDriver.FindElement(By.XPath("//*[@id='searchResultsTable']/tbody"));
                                        IList<IWebElement> taxbilltableRow = taxbilltable.FindElements(By.TagName("tr"));
                                        int rowcount = taxbilltableRow.Count;
                                        IList<IWebElement> taxbillrowTD;
                                        int w = 0;
                                        foreach (IWebElement rowid in taxbilltableRow)
                                        {
                                            taxbillrowTD = rowid.FindElements(By.TagName("td"));
                                            if (taxbillrowTD.Count != 0 && !rowid.Text.Contains("Description"))
                                            {
                                                if (taxbillrowTD[0].Text.Contains("R"))
                                                {
                                                    chDriver.FindElement(By.XPath("//*[@id='searchResultsTable']/tbody/tr[" + w + "]/td[1]/strong/a")).SendKeys(Keys.Enter);
                                                    break;
                                                }
                                                w++;
                                            }
                                        }
                                    }

                                    IWebElement IRecieptBillTable = chDriver.FindElement(By.Id("receiptHistory"));
                                    IList<IWebElement> IRecieptBillRow = IRecieptBillTable.FindElements(By.TagName("a"));
                                    foreach (IWebElement Bills in IRecieptBillRow)
                                    {
                                        if (Bills.Text != "" && Bills.Text.Contains("Receipt"))
                                        {
                                            string RBillLink = Bills.GetAttribute("href");
                                            string Rbillno = Bills.Text;
                                            if (Rbillno == billno)
                                            {
                                                Bills.SendKeys(Keys.Enter);
                                                try
                                                {
                                                    gc.downloadfile(RBillLink, orderNumber, parcelNumber, "Reciept" + k, "NM", "Dona Ana");
                                                }
                                                catch { }
                                                break;
                                            }
                                        }
                                    }
                                    try
                                    {
                                        string FilePath = gc.filePath(orderNumber, parcelNumber) + "Reciept" + k + ".pdf";
                                        PdfReader reader;
                                        string pdfData;
                                        reader = new PdfReader(FilePath);
                                        String textFromPage = PdfTextExtractor.GetTextFromPage(reader, 1);
                                        System.Diagnostics.Debug.WriteLine("" + textFromPage);

                                        string pdftext = textFromPage;
                                        string tableassess = gc.Between(pdftext, "Receipt Number", strOwnerName).Trim();
                                        string[] tableArray = tableassess.Split('\n');


                                        int count1 = tableArray.Length;
                                        for (int i = 0; i < count1; i++)
                                        {
                                            string a1 = tableArray[i];
                                            string[] strPaymentSplit = a1.Split(' ');
                                            string receipt = "";
                                            if (strPaymentSplit.Count() == 6)
                                            {
                                                try
                                                {
                                                    if (tableArray[1].Count() != 0)
                                                    {
                                                        receipt = tableArray[1];
                                                    }
                                                }
                                                catch { }
                                                gc.insert_date(orderNumber, strPaymentSplit[1], 749, strPaymentSplit[0] + "~" + strPaymentSplit[2] + strPaymentSplit[3] + strPaymentSplit[4] + "~" + "~" + strPaymentSplit[5] + receipt, 1, DateTime.Now);
                                                break;
                                            }
                                            if (strPaymentSplit.Count() == 9)
                                            {
                                                try
                                                {
                                                    if (tableArray[1].Count() != 0)
                                                    {
                                                        receipt = tableArray[1];
                                                    }
                                                }
                                                catch { }
                                                gc.insert_date(orderNumber, strPaymentSplit[1], 749, strPaymentSplit[0] + "~" + strPaymentSplit[2] + strPaymentSplit[3] + strPaymentSplit[4] + "~" + strPaymentSplit[5] + strPaymentSplit[6] + strPaymentSplit[7] + "~" + strPaymentSplit[8] + receipt, 1, DateTime.Now);
                                                break;
                                            }
                                        }
                                    }
                                    catch (Exception Ex) { }
                                    k++;
                                    chDriver.Quit();
                                }
                                catch (Exception e)
                                {
                                }

                                driver.SwitchTo().Window(strCurrent);
                            }
                        }
                    }
                    catch { }
                    driver.FindElement(By.LinkText("Account Summary")).SendKeys(Keys.Enter);
                    IWebElement Iparcel = driver.FindElement(By.Id("taxAccountSummary"));
                    string TaxParcel = gc.Between(Iparcel.Text, "Parcel Number", "Owner");
                    string Current = driver.CurrentWindowHandle;

                    //Account Value Summary
                    string read = "", Title0 = "";
                    IWebElement IValue = driver.FindElement(By.XPath("//*[@id='taxAccountValueSummary']/div/table/tbody"));
                    IList<IWebElement> IValueRow = IValue.FindElements(By.TagName("tr"));
                    IList<IWebElement> IValueTD;
                    foreach (IWebElement value in IValueRow)
                    {
                        IValueTD = value.FindElements(By.TagName("td"));
                        if (IValueTD.Count != 0 && value.Text != "")
                        {
                            if (IValueTD[1].Text.Trim() == "" && IValueTD[0].Text.Trim() != "" && IValueTD[2].Text.Trim() != "" && !IValueTD[0].Text.Contains("Area Id"))
                            {
                                if (check != "" && check == "Area ID")
                                {
                                    check = "";
                                    ValueAmount += IValueTD[0].Text + "~" + IValueTD[2].Text + "~";
                                }
                                else
                                {
                                    try
                                    {
                                        IWebElement Iread = IValueTD[0].FindElement(By.TagName("b"));
                                        read = Iread.Text;
                                    }
                                    catch { }
                                    if (read.Trim() != "")
                                    {
                                        ValueTitle += IValueTD[0].Text + "~";
                                        ValueAmount += IValueTD[2].Text + "~";
                                    }
                                    else
                                    {
                                        ValueTitle += Title0.Trim() + " " + IValueTD[0].Text + "~";
                                        ValueAmount += IValueTD[2].Text + "~";
                                    }
                                }
                            }
                            if (IValueTD[1].Text.Trim() == "" && IValueTD[0].Text.Trim() != "" && IValueTD[2].Text.Trim() != "" && IValueTD[0].Text.Contains("Area Id"))
                            {
                                ValueTitle += IValueTD[0].Text + "~" + IValueTD[2].Text + "~";
                                check = "Area ID";
                            }
                            if (IValueTD[0].Text.Trim() == "" && IValueTD[1].Text.Trim() != "" && IValueTD[2].Text.Trim() != "")
                            {
                                Title1 = IValueTD[1].Text;
                                Title2 = IValueTD[2].Text;
                            }
                            if (IValueTD[0].Text.Trim() != "" && IValueTD[1].Text.Trim() == "" && IValueTD[2].Text.Trim() == "")
                            {
                                Title0 = IValueTD[0].Text;
                            }
                            if (IValueTD[0].Text.Trim() != "" && IValueTD[1].Text.Trim() != "" && IValueTD[2].Text.Trim() != "")
                            {
                                ValueTitle += IValueTD[0].Text + "(" + Title1 + ")" + "~" + IValueTD[0].Text + "(" + Title2 + ")" + "~";
                                ValueAmount += IValueTD[1].Text + "~" + IValueTD[2].Text + "~";
                            }
                        }
                    }

                    db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + ValueTitle.Remove(ValueTitle.Length - 1, 1) + "' where Id = '" + 677 + "'");
                    gc.insert_date(orderNumber, parcelNumber, 677, ValueAmount.Remove(ValueAmount.Length - 1, 1), 1, DateTime.Now);

                    //TaxDue
                    IWebElement IDue;
                    try
                    {
                        driver.FindElement(By.Id("paymentTypeFirst")).Click();
                    }
                    catch { }
                    IDue = driver.FindElement(By.Id("totals"));
                    if (IDue.Text.Contains("$0.00") || (!IDue.Text.Contains("Interest") && !IDue.Text.Contains("Interest")))
                    {
                        IWebElement ITaxDate = driver.FindElement(By.Id("paymentDate"));
                        string strASDate = ITaxDate.GetAttribute("value");
                        IWebElement ITotal = driver.FindElement(By.XPath("//*[@id='totals']/table/tbody"));
                        string strTotalDue = GlobalClass.After(ITotal.Text, "Total Due");
                        strType = driver.FindElement(By.XPath("//*[@id='inquiryForm']/table/tbody/tr[2]/td[2]/label[1]")).Text;

                        string PayFDueDetails = strASDate + "~" + strType + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + strTotalDue + "~" + TaxAuthority;
                        gc.insert_date(orderNumber, TaxParcel, 676, PayFDueDetails, 1, DateTime.Now);
                    }
                    try
                    {
                        driver.FindElement(By.Id("paymentTypeFull")).Click();
                    }
                    catch { }
                    try
                    {
                        driver.FindElement(By.Id("paymentTypeSecond")).Click();
                    }
                    catch { }
                    if (IDue.Text.Contains("$0.00") || (!IDue.Text.Contains("Interest") && !IDue.Text.Contains("Interest")))
                    {
                        IWebElement ITaxDate = driver.FindElement(By.Id("paymentDate"));
                        string strASDate = ITaxDate.GetAttribute("value");
                        IWebElement ITotal = driver.FindElement(By.XPath("//*[@id='totals']/table/tbody"));
                        string strTotalDue = GlobalClass.After(ITotal.Text, "Total Due");
                        strType = driver.FindElement(By.XPath("//*[@id='inquiryForm']/table/tbody/tr[2]/td[2]/label[2]")).Text;

                        string PayFDueDetails = strASDate + "~" + strType + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + strTotalDue + "~" + TaxAuthority;
                        gc.insert_date(orderNumber, TaxParcel, 676, PayFDueDetails, 1, DateTime.Now);
                    }
                    if (!IDue.Text.Contains("$0.00") || (IDue.Text.Contains("Interest") && IDue.Text.Contains("Interest") && IDue.Text.Contains("Total")))
                    {
                        string DueDate = "";
                        IWebElement ITaxDate = driver.FindElement(By.Id("paymentDate"));
                        string strDueDate = ITaxDate.GetAttribute("value");
                        driver.FindElement(By.Id("paymentTypeFirst")).Click();
                        Thread.Sleep(3000);
                        string Firsttype = driver.FindElement(By.XPath("//*[@id='inquiryForm']/table/tbody/tr[2]/td[2]/label[1]")).Text;
                        TaxDueDate(orderNumber, TaxParcel, strDueDate, Firsttype, TaxAuthority);
                        try
                        {
                            driver.FindElement(By.Id("paymentTypeFull")).Click();
                        }
                        catch { }
                        try
                        {
                            driver.FindElement(By.Id("paymentTypeSecond")).Click();
                        }
                        catch { }
                        Thread.Sleep(3000);
                        string Fulltype = driver.FindElement(By.XPath("//*[@id='inquiryForm']/table/tbody/tr[2]/td[2]/label[2]")).Text;
                        TaxDueDate(orderNumber, TaxParcel, strDueDate, Fulltype, TaxAuthority);
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
                            DueDate = nextEndOfMonth;
                        }
                        else
                        {
                            string EndOfMonth = new DateTime(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(Convert.ToInt16(DateTime.Now.ToString("MM"))), DateTime.DaysInMonth(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(DateTime.Now.ToString("MM")))).ToString("MM/dd/yyyy");
                            DueDate = EndOfMonth;
                        }
                        IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                        js.ExecuteScript("document.getElementById('paymentDate').setAttribute('value', '" + DueDate + "')");
                        strDueDate = ITaxDate.GetAttribute("value");
                        driver.FindElement(By.Id("paymentTypeFirst")).Click();
                        Thread.Sleep(3000);
                        string DFirsttype = driver.FindElement(By.XPath("//*[@id='inquiryForm']/table/tbody/tr[2]/td[2]/label[1]")).Text;
                        TaxDueDate(orderNumber, TaxParcel, strDueDate, DFirsttype, TaxAuthority);
                        try
                        {
                            driver.FindElement(By.Id("paymentTypeFull")).Click();
                        }
                        catch { }
                        try
                        {
                            driver.FindElement(By.Id("paymentTypeSecond")).Click();
                        }
                        catch { }
                        Thread.Sleep(3000);
                        string DFulltype = driver.FindElement(By.XPath("//*[@id='inquiryForm']/table/tbody/tr[2]/td[2]/label[2]")).Text;
                        TaxDueDate(orderNumber, TaxParcel, strDueDate, DFulltype, TaxAuthority);
                    }

                    //Account Value Details
                    driver.FindElement(By.LinkText("Account Value")).SendKeys(Keys.Enter);
                    gc.CreatePdf(orderNumber, parcelNumber, "Tax Account Value Details", driver, "NM", "Dona Ana");
                    string Authority = "", AuthorityId = "", MillyLevy = "", Amount = "", PropCode = "", ValueType = "", PropActual = "", PropAssessed = "";
                    string strTaxbilled = driver.FindElement(By.XPath("//*[@id='middle']/h2")).Text;
                    IWebElement ITaxBillTable = driver.FindElement(By.XPath("//*[@id='middle']/table[3]/tbody"));
                    IList<IWebElement> ITaxBillRow = ITaxBillTable.FindElements(By.TagName("tr"));
                    IList<IWebElement> ITaxBillTd;
                    foreach (IWebElement TaxBill in ITaxBillRow)
                    {
                        ITaxBillTd = TaxBill.FindElements(By.TagName("td"));
                        if (ITaxBillTd.Count != 0)
                        {
                            Authority = ITaxBillTd[0].Text;
                            AuthorityId = ITaxBillTd[1].Text;
                            MillyLevy = ITaxBillTd[2].Text;
                            Amount = ITaxBillTd[3].Text;

                            string TaxBillDetails = strTaxbilled + "~" + Authority + "~" + AuthorityId + "~" + MillyLevy + "~" + Amount + "~" + " " + "~" + " " + "~" + " " + "~" + " ";
                            gc.insert_date(orderNumber, TaxParcel, 678, TaxBillDetails, 1, DateTime.Now);
                        }
                    }

                    IWebElement IPropertyCodeTable = driver.FindElement(By.XPath("//*[@id='middle']/table[4]/tbody"));
                    IList<IWebElement> IPropertyCodeRow = IPropertyCodeTable.FindElements(By.TagName("tr"));
                    IList<IWebElement> IPropertyCodeTd;
                    foreach (IWebElement Property in IPropertyCodeRow)
                    {
                        IPropertyCodeTd = Property.FindElements(By.TagName("td"));
                        if (IPropertyCodeTd.Count != 0)
                        {
                            PropCode = IPropertyCodeTd[0].Text;
                            ValueType = IPropertyCodeTd[1].Text;
                            PropActual = IPropertyCodeTd[2].Text;
                            PropAssessed = IPropertyCodeTd[3].Text;

                            string PropertyCodeDetails = "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + PropCode + "~" + ValueType + "~" + PropActual + "~" + PropAssessed;
                            gc.insert_date(orderNumber, TaxParcel, 678, PropertyCodeDetails, 1, DateTime.Now);
                        }
                    }
                    //Tax Transaction Details
                    driver.FindElement(By.LinkText("Transaction Detail")).SendKeys(Keys.Enter);
                    gc.CreatePdf(orderNumber, parcelNumber, "Tax Transaction Details", driver, "NM", "Dona Ana");
                    int year = DateTime.Now.Year;
                    string firstTransYear = Convert.ToString(year - 1), secondTransYear = Convert.ToString(year - 2), thirdTransYear = Convert.ToString(year - 3);
                    string TaxYear = "", TaxDue = "", InterestDue = "", PenaltyDue = "", MiscDue = "", LienDue = "", LienInterestDue = "", TotalDue = "", TaxTransYear = "", Type = "", EffectiveDate = "", TransAmount = "", TransBalance = "";
                    IWebElement ITaxSummary = driver.FindElement(By.XPath("//*[@id='middle']/table[1]"));
                    IList<IWebElement> ITaxSummaryRow = ITaxSummary.FindElements(By.TagName("tr"));
                    IList<IWebElement> ITaxSummaryTd;
                    foreach (IWebElement TaxSummary in ITaxSummaryRow)
                    {
                        ITaxSummaryTd = TaxSummary.FindElements(By.TagName("td"));
                        if (ITaxSummaryTd.Count != 0)
                        {
                            TaxYear = ITaxSummaryTd[0].Text;
                            TaxDue = ITaxSummaryTd[1].Text;
                            InterestDue = ITaxSummaryTd[2].Text;
                            PenaltyDue = ITaxSummaryTd[3].Text;
                            MiscDue = ITaxSummaryTd[4].Text;
                            LienDue = ITaxSummaryTd[5].Text;
                            LienInterestDue = ITaxSummaryTd[6].Text;
                            TotalDue = ITaxSummaryTd[7].Text;


                            string TaxSummaryDetails = TaxYear + "~" + TaxDue + "~" + InterestDue + "~" + PenaltyDue + "~" + MiscDue + "~" + LienDue + "~" + LienInterestDue + "~" + TotalDue + "~" + "" + "~" + "" + "~" + "" + "~" + "";
                            gc.insert_date(orderNumber, TaxParcel, 679, TaxSummaryDetails, 1, DateTime.Now);
                        }
                    }

                    IWebElement ITaxTrasaction = driver.FindElement(By.XPath("//*[@id='middle']/table[2]/tbody"));
                    IList<IWebElement> ITaxTrasactionRow = ITaxTrasaction.FindElements(By.TagName("tr"));
                    IList<IWebElement> ITaxTrasactionTd;
                    foreach (IWebElement Trasaction in ITaxTrasactionRow)
                    {
                        ITaxTrasactionTd = Trasaction.FindElements(By.TagName("td"));
                        if (ITaxTrasactionTd.Count != 0 || Trasaction.Text.Contains(Convert.ToString(year)) || Trasaction.Text.Contains(firstTransYear) || Trasaction.Text.Contains(secondTransYear) || Trasaction.Text.Contains(thirdTransYear))
                        {
                            TaxTransYear = ITaxTrasactionTd[0].Text;
                            Type = ITaxTrasactionTd[1].Text;
                            EffectiveDate = ITaxTrasactionTd[2].Text;
                            TransAmount = ITaxTrasactionTd[3].Text;
                            TransBalance = ITaxTrasactionTd[4].Text;

                            string TaxTransactionDetails = TaxTransYear + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + Type + "~" + EffectiveDate + "~" + TransAmount + "~" + TransBalance;
                            gc.insert_date(orderNumber, TaxParcel, 679, TaxTransactionDetails, 1, DateTime.Now);
                        }
                    }

                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "NM", "Dona Ana", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    gc.mergpdf(orderNumber, "NM", "Dona Ana");
                    return "Data Inserted Successfully";
                }
                catch (Exception ex)
                {
                    driver.Quit();
                    throw ex;
                }
            }
        }
        public void TaxDueDate(string orderNumber,string TaxParcel,string AsDate,string strType,string TaxAuthority)
        {
            string TDueDate="",PayTaxDue = "", PayInterestDue = "", PayPenaltyDue = "", PayTotalDue = "", PayMiscDue="";
            try
            {
               IWebElement IPayment = driver.FindElement(By.XPath("//*[@id='paymentLinks']/table/tbody/tr[1]/td[1]"));
                TDueDate = GlobalClass.After(IPayment.Text, "First Half Payment\r\nDue ");
            }
            catch { }
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
                    if (Due.Text.Contains("Interest Due"))
                    {
                        PayInterestDue = IDueDetailTd[1].Text;
                    }
                    if (Due.Text.Contains("Penalty Due"))
                    {
                        PayPenaltyDue = IDueDetailTd[1].Text;
                    }
                    if (Due.Text.Contains("Misc Due"))
                    {
                        PayMiscDue = IDueDetailTd[1].Text;
                    }
                    if (Due.Text.Contains("Total Due"))
                    {
                        PayTotalDue = IDueDetailTd[1].Text;
                    }
                }
            }

            string PayDueDetails = AsDate + "~" + strType + "~" + TDueDate + "~" + PayTaxDue + "~" + PayInterestDue + "~" + PayPenaltyDue + "~" + PayMiscDue + "~" + PayTotalDue + "~" + TaxAuthority;
            gc.insert_date(orderNumber, TaxParcel, 676, PayDueDetails, 1, DateTime.Now);
        }
    }
}