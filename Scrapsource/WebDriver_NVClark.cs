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
    public class WebDriver_NVClark
    {
        string Outparcelno = "", outputPath = "";
        string strAddress = "-", strCity = "-", strOwner = "", strParcelId = "-", strMultiCount = "", strParcelNumber = "-", strOwnerMailingAddress = "-", strLocationAddress = "-", strAssesser = "-", strTaxDistrict = "-", strApprisalYear = "", strFiscalYear = "", strSupplImpValue = "", strIncreLand = "",
               strIncreImprove = "", strEstimatdSize = "", strOriginalYear = "", strLandUse = "-", strRealFiscalYear = "-", strLand = "", strImprovements = "", strPersonalProperty = "", strExempt = "", strGrossAssessed = "", strTaxableLandImp = "", strCommonElement = "",
               strTotalAssessed = "", strTotalTaxable = "", strAuthority = "", strRealFirst = "", strRealSecond = "", strTaxAssess = "", strTaxYear = "", strTaxParcel = "-", strTaxAssessDistrict = "-", strTaxRate = "-", strTaxSitusAddress = "", strTaxLegalDis = "", strTaxAddress = "",
               strTaxPro = "", strTaxProValueRole = "", strTaxRole = "", strTaxRoleName = "", strTaxRoleAddress = "", strTaxAmount = "", strTaxPastYear = "", strTaxPastCharge = "", strTaxPastAmount = "-", strTaxNextYear = "-", strTaxNextCharge = "", strTaxNextAmount = "",
               strTaxTotalYear = "", strTaxTotalCharge = "", strTaxTotalAmount = "-", strTaxPaymentAmount = "", strDYear = "", strCCategory = "", strDDistrict = "", strDCharge = "", strMinimum = "-", strBalance = "", strDateDue = "-", strPaymentPost = "", strNumber = "", strDuecharges = "",
               strAmountPaid = "", strTaxAuthority = "", strAMGParcelId = "", strAMGParcel = "", strLegal = "", strOriginalassess = "", strPayOff = "", strType = "", strPrinicipal = "-", strInterest = "-", strPenality = "", strOther = "-", strTotalDue = "", strAMGDistrict = "", strAMGName = "",
               strAMGStatus = "", strAMGUnbilled = "", strBreakdown = "", strBreakDistrict = "", strBreakParcel = "", strTaxStatus = "", strPastType = "", strNextType = "", strTotalType = "", strAMGParcelDetails = "", strAMGAddressDetail = "", strTaxPast = "", strTaxNext = "", strTaxTotal = "",
               strTaxProSpec = "", strTaxProValueDetails = "", strAmgTaxAuthority = "";
        int MultiCount;
        IWebDriver driver;
        IWebElement IAmg;
        DBconnection db = new DBconnection();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        private TimeSpan timeOutInSeconds;
        MySqlParameter[] mParam;
        GlobalClass gc = new GlobalClass();
        public string FTP_NVClark(string houseno, string sname, string sttype, string unitno, string ownername, string parcelNumber, string searchType, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
            var driverService = PhantomJSDriverService.CreateDefaultService();
            //driverService.HideCommandPromptWindow = true;
            //driver = new ChromeDriver();
            using (driver = new PhantomJSDriver())
            {
                StartTime = DateTime.Now.ToString("HH:mm:ss");
                try
                {
                    if (searchType == "titleflex")
                    {
                        string titleaddress = houseno + " " + sname + " " + sttype + " " + unitno;
                        gc.TitleFlexSearch(orderNumber, "", "", titleaddress, "NV", "Clark");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_NVClark"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }


                    if (searchType == "address")
                    {
                        driver.Navigate().GoToUrl("http://redrock.clarkcountynv.gov/assrrealprop/site.aspx");
                        driver.FindElement(By.Id("txtNumber")).SendKeys(houseno);
                        driver.FindElement(By.Id("txtName")).SendKeys(sname);
                        if (directParcel != "")
                        {
                            IWebElement Idirection = driver.FindElement(By.Id("lstDirection"));
                            SelectElement directionselect = new SelectElement(Idirection);
                            directionselect.SelectByValue(directParcel.ToUpper());
                        }
                        if (sttype != "")
                        {
                            IWebElement IstreetType = driver.FindElement(By.Id("lstType"));
                            SelectElement streetTypeselect = new SelectElement(IstreetType);
                            streetTypeselect.SelectByValue(sttype.ToUpper());
                        }
                        gc.CreatePdf_WOP(orderNumber, "Address Search", driver, "NV", "Clark");
                        driver.FindElement(By.Id("Submit1")).SendKeys(Keys.Enter);

                        try
                        {
                            IWebElement IMulti = driver.FindElement(By.XPath("//*[@id='DataGrid1']/tbody"));
                            IList<IWebElement> IMultiRow = IMulti.FindElements(By.TagName("tr"));
                            IList<IWebElement> IMultiTD;
                            foreach (IWebElement multi in IMultiRow)
                            {

                                IMultiTD = multi.FindElements(By.TagName("td"));
                                try
                                {
                                    if (IMultiRow.Count != 0 && IMultiRow.Count == 3 && !multi.Text.Contains("LOCATION ADDRESS"))
                                    {
                                        IWebElement IOwner = IMultiTD[2].FindElement(By.TagName("a"));
                                        string strOwnerLInk = IOwner.GetAttribute("href");
                                        driver.Navigate().GoToUrl(strOwnerLInk);
                                    }
                                }
                                catch { }
                                try
                                {
                                    string address = "";
                                    if (IMultiTD.Count != 0 && IMultiRow.Count != 3 && !multi.Text.Contains("LOCATION ADDRESS") && Convert.ToInt32(MultiCount) < 25 && IMultiRow.Count != 3)
                                    {
                                        if (directParcel != "")
                                        {
                                            address = houseno.ToUpper() + " " + directParcel.ToUpper() + " " + sname.ToUpper();
                                        }
                                        else
                                        {
                                            address = houseno.ToUpper() + " " + sname.ToUpper();
                                        }
                                        if (IMultiTD[0].Text.Contains(address))
                                        {
                                            IWebElement IOwner = IMultiTD[2].FindElement(By.TagName("a"));
                                            string straddressLInk = IOwner.GetAttribute("href");
                                            driver.Navigate().GoToUrl(straddressLInk);
                                            break;
                                        }
                                        else
                                        {
                                            try
                                            {
                                                HttpContext.Current.Session["multiparcel_NVClark"] = "Yes";
                                                gc.CreatePdf_WOP(orderNumber, "Multi Search Result", driver, "NV", "Clark");
                                                strAddress = IMultiTD[0].Text;
                                                strCity = IMultiTD[1].Text;
                                                strParcelId = IMultiTD[2].Text;

                                                string strMultiDetails = strAddress + "~" + strCity + "~" + "-";
                                                gc.insert_date(orderNumber, strParcelId, 467, strMultiDetails, 1, DateTime.Now);

                                                MultiCount++;
                                            }
                                            catch { }
                                        }
                                    }
                                }
                                catch { }
                            }

                            if (HttpContext.Current.Session["multiparcel_NVClark"] != null && HttpContext.Current.Session["multiparcel_NVClark"].ToString() == "Yes")
                            {
                                driver.Quit();
                                return "MultiParcel";
                            }
                        }
                        catch { }
                    }

                    if (searchType == "parcel")
                    {

                        if (GlobalClass.titleparcel.Contains(".") || GlobalClass.titleparcel.Contains("-"))
                        {
                            parcelNumber = GlobalClass.titleparcel.Replace(".", "").Replace("-", "");
                        }


                        driver.Navigate().GoToUrl("http://redrock.clarkcountynv.gov/assrrealprop/pcl.aspx");
                        driver.FindElement(By.Id("parcel")).SendKeys(parcelNumber);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search", driver, "NV", "Clark");
                        driver.FindElement(By.Id("Submit1")).SendKeys(Keys.Enter);
                    }


                    if (searchType == "ownername")
                    {
                        driver.Navigate().GoToUrl("http://redrock.clarkcountynv.gov/assrrealprop/ownr.aspx");
                        string[] strOwnerName = ownername.Split(' ');
                        if (strOwnerName.Length == 2)
                        {
                            driver.FindElement(By.Id("txtBxFirstName")).SendKeys(strOwnerName[1]);
                            driver.FindElement(By.Id("txtBxLastName")).SendKeys(strOwnerName[0]);
                        }
                        if (strOwnerName.Length > 2)
                        {
                            driver.FindElement(By.Id("txtBxLastName")).SendKeys(ownername);
                        }
                        gc.CreatePdf_WOP(orderNumber, "Owner Search", driver, "NV", "Clark");
                        driver.FindElement(By.Id("Submit1")).SendKeys(Keys.Enter);

                        try
                        {
                            IWebElement IMulti = driver.FindElement(By.XPath("//*[@id='DataGrid1']/tbody"));
                            IList<IWebElement> IMultiRow = IMulti.FindElements(By.TagName("tr"));
                            IList<IWebElement> IMultiTD;
                            foreach (IWebElement multi in IMultiRow)
                            {
                                IMultiTD = multi.FindElements(By.TagName("td"));
                                try
                                {
                                    if (IMultiRow.Count != 0 && IMultiRow.Count == 3 && !multi.Text.Contains("OWNER NAME"))
                                    {
                                        IWebElement IOwner = IMultiTD[3].FindElement(By.TagName("a"));
                                        string strOwnerLInk = IOwner.GetAttribute("href");
                                        driver.Navigate().GoToUrl(strOwnerLInk);
                                    }
                                }
                                catch { }
                                try
                                {
                                    if (IMultiTD.Count != 0 && IMultiRow.Count != 3 && !multi.Text.Contains("OWNER NAME") && Convert.ToInt32(MultiCount) < 25 && IMultiRow.Count != 3)
                                    {
                                        try
                                        {
                                            HttpContext.Current.Session["multiparcel_NVClark"] = "Yes";
                                            gc.CreatePdf_WOP(orderNumber, "Multi Search Result", driver, "NV", "Clark");
                                            strOwner = IMultiTD[0].Text;
                                            strParcelId = IMultiTD[3].Text;

                                            string strMultiDetails = "-" + "~" + "-" + "~" + strOwner;
                                            gc.insert_date(orderNumber, strParcelId, 467, strMultiDetails, 1, DateTime.Now);

                                            MultiCount++;
                                        }
                                        catch { }
                                    }
                                }
                                catch { }
                            }

                            if (HttpContext.Current.Session["multiparcel_NVClark"] != null && HttpContext.Current.Session["multiparcel_NVClark"].ToString() == "Yes")
                            {
                                driver.Quit();
                                return "MultiParcel";
                            }
                        }
                        catch (Exception ex) { }
                    }

                    try
                    {
                        IWebElement INodata = driver.FindElement(By.XPath("/html/body"));
                        if(INodata.Text.Contains("No record found"))
                        {
                            HttpContext.Current.Session["Nodata_NVClark"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }

                    //Property Details
                    strParcelNumber = driver.FindElement(By.Id("lblParcel")).Text;
                    gc.CreatePdf(orderNumber, strParcelNumber, "Parcel Search Result", driver, "NV", "Clark");
                    strOwnerMailingAddress = driver.FindElement(By.XPath("//*[@id='printReady']/center/table[1]/tbody/tr[3]/td[2]")).Text.Replace("\r\n", " ");
                    strLocationAddress = driver.FindElement(By.XPath("//*[@id='printReady']/center/table[1]/tbody/tr[4]/td[2]")).Text.Replace("\r\n", " ");
                    strAssesser = driver.FindElement(By.XPath("//*[@id='printReady']/center/table[1]/tbody/tr[5]/td[2]")).Text.Replace("\r\n", " ");
                    strTaxDistrict = driver.FindElement(By.XPath("//*[@id='printReady']/center/table[2]/tbody/tr[2]/td[2]")).Text;
                    strApprisalYear = driver.FindElement(By.XPath("//*[@id='printReady']/center/table[2]/tbody/tr[3]/td[2]")).Text;
                    strFiscalYear = driver.FindElement(By.XPath("//*[@id='printReady']/center/table[2]/tbody/tr[4]/td[2]")).Text;
                    strSupplImpValue = driver.FindElement(By.XPath("//*[@id='printReady']/center/table[2]/tbody/tr[5]/td[2]")).Text;
                    strIncreLand = driver.FindElement(By.XPath("//*[@id='printReady']/center/table[2]/tbody/tr[6]/td[2]")).Text;
                    strIncreImprove = driver.FindElement(By.XPath("//*[@id='printReady']/center/table[2]/tbody/tr[7]/td[2]")).Text;

                    //Estomated Value
                    strEstimatdSize = driver.FindElement(By.XPath("//*[@id='printReady2']/center/table/tbody/tr[2]/td[2]")).Text;
                    strOriginalYear = driver.FindElement(By.XPath("//*[@id='printReady2']/center/table/tbody/tr[3]/td[2]")).Text;
                    strLandUse = driver.FindElement(By.XPath("//*[@id='printReady2']/center/table/tbody/tr[5]/td[2]")).Text;

                    string strPropertyDetails = strOwnerMailingAddress + "~" + strLocationAddress + "~" + strAssesser + "~" + strTaxDistrict + "~" + strApprisalYear + "~" + strFiscalYear + "~" + strSupplImpValue + "~" + strIncreLand + "~" + strIncreImprove + "~" + strEstimatdSize + "~" + strOriginalYear + "~" + strLandUse;
                    gc.insert_date(orderNumber, strParcelNumber, 465, strPropertyDetails, 1, DateTime.Now);

                    //Real Property
                    IWebElement IRealPropertyTable = driver.FindElement(By.XPath("//*[@id='printReady']/center/table[3]/tbody"));
                    IList<IWebElement> IRealPropertyRow = IRealPropertyTable.FindElements(By.TagName("tr"));
                    IList<IWebElement> IRealPropertyTD;
                    foreach (IWebElement Real in IRealPropertyRow)
                    {
                        IRealPropertyTD = Real.FindElements(By.TagName("td"));
                        if (IRealPropertyTD.Count != 0 && !Real.Text.Contains("REAL PROPERTY ASSESSED VALUE"))
                        {
                            try
                            {
                                strRealFirst += IRealPropertyTD[1].Text + "~";
                                strRealSecond += IRealPropertyTD[2].Text + "~";
                            }
                            catch { }
                        }
                    }

                    if (strRealFirst.Count() != 0 && strRealSecond.Count() != 0)
                    {
                        string strPFirstYearDetails = strRealFirst.Remove(strRealFirst.Length - 1);
                        gc.insert_date(orderNumber, strParcelNumber, 466, strPFirstYearDetails, 1, DateTime.Now);
                        string strPSecondYearDetails = strRealSecond.Remove(strRealSecond.Length - 1);
                        gc.insert_date(orderNumber, strParcelNumber, 466, strPSecondYearDetails, 1, DateTime.Now);
                    }

                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");

                    driver.Navigate().GoToUrl("http://www.clarkcountynv.gov/treasurer/Pages/ContactUs.aspx");
                    string strcontact = driver.FindElement(By.XPath("//*[@id='ctl00_ctl54_g_fe27b640_8a9a_4c99_86a8_60190c39074d']/div/div/div/div[1]")).Text;
                    strAuthority = gc.Between(strcontact, "Mailing Address:", "\r\nOffice Hours:").Replace("\r\n", " ");

                    driver.Navigate().GoToUrl("http://trweb.co.clark.nv.us/search_public1.asp");
                    driver.FindElement(By.XPath("/html/body/div[1]/center/table/tbody/tr[2]/td[2]/table[3]/tbody/tr[2]/td/table/tbody/tr[1]/td[1]/form/table/tbody/tr[1]/td[1]/table/tbody/tr/td[2]/input")).SendKeys(strParcelNumber);
                    gc.CreatePdf(orderNumber, strParcelNumber, "Tax Search", driver, "NV", "Clark");
                    driver.FindElement(By.XPath("/html/body/div[1]/center/table/tbody/tr[2]/td[2]/table[3]/tbody/tr[2]/td/table/tbody/tr[1]/td[1]/form/table/tbody/tr[1]/td[2]/input")).SendKeys(Keys.Enter);
                    gc.CreatePdf(orderNumber, strParcelNumber, "Tax Result", driver, "NV", "Clark");
                    IWebElement ITaxAssess = driver.FindElement(By.XPath("/html/body/table[4]/tbody/tr[3]/td/table"));
                    strTaxAssess = ITaxAssess.Text;

                    strTaxParcel = gc.Between(strTaxAssess, "Parcel ID  ", "  Tax Year");
                    strTaxYear = gc.Between(strTaxAssess, "  Tax Year  ", "  District");
                    strTaxAssessDistrict = gc.Between(strTaxAssess, "  District  ", "  Rate");
                    strTaxRate = GlobalClass.After(strTaxAssess, "Rate  ");
                    strTaxSitusAddress = driver.FindElement(By.XPath("/html/body/table[4]/tbody/tr[4]/td/table/tbody/tr[1]/td[2]")).Text;
                    strTaxLegalDis = driver.FindElement(By.XPath("/html/body/table[4]/tbody/tr[4]/td/table/tbody/tr[2]/td[2]")).Text;

                    IWebElement IStatus = driver.FindElement(By.XPath("/html/body/table[4]/tbody/tr[5]/td/table[1]/tbody/tr/td[1]/table/tbody"));
                    strTaxStatus = GlobalClass.After(IStatus.Text, " Status:\r\n");
                    string strTaxProTitle = "";
                    IWebElement IPropertyTable = driver.FindElement(By.XPath("/html/body/table[4]/tbody/tr[5]/td/table[1]/tbody/tr/td[2]/table/tbody"));
                    IList<IWebElement> IPropertyRow = IPropertyTable.FindElements(By.TagName("tr"));
                    IList<IWebElement> IPropertyTD;
                    foreach (IWebElement Pro in IPropertyRow)
                    {
                        IPropertyTD = Pro.FindElements(By.TagName("td"));
                        if (IPropertyTD.Count != 0 && !Pro.Text.Contains("Property Characteristics") && Pro.Text.Trim() != "")
                        {
                            try
                            {
                                if (Pro.Text.Contains(" Special Improvement Dist "))
                                {
                                    strTaxProTitle += IPropertyTD[0].Text + "~";
                                    strTaxPro += IPropertyTD[1].Text + "~";
                                }
                                if (Pro.Text.Contains("Tax Cap Increase Pct."))
                                {
                                    strTaxProTitle += IPropertyTD[0].Text + "~";
                                    strTaxPro += IPropertyTD[1].Text + "~";
                                }
                                else if (Pro.Text.Contains(" Tax Cap Limit Amount "))
                                {
                                    strTaxProTitle += IPropertyTD[0].Text + "~";
                                    strTaxPro += IPropertyTD[1].Text + "~";
                                }
                                else if (Pro.Text.Contains(" Tax Cap Reduction "))
                                {
                                    strTaxProTitle += IPropertyTD[0].Text + "~";
                                    strTaxPro += IPropertyTD[1].Text + "~";
                                }
                                else if (Pro.Text.Contains(" Land Use "))
                                {
                                    strTaxProTitle += IPropertyTD[0].Text + "~";
                                    strTaxPro += IPropertyTD[1].Text + "~";
                                }
                                else if (Pro.Text.Contains(" Cap Type "))
                                {
                                    strTaxProTitle += IPropertyTD[0].Text + "~";
                                    strTaxPro += IPropertyTD[1].Text + "~";
                                }
                                else if (Pro.Text.Contains(" Acreage "))
                                {
                                    strTaxProTitle += IPropertyTD[0].Text + "~";
                                    strTaxPro += IPropertyTD[1].Text + "~";
                                }
                                else if (Pro.Text.Contains(" Exemption Amount "))
                                {
                                    strTaxProTitle += IPropertyTD[0].Text + "~";
                                    strTaxPro += IPropertyTD[1].Text + "~";
                                }
                            }

                            catch { }
                        }
                    }

                    IWebElement IProValueTable = driver.FindElement(By.XPath("/html/body/table[4]/tbody/tr[5]/td/table[1]/tbody/tr/td[3]/table/tbody"));
                    IList<IWebElement> IProValueRow = IProValueTable.FindElements(By.TagName("tr"));
                    IList<IWebElement> IProValueTD;
                    foreach (IWebElement Role in IProValueRow)
                    {
                        IProValueTD = Role.FindElements(By.TagName("td"));
                        try
                        {
                            if (IProValueTD.Count != 0 && !Role.Text.Contains("Property Values") && Role.Text.Trim() != "")
                            {
                                if (Role.Text.Contains(" Land "))
                                {
                                    strTaxProTitle += IProValueTD[0].Text + "~";
                                    strTaxPro += IProValueTD[1].Text + "~";
                                }
                                else if (Role.Text.Contains(" Improvements "))
                                {
                                    strTaxProTitle += IProValueTD[0].Text + "~";
                                    strTaxPro += IProValueTD[1].Text + "~";
                                }
                                else if (Role.Text.Contains(" Total Assessed Value "))
                                {
                                    strTaxProTitle += IProValueTD[0].Text + "~";
                                    strTaxPro += IProValueTD[1].Text + "~";
                                }
                                else if (Role.Text.Contains(" Net Assessed Value "))
                                {
                                    strTaxProTitle += IProValueTD[0].Text + "~";
                                    strTaxPro += IProValueTD[1].Text + "~";
                                }
                                else if (Role.Text.Contains(" Exemption Value New Construction "))
                                {
                                    strTaxProTitle += IProValueTD[0].Text + "~";
                                    strTaxPro += IProValueTD[1].Text + "~";
                                }
                                else if (Role.Text.Contains(" New Construction - Supp Value "))
                                {
                                    strTaxProTitle += IProValueTD[0].Text + "~";
                                    strTaxPro += IProValueTD[1].Text + "~";
                                }
                            }
                        }
                        catch { }
                    }

                    if (strTaxPro.Length != 0 && strTaxPro != "")
                    {
                        db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + strTaxProTitle.Remove(strTaxProTitle.Length - 1) + "' where Id = '" + 479 + "'");
                        //strTaxProValueDetails = strTaxProValueRole.Remove(strTaxProValueRole.Length - 1);
                        gc.insert_date(orderNumber, strParcelNumber, 479, strTaxPro.Remove(strTaxPro.Length - 1), 1, DateTime.Now);
                    }

                    IWebElement IRoleTable = driver.FindElement(By.XPath("/html/body/table[4]/tbody/tr[5]/td/table[2]/tbody"));
                    IList<IWebElement> IRoleRow = IRoleTable.FindElements(By.TagName("tr"));
                    IList<IWebElement> IRoleTD;
                    foreach (IWebElement Role in IRoleRow)
                    {
                        IRoleTD = Role.FindElements(By.TagName("td"));
                        if (IRoleTD.Count != 0 && !Role.Text.Contains(" Role"))
                        {
                            try
                            {
                                strTaxRole = IRoleTD[0].Text;
                                strTaxRoleName = IRoleTD[1].Text;
                                strTaxRoleAddress = IRoleTD[2].Text;
                            }
                            catch { }

                        }
                    }

                    if (strTaxRole.Trim() != "" && strTaxRoleName.Trim() != "" && strTaxRoleAddress.Trim() != "")
                    {
                        string strTaxAssessmentDetails = strTaxYear + "~" + strTaxAssessDistrict + "~" + strTaxRate + "~" + strTaxSitusAddress + "~" + strTaxLegalDis + "~" + strTaxStatus.Replace("r\n", "") + "~" + strTaxRole + "~" + strTaxRoleName + "~" + strTaxRoleAddress + "~" + strAuthority;
                        gc.insert_date(orderNumber, strTaxParcel, 478, strTaxAssessmentDetails, 1, DateTime.Now);
                    }

                    IWebElement ISummaryTable = driver.FindElement(By.XPath("/html/body/table[4]/tbody/tr[5]/td/table[3]/tbody"));
                    IList<IWebElement> ISummaryRow = ISummaryTable.FindElements(By.TagName("tr"));
                    IList<IWebElement> ISummaryTD;
                    foreach (IWebElement sum in ISummaryRow)
                    {
                        ISummaryTD = sum.FindElements(By.TagName("td"));
                        if (ISummaryTD.Count != 0 && !sum.Text.Contains(" Item") && !sum.Text.Contains("Summary"))
                        {
                            try
                            {
                                strTaxAmount += ISummaryTD[1].Text + "~";
                            }
                            catch { }
                        }
                    }


                    IWebElement IPastCurrentTable = driver.FindElement(By.XPath("/html/body/table[4]/tbody/tr[5]/td/table[4]/tbody"));
                    IList<IWebElement> IPastCurrentRow = IPastCurrentTable.FindElements(By.TagName("tr"));
                    IList<IWebElement> IPastCurrentTD;
                    foreach (IWebElement Past in IPastCurrentRow)
                    {
                        IPastCurrentTD = Past.FindElements(By.TagName("td"));
                        if (Past.Text.Contains("PAST AND CURRENT CHARGES"))
                        {
                            try
                            {
                                strPastType = IPastCurrentTD[0].Text.Trim();
                            }
                            catch { }
                        }
                        if (IPastCurrentTD.Count != 0 && IPastCurrentTD.Count == 3 && !Past.Text.Contains(" Tax Year") && !Past.Text.Contains("PAST AND CURRENT CHARGES DUE TODAY") && !Past.Text.Contains("THERE IS NO PAST OR CURRENT AMOUNT DUE"))
                        {
                            try
                            {
                                strTaxPastYear = IPastCurrentTD[0].Text.Trim();
                                strTaxPastCharge = IPastCurrentTD[1].Text.Trim();
                                strTaxPastAmount = IPastCurrentTD[2].Text.Trim();

                                string strTaxPastDetails = strPastType + "~" + strTaxPastYear + "~" + strTaxPastCharge + "~" + strTaxPastAmount + "~" + "-" + "~" + "-";
                                gc.insert_date(orderNumber, strParcelNumber, 482, strTaxPastDetails, 1, DateTime.Now);
                            }
                            catch { }
                            strTaxPastYear = ""; strTaxPastCharge = ""; strTaxPastAmount = "";
                        }

                        if (IPastCurrentTD.Count != 0 && IPastCurrentTD.Count == 2 && IPastCurrentRow.Count != 3)
                        {
                            try
                            {
                                strTaxPastCharge = IPastCurrentTD[0].Text.Trim();
                                strTaxPastAmount = IPastCurrentTD[1].Text.Trim();

                                string strTaxPastDetails = strPastType + "~" + strTaxPastYear + "~" + strTaxPastCharge + "~" + strTaxPastAmount + "~" + "-" + "~" + "-";
                                gc.insert_date(orderNumber, strParcelNumber, 482, strTaxPastDetails, 1, DateTime.Now);
                            }
                            catch { }
                            strTaxPastYear = ""; strTaxPastCharge = ""; strTaxPastAmount = "";
                        }

                        if (IPastCurrentTD.Count == 2 && IPastCurrentRow.Count == 3)
                        {
                            strTaxPastYear = IPastCurrentTD[0].Text.Trim();
                            strTaxPast = IPastCurrentTD[1].Text.Trim();

                            string strTaxPastDetails = strPastType + "~" + "-" + "~" + strTaxPastYear + "~" + strTaxPast + "~" + "-" + "~" + "-";
                            gc.insert_date(orderNumber, strParcelNumber, 482, strTaxPastDetails, 1, DateTime.Now);
                            strTaxPastYear = ""; strTaxPastCharge = ""; strTaxPastAmount = "";
                        }
                    }

                    IWebElement INextTable = driver.FindElement(By.XPath("/html/body/table[4]/tbody/tr[5]/td/table[5]/tbody"));
                    IList<IWebElement> INextRow = INextTable.FindElements(By.TagName("tr"));
                    IList<IWebElement> INextTD;
                    foreach (IWebElement Next in INextRow)
                    {
                        INextTD = Next.FindElements(By.TagName("td"));
                        if (Next.Text.Contains("NEXT INSTALLMENT AMOUNTS"))
                        {
                            try
                            {
                                strNextType = INextTD[0].Text.Trim();
                            }
                            catch { }
                        }
                        if (INextTD.Count != 0 && INextTD.Count == 3 && !Next.Text.Contains(" Tax Year") && !Next.Text.Contains("NEXT INSTALLMENT AMOUNTS") && !Next.Text.Contains("THERE IS NO NEXT INSTALLMENT AMOUNT DUE"))
                        {
                            try
                            {
                                strTaxNextYear = INextTD[0].Text.Trim();
                                strTaxNextCharge = INextTD[1].Text.Trim();
                                strTaxNextAmount = INextTD[2].Text.Trim();

                                string strTaxNextDetails = strNextType + "~" + strTaxNextYear + "~" + strTaxNextCharge + "~" + "-" + "~" + strTaxNextAmount + "~" + "-";
                                gc.insert_date(orderNumber, strParcelNumber, 482, strTaxNextDetails, 1, DateTime.Now);
                            }
                            catch { }
                            strTaxNextYear = ""; strTaxNextCharge = ""; strTaxNextAmount = "";
                        }
                        if (INextTD.Count != 0 && INextTD.Count == 2 && INextRow.Count != 3)
                        {
                            try
                            {
                                strTaxNextCharge = INextTD[0].Text.Trim();
                                strTaxNextAmount = INextTD[1].Text.Trim();

                                string strTaxNextDetails = strNextType + "~" + strTaxNextYear + "~" + strTaxNextCharge + "~" + "-" + "~" + strTaxNextAmount + "~" + "-";
                                gc.insert_date(orderNumber, strParcelNumber, 482, strTaxNextDetails, 1, DateTime.Now);

                            }
                            catch { }
                            strTaxNextYear = ""; strTaxNextCharge = ""; strTaxNextAmount = "";
                        }

                        if (INextRow.Count == 3 && INextTD.Count == 2)
                        {
                            strTaxNext = INextTD[1].Text.Trim();
                            strTaxNextYear = INextTD[0].Text.Trim();

                            string strTaxNextDetails = strNextType + "~" + strTaxNextYear + "~" + "" + "~" + "" + "~" + strTaxNext + "~" + "-";
                            gc.insert_date(orderNumber, strParcelNumber, 482, strTaxNextDetails, 1, DateTime.Now);

                            strTaxNextYear = ""; strTaxNextCharge = ""; strTaxNextAmount = "";
                        }
                    }

                    try
                    {
                        IWebElement ITotalTable = driver.FindElement(By.XPath("/html/body/table[4]/tbody/tr[5]/td/table[6]/tbody"));
                        IList<IWebElement> ITotalRow = ITotalTable.FindElements(By.TagName("tr"));
                        IList<IWebElement> ITotalTD;
                        foreach (IWebElement Total in ITotalRow)
                        {
                            ITotalTD = Total.FindElements(By.TagName("td"));
                            if (Total.Text.Contains("TOTAL AMOUNTS DUE FOR ENTIRE TAX YEAR"))
                            {
                                try
                                {
                                    strTotalType = ITotalTD[0].Text.Trim();
                                }
                                catch { }
                            }
                            if (ITotalTD.Count != 0 && !Total.Text.Contains(" Tax Year") && !Total.Text.Contains("TOTAL AMOUNTS DUE FOR ENTIRE TAX YEAR") && !Total.Text.Contains("THERE IS NO TOTAL AMOUNT DUE FOR THE ENTIRE TAX YEAR"))
                            {
                                try
                                {
                                    strTaxTotalYear = ITotalTD[0].Text.Trim();
                                    strTaxTotalCharge = ITotalTD[1].Text.Trim();
                                    strTaxTotalAmount = ITotalTD[2].Text.Trim();


                                    string strTaxTotalDetails = strTotalType + "~" + strTaxTotalYear + "~" + strTaxTotalCharge + "~" + "-" + "~" + "-" + "~" + strTaxTotalAmount;
                                    gc.insert_date(orderNumber, strParcelNumber, 482, strTaxTotalDetails, 1, DateTime.Now);
                                }
                                catch { }
                                strTaxTotalYear = ""; strTaxTotalCharge = ""; strTaxTotalAmount = "";
                            }
                            if (ITotalTD.Count != 0 && ITotalTD.Count == 2 && ITotalRow.Count != 3)
                            {
                                try
                                {
                                    strTaxTotalCharge = ITotalTD[0].Text.Trim();
                                    strTaxTotalAmount = ITotalTD[1].Text.Trim();

                                    string strTaxTotalDetails = strTotalType + "~" + strTaxTotalYear + "~" + strTaxTotalCharge + "~" + "-" + "~" + "-" + "~" + strTaxTotalAmount;
                                    gc.insert_date(orderNumber, strParcelNumber, 482, strTaxTotalDetails, 1, DateTime.Now);

                                }
                                catch { }
                                strTaxTotalYear = ""; strTaxTotalCharge = ""; strTaxTotalAmount = "";
                            }

                            if (ITotalRow.Count == 3 && ITotalTD.Count == 2)
                            {
                                strTaxTotal = ITotalTD[1].Text.Trim();
                                strTaxTotalYear = ITotalTD[0].Text.Trim();

                                string strTaxTotalDetails = strTotalType + "~" + strTaxTotalYear + "~" + "" + "~" + "" + "~" + "-" + "~" + strTaxTotal;
                                gc.insert_date(orderNumber, strParcelNumber, 482, strTaxTotalDetails, 1, DateTime.Now);
                                strTaxTotalYear = ""; strTaxTotalCharge = ""; strTaxTotalAmount = "";
                            }
                        }

                    }
                    catch { }

                    //if (strTaxAmount.Trim() != "" || (strTaxPast.Trim() != "" && strTaxNext.Trim() != "" && strTaxTotal.Trim() != ""))
                    //{
                    //    string strTaxsummaryDetails = strTaxAmount + strTaxPast.Trim() + "~" + strTaxNext.Trim() + "~" + strTaxTotal.Trim();
                    //    gc.insert_date(orderNumber, strParcelNumber, 489, strTaxsummaryDetails, 1, DateTime.Now);
                    //}

                    try { 
                    IWebElement IPaymentTable = driver.FindElement(By.XPath("/html/body/table[4]/tbody/tr[5]/td/table[7]/tbody"));
                    IList<IWebElement> IPaymentRow = IPaymentTable.FindElements(By.TagName("tr"));
                    IList<IWebElement> IPaymentTD;
                    foreach (IWebElement Payment in IPaymentRow)
                    {
                        IPaymentTD = Payment.FindElements(By.TagName("td"));
                        if (IPaymentTD.Count != 0 && !Payment.Text.Contains(" PAYMENT HISTORY"))
                        {
                            try
                            {
                                strTaxPaymentAmount += IPaymentTD[1].Text + "~";
                            }
                            catch { }
                        }
                    }

                        string strTaxPaymentDetails = strTaxPaymentAmount.Remove(strTaxPaymentAmount.Length - 1);
                        gc.insert_date(orderNumber, strParcelNumber, 486, strTaxPaymentDetails, 1, DateTime.Now);
                    }
                    catch { }

                    

                    driver.Navigate().GoToUrl("http://trtitle.co.clark.nv.us/WEP_Summary.asp?Parcel=" + strParcelNumber + "");
                    gc.CreatePdf(orderNumber, strParcelNumber, "Delinquent Result", driver, "NV", "Clark");
                    IWebElement IDetailsTable = driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr[5]/td/table[4]/tbody"));
                    IList<IWebElement> IDetailsRow = IDetailsTable.FindElements(By.TagName("tr"));
                    IList<IWebElement> IDetailsTd;
                    foreach (IWebElement Detail in IDetailsRow)
                    {
                        IDetailsTd = Detail.FindElements(By.TagName("td"));
                        if (IDetailsTd.Count != 0 && !Detail.Text.Contains("GRAND TOTALS") && !Detail.Text.Contains("Detail of Amount Due") && !Detail.Text.Contains(" Year"))
                        {
                            try
                            {
                                strDYear = IDetailsTd[0].Text;
                                strCCategory = IDetailsTd[1].Text;
                                strDDistrict = IDetailsTd[2].Text;
                                strDCharge = IDetailsTd[3].Text;
                                strMinimum = IDetailsTd[4].Text;
                                strBalance = IDetailsTd[5].Text;
                                strDateDue = IDetailsTd[6].Text;
                            }
                            catch { }

                            string strTaxDetail = strDYear + "~" + strCCategory + "~" + strDDistrict + "~" + strDCharge + "~" + strMinimum + "~" + strBalance + "~" + strDateDue;
                            gc.insert_date(orderNumber, strParcelNumber, 483, strTaxDetail, 1, DateTime.Now);
                        }
                        if (IDetailsTd.Count != 0 && !Detail.Text.Contains("Detail of Amount Due") && Detail.Text.Contains("GRAND TOTALS") && !Detail.Text.Contains(" Year"))
                        {
                            try
                            {
                                strCCategory = IDetailsTd[0].Text;
                                strDCharge = IDetailsTd[1].Text;
                                strMinimum = IDetailsTd[2].Text;
                                strBalance = IDetailsTd[3].Text;
                                strDateDue = IDetailsTd[4].Text;
                            }
                            catch { }

                            string strTaxDetail = strDYear + "~" + strCCategory + "~" + strDDistrict + "~" + strDCharge + "~" + strMinimum + "~" + strBalance + "~" + strDateDue;
                            gc.insert_date(orderNumber, strParcelNumber, 483, strTaxDetail, 1, DateTime.Now);
                        }
                    }
                    try
                    {
                        IWebElement IPaymentPostTable = driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr[5]/td/table[6]/tbody/tr/td[1]/table/tbody"));
                        IList<IWebElement> IPaymentPostRow = IPaymentPostTable.FindElements(By.TagName("tr"));
                        IList<IWebElement> IPaymentPostTd;
                        foreach (IWebElement Post in IPaymentPostRow)
                        {
                            IPaymentPostTd = Post.FindElements(By.TagName("td"));
                            if (IPaymentPostTd.Count != 0 && !Post.Text.Contains(" Payment Posted"))
                            {
                                try
                                {
                                    strPaymentPost = IPaymentPostTd[0].Text;
                                    strNumber = IPaymentPostTd[1].Text;
                                    strDuecharges = IPaymentPostTd[2].Text;
                                    strAmountPaid = IPaymentPostTd[3].Text;
                                }
                                catch { }

                                string strPaymentPostDetail = strPaymentPost + "~" + strNumber + "~" + strDuecharges + "~" + strAmountPaid;
                                gc.insert_date(orderNumber, strParcelNumber, 484, strPaymentPostDetail, 1, DateTime.Now);
                            }
                        }
                    }
                    catch { }



                        try
                        {
                        driver.Navigate().GoToUrl("https://amgnv.com/");
                        driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr/td[6]/table/tbody/tr[2]/td[2]/p/table[2]/tbody/tr/td/table/tbody/tr[2]/td[1]/form/center/b/input")).SendKeys(strParcelNumber);
                        gc.CreatePdf(orderNumber, strParcelNumber, "Special Assessment", driver, "NV", "Clark");
                        driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr/td[6]/table/tbody/tr[2]/td[2]/p/table[2]/tbody/tr/td/table/tbody/tr[2]/td[1]/form/center/b/font/font/input")).SendKeys(Keys.Enter);
                        gc.CreatePdf(orderNumber, strParcelNumber, "Special Assessment Result", driver, "NV", "Clark");
                        Thread.Sleep(4000);
                        IWebElement IAMGParcelSearch = driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr/td[6]/table/tbody/tr[2]/td[2]/p/font[1]/table/tbody/tr[2]/td[4]/div/font/a"));
                        string stramgParcel = IAMGParcelSearch.GetAttribute("href");
                        driver.Navigate().GoToUrl(stramgParcel);
                        gc.CreatePdf(orderNumber, strParcelNumber, "AMG Result", driver, "NV", "Clark");
                        IWebElement IParcelTable = driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr/td[6]/table/tbody/tr[2]/td[2]/table[3]/tbody"));
                        IList<IWebElement> IParcelRow = IParcelTable.FindElements(By.TagName("tr"));
                        IList<IWebElement> IParcelTD;
                        foreach (IWebElement parcel in IParcelRow)
                        {
                            IParcelTD = parcel.FindElements(By.TagName("td"));
                            if (IParcelTD.Count != 0 && !parcel.Text.Contains("Parcel #") && !parcel.Text.Contains("Amounts updated"))
                            {
                                try
                                {
                                    strAMGParcelId = IParcelTD[0].Text;
                                    strAMGDistrict = IParcelTD[1].Text;
                                    strAMGName = IParcelTD[2].Text;
                                    strAMGStatus = IParcelTD[3].Text;
                                    strAMGUnbilled = IParcelTD[4].Text;
                                }
                                catch { }

                                strAMGParcelDetails = strAMGDistrict + "~" + strAMGName + "~" + strAMGStatus + "~" + strAMGUnbilled;

                            }
                        }

                        IWebElement IAMGAddressTable = driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr/td[6]/table/tbody/tr[2]/td[2]/table[4]/tbody"));
                        IList<IWebElement> IAMGAddressRow = IAMGAddressTable.FindElements(By.TagName("tr"));
                        IList<IWebElement> IAMGAddressTd;
                        foreach (IWebElement address in IAMGAddressRow)
                        {
                            IAMGAddressTd = address.FindElements(By.TagName("td"));
                            if (IAMGAddressTd.Count != 0 && !address.Text.Contains("Situs & Legal Description"))
                            {
                                try
                                {
                                    strLegal += IAMGAddressTd[0].Text + " ";
                                    strOriginalassess = IAMGAddressTd[1].Text;
                                    strPayOff = IAMGAddressTd[2].Text;
                                    IAmg = IAMGAddressTd[2].FindElement(By.TagName("a"));
                                }
                                catch { }

                                strAMGAddressDetail = strLegal + "~" + strOriginalassess + "~" + strPayOff;
                            }
                        }
                        if (strAMGParcelDetails.Trim() != "" && strAMGAddressDetail != "")
                        {
                            try
                            {
                                strAmgTaxAuthority = driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr/td[6]/table/tbody/tr[2]/td[2]/table[8]/tbody/tr[2]/td[2]/table/tbody/tr/td[2]")).Text;
                            }
                            catch { }

                            gc.insert_date(orderNumber, strParcelNumber, 481, strAMGParcelDetails + "~" + strAMGAddressDetail + "~" + strAmgTaxAuthority, 1, DateTime.Now);
                        }
                        IWebElement IAMGDuePayTable = driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr/td[6]/table/tbody/tr[2]/td[2]/table[6]/tbody"));
                        IList<IWebElement> IAMGDuePayRow = IAMGDuePayTable.FindElements(By.TagName("tr"));
                        IList<IWebElement> IAMGDuePayTd;
                        foreach (IWebElement Due in IAMGDuePayRow)
                        {
                            IAMGDuePayTd = Due.FindElements(By.TagName("td"));
                            if (IAMGDuePayTd.Count != 0 && !Due.Text.Contains("Principal") && !Due.Text.Contains("Current Due and Payoff Amounts are valid ") && !Due.Text.Contains("* Penalties are added monthly until the Total Due is paid in full.") && !Due.Text.Contains("**Estimated installments") && !Due.Text.Contains("*** Payoff value"))
                            {
                                try
                                {
                                    strType = IAMGDuePayTd[0].Text;
                                    strPrinicipal = IAMGDuePayTd[1].Text;
                                    strInterest = IAMGDuePayTd[2].Text;
                                    strPenality = IAMGDuePayTd[3].Text;
                                    strOther = IAMGDuePayTd[4].Text;
                                    strTotalDue = IAMGDuePayTd[5].Text;
                                }
                                catch { }

                                string strAMGAddressDetail = strType + "~" + strPrinicipal + "~" + strInterest + "~" + strPenality + "~" + strOther + "~" + strTotalDue;
                                gc.insert_date(orderNumber, strParcelNumber, 485, strAMGAddressDetail, 1, DateTime.Now);
                            }
                        }

                        try
                        {
                            IAmg.SendKeys(Keys.Enter);
                            string strURL = driver.CurrentWindowHandle;
                            string strURLLast = driver.WindowHandles.Last();
                            driver.SwitchTo().Window(strURLLast);
                            gc.CreatePdf(orderNumber, strParcelNumber, "BreakDown Result", driver, "NV", "Clark");
                            strBreakParcel = driver.FindElement(By.XPath("/html/body/center/table[1]/tbody/tr/td/table/tbody/tr[2]/td[1]")).Text;
                            strBreakDistrict = driver.FindElement(By.XPath("/html/body/center/table[1]/tbody/tr/td/table/tbody/tr[2]/td[2]")).Text;
                            IWebElement IBreakDownTable = driver.FindElement(By.XPath("/html/body/center/table[2]/tbody/tr/td/table/tbody"));
                            IList<IWebElement> IBreakDownRow = IBreakDownTable.FindElements(By.TagName("tr"));
                            IList<IWebElement> IBreakDownTD;
                            foreach (IWebElement Break in IBreakDownRow)
                            {
                                IBreakDownTD = Break.FindElements(By.TagName("td"));
                                if (IBreakDownTD.Count != 0 && !Break.Text.Contains("REAL PROPERTY ASSESSED VALUE"))
                                {
                                    try
                                    {
                                        strBreakdown += IBreakDownTD[1].Text + "~";
                                    }
                                    catch { }
                                }
                            }

                            if (strBreakdown.Length != 0)
                            {
                                string strstrBreakdownDetails = strBreakdown.Remove(strBreakdown.Length - 1);
                                gc.insert_date(orderNumber, strParcelNumber, 480, strBreakDistrict + "~" + strstrBreakdownDetails, 1, DateTime.Now);
                            }
                        }
                        catch { }
                    }
                    catch { }

                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "NV", "Clark", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);



                    driver.Quit();
                    gc.mergpdf(orderNumber, "NV", "Clark");
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