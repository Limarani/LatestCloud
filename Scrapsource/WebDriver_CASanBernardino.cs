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
    public class WebDriver_CASanBernardino
    {
        string Parcelno = "", Name = "", Address = "", MultiOwner_details = "";
        string ownername1 = "", Effective_Date = "", ownername2 = "", Owner_Mailing_Address = "", Legal_Description = "", Property_Details = "";
        string Land_Value = "", Improvement_Value = "", Improvement_Penalty = "", PresProp_Value = "", PresProp_Penalty = "", Total_Penalties = "", TotalAssessed_Value = "", Homeowner_Exemption = "", Special_Exemptions = "", Net_Value = "", Assessment_Details = "";
        string Bill_Number = "", Eff_Date = "", Extnd_Date = "", Correction = "", Remarks = "", Payment_details = "";
        string Co_Owner = "", Billed_Owner = "", Default_date = "", Tax_Type = "", TaxBill_Number = "", Extend_date = "", Effective_date = "", Taxrate_Year = "", Penalty = "", Cost = "", CurrentTax_Details = "", Inst_Del = "";
        string UnSecuredCurrentTax_Details = "", UnBilled_Owner = "", UnCo_Owner = "", UnDefault_Date = "", UnTax_Type = "", UnBill_Number = "", UnExtended_Date = "", UnEffective_Owner = "", UnTaxrate_Year = "", SecuredCurrentTax_Details = "";
        string UnCurrentTax_Details = "", Lien = "", Intrest = "", Unpenalty = "", Uncost = "", Redemption = "", Unurl = "", bill_type = "", url = "", Bill_Type = "";
        string address = "", MultiAddress_details = "";
        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        MySqlParameter[] mParam;
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());

        public string FTP_CASanBernardino(string houseno, string sname, string stype, string city, string parcelNumber, string searchType, string orderNumber, string ownername, string unitno)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
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
                        address = houseno + " " + sname + " " + stype + " " + unitno;
                        gc.TitleFlexSearch(orderNumber, parcelNumber, "", address, "CA", "San Bernardino");

                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_CASanBernardino"] = "Zero";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }

                    if (searchType == "address")
                    {
                        driver.Navigate().GoToUrl("http://www.mytaxcollector.com/trSearch.aspx");
                        Thread.Sleep(2000);

                        driver.FindElement(By.XPath("/html/body/table/tbody/tr[3]/td[3]/form/table/tbody/tr[1]/td[2]/nobr/input")).SendKeys(houseno);
                        driver.FindElement(By.XPath("/html/body/table/tbody/tr[3]/td[3]/form/table/tbody/tr[2]/td[2]/nobr/input")).SendKeys(sname);

                        gc.CreatePdf_WOP(orderNumber, "Address search", driver, "CA", "San Bernardino");
                        driver.FindElement(By.Id("ctl00_contentHolder_cmdSearch")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);
                        gc.CreatePdf_WOP(orderNumber, "Address search Result", driver, "CA", "San Bernardino");


                        try
                        {
                            IWebElement MultiAddressTB = driver.FindElement(By.XPath("/html/body/table/tbody/tr[3]/td[3]/div[1]/table/tbody"));
                            IList<IWebElement> MultiAddressTR = MultiAddressTB.FindElements(By.TagName("tr"));
                            IList<IWebElement> MultiAddressTD;
                            gc.CreatePdf_WOP(orderNumber, "Multi Address search", driver, "CA", "San Bernardino");
                            int AddressmaxCheck = 0;

                            if (MultiAddressTR.Count == 2)
                            {
                                driver.FindElement(By.XPath("/html/body/table/tbody/tr[3]/td[3]/div[1]/table/tbody/tr[2]/td[1]/a")).Click();
                                Thread.Sleep(2000);
                            }

                            else
                            {
                                foreach (IWebElement MultiAddress in MultiAddressTR)
                                {
                                    if (AddressmaxCheck <= 25)
                                    {
                                        MultiAddressTD = MultiAddress.FindElements(By.TagName("td"));
                                        if (MultiAddressTD.Count != 0 && !MultiAddress.Text.Contains("Parcel Number"))
                                        {
                                            Parcelno = MultiAddressTD[0].Text;
                                            Address = MultiAddressTD[1].Text;

                                            MultiAddress_details = Address;
                                            gc.insert_date(orderNumber, Parcelno, 557, MultiAddress_details, 1, DateTime.Now);
                                        }
                                        AddressmaxCheck++;
                                    }
                                }
                                if (MultiAddressTR.Count > 25)
                                {
                                    HttpContext.Current.Session["multiParcel_SanBernardino_Multicount"] = "Maximum";
                                }
                                else
                                {
                                    HttpContext.Current.Session["multiparcel_SanBernardino"] = "Yes";
                                }
                                driver.Quit();

                                return "MultiParcel";
                            }
                        }
                        catch
                        { }
                    }
                    if (searchType == "parcel")
                    {
                        driver.Navigate().GoToUrl("http://www.mytaxcollector.com/trSearch.aspx");
                        Thread.Sleep(2000);

                        driver.FindElement(By.XPath("/html/body/table/tbody/tr[3]/td[3]/form/table/tbody/tr[6]/td[2]/input")).SendKeys(parcelNumber);
                        gc.CreatePdf(orderNumber, parcelNumber, "ParcelSearch", driver, "CA", "San Bernardino");

                        driver.FindElement(By.Id("ctl00_contentHolder_cmdSearch")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                    }

                    else if (searchType == "ownername")
                    {
                        driver.Navigate().GoToUrl("http://www.sbcounty.gov/assessor/pims/(S(fbwsp3kdg4b2bm5y5tgxd2r3))/PIMSINTERFACE.ASPX");
                        Thread.Sleep(2000);

                        IWebElement iframeElementOwner = driver.FindElement(By.XPath("//*[@id='frameSearchResults']"));
                        driver.SwitchTo().Frame(iframeElementOwner);
                        Thread.Sleep(2000);

                        driver.FindElement(By.Id("mnuPIMS_2")).Click();
                        Thread.Sleep(3000);

                        driver.FindElement(By.Id("FormattedNameSearchInput1_txtLASTNAME")).SendKeys(ownername);
                        driver.FindElement(By.Id("FormattedNameSearchInput1_btnStartNameSearch")).SendKeys(Keys.Enter);
                        Thread.Sleep(4000);
                        try
                        {
                            IWebElement MultiOwnerTB = driver.FindElement(By.XPath("//*[@id='G_UltraWebGridSearchResults']/tbody"));
                            IList<IWebElement> MultiOwnerTR = MultiOwnerTB.FindElements(By.TagName("tr"));
                            IList<IWebElement> MultiOwnerTD;
                            gc.CreatePdf_WOP(orderNumber, "Multi Owner search", driver, "CA", "San Bernardino");
                            int maxCheck = 0;

                            foreach (IWebElement MultiOwner in MultiOwnerTR)
                            {
                                if (maxCheck <= 25)
                                {
                                    MultiOwnerTD = MultiOwner.FindElements(By.TagName("td"));
                                    if (MultiOwnerTD.Count != 0)
                                    {
                                        Parcelno = MultiOwnerTD[1].Text;
                                        Address = MultiOwnerTD[6].Text;

                                        MultiOwner_details = Address;
                                        gc.insert_date(orderNumber, Parcelno, 557, MultiOwner_details, 1, DateTime.Now);
                                    }
                                    maxCheck++;
                                }
                            }

                            if (MultiOwnerTR.Count > 25)
                            {
                                HttpContext.Current.Session["multiParcel_SanBernardino_Multicount"] = "Maximum";
                            }
                            else
                            {
                                HttpContext.Current.Session["multiparcel_SanBernardino"] = "Yes";
                            }
                            driver.Quit();

                            return "MultiParcel";
                        }
                        catch
                        { }

                    }
                    try
                    {
                        Parcelno = driver.FindElement(By.XPath("/html/body/table/tbody/tr[3]/td[3]/table[1]/tbody/tr[2]/td")).Text;
                        Parcelno = WebDriverTest.After(Parcelno, "Parcel ").Replace("-", "");
                    }
                    catch
                    { }

                    //Property Details
                    driver.Navigate().GoToUrl("http://www.sbcounty.gov/assessor/pims/(S(fbwsp3kdg4b2bm5y5tgxd2r3))/PIMSINTERFACE.ASPX");
                    Thread.Sleep(4000);

                    IWebElement iframeElementAdd = driver.FindElement(By.XPath("//*[@id='frameSearchResults']"));
                    driver.SwitchTo().Frame(iframeElementAdd);
                    Thread.Sleep(2000);

                    driver.FindElement(By.Id("mnuPIMS_1")).Click();
                    Thread.Sleep(3000);

                    driver.FindElement(By.Id("ParcelInquiryInput1_txtParcelNbr")).SendKeys(Parcelno);
                    driver.FindElement(By.Id("ParcelInquiryInput1_btnStartParcelInquiry")).SendKeys(Keys.Enter);
                    Thread.Sleep(6000);
                    driver.SwitchTo().DefaultContent();

                    IWebElement iframeElementAdd1 = driver.FindElement(By.XPath("//*[@id='frmset']/frame[2]"));
                    driver.SwitchTo().Frame(iframeElementAdd1);
                    Thread.Sleep(2000);

                    IWebElement iframeElementAdd2 = driver.FindElement(By.XPath("//*[@id='RadPageViewPropInfo']/iframe"));
                    driver.SwitchTo().Frame(iframeElementAdd2);
                    Thread.Sleep(2000);

                    try
                    {
                        Parcelno = driver.FindElement(By.XPath("//*[@id='spnPROPERTYINFO']/center[1]/table/tbody/tr[2]/td[1]")).Text;
                        ownername1 = driver.FindElement(By.XPath("//*[@id='spnPROPERTYINFO']/div[1]/table/tbody/tr/td[3]/table/tbody/tr[2]/td[1]")).Text;
                        Effective_Date = driver.FindElement(By.XPath("//*[@id='spnPROPERTYINFO']/div[1]/table/tbody/tr/td[3]/table/tbody/tr[2]/td[2]")).Text;
                        ownername2 = driver.FindElement(By.XPath("//*[@id='spnPROPERTYINFO']/div[1]/table/tbody/tr/td[3]/table/tbody/tr[3]/td")).Text;
                        Owner_Mailing_Address = ownername1 + ownername2;
                        Legal_Description = driver.FindElement(By.XPath("//*[@id='spnPROPERTYINFO']/div[3]/textarea")).Text;

                        Property_Details = Owner_Mailing_Address + "~" + Effective_Date + "~" + Legal_Description;
                        gc.CreatePdf(orderNumber, Parcelno, "Property Details", driver, "CA", "San Bernardino");
                        gc.insert_date(orderNumber, Parcelno, 558, Property_Details, 1, DateTime.Now);
                    }
                    catch
                    { }
                    driver.SwitchTo().DefaultContent();

                    //Assessment Details
                    IWebElement iframeElementAdd3 = driver.FindElement(By.XPath("//*[@id='frmset']/frame[2]"));
                    driver.SwitchTo().Frame(iframeElementAdd3);
                    Thread.Sleep(2000);

                    driver.FindElement(By.XPath("//*[@id='RadTabStripMain']/div/ul/li[2]/a")).Click();
                    Thread.Sleep(2000);

                    IWebElement iframeElementAdd4 = driver.FindElement(By.XPath("//*[@id='RadPageViewRollValuesHist']/iframe"));
                    driver.SwitchTo().Frame(iframeElementAdd4);
                    Thread.Sleep(4000);

                    Land_Value = driver.FindElement(By.XPath("//*[@id='mainsect']/tbody/tr[8]/td[1]")).Text;
                    Improvement_Value = driver.FindElement(By.XPath("//*[@id='mainsect']/tbody/tr[9]/td[1]")).Text;
                    Improvement_Penalty = driver.FindElement(By.XPath("//*[@id='mainsect']/tbody/tr[10]/td[1]")).Text;
                    PresProp_Value = driver.FindElement(By.XPath("//*[@id='mainsect']/tbody/tr[11]/td[1]")).Text;
                    PresProp_Penalty = driver.FindElement(By.XPath("//*[@id='mainsect']/tbody/tr[12]/td[1]")).Text;
                    Total_Penalties = driver.FindElement(By.XPath("//*[@id='mainsect']/tbody/tr[13]/td[1]")).Text;
                    TotalAssessed_Value = driver.FindElement(By.XPath("//*[@id='mainsect']/tbody/tr[14]/td[1]")).Text;
                    Homeowner_Exemption = driver.FindElement(By.XPath("//*[@id='mainsect']/tbody/tr[15]/td[1]")).Text;
                    Special_Exemptions = driver.FindElement(By.XPath("//*[@id='mainsect']/tbody/tr[16]/td[1]")).Text;
                    Net_Value = driver.FindElement(By.XPath("//*[@id='mainsect']/tbody/tr[17]/td[1]")).Text;

                    Assessment_Details = Land_Value + "~" + Improvement_Value + "~" + Improvement_Penalty + "~" + PresProp_Value + "~" + PresProp_Penalty + "~" + Total_Penalties + "~" + TotalAssessed_Value + "~" + Homeowner_Exemption + "~" + Special_Exemptions + "~" + Net_Value;
                    gc.CreatePdf(orderNumber, Parcelno, "Assessment Details", driver, "CA", "San Bernardino");
                    gc.insert_date(orderNumber, Parcelno, 559, Assessment_Details, 1, DateTime.Now);
                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");

                    driver.Navigate().GoToUrl("http://www.mytaxcollector.com/trSearch.aspx");
                    Thread.Sleep(2000);

                    driver.FindElement(By.XPath("/html/body/table/tbody/tr[3]/td[3]/form/table/tbody/tr[6]/td[2]/input")).SendKeys(Parcelno);

                    driver.FindElement(By.Id("ctl00_contentHolder_cmdSearch")).SendKeys(Keys.Enter);
                    Thread.Sleep(2000);

                    driver.FindElement(By.XPath("//*[@id='ctl00_menuHolder_trLeftNav_LeftNavMenuControln9']/td/table/tbody/tr/td/a")).Click();
                    Thread.Sleep(2000);

                    //Tax Payment History Details

                    IWebElement PaymentTB = driver.FindElement(By.XPath("/html/body/table/tbody/tr[3]/td[3]/div[1]/table/tbody"));
                    IList<IWebElement> PaymentTR = PaymentTB.FindElements(By.TagName("tr"));
                    IList<IWebElement> PaymentTD;

                    foreach (IWebElement Payment in PaymentTR)
                    {
                        PaymentTD = Payment.FindElements(By.TagName("td"));
                        if (PaymentTD.Count != 0 && !Payment.Text.Contains("Bill Number"))
                        {
                            Bill_Number = PaymentTD[0].Text;
                            Eff_Date = PaymentTD[1].Text;
                            Extnd_Date = PaymentTD[2].Text;
                            Correction = PaymentTD[3].Text;
                            Remarks = PaymentTD[4].Text;

                            Payment_details = Bill_Number + "~" + Eff_Date + "~" + Extnd_Date + "~" + Correction + "~" + Remarks;
                            gc.CreatePdf(orderNumber, Parcelno, "Tax Paymnt Details", driver, "CA", "San Bernardino");
                            gc.insert_date(orderNumber, Parcelno, 560, Payment_details, 1, DateTime.Now);
                        }
                    }

                    //Current Tax Details
                    driver.FindElement(By.XPath("//*[@id='ctl00_menuHolder_trLeftNav_LeftNavMenuControln10']/td/table/tbody/tr/td/a")).Click();
                    Thread.Sleep(2000);

                    List<string> secure = new List<string>();
                    List<string> Unsecure = new List<string>();
                    try
                    {
                        Bill_Type = driver.FindElement(By.XPath("/html/body/table/tbody/tr[3]/td[3]/form/div[1]/div")).Text;

                        IWebElement Receipttable = driver.FindElement(By.XPath("/html/body/table/tbody/tr[3]/td[3]/form/div[1]/table[3]/tbody"));
                        IList<IWebElement> ReceipttableRow = Receipttable.FindElements(By.TagName("tr"));
                        IList<IWebElement> ReceipttableTD;

                        foreach (IWebElement Receipt in ReceipttableRow)
                        {
                            ReceipttableTD = Receipt.FindElements(By.TagName("td"));
                            if (ReceipttableTD.Count != 0 && Receipt.Text.Contains("Installment"))
                            {
                                IWebElement Bill_link = ReceipttableTD[0].FindElement(By.TagName("a"));
                                url = Bill_link.GetAttribute("href");
                                secure.Add(url);
                            }
                            if (ReceipttableTD.Count != 0 && !Receipt.Text.Contains("Bill Number") && !Receipt.Text.Contains("Total:"))
                            {
                                Penalty = ReceipttableTD[2].Text;
                                Cost = ReceipttableTD[3].Text;
                                Inst_Del = ReceipttableTD[6].Text;

                                CurrentTax_Details = Bill_Type + "~" + Penalty + "~" + Cost + "~" + Inst_Del + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-";
                                gc.CreatePdf(orderNumber, Parcelno, "Secured Tax Bills Details", driver, "CA", "San Bernardino");
                                gc.insert_date(orderNumber, Parcelno, 561, CurrentTax_Details, 1, DateTime.Now);
                            }
                        }

                        //Deliquent taxes
                        try
                        {
                            bill_type = driver.FindElement(By.XPath("/html/body/table/tbody/tr[3]/td[3]/form/div[2]/table[1]/tbody/tr/th/center/font/a")).Text;

                            IWebElement Unsecuredtable = driver.FindElement(By.XPath("/html/body/table/tbody/tr[3]/td[3]/form/div[2]/table[2]/tbody"));
                            IList<IWebElement> UnsecuredRow = Unsecuredtable.FindElements(By.TagName("tr"));
                            IList<IWebElement> UnsecuredTD;

                            foreach (IWebElement Unsecured in UnsecuredRow)
                            {
                                UnsecuredTD = Unsecured.FindElements(By.TagName("td"));
                                if (UnsecuredTD.Count != 0 && Unsecured.Text.Contains("*"))
                                {
                                    IWebElement UnBill_link = UnsecuredTD[0].FindElement(By.TagName("a"));
                                    Unurl = UnBill_link.GetAttribute("href");
                                    Unsecure.Add(Unurl);
                                }
                                if (UnsecuredTD.Count != 0 && !Unsecured.Text.Contains("Bill Number") && !Unsecured.Text.Contains("Responsible Parties"))
                                {
                                    Lien = UnsecuredTD[3].Text;
                                    Intrest = UnsecuredTD[5].Text;
                                    Unpenalty = UnsecuredTD[6].Text;
                                    Uncost = UnsecuredTD[7].Text;
                                    Redemption = UnsecuredTD[8].Text;

                                    UnCurrentTax_Details = bill_type + "~" + Lien + "~" + Intrest + "~" + Unpenalty + "~" + Uncost + "~" + Redemption + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-";
                                    gc.CreatePdf(orderNumber, Parcelno, "UnSecured Tax Bills Details", driver, "CA", "San Bernardino");
                                    gc.insert_date(orderNumber, Parcelno, 566, UnCurrentTax_Details, 1, DateTime.Now);
                                }
                            }
                        }
                        catch
                        { }

                        foreach (string bill in secure)
                        {
                            driver.Navigate().GoToUrl(bill);
                            Thread.Sleep(3000);
                            try
                            {
                                Billed_Owner = driver.FindElement(By.XPath("/html/body/table/tbody/tr[3]/td[3]/table[2]/tbody/tr[1]/td/table/tbody/tr[2]/td[2]")).Text;
                                try
                                {
                                    Co_Owner = driver.FindElement(By.XPath("/html/body/table/tbody/tr[3]/td[3]/table[2]/tbody/tr[1]/td/table/tbody/tr[3]/td[2]")).Text;
                                }
                                catch { }
                                Default_date = driver.FindElement(By.XPath("/html/body/table/tbody/tr[3]/td[3]/table[2]/tbody/tr[3]/td/table/tbody/tr[1]/td[2]")).Text;
                                Tax_Type = driver.FindElement(By.XPath("/html/body/table/tbody/tr[3]/td[3]/table[2]/tbody/tr[3]/td/table/tbody/tr[1]/td[3]")).Text;
                                TaxBill_Number = driver.FindElement(By.XPath("/html/body/table/tbody/tr[3]/td[3]/table[2]/tbody/tr[3]/td/table/tbody/tr[2]/td[1]")).Text;
                                Extend_date = driver.FindElement(By.XPath("/html/body/table/tbody/tr[3]/td[3]/table[2]/tbody/tr[3]/td/table/tbody/tr[2]/td[2]")).Text;
                                Effective_date = driver.FindElement(By.XPath("/html/body/table/tbody/tr[3]/td[3]/table[2]/tbody/tr[3]/td/table/tbody/tr[2]/td[3]")).Text;
                                Taxrate_Year = driver.FindElement(By.XPath("/html/body/table/tbody/tr[3]/td[3]/table[2]/tbody/tr[3]/td/table/tbody/tr[4]/td[3]")).Text;

                                IWebElement valuetableElement = driver.FindElement(By.XPath("/html/body/table/tbody/tr[3]/td[3]/table[2]/tbody/tr[5]/td/table/tbody/tr/td[2]/table/tbody"));
                                IList<IWebElement> valuetableRow = valuetableElement.FindElements(By.TagName("tr"));
                                IList<IWebElement> valuerowTD;

                                List<string> Installment1 = new List<string>();
                                List<string> Due_Amt1 = new List<string>();
                                List<string> Delq_Amt1 = new List<string>();
                                List<string> Due_Date1 = new List<string>();
                                List<string> Pmt_Posted1 = new List<string>();
                                List<string> Installment2 = new List<string>();
                                List<string> Due_Amt2 = new List<string>();
                                List<string> Delq_Amt2 = new List<string>();
                                List<string> Due_Date2 = new List<string>();
                                List<string> Total_Tax = new List<string>();
                                List<string> Pay_Status = new List<string>();

                                int j = 0;
                                foreach (IWebElement row in valuetableRow)
                                {
                                    valuerowTD = row.FindElements(By.TagName("td"));
                                    if (j == 0)
                                    {
                                        Installment1.Add(valuerowTD[0].Text);
                                    }
                                    else if (j == 1)
                                    {
                                        Due_Amt1.Add(valuerowTD[0].Text);
                                    }
                                    else if (j == 2)
                                    {
                                        Delq_Amt1.Add(valuerowTD[0].Text);
                                    }
                                    else if (j == 3)
                                    {
                                        Due_Date1.Add(valuerowTD[0].Text);
                                    }
                                    else if (j == 4)
                                    {
                                        Pmt_Posted1.Add(valuerowTD[0].Text);
                                    }
                                    else if (j == 5)
                                    {
                                        Installment2.Add(valuerowTD[0].Text);
                                    }
                                    else if (j == 6)
                                    {
                                        Due_Amt2.Add(valuerowTD[0].Text);
                                    }
                                    else if (j == 7)
                                    {
                                        Delq_Amt2.Add(valuerowTD[0].Text);
                                    }
                                    else if (j == 8)
                                    {
                                        Due_Date2.Add(valuerowTD[0].Text);
                                    }
                                    else if (j == 9)
                                    {
                                        Total_Tax.Add(valuerowTD[0].Text);
                                    }
                                    else if (j == 10)
                                    {
                                        Pay_Status.Add(valuerowTD[0].Text);
                                    }
                                    j++;
                                }

                                SecuredCurrentTax_Details = "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + Billed_Owner + "~" + Co_Owner + "~" + Default_date + "~" + Tax_Type + "~" + TaxBill_Number + "~" + Extend_date + "~" + Effective_date + "~" + Taxrate_Year + "~" + Installment1[0] + "~" + Due_Amt1[0] + "~" + Delq_Amt1[0] + "~" + Due_Date1[0] + "~" + Pmt_Posted1[0] + "~" + Installment2[0] + "~" + Due_Amt2[0] + "~" + Delq_Amt2[0] + "~" + Due_Date2[0] + "~" + Total_Tax[0] + "~" + Pay_Status[0];
                                gc.CreatePdf(orderNumber, Parcelno, "Secured Bills Details", driver, "CA", "San Bernardino");
                                gc.insert_date(orderNumber, Parcelno, 561, SecuredCurrentTax_Details, 1, DateTime.Now);
                            }
                            catch
                            { }
                        }
                    }
                    catch
                    { }

                    try
                    {
                        foreach (string Unbill in Unsecure)
                        {
                            driver.Navigate().GoToUrl(Unbill);
                            Thread.Sleep(3000);

                            UnBilled_Owner = driver.FindElement(By.XPath("/html/body/table/tbody/tr[3]/td[3]/table[2]/tbody/tr[1]/td/table/tbody/tr[2]/td[2]")).Text;
                            UnCo_Owner = driver.FindElement(By.XPath("/html/body/table/tbody/tr[3]/td[3]/table[2]/tbody/tr[1]/td/table/tbody/tr[3]/td[2]")).Text;
                            UnDefault_Date = driver.FindElement(By.XPath("/html/body/table/tbody/tr[3]/td[3]/table[2]/tbody/tr[3]/td/table/tbody/tr[1]/td[2]")).Text;
                            UnTax_Type = driver.FindElement(By.XPath("/html/body/table/tbody/tr[3]/td[3]/table[2]/tbody/tr[3]/td/table/tbody/tr[1]/td[3]")).Text;
                            UnBill_Number = driver.FindElement(By.XPath("/html/body/table/tbody/tr[3]/td[3]/table[2]/tbody/tr[3]/td/table/tbody/tr[2]/td[1]")).Text;
                            UnExtended_Date = driver.FindElement(By.XPath("/html/body/table/tbody/tr[3]/td[3]/table[2]/tbody/tr[3]/td/table/tbody/tr[2]/td[2]")).Text;
                            UnEffective_Owner = driver.FindElement(By.XPath("/html/body/table/tbody/tr[3]/td[3]/table[2]/tbody/tr[3]/td/table/tbody/tr[2]/td[3]")).Text;
                            UnTaxrate_Year = driver.FindElement(By.XPath("/html/body/table/tbody/tr[3]/td[3]/table[2]/tbody/tr[3]/td/table/tbody/tr[4]/td[3]")).Text;

                            IWebElement UnvaluetableElement = driver.FindElement(By.XPath("/html/body/table/tbody/tr[3]/td[3]/table[2]/tbody/tr[5]/td/table/tbody/tr/td[2]/table/tbody"));
                            IList<IWebElement> UnvaluetableRow = UnvaluetableElement.FindElements(By.TagName("tr"));
                            IList<IWebElement> UnvaluerowTD;

                            List<string> UnInstallment = new List<string>();
                            List<string> UnDue_Amt = new List<string>();
                            List<string> UnDelq_Amt = new List<string>();
                            List<string> UnDue_Date = new List<string>();
                            List<string> UnTotal_Tax = new List<string>();
                            List<string> UnPay_Status = new List<string>();


                            int K = 0;
                            foreach (IWebElement Unrow in UnvaluetableRow)
                            {
                                UnvaluerowTD = Unrow.FindElements(By.TagName("td"));
                                if (K == 0)
                                {
                                    UnInstallment.Add(UnvaluerowTD[0].Text);
                                }
                                else if (K == 1)
                                {
                                    UnDue_Amt.Add(UnvaluerowTD[0].Text);
                                }
                                else if (K == 2)
                                {
                                    UnDelq_Amt.Add(UnvaluerowTD[0].Text);
                                }
                                else if (K == 3)
                                {
                                    UnDue_Date.Add(UnvaluerowTD[0].Text);
                                }
                                else if (K == 4)
                                {
                                    UnTotal_Tax.Add(UnvaluerowTD[0].Text);
                                }
                                else if (K == 5)
                                {
                                    UnPay_Status.Add(UnvaluerowTD[0].Text);
                                }
                                K++;
                            }
                            UnSecuredCurrentTax_Details = "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + UnBilled_Owner + "~" + UnCo_Owner + "~" + UnDefault_Date + "~" + UnTax_Type + "~" + UnBill_Number + "~" + UnExtended_Date + "~" + UnEffective_Owner + "~" + UnTaxrate_Year + "~" + UnInstallment[0] + "~" + UnDue_Amt[0] + "~" + UnDelq_Amt[0] + "~" + UnDue_Date[0] + "~" + UnTotal_Tax[0] + "~" + UnPay_Status[0];
                            gc.CreatePdf(orderNumber, Parcelno, "UnSecured Bills Details", driver, "CA", "San Bernardino");
                            gc.insert_date(orderNumber, Parcelno, 566, UnSecuredCurrentTax_Details, 1, DateTime.Now);
                        }
                    }
                    catch
                    { }

                    TaxTime = DateTime.Now.ToString("HH:mm:ss");
                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "CA", "San Bernardino", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    gc.mergpdf(orderNumber, "CA", "San Bernardino");
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