using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ScrapMaricopa.Scrapsource
{
    public class NJ_Link
    {
        int i = 0;
        public String[] link(string District, string countyname)
        {
            string url = "", taxCollector = "";

            //No tax
            //var distirctCode_Notax = new List<string> { "1203", "1208", "1213", "1215", "0202", "0206", "0207","0209", "0211", "0215", "0218", "0221", "0222", "0224", "0225", "0226", "0228", "0232", "0234", "0235", "0237", "0240", "0245","0249","0250", "0252", "0254", "0262", "0264","0265", "0314", "0325", "0328", "0336", "0339", "0340", "1815", "1819","0402", "0407", "0410", "0419", "0420", "0421", "0427", "0429", "0432", "0433", "0437", "0901", "0902", "0903", "0904", "0910", "0911", "1302", "1305", "1306", "1315", "1323", "1346", "1335", "1341", "1345", "1347", "1349", "2004", "2007", "2015", "2021", "1504", "1505", "1509", "1514", "1520", "1522", "1523", "1531", "1532" };

            //if (distirctCode_Notax.Exists(x => string.Equals(x, District, StringComparison.OrdinalIgnoreCase)))
            //{
            //    i = 10;
            //    url = "Tax Not Available";
            //}



            //Middlesex
            if (countyname == "Middlesex")
            {
                var distirctCode_middlesex = new List<string> { "1201", "1202", "1204", "1205", "1206", "1207", "1209", "1210", "1212", "1214", "1216", "1218", "1220", "1221", "1222", "1223", "1224", "1225", };

                if (distirctCode_middlesex.Exists(x => string.Equals(x, District, StringComparison.OrdinalIgnoreCase)))
                {
                    i = 0;
                    url = "https://wipp.edmundsassoc.com/Wipp/?wippid=" + District;
                }
                else if (District == "1211")
                {
                    i = 1;
                    url = "https://www.cit-e.net/milltown-nj/Cit-e-Access/TaxBill_Std/TaxAmount.cfm?TID=144&TPID=13890";
                    taxCollector = "http://www.milltownnj.org/160/Tax-Collection";
                }
                else if (District == "1217")
                {
                    i = 1;
                    url = "https://www.cit-e.net/piscataway-nj/Cit-e-Access/TaxBill_Std/TaxAmount.cfm?TID=125&TPID=12112";
                    taxCollector = "http://www.piscatawaynj.org/tax_collect/tax-collector";
                }
                else if (District == "1219")
                {
                    i = 1;
                    url = "https://www.cit-e.net/sayreville-nj/Cit-e-Access/TaxBill_Std/TaxAmount.cfm?TID=87&TPID=8974";
                    taxCollector = "http://www.sayreville.com/Cit-e-Access/ContactInfo/?TID=87&TPID=8641";

                }
            }
            //Bergen
            if (countyname == "Bergen")
            {
                var distirctCode_bergen = new List<string> { "0201", "0205", "0208", "0210", "0214", "0217", "0220", "0223", "0227", "0229", "0230", "0231", "0233", "0236", "0241", "0242", "0243", "0244", "0247", "0248", "0249", "0251", "0253", "0258", "0259", "0260", "0261", "0263", "0266", "0267", "0268", "0269", "0270" };


                if (distirctCode_bergen.Exists(x => string.Equals(x, District, StringComparison.OrdinalIgnoreCase)))
                {
                    i = 0;
                    url = "https://wipp.edmundsassoc.com/Wipp/?wippid=" + District;
                }
                else if (District == "0203")
                {
                    i = 1;
                    url = "https://www.cit-e.net/bergenfield-nj/Cit-e-Access/TaxBill_Std/TaxAmount.cfm?TID=171&TPID=15731";
                    taxCollector = "https://www.cit-e.net/bergenfield-nj/Cit-e-Access/TaxBill_Std/TaxAmount.cfm?TID=171&TPID=15731";
                }
                else if (District == "0213")
                {
                    i = 1;
                    url = "https://www.cit-e.net/edgewater-nj/Cit-e-Access/TaxBill_Std/TaxAmount.cfm?TID=114&TPID=16253";
                    taxCollector = "https://www.cit-e.net/edgewater-nj/Cit-e-Access/TaxBill_Std/TaxAmount.cfm?TID=114&TPID=16253";
                }
                else if (District == "0204")
                {
                    i = 1;
                    url = "https://www.cit-e.net/bogota_nj/Cit-e-Access/TaxBill_Std/TaxAmount.cfm?TID=140&TPID=13345";
                    taxCollector = "https://www.bogotaonline.org/";
                }
                else if (District == "0212")
                {
                    i = 1;
                    url = "https://www.cit-e.net/rutherford-nj/Cit-e-Access/TaxBill_Std/TaxAmount.cfm?TID=167&TPID=15571";
                    taxCollector = "https://www.rutherfordboronj.com/departments/tax-collector/";
                }

                else if (District == "0216")
                {
                    i = 1;
                    url = "https://www.cit-e.net/englewoodcliffs-nj/Cit-e-Access/TaxBill_Std/TaxAmount.cfm?TID=154&TPID=14779";
                    taxCollector = "https://www.cit-e.net/englewoodcliffs-nj/Cit-e-Access/TaxBill_Std/TaxAmount.cfm?TID=154&TPID=14779";

                }
                else if (District == "0219")
                {
                    i = 1;
                    url = "https://www.cit-e.net/fortlee-nj/Cit-e-Access/TaxBill_Std/TaxAmount.cfm?TID=111&TPID=11282";
                    taxCollector = "https://www.cit-e.net/fortlee-nj/Cit-e-Access/TaxBill_Std/TaxAmount.cfm?TID=111&TPID=11282";

                }
                else if (District == "0238")
                {
                    i = 1;
                    url = "https://www.cit-e.net/newmilford-nj/Cit-e-Access/TaxBill_Std/TaxAmount.cfm?TID=183&TPID=16524";
                    taxCollector = "http://www.newmilfordboro.com/page/180013952/180057731/Contact-Directory";
                }
                else if (District == "0246")
                {
                    i = 1;
                    url = "https://www.cit-e.net/paramus-nj/Cit-e-Access/TaxBill_Std/TaxAmount.cfm?TID=175&TPID=15866";
                    taxCollector = "https://www.cit-e.net/paramus-nj/Cit-e-Access/TaxBill_Std/TaxAmount.cfm?TID=175&TPID=15866";

                }
                else if (District == "0256")
                {
                    i = 1;
                    url = "https://www.cit-e.net/rutherford-nj/Cit-e-Access/TaxBill_Std/TaxAmount.cfm?TID=167&TPID=15571";
                    taxCollector = "https://www.rutherfordboronj.com/departments/tax-collector/";
                }
                else if (District == "0239")
                {
                    i = 2;
                    url = "https://www.cit-e.net/northarlington-nj/cit-e-access/taxinquiry/?TID=178&TPID=16162";
                    taxCollector = "https://www.northarlington.org/contactus";
                }
                else if (District == "0255")
                {
                    i = 3;
                    url = "http://munidex.com/apps/appmain.aspx?m=0255&ma=1";
                    taxCollector = "http://www.rockleighnj.org/contact/";
                }
                else if (District == "0257") //Saddle Brook

                {
                    i = 3;
                    url = "http://www.munidex.com/apps/appmain.aspx?m=0257&ma=1";
                    taxCollector = "https://saddlebrooknj.us/saddle-brook-tax-collectors-office/home/";
                }
                else if (District == "0250") //ridgefieldpark

                {
                    i = 3;
                    url = "http://www.munidex.com/apps/appmain.aspx?m=0250&ma=1";
                    taxCollector = "https://www.ridgefieldpark.org/about-us";
                }

            }
            //Burlington
            if (countyname == "Burlington")
            {
                var distirctCode_burlington = new List<string> { "0301", "0302", "0303", "0304", "0305", "0306", "0307", "0308", "0309", "0310", "0311", "0312", "0313", "0315", "0316", "0317", "0318", "0319", "0320", "0321", "0322", "0323", "0324", "0326", "0327", "0329", "0330", "0331", "0332", "0333", "0334", "0335", "0337", "0338" };


                if (distirctCode_burlington.Exists(x => string.Equals(x, District, StringComparison.OrdinalIgnoreCase)))
                {
                    i = 0;
                    url = "https://wipp.edmundsassoc.com/Wipp/?wippid=" + District;
                }
            }

            //Essex-NJ
            if (countyname == "Essex")
            {
                var distirctCode_Essex = new List<string> { "0701", "0702", "0703", "0705", "0708", "0709", "0711", "0713", "0716", "0717", "0718", "0720", "0721" };

                if (distirctCode_Essex.Exists(x => string.Equals(x, District, StringComparison.OrdinalIgnoreCase)))
                {
                    i = 0;
                    url = "https://wipp.edmundsassoc.com/Wipp/?wippid=" + District;
                }
                else if (District == "0704")
                {
                    i = 1;
                    url = "https://www.cit-e.net/cedargrove-nj/Cit-e-Access/TaxBill_Std/TaxAmount.cfm?TID=174&TPID=15864";
                    taxCollector = "https://cedargrovenj.org/";
                }
                else if (District == "0707")
                {
                    i = 1;
                    url = "https://www.cit-e.net/fairfield_nj/Cit-e-Access/TaxBill_Std/TaxAmount.cfm?TID=158&TPID=15189";
                    taxCollector = "";
                }
                else if (District == "0710")
                {
                    i = 1;
                    url = "https://www.cit-e.net/livingston-nj/Cit-e-Access/TaxBill_Std/TaxAmount.cfm?TID=177&TPID=16014";
                    taxCollector = "https://www.cit-e.net/livingston-nj/Cit-e-Access/TaxBill_Std/TaxAmount.cfm?TID=177&TPID=16014";
                }
                else if (District == "0719")
                {
                    i = 1;
                    url = "https://www.cit-e.net/s_orange-nj/Cit-e-Access/TaxBill_Std/TaxAmount.cfm?TID=53&TPID=5548";
                    taxCollector = "http://www.southorange.org/301/Tax-Collector";
                }
                else if (District == "0722")
                {
                    i = 1;
                    url = "https://www.cit-e.net/westorange-nj/Cit-e-Access/TaxBill_Std/TaxAmount.cfm?TID=145&TPID=13893";
                    taxCollector = "http://www.westorange.org/";
                }

                else if (District == "0706")
                {
                    i = 4;
                    url = "https://www.cit-e.net/essexfells-nj/cn/TaxBill_Std/TaxAmount.cfm?TPID=16756";
                    taxCollector = "http://www.essexfellsboro.com/Departments/Finance/";
                }
                else if (District == "0712")
                {
                    i = 5;
                    url = "http://www.millburntwp.msitax.org:5600/";
                    taxCollector = "http://nj-millburntownship.civicplus.com/181/Tax-Collector";
                }
                else if (District == "0714")
                {
                    i = 6;
                    url = "http://taxes.ci.newark.nj.us/";
                    taxCollector = "https://www.newarknj.gov/";
                }

            }
            //Somerset_NJ
            if (countyname == "Somerset")
            {
                var distirctCode_Somerset = new List<string> { "1802", "1803", "1805", "1811", "1814", "1816", "1820", "1821" };


                if (distirctCode_Somerset.Exists(x => string.Equals(x, District, StringComparison.OrdinalIgnoreCase)))
                {
                    i = 0;
                    url = "https://wipp.edmundsassoc.com/Wipp/?wippid=" + District;
                }

                else if (District == "1806")
                {
                    i = 1;
                    url = "https://www.cit-e.net/bridgewater-nj/Cit-e-Access/TaxBill_Std/TaxAmount.cfm?TID=116&TPID=11646";
                    taxCollector = "https://www.bridgewaternj.gov/tax-collector/";
                }
                else if (District == "1807")
                {
                    i = 7;
                    url = "http://taxlookup.njtown.net/propertytax.aspx?cd=1807";
                    taxCollector = "https://www.farhillsnj.org/tax_and_sewer_collector.php";
                }
                else if (District == "1808")
                {
                    i = 1;
                    url = "https://www.cit-e.net/franklin-nj/Cit-e-Access/TaxBill_Std/TaxAmount.cfm?TID=119&TPID=11711";
                    taxCollector = "https://www.franklintwpnj.org/government/departments/tax-and-water-collection/contact-us";
                }
                else if (District == "1809")
                {
                    i = 1;
                    url = "https://www.cit-e.net/green_brook-nj/Cit-e-Access/TaxBill_Std/TaxAmount.cfm?TID=157&TPID=14934";
                    taxCollector = "http://greenbrooktwp.org/Default.aspx";
                }
                else if (District == "1810")
                {
                    i = 1;
                    url = "https://www.cit-e.net/hillsborough-nj/Cit-e-Access/TaxBill_Std/TaxAmount.cfm?TID=45&TPID=5357";
                    taxCollector = "http://www.hillsborough-nj.org/";
                }
                else if (District == "1813")
                {
                    i = 1;
                    url = "https://www.cit-e.net/montgomery-nj/Cit-e-Access/TaxBill_Std/TaxAmount.cfm?TID=101&TPID=10526";
                    taxCollector = "https://www.cit-e.net/montgomery-nj/Cit-e-Access/TaxBill_Std/TaxAmount.cfm?TID=101&TPID=10526";
                }
                else if (District == "1818")
                {
                    i = 1;
                    url = "https://www.cit-e.net/somerville-nj/Cit-e-Access/TaxBill_Std/TaxAmount.cfm?TID=118&TPID=11707";
                    taxCollector = "https://www.cit-e.net/somerville-nj/Cit-e-Access/TaxBill_Std/TaxAmount.cfm?TID=118&TPID=11707";
                }
                else if (District == "1804")
                {
                    i = 2;
                    url = "https://www.cit-e.net/boundbrook-nj/Cit-e-Access/taxinquiry/?TID=173&TPID=15890";
                    taxCollector = "https://boundbrook-nj.org/";
                }
                else if (District == "1812")
                {
                    i = 5;
                    url = "http://millstoneboro.dyndns.org/index.html";
                    taxCollector = "https://www.millstoneboro.org/about/contact";
                }
                else if (District == "1817")
                {
                    i = 5;
                    url = "http://50.248.130.217/index.html";
                    taxCollector = "http://www.rockyhill-nj.gov/borough-contacts";
                }
                else if (District == "1801")
                {
                    i = 7;
                    url = "http://taxlookup.njtown.net/propertytax.aspx?cd=1801";
                    taxCollector = "https://www.mapquest.com/us/new-jersey/bedminster-township-tax-collector-363177008";
                }

            }

            //Camden
            if (countyname == "Camden")
            {
                var distirctCode_Camden = new List<string> { "0403", "0401", "0404", "0405", "0406", "0408", "0409", "0411", "0412", "0413", "0415", "0416", "0417", "0418", "0422", "0423", "0424", "0425", "0426", "0428", "0430", "0431", "0434", "0435", "0436" };

                if (distirctCode_Camden.Exists(x => string.Equals(x, District, StringComparison.OrdinalIgnoreCase)))
                {
                    i = 0;
                    url = "https://wipp.edmundsassoc.com/Wipp/?wippid=" + District;
                }

                else if (District == "0414")
                {
                    i = 5;
                    url = "http://tax.cityofgloucester.org/index.html";
                    taxCollector = "https://www.cityofgloucester.org/directory/";
                }

            }

            //Hudson
            if (countyname == "Hudson")
            {
                var distirctCode_Hudson = new List<string> { "0905", "0907", "0908" };


                if (distirctCode_Hudson.Exists(x => string.Equals(x, District, StringComparison.OrdinalIgnoreCase)))
                {
                    i = 0;
                    url = "https://wipp.edmundsassoc.com/Wipp/?wippid=" + District;
                }

                else if (District == "0912")
                {
                    i = 2;
                    url = "https://www.cit-e.net/westnewyork-nj/cit-e-access/taxinquiry/?TID=168&TPID=16226";
                    taxCollector = ""; //"http://www.westnewyorknj.org";
                }
                else if (District == "0909")
                {
                    i = 3;
                    url = "http://www.munidex.com/apps/appmain.aspx?m=0909&ma=1";
                    taxCollector = "https://secaucusnj.gov/departments/tax-collector.html";
                }
                else if (District == "0906")
                {
                    i = 6;
                    url = "http://taxes.cityofjerseycity.com/";
                    taxCollector = "https://www.jerseycitynj.gov/CityHall/taxes";
                }

            }
            //Monmouth
            if (countyname == "Monmouth")
            {
                var distirctCode_Monmouth = new List<string> { "1301", "1303", "1304", "1307", "1308", "1309", "1310", "1311", "1312", "1313", "1314", "1316", "1317", "1318", "1319", "1320", "1321", "1322", "1324", "1325", "1327", "1328", "1329", "1331", "1334", "1336", "1337", "1338", "1339", "1340", "1342", "1343", "1344", "1348", "1350", "1351", "1352", "1353" };


                if (distirctCode_Monmouth.Exists(x => string.Equals(x, District, StringComparison.OrdinalIgnoreCase)))
                {
                    i = 0;
                    url = "https://wipp.edmundsassoc.com/Wipp/?wippid=" + District;
                }

                else if (District == "1332")
                {
                    i = 1;
                    url = "https://www.cit-e.net/middletown-nj/Cit-e-Access/TaxBill_Std/TaxAmount.cfm?TID=63&TPID=6455";
                    taxCollector = "https://www.middletownnj.org/Directory.aspx?did=37";
                }
                else if (District == "1333")
                {
                    i = 1;
                    url = "https://www.cit-e.net/millstone-nj/Cit-e-Access/TaxBill_Std/TaxAmount.cfm?TID=85&TPID=8376";
                    taxCollector = "https://www.millstonenj.gov/taxoffice.html";
                }
                else if (District == "1330")
                {
                    i = 5;
                    url = "http://msi.mymarl.com/";
                    taxCollector = "http://www.marlboro-nj.gov/tax_collector_division_main.html";
                }

            }

            //Union
            if (countyname == "Union")
            {
                var distirctCode_Monmouth = new List<string> { "2002", "2003", "2005", "2006", "2008", "2009", "2010", "2011", "2014", "2018", "2019", "2020", };


                if (distirctCode_Monmouth.Exists(x => string.Equals(x, District, StringComparison.OrdinalIgnoreCase)))
                {
                    i = 0;
                    url = "https://wipp.edmundsassoc.com/Wipp/?wippid=" + District;

                }

                else if (District == "2016")
                {
                    i = 1;
                    url = "https://www.cit-e.net/scotchplains-nj/Cit-e-Access/TaxBill_Std/TaxAmount.cfm?TID=172&TPID=15734";
                    taxCollector = "https://www.scotchplainsnj.gov/departments/taxes-finance/";
                }
                else if (District == "2017")
                {
                    i = 1;
                    url = "https://www.cit-e.net/springfield-nj/Cit-e-Access/TaxBill_Std/TaxAmount.cfm?TID=132&TPID=12728";
                    taxCollector = "https://www.cit-e.net/springfield-nj/Cit-e-Access/TaxBill_Std/TaxAmount.cfm?TID=132&TPID=12728";
                }
                else if (District == "2012")
                {
                    i = 1;
                    url = "https://www.cit-e.net/plainfield-nj/Cit-e-Access/TaxBill_Std/TaxAmount.cfm?TID=123&TPID=12099https://www.cit-e.net/plainfield-nj/Cit-e-Access/TaxBill_Std/TaxAmount.cfm?TID=123&TPID=12099";
                    taxCollector = "https://www.cit-e.net/plainfield-nj/Cit-e-Access/TaxBill_Std/TaxAmount.cfm?TID=123&TPID=12099";
                }
                else if (District == "2013")
                {
                    i = 8;
                    url = "https://apps.hlssystems.com/Rahway/PropertyTaxInquiry";
                    taxCollector = "https://www.cityofrahway.org/departments/revenue-finance/tax-collector/";
                }
                else if (District == "2001")
                {
                    i = 4;
                    url = "https://www.cit-e.net/berkeleyheights-nj/Cit-e-Access/TaxBill_Std/TaxAmount.cfm?TID=164&TPID=15451";
                    taxCollector = "https://www.cit-e.net/berkeleyheights-nj/Cit-e-Access/TaxBill_Std/TaxAmount.cfm?TID=164&TPID=15451";
                }

            }

            //Ocean
            if (countyname == "Ocean")
            {
                var distirctCode_Monmouth = new List<string> { "1501", "1502", "1503", "1506", "1508", "1510", "1511", "1512", "1515", "1516", "1517", "1518", "1519", "1521", "1524", "1525", "1526", "1527", "1528", "1529", "1530", "1533" };

                if (distirctCode_Monmouth.Exists(x => string.Equals(x, District, StringComparison.OrdinalIgnoreCase)))
                {
                    i = 0;
                    url = "https://wipp.edmundsassoc.com/Wipp/?wippid=" + District;

                }

                else if (District == "1507")
                {
                    i = 8;
                    url = "https://apps.hlssystems.com/Brick/PropertyTaxInquiry";
                    taxCollector = "http://www.bricktownship.net/index.php/departments/tax-collector/";
                }
                else if (District == "1513")
                {
                    i = 8;
                    url = "https://apps.hlssystems.com/Lacey/PropertyTaxInquiry";
                    taxCollector = "http://laceytownship.org/content/4700/3668/default.aspx";
                }
            }

            //Morris
            if (countyname == "Morris")
            {
                var distirctCode_Monmouth = new List<string> { "1401", "", "1405", "1406", "1410", "1415", "1416", "1422", "1424", "1426", "1427", "1438" };

                if (distirctCode_Monmouth.Exists(x => string.Equals(x, District, StringComparison.OrdinalIgnoreCase)))
                {
                    i = 0;
                    url = "https://wipp.edmundsassoc.com/Wipp/?wippid=" + District;

                }
                else if (District == "1429")
                {
                    i = 1;
                    url = "https://www.cit-e.net/parsippany-nj/Cit-e-Access/TaxBill_Std/TaxAmount.cfm?TID=35&TPID=4685";
                    taxCollector = "http://www.parsippany.net/";
                }
                else if (District == "1403")
                {
                    i = 1;
                    url = "https://www.cit-e.net/butler-nj/Cit-e-Access/TaxBill_Std/TaxAmount.cfm?TID=19&TPID=9348";
                    taxCollector = "http://www.butlerborough.com/Cit-e-Access/ContactInfo/?TID=19&TPID=2402";
                }
                else if (District == "1404")
                {
                    i = 1;
                    url = "https://www.cit-e.net/chatham-nj/Cit-e-Access/TaxBill_Std/TaxAmount.cfm?TID=43&TPID=4826";
                    taxCollector = "https://www.cit-e.net/chatham-nj/Cit-e-Access/TaxBill_Std/TaxAmount.cfm?TID=43&TPID=4826";
                }
                else if (District == "1407")
                {
                    i = 1;
                    url = "https://www.cit-e.net/chester-nj/Cit-e-Access/TaxBill_Std/TaxAmount.cfm?TID=96&TPID=10274";
                    taxCollector = "https://www.cit-e.net/chester-nj/Cit-e-Access/TaxBill_Std/TaxAmount.cfm?TID=96&TPID=10274";
                }
                else if (District == "1412")
                {
                    i = 1;
                    url = "https://www.cit-e.net/hanover-nj/Cit-e-Access/TaxBill_Std/TaxAmount.cfm?TID=49&TPID=5392";
                    taxCollector = "http://hanovertownship.com/";
                }
                else if (District == "1413")
                {
                    i = 1;
                    url = "https://www.cit-e.net/harding-nj/Cit-e-Access/TaxBill_Std/TaxAmount.cfm?TID=163&TPID=15406";
                    taxCollector = "https://www.cit-e.net/harding-nj/Cit-e-Access/TaxBill_Std/TaxAmount.cfm?TID=163&TPID=15406";
                }
                else if (District == "1414")
                {
                    i = 1;
                    url = "https://www.cit-e.net/jefferson-nj/Cit-e-Access/TaxBill_Std/TaxAmount.cfm?TID=4&TPID=7668";
                    taxCollector = "http://www.jeffersontownship.net/Cit-e-Access/ContactInfo/?TID=4&TPID=122";
                }
                else if (District == "1417")
                {
                    i = 1;
                    url = "https://www.cit-e.net/madison-nj/Cit-e-Access/TaxBill_Std/TaxAmount.cfm?TID=86&TPID=8378";
                    taxCollector = "https://www.cit-e.net/madison-nj/Cit-e-Access/TaxBill_Std/TaxAmount.cfm?TID=86&TPID=8378";
                }
                else if (District == "1421")
                {
                    i = 1;
                    url = "https://www.cit-e.net/montville-nj/Cit-e-Access/TaxBill_Std/TaxAmount.cfm?TID=90&TPID=9078";
                    taxCollector = "http://www.montvillenj.org/";
                }
                else if (District == "1430")
                {
                    i = 1;
                    url = "https://www.cit-e.net/longhill-nj/Cit-e-Access/TaxBill_Std/TaxAmount.cfm?TID=170&TPID=15728";
                    taxCollector = "https://www.cit-e.net/longhill-nj/Cit-e-Access/TaxBill_Std/TaxAmount.cfm?TID=170&TPID=15728";
                }
                else if (District == "1432")
                {
                    i = 1;
                    url = "https://www.cit-e.net/randolph-nj/Cit-e-Access/TaxBill_Std/TaxAmount.cfm?TID=48&TPID=5369";
                    taxCollector = "https://www.cit-e.net/randolph-nj/Cit-e-Access/TaxBill_Std/TaxAmount.cfm?TID=48&TPID=5369";
                }
                else if (District == "1435")
                {
                    i = 1;
                    url = "https://www.cit-e.net/rockaway-nj/Cit-e-Access/TaxBill_Std/TaxAmount.cfm?TID=109&TPID=11183";
                    taxCollector = "https://www.rockawaytownship.org/217/Tax-Collector";
                }
                else if (District == "1436")
                {
                    i = 1;
                    url = "https://www.cit-e.net/roxbury-nj/Cit-e-Access/TaxBill_Std/TaxAmount.cfm?TID=100&TPID=10403";
                    taxCollector = "http://www.roxburynj.us/16/Tax-Utilities-Collector";
                }
                else if (District == "1409")
                {
                    i = 4;
                    url = "https://www.cit-e.net/dover-nj-new/cn/TaxBill_Std/TaxAmount.cfm?TPID=9791";
                    taxCollector = "http://www.dover.nj.us/cn/ContactInfo/?tpid=2315";
                }
                else if (District == "1418")
                {
                    i = 7;
                    url = "http://taxlookup.njtown.net/propertytax.aspx?cd=1418";
                    taxCollector = "http://www.mendhamnj.org/Cit-e-Access/webpage.cfm?TID=94&TPID=9757";
                }
                else if (District == "1419")
                {
                    i = 4;
                    url = "https://www.cit-e.net/mendham_twshp-nj/cn/TaxBill_Std/TaxAmount.cfm?TPID=7694";
                    taxCollector = "http://www.mendhamtownship.org/cn/ContactInfo/?tpid=9844";
                }
                else if (District == "1420")
                {
                    i = 5;
                    url = "http://www.minehillboro.msitax.org/";
                    taxCollector = "http://minehill.com/township-departments/tax-water-sewer/";
                }
                else if (District == "1423")
                {
                    i = 7;
                    url = "http://taxlookup.njtown.net/propertytax.aspx?cd=1423";
                    taxCollector = "http://www.morrisplainsboro.org/directory/town-directory";
                }
                else if (District == "1425")
                {
                    i = 5;
                    url = "http://173.220.138.230/";
                    taxCollector = "https://mtnlakes.org/departments/finance-office/";
                }
                else if (District == "1431")
                {
                    i = 5;
                    url = "http://96.57.162.138/";
                    taxCollector = "http://www.peqtwp.org/Cit-e-Access/webpage.cfm?TID=60&TPID=6302";
                }
                else if (District == "1434")
                {
                    i = 7;
                    url = "http://taxlookup.njtown.net/propertytax.aspx?cd=1434";
                    taxCollector = "http://www.rockawayborough.org/departments";
                }
                else if (District == "1439")
                {
                    i = 5;
                    url = "http://96.56.16.106:81/";
                    taxCollector = "http://www.whartonnj.com/index.php/contacts";
                }

            }














            string count = i.ToString();
            return new string[] { url, count, taxCollector };
        }

    }
}