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
using System.Text.RegularExpressions;
using OpenQA.Selenium.PhantomJS;
using OpenQA.Selenium.Support.Extensions;
using System.Net;
using OpenQA.Selenium.Firefox;
using System.Diagnostics;
using OpenQA.Selenium.Remote;

namespace ScrapMaricopa.Scrapsource
{
    public class WebDriver_NVWashoe
    {
        string Outparcelno = "", outputPath = "";
        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        MySqlParameter[] mParam;
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        public string FTP_NVWashoe(string address, string ownername, string parcelNumber, string searchType, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            //GlobalClass.global_county = county;
            string outparcelno = "", siteaddr = "", owner1 = "", legal_desc = "", year_built = "", tax_authority = "", sub_div = "", T_legal_desc;
            string valued_year = "", tax_year = "", assess_land = "", ass_improve = "", ass_total = "", tax_value = "", exem = "", pathid = "";
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
            using (driver = new PhantomJSDriver())
            {

                try
                {

                    StartTime = DateTime.Now.ToString("HH:mm:ss");

                    if (searchType == "titleflex")
                    {

                        gc.TitleFlexSearch(orderNumber, parcelNumber, "", address, "NV", "Washoe");

                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_NVWashoe"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }

                    if (searchType == "address")
                    {
                        driver.Navigate().GoToUrl("https://www.washoecounty.us/assessor/cama/index.php");
                        driver.FindElement(By.Id("street_address")).SendKeys(address);

                        gc.CreatePdf_WOP(orderNumber, "InputPassed_AddressSearch", driver, "NV", "Washoe");

                        driver.FindElement(By.Name("situssubmit")).SendKeys(Keys.Enter);
                        Thread.Sleep(7000);


                        By Icount = By.Id("search_results_info");
                        if (Exists(Icount))
                        {
                            // success
                            multiparcel(orderNumber);
                            HttpContext.Current.Session["multiParcel_NVWashoe"] = "Yes";
                            return "MultiParcel";
                        }

                    }
                    else if (searchType == "parcel")
                    {

                        driver.Navigate().GoToUrl("https://www.washoecounty.us/assessor/cama/index.php");
                        driver.FindElement(By.Id("parid")).SendKeys(parcelNumber);
                        gc.CreatePdf(orderNumber, parcelNumber, "InputPassed_ParcelSearch", driver, "NV", "Washoe");
                        driver.FindElement(By.Id("apnsubmit")).SendKeys(Keys.Enter);
                        Thread.Sleep(7000);
                    }

                    else if (searchType == "ownername")
                    {
                        //Thread.Sleep(3000);
                        driver.Navigate().GoToUrl("https://www.washoecounty.us/assessor/cama/index.php");
                        driver.FindElement(By.Id("o_lastname")).SendKeys(ownername);
                        gc.CreatePdf_WOP(orderNumber, "InputPassed_OwnerNameSearch", driver, "NV", "Washoe");
                        driver.FindElement(By.Name("ownsubmit")).SendKeys(Keys.Enter);
                        Thread.Sleep(7000);
                        bool norec = false;
                        try
                        {
                            Thread.Sleep(3000);
                            IWebElement norecord = driver.FindElement(By.XPath("//*[@id='search_div']/div[1]"));
                            string[] name = ownername.Split(null);
                            string firstname = name[0]; string lastname = name[1];
                            if (firstname.Contains(",") && lastname.Contains(","))
                            {
                                firstname = firstname.Replace(",", "").Trim();
                                lastname = lastname.Replace(",", "").Trim();
                            }
                            if (lastname.Length == 1)
                            {
                                driver.FindElement(By.Id("o_firstname")).SendKeys(firstname);
                                gc.CreatePdf_WOP(orderNumber, "InputPassed_OwnerNameSearch", driver, "NV", "Washoe");
                                driver.FindElement(By.Name("ownsubmit")).SendKeys(Keys.Enter);
                                Thread.Sleep(7000);
                            }
                            else
                            {
                                driver.FindElement(By.Id("o_firstname")).SendKeys(lastname);
                                driver.FindElement(By.Id("o_lastname")).SendKeys(firstname);
                                gc.CreatePdf_WOP(orderNumber, "InputPassed_OwnerNameSearch", driver, "NV", "Washoe");
                                driver.FindElement(By.Name("ownsubmit")).SendKeys(Keys.Enter);
                                Thread.Sleep(7000);
                            }
                        }
                        catch
                        {


                        }

                        try
                        {
                            IWebElement multi = driver.FindElement(By.XPath("//*[@id='search_results']"));
                            multiparcel(orderNumber);
                            HttpContext.Current.Session["multiParcel_NVWashoe"] = "Yes";
                            return "MultiParcel";

                        }
                        catch
                        {
                        }



                    }

                    try
                    {
                        IWebElement INodata = driver.FindElement(By.Id("search_div"));
                        if(INodata.Text.Contains("No results were found"))
                        {
                            HttpContext.Current.Session["Nodata_NVWashoe"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }
                    //APN: 078-221-13

                    outparcelno = driver.FindElement(By.XPath("//*[@id='search_div']/div[1]/div[5]/span[1]")).Text.Trim();
                    outparcelno = outparcelno.Replace("APN: ", "");
                    string outparcelnowoh = outparcelno.Replace("-", "").Trim();
                    gc.CreatePdf(orderNumber, outparcelnowoh, "AssessmentDetails", driver, "NV", "Washoe");
                    siteaddr = driver.FindElement(By.XPath("//*[@id='owner_data']/table/tbody/tr[1]/td[2]")).Text.Trim();
                    owner1 = driver.FindElement(By.XPath("//*[@id='owner_data']/table/tbody/tr[2]/td[2]")).Text.Trim();

                    //legal_desc = driver.FindElement(By.XPath("//*[@id='owner_data']/table/tbody/tr[8]/td[2]")).Text.Replace("'", "").Trim();
                    sub_div = driver.FindElement(By.XPath("//*[@id='owner_data']/table/tbody/tr[9]/td[2]")).Text.Replace("'", "").Trim();
                    year_built = driver.FindElement(By.XPath("//*[@id='building_data']/table/tbody/tr[3]/td[2]")).Text.Trim();

                    string property_details = siteaddr + "~" + owner1 + "~" + legal_desc + "~" + sub_div + "~" + year_built;
                    gc.insert_date(orderNumber, outparcelno, 1, property_details, 1, DateTime.Now);
                    //db.ExecuteQuery("insert into data_value_master (Order_no,parcel_no,Data_Field_Text_Id,Data_Field_value,Is_Table,Sequence) values ('" + orderNumber + "','" + outparcelno + "',1 ,'" + property_details + "',1,1)");
                    //db.ExecuteQuery("insert into real_property (orderno, apn, address,legal_description,subdivision,year_built,owner) values ('" + orderNumber + "', '" + outparcelno + "','" + siteaddr + "','" + legal_desc + "','" + sub_div + "','" + year_built + "','" + owner1 + "')");

                    //valuation Information                
                    IWebElement valuetableElement = driver.FindElement(By.XPath("//*[@id='value_data']/table"));
                    IList<IWebElement> valuetableRow = valuetableElement.FindElements(By.TagName("tr"));
                    IList<IWebElement> valuerowTD;
                    List<string> history = new List<string>();
                    List<string> taxland = new List<string>();
                    List<string> taximprove = new List<string>();
                    List<string> taxtotal = new List<string>();
                    List<string> assessland = new List<string>();
                    List<string> assessimprove = new List<string>();
                    List<string> assesstotal = new List<string>();
                    int i = 0;
                    foreach (IWebElement row in valuetableRow)
                    {
                        valuerowTD = row.FindElements(By.TagName("td"));
                        if (valuerowTD.Count != 0)
                        {
                            if (i == 0)
                            {

                                history.Add(valuerowTD[1].Text.Trim().Replace("\r\n", ""));
                                history.Add(valuerowTD[2].Text.Trim().Replace("\r\n", ""));
                            }
                            else if (i == 1)
                            {
                                taxland.Add(valuerowTD[1].Text);
                                taxland.Add(valuerowTD[2].Text);
                            }
                            else if (i == 2)
                            {
                                taximprove.Add(valuerowTD[1].Text);
                                taximprove.Add(valuerowTD[2].Text);
                            }
                            else if (i == 3)
                            {
                                taxtotal.Add(valuerowTD[1].Text);
                                taxtotal.Add(valuerowTD[2].Text);
                            }
                            else if (i == 4)
                            {
                                assessland.Add(valuerowTD[1].Text);
                                assessland.Add(valuerowTD[2].Text);
                            }
                            else if (i == 5)
                            {
                                assessimprove.Add(valuerowTD[1].Text);
                                assessimprove.Add(valuerowTD[2].Text);
                            }
                            else if (i == 6)
                            {
                                assesstotal.Add(valuerowTD[1].Text);
                                assesstotal.Add(valuerowTD[2].Text);
                            }
                        }
                        i++;
                    }
                    string assessment1 = history[0] + "~" + taxland[0] + "~" + taximprove[0] + "~" + taxtotal[0] + "~" + assessland[0] + "~" + assessimprove[0] + "~" + assesstotal[0];
                    string assessment2 = history[1] + "~" + taxland[1] + "~" + taximprove[1] + "~" + taxtotal[1] + "~" + assessland[1] + "~" + assessimprove[1] + "~" + assesstotal[1];
                    gc.insert_date(orderNumber, outparcelno, 2, assessment1, 1, DateTime.Now);
                    gc.insert_date(orderNumber, outparcelno, 2, assessment2, 1, DateTime.Now);
                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");
                    //db.ExecuteQuery("insert into data_value_master (Order_no,parcel_no,Data_Field_Text_Id,Data_Field_value,Is_Table,Sequence) values ('" + orderNumber + "','" + outparcelno + "',2 ,'" + assessment1 + "',1,1)");
                    //db.ExecuteQuery("insert into data_value_master (Order_no,parcel_no,Data_Field_Text_Id,Data_Field_value,Is_Table,Sequence) values ('" + orderNumber + "','" + outparcelno + "',2 ,'" + assessment2 + "',1,1)");
                    //db.ExecuteQuery("insert into la_assessor(order_no,parcel_no,year,land,improvements,total,net_asses_value,tax_value,total_assessment) values('" + orderNumber + "','" + outparcelno + "','" + history[0] + "','" + taxland[0] + "','" + taximprove[0] + "','" + taxtotal[0] + "','" + assessland[0] + "','" + assessimprove[0] + "','" + assesstotal[0] + "') ");
                    //db.ExecuteQuery("insert into la_assessor(order_no,parcel_no,year,land,improvements,total,net_asses_value,tax_value,total_assessment) values('" + orderNumber + "','" + outparcelno + "','" + history[1] + "','" + taxland[1] + "','" + taximprove[1] + "','" + taxtotal[1] + "','" + assessland[1] + "','" + assessimprove[1] + "','" + assesstotal[1] + "') ");


                    //outparcelno = "035-310-41";
                    //Treasurer Details
                    driver.Navigate().GoToUrl("https://nv-washoe-treasurer.manatron.com/Tabs/TaxSearch.aspx");
                    var ddlsearch = driver.FindElement(By.Id("selSearchBy"));
                    var selectElement = new SelectElement(ddlsearch);
                    selectElement.SelectByValue("Column1=");
                    driver.FindElement(By.Id("fldInput")).SendKeys(outparcelnowoh);
                    var wait = new WebDriverWait(driver, TimeSpan.FromMilliseconds(3000));
                    wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//*[@id='btnsearch']")));



                    gc.CreatePdf(orderNumber, outparcelnowoh, "InputPassed_Tax_ParcelSearch", driver, "NV", "Washoe");
                    driver.FindElement((By.XPath("//*[@id='btnsearch']"))).SendKeys(Keys.Enter);
                    Thread.Sleep(3000);
                    driver.FindElement(By.LinkText(outparcelnowoh)).SendKeys(Keys.Enter);
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, outparcelnowoh, "Tax_MainPage", driver, "NV", "Washoe");

                    T_legal_desc = driver.FindElement(By.XPath("//*[@id='lxT528']/table/tbody/tr[5]/td")).Text;
                    tax_authority = driver.FindElement(By.XPath("//*[@id='dnn_ctr480_HtmlModule_HtmlModule_lblContent']")).Text;
                    tax_authority = gc.Between(tax_authority, "Mailing Address:", "Overnight Address:").Trim();

                    //db.ExecuteQuery("update real_property set legal_description ='" + legal_desc + "', tax_authority = '" + tax_authority + "' where orderno ='" + orderNumber + "' and apn = '" + outparcelno + "' ");
                    property_details = siteaddr + "~" + owner1 + "~" + T_legal_desc + "~" + sub_div + "~" + year_built;
                    //db.ExecuteQuery("insert into data_value_master (Order_no,parcel_no,Data_Field_Text_Id,Data_Field_value,Is_Table,Sequence) values ('" + orderNumber + "','" + outparcelno + "',1 ,'" + property_details + "',1,1)");
                    gc.insert_date(orderNumber, outparcelno, 1, property_details, 1, DateTime.Now);
                    gc.CreatePdf(orderNumber, outparcelnowoh, "TaxBill_DetailsPage", driver, "NV", "Washoe");
                    //*[@id="529"]/table
                    IWebElement taxbilltable = driver.FindElement(By.XPath("//*[@id='529']/table"));
                    IList<IWebElement> taxbilltableRow = taxbilltable.FindElements(By.TagName("tr"));
                    int rowcount = taxbilltableRow.Count;
                    IList<IWebElement> taxbillrowTD;
                    int w = 1;
                    foreach (IWebElement rowid in taxbilltableRow)
                    {
                        taxbillrowTD = rowid.FindElements(By.TagName("td"));
                        if (taxbillrowTD.Count != 0 && !rowid.Text.Contains("Tax Year"))
                        {
                            if (w < rowcount)
                            {
                                string cons = taxbillrowTD[0].Text + "~" + taxbillrowTD[1].Text + "~" + taxbillrowTD[2].Text + "~" + taxbillrowTD[3].Text + "~" + taxbillrowTD[4].Text + "~" + taxbillrowTD[5].Text;
                                gc.insert_date(orderNumber, outparcelno, 66, cons, 1, DateTime.Now);
                            }
                            else
                            {
                                string cons = "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "Total" + "~" + taxbillrowTD[0].Text;
                                gc.insert_date(orderNumber, outparcelno, 66, cons, 1, DateTime.Now);
                            }
                        }
                        w++;
                    }


                    List<string> bill_year = new List<string>();

                    int l = 0;
                    for (int m = 1; m < rowcount - 1; m++)
                    {
                        driver.FindElement(By.XPath("//*[@id='529']/table/tbody/tr[" + m + "]/td[1]/a")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);

                        //installments
                        IWebElement inst1table = driver.FindElement(By.Id("installments"));
                        IList<IWebElement> inst1tableRow = inst1table.FindElements(By.TagName("tr"));
                        int inst1tableRowcount = inst1tableRow.Count;
                        IList<IWebElement> inst1rowTD;
                        int a = 0;
                        string inst = "";
                        foreach (IWebElement rowid in inst1tableRow)
                        {
                            inst1rowTD = rowid.FindElements(By.TagName("td"));
                            if (inst1rowTD.Count != 0)
                            {
                                if (a > 0 && a < inst1tableRowcount - 1)
                                {
                                    //installment~due_date~tax_year~tax_amount~penalty_amount~Interest~total_due
                                    inst = inst1rowTD[0].Text + "~" + inst1rowTD[1].Text + "~" + inst1rowTD[2].Text + "~" + inst1rowTD[3].Text + "~" + inst1rowTD[4].Text + "~" + inst1rowTD[5].Text + "~" + inst1rowTD[6].Text;
                                    gc.insert_date(orderNumber, outparcelno, 3, inst, 1, DateTime.Now);

                                }
                                if (a == inst1tableRowcount - 1)
                                {
                                    inst = " - " + "~" + " - " + "~" + inst1rowTD[0].Text + "~" + inst1rowTD[1].Text + "~" + inst1rowTD[2].Text + "~" + inst1rowTD[3].Text + "~" + inst1rowTD[4].Text;
                                    gc.insert_date(orderNumber, outparcelno, 3, inst, 1, DateTime.Now);
                                }
                            }
                            a++;
                        }

                        try
                        {
                            //Payment History                    
                            IWebElement payhisttable = driver.FindElement(By.XPath("//*[@id='lxT536']/table"));
                            IList<IWebElement> payhisttableRow = payhisttable.FindElements(By.TagName("tr"));
                            IList<IWebElement> payhistrowTD;

                            foreach (IWebElement rowid1 in payhisttableRow)
                            {
                                payhistrowTD = rowid1.FindElements(By.TagName("td"));
                                if (payhistrowTD.Count != 0)
                                {
                                    string bill = payhistrowTD[0].Text + "~" + payhistrowTD[1].Text + "~" + payhistrowTD[2].Text + "~" + payhistrowTD[3].Text + "~" + payhistrowTD[4].Text;
                                    gc.insert_date(orderNumber, outparcelno, 4, bill, 1, DateTime.Now);


                                }

                            }
                        }
                        catch
                        {
                        }


                        //Tax Detail
                        //*[@id="lxT534"]/table
                        //IList<IWebElement> breakdownslist = driver.FindElements(By.XPath("//*[@id='534_" + pathid + "]/div/table"));
                        IWebElement taxdetailtable = driver.FindElement(By.XPath("//*[@id='lxT534']/table"));
                        IList<IWebElement> taxdetailtableRow = taxdetailtable.FindElements(By.TagName("tr"));
                        int taxrowcount = taxdetailtableRow.Count;
                        IList<IWebElement> taxdetailrowTD;
                        int b = 1;
                        string tax_breakdown = "";
                        foreach (IWebElement rowid1 in taxdetailtableRow)
                        {
                            taxdetailrowTD = rowid1.FindElements(By.TagName("td"));
                            if (taxdetailrowTD.Count != 0 && b <= taxrowcount)
                            {

                                if (b % 2 == 0 && b != taxrowcount)
                                {
                                    if (l == 0)
                                    {
                                        pathid = driver.FindElement(By.XPath("//*[@id=\"lxT534\"]/table/tbody/tr[" + b + "]/td[1]")).GetAttribute("tb");
                                        driver.FindElement(By.XPath("//*[@id='lxT534']/table/tbody/tr[" + b + "]/td[1]/a")).SendKeys(Keys.Enter);
                                        Thread.Sleep(2000);
                                    }
                                    tax_breakdown = taxdetailrowTD[0].Text + "~" + "-" + "~" + taxdetailrowTD[1].Text + "~" + taxdetailrowTD[2].Text + "~" + taxdetailrowTD[3].Text;
                                    gc.insert_date(orderNumber, outparcelno, 5, tax_breakdown, 1, DateTime.Now);
                                    //db.ExecuteQuery("insert into data_value_master (Order_no,parcel_no,Data_Field_Text_Id,Data_Field_value,Is_Table,Sequence) values ('" + orderNumber + "','" + outparcelno + "',5,'" + tax_breakdown + "',1,1)");
                                    //db.ExecuteQuery("insert into tax_breakdown_details (order_no, parcel_no,tax_authority,gross_tax,credit,net_tax) values ('" + orderNumber + "', '" + outparcelno + "', '" + taxdetailrowTD[0].Text + "', '" + taxdetailrowTD[1].Text + "', '" + taxdetailrowTD[2].Text + "', '" + taxdetailrowTD[3].Text + "')");
                                }
                                if (b == taxrowcount)
                                {
                                    tax_breakdown = "Total Tax" + "~" + "-" + "~" + taxdetailrowTD[0].Text + "~" + taxdetailrowTD[1].Text + "~" + taxdetailrowTD[2].Text;
                                    gc.insert_date(orderNumber, outparcelno, 5, tax_breakdown, 1, DateTime.Now);
                                    //  db.ExecuteQuery("insert into tax_breakdown_details (order_no, parcel_no,tax_authority,gross_tax,credit,net_tax) values ('" + orderNumber + "', '" + outparcelno + "', 'Total Tax','" + taxdetailrowTD[0].Text + "', '" + taxdetailrowTD[1].Text + "', '" + taxdetailrowTD[2].Text + "')");
                                }

                            }
                            b++;


                        }

                        if (l == 0)
                        {
                            gc.CreatePdf(orderNumber, outparcelnowoh, "Tax_HistoryDetailsPage", driver, "NV", "Washoe");

                            try
                            {
                                IWebElement newtable = driver.FindElement(By.XPath("//*[@id='534_" + pathid + " + 0']/div/table"));
                                IList<IWebElement> newtableRow = newtable.FindElements(By.TagName("tr"));
                                IList<IWebElement> newtablerowTD;
                                int newtableRowcount = newtableRow.Count;

                                foreach (IWebElement rowid1 in newtableRow)
                                {
                                    newtablerowTD = rowid1.FindElements(By.TagName("td"));
                                    if (newtablerowTD.Count != 0 && !rowid1.Text.Contains("Authority"))
                                    {
                                        tax_breakdown = tax_breakdown = newtablerowTD[0].Text + "~" + newtablerowTD[1].Text + "~" + newtablerowTD[2].Text + "~" + newtablerowTD[3].Text + "~" + newtablerowTD[4].Text;
                                        gc.insert_date(orderNumber, outparcelno, 5, tax_breakdown, 1, DateTime.Now);
                                        //db.ExecuteQuery("insert into data_value_master (Order_no,parcel_no,Data_Field_Text_Id,Data_Field_value,Is_Table,Sequence) values ('" + orderNumber + "','" + outparcelno + "',5,'" + tax_breakdown + "',1,1)");
                                        // db.ExecuteQuery("insert into tax_breakdown_details (order_no, parcel_no,tax_authority,net_rate,gross_tax,credit,net_tax) values ('" + orderNumber + "', '" + outparcelno + "', '" + newtablerowTD[0].Text + "', '" + newtablerowTD[1].Text + "', '" + newtablerowTD[2].Text + "', '" + newtablerowTD[3].Text + "', '" + newtablerowTD[4].Text + "')");
                                    }
                                }
                            }

                            catch
                            {
                            }


                            for (int z = 300000; z <= 300070; z++)
                            {
                                string tableid = Convert.ToInt32(z).ToString();
                                string pathtdid = pathid + tableid;
                                try
                                {
                                    IWebElement commontable = driver.FindElement(By.XPath("//*[@id='534_" + pathtdid + "']/div/table"));
                                    IList<IWebElement> commontableRow = commontable.FindElements(By.TagName("tr"));
                                    IList<IWebElement> commontablerowTD;
                                    int commontableRowcount = commontableRow.Count;

                                    foreach (IWebElement rowid1 in commontableRow)
                                    {
                                        commontablerowTD = rowid1.FindElements(By.TagName("td"));
                                        if (commontablerowTD.Count != 0 && !rowid1.Text.Contains("Authority"))
                                        {
                                            tax_breakdown = commontablerowTD[0].Text + "~" + commontablerowTD[1].Text + "~" + commontablerowTD[2].Text + "~" + commontablerowTD[3].Text + "~" + commontablerowTD[4].Text;
                                            gc.insert_date(orderNumber, outparcelno, 5, tax_breakdown, 1, DateTime.Now);
                                            // db.ExecuteQuery("insert into data_value_master (Order_no,parcel_no,Data_Field_Text_Id,Data_Field_value,Is_Table,Sequence) values ('" + orderNumber + "','" + outparcelno + "',5,'" + tax_breakdown + "',1,1)");
                                            //db.ExecuteQuery("insert into tax_breakdown_details (order_no, parcel_no,tax_authority,net_rate,gross_tax,credit,net_tax) values ('" + orderNumber + "', '" + outparcelno + "', '" + commontablerowTD[0].Text + "', '" + commontablerowTD[1].Text + "', '" + commontablerowTD[2].Text + "', '" + commontablerowTD[3].Text + "', '" + commontablerowTD[4].Text + "')");
                                        }
                                    }
                                }

                                catch
                                {
                                }
                                pathtdid = "";
                            }

                        }
                        l++;
                        driver.Navigate().Back();
                    }
                    TaxTime = DateTime.Now.ToString("HH:mm:ss");
                    // AMG Details 
                    string district_amgid = "-", amg_name = "-", status = "-", unbill_principal = "-", situs = "-", Legal_desc = "-", original_ass = "-", payoff = "-", due_desc = "-";
                    string curr_due_principal = "-", curr_due_interest = "-", curr_due_penalty = "-", curr_due_other = "-", curr_due_total_due = "-", amg_tax_authority = "-";
                    string ENI_principal = "-", ENI_interest = "-", ENI_penalty = "-", ENI_other = "-", ENI_total_due = "-";
                    string POB_Prepaid_principal = "-", POB_prepaid_interest = "-", POB_Prepay_Penalty = "-", POB_lien_reles = "-", POB_Curr_due = "-", POB_total_payoff = "-";
                    //string outparcelnowoh = "20808013";
                    // string outparcelnowoh = "03531041"; 
                    driver.Navigate().GoToUrl("https://www.amgnv.com/");
                    driver.FindElement(By.Name("Parcel")).SendKeys(outparcelnowoh);
                    gc.CreatePdf(orderNumber, outparcelnowoh, "InputPassed_Amg_ParcelSearch", driver, "NV", "Washoe");
                    driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr/td[6]/table/tbody/tr[2]/td[2]/p/table[2]/tbody/tr/td/table/tbody/tr[2]/td[1]/form/center/b/font/font/input")).SendKeys(Keys.Enter);
                    Thread.Sleep(2000);


                    gc.CreatePdf(orderNumber, outparcelnowoh, "Amg_MainPage", driver, "NV", "Washoe");

                    By amgfound = By.XPath("/html/body/table[3]/tbody/tr/td[6]/table/tbody/tr[2]/td[2]/p/table[2]/tbody/tr/td/b/font");

                    try
                    {
                        if (!Exists(amgfound))
                        {

                            driver.FindElement(By.LinkText(outparcelnowoh)).SendKeys(Keys.Enter);
                            Thread.Sleep(1000);
                            gc.CreatePdf(orderNumber, outparcelnowoh, "Amg_DetailsPage", driver, "NV", "Washoe");


                            IWebElement amgdetailtable = driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr/td[6]/table/tbody/tr[2]/td[2]/table[3]"));
                            IList<IWebElement> amgdetailtableRow = amgdetailtable.FindElements(By.TagName("tr"));
                            IList<IWebElement> amgdetailtablerowTD;
                            foreach (IWebElement rowid1 in amgdetailtableRow)
                            {
                                amgdetailtablerowTD = rowid1.FindElements(By.TagName("td"));
                                if (amgdetailtablerowTD.Count != 0 && !rowid1.Text.Contains("Parcel") && !rowid1.Text.Contains("Amounts"))
                                {
                                    district_amgid = amgdetailtablerowTD[1].Text.Trim(); amg_name = amgdetailtablerowTD[2].Text.Trim(); unbill_principal = amgdetailtablerowTD[4].Text.Trim();
                                }
                            }

                            IWebElement situstable = driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr/td[6]/table/tbody/tr[2]/td[2]/table[4]"));
                            IList<IWebElement> situstableRow = situstable.FindElements(By.TagName("tr"));
                            IList<IWebElement> situstablerowTD;
                            int situsrowcount = situstableRow.Count;
                            int s = 0;
                            foreach (IWebElement rowid1 in situstableRow)
                            {
                                situstablerowTD = rowid1.FindElements(By.TagName("td"));
                                if (situstablerowTD.Count != 0 && !rowid1.Text.Contains("Situs ") && s < situsrowcount - 1)
                                {
                                    situs = situstablerowTD[0].Text.Replace(",", "").Trim(); original_ass = situstablerowTD[1].Text.Trim(); payoff = situstablerowTD[2].Text.Trim();
                                }
                                if (s == situsrowcount - 1)
                                {
                                    Legal_desc = situstablerowTD[0].Text.Trim();
                                }
                                s++;
                            }

                            IWebElement duetable = driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr/td[6]/table/tbody/tr[2]/td[2]/table[6]"));
                            IList<IWebElement> duetableRow = duetable.FindElements(By.TagName("tr"));
                            IList<IWebElement> duetablerowTD;
                            int duerowcount = duetableRow.Count;
                            int d = 0;
                            foreach (IWebElement rowid1 in duetableRow)
                            {
                                duetablerowTD = rowid1.FindElements(By.TagName("td"));
                                if (duetablerowTD.Count != 0 && !rowid1.Text.Contains("Principal") && d == 1)
                                {
                                    curr_due_principal = duetablerowTD[1].Text.Trim(); curr_due_interest = duetablerowTD[2].Text.Trim(); curr_due_penalty = duetablerowTD[3].Text.Trim(); curr_due_other = duetablerowTD[4].Text.Trim(); curr_due_total_due = duetablerowTD[5].Text.Trim();
                                }
                                if (d == 2)
                                {
                                    ENI_principal = duetablerowTD[1].Text.Trim(); ENI_interest = duetablerowTD[2].Text.Trim(); ENI_penalty = duetablerowTD[3].Text.Trim(); ENI_other = duetablerowTD[4].Text.Trim(); ENI_total_due = duetablerowTD[5].Text.Trim();
                                }
                                d++;
                            }

                            amg_tax_authority = driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr/td[6]/table/tbody/tr[2]/td[2]/table[8]/tbody/tr[2]/td[2]/table/tbody/tr/td[2]")).Text;
                            amg_tax_authority = amg_tax_authority.Replace("\n", "");

                            try
                            {

                                string currentHandle = driver.CurrentWindowHandle;
                                IWebElement element = driver.FindElement(By.LinkText(payoff));
                                PopupWindowFinder finder = new PopupWindowFinder(driver);
                                string popupWindowHandle = finder.Click(element);

                                driver.SwitchTo().Window(popupWindowHandle);
                                gc.CreatePdf(orderNumber, outparcelnowoh, "Amg_PayOffPage", driver, "NV", "Washoe");

                                IWebElement POBtable = driver.FindElement(By.XPath("/html/body/center/table[2]/tbody/tr/td/table"));
                                IList<IWebElement> POBtableRow = POBtable.FindElements(By.TagName("tr"));
                                //IList<IWebElement> POBtablerowTD;
                                int POBrowcount = POBtableRow.Count;
                                POB_Prepaid_principal = POBtableRow[1].Text.Replace("Prepaid Principal:", "").Trim();
                                POB_prepaid_interest = POBtableRow[2].Text.Replace("Prepaid Interest:", "").Trim();
                                POB_Prepay_Penalty = POBtableRow[3].Text.Replace("Prepayment Penalty:", "").Trim();
                                POB_lien_reles = POBtableRow[4].Text.Replace("Lien Release:", "").Trim(); ;
                                POB_Curr_due = POBtableRow[5].Text.Replace("Current Due:", "").Trim();
                                POB_total_payoff = POBtableRow[6].Text.Replace("Total Payoff:", "").Trim();
                                string amgdetails = district_amgid + "~" + amg_name + "~" + situs + "~" + unbill_principal + "~" + Legal_desc + "~" + original_ass + "~" + payoff + "~" + curr_due_principal + "~" + curr_due_interest + "~" + curr_due_penalty + "~" + curr_due_other + "~" + curr_due_total_due + "~" + ENI_principal + "~" + ENI_interest + "~" + ENI_penalty + "~" + ENI_other + "~" + ENI_total_due + "~" + amg_tax_authority + "~" + POB_Prepaid_principal + "~" + POB_prepaid_interest + "~" + POB_Prepay_Penalty + "~" + POB_lien_reles + "~" + POB_Curr_due + "~" + POB_total_payoff;
                                gc.insert_date(orderNumber, outparcelno, 6, amgdetails, 1, DateTime.Now);
                                //db.ExecuteQuery("insert into data_value_master (Order_no,parcel_no,Data_Field_Text_Id,Data_Field_value,Is_Table,Sequence) values ('" + orderNumber + "','" + outparcelno + "',6,'" + amgdetails + "',1,1)");
                                //  db.ExecuteQuery("insert into amg_details (order_no,parcel_no,District_amgid,Name,Situs,Unbill_prinicipal,Legaldescription,Original_Assesment,Payoff,curr_due_Principal,curr_due_Interest,curr_due_Penalty,curr_due_Other,curr_due_Total_Due,ENI_principal,ENI_interest,ENI_penalty,ENI_other,ENI_total_due,Tax_Authority,PayOff_Br_Prepaid_Principal,PayOff_Br_Prepaid_Interest,PayOff_Br_Prepayment_Penalty,PayOff_Br_Lien_Release,PayOff_Br_Current_Due,PayOff_Br_Total_Payoff) values ('" + orderNumber + "','" + parcelNumber + "','" + district_amgid + "','" + amg_name + "','" + situs + "','" + unbill_principal + "','" + Legal_desc + "','" + original_ass + "','" + payoff + "','" + curr_due_principal + "','" + curr_due_interest + "','" + curr_due_penalty + "','" + curr_due_other + "','" + curr_due_total_due + "','" + amg_tax_authority + "','" + ENI_principal + "','" + ENI_interest + "','" + ENI_penalty + "','" + ENI_other + "','" + ENI_total_due + "','" + POB_Prepaid_principal + "','" + POB_prepaid_interest + "','" + POB_Prepay_Penalty + "','" + POB_lien_reles + "','" + POB_Curr_due + "','" + POB_total_payoff + "')");
                                // Do whatever you need to on the popup browser, then...
                                driver.Quit();
                                //driver.SwitchTo().Window(currentHandle);
                            }
                            catch
                            {
                            }

                        }

                        else
                        {
                            driver.Quit();
                            gc.mergpdf(orderNumber, "NV", "Washoe");
                            LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                            gc.insert_TakenTime(orderNumber, "NV", "Washoe", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);
                            return "Data Inserted Successfully";
                        }
                    }
                    catch
                    {
                    }
                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");

                    driver.Quit();
                    gc.insert_TakenTime(orderNumber, "NV", "Washoe", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);
                    gc.mergpdf(orderNumber, "NV", "Washoe");
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
        public bool Exists(By by)
        {
            if (driver.FindElements(by).Count != 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        
   

        private string multiparcel(string ordernumber)
        {
            gc.CreatePdf_WOP(ordernumber, "Amg_PayOffPage",driver, "NV", "Washoe");
            var ddl = driver.FindElement(By.XPath("//*[@id='search_results_length']/label/select"));
            var selectElement = new SelectElement(ddl);
            selectElement.SelectByValue("-1");
            string entrycount = driver.FindElement(By.Id("search_results_info")).Text;
            //Showing 1 to 63 of 63 entries
            entrycount = gc.Between(entrycount, "1 to ", " of ");
            int count = Int32.Parse(entrycount);

            if (count > 1 && count <= 10)
            {
                HttpContext.Current.Session["multiParcel_NVWashoeMaximum"] = "Yes";
                //Select address from list....
                IWebElement multiaddrtableElement = driver.FindElement(By.XPath("//*[@id='search_results']"));
                IList<IWebElement> multiaddrtableRow = multiaddrtableElement.FindElements(By.TagName("tr"));
                IList<IWebElement> multiaddrrowTD;
                int j = 0;
                foreach (IWebElement row in multiaddrtableRow)
                {
                    multiaddrrowTD = row.FindElements(By.TagName("td"));
                    if (multiaddrrowTD.Count != 0)
                    {
                        if (j < 10)
                        {
                            try
                            {
                                string multiowner = multiaddrrowTD[1].Text + "~" + multiaddrrowTD[2].Text + "~" + multiaddrrowTD[3].Text;
                                gc.insert_date(ordernumber, multiaddrrowTD[0].Text.Trim(), 7, multiowner, 1, DateTime.Now);
                                //db.ExecuteQuery("insert into data_value_master (Order_no,parcel_no,Data_Field_Text_Id,Data_Field_value,Is_Table,Sequence) values ('" + ordernumber + "','" + multiaddrrowTD[0].Text + "',7,'" + multiowner + "',1,1)");
                                // db.ExecuteQuery("insert into la_multiowner(order_no,parcel_no,type,situs_address,taxpayer_name) values ('" + ordernumber + "','" + multiaddrrowTD[0].Text + "','" + multiaddrrowTD[1].Text + "','" + multiaddrrowTD[2].Text + "','" + multiaddrrowTD[3].Text + "') ");
                            }
                            catch
                            {
                            }
                            j++;
                        }

                    }
                }

            }

            else
            {
                HttpContext.Current.Session["multiParcel_NVWashoeMaximum"] = "Maximum";
               // GlobalClass.multiParcel_washoe_count = "Maximum";
                return "Maximum";
            }
            driver.Quit();
            return "Maximum";
        }
    }

}