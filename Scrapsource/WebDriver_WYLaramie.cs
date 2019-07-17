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
using OpenQA.Selenium.Firefox;
using System.Diagnostics;
using OpenQA.Selenium.Remote;

namespace ScrapMaricopa.Scrapsource
{
    public class WebDriver_WYLaramie
    {
        string Outparcelno = "", outputPath = "";
        string strMultiCount = "", strMultiAdddress = "", strParcelId = "-", strParcelNumber = "", strProperty = "", strAddress = "", strOwner = "", strTaxId = "", strTaxDistrict = "", strPropertyOwner = "",
               strStreetAddress = "", strLocation = "", strMarketValue = "", strAssessedValue = "", strAcres = "", strClass = "", strYearBuilt = "", strYear = "", strTaxAll = "", strTaxParcel = "",
               strTaxStatus = "", strTaxReceipt = "", strTaxOwner = "", strTaxDiatrict = "", strTMarketValue = "", strTTaxableValue = "", strTVetexempt = "", strTNetTaxable = "", strTFirstHalf = "",
               strTFirstDue = "", strTSecondHalf = "", strTSecondDue = "", strTTotal = "", strPFirstHalf = "", strPSecondHalf = "", strPTotal = "", strAuthority = "", strAuthorityAddress = "",
               strAuthorityPhone = "", strGeoCode = "", strPAddress = "", strLegal = "", strTaxYear = "", strTaxStatement = "", strTaxBillDate = "", strTaxBillAmount = "", strTaxDatePaid = "",
               strTaxPaidAmount = "", strTaxNotes = "", strTaxType = "", strUnpaidTaxYear = "", strUnpaidAmount = "", strUnpaidDiscount = "", strUnpaidDue = "", strCurrentYear = "", strPayTaxYear = "",
               strPayDueDate = "", strPaystatement = "", strPayHalf = "", strPayTaxAmount = "", strPayInterest = "", strPayPenalty = "", strPayDiscount = "", strPayTotalDue = "", strPayoff = "", strPayoffAmount = "",
               strDetail = "", strTaxDetails = "", strPayoffType = "", strUnPaidType = "", strValuesType = "", strTaxesType = "", strPaymentsType = "", strComment = "";
        int MultiCount, DetailCount, HistoryCount, classcount;
        List<string> strDeatilURL = new List<string>();
        IList<IWebElement> Idetail;
        IWebDriver driver;
        DBconnection db = new DBconnection();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        private TimeSpan timeOutInSeconds;
        MySqlParameter[] mParam;
        GlobalClass gc = new GlobalClass();
        public string FTP_WYLaramie(string houseno, string sname, string sttype, string unitno, string ownername, string parcelNumber, string searchType, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
            //var driverService = PhantomJSDriverService.CreateDefaultService();
            //driverService.HideCommandPromptWindow = true;
            // driver = new PhantomJSDriver();
            var option = new ChromeOptions();
            option.AddArgument("No-Sandbox");
            using (driver = new ChromeDriver(option))
            {
                StartTime = DateTime.Now.ToString("HH:mm:ss");
                try
                {
                    if (searchType == "titleflex")
                    {
                        string address = houseno + " " + sname + " " + sttype + " " + unitno;
                        gc.TitleFlexSearch(orderNumber, parcelNumber, "", address, "WY", "Laramie");
                        if (HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes")
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["WYLaramie_Nodata"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }

                    driver.Navigate().GoToUrl("https://maps.laramiecounty.com/mapserver/map?tab=search");
                    IWebElement IAdvSearch = driver.FindElement(By.XPath("//*[@id='search-tabs']/button[2]"));
                    IJavaScriptExecutor jsa = driver as IJavaScriptExecutor;
                    jsa.ExecuteScript("arguments[0].click();", IAdvSearch);
                    gc.CreatePdf_WOP(orderNumber, "Advance", driver, "WY", "Laramie");
                    if (searchType == "address")
                    {
                        driver.FindElement(By.Name("St_Num")).SendKeys(houseno);
                        try
                        {
                            IWebElement direction = driver.FindElement(By.XPath("//*[@id='addressSearch']/table/tbody/tr[2]/td[2]/select"));
                            SelectElement dir = new SelectElement(direction);
                            dir.SelectByValue(directParcel);
                        }
                        catch { }
                        driver.FindElement(By.Name("St_Name")).SendKeys(sname + " " + sttype);
                        gc.CreatePdf_WOP(orderNumber, "Address Search", driver, "WY", "Laramie");
                        IWebElement IAddressSearch = driver.FindElement(By.XPath("//*[@id='addressSearch']/table/tbody/tr[2]/td[4]/input"));
                        IJavaScriptExecutor js = driver as IJavaScriptExecutor;
                        js.ExecuteScript("arguments[0].click();", IAddressSearch);
                        Thread.Sleep(3000);
                        gc.CreatePdf_WOP(orderNumber, "Address Search Result", driver, "WY", "Laramie");

                        try
                        {
                            string nodata = driver.FindElement(By.XPath("//*[@id='search_results']/p/font")).Text;
                            if (nodata.Contains("no matching"))
                            {
                                HttpContext.Current.Session["WYLaramie_Nodata"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch
                        { }

                        try
                        {
                            IWebElement IMultiCount = driver.FindElement(By.XPath("//*[@id='search_results']/p"));
                            strMultiCount = GlobalClass.Before(IMultiCount.Text, " Record(s) Found.");
                            if (Convert.ToInt32(strMultiCount) >= 25)
                            {
                                HttpContext.Current.Session["multiparcel_WYLaramie_Count"] = "Maximum";
                                driver.Quit();
                                return "Maximum";
                            }

                            IWebElement IMulti1 = driver.FindElement(By.XPath("//*[@id='form1']/table/tbody"));
                            IList<IWebElement> IMultiRow1 = IMulti1.FindElements(By.TagName("tr"));
                            foreach (IWebElement multi in IMultiRow1)
                            {
                                //HttpContext.Current.Session["multiparcel_WYLaramie"] = "Yes";
                                if (MultiCount <= 25)
                                {
                                    js.ExecuteScript("arguments[0].click();", multi);
                                    try
                                    {
                                        strMultiAdddress = driver.FindElement(By.XPath("//*[@id='propdetail']/table/tbody/tr[2]/td/table/tbody")).Text;
                                        strParcelId = gc.Between(strMultiAdddress, "Parcel\r\nPIDN: ", "\r\nTax ID: ");
                                        strAddress = gc.Between(strMultiAdddress, "Street Address: ", "\r\nDeed: ");
                                        strOwner = gc.Between(strMultiAdddress, "Property Owner(s):  ", "\r\nMailing Address: ");


                                        string strMultiDetails = strAddress + "~" + strOwner;
                                        gc.insert_date(orderNumber, strParcelId, 468, strMultiDetails, 1, DateTime.Now);
                                    }
                                    catch { }
                                    MultiCount++;
                                }
                            }
                            driver.Quit();
                            return "MultiParcel";
                        }
                        catch { }
                    }

                    if (searchType == "parcel")
                    {
                        driver.FindElement(By.XPath("//*[@id='numSearch']/table/tbody/tr/td[1]/input")).SendKeys(parcelNumber);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search", driver, "WY", "Laramie");
                        IWebElement IParcelSearch = driver.FindElement(By.XPath("//*[@id='numSearch']/table/tbody/tr/td[2]/input"));
                        IJavaScriptExecutor js = driver as IJavaScriptExecutor;
                        js.ExecuteScript("arguments[0].click();", IParcelSearch);
                        Thread.Sleep(3000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search Result", driver, "WY", "Laramie");
                    }


                    if (searchType == "ownername")
                    {
                        string[] strOwnerName = ownername.Replace(",", "").Split(' ');
                        if (strOwnerName.Length == 2)
                        {
                            driver.FindElement(By.XPath("//*[@id='ownerSearch']/table/tbody/tr/td[1]/input")).SendKeys(strOwnerName[1]);
                            driver.FindElement(By.XPath("//*[@id='ownerSearch']/table/tbody/tr/td[2]/input")).SendKeys(strOwnerName[0]);
                        }
                        if (strOwnerName.Length == 1)
                        {
                            driver.FindElement(By.XPath("//*[@id='ownerSearch']/table/tbody/tr/td[2]/input")).SendKeys(strOwnerName[0]);
                        }
                        gc.CreatePdf_WOP(orderNumber, "Owner Search", driver, "WY", "Laramie");
                        IWebElement IOwnerSearch = driver.FindElement(By.XPath("//*[@id='ownerSearch']/table/tbody/tr/td[3]/input"));
                        IJavaScriptExecutor js = driver as IJavaScriptExecutor;
                        js.ExecuteScript("arguments[0].click();", IOwnerSearch);
                        Thread.Sleep(3000);
                        gc.CreatePdf_WOP(orderNumber, "Owner Search Result", driver, "WY", "Laramie");

                        try
                        {
                            IWebElement IMultiCount = driver.FindElement(By.XPath("//*[@id='search_results']/p"));
                            strMultiCount = GlobalClass.Before(IMultiCount.Text, " Record(s) Found.");
                            if (Convert.ToInt32(strMultiCount) >= 25)
                            {
                                HttpContext.Current.Session["multiparcel_WYLaramie_Count"] = "Maximum";
                                return "Maximum";
                            }

                            IWebElement IMulti1 = driver.FindElement(By.XPath("//*[@id='form1']/table/tbody"));
                            IList<IWebElement> IMultiRow1 = IMulti1.FindElements(By.LinkText("Detail"));
                            foreach (IWebElement multi in IMultiRow1)
                            {
                                HttpContext.Current.Session["multiparcel_WYLaramie"] = "Yes";
                                if (MultiCount <= 25)
                                {
                                    js.ExecuteScript("arguments[0].click();", multi);
                                    try
                                    {
                                        strMultiAdddress = driver.FindElement(By.XPath("//*[@id='propdetail']/table/tbody/tr[2]/td/table/tbody")).Text;
                                        strParcelId = gc.Between(strMultiAdddress, "Parcel\r\nPIDN: ", "\r\nTax ID: ");
                                        strAddress = gc.Between(strMultiAdddress, "Street Address: ", "\r\nDeed: ");
                                        strOwner = gc.Between(strMultiAdddress, "Property Owner(s):  ", "\r\nMailing Address: ");


                                        string strMultiDetails = strAddress + "~" + strOwner;
                                        gc.insert_date(orderNumber, strParcelId, 468, strMultiDetails, 1, DateTime.Now);
                                    }
                                    catch { }
                                    MultiCount++;
                                }
                            }
                            driver.Quit();
                            return "MultiParcel";
                        }
                        catch { }
                    }

                    try
                    {
                        string nodata = driver.FindElement(By.XPath("//*[@id='search_results']/p/font")).Text;
                        if (nodata.Contains("no matching"))
                        {
                            HttpContext.Current.Session["WYLaramie_Nodata"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }

                    //Property Details 
                    IWebElement ISearch = driver.FindElement(By.Id("infotool"));
                    IList<IWebElement> IlSearch = ISearch.FindElements(By.TagName("a"));
                    IList<IWebElement> IlTD;

                    foreach (IWebElement serach in IlSearch)
                    {
                        if (IlSearch.Count != 0 && serach.Text.Contains("Property"))
                        {
                            serach.SendKeys(Keys.Enter);
                        }
                    }

                    Thread.Sleep(5000);

                    strProperty = driver.FindElement(By.XPath("//*[@id='propdetail']/table/tbody/tr[2]/td/table/tbody")).Text;
                    // strParcelNumber = gc.Between(strProperty, "Parcel\r\nPIDN: ", "\r\nTax ID: ");

                    try
                    {

                        int i = 0;
                        IWebElement tbmulti11 = driver.FindElement(By.XPath("//*[@id='propdetail']/table/tbody/tr[2]/td/table/tbody"));
                        IList<IWebElement> TRmulti11 = tbmulti11.FindElements(By.TagName("tr"));
                        IList<IWebElement> TDmulti11;
                        foreach (IWebElement row in TRmulti11)
                        {

                            TDmulti11 = row.FindElements(By.TagName("td"));
                            if (TDmulti11.Count != 0)
                            {
                                if (i == 1)
                                    strParcelNumber = TDmulti11[1].Text;
                                if (i == 2)
                                    strTaxId = TDmulti11[1].Text;
                                if (i == 3)
                                    strTaxDistrict = TDmulti11[1].Text;
                                if (i == 4)
                                    strPropertyOwner = TDmulti11[1].Text;
                                if (i == 6)
                                    strStreetAddress = TDmulti11[1].Text;
                                if (i == 8)
                                    strLocation = TDmulti11[1].Text;
                                if (i == 9)
                                    strMarketValue = TDmulti11[1].Text;
                                if (i == 10)
                                    strAssessedValue = TDmulti11[1].Text;


                                i++;
                            }


                        }

                    }
                    catch { }







                    // strTaxId = gc.Between(strProperty, "\r\nTax ID: ", " Property Taxes\r\nTax District:");
                    //   strTaxDistrict = gc.Between(strProperty, " Property Taxes\r\nTax District: ", "\r\nProperty Owner(s): ");
                    //  strPropertyOwner = gc.Between(strProperty, "\r\nProperty Owner(s):  ", "\r\nMailing Address: ");
                    //  try
                    //   {
                    //    strStreetAddress = gc.Between(strProperty, "Street Address:  ", "\r\nDeed: ");
                    // }
                    // catch { }
                    //  try
                    //   {
                    //   if(strStreetAddress == "")
                    //  {
                    //    strStreetAddress = gc.Between(strProperty, "Street Address:  ", "\r\nLocation: ");
                    //  }
                    // }
                    //  catch { }
                    // strLocation = gc.Between(strProperty, "Location:  ", "\r\n2018 Market Value:  ");
                    // strMarketValue = gc.Between(strProperty, "2018 Market Value:  ", "2018 Assessed Value: ").TrimStart().TrimEnd().Trim();
                    // strAssessedValue = gc.Between(strProperty, "2018 Assessed Value:  ", "The ").TrimStart().TrimEnd().Trim();

                    try
                    {
                        strYear = gc.Between(strProperty, " Improvements)\r\n", " Assessed Value: ");
                    }
                    catch { }

                    try
                    {
                        strYearBuilt = driver.FindElement(By.XPath("//*[@id='propdetail']/table/tbody/tr[3]/td/table/tbody/tr[2]/td[6]")).Text;

                    }
                    catch (Exception ex) { }
                    try
                    {
                        strYearBuilt = driver.FindElement(By.XPath("//*[@id='propdetail']/table/tbody/tr[4]/td/table/tbody/tr[2]/td[6]")).Text;

                    }
                    catch { }

                    try
                    {
                        IWebElement ILandTable = driver.FindElement(By.XPath("//*[@id='propdetail']/table/tbody/tr[3]/td/table/tbody"));
                        IList<IWebElement> ILandRow = ILandTable.FindElements(By.TagName("tr"));
                        IList<IWebElement> ILandTD;
                        foreach (IWebElement land in ILandRow)
                        {
                            try
                            {
                                ILandTD = land.FindElements(By.TagName("td"));
                                if (ILandTD.Count != 0 && !land.Text.Contains("Land") && !land.Text.Contains("Total") && land.Text.Contains("Year Built*"))
                                {
                                    break;
                                }
                                if (ILandTD.Count != 0 && !land.Text.Contains("Land") && land.Text.Contains("Total"))
                                {
                                    strAcres = ILandTD[0].Text;
                                }
                                if (ILandTD.Count != 0 && land.Text.Trim() != "Land" && !land.Text.Contains("Total") && !land.Text.Contains("Class") && classcount < 1)
                                {
                                    strClass = ILandTD[2].Text;
                                    classcount++;
                                }
                            }
                            catch { }
                        }
                    }
                    catch { }

                    string strPropertyDetails = strTaxId + "~" + strTaxDistrict + "~" + strPropertyOwner + "~" + strStreetAddress + "~" + strLocation + "~" + strYear + "~" + strMarketValue + "~" + strAssessedValue + "~" + strAcres + "~" + strClass + "~" + strYearBuilt;
                    gc.insert_date(orderNumber, strParcelNumber, 476, strPropertyDetails, 1, DateTime.Now);

                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");

                    gc.CreatePdf(orderNumber, strParcelNumber, "Tax Details Result", driver, "WY", "Laramie");

                    IWebElement ITaxSearch = driver.FindElement(By.XPath("//*[@id='propdetail']/table/tbody/tr[2]/td/table/tbody/tr[3]/td[2]/a"));
                    string strTaxSearch = ITaxSearch.GetAttribute("href");
                    driver.Navigate().GoToUrl(strTaxSearch);

                    IWebElement ITaxDetail = driver.FindElement(By.XPath("//*[@id='Table2']/tbody"));
                    strTaxAll = ITaxDetail.Text;
                    strTaxParcel = gc.Between(strTaxAll, "Parcel Number:", "\r\n\r\nStatus:").Trim();
                    gc.CreatePdf(orderNumber, strParcelNumber, "Property Details Result", driver, "WY", "Laramie");
                    strTaxStatus = gc.Between(strTaxAll, "Status:", "Receipt:").Trim();
                    string strTaxRec = gc.Between(strTaxAll, "Receipt:", "Owner(s):").Trim();
                    strTaxReceipt = strTaxRec.Remove(strTaxRec.Length - 4);
                    strTaxOwner = gc.Between(strTaxAll, "Owner(s):", "Mailing Address:");
                    strTaxDiatrict = GlobalClass.After(strTaxAll, "Levy District:\r\n");

                    try
                    {
                        strValuesType = driver.FindElement(By.Id("_ctl0_ContentPlaceHolder1_lblYearValue")).Text.Replace(":", "");
                        strTaxesType = driver.FindElement(By.Id("_ctl0_ContentPlaceHolder1_lblYearTaxes")).Text.Replace(":", "");
                        strPaymentsType = driver.FindElement(By.Id("_ctl0_ContentPlaceHolder1_lblYearPayments")).Text.Replace(":", "");
                        strComment = driver.FindElement(By.Id("_ctl0_ContentPlaceHolder1_Label20")).Text;
                    }
                    catch { }

                    IWebElement ITaxDetailTable = driver.FindElement(By.Id("Table1"));
                    Idetail = ITaxDetailTable.FindElements(By.TagName("a"));
                    foreach (IWebElement detail in Idetail)
                    {
                        strDetail = detail.GetAttribute("id");
                        strDeatilURL.Add(strDetail);
                    }
                    IWebElement ITaxValueTable = driver.FindElement(By.XPath("//*[@id='Table3']/tbody"));
                    IList<IWebElement> ITaxValueRow = ITaxValueTable.FindElements(By.TagName("table"));
                    IList<IWebElement> ITaxValueTD;
                    IList<IWebElement> ITaxValuePay;
                    foreach (IWebElement value in ITaxValueRow)
                    {
                        ITaxValueTD = value.FindElements(By.TagName("tr"));
                        if (ITaxValueTD.Count != 0 && (value.Text.Contains("Market: ") && strValuesType.Contains("Value")))
                        {
                            strTMarketValue = GlobalClass.After(ITaxValueTD[0].Text, "Market: ");
                            strTTaxableValue = GlobalClass.After(ITaxValueTD[1].Text, "Taxable:   ");
                            strTVetexempt = GlobalClass.After(ITaxValueTD[3].Text, "Vet Exempt:   ");
                            strTNetTaxable = GlobalClass.After(ITaxValueTD[5].Text, "Net Taxable:  ");

                            string strTaxValueDetails = strValuesType + "~" + strTMarketValue + "~" + strTTaxableValue + "~" + strTVetexempt + "~" + strTNetTaxable + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-";
                            gc.insert_date(orderNumber, strTaxParcel, 519, strTaxValueDetails, 1, DateTime.Now);
                        }
                        else if (ITaxValueTD.Count != 0 && (value.Text.Contains("First Half: ") && value.Text.Contains("Due: ")) && strTaxesType.Contains("Taxes"))
                        {
                            strTFirstHalf = gc.Between(ITaxValueTD[0].Text, "First Half: ", "Due:  ");
                            strTFirstDue = GlobalClass.After(ITaxValueTD[0].Text, "Due:  ");
                            strTSecondHalf = gc.Between(ITaxValueTD[1].Text, "Second Half: ", "Due:  ");
                            strTSecondDue = GlobalClass.After(ITaxValueTD[1].Text, "Due:  ");
                            strTTotal = GlobalClass.After(ITaxValueTD[2].Text, "Total: ");

                            string strTaxesDetails = strTaxesType + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + strTFirstHalf + "~" + strTFirstDue + "~" + strTSecondHalf + "~" + strTSecondDue + "~" + strTTotal;
                            gc.insert_date(orderNumber, strTaxParcel, 519, strTaxesDetails, 1, DateTime.Now);
                        }
                        else if (ITaxValueTD.Count != 0 && value.Text.Contains("First Half: ") && strPaymentsType.Contains("Payments"))
                        {
                            strPFirstHalf = GlobalClass.After(ITaxValueTD[0].Text, "First Half: ");
                            strPSecondHalf = GlobalClass.After(ITaxValueTD[1].Text, "Second Half: ");
                            strPTotal = GlobalClass.After(ITaxValueTD[3].Text, "Total: ");

                            string strTaxPaymentDetails = strPaymentsType + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + strPFirstHalf + "~" + "-" + "~" + strPSecondHalf + "~" + "-" + "~" + strPTotal + " " + strComment;
                            gc.insert_date(orderNumber, strTaxParcel, 519, strTaxPaymentDetails, 1, DateTime.Now);
                        }


                    }

                    IWebElement Ilegal = driver.FindElement(By.Id("_ctl0_ContentPlaceHolder1_Panel5"));
                    strGeoCode = gc.Between(Ilegal.Text, "Geo Code: ", "\r\nProperty address: ");
                    strPAddress = gc.Between(Ilegal.Text, "Property address: ", "\r\nLegal:");
                    strLegal = GlobalClass.After(Ilegal.Text, "Legal: ");

                    IWebElement IHistory = driver.FindElement(By.XPath("//*[@id='_ctl0_ContentPlaceHolder1_linkHistory']/img"));
                    IHistory.Click();
                    gc.CreatePdf(orderNumber, strParcelNumber, "Tax History Result", driver, "WY", "Laramie");
                    strTaxType = gc.Between(driver.FindElement(By.Id("_ctl0_ContentPlaceHolder1_Panel1")).Text, "Type:  ", "Owner");
                    IWebElement IHistoryTable = driver.FindElement(By.XPath("//*[@id='_ctl0_ContentPlaceHolder1_dgHistory']/tbody"));
                    IList<IWebElement> IHistoryRow = IHistoryTable.FindElements(By.TagName("tr"));
                    IList<IWebElement> IHistoryTD;
                    foreach (IWebElement History in IHistoryRow)
                    {
                        IHistoryTD = History.FindElements(By.TagName("td"));
                        if (IHistoryTD.Count != 0 && !History.Text.Contains("Tax Year"))
                        {
                            try
                            {
                                strTaxYear = IHistoryTD[0].Text;
                                strTaxStatement = IHistoryTD[1].Text;
                                strTaxBillDate = IHistoryTD[2].Text;
                                strTaxBillAmount = IHistoryTD[3].Text;
                                strTaxDatePaid = IHistoryTD[4].Text;
                                strTaxPaidAmount = IHistoryTD[5].Text;
                                strTaxNotes = IHistoryTD[6].Text;
                            }
                            catch { }

                            string strHistoryDetails = strTaxYear + "~" + strTaxStatement + "~" + strTaxBillDate + "~" + strTaxBillAmount + "~" + strTaxDatePaid + "~" + strTaxPaidAmount + "~" + strTaxNotes;
                            gc.insert_date(orderNumber, strTaxParcel, 520, strHistoryDetails, 1, DateTime.Now);
                        }
                    }

                    driver.Navigate().Back();

                    IWebElement IPayOff = driver.FindElement(By.XPath("//*[@id='_ctl0_ContentPlaceHolder1_linkPayoff']/img"));
                    IPayOff.Click();
                    gc.CreatePdf(orderNumber, strParcelNumber, "Tax Pay Off Result", driver, "WY", "Laramie");

                    driver.Navigate().Back();

                    IWebElement IPayTaxes = driver.FindElement(By.XPath("//*[@id='_ctl0_ContentPlaceHolder1_linkPayTaxes']/img"));
                    IPayTaxes.Click();
                    gc.CreatePdf(orderNumber, strParcelNumber, "Tax Pay Taxes Result", driver, "WY", "Laramie");
                    try
                    {
                        strCurrentYear = GlobalClass.After(driver.FindElement(By.Id("_ctl0_ContentPlaceHolder1_pnlDetail")).Text, "Current Tax Year: ");
                    }
                    catch { }
                    try
                    {
                        strPayoffType = GlobalClass.Before(driver.FindElement(By.Id("_ctl0_ContentPlaceHolder1_lblUnpaidheader")).Text, "as of ");
                        strPayoff = gc.Between(driver.FindElement(By.Id("_ctl0_ContentPlaceHolder1_lblUnpaidheader")).Text, "Total payoff amount as of ", ":");
                        strPayoffAmount = driver.FindElement(By.Id("_ctl0_ContentPlaceHolder1_lblUnpaid")).Text;
                    }
                    catch { }
                    try
                    {
                        strUnPaidType = driver.FindElement(By.Id("_ctl0_ContentPlaceHolder1_Label5")).Text;
                    }
                    catch { }
                    IWebElement ITotalUnpaidTable = driver.FindElement(By.XPath("//*[@id='_ctl0_ContentPlaceHolder1_dgTotals']/tbody"));
                    IList<IWebElement> ITotalUnpaidRow = ITotalUnpaidTable.FindElements(By.TagName("tr"));
                    IList<IWebElement> ITotalUnpaidTD;
                    foreach (IWebElement unpaid in ITotalUnpaidRow)
                    {
                        ITotalUnpaidTD = unpaid.FindElements(By.TagName("td"));
                        if (ITotalUnpaidTD.Count != 0 && !unpaid.Text.Contains("Tax Year"))
                        {
                            strUnpaidTaxYear = ITotalUnpaidTD[0].Text;
                            strUnpaidAmount = ITotalUnpaidTD[1].Text;
                            strUnpaidDiscount = ITotalUnpaidTD[2].Text;
                            strUnpaidDue = ITotalUnpaidTD[3].Text;

                            string strTotalUnpaid = strUnPaidType.Replace(":", "") + "~" + strUnpaidTaxYear + "~" + "-" + "~" + "-" + "~" + "-" + "~" + strUnpaidAmount + "~" + "-" + "~" + "-" + "~" + strUnpaidDiscount + "~" + "-" + "~" + strUnpaidDue;
                            gc.insert_date(orderNumber, strTaxParcel, 521, strTotalUnpaid, 1, DateTime.Now);
                        }
                    }

                    string strTotalPayOff = strPayoffType + "~" + "-" + "~" + strPayoff + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + strPayoffAmount + "~" + "-";
                    gc.insert_date(orderNumber, strTaxParcel, 521, strTotalPayOff, 1, DateTime.Now);

                    IWebElement ITotalPayoffTable = driver.FindElement(By.XPath("//*[@id='_ctl0_ContentPlaceHolder1_dgPayoff']/tbody"));
                    IList<IWebElement> ITotalPayoffRow = ITotalPayoffTable.FindElements(By.TagName("tr"));
                    IList<IWebElement> ITotalPayoffTD;
                    foreach (IWebElement payoff in ITotalPayoffRow)
                    {
                        ITotalPayoffTD = payoff.FindElements(By.TagName("td"));
                        if (ITotalPayoffTD.Count != 0 && !payoff.Text.Contains("Tax Year"))
                        {
                            strPayTaxYear = ITotalPayoffTD[0].Text;
                            strPayDueDate = ITotalPayoffTD[1].Text;
                            strPaystatement = ITotalPayoffTD[2].Text;
                            strPayHalf = ITotalPayoffTD[3].Text;
                            strPayTaxAmount = ITotalPayoffTD[4].Text;
                            strPayInterest = ITotalPayoffTD[5].Text;
                            strPayPenalty = ITotalPayoffTD[6].Text;
                            strPayDiscount = ITotalPayoffTD[7].Text;
                            strPayTotalDue = ITotalPayoffTD[8].Text;

                            string strPayOffDetails = strPayoffType + "~" + strPayTaxYear + "~" + strPayDueDate + "~" + strPaystatement + "~" + strPayHalf + "~" + strPayTaxAmount + "~" + strPayInterest + "~" + strPayPenalty + "~" + strPayDiscount + "~" + "-" + "~" + strPayTotalDue;
                            gc.insert_date(orderNumber, strTaxParcel, 521, strPayOffDetails, 1, DateTime.Now);
                        }
                    }

                    try
                    {
                        driver.Navigate().GoToUrl("http://www.laramiecounty.com/_officials/CountyTreasurer/index.aspx");
                        Thread.Sleep(2000);
                        strAuthorityAddress = driver.FindElement(By.XPath("//*[@id='OneHalf']/p[6]")).Text.Replace("\r\n", " ").Trim();
                        strAuthorityPhone = driver.FindElement(By.XPath("//*[@id='OneHalf']/p[7]")).Text.Replace("\r\n", " ").Trim();
                        strAuthority = GlobalClass.After(strAuthorityAddress, "Property Tax: ") + " " + strAuthorityPhone;
                    }
                    catch { }

                    string strTaxAssessDetails = strTaxStatus + "~" + strTaxReceipt + "~" + strTaxOwner + "~" + strTaxDiatrict + "~" + strGeoCode + "~" + strPAddress + "~" + strLegal + "~" + strTaxType + "~" + strCurrentYear + "~" + strAuthority;
                    gc.insert_date(orderNumber, strTaxParcel, 477, strTaxAssessDetails, 1, DateTime.Now);

                    try
                    {
                        foreach (string strdetail in strDeatilURL)
                        {
                            driver.Navigate().GoToUrl(strTaxSearch);
                            driver.FindElement(By.Id(strdetail)).SendKeys(Keys.Enter);
                            gc.CreatePdf(orderNumber, strParcelNumber, "Tax Detail Result" + DetailCount, driver, "WY", "Laramie");
                            DetailCount++;
                        }
                    }
                    catch { }
                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "WY", "Laramie", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    gc.mergpdf(orderNumber, "WY", "Laramie");
                    return "Data Inserted Successfully";
                }

                catch (NoSuchElementException ex1)
                {
                    driver.Quit();
                    throw ex1;
                }
            }
        }
    }
}