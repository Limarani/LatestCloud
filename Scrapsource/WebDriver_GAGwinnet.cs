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

namespace ScrapMaricopa.Scrapsource
{
    public class WebDriver_GAGwinnet
    {
        string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "", AssessTakenTime = "", TaxTakentime = "", CityTaxtakentime = "";
        string TotaltakenTime = "";

        string strOwner = "", strOwnerName = "", strParcelno = "", strAlterPaercelno = "", strPropertyAddress = "", strPropertyType = "", strYear = "", strDescrption = ""
                  , strAssYear = "", strApplandValue = "", strAppBuildValue = "", strAppTotal = "", strAsslandValue = "", strAsslandUse = "", strAssBuildValue = "", strAssTotal = "";
        string strMultiownerno = "", strMultiowner = "", strProperty = "";
        string Outparcelno = "", outputPath = "", year = "";
        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        public string FTP_GAGwinnett(string address, string parcelNumber, string ownername, string searchType, string orderNumber, string directParcel)
        {
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "", AssessTakenTime = "", TaxTakentime = "", CityTaxtakentime = "";
            string TotaltakenTime = "";
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            //string fulladdress, assyear = "", fullassess = "", hometownasses = "", totalnetasses = "", exemption_type = "Homeowner Exemption";
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            using (driver = new PhantomJSDriver())
            {
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");
                    if (searchType == "titleflex")
                    {

                        gc.TitleFlexSearch(orderNumber, parcelNumber, "", address, "GA", "Gwinnett");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_GAGwinnet"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }

                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }
                    if (searchType == "address")
                    {
                        driver.Navigate().GoToUrl("http://www.gwinnettassessor.manatron.com/IWantTo/PropertyGISSearch.aspx");
                        driver.FindElement(By.Id("fldSearchFor")).SendKeys(address);
                        gc.CreatePdf_WOP(orderNumber, "Address Search", driver, "GA", "Gwinnett");
                        driver.FindElement(By.Name("btnSearch")).Click();
                        gc.CreatePdf_WOP(orderNumber, "Address Search Result", driver, "GA", "Gwinnett");
                        Thread.Sleep(2000);
                        //var wait = new WebDriverWait(driver, TimeSpan.FromMilliseconds(1000));
                        //wait.Until(ExpectedConditions.ElementIsVisible(By.Id("//*[@id='QuickSearch']/div[1]/div[2]")));
                        try
                        {
                            IWebElement Iresult = driver.FindElement(By.XPath("//*[@id='QuickSearch']/div[1]/div[2]"));
                            string result = Iresult.Text;//*[@id="QuickSearch"]/div[1]/div[2]
                            if (!result.Contains("1 records"))
                            {
                                multiparcel(result, orderNumber);
                                driver.Quit();
                                return "MultiParcel";
                            }
                            else
                            {
                                driver.FindElement(By.XPath("//*[@id='QuickSearch']/div[2]/div[1]/ul[2]/li[1]/a")).SendKeys(Keys.Enter);
                                aftersearch(orderNumber, strParcelno);
                            }
                        }
                        catch { }
                    }

                    else if (searchType == "parcel")
                    {
                        driver.Navigate().GoToUrl("http://www.gwinnettassessor.manatron.com/IWantTo/PropertyGISSearch.aspx");
                        Thread.Sleep(3000);
                        driver.FindElement(By.Id("fldSearchFor")).SendKeys(parcelNumber.Replace(" ", ""));
                        gc.CreatePdf(orderNumber, parcelNumber, "ParcelSearch", driver, "GA", "Gwinnett");
                        driver.FindElement(By.Name("btnSearch")).SendKeys(Keys.Enter);
                        gc.CreatePdf(orderNumber, parcelNumber, "ParcelSearchResult", driver, "GA", "Gwinnett");
                        try
                        {
                            driver.FindElement(By.XPath("//*[@id='QuickSearch']/div[2]/div[1]/ul[2]/li[1]/a")).SendKeys(Keys.Enter);
                            Thread.Sleep(2000);
                            aftersearch(orderNumber, strParcelno);
                        }
                        catch { }
                    }
                    else if (searchType == "ownername")
                    {
                        driver.Navigate().GoToUrl("http://www.gwinnettassessor.manatron.com/IWantTo/PropertyGISSearch.aspx");
                        IWebElement Isearch = driver.FindElement(By.Id("fldSearchFor"));
                        Isearch.SendKeys(ownername);
                        gc.CreatePdf_WOP(orderNumber, "MultiAddressSearch", driver, "GA", "Gwinnett");
                        driver.FindElement(By.Name("btnSearch")).Click();
                        gc.CreatePdf_WOP(orderNumber, "MultiAddressSearchResult", driver, "GA", "Gwinnett");
                        Thread.Sleep(2000);
                        //var wait = new WebDriverWait(driver, TimeSpan.FromMilliseconds(3000));
                        //wait.Until(ExpectedConditions.ElementIsVisible(By.Id("//*[@id='QuickSearch']/div[1]/div[2]")));
                        try
                        {
                            IWebElement Iresult = driver.FindElement(By.XPath("//*[@id='QuickSearch']/div[1]/div[2]"));
                            string result = Iresult.Text;//*[@id="QuickSearch"]/div[1]/div[2]
                            string resultcount = gc.Between(result, "returned", "record").Trim();
                            int Iresultcount = Convert.ToInt16(resultcount);
                            if (Iresultcount <= 10)
                            {
                                if (!result.Contains("Your search returned 1 records."))
                                {
                                    multiparcel(result, orderNumber);
                                    driver.Quit();
                                    return "MultiParcel";
                                }
                                else
                                {
                                    //*[@id="QuickSearch"]/div[2]/div[1]/ul[2]/li[1]/a
                                    driver.FindElement(By.XPath("//*[@id='QuickSearch']/div[2]/div[1]/ul[2]/li[1]/a")).SendKeys(Keys.Enter);
                                    Thread.Sleep(2000);
                                    aftersearch(orderNumber, strParcelno);
                                }
                            }
                            else
                            {
                                HttpContext.Current.Session["multiparcel_gw_count"] = "Maximum";
                                // GlobalClass.multiparcel_gw_count = "Maximum";
                                return "Maximum";
                            }
                        }
                        catch { }
                    }
                    try
                    {
                        IWebElement INodata = driver.FindElement(By.Id("QuickSearch"));
                        if(INodata.Text.Contains("no records were found"))
                        {
                            HttpContext.Current.Session["Nodata_GAGwinnet"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }
                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "GA", "Gwinnett", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    gc.mergpdf(orderNumber, "GA", "Gwinnett");
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


        public void TaxDetails(string orderNumber, string parcelNumber)
        {
            //Tax Payment 
            driver.Navigate().GoToUrl("http://gwinnetttaxcommissioner.publicaccessnow.com/ViewPayYourTaxes.aspx");
            var SerachBy = driver.FindElement(By.Name("selSearchBy"));
            var selectElement = new SelectElement(SerachBy);
            selectElement.SelectByText("Parcel Number");
            driver.FindElement(By.Name("fldInput")).SendKeys(parcelNumber);

            //screenshot
            gc.CreatePdf(orderNumber, parcelNumber, "ParcelSearch", driver, "GA", "Gwinnett");

            driver.FindElement(By.Id("btnsearch")).SendKeys(Keys.Enter);
            //screenshot
            gc.CreatePdf(orderNumber, parcelNumber, "PropertySearch", driver, "GA", "Gwinnett");
            driver.FindElement(By.XPath("//*[@id='grm-search']/tbody/tr/td[4]/a")).SendKeys(Keys.Enter);
            //*[@id="grm-search"]/tbody/tr/td[4]/a
            //Account Detail- Tax Account
            string tax_district = driver.FindElement(By.XPath("/html/body/form/div[3]/div/div[6]/div[5]/div[1]/div[1]/div/div[2]/div/div/div/div[2]/table[1]/tbody/tr/td[2]/p")).Text;
            tax_district = tax_district.Replace("Tax District:", " ").Trim();
            string prop_type = driver.FindElement(By.XPath("/html/body/form/div[3]/div/div[6]/div[5]/div[1]/div[1]/div/div[2]/div/div/div/div[2]/table[2]/tbody/tr/td[2]")).Text;
            string legal_desc = driver.FindElement(By.XPath("/html/body/form/div[3]/div/div[6]/div[5]/div[1]/div[1]/div/div[2]/div/div/div/div[2]/table[3]/tbody/tr/td")).Text;

            string property_details = strOwnerName + "~" + strAlterPaercelno + "~" + strPropertyAddress + "~" + strPropertyType + "~" + strYear + "~" + legal_desc + "~" + tax_district;
            gc.insert_date(orderNumber, parcelNumber, 8, property_details, 1, DateTime.Now);
            

            try
            {
                //download print tax bill
                IWebElement url = driver.FindElement(By.XPath("/html/body/form/div[3]/div/div[6]/div[5]/div[1]/div[3]/div/div[2]/div/div/div/div[2]/table/tbody/tr/td/div/a"));
                string URL = url.GetAttribute("href");            
                gc.downloadfile(URL, orderNumber, parcelNumber, "Tax_PayBill.pdf", "GA", "Gwinnett");
            }
            catch { }

            //Account detail and tax bills- take year
            IList<IWebElement> Taxbillsrowcount = driver.FindElements(By.XPath("//table[contains(@class,'TaxBillModDataOn')]/tbody/tr"));
            int count = Taxbillsrowcount.Count();
            string inst = "";
            //Take tax bills amount and store into table
            IWebElement tbTaxbill = driver.FindElement(By.XPath("//table[contains(@class,'TaxBillModDataOn ')]/tbody"));
            IList<IWebElement> trTaxbill = tbTaxbill.FindElements(By.TagName("tr"));
            IList<IWebElement> tdTaxbill;
            foreach (IWebElement row1 in trTaxbill)
            {
                tdTaxbill = row1.FindElements(By.TagName("td"));                
                inst = "-" + "~" + "-" + "~" + tdTaxbill[0].Text + "~" + tdTaxbill[1].Text + "~" + tdTaxbill[2].Text + "~" + "-" + "~" + tdTaxbill[3].Text + "~" + tdTaxbill[4].Text + "~" + tdTaxbill[5].Text + "~" + tdTaxbill[6].Text;
                gc.insert_date(orderNumber, parcelNumber, 10, inst, 1, DateTime.Now);
                
            }

            IWebElement totalamt = driver.FindElement(By.XPath("/html/body/form/div[3]/div/div[6]/div[5]/div[1]/div[2]/div/div[2]/div/div/div/div/table/tfoot/tr/th[1]"));
            string totalamount = totalamt.Text;
            IWebElement amt = driver.FindElement(By.XPath("/html/body/form/div[3]/div/div[6]/div[5]/div[1]/div[2]/div/div[2]/div/div/div/div/table/tfoot/tr/td"));
            string amount = amt.Text;
            inst = "-" + "~" + "-" + "~" + totalamount + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + amount;
            gc.insert_date(orderNumber, parcelNumber, 10, inst, 1, DateTime.Now);
            


            for (int i = 1; i <= count; i++)
            {
                IWebElement TaxBillslink = driver.FindElement(By.XPath("//html/body/form/div[3]/div/div[6]/div[5]/div[1]/div[2]/div/div[2]/div/div/div/div/table/tbody/tr[" + i + "]/td[1]/a"));
                TaxBillslink.SendKeys(Keys.Enter);
                Thread.Sleep(3000);

                if (i == 1)
                {

                    IWebElement maintable = driver.FindElement(By.XPath("/html/body/form/div[3]/div/div[6]/div[3]/div/div[3]/div/div[2]/div/div/div/table"));
                    IList<IWebElement> maintablerow = maintable.FindElements(By.TagName("tr"));
                    int maintablerowcount = maintablerow.Count;
                    IList<IWebElement> maintabletd;
                    int b = 1;
                    int d = 2;
                    foreach (IWebElement rowid in maintablerow)
                    {

                        maintabletd = rowid.FindElements(By.TagName("td"));
                        if (maintabletd.Count != 0 && b <= maintablerowcount)
                        {
                            if (b % 2 == 0 && b < maintablerowcount)
                            {
                                try
                                {
                                    string breakdown = "";
                                    driver.FindElement(By.XPath("/html/body/form/div[3]/div/div[6]/div[3]/div/div[3]/div/div[2]/div/div/div/table/tbody/tr[" + b + "]/td[1]/a")).SendKeys(Keys.Enter);
                                    Thread.Sleep(2000);
                                    string assessment = driver.FindElement(By.XPath("/html/body/form/div[3]/div/div[6]/div[3]/div/div[3]/div/div[2]/div/div/div/table/tbody/tr[" + b + "]/td[1]")).Text;
                                    string net_tax = driver.FindElement(By.XPath("/html/body/form/div[3]/div/div[6]/div[3]/div/div[3]/div/div[2]/div/div/div/table/tbody/tr[" + b + "]/td[2]")).Text;
                                    string savings = driver.FindElement(By.XPath("/html/body/form/div[3]/div/div[6]/div[3]/div/div[3]/div/div[2]/div/div/div/table/tbody/tr[" + b + "]/td[3]")).Text;
                                    breakdown = assessment + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + net_tax + "~" + savings;
                                    gc.insert_date(orderNumber, parcelNumber, 11, breakdown, 1, DateTime.Now);
                                    //db.ExecuteQuery("insert into data_value_master (Order_no,parcel_no,Data_Field_Text_Id,Data_Field_value,Is_Table,Sequence) values ('" + orderNumber + "','" + parcelNumber + "',11,'" + breakdown + "',1,1)");
                                    //db.ExecuteQuery("insert into tax_breakdown_details(order_no, parcel_no, tax_authority, net_tax, savings) values ('" + orderNumber + "','" + parcelNumber + "','" + assessment + "','" + net_tax + "','" + savings + "') ");

                                    IWebElement BRDtable = driver.FindElement(By.XPath("/html/body/form/div[3]/div/div[6]/div[3]/div/div[3]/div/div[2]/div/div/div/table/tbody/tr[" + d + "]/td/div/table/tbody"));
                                    IList<IWebElement> BRDtablerow = BRDtable.FindElements(By.TagName("tr"));
                                    int BRDtablerowcount = BRDtablerow.Count;
                                    IList<IWebElement> BRDtabletd;

                                    foreach (IWebElement row1 in BRDtablerow)
                                    {
                                        BRDtabletd = row1.FindElements(By.TagName("td"));
                                        if ((BRDtabletd[0].Text) != "Authority")
                                        {
                                            breakdown = BRDtabletd[0].Text + "~" + BRDtabletd[1].Text + "~" + BRDtabletd[2].Text + "~" + BRDtabletd[3].Text + "~" + BRDtabletd[4].Text + "~" + BRDtabletd[5].Text + "~" + BRDtabletd[6].Text;
                                            //db.ExecuteQuery("insert into data_value_master (Order_no,parcel_no,Data_Field_Text_Id,Data_Field_value,Is_Table,Sequence) values ('" + orderNumber + "','" + parcelNumber + "',11,'" + breakdown + "',1,1)");
                                            gc.insert_date(orderNumber, parcelNumber, 11, breakdown, 1, DateTime.Now);
                                            // db.ExecuteQuery("insert into tax_breakdown_details (order_no,parcel_no,tax_authority,assessed_value,exemption,taxable,net_rate,net_tax,savings) values ('" + orderNumber + "','" + parcelNumber + "','" + BRDtabletd[0].Text + "','" + BRDtabletd[1].Text + "', '" + BRDtabletd[2].Text + "', '" + BRDtabletd[3].Text + "' , '" + BRDtabletd[4].Text + "','" + BRDtabletd[5].Text + "','" + BRDtabletd[6].Text + "') ");

                                        }

                                    }

                                }

                                catch { }
                                try
                                {
                                    //subtotal
                                    string subtotal = driver.FindElement(By.XPath("/html/body/form/div[3]/div/div[6]/div[3]/div/div[3]/div/div[2]/div/div/div/table/tbody/tr[" + b + "]/th")).Text;
                                    string sub_nettax = driver.FindElement(By.XPath("/html/body/form/div[3]/div/div[6]/div[3]/div/div[3]/div/div[2]/div/div/div/table/tbody/tr[" + b + "]/td[1]")).Text;
                                    string sub_savings = driver.FindElement(By.XPath("/html/body/form/div[3]/div/div[6]/div[3]/div/div[3]/div/div[2]/div/div/div/table/tbody/tr[" + b + "]/td[2]")).Text;
                                    string breakdown1 = subtotal + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + sub_nettax + "~" + sub_savings;
                                    gc.insert_date(orderNumber, parcelNumber, 11, breakdown1, 1, DateTime.Now);
                                    //db.ExecuteQuery("insert into data_value_master (Order_no,parcel_no,Data_Field_Text_Id,Data_Field_value,Is_Table,Sequence) values ('" + orderNumber + "','" + parcelNumber + "',11,'" + breakdown1 + "',1,1)");
                                    //db.ExecuteQuery("insert into tax_breakdown_details(order_no, parcel_no, tax_authority, net_tax, savings) values ('" + orderNumber + "','" + parcelNumber + "','" + subtotal + "','" + sub_nettax + "','" + sub_savings + "') ");
                                }

                                catch { }

                                try
                                {
                                    string total = driver.FindElement(By.XPath("/html/body/form/div[3]/div/div[6]/div[3]/div/div[3]/div/div[2]/div/div/div/table/tbody/tr[" + d + "]/th")).Text;
                                    string total_nettax = driver.FindElement(By.XPath("/html/body/form/div[3]/div/div[6]/div[3]/div/div[3]/div/div[2]/div/div/div/table/tbody/tr[" + d + "]/td[1]")).Text;
                                    string total_savings = driver.FindElement(By.XPath("/html/body/form/div[3]/div/div[6]/div[3]/div/div[3]/div/div[2]/div/div/div/table/tbody/tr[" + d + "]/td[2]")).Text;
                                    string breakdown2 = total + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + total_nettax + "~" + total_savings;
                                    gc.insert_date(orderNumber, parcelNumber, 11, breakdown2, 1, DateTime.Now);
                                    //db.ExecuteQuery("insert into data_value_master (Order_no,parcel_no,Data_Field_Text_Id,Data_Field_value,Is_Table,Sequence) values ('" + orderNumber + "','" + parcelNumber + "',11,'" + breakdown2 + "',1,1)");
                                    // db.ExecuteQuery("insert into tax_breakdown_details(order_no, parcel_no, tax_authority, net_tax, savings) values ('" + orderNumber + "','" + parcelNumber + "','" + total + "','" + total_nettax + "','" + total_savings + "') ");
                                }
                                catch { }

                            }
                        }
                        b++;
                        d++;
                        //screenshot
                        gc.CreatePdf(orderNumber, parcelNumber, "AssessmentBreakdown", driver, "GA", "Gwinnett");
                    }

                }

                //Thread.Sleep(5000);
                // Tax Installation information
                IWebElement Tableinstallment = driver.FindElement(By.XPath("//*[@id='installments']/tbody"));
                IList<IWebElement> TRinstallment = Tableinstallment.FindElements(By.TagName("tr"));
                IList<IWebElement> TDinstallment;

                int c = 0;
                int trcount = TRinstallment.Count();
                foreach (IWebElement row in TRinstallment)
                {

                    TDinstallment = row.FindElements(By.TagName("td"));
                    if (TDinstallment.Count != 0)
                    {

                        string a = "";

                        if (c < trcount - 1)
                        {
                            inst = TDinstallment[0].Text + "~" + TDinstallment[1].Text + "~" + TDinstallment[3].Text + "~" + TDinstallment[4].Text + "~" + "-" + "~" + TDinstallment[7].Text + "~" + TDinstallment[5].Text + "~" + TDinstallment[6].Text + "~" + TDinstallment[2].Text + "~" + "-";
                            //installment~seq_no~tax_year~tax_amount~Total_Paid~total_due~penalty_amount~Interest~due_date~balance_due
                            gc.insert_date(orderNumber, parcelNumber, 10, inst, 1, DateTime.Now);
                            //db.ExecuteQuery("insert into data_value_master (Order_no,parcel_no,Data_Field_Text_Id,Data_Field_value,Is_Table,Sequence) values ('" + orderNumber + "','" + parcelNumber + "',10 ,'" + inst + "',1,1)");
                            //db.ExecuteQuery("insert into la_tax_summary (order_no, parcel_no,installment,seq_no,due_date,tax_year,tax_amount,penalty_amount,Interest,total_due) values ('" + orderNumber + "','" + parcelNumber + "','" + TDinstallment[0].Text + "','" + TDinstallment[1].Text + "', '" + TDinstallment[2].Text + "', '" + TDinstallment[3].Text + "' , '" + TDinstallment[4].Text + "','" + TDinstallment[5].Text + "' ,'" + TDinstallment[6].Text + "' ,'" + TDinstallment[7].Text + "') ");
                        }
                        if (c == trcount - 1)
                        {
                            //installment~seq_no~tax_year~tax_amount~Total_Paid~total_due~penalty_amount~Interest~due_date~balance_due
                            inst = TDinstallment[0].Text + "~" + TDinstallment[1].Text + "~" + TDinstallment[3].Text + "~" + TDinstallment[4].Text + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + TDinstallment[2].Text + "~" + "-";
                            gc.insert_date(orderNumber, parcelNumber, 10, inst, 1, DateTime.Now);
                            //db.ExecuteQuery("insert into data_value_master (Order_no,parcel_no,Data_Field_Text_Id,Data_Field_value,Is_Table,Sequence) values ('" + orderNumber + "','" + parcelNumber + "',10 ,'" + inst + "',1,1)");
                            //db.ExecuteQuery("insert into la_tax_summary (order_no, parcel_no,installment,seq_no,due_date,tax_year,tax_amount,penalty_amount,Interest,total_due) values ('" + orderNumber + "','" + parcelNumber + "', '" + TDinstallment[0].Text + "','" + TDinstallment[1].Text + "', '" + TDinstallment[2].Text + "', '" + TDinstallment[3].Text + "' , '" + TDinstallment[4].Text + "', '" + a + "','" + a + "','" + a + "')");
                        }

                        c++;

                    }

                }

                //payement History
                try
                {
                    IWebElement tablePayment = driver.FindElement(By.XPath("/html/body/form/div[3]/div/div[6]/div[3]/div/div[5]/div/div[2]/div/div/div/table/tbody"));
                    IList<IWebElement> TRPayment = tablePayment.FindElements(By.TagName("tr"));
                    IList<IWebElement> TDPayment;

                    foreach (IWebElement row1 in TRPayment)
                    {

                        TDPayment = row1.FindElements(By.TagName("td"));
                        string bill = "-" + "~" + TDPayment[0].Text + "~ " + TDPayment[1].Text + "~" + TDPayment[2].Text + "~" + "-" + "~" + TDPayment[3].Text + "~" + TDPayment[4].Text + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-";
                        gc.insert_date(orderNumber, parcelNumber, 12, bill, 1, DateTime.Now);
                        //db.ExecuteQuery("insert into data_value_master (Order_no,parcel_no,Data_Field_Text_Id,Data_Field_value,Is_Table,Sequence) values ('" + orderNumber + "','" + parcelNumber + "',12,'" + bill + "',1,1)");
                        //db.ExecuteQuery("insert into bill_details (order_no, parcel_no,year,bill_number,receipt_no,amount,paid_date) values ('" + orderNumber + "','" + parcelNumber + "','" + TDPayment[0].Text + "','" + TDPayment[1].Text + "', '" + TDPayment[2].Text + "', '" + TDPayment[3].Text + "' , '" + TDPayment[4].Text + "') ");
                        year = TDPayment[0].Text;
                    }

                    //screenshot
                    gc.CreatePdf(orderNumber, parcelNumber, "tax_Assessment" + year, driver, "GA", "Gwinnett");
                }
                catch
                {

                }
                Thread.Sleep(1000);
                driver.Navigate().Back();

            }

        }



        public void multiparcel(string result, string orderNumber)
        {
            gc.CreatePdf_WOP(orderNumber, "MultipleAddress", driver, "GA", "Gwinnett");
            HttpContext.Current.Session["multiparcel_gw"] = "Yes";
            // GlobalClass.multiparcel_gw = "Yes";
            strMultiownerno = WebDriverTest.Between(result, "returned", "records.").Trim();
            try
            {
                for (int i = 1; i <= 25; i++)
                {
                    IWebElement Iownerlink = driver.FindElement(By.XPath("//*[@id='QuickSearch']/div[2]/div[" + i + "]/ul[2]/li[1]/a"));
                    strMultiowner = Iownerlink.Text;
                    IWebElement Ipropertyaddress = driver.FindElement(By.XPath("//*[@id='QuickSearch']/div[2]/div[" + i + "]/ul[2]/li[5]"));
                    strProperty = Ipropertyaddress.Text;
                    strPropertyAddress = strProperty.Substring(0, strProperty.IndexOf(" | "));
                    strParcelno = WebDriverTest.After(strProperty, "| ");

                    string multiowner = strPropertyAddress + "~" + strMultiowner;
                    gc.insert_date(orderNumber, strParcelno, 13, multiowner, 1, DateTime.Now);
                    //db.ExecuteQuery("insert into data_value_master (Order_no,parcel_no,Data_Field_Text_Id,Data_Field_value,Is_Table,Sequence) values ('" + orderNumber + "','" + strParcelno + "',13,'" + multiowner + "',1,1)");
                    //db.ExecuteQuery("insert into la_multiowner(parcel_no,situs_address,legal_description,order_no) values('" + strParcelno + "','" + strPropertyAddress + "','" + strMultiowner + "','" + orderNumber + "') ");

                }
            }

            catch { }


        }


        public void aftersearch(string orderNumber, string strParcelno)
        {
            strOwner = driver.FindElement(By.XPath("//*[@id='lxT1385']/table/tbody/tr[1]/td")).Text;
            strOwnerName = strOwner.Substring(0, strOwner.IndexOf("\r"));//using between for owner address
            strParcelno = driver.FindElement(By.XPath("//*[@id='lxT1385']/table/tbody/tr[2]/td")).Text;
            strAlterPaercelno = driver.FindElement(By.XPath("//*[@id='lxT1385']/table/tbody/tr[3]/td")).Text;
            strPropertyAddress = driver.FindElement(By.XPath("//*[@id='lxT1385']/table/tbody/tr[4]/td")).Text;
            strPropertyType = driver.FindElement(By.XPath("//*[@id='lxT1385']/table/tbody/tr[5]/td")).Text;
            try
            {
                IWebElement Iyear = driver.FindElement(By.XPath("//*[@id='1388gallerywrap']/table[2]/tbody/tr[4]/td"));
                if (Iyear != null)
                {
                    strYear = Iyear.Text;
                }
                else
                {
                    strYear = "";
                }
            }
            catch
            {

            }
            try
            {
                IWebElement IBuildyear = driver.FindElement(By.XPath("//*[@id='1388gallerywrap']/table[2]/tbody/tr[3]/td"));
                string strBuildYear = IBuildyear.Text;
                if (strBuildYear != null && strBuildYear.Length == 4)
                {
                    strYear = strBuildYear;
                }
            }

            catch
            {

            }


            strDescrption = driver.FindElement(By.XPath("//*[@id='lxT1391']/table/tbody/tr[2]/td[2]")).Text;

            gc.CreatePdf(orderNumber, strParcelno, "PropertyDetails", driver, "GA", "Gwinnett");
            //string property_details = strOwnerName + "~" + strAlterPaercelno + "~" + strPropertyAddress + "~" + strPropertyType + "~" + strYear + "~" + strDescrption + "~" + "-";
            //gc.insert_date(orderNumber,DateTime.Now , strParcelno, 8, property_details, 1);
            //db.ExecuteQuery("insert into data_value_master (Order_no,parcel_no,Data_Field_Text_Id,Data_Field_value,Is_Table,Sequence) values ('" + orderNumber + "','" + strParcelno + "',8 ,'" + property_details + "',1,1)");
            //db.ExecuteQuery("insert into real_property(orderno,str,owner,conto_owner,address,subdivision,apn,mcr) values('" + orderNumber + "','" + strParcelno + "','" + strOwnerName + "','" + strAlterPaercelno + "','" + strPropertyAddress + "','" + strPropertyType + "','" + strYear + "','" + strDescrption + "')");

            //Assessment Details
            strAssYear = driver.FindElement(By.XPath("//*[@id='ValueHistory']/tbody/tr[1]/th[2]")).Text;
            strApplandValue = driver.FindElement(By.XPath("//*[@id='ValueHistory']/tbody/tr[3]/td[1]")).Text;
            strAppBuildValue = driver.FindElement(By.XPath("//*[@id='ValueHistory']/tbody/tr[4]/td[1]")).Text;
            strAppTotal = driver.FindElement(By.XPath("//*[@id='ValueHistory']/tbody/tr[5]/td[1]")).Text;
            strAsslandValue = driver.FindElement(By.XPath("//*[@id='ValueHistory']/tbody/tr[6]/td[1]")).Text;
            strAsslandUse = driver.FindElement(By.XPath("//*[@id='ValueHistory']/tbody/tr[7]/td[1]")).Text;
            strAssBuildValue = driver.FindElement(By.XPath("//*[@id='ValueHistory']/tbody/tr[8]/td[1]")).Text;
            strAssTotal = driver.FindElement(By.XPath("//*[@id='ValueHistory']/tbody/tr[9]/td[1]")).Text;

            string assessment_details = strAssYear + "~" + strApplandValue + "~" + strAppBuildValue + "~" + strAppTotal + "~" + strAsslandValue + "~" + strAsslandUse + "~" + strAssBuildValue + "~" + strAssTotal;
            gc.insert_date(orderNumber, strParcelno, 9, assessment_details, 1, DateTime.Now);
            //db.ExecuteQuery("insert into data_value_master (Order_no,parcel_no,Data_Field_Text_Id,Data_Field_value,Is_Table,Sequence) values ('" + orderNumber + "','" + strParcelno + "',9 ,'" + assessment_details + "',1,1)");
            //db.ExecuteQuery("insert into la_assessor(order_no,parcel_no,year,land,improvements,total,legal_description,date,value_year,total_assessment) Values('" + orderNumber + "','" + strParcelno + "','" + strAssYear + "','" + strApplandValue + "','" + strAppBuildValue + "','" + strAppTotal + "','" + strAsslandValue + "','" + strAsslandUse + "','" + strAssBuildValue + "','" + strAssTotal + "')");

            //Download the pdf for Assessment and Property Details 
            IWebElement url = driver.FindElement(By.XPath("//*[@id='lxT1401']/p/a[3]"));
            string URL = url.GetAttribute("href");
            gc.downloadfile(URL, orderNumber, strParcelno, "bill_Assessment.pdf", "GA", "Gwinnett");
            //string Assessmentpdf = outputPath + "bill_Assessment.pdf";
            //WebClient downloadpdf = new WebClient();
            //downloadpdf.DownloadFile(URL, Assessmentpdf);
            AssessmentTime = DateTime.Now.ToString("HH:mm:ss");
            //Tax Details
            TaxDetails(orderNumber, strParcelno);
            TaxTime = DateTime.Now.ToString("HH:mm:ss");
            try
            {
                driver.Navigate().GoToUrl("http://gwinnetttaxcommissioner.publicaccessnow.com/ViewPayYourTaxes.aspx");
                var waitsearch = new WebDriverWait(driver, TimeSpan.FromMilliseconds(5000));
                waitsearch.Until(ExpectedConditions.ElementIsVisible(By.Id("selSearchBy")));
                IWebElement Iparcelno = driver.FindElement(By.Id("selSearchBy"));
                SelectElement selectParcelno = new SelectElement(Iparcelno);
                selectParcelno.SelectByText("Parcel Number");
                driver.FindElement(By.Id("fldInput")).SendKeys(strParcelno);
                driver.FindElement(By.Id("btnsearch")).SendKeys(Keys.Enter);
                gc.CreatePdf(orderNumber, strParcelno, "GwinnettCountyTaxSearch", driver, "GA", "Gwinnett");
                IWebElement Iparcelsearch = driver.FindElement(By.XPath("//*[@id='grm-search']/tbody/tr/td[4]/a"));
                Iparcelsearch.SendKeys(Keys.Enter);
                gc.CreatePdf(orderNumber, strParcelno, "GwinnettCountyTaxSearchResult", driver, "GA", "Gwinnett");
                var waittax = new WebDriverWait(driver, TimeSpan.FromMilliseconds(5000));
                waittax.Until(ExpectedConditions.ElementIsVisible(By.XPath("//*[@id='lxT473']/div[2]/table[1]/tbody/tr/td[2]/p")));
                string strTaxDis = driver.FindElement(By.XPath("//*[@id='lxT473']/div[2]/table[1]/tbody/tr/td[2]/p")).Text;
                string strTaxDistrict = WebDriverTest.After(strTaxDis, "\r\n");
                if (strTaxDistrict.Contains("COUNTY Unincorporated"))
                {
                    driver.Quit();
                }
                else if (strTaxDistrict.Contains("SUWANEE"))
                {
                    SuwaneCityTaxDetails(orderNumber, strParcelno, strTaxDistrict, driver);
                }
                else if (strTaxDistrict.Contains("DULUTH"))
                {
                    DuluthCityTaxDetails(strParcelno, orderNumber);
                }
                else if (strTaxDistrict.Contains("NORCROSS"))
                {
                    NorcrossCityTaxDetails(strParcelno, orderNumber);
                }
                CitytaxTime = DateTime.Now.ToString("HH:mm:ss");
            }
            catch
            {
            }

            driver.Quit();
        }


        public void SuwaneCityTaxDetails(string orderNumber, string parcelNumber, string strTaxDistrict, IWebDriver driver)
        {
            string strTaxDis = "", strowner = "", strOwnername = "", strTaxyear = "", strInstalltype = "", strTotaltax = "", strTotalpaid = "", strPaiddate = "", strTotaldue = "";
            try
            {

                string URLAccess = "https://accessmygov.com/SiteSearch/SiteSearchDetails?SearchFocus=All+Records&SearchCategory=Parcel+Number&SearchText=" + parcelNumber + "&uid=2338&PageIndex=1&ReferenceKey=" + parcelNumber + "&ReferenceType=0&SortBy=&SearchOrigin=0&RecordKeyDisplayString=" + parcelNumber + "&RecordKey=4%3dR7277+081&RecordKeyType=4%3d0";
                driver.Navigate().GoToUrl(URLAccess);
                gc.CreatePdf(orderNumber, parcelNumber, "SuwaneCityTaxSearch", driver, "GA", "Gwinnett");
                Thread.Sleep(2000);
                IWebElement Iownername = driver.FindElement(By.XPath("/html/body/div[7]/div[3]/div[1]/form/div/div[1]/div/div[4]/table/tbody/tr/td/div/div[1]"));
                strowner = Iownername.Text;
                strOwnername = WebDriverTest.After(strowner, "Property Owner: ");
                IWebElement Itaxyear = driver.FindElement(By.XPath("//*[@id='ApplicationTabs-1']/div[2]/div[9]/div/div[2]/div/div[3]/table/tbody/tr[1]/td[2]"));
                strTaxyear = Itaxyear.Text;
                IWebElement Iinstalltype = driver.FindElement(By.XPath("//*[@id='ApplicationTabs-1']/div[2]/div[9]/div/div[2]/div/div[3]/table/tbody/tr[1]/td[3]"));
                strInstalltype = Iinstalltype.Text;
                IWebElement Itotaltax = driver.FindElement(By.XPath("//*[@id='ApplicationTabs-1']/div[2]/div[9]/div/div[2]/div/div[3]/table/tbody/tr[1]/td[4]"));
                strTotaltax = Itotaltax.Text;
                IWebElement Itotalpaid = driver.FindElement(By.XPath("//*[@id='ApplicationTabs-1']/div[2]/div[9]/div/div[2]/div/div[3]/table/tbody/tr[1]/td[5]"));
                strTotalpaid = Itotalpaid.Text;
                IWebElement Ipaiddate = driver.FindElement(By.XPath("//*[@id='ApplicationTabs-1']/div[2]/div[9]/div/div[2]/div/div[3]/table/tbody/tr[1]/td[6]"));
                strPaiddate = Ipaiddate.Text;
                IWebElement Itotaldue = driver.FindElement(By.XPath("//*[@id='ApplicationTabs-1']/div[2]/div[9]/div/div[2]/div/div[3]/table/tbody/tr[1]/td[7]"));
                strTotaldue = Itotaldue.Text;

                gc.CreatePdf(orderNumber, parcelNumber, "SuwaneCityTotalTax", driver, "GA", "Gwinnett");
                //School Taxes~-~-~-~-~$2,250.86~$79.20
                string bill = strTaxDistrict + "~" + strTaxyear + "~" + "-" + "~" + "-" + "~" + "-" + "~" + strTotalpaid + "~" + strPaiddate + "~" + "-" + "~" + strOwnername + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + strTotaltax + "~" + "-" + "~" + "-" + "~" + strInstalltype + "~" + strTotaldue;
                gc.insert_date(orderNumber, parcelNumber, 12, bill, 1, DateTime.Now);
                //db.ExecuteQuery("insert into bill_details(order_no,parcel_no,city_name,receipt_no,year,type,total,amount,paid_date,total_due) Values('" + orderNumber + "','" + parcelNumber + "','" + strTaxDistrict + "','" + strOwnername + "','" + strTaxyear + "','" + strInstalltype + "','" + strTotaltax + "','" + strTotalpaid + "','" + strPaiddate + "','" + strTotaldue + "')");

                //  }
            }

            catch (Exception ex)
            {
                driver.Quit();
                throw ex;
            }
        
        }

        public void DuluthCityTaxDetails(string parcelNumber, string orderNumber)
        {
            // driver = new ChromeDriver();
            driver.Navigate().GoToUrl("https://www.municipalonlinepayments.com/duluthga/tax/search");
            driver.FindElement(By.Id("ParcelNumber")).SendKeys(parcelNumber);

            gc.CreatePdf(orderNumber, parcelNumber, "duluthCity_search", driver, "GA", "Gwinnett");


            driver.FindElement(By.XPath("//*[@id='main']/section/div[1]/form/div[4]/div/button")).Click();

            string noparcelFound = driver.FindElement(By.XPath("//*[@id='main']/section/div[2]/h3")).Text;

            if (!noparcelFound.Contains("No Parcel Found"))
            {
                string dataURL = "https://www.municipalonlinepayments.com/duluthga/tax/search/detail/" + parcelNumber;
                driver.Navigate().GoToUrl(dataURL);




                //unpaid charges....
                int indx = 0;
                try
                {
                    IWebElement tableElement = driver.FindElement(By.Id("charge_detail"));
                    IList<IWebElement> multiaddrtableRow = tableElement.FindElements(By.TagName("tr"));
                    IList<IWebElement> rowTD;
                    foreach (IWebElement row in multiaddrtableRow)
                    {
                        if (indx != 0)
                        {

                            rowTD = row.FindElements(By.TagName("td"));
                            if (rowTD.Count != 0)
                            {
                                //+"~"
                                string bill = "Duluth City" + "~" + rowTD[0].Text + "~" + rowTD[6].Text + "~" + "-" + "~" + "-" + "~" + rowTD[2].Text + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + rowTD[3].Text + "~" + rowTD[4].Text + "~" + "-" + "~" + rowTD[5].Text + "~" + rowTD[6].Text + "~" + "un paid charges" + "~" + rowTD[1].Text + "~" + "-";
                                gc.insert_date(orderNumber, strParcelno, 12, bill, 1, DateTime.Now);
                                //db.ExecuteQuery("insert into data_value_master (Order_no,parcel_no,Data_Field_Text_Id,Data_Field_value,Is_Table,Sequence) values ('" + orderNumber + "','" + parcelNumber + "',12,'" + bill + "',1,1)");
                                //db.ExecuteQuery("insert into bill_details (order_no,parcel_no,year,type,amount,interest,penalty,misc,total,bill_number,special_drainage,remarks,city_name) values('" + orderNumber + "','" + parcelNumber + "','" + rowTD[0].Text + "','" + rowTD[1].Text + "','" + rowTD[2].Text + "','" + rowTD[3].Text + "','" + rowTD[4].Text + "','" + rowTD[5].Text + "','" + rowTD[6].Text + "','" + rowTD[7].Text + "','" + rowTD[8].Text + "','un paid charges','Duluth City')");
                            }
                        }

                        indx++;
                    }

                }
                catch
                {

                }

                IWebElement tableElement1 = driver.FindElement(By.Id("tax_history"));
                IList<IWebElement> multiaddrtableRow1 = tableElement1.FindElements(By.TagName("tr"));
                IList<IWebElement> rowTD1;
                indx = 0;
                foreach (IWebElement row in multiaddrtableRow1)
                {
                    if (indx != 0)
                    {

                        rowTD1 = row.FindElements(By.TagName("td"));
                        if (rowTD1.Count != 0)
                        {
                            //Duluth City~2017~2017 - 2017 Stormwater~-~-~$41.50~-~-~-~-~$0.83~$4.15~$46.48~016211~-~-~Special Tax~-
                            //city_name~year~bill_number~receipt_no~prior_payment~amount~paid_date~good_through~tax_payer~tax_authority~interest~penalty~misc~total~special_drainage~remarks~type~total_due
                            string bill = "Duluth City" + "~" + rowTD1[0].Text + "~" + rowTD1[6].Text + "~" + "-" + "~" + "-" + "~" + rowTD1[2].Text + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + rowTD1[3].Text + "~" + rowTD1[4].Text + "~" + "-" + "~" + rowTD1[5].Text + "~" + rowTD1[7].Text + "~" + "-" + "~" + rowTD1[1].Text + "~" + "-";
                            //db.ExecuteQuery("insert into data_value_master (Order_no,parcel_no,Data_Field_Text_Id,Data_Field_value,Is_Table,Sequence) values ('" + orderNumber + "','" + parcelNumber + "',12,'" + bill + "',1,1)");
                            gc.insert_date(orderNumber, parcelNumber, 12, bill, 1, DateTime.Now);
                            //db.ExecuteQuery("insert into bill_details (order_no,parcel_no,year,type,amount,interest,penalty,total,bill_number,special_drainage,city_name) values('" + orderNumber + "','" + parcelNumber + "','" + rowTD1[0].Text + "','" + rowTD1[1].Text + "','" + rowTD1[2].Text + "','" + rowTD1[3].Text + "','" + rowTD1[4].Text + "','" + rowTD1[5].Text + "','" + rowTD1[6].Text + "','" + rowTD1[7].Text + "','Duluth City')");
                        }
                    }

                    indx++;
                }

                gc.CreatePdf(orderNumber, parcelNumber, "DuluthCity_tax_details", driver, "GA", "Gwinnett");

                try
                {
                    //outputPath = ConfigurationManager.AppSettings["screenShotPath-GAgw"];
                    //outputPath = outputPath + orderNumber + "\\" + parcelNumber + "\\" + "duluthCity_tax_bill.pdf";
                    driver.FindElement(By.Id("ReportName")).SendKeys("Tax Statement Georgia");
                    driver.FindElement(By.XPath("//*[@id='report_form']/div/span/button")).Click();
                    IWebElement frame = driver.FindElement(By.Id("form-data"));
                    driver.SwitchTo().Frame(frame);
                    string pdfDownloadURL = driver.FindElement(By.Id("plugin")).GetAttribute("src");
                    gc.downloadfile(pdfDownloadURL, orderNumber, strParcelno, "duluthCity_tax_bill.pdf", "GA", "Gwinnett");
                    // WebClient downloadPDF = new WebClient();
                    //downloadPDF.DownloadFile(pdfDownloadURL, outputPath);
                }

                catch
                {
                }

            }

        }

        public void NorcrossCityTaxDetails(string parcelNumber, string orderNumber)
        {
            // driver = new PhantomJSDriver();
            driver.Navigate().GoToUrl("https://norcrossga.governmentwindow.com/tax.html");
            driver.FindElement(By.XPath("//*[@id='taxesSearchForm']/div[2]/div[2]/div/div/input[1]")).SendKeys(parcelNumber);
            gc.CreatePdf(orderNumber, parcelNumber, "NorcrossCity_search", driver, "GA", "Gwinnett");
            driver.FindElement(By.XPath("//*[@id='taxesSearchForm']/div[2]/div[2]/div/div/input[2]")).Click();

            gc.CreatePdf(orderNumber, parcelNumber, "Norcross_search_results", driver, "GA", "Gwinnett");
            IWebElement tableElement = driver.FindElement(By.Id("tbl_tax_results"));
            IList<IWebElement> multiaddrtableRow = tableElement.FindElements(By.TagName("tr"));
            IList<IWebElement> rowTD;
            List<string> taxSearchUrl = new List<string>();
            int index = 0;
            foreach (IWebElement row in multiaddrtableRow)
            {
                if (index != 0)
                {
                    rowTD = row.FindElements(By.TagName("td"));
                    if (rowTD.Count != 0)
                    {
                        string year = rowTD[0].Text;
                        string name = rowTD[2].Text;
                        IWebElement viewBtn = row.FindElement(By.TagName("a"));
                        taxSearchUrl.Add(name + "-" + year + "-" + viewBtn.GetAttribute("href"));
                    }

                }
                index++;
            }

            foreach (string url in taxSearchUrl)
            {

                //need to add good through & paid_date....
                string[] yrURL = url.Split('-');
                string billNum = "-", due_date = "-", prior_payment = "-", total_due = "-", tax_payer = yrURL[0], tax_authority = "-", year = yrURL[1], good_through = "-", paid_date = "-";
                driver.Navigate().GoToUrl(yrURL[2].ToString());
                gc.CreatePdf(orderNumber, parcelNumber, "norcross_tax_bill_" + year, driver, "GA", "Gwinnett");
                // tax_authority = driver.FindElement(By.XPath("//*[@id='tax_pay_bill']/div/div[2]/div[1]/div")).Text;
                tax_authority = "City of Norcross 65 Lawrenceville Street Norcross, GA 30071 (770) 448-2122";
                IWebElement table = driver.FindElement(By.Id("tbl_tax_bill_total"));
                IList<IWebElement> tableRow = table.FindElements(By.TagName("tr"));
                IList<IWebElement> TD;
                int idx = 0;
                foreach (IWebElement row in tableRow)
                {
                    TD = row.FindElements(By.TagName("td"));
                    if (idx != 0)
                    {
                        if (TD.Count != 0)
                        {
                            billNum = TD[0].Text;
                            due_date = TD[1].Text;
                            prior_payment = TD[2].Text;
                            total_due = TD[3].Text;
                            string bill = "Norcross" + "~" + "-" + "~" + billNum + "~" + "-" + "~" + prior_payment + "~" + "-" + "~" + paid_date + "~" + good_through + "~" + tax_payer + "~" + tax_authority + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + total_due;
                            gc.insert_date(orderNumber, parcelNumber, 12, bill, 1, DateTime.Now);
                            //db.ExecuteQuery("insert into data_value_master (Order_no,parcel_no,Data_Field_Text_Id,Data_Field_value,Is_Table,Sequence) values ('" + orderNumber + "','" + parcelNumber + "',12,'" + bill + "',1,1)");
                            //db.ExecuteQuery("insert into bill_details(order_no,parcel_no,paid_date,bill_number,due_date,prior_payment,total_due,good_through,tax_payer,tax_authority,county,state,city_name,year) values ('" + orderNumber + "','" + parcelNumber + "','" + paid_date + "','" + billNum + "','" + due_date + "','" + prior_payment + "','" + total_due + "','" + good_through + "','" + tax_payer + "','" + tax_authority + "','Gwinnett','Georgia','Norcross','" + year + "')");
                        }
                    }
                    idx++;
                }

            }


        }



    }
}