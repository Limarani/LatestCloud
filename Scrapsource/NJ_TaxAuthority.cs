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
using System.Web.UI;
namespace ScrapMaricopa.Scrapsource
{
    public class NJ_TaxAuthority
    {
        IWebDriver driver;
        GlobalClass gc = new GlobalClass();
        public string TaxCollector(IWebDriver driver, string district, string orderNumber, string Parcel_Id, string countynameNJ)
        {

            string result = "", Taxing = "";
            //Ocean
            if (district == "1513")
            {
                result = driver.FindElement(By.XPath("//*[@id='footer_content']/div[1]/div/p")).Text.Replace("\r\n", " ");

            }
            if (district == "1507")
            {
                result = driver.FindElement(By.XPath("//*[@id='lsvr_custom_code_widget-3']/div/div")).Text.Replace("\r\n", " ");

            }
            //Morris
            if (district == "1403")
            {
                result = driver.FindElement(By.XPath("/html/body/table/tbody/tr/td[2]/table[2]/tbody/tr[1]/td[1]/div[3]")).Text.Replace("\r\n", " ");
                result = GlobalClass.Before(result, " Copyright");

            }
            if (district == "1404")
            {
                result = driver.FindElement(By.XPath("/html/body/table/tbody/tr/td[2]/table[1]/tbody/tr[1]/td/div/div[2]/span")).Text.Replace("\r\n", " ");
                result = GlobalClass.Before(result, "       Hours:");
                result = "Borough of Chatham" + " " + result;
            }
            if (district == "1407")
            {
                result = driver.FindElement(By.XPath("//*[@id='alterna-header']/div/div[2]/h4/strong")).Text.Replace("\r\n", " ");

            }
            if (district == "1412")
            {
                result = driver.FindElement(By.XPath("//*[@id='divInfoAdv4a586d09-b77a-4482-868b-984251b2945f']/div[1]/div/div/ol/li")).Text.Replace("\r\n", " ");
                result = GlobalClass.Before(result, "Email");
                result = "Borough of Chatham" + " " + result;
            }
            if (district == "1413")
            {
                result = driver.FindElement(By.XPath("//*[@id='footer']/div/div/div[1]")).Text.Replace("Municipal Offices", " ").Replace("\r\n", " ");
                result = gc.Between(result, "Contact Us", "Phone:");
            }
            if (district == "1414")
            {
                result = driver.FindElement(By.XPath("//*[@id='cn_contents']/font/p[2]")).Text.Replace("\r\n", " ");
                result = GlobalClass.Before(result, "Business Hours:");
                result = "Township of Jefferson" + " " + result;
            }
            if (district == "1417")
            {
                driver.FindElement(By.XPath("//*[@id='cn_contents']/table[1]/tbody/tr[2]/td[2]/table/tbody/tr/td/table/tbody/tr/td/a")).Click();
                Thread.Sleep(2000);
                gc.CreatePdf(orderNumber, Parcel_Id, "Tax Authority", driver, "NJ", countynameNJ);
                result = driver.FindElement(By.XPath("//*[@id='winContactInfo']/div[2]/div/table/tbody")).Text.Replace("\r\n", " ");
            }
            if (district == "1421")
            {
                result = driver.FindElement(By.XPath("//*[@id='divInfoAdvcf647e26-3aab-4490-9ff7-56ca8ed30bb3']/div[1]/div/div/ol/li")).Text.Replace("\r\n", " ");
                result = GlobalClass.Before(result, "Monday");
            }
            if (district == "1429")
            {
                result = driver.FindElement(By.Id("divInfoAdv36d4b8ef-21dd-41d8-98cb-e8a41596f093")).Text.Replace("\r\n", " ");
               
            }
            if (district == "1430")
            {
                driver.FindElement(By.XPath("//*[@id='cn_contents']/table[1]/tbody/tr[2]/td[2]/table/tbody/tr/td/table/tbody/tr/td/a")).Click();
                Thread.Sleep(2000);
                gc.CreatePdf(orderNumber, Parcel_Id, "Tax Authority", driver, "NJ", countynameNJ);
                result = driver.FindElement(By.XPath("//*[@id='winContactInfo']/div[2]/div/p")).Text.Replace("\r\n", " ");
                result = gc.Between(result, "Tax Collections", "   David Griffith");
            }
            if (district == "1432")
            {
                driver.FindElement(By.XPath("//*[@id='cn_contents']/table[1]/tbody/tr[2]/td[2]/table/tbody/tr/td/table/tbody/tr/td/a")).Click();
                Thread.Sleep(2000);
                gc.CreatePdf(orderNumber, Parcel_Id, "Tax Authority", driver, "NJ", countynameNJ);
                result = driver.FindElement(By.XPath("//*[@id='winContactInfo']/div[2]/div")).Text.Replace("Linda Ann Roth, CTC", " ").Replace("\r\n", " ");

            }
            if (district == "1435")
            {
                driver.FindElement(By.XPath("//*[@id='cn_contents']/table[1]/tbody/tr[2]/td[2]/table/tbody/tr/td/table/tbody/tr/td/a")).Click();
                Thread.Sleep(2000);
                gc.CreatePdf(orderNumber, Parcel_Id, "Tax Authority", driver, "NJ", countynameNJ);
                result = driver.FindElement(By.XPath("//*[@id='winContactInfo']/div[2]/div")).Text.Replace("Linda Ann Roth, CTC", " ").Replace("\r\n", " ");

            }
            if (district == "1436")
            {
                //driver.FindElement(By.XPath("//*[@id='cn_contents']/table[1]/tbody/tr[2]/td[2]/table/tbody/tr/td/table/tbody/tr/td/a")).Click();
                // Thread.Sleep(2000);
                result = driver.FindElement(By.XPath("//*[@id='divInfoAdv55353b75-bbc8-4928-a40a-d82b0214e059']/div[1]/div/div/ol/li[1]")).Text.Replace("|  info@roxburynj.us", " ").Replace("\r\n", " ");
                result = "Township of Roxbury" + " " + result;
            }
            if (district == "1409")
            {
                result = driver.FindElement(By.XPath("//*[@id='main-content']/article/section/table[17]/tbody/tr[1]")).Text.Replace("\r\n", " ");
                result = GlobalClass.After(result, "Tax Collector");
                result = "Township of Dover" + " " + result;
            }
            if (district == "1418")
            {
                result = driver.FindElement(By.XPath("//*[@id='cn_contents']/p/table/tbody/tr/td/font/p[2]")).Text.Replace("\r\n", " ");
                result = GlobalClass.After(result, "Bowers Building");
                result = "Township of Mendham Borough" + " " + result;
            }
            if (district == "1419")
            {
                result = driver.FindElement(By.XPath("//*[@id='page-body']/div[2]/article/header/div/p")).Text.Replace("\r\n", " ");

            }
            if (district == "1420")
            {
                result = driver.FindElement(By.XPath("//*[@id='text-3']/div/p")).Text.Replace("\r\n", " ");
                result = GlobalClass.Before(result, "Phone");

            }
            if (district == "1423")
            {
                result = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div/table/tbody/tr[2]/td[2]")).Text.Replace("\r\n", " ");
                result = "Township of Morris Plains Borough" + " " + result;

            }
            if (district == "1425")
            {
                result = driver.FindElement(By.XPath("//*[@id='text-4']/div/p")).Text.Replace("Hall", " ").Replace("\r\n", " ");
                result = GlobalClass.Before(result, "Hours");
            }
            if (district == "1431")
            {
                result = driver.FindElement(By.XPath("/html/body/table/tbody/tr[5]/td[3]/span/div")).Text.Replace("\r\n", " ").Replace("Copyright 2015 ?", " ");
                result = GlobalClass.Before(result, " visitors");
            }
            if (district == "1434")
            {
                result = driver.FindElement(By.XPath("//*[@id='contact-us-info']")).Text/*.Replace("Hall", " ")*/.Replace("\r\n", " ");
                result = GlobalClass.Before(result, "  Get Directions");
                //result = "Township of Morris Plains Borough" + " " + result;

            }
            if (district == "1439")
            {
                result = driver.FindElement(By.XPath("//*[@id='sp-bottom7']/div[1]/div/div/div/div")).Text/*.Replace("Hall", " ")*/.Replace("\r\n", " ");
                result = GlobalClass.Before(result, "  Town Hall");
                result = "Borough of Warton " + " " + result;

            }
            //Essex
            if (district == "0704")
            {
                result = driver.FindElement(By.XPath("//*[@id='mm-0']/div[6]/div/footer/div/div[1]/div/div[2]")).Text.Replace("\r\n", " ");

            }
            if (district == "0710")
            {
                driver.FindElement(By.LinkText("Home")).Click();
                Thread.Sleep(2000);
                gc.CreatePdf(orderNumber, Parcel_Id, "Tax Authority", driver, "NJ", countynameNJ);
                result = driver.FindElement(By.XPath("//*[@id='divInfoAdv13c08e46-519b-4f77-bd17-759ce9d5db08']/div[1]/div/div")).Text.Replace("Contact Us", "").Replace("\r\n", " ").Trim();
            }

            if (district == "0719")
            {
                result = driver.FindElement(By.XPath("//*[@id='divInfoAdvbc2a15e3-7673-4304-a8f8-e76a0d67a23f']/div[1]/div/div/ol/li/span[3]")).Text.Replace("\r\n", " ").Trim();

            }

            if (district == "0722")
            {
                result = driver.FindElement(By.XPath("//*[@id='divInfoAdv794f6136-4300-48fe-8a07-f6bf340698e4']/div[1]/div/div")).Text.Replace("Contact Us", "").Replace("\r\n", " ").Trim();

            }

            if (district == "0706")
            {
                result = driver.FindElement(By.XPath("//*[@id='container-content']/div[2]/div[2]/div/div[2]/div[2]/div/div/p[2]")).Text.Replace("\r\n", " ").Replace("Location:", " ").Trim();
                result = "Borough of Essex Fells " + result;
            }

            if (district == "0712")
            {
                result = driver.FindElement(By.XPath("//*[@id='divInfoAdv21b3d016-9166-4270-9802-1304b8ffae55']/div[1]")).Text;
                result = GlobalClass.Before(result, "Hours").Replace("\r\n", " ").Trim();
            }

            if (district == "0714")
            {
                result = driver.FindElement(By.XPath("/html/body/div[14]/div[1]/div/div[1]/p")).Text.Replace("\r\n", " ").Trim();
                result = "CITY OF NEWARK " + result;
            }

            //Somerset

            if (district == "1806")
            {
                result = driver.FindElement(By.Id("copyrights")).Text.Replace("Municipal Building", "").Replace("Monday- Thursday: 9 AM - 5 PM Fridays: 8 AM - 5 PM", "").Replace("\r\n", " ").Trim();

            }

            if (district == "1807")
            {
                result = driver.FindElement(By.XPath("//*[@id='footer']/div/div[1]")).Text.Replace("\r\n", " ");
                result = GlobalClass.Before(result, "WWW").Trim();

            }

            if (district == "1808")
            {
                result = driver.FindElement(By.XPath("//*[@id='form_43_279']/div[1]/div/div/blockquote/p")).Text.Replace("Tax and Water Collection", "").Replace("\r\n", " ").Trim();


            }

            if (district == "1809")
            {
                result = driver.FindElement(By.XPath("//*[@id='form1']/div[3]/div[2]/div/div[1]/div[2]/div/div[2]")).Text.Replace("\r\n", " ").Trim();


            }

            if (district == "1810")
            {
                result = driver.FindElement(By.XPath("//*[@id='shortcodes-ultimate-3']/div")).Text.Replace("Contact Us", "").Replace("Municipal Complex", "").Replace("The Peter J. Biondi Building", "").Replace("  ", " ").Replace("\r\n", " ");
                result = result.Replace("Hours of Operation:", "").Replace("8AM - 4:30PM Mon-Fri", "").Trim();

            }

            if (district == "1813")
            {
                result = driver.FindElement(By.XPath("//*[@id='departmentbox']/p[4]")).Text.Replace("Attn: Tax Collector", "").Replace("  ", " ").Replace("\r\n", " ").Trim();

            }

            if (district == "1818")
            {
                result = driver.FindElement(By.XPath("/html/body/div[3]/footer/div/div[1]/div[2]")).Text.Replace("Borough Hall", "").Replace("\r\n", " ").Trim();
                result = "Borough of Somerville " + result;
            }

            if (district == "1804")
            {
                result = driver.FindElement(By.XPath("//*[@id='text-2']/div[2]")).Text.Replace("Borough Hall Address", "").Replace("\r\n", " ");
                result = result.Replace("Borough Hall Hours", "").Replace("Monday, Wednesday, Thursday, Friday: 8:30 AM-4:30 PM", "").Replace("Tuesday: 8:30 AM-8:00 PM", "").Trim();
                result = "Borough Of Bound Brook " + result;
            }

            if (district == "1812")
            {
                string Tax = driver.FindElement(By.XPath("//*[@id='container']/div/div/strong")).Text.Replace("Hall", "").Trim();
                result = driver.FindElement(By.XPath("//*[@id='container']/div/div/p[3]")).Text.Replace("Mayor’s Line: 908.359.5783", "").Replace("\r\n", " ").Trim();
                result = Tax + " " + result;

            }

            if (district == "1817")
            {
                string t1 = "", t2 = "", t3 = "", t4 = "";
                t1 = driver.FindElement(By.XPath("//*[@id='sites-canvas-main-content']/table/tbody/tr/td/div/p[1]")).Text.Trim();
                t2 = driver.FindElement(By.XPath("//*[@id='sites-canvas-main-content']/table/tbody/tr/td/div/p[4]")).Text.Trim();
                t3 = driver.FindElement(By.XPath("//*[@id='sites-canvas-main-content']/table/tbody/tr/td/div/p[5]")).Text.Trim();
                t4 = driver.FindElement(By.XPath("//*[@id='sites-canvas-main-content']/table/tbody/tr/td/div/p[2]")).Text.Trim();

                result = t1 + " " + t2 + " " + t3 + " " + t4;

            }

            if (district == "1801")
            {
                try
                {
                    //string ta1 = "", ta2 = "";
                    //ta1 = driver.FindElement(By.XPath("//*[@id='schema-name-address']/div/div[1]/div[2]/location-address/div/p")).Text;
                    //ta2 = driver.FindElement(By.XPath("//*[@id='schema-name-address']/div/div[1]/div[2]/location-address/div/p")).Text;
                    result = "1 Miller Ln, Bedminster, NJ 07921 (908) 212-7022";

                }
                catch { }
            }





            //Camden

            if (district == "0414")
            {
                result = driver.FindElement(By.XPath("//*[@id='content']/div[2]/div/p[21]")).Text.Trim();
                result = gc.Between(result, "Sewer Collections", ", Contact");
                result = GlobalClass.After(result, "Tax Collector");
                result = "City of Gloucester" + "  " + result;

            }

            //Hudson
            if (district == "0912")
            {
                //string result1 = "", result2 = "";
                //result1 = driver.FindElement(By.XPath("//*[@id='container-content']/div/div[2]/div[2]/div[2]/h3")).Text.Trim();
                //result2 = driver.FindElement(By.XPath("//*[@id='container-content']/div/div[2]/div[2]/div[2]/p/a")).Text.Trim();
                //result = result1 + "  " + result2.Replace("\r\n", " ");
                result = "The Town of West New York, NJ 428 - 60th Street West New York, NJ, 07093";
            }
            if (district == "0909")
            {

                result = driver.FindElement(By.XPath("/html/body/div[1]/div/div/footer/div/div[2]/p")).Text.Trim();

            }
            if (district == "0906")
            {

                result = driver.FindElement(By.XPath("//*[@id='main-footer']/div[1]/div")).Text.Trim();
                result = GlobalClass.After(result, "ServiceLink Usage").Trim();
                result = "City of Jersey" + " " + result;

            }
            //Monmouth
            if (district == "1332")
            {
                result = driver.FindElement(By.XPath("//*[@id='CityDirectoryLeftMargin']/span[1]/p[1]")).Text.Trim();
                result = "Township of Middletown" + " " + result;
            }
            if (district == "1333")
            {
                result = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/div/div/p")).Text.Trim();
                result = gc.Between(result, "Town Hall •", "Municipal").Trim();
                result = "Township of Millstone" + " " + result;
            }
            if (district == "1330")
            {
                result = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td[1]/p[5]")).Text.Trim();
                result = GlobalClass.After(result, "Town Hall").Trim();
                result = "Township of Marlboro" + " " + result;
            }
            //Union

            if (district == "2016")
            {
                result = driver.FindElement(By.Id("cc-m-11510368230")).Text.Trim();
                result = GlobalClass.Before(result, "Office Hours").Trim();
                result = "Township of Scotch Plains" + " " + result;
            }
            if (district == "2017")
            {
                result = driver.FindElement(By.XPath("//*[@id='text-15']/div")).Text.Trim();
                result = GlobalClass.After(result, "Floor").Trim();
                result = "Township of Springfield" + " " + result;
            }
            if (district == "2012")
            {
                driver.FindElement(By.XPath("//*[@id='cn_contents']/table[1]/tbody/tr[2]/td[2]/table/tbody/tr/td/table/tbody/tr/td/a")).Click();
                Thread.Sleep(2000);
                gc.CreatePdf(orderNumber, Parcel_Id, "Tax Authority", driver, "NJ", countynameNJ);
                result = driver.FindElement(By.XPath("//*[@id='winContactInfo']/div[2]/div/p")).Text.Trim();
                result = GlobalClass.After(result, "contact the").Trim();
                result = "City of Plainfield" + " " + result;
            }
            if (district == "2013")
            {
                result = driver.FindElement(By.XPath("//*[@id='text-3']/div/div/p[1]")).Text.Replace("New Jersey", "").Trim();
            }
            if (district == "2001")
            {               
                result = "Township of Berkeley Heights 29 Park Avenue Berkeley Heights, NJ 07922";              
            }
            //Bergen
            if (district == "0212")// East Rutherford

            {
                result = driver.FindElement(By.XPath("/html/body/div[4]/div/div[2]/div/div[4]/div[8]")).Text.Replace("\r\n", " ").Trim();

            }
            if (district == "0216")//Englewood Cliffs
            {

                driver.FindElement(By.XPath("//*[@id='cn_contents']/table[1]/tbody/tr[2]/td[2]/table/tbody/tr/td/table/tbody/tr/td/a")).Click();
                Thread.Sleep(2000);
                gc.CreatePdf(orderNumber, Parcel_Id, "Tax Authority", driver, "NJ", countynameNJ);
                result = driver.FindElement(By.XPath("//*[@id='winContactInfo']/div[2]")).Text.Replace("\r\n", " ").Trim();
                result = gc.Between(result, "contact us at ...", "Public Hours");

            }
            if (district == "0213")//Edgewater
            {

                driver.FindElement(By.XPath("//*[@id='cn_contents']/table[1]/tbody/tr[2]/td[2]/table/tbody/tr/td/table/tbody/tr/td/a")).Click();
                Thread.Sleep(2000);
                gc.CreatePdf(orderNumber, Parcel_Id, "Tax Authority", driver, "NJ", countynameNJ);

                result = driver.FindElement(By.XPath("//*[@id='winContactInfo']/div[2]/div/p")).Text.Replace("\r\n", " ").Trim();
                result = gc.Between(result, "Tax Collector Clerk", "Hours");

            }
            if (district == "0219")//Fort Lee

            {

                driver.FindElement(By.XPath("//*[@id='cn_contents']/table[1]/tbody/tr[2]/td[2]/table/tbody/tr/td/table/tbody/tr/td/a")).Click();
                Thread.Sleep(2000);
                gc.CreatePdf(orderNumber, Parcel_Id, "Tax Authority", driver, "NJ", countynameNJ);
                result = driver.FindElement(By.XPath("//*[@id='winContactInfo']/div[2]")).Text.Replace("\r\n", " ").Trim();
                result = gc.Between(result, "Tax Collector's Office. ", "Hours of Operation:");

            }
            if (district == "0238")//New Milford

            {
                result = driver.FindElement(By.XPath("//*[@id='Table5']/tbody/tr/td/table[1]/tbody/tr/td/table/tbody/tr/td/div/p[1]/span")).Text.Replace("Borough Hall", "").Replace("\r\n", " ").Trim();
                result = "Borough of New Milford " + result;
            }
            if (district == "0255")//Rockleigh
            {
                result = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr/td[1]/p[4]/font")).Text.Replace("Borough Hall Address", "").Replace("\r\n", " ").Trim();
                result = "Borough of Rockleigh " + result;

            }
            if (district == "0256")//Rutherford

            {
                result = driver.FindElement(By.XPath("/html/body/div[4]/div/div[2]/div/div[4]/div[8]")).Text.Replace("\r\n", " ").Trim();


            }
            if (district == "0204")//Bogota

            {
                result = driver.FindElement(By.XPath("//*[@id='WRchTxt8']/p[2]")).Text.Replace("\r\n", " ").Trim();


            }
            if (district == "0203")//Bergenfield

            {
                driver.FindElement(By.XPath("//*[@id='cn_contents']/table[1]/tbody/tr[2]/td[2]/table/tbody/tr/td/table/tbody/tr/td/a")).Click();
                Thread.Sleep(2000);
                gc.CreatePdf(orderNumber, Parcel_Id, "Tax Authority", driver, "NJ", countynameNJ);
                result = driver.FindElement(By.XPath("//*[@id='winContactInfo']/div[2]/div/table/tbody/tr[1]/td[2]/p")).Text.Replace("\r\n", " ").Trim();
                result = "BOROUGH OF BERGENFIELD TAX COLLECTOR " + result;


            }
            if (district == "0257")//Saddle Brook
            {
                result = driver.FindElement(By.XPath("/html/body/footer/div[1]/div/div/div[3]/address")).Text.Replace("\r\n", " ").Trim();


            }
            if (district == "0239")//North Arlington
            {
                result = driver.FindElement(By.XPath("//*[@id='Home']/main/div/div/div/div[2]/div[2]")).Text.Replace("\r\n", " ").Trim();

            }
            if (district == "0250")//ridgefieldpark
            {
                result = driver.FindElement(By.XPath("//*[@id='mini-panel-two_column_footer']/div/div[1]/p")).Text.Replace("\r\n", " ").Trim();

            }
            if (district == "0246")//Paramus

            {
                driver.FindElement(By.XPath("//*[@id='cn_contents']/table[1]/tbody/tr[2]/td[2]/table/tbody/tr/td/table/tbody/tr/td/a")).Click();
                Thread.Sleep(2000);
                gc.CreatePdf(orderNumber, Parcel_Id, "Tax Authority", driver, "NJ", countynameNJ);
                result = driver.FindElement(By.XPath("//*[@id='winContactInfo']/div[2]/div/div[3]")).Text.Replace("\r\n", " ").Trim();
                result = "Tax Collector " + result;



            }
            //Middlesex-Carteret
            if (district == "1219") //-Carteret
            {
                result = driver.FindElement(By.XPath("//*[@id='cn_contents']/p/table/tbody/tr/td/table/tbody/tr[129]/td[2]")).Text.Replace("\r\n", " ").Trim();

            }
            if (district == "1217")
            {
                result = driver.FindElement(By.XPath("//*[@id='node-363']/div/div/div/div/p[12]")).Text.Replace("\r\n", " ").Trim();
                result = GlobalClass.After(result, "Tax Collector");
            }
            if (district == "1211")
            {
                result = driver.FindElement(By.XPath("//*[@id='cc782ccaf3-204c-48df-9aee-39c3740d0f73']/div[1]")).Text.Replace("Website Committee", "").Replace("|", "").Trim();
                result = Regex.Replace(result, @"\s+", " ");
            }
            else
            {
                IList<IWebElement> divs = driver.FindElements(By.XPath("/html/body/table/tbody/tr[2]/td/div/div"));
                int count1 = divs.Count;
                foreach (IWebElement div in divs)
                {
                    if (div.Text.Contains("Office Hours") || div.Text.Contains("Tax Collector") || div.Text.Contains("Township") || div.Text.Contains("Borough") || div.Text.Contains("Town"))
                    {
                        Taxing = div.Text.Replace("\r\n", " ");
                        if (Taxing.Contains("Tax Office Hours"))
                        {
                            result = GlobalClass.Before(Taxing, "Tax Office Hours");
                        }
                        else if (Taxing.Contains("Regular Office Hours are: "))
                        {
                            result = GlobalClass.Before(Taxing, "Regular Office Hours are: ");
                        }
                        else if (Taxing.Contains("Normal Office Hours"))
                        {
                            result = GlobalClass.Before(Taxing, "Normal Office Hours");
                        }
                        else if (Taxing.Contains("Office Hours") || Taxing.Contains("Office hours"))
                        {
                            result = GlobalClass.Before(Taxing, "Office Hours");
                            if (result == "")
                            {
                                result = GlobalClass.Before(Taxing, "Office hours");
                            }
                            if (result.Contains("All major credit"))
                            {
                                result = GlobalClass.Before(result, "All major credit");

                            }
                            if (result.Contains("if they have STAR, PULSE or NYCE logos."))
                            {
                                result = GlobalClass.After(result, "if they have STAR, PULSE or NYCE logos.");

                            }
                            if (result.Contains("Tax Assessment & Valuation"))
                            {
                                result = GlobalClass.Before(result, "Tax Assessment & Valuation");
                                result = result.Replace("Tax Collection & Billing", " ");
                            }
                            if (result.Contains("DELINQUENT TAXPAYERS"))
                            {
                                result = GlobalClass.After(result, "made through the tax office.");

                            }
                            if (result.Contains("Credit Card will be charged that day."))
                            {
                                result = GlobalClass.After(result, "Credit Card will be charged that day.");

                            }
                            if (result.Contains("the annual assessment amount for appeal purposes only."))
                            {
                                result = GlobalClass.After(result, "the annual assessment amount for appeal purposes only.");

                            }
                            if (result.Contains("interest calculations for a future date."))
                            {
                                result = GlobalClass.After(result, "interest calculations for a future date.");

                            }
                            if (result.Contains("NOW ACCEPTED USING ALL VISA CARDS"))
                            {
                                result = GlobalClass.After(result, "NOW ACCEPTED USING ALL VISA CARDS");

                            }
                            if (result.Contains("Tax Collector."))
                            {
                                result = GlobalClass.After(result, "Tax Collector.");

                            }
                            if (result.Contains("Physical Address:"))
                            {
                                result = GlobalClass.After(result, "Physical Address:");

                            }
                            if (result.Contains("Township of Aberdeen"))
                            {
                                result = gc.Between(result, "removed from the tax sale.", "Tax Department:");

                            }
                            if (result.Contains("VOORHEES TAX"))
                            {
                                result = gc.Between(result, "Tax Collector's Office.", "Administrative");

                            }
                        }

                        else if (Taxing.Contains("water & sewer quarterly"))
                        {
                            result = GlobalClass.Before(Taxing, "water & sewer quarterly");
                        }
                        else if (Taxing.Contains("Water and Sewer Revenue Office"))
                        {
                            result = GlobalClass.Before(Taxing, "Water and Sewer Revenue Office");
                        }
                        else if (Taxing.Contains("The interest listed on delinquent"))
                        {
                            try
                            {
                                result = GlobalClass.Before(Taxing, "The interest listed on delinquent");
                            }
                            catch { }
                             if (Taxing.Contains("Payments"))
                            {
                                try
                                {
                                    result = GlobalClass.Before(result, "Payments");
                                }
                                catch { }
                            }
                            if (Taxing.Contains("Hours of Operation"))
                            {
                                try
                                {
                                    result = GlobalClass.Before(result, "Hours of Operation");
                                }
                                catch { }
                            }
                        }
                        
                        else if (Taxing.Contains("Interest listed on delinquent"))
                        {
                            result = GlobalClass.Before(Taxing, "Interest listed on delinquent");
                        }
                        else if (Taxing.Contains("Mahwah Tax and Utility Collector"))
                        {
                            result = gc.Between(Taxing, "STAR, PULSE or NYCE logos.", "Office Hours");
                        }
                        else if (Taxing.Contains("Tax Collector’s Hours"))
                        {
                            result = GlobalClass.Before(Taxing, "Tax Collector’s Hours");
                        }
                        else if (Taxing.Contains("Interest on delinquencies"))
                        {
                            result = GlobalClass.Before(Taxing, "Interest on delinquencies");
                        }
                        else if (Taxing.Contains("All major credit cards and debit cards are accepted."))
                        {
                            result = GlobalClass.Before(Taxing, "All major credit cards and debit cards are accepted. ");
                        }
                        else if (Taxing.Contains("Administrative Office Hours"))
                        {
                            result = GlobalClass.Before(Taxing, "Administrative Office Hours");
                            if (result.Contains("will remain along with any accruing interest."))
                            {
                                result = GlobalClass.After(result, "will remain along with any accruing interest.");

                            }
                        }
                        else if (Taxing.Contains("Hours of Operation"))
                        {
                            result = GlobalClass.Before(Taxing, "Hours of Operation");
                        }
                        else if (Taxing.Contains("Office open to the public Monday"))
                        {
                            result = GlobalClass.Before(Taxing, "Office open to the public Monday");
                        }
                        else if (Taxing.Contains("Payments accepted at our Tax Office"))
                        {
                            result = GlobalClass.Before(Taxing, "Payments accepted at our Tax Office");
                        }
                        else if (Taxing.Contains("Regular Office Hours are: "))
                        {
                            result = GlobalClass.Before(Taxing, "Regular Office Hours are: ");
                        }
                        else if (Taxing.Contains("Payments accepted"))
                        {
                            result = GlobalClass.Before(Taxing, "Payments accepted ");
                        }
                        else if (Taxing.Contains("LONG BEACH TOWNSHIP"))
                        {
                            result = "LONG BEACH TOWNSHIP, 6805 Long Beach Blvd, Brant Beach, NJ 08008 ";
                        }
                        else if (Taxing.Contains("CITY OF LINDEN"))
                        {
                            result = gc.Between(Taxing, "Tax Collector.", "OFFICE HOURS");
                        }
                        else if (Taxing.Contains("Borough of Highland"))
                        {
                            result = GlobalClass.Before(Taxing, "The interested listed");
                        }
                    }
                }



            }

            return result;
        }



    }
}