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
using System.ComponentModel;

namespace ScrapMaricopa.Scrapsource
{
    public class Webdriver_OHFranklin
    {
        string Outparcelno = "-", outputPath = "", year = "";
        //PropertyDetails
        string strOwner1 = "-", strOwner2 = "-", strOwnerName = "-", strSitusAddress = "-", strLegalDescription = "-", strYearBuild = "-", strPropertyClass = "-", strLandUse = "", strTaxDistrict = "-",
               strSchoolDistrict = "-", strCityVillage = "-", strTownship = "-", strTaxLien = "-", strCAUV = "-", strOwnerCredit = "-", strHomesteadCredit = "-";
        // Market Value
        string strMarketYear = "-", strMarketBase = "-", strMarketTIF = "-", strMarketExempt = "-", strMarketTotal = "-", strMarketCAUV = "-", strMarketImproveBase = "-",
               strMarketImproveTIF = "-", strMarketImproveExempt = "-", strMarketImproveTotal = "-", strMarketImproveCAUV = "-", strMarketTotalBase = "-", strMarketTotalTIF = "-",
               strMarketTotalExempt = "-", strMarketTTotal = "-", strMarketTotalCAUV = "-";
        // Taxable Value
        string strTaxableYear = "-", strTaxableBase = "-", strTaxableTIF = "-", strTaxableExempt = "-", strTaxableTotal = "-", strTaxableImproveBase = "-",
               strTaxableImproveTIF = "-", strTaxableImproveExempt = "-", strTaxableImproveTotal = "-", strTaxableTotalBase = "-", strTaxableTotalTIF = "-", strTaxableTotalExempt = "-",
               strTaxableTTotal = "-";
        //Net value
        string strNetTaxYear = "-", strTotalNetAnnualTax = "-", strTaxPaid = "-";
        //MultiParcel
        string strMultiParcelID = "-", strMultiParcelAddress = "-", strMultiParcelOwner = "-", strmultiaddress = "", strmultiowner = "", strmultiownercount = "";
        //TAx Information
        string strCurrentOwner = "-", strLocationAddress = "-", strLandValue = "-",
               strImproveValue = "-", strTotalValue = "-", strTaxes = "-", strSpeicalAssessment = "-", strBalanceDue = "-", strLegalDesc = "-";
        //Tax Information First Half
        string strTaxInstall = "-", strInstallType = "-", strFirstHalfType = "-", strFirstHalfCharges = "-", strFirstHalfCredits = "-", strFirstHalfBalance = "-", strTaxInstallYear = "-";
        //Tax Information Second Half
        string strTaxSecondYear = "-", strSecondInstall = "-", strSecondHalfType = "-", strSecondHalfCharges = "-", strSecondHalfCredits = "-", strSecondHalfBalance = "-";
        //Distribution
        string strDisYear = "-", strPoliticalSub = "-", strADMH = "-", strChildService = "-", strGeneralFund = "-", strFCBDD = "-", strJoinVacaSchool = "-", strLibrary = "-", strParks = "-", strSchools = "-",
            strSeniorOption = "-", strDisTownship = "-", strZoo = "-", strTotal = "-", strPayYear = "-", strPaymentDate = "-", strPaymentTrans = "-", strPayment = "-", strSpecYear = "-", strspecial = "-", strspecialDesc = "-", strspecialAmount = "-",
            political_subDivision = "-", amount = "-";

