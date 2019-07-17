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
    public class Webdriver_BuncombeNC
    {
        List<string> AddressSearch = new List<string>();
        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());


        public string FTP_Buncombe(string streetno, string streetname, string direction, string streetype, string unitno, string ownername, string parcelNumber, string searchType, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            List<string> multiparcel = new List<string>();
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "", Taxauthority = "";

            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            using (driver = new PhantomJSDriver())
            {
                StartTime = DateTime.Now.ToString("HH:mm:ss");

                try
                {
                    driver.Navigate().GoToUrl("https://tax.buncombecounty.org/");
                    try
                    {
                        driver.FindElement(By.Id("ctl00_cph_content_btnAccept")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                    }
                    catch { }
                    try
                    {
                        Taxauthority = driver.FindElement(By.XPath("//*[@id='pageImage']/p")).Text;
                    }
                    catch { }

                    var Selectparcel = driver.FindElement(By.Id("ctl00_cph_content_lbSearchType"));
                    var Selectparcel1 = new SelectElement(Selectparcel);
                    Selectparcel1.SelectByValue("Parcels");


                    if (searchType == "titleflex")
                    {
                        string address = streetno + " " + direction + " " + streetname + " " + streetype + " " + unitno;
                        gc.TitleFlexSearch(orderNumber, parcelNumber, "", address, "NC", "Buncombe");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_BuncombeNC"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }

                    if (searchType == "address")
                    {
                        var Selectstreetname = driver.FindElement(By.Id("ctl00_cph_content_lbSearchField"));
                        var Selectstreetname1 = new SelectElement(Selectstreetname);
                        Selectstreetname1.SelectByValue("Street Name");

                        driver.FindElement(By.Id("ctl00_cph_content_txtSearch1")).SendKeys(streetno);
                        driver.FindElement(By.Id("ctl00_cph_content_txtSearch2")).SendKeys(direction);
                        driver.FindElement(By.Id("ctl00_cph_content_txtSearch3")).SendKeys(streetname);
                        driver.FindElement(By.Id("ctl00_cph_content_txtSearch4")).SendKeys(streetype);
                        gc.CreatePdf_WOP(orderNumber, "Address Search Before", driver, "NC", "Buncombe");
                        driver.FindElement(By.Id("ctl00_cph_content_btnSearch")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "Address Search After", driver, "NC", "Buncombe");


                        //Multiparcel
                        try
                        {
                            int Count = 0;
                            IWebElement Multiaddresstable = driver.FindElement(By.Id("ctl00_cph_content_gvResults"));
                            IList<IWebElement> multiaddressrow = Multiaddresstable.FindElements(By.TagName("tr"));
                            IList<IWebElement> Multiaddressid;
                            foreach (IWebElement Multiaddress in multiaddressrow)
                            {
                                Multiaddressid = Multiaddress.FindElements(By.TagName("td"));
                                if (multiaddressrow.Count != 0 && !Multiaddress.Text.Contains("Parcel") && Multiaddressid.Count == 5)
                                {
                                    string Multiparcelnumber = Multiaddressid[0].Text;
                                    string OWnername = Multiaddressid[1].Text;
                                    string Address = Multiaddressid[3].Text;
                                    string multiaddressresult = OWnername + "~" + Address;
                                    gc.insert_date(orderNumber, Multiparcelnumber, 1034, multiaddressresult, 1, DateTime.Now);
                                    Count++;
                                }
                                if (multiaddressrow.Count < 6)
                                {
                                    Multiaddressid[0].FindElement(By.TagName("a")).Click();
                                }

                            }
                            if (Count > 5 && multiaddressrow.Count < 26)
                            {
                                HttpContext.Current.Session["multiparcel_Buncombe"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                            if (multiaddressrow.Count > 1 && multiaddressrow.Count > 25)
                            {
                                HttpContext.Current.Session["multiparcel_Buncombe_Maximum"] = "Maximum";
                                driver.Quit();
                                return "Maximum";
                            }
                        }
                        catch { }
                        //No Data Found
                        try
                        {
                            string nodata = driver.FindElement(By.Id("ctl00_cph_content_lblErrorMsg")).Text;
                            if (nodata.Contains("No Results Found"))
                            {
                                HttpContext.Current.Session["Nodata_BuncombeNC"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }

                    if (searchType == "parcel")
                    {
                        var Selectparcelid = driver.FindElement(By.Id("ctl00_cph_content_lbSearchField"));
                        var Selectparcelid1 = new SelectElement(Selectparcelid);
                        Selectparcelid1.SelectByValue("Parcel Id");

                        driver.FindElement(By.Id("ctl00_cph_content_txtSearch")).SendKeys(parcelNumber);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search Before", driver, "NC", "Buncombe");
                        driver.FindElement(By.Id("ctl00_cph_content_btnSearch")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search After", driver, "NC", "Buncombe");
                        //No Data Found
                        try
                        {
                            string nodata = driver.FindElement(By.Id("ctl00_cph_content_lblErrorMsg")).Text;
                            if (nodata.Contains("No Results Found"))
                            {
                                HttpContext.Current.Session["Nodata_BuncombeNC"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }

                    if (searchType == "ownername")
                    {
                        var Selectownername = driver.FindElement(By.Id("ctl00_cph_content_lbSearchField"));
                        var Selectownername1 = new SelectElement(Selectownername);
                        Selectownername1.SelectByValue("Owner Name");

                        string[] owner = ownername.Split(' ');
                        string Lastname = owner[1];
                        string Firstname = owner[0];
                        driver.FindElement(By.Id("ctl00_cph_content_txtSearch1")).SendKeys(Lastname);
                        driver.FindElement(By.Id("ctl00_cph_content_txtSearch2")).SendKeys(Firstname);
                        gc.CreatePdf(orderNumber, parcelNumber, "Ownername Search Before", driver, "NC", "Buncombe");
                        driver.FindElement(By.Id("ctl00_cph_content_btnSearch")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Ownername Search After", driver, "NC", "Buncombe");


                        //Multiparcel
                        try
                        {
                            int Count = 0;
                            IWebElement Multiaddresstable = driver.FindElement(By.Id("ctl00_cph_content_gvResults"));
                            IList<IWebElement> multiaddressrow = Multiaddresstable.FindElements(By.TagName("tr"));
                            IList<IWebElement> Multiaddressid;
                            foreach (IWebElement Multiaddress in multiaddressrow)
                            {
                                Multiaddressid = Multiaddress.FindElements(By.TagName("td"));
                                if (multiaddressrow.Count != 0 && !Multiaddress.Text.Contains("Parcel") && Multiaddressid.Count == 5)
                                {
                                    string multiParcelnumber = Multiaddressid[0].Text;
                                    string OWnername = Multiaddressid[1].Text;
                                    string Address = Multiaddressid[3].Text;
                                    string multiaddressresult = OWnername + "~" + Address;
                                    gc.insert_date(orderNumber, multiParcelnumber, 1034, multiaddressresult, 1, DateTime.Now);
                                    Count++;
                                }

                                if (multiaddressrow.Count < 6)
                                {
                                    Multiaddressid[0].FindElement(By.TagName("a")).Click();
                                    break;
                                }

                            }
                            if (Count > 1 && multiaddressrow.Count < 26)
                            {
                                HttpContext.Current.Session["multiparcel_Buncombe"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                            if (Count > 1 && multiaddressrow.Count > 25)
                            {
                                HttpContext.Current.Session["multiparcel_Buncombe_Maximum"] = "Maximum";
                                driver.Quit();
                                return "Maximum";
                            }
                        }
                        catch { }
                        try
                        {
                            //No Data Found
                            string nodata = driver.FindElement(By.Id("ctl00_cph_content_lblErrorMsg")).Text;
                            if (nodata.Contains("No Results Found"))
                            {
                                HttpContext.Current.Session["Nodata_BuncombeNC"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }

                    //Property Details
                    string propertydetails = "";
                    IWebElement IAddressSearch = driver.FindElement(By.XPath("//*[@id='ctl00_cph_content_gvResults']/tbody/tr[2]/td[1]/a"));
                    IJavaScriptExecutor js = driver as IJavaScriptExecutor;
                    js.ExecuteScript("arguments[0].click();", IAddressSearch);
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, parcelNumber, "Property Info", driver, "NC", "Buncombe");

                    string ParcelInformationID = "", PropertyLocation = "", AppraisalAreaAndAppraiser = "", Acres = "", LegalReference = "", Class = "", Neighborhood = "";
                    ParcelInformationID = driver.FindElement(By.Id("ctl00_cph_content_lblParcel")).Text.Trim();
                    PropertyLocation = driver.FindElement(By.Id("ctl00_cph_content_lblSitus")).Text.Trim();
                    AppraisalAreaAndAppraiser = driver.FindElement(By.Id("ctl00_cph_content_lblAppraisalArea")).Text.Trim();
                    Acres = driver.FindElement(By.Id("ctl00_cph_content_lblAcres")).Text.Trim();
                    LegalReference = driver.FindElement(By.Id("ctl00_cph_content_lblLegalRef")).Text.Trim();
                    Class = driver.FindElement(By.Id("ctl00_cph_content_lblClass")).Text.Trim();
                    Neighborhood = driver.FindElement(By.Id("ctl00_cph_content_lblNeighborhood")).Text.Trim();


                    propertydetails = PropertyLocation + "~" + AppraisalAreaAndAppraiser + "~" + Acres + "~" + LegalReference + "~" + Class + "~" + Neighborhood + "~" + Taxauthority;
                    //gc.insert_date(orderNumber, ParcelInformationID, 1021, propertydetails, 1, DateTime.Now);

                    string BillsinformationvalueDetails = "";
                    try
                    {
                        IWebElement Billspropertydetailsvalueinfo2 = driver.FindElement(By.XPath("//*[@id='ctl00_cph_content_pnlResults']/fieldset[1]/div[4]/table/tbody"));
                        IList<IWebElement> TRBillspropertydetailsvalueinfo2 = Billspropertydetailsvalueinfo2.FindElements(By.TagName("tr"));
                        IList<IWebElement> TDBillspropertydetailsvalueinfo2;
                        foreach (IWebElement row in TRBillspropertydetailsvalueinfo2)
                        {
                            TDBillspropertydetailsvalueinfo2 = row.FindElements(By.TagName("td"));
                            if (TDBillspropertydetailsvalueinfo2.Count != 0 && TDBillspropertydetailsvalueinfo2.Count == 5 && !row.Text.Contains("Levy"))
                            {

                                string County = TDBillspropertydetailsvalueinfo2[0].Text;
                                string City = TDBillspropertydetailsvalueinfo2[1].Text;
                                string Fire = TDBillspropertydetailsvalueinfo2[2].Text;
                                string School = TDBillspropertydetailsvalueinfo2[3].Text;
                                string Township = TDBillspropertydetailsvalueinfo2[4].Text;

                                BillsinformationvalueDetails = propertydetails + "~" + County.Trim() + "~" + City.Trim() + "~" + Fire.Trim() + "~" + School.Trim() + "~" + Township.Trim();
                                gc.insert_date(orderNumber, ParcelInformationID, 1021, BillsinformationvalueDetails, 1, DateTime.Now);
                            }
                        }
                    }
                    catch { }
                    try
                    {
                        IWebElement Billspropertydetailsvalueinfo1 = driver.FindElement(By.Id("ctl00_cph_content_AccountInfo_gvAccountOwners"));
                        IList<IWebElement> TRBillspropertydetailsvalueinfo1 = Billspropertydetailsvalueinfo1.FindElements(By.TagName("tr"));
                        IList<IWebElement> TDBillspropertydetailsvalueinfo1;
                        foreach (IWebElement row in TRBillspropertydetailsvalueinfo1)
                        {
                            TDBillspropertydetailsvalueinfo1 = row.FindElements(By.TagName("td"));
                            if (TDBillspropertydetailsvalueinfo1.Count != 0 && TDBillspropertydetailsvalueinfo1.Count == 6 && !row.Text.Contains("Last Name"))
                            {
                                string LastName = TDBillspropertydetailsvalueinfo1[0].Text;
                                string FirstName = TDBillspropertydetailsvalueinfo1[1].Text;
                                string ThirdName = TDBillspropertydetailsvalueinfo1[2].Text;
                                string SuffixName = TDBillspropertydetailsvalueinfo1[3].Text;
                                string Address = TDBillspropertydetailsvalueinfo1[4].Text;
                                string CitystateZip = TDBillspropertydetailsvalueinfo1[5].Text;

                                string Billsinformationvalue = LastName.Trim() + "~" + FirstName.Trim() + "~" + ThirdName.Trim() + "~" + SuffixName.Trim() + "~" + Address.Trim() + "~" + CitystateZip.Trim();
                                gc.insert_date(orderNumber, ParcelInformationID, 1114, Billsinformationvalue, 1, DateTime.Now);

                            }
                        }
                    }
                    catch { }

                    //Assessment History
                    IWebElement assessinfo2 = driver.FindElement(By.Id("ctl00_cph_content_gvAssessmentHistory"));
                    IList<IWebElement> TRassessinfo2 = assessinfo2.FindElements(By.TagName("tr"));
                    IList<IWebElement> TDassessinfo2;
                    foreach (IWebElement row in TRassessinfo2)
                    {
                        TDassessinfo2 = row.FindElements(By.TagName("td"));
                        if (TDassessinfo2.Count != 0 && TDassessinfo2.Count == 8 && !row.Text.Contains("Tax Year"))
                        {
                            string Taxyear = TDassessinfo2[0].Text;
                            string Owners = TDassessinfo2[1].Text;
                            string AssessAcres = TDassessinfo2[2].Text;
                            string Landvalue = TDassessinfo2[3].Text;
                            string Buildingvalue = TDassessinfo2[4].Text;
                            string Improvementvalue = TDassessinfo2[5].Text;
                            string Exemptvalue = TDassessinfo2[6].Text;
                            string Taxablevalue = TDassessinfo2[7].Text;

                            string AssessmentHisotryDetails = Taxyear.Trim() + "~" + Owners.Trim() + "~" + AssessAcres + "~" + Landvalue.Trim() + "~" + Buildingvalue.Trim() + "~" + Improvementvalue + "~" + Exemptvalue + "~" + Taxablevalue;
                            gc.insert_date(orderNumber, ParcelInformationID, 1022, AssessmentHisotryDetails, 1, DateTime.Now);

                        }
                    }
                    //Tax Payment History Details
                    List<string> billinfo = new List<string>();
                    IWebElement Billsinfo2 = driver.FindElement(By.Id("ctl00_cph_content_gvBills"));
                    IList<IWebElement> TRBillsinfo2 = Billsinfo2.FindElements(By.TagName("tr"));
                    IList<IWebElement> TDBillsinfo2;
                    foreach (IWebElement row in TRBillsinfo2)
                    {
                        TDBillsinfo2 = row.FindElements(By.TagName("td"));
                        if (TDBillsinfo2.Count != 0 && TDBillsinfo2.Count == 4 && !row.Text.Contains("Bill"))
                        {
                            string Bill = TDBillsinfo2[0].Text;
                            string Ownernames = TDBillsinfo2[1].Text;
                            string Value = TDBillsinfo2[2].Text;
                            string Due = TDBillsinfo2[3].Text;

                            string BillHisotryDetails = Bill.Trim() + "~" + Ownernames.Trim() + "~" + Value.Trim() + "~" + Due.Trim();
                            gc.insert_date(orderNumber, ParcelInformationID, 1023, BillHisotryDetails, 1, DateTime.Now);

                            if (billinfo.Count < 3)
                            {
                                billinfo.Add(Bill);
                            }
                        }
                    }
                    //Tax Bills yearwise click Details
                    //Tax Bill Value Transaction Details 
                    try
                    {
                        IWebElement clicknext = driver.FindElement(By.XPath("//*[@id='ctl00_cph_content_gvBills']/tbody"));
                        IList<IWebElement> links = clicknext.FindElements(By.TagName("a"));

                        for (int i = 0; i < links.Count; i++)
                        {
                            links = clicknext.FindElements(By.TagName("a"));

                            if (!string.IsNullOrEmpty(links[i].Text))
                                links[i].Click();
                            break;
                        }
                    }
                    catch { }

                    foreach (string billing in billinfo)
                    {
                        try
                        {

                            driver.FindElement(By.Id("ctl00_cph_content_txtSearch")).SendKeys(billing);
                            driver.FindElement(By.Id("ctl00_cph_content_btnSearch")).SendKeys(Keys.Enter);
                            Thread.Sleep(2000);
                            gc.CreatePdf(orderNumber, ParcelInformationID, "Yearwise Details Infopdf" + billing, driver, "NC", "Buncombe");

                            string Taxyear = driver.FindElement(By.Id("ctl00_cph_content_lblLevy")).Text.Trim();
                            try
                            {
                                IWebElement Billsvalueinfo2 = driver.FindElement(By.Id("ctl00_cph_content_pnlBillValue"));
                                IList<IWebElement> TRBillsvalueinfo2 = Billsvalueinfo2.FindElements(By.TagName("tr"));
                                IList<IWebElement> TDBillsvalueinfo2;
                                foreach (IWebElement row in TRBillsvalueinfo2)
                                {
                                    TDBillsvalueinfo2 = row.FindElements(By.TagName("td"));
                                    if (TDBillsvalueinfo2.Count != 0 && TDBillsvalueinfo2.Count == 5 && !row.Text.Contains("Real Prop"))
                                    {
                                        string Realprop = TDBillsvalueinfo2[0].Text;
                                        string Personalprop = TDBillsvalueinfo2[1].Text;
                                        string Deferment = TDBillsvalueinfo2[2].Text;
                                        string Exemption = TDBillsvalueinfo2[3].Text;
                                        string Billstotal = TDBillsvalueinfo2[4].Text;

                                        string BillsvalueDetails = Taxyear.Trim() + "~" + Realprop.Trim() + "~" + Personalprop.Trim() + "~" + Deferment.Trim() + "~" + Exemption.Trim() + "~" + Billstotal.Trim();
                                        gc.insert_date(orderNumber, ParcelInformationID, 1024, BillsvalueDetails, 1, DateTime.Now);

                                    }
                                }
                            }
                            catch { }
                            try
                            {
                                IWebElement Billstransinfo2 = driver.FindElement(By.Id("ctl00_cph_content_gvBillTrans"));
                                IList<IWebElement> TRBillstransinfo2 = Billstransinfo2.FindElements(By.TagName("tr"));
                                IList<IWebElement> TDBillstransinfo2;
                                foreach (IWebElement row1 in TRBillstransinfo2)
                                {
                                    TDBillstransinfo2 = row1.FindElements(By.TagName("td"));
                                    if (TDBillstransinfo2.Count != 0 && TDBillstransinfo2.Count == 9 && !row1.Text.Contains("Type"))
                                    {
                                        string Type = TDBillstransinfo2[0].Text;
                                        string Transdate = TDBillstransinfo2[1].Text;
                                        string Rcpt = TDBillstransinfo2[2].Text;
                                        string Tax = TDBillstransinfo2[3].Text;
                                        string Late = TDBillstransinfo2[4].Text;
                                        string Interest = TDBillstransinfo2[5].Text;
                                        string Costfee = TDBillstransinfo2[6].Text;
                                        string Total = TDBillstransinfo2[7].Text;
                                        string BillReceipt = TDBillstransinfo2[8].Text;

                                        string BilltransactionDetails = Type.Trim() + "~" + Transdate.Trim() + "~" + Rcpt.Trim() + "~" + Tax.Trim() + "~" + Late.Trim() + "~" + Interest.Trim() + "~" + Costfee.Trim() + "~" + Total.Trim() + "~" + BillReceipt;
                                        gc.insert_date(orderNumber, ParcelInformationID, 1033, BilltransactionDetails, 1, DateTime.Now);

                                    }
                                }
                                try
                                {
                                    IWebElement downloadreceipt = driver.FindElement(By.Id("ctl00_cph_content_gvBillTrans"));
                                    IList<IWebElement> linkss = downloadreceipt.FindElements(By.TagName("a"));

                                    foreach (IWebElement download1 in linkss)
                                    {
                                        if (download1.Text == "Receipt")
                                        {
                                            download1.Click();
                                            gc.CreatePdf(orderNumber, ParcelInformationID, "Receipt Pdf" + billing, driver, "NC", "Buncombe");
                                            driver.Navigate().Back();
                                        }
                                    }
                                }
                                catch { }
                            }
                            catch { }
                        }
                        catch { }
                    }
                    TaxTime = DateTime.Now.ToString("HH:mm:ss");
                    driver.Quit();
                    gc.mergpdf(orderNumber, "NC", "Buncombe");
                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "NC", "Buncombe", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);
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

