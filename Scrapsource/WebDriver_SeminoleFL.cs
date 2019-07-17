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
    public class WebDriver_SeminoleFL
    {
        IWebDriver driver;
        DBconnection db = new DBconnection();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        GlobalClass gc = new GlobalClass();
        public string FTP_SeminoleFL(string sno, string sname, string direction, string sttype, string unino, string parcelNumber, string ownername, string searchType, string orderNumber, string directparcel)
        {
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "", AssessTakenTime = "", TaxTakentime = "", CityTaxtakentime = "";
            string straddress = "";
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            // driver = new PhantomJSDriver();
            //driver = new ChromeDriver();           
            using (driver = new PhantomJSDriver())
            {
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");
                    if (searchType == "titleflex")
                    {

                        if (direction != "")
                        {
                            straddress = sno + " " + direction + " " + sname + " " + sttype + " " + unino;
                        }
                        else
                        {
                            straddress = sno + " " + sname + " " + sttype + " " + unino;
                        }
                        gc.TitleFlexSearch(orderNumber, "", ownername, straddress, "FL", "Seminole");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_SeminoleFL"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }
                    //driver.Navigate().GoToUrl("https://www.scpafl.org/RealPropertySearch");
                    //Thread.Sleep(3000);
                    driver.Navigate().GoToUrl("https://www.scpafl.org/");
                    Thread.Sleep(1000);

                    if (searchType == "address")
                    {
                        driver.FindElement(By.Id("dnn_ctr443_View_txtAddr_I")).SendKeys(sno);
                        driver.FindElement(By.Id("dnn_ctr443_View_cmbListDir_I")).SendKeys(direction);
                        driver.FindElement(By.Id("dnn_ctr443_View_cmbStreet_I")).SendKeys(sname);
                        driver.FindElement(By.Id("dnn_ctr443_View_cmbSuffix_I")).SendKeys(sttype);

                        IWebElement IAddressSearch1 = driver.FindElement(By.Id("dnn_ctr443_View_ctl01"));
                        IJavaScriptExecutor js1 = driver as IJavaScriptExecutor;
                        js1.ExecuteScript("arguments[0].click();", IAddressSearch1);
                        Thread.Sleep(3000);

                        gc.CreatePdf_WOP(orderNumber, "Parcel search Input ", driver, "FL", "Seminole");
                        ////Multiparcel
                        try
                        {
                            //int Count = 0;                           
                            IWebElement Multiaddresstable = driver.FindElement(By.Id("dnn_ctr446_View_callbackSearch_gridResult_DXMainTable"));
                            IList<IWebElement> multiaddressrow = Multiaddresstable.FindElements(By.TagName("tr"));
                            IList<IWebElement> Multiaddressid;
                            foreach (IWebElement Multiaddress in multiaddressrow)
                            {
                                Multiaddressid = Multiaddress.FindElements(By.TagName("td"));
                                if (multiaddressrow.Count > 8 && Multiaddressid.Count != 0 && Multiaddressid.Count == 8 && !Multiaddress.Text.Contains("Parcel") & Multiaddress.Text.Trim().Contains(straddress.ToUpper().Trim()))
                                {
                                    string Multiparcelnumber = Multiaddressid[0].Text;
                                    string OWnername = Multiaddressid[1].Text;
                                    string Address1 = Multiaddressid[2].Text;

                                    string multiaddressresult = OWnername + "~" + Address1;
                                    gc.insert_date(orderNumber, Multiparcelnumber, 1633, multiaddressresult, 1, DateTime.Now);
                                    //Count++;
                                }
                            }
                            if (multiaddressrow.Count == 8)
                            {
                                driver.FindElement(By.Id("dnn_ctr940_View_callbackSearch_gridResult_DXDataRow0")).Click();
                                Thread.Sleep(2000);
                            }
                            if (multiaddressrow.Count > 8)
                            {
                                HttpContext.Current.Session["multiparcel_SeminoleFL"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                            if (multiaddressrow.Count > 35)
                            {
                                HttpContext.Current.Session["multiParcel_SeminoleFL_Maximum"] = "Maximum";
                                driver.Quit();
                                return "Maximum";
                            }
                        }
                        catch { }
                        try
                        {
                            //No Data Found
                            string nodata = driver.FindElement(By.Id("ctl00_BodyContentPlaceHolder_ErrorLabel")).Text;
                            if (nodata.Contains("Returned 0 records."))
                            {
                                HttpContext.Current.Session["Nodata_SeminoleFL"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }
                    if (searchType == "parcel")
                    {
                        parcelNumber = parcelNumber.Replace(" ", "").Replace(".", "").Replace("-", "").Trim();                        
                        driver.FindElement(By.Id("dnn_ctr443_View_txtParcel_I")).SendKeys(parcelNumber);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel search Input ", driver, "FL", "Seminole");
                        driver.FindElement(By.Id("dnn_ctr443_View_ctl02")).Click();
                        Thread.Sleep(3000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel search Output ", driver, "FL", "Seminole");

                        try
                        {
                            //No Data Found
                            string nodata = driver.FindElement(By.XPath("//*[@id='dnn_ctr446_View_callbackSearch_gridResult_DXEmptyRow']/td[1]/div")).Text;
                            if (nodata.Contains("No Address by name"))
                            {
                                HttpContext.Current.Session["Nodata_SeminoleFL"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch (Exception e) { }
                    }
                    if (searchType == "ownername")
                    {
                        driver.FindElement(By.Id("dnn_ctr443_View_cmbOwner_I")).SendKeys(ownername);
                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "Owner search Input ", driver, "FL", "Seminole");

                        IWebElement IAddressSearch1 = driver.FindElement(By.Id("dnn_ctr443_View_ctl00"));
                        IJavaScriptExecutor js1 = driver as IJavaScriptExecutor;
                        js1.ExecuteScript("arguments[0].click();", IAddressSearch1);
                        Thread.Sleep(3000);

                        gc.CreatePdf_WOP(orderNumber, "Owner search Result ", driver, "FL", "Seminole");
                        ////Multiparcel
                        try
                        {
                            //int Count = 0;                           
                            IWebElement Multiaddresstable = driver.FindElement(By.Id("dnn_ctr446_View_callbackSearch_gridResult_DXMainTable"));
                            IList<IWebElement> multiaddressrow = Multiaddresstable.FindElements(By.TagName("tr"));
                            IList<IWebElement> Multiaddressid;
                            foreach (IWebElement Multiaddress in multiaddressrow)
                            {
                                Multiaddressid = Multiaddress.FindElements(By.TagName("td"));
                                if (multiaddressrow.Count > 8 && Multiaddressid.Count != 0 && Multiaddressid.Count == 8 && !Multiaddress.Text.Contains("Parcel"))
                                {
                                    string Multiparcelnumber = Multiaddressid[0].Text;
                                    string OWnername = Multiaddressid[1].Text;
                                    string Address1 = Multiaddressid[2].Text;

                                    string multiaddressresult = OWnername + "~" + Address1;
                                    gc.insert_date(orderNumber, Multiparcelnumber, 1633, multiaddressresult, 1, DateTime.Now);
                                    //Count++;
                                }
                            }
                            if (multiaddressrow.Count == 8)
                            {
                                driver.FindElement(By.Id("dnn_ctr446_View_callbackSearch_gridResult_DXDataRow0")).Click();
                                Thread.Sleep(2000);
                            }
                            if (multiaddressrow.Count > 8)
                            {
                                HttpContext.Current.Session["multiparcel_SeminoleFL"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                            if (multiaddressrow.Count > 35)
                            {
                                HttpContext.Current.Session["multiParcel_SeminoleFL_Maximum"] = "Maximum";
                                driver.Quit();
                                return "Maximum";
                            }
                        }
                        catch { }
                        try
                        {
                            //No Data Found
                            string nodata = driver.FindElement(By.XPath("//*[@id='dnn_ctr446_View_callbackSearch_gridResult_DXEmptyRow']/td[1]/div")).Text;
                            if (nodata.Contains("No Address by name"))
                            {
                                HttpContext.Current.Session["Nodata_SeminoleFL"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }

                    try
                    {
                        string nodata = driver.FindElement(By.Id("dnn_ctr446_View_callbackSearch_gridResult_DXMainTable")).Text;
                        if (nodata.Contains("No Parcel") || nodata.Contains("No Owner") || nodata.Contains("No Address"))
                        {
                            HttpContext.Current.Session["Nodata_SeminoleFL"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }

                    //Property Details    

                    string ParcelID = "", OwnerName = "", PropertyAddress = "", MailingAddress = "", Subdivisionname = "", Taxdistrict = "", Propertytype = "", Exemption = "", Yearbuilt = "", Legaldescription = "";

                    IWebElement PropertyTB = driver.FindElement(By.XPath("//*[@id='ParcelInfo']/tbody"));
                    IList<IWebElement> PropertyTR = PropertyTB.FindElements(By.TagName("tr"));
                    IList<IWebElement> PropertyTD;

                    foreach (IWebElement Property in PropertyTR)
                    {
                        PropertyTD = Property.FindElements(By.TagName("td"));

                        if (Property.Text.Contains("Parcel"))
                        {
                            ParcelID = PropertyTD[0].Text;
                        }
                        if (Property.Text.Contains("Owner(s)"))
                        {
                            OwnerName = PropertyTD[0].Text.Replace("- Trust :50", "");
                        }
                        if (Property.Text.Contains("Property Address"))
                        {
                            PropertyAddress = PropertyTD[0].Text;
                        }
                        if (Property.Text.Contains("Mailing"))
                        {
                            MailingAddress = PropertyTD[0].Text;
                        }
                        if (Property.Text.Contains("Subdivision Name"))
                        {
                            Subdivisionname = PropertyTD[0].Text;
                        }
                        if (Property.Text.Contains("Tax District"))
                        {
                            Taxdistrict = PropertyTD[0].Text;
                        }
                        if (Property.Text.Contains("DOR Use Code"))
                        {
                            Propertytype = PropertyTD[0].Text;
                        }
                        if (Property.Text.Contains("Exemptions"))
                        {
                            Exemption = PropertyTD[0].Text;
                        }
                    }

                    try
                    {
                        driver.FindElement(By.Id("ctl00_Content_PageControl1_T1T")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, ParcelID, "Legal Description", driver, "FL", "Seminole");
                    }
                    catch { }
                    Legaldescription = driver.FindElement(By.Id("ctl00_Content_PageControl1_txtLegalInfo")).Text.Trim();
                    try
                    {
                        driver.FindElement(By.Id("ctl00_Content_PageControl1_T3T")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, ParcelID, "Building Information", driver, "FL", "Seminole");
                    }
                    catch { }
                    try
                    {
                        Yearbuilt = driver.FindElement(By.XPath("//*[@id='ctl00_Content_PageControl1_grdResBldg_DXDataRow0']/td[3]")).Text.Trim();
                    }
                    catch { }

                    string propertydetails = OwnerName + "~" + PropertyAddress + "~" + MailingAddress + "~" + Subdivisionname + "~" + Taxdistrict + "~" + Propertytype + "~" + Exemption + "~" + Legaldescription + "~" + Yearbuilt;
                    gc.insert_date(orderNumber, ParcelID, 1627, propertydetails, 1, DateTime.Now);
                    gc.CreatePdf(orderNumber, ParcelID, "Property Page Pdf", driver, "FL", "Seminole");
                    //Assessment Details
                    try
                    {
                        driver.FindElement(By.Id("ctl00_Content_PageControl1_T0T")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, ParcelID, "Value Summary", driver, "FL", "Seminole");
                    }
                    catch { }

                    string title = "", value1 = "", value2 = "", value3 = "";
                    IWebElement Propinfo1 = driver.FindElement(By.Id("ctl00_Content_PageControl1_gridValue_DXMainTable"));
                    IList<IWebElement> TRPropinfo1 = Propinfo1.FindElements(By.TagName("tr"));
                    IList<IWebElement> AherfProp;
                    foreach (IWebElement row in TRPropinfo1)
                    {
                        AherfProp = row.FindElements(By.TagName("td"));

                        if (AherfProp.Count != 0 && AherfProp.Count == 2 && AherfProp[0].Text.Trim() != "")
                        {
                            title += AherfProp[0].Text.Trim() + "~";
                        }
                        if (AherfProp.Count != 0 && AherfProp.Count == 4 && AherfProp[0].Text.Trim() != "")
                        {
                            value1 = AherfProp[0].Text.Trim();
                            value2 = AherfProp[1].Text.Trim();
                            value3 = AherfProp[2].Text.Trim();
                            string Assessmentdetails = value1 + "~" + value2 + "~" + value3;
                            gc.insert_date(orderNumber, ParcelID, 1629, Assessmentdetails, 1, DateTime.Now);
                        }
                    }
                    title = title.TrimEnd('~');
                    db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + "Year~" + title + "' where Id = '" + 1629 + "'");
                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, ParcelID, "Tax Information Page2", driver, "FL", "Seminole");
                    //Tax Information Details
                    //Tax Infor Datas
                    try
                    {
                        driver.FindElement(By.Id("ctl00_Content_btnTaxBill")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);
                    }
                    catch { }
                    string Parcelid = "", Owner = "", Propertyadd = "", Taxyear = "", Billnumber = "", NonSchoolAssessedValue = "", SchoolAssessedValue = "", GrossTaxAmount = "", MillageCode = "", ExemptionsGranted = "", Homestead = "", AdditionalExemptions = "", NonAdValoremAssessments = "", TaxStatus = "", DelinquentTaxes = "";
                    driver.SwitchTo().Window(driver.WindowHandles.Last());
                    gc.CreatePdf(orderNumber, ParcelID, "Tax Bill Details111", driver, "FL", "Seminole");
                    //navBarHeader_GC0.
                    try
                    {
                        IWebElement TaxTB = driver.FindElement(By.XPath("/html/body/div/main/div[2]/div/table[1]/tbody"));
                        IList<IWebElement> TaxTR = TaxTB.FindElements(By.TagName("tr"));
                        IList<IWebElement> TaxTD;

                        foreach (IWebElement Tax in TaxTR)
                        {
                            TaxTD = Tax.FindElements(By.TagName("td"));

                            if (Tax.Text.Contains("Parcel"))
                            {
                                Parcelid = TaxTD[1].Text;
                            }
                            if (Tax.Text.Contains("Property Address:"))
                            {
                                string Owner1 = TaxTD[1].Text;
                                string[] Owner1split = Owner1.Split('\r');
                                Propertyadd = Owner1split[0].Trim();
                            }
                            if (Tax.Text.Contains("Owner & Mailing Address:"))
                            {
                                string Owner2 = TaxTD[1].Text;
                                string[] Owner2split = Owner2.Split('\r');
                                Owner = Owner2split[0].Trim();
                            }
                            if (Tax.Text.Contains("Tax Year:"))
                            {
                                Taxyear = TaxTD[1].Text;
                            }
                            if (Tax.Text.Contains("Tax Bill #:"))
                            {
                                Billnumber = TaxTD[1].Text;
                            }
                            if (Tax.Text.Contains("Non-School Assessed Value:"))
                            {
                                NonSchoolAssessedValue = TaxTD[1].Text;
                            }
                            if (Tax.Text.Contains("School Assessed Value:") && !Tax.Text.Contains("Non-School Assessed Value:"))
                            {
                                SchoolAssessedValue = TaxTD[1].Text;
                            }
                            if (Tax.Text.Contains("Gross Tax Amount:"))
                            {
                                GrossTaxAmount = TaxTD[1].Text;
                            }
                            if (Tax.Text.Contains("Millage Code:"))
                            {
                                MillageCode = TaxTD[1].Text;
                            }
                            if (Tax.Text.Contains("Exemptions Granted:"))
                            {
                                ExemptionsGranted = TaxTD[1].Text;
                            }
                            if (Tax.Text.Contains("Homestead:"))
                            {
                                Homestead = TaxTD[1].Text;
                            }
                            if (Tax.Text.Contains("Additional Exemptions:"))
                            {
                                AdditionalExemptions = TaxTD[1].Text;
                            }
                            if (Tax.Text.Contains("Non-Ad Valorem Assessments:"))
                            {
                                NonAdValoremAssessments = TaxTD[1].Text;
                            }
                        }
                    }
                    catch { }
                    try
                    {
                        TaxStatus = driver.FindElement(By.XPath("/html/body/div/main/div[2]/div/h4[1]")).Text.Trim();
                    }
                    catch { }
                    try
                    {
                        if (TaxStatus == "")
                        {
                            TaxStatus = driver.FindElement(By.XPath("/html/body/div/main/div[2]/div/h4[2]")).Text.Trim();
                        }
                    }
                    catch { }
                    try
                    {
                        DelinquentTaxes = driver.FindElement(By.XPath("/html/body/div/main/div[2]/div/h4[6]")).Text.Trim().Replace("Prior Years Unpaid Delinquent Taxes:", "");
                    }
                    catch { }
                    try
                    {
                        if (DelinquentTaxes == "")
                        {
                            DelinquentTaxes = "Delinquent Tax";
                        }
                    }
                    catch { }

                    string Taxinfordetails = Owner + "~" + Propertyadd + "~" + Taxyear + "~" + Billnumber + "~" + NonSchoolAssessedValue + "~" + SchoolAssessedValue + "~" + GrossTaxAmount + "~" + MillageCode + "~" + ExemptionsGranted + "~" + Homestead + "~" + AdditionalExemptions + "~" + NonAdValoremAssessments + "~" + TaxStatus + "~" + DelinquentTaxes;
                    gc.insert_date(orderNumber, ParcelID, 1630, Taxinfordetails, 1, DateTime.Now);
                    gc.CreatePdf(orderNumber, ParcelID, "Property Page Pdf", driver, "FL", "Seminole");

                    //Due Date Details 1 
                    try
                    {
                        string pleaspay1 = "", pleaspay2 = "", pleaspay3 = "", pleaspay4 = "", pleaspay5 = "";
                        IWebElement dueTB = driver.FindElement(By.XPath("/html/body/div/main/div[2]/div/table[3]/tbody"));
                        IList<IWebElement> dueTR = dueTB.FindElements(By.TagName("tr"));
                        IList<IWebElement> dueTD;
                        IList<IWebElement> dueTH;
                        foreach (IWebElement due1 in dueTR)
                        {
                            dueTD = due1.FindElements(By.TagName("td"));
                            dueTH = due1.FindElements(By.TagName("th"));
                            if (due1.Text.Trim() != "")
                            {
                                if (!due1.Text.Contains("Year") && dueTD.Count == 5 && dueTD.Count != 0 && due1.Text.Trim() != "")
                                {
                                    pleaspay1 += dueTD[0].Text + "~";
                                    pleaspay2 += dueTD[1].Text + "~";
                                    pleaspay3 += dueTD[2].Text + "~";
                                    pleaspay4 += dueTD[3].Text + "~";
                                    pleaspay5 += dueTD[4].Text + "~";
                                }

                            }
                        }
                        if (pleaspay1 != "")
                        {
                            pleaspay1 = pleaspay1.TrimEnd('~');
                            pleaspay2 = pleaspay2.TrimEnd('~');
                            pleaspay3 = pleaspay3.TrimEnd('~');
                            pleaspay4 = pleaspay4.TrimEnd('~');
                            pleaspay5 = pleaspay5.TrimEnd('~');

                            gc.insert_date(orderNumber, ParcelID, 1631, pleaspay1.Remove(pleaspay1.Length - 1, 1), 1, DateTime.Now);
                            gc.insert_date(orderNumber, ParcelID, 1631, pleaspay2.Remove(pleaspay2.Length - 1, 1), 1, DateTime.Now);
                            gc.insert_date(orderNumber, ParcelID, 1631, pleaspay3.Remove(pleaspay3.Length - 1, 1), 1, DateTime.Now);
                            gc.insert_date(orderNumber, ParcelID, 1631, pleaspay4.Remove(pleaspay4.Length - 1, 1), 1, DateTime.Now);
                            gc.insert_date(orderNumber, ParcelID, 1631, pleaspay5.Remove(pleaspay5.Length - 1, 1), 1, DateTime.Now);
                        }
                    }
                    catch { }

                    //Due Date Details 2 xpath different
                    try
                    {
                        string pleaspay1 = "", pleaspay2 = "", pleaspay3 = "", pleaspay4 = "", pleaspay5 = "";
                        IWebElement dueTB = driver.FindElement(By.XPath("/html/body/div/main/div[2]/div/table[2]/tbody"));
                        IList<IWebElement> dueTR = dueTB.FindElements(By.TagName("tr"));
                        IList<IWebElement> dueTD;
                        IList<IWebElement> dueTH;
                        foreach (IWebElement due1 in dueTR)
                        {
                            dueTD = due1.FindElements(By.TagName("td"));
                            dueTH = due1.FindElements(By.TagName("th"));
                            if (due1.Text.Trim() != "" && !due1.Text.Contains("Date"))
                            {
                                if (!due1.Text.Contains("Date") && dueTD.Count == 5 && dueTD.Count != 0 && due1.Text.Trim() != "")
                                {
                                    pleaspay1 += dueTD[0].Text + "~";
                                    pleaspay2 += dueTD[1].Text + "~";
                                    pleaspay3 += dueTD[2].Text + "~";
                                    pleaspay4 += dueTD[3].Text + "~";
                                    pleaspay5 += dueTD[4].Text + "~";
                                }
                            }
                        }
                        if (pleaspay1 != "")
                        {
                            pleaspay1 = pleaspay1.TrimEnd('~');
                            pleaspay2 = pleaspay2.TrimEnd('~');
                            pleaspay3 = pleaspay3.TrimEnd('~');
                            pleaspay4 = pleaspay4.TrimEnd('~');
                            pleaspay5 = pleaspay5.TrimEnd('~');

                            gc.insert_date(orderNumber, ParcelID, 1631, pleaspay1.Remove(pleaspay1.Length - 1, 1), 1, DateTime.Now);
                            gc.insert_date(orderNumber, ParcelID, 1631, pleaspay2.Remove(pleaspay2.Length - 1, 1), 1, DateTime.Now);
                            gc.insert_date(orderNumber, ParcelID, 1631, pleaspay3.Remove(pleaspay3.Length - 1, 1), 1, DateTime.Now);
                            gc.insert_date(orderNumber, ParcelID, 1631, pleaspay4.Remove(pleaspay4.Length - 1, 1), 1, DateTime.Now);
                            gc.insert_date(orderNumber, ParcelID, 1631, pleaspay5.Remove(pleaspay5.Length - 1, 1), 1, DateTime.Now);
                            pleaspay1 = ""; pleaspay2 = ""; pleaspay3 = ""; pleaspay4 = ""; pleaspay5 = "";
                        }

                    }
                    catch { }
                    //Installment Plan                   
                    try
                    {
                        IWebElement bulkdata = driver.FindElement(By.XPath("/html/body/div/main/div[2]/div/h4[3]"));
                        string Installment = GlobalClass.After(bulkdata.Text, "PM ON").Trim();
                        string Pay = gc.Between(bulkdata.Text, "Amount Due:", "if payment").Trim();
                        string Paid = GlobalClass.After(bulkdata.Text, "POSTMARKED by").Trim();
                        string Ifpaid = Installment + " " + Paid;
                        string Duedatedetails = Ifpaid + "~" + Pay;
                        gc.insert_date(orderNumber, ParcelID, 1631, Duedatedetails, 1, DateTime.Now);
                    }
                    catch { }
                    //Payment Details xpath different
                    try
                    {
                        string Paiddate1 = "", Paiddate2 = "", Paiddate3 = "", ReceiptNumber1 = "", ReceiptNumber2 = "", ReceiptNumber3 = "", PaidAmount1 = "", PaidAmount2 = "", PaidAmount3 = "";
                        IWebElement dueTB = driver.FindElement(By.XPath("/html/body/div/main/div[2]/div/table[2]/tbody"));
                        IList<IWebElement> dueTR = dueTB.FindElements(By.TagName("tr"));
                        IList<IWebElement> dueTD;
                        IList<IWebElement> dueTH;
                        foreach (IWebElement due in dueTR)
                        {
                            dueTD = due.FindElements(By.TagName("td"));
                            dueTH = due.FindElements(By.TagName("th"));
                            if (!due.Text.Contains("Date") && dueTD.Count == 6 && dueTD.Count != 0 && due.Text.Trim() != "")
                            {
                                try
                                {
                                    if (dueTD[0].Text.Trim() != "" && dueTD[1].Text.Trim() != "" && dueTD[2].Text.Trim() != "")
                                    {
                                        Paiddate1 = dueTD[0].Text;
                                        ReceiptNumber1 = dueTD[1].Text;
                                        PaidAmount1 = dueTD[2].Text;
                                        gc.insert_date(orderNumber, ParcelID, 1660, Paiddate1 + "~" + ReceiptNumber1 + "~" + PaidAmount1, 1, DateTime.Now);
                                    }
                                }
                                catch { }
                                try
                                {
                                    if (dueTD[3].Text.Trim() != "" && dueTD[4].Text.Trim() != "" && dueTD[5].Text.Trim() != "")
                                    {
                                        Paiddate2 = dueTD[3].Text;
                                        ReceiptNumber2 = dueTD[4].Text;
                                        PaidAmount2 = dueTD[5].Text;
                                        gc.insert_date(orderNumber, ParcelID, 1660, Paiddate2 + "~" + ReceiptNumber2 + "~" + PaidAmount2, 1, DateTime.Now);
                                    }
                                }
                                catch { }
                                Paiddate2 = ""; Paiddate3 = ""; ReceiptNumber1 = ""; ReceiptNumber2 = ""; ReceiptNumber3 = ""; PaidAmount1 = ""; PaidAmount2 = ""; PaidAmount3 = "";
                            }
                        }
                    }
                    catch { }
                    //Delinquent Details2 xpath different
                    try
                    {///html/body/div/main/div[2]/div/table[3]/tbody
                        //Year~Certification Number~Payoff~Paid By

                        string Year = "", Cert = "", Payoff1 = "", Payoff2 = "", Payoff3 = "", Paidby1 = "", Paidby2 = "", Paidby3 = "";
                        IWebElement dueTB = driver.FindElement(By.XPath("/html/body/div/main/div[2]/div/table[2]/tbody"));
                        IList<IWebElement> dueTR = dueTB.FindElements(By.TagName("tr"));
                        IList<IWebElement> dueTD;
                        IList<IWebElement> dueTH;
                        foreach (IWebElement due in dueTR)
                        {
                            dueTD = due.FindElements(By.TagName("td"));
                            dueTH = due.FindElements(By.TagName("th"));
                            if (!due.Text.Contains("Date") && !due.Text.Contains("Year") && dueTD.Count == 8 && dueTD.Count != 0 && due.Text.Trim() != "")
                            {
                                Year = dueTD[0].Text + "~";
                                Cert = dueTD[1].Text + "~";
                                Payoff1 = dueTD[2].Text + "~";
                                Payoff2 = dueTD[4].Text + "~";
                                Payoff3 = dueTD[6].Text + "~";
                                Paidby1 = dueTD[3].Text + "~";
                                Paidby2 = dueTD[5].Text + "~";
                                Paidby3 = dueTD[7].Text + "~";
                                Year = Year.TrimEnd('~');
                                Cert = Cert.TrimEnd('~');
                                Payoff1 = Payoff1.TrimEnd('~');
                                Paidby1 = Paidby1.TrimEnd('~');
                                Payoff2 = Payoff2.TrimEnd('~');
                                Paidby2 = Paidby2.TrimEnd('~');
                                Payoff3 = Payoff3.TrimEnd('~');
                                Paidby3 = Paidby3.TrimEnd('~');
                                gc.insert_date(orderNumber, ParcelID, 1632, Year + "~" + Cert + "~" + Payoff1 + "~" + Paidby1, 1, DateTime.Now);
                                gc.insert_date(orderNumber, ParcelID, 1632, "" + "~" + "" + "~" + Payoff2 + "~" + Paidby2, 1, DateTime.Now);
                                gc.insert_date(orderNumber, ParcelID, 1632, "" + "~" + "" + "~" + Payoff3 + "~" + Paidby3, 1, DateTime.Now);
                                Year = ""; Cert = ""; Payoff1 = ""; Paidby1 = ""; Payoff2 = ""; Payoff3 = ""; Paidby2 = ""; Paidby3 = "";
                            }
                        }

                    }
                    catch { }

                    try
                    {///html/body/div/main/div[2]/div/table[3]/tbody
                        string Year = "", Cert = "", Payoff1 = "", Payoff2 = "", Payoff3 = "", Paidby1 = "", Paidby2 = "", Paidby3 = "";
                        IWebElement dueTB1 = driver.FindElement(By.XPath("/html/body/div/main/div[2]/div/table[3]/tbody"));
                        IList<IWebElement> dueTR1 = dueTB1.FindElements(By.TagName("tr"));
                        IList<IWebElement> dueTD1;
                        IList<IWebElement> dueTH1;
                        foreach (IWebElement due1 in dueTR1)
                        {
                            dueTD1 = due1.FindElements(By.TagName("td"));
                            dueTH1 = due1.FindElements(By.TagName("th"));
                            if (!due1.Text.Contains("Date") && !due1.Text.Contains("Year") && dueTD1.Count == 8 && dueTD1.Count != 0 && due1.Text.Trim() != "")
                            {
                                Year = dueTD1[0].Text + "~";
                                Cert = dueTD1[1].Text + "~";
                                Payoff1 = dueTD1[2].Text + "~";
                                Payoff2 = dueTD1[4].Text + "~";
                                Payoff3 = dueTD1[6].Text + "~";
                                Paidby1 = dueTD1[3].Text + "~";
                                Paidby2 = dueTD1[5].Text + "~";
                                Paidby3 = dueTD1[7].Text + "~";
                                Year = Year.TrimEnd('~');
                                Cert = Cert.TrimEnd('~');
                                Payoff1 = Payoff1.TrimEnd('~');
                                Paidby1 = Paidby1.TrimEnd('~');
                                Payoff2 = Payoff2.TrimEnd('~');
                                Paidby2 = Paidby2.TrimEnd('~');
                                Payoff3 = Payoff3.TrimEnd('~');
                                Paidby3 = Paidby3.TrimEnd('~');
                                gc.insert_date(orderNumber, ParcelID, 1632, Year + "~" + Cert + "~" + Payoff1 + "~" + Paidby1, 1, DateTime.Now);
                                gc.insert_date(orderNumber, ParcelID, 1632, "" + "~" + "" + "~" + Payoff2 + "~" + Paidby2, 1, DateTime.Now);
                                gc.insert_date(orderNumber, ParcelID, 1632, "" + "~" + "" + "~" + Payoff3 + "~" + Paidby3, 1, DateTime.Now);
                                Year = ""; Cert = ""; Payoff1 = ""; Paidby1 = ""; Payoff2 = ""; Payoff3 = ""; Paidby2 = ""; Paidby3 = "";
                            }
                        }

                    }
                    catch { }
                    string latestfile = "";
                    string fileName = "";
                    string fileName1 = "";
                    //Pdf Download
                    string current1 = driver.CurrentWindowHandle;
                    try
                    {
                        var chromeOptions = new ChromeOptions();
                        var downloadDirectory = ConfigurationManager.AppSettings["AutoPdf"];
                        chromeOptions.AddUserProfilePreference("download.default_directory", downloadDirectory);
                        chromeOptions.AddUserProfilePreference("download.prompt_for_download", false);
                        chromeOptions.AddUserProfilePreference("disable-popup-blocking", "true");
                        chromeOptions.AddUserProfilePreference("plugins.always_open_pdf_externally", true);
                        var chDriver = new ChromeDriver(chromeOptions);
                        Array.ForEach(Directory.GetFiles(@downloadDirectory), File.Delete);
                        chDriver.Navigate().GoToUrl(driver.Url);
                        Thread.Sleep(2000);
                        try
                        {
                            IWebElement Multyaddresstable2 = chDriver.FindElement(By.Id("scheduler-embed"));
                            chDriver.SwitchTo().Frame(Multyaddresstable2);
                            SelectElement ss1 = new SelectElement(chDriver.FindElement(By.Id("search-type")));
                            ss1.SelectByText("Property Tax");

                            chDriver.FindElement(By.Id("property-parcel")).SendKeys(ParcelID);
                            chDriver.FindElement(By.XPath("//*[@id='property-search']/button[3]")).Click();
                            Thread.Sleep(4000);
                            chDriver.FindElement(By.XPath("/html/body/div/main/div[2]/div/table/tbody/tr[2]/td[2]/a")).Click();
                            Thread.Sleep(4000);
                        }
                        catch { }
                        IWebElement ISpan121 = chDriver.FindElement(By.XPath("//*[@id='ViewBill']/input"));
                        IJavaScriptExecutor js121 = chDriver as IJavaScriptExecutor;
                        js121.ExecuteScript("arguments[0].click();", ISpan121);
                        Thread.Sleep(15000);
                        try

                        {
                            chDriver.FindElement(By.XPath("//*[@id='main-content']/a")).Click();
                            Thread.Sleep(6000);
                        }
                        catch { }
                        fileName = latestfilename();
                        Thread.Sleep(2000);
                        gc.AutoDownloadFile(orderNumber, ParcelID, "Seminole", "FL", fileName);
                        chDriver.Quit();
                    }
                    catch { }
                    //Prior Year Tax Information Details
                    ////*[@id="ViewHist2"]/input[1]
                    //driver.Navigate().GoToUrl("http://www.seminoletax.org/pay-taxes/#");
                    //Thread.Sleep(2000);
                    //SelectElement ss = new SelectElement(driver.FindElement(By.XPath("//*[@id='search-type']")));
                    //ss.SelectByText("Property Tax");
                    //driver.FindElement(By.Id("property-parcel")).SendKeys(ParcelID);
                    //driver.FindElement(By.XPath("//*[@id='property-search']/button[3]")).Click();
                    //Thread.Sleep(4000);
                    //gc.CreatePdf(orderNumber, ParcelID, "Tax Information Page1", driver, "FL", "Seminole");

                    //driver.FindElement(By.XPath("/html/body/div/main/div[2]/div/table/tbody/tr[2]/td[2]/a")).Click();
                    //Thread.Sleep(4000);
                    //gc.CreatePdf(orderNumber, ParcelID, "Tax Information Page2", driver, "FL", "Seminole");

                    driver.SwitchTo().Window(current1);
                    try
                    {

                        IWebElement ISpan1212 = driver.FindElement(By.XPath("//*[@id='ViewHist2']/input[1]"));
                        IJavaScriptExecutor js1212 = driver as IJavaScriptExecutor;
                        js1212.ExecuteScript("arguments[0].click();", ISpan1212);
                        Thread.Sleep(2000);
                    }
                    catch { }
                    try
                    {

                        IWebElement ISpan1212 = driver.FindElement(By.XPath("//*[@id='Bill']/td[3]"));
                        IJavaScriptExecutor js1212 = driver as IJavaScriptExecutor;
                        js1212.ExecuteScript("arguments[0].click();", ISpan1212);
                        Thread.Sleep(15000);
                    }
                    catch { }

                    //driver.FindElement(By.XPath("//*[@id='Bill']/td[3]")).Click();
                    //Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, ParcelID, "Prior Year Tax Page Pdf", driver, "FL", "Seminole");
                    List<string> billinfo = new List<string>();
                    IWebElement Billsinfo2 = driver.FindElement(By.XPath("/html/body/div/main/div[2]/div/table/tbody"));
                    IList<IWebElement> TRBillsinfo2 = Billsinfo2.FindElements(By.TagName("tr"));
                    IList<IWebElement> Aherftax;
                    int i = 0;
                    foreach (IWebElement row in TRBillsinfo2)
                    {
                        Aherftax = row.FindElements(By.TagName("td"));

                        if (Aherftax.Count != 0 && Aherftax.Count == 5 && !row.Text.Contains("Tax Year"))
                        {
                            string Taxxyearr = Aherftax[1].Text;
                            string PersonName = Aherftax[2].Text;
                            string Address = Aherftax[3].Text;
                            string Parcel = Aherftax[4].Text;

                            string taxhisdetails = Taxxyearr.Trim() + "~" + PersonName.Trim() + "~" + Address.Trim();
                            //gc.insert_date(orderNumber, Parcel, 1314, taxhisdetails, 1, DateTime.Now);
                        }
                        if (billinfo.Count != 2 && Aherftax.Count != 0 && Aherftax.Count == 5 && !row.Text.Contains("Tax Year"))
                        {
                            IWebElement value = Aherftax[1].FindElement(By.TagName("a"));
                            string addview = value.GetAttribute("href");
                            billinfo.Add(addview);
                        }
                    }
                    string Taxyear1 = "";
                    foreach (string assessmentclick in billinfo)
                    {
                        try
                        {
                            driver.Navigate().GoToUrl(assessmentclick);
                            Thread.Sleep(2000);
                            string Parcelprior = "", Owner1 = "", Propertyadd1 = "", Billnumber1 = "", NonSchoolAssessedValue1 = "", SchoolAssessedValue1 = "", GrossTaxAmount1 = "", MillageCode1 = "", ExemptionsGranted1 = "", Homestead1 = "", AdditionalExemptions1 = "", NonAdValoremAssessments1 = "", TaxStatus1 = "", PaidDate1 = "", ReceiptNumber1 = "", PaidAmount1 = "", DelinquentTaxes1 = "";
                            //Prior Year Tax Information Details
                            IWebElement priorTB = driver.FindElement(By.Id("showamts"));
                            IList<IWebElement> priorTR = priorTB.FindElements(By.TagName("tr"));
                            IList<IWebElement> priorTD;

                            foreach (IWebElement prior in priorTR)
                            {
                                priorTD = prior.FindElements(By.TagName("td"));

                                if (priorTD.Count == 2 && prior.Text.Trim() != "")
                                {
                                    if (prior.Text.Contains("Parcel"))
                                    {
                                        Parcelprior = priorTD[1].Text;
                                    }

                                    if (prior.Text.Contains("Tax Year:"))
                                    {
                                        Taxyear1 = priorTD[1].Text;
                                    }

                                    if (prior.Text.Contains("Non-School Assessed Value:"))
                                    {
                                        NonSchoolAssessedValue1 = priorTD[1].Text;
                                    }
                                    if (prior.Text.Contains("School Assessed Value:") && !prior.Text.Contains("Non-"))
                                    {
                                        SchoolAssessedValue1 = priorTD[1].Text;
                                    }
                                    if (prior.Text.Contains("Gross Tax Amount:"))
                                    {
                                        GrossTaxAmount1 = priorTD[1].Text;
                                    }
                                    if (prior.Text.Contains("Millage Code:"))
                                    {
                                        MillageCode1 = priorTD[1].Text;
                                    }
                                    if (prior.Text.Contains("Exemptions Granted:"))
                                    {
                                        ExemptionsGranted1 = priorTD[1].Text;
                                    }
                                    if (prior.Text.Contains("Homestead:"))
                                    {
                                        Homestead1 = priorTD[1].Text;
                                    }
                                    if (prior.Text.Contains("Additional Exemptions:"))
                                    {
                                        AdditionalExemptions = priorTD[1].Text;
                                    }
                                    if (prior.Text.Contains("Non-Ad Valorem Assessments:"))
                                    {
                                        NonAdValoremAssessments1 = priorTD[1].Text;
                                    }
                                }
                            }
                            string prioryearTaxinfordetails = Taxyear1 + "~" + NonSchoolAssessedValue1 + "~" + SchoolAssessedValue1 + "~" + GrossTaxAmount1 + "~" + MillageCode1 + "~" + ExemptionsGranted1 + "~" + Homestead1+"~"+ AdditionalExemptions + "~" + NonAdValoremAssessments1;
                            gc.insert_date(orderNumber, Parcelprior, 1666, prioryearTaxinfordetails, 1, DateTime.Now);
                            gc.CreatePdf(orderNumber, Parcelprior, "Prior Year Tax Page Pdf" + Taxyear1, driver, "FL", "Seminole");
                        }
                        catch { }
                        //Prior Year Delinquent Details
                        try
                        {
                            string Paiddate1 = "", Paiddate2 = "", Paiddate3 = "", ReceiptNumber1 = "", ReceiptNumber2 = "", ReceiptNumber3 = "", PaidAmount1 = "", PaidAmount2 = "", PaidAmount3 = "";
                            IWebElement dueTB = driver.FindElement(By.Id("paid"));
                            IList<IWebElement> dueTR = dueTB.FindElements(By.TagName("tr"));
                            IList<IWebElement> dueTD;
                            IList<IWebElement> dueTH;
                            foreach (IWebElement due in dueTR)
                            {
                                dueTD = due.FindElements(By.TagName("td"));
                                dueTH = due.FindElements(By.TagName("th"));
                                if (!due.Text.Contains("Date") && dueTD.Count == 6 && dueTD.Count != 0 && due.Text.Trim() != "")
                                {
                                    try
                                    {
                                        if (dueTD[0].Text.Trim() != "" && dueTD[1].Text.Trim() != "" && dueTD[2].Text.Trim() != "")
                                        {
                                            Paiddate1 = dueTD[0].Text;
                                            ReceiptNumber1 = dueTD[1].Text;
                                            PaidAmount1 = dueTD[2].Text;
                                            gc.insert_date(orderNumber, ParcelID, 1667, Taxyear1 + "~" + Paiddate1 + "~" + ReceiptNumber1 + "~" + PaidAmount1, 1, DateTime.Now);
                                        }
                                    }
                                    catch { }
                                    try
                                    {
                                        if (dueTD[3].Text.Trim() != "" && dueTD[4].Text.Trim() != "" && dueTD[5].Text.Trim() != "")
                                        {
                                            Paiddate2 = dueTD[3].Text;
                                            ReceiptNumber2 = dueTD[4].Text;
                                            PaidAmount2 = dueTD[5].Text;
                                            gc.insert_date(orderNumber, ParcelID, 1667, Taxyear1 + "~" + Paiddate2 + "~" + ReceiptNumber2 + "~" + PaidAmount2, 1, DateTime.Now);
                                        }
                                    }
                                    catch { }
                                    Paiddate2 = ""; Paiddate3 = ""; ReceiptNumber1 = ""; ReceiptNumber2 = ""; ReceiptNumber3 = ""; PaidAmount1 = ""; PaidAmount2 = ""; PaidAmount3 = "";
                                }
                            }
                        }
                        catch { }
                        //Pdf download
                        try
                        {
                            var chromeOptions = new ChromeOptions();
                            var downloadDirectory = ConfigurationManager.AppSettings["AutoPdf"];
                            chromeOptions.AddUserProfilePreference("download.default_directory", downloadDirectory);
                            chromeOptions.AddUserProfilePreference("download.prompt_for_download", false);
                            chromeOptions.AddUserProfilePreference("disable-popup-blocking", "true");
                            chromeOptions.AddUserProfilePreference("plugins.always_open_pdf_externally", true);
                            var chDriver = new ChromeDriver(chromeOptions);
                            Array.ForEach(Directory.GetFiles(@downloadDirectory), File.Delete);
                            chDriver.Navigate().GoToUrl(driver.Url);
                            Thread.Sleep(5000);
                            //driver.FindElement(By.XPath("//*[@id='ViewBill']/div/input")).Click();
                            //Thread.Sleep(2000);

                            IWebElement ISpan12 = chDriver.FindElement(By.XPath("//*[@id='ViewBill']/div/input"));
                            IJavaScriptExecutor js12 = chDriver as IJavaScriptExecutor;
                            js12.ExecuteScript("arguments[0].click();", ISpan12);
                            Thread.Sleep(15000);

                            fileName1 = latestfilename();
                            Thread.Sleep(2000);
                            gc.AutoDownloadFile(orderNumber, ParcelID, "Seminole", "FL", fileName1);
                            chDriver.Quit();
                        }
                        catch { }
                    }

                    TaxTime = DateTime.Now.ToString("HH:mm:ss");
                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "FL", "Seminole", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);
                    driver.Quit();
                    gc.mergpdf(orderNumber, "FL", "Seminole");
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
