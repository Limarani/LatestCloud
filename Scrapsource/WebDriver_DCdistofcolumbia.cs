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
using System.ComponentModel;
using System.Text;
using HtmlAgilityPack;
using iTextSharp.text;
using System.Text.RegularExpressions;
using OpenQA.Selenium.PhantomJS;
using OpenQA.Selenium.Support.Extensions;
using System.Net;
using OpenQA.Selenium.Firefox;
using System.Diagnostics;
using OpenQA.Selenium.Remote;
using Org.BouncyCastle.Utilities;

namespace ScrapMaricopa.Scrapsource
{
    public class WebDriver_DCdistofcolumbia
    {
        string Outparcelno = "", outputPath = "", strMulti = "", strCount = "", strMultiresult = "", splitparcel = "",
        strsplitparfirst = "", strsplitparsecond = "", strsplitparthird = "";
        string strAddress = "-", strOwner = "-", strproperaddress = "-", strParcelnumber = "-", strUsecode = "-", strHomestatus = "-", strLand = "-",
        strImprovements = "-", strTotalvalue = "-", strParcel = "-", strTotalAssvalue = "-", strYear = "-", strProperYear = "-", strDesc = "-",
        strTax = "-", strPen = "-", strInter = "-", strFee = "-", strDue = "-", strCredit = "-", strPay = "-", strBal = "-", strTaxsale = "-", strDcn = "-",
        strOwnerName = "-", strSourceId = "-", strTransId = "-", strTaxyear = "-", strPaiddate = "-", strPaidamount = "-", strInterest = "-", strPenalty = "-", strTotalpaid = "-";
        string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
        IWebDriver driver;
        DBconnection db = new DBconnection();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        GlobalClass gc = new GlobalClass();


