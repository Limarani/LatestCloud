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
using System;
using System.Collections.Generic;
using System.Linq;

namespace ScrapMaricopa.Scrapsource
{
    public class Webdiver_CASantaClara
    {
        string Tax_Year_Payment = " ", APN_Suffix_Payment = " ", Installment_Payment = " ", Tax_Amount_Payment = " ", Additional_Charges_Payment = " ", Paid_Amount_Payment = " ", Paid_Date_Payment = " ", Taxpayment_Details = "";
        string parcelno = " ", Property_Address = " ", Property_Details = " ";
        string lan = " ", Imp = " ", TAss = " ", Homeowner = " ", other = " ", TExe = " ", Net_Total = " ", MultiParcelData = "";
        string Land_Value = " ", Assesssment_Details = " ", Impovement = " ", Total_Assessed = " ", Home_Owner = " ", Other = " ", Total_Expections = " ", Net_Assed_Value = " ", Tax_Default_Date = " ";
        string Installment = "", tax_type = " ", taxType_Supplemnt = "", taxType_Supplemnt1 = "", InstallmentSupplemnt = "", InstallmentSupplemnt1 = "", property_Address = "", TRA = "", tax_type_Supp = "";
        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());

        public string FTP_CASantaClara(string address, string assessment_id, string parcelNumber, string searchType, string orderNumber, string directParcel, string ownername)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;

            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";

            //var driverService = PhantomJSDriverService.CreateDefaultService();
            //driverService.HideCommandPromptWindow = true;
            // driver = new PhantomJSDriver();
            //driver = new ChromeDriver();

