using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ScrapMaricopa.Scrapsource
{
    public class CT_Link
    {
        int i = 0;
        string iA = "", iT = "";
        string urlAssessment = "", taxCollector = "", urlTax = "", result = "";
        string url = "";
        public String[] link(string Districtcode, string District, string countyname)
        {


            //District = District.ToLower();
            //var townshipcode1 = new List<string> { "02", "03", "05", "08", "09", "11", "12", "14", "15", "16", "18", "19", "20", "23", "26", "27", "29", "31", "32" };
            //if (townshipcode1.Exists(x => string.Equals(x, Districtcode, StringComparison.OrdinalIgnoreCase)))
            //{
            //    i = 0;
            //    url = "http://gis.vgsi.com/" + District + "ct/Search.aspx";
            //}
            if (countyname == "Fairfield")
            {
                if (Districtcode == "02")
                {
                    iA = "0";
                    iT = "0";
                    urlAssessment = "http://gis.vgsi.com/bridgeportct/Search.aspx";
                    urlTax = "https://www.mytaxbill.org/inet/bill/home.do?town=bridgeport";
                    result = "City of Bridgeport 45 Lyon Terrace #121, Bridgeport, CT 06604 Phone: (203) 576-7271";

                }
                if (Districtcode == "03")
                {
                    iA = "0";
                    iT = "0";
                    urlAssessment = "http://gis.vgsi.com/brookfieldct/Search.aspx";
                    urlTax = "https://www.mytaxbill.org/inet/bill/home.do?town=brookfield";
                    result = "Town of Brookfield 100 Pocono Road  P. O. Box 5106 Brookfield, CT 06804 Phone:203-775-7304 ";

                }
                if (Districtcode == "05")
                {
                    iA = "0";
                    iT = "0";
                    urlAssessment = "http://gis.vgsi.com/danburyct/Search.aspx";
                    urlTax = "https://www.mytaxbill.org/inet/bill/home.do?town=Danbury";
                    result = "City of Danbury 155 Deer Hill Avenue Danbury, CT 06810 Phone: 203-797-4556";

                }
                if (Districtcode == "08")
                {
                    iA = "0";
                    iT = "0";
                    urlAssessment = "http://gis.vgsi.com/fairfieldct/Search.aspx";
                    urlTax = "https://www.mytaxbill.org/inet/bill/home.do?town=fairfield";
                    result = "Town of Fairfield 611 Old Post Road Fairfield, CT 06824  Phone: 203-256-3100";

                }
                if (Districtcode == "09")
                {
                    iA = "0";
                    iT = "0";
                    urlAssessment = "http://gis.vgsi.com/reddingct/Search.aspx";
                    urlTax = "https://www.mytaxbill.org/inet/bill/home.do?town=redding";
                    result = "Town of Redding 100 Hill Road (Route 107)  P. O. Box 1028  Redding, CT 06875-1028  Phone: 203-938-2706";

                }
                if (Districtcode == "11")
                {
                    iA = "0";
                    iT = "0";
                    urlAssessment = "http://gis.vgsi.com/newtownct/Search.aspx";
                    urlTax = "https://www.mytaxbill.org/inet/bill/home.do?town=newtown";
                    result = "Town of Newtown 3 Primrose Street Newtown, CT 06470  Phone: 203-270-4320";
                }
                if (Districtcode == "12")
                {
                    iA = "0";
                    iT = "0";
                    urlAssessment = "http://gis.vgsi.com/monroect/Search.aspx";
                    urlTax = "https://www.mytaxbill.org/inet/bill/home.do?town=monroe";
                    result = "Town of Monroe 7 Fan Hill Road Monroe, CT 06468  Phone: 203-452-2804";
                }
                if (Districtcode == "14")
                {
                    iA = "0";
                    iT = "0";
                    urlAssessment = "http://gis.vgsi.com/newfairfieldct/Search.aspx";
                    urlTax = "https://www.mytaxbill.org/inet/bill/home.do?town=newfairfield";
                    result = "Town of Newfairfield 4 Brush Hill Road New Fairfield, CT 06812  Phone: 203-312-5620";
                }
                if (Districtcode == "15")
                {
                    iA = "0";
                    iT = "0";
                    urlAssessment = "http://gis.vgsi.com/newtownct/Search.aspx";
                    urlTax = "https://www.mytaxbill.org/inet/bill/home.do?town=newtown";
                    result = "Town of Newtown 4 Brush Hill Road New Fairfield, CT 06812  Phone: 203-312-5620";
                }
                if (Districtcode == "16")
                {
                    iA = "0";
                    iT = "2";
                    urlAssessment = "http://gis.vgsi.com/NorwalkCT/Search.aspx";
                    urlTax = "http://my.norwalkct.org/eTaxbill/eTaxBill.aspx";
                    result = "City of Norwalk 125 East Avenue P. O. Box 5125  Norwalk, CT 06856  Phone: 203-854-7731";
                }
                if (Districtcode == "18")
                {
                    iA = "0";
                    iT = "0";
                    urlAssessment = "http://gis.vgsi.com/reddingct/Search.aspx";
                    urlTax = "https://www.mytaxbill.org/inet/bill/home.do?town=redding";
                    result = "Town of Newfairfield 4 Brush Hill Road New Fairfield, CT 06812  Phone: 203-312-5620";
                }
                if (Districtcode == "19")
                {
                    iA = "0";
                    iT = "0";
                    urlAssessment = "http://gis.vgsi.com/reddingct/Search.aspx";
                    urlTax = "https://www.mytaxbill.org/inet/bill/home.do?town=redding";
                    result = "Town of Redding 100 Hill Road (Route 107) P. O. Box 1028  Redding, CT 06875-1028  Phone: 203-938-2706";
                }
                if (Districtcode == "20")
                {
                    iA = "0";
                    iT = "0";
                    urlAssessment = "http://gis.vgsi.com/reddingct/Search.aspx";
                    urlTax = "https://www.mytaxbill.org/inet/bill/home.do?town=redding";
                    result = "Town of Redding 100 Hill Road (Route 107) P. O. Box 1028  Redding, CT 06875-1028  Phone: 203-938-2706";


                }
                if (Districtcode == "23")
                {
                    iA = "0";
                    iT = "0";
                    urlAssessment = "http://gis.vgsi.com/newtownct/Search.aspx";
                    urlTax = "https://www.mytaxbill.org/inet/bill/home.do?town=newtown";
                    result = "Town of Sandy Hook  4 Brush Hill Road New Fairfield, CT 06812  Phone: 203-312-5620";

                }
                if (Districtcode == "26")
                {
                    iA = "0";
                    iT = "0";
                    urlAssessment = "http://gis.vgsi.com/fairfieldct/Search.aspx";
                    urlTax = "https://www.mytaxbill.org/inet/bill/home.do?town=fairfield";
                    result = "Town of Southfort 4 Brush Hill Road New Fairfield, CT 06812  Phone: 203-312-5620";

                }
                if (Districtcode == "27")
                {
                    iA = "0";
                    iT = "0";
                    urlAssessment = "http://gis.vgsi.com/stamfordct/Search.aspx";
                    urlTax = "https://www.mytaxbill.org/inet/bill/home.do?town=stamford";
                    result = "City of Stamford 888 Washington Boulevard  P. O. Box 10152 Stamford, CT 06901 Phone: 203-977-5888";

                }
                if (Districtcode == "29")
                {
                    iA = "0";
                    iT = "0";
                    urlAssessment = "http://gis.vgsi.com/trumbullct/Search.aspx";
                    urlTax = "https://www.mytaxbill.org/inet/bill/home.do?town=Trumbull";
                    result = "Town of Trumbull 5866 Main Street Trumbull, CT 06611 Phone: 203-452-5024";

                }
                if (Districtcode == "31")
                {
                    iA = "0";
                    iT = "0";
                    urlAssessment = "http://gis.vgsi.com/westportct/Search.aspx";
                    urlTax = "https://www.mytaxbill.org/inet/bill/home.do?town=westport";
                    result = "Town of 110 Myrtle Avenue  Westport, CT 06880 Phone: 203-341-1060";
                }
                if (Districtcode == "32")
                {
                    iA = "0";
                    iT = "0";
                    urlAssessment = "http://gis.vgsi.com/wiltonct/Search.aspx";
                    urlTax = "https://www.mytaxbill.org/inet/bill/home.do?town=wilton";
                    result = "Town of Wilton 238 Danbury Road (Route 7)  Wilton, CT 06897 Phone: 203-563-0125";
                }

                if (Districtcode == "07")
                {
                    iA = "1";
                    iT = "0";
                    urlAssessment = "http://www.propertyrecordcards.com/searchmaster.aspx?towncode=046";
                    urlTax = "https://www.mytaxbill.org/inet/bill/home.do?town=easton";
                    result = "Town of Easton 225 Center Road  Easton, CT 06612 Phone: 203-268-6291";
                }
                if (Districtcode == "21")
                {
                    iA = "1";
                    iT = "0";
                    urlAssessment = "http://www.propertyrecordcards.com/SearchMaster.aspx?towncode=118";
                    urlTax = "https://www.mytaxbill.org/inet/bill/home.do?town=ridgefield";
                    result = "Town of Ridgefield 400 Main Street P. O. Box 299 Ridgefield, CT 06877 Phone: 203-431-2779";
                }
                if (Districtcode == "30")
                {
                    iA = "1";
                    iT = "0";
                    urlAssessment = "http://www.propertyrecordcards.com/SearchMaster.aspx?towncode=157";
                    urlTax = "https://www.mytaxbill.org/inet/bill/home.do?town=weston";
                    result = "Town of Ridgefield 400 Main Street P. O. Box 299 Ridgefield, CT 06877 Phone: 203-431-2779";
                }
                if (Districtcode == "25")
                {
                    iA = "2";
                    iT = "0";
                    urlAssessment = "http://www.propertyrecordcards.com/sherman/searchmaster.aspx?towncode=127";
                    urlTax = "https://www.mytaxbill.org/inet/bill/home.do?town=Sherman";
                    result = "Town of Sherman 9 Route 39 North P. O. Box 39 Sherman, CT 06784 Phone: 860-354-4146";
                }
                if (Districtcode == "01")
                {
                    iA = "3";
                    iT = "0";
                    urlAssessment = "http://bethel.ias-clt.com/parcel.list.php";
                    urlTax = "https://www.mytaxbill.org/inet/bill/home.do?town=bethel";
                    result = "Town of Bethel 1 School Street Bethel, CT 06801 Phone: 203-794-8509";

                }
                if (Districtcode == "06")
                {
                    iA = "4";
                    iT = "1";
                    urlAssessment = "http://assessment.darienct.gov/pt/forms/htmlframe.aspx?mode=content/home.htm";
                    urlTax = "https://gemsnt.com/darien-webtax/search.php?agree=I+AGREE";
                    result = "Town of Darien 2 Renshaw Road Darien, CT 06820 Phone: 203-656-7314";

                }
                if (Districtcode == "22")
                {
                    iA = "5";
                    iT = "0";
                    urlAssessment = "http://burlington.mapxpress.net/";
                    urlTax = "https://www.mytaxbill.org/inet/bill/home.do?town=burlington";
                    result = "Town of Riverside 200 Spielman Highway Burlington, CT 06013";
                }
                if (Districtcode == "24")
                {
                    iA = "5";
                    iT = "0";
                    urlAssessment = "http://shelton.mapxpress.net/";
                    urlTax = "https://www.mytaxbill.org/inet/bill/home.do?town=shelton";
                    result = "City of Shelton 54 Hill Street P. O. Box 364  Shelton, CT 06484";
                }
                if (Districtcode == "28")
                {
                    iA = "6";
                    iT = "0";
                    urlAssessment = "https://qpublic.schneidercorp.com/Application.aspx?App=TownofStratfordCT&PageType=Search";
                    urlTax = "https://www.mytaxbill.org/inet/bill/home.do?town=stratford";
                    result = "Town of Stafford 1 Main Street P. O. Box 11 Stafford Springs, CT 06076 Phone:860-684-1760";
                }
                if (Districtcode == "04")
                {
                    iA = "titleflex";
                    iT = "0";
                    urlAssessment = "";
                    urlTax = "https://www.mytaxbill.org/inet/bill/home.do?town=greenwich";
                    result = "Town of Cos Cob 101 Field Point Road Greenwich, CT 06830 Phone:203-622-789";
                }
                if (Districtcode == "10")
                {
                    iA = "titleflex";
                    iT = "0";
                    urlAssessment = "";
                    urlTax = "https://www.mytaxbill.org/inet/bill/home.do?town=greenwich";
                    result = "Town of Greenwich 101 Field Point Road Greenwich, CT 06830 Phone:203-622-789";
                }
                if (Districtcode == "13")
                {
                    iA = "13";
                    iT = "0";
                    urlAssessment = "http://appraisalonline.newcanaanct.gov/search.php";
                    urlTax = "https://www.mytaxbill.org/inet/bill/home.do?town=newcanaan";
                    result = "Town of New Canaan 77 Main Street  New Canaan, CT 06840 Phone:203-594-3064";
                }
                if (Districtcode == "17")
                {
                    iA = "titleflex";
                    iT = "0";
                    urlAssessment = "";
                    urlTax = "https://www.mytaxbill.org/inet/bill/home.do?town=greenwich";
                    result = "Town of Old Greenwich 77 Main Street  New Canaan, CT 06840 Phone:203-594-3064";
                }
            }
            else if (countyname == "New Haven")
            {
                if (Districtcode == "01")
                {
                    iA = "5";
                    iT = "0";
                    urlAssessment = "http://ansonia.mapxpress.net/portal.asp";
                    urlTax = "https://www.mytaxbill.org/inet/bill/home.do?town=ansonia";
                    result = "City of Ansonia 253 Main Street  Ansonia, CT 06401 Phone:203-736-5910";
                }
                if (Districtcode == "02")
                {
                    iA = "7";
                    iT = "0";
                    urlAssessment = "http://www.beaconfalls-ct.org/Pages/BeaconFallsCT_Assessor/propertycards";
                    urlTax = "https://www.mytaxbill.org/inet/bill/home.do?town=beaconfalls";
                    result = "Town of Beacon Falls 10 Maple Avenue Beacon Falls, CT 06403 Phone:203-723-5244";
                }
                if (Districtcode == "03")
                {
                    iA = "5";
                    iT = "0";
                    urlAssessment = "http://bethany.mapxpress.net/";
                    urlTax = "https://www.mytaxbill.org/inet/bill/home.do?town=Bethany";
                    result = "Town of Bethany 40 Peck Road Bethany, CT 06524";
                }
                if (Districtcode == "04")
                {
                    iA = "0";
                    iT = "0";
                    urlAssessment = "http://gis.vgsi.com/branfordct/Search.aspx";
                    urlTax = "https://www.mytaxbill.org/inet/bill/home.do?town=Branford";
                    result = "Town of Branford 1019 Main Street P. O. Box 150 Branford, CT 06405 Phone:203-315-0672";
                }
                if (Districtcode == "05")
                {
                    iA = "1";
                    iT = "0";
                    urlAssessment = "https://www.propertyrecordcards.com/searchmaster.aspx?towncode=025";
                    urlTax = "https://www.mytaxbill.org/inet/bill/home.do?town=Cheshire";
                    result = "Town of Cheshire 84 South Main Street Cheshire, CT 06410 Phone:203-271-6630";
                }
                if (Districtcode == "06")
                {
                    iA = "5";
                    iT = "3";
                    urlAssessment = "http://derby.mapxpress.net/portal.asp";
                    urlTax = "https://derby.gemsnt.com/s_index.php";
                    result = "City of Derby 1 Elizabeth Street Derby, CT 06418 Phone:203-736-1459";
                }
                if (Districtcode == "07")
                {
                    iA = "1";
                    iT = "0";
                    urlAssessment = "http://www.propertyrecordcards.com/searchmaster.aspx?towncode=044";
                    urlTax = "https://www.mytaxbill.org/inet/bill/home.do?town=EastHaven";
                    result = "Town of East Haven 250 Main Street East Haven, CT 06512 Phone:203-468-3307";
                }
                if (Districtcode == "08")
                {
                    iA = "1";
                    iT = "0";
                    urlAssessment = "http://www.propertyrecordcards.com/SearchMaster.aspx?towncode=060";
                    urlTax = "https://www.mytaxbill.org/inet/bill/home.do?town=Guilford";
                    result = "Town of Guilford 31 Park Street Guilford, CT 06437 Phone:203-453-8014";
                }
                if (Districtcode == "09")
                {
                    iA = "0";
                    iT = "0";
                    urlAssessment = "http://gis.vgsi.com/hamdenct/Search.aspx";
                    urlTax = "https://www.mytaxbill.org/inet/bill/home.do?town=Hamden";
                    result = "Town of Hamden 2750 Dixwell Avenue Hamden, CT 06518 Phone:203-287-7140";
                }
                if (Districtcode == "10")
                {
                    iA = "0";
                    iT = "0";
                    urlAssessment = "http://gis.vgsi.com/madisonct/Search.aspx";
                    urlTax = "https://www.mytaxbill.org/inet/bill/home.do?town=Madison";
                    result = "Town of Madison 8 Campus Drive Madison, CT 06443 Phone:203-245-5641";
                }
                if (Districtcode == "11")
                {
                    iA = "14";
                    iT = "0";
                    urlAssessment = "https://gis.meridenct.gov/meriden/PropertySearch.aspx";
                    urlTax = "https://www.mytaxbill.org/inet/bill/home.do?town=Meriden";
                    result = "City of Meriden 142 East Main Street Meriden, CT 06450 Phone:203-630-4053";
                }
                if (Districtcode == "12")
                {
                    iA = "0";
                    iT = "0";
                    urlAssessment = "http://gis.vgsi.com/middleburyct/Search.aspx";
                    urlTax = "https://www.mytaxbill.org/inet/bill/home.do?town=Middlebury";
                    result = "Town of Middlebury 1212 Whittemore Road Middlebury, CT 06762 Phone:203-758-1373";
                }
                if (Districtcode == "13")
                {
                    iA = "0";
                    iT = "0";
                    urlAssessment = "http://gis.vgsi.com/milfordct/Search.aspx";
                    urlTax = "https://milford-webtax.gemsnt.com/search.php?agree=I+AGREE";
                    result = "Town of Milford 70 West River Street Milford, CT 06460 Phone:203-783-3217";
                }
                if (Districtcode == "14")
                {
                    iA = "5";
                    iT = "0";
                    urlAssessment = "http://naugatuck.mapxpress.net/";
                    urlTax = "https://www.mytaxbill.org/inet/bill/home.do?town=Naugatuck";
                    result = "Borough of Naugatuck 229 Church Street Naugatuck, CT 06770 Phone:203-720-7051";
                }
                if (Districtcode == "15")
                {
                    iA = "0";
                    iT = "0";
                    urlAssessment = "http://gis.vgsi.com/newhavenct/Search.aspx";
                    urlTax = "https://www.mytaxbill.org/inet/bill/home.do?town=NewHaven";
                    result = "City of New Haven 200 Orange Street New Haven, CT 06510 Phone:203-946-8054 ,203-946-7073";
                }
                if (Districtcode == "16")
                {
                    iA = "0";
                    iT = "1";
                    urlAssessment = "http://gis.vgsi.com/northbranfordct/Search.aspx";
                    urlTax = "https://gemsnt.com/northbranford-webtax/search.php?agree=I+AGREE";
                    result = "Town of North Branford 909 Foxon Road P.O Box 287 North Branford, CT 06471 Phone:203-484-6011";
                }
                if (Districtcode == "17")
                {
                    iA = "0";
                    iT = "0";
                    urlAssessment = "http://gis.vgsi.com/northhavenct/Search.aspx";
                    urlTax = "https://www.mytaxbill.org/inet/bill/home.do?town=NorthHaven";
                    result = "Town of North Haven 18 Church Street North Haven, CT 06473 ";
                }
                if (Districtcode == "18")
                {
                    iA = "0";
                    iT = "0";
                    urlAssessment = "http://gis.vgsi.com/orangect/Search.aspx";
                    urlTax = "https://www.mytaxbill.org/inet/bill/home.do?town=Orange";
                    result = "Town of Orange 617 Orange Center Road  Orange, CT 06477 Phone:203-891-4726,203-891-4727";
                }
                if (Districtcode == "19")
                {
                    iA = "0";
                    iT = "0";
                    urlAssessment = "http://gis.vgsi.com/oxfordct/Search.aspx";
                    urlTax = "https://www.mytaxbill.org/inet/bill/home.do?town=Oxford";
                    result = "Town of Oxford 486 Oxford Road (Route 67) Oxford, CT 06478 ";
                }
                if (Districtcode == "20")
                {
                    iA = "1";
                    iT = "0";
                    urlAssessment = "http://www.propertyrecordcards.com/SearchMaster.aspx?towncode=115";
                    urlTax = "https://www.mytaxbill.org/inet/bill/home.do?town=Prospect";
                    result = "Town of Prospect 36 Center Street Prospect, CT 06712 Phone:203-758-4461";
                }
                if (Districtcode == "21")
                {
                    iA = "8";
                    iT = "0";
                    urlAssessment = "http://www.seymourgis.com/mapsearch.asp";
                    urlTax = "https://www.mytaxbill.org/inet/bill/home.do?town=Seymour";
                    result = "Town of Seymour 1 First Street Seymour, CT 06483 Phone:203-888-0517";
                }
                if (Districtcode == "22")
                {
                    iA = "0";
                    iT = "0";
                    urlAssessment = "http://gis.vgsi.com/southburyct/Search.aspx";
                    urlTax = "https://www.mytaxbill.org/inet/bill/home.do?town=Southbury";
                    result = "Town of Southbury 501 Main Street South Southbury, CT 06488-2295 Phone:203-262-0654";
                }
                if (Districtcode == "23")
                {
                    iA = "No Tax";
                    iT = "0";
                    urlAssessment = "";
                    urlTax = "";
                    taxCollector = "";
                }
                if (Districtcode == "24")
                {
                    iA = "1";
                    iT = "0";
                    urlAssessment = "http://www.propertyrecordcards.com/searchmaster.aspx?towncode=151";
                    urlTax = "https://www.mytaxbill.org/inet/bill/home.do?town=Waterbury";
                    result = "City of Waterbury 235 Grand Street Waterbury, CT 06702 (Town Clerk's Office only)All other offices have moved to 26 Kendrick Ave., Waterbury, CT 06723 Phone:203-574-6810*";
                }
                if (Districtcode == "25")
                {
                    iA = "5";
                    iT = "0";
                    urlAssessment = "http://www.westhavengis.com/";
                    urlTax = "https://www.mytaxbill.org/inet/bill/home.do?town=westhaven";
                    result = "City of West Haven 355 Main Street West Haven, CT 06516 Phone:203-937-3525";
                }

                if (Districtcode == "26")
                {
                    iA = "0";
                    iT = "0";
                    urlAssessment = "http://gis.vgsi.com/WolcottCT/Search.aspx";
                    urlTax = "https://www.mytaxbill.org/inet/bill/home.do?town=Wolcott";
                    result = "Town of Wolcott 10 Kenea Avenue Wolcott, CT 06716";
                }
                if (Districtcode == "27")
                {
                    iA = "0";
                    iT = "0";
                    urlAssessment = "http://gis.vgsi.com/WoodbridgeCT/Search.aspx";
                    urlTax = "https://www.mytaxbill.org/inet/bill/home.do?town=Woodbridge";
                    result = "Town of Woodbridge 11 Meetinghouse Lane Woodbridge, CT 06525 Phone:203-389-3425,203-389-3474";
                }

            }
            else if (countyname == "Hartford")
            {
                if (Districtcode == "01")
                {
                    iA = "No Tax";
                    iT = "0";
                    urlAssessment = "";
                    urlTax = "";
                    taxCollector = "";
                }
                if (Districtcode == "02")
                {
                    iA = "5";
                    iT = "0";
                    urlAssessment = "http://www.berlingis.com/";
                    urlTax = "https://www.mytaxbill.org/inet/bill/home.do?town=berlin";
                    result = "Town of Berlin 240 Kensington Road Berlin, CT 06037 Phone:860-828-7023";
                }
                if (Districtcode == "03")
                {
                    iA = "5";
                    iT = "0";
                    urlAssessment = "http://bloomfield.mapxpress.net/portal.asp";
                    urlTax = "https://www.mytaxbill.org/inet/bill/home.do?town=Bloomfield";
                    result = "Town of Bloomfield  800 Bloomfield Avenue  Bloomfield, CT 06002 Phone:860-769-3510";
                }
                if (Districtcode == "04")
                {
                    iA = "0";
                    iT = "0";
                    urlAssessment = "http://gis.vgsi.com/bristolct/Search.aspx";
                    urlTax = "https://www.mytaxbill.org/inet/bill/home.do?town=bristol";
                    result = "City of Bristol 111 North Main Street Bristol, CT 06010 Phone:860-584-6270";
                }
                if (Districtcode == "05")
                {
                    iA = "5";
                    iT = "0";
                    urlAssessment = "http://burlington.mapxpress.net/";
                    urlTax = "https://www.mytaxbill.org/inet/bill/home.do?town=burlington";
                    result = "Town of Burlington 200 Spielman Highway Burlington, CT 06013";
                }
                if (Districtcode == "06")
                {
                    iA = "1";
                    iT = "0";
                    urlAssessment = "http://www.propertyrecordcards.com/SearchMaster.aspx?towncode=023";
                    urlTax = "https://www.mytaxbill.org/inet/bill/home.do?town=Canton";
                    result = "Town of Canton 4 Market Street P. O. Box 168 Collinsville, CT 06022 Phone:860-693-7843";
                }
                if (Districtcode == "08")
                {
                    iA = "9";
                    iT = "0";
                    urlAssessment = "https://www.mapsonline.net/easthartfordct/web_assessor/search.php#sid=d699fc86f03af2b5119bba91c525ff11";
                    urlTax = "https://www.mytaxbill.org/inet/bill/home.do?town=easthartford";
                    result = "Town of East Hartford  740 Main Street East Hartford, CT 06108 Phone:860-291-7250";

                }
                if (Districtcode == "07")
                {
                    iA = "0";
                    iT = "0";
                    urlAssessment = "http://gis.vgsi.com/EastGranbyCT/Search.aspx";
                    urlTax = "https://www.mytaxbill.org/inet/bill/home.do?town=eastgranby";
                    result = "Town of East Granby  9 Center Street  P. O. Box 1858 East Granby, CT 06026 Phone:860-653-2004";

                }
                if (Districtcode == "09")
                {
                    iA = "1";
                    iT = "0";
                    urlAssessment = "http://www.propertyrecordcards.com/searchmaster.aspx?towncode=047";
                    urlTax = "https://www.mytaxbill.org/inet/bill/home.do?town=eastwindsor";
                    result = "Town of East Windsor  11 Rye Street Broad Brook, CT 06016 Phone:860-623-8904";
                }
                if (Districtcode == "10")
                {
                    iA = "0";
                    iT = "0";
                    urlAssessment = "http://gis.vgsi.com/enfieldct/Search.aspx";
                    urlTax = "https://www.mytaxbill.org/inet/bill/home.do?town=Enfield";
                    result = "Town of Enfield 820 Enfield Street Enfield, CT 06082 Phone:860-253-6340";
                }
                if (Districtcode == "11")
                {
                    iA = "1";
                    iT = "0";
                    urlAssessment = "http://www.propertyrecordcards.com/SearchMaster.aspx?towncode=052";
                    urlTax = "https://www.mytaxbill.org/inet/bill/home.do?town=farmington";
                    result = "Town of Farmington  1 Monteith Drive Farmington, CT 06032 Phone:860-675-2340";
                }
                if (Districtcode == "12")
                {
                    iA = "10";
                    iT = "0";
                    urlAssessment = "https://gis.glastonbury-ct.gov/Html5/Index.html?viewer=public";
                    urlTax = "https://www.mytaxbill.org/inet/bill/home.do?town=glastonbury";
                    result = "Town of Glastonbury  2155 Main Street Glastonbury, CT 06033 Phone:860-652-7614";
                }
                if (Districtcode == "13")
                {
                    iA = "0";
                    iT = "0";
                    urlAssessment = "http://gis.vgsi.com/granbyct/Search.aspx";
                    urlTax = "https://www.mytaxbill.org/inet/bill/home.do?town=granby";
                    result = "Town of Granby  15 North Granby Road Granby, CT 06035 Phone:860-844-5315";
                }
                if (Districtcode == "14")
                {
                    iA = "11";
                    iT = "0";
                    urlAssessment = "http://assessor1.hartford.gov/Default.asp";
                    urlTax = "https://www.mytaxbill.org/inet/bill/home.do?town=hartford";
                    result = "City of Hartford  Municipal Building 550 Main Street Hartford, CT 06103 Phone:860-757-9630*";

                }
                if (Districtcode == "15")
                {
                    iA = "";
                    iT = "0";
                    urlAssessment = "";
                    urlTax = "https://www.mytaxbill.org/inet/bill/home.do?town=hartland";
                    result = "Town of Hartland 22 South Road P. O. Box 297 East Hartland, CT 06027 Phone:860-653-0609";
                }
                if (Districtcode == "16")
                {
                    iA = "0";
                    iT = "0";
                    urlAssessment = "http://gis.vgsi.com/manchesterct/Search.aspx";
                    urlTax = "https://www.mytaxbill.org/inet/bill/home.do?town=manchester";
                    result = "Town of Manchester 41 Center Street P. O. Box 191  Manchester, CT 06045-0191 Phone:860-647-3018";
                }
                if (Districtcode == "17")
                {
                    iA = "15";
                    iT = "0";
                    urlAssessment = "https://marlboroughct.mapgeo.io/datasets/properties?abuttersDistance=100&latlng=41.642345%2C-72.458935&panel=search&zoom=12";
                    urlTax = "https://www.mytaxbill.org/inet/bill/home.do?town=Marlborough";
                    result = "Town of Marlborough 26 North Main Street P. O. Box 29 Marlborough, CT 06447 Phone:860-295-6205";
                }
                if (Districtcode == "18")
                {
                    iA = "0";
                    iT = "0";
                    urlAssessment = "http://gis.vgsi.com/newbritainct/Search.aspx";
                    urlTax = "https://www.mytaxbill.org/inet/bill/home.do?town=newbritain";
                    result = "City of New Britain 27 West Main Street New Britain, CT 06051 Phone:860-826-3317";
                }
                if (Districtcode == "19")
                {
                    iA = "1";
                    iT = "0";
                    urlAssessment = "http://www.propertyrecordcards.com/SearchMaster.aspx?towncode=094";
                    urlTax = "https://www.mytaxbill.org/inet/bill/home.do?town=newington";
                    result = "Town of Newington  131 Cedar Street  Newington, CT 06111 Phone:860-665-8540";
                }
                if (Districtcode == "20")
                {
                    iA = "3";
                    iT = "0";
                    urlAssessment = "http://plainville.ias-clt.com/parcel.list.php";
                    urlTax = "https://www.mytaxbill.org/inet/bill/home.do?town=Plainville";
                    result = "Town of Plainville 1 Central Square  Plainville, CT 06062 Phone:860-793-0221";
                }
                if (Districtcode == "21")
                {
                    iA = "9";
                    iT = "0";
                    urlAssessment = "https://www.mapsonline.net/rockyhillct/web_assessor/search.php#sid=fce48222e085b2d49f7f9751e2a199c7";
                    urlTax = "https://www.mytaxbill.org/inet/bill/home.do?town=rockyhill";
                    result = "Town of Rocky Hill 761 Old Main Street Rocky Hill, CT 06067 Phone:860-258-2717";
                }
                if (Districtcode == "22")
                {
                    iA = "1";
                    iT = "0";
                    urlAssessment = "http://www.propertyrecordcards.com/searchmaster.aspx?towncode=128";
                    urlTax = "https://www.mytaxbill.org/inet/bill/home.do?town=simsbury";
                    result = "Town of Simsbury 933 Hopmeadow Street Simsbury, CT 06070 Phone:860-658-3238";
                }
                if (Districtcode == "23")
                {
                    iA = "0";
                    iT = "0";
                    urlAssessment = "http://gis.vgsi.com/southwindsorct/Search.aspx";
                    urlTax = "https://www.mytaxbill.org/inet/bill/home.do?town=southwindsor";
                    result = "Town of South Windsor 1540 Sullivan Avenue South Windsor, CT 06074";
                }
                if (Districtcode == "24")
                {
                    iA = "0";
                    iT = "1";
                    urlAssessment = "http://gis.vgsi.com/southingtonct/Search.aspx";
                    urlTax = "https://gemsnt.com/southington-webtax/search.php?agree=I+AGREE";
                    result = "Town of Southington 75 Main Street P. O. Box 152 Southington, CT 06489 Phone:860-276-6259";
                }
                if (Districtcode == "25")
                {
                    iA = "0";
                    iT = "0";
                    urlAssessment = "http://gis.vgsi.com/SuffieldCT/Search.aspx";
                    urlTax = "https://www.mytaxbill.org/inet/bill/home.do?town=suffield";
                    result = "Town of Suffield 83 Mountain Road Suffield, CT 06078 Phone:860-668-3841";
                }
                if (Districtcode == "26")
                {
                    iA = "0";
                    iT = "0";
                    urlAssessment = "http://gis.vgsi.com/westhartfordct/Search.aspx";
                    urlTax = "https://www.mytaxbill.org/inet/bill/home.do?town=westhartford";
                    result = "Town of West Hartford 50 South Main Street West Hartford, CT 06107 Phone:860-561-7474";
                }
                if (Districtcode == "27")
                {
                    iA = "1";
                    iT = "0";
                    urlAssessment = "https://www.propertyrecordcards.com/SearchMaster.aspx?towncode=159";
                    urlTax = "https://www.mytaxbill.org/inet/bill/home.do?town=wethersfield";
                    result = "Town of Wethersfield 505 Silas Deane Highway Wethersfield, CT 06109 Phone:860-721-2825";
                }
                if (Districtcode == "28")
                {
                    iA = "12";
                    iT = "0";
                    urlAssessment = "https://info.townofwindsorct.com/gis/";
                    urlTax = "https://www.mytaxbill.org/inet/bill/home.do?town=windsor";
                    result = "Town of Windsor 275 Broad Street  Windsor, CT 06095 Phone:860-285-1810";
                }
                if (Districtcode == "29")
                {
                    iA = "0";
                    iT = "0";
                    urlAssessment = "http://gis.vgsi.com/WindsorLocksCT/Search.aspx";
                    urlTax = "https://www.mytaxbill.org/inet/bill/home.do?town=windsorlocks";
                    result = "Town of Windsor Locks 50 Church Street Windsor Locks, CT 06096, CT 06840 Phone:860-627-1415";
                }
            }
            //  string count = i.ToString();
            return new string[] { urlAssessment, urlTax, iA, iT, result };
        }
    }
}