        public string FTP_WADC(string houseno, string sname, string sttype, string blockno, string parcelNumber, string searchType, string orderNumber, string directParcel)
        {
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
                        string titleaddress = houseno + " " + sname + " " + sttype + " " + blockno;
                        gc.TitleFlexSearch(orderNumber, "", directParcel, titleaddress, "DC", "District of Columbia");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_DCDC"] = "Zero";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }

                    if (searchType == "address")
                    {
                        driver.Navigate().GoToUrl("https://www.taxpayerservicecenter.com/RP_Search.jsp?search_type=Assessment");
                        Thread.Sleep(2000);
                        //*[@id='SearchForm']/table/tbody/tr/td/table[2]/tbody/tr[4]/td[2]/table/tbody/tr[1]/td[2]/input
                        driver.FindElement(By.XPath("//*[@id='SearchForm']/div/table/tbody/tr/td/table[2]/tbody/tr[3]/td[2]/table/tbody/tr[1]/td[1]/input")).SendKeys(houseno);
                        driver.FindElement(By.XPath("//*[@id='SearchForm']/div/table/tbody/tr/td/table[2]/tbody/tr[3]/td[2]/table/tbody/tr[1]/td[2]/input")).SendKeys(sname);
                        gc.CreatePdf_WOP(orderNumber, "DCAddressSearch", driver, "DC", "District of Columbia");
                        driver.FindElement(By.Id("imgSearch")).SendKeys(Keys.Enter);
                        try
                        {
                            IWebElement Imulti = driver.FindElement(By.XPath("//*[@id='TABLE1']/tbody/tr/td[2]/div/table/tbody/tr[1]/td/table/tbody/tr[1]/td[1]/font"));
                            strMulti = Imulti.Text;
                        }
                        catch { }

                        try
                        {
                            IWebElement INodata = driver.FindElement(By.XPath("//*[@id='SearchForm']/div/table/tbody/tr/td/table[1]/tbody/tr[3]/td"));
                            if (INodata.Text.Contains("No records were found"))
                            {
                                HttpContext.Current.Session["Nodata_DCDC"] = "Zero";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                        //Multiparcel for Address
                        if (strMulti.Contains("Results 1 to "))
                        {
                            try
                            {
                                strMultiresult = WebDriverTest.Between(strMulti, "Results 1 to ", " of");
                                if ((Convert.ToInt32(strMultiresult) > 1))
                                {
                                    HttpContext.Current.Session["multiparcel_dc"] = "Yes";
                                    int multicount = 0;
                                    gc.CreatePdf_WOP(orderNumber, "DCMultiPropertyDetails", driver, "DC", "District of Columbia");
                                    IWebElement ImultiTable = driver.FindElement(By.XPath("//*[@id='TABLE1']/tbody/tr/td[2]/div/table/tbody/tr[2]/td/table"));
                                    IList<IWebElement> ImultiRow = ImultiTable.FindElements(By.TagName("tr"));
                                    IList<IWebElement> ImultiTD;
                                    foreach (IWebElement rows in ImultiRow)
                                    {
                                        ImultiTD = rows.FindElements(By.TagName("td"));
                                        if (ImultiTD.Count != 0)
                                        {
                                            if (multicount <= 25)
                                            {
                                                string strParcel = ImultiTD[0].Text;
                                                string strAddress = ImultiTD[1].Text;
                                                string strOwner = ImultiTD[2].Text;

                                                string multi = strOwner + "~" + strAddress;
                                                // db.ExecuteQuery("insert into data_value_master (Order_no,parcel_no,Data_Field_Text_Id,Data_Field_value,Is_Table) values ('" + orderNumber + "','" + strParcel.Trim() + "',47 ,'" + multi + "',1)");
                                                gc.insert_date(orderNumber, strParcel.Trim(), 47, multi, 1, DateTime.Now);
                                            }
                                        }
                                        multicount++;
                                    }
                                }
                                if ((Convert.ToInt32(strMultiresult) > 25))
                                {
                                    HttpContext.Current.Session["multiparcel_dc_count"] = "Maximum";
                                    return "Maximum";
                                }
                            }

                            catch { }

                            driver.Quit();
                            return "MultiParcel";
                        }
                        else
                        {
                            IWebElement Iparcel = driver.FindElement(By.XPath("//*[@id='form1']/table/tbody/tr[2]/td/table/tbody/tr[2]/td[2]"));
                            if (Iparcel.Text != " ")
                            {
                                parcelNumber = Iparcel.Text;
                            }
                            propertyDetails(orderNumber, parcelNumber);
                        }
                    }
                    if (searchType == "parcel")
                    {
                        if (GlobalClass.titleparcel != "")
                        {
                            parcelNumber = GlobalClass.titleparcel;
                        }
                        if (parcelNumber.Contains("-"))
                        {
                            parcelNumber = parcelNumber.Replace("-", "");
                        }
                        Outparcelno = parcelNumber.Replace(" ", "");
                        driver.Navigate().GoToUrl("https://www.taxpayerservicecenter.com/RP_Search.jsp?search_type=Assessment");
                        Thread.Sleep(2000);
                        if (Outparcelno.Length == 12)
                        {
                            strsplitparfirst = Outparcelno.Substring(0, 4);
                            driver.FindElement(By.XPath("//*[@id='SearchForm']/div/table/tbody/tr/td/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td[2]/input")).SendKeys(strsplitparfirst);
                            strsplitparsecond = Outparcelno.Substring(4, 4);
                            if (strsplitparsecond == "0000")
                            {
                                strsplitparsecond = "";
                            }
                            driver.FindElement(By.XPath("//*[@id='SearchForm']/div/table/tbody/tr/td/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td[4]/input")).SendKeys(strsplitparsecond);
                            strsplitparthird = Outparcelno.Substring(8, 4);
                            driver.FindElement(By.XPath("//*[@id='SearchForm']/div/table/tbody/tr/td/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td[6]/input")).SendKeys(strsplitparthird);
                            //Screenshot
                            gc.CreatePdf(orderNumber, Outparcelno, "DCParcelSearch", driver, "DC", "District of Columbia");
                            driver.FindElement(By.Id("imgSearch")).SendKeys(Keys.Enter);

                            try
                            {
                                IWebElement INodata = driver.FindElement(By.XPath("//*[@id='SearchForm']/div/table/tbody/tr/td/table[1]/tbody/tr[3]/td"));
                                if (INodata.Text.Contains("No records were found"))
                                {
                                    HttpContext.Current.Session["Nodata_DCDC"] = "Zero";
                                    driver.Quit();
                                    return "No Data Found";
                                }
                            }
                            catch { }
                            ParcelSearch(orderNumber, Outparcelno);
                        }
                        if (Outparcelno.Length == 8)
                        {
                            strsplitparfirst = Outparcelno.Substring(0, 4);
                            driver.FindElement(By.XPath("//*[@id='SearchForm']/div/table/tbody/tr/td/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td[2]/input")).SendKeys(strsplitparfirst);
                            strsplitparsecond = Outparcelno.Substring(4, 4);
                            driver.FindElement(By.XPath("//*[@id='SearchForm']/div/table/tbody/tr/td/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td[6]/input")).SendKeys(strsplitparsecond);
                            driver.FindElement(By.Id("imgSearch")).SendKeys(Keys.Enter);
                            //Screenshot
                            gc.CreatePdf(orderNumber, Outparcelno, "DCParcelSearch", driver, "DC", "District of Columbia");

                            try
                            {
                                IWebElement INodata = driver.FindElement(By.XPath("//*[@id='SearchForm']/div/table/tbody/tr/td/table[1]/tbody/tr[3]/td"));
                                if (INodata.Text.Contains("No records were found"))
                                {
                                    HttpContext.Current.Session["Nodata_DCDC"] = "Zero";
                                    driver.Quit();
                                    return "No Data Found";
                                }
                            }
                            catch { }
                            propertyDetails(orderNumber, Outparcelno);
                        }
                    }
                    GlobalClass.titleparcel = "";

                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "DC", "District of Columbia", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);


                    driver.Quit();
                    gc.mergpdf(orderNumber, "DC", "District of Columbia");
                    return "Data Inserted Successfully";
                }
                catch (Exception ex)
                {
                    driver.Quit();
                    throw ex;
                }
            }
        }
        public void propertyDetails(string orderNumber, string parcelNumber)
        {
            try
            {
                //Property Details
                strproperaddress = driver.FindElement(By.XPath("//*[@id='form1']/table/tbody/tr[2]/td/table/tbody/tr[1]/td[2]")).Text;
                strParcelnumber = driver.FindElement(By.XPath("//*[@id='form1']/table/tbody/tr[2]/td/table/tbody/tr[2]/td[2]")).Text;
                strUsecode = driver.FindElement(By.XPath("//*[@id='form1']/table/tbody/tr[3]/td/table/tbody/tr[3]/td[2]")).Text;
                strHomestatus = driver.FindElement(By.XPath("//*[@id='form1']/table/tbody/tr[3]/td/table/tbody/tr[5]/td[2]")).Text;
                strOwnerName = driver.FindElement(By.XPath("//*[@id='form1']/table/tbody/tr[4]/td/table/tbody/tr[2]/td[2]")).Text;
                //Assessment Details
                strLand = driver.FindElement(By.XPath("//*[@id='form1']/table/tbody/tr[5]/td/table/tbody/tr[3]/td[2]")).Text;
                strImprovements = driver.FindElement(By.XPath("//*[@id='form1']/table/tbody/tr[5]/td/table/tbody/tr[4]/td[2]")).Text;
                strTotalvalue = driver.FindElement(By.XPath("//*[@id='form1']/table/tbody/tr[5]/td/table/tbody/tr[5]/td[2]")).Text;
                strTotalAssvalue = driver.FindElement(By.XPath("//*[@id='form1']/table/tbody/tr[5]/td/table/tbody/tr[6]/td[2]")).Text;
                gc.CreatePdf(orderNumber, parcelNumber, "DCAssessmentDetails", driver, "DC", "District of Columbia");
                IWebElement Iviewtax = driver.FindElement(By.XPath("//font[contains(text(),'View Property Features')]"));
                IJavaScriptExecutor js = driver as IJavaScriptExecutor;
                js.ExecuteScript("arguments[0].click();", Iviewtax);
                Thread.Sleep(2000);
                gc.CreatePdf(orderNumber, parcelNumber, "DCPropertyFeatures", driver, "DC", "District of Columbia");
                IWebElement Iyear = driver.FindElement(By.XPath("//*[@id='frm_acct_sum']/table/tbody/tr[4]/td/table[1]/tbody/tr[5]/td[2]"));
                strYear = Iyear.Text;
                //Property Details 
                string strPropertyDetails = strOwnerName + "~" + strproperaddress + "~" + strUsecode + "~" + strHomestatus + "~" + strYear;
                // db.ExecuteQuery("insert into data_value_master(Order_no,parcel_no,Data_Field_Text_Id,Data_Field_value,Is_Table,Sequence) Values('" + orderNumber + "','" + parcelNumber + "',26,'" + strPropertyDetails + "',1,1)");
                gc.insert_date(orderNumber, parcelNumber, 26, strPropertyDetails, 1, DateTime.Now);
                //Assessment Details 
                string strAssementDetails = strLand + "~" + strImprovements + "~" + strTotalvalue + "~" + strTotalAssvalue;
                // db.ExecuteQuery("insert into data_value_master(Order_no,parcel_no,Data_Field_Text_Id,Data_Field_value,Is_Table,Sequence) Values('" + orderNumber + "','" + parcelNumber + "',27,'" + strAssementDetails + "',1,1)");
                gc.insert_date(orderNumber, parcelNumber, 27, strAssementDetails, 1, DateTime.Now);
                AssessmentTime = DateTime.Now.ToString("HH:mm:ss");

                try
                {
                    IWebElement Iviewinform = driver.FindElement(By.LinkText("View Tax Information"));
                    js.ExecuteScript("arguments[0].click();", Iviewinform);
                    Thread.Sleep(3000);
                    IWebElement Irealpath = driver.FindElement(By.XPath("/html/body/div/table/tbody/tr/td[2]/div/form/table/tbody/tr[1]/td/table[1]/tbody/tr[3]/td/table/tbody/tr[1]/td[1]/a/img"));
                    Irealpath.GetAttribute("href");
                    //Screenshot
                    js.ExecuteScript("arguments[0].click();", Irealpath);
                    gc.CreatePdf(orderNumber, parcelNumber, "DCViewTaxInformation", driver, "DC", "District of Columbia");
                    Thread.Sleep(2000);
                    //Screenshot
                    gc.CreatePdf(orderNumber, parcelNumber, "DCViewRealPropertyTax", driver, "DC", "District of Columbia");
                    int count = 0;

                    IWebElement IDcn = driver.FindElement(By.XPath("//*[@id='frm_acct_sum']/table/tbody/tr[3]/td/table/tbody/tr/td[5]"));
                    strDcn = IDcn.Text;
                    if (strDcn == " ")
                    {
                        strDcn = "-";
                    }

                    IWebElement Irealtaxtable = driver.FindElement(By.XPath("//*[@id='frm_acct_sum']/table/tbody/tr[2]/td/table[2]/tbody"));
                    IList<IWebElement> Irealtaxrow = Irealtaxtable.FindElements(By.TagName("tr"));
                    IList<IWebElement> IrealtaxTD;
                    foreach (IWebElement taxrows in Irealtaxrow)
                    {
                        IrealtaxTD = taxrows.FindElements(By.TagName("td"));
                        if (IrealtaxTD.Count != 0)
                        {
                            if (count < 2)
                            {
                                strDesc = IrealtaxTD[0].Text;
                                strTax = IrealtaxTD[1].Text;
                                strPen = IrealtaxTD[2].Text;
                                strInter = IrealtaxTD[3].Text;
                                strFee = IrealtaxTD[4].Text;
                                strDue = IrealtaxTD[5].Text;
                                strCredit = IrealtaxTD[6].Text;
                                strPay = IrealtaxTD[7].Text;
                                strBal = IrealtaxTD[8].Text;
                                if (IrealtaxTD[9].Text != " ")
                                {
                                    strTaxsale = IrealtaxTD[9].Text;
                                }

                                //Tax Installment Information Details
                                string strTaxinformation = strDcn + "~" + strDesc + "~" + strTax + "~" + strPen + "~" + strInter + "~" + strFee + "~" + strDue + "~" + strCredit + "~" + strPay + "~" + strBal + "~" + strTaxsale;
                                // db.ExecuteQuery("insert into data_value_master (Order_no,parcel_no,Data_Field_Text_Id,Data_Field_value,Is_Table,Sequence) values ('" + orderNumber + "','" + parcelNumber + "',28 ,'" + strTaxinformation + "',1,1)");
                                gc.insert_date(orderNumber, parcelNumber, 28, strTaxinformation, 1, DateTime.Now);
                            }
                            count++;
                        }
                    }
                }
                catch (Exception e)
                { }
                try
                {
                    string strDeposite = "", strPaymentType = "", strPaymentAmount = "", strStatus = "";
                    IWebElement Iviewpay = driver.FindElement(By.LinkText("View Payments"));
                    IJavaScriptExecutor js1 = driver as IJavaScriptExecutor;
                    js1.ExecuteScript("arguments[0].click();", Iviewpay);
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, parcelNumber, "Payment Details", driver, "DC", "District of Columbia");
                    IWebElement IrealPaytable = driver.FindElement(By.XPath("//*[@id='TABLE1']/tbody/tr/td[2]/div/table[1]/tbody"));
                    IList<IWebElement> Irealpaytrow = IrealPaytable.FindElements(By.TagName("tr"));
                    IList<IWebElement> IrealpayTD;
                    foreach (IWebElement taxrows in Irealpaytrow)
                    {
                        IrealpayTD = taxrows.FindElements(By.TagName("td"));
                        if (IrealpayTD.Count > 3 && !taxrows.Text.Contains("displayed on this page") && IrealpayTD.Count == 5)
                        {
                            strDeposite = IrealpayTD[0].Text;
                            strTransId = IrealpayTD[1].Text;
                            strPaymentType = IrealpayTD[2].Text;
                            strPaymentAmount = IrealpayTD[3].Text;
                            strStatus = IrealpayTD[4].Text;

                            //Tax History Details
                            string strTaxHistory = strDeposite + "~" + strTransId + "~" + strPaymentType + "~" + strPaymentAmount + "~" + strStatus;
                            // db.ExecuteQuery("insert into data_value_master (Order_no,parcel_no,Data_Field_Text_Id,Data_Field_value,Is_Table,Sequence) values ('" + orderNumber + "','" + parcelNumber + "',33 ,'" + strTaxHistory + "',1,1)");
                            gc.insert_date(orderNumber, parcelNumber, 33, strTaxHistory, 1, DateTime.Now);
                        }
                        //if (IrealpayTD.Count > 3 && !taxrows.Text.Contains("displayed on this page") && IrealpayTD.Count == 8)
                        //{
                        //    strSourceId = IrealpayTD[0].Text;
                        //    strTransId = IrealpayTD[1].Text;
                        //    strTaxyear = IrealpayTD[2].Text;
                        //    strPaiddate = IrealpayTD[3].Text;
                        //    strPaidamount = IrealpayTD[4].Text;
                        //    strInterest = IrealpayTD[5].Text;
                        //    strPenalty = IrealpayTD[6].Text;
                        //    strTotalpaid = IrealpayTD[7].Text;

                        //    Tax History Details
                        //    string strTaxHistory = strSourceId + "~" + strTransId + "~" + strTaxyear + "~" + strPaiddate + "~" + strPaidamount + "~" + strInterest + "~" + strPenalty + "~" + strTotalpaid;
                        //    db.ExecuteQuery("insert into data_value_master (Order_no,parcel_no,Data_Field_Text_Id,Data_Field_value,Is_Table,Sequence) values ('" + orderNumber + "','" + parcelNumber + "',33 ,'" + strTaxHistory + "',1,1)");
                        //    gc.insert_date(orderNumber, parcelNumber, 33, strTaxHistory, 1, DateTime.Now);
                        //}
                    }
                }
                catch { }

                try
                {
                    IWebElement url = driver.FindElement(By.LinkText("View Current Tax Bill"));
                    string URL = url.GetAttribute("href");
                    gc.downloadfile(URL, orderNumber, parcelNumber, "CurrentTax_Bill", "DC", "District of Columbia");
                }
                catch { }

          
            }
            catch (Exception ex)
            {
                driver.Quit();
                throw ex;
            }
        }

        public void ParcelSearch(string orderNumber, string parcelNumber)
        {
            try
            {
                //Property Details                        
                strproperaddress = driver.FindElement(By.XPath("//*[@id='form1']/table/tbody/tr[2]/td/table/tbody/tr[1]/td[2]")).Text;
                strParcelnumber = driver.FindElement(By.XPath("//*[@id='form1']/table/tbody/tr[2]/td/table/tbody/tr[2]/td[2]")).Text;
                strUsecode = driver.FindElement(By.XPath("//*[@id='form1']/table/tbody/tr[3]/td/table/tbody/tr[3]/td[2]")).Text;
                strHomestatus = driver.FindElement(By.XPath("//*[@id='form1']/table/tbody/tr[3]/td/table/tbody/tr[5]/td[2]")).Text;
                strOwnerName = driver.FindElement(By.XPath("//*[@id='form1']/table/tbody/tr[4]/td/table/tbody/tr[2]/td[2]")).Text;
                //Assessment Details
                strLand = driver.FindElement(By.XPath("//*[@id='form1']/table/tbody/tr[5]/td/table/tbody/tr[3]/td[2]")).Text;
                strImprovements = driver.FindElement(By.XPath("//*[@id='form1']/table/tbody/tr[5]/td/table/tbody/tr[4]/td[2]")).Text;
                strTotalvalue = driver.FindElement(By.XPath("//*[@id='form1']/table/tbody/tr[5]/td/table/tbody/tr[5]/td[2]")).Text;
                strTotalAssvalue = driver.FindElement(By.XPath("//*[@id='form1']/table/tbody/tr[5]/td/table/tbody/tr[6]/td[2]")).Text;
                //Screenshot
                gc.CreatePdf(orderNumber, parcelNumber, "DCAssessmentDetails", driver, "DC", "District of Columbia");
                //Property Details 
                if (strOwnerName == "")
                {
                    strOwnerName = "-";
                }
                if (strproperaddress == "")
                {
                    strproperaddress = "-";
                }
                if (strUsecode == "")
                {
                    strUsecode = "-";
                }
                if (strHomestatus == "")
                {
                    strHomestatus = "-";
                }
                if (strYear == "")
                {
                    strYear = "-";
                }
                string strPropertyDetails = strOwnerName + "~" + strproperaddress + "~" + strUsecode + "~" + strHomestatus + "~" + strYear;
                // db.ExecuteQuery("insert into data_value_master(Order_no,parcel_no,Data_Field_Text_Id,Data_Field_value,Is_Table,Sequence) Values('" + orderNumber + "','" + parcelNumber + "',26,'" + strPropertyDetails + "',1,1)");
                gc.insert_date(orderNumber, parcelNumber, 26, strPropertyDetails, 1, DateTime.Now);

                //Assessment Details 
                if (strLand == "")
                {
                    strLand = "-";
                }
                if (strImprovements == "")
                {
                    strImprovements = "-";
                }
                if (strTotalvalue == "")
                {
                    strTotalvalue = "-";
                }
                if (strTotalAssvalue == "")
                {
                    strTotalAssvalue = "-";
                }
                string strAssementDetails = strLand + "~" + strImprovements + "~" + strTotalvalue + "~" + strTotalAssvalue;
                //db.ExecuteQuery("insert into data_value_master(Order_no,parcel_no,Data_Field_Text_Id,Data_Field_value,Is_Table,Sequence) Values('" + orderNumber + "','" + parcelNumber + "',27,'" + strAssementDetails + "',1,1)");
                gc.insert_date(orderNumber, parcelNumber, 27, strAssementDetails, 1, DateTime.Now);

                AssessmentTime = DateTime.Now.ToString("HH:mm:ss");

                //Tax Property
                try
                {
                    IWebElement Iviewinform = driver.FindElement(By.LinkText("View Tax Information"));
                    IJavaScriptExecutor js = driver as IJavaScriptExecutor;
                    js.ExecuteScript("arguments[0].click();", Iviewinform);
                    Thread.Sleep(2000);
                    IWebElement Irealpath = driver.FindElement(By.XPath("//*[@id='frm_acct_sum']/table/tbody/tr[1]/td/table[1]/tbody/tr[3]/td/table/tbody/tr[1]/td[1]/a[1]/img"));
                    IJavaScriptExecutor js1 = driver as IJavaScriptExecutor;
                    js1.ExecuteScript("arguments[0].click();", Irealpath);
                    Thread.Sleep(2000);
                    //Irealpath.SendKeys(Keys.Enter);
                    int count = 0;

                    IWebElement IDcn = driver.FindElement(By.XPath("//*[@id='frm_acct_sum']/table/tbody/tr[3]/td/table/tbody/tr/td[5]"));
                    strDcn = IDcn.Text;
                    if (strDcn == "")
                    {
                        strDcn = "-";
                    }

                    IWebElement Irealtaxtable = driver.FindElement(By.XPath("//*[@id='frm_acct_sum']/table/tbody/tr[2]/td/table[2]/tbody"));
                    IList<IWebElement> Irealtaxrow = Irealtaxtable.FindElements(By.TagName("tr"));
                    IList<IWebElement> IrealtaxTD;
                    foreach (IWebElement taxrows in Irealtaxrow)
                    {
                        IrealtaxTD = taxrows.FindElements(By.TagName("td"));
                        if (IrealtaxTD.Count != 0)
                        {
                            if (count < 2)
                            {
                                strDesc = IrealtaxTD[0].Text;
                                strTax = IrealtaxTD[1].Text;
                                strPen = IrealtaxTD[2].Text;
                                strInter = IrealtaxTD[3].Text;
                                strFee = IrealtaxTD[4].Text;
                                strDue = IrealtaxTD[5].Text;
                                strCredit = IrealtaxTD[6].Text;
                                strPay = IrealtaxTD[7].Text;
                                strBal = IrealtaxTD[8].Text;
                                if (IrealtaxTD[9].Text != " ")
                                {
                                    strTaxsale = IrealtaxTD[9].Text;
                                }

                                //Tax Installment Information Details
                                string strTaxinformation = strDcn + "~" + strDesc + "~" + strTax + "~" + strPen + "~" + strInter + "~" + strFee + "~" + strDue + "~" + strCredit + "~" + strPay + "~" + strBal + "~" + strTaxsale;
                                // db.ExecuteQuery("insert into data_value_master (Order_no,parcel_no,Data_Field_Text_Id,Data_Field_value,Is_Table,Sequence) values ('" + orderNumber + "','" + parcelNumber + "',28 ,'" + strTaxinformation + "',1,1)");
                                gc.insert_date(orderNumber, parcelNumber, 28, strTaxinformation, 1, DateTime.Now);
                            }
                            count++;
                        }
                    }
                }
                catch { }
                //Tax Payment Details

                try
                {
                    string strDeposite = "", strPaymentType = "", strPaymentAmount = "", strStatus = "";
                    IWebElement Iviewpay = driver.FindElement(By.LinkText("View Payments"));
                    IJavaScriptExecutor js = driver as IJavaScriptExecutor;
                    js.ExecuteScript("arguments[0].click();", Iviewpay);
                    Thread.Sleep(2000);
                    IWebElement IrealPaytable = driver.FindElement(By.XPath("//*[@id='TABLE1']/tbody/tr/td[2]/div/table[1]/tbody"));
                    IList<IWebElement> Irealpaytrow = IrealPaytable.FindElements(By.TagName("tr"));
                    IList<IWebElement> IrealpayTD;
                    foreach (IWebElement taxrows in Irealpaytrow)
                    {
                        IrealpayTD = taxrows.FindElements(By.TagName("td"));
                        if (IrealpayTD.Count > 3 && !taxrows.Text.Contains("displayed on this page") && IrealpayTD.Count == 5)
                        {
                            strDeposite = IrealpayTD[0].Text;
                            strTransId = IrealpayTD[1].Text;
                            strPaymentType = IrealpayTD[2].Text;
                            strPaymentAmount = IrealpayTD[3].Text;
                            strStatus = IrealpayTD[4].Text;

                            //Tax History Details
                            string strTaxHistory = strDeposite + "~" + strTransId + "~" + strPaymentType + "~" + strPaymentAmount + "~" + strStatus;
                            // db.ExecuteQuery("insert into data_value_master (Order_no,parcel_no,Data_Field_Text_Id,Data_Field_value,Is_Table,Sequence) values ('" + orderNumber + "','" + parcelNumber + "',33 ,'" + strTaxHistory + "',1,1)");
                            gc.insert_date(orderNumber, parcelNumber, 33, strTaxHistory, 1, DateTime.Now);
                        }
                        //if (IrealpayTD.Count > 3 && !taxrows.Text.Contains("displayed on this page") && IrealpayTD.Count == 8)
                        //{
                        //    strSourceId = IrealpayTD[0].Text;
                        //    strTransId = IrealpayTD[1].Text;
                        //    strTaxyear = IrealpayTD[2].Text;
                        //    strPaiddate = IrealpayTD[3].Text;
                        //    strPaidamount = IrealpayTD[4].Text;
                        //    strInterest = IrealpayTD[5].Text;
                        //    strPenalty = IrealpayTD[6].Text;
                        //    strTotalpaid = IrealpayTD[7].Text;

                        //    Tax History Details
                        //    string strTaxHistory = strSourceId + "~" + strTransId + "~" + strTaxyear + "~" + strPaiddate + "~" + strPaidamount + "~" + strInterest + "~" + strPenalty + "~" + strTotalpaid;
                        //    db.ExecuteQuery("insert into data_value_master (Order_no,parcel_no,Data_Field_Text_Id,Data_Field_value,Is_Table,Sequence) values ('" + orderNumber + "','" + parcelNumber + "',33 ,'" + strTaxHistory + "',1,1)");
                        //    gc.insert_date(orderNumber, parcelNumber, 33, strTaxHistory, 1, DateTime.Now);
                        //}
                    }
                }
                catch { }
                //PDF Download Current tax bill
                try
                {
                    IWebElement url = driver.FindElement(By.LinkText("View Current Tax Bill"));
                    string URL = url.GetAttribute("href");
                    gc.downloadfile(URL, orderNumber, parcelNumber, "CurrentTax_Bill", "DC", "District of Columbia");
                }
                catch { }
            }
            catch (Exception ex)
            {
                driver.Quit();
                throw ex;
            }
        }



    }
}