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
    public class Webdriver_INHamilton
    {
        IWebDriver driver;
        DBconnection db = new DBconnection();

        GlobalClass gc = new GlobalClass();
        MySqlParameter[] mParam;
        public string FTP_Hamilton(string Address, string parcelNumber, string ownername, string searchType, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;

            string StartTime = "0", AssessmentTime = "0", TaxTime = "0", CitytaxTime = "0", LastEndTime = "0", AssessTakenTime = "", TaxTakentime = "", CityTaxtakentime = "";
            string TotaltakenTime = "";
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            //driver = new ChromeDriver();
            //  driver = new PhantomJSDriver();
            string AlterNateID = "", PropertyAddress = "", owner = "";
            string[] stringSeparators1 = new string[] { "\r\n" };

            List<string> listurl = new List<string>();

            using (driver = new PhantomJSDriver())
            {
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");
                    if (searchType == "titleflex")
                    {
                        gc.TitleFlexSearch(orderNumber, parcelNumber, ownername, Address, "IN", "Hamilton");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_INHamilton"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }

                    if (searchType == "address")
                    {

                        driver.Navigate().GoToUrl("https://secure2.hamiltoncounty.in.gov/propertyreports/index.aspx");
                        Thread.Sleep(4000);
                        driver.FindElement(By.XPath("//*[@id='searchText']")).SendKeys(Address);

                        gc.CreatePdf_WOP(orderNumber, "Address search", driver, "IN", "Hamilton");

                        driver.FindElement(By.XPath("//*[@id='search']")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);
                        gc.CreatePdf_WOP(orderNumber, "Address search Result", driver, "IN", "Hamilton");

                        string Muiti = driver.FindElement(By.XPath("//*[@id='searchResults']/div[1]/div/div")).Text;
                        try
                        {
                            string MultiCount = GlobalClass.Before(Muiti, " RESULTS FOUND");
                            if (Convert.ToInt32(MultiCount) > 25)
                            {
                                HttpContext.Current.Session["multiParcel_Hamiltion_Count"] = "Maximum";
                                return "Maximum";
                            }
                        }
                        catch { }
                        if ((Muiti.Trim().ToUpper() != "1 RESULTS FOUND") && (!Muiti.Contains("NO RESULTS FOUND")))
                        {

                            IWebElement MultiOwnerTable = driver.FindElement(By.XPath("//*[@id='searchResults']"));
                            IList<IWebElement> MultiOwnerRow = MultiOwnerTable.FindElements(By.ClassName("media"));
                            IList<IWebElement> MultiOwnerTD;

                            //string AlterNateID = "", PropertyAddress = "", LegalDescriptoin = "", YearBuilt = "";
                            foreach (IWebElement row1 in MultiOwnerRow)
                            {
                                MultiOwnerTD = row1.FindElements(By.TagName("span"));
                                if (MultiOwnerTD.Count != 0 && MultiOwnerTD.Count != 2 && MultiOwnerTD.Count != 1)
                                {

                                    parcelNumber = MultiOwnerTD[1].Text.Replace("Parcel #:", "");

                                    PropertyAddress = MultiOwnerTD[0].Text;


                                    string Multi = PropertyAddress;
                                    gc.insert_date(orderNumber, parcelNumber, 460, Multi, 1, DateTime.Now);

                                }
                            }


                            HttpContext.Current.Session["multiParcel_Hamiltion"] = "Yes";
                            driver.Quit();
                            return "MultiParcel";

                        }
                        try
                        {
                            driver.FindElement(By.XPath("//*[@id='searchResults']/div[2]/a/div/div/div/div/span[1]")).Click();
                        }
                        catch { }
                    }

                    else if (searchType == "parcel")
                    {
                        driver.Navigate().GoToUrl("https://secure2.hamiltoncounty.in.gov/propertyreports/index.aspx");
                        Thread.Sleep(4000);
                        driver.FindElement(By.XPath("//*[@id='searchText']")).SendKeys(parcelNumber);

                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search", driver, "IN", "Hamilton");

                        driver.FindElement(By.XPath("//*[@id='search']")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search Result", driver, "IN", "Hamilton");
                        try
                        {
                            driver.FindElement(By.XPath("//*[@id='searchResults']/div[2]/a/div/div/div/div/span[1]")).Click();
                        }
                        catch { }
                    }

                    try
                    {
                        IWebElement INodata = driver.FindElement(By.Id("searchResults"));
                        if (INodata.Text.Contains("NO RESULTS FOUND"))
                        {
                            HttpContext.Current.Session["Nodata_INHamilton"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }
                    Thread.Sleep(2000);
                    driver.FindElement(By.XPath("//*[@id='propertyAssessment']")).Click();
                    Thread.Sleep(4000);
                    string StateParcelID = "", MailingAddress = "", StateTaxDistrict = "", TaxingUnit = "", LegalDescription = "";
                    parcelNumber = driver.FindElement(By.XPath("//*[@id='parcelPA']")).Text;
                    gc.CreatePdf(orderNumber, parcelNumber, "Propety Deatil", driver, "IN", "Hamilton");
                    PropertyAddress = driver.FindElement(By.XPath("//*[@id='propassess']/div/div[2]/div[2]/div[1]/ul")).Text.Replace("\r\n", "");
                    ownername = driver.FindElement(By.XPath("//*[@id='deededOwnerPA']")).Text;
                    StateParcelID = driver.FindElement(By.XPath("//*[@id='stateParcelPA']")).Text;
                    MailingAddress = driver.FindElement(By.XPath("//*[@id='propassess']/div/div[2]/div[2]/div[2]/ul")).Text.Replace("\r\n", " ");
                    StateTaxDistrict = driver.FindElement(By.XPath("//*[@id='taxDistrict']")).Text;
                    TaxingUnit = driver.FindElement(By.XPath("//*[@id='taxUnit']")).Text;



                    driver.FindElement(By.XPath("//*[@id='ownershipInfo']")).Click();
                    Thread.Sleep(4000);
                    LegalDescription = driver.FindElement(By.XPath("//*[@id='legalDescription']")).Text;

                    string Property = StateParcelID + "~" + ownername + "~" + PropertyAddress + "~" + MailingAddress + "~" + StateTaxDistrict + "~" + TaxingUnit + "~" + LegalDescription;
                    gc.insert_date(orderNumber, parcelNumber, 456, Property, 1, DateTime.Now);

                    driver.FindElement(By.XPath("//*[@id='propertyAssessment']")).Click();
                    Thread.Sleep(4000);
                    string AssessedLandValue = "", AssessedImprovementValue = "", TotalAssessedValue = "";
                    gc.CreatePdf(orderNumber, parcelNumber, "Assessment Deatil", driver, "IN", "Hamilton");
                    AssessedLandValue = driver.FindElement(By.XPath("//*[@id='propertyAssessmentsTable']/div[1]/div[2]/table/tbody/tr[1]/td[2]")).Text;
                    AssessedImprovementValue = driver.FindElement(By.XPath("//*[@id='propertyAssessmentsTable']/div[1]/div[2]/table/tbody/tr[2]/td[2]")).Text;
                    TotalAssessedValue = driver.FindElement(By.XPath("//*[@id='propertyAssessmentsTable']/div[1]/div[2]/table/tbody/tr[3]/td[2]")).Text;
                    string Asses = AssessedLandValue + "~" + AssessedImprovementValue + "~" + TotalAssessedValue;
                    gc.insert_date(orderNumber, parcelNumber, 457, Asses, 1, DateTime.Now);


                    string SprindDuedate = "", FallDuedate = "", SpringTax = "", DelqTaxPen = "", SpecialAssessments = "", BackTaxCivilPenalty = "", SpringTotals = "", FallTax = "", FSpecialAssessments = "", Penalty = "";
                    string FBackTaxCivilPenalty = "", FallTotals = "", ABackTaxCivilPenalty = "", OtherSpecialAssessments = "", APenalty = "";
                    driver.FindElement(By.XPath("//*[@id='taxPayments']")).Click();
                    Thread.Sleep(4000);
                    gc.CreatePdf(orderNumber, parcelNumber, "Tax Deatil", driver, "IN", "Hamilton");
                    SpringTax = driver.FindElement(By.XPath("//*[@id='SpringBreakdown']/tr[2]/td[1]")).Text;
                    DelqTaxPen = driver.FindElement(By.XPath("//*[@id='SpringBreakdown']/tr[3]/td[1]")).Text;
                    SpecialAssessments = driver.FindElement(By.XPath("//*[@id='SpringBreakdown']/tr[4]/td[1]")).Text;
                    BackTaxCivilPenalty = driver.FindElement(By.XPath("//*[@id='SpringBreakdown']/tr[5]/td[1]")).Text;
                    SpringTotals = driver.FindElement(By.XPath("//*[@id='SpringBreakdown']/tr[6]/td[1]/h6")).Text;
                    FallTax = driver.FindElement(By.XPath("//*[@id='FallBreakdown']/tr[2]/td[1]")).Text;
                    FSpecialAssessments = driver.FindElement(By.XPath("//*[@id='FallBreakdown']/tr[3]/td[1]")).Text;
                    Penalty = driver.FindElement(By.XPath("//*[@id='FallBreakdown']/tr[4]/td[1]")).Text;
                    FBackTaxCivilPenalty = driver.FindElement(By.XPath("//*[@id='FallBreakdown']/tr[5]/td[1]")).Text;
                    FallTotals = driver.FindElement(By.XPath("//*[@id='FallBreakdown']/tr[6]/td[1]/h6")).Text;
                    ABackTaxCivilPenalty = driver.FindElement(By.XPath("//*[@id='afterFallBackTax']")).Text;
                    OtherSpecialAssessments = driver.FindElement(By.XPath("//*[@id='afterFallSpecialAssessments']")).Text;
                    APenalty = driver.FindElement(By.XPath("//*[@id='afterFallPenalty']")).Text;
                    SprindDuedate = driver.FindElement(By.XPath("//*[@id='SpringBreakdown']/tr[6]/th/h6/small[1]")).Text;
                    FallDuedate = driver.FindElement(By.XPath("//*[@id='FallBreakdown']/tr[6]/th/h6/small[1]")).Text;


                    string Charges = "Charge" + "~" + SpringTax + "~" + DelqTaxPen + "~" + SpecialAssessments + "~" + BackTaxCivilPenalty + "~" + SpringTotals + "~" + FallTax + "~" + FSpecialAssessments + "~" + Penalty + "~" + FBackTaxCivilPenalty + "~" + FallTotals + "~" + ABackTaxCivilPenalty + "~" + OtherSpecialAssessments + "~" + APenalty + "~" + "" + "~" + "" + "~" + SprindDuedate + "~" + FallDuedate;
                    string TotalPaid = "", TotalDue = "";


                    gc.insert_date(orderNumber, parcelNumber, 458, Charges, 1, DateTime.Now);

                    SpringTax = driver.FindElement(By.XPath("//*[@id='SpringBreakdown']/tr[2]/td[2]")).Text;
                    DelqTaxPen = driver.FindElement(By.XPath("//*[@id='SpringBreakdown']/tr[3]/td[2]")).Text;
                    SpecialAssessments = driver.FindElement(By.XPath("//*[@id='SpringBreakdown']/tr[4]/td[2]")).Text;
                    BackTaxCivilPenalty = driver.FindElement(By.XPath("//*[@id='SpringBreakdown']/tr[5]/td[2]")).Text;
                    SpringTotals = driver.FindElement(By.XPath("//*[@id='SpringBreakdown']/tr[6]/td[2]/h6")).Text;
                    FallTax = driver.FindElement(By.XPath("//*[@id='FallBreakdown']/tr[2]/td[2]")).Text;
                    FSpecialAssessments = driver.FindElement(By.XPath("//*[@id='FallBreakdown']/tr[3]/td[2]")).Text;
                    Penalty = driver.FindElement(By.XPath("//*[@id='FallBreakdown']/tr[4]/td[2]")).Text;
                    FBackTaxCivilPenalty = driver.FindElement(By.XPath("//*[@id='FallBreakdown']/tr[5]/td[2]")).Text;
                    FallTotals = driver.FindElement(By.XPath("//*[@id='FallBreakdown']/tr[6]/td[2]/h6")).Text;
                    ABackTaxCivilPenalty = driver.FindElement(By.XPath("//*[@id='afterFallBackTaxBalance']")).Text;
                    OtherSpecialAssessments = driver.FindElement(By.XPath("//*[@id='afterFallSpecialAssessmentsBalance']")).Text;
                    APenalty = driver.FindElement(By.XPath("//*[@id='afterFallPenaltyBalance']")).Text;
                    try
                    {
                        TotalPaid = driver.FindElement(By.XPath("//*[@id='totalPayments']")).Text;
                    }
                    catch
                    {

                    }
                    
                    TotalDue = driver.FindElement(By.XPath("//*[@id='panel-taxpmt']/div[4]/div/div[2]/div/div[1]/table/tbody/tr/td")).Text.Replace("Balance Due:", "");


                    string Balance = "Balance" + "~" + SpringTax + "~" + DelqTaxPen + "~" + SpecialAssessments + "~" + BackTaxCivilPenalty + "~" + SpringTotals + "~" + FallTax + "~" + FSpecialAssessments + "~" + Penalty + "~" + FBackTaxCivilPenalty + "~" + FallTotals + "~" + ABackTaxCivilPenalty + "~" + OtherSpecialAssessments + "~" + APenalty + "~" + TotalPaid + "~" + TotalDue + "~" + "" + "~" + "";

                    gc.insert_date(orderNumber, parcelNumber, 458, Balance, 1, DateTime.Now);


                    driver.FindElement(By.XPath("//*[@id='statements']")).Click();
                    Thread.Sleep(1000);

                    gc.CreatePdf(orderNumber, parcelNumber, "Statement Deatil", driver, "IN", "Hamilton");

                    //string AssessedFCV = "", AssessedLPV = "", PropertyUseCode = "", PUDescription = "", TaxAreaCode = "", ValuationSource = "";



                    //IWebElement TaxDisTB = driver.FindElement(By.XPath("//*[@id='SpringBreakdown']"));
                    //IList<IWebElement> TaxDisTR = TaxDisTB.FindElements(By.TagName("tr"));
                    //IList<IWebElement> TaxDisTD;

                    //foreach (IWebElement row1 in TaxDisTR)
                    //{
                    //    TaxDisTD = row1.FindElements(By.TagName("td"));

                    //    if (TaxDisTD.Count != 0&& TaxDisTD.Count != 1)
                    //    {

                    //        SpringTax = TaxDisTD[0].Text+"~"+ SpringTax;
                    //        DelqTaxPen = TaxDisTD[1].Text + "~" + DelqTaxPen;

                    //    }
                    //}

                    //IWebElement TaxDisTB1 = driver.FindElement(By.XPath("//*[@id='FallBreakdown']"));
                    //IList<IWebElement> TaxDisTR1 = TaxDisTB1.FindElements(By.TagName("tr"));
                    //IList<IWebElement> TaxDisTD1;

                    //foreach (IWebElement row1 in TaxDisTR1)
                    //{
                    //    TaxDisTD1 = row1.FindElements(By.TagName("td"));

                    //    if (TaxDisTD1.Count != 0 && TaxDisTD1.Count != 1)
                    //    {

                    //        SpecialAssessments = TaxDisTD1[0].Text + "~" + SpecialAssessments;
                    //        BackTaxCivilPenalty = TaxDisTD1[1].Text + "~" + BackTaxCivilPenalty;

                    //    }
                    //}
                    //SpringTotals = SpringTax + "qw";
                    //FallTax = DelqTaxPen + "qw";

                    //SpringTotals = SpringTotals.Replace("~qw", "");
                    //FallTax = FallTax.Replace("~qw", "");

                    //var Spilt = SpringTotals.Split('~');
                    //var Spilt1 = FallTax.Split('~');
                    //string Tax1 = "", Tax2 = "", Tax3 = "", Tax4 = "";
                    //for (int K= Spilt.Length-1;K>=0;K--)
                    //{
                    //    Tax1 = Spilt[K] +"~"+ Tax1;


                    //}
                    //for (int J = Spilt1.Length - 1; J >= 0; J--)
                    //{
                    //    Tax2 = Spilt1[J] +"~" +Tax1;


                    //}
                    //Tax1 = Tax1.Replace("~~", "");
                    //Tax2 = Tax2.Replace("~~", "");
                    //string Final = Tax1+ Tax2+"hg";

                    //Final = Final.Replace("~hg", "");


                    //FallTotals = SpecialAssessments + "qw";
                    //ABackTaxCivilPenalty = BackTaxCivilPenalty + "qw";

                    //FallTotals = FallTotals.Replace("~qw", "");
                    //ABackTaxCivilPenalty = ABackTaxCivilPenalty.Replace("~qw", "");

                    //var Spilt2 = FallTotals.Split('~');
                    //var Spilt3 = ABackTaxCivilPenalty.Split('~');

                    //for (int M = Spilt2.Length - 1; M >= 0;M--)
                    //{
                    //    Tax3 = Spilt2[M] + "~" + Tax1;


                    //}
                    //for (int N = Spilt3.Length - 1; N >= 0; N--)
                    //{
                    //    Tax4 = Spilt3[N] + "~" + Tax4;


                    //}
                    //Tax3 = Tax3.Replace("~~", "");
                    //Tax4 = Tax4.Replace("~~", "");
                    //string Final1 = Tax3 + Tax4 + "hg";

                    //Final1 = Final1.Replace("~hg", "");

                    //Final1 = "Charge" +"~"+ Final1;
                    //Final = "Balance" +"~"+ Final;





                    driver.FindElement(By.XPath("//*[@id='specialAssessments']")).Click();
                    Thread.Sleep(4000);
                    gc.CreatePdf(orderNumber, parcelNumber, "Special Assessment Deatil", driver, "IN", "Hamilton");
                    string SpecialAssessmentName = "", SpringAssessment = "", DelinquentPenalties = "", FallAssessment = "";

                    try
                    {

                        IWebElement TaxSpecial = driver.FindElement(By.XPath("//*[@id='specialAssessmentsTable']/div/div[2]/table/tbody"));
                        IList<IWebElement> TaxSpecialTR = TaxSpecial.FindElements(By.TagName("tr"));
                        IList<IWebElement> TaxSpecialTD;

                        foreach (IWebElement row1 in TaxSpecialTR)
                        {
                            TaxSpecialTD = row1.FindElements(By.TagName("td"));

                            if (TaxSpecialTD.Count != 0 && TaxSpecialTD.Count != 1)
                            {
                                SpecialAssessmentName = TaxSpecialTD[0].Text;
                                SpringAssessment = TaxSpecialTD[1].Text;
                                DelinquentPenalties = TaxSpecialTD[2].Text;
                                FallAssessment = TaxSpecialTD[3].Text;
                                TotalDue = TaxSpecialTD[4].Text;

                                string SpAsses = SpecialAssessmentName + "~" + SpringAssessment + "~" + DelinquentPenalties + "~" + FallAssessment + "~" + TotalDue;

                                gc.insert_date(orderNumber, parcelNumber, 459, SpAsses, 1, DateTime.Now);
                            }
                        }
                        driver.FindElement(By.Id("comparisonReport")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Comparison Report Deatil", driver, "IN", "Hamilton");
                        AssessmentTime = DateTime.Now.ToString("HH:mm:ss");

                    }
                    catch
                    {

                    }

                    driver.FindElement(By.XPath("//*[@id='ownershipInfo']")).Click();
                    Thread.Sleep(4000);
                    gc.CreatePdf(orderNumber, parcelNumber, "Owner Deatil", driver, "IN", "Hamilton");
                    driver.FindElement(By.XPath("//*[@id='currentDeductions']")).Click();
                    Thread.Sleep(4000);
                    gc.CreatePdf(orderNumber, parcelNumber, "Owner Current Deatil", driver, "IN", "Hamilton");
                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");


                    gc.insert_TakenTime(orderNumber, "IN", "Hamilton", StartTime, AssessmentTime, TaxTime, CityTaxtakentime, LastEndTime);

                    driver.Quit();
                    gc.mergpdf(orderNumber, "IN", "Hamilton");
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