        string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
        IWebDriver driver;
        IWebElement Itable;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        public string FTP_OHFranklin(string houseno, string sname, string sttype, string parcelNumber, string searchType, string orderNumber, string ownername, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            using (driver = new PhantomJSDriver())//
            {
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");
                    if (searchType == "titleflex")
                    {
                        string address = houseno + " " + sname + " " + sttype + " " + directParcel;
                        gc.TitleFlexSearch(orderNumber, parcelNumber, "", address, "OH", "Franklin");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_OHFranklin"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }
                    if (searchType == "address")
                    {
                        driver.Navigate().GoToUrl("http://property.franklincountyauditor.com/_web/search/commonsearch.aspx?mode=address");
                        driver.FindElement(By.Id("inpNumber")).SendKeys(houseno);
                        driver.FindElement(By.Id("inpStreet")).SendKeys(sname);
                        driver.FindElement(By.Id("btSearch")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "OHFranklinAddressSearch", driver, "OH", "Franklin");
                        try
                        {
                            strmultiaddress = driver.FindElement(By.XPath("//*[@id='frmMain']/table/tbody/tr/td/div/div/table[2]/tbody/tr/td[1]/table/tbody/tr[3]/td/center/table[1]/tbody/tr/td[3]")).Text;
                        }
                        catch { }
                        try
                        {
                            IWebElement Inodata = driver.FindElement(By.XPath("//*[@id='frmMain']/table/tbody/tr/td/div/div/table[2]"));
                            if (Inodata.Text.Contains("did not find any records"))
                            {
                                HttpContext.Current.Session["Nodata_OHFranklin"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                        if (!strmultiaddress.Contains("Displaying 1 - 1 of 1") && strmultiaddress != "")
                        {
                            gc.CreatePdf_WOP(orderNumber, "OHFranklinMultiAddress", driver, "OH", "Franklin");
                            mulriParcelSearch(orderNumber, parcelNumber);
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else
                        {
                            FraklinSearch(orderNumber, parcelNumber);
                        }
                    }
                    else if (searchType == "parcel")
                    {
                        driver.Navigate().GoToUrl("http://property.franklincountyauditor.com/_web/search/commonsearch.aspx?mode=parid");
                        if (HttpContext.Current.Session["titleparcel"] != null && HttpContext.Current.Session["titleparcel"].ToString() != "")
                        {
                            Outparcelno = HttpContext.Current.Session["titleparcel"].ToString();
                        }
                        else
                        {
                            string parcelsplit = parcelNumber.Replace("-", "").Replace(" ", "");
                            string parcelsplit1 = parcelsplit.Substring(0, 3);
                            string parcelsplit2 = parcelsplit.Substring(3, 6);
                            string parcelsplit3 = parcelsplit.Substring(9, 2);
                            Outparcelno = parcelsplit1 + "-" + parcelsplit2 + "-" + parcelsplit3;
                        }
                        driver.FindElement(By.Id("inpParid")).SendKeys(Outparcelno);
                        driver.FindElement(By.Id("btSearch")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, Outparcelno, "OHFranklinParcelSearch", driver, "OH", "Franklin");
                        try
                        {
                            IWebElement Inodata = driver.FindElement(By.XPath("//*[@id='frmMain']/table/tbody/tr/td/div/div/table[2]"));
                            if (Inodata.Text.Contains("did not find any records"))
                            {
                                HttpContext.Current.Session["Nodata_OHFranklin"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                        FraklinSearch(orderNumber, Outparcelno);

                    }
                    else if (searchType == "ownername")
                    {
                        driver.Navigate().GoToUrl("http://property.franklincountyauditor.com/_web/search/commonsearch.aspx?mode=owner");
                        driver.FindElement(By.Id("inpOwner")).SendKeys(ownername);
                        driver.FindElement(By.Id("btSearch")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "OHFranklinNameSearch", driver, "OH", "Franklin");
                        try
                        {
                            strmultiaddress = driver.FindElement(By.XPath("//*[@id='frmMain']/table/tbody/tr/td/div/div/table[2]/tbody/tr/td[1]/table/tbody/tr[3]/td/center/table[1]/tbody/tr/td[3]")).Text;
                        }
                        catch { }
                        try
                        {
                            IWebElement Inodata = driver.FindElement(By.XPath("//*[@id='frmMain']/table/tbody/tr/td/div/div/table[2]"));
                            if (Inodata.Text.Contains("did not find any records"))
                            {
                                HttpContext.Current.Session["Nodata_OHFranklin"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                        if (!strmultiaddress.Contains("Displaying 1 - 1 of 1") && strmultiaddress != "")
                        {
                            gc.CreatePdf_WOP(orderNumber, "OHFranklinMultiName", driver, "OH", "Franklin");
                            mulriParcelSearch(orderNumber, parcelNumber);
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else
                        {
                            FraklinSearch(orderNumber, Outparcelno);
                        }
                    }

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "OH", "Franklin", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);


                    driver.Quit();
                    gc.mergpdf(orderNumber, "OH", "Franklin");
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

        //MultiParcel Address/Owner Details
        public string mulriParcelSearch(string orderNumber, string parcelNumber)
        {
            HttpContext.Current.Session["multiparcel_OHFranklin"] = "Yes";
            int count = 3;
            strmultiownercount = WebDriverTest.Between(strmultiaddress, "Displaying 1 - ", " of");
            if (Convert.ToInt32(strmultiownercount) >= 25)
            {
                HttpContext.Current.Session["multiparcel_Maximum_OHFranklin"] = "Maximum";
                return "Maximum";
            }
            for (int i = 0; i < Convert.ToInt32(strmultiownercount); i++)
            {
                strMultiParcelID = driver.FindElement(By.XPath("//*[@id='searchResults']/tbody/tr[" + count + "]/td[3]/div")).Text;
                strMultiParcelAddress = driver.FindElement(By.XPath("//*[@id='searchResults']/tbody/tr[" + count + "]/td[4]/div")).Text;
                strMultiParcelOwner = driver.FindElement(By.XPath("//*[@id='searchResults']/tbody/tr[" + count + "]/td[5]/div")).Text;
                string strMultiparcel = strMultiParcelOwner + "~" + strMultiParcelAddress;
                gc.insert_date(orderNumber, strMultiParcelID, 63, strMultiparcel, 1, DateTime.Now);
                count++;
            }
            driver.Quit();
            return "MultiParcel";

        }

        //Property and Assessment
        public void FraklinSearch(string orderNumber, string Outparcelno)
        {
            string parcelsplit = "-", strDesc1 = "", strDesc2 = "", strDesc3 = "", strMYear = "", strTYear = "", strTaxYear = "", strDescTax1 = "", strDescTax2 = "", strDescTax3 = "";
            gc.CreatePdf(orderNumber, Outparcelno, "OHFranklinProperty", driver, "OH", "Franklin");

            parcelsplit = driver.FindElement(By.XPath("//*[@id='datalet_header_row']/td/table/tbody/tr[1]/td[1]")).Text;
            Outparcelno = WebDriverTest.After(parcelsplit, "ParcelID: ");
            string fulltext = driver.FindElement(By.XPath("//*[@id='Owner']")).Text;
            try
            {
                strOwner1 = gc.Between(fulltext, "Owner", "Owner Address").Trim();
            }
            catch
            { }
            strSitusAddress = gc.Between(fulltext, "Owner Address", "Legal Description").Trim();
            strLegalDescription = gc.Between(fulltext, "Legal Description", "Calculated Acres").Trim();

            //Year Built
            try
            {
                IWebElement inst1table = driver.FindElement(By.XPath("//*[@id='Dwelling Data']/tbody"));
                IList<IWebElement> inst1tableRow = inst1table.FindElements(By.TagName("tr"));
                int inst1tableRowcount = inst1tableRow.Count;
                IList<IWebElement> inst1rowTD;
                int z = 0;
                foreach (IWebElement rowid in inst1tableRow)
                {

                    inst1rowTD = rowid.FindElements(By.TagName("td"));
                    if (inst1rowTD.Count != 0 && !rowid.Text.Contains("Yr Built") && z == 0)
                    {
                        strYearBuild = inst1rowTD[0].Text;
                        z++;
                    }

                }
            }
            catch { }
            //strYearBuild = driver.FindElement(By.XPath("//*[@id='Dwelling Data']/tbody/tr[2]/td[1]")).Text;
            //if (strOwnerName == " ")
            //{
            //    strOwnerName = "-";
            //}

            //strOwnerName = strOwner1 + " " + strOwner2;
            //strSitusAddress = driver.FindElement(By.XPath("/html/body/div/div[3]/section/div/form/div[3]/div/div/table/tbody/tr/td/table/tbody/tr[2]/td/div/div[1]/table[2]/tbody/tr[3]/td[2]")).Text;
            //if (strSitusAddress == " ")
            //{
            //    strSitusAddress = "-";
            //}
            //strDesc1 = driver.FindElement(By.XPath("/html/body/div/div[3]/section/div/form/div[3]/div/div/table/tbody/tr/td/table/tbody/tr[2]/td/div/div[1]/table[2]/tbody/tr[6]/td[2]")).Text;
            //if (strDesc1 == " ")
            //{
            //    strDesc1 = "-";
            //}
            //strDesc2 = driver.FindElement(By.XPath("/html/body/div/div[3]/section/div/form/div[3]/div/div/table/tbody/tr/td/table/tbody/tr[2]/td/div/div[1]/table[2]/tbody/tr[7]/td[2]")).Text;
            //if (strDesc2 == " ")
            //{
            //    strDesc2 = "-";
            //}
            //strDesc3 = driver.FindElement(By.XPath("/html/body/div/div[3]/section/div/form/div[3]/div/div/table/tbody/tr/td/table/tbody/tr[2]/td/div/div[1]/table[2]/tbody/tr[8]/td[2]")).Text;
            //if (strDesc3 == " ")
            //{
            //    strDesc3 = "-";
            //}
            //strLegalDescription = strDesc1 + strDesc2 + strDesc3;
            //if (strLegalDescription == " ")
            //{
            //    strLegalDescription = "-";
            //}
            //try
            //{
            //    strYearBuild = driver.FindElement(By.XPath("/html/body/div/div[3]/section/div/form/div[3]/div/div/table/tbody/tr/td/table/tbody/tr[2]/td/div/div[7]/table[2]/tbody/tr[2]/td[1]")).Text;
            //    if (strYearBuild == " ")
            //    {
            //        strYearBuild = "-";
            //    }
            //}
            //catch { }


            //Tax Status
            string fulltaxstatus = driver.FindElement(By.XPath("//*[@id='2018 Tax Status']/tbody")).Text;
            strPropertyClass = gc.Between(fulltaxstatus, "Property Class", "Land Use").Trim();
            strLandUse = gc.Between(fulltaxstatus, "Land Use", "Tax District").Trim();
            strTaxDistrict = gc.Between(fulltaxstatus, "Tax District", "School District").Trim();
            strSchoolDistrict = gc.Between(fulltaxstatus, "School District", "City/Village").Trim();
            strCityVillage = gc.Between(fulltaxstatus, "City/Village", "Township").Trim();
            strTownship = gc.Between(fulltaxstatus, "Township", "Appraisal Neighborhood").Trim();
            strTaxLien = gc.Between(fulltaxstatus, "Tax Lien", "CAUV Property").Trim();
            strCAUV = gc.Between(fulltaxstatus, "CAUV Property", "Owner Occ. Credit").Trim();
            strOwnerCredit = gc.Between(fulltaxstatus, "Owner Occ. Credit", "Homestead Credit").Trim();
            strHomesteadCredit = gc.Between(fulltaxstatus, "Homestead Credit", "Rental Registration").Trim();

            string strTaxStatus = strOwnerName + "~" + strSitusAddress + "~" + strLegalDescription + "~" + strYearBuild + "~" + strPropertyClass + "~" + strLandUse + "~" + strTaxDistrict + "~" + strSchoolDistrict + "~" + strCityVillage + "~" + strTownship + "~" + strTaxLien + "~" + strCAUV + "~" + strOwnerCredit + "~" + strHomesteadCredit;
            gc.insert_date(orderNumber, Outparcelno, 62, strTaxStatus, 1, DateTime.Now);

            //Assessment Market Value
            strMYear = driver.FindElement(By.XPath("//*[@id='datalet_div_5']/table[1]/tbody/tr/td")).Text;
            strMarketYear = strMYear.Substring(0, 4);
            //h4/a[contains(text(),'SAP M')]
            //Xpath =//*[contains(text(),'Current Market Value')]
            IWebElement asstable = driver.FindElement(By.XPath("//*[@id='" + strMarketYear + " Current Market Value']"));
            IList<IWebElement> asstableRow = asstable.FindElements(By.TagName("tr"));
            int asstableRowCount = asstableRow.Count;
            IList<IWebElement> asstableRowTD;
            int y = 0;
            foreach (IWebElement rowid in asstableRow)
            {

                asstableRowTD = rowid.FindElements(By.TagName("td"));
                if (asstableRowTD.Count != 0 && !rowid.Text.Contains("Land"))
                {
                    if (y == 1)
                    {
                        strMarketBase = asstableRowTD[1].Text;
                        strMarketImproveBase = asstableRowTD[2].Text;
                        strMarketTotalBase = asstableRowTD[3].Text;
                    }
                    if (y == 2)
                    {
                        strMarketTIF = asstableRowTD[1].Text;
                        strMarketImproveTIF = asstableRowTD[2].Text;
                        strMarketTotalTIF = asstableRowTD[3].Text;
                    }
                    if (y == 3)
                    {
                        strMarketExempt = asstableRowTD[1].Text;
                        strMarketImproveExempt = asstableRowTD[2].Text;
                        strMarketTotalExempt = asstableRowTD[3].Text;

                    }
                    if (y == 4)
                    {
                        strMarketTotal = asstableRowTD[1].Text;
                        strMarketImproveTotal = asstableRowTD[2].Text;
                        strMarketTTotal = asstableRowTD[3].Text;
                    }
                    if (y == 5)
                    {
                        strMarketCAUV = asstableRowTD[1].Text;
                        strMarketImproveCAUV = asstableRowTD[2].Text;
                        strMarketTotalCAUV = asstableRowTD[3].Text;
                    }

                }

                y++;

            }


            //Taxable Value
            strTYear = driver.FindElement(By.XPath("//*[@id='datalet_div_6']/table[1]/tbody/tr/td")).Text;
            strTaxableYear = strTYear.Substring(0, 4);
            //*[@id="2018 Taxable Value"]/tbody
            IWebElement asstable1 = driver.FindElement(By.XPath("//*[@id='" + strTaxableYear + " Taxable Value']"));
            IList<IWebElement> asstableRow1 = asstable1.FindElements(By.TagName("tr"));
            int asstableRowCount1 = asstableRow1.Count;
            IList<IWebElement> asstableRowTD1;
            int x = 0;
            foreach (IWebElement rowid in asstableRow1)
            {

                asstableRowTD1 = rowid.FindElements(By.TagName("td"));
                if (asstableRowTD1.Count != 0 && !rowid.Text.Contains("Land"))
                {
                    if (x == 1)
                    {
                        strTaxableBase = asstableRowTD1[1].Text;
                        strTaxableImproveBase = asstableRowTD1[2].Text;
                        strTaxableTotalBase = asstableRowTD1[3].Text;
                    }
                    if (x == 2)
                    {
                        strTaxableTIF = asstableRowTD1[1].Text;
                        strTaxableImproveTIF = asstableRowTD1[2].Text;
                        strTaxableTotalTIF = asstableRowTD1[3].Text;
                    }
                    if (x == 3)
                    {
                        strTaxableExempt = asstableRowTD1[1].Text;
                        strTaxableImproveExempt = asstableRowTD1[2].Text;
                        strTaxableTotalExempt = asstableRowTD1[3].Text;

                    }
                    if (x == 4)
                    {
                        strTaxableTotal = asstableRowTD1[1].Text;
                        strTaxableImproveTotal = asstableRowTD1[2].Text;
                        strTaxableTTotal = asstableRowTD1[3].Text;
                    }

                }

                x++;

            }


            //strTaxYear = driver.FindElement(By.XPath("//*[@id='datalet_div_6']/table[1]/tbody/tr/td")).Text;
            ////*[@id="2018 Taxes"]         
            //strNetTaxYear = strTaxYear.Substring(0, 4);
            //*[@id="2018 Taxes"]/tbody
            IWebElement asstable2 = driver.FindElement(By.XPath("//*[@id='" + strTaxableYear + " Taxes']"));
            IList<IWebElement> asstableRow2 = asstable2.FindElements(By.TagName("tr"));
            int asstableRowCount2 = asstableRow2.Count;
            IList<IWebElement> asstableRowTD2;
            int t = 0;
            foreach (IWebElement rowid in asstableRow2)
            {

                asstableRowTD2 = rowid.FindElements(By.TagName("td"));
                if (asstableRowTD2.Count != 0 && !rowid.Text.Contains("Net Annual Tax"))
                {
                    if (t == 1)
                    {
                        strTotalNetAnnualTax = asstableRowTD2[0].Text;
                        strTaxPaid = asstableRowTD2[1].Text;
                    }
                }

                t++;

            }

            //MarketValue 
            string str1 = "Base" + "~" + strMarketBase + "~" + strMarketImproveBase + "~" + strMarketTotalBase + "~" + "MarKet" + "~" + strMYear + "~" + "-" + "~" + "-";
            string str2 = "TIF" + "~" + strMarketTIF + "~" + strMarketImproveTIF + "~" + strMarketTotalTIF + "~" + "MarKet" + "~" + strMYear + "~" + "-" + "~" + "-";
            string str3 = "Exempt" + "~" + strMarketExempt + "~" + strMarketImproveTotal + "~" + strMarketTotalExempt + "~" + "MarKet" + "~" + strMYear + "~" + "-" + "~" + "-";
            string str4 = "Total" + "~" + strMarketTotal + "~" + strMarketImproveTotal + "~" + strMarketTTotal + "~" + "MarKet" + "~" + strMYear + "~" + "-" + "~" + "-";
            string str5 = "CAUV" + "~" + strMarketCAUV + "~" + strMarketImproveCAUV + "~" + strMarketTotalCAUV + "~" + "MarKet" + "~" + strMYear + "~" + "-" + "~" + "-";
            //Taxable value
            string str6 = "Base" + "~" + strTaxableBase + "~" + strTaxableImproveBase + "~" + strTaxableTotalBase + "~" + "Taxable" + "~" + strTYear + "~" + "-" + "~" + "-";
            string str7 = "TIF" + "~" + strTaxableTIF + "~" + strTaxableImproveTIF + "~" + strTaxableTotalExempt + "~" + "Taxable" + "~" + strTYear + "~" + "-" + "~" + "-";
            string str8 = "Exempt" + "~" + strTaxableExempt + "~" + strTaxableImproveExempt + "~" + strTaxableTotalExempt + "~" + "Taxable" + "~" + strTYear + "~" + "-" + "~" + "-";
            string str9 = "Total" + "~" + strTaxableTotal + "~" + strTaxableImproveTotal + "~" + strTaxableTTotal + "~" + "Taxable" + "~" + strTYear + "~" + "-" + "~" + "-";
            //NetValue
            string str10 = "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + strNetTaxYear + "~" + strTotalNetAnnualTax + "~" + strTaxPaid;

            IList<string> strAssess = new List<string>();
            strAssess.Add(str1);
            strAssess.Add(str2);
            strAssess.Add(str3);
            strAssess.Add(str4);
            strAssess.Add(str5);
            strAssess.Add(str6);
            strAssess.Add(str7);
            strAssess.Add(str8);
            strAssess.Add(str9);
            strAssess.Add(str10);

            foreach (string strAss in strAssess)
            {
                gc.insert_date(orderNumber, Outparcelno, 65, strAss, 1, DateTime.Now);
            }


            AssessmentTime = DateTime.Now.ToString("HH:mm:ss");

            try
            {
                IWebElement url = driver.FindElement(By.XPath("datalet_div_1"));
                string URL = url.GetAttribute("href");
                gc.downloadfile(URL, orderNumber, Outparcelno, "OHFranklinOriginalTaxBill.pdf", "OH", "Franklin");
            }
            catch (Exception E) { }

            //Tax Information
            driver.Navigate().GoToUrl("http://treapropsearch.franklincountyohio.gov/Default.aspx");
            IWebElement IradioSearch = driver.FindElement(By.Id("ctl00_cphBodyContent_sbPropertySearch_rbSearchByParcelID"));
            IJavaScriptExecutor js1 = driver as IJavaScriptExecutor;
            js1.ExecuteScript("arguments[0].click();", IradioSearch);
            Thread.Sleep(2000);
            js1.ExecuteScript("document.getElementById('ctl00_cphBodyContent_sbPropertySearch_tbSearch').value='" + Outparcelno + "'");
            gc.CreatePdf(orderNumber, Outparcelno, "OHFranklinTaxSearch", driver, "OH", "Franklin");
            driver.FindElement(By.Id("ctl00_cphBodyContent_sbPropertySearch_btnSearch")).SendKeys(Keys.Enter);
            gc.CreatePdf(orderNumber, Outparcelno, "OHFranklinTaxDetails", driver, "OH", "Franklin");
            strCurrentOwner = driver.FindElement(By.Id("ctl00_cphBodyContent_lblOwn1_1")).Text;
            strLocationAddress = driver.FindElement(By.Id("ctl00_cphBodyContent_fcDetailsHeader_LocationAddressLine1")).Text;
            strLandValue = driver.FindElement(By.Id("ctl00_cphBodyContent_fcDetailsHeader_lblLand")).Text;
            strImproveValue = driver.FindElement(By.Id("ctl00_cphBodyContent_fcDetailsHeader_lblImprovement")).Text;
            strTotalValue = driver.FindElement(By.Id("ctl00_cphBodyContent_fcDetailsHeader_lblTotal")).Text;
            strTaxes = driver.FindElement(By.Id("ctl00_cphBodyContent_fcDetailsHeader_Taxes")).Text;
            strSpeicalAssessment = driver.FindElement(By.Id("ctl00_cphBodyContent_fcDetailsHeader_SpecialAssessment")).Text;
            strBalanceDue = driver.FindElement(By.Id("ctl00_cphBodyContent_fcDetailsHeader_BalanceDue")).Text;
            strDescTax1 = driver.FindElement(By.Id("ctl00_cphBodyContent_fcDetailsHeader_lblLegal1")).Text;
            strDescTax2 = driver.FindElement(By.Id("ctl00_cphBodyContent_fcDetailsHeader_lblLegal2")).Text;
            strDescTax3 = driver.FindElement(By.Id("ctl00_cphBodyContent_fcDetailsHeader_lblLegal3")).Text;
            strLegalDesc = strDescTax1 + strDescTax2 + strDescTax3;

            string strTaxAssessment = strCurrentOwner + "~" + strLocationAddress + "~" + strLandValue + "~" + strImproveValue + "~" + strTotalValue + "~" + strTaxes + "~" + strSpeicalAssessment + "~" + strBalanceDue + "~" + strLegalDesc;
            gc.insert_date(orderNumber, Outparcelno, 67, strTaxAssessment, 1, DateTime.Now);

            try
            {
                IWebElement Iurl = driver.FindElement(By.Id("ctl00_cphBodyContent_fcDetailsHeader_TaxBillLink1Half"));
                string strURL = Iurl.GetAttribute("href");
                gc.downloadfile(strURL, orderNumber, Outparcelno, "OHFranklinOriginalTaxBill.pdf", "OH", "Franklin");
            }
            catch { }

            driver.FindElement(By.XPath("//*[@id='propertyHeaderList']/ul/li[2]/a")).SendKeys(Keys.Enter);
            Thread.Sleep(2000);
            for (int i = 2; i <= 5; i++)
            {
                IWebElement Iaccount = driver.FindElement(By.XPath("//*[@id='tabs-1']/div[2]/div[1]/ul"));
                IList<IWebElement> IaccountRow = Iaccount.FindElements(By.TagName("li"));
                if (IaccountRow.Count != 0 && IaccountRow.Count == 5 && i == 2)
                {
                    i++;
                }
                if (IaccountRow.Count != 0 && IaccountRow.Count == 4 && i == 5)
                {
                    break;
                }
                IWebElement Itaxinstall = driver.FindElement(By.XPath("//*[@id='tabs-1']/div[2]/div[1]/ul/li[" + i + "]/a"));
                strTaxInstall = Itaxinstall.Text;
                js1.ExecuteScript("arguments[0].click();", Itaxinstall);
                Thread.Sleep(2000);
                gc.CreatePdf(orderNumber, Outparcelno, "OHFranklin" + strTaxInstall + "TaxDetails", driver, "OH", "Franklin");
                strInstallType = driver.FindElement(By.XPath("//*[@id='firstHalf']/table/thead/tr/th[1]")).Text;
                if (strInstallType == "")
                {
                    strInstallType = "-";
                }
                strTaxInstallYear = strTaxInstall + "~" + strInstallType;
                //Tax First Half
                IWebElement IFirstHalfTable = driver.FindElement(By.XPath("//*[@id='firstHalf']/table"));
                IList<IWebElement> IFirstHalfRow = IFirstHalfTable.FindElements(By.TagName("tr"));
                IList<IWebElement> IFirstHalfTD;
                IList<IWebElement> IFirstHalfTH;
                foreach (IWebElement first in IFirstHalfRow)
                {
                    IFirstHalfTD = first.FindElements(By.TagName("td"));
                    if (IFirstHalfTD.Count != 0 && first.Text.Trim() != "" && !first.Text.Contains("Prior Taxes") && !first.Text.Contains("1st Half Taxes") && !first.Text.Contains("Special Assessments"))
                    {
                        try
                        {
                            strFirstHalfType = IFirstHalfTD[0].Text;
                            strFirstHalfCharges = IFirstHalfTD[1].Text;
                            strFirstHalfCredits = IFirstHalfTD[2].Text;
                            strFirstHalfBalance = IFirstHalfTD[3].Text;
                        }
                        catch { }

                        string strFirstHalfDetails = strTaxInstallYear + "~" + strFirstHalfType + "~" + strFirstHalfCharges + "~" + strFirstHalfCredits + "~" + strFirstHalfBalance;
                        gc.insert_date(orderNumber, Outparcelno, 68, strFirstHalfDetails, 1, DateTime.Now);

                    }
                    IFirstHalfTH = first.FindElements(By.TagName("th"));
                    if (IFirstHalfTH.Count != 0 && first.Text.Trim() != "" && first.Text.Contains("1st Half Total"))
                    {
                        try
                        {
                            strSecondHalfType = IFirstHalfTH[0].Text;
                            strSecondHalfCharges = IFirstHalfTH[1].Text;
                            strSecondHalfCredits = IFirstHalfTH[2].Text;
                            strSecondHalfBalance = IFirstHalfTH[3].Text;
                        }
                        catch { }

                        string strFirstHalfDetails = strTaxInstallYear + "~" + strFirstHalfType + "~" + strFirstHalfCharges + "~" + strFirstHalfCredits + "~" + strFirstHalfBalance;
                        gc.insert_date(orderNumber, Outparcelno, 68, strFirstHalfDetails, 1, DateTime.Now);
                    }
                }


                strSecondInstall = driver.FindElement(By.XPath("//*[@id='secondHalf']/table/thead/tr/th[1]")).Text;
                if (strSecondInstall == "")
                {
                    strSecondInstall = "-";
                }
                strTaxSecondYear = strTaxInstall + "~" + strSecondInstall;
                //Tax Second Half
                IWebElement ISecondHalfTable = driver.FindElement(By.XPath("//*[@id='secondHalf']/table"));
                IList<IWebElement> ISecondHalfRow = ISecondHalfTable.FindElements(By.TagName("tr"));
                IList<IWebElement> ISecondHalfTD;
                IList<IWebElement> ISecondHalfTH;
                foreach (IWebElement second in ISecondHalfRow)
                {
                    ISecondHalfTD = second.FindElements(By.TagName("td"));
                    if (ISecondHalfTD.Count != 0 && second.Text.Trim() != "" && !second.Text.Contains("Future Charges") && !second.Text.Contains("2nd Half Taxes") && !second.Text.Contains("Special Assessments"))
                    {
                        try
                        {
                            strSecondHalfType = ISecondHalfTD[0].Text;
                            strSecondHalfCharges = ISecondHalfTD[1].Text;
                            strSecondHalfCredits = ISecondHalfTD[2].Text;
                            strSecondHalfBalance = ISecondHalfTD[3].Text;
                        }
                        catch { }

                        string strSecondHalfDetails = strTaxSecondYear + "~" + strSecondHalfType + "~" + strSecondHalfCharges + "~" + strSecondHalfCredits + "~" + strSecondHalfBalance;
                        gc.insert_date(orderNumber, Outparcelno, 68, strSecondHalfDetails, 1, DateTime.Now);
                    }
                    ISecondHalfTH = second.FindElements(By.TagName("th"));
                    if (ISecondHalfTH.Count != 0 && second.Text.Trim() != "" && (second.Text.Contains("Scheduled to Apply") || second.Text.Contains("Full Year")))
                    {
                        try
                        {
                            strSecondHalfType = ISecondHalfTH[0].Text;
                            strSecondHalfCharges = ISecondHalfTH[1].Text;
                            strSecondHalfCredits = ISecondHalfTH[2].Text;
                            strSecondHalfBalance = ISecondHalfTH[3].Text;
                        }
                        catch { }

                        string strSecondHalfDetails = strTaxSecondYear + "~" + strSecondHalfType + "~" + strSecondHalfCharges + "~" + strSecondHalfCredits + "~" + strSecondHalfBalance;
                        gc.insert_date(orderNumber, Outparcelno, 68, strSecondHalfDetails, 1, DateTime.Now);
                    }
                }
            }

            //Valuation Details
            driver.FindElement(By.XPath("//*[@id='propertyHeaderList']/ul/li[3]/a")).SendKeys(Keys.Enter);
            for (int v = 2; v < 6; v++)
            {
                IWebElement Ivaluation = driver.FindElement(By.XPath("//*[@id='tabs-2']/div[2]/div[1]/ul"));
                IList<IWebElement> IvaluationRow = Ivaluation.FindElements(By.TagName("li"));
                if (IvaluationRow.Count != 0 && IvaluationRow.Count == 5 && v == 2)
                {
                    v++;
                }
                if (IvaluationRow.Count != 0 && IvaluationRow.Count == 4 && v == 5)
                {
                    break;
                }
                IWebElement Iyear = driver.FindElement(By.XPath("//*[@id='tabs-4']/div[2]/div[1]/ul/li[" + v + "]/a"));
                strPayYear = Iyear.Text;
                js1.ExecuteScript("arguments[0].click();", Iyear);
                Thread.Sleep(2000);
                gc.CreatePdf(orderNumber, Outparcelno, "OHFranklin" + strPayYear + "ValuationDetails", driver, "OH", "Franklin");
            }

            //Payments Details
            driver.FindElement(By.XPath("//*[@id='propertyHeaderList']/ul/li[4]/a")).SendKeys(Keys.Enter);
            Thread.Sleep(2000);
            gc.CreatePdf(orderNumber, Outparcelno, "OHFranklinPaymentDetails", driver, "OH", "Franklin");
            try
            {
                for (int p = 2; p < 6; p++)
                {
                    IWebElement Ipayment = driver.FindElement(By.XPath("//*[@id='tabs-3']/div[2]/div[1]/ul"));
                    IList<IWebElement> IpaymentRow = Ipayment.FindElements(By.TagName("li"));
                    if (IpaymentRow.Count != 0 && IpaymentRow.Count == 5 && p == 2)
                    {
                        p++;
                    }
                    if (IpaymentRow.Count != 0 && IpaymentRow.Count == 4 && p == 5)
                    {
                        break;
                    }
                    IWebElement Iyear = driver.FindElement(By.XPath("//*[@id='tabs-3']/div[2]/div[1]/ul/li[" + p + "]/a"));
                    strPayYear = Iyear.Text;
                    js1.ExecuteScript("arguments[0].click();", Iyear);
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, Outparcelno, "OHFranklin" + strPayYear + "PaymentDetails", driver, "OH", "Franklin");
                    IWebElement Itable = driver.FindElement(By.XPath("//*[@id='tabs-3']/div[2]"));
                    IList<IWebElement> Itrow = Itable.FindElements(By.TagName("tr"));
                    IList<IWebElement> ItableTd;
                    foreach (IWebElement taxrows in Itrow)//*[@id="current"]/div/table
                    {
                        ItableTd = taxrows.FindElements(By.TagName("td"));
                        if (ItableTd.Count != 0 && !ItableTd[0].Text.Contains("Sorry, no Special Assessment data available for " + strPayYear + ".") && taxrows.Text.Trim() != "")
                        {
                            strPaymentDate = ItableTd[0].Text;
                            strPaymentTrans = ItableTd[1].Text;
                            strPayment = strPaymentDate + "~" + strPaymentTrans;
                            gc.insert_date(orderNumber, Outparcelno, 69, strPayment, 1, DateTime.Now);
                        }
                    }
                }
            }
            catch { }
            try
            {
                //Distribution Details//*[@id="ctl00_cphBodyContent_fcTaxDistributionContainer_ctl12_TaxDistributionPanel"]/table
                driver.FindElement(By.XPath("//*[@id='propertyHeaderList']/ul/li[5]/a")).SendKeys(Keys.Enter);
                for (int d = 2; d < 6; d++)
                {
                    IWebElement Idistribution = driver.FindElement(By.XPath("//*[@id='tabs-4']/div[2]/div[1]/ul"));
                    IList<IWebElement> IdistributionRow = Idistribution.FindElements(By.TagName("li"));
                    if (IdistributionRow.Count != 0 && IdistributionRow.Count == 5 && d == 2)
                    {
                        d++;
                    }
                    if (IdistributionRow.Count != 0 && IdistributionRow.Count == 4 && d == 5)
                    {
                        break;
                    }
                    IWebElement IDisYear = driver.FindElement(By.XPath("//*[@id='tabs-4']/div[2]/div[1]/ul/li[" + d + "]/a"));
                    strDisYear = IDisYear.Text;
                    js1.ExecuteScript("arguments[0].click();", IDisYear);
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, Outparcelno, "OHFranklinDistributionDetails" + d, driver, "OH", "Franklin");
                    IWebElement TbDistribuiton = driver.FindElement(By.XPath("//*[@id='tabs-4']"));
                    IList <IWebElement> TrDistribuiton = TbDistribuiton.FindElements(By.TagName("tr"));
                    IList<IWebElement> TdDistribuiton;
                    foreach (IWebElement rows in TrDistribuiton)
                    {
                        TdDistribuiton = rows.FindElements(By.TagName("td"));
                        if (TdDistribuiton.Count != 0 && !rows.Text.Contains("Political Subdivision"))
                        {
                            political_subDivision = TdDistribuiton[0].Text;
                            amount = TdDistribuiton[1].Text;
                            string strDistribution = strDisYear + "~" + political_subDivision + "~" + amount;
                            gc.insert_date(orderNumber, Outparcelno, 70, strDistribution, 1, DateTime.Now);
                        }
                    }
                }
            }
            catch { }
            //Special Assessment
            driver.FindElement(By.XPath("//*[@id='propertyHeaderList']/ul/li[6]/a")).SendKeys(Keys.Enter);
            gc.CreatePdf(orderNumber, Outparcelno, "OHFranklinSpecialAssessmentDetails", driver, "OH", "Franklin");
            try
            {
                string strtaxYear = DateTime.Now.Year.ToString();
                int taxYear = 0;
                for (int s = 2; s < 6; s++)
                {
                    IWebElement ISpecial = driver.FindElement(By.XPath("//*[@id='tabs-5']/div[2]/div[1]/ul"));
                    IList<IWebElement> ISpecialRow = ISpecial.FindElements(By.TagName("li"));
                    if (ISpecialRow.Count != 0 && ISpecialRow.Count == 5 && s == 2)
                    {
                        s++;
                    }
                    if (ISpecialRow.Count != 0 && ISpecialRow.Count == 4 && s == 5)
                    {
                        break;
                    }
                    IWebElement Iyear = driver.FindElement(By.XPath("//*[@id='tabs-5']/div[2]/div[1]/ul/li[" + s + "]/a"));
                    strSpecYear = Iyear.Text;
                    js1.ExecuteScript("arguments[0].click();", Iyear);
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, Outparcelno, "OHFranklin" + strSpecYear + "SpecialAssessmentDetails", driver, "OH", "Franklin");
                    if (strSpecYear.Contains(strtaxYear) || strSpecYear.Contains(strtaxYear) || strSpecYear.Contains(strtaxYear))
                    {
                        try
                        {
                            Itable = driver.FindElement(By.XPath("//*[@id='current']/div/table/tbody"));
                        }
                        catch { }
                        try
                        {
                            Itable = driver.FindElement(By.XPath("//*[@id='ctl00_cphBodyContent_fcSpecialAssessmentContainer_ctl12_SpecialAssessmentNoData']/tbody"));
                        }
                        catch { }
                        try
                        {
                            Itable = driver.FindElement(By.XPath("//*[@id='tabs-5']"));
                        }
                        catch { }
                        IList<IWebElement> Itrow = Itable.FindElements(By.TagName("tr"));
                        IList<IWebElement> ItableTd;
                        foreach (IWebElement taxrows in Itrow)
                        {
                            ItableTd = taxrows.FindElements(By.TagName("td"));
                            if (ItableTd.Count != 0 && !ItableTd[0].Text.Contains("Sorry, no Special Assessment data available for " + strSpecYear + "."))
                            {
                                strspecialDesc = ItableTd[0].Text;
                                strspecialAmount = ItableTd[1].Text;
                                strspecial = strSpecYear + "~" + strspecialDesc + "~" + strspecialAmount;
                                gc.insert_date(orderNumber, Outparcelno, 71, strspecial, 1, DateTime.Now);
                            }
                        }
                    }
                    taxYear = Convert.ToInt32(strtaxYear);
                    taxYear--;
                    strtaxYear = Convert.ToString(taxYear);
                }
            }
            catch (Exception e) { }

            TaxTime = DateTime.Now.ToString("HH:mm:ss");
            driver.Quit();
        }
    }
}

////Tax Information
//driver.Navigate().GoToUrl("http://treapropsearch.franklincountyohio.gov/Default.aspx");
//IWebElement IradioSearch = driver.FindElement(By.Id("ctl00_cphBodyContent_sbPropertySearch_rbSearchByParcelID"));
//IJavaScriptExecutor js1 = driver as IJavaScriptExecutor;
//js1.ExecuteScript("arguments[0].click();", IradioSearch);
//            Thread.Sleep(2000);
//            js1.ExecuteScript("document.getElementById('ctl00_cphBodyContent_sbPropertySearch_tbSearch').value='" + Outparcelno + "'");
//            gc.CreatePdf(orderNumber, Outparcelno, "OHFranklinTaxSearch", driver, "OH", "Franklin");
//            driver.FindElement(By.Id("ctl00_cphBodyContent_sbPropertySearch_btnSearch")).SendKeys(Keys.Enter);
//gc.CreatePdf(orderNumber, Outparcelno, "OHFranklinTaxDetails", driver, "OH", "Franklin");
//            strCurrentOwner = driver.FindElement(By.Id("ctl00_cphBodyContent_lblOwn1_1")).Text;
//            strLocationAddress = driver.FindElement(By.Id("ctl00_cphBodyContent_fcDetailsHeader_LocationAddressLine1")).Text;
//            strLandValue = driver.FindElement(By.Id("ctl00_cphBodyContent_fcDetailsHeader_lblLand")).Text;
//            strImproveValue = driver.FindElement(By.Id("ctl00_cphBodyContent_fcDetailsHeader_lblImprovement")).Text;
//            strTotalValue = driver.FindElement(By.Id("ctl00_cphBodyContent_fcDetailsHeader_lblTotal")).Text;
//            strTaxes = driver.FindElement(By.Id("ctl00_cphBodyContent_fcDetailsHeader_Taxes")).Text;
//            strSpeicalAssessment = driver.FindElement(By.Id("ctl00_cphBodyContent_fcDetailsHeader_SpecialAssessment")).Text;
//            strBalanceDue = driver.FindElement(By.Id("ctl00_cphBodyContent_fcDetailsHeader_BalanceDue")).Text;
//            strDescTax1 = driver.FindElement(By.Id("ctl00_cphBodyContent_fcDetailsHeader_lblLegal1")).Text;
//            strDescTax2 = driver.FindElement(By.Id("ctl00_cphBodyContent_fcDetailsHeader_lblLegal2")).Text;
//            strDescTax3 = driver.FindElement(By.Id("ctl00_cphBodyContent_fcDetailsHeader_lblLegal3")).Text;
//            strLegalDesc = strDescTax1 + strDescTax2 + strDescTax3;

//            string strTaxAssessment = strCurrentOwner + "~" + strLocationAddress + "~" + strLandValue + "~" + strImproveValue + "~" + strTotalValue + "~" + strTaxes + "~" + strSpeicalAssessment + "~" + strBalanceDue + "~" + strLegalDesc;
//gc.insert_date(orderNumber, Outparcelno, 67, strTaxAssessment, 1, DateTime.Now);

//            try
//            {
//                IWebElement Iurl = driver.FindElement(By.Id("ctl00_cphBodyContent_fcDetailsHeader_TaxBillLink1Half"));
//string strURL = Iurl.GetAttribute("href");
//gc.downloadfile(strURL, orderNumber, Outparcelno, "OHFranklinOriginalTaxBill.pdf", "OH", "Franklin");
//            }
//            catch { }

//            driver.FindElement(By.XPath("//*[@id='propertyHeaderList']/ul/li[2]/a")).SendKeys(Keys.Enter);
//Thread.Sleep(2000);
//            for (int i = 2; i< 5; i++)
//            {
//                IWebElement Itaxinstall = driver.FindElement(By.XPath("//*[@id='tabs-2']/div[2]/div[1]/ul/li[" + i + "]/a"));
//strTaxInstall = Itaxinstall.Text;
//                js1.ExecuteScript("arguments[0].click();", Itaxinstall);
//                Thread.Sleep(2000);
//                gc.CreatePdf(orderNumber, Outparcelno, "OHFranklin" + strTaxInstall + "TaxDetails", driver, "OH", "Franklin");
//                strInstallType = driver.FindElement(By.XPath("//*[@id='firstHalf']/table/thead/tr/th[1]")).Text;
//                if (strInstallType == "")
//                {
//                    strInstallType = "-";
//                }
//                strTaxInstallYear = strTaxInstall + "~" + strInstallType;
//                //Tax First Half
//                IWebElement IFirstHalfTable = driver.FindElement(By.XPath("//*[@id='firstHalf']/table"));
//IList<IWebElement> IFirstHalfBody = IFirstHalfTable.FindElements(By.TagName("tbody"));
//IList<IWebElement> IFirstHalfRow;
//IList<IWebElement> IFirstHalfTD;
//                foreach (IWebElement Half in IFirstHalfBody)
//                {
//                    IFirstHalfRow = Half.FindElements(By.TagName("tr"));
//                    if (IFirstHalfRow.Count != 0)
//                    {
//                        foreach (IWebElement first in IFirstHalfRow)
//                        {
//                            IFirstHalfTD = first.FindElements(By.TagName("td"));
//                            if (IFirstHalfRow.Count != 0 && first.Text.Trim() != "")
//                            {
//                                try
//                                {
//                                    strFirstHalfType = IFirstHalfTD[0].Text;
//                                    strFirstHalfCharges = IFirstHalfTD[1].Text;
//                                    strFirstHalfCredits = IFirstHalfTD[2].Text;
//                                    strFirstHalfBalance = IFirstHalfTD[3].Text;
//                                }
//                                catch { }

//                                string strFirstHalfDetails = strTaxInstallYear + "~" + strFirstHalfType + "~" + strFirstHalfCharges + "~" + strFirstHalfCredits + "~" + strFirstHalfBalance;
//gc.insert_date(orderNumber, Outparcelno, 68, strFirstHalfDetails, 1, DateTime.Now);

//                            }
//                        }
//                    }
//                }


//                strSecondInstall = driver.FindElement(By.XPath("//*[@id='secondHalf']/table/thead/tr/th[1]")).Text;
//                if (strSecondInstall == "")
//                {
//                    strSecondInstall = "-";
//                }
//                strTaxSecondYear = strTaxInstall + "~" + strSecondInstall;
//                //Tax Second Half
//                IWebElement ISecondHalfTable = driver.FindElement(By.XPath("//*[@id='firstHalf']/table"));
//IList<IWebElement> ISecondHalfBody = ISecondHalfTable.FindElements(By.TagName("tbody"));
//IList<IWebElement> ISecondHalfRow;
//IList<IWebElement> ISecondHalfTD;
//                foreach (IWebElement Half in ISecondHalfBody)
//                {
//                    ISecondHalfRow = Half.FindElements(By.TagName("tr"));
//                    if (ISecondHalfRow.Count != 0)
//                    {
//                        foreach (IWebElement second in ISecondHalfRow)
//                        {
//                            ISecondHalfTD = second.FindElements(By.TagName("td"));
//                            if (ISecondHalfRow.Count != 0 && second.Text.Trim() != "")
//                            {
//                                try
//                                {
//                                    strSecondHalfType = ISecondHalfTD[0].Text;
//                                    strSecondHalfCharges = ISecondHalfTD[1].Text;
//                                    strSecondHalfCredits = ISecondHalfTD[2].Text;
//                                    strSecondHalfBalance = ISecondHalfTD[3].Text;
//                                }
//                                catch { }

//                                string strSecondHalfDetails = strTaxSecondYear + "~" + strSecondHalfType + "~" + strSecondHalfCharges + "~" + strSecondHalfCredits + "~" + strSecondHalfBalance;
//gc.insert_date(orderNumber, Outparcelno, 68, strSecondHalfDetails, 1, DateTime.Now);

//                            }

//                        }
//                    }
//                }
//            }

//            //Valuation Details
//            driver.FindElement(By.XPath("//*[@id='propertyHeaderList']/ul/li[3]/a")).SendKeys(Keys.Enter);
//driver.FindElement(By.XPath("//*[@id='tabs-2']/div[2]/div[1]/ul/li[4]/a")).SendKeys(Keys.Enter);
//Thread.Sleep(2000);
//            gc.CreatePdf(orderNumber, Outparcelno, "OHFranklinValuationDetails", driver, "OH", "Franklin");

//            //Payments Details
//            driver.FindElement(By.XPath("//*[@id='propertyHeaderList']/ul/li[4]/a")).SendKeys(Keys.Enter);
//Thread.Sleep(2000);
//            gc.CreatePdf(orderNumber, Outparcelno, "OHFranklinPaymentDetails", driver, "OH", "Franklin");
//            try
//            {
//                for (int p = 3; p< 6; p++)
//                {
//                    IWebElement Iyear = driver.FindElement(By.XPath("//*[@id='tabs-3']/div[2]/div[1]/ul/li[" + p + "]/a"));
//strPayYear = Iyear.Text;
//                    js1.ExecuteScript("arguments[0].click();", Iyear);
//                    Thread.Sleep(2000);
//                    gc.CreatePdf(orderNumber, Outparcelno, "OHFranklin" + strPayYear + "PaymentDetails", driver, "OH", "Franklin");
//                    IWebElement Itable = driver.FindElement(By.XPath("//*[@id='ctl00_cphBodyContent_fcPaymentContainer_ctl12_PaymentPanel']/table/tbody"));
//IList<IWebElement> Itrow = Itable.FindElements(By.TagName("tr"));
//IList<IWebElement> ItableTd;
//                    foreach (IWebElement taxrows in Itrow)
//                    {
//                        ItableTd = taxrows.FindElements(By.TagName("td"));
//                        if (ItableTd.Count != 0 && !ItableTd[0].Text.Contains("Sorry, no Special Assessment data available for " + strPayYear + "."))
//                        {
//                            strPaymentDate = ItableTd[0].Text;
//                            strPaymentTrans = ItableTd[1].Text;
//                            strPayment = strPaymentDate + "~" + strPaymentTrans;
//                            gc.insert_date(orderNumber, Outparcelno, 69, strPayment, 1, DateTime.Now);
//                        }
//                    }
//                }
//            }
//            catch { }
//            try
//            {
//                //Distribution Details//*[@id="ctl00_cphBodyContent_fcTaxDistributionContainer_ctl12_TaxDistributionPanel"]/table
//                driver.FindElement(By.XPath("//*[@id='propertyHeaderList']/ul/li[5]/a")).SendKeys(Keys.Enter);
//IWebElement IDisYear = driver.FindElement(By.XPath("//*[@id='tabs-4']/div[2]/div[1]/ul/li[4]/a"));
//strDisYear = IDisYear.Text;
//                js1.ExecuteScript("arguments[0].click();", IDisYear);
//                Thread.Sleep(2000);
//                gc.CreatePdf(orderNumber, Outparcelno, "OHFranklinDistributionDetails", driver, "OH", "Franklin");
//                IWebElement TbDistribuiton = driver.FindElement(By.XPath("//*[@id='ctl00_cphBodyContent_fcTaxDistributionContainer_ctl12_TaxDistributionPanel']/table/tbody"));
//IList<IWebElement> TrDistribuiton = TbDistribuiton.FindElements(By.TagName("tr"));
//IList<IWebElement> TdDistribuiton;
//                foreach (IWebElement rows in TrDistribuiton)
//                {
//                    TdDistribuiton = rows.FindElements(By.TagName("td"));
//                    if (TdDistribuiton.Count != 0 && !rows.Text.Contains("Political Subdivision"))
//                    {
//                        political_subDivision = TdDistribuiton[0].Text;
//                        amount = TdDistribuiton[1].Text;
//                        string strDistribution = strDisYear + "~" + political_subDivision + "~" + amount;
//gc.insert_date(orderNumber, Outparcelno, 70, strDistribution, 1, DateTime.Now);
//                    }
//                }
//            }
//            catch { }
//            //Special Assessment
//            driver.FindElement(By.XPath("//*[@id='propertyHeaderList']/ul/li[6]/a")).SendKeys(Keys.Enter);
//gc.CreatePdf(orderNumber, Outparcelno, "OHFranklinSpecialAssessmentDetails", driver, "OH", "Franklin");
//            try
//            {
//                string strtaxYear = DateTime.Now.Year.ToString();
//int taxYear = 0;
//                for (int s = 3; s< 6; s++)
//                {
//                    IWebElement Iyear = driver.FindElement(By.XPath("//*[@id='tabs-5']/div[2]/div[1]/ul/li[" + s + "]/a"));
//strSpecYear = Iyear.Text;
//                    js1.ExecuteScript("arguments[0].click();", Iyear);
//                    Thread.Sleep(2000);
//                    gc.CreatePdf(orderNumber, Outparcelno, "OHFranklin" + strSpecYear + "SpecialAssessmentDetails", driver, "OH", "Franklin");
//                    if (strSpecYear.Contains(strtaxYear) || strSpecYear.Contains(strtaxYear) || strSpecYear.Contains(strtaxYear))
//                    {
//                        try
//                        {
//                            Itable = driver.FindElement(By.XPath("//*[@id='ctl00_cphBodyContent_fcSpecialAssessmentContainer_ctl12_SpecialAssessmentDetailTable']/tbody"));
//                        }
//                        catch { }
//                        try
//                        {
//                            Itable = driver.FindElement(By.XPath("//*[@id='ctl00_cphBodyContent_fcSpecialAssessmentContainer_ctl12_SpecialAssessmentNoData']/tbody"));
//                        }
//                        catch { }
//                        IList<IWebElement> Itrow = Itable.FindElements(By.TagName("tr"));
//IList<IWebElement> ItableTd;
//                        foreach (IWebElement taxrows in Itrow)
//                        {
//                            ItableTd = taxrows.FindElements(By.TagName("td"));
//                            if (ItableTd.Count != 0 && !ItableTd[0].Text.Contains("Sorry, no Special Assessment data available for " + strSpecYear + "."))
//                            {
//                                strspecialDesc = ItableTd[0].Text;
//                                strspecialAmount = ItableTd[1].Text;
//                                strspecial = strSpecYear + "~" + strspecialDesc + "~" + strspecialAmount;
//                                gc.insert_date(orderNumber, Outparcelno, 71, strspecial, 1, DateTime.Now);
//                            }
//                        }
//                    }
//                    taxYear = Convert.ToInt32(strtaxYear);
//                    taxYear--;
//                    strtaxYear = Convert.ToString(taxYear);
//                }