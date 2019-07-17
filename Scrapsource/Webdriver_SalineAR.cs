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
    public class Webdriver_SalineAR
    {
        IWebDriver driver;
        DBconnection db = new DBconnection();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        GlobalClass gc = new GlobalClass();
        string Yearbuild, parcel_number, Addressmax;
        string Tax_Status = "", TotalMandatory = "", TaxPaid = "", Balance = "";
        IWebElement Currenttaxinfo;
        public string FTP_Saline(string streetno, string streettype, string streetname, string unitnumber, string ownernm, string parcelNumber, string searchType, string orderNumber, string directParcel)
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
                        string address = streetno + " " + streettype + " " + streetname + " " + unitnumber;
                        gc.TitleFlexSearch(orderNumber, parcelNumber, "", address, "AR", "Saline");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_SalineAR"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }
                    driver.Navigate().GoToUrl("https://www.arcountydata.com/county.asp?county=Saline");
                    driver.FindElement(By.XPath("//*[@id='Assessor']/div/div[2]/a")).SendKeys(Keys.Enter);
                    Thread.Sleep(2000);
                    if (searchType == "address")
                    {
                        driver.FindElement(By.Id("StreetNumber")).SendKeys(streetno);
                        driver.FindElement(By.Id("StreetName")).SendKeys(streetname);
                        gc.CreatePdf_WOP(orderNumber, "SearchBefore", driver, "AR", "Saline");
                        driver.FindElement(By.Id("Search")).SendKeys(Keys.Enter);
                        gc.CreatePdf_WOP(orderNumber, "SearchAfter", driver, "AR", "Saline");
                        Thread.Sleep(2000);

                        try
                        {
                            Addressmax = driver.FindElement(By.XPath("/html/body/div[2]/div[2]/div[2]/table/tbody/tr[1]/td[2]")).Text.Trim();
                            if (Convert.ToInt32(Addressmax) == 1 && Convert.ToInt32(Addressmax) != 0)
                            {

                                driver.FindElement(By.XPath("//*[@id='parcel_report']/table/tbody/tr[2]/td[1]/a")).SendKeys(Keys.Enter);
                                Thread.Sleep(2000);
                                gc.CreatePdf_WOP(orderNumber, "AddressSearch Result", driver, "AR", "Saline");
                            }
                            else
                            {
                                Multiple(driver, orderNumber, Addressmax);
                                driver.Quit();
                                return "MultiParcel";
                            }

                        }
                        catch { }
                    }

                    if (searchType == "parcel")
                    {
                        driver.FindElement(By.Id("ParcelNumber")).SendKeys(parcelNumber);
                        driver.FindElement(By.Id("Search")).SendKeys(Keys.Enter);
                        gc.CreatePdf(orderNumber, parcelNumber, "SearchParcel Before", driver, "AR", "Saline");
                        Thread.Sleep(2000);
                        try
                        {
                            driver.FindElement(By.XPath("//*[@id='parcel_report']/table/tbody/tr[2]/td[1]/a")).SendKeys(Keys.Enter);
                            gc.CreatePdf(orderNumber, parcelNumber, "SearchParcel After", driver, "AR", "Saline");
                            Thread.Sleep(2000);
                        }
                        catch { }
                    }
                    if (searchType == "ownername")
                    {
                        driver.FindElement(By.Id("OwnerName")).SendKeys(ownernm);
                        driver.FindElement(By.Id("Search")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "Searchownername After", driver, "AR", "Saline");
                        try
                        {
                            Addressmax = driver.FindElement(By.XPath("/html/body/div[2]/div[2]/div[2]/table/tbody/tr[1]/td[2]")).Text.Trim();
                            if (Convert.ToInt32(Addressmax) == 1 && Convert.ToInt32(Addressmax) != 0)
                            {
                                driver.FindElement(By.XPath("//*[@id='parcel_report']/table/tbody/tr[2]/td[1]/a")).SendKeys(Keys.Enter);
                                Thread.Sleep(2000);
                            }
                            else
                            {
                                Multiple(driver, orderNumber, Addressmax);
                                driver.Quit();
                                return "MultiParcel";
                            }

                        }
                        catch { }
                    }

                    try
                    {
                        IWebElement INodata = driver.FindElement(By.XPath("/html/body/div[2]/div[2]"));
                        if(INodata.Text.Contains("Nothing matching your search criteria was found"))
                        {
                            HttpContext.Current.Session["Nodata_SalineAR"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }

                    // Property_Details
                    string CountyName, OwnerDetails, MailingAddress, PropertyAddress, TotalAcres, SecTwp_Rng, Subdivision, LegalDescription, SchoolDistrict, HomesteadParcel, TaxStatus, Over;
                    // driver.FindElement(By.XPath("//*[@id='parcel_report']/table/tbody/tr[2]/td[1]/a")).SendKeys(Keys.Enter);
                    //Thread.Sleep(2000);
                    IWebElement Propertydetailtabel = driver.FindElement(By.XPath("//*[@id='Basic']/div/div[2]/table/tbody"));
                    parcel_number = gc.Between(Propertydetailtabel.Text, "Parcel Number:", "County Name:");
                    gc.CreatePdf(orderNumber, parcel_number, "PropertyDetailTabel", driver, "AR", "Saline");
                    CountyName = gc.Between(Propertydetailtabel.Text, "County Name:", "Mailing Address:");
                    string MOwnerDetails1 = driver.FindElement(By.XPath("//*[@id='Basic']/div/div[2]/table/tbody/tr[3]/td[2]")).Text;
                    MOwnerDetails1 = MOwnerDetails1.Replace("\r\n", "~");
                    string[] Mownersplit = MOwnerDetails1.Split('~');
                    OwnerDetails = Mownersplit[0];
                    MailingAddress = Mownersplit[1] + " " + Mownersplit[2];
                    string PropertyAddress1 = driver.FindElement(By.XPath("//*[@id='Basic']/div/div[2]/table/tbody/tr[4]/td[2]")).Text;
                    PropertyAddress1 = PropertyAddress1.Replace("\r\n", "~");
                    string[] propertyaddresssplit = PropertyAddress1.Split('~');
                    PropertyAddress = propertyaddresssplit[1] + " " + propertyaddresssplit[2];

                    TotalAcres = gc.Between(Propertydetailtabel.Text, "Total Acres:", "Timber Acres:");
                    SecTwp_Rng = gc.Between(Propertydetailtabel.Text, "Sec-Twp-Rng:", "Lot/Block:");
                    Subdivision = gc.Between(Propertydetailtabel.Text, "Subdivision:", "Legal Description:");
                    LegalDescription = gc.Between(Propertydetailtabel.Text, "Legal Description:", "School District:");
                    SchoolDistrict = driver.FindElement(By.XPath("//*[@id='Basic']/div/div[2]/table/tbody/tr[12]/td[2]")).Text; //gc.Between(Propertydetailtabel.Text, "School District:", "Improvement Districts:");
                    HomesteadParcel = gc.Between(Propertydetailtabel.Text, "Homestead Parcel?:", "Tax Status:");
                    TaxStatus = gc.Between(Propertydetailtabel.Text, "Tax Status:", "Over 65?:");
                    Over = GlobalClass.After(Propertydetailtabel.Text, "Over 65?:");
                    IWebElement YearBuilddata = driver.FindElement(By.XPath("/html/body/div[2]/div[3]/div/ul/li[7]/a"));

                    // driver.FindElement(By.XPath("/html/body/div[2]/div[3]/div/ul/li[7]/a")).SendKeys(Keys.Enter);

                    try
                    {
                        driver.FindElement(By.LinkText("Improvements")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcel_number, "YearBuild", driver, "AR", "Saline");
                        Yearbuild = driver.FindElement(By.XPath("//*[@id='Improvements']/div/div[2]/table/tbody/tr[10]/td[2]")).Text;
                    }
                    catch { }
                    string Property_Details = CountyName + "~" + OwnerDetails + "~" + MailingAddress + "~" + PropertyAddress + "~" + TotalAcres + "~" + SecTwp_Rng + "~" + Subdivision + "~" + LegalDescription + "~" + SchoolDistrict + "~" + HomesteadParcel + "~" + TaxStatus + "~" + Over + "~" + Yearbuild;
                    gc.insert_date(orderNumber, parcel_number, 574, Property_Details, 1, DateTime.Now);

                    //Assessment Details
                    string header = "";
                    try
                    {
                        driver.FindElement(By.LinkText("Valuation")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcel_number, "Assessment Details", driver, "AR", "Saline");
                        string Assessmentdetail = "";
                        //Land Appraised~Land Assessed~Improvements Appraised~Improvements Assessed~Total Value Appraised~Total Value Assessed~Taxable Value Appraised~Taxable Value Assessed~Millage Appraised~Millage Assessed~Estimated Taxes Appraised~Estimated Taxes Assessed~Homestead Credit Appraised~Homestead Credit Assessed~Estimated Taxes w Credit Appraised~Estimated Taxes w Credit Assessed~Assessment Year Appraised~Assessment Year Assessed
                        IWebElement AssessmentDetails_Table = driver.FindElement(By.XPath("//*[@id='Valuation']/div/div[2]/table/tbody"));
                        IList<IWebElement> Assessmentrow = AssessmentDetails_Table.FindElements(By.TagName("tr"));
                        IList<IWebElement> Assessmentid;
                        foreach (IWebElement assessment in Assessmentrow)
                        {
                            Assessmentid = assessment.FindElements(By.TagName("td"));
                            if (Assessmentid.Count != 0 && Assessmentid.Count > 2)
                            {
                                Assessmentdetail += Assessmentid[1].Text + "~" + Assessmentid[2].Text.Replace("(", "").Replace(")", "") + "~";

                            }
                            if (Assessmentid.Count != 0 && Assessmentid.Count < 3)
                            {
                                Assessmentdetail += " " + "~" + Assessmentid[1].Text + "~";

                            }
                            if (Assessmentid.Count != 0)
                            {
                                header += Assessmentid[0].Text.Replace(":", "") + " Appraised~" + Assessmentid[0].Text.Replace(":", "") + " Assessed~";
                            }
                        }
                        header = header.Remove(header.Length - 1, 1);
                        db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + header + "' where Id = '" + 576 + "'");
                        Assessmentdetail = Assessmentdetail.Remove(Assessmentdetail.Length - 1, 1);
                        gc.insert_date(orderNumber, parcel_number, 576, Assessmentdetail, 1, DateTime.Now);
                    }
                    catch { }

                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");
                    try
                    {
                        driver.FindElement(By.LinkText("Receipts")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcel_number, "Receipts", driver, "AR", "Saline");
                        String Receiptdetail = "";
                        IWebElement Receipt_table = driver.FindElement(By.XPath("//*[@id='Receipts']/div/div[2]/table/tbody"));
                        IList<IWebElement> Receipt_row = Receipt_table.FindElements(By.TagName("tr"));
                        IList<IWebElement> Receiptid;
                        foreach (IWebElement receipt in Receipt_row)
                        {
                            Receiptid = receipt.FindElements(By.TagName("td"));
                            if (Receiptid.Count != 0)
                            {
                                Receiptdetail = Receiptid[0].Text + "~" + Receiptid[1].Text + "~" + Receiptid[2].Text + "~" + Receiptid[3].Text + "~" + Receiptid[4].Text + "~" + Receiptid[5].Text + "~" + Receiptid[6].Text + "~" + Receiptid[7].Text;
                                gc.insert_date(orderNumber, parcel_number, 577, Receiptdetail, 1, DateTime.Now);
                            }

                        }
                    }
                    catch
                    {

                    }
                    //Taxes
                    try
                    {
                        driver.FindElement(By.LinkText("Taxes")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcel_number, "Taxes Detail", driver, "AR", "Saline");
                        int CurrentYear = DateTime.Now.Year;
                        List<string> Firsttear = new List<string>();
                        IWebElement Tax_tabel = driver.FindElement(By.XPath("//*[@id='Taxes']/div/div[2]/table/tbody"));
                        IList<IWebElement> Taxrow = Tax_tabel.FindElements(By.TagName("tr"));
                        IList<IWebElement> Taxid;
                        foreach (IWebElement tax in Taxrow)
                        {
                            Taxid = tax.FindElements(By.TagName("td"));
                            if (Taxid.Count != 0)
                            {
                                string Taxresult = Taxid[0].Text + "~" + Taxid[1].Text + "~" + Taxid[2].Text + "~" + Taxid[3].Text + "~" + Taxid[4].Text;
                                gc.insert_date(orderNumber, parcel_number, 578, Taxresult, 1, DateTime.Now);
                            }
                            if (Taxid.Count != 0 && Firsttear.Count < 1)
                            {
                                for (int i = 0; i < 2; i++)
                                {
                                    if (tax.Text.Contains(Convert.ToString(CurrentYear)) && Firsttear.Count < 1)
                                    {
                                        IWebElement Year = Taxid[0].FindElement(By.TagName("a"));
                                        string Taxyear = Year.GetAttribute("href");
                                        Firsttear.Add(Taxyear);
                                    }
                                    CurrentYear--;
                                }
                            }
                        }


                        //Property Information
                        foreach (string Firsttear1 in Firsttear)
                        {
                            driver.Navigate().GoToUrl(Firsttear1);
                            gc.CreatePdf(orderNumber, parcel_number, "Taxes Assessment", driver, "AR", "Saline");
                            Thread.Sleep(2000);
                            try
                            {
                                IWebElement PropertyInformation = driver.FindElement(By.XPath("/html/body/div[2]/table[1]/tbody"));
                                Tax_Status = gc.Between(PropertyInformation.Text, "Tax Status:", "Total Mandatory:");
                                TotalMandatory = gc.Between(PropertyInformation.Text, "Total Mandatory:", "Tax Paid:");
                                TaxPaid = gc.Between(PropertyInformation.Text, "Tax Paid:", "Balance:");
                                Balance = driver.FindElement(By.XPath("/html/body/div[2]/table[1]/tbody/tr[16]/td[2]/strong")).Text;
                            }
                            catch { }
                        }
                    }
                    catch
                    { }
                    //Current Tax Information

                    //try
                    //{
                    //    //Currenttaxinfo = driver.FindElement(By.XPath("/html/body/div[2]/table[4]/tbody"));
                    //    IList<IWebElement> tables1 = driver.FindElements(By.XPath("/html/body"));
                    //    int count1 = tables1.Count;
                    //    foreach (IWebElement tab in tables1)
                    //    {
                    //        if (tab.Text.Contains("Tax Information"))
                    //        {
                    //            IList<IWebElement> ITaxRealRowQ1 = tab.FindElements(By.TagName("tr"));

                    //            foreach (IWebElement ItaxReal1 in ITaxRealRowQ1)
                    //            {
                    //                IList<IWebElement> currenttaxid;
                    //                currenttaxid = ItaxReal1.FindElements(By.TagName("td"));
                    //                if (ItaxReal1.Text.Contains("Tax Information"))
                    //                {

                    //                    if (currenttaxid.Count == 8)
                    //                    {
                    //                        if (currenttaxid.Count != 0 && currenttaxid.Count > 4 && !ItaxReal1.Text.Contains("Tax Type"))
                    //                        {
                    //                            string currenttaxresult = currenttaxid[0].Text + "~" + currenttaxid[1].Text + "~" + currenttaxid[2].Text + "~" + currenttaxid[3].Text + "~" +
                    //                                currenttaxid[4].Text + "~" + currenttaxid[5].Text + "~" + currenttaxid[6].Text + "~" + currenttaxid[7].Text;
                    //                            gc.insert_date(orderNumber, parcel_number, 585, currenttaxresult, 1, DateTime.Now);
                    //                        }
                    //                        if (currenttaxid.Count != 0 && currenttaxid.Count < 5)
                    //                        {
                    //                            string currenttaxresult = currenttaxid[0].Text + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + currenttaxid[1].Text + "~" + currenttaxid[2].Text + "~" +
                    //                                currenttaxid[3].Text;
                    //                            gc.insert_date(orderNumber, parcel_number, 585, currenttaxresult, 1, DateTime.Now);
                    //                        }
                    //                    }

                    //                }
                    //            }

                    //        }
                    //    }
                    //}
                    //catch
                    //{
                    //}

                    try
                    {
                        Currenttaxinfo = driver.FindElement(By.XPath("/html/body/div[2]/table[4]/tbody"));
                        IList<IWebElement> Currenttaxinforow = Currenttaxinfo.FindElements(By.TagName("tr"));
                        IList<IWebElement> currenttaxid;
                        foreach (IWebElement current in Currenttaxinforow)
                        {
                            currenttaxid = current.FindElements(By.TagName("td"));
                            if (currenttaxid.Count != 0 && !current.Text.Contains("Tax Type"))
                            {
                                if (currenttaxid.Count > 4)
                                {
                                    string currenttaxresult = currenttaxid[0].Text + "~" + currenttaxid[1].Text + "~" + currenttaxid[2].Text + "~" + currenttaxid[3].Text + "~" +
                                        currenttaxid[4].Text + "~" + currenttaxid[5].Text + "~" + currenttaxid[6].Text + "~" + currenttaxid[7].Text;
                                    gc.insert_date(orderNumber, parcel_number, 585, currenttaxresult, 1, DateTime.Now);
                                }
                                if (currenttaxid.Count < 5)
                                {
                                    string currenttaxresult = currenttaxid[0].Text + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + currenttaxid[1].Text + "~" + currenttaxid[2].Text + "~" +
                                        currenttaxid[3].Text;
                                    gc.insert_date(orderNumber, parcel_number, 585, currenttaxresult, 1, DateTime.Now);
                                }
                            }
                        }
                    }
                    catch
                    {
                        try
                        {
                            Currenttaxinfo = driver.FindElement(By.XPath("/html/body/div[2]/table[3]/tbody"));
                            IList<IWebElement> Currenttaxinforow = Currenttaxinfo.FindElements(By.TagName("tr"));
                            IList<IWebElement> currenttaxid;
                            foreach (IWebElement current in Currenttaxinforow)
                            {
                                currenttaxid = current.FindElements(By.TagName("td"));
                                if (currenttaxid.Count != 0 && !current.Text.Contains("Tax Type") && !current.Text.Contains("Receipt #"))
                                {
                                    if (currenttaxid.Count > 4)
                                    {
                                        string currenttaxresult = currenttaxid[0].Text + "~" + currenttaxid[1].Text + "~" + currenttaxid[2].Text + "~" + currenttaxid[3].Text + "~" +
                                            currenttaxid[4].Text + "~" + currenttaxid[5].Text + "~" + currenttaxid[6].Text + "~" + currenttaxid[7].Text;
                                        gc.insert_date(orderNumber, parcel_number, 585, currenttaxresult, 1, DateTime.Now);
                                    }
                                    if (currenttaxid.Count < 5)
                                    {
                                        string currenttaxresult = currenttaxid[0].Text + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + currenttaxid[1].Text + "~" + currenttaxid[2].Text + "~" +
                                            currenttaxid[3].Text;
                                        gc.insert_date(orderNumber, parcel_number, 585, currenttaxresult, 1, DateTime.Now);
                                    }
                                }
                            }
                        }
                        catch { }


                        string TaxingAuthority = "";
                        try
                        {
                            driver.Navigate().GoToUrl("https://www.salinecollector.ar.gov/contact.html");
                            string Taxauthaddress = driver.FindElement(By.XPath("//*[@id='content']/div/div/div[1]/div")).Text;
                            TaxingAuthority = GlobalClass.Before(Taxauthaddress, "101").Replace("\r\n", "");
                            string Taxnumber = gc.Between(Taxauthaddress, "pm", "E-mail:").Replace("\r\n", "");
                            string TaxPhone = " Telephone: " + GlobalClass.Before(Taxnumber, "Telephone:") + " FAX: " + gc.Between(Taxauthaddress, "Telephone:", "FAX:");
                            TaxingAuthority = TaxingAuthority + TaxPhone;
                            gc.CreatePdf(orderNumber, parcel_number, "Tax Authority", driver, "AR", "Saline");
                        }
                        catch { }

                        gc.insert_date(orderNumber, parcel_number, 583, Tax_Status + "~" + TotalMandatory + "~" + TaxPaid + "~" + Balance + "~" + TaxingAuthority.Replace("\r\n", " "), 1, DateTime.Now);

                    }

                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "AR", "Saline", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    gc.mergpdf(orderNumber, "AR", "Saline");
                    return "Data Inserted Successfully....";
                }

                catch (Exception ex)
                {
                    driver.Quit();
                    throw ex;
                }
            }
        }
        public void Multiple(IWebDriver driver, string orderNumber, string Addressmax)
        {
            try
            {
                //string Addressmax = driver.FindElement(By.XPath("/html/body/div[2]/div[2]/div[2]/table/tbody/tr[1]/td[2]")).Text.Trim();

                //if (Convert.ToInt32(Addressmax) == 1 && Convert.ToInt32(Addressmax) != 0)
                //{
                //    driver.FindElement(By.XPath("//*[@id='parcel_report']/table/tbody/tr[2]/td[1]/a")).SendKeys(Keys.Enter);
                //    Thread.Sleep(2000);
                //}
                gc.CreatePdf_WOP(orderNumber, "MultyParcel", driver, "AR", "Saline");
                if (Convert.ToInt32(Addressmax) > 1 && Convert.ToInt32(Addressmax) < 25 && Convert.ToInt32(Addressmax) != 0)
                {
                    IWebElement MultipleTabel = driver.FindElement(By.XPath("//*[@id='parcel_report']/table/tbody"));
                    IList<IWebElement> MultipleRow = MultipleTabel.FindElements(By.TagName("tr"));
                    IList<IWebElement> Multipleid;
                    foreach (IWebElement multiple in MultipleRow)
                    {
                        Multipleid = multiple.FindElements(By.TagName("td"));
                        if (Multipleid.Count != 0 && !multiple.Text.Contains("Parcel #"))
                        {
                            string parcelNumber = Multipleid[0].Text;
                            string Multipledata = Multipleid[1].Text + "~" + Multipleid[2].Text + "~" + Multipleid[3].Text + "~" + Multipleid[4].Text + "~" + Multipleid[5].Text;
                            gc.insert_date(orderNumber, parcelNumber, 588, Multipledata, 1, DateTime.Now);
                        }
                    }
                    HttpContext.Current.Session["multiparcel_Saline"] = "Yes";
                    gc.CreatePdf_WOP(orderNumber, "MultyAddressSearch", driver, "AR", "Saline");


                }
                if (Convert.ToInt32(Addressmax) > 25)
                {
                    HttpContext.Current.Session["multiParcel_Saline_Multicount"] = "Maximum";
                    gc.CreatePdf_WOP(orderNumber, "MultyAddressSearch", driver, "AR", "Saline");
                }
            }
            catch { }

        }
    }
}