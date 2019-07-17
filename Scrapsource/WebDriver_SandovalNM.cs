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
    public class WebDriver_SandovalNM
    {
        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        MySqlParameter[] mParam;
        public string FTP_SandovalNM(string houseno, string Direction, string sname, string stype, string account, string parcelNumber, string ownername, string searchType, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
            string multi = "";
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            using (driver = new PhantomJSDriver())
            {
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");
                    driver.Navigate().GoToUrl("https://eaweb.sandovalcountynm.gov/Assessor/taxweb/search.jsp");
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
                            driver.FindElement(By.Id("SitusIDUnitNumber")).SendKeys(account);
                        }
                        catch { }
                        gc.CreatePdf_WOP(orderNumber, "Address Search", driver, "NM", "Sandoval");
                        driver.FindElement(By.XPath("//*[@id='middle']/form/font/font/table/tbody/tr/td[1]/input")).SendKeys(Keys.Enter);

                        try
                        {
                            string straccount = "", strparcel = "", strname = "", straddress = "";
                            string strmulti = driver.FindElement(By.XPath("//*[@id='middle']/p[3]")).Text;
                            multi = gc.Between(strmulti, "Showing ", " results").Trim();
                            if (Convert.ToInt32(multi) > 25)
                            {
                                HttpContext.Current.Session["multiParcel_Sandoval_Maximum"] = "Maximum";
                                driver.Quit();
                                return "Maximum";
                            }
                            if (Convert.ToInt32(multi) < 25 && Convert.ToInt32(multi) != 0)
                            {
                                gc.CreatePdf_WOP(orderNumber, "Multi Name Search", driver, "NM", "Sandoval");
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
                                                    strparcel = GlobalClass.Before(IMultiSummaryTD[0].Text, "\r\n");
                                                    if (strparcel == "")
                                                    {
                                                        strparcel = IMultiSummaryTD[0].Text;
                                                    }
                                                    strname = IMultiSummaryTD[1].Text;
                                                    straddress = IMultiSummaryTD[2].Text.Replace("\r\n", "");
                                                }
                                            }

                                            string multiDetails = straccount + "~" + strname + "~" + straddress;
                                            gc.insert_date(orderNumber, strparcel, 915, multiDetails, 1, DateTime.Now);
                                        }
                                    }
                                }
                                HttpContext.Current.Session["multiParcel_Sandoval"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                        }
                        catch { }
                    }

                    if (searchType == "parcel")
                    {
                        driver.FindElement(By.Id("ParcelNumberID")).SendKeys(parcelNumber.Replace("-", ""));
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search", driver, "NM", "Sandoval");
                        driver.FindElement(By.XPath("//*[@id='middle']/form/font/font/table/tbody/tr/td[1]/input")).SendKeys(Keys.Enter);
                    }
                    if (searchType == "ownername")
                    {
                        driver.FindElement(By.Id("OwnerIDSearchString")).SendKeys(ownername);
                        gc.CreatePdf_WOP(orderNumber, "Owner Name Search", driver, "NM", "Sandoval");
                        driver.FindElement(By.XPath("//*[@id='middle']/form/font/font/table/tbody/tr/td[1]/input")).SendKeys(Keys.Enter);

                        try
                        {
                            string straccount = "", strparcel = "", strname = "", straddress = "";
                            string strmulti = driver.FindElement(By.XPath("//*[@id='middle']/p[3]")).Text;
                            multi = gc.Between(strmulti, "Showing ", " results").Trim();
                            if (Convert.ToInt32(multi) > 25)
                            {
                                HttpContext.Current.Session["multiParcel_Sandoval_Maximum"] = "Maximum";
                                driver.Quit();
                                return "Maximum";
                            }
                            if (Convert.ToInt32(multi) < 25 && Convert.ToInt32(multi) != 0)
                            {
                                gc.CreatePdf_WOP(orderNumber, "Multi Name Search", driver, "NM", "Sandoval");
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
                                            gc.insert_date(orderNumber, strparcel, 915, multiDetails, 1, DateTime.Now);
                                        }
                                    }
                                }
                                HttpContext.Current.Session["multiParcel_Sandoval"] = "Yes";
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
                            gc.CreatePdf_WOP(orderNumber, "Account Search", driver, "NM", "Sandoval");
                            driver.FindElement(By.XPath("//*[@id='middle']/form/font/font/table/tbody/tr/td[1]/input")).SendKeys(Keys.Enter);
                        }
                    }

                    try
                    {
                        IWebElement Inodata = driver.FindElement(By.Id("middle"));
                        if(Inodata.Text.Contains("No results found"))
                        {
                            HttpContext.Current.Session["Nodata_SandovalNM"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }
                    //Property Details
                    string AccountNo = "", SitusAddress = "", TaxArea = "", LegalSummary = "", strOwnerName = "", strAddress = "", ValueSummary = "", MValueSummary = "", PropertyStatus = "";
                    driver.FindElement(By.XPath("//*[@id='searchResultsTable']/tbody/tr[2]/td[1]/a")).SendKeys(Keys.Enter);
                    AccountNo = GlobalClass.After(driver.FindElement(By.XPath("//*[@id='middle']/h1[1]")).Text, "Account: ").Trim();
                    IWebElement Iaddress = driver.FindElement(By.XPath("//*[@id='middle']/table/tbody/tr[2]/td[1]/table"));
                    SitusAddress = gc.Between(Iaddress.Text, "Situs Address ", "\r\nLegal Summary Legal");
                    TaxArea = gc.Between(Iaddress.Text, "Tax Area ", "\r\nSitus Address");
                    parcelNumber = gc.Between(Iaddress.Text, "Parcel Number ", "\r\nTax Area");
                    gc.CreatePdf(orderNumber, parcelNumber.Replace("-", ""), "Property Details", driver, "NM", "Sandoval");
                    LegalSummary = GlobalClass.After(Iaddress.Text, "Legal Summary ");
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
                            strHistoryHead += IAssessmentTd[0].Text.Replace("'", "") + "~";
                            strHistoryValue += IAssessmentTd[1].Text + "~";
                        }
                        if (IAssessmentTd.Count != 0 && IAssessmentTd.Count == 1)
                        {
                            strHistoryHead += IAssessmentTd[0].Text.Replace("'", "") + "~";
                            strHistoryValue += " " + "~";
                        }
                    }
                    string Year = gc.Between(IAssessmentHistory.Text, "Actual Value (", ")");
                    IWebElement IMillLevy = driver.FindElement(By.XPath("//*[@id='middle']/table/tbody/tr[2]/td[3]/table[2]/caption"));
                    string strTaxArea = gc.Between(IMillLevy.Text, "Tax Area: ", "Mill Levy: ");
                    string strMillLevy = GlobalClass.After(IMillLevy.Text, "Mill Levy: ");
                    db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + " Year" + "~" + strHistoryHead + "Tax Area" + "~" + "Mill Levy" + "' where Id = '" + 917 + "'");
                    gc.insert_date(orderNumber, parcelNumber, 917, Year + "~" + strHistoryValue + strTaxArea + "~" + strMillLevy, 1, DateTime.Now);

                    //Assessmnet History Details
                    driver.FindElement(By.LinkText("Assessment History")).SendKeys(Keys.Enter);
                    gc.CreatePdf(orderNumber, parcelNumber.Replace("-", ""), "Assessment History Details", driver, "NM", "Sandoval");
                    string strType = "", firstYear = "", secondYear = "", thirdYear = "", fourthYear = "";
                    string[] strsummary = new string[] { "", "", "", "", "", "", "", "", "", "" };
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
                                try
                                {
                                    strsummary[i] = IValueSummaryTh[i].Text;
                                }
                                catch { }
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
                        db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + "Year" + "~" + strType.Remove(strType.Length - 1, 1) + "' where Id = '" + 918 + "'");
                        gc.insert_date(orderNumber, parcelNumber, 918, strsummary[1] + "~" + firstYear.Remove(firstYear.Length - 1, 1), 1, DateTime.Now);
                        gc.insert_date(orderNumber, parcelNumber, 918, strsummary[2] + "~" + secondYear.Remove(secondYear.Length - 1, 1), 1, DateTime.Now);
                        gc.insert_date(orderNumber, parcelNumber, 918, strsummary[3] + "~" + thirdYear.Remove(thirdYear.Length - 1, 1), 1, DateTime.Now);
                        gc.insert_date(orderNumber, parcelNumber, 918, strsummary[4] + "~" + fourthYear.Remove(fourthYear.Length - 1, 1), 1, DateTime.Now);
                    }
                    catch { }
                    string PropertyCode = "", Acres = "", YearBuilt = "", EffectiveYear = "", MPropertyCode = "", MAcres = "", MYearBuilt = "", MEffectiveYear = "";
                    try
                    {
                        //Residential Details
                        driver.FindElement(By.LinkText("Residential")).SendKeys(Keys.Enter);
                        gc.CreatePdf(orderNumber, parcelNumber.Replace("-", ""), "Residential Details", driver, "NM", "Sandoval");
                        IWebElement IPropertyCode = driver.FindElement(By.Id("tabcontentcontainer"));
                        PropertyCode = gc.Between(IPropertyCode.Text, "Abstract Code", "Override");
                        try
                        {
                            YearBuilt = gc.Between(IPropertyCode.Text, "Estimated Year Built", "  Frame");
                        }
                        catch { }
                    }
                    catch { }

                    try
                    {
                        //Mobile details
                        driver.FindElement(By.LinkText("Land")).SendKeys(Keys.Enter);
                        gc.CreatePdf(orderNumber, parcelNumber, "Mobile Details", driver, "NM", "Sandoval");
                        IWebElement IMPropertyCode = driver.FindElement(By.Id("tabcontentcontainer"));
                        Acres = gc.Between(IMPropertyCode.Text, "Acres", "SQFT");
                    }
                    catch { }
                    ValueSummary = AccountNo + "~" + SitusAddress + "~" + TaxArea + "~" + LegalSummary + "~" + strOwnerName + "~" + strAddress + "~" + PropertyCode + "~" + Acres + "~" + YearBuilt;
                    gc.insert_date(orderNumber, parcelNumber, 916, ValueSummary, 1, DateTime.Now);
                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");

                    string first = "", check = "", AreaID = "", Title = "", MillLevy = "", Actual = "", Assessed = "", Taxes = "", TActual = "", TAssessed = "";
                    string ValueTitle = "", ValueAmount = "", Title1 = "", Title2 = "";
                    int k = 0, valuecount = 1;
                    driver.Navigate().GoToUrl("https://etweb.sandovalcountynm.gov/Treasurer/web/login.jsp");
                    gc.CreatePdf(orderNumber, parcelNumber.Replace("-", ""), "TaxAuthority Details", driver, "NM", "Sandoval");
                    string strtaxauthority = driver.FindElement(By.Id("left")).Text;
                    string TaxAuthority = GlobalClass.After(strtaxauthority, "Mailing address:").Replace("\r\n", " ").Trim();
                    driver.FindElement(By.XPath("//*[@id='middle_left']/form/input[1]")).SendKeys(Keys.Enter);
                    driver.FindElement(By.Id("TaxAParcelID")).SendKeys(parcelNumber.Replace("-", "").Trim());
                    gc.CreatePdf(orderNumber, parcelNumber.Replace("-", ""), "Tax Details Search", driver, "NM", "Sandoval");
                    driver.FindElement(By.XPath("//*[@id='middle']/form/table[4]/tbody/tr/td[1]/input")).SendKeys(Keys.Enter);
                    gc.CreatePdf(orderNumber, parcelNumber.Replace("-", ""), "Tax Details Search Result", driver, "NM", "Sandoval");
                    driver.FindElement(By.XPath("//*[@id='searchResultsTable']/tbody/tr/td[1]/strong/a")).SendKeys(Keys.Enter);
                    gc.CreatePdf(orderNumber, parcelNumber.Replace("-", ""), "Tax Details", driver, "NM", "Sandoval");

                    driver.FindElement(By.LinkText("Account Summary")).SendKeys(Keys.Enter);
                    IWebElement Iparcel = driver.FindElement(By.Id("taxAccountSummary"));
                    string TaxParcel = gc.Between(Iparcel.Text, "Parcel Number", "Owner");
                    string Current = driver.CurrentWindowHandle;

                    //Account Value Summary
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
                                    if (ValueTitle == (IValueTD[0].Text))
                                    {
                                        ValueTitle += first + IValueTD[0].Text + "~";
                                        ValueAmount += IValueTD[2].Text + "~";
                                        // valuecount++;
                                    }
                                    else
                                    {
                                        if (IValueTD[0].Text == "Taxes")
                                        {
                                            ValueTitle += IValueTD[0].Text + "~";
                                            ValueAmount += IValueTD[2].Text + "~";
                                        }
                                        else
                                        {
                                            ValueTitle += first + " " + IValueTD[0].Text + "~";
                                            ValueAmount += IValueTD[2].Text + "~";
                                        }
                                    }
                                }
                            }
                            if (IValueTD[1].Text.Trim() == "" && IValueTD[0].Text.Trim() != "" && IValueTD[2].Text.Trim() == "")
                            {
                                first = IValueTD[0].Text;
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
                            if (IValueTD[0].Text.Trim() != "" && IValueTD[1].Text.Trim() != "" && IValueTD[2].Text.Trim() != "")
                            {
                                ValueTitle += IValueTD[0].Text + "(" + Title1 + ")" + "~" + IValueTD[0].Text + "(" + Title2 + ")" + "~";
                                ValueAmount += IValueTD[1].Text + "~" + IValueTD[2].Text + "~";
                            }
                        }
                    }

                    db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + ValueTitle.Remove(ValueTitle.Length - 1, 1).Replace("'", "") + "' where Id = '" + 920 + "'");
                    gc.insert_date(orderNumber, parcelNumber, 920, ValueAmount.Remove(ValueAmount.Length - 1, 1), 1, DateTime.Now);

                    //TaxDue
                    IWebElement IDue;
                    string strFDueDate = "", strSDueDate = "";
                    try
                    {
                        driver.FindElement(By.Id("paymentTypeFirst")).Click();
                    }
                    catch { }
                    IDue = driver.FindElement(By.Id("totals"));
                    if (IDue.Text.Contains("$0.00") || (!IDue.Text.Contains("Interest") && !IDue.Text.Contains("Total")))
                    {
                        IWebElement ITaxDate = driver.FindElement(By.Id("paymentDate"));
                        string strASDate = ITaxDate.GetAttribute("value");
                        IWebElement ITotal = driver.FindElement(By.XPath("//*[@id='totals']/table/tbody"));
                        string strTotalDue = GlobalClass.After(ITotal.Text, "Total Due");
                        strType = driver.FindElement(By.XPath("//*[@id='inquiryForm']/table/tbody/tr[2]/td[2]/label[1]")).Text;
                        try
                        {
                            string FirstDueDate = driver.FindElement(By.XPath("//*[@id='middle']/p/b")).Text;
                            strFDueDate = gc.Between(FirstDueDate, "first half taxes by ", " and second");
                        }
                        catch (Exception EX) { }
                        string PayFDueDetails = strASDate + "~" + strType + "~" + strFDueDate + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + strTotalDue + "~" + TaxAuthority;
                        gc.insert_date(orderNumber, TaxParcel, 919, PayFDueDetails, 1, DateTime.Now);
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
                    if (IDue.Text.Contains("$0.00") || (!IDue.Text.Contains("Interest") && !IDue.Text.Contains("Total")))
                    {
                        IWebElement ITaxDate = driver.FindElement(By.Id("paymentDate"));
                        string strASDate = ITaxDate.GetAttribute("value");
                        IWebElement ITotal = driver.FindElement(By.XPath("//*[@id='totals']/table/tbody"));
                        string strTotalDue = GlobalClass.After(ITotal.Text, "Total Due");
                        strType = driver.FindElement(By.XPath("//*[@id='inquiryForm']/table/tbody/tr[2]/td[2]/label[2]")).Text;
                        try
                        {
                            strSDueDate = gc.Between(driver.FindElement(By.XPath("//*[@id='middle']/p")).Text, "second half taxes by ", ".");
                        }
                        catch { }

                        string PayFDueDetails = strASDate + "~" + strType + "~" + strSDueDate + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + strTotalDue + "~" + TaxAuthority;
                        gc.insert_date(orderNumber, TaxParcel, 919, PayFDueDetails, 1, DateTime.Now);
                    }
                    if (!IDue.Text.Contains("$0.00") || (IDue.Text.Contains("Interest") && IDue.Text.Contains("Interest") && IDue.Text.Contains("Total")))
                    {
                        string DueDate = "";
                        IWebElement ITaxDate = driver.FindElement(By.Id("paymentDate"));
                        string strDueDate = ITaxDate.GetAttribute("value");
                        driver.FindElement(By.Id("paymentTypeFirst")).Click();
                        Thread.Sleep(3000);
                        string Firsttype = driver.FindElement(By.XPath("//*[@id='inquiryForm']/table/tbody/tr[2]/td[2]/label[1]")).Text;
                        // gc.CreatePdf(orderNumber, parcelNumber.Replace("-", ""), "Tax Due First Details", driver, "NM", "Sandoval");
                        //TaxDueDate(orderNumber, TaxParcel, strDueDate, Firsttype, TaxAuthority);
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
                        gc.CreatePdf(orderNumber, parcelNumber.Replace("-", ""), "Tax Due Full Second Details", driver, "NM", "Sandoval");
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
                    gc.CreatePdf(orderNumber, parcelNumber.Replace("-", ""), "Tax Account Value Details", driver, "NM", "Sandoval");
                    string Authority = "", AuthorityId = "", MillyLevy = "", Amount = "", PropCode = "", PreviousPropCode = "", ValueType = "", PropActual = "", PropAssessed = "";
                    int count = 1;
                    string strTaxbilled = driver.FindElement(By.XPath("//*[@id='middle']/h2")).Text;
                    IWebElement ITaxBillTable = driver.FindElement(By.XPath("//*[@id='middle']/table[3]/tbody"));
                    IList<IWebElement> ITaxBillRow = ITaxBillTable.FindElements(By.TagName("tr"));
                    IList<IWebElement> ITaxBillTd;
                    foreach (IWebElement TaxBill in ITaxBillRow)
                    {
                        ITaxBillTd = TaxBill.FindElements(By.TagName("td"));
                        if (ITaxBillTd.Count != 0 && ITaxBillTd.Count == 4)
                        {
                            Authority = ITaxBillTd[0].Text;
                            AuthorityId = ITaxBillTd[1].Text;
                            MillyLevy = ITaxBillTd[2].Text;
                            Amount = ITaxBillTd[3].Text;

                            string TaxBillDetails = strTaxbilled + "~" + Authority + "~" + AuthorityId + "~" + MillyLevy + "~" + Amount + "~" + " " + "~" + " " + "~" + " " + "~" + " ";
                            gc.insert_date(orderNumber, TaxParcel, 921, TaxBillDetails, 1, DateTime.Now);
                        }
                        if (ITaxBillTd.Count != 0 && ITaxBillTd.Count == 5)
                        {
                            Authority = ITaxBillTd[0].Text;
                            AuthorityId = ITaxBillTd[1].Text;
                            MillyLevy = ITaxBillTd[2].Text;
                            Amount = ITaxBillTd[4].Text.Replace("(", "").Replace(")", "");

                            string TaxBillDetails = strTaxbilled + "~" + Authority + "~" + AuthorityId + "~" + MillyLevy + "~" + Amount + "~" + " " + "~" + " " + "~" + " " + "~" + " ";
                            gc.insert_date(orderNumber, TaxParcel, 921, TaxBillDetails, 1, DateTime.Now);
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
                            gc.insert_date(orderNumber, TaxParcel, 921, PropertyCodeDetails, 1, DateTime.Now);
                        }
                    }
                    //Tax Transaction Details
                    driver.FindElement(By.LinkText("Transaction Detail")).SendKeys(Keys.Enter);
                    gc.CreatePdf(orderNumber, parcelNumber.Replace("-", ""), "Tax Transaction Details", driver, "NM", "Sandoval");
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
                            gc.insert_date(orderNumber, TaxParcel, 922, TaxSummaryDetails, 1, DateTime.Now);
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
                            gc.insert_date(orderNumber, TaxParcel, 922, TaxTransactionDetails, 1, DateTime.Now);
                        }
                    }

                    try
                    {
                        string fileName = "";
                        var chromeOptions = new ChromeOptions();
                        var downloadDirectory = ConfigurationManager.AppSettings["AutoPdf"];
                        chromeOptions.AddUserProfilePreference("download.default_directory", downloadDirectory);
                        chromeOptions.AddUserProfilePreference("download.prompt_for_download", false);
                        chromeOptions.AddUserProfilePreference("disable-popup-blocking", "true");
                        chromeOptions.AddUserProfilePreference("plugins.always_open_pdf_externally", true);
                        var driver1 = new ChromeDriver(chromeOptions);
                        driver1.Navigate().GoToUrl(driver.Url);
                        driver1.FindElement(By.XPath("//*[@id='middle_left']/form/input[1]")).SendKeys(Keys.Enter);
                        try
                        {
                            driver1.FindElement(By.Id("TaxAParcelID")).SendKeys(parcelNumber.Replace("-", "").Trim());
                            driver1.FindElement(By.XPath("//*[@id='middle']/form/table[4]/tbody/tr/td[1]/input")).SendKeys(Keys.Enter);
                            driver1.FindElement(By.XPath("//*[@id='searchResultsTable']/tbody/tr/td[1]/strong/a")).SendKeys(Keys.Enter);
                        }
                        catch { }

                        for (int j = 1; j < 4; j++)
                        {
                            try
                            {
                                IWebElement Receipttable = driver1.FindElement(By.XPath("//*[@id='receiptHistory']/a[" + j + "]"));
                                string RecieptName = Receipttable.GetAttribute("href");
                                fileName = gc.Between(RecieptName, "taxreceipt/", "?id=").Replace("-", "_");
                                Receipttable.Click();
                                Thread.Sleep(3000);
                                try
                                {
                                    //gc.downloadfile(chDriver.Url, orderNumber, parcelNumber.Replace("-",""), "Reciept" + k, "NM", "Sandoval");
                                    gc.AutoDownloadFile(orderNumber, parcelNumber.Replace("-", ""), "Sandoval", "NM", fileName);
                                }
                                catch (Exception Ex) { }

                                string FilePath = gc.filePath(orderNumber, parcelNumber.Replace("-", "")) + fileName;
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
                                        gc.insert_date(orderNumber, strPaymentSplit[1], 923, strPaymentSplit[0] + "~" + strPaymentSplit[2] + strPaymentSplit[3] + strPaymentSplit[4] + "~" + "~" + strPaymentSplit[5] + receipt, 1, DateTime.Now);
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
                                        gc.insert_date(orderNumber, strPaymentSplit[1], 923, strPaymentSplit[0] + "~" + strPaymentSplit[2] + strPaymentSplit[3] + strPaymentSplit[4] + "~" + strPaymentSplit[5] + strPaymentSplit[6] + strPaymentSplit[7] + "~" + strPaymentSplit[8] + receipt, 1, DateTime.Now);
                                        break;
                                    }
                                }
                            }
                            catch (Exception Ex) { }
                        }
                        driver1.Quit();
                    }
                    catch { }


                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "NM", "Sandoval", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    gc.mergpdf(orderNumber, "NM", "Sandoval");
                    return "Data Inserted Successfully";
                }
                catch (Exception ex)
                {
                    driver.Quit();
                    throw ex;
                }
            }
        }
        public void TaxDueDate(string orderNumber, string TaxParcel, string AsDate, string strType, string TaxAuthority)
        {
            string TDueDate = "", SDueDate="", PayTaxDue = "", PayInterestDue = "", PayPenaltyDue = "", PayTotalDue = "", PayMiscDue = "";
            try
            {
                    string FirstDueDate = driver.FindElement(By.XPath("//*[@id='middle']/p/b")).Text;
                    TDueDate = gc.Between(FirstDueDate, "first half taxes by ", " and second");

                    SDueDate = gc.Between(driver.FindElement(By.XPath("//*[@id='middle']/p")).Text, "second half taxes by ", ".");
          
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

            string PayDueDetails = AsDate + "~" + strType + "~" + TDueDate + "~" + SDueDate + "~" + PayTaxDue + "~" + PayInterestDue + "~" + PayPenaltyDue + "~" + PayMiscDue + "~" + PayTotalDue + "~" + TaxAuthority;
            gc.insert_date(orderNumber, TaxParcel, 919, PayDueDetails, 1, DateTime.Now);
        }
    }
}

