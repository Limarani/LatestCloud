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
    public class Webdriver_DelawareOH
    {
        IWebDriver driver;
        DBconnection db = new DBconnection();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        GlobalClass gc = new GlobalClass();
        public string DelawareOH(string streetno, string direction, string streetname, string city, string streettype, string unitnumber, string ownernm, string parcelNumber, string searchType, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
            string Parcel_number, Tax_Authority = "", Year = "", Total_Due = "";
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            using (driver = new PhantomJSDriver()) 
            {
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");
                    driver.Navigate().GoToUrl("http://delaware-auditor-ohio.manatron.com/");
                    Thread.Sleep(10000);
                    //IWebElement Logintable = driver.FindElement(By.XPath("//*[@id='lxT483']/div/table/tbody"));
                    //Thread.Sleep(2000);
                    driver.FindElement(By.LinkText("Contact Us")).Click();
                    Thread.Sleep(10000);
                    Tax_Authority = driver.FindElement(By.XPath("//*[@id='dnn_ctr852_HtmlModule_HtmlModule_lblContent']/div[2]")).Text;

                    gc.CreatePdf_WOP(orderNumber, "Tax_Authority", driver, "OH", "Delaware");
                    if (searchType == "titleflex")
                    {
                        string address = streetno + " " + streetname + " " + streettype + " " + unitnumber;
                        gc.TitleFlexSearch(orderNumber, "", ownernm, address.Trim(), "OH", "Delaware");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_DelawareOH"] = "Zero";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }

                    if (searchType == "address")
                    {
                        IWebElement Addresssearch = driver.FindElement(By.XPath("//*[@id='lxT483']/div/table/tbody/tr[3]/td/a"));
                        string addressrow = Addresssearch.GetAttribute("href");
                        Addresssearch.Click();
                        Thread.Sleep(10000);
                        gc.CreatePdf_WOP(orderNumber, "Adddress click", driver, "OH", "Delaware");
                        driver.FindElement(By.Id("HouseNoLow")).SendKeys(streetno);
                        driver.FindElement(By.Id("Street")).SendKeys(streetname);
                        driver.FindElement(By.XPath("//*[@id='tabs-1']/button")).SendKeys(Keys.Enter);
                        Thread.Sleep(10000);
                        try
                        {
                            IWebElement ownernamemulti = driver.FindElement(By.XPath("//*[@id='tabs-1']/table[2]/tbody"));
                            IList<IWebElement> ownernamemultirow = ownernamemulti.FindElements(By.TagName("table"));
                            IList<IWebElement> Ownernamemultiid;
                            foreach (IWebElement ownnermul in ownernamemultirow)
                            {
                                Ownernamemultiid = ownnermul.FindElements(By.TagName("td"));

                                if (Ownernamemultiid.Count != 0 && !ownnermul.Text.Contains("Property"))
                                {
                                    string ownernameresultad = Ownernamemultiid[1].Text + "~" + Ownernamemultiid[2].Text;
                                    string parcelnumberad = Ownernamemultiid[0].Text;
                                    gc.insert_date(orderNumber, parcelnumberad, 878, ownernameresultad, 1, DateTime.Now);
                                }

                            }
                            if (ownernamemultirow.Count != 0 && ownernamemultirow.Count < 26)
                            {
                                HttpContext.Current.Session["multiParcel_Delaware"] = "Yes";
                                gc.CreatePdf_WOP(orderNumber, "Multiple Parcel", driver, "OH", "Delaware");
                                driver.Quit();
                                return "MultiParcel";
                            }
                            if (ownernamemultirow.Count != 0 && ownernamemultirow.Count > 25)
                            {
                                HttpContext.Current.Session["multiParcel_Delaware_Multicount"] = "Maximum";
                                gc.CreatePdf_WOP(orderNumber, "MultyAddressSearch", driver, "OH", "Delaware");
                                driver.Quit();
                                return "Maximum";
                            }

                        }
                        catch { }
                    }
                    if (searchType == "parcel")
                    {
                        driver.FindElement(By.LinkText("Parcel Number Search")).Click();
                        Thread.Sleep(10000);
                        driver.FindElement(By.Id("owner")).SendKeys(parcelNumber);
                        gc.CreatePdf_WOP(orderNumber, "Parcel", driver, "OH", "Delaware");
                        driver.FindElement(By.XPath("//*[@id='tabs-1']/button")).Click();
                        Thread.Sleep(10000);
                    }
                    if (searchType == "ownername")
                    {
                        driver.FindElement(By.LinkText("Owner Search")).Click();
                        Thread.Sleep(8000);
                        driver.FindElement(By.Id("owner")).SendKeys(ownernm);
                        gc.CreatePdf_WOP(orderNumber, "Ownername", driver, "OH", "Delaware");
                        driver.FindElement(By.XPath("//*[@id='tabs-1']/button")).Click();
                        Thread.Sleep(10000);
                        try
                        {
                            IWebElement ownernamemulti = driver.FindElement(By.XPath("//*[@id='tabs-1']/table[2]/tbody"));
                            IList<IWebElement> ownernamemultirow = ownernamemulti.FindElements(By.TagName("table"));
                            IList<IWebElement> Ownernamemultiid;
                            foreach (IWebElement ownnermul in ownernamemultirow)
                            {
                                Ownernamemultiid = ownnermul.FindElements(By.TagName("td"));

                                if (Ownernamemultiid.Count != 0)
                                {
                                    string ownernameresult = Ownernamemultiid[0].Text + "~" + Ownernamemultiid[2].Text;
                                    string parcelnumberown = Ownernamemultiid[1].Text;
                                    gc.insert_date(orderNumber, parcelnumberown, 878, ownernameresult, 1, DateTime.Now);
                                }
                            }
                            if (ownernamemultirow.Count != 0 && ownernamemultirow.Count < 26)
                            {
                                HttpContext.Current.Session["multiParcel_Delaware"] = "Yes";
                                gc.CreatePdf_WOP(orderNumber, "Multiple Parcel", driver, "OH", "Delaware");
                                driver.Quit();
                                return "MultiParcel";
                            }
                            if (ownernamemultirow.Count != 0 && ownernamemultirow.Count > 25)
                            {
                                HttpContext.Current.Session["multiParcel_Delaware_Multicount"] = "Maximum";
                                gc.CreatePdf_WOP(orderNumber, "MultyAddressSearch", driver, "OH", "Delaware");
                                driver.Quit();
                                return "Maximum";
                            }

                        }
                        catch { }
                    }

                    try
                    {
                        IWebElement Inodata = driver.FindElement(By.XPath("//*[@id='tabs-1']/div"));
                        if(Inodata.Text.Contains("No Results Found"))
                        {
                            HttpContext.Current.Session["Nodata_DelawareOH"] = "Zero";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }
                    gc.CreatePdf_WOP(orderNumber, "Searchafter", driver, "OH", "Delaware");
                    IWebElement propertydetailtable = driver.FindElement(By.XPath("//*[@id='lxT581']/div/table/tbody"));
                    Parcel_number = gc.Between(propertydetailtable.Text, "Parcel Number", "Owner Name");
                    string PropertyAddress = gc.Between(propertydetailtable.Text, "Property Address:", "Tax Payer Address:");
                    string OwnerName = gc.Between(propertydetailtable.Text, "Owner Name", "Owner Address");
                    string MailingAddress = gc.Between(propertydetailtable.Text, "Owner Address", "Tax District");
                    string TaxDistrict = gc.Between(propertydetailtable.Text, "Tax District", "School District");
                    string schoolDistrict = gc.Between(propertydetailtable.Text, "School District", "Neighborhood");
                    string Neighborhood = gc.Between(propertydetailtable.Text, "Neighborhood", "Use Code");
                    string UseCode = gc.Between(propertydetailtable.Text, "Use Code", "Acres");
                    string Acres = gc.Between(propertydetailtable.Text, "Acres", "Description");
                    string Description = gc.Between(propertydetailtable.Text, "Description", "Property Address:");
                    try
                    {
                        IWebElement yeartable = driver.FindElement(By.XPath("//*[@id='lxT581']/div/table/tbody/tr[4]/td/table/tbody/tr/td/table/tbody"));
                        Year = gc.Between(yeartable.Text, "Year Built", "Finished Basement");
                    }
                    catch { }
                    string propertyresult = PropertyAddress + "~" + OwnerName + "~" + MailingAddress + "~" + TaxDistrict + "~" + schoolDistrict + "~" + Neighborhood + "~" + UseCode + "~" + Acres + "~" + Description + "~" + Year;
                    gc.insert_date(orderNumber, Parcel_number, 821, propertyresult, 1, DateTime.Now);
                    gc.CreatePdf(orderNumber, Parcel_number, "Peoperty Detail", driver, "OH", "Delaware");
                    //assessment
                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");
                    string LandValue = gc.Between(propertydetailtable.Text, "Mkt Land Value", "Homestead/Disability");
                    string CAUV = gc.Between(propertydetailtable.Text, "CAUV", "# Parcels");
                    string MktImprValue = gc.Between(propertydetailtable.Text, "Mkt Impr Value", "Deed Type");
                    string Total = gc.Between(propertydetailtable.Text, "Total", "Amount");
                    string Boardof_Revision = gc.Between(propertydetailtable.Text, "Board of Revision", "Mkt Land Value");
                    string Homestead = gc.Between(propertydetailtable.Text, "Homestead/Disability", "CAUV");
                    string OwnerOccCredit = gc.Between(propertydetailtable.Text, "Owner Occ Credit", "Mkt Impr Value");
                    string DividedProperty = gc.Between(propertydetailtable.Text, "Divided Property", "Total");
                    string NewConstruction = gc.Between(propertydetailtable.Text, "New Construction", "Current Tax");
                    string Foreclosure = gc.Between(propertydetailtable.Text, "Foreclosure", "Tax Due");
                    string OtherAssessments = gc.Between(propertydetailtable.Text, "Other Assessments", "Paid To Date");
                    string Front_Ft = gc.Between(propertydetailtable.Text, "Front Ft.", "Current Balance Due");
                    IWebElement Taxclick = driver.FindElement(By.XPath("//*[@id='radioset']/label[2]/span/a"));
                    string taxclickrow = Taxclick.GetAttribute("href");
                    Taxclick.Click();
                    Thread.Sleep(10000);
                    // driver.FindElement(By.LinkText("Tax")).Click();
                    gc.CreatePdf(orderNumber, Parcel_number, "Tax Detail", driver, "OH", "Delaware");
                    IWebElement taxassment = driver.FindElement(By.XPath("//*[@id='lxT613']/div/table/tbody/tr/td/table/tbody/tr[1]/td/table[2]/tbody/tr[4]/td/table/tbody/tr/td/table/tbody"));
                    string AssessedlandValue = gc.Between(taxassment.Text, "Land", "Effective Rate");
                    string AssessedValue = gc.Between(taxassment.Text, "Improvements", "CDQYear");
                    string TotalAssessed = gc.Between(taxassment.Text, "Total", "Tax Lien Flag");
                    string Assessmentresult = LandValue + "~" + CAUV + "~" + MktImprValue + "~" + Total + "~" + Boardof_Revision + "~" + Homestead + "~" + OwnerOccCredit + "~" + DividedProperty + "~" + NewConstruction + "~" + Foreclosure + "~" + OtherAssessments + "~" + Front_Ft + "~" + AssessedlandValue + "~" + AssessedValue + "~" + TotalAssessed;
                    gc.insert_date(orderNumber, Parcel_number, 823, Assessmentresult, 1, DateTime.Now);
                    //tax
                    TaxTime = DateTime.Now.ToString("HH:mm:ss");
                    string FullRate = gc.Between(taxassment.Text, "Full Rate", "Assessed Value");
                    string ReductionFactor = gc.Between(taxassment.Text, "Reduction Factor", "Land");
                    string EffectiveRate = gc.Between(taxassment.Text, "Effective Rate", "Improvements");
                    string CDQYear = gc.Between(taxassment.Text, "CDQYear", "Total");
                    string TaxLienFlag = gc.Between(taxassment.Text, "Tax Lien Flag", "Tax Due");
                    string taxdue = driver.FindElement(By.XPath("//*[@id='lxT613']/div/table/tbody/tr/td/table/tbody/tr[1]/td/table[2]/tbody/tr[4]/td/table/tbody/tr/td/table/tbody/tr[6]/td[3]")).Text;
                    string PaidtoDate = driver.FindElement(By.XPath("//*[@id='lxT613']/div/table/tbody/tr/td/table/tbody/tr[1]/td/table[2]/tbody/tr[4]/td/table/tbody/tr/td/table/tbody/tr[6]/td[4]")).Text;
                    //current tax year
                    string currenttax = driver.FindElement(By.XPath("//*[@id='lxT613']/div/table/tbody/tr/td/table/tbody/tr[2]/td/table/tbody/tr[1]/td/font")).Text;

                    IWebElement taxinformationtable = driver.FindElement(By.XPath("//*[@id='lxT613']/div/table/tbody/tr/td/table/tbody/tr[2]/td/table/tbody/tr[2]/td/table/tbody"));
                    IList<IWebElement> taxinformationrow = taxinformationtable.FindElements(By.TagName("tr"));
                    IList<IWebElement> taxid;
                    foreach (IWebElement taxinformation in taxinformationrow)
                    {
                        taxid = taxinformation.FindElements(By.TagName("td"));

                        if (taxid.Count != 0 && !taxinformation.Text.Contains("Prior") && !taxinformation.Text.Contains("Current") && !taxinformation.Text.Contains("Chg"))
                        {
                            string taxresulttable = taxid[0].Text + "~" + currenttax + "~" + taxid[1].Text + "~" + taxid[2].Text + "~" + taxid[3].Text + "~" + taxid[4].Text + "~" + taxid[5].Text + "~" + taxid[6].Text;
                            gc.insert_date(orderNumber, Parcel_number, 829, taxresulttable, 1, DateTime.Now);
                        }
                    }
                    string assessmentspl = "";
                    try
                    {
                        IWebElement taxinformationtable1 = driver.FindElement(By.XPath("//*[@id='lxT615']/div/table/tbody/tr/td/table/tbody/tr[2]/td/table/tbody/tr/td/table/tbody"));
                        IList<IWebElement> taxinformationrow1 = taxinformationtable1.FindElements(By.TagName("tr"));
                        IList<IWebElement> taxid1;
                        foreach (IWebElement taxinformation1 in taxinformationrow1)
                        {
                            taxid1 = taxinformation1.FindElements(By.TagName("td"));
                            if (taxid1.Count != 0 && taxid1.Count == 1)
                            {
                                assessmentspl = taxid1[0].Text;
                            }

                            if (taxid1.Count != 0 && taxid1.Count != 1 && !taxinformation1.Text.Contains("Prior") && !taxinformation1.Text.Contains("Current") && !taxinformation1.Text.Contains("Chg"))
                            {
                                string taxresulttable1 = taxid1[0].Text + "~" + assessmentspl + "~" + taxid1[1].Text + "~" + taxid1[2].Text + "~" + taxid1[3].Text + "~" + taxid1[4].Text + "~" + taxid1[5].Text + "~" + taxid1[6].Text;
                                gc.insert_date(orderNumber, Parcel_number, 829, taxresulttable1, 1, DateTime.Now);
                            }
                        }
                    }
                    catch { }
                    IWebElement paymenttable = driver.FindElement(By.XPath("//*[@id='lxT616']/div/table/tbody/tr[3]/td/table/tbody"));
                    IList<IWebElement> paymentrow = paymenttable.FindElements(By.TagName("tr"));
                    IList<IWebElement> paymentid;
                    foreach (IWebElement payment in paymentrow)
                    {
                        paymentid = payment.FindElements(By.TagName("td"));
                        if (paymentid.Count != 0)
                        {
                            string paymentresult = paymentid[0].Text + "~" + paymentid[1].Text + "~" + paymentid[2].Text + "~" + paymentid[3].Text + "~" + paymentid[4].Text;
                            gc.insert_date(orderNumber, Parcel_number, 836, paymentresult, 1, DateTime.Now);
                        }
                    }
                    //Tax Distribution
                    driver.FindElement(By.LinkText("Tax Distribution")).Click();
                    Thread.Sleep(10000);
                    gc.CreatePdf(orderNumber, Parcel_number, "Tax Distibution", driver, "OH", "Delaware");
                    IWebElement taxannualtable = driver.FindElement(By.XPath("//*[@id='r']"));
                    string Tax_District = gc.Between(taxannualtable.Text, "Tax District: ", "Annual Tax: ");
                    string Annualtax = gc.Between(taxannualtable.Text, "Annual Tax: ", "Eff. Rate");
                    string taxresult = FullRate + "~" + ReductionFactor + "~" + EffectiveRate + "~" + CDQYear + "~" + TaxLienFlag + "~" + Tax_District + "~" + Annualtax + "~" + taxdue + "~" + PaidtoDate + "~" + Tax_Authority;
                    gc.insert_date(orderNumber, Parcel_number, 828, taxresult, 1, DateTime.Now);
                    IWebElement taxdistibutiontable = driver.FindElement(By.XPath("//*[@id='box-table-a']/tbody"));
                    IList<IWebElement> taxdistibutionrow = taxdistibutiontable.FindElements(By.TagName("tr"));
                    IList<IWebElement> Taxdistibutionid;
                    foreach (IWebElement Taxdistibution in taxdistibutionrow)
                    {
                        Taxdistibutionid = Taxdistibution.FindElements(By.TagName("td"));
                        if (Taxdistibutionid.Count != 0 && !Taxdistibution.Text.Contains("Eff. Rate") && Taxdistibution.Text != "")
                        {
                            string Taxdistibutionresult = Taxdistibutionid[0].Text + "~" + Taxdistibutionid[1].Text + "~" + Taxdistibutionid[2].Text;
                            gc.insert_date(orderNumber, Parcel_number, 840, Taxdistibutionresult, 1, DateTime.Now);
                        }

                    }
                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "OH", "Delaware", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);
                    driver.Quit();
                    gc.mergpdf(orderNumber, "OH", "Delaware");
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