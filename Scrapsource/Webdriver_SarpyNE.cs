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
    public class Webdriver_SarpyNE
    {
        IWebDriver driver;
        GlobalClass gc = new GlobalClass();
        public string FTP_SarpyNE(string address, string ownername, string parcelNumber, string searchType, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = ""; int rescount = 0;
            string taxsales = "", certno = "", redemno = "", bankrup = "", foreno = "", foredate = "", maturitydate = "", redemdate = "", ExemptionCode = "", ExemptionAmount = "", SpecialAssessment = "", billno = "", delingurl = "", specialurl = "";
            List<string> taxurl = new List<string>();
            List<string> billurl = new List<string>();
            using (driver = new PhantomJSDriver())
            {

                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");

                    if (searchType == "titleflex")
                    {
                        gc.TitleFlexSearch(orderNumber, parcelNumber, "", address, "NE", "Sarpy");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_SarpyNE"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }

                    if (searchType == "address")
                    {
                        driver.Navigate().GoToUrl("https://apps.sarpy.com/sarpyproperty/");
                        driver.FindElement(By.Id("MainContent_SimpleControl2_txtAddress")).SendKeys(address);
                        gc.CreatePdf_WOP(orderNumber, "InputPassed Address Search", driver, "NE", "Sarpy");
                        driver.FindElement(By.Id("MainContent_SimpleControl2_cmdSubmit")).SendKeys(Keys.Enter);
                        Thread.Sleep(5000);
                        gc.CreatePdf_WOP(orderNumber, "Address Search Results", driver, "NE", "Sarpy");
                        IWebElement multiaddrtableElement = driver.FindElement(By.XPath("//*[@id='ResultsGrid']/table/tbody/tr[1]/td[1]/table/tbody[2]"));
                        IList<IWebElement> multiaddrtableRow = multiaddrtableElement.FindElements(By.TagName("tr"));
                        rescount = multiaddrtableRow.Count;
                        if (rescount - 1 > 1)
                        {
                            multiparcel(orderNumber);
                            return "MultiParcel";
                        }
                    }

                    if (searchType == "parcel")
                    {
                        driver.Navigate().GoToUrl("https://apps.sarpy.com/sarpyproperty/");
                        driver.FindElement(By.Id("MainContent_SimpleControl2_txtLocid")).SendKeys(parcelNumber);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search Input Passed", driver, "NE", "Sarpy");
                        driver.FindElement(By.Id("MainContent_SimpleControl2_cmdSubmit")).SendKeys(Keys.Enter);
                        Thread.Sleep(5000);
                        gc.CreatePdf_WOP(orderNumber, "Parcel Search Result", driver, "NE", "Sarpy");
                    }
                    if (searchType == "ownername")
                    {
                        driver.Navigate().GoToUrl("https://apps.sarpy.com/sarpyproperty/");
                        if (ownername.Contains(" "))
                        {
                            ownername = ownername.Replace(" ", "%");
                        }
                        Thread.Sleep(3000);
                        driver.FindElement(By.Id("MainContent_SimpleControl2_txtName")).SendKeys(ownername);
                        gc.CreatePdf_WOP(orderNumber, "Ownername Search Input Passed", driver, "NE", "Sarpy");
                        driver.FindElement(By.Id("MainContent_SimpleControl2_cmdSubmit")).SendKeys(Keys.Enter);
                        Thread.Sleep(5000);
                        gc.CreatePdf_WOP(orderNumber, "Ownername Search Result", driver, "NE", "Sarpy");
                        IWebElement multiaddrtableElement = driver.FindElement(By.XPath("//*[@id='ResultsGrid']/table/tbody/tr[1]/td[1]/table/tbody[2]"));
                        IList<IWebElement> multiaddrtableRow = multiaddrtableElement.FindElements(By.TagName("tr"));
                        rescount = multiaddrtableRow.Count;
                        if (rescount - 1 > 1)
                        {
                            multiparcel(orderNumber);
                            return "MultiParcel";
                        }
                    }


                    try
                    {
                        IWebElement INodata = driver.FindElement(By.XPath("//*[@id='ResultsGrid']/table/tbody/tr[1]/td[1]/table"));
                        if (!INodata.Text.Contains("Property Record File"))
                        {
                            HttpContext.Current.Session["Nodata_SarpyNE"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }


                    //property Details
                    driver.FindElement(By.LinkText("View Details")).SendKeys(Keys.Enter);
                    Thread.Sleep(2000);
                    string propbulktext = driver.FindElement(By.XPath("//*[@id='AtrControl1_TableMain']/tbody")).Text;
                    string outparcelno = "", propaddr = "", owner = "", coowner = "", mailaddress = "", legal = "", taxdistrict = "", map = "", proclass = "", year_built = "";
                    outparcelno = gc.Between(propbulktext, "Parcel Number:", "Location:").Trim();
                    gc.CreatePdf(orderNumber, outparcelno, "Property Details", driver, "NE", "Sarpy");
                    propaddr = gc.Between(propbulktext, "Location:", "Owner:");
                    owner = gc.Between(propbulktext, "Owner:", "C\\O");
                    coowner = gc.Between(propbulktext, "C\\O", "Mail Address:");
                    mailaddress = gc.Between(propbulktext, "Mail Address:", "Legal:");
                    legal = gc.Between(propbulktext, "Legal:", "Tax District:");
                    taxdistrict = gc.Between(propbulktext, "Tax District:", "Map #:");
                    try
                    {
                        map = gc.Between(propbulktext, "Map #:", "Property Class:");
                        proclass = gc.Between(propbulktext, "Property Class:", "NBHD Code:");
                    }
                    catch
                    {
                    }
                    try
                    {
                        map = gc.Between(propbulktext, "Map #:", "Greenbelt Area:");
                        proclass = gc.Between(propbulktext, "Property Class:", "NBHD Code:");
                    }
                    catch { }
                    try
                    {
                        if (proclass == "")
                        {
                            proclass = GlobalClass.After(propbulktext, "Property Class:").Replace("Click Picture for Larger View.", "").Trim();
                        }
                    }
                    catch { }
                    //Xpath =	//label[@id='message23']
                    try
                    {
                        string yearbuiltfulltext = driver.FindElement(By.XPath("//table[contains(@id, '_tblFarm')]/tbody")).Text;
                        year_built = gc.Between(yearbuiltfulltext, "Year Built:	", "#Bedrooms above Grade").Trim();
                        year_built = GlobalClass.After(year_built, "Year Built:");
                    }
                    catch
                    { }
                    try
                    {
                        string yearbuiltfulltext = driver.FindElement(By.XPath("//table[contains(@id, '_tblResidential')]/tbody")).Text;
                        year_built = gc.Between(yearbuiltfulltext, "Year Built:	", "#Bedrooms above Grade").Trim();
                        year_built = GlobalClass.After(year_built, "Year Built:");
                    }
                    catch { }
                    try
                    {

                        IWebElement commercialtable = driver.FindElement(By.XPath("//table[contains(@id, '_gridBuilding')]/tbody"));
                        IList<IWebElement> commercialtableow = commercialtable.FindElements(By.TagName("tr"));
                        IList<IWebElement> commercialtablerowtd;
                        foreach (IWebElement rowid in commercialtableow)
                        {
                            commercialtablerowtd = rowid.FindElements(By.TagName("td"));
                            if (commercialtablerowtd.Count != 0 && !rowid.Text.Contains("Built"))
                            {
                                year_built = commercialtablerowtd[1].Text;
                                HttpContext.Current.Session["Commercial_NESarpy"] = "Yes";
                            }
                        }

                    }
                    catch { }
                    //Property Address~Owner Name~Coowner~Mail Address~Legal Description~Tax District~Map~Property Class~Year Built
                    string propdetails = propaddr + "~" + owner + "~" + coowner + "~" + mailaddress + "~" + legal + "~" + taxdistrict + "~" + map + "~" + proclass + "~" + year_built;
                    gc.insert_date(orderNumber, outparcelno, 580, propdetails, 1, DateTime.Now);

                    //assessment Details
                    IWebElement valuetableElement = driver.FindElement(By.XPath("//*[@id='ValuationControl_gridValuation']/tbody"));
                    gc.CreatePdf(orderNumber, outparcelno, "Assessment Details", driver, "NE", "Sarpy");
                    IList<IWebElement> valuetableRow = valuetableElement.FindElements(By.TagName("tr"));
                    int valoremtablerowcount = valuetableRow.Count;
                    IList<IWebElement> valuetableRowTD;
                    int d = 0;
                    foreach (IWebElement rowid in valuetableRow)
                    {
                        valuetableRowTD = rowid.FindElements(By.TagName("td"));
                        if (valuetableRowTD.Count != 0 && !rowid.Text.Contains("Roll Year") && d < 2)
                        {
                            //Roll Year~Land Value~Impr Value~Outbuildings~Total Value~PV~Form191
                            string assesmentdetails = valuetableRowTD[0].Text + "~" + valuetableRowTD[1].Text + "~" + valuetableRowTD[2].Text + "~" + valuetableRowTD[3].Text + "~" + valuetableRowTD[4].Text + "~" + valuetableRowTD[5].Text + "~" + valuetableRowTD[6].Text;
                            gc.insert_date(orderNumber, outparcelno, 581, assesmentdetails, 1, DateTime.Now);
                            d++;
                        }
                    }

                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");

                    //Tax Sale Details
                    //Tax Sales~Certificate Number~Redemption Number~Bankruptcy~Foreclosure Number~Foreclosure Date~Maturity Date~Redemption Date

                    string taxsalefulltext = driver.FindElement(By.XPath("//*[@id='TreasurerControl_tblTreasurer']/tbody")).Text;
                    IWebElement taxsaletableElement = driver.FindElement(By.XPath("//*[@id='TreasurerControl_tblTreasurer']/tbody"));
                    gc.CreatePdf(orderNumber, outparcelno, "Assessment Details", driver, "NE", "Sarpy");
                    IList<IWebElement> taxsaletableElementrow = taxsaletableElement.FindElements(By.TagName("tr"));
                    IList<IWebElement> valuetableRowa;
                    foreach (IWebElement rowid in taxsaletableElementrow)
                    {
                        valuetableRowa = rowid.FindElements(By.TagName("a"));
                        if (valuetableRowa.Count != 0 && !rowid.Text.Contains("No Tax"))
                        {
                            if (rowid.Text.Contains("DELINQUENT"))
                            {
                                delingurl = valuetableRowa[0].GetAttribute("href");
                            }
                            else
                            {
                                specialurl = valuetableRowa[0].GetAttribute("href");
                            }
                        }
                    }
                    //*[@id="TreasurerControl_TableCell26"]/a
                    gc.CreatePdf(orderNumber, outparcelno, "Tax Sale Details", driver, "NE", "Sarpy");
                    taxsales = gc.Between(taxsalefulltext, "Tax Sales", "Certificate #").Trim();
                    certno = gc.Between(taxsalefulltext, "Certificate #", "Maturity Date").Trim();
                    redemno = gc.Between(taxsalefulltext, "Redemption #", "Redemption Date").Trim();
                    bankrup = GlobalClass.After(taxsalefulltext, "Bankruptcy").Trim();
                    foreno = gc.Between(taxsalefulltext, "Forclosure #", "Mortgage Company #").Trim();
                    foredate = gc.Between(taxsalefulltext, "Foreclosure Date", "Exemption Code").Trim().Replace("Mortgage Company", "");
                    maturitydate = gc.Between(taxsalefulltext, "Maturity Date", "Redemption #").Trim();
                    redemdate = gc.Between(taxsalefulltext, "Redemption Date", "Bankruptcy").Trim();
                    string taxsaledetails = taxsales + "~" + certno + "~" + redemno + "~" + bankrup + "~" + foreno + "~" + foredate + "~" + maturitydate + "~" + redemdate;
                    gc.insert_date(orderNumber, outparcelno, 584, taxsaledetails, 1, DateTime.Now);

                    //Tax history details

                    IWebElement taxhistableElement = driver.FindElement(By.XPath("//*[@id='TreasurerControl_TaxGrid']/tbody"));
                    gc.CreatePdf(orderNumber, outparcelno, "Tax Full History", driver, "NE", "Sarpy");
                    IList<IWebElement> taxhistableRow = taxhistableElement.FindElements(By.TagName("tr"));
                    //int valoremtablerowcount = taxhistableRow.Count;
                    IList<IWebElement> taxhistableRowTD;
                    int i = 0;
                    foreach (IWebElement rowid in taxhistableRow)
                    {
                        taxhistableRowTD = rowid.FindElements(By.TagName("td"));
                        if (taxhistableRowTD.Count != 0 && !rowid.Text.Contains("Year"))
                        {
                            if (i < 3)
                            {
                                IWebElement ITaxBillCount = taxhistableRowTD[1].FindElement(By.TagName("a"));
                                string taxlink = ITaxBillCount.GetAttribute("href");
                                taxurl.Add(taxlink);
                                i++;
                            }
                            //Year~Statement~Tax District~Source~Taxes Due~Total Due~Balance
                            string taxhisdetails = taxhistableRowTD[0].Text + "~" + taxhistableRowTD[1].Text + "~" + taxhistableRowTD[2].Text + "~" + taxhistableRowTD[3].Text + "~" + taxhistableRowTD[4].Text + "~" + taxhistableRowTD[5].Text + "~" + taxhistableRowTD[6].Text;
                            gc.insert_date(orderNumber, outparcelno, 586, taxhisdetails, 1, DateTime.Now);
                        }
                    }

                    //Tax Details
                    int j = 0;
                    foreach (string URL in taxurl)
                    {
                        driver.Navigate().GoToUrl(URL);
                        Thread.Sleep(3000);

                        string Statement = "", TaxPayer = "", RollYear = "", TaxYear = "", Source = "", GrossTax = "", Homestead = "", TaxesDue = "", Drainage = "", PenaltyTax = "", CertFees = "", Advertising = "", TaxCredit = "", AgTaxCredit = "", TotalDue = "", TaxPaid = "", TaxDue = "", InterestDue = "", TotalDue1 = "";
                        try
                        {
                            //Statement~Tax Payer~Roll Year~Tax Year~Source~Gross Tax~Homestead~Taxes Due~Drainage~Penalty Tax~Cert Fees~Advertising~Tax Credit~Ag Tax Credit~Total Due~Tax Paid~Tax Due~Interest Due~Total Due~Exemption Code~Exemption Amount~Special Assessment
                            string taxdetailsfulltext = driver.FindElement(By.XPath("//*[@id='form1']/div[3]/table[1]")).Text;
                            IWebElement IStatement = driver.FindElement(By.Id("txtStatement"));
                            Statement = IStatement.GetAttribute("value");
                            IWebElement ITaxPayer = driver.FindElement(By.Id("txtTaxPayer"));
                            TaxPayer = ITaxPayer.GetAttribute("value");
                            IWebElement IRollYear = driver.FindElement(By.Id("txtRollYear"));
                            RollYear = IRollYear.GetAttribute("value");
                            IWebElement ITaxYear = driver.FindElement(By.Id("txtTaxYear"));
                            TaxYear = ITaxYear.GetAttribute("value");
                            gc.CreatePdf(orderNumber, outparcelno, "Tax Details " + TaxYear, driver, "NE", "Sarpy");
                            IWebElement ISource = driver.FindElement(By.Id("txtSource"));
                            Source = ISource.GetAttribute("value");
                            IWebElement IGrossTax = driver.FindElement(By.Id("txtGrossTax"));
                            GrossTax = IGrossTax.GetAttribute("value");
                            IWebElement IHomestead = driver.FindElement(By.Id("txtHomeStead"));
                            Homestead = IHomestead.GetAttribute("value");
                            IWebElement ITaxesDue = driver.FindElement(By.Id("txtTaxesDue"));
                            TaxesDue = ITaxesDue.GetAttribute("value");
                            IWebElement IDrainage = driver.FindElement(By.Id("txtDrainage"));
                            Drainage = IDrainage.GetAttribute("value");
                            IWebElement IPenaltyTax = driver.FindElement(By.Id("txtPenaltyTax"));
                            PenaltyTax = IPenaltyTax.GetAttribute("value");
                            IWebElement ICertFees = driver.FindElement(By.Id("txtCertFees"));
                            CertFees = ICertFees.GetAttribute("value");
                            IWebElement IAdvertising = driver.FindElement(By.Id("txtAdvertising"));
                            Advertising = IAdvertising.GetAttribute("value");
                            IWebElement ITaxCredit = driver.FindElement(By.Id("txtTaxCredit"));
                            TaxCredit = ITaxCredit.GetAttribute("value");
                            IWebElement IAgTaxCredit = driver.FindElement(By.Id("txtAGTaxCredit"));
                            AgTaxCredit = IAgTaxCredit.GetAttribute("value");
                            IWebElement ITotalDue = driver.FindElement(By.Id("txtTotalDue"));
                            TotalDue = ITotalDue.GetAttribute("value");
                            IWebElement ITaxPaid = driver.FindElement(By.Id("txtTaxPaid"));
                            TaxPaid = ITaxPaid.GetAttribute("value");
                            IWebElement ITaxDue = driver.FindElement(By.Id("txtTaxDue"));
                            TaxDue = ITaxDue.GetAttribute("value");
                            IWebElement IInterestDue = driver.FindElement(By.Id("txtInterestDue"));
                            InterestDue = IInterestDue.GetAttribute("value");
                            IWebElement ITotalDue1 = driver.FindElement(By.Id("txtGrandTotal"));
                            TotalDue1 = ITotalDue1.GetAttribute("value");
                            //
                            if (j == 0)
                            {
                                ExemptionCode = gc.Between(taxsalefulltext, "Exemption Code", "Exemption Amount").Trim();
                                ExemptionAmount = gc.Between(taxsalefulltext, "Exemption Amount", "Specials").Trim();
                                SpecialAssessment = gc.Between(taxsalefulltext, "Specials", "Tax Sales").Trim();
                                j++;
                            }
                            else
                            {
                                ExemptionCode = "";
                                ExemptionAmount = "";
                                SpecialAssessment = "";
                            }
                            string taxdetails = Statement + "~" + TaxPayer + "~" + RollYear + "~" + TaxYear + "~" + Source + "~" + GrossTax + "~" + Homestead + "~" + TaxesDue + "~" + Drainage + "~" + PenaltyTax + "~" + CertFees + "~" + Advertising + "~" + TaxCredit + "~" + AgTaxCredit + "~" + TotalDue + "~" + TaxPaid + "~" + TaxDue + "~" + InterestDue + "~" + TotalDue1 + "~" + ExemptionCode + "~" + ExemptionAmount + "~" + SpecialAssessment;
                            gc.insert_date(orderNumber, outparcelno, 587, taxdetails, 1, DateTime.Now);
                        }

                        catch
                        {

                        }
                        try
                        {
                            //Receipt~Code~Pay Date~Tax Payment~Weed Pay~Interest~Total Payment
                            IWebElement paymenttable = driver.FindElement(By.XPath("//*[@id='tblRct']/tbody"));
                            IList<IWebElement> paymenttablerow = paymenttable.FindElements(By.TagName("tr"));
                            IList<IWebElement> paymenttablerowTD;
                            foreach (IWebElement rowid in paymenttablerow)
                            {
                                paymenttablerowTD = rowid.FindElements(By.TagName("td"));
                                if (paymenttablerowTD.Count != 0 && !rowid.Text.Contains("Payments") && !rowid.Text.Contains("Receipt #"))
                                {
                                    billno = paymenttablerowTD[0].Text;
                                    string paymentdetails = paymenttablerowTD[0].Text + "~" + paymenttablerowTD[1].Text + "~" + paymenttablerowTD[2].Text + "~" + paymenttablerowTD[3].Text + "~" + paymenttablerowTD[4].Text + "~" + paymenttablerowTD[5].Text + "~" + paymenttablerowTD[6].Text;
                                    gc.insert_date(orderNumber, outparcelno, 589, paymentdetails, 1, DateTime.Now);
                                    IWebElement IBillCount = paymenttablerowTD[7].FindElement(By.TagName("a"));
                                    string billlink = IBillCount.GetAttribute("href");
                                    billurl.Add(billlink);
                                    //  gc.downloadfile(billlink, orderNumber, outparcelno, paymenttablerowTD[0].Text, "NE", "Sarpy");
                                }
                            }
                        }
                        catch (Exception e)
                        {
                        }
                    }
                    //bill pdf
                    int z = 0;
                    foreach (string URL in billurl)
                    {
                        driver.Navigate().GoToUrl(URL);
                        Thread.Sleep(3000);
                        gc.CreatePdf(orderNumber, outparcelno, "Tax bill " + z, driver, "NE", "Sarpy");
                        z++;
                    }

                    //tax sale delinq
                    if (delingurl != "")
                    {
                        driver.Navigate().GoToUrl(delingurl);
                        Thread.Sleep(3000);
                        gc.CreatePdf(orderNumber, outparcelno, "Tax sale delinq", driver, "NE", "Sarpy");
                    }

                    //special
                    if (specialurl != "")
                    {
                        driver.Navigate().GoToUrl(specialurl);
                        Thread.Sleep(3000);
                        gc.CreatePdf(orderNumber, outparcelno, "Special Assessment Details", driver, "NE", "Sarpy");
                    }
                    TaxTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.mergpdf(orderNumber, "NE", "Sarpy");
                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "NE", "Sarpy", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);
                    driver.Quit();
                    return "Data Inserted Successfully";
                }
                catch (Exception ex)
                {
                    driver.Quit();
                    GlobalClass.LogError(ex, orderNumber);
                    if (ex.ToString().Contains("The page size must be smaller than 14400 by 14400"))
                    {
                        HttpContext.Current.Session["SarpyNE_Page smaller_Error"] = "Yes";
                        return "";
                    }
                    else
                    {
                        throw ex;
                    }
                }
            }

        }
        private string multiparcel(string ordernumber)
        {
            //Select address from list....
            IWebElement multiaddrtableElement = driver.FindElement(By.XPath("//*[@id='ResultsGrid']/table/tbody/tr[1]/td[1]/table/tbody[2]"));
            IList<IWebElement> multiaddrtableRow = multiaddrtableElement.FindElements(By.TagName("tr"));
            int rescount = multiaddrtableRow.Count;
            rescount = rescount - 1;
            IList<IWebElement> multiaddrrowTD;
            int j = 0;
            if (rescount <= 25)
            {
                HttpContext.Current.Session["multiParcel_NESarpy"] = "Yes";
                foreach (IWebElement row in multiaddrtableRow)
                {
                    multiaddrrowTD = row.FindElements(By.TagName("td"));

                    if (multiaddrrowTD.Count != 0)
                    {
                        try
                        {
                            if (j > 0)
                            {
                                string multiparcel = multiaddrrowTD[3].Text;
                                multiparcel = GlobalClass.Before(multiparcel, "View Details").Trim();
                                string multiowner = multiaddrrowTD[4].Text + "~" + multiaddrrowTD[5].Text + "~" + multiaddrrowTD[6].Text;
                                gc.insert_date(ordernumber, multiparcel, 582, multiowner, 1, DateTime.Now);
                            }
                        }
                        catch
                        {
                        }
                        j++;
                    }
                }
            }

            else
            {
                HttpContext.Current.Session["multiParcel_NESarpyMaximum"] = "Maximum";
                return "Maximum";
            }
            driver.Quit();
            return "MultiParcel";
        }
    }
}