            var option = new ChromeOptions();
            option.AddArgument("No-Sandbox");
            using (driver = new ChromeDriver(option))
            {

                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");

                    if (searchType == "titleflex")
                    {
                        gc.TitleFlexSearch(orderNumber, parcelNumber, ownername, "", "CA", "Santa Clara");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_CASantaClara"] = "Zero";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }

                    if (searchType == "address")
                    {
                        try
                        {
                            driver.Navigate().GoToUrl("https://www.sccassessor.org/index.php/online-services/property-search/real-property");
                            Thread.Sleep(2000);

                            IWebElement iframeElementAdd = driver.FindElement(By.XPath("//*[@id='blockrandom']"));
                            driver.SwitchTo().Frame(iframeElementAdd);
                            Thread.Sleep(2000);

                            driver.FindElement(By.XPath("//*[@id='searchinput']")).SendKeys(address);
                            gc.CreatePdf_WOP(orderNumber, "Address search", driver, "CA", "Santa Clara");
                            driver.FindElement(By.Id("btnAddress")).SendKeys(Keys.Enter);
                            Thread.Sleep(4000);

                            //MultiParcel
                            try
                            {
                                IWebElement MultiTable = driver.FindElement(By.XPath("//*[@id='asrGrid']/div[2]/table/tbody"));
                                IList<IWebElement> MultiTR = MultiTable.FindElements(By.TagName("tr"));
                                IList<IWebElement> MultiTD;
                                gc.CreatePdf_WOP(orderNumber, "Multi Address search", driver, "CA", "Santa Clara");
                                int maxCheck = 0;

                                foreach (IWebElement multi in MultiTR)
                                {
                                    if (maxCheck <= 25)
                                    {
                                        MultiTD = multi.FindElements(By.TagName("td"));
                                        if (MultiTD.Count != 0)
                                        {
                                            parcelNumber = MultiTD[0].Text;
                                            property_Address = MultiTD[1].Text;
                                            MultiParcelData = property_Address;
                                            gc.insert_date(orderNumber, parcelNumber, 547, MultiParcelData, 1, DateTime.Now);
                                        }
                                        maxCheck++;
                                    }
                                }

                                if (MultiTR.Count > 25)
                                {
                                    HttpContext.Current.Session["multiParcel_SantaClara_Multicount"] = "Maximum";
                                }
                                else
                                {
                                    HttpContext.Current.Session["multiparcel_SantaClara"] = "Yes";
                                }
                                driver.Quit();

                                return "MultiParcel";
                            }
                            catch
                            { }
                        }
                        catch
                        { }
                        try
                        {
                            //No Data Found
                            string nodata = driver.FindElement(By.XPath("//*[@id='ContentMasterPage_lblError']/div")).Text;
                            if (nodata.Contains("No records meet your search criteria:"))
                            {
                                HttpContext.Current.Session["Nodata_CASantaClara"] = "Zero";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                        try
                        {
                            //No Data Found
                            string nodata = driver.FindElement(By.XPath("//*[@id='ui-id-1']")).Text;
                            if (nodata.Contains("No Such Address"))
                            {
                                HttpContext.Current.Session["Nodata_CASantaClara"] = "Zero";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }

                    if (searchType == "parcel")
                    {
                        try
                        {
                            driver.Navigate().GoToUrl("https://www.sccassessor.org/index.php/online-services/property-search/real-property");
                            Thread.Sleep(2000);

                            IWebElement iframeElement1 = driver.FindElement(By.XPath("//*[@id='blockrandom']"));
                            driver.SwitchTo().Frame(iframeElement1);
                            Thread.Sleep(2000);

                            driver.FindElement(By.XPath("//*[@id='myTab-accordion']/div[3]/div[1]/h4/a")).Click();
                            Thread.Sleep(2000);

                            driver.FindElement(By.Id("apninput")).SendKeys(parcelNumber);
                            gc.CreatePdf_WOP(parcelNumber, "Parcel search", driver, "CA", "Santa Clara");
                            driver.FindElement(By.Id("btnAPN")).SendKeys(Keys.Enter);
                            Thread.Sleep(4000);
                            try
                            {
                                //No Data Found
                                string nodata = driver.FindElement(By.XPath("//*[@id='ContentMasterPage_lblError']/div")).Text;
                                if (nodata.Contains("No records meet your search criteria:"))
                                {
                                    HttpContext.Current.Session["Nodata_CASantaClara"] = "Zero";
                                    driver.Quit();
                                    return "No Data Found";
                                }
                            }
                            catch { }
                        }
                        catch
                        { }
                    }

                    //Property Details        
                    parcelno = driver.FindElement(By.XPath("//*[@id='divFrame']/div[1]/div/div[1]")).Text;
                    parcelno = WebDriverTest.After(parcelno, ":").Replace("\r\n", " ").Trim();
                    Property_Address = driver.FindElement(By.XPath("//*[@id='oneAddress']/div/div[1]/div")).Text;


                    //Assessment Deatils
                    Tax_Default_Date = driver.FindElement(By.XPath("//*[@id='tab-1-collapse']/div/div/div[1]/div/div/div/table/tbody/tr[2]/td[2]")).Text;
                    Tax_Default_Date = WebDriverTest.After(Tax_Default_Date, ": ");

                    IWebElement AssementTB = driver.FindElement(By.XPath("//*[@id='tab-1-collapse']/div/div/div[2]/div/div[2]/div/table/tbody"));
                    IList<IWebElement> AssementTR = AssementTB.FindElements(By.TagName("tr"));
                    IList<IWebElement> AssementTD;
                    foreach (IWebElement Assement in AssementTR)
                    {
                        AssementTD = Assement.FindElements(By.TagName("td"));
                        if (AssementTD.Count != 0 || !Assement.Text.Contains(" "))
                        {
                            lan = AssementTD[0].Text;
                            if (lan.Contains("Land:"))
                            {
                                Land_Value = AssementTD[1].Text;
                            }
                            Imp = AssementTD[0].Text;
                            if (Imp.Contains("Improvements:"))
                            {
                                Impovement = AssementTD[1].Text;
                            }
                            TAss = AssementTD[0].Text;
                            if (TAss.Contains("Total:"))
                            {
                                Total_Assessed = AssementTD[1].Text;
                            }
                        }
                    }

                    IWebElement AssementTB1 = driver.FindElement(By.XPath("//*[@id='tab-1-collapse']/div/div/div[2]/div/div[4]/div/table/tbody"));
                    IList<IWebElement> AssementTR1 = AssementTB1.FindElements(By.TagName("tr"));
                    IList<IWebElement> AssementTD1;
                    foreach (IWebElement Assement1 in AssementTR1)
                    {
                        AssementTD1 = Assement1.FindElements(By.TagName("td"));
                        if (AssementTD1.Count != 0 || !Assement1.Text.Contains(" "))
                        {
                            Homeowner = AssementTD1[0].Text;
                            if (Homeowner.Contains("Homeowner:"))
                            {
                                Home_Owner = AssementTD1[1].Text;
                            }
                            other = AssementTD1[0].Text;
                            if (other.Contains("Other:"))
                            {
                                Other = AssementTD1[1].Text;
                            }
                            TExe = AssementTD1[0].Text;
                            if (TExe.Contains("Total:"))
                            {
                                Total_Expections = AssementTD1[1].Text;
                            }
                        }
                    }

                    IWebElement AssementTB2 = driver.FindElement(By.XPath("//*[@id='tab-1-collapse']/div/div/div[2]/div/div[5]/div/table/tbody"));
                    IList<IWebElement> AssementTR2 = AssementTB2.FindElements(By.TagName("tr"));
                    IList<IWebElement> AssementTD2;
                    foreach (IWebElement Assement2 in AssementTR2)
                    {
                        AssementTD2 = Assement2.FindElements(By.TagName("td"));
                        if (AssementTD2.Count != 0 || !Assement2.Text.Contains(" "))
                        {
                            Net_Total = AssementTD2[0].Text;
                            if (Net_Total.Contains("Total:"))
                            {
                                Net_Assed_Value = AssementTD2[1].Text;
                            }
                        }
                    }

                    Assesssment_Details = Land_Value + "~" + Impovement + "~" + Total_Assessed + "~" + Home_Owner + "~" + Other + "~" + Total_Expections + "~" + Net_Assed_Value + "~" + Tax_Default_Date;
                    gc.CreatePdf(orderNumber, parcelno, "Assessment Details", driver, "CA", "Santa Clara");
                    ByVisibleElement(driver.FindElement(By.XPath("//*[@id='tab-1-collapse']/div/div/div[2]/div/div[2]/div/table/tbody/tr[4]/td[1]")));
                    gc.CreatePdf(orderNumber, parcelno, "Assessment Details1", driver, "CA", "Santa Clara");
                    ByVisibleElement(driver.FindElement(By.XPath("//*[@id='tab-1-collapse']/div/div/div[2]/div/div[4]/div/table/thead/tr/td")));
                    gc.CreatePdf(orderNumber, parcelno, "Assessment Details2", driver, "CA", "Santa Clara");
                    gc.insert_date(orderNumber, parcelno, 542, Assesssment_Details, 1, DateTime.Now);

                    driver.FindElement(By.XPath("//*[@id='myTab-accordion']/div[5]/div[1]/h4/a")).Click();
                    Thread.Sleep(2000);

                    TRA = driver.FindElement(By.XPath("//*[@id='tab-5-collapse']/div/div/div/div/div[1]/div[1]/div/div/div[1]")).Text;
                    TRA = gc.Between(TRA, "TAX RATE AREA INFORMATION  ", "(Tax Rate Information as").Trim();

                    Property_Details = Property_Address + "~" + TRA;
                    gc.insert_date(orderNumber, parcelno, 541, Property_Details, 1, DateTime.Now);

                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");

                    //Tax Information
                    driver.Navigate().GoToUrl("https://payments.sccgov.org/propertytax/Secured");
                    Thread.Sleep(2000);

                    //Address Search
                    //driver.FindElement(By.XPath("//*[@id='tab2_link']/a")).SendKeys(Keys.Enter);
                    //Thread.Sleep(2000);

                    //driver.FindElement(By.Id("FullAddress")).SendKeys(address);
                    //driver.FindElement(By.XPath("//*[@id='FullAddress']")).SendKeys(Keys.Enter);
                    //Thread.Sleep(2000);

                    //parcelno = driver.FindElement(By.XPath("/html/body/div[2]/div/div[2]/div/form/div[1]/div/dl/dd[1]")).Text;
                    //parcelno = WebDriverTest.Before(parcelno, " View Payment History");

                    driver.FindElement(By.Id("ParcelNumber")).SendKeys(parcelno);
                    try
                    {
                        gc.CreatePdf(orderNumber, parcelno, "Tax Details 1", driver, "CA", "Santa Clara");
                        driver.FindElement(By.Id("ParcelSubmit")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                    }
                    catch
                    { }


                    try
                    {
                        ByVisibleElement(driver.FindElement(By.XPath("//*[@id='index_avoidconv_link']/span")));
                        gc.CreatePdf(orderNumber, parcelno, "Tax Details 2", driver, "CA", "Santa Clara");
                    }
                    catch
                    {

                    }
                    //Tax Payment History Details
                    try
                    {

                        driver.FindElement(By.XPath("//*[@id='tabs']/a/span")).Click();
                        Thread.Sleep(5000);
                    }
                    catch
                    { }

                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div[2]/div/div[2]/div/form/div[1]/div/dl/dd[1]/a")).Click();
                        Thread.Sleep(5000);
                    }
                    catch
                    { }
                    IWebElement TaxHistoryTB = driver.FindElement(By.XPath("//*[@id='historybody']"));
                    IList<IWebElement> TaxHistoryTR = TaxHistoryTB.FindElements(By.TagName("tr"));
                    IList<IWebElement> TaxHistoryTD;

                    foreach (IWebElement TaxHistory in TaxHistoryTR)
                    {
                        TaxHistoryTD = TaxHistory.FindElements(By.TagName("td"));
                        if (TaxHistoryTD.Count != 0)
                        {
                            Tax_Year_Payment = TaxHistoryTD[0].Text;
                            APN_Suffix_Payment = TaxHistoryTD[1].Text;
                            Installment_Payment = TaxHistoryTD[2].Text;
                            Tax_Amount_Payment = TaxHistoryTD[3].Text;
                            Additional_Charges_Payment = TaxHistoryTD[4].Text;
                            Paid_Amount_Payment = TaxHistoryTD[5].Text;
                            Paid_Date_Payment = TaxHistoryTD[6].Text;


                            Taxpayment_Details = Tax_Year_Payment + "~" + APN_Suffix_Payment + "~" + Installment_Payment + "~" + Tax_Amount_Payment + "~" + Additional_Charges_Payment + "~" + Paid_Amount_Payment + "~" + Paid_Date_Payment;
                            gc.CreatePdf(orderNumber, parcelno, "Tax Payment History Details", driver, "CA", "Santa Clara");
                            gc.insert_date(orderNumber, parcelno, 544, Taxpayment_Details, 1, DateTime.Now);
                        }
                    }
                    try
                    {
                        ByVisibleElement(driver.FindElement(By.XPath("//*[@id='historybody']/tr[11]/td[1]")));
                        gc.CreatePdf(orderNumber, parcelno, "Tax Payment History Details1", driver, "CA", "Santa Clara");
                    }
                    catch { }

                    try
                    {
                        driver.FindElement(By.XPath("//*[@id='closebutton']")).Click();
                        Thread.Sleep(2000);
                    }
                    catch { }



                    //Current Tax Details
                    try
                    {
                        tax_type = driver.FindElement(By.XPath("//*[@id='select_invoiceslist']/div[1]/div[1]/span")).Text;

                        try
                        {
                            IWebElement CurrentBill = driver.FindElement(By.XPath("//*[@id='select_invoiceslist']/div/div[1]/div/a"));
                            string CurrentTaxBill = CurrentBill.GetAttribute("href");
                            gc.downloadfile(CurrentTaxBill, orderNumber, parcelno, "Current Tax Bill", "CA", "Santa Clara");
                        }
                        catch
                        { }

                        if (tax_type == "Annual Tax Bill" || tax_type == "Supplemental Tax Bill")
                        {
                            for (int i = 0; i < 3; i++)
                            {
                                for (int j = 2; j < 11; j++)
                                {
                                    Installment += driver.FindElement(By.XPath("//*[@id='select_invoiceslist']/div/div[3]/div[" + (i + 2) + "]/div[ " + j + " ]")).Text + "~";
                                }
                                Installment = tax_type + "~" + Installment.Remove(Installment.Length - 1, 1);
                                i++;
                                gc.CreatePdf(orderNumber, parcelno, "Current Tax Details", driver, "CA", "Santa Clara");
                                gc.insert_date(orderNumber, parcelno, 540, Installment, 1, DateTime.Now);
                                Installment = "";
                            }
                        }
                    }
                    catch
                    { }


                    try
                    {
                        tax_type_Supp = driver.FindElement(By.XPath("//*[@id='select_invoiceslist']/div[2]/div[1]/span")).Text;

                        try
                        {
                            IWebElement SupplementalBill = driver.FindElement(By.XPath("//*[@id='select_invoiceslist']/div[2]/div[1]/div/a"));
                            string SupplementalTaxBill = SupplementalBill.GetAttribute("href");
                            gc.downloadfile(SupplementalTaxBill, orderNumber, parcelno, "Supplemental Tax Bill", "CA", "Santa Clara");
                        }
                        catch
                        { }

                        if (tax_type_Supp == "Supplemental Tax Bill")
                        {
                            InstallmentSupplemnt = "";
                            for (int k = 0; k < 3; k++)
                            {
                                for (int l = 2; l < 11; l++)
                                {
                                    InstallmentSupplemnt += driver.FindElement(By.XPath("//*[@id='select_invoiceslist']/div[2]/div[3]/div[" + (k + 2) + "]/div[" + l + "]")).Text + "~";
                                }
                                InstallmentSupplemnt = tax_type_Supp + "~" + InstallmentSupplemnt.Remove(InstallmentSupplemnt.Length - 1, 1);
                                k++;
                                gc.CreatePdf(orderNumber, parcelno, "Supplemental Tax Details", driver, "CA", "Santa Clara");
                                gc.insert_date(orderNumber, parcelno, 545, InstallmentSupplemnt, 1, DateTime.Now);
                                InstallmentSupplemnt = "";
                            }
                        }


                        if (tax_type_Supp == "Supplemental Tax Bill")
                        {
                            InstallmentSupplemnt1 = "";
                            for (int m = 0; m < 3; m++)
                            {
                                for (int n = 2; n < 11; n++)
                                {
                                    try
                                    {
                                        InstallmentSupplemnt1 += driver.FindElement(By.XPath("//*[@id='select_invoiceslist']/div[3]/div[3]/div[" + (m + 2) + "]/div[" + n + "]")).Text + "~";
                                    }
                                    catch
                                    { }
                                }
                                InstallmentSupplemnt1 = tax_type_Supp + "~" + InstallmentSupplemnt1.Remove(InstallmentSupplemnt1.Length - 1, 1);
                                m++;
                                try
                                {
                                    ByVisibleElement(driver.FindElement(By.XPath("//*[@id='select_invoiceslist']/div[3]/div[3]/div[1]/div[2]")));
                                    gc.CreatePdf(orderNumber, parcelno, "Supplemental Tax Details1", driver, "CA", "Santa Clara");
                                    Thread.Sleep(2000);
                                }
                                catch { }
                                gc.insert_date(orderNumber, parcelno, 546, InstallmentSupplemnt1, 1, DateTime.Now);
                                InstallmentSupplemnt1 = "";
                            }
                        }

                    }
                    catch
                    { }


                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "CA", "Santa Clara", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    gc.mergpdf(orderNumber, "CA", "Santa Clara");
                    return "Data Inserted Successfully";
                }
                catch (Exception ex)
                {
                    driver.Quit();
                    GlobalClass.LogError(ex, orderNumber);
                    throw;
                }
            }
        }

        public void ByVisibleElement(IWebElement Element)
        {
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            js.ExecuteScript("arguments[0].scrollIntoView();", Element);
        }
    }
}