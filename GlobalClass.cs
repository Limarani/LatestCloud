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
using System.Net;
using System.Xml;
using System.Text.RegularExpressions;
using OpenQA.Selenium.PhantomJS;
using OpenQA.Selenium.Support.Extensions;
using OpenQA.Selenium.Firefox;
using System.Diagnostics;
using OpenQA.Selenium.Remote;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using System.Text;
using Newtonsoft.Json;

namespace ScrapMaricopa
{
    public class GlobalClass
    {
        DBconnection newcon = new DBconnection();
        TitleflexConnection tcon = new TitleflexConnection();
        string con = ConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;
        MySqlDataAdapter mDa;
        MySqlCommand mCmd;
        MySqlDataReader mDr;
        DataSet ds = new DataSet();        
        #region Variable Declareation
        MySqlParameter[] mParam;
        MySqlCommand cmd;
        DataSet dst = new DataSet();
        DataSet ds1 = new DataSet();
        MySqlConnection mConnection = new MySqlConnection();
        DataView dataview;
        string strInput = ConfigurationManager.AppSettings["TitleFlexInput"];
        string strOutput = ConfigurationManager.AppSettings["TitleFlexOutput"];
        #endregion
        List<string> addrTitle = new List<string>();
        List<string> nameTitle = new List<string>();
        List<string> parcelTitle = new List<string>();
        string outputparcel = "";
        string AlternateAPN = "";
        DataSet dsFetch = new DataSet();
        //public static string today = Convert.ToString(DateTime.Now);
        public static string titleparcel = "";
        public static string TitleFlexAssess = "";
        public static string sname = "";
        public static string cname = "";
        public static string stateYear1 = "";
        public static string stateYear2 = "";
        public static string multipleParcel = "";
        public static string multiParcel_la = "";
        public static string parcelNumber_la = "";
        public static string multiparcel_gw = "";
        public static string multiparcel_ORMulttomah = "";
        public static string multiparcel_CAAlamenda = "";
        public static string multiparcel_WAPierce = "";
        public static string imgURL = "";
        public static string global_orderNo = "";
        public static string global_parcelNo = "";
        public static string parcel_status = "";
        public static string multiparcel_dc = "";
        public static string multiparcel_StLouis = "";
        public static string multipleParcel_deKalb = "";
        public static string multiParcel_washoe = "";
        public static string multiParcel_mecklenberg = "";
        public static string multiParcel_CAKern = "";
        public static string multiParcel_OHFranklin = "";
        public static string SpecialHandling_SanFrancisco = "";
        public static string multiparcel_CASanJoaquin = "";
        public static string multiparcel_CAfreshno = "";
        public static string result_CAfreshno = "";
        public static string multiparcel_NMBernalillo = "";
        public static string multiParcel_NMBernalillo_count = "";
        public static string multiparcel_Tulsa = "";
        public static string multiparcel_Tulsa_count = "";
        public static string multiParcel_StCharles = "";
        public static string multiParcel_StCharles_Multicount = "";
        public static string deliquent_harrison = "";
        public static string MobileTax_harrison = "";
        public static string multiparcel_harrison = "";
        public static string multiParcel_Summit = "";
        public static string multiparcel_Utah = "";
        public static string multiParcel_Utah_Multicount = "";
        public static string multiparcel_NCDelaware = "";
        public static string multiParcel_NCDelaware_count = "";
        public static string multiparcel_Pinal = "";
        public static string TitleFlex_Pinal = "";
        public static string multiParcel_Marion = "";
        public static string delinquent_Hennepin = "";
        public static string multiparcel_Hennepin = "";
        public static string TitleFlex_Search = "";
        public static string multiparcel_Anoka = "";
        public static string multiParcel_Denver = "";
        public static string multiParcel_washoe_count = "";
        public static string multipleParcel_deKalb_count = "";
        public static string multiparcel_StLouis_count = "";
        public static string multiParcel_mecklenberg_count = "";
        public static string multiparcel_gw_count = "";
        public static string multiParcel_CAKern_count = "";
        public static string multiParcel_OHFranklin_Multicount = "";
        public static string multiParcel_Placer = "";
        public static string delinquent_Placer = "";
        public static string multiparcel_ORClackamas = "";
        public static string multiParcel_ORClackamas_count = "";
        public static string multiParcel_LAEastBatonRouge = "";
        public static string multiParcel_LAEastBatonRouge_Multicount = "";
        public static string multiParcel_TNShelby = "";
        public static string multiParcel_ORDeschutes = "";
        public static string multiParcel_ORDeschutes_Multicount = "";
        public static string multiParcel_Baltimore = "";
        public static string multiparcel_Polk = "";
        public static string multiparcel_Pasco = "";
        public static string multiParcel_SCCharleston = "";
        public static string multiParcel_Douglas = "";
        public static string multiParcel_CAEldorado = "";
        public static string multiParcel_Jefferson = "";
        public static string multiParcel_FlVolusia = "";
        public static string multiParcel_sarasota = "";
        public static string multiParcel_sarasota_count = "";
        public static string multiParcel_CASantaBarbara = "";
        public static string multiParcel_CASantaBarbara_Multicount = "";
        public static string multiPArcel_OHHamilton = "";
        public static string multiParcel_OHHamilton_count = "";
        public static string multiPArcel_FLBroward = "";
        public static string multiparcel_Stark = "";
        public static string multiparcel_Polk_Maximum = "";
        public static string multiparcel_duval = "";
        public static string multiParcel_CAContraCosta = "";
        public static string multiParcel_CAContraCosta_Multicount = "";
        public static string multiparcel_Madison = "";
        public static string multiParcel_Harford = "";

        public static string multiparcel_alameda = "";
        public static string multiParcel_MiamiDade = "";
        public static string multiParcel_MiamiDade_Multicount = "";
        public static string multiParcel_Hillsborough = "";
        public static string multiParcel_FLPalmBeach = "";
        public static string multiParcel_FLPalmBeach_Multicount = "";
        public static string multiParcel_Cherokee = "";
        public static IWebDriver sDriver;

        public string Between(string Text, string FirstString, string LastString)
        {

            string STR = Text;
            string STRFirst = FirstString;
            string STRLast = LastString;
            string FinalString;
            int Pos1 = STR.IndexOf(FirstString) + FirstString.Length;
            int Pos2 = STR.IndexOf(LastString);
            FinalString = STR.Substring(Pos1, Pos2 - Pos1);
            FinalString = FinalString.Replace(System.Environment.NewLine, string.Empty);
            return FinalString;

        }
        public static string Before(string value, string a)
        {
            int posA = value.IndexOf(a);
            if (posA == -1)
            {
                return "";
            }
            return value.Substring(0, posA);
        }
        public static string After(string value, string a)
        {
            int posA = value.LastIndexOf(a);
            if (posA == -1)
            {
                return "";
            }
            int adjustedPosA = posA + a.Length;
            if (adjustedPosA >= value.Length)
            {
                return "";
            }
            return value.Substring(adjustedPosA);
        }

        public void insert_data(string orderno, DateTime today, string parcelno, int FieldID, string FieldValue, int Table)
        {

            mParam = new MySqlParameter[5];
            mParam[0] = new MySqlParameter("?$order_no", orderno);
            mParam[0].MySqlDbType = MySqlDbType.VarChar;
            mParam[0].IsNullable = false;

            mParam[1] = new MySqlParameter("?$parcel_no", parcelno);
            mParam[1].MySqlDbType = MySqlDbType.VarChar;

            mParam[2] = new MySqlParameter("?$field_id", FieldID);
            mParam[2].MySqlDbType = MySqlDbType.Int16;

            mParam[3] = new MySqlParameter("?$field_value", FieldValue);
            mParam[3].MySqlDbType = MySqlDbType.VarChar;

            mParam[4] = new MySqlParameter("?$istable", Table);
            mParam[4].MySqlDbType = MySqlDbType.Int16;

            newcon.ExecuteSPNonQuery("sp_InsertDate", true, mParam);

        }
        public void insert_date(string orderno, string parcelno, int FieldID, string FieldValue, int Table, DateTime sdate)
        {

            mParam = new MySqlParameter[6];
            mParam[0] = new MySqlParameter("?$order_no", orderno);
            mParam[0].MySqlDbType = MySqlDbType.VarChar;
            mParam[0].IsNullable = false;

            mParam[1] = new MySqlParameter("?$parcel_no", parcelno);
            mParam[1].MySqlDbType = MySqlDbType.VarChar;

            mParam[2] = new MySqlParameter("?$field_id", FieldID);
            mParam[2].MySqlDbType = MySqlDbType.Int16;

            mParam[3] = new MySqlParameter("?$field_value", FieldValue);
            mParam[3].MySqlDbType = MySqlDbType.VarChar;

            mParam[4] = new MySqlParameter("?$istable", Table);
            mParam[4].MySqlDbType = MySqlDbType.Int16;

            mParam[5] = new MySqlParameter("?$sdate", sdate);
            mParam[5].MySqlDbType = MySqlDbType.DateTime;

            newcon.ExecuteSPNonQuery("sp_InsertDate", true, mParam);

        }
        public void CreatePdf(string orderno, string parcelno, string pdfName, IWebDriver driver, string sname, string cname)
        {
            string outputPath = ReturnPath(sname, cname);
            outputPath = outputPath + orderno + "\\" + parcelno + "\\";
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }
            string img = outputPath + pdfName + ".png";
            string pdf = outputPath + pdfName + ".pdf";

            driver.Manage().Window.Maximize();
            driver.TakeScreenshot().SaveAsFile(img, ScreenshotImageFormat.Png);

            WebDriverTest.ConvertImageToPdf(img, pdf);
            if (File.Exists(img))
            {
                File.Delete(img);
            }

        }
        public void CreatePdf_WOP(string orderno, string pdfName, IWebDriver driver, string sname, string cname)
        {
            string outputPath = ReturnPath(sname, cname);
            outputPath = outputPath + orderno + "\\";
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }
            string img = outputPath + pdfName + ".png";
            string pdf = outputPath + pdfName + ".pdf";

            driver.Manage().Window.Maximize();
            driver.TakeScreenshot().SaveAsFile(img, ScreenshotImageFormat.Png);

            WebDriverTest.ConvertImageToPdf(img, pdf);
            if (File.Exists(img))
            {
                File.Delete(img);
            }

        }

        public void CreatePdf_Chrome(string orderno, string parcelno, string pdfName, IWebDriver driver, string sname, string cname)
        {
            string outputPath = ReturnPath(sname, cname);

            outputPath = outputPath + orderno + "\\" + parcelno + "\\";
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }
            string img = outputPath + pdfName + ".png";
            string pdf = outputPath + pdfName + ".pdf";

            driver.TakeScreenshot().SaveAsFile(img, ScreenshotImageFormat.Png);
            WebDriverTest.ConvertImageToPdf(img, pdf);
            if (File.Exists(img))
            {
                File.Delete(img);
            }

        }
        public void CreatePdf_WOP_Chrome(string orderno, string pdfName, IWebDriver driver, string sname, string cname)
        {
            string outputPath = ReturnPath(sname, cname);
            outputPath = outputPath + orderno + "\\";
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }
            string img = outputPath + pdfName + ".png";
            string pdf = outputPath + pdfName + ".pdf";

            driver.TakeScreenshot().SaveAsFile(img, ScreenshotImageFormat.Png);

            WebDriverTest.ConvertImageToPdf(img, pdf);
            if (File.Exists(img))
            {
                File.Delete(img);
            }

        }
        public static void CombineMultiplePDFs(List<string> fileNames, string outFile)
        {
            try
            {
                // step 1: creation of a document-object
                Document document = new Document();

                // step 2: we create a writer that listens to the document
                PdfCopy writer = new PdfCopy(document, new FileStream(outFile, FileMode.Create));
                if (writer == null)
                {
                    return;
                }

                // step 3: we open the document
                document.Open();

                foreach (string fileName in fileNames)
                {
                    try
                    {
                        PdfReader reader = new PdfReader(fileName);
                        PdfReader.unethicalreading = true;
                        reader.ConsolidateNamedDestinations();

                        // step 4: we add content
                        for (int i = 1; i <= reader.NumberOfPages; i++)
                        {
                            PdfImportedPage page = writer.GetImportedPage(reader, i);
                            writer.AddPage(page);
                        }

                        PRAcroForm form = reader.AcroForm;
                        if (form != null)
                        {
                            writer.AddDocument(reader);
                        }

                        reader.Close();
                    }
                    catch
                    {

                    }
                }

                // step 5: we close the document and writer
                writer.Close();
                document.Close();
            }
            catch
            {

            }
        }
        public void mergpdf(string orderno, string sname, string cname)
        {
            string outputPath = ReturnPath(sname, cname);
            //string pdfDistnation = System.Web.HttpContext.Current.Server.MapPath("~/MergePDF\\") + orderno + ".pdf";
            string pdfDistnation = ConfigurationManager.AppSettings["pdfMergePath"] + orderno + ".pdf";

            outputPath = outputPath + orderno;
            List<string> fileList = GlobalClass.DirSearch(outputPath);
            var orderedFileList = fileList.Select(path => new FileInfo(path))
                    .OrderBy(x => x.CreationTime)
                    .Select(x => x.FullName)
                    .ToList();
            GlobalClass.CombineMultiplePDFs(orderedFileList, pdfDistnation);
            if (cname == "Madera" || cname == "San Luis Obispo" || cname == "Napa")
            {
                string pdfPlacer = ConfigurationManager.AppSettings["pdfPlacerTitle"] + orderno + "\\";
                if (!Directory.Exists(pdfPlacer))
                {
                    Directory.CreateDirectory(pdfPlacer);
                }
                pdfPlacer = pdfPlacer + "tax statement.pdf";
                GlobalClass.CombineMultiplePDFs(orderedFileList, pdfPlacer);
            }
        }
        public static List<String> DirSearch(string sDir)
        {
            List<String> filelist = new List<String>();
            try
            {
                foreach (string f in Directory.GetFiles(sDir))
                {
                    filelist.Add(f);
                }
                foreach (string d in Directory.GetDirectories(sDir))
                {
                    filelist.AddRange(DirSearch(d));
                }
            }
            catch (System.Exception excpt)
            {
                //MessageBox.Show(excpt.Message);
            }

            return filelist;
        }
        public static void LogError(Exception ex, string ordernumber)
        {

            string fileName = "ScrappingError.txt";

            string outputPath = ConfigurationManager.AppSettings["error_log"];
            // outputPath = sourcePath + strDate + "\\" + CategoryObjectList.OrderID;

            if (!System.IO.Directory.Exists(outputPath))
            {
                System.IO.Directory.CreateDirectory(outputPath);
            }
            string sourceFile = System.IO.Path.Combine(outputPath, fileName);
            if (!File.Exists(sourceFile))
            {
                File.Create(sourceFile).Dispose();
            }

            string message = string.Format("Time: {0}", DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt"));
            message += Environment.NewLine;
            message += "-----------------------------------------------------------";
            message += Environment.NewLine;
            message += string.Format("Message: " + ordernumber + " {0}", ex.Message);
            message += Environment.NewLine;
            message += string.Format("StackTrace: {0}", ex.StackTrace);
            message += Environment.NewLine;
            message += string.Format("Source: {0}", ex.Source);
            message += Environment.NewLine;
            message += string.Format("TargetSite: {0}", ex.TargetSite.ToString());
            message += Environment.NewLine;
            message += "-----------------------------------------------------------";
            message += Environment.NewLine;

            using (StreamWriter writer = new StreamWriter(sourceFile, true))
            {
                writer.WriteLine(message);
                writer.Close();
            }
        }

        public DataTable GridDisplay(string Query)
        {

            DataSet ds = newcon.ExecuteQuery(Query);
            DataTable dTable = new DataTable();
            if (ds.Tables[0].Rows.Count > 0)
            {
                string data_text_id = ds.Tables[0].Rows[0]["Data_Field_Text_Id"].ToString();
                string order_no = ds.Tables[0].Rows[0]["order_no"].ToString();

                DataSet dsField = newcon.ExecuteQuery("select Data_Fields_Text from data_field_master where id='" + data_text_id + "'");
                string columnName = "order_no" + "~" + "Parcel_No" + "~" + dsField.Tables[0].Rows[0]["Data_Fields_Text"].ToString();
                string[] columnArray = columnName.Split('~');


                foreach (string cName in columnArray)
                {
                    dTable.Columns.Add(cName);
                }
                DataRow dr = dTable.NewRow();
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    dTable.Rows.Add();

                }

                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    string rowvals = order_no + "~" + ds.Tables[0].Rows[i]["parcel_no"].ToString() + "~" + ds.Tables[0].Rows[i]["Data_Field_value"].ToString();
                    string[] rowValue = rowvals.Split('~');
                    for (int k = 0; k < rowValue.Count(); k++)
                    {
                        dTable.Rows[i][k] = rowValue[k];

                    }
                }
            }

            return dTable;

        }
        public void downloadfile(string downloadURL, string orderno, string parcelno, string filename, string sname, string cname)
        {
            string outputPath = ReturnPath(sname, cname);
            string billpdf = outputPath + orderno + "\\" + parcelno + "\\" + filename + ".pdf";
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            WebClient downloadpdf = new WebClient();
            downloadpdf.DownloadFile(downloadURL, billpdf);
        }

        public void downloadfileHeader(string downloadURL, string orderno, string parcelno, string filename, string sname, string cname, IWebDriver driver)
        {
            string outputPath = ReturnPath(sname, cname);
            string billpdf = outputPath + orderno + "\\" + parcelno + "\\" + filename + ".pdf";

            using (WebClient wc = new WebClient())
            {
                wc.Headers[HttpRequestHeader.Cookie] = GetCookieHeaderString(driver);
                wc.DownloadFile(downloadURL, billpdf);
            }


        }

        public string ReturnPath(string sname, string cname)
        {
            string outputPath = "";
            MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString);
            string query = "SELECT Storage_Path FROM state_county_master where State_Name = '" + sname + "' and County_Name='" + cname + "'";
            MySqlCommand cmd = new MySqlCommand(query, con);
            con.Open();
            MySqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                outputPath = dr["Storage_Path"].ToString();
            }
            dr.Close();
            con.Close();
            return outputPath;
        }
        public DataSet GetCountyId(string sname, string cname)
        {

            MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString);
            string query = "SELECT * FROM state_county_master where State_Name = '" + sname + "' and County_Name='" + cname + "'";
            MySqlCommand cmd = new MySqlCommand(query, con);
            con.Open();
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            return ds;

        }
        public string ReturnStType(string name)
        {
            MySqlConnection ftp = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString);
            ftp.Open();
            MySqlCommand cmd = new MySqlCommand("SELECT short_name from street_type WHERE name = '" + name.ToUpper() + "'", ftp);
            MySqlDataReader reader = cmd.ExecuteReader();
            String result = null;
            while (reader.Read())
            {
                result = reader["short_name"].ToString();
            }
            reader.Close();
            ftp.Close();
            return result;
        }

        public string[] ReadPdfFile(string orderno, string parcelno, string filename, string sname, string cname)
        {

            string outputPath = ReturnPath(sname, cname);
            string billPath = outputPath + orderno + "\\" + parcelno + "\\" + filename + ".pdf";

            PdfReader reader = new PdfReader(billPath);
            int PageNum = reader.NumberOfPages;
            string[] words = new string[] { };
            string line;

            for (int i = 1; i <= PageNum; i++)
            {
                string text = PdfTextExtractor.GetTextFromPage(reader, i, new LocationTextExtractionStrategy());

                words = text.Split('\n');
                for (int j = 0, len = words.Length; j < len; j++)
                {
                    line = Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(words[j]));
                }
            }
            return words;
        }

        public void AutoDownloadFileSpokane(string orderno, string parcelno, string county, string state, string fileName)
        {

            string outputPath = ReturnPath(state, county);
            outputPath = outputPath + orderno + "\\" + parcelno + "\\";
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            // string pathUser = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            //string pathDownload = Path.Combine(pathUser, "Downloads\\");
            string pathDownload = ConfigurationManager.AppSettings["AutoPdf"];
            pathDownload = pathDownload + fileName;
            string destNameWithNumber = outputPath + fileName;
            if (File.Exists(outputPath + fileName))
            {
                string destpath = outputPath + fileName;

                destNameWithNumber = GetFreeFileNumber(destpath);
                // File.Move(destpath, destNameWithNumber);

            }
            File.Copy(pathDownload, destNameWithNumber, true);
            if (File.Exists(pathDownload))
            {
                File.Delete(pathDownload);
            }
        }
        private Regex fileNumber = new Regex("\\d+$", RegexOptions.Compiled);
        private string GetFreeFileNumber(string path)
        {
            string pathOnly = path.Substring(0, path.LastIndexOf('\\') + 1);
            string nameOnly = System.IO.Path.GetFileNameWithoutExtension(path);
            string extOnly = System.IO.Path.GetExtension(path);
            string[] files = Directory.GetFiles(pathOnly, nameOnly + "*" + extOnly);
            int largest = files.Max(f => GetFileNumber(f));
            return string.Format("{0}{1}{2}{3}", pathOnly, nameOnly, largest + 1, extOnly);
        }
        private int GetFileNumber(string file)
        {
            Match m = fileNumber.Match(System.IO.Path.GetFileNameWithoutExtension(file));
            if (!m.Success) return 0;
            return int.Parse(m.Value);
        }

        static string GetCookieHeaderString(IWebDriver driver)
        {
            var cookies = driver.Manage().Cookies.AllCookies;
            return string.Join("; ", cookies.Select(c => string.Format("{0}={1}", c.Name, c.Value)));
        }

        //titlrflex
        public void TitleFlexSearch(string orderNumber, string parcelNumber, string ownerName, string address, string state, string county)
        {
            if (orderNumber.All(char.IsDigit))
            {
                if (address.Trim() != "" && parcelNumber.Trim() == "" && ownername.Trim() == "")
                {
                    DataSet dsoutput = new DataSet();
                    dsoutput = Fetch_Output_Data(orderNumber, state, county);
                    var totaldays = 0;

                    if (dsoutput.Tables[0].Rows.Count > 0)
                    {
                        DateTime lastupdateddate = Convert.ToDateTime(dsoutput.Tables[0].Rows[0]["LASTUPDATED_DATE"].ToString());
                        DateTime currentdate = Convert.ToDateTime(dsoutput.Tables[0].Rows[0]["CURRENTT_DATE"].ToString());
                        TimeSpan dayss = lastupdateddate - currentdate;
                        totaldays = dayss.Days;
                        dsFetch = tcon.ExecuteQuery("select OutputParcel,AlternateAPN from tbl_output where orderno = '" + orderNumber + "' order by lastupdateddate desc limit 1 ");
                        if (dsFetch.Tables[0].Rows.Count > 0)
                        {
                            if (dsFetch.Tables[0].Rows[0]["OutputParcel"].ToString() != "")
                            {
                                HttpContext.Current.Session["titleparcel"] = dsFetch.Tables[0].Rows[0]["OutputParcel"].ToString();
                                readoutput(HttpContext.Current.Session["titleparcel"].ToString());
                                return;
                            }
                            else if (dsFetch.Tables[0].Rows[0]["AlternateAPN"].ToString() != "")
                            {
                                HttpContext.Current.Session["titleparcel"] = dsFetch.Tables[0].Rows[0]["AlternateAPN"].ToString();
                                readoutput1(HttpContext.Current.Session["titleparcel"].ToString());
                                return;
                            }
                            else if (dsFetch.Tables[0].Rows[0]["AlternateAPN"].ToString() == "" && dsFetch.Tables[0].Rows[0]["OutputParcel"].ToString() == "")
                            {
                               // DataSet ds = new DataSet();
                                string bindquery = "select orderno,parcelno,ownername,address,county,state from multiparcels where orderno = '" + orderNumber + "' group by parcelno";
                                dst = tcon.ExecuteQuery(bindquery);
                                if (dst.Tables[0].Rows.Count > 0)
                                {
                                    GlobalClass.TitleFlex_Search = "Yes";
                                    HttpContext.Current.Session["TitleFlex_Search"] = "Yes";
                                    return;
                                }

                                else
                                {
                                    HttpContext.Current.Session["titleparcel"] = "";
                                    return;
                                }
                                
                            }
                        }
                    }



                    if (totaldays > 29 || dsoutput.Tables[0].Rows.Count == 0)
                    {
                        insert_titleflex(orderNumber, DateTime.Now.ToString(), address, county, "", state, "", ownerName, "", "", parcelNumber, "STARS");
                        XmlDocument XD = new XmlDocument();
                        XmlNode MESSAGE = XD.AppendChild(XD.CreateElement("MESSAGE"));
                        XmlNode DEAL_SETS = MESSAGE.AppendChild(XD.CreateElement("DEAL_SETS"));
                        XmlNode DEAL_SET = DEAL_SETS.AppendChild(XD.CreateElement("DEAL_SET"));
                        XmlAttribute DEAL_SETChildAtt = DEAL_SET.Attributes.Append(XD.CreateAttribute("SequenceNumber"));
                        DEAL_SETChildAtt.InnerText = "";
                        XmlNode DEALS = DEAL_SET.AppendChild(XD.CreateElement("DEALS"));
                        XmlNode DEAL = DEALS.AppendChild(XD.CreateElement("DEAL"));
                        XmlAttribute DEAL_ChildAtt = DEAL.Attributes.Append(XD.CreateAttribute("SequenceNumber"));
                        XmlAttribute DEAL_ChildAtt1 = DEAL.Attributes.Append(XD.CreateAttribute("MISMOReferenceModelIdentifier"));
                        XmlAttribute DEAL_ChildAtt2 = DEAL.Attributes.Append(XD.CreateAttribute("MISMOLogicalDataDictionaryIdentifier"));
                        DEAL_ChildAtt.InnerText = "1";
                        DEAL_ChildAtt1.InnerText = "";
                        DEAL_ChildAtt2.InnerText = "";

                        #region parties
                        XmlNode PARTIES = DEAL.AppendChild(XD.CreateElement("PARTIES"));

                        #region PARTY1
                        XmlNode PARTY1 = PARTIES.AppendChild(XD.CreateElement("PARTY"));
                        XmlAttribute DPARTY1ChildAtt = PARTY1.Attributes.Append(XD.CreateAttribute("SequenceNumber"));
                        DPARTY1ChildAtt.InnerText = "1";

                        // INDIVIDUAL

                        XmlNode INDIVIDUAL1 = PARTY1.AppendChild(XD.CreateElement("INDIVIDUAL"));
                        XmlNode NAME1 = INDIVIDUAL1.AppendChild(XD.CreateElement("NAME"));

                        if (state == "KY" && county == "Fayette")
                        {
                            string[] ownersplit = ownerName.Trim().Split(' ');
                            if (ownersplit.Count() == 2)
                            {
                                XmlNode EducationalAchievementsDescription1 = NAME1.AppendChild(XD.CreateElement("EducationalAchievementsDescription"));

                                XmlNode FirstName1 = NAME1.AppendChild(XD.CreateElement("FirstName"));
                                FirstName1.InnerText = ownersplit[1].Trim();

                                XmlNode MiddleName1 = NAME1.AppendChild(XD.CreateElement("MiddleName"));
                                ///  MiddleName1.InnerText = txtmiddlename.Text.Trim();

                                XmlNode LastName1 = NAME1.AppendChild(XD.CreateElement("LastName"));
                                LastName1.InnerText = ownersplit[0].Trim();
                            }
                            else if (ownersplit.Count() == 3)
                            {
                                XmlNode EducationalAchievementsDescription1 = NAME1.AppendChild(XD.CreateElement("EducationalAchievementsDescription"));
                                XmlNode FirstName1 = NAME1.AppendChild(XD.CreateElement("FirstName"));
                                FirstName1.InnerText = ownersplit[2].Trim();

                                XmlNode MiddleName1 = NAME1.AppendChild(XD.CreateElement("MiddleName"));
                                MiddleName1.InnerText = ownersplit[1].Trim();

                                XmlNode LastName1 = NAME1.AppendChild(XD.CreateElement("LastName"));
                                LastName1.InnerText = ownersplit[0].Trim();
                            }
                            else if (ownersplit.Count() == 1)
                            {
                                XmlNode EducationalAchievementsDescription1 = NAME1.AppendChild(XD.CreateElement("EducationalAchievementsDescription"));
                                XmlNode FirstName1 = NAME1.AppendChild(XD.CreateElement("FirstName"));
                                // FirstName1.InnerText = ownersplit[2].Trim();

                                XmlNode MiddleName1 = NAME1.AppendChild(XD.CreateElement("MiddleName"));
                                //  MiddleName1.InnerText = ownersplit[1].Trim();

                                XmlNode LastName1 = NAME1.AppendChild(XD.CreateElement("LastName"));
                                LastName1.InnerText = ownersplit[0].Trim();
                            }
                        }
                        else
                        {
                            XmlNode EducationalAchievementsDescription1 = NAME1.AppendChild(XD.CreateElement("EducationalAchievementsDescription"));

                            XmlNode FirstName1 = NAME1.AppendChild(XD.CreateElement("FirstName"));
                            // FirstName1.InnerText = txtfirstname.Text.Trim();

                            XmlNode MiddleName1 = NAME1.AppendChild(XD.CreateElement("MiddleName"));
                            ///  MiddleName1.InnerText = txtmiddlename.Text.Trim();

                            XmlNode LastName1 = NAME1.AppendChild(XD.CreateElement("LastName"));
                            LastName1.InnerText = ownerName.Trim();
                        }

                        XmlNode FullName1 = NAME1.AppendChild(XD.CreateElement("FullName"));
                        XmlNode PrefixName1 = NAME1.AppendChild(XD.CreateElement("PrefixName"));
                        XmlNode SuffixName1 = NAME1.AppendChild(XD.CreateElement("SuffixName"));
                        XmlNode EXTENSION1 = NAME1.AppendChild(XD.CreateElement("EXTENSION"));
                        XmlNode IND1EXTENSION = INDIVIDUAL1.AppendChild(XD.CreateElement("EXTENSION"));

                        //Adress

                        XmlNode ADDRESSES1 = PARTY1.AppendChild(XD.CreateElement("ADDRESSES"));
                        XmlNode ADDRESS1 = ADDRESSES1.AppendChild(XD.CreateElement("ADDRESS"));

                        XmlAttribute ADDRESS1ChildAtt = ADDRESS1.Attributes.Append(XD.CreateAttribute("SequenceNumber"));
                        ADDRESS1ChildAtt.InnerText = "";

                        XmlNode AddressType1 = ADDRESS1.AppendChild(XD.CreateElement("AddressType"));
                        AddressType1.InnerText = "Primary";
                        XmlNode AddressLineText1 = ADDRESS1.AppendChild(XD.CreateElement("AddressLineText"));
                        XmlAttribute AddressLineText1_ChildAtt1 = DEAL.Attributes.Append(XD.CreateAttribute("lang"));
                        XmlAttribute AddressLineText1_ChildAtt2 = DEAL.Attributes.Append(XD.CreateAttribute("SensitiveIndicator"));
                        AddressLineText1_ChildAtt1.InnerText = "";
                        AddressLineText1_ChildAtt2.InnerText = "";
                        AddressLineText1.InnerText = address.Trim();

                        XmlNode AddressTypeOtherDescription1 = ADDRESS1.AppendChild(XD.CreateElement("AddressTypeOtherDescription"));
                        XmlNode AddressUnitDesignatorType1 = ADDRESS1.AppendChild(XD.CreateElement("AddressUnitDesignatorType"));
                        AddressUnitDesignatorType1.InnerText = "LOT";
                        XmlNode AddressUnitDesignatorTypeOtherDescription1 = ADDRESS1.AppendChild(XD.CreateElement("AddressUnitDesignatorTypeOtherDescription"));
                        XmlNode AddressUnitIdentifier1 = ADDRESS1.AppendChild(XD.CreateElement("AddressUnitIdentifier"));
                        XmlNode CountryName1 = ADDRESS1.AppendChild(XD.CreateElement("CountryName"));
                        XmlNode CountryCode1 = ADDRESS1.AppendChild(XD.CreateElement("CountryCode"));
                        XmlNode StateName1 = ADDRESS1.AppendChild(XD.CreateElement("StateName"));
                        XmlNode StateCode1 = ADDRESS1.AppendChild(XD.CreateElement("StateCode"));
                        StateCode1.InnerText = state.Trim();
                        XmlNode CountyName1 = ADDRESS1.AppendChild(XD.CreateElement("CountyName"));
                        CountyName1.InnerText = county.Trim();
                        XmlNode CountyCode1 = ADDRESS1.AppendChild(XD.CreateElement("CountyCode"));
                        XmlNode AddressLineText12 = ADDRESS1.AppendChild(XD.CreateElement("AddressLineText"));
                        XmlNode AddressAdditionalLineText1 = ADDRESS1.AppendChild(XD.CreateElement("AddressAdditionalLineText"));
                        XmlNode CityName1 = ADDRESS1.AppendChild(XD.CreateElement("CityName"));
                        //CityName1.InnerText = txtcity.Text.Trim();
                        XmlNode PlusFourZipCode1 = ADDRESS1.AppendChild(XD.CreateElement("PlusFourZipCode"));
                        XmlNode PostalCode1 = ADDRESS1.AppendChild(XD.CreateElement("PostalCode"));
                        //  PostalCode1.InnerText = txtzip.Text.Trim();


                        //Extension APN

                        XmlNode exten = ADDRESS1.AppendChild(XD.CreateElement("EXTENSION"));

                        //  XmlNode propid = exten.AppendChild(XD.CreateElement("PropertyID"));

                        XmlNode legal = exten.AppendChild(XD.CreateElement("LEGAL_DESCRIPTIONS"));
                        XmlAttribute legal_ChildAtt1 = legal.Attributes.Append(XD.CreateAttribute("title"));
                        XmlAttribute legal_ChildAtt2 = legal.Attributes.Append(XD.CreateAttribute("role"));
                        XmlAttribute legal_ChildAtt3 = legal.Attributes.Append(XD.CreateAttribute("label"));
                        XmlAttribute legal_ChildAtt4 = legal.Attributes.Append(XD.CreateAttribute("type"));

                        XmlNode legal1 = legal.AppendChild(XD.CreateElement("LEGAL_DESCRIPTION"));
                        XmlAttribute legal1_ChildAtt1 = legal1.Attributes.Append(XD.CreateAttribute("title"));
                        XmlAttribute legal1_ChildAtt2 = legal1.Attributes.Append(XD.CreateAttribute("role"));
                        XmlAttribute legal1_ChildAtt3 = legal1.Attributes.Append(XD.CreateAttribute("label"));
                        XmlAttribute legal1_ChildAtt4 = legal1.Attributes.Append(XD.CreateAttribute("type"));

                        XmlNode parcelid1 = legal1.AppendChild(XD.CreateElement("PARCEL_IDENTIFICATIONS"));

                        XmlNode parcelid2 = parcelid1.AppendChild(XD.CreateElement("PARCEL_IDENTIFICATION"));

                        XmlNode ParcelIdentificationType = parcelid2.AppendChild(XD.CreateElement("ParcelIdentificationType"));
                        ParcelIdentificationType.InnerText = "ParcelIdentificationNumber";

                        XmlNode parceldescr = parcelid2.AppendChild(XD.CreateElement("ParcelIdentificationTypeOtherDescription"));
                        XmlAttribute legal1parceldescr_ChildAtt1 = parceldescr.Attributes.Append(XD.CreateAttribute("lang"));
                        XmlAttribute egal1lparceldescr_ChildAtt2 = parceldescr.Attributes.Append(XD.CreateAttribute("SensitiveIndicator"));

                        XmlNode Parcelidenti = parcelid2.AppendChild(XD.CreateElement("ParcelIdentifier"));
                        XmlAttribute Parcelidenti_ChildAtt1 = Parcelidenti.Attributes.Append(XD.CreateAttribute("SensitiveIndicator"));
                        XmlAttribute Parcelidenti_ChildAtt2 = Parcelidenti.Attributes.Append(XD.CreateAttribute("IdentifierEffectiveDate"));
                        XmlAttribute Parcelidenti_ChildAtt3 = Parcelidenti.Attributes.Append(XD.CreateAttribute("IdentifierOwnerURI"));
                        Parcelidenti.InnerText = parcelNumber.Trim();


                        // Roles

                        XmlNode ROLES1 = PARTY1.AppendChild(XD.CreateElement("ROLES"));
                        XmlNode ROLE1 = ROLES1.AppendChild(XD.CreateElement("ROLE"));

                        XmlNode PROPERTY_OWNER = ROLE1.AppendChild(XD.CreateElement("PROPERTY_OWNER"));
                        XmlNode PROPERTY_OWNER_EXTENSION = PROPERTY_OWNER.AppendChild(XD.CreateElement("EXTENSION"));

                        XmlNode ROLE_DETAIL1 = ROLE1.AppendChild(XD.CreateElement("ROLE_DETAIL"));
                        XmlNode ROLE_DETAIL1_PartyRoleType = ROLE_DETAIL1.AppendChild(XD.CreateElement("PartyRoleType"));
                        ROLE_DETAIL1_PartyRoleType.InnerText = "PropertyOwner";

                        XmlNode ROLE_DETAIL1_EXTENSION = ROLE1.AppendChild(XD.CreateElement("EXTENSION"));


                        //XmlNode parcel = ROLE_DETAIL1_EXTENSION.AppendChild(XD.CreateElement("PARCEL_IDENTIFICATION"));
                        //XmlNode parceltype = parcel.AppendChild(XD.CreateElement("ParcelIdentificationType"));
                        //parceltype.InnerText = "ParcelIdentificationNumber";

                        //XmlNode parcelid = parcel.AppendChild(XD.CreateElement("ParcelIdentifier"));
                        //parcelid.InnerText = txtparcel.Text.Trim();


                        #endregion

                        #region PARTY2
                        XmlNode PARTY2 = PARTIES.AppendChild(XD.CreateElement("PARTY"));
                        XmlAttribute DPARTY2ChildAtt = PARTY2.Attributes.Append(XD.CreateAttribute("SequenceNumber"));
                        DPARTY2ChildAtt.InnerText = "2";

                        XmlNode INDIVIDUAL2 = PARTY2.AppendChild(XD.CreateElement("INDIVIDUAL"));
                        XmlNode NAME2 = INDIVIDUAL2.AppendChild(XD.CreateElement("NAME"));

                        XmlNode EducationalAchievementsDescription2 = NAME2.AppendChild(XD.CreateElement("EducationalAchievementsDescription"));
                        XmlNode FirstName2 = NAME2.AppendChild(XD.CreateElement("FirstName"));
                        XmlNode MiddleName2 = NAME2.AppendChild(XD.CreateElement("MiddleName"));
                        XmlNode LastName2 = NAME2.AppendChild(XD.CreateElement("LastName"));
                        XmlNode FullName2 = NAME2.AppendChild(XD.CreateElement("FullName"));
                        XmlNode PrefixName2 = NAME2.AppendChild(XD.CreateElement("PrefixName"));
                        XmlNode SuffixName2 = NAME2.AppendChild(XD.CreateElement("SuffixName"));
                        XmlNode EXTENSION2 = NAME2.AppendChild(XD.CreateElement("EXTENSION"));

                        XmlNode IND2EXTENSION = INDIVIDUAL2.AppendChild(XD.CreateElement("EXTENSION"));


                        //Adress

                        XmlNode ADDRESSES2 = PARTY2.AppendChild(XD.CreateElement("ADDRESSES"));
                        XmlNode ADDRESS2 = ADDRESSES2.AppendChild(XD.CreateElement("ADDRESS"));

                        XmlAttribute ADDRESS2ChildAtt = ADDRESS2.Attributes.Append(XD.CreateAttribute("SequenceNumber"));
                        ADDRESS2ChildAtt.InnerText = "1";

                        XmlNode AddressType2 = ADDRESS2.AppendChild(XD.CreateElement("AddressType"));
                        AddressType2.InnerText = "Primary";
                        XmlNode AddressTypeOtherDescription2 = ADDRESS2.AppendChild(XD.CreateElement("AddressTypeOtherDescription"));
                        XmlNode AddressUnitDesignatorType2 = ADDRESS2.AppendChild(XD.CreateElement("AddressUnitDesignatorType"));
                        AddressUnitDesignatorType2.InnerText = "LOT";
                        XmlNode AddressUnitDesignatorTypeOtherDescription2 = ADDRESS2.AppendChild(XD.CreateElement("AddressUnitDesignatorTypeOtherDescription"));
                        XmlNode AddressUnitIdentifier2 = ADDRESS2.AppendChild(XD.CreateElement("AddressUnitIdentifier"));
                        XmlNode CountryName2 = ADDRESS2.AppendChild(XD.CreateElement("CountryName"));
                        XmlNode CountryCode2 = ADDRESS2.AppendChild(XD.CreateElement("CountryCode"));
                        XmlNode StateName2 = ADDRESS2.AppendChild(XD.CreateElement("StateName"));
                        XmlNode StateCode2 = ADDRESS2.AppendChild(XD.CreateElement("StateCode"));
                        XmlNode CountyName2 = ADDRESS2.AppendChild(XD.CreateElement("CountyName"));
                        XmlNode CountyCode2 = ADDRESS2.AppendChild(XD.CreateElement("CountyCode"));
                        XmlNode AddressLineText22 = ADDRESS2.AppendChild(XD.CreateElement("AddressLineText"));
                        XmlNode AddressAdditionalLineText2 = ADDRESS2.AppendChild(XD.CreateElement("AddressAdditionalLineText"));
                        XmlNode CityName2 = ADDRESS2.AppendChild(XD.CreateElement("CityName"));
                        XmlNode PlusFourZipCode2 = ADDRESS2.AppendChild(XD.CreateElement("PlusFourZipCode"));
                        XmlNode PostalCode2 = ADDRESS2.AppendChild(XD.CreateElement("PostalCode"));



                        // Roles

                        XmlNode ROLES2 = PARTY2.AppendChild(XD.CreateElement("ROLES"));
                        XmlNode ROLE2 = ROLES2.AppendChild(XD.CreateElement("ROLE"));

                        XmlNode SUBMITTING_PARTY = ROLE2.AppendChild(XD.CreateElement("SUBMITTING_PARTY"));
                        XmlNode SubmittingPartySequenceNumber = SUBMITTING_PARTY.AppendChild(XD.CreateElement("SubmittingPartySequenceNumber"));
                        SubmittingPartySequenceNumber.InnerText = "1";
                        XmlNode SubmittingPartyTransactionIdentifier = SUBMITTING_PARTY.AppendChild(XD.CreateElement("SubmittingPartyTransactionIdentifier"));
                        XmlNode SubmittingPartyEXTENSION = SUBMITTING_PARTY.AppendChild(XD.CreateElement("EXTENSION"));
                        XmlNode SubmittingPartyLoginAccountIdentifier = SubmittingPartyEXTENSION.AppendChild(XD.CreateElement("LoginAccountIdentifier"));
                        XmlNode SubmittingPartyLoginAccountPassword = SubmittingPartyEXTENSION.AppendChild(XD.CreateElement("LoginAccountPassword"));
                        XmlNode ROLE_DETAIL2 = ROLE2.AppendChild(XD.CreateElement("ROLE_DETAIL"));
                        XmlNode ROLE_DETAIL2_PartyRoleType = ROLE_DETAIL2.AppendChild(XD.CreateElement("PartyRoleType"));
                        ROLE_DETAIL2_PartyRoleType.InnerText = "SubmittingParty";

                        XmlNode ROLE_DETAIL2_EXTENSION = ROLE2.AppendChild(XD.CreateElement("EXTENSION"));


                        #endregion

                        #region PARTY3
                        XmlNode PARTY3 = PARTIES.AppendChild(XD.CreateElement("PARTY"));
                        XmlAttribute DPARTY3ChildAtt = PARTY3.Attributes.Append(XD.CreateAttribute("SequenceNumber"));
                        DPARTY3ChildAtt.InnerText = "3";

                        XmlNode INDIVIDUAL3 = PARTY3.AppendChild(XD.CreateElement("INDIVIDUAL"));
                        XmlNode NAME3 = INDIVIDUAL3.AppendChild(XD.CreateElement("NAME"));

                        XmlNode EducationalAchievementsDescription3 = NAME3.AppendChild(XD.CreateElement("EducationalAchievementsDescription"));
                        XmlNode FirstName3 = NAME3.AppendChild(XD.CreateElement("FirstName"));
                        XmlNode MiddleName3 = NAME3.AppendChild(XD.CreateElement("MiddleName"));
                        XmlNode LastName3 = NAME3.AppendChild(XD.CreateElement("LastName"));
                        XmlNode FullName3 = NAME3.AppendChild(XD.CreateElement("FullName"));
                        XmlNode PrefixName3 = NAME3.AppendChild(XD.CreateElement("PrefixName"));
                        XmlNode SuffixName3 = NAME3.AppendChild(XD.CreateElement("SuffixName"));
                        XmlNode EXTENSION3 = NAME3.AppendChild(XD.CreateElement("EXTENSION"));

                        XmlNode IND3EXTENSION = INDIVIDUAL3.AppendChild(XD.CreateElement("EXTENSION"));

                        //Adress

                        XmlNode ADDRESSES3 = PARTY3.AppendChild(XD.CreateElement("ADDRESSES"));
                        XmlNode ADDRESS3 = ADDRESSES3.AppendChild(XD.CreateElement("ADDRESS"));

                        XmlAttribute ADDRESS3ChildAtt = ADDRESS3.Attributes.Append(XD.CreateAttribute("SequenceNumber"));
                        ADDRESS3ChildAtt.InnerText = "1";

                        XmlNode AddressType3 = ADDRESS3.AppendChild(XD.CreateElement("AddressType"));
                        AddressType3.InnerText = "Primary";
                        XmlNode AddressTypeOtherDescription3 = ADDRESS3.AppendChild(XD.CreateElement("AddressTypeOtherDescription"));
                        XmlNode AddressUnitDesignatorType3 = ADDRESS3.AppendChild(XD.CreateElement("AddressUnitDesignatorType"));
                        AddressUnitDesignatorType3.InnerText = "LOT";
                        XmlNode AddressUnitDesignatorTypeOtherDescription3 = ADDRESS3.AppendChild(XD.CreateElement("AddressUnitDesignatorTypeOtherDescription"));
                        XmlNode AddressUnitIdentifier3 = ADDRESS3.AppendChild(XD.CreateElement("AddressUnitIdentifier"));
                        XmlNode CountryName3 = ADDRESS3.AppendChild(XD.CreateElement("CountryName"));
                        XmlNode CountryCode3 = ADDRESS3.AppendChild(XD.CreateElement("CountryCode"));
                        XmlNode StateName3 = ADDRESS3.AppendChild(XD.CreateElement("StateName"));
                        XmlNode StateCode3 = ADDRESS3.AppendChild(XD.CreateElement("StateCode"));
                        XmlNode CountyName3 = ADDRESS3.AppendChild(XD.CreateElement("CountyName"));
                        XmlNode CountyCode3 = ADDRESS3.AppendChild(XD.CreateElement("CountyCode"));
                        XmlNode AddressLineText32 = ADDRESS3.AppendChild(XD.CreateElement("AddressLineText"));
                        XmlNode AddressAdditionalLineText3 = ADDRESS3.AppendChild(XD.CreateElement("AddressAdditionalLineText"));
                        XmlNode CityName3 = ADDRESS3.AppendChild(XD.CreateElement("CityName"));
                        XmlNode PlusFourZipCode3 = ADDRESS3.AppendChild(XD.CreateElement("PlusFourZipCode"));
                        XmlNode PostalCode3 = ADDRESS3.AppendChild(XD.CreateElement("PostalCode"));

                        // Roles

                        XmlNode ROLES3 = PARTY3.AppendChild(XD.CreateElement("ROLES"));
                        XmlNode ROLE3 = ROLES3.AppendChild(XD.CreateElement("ROLE"));
                        XmlNode REQUESTING_PARTY = ROLE3.AppendChild(XD.CreateElement("REQUESTING_PARTY"));
                        XmlNode InternalAccountIdentifier = REQUESTING_PARTY.AppendChild(XD.CreateElement("InternalAccountIdentifier"));
                        XmlNode RequestedByName = REQUESTING_PARTY.AppendChild(XD.CreateElement("RequestedByName"));
                        XmlNode RequestingPartyBranchIdentifier = REQUESTING_PARTY.AppendChild(XD.CreateElement("RequestingPartyBranchIdentifier"));
                        XmlNode RequestingPartySequenceNumber = REQUESTING_PARTY.AppendChild(XD.CreateElement("RequestingPartySequenceNumber"));
                        RequestingPartySequenceNumber.InnerText = "1";
                        XmlNode RequestingPartyEXTENSION = REQUESTING_PARTY.AppendChild(XD.CreateElement("EXTENSION"));
                        XmlNode RequestingPartyLoginAccountIdentifier = RequestingPartyEXTENSION.AppendChild(XD.CreateElement("LoginAccountIdentifier"));
                        RequestingPartyLoginAccountIdentifier.InnerText = "StringInfo";
                        XmlNode RequestingPartyLoginAccountPassword = RequestingPartyEXTENSION.AppendChild(XD.CreateElement("LoginAccountPassword"));
                        RequestingPartyLoginAccountPassword.InnerText = "StringXML1@";

                        XmlNode ROLE_DETAIL3 = ROLE3.AppendChild(XD.CreateElement("ROLE_DETAIL"));
                        XmlNode ROLE_DETAIL3_PartyRoleType = ROLE_DETAIL3.AppendChild(XD.CreateElement("PartyRoleType"));
                        ROLE_DETAIL3_PartyRoleType.InnerText = "SubmittingParty";

                        XmlNode ROLE_DETAIL3_EXTENSION = ROLE3.AppendChild(XD.CreateElement("EXTENSION"));

                        #endregion

                        #region PARTY4

                        XmlNode PARTY4 = PARTIES.AppendChild(XD.CreateElement("PARTY"));
                        XmlAttribute DPARTY4ChildAtt = PARTY4.Attributes.Append(XD.CreateAttribute("SequenceNumber"));
                        DPARTY4ChildAtt.InnerText = "4";

                        XmlNode INDIVIDUAL4 = PARTY4.AppendChild(XD.CreateElement("INDIVIDUAL"));
                        XmlNode NAME4 = INDIVIDUAL4.AppendChild(XD.CreateElement("NAME"));

                        XmlNode EducationalAchievementsDescription4 = NAME4.AppendChild(XD.CreateElement("EducationalAchievementsDescription"));
                        XmlNode FirstName4 = NAME4.AppendChild(XD.CreateElement("FirstName"));
                        XmlNode MiddleName4 = NAME4.AppendChild(XD.CreateElement("MiddleName"));
                        XmlNode LastName4 = NAME4.AppendChild(XD.CreateElement("LastName"));
                        XmlNode FullName4 = NAME4.AppendChild(XD.CreateElement("FullName"));
                        XmlNode PrefixName4 = NAME4.AppendChild(XD.CreateElement("PrefixName"));
                        XmlNode SuffixName4 = NAME4.AppendChild(XD.CreateElement("SuffixName"));
                        XmlNode EXTENSION4 = NAME4.AppendChild(XD.CreateElement("EXTENSION"));

                        XmlNode IND4EXTENSION = INDIVIDUAL4.AppendChild(XD.CreateElement("EXTENSION"));

                        //Adress

                        XmlNode ADDRESSES4 = PARTY4.AppendChild(XD.CreateElement("ADDRESSES"));
                        XmlNode ADDRESS4 = ADDRESSES4.AppendChild(XD.CreateElement("ADDRESS"));

                        XmlAttribute ADDRESS4ChildAtt = ADDRESS4.Attributes.Append(XD.CreateAttribute("SequenceNumber"));
                        ADDRESS4ChildAtt.InnerText = "1";

                        XmlNode AddressType4 = ADDRESS4.AppendChild(XD.CreateElement("AddressType"));
                        AddressType4.InnerText = "Primary";
                        XmlNode AddressTypeOtherDescription4 = ADDRESS4.AppendChild(XD.CreateElement("AddressTypeOtherDescription"));
                        XmlNode AddressUnitDesignatorType4 = ADDRESS4.AppendChild(XD.CreateElement("AddressUnitDesignatorType"));
                        AddressUnitDesignatorType4.InnerText = "LOT";
                        XmlNode AddressUnitDesignatorTypeOtherDescription4 = ADDRESS4.AppendChild(XD.CreateElement("AddressUnitDesignatorTypeOtherDescription"));
                        XmlNode AddressUnitIdentifier4 = ADDRESS4.AppendChild(XD.CreateElement("AddressUnitIdentifier"));
                        XmlNode CountryName4 = ADDRESS4.AppendChild(XD.CreateElement("CountryName"));
                        XmlNode CountryCode4 = ADDRESS4.AppendChild(XD.CreateElement("CountryCode"));
                        XmlNode StateName4 = ADDRESS4.AppendChild(XD.CreateElement("StateName"));
                        XmlNode StateCode4 = ADDRESS4.AppendChild(XD.CreateElement("StateCode"));
                        XmlNode CountyName4 = ADDRESS4.AppendChild(XD.CreateElement("CountyName"));
                        XmlNode CountyCode4 = ADDRESS4.AppendChild(XD.CreateElement("CountyCode"));
                        XmlNode AddressLineText42 = ADDRESS4.AppendChild(XD.CreateElement("AddressLineText"));
                        XmlNode AddressAdditionalLineText4 = ADDRESS4.AppendChild(XD.CreateElement("AddressAdditionalLineText"));
                        XmlNode CityName4 = ADDRESS4.AppendChild(XD.CreateElement("CityName"));
                        XmlNode PlusFourZipCode4 = ADDRESS4.AppendChild(XD.CreateElement("PlusFourZipCode"));
                        XmlNode PostalCode4 = ADDRESS4.AppendChild(XD.CreateElement("PostalCode"));


                        // Roles

                        XmlNode ROLES4 = PARTY4.AppendChild(XD.CreateElement("ROLES"));
                        XmlNode ROLE4 = ROLES4.AppendChild(XD.CreateElement("ROLE"));
                        XmlNode RETURN_TO = ROLE4.AppendChild(XD.CreateElement("RETURN_TO"));
                        XmlNode PREFERRED_RESPONSES = RETURN_TO.AppendChild(XD.CreateElement("PREFERRED_RESPONSES"));

                        XmlNode PREFERRED_RESPONSES1 = PREFERRED_RESPONSES.AppendChild(XD.CreateElement("PREFERRED_RESPONSE"));
                        XmlAttribute PREFERRED_RESPONSES1ChildAtt = PREFERRED_RESPONSES1.Attributes.Append(XD.CreateAttribute("SequenceNumber"));
                        PREFERRED_RESPONSES1ChildAtt.InnerText = "";

                        XmlNode PreferredResponseFormatType = PREFERRED_RESPONSES1.AppendChild(XD.CreateElement("PreferredResponseFormatType"));
                        PreferredResponseFormatType.InnerText = "XML";
                        XmlNode PreferredResponseMethodType = PREFERRED_RESPONSES1.AppendChild(XD.CreateElement("PreferredResponseMethodType"));
                        PreferredResponseMethodType.InnerText = "HTTP";

                        XmlNode ROLE_DETAIL4 = ROLE4.AppendChild(XD.CreateElement("ROLE_DETAIL"));
                        XmlNode ROLE_DETAIL4_PartyRoleType = ROLE_DETAIL4.AppendChild(XD.CreateElement("PartyRoleType"));
                        ROLE_DETAIL4_PartyRoleType.InnerText = "RespondToParty";

                        XmlNode ROLE_DETAIL4_EXTENSION = ROLE4.AppendChild(XD.CreateElement("EXTENSION"));

                        #endregion

                        #endregion

                        #region Services
                        XmlNode SERVICES = DEAL.AppendChild(XD.CreateElement("SERVICES"));
                        XmlNode SERVICE = SERVICES.AppendChild(XD.CreateElement("SERVICE"));
                        XmlNode SERVICE_PRODUCT = SERVICE.AppendChild(XD.CreateElement("SERVICE_PRODUCT"));
                        XmlNode SERVICE_PRODUCT_REQUEST = SERVICE_PRODUCT.AppendChild(XD.CreateElement("SERVICE_PRODUCT_REQUEST"));

                        XmlNode SERVICE_PRODUCT_DETAIL = SERVICE_PRODUCT_REQUEST.AppendChild(XD.CreateElement("SERVICE_PRODUCT_DETAIL"));
                        XmlNode ServiceProductDescription = SERVICE_PRODUCT_DETAIL.AppendChild(XD.CreateElement("ServiceProductDescription"));
                        ServiceProductDescription.InnerText = "Property Information";
                        XmlNode ServiceProductIdentifier = SERVICE_PRODUCT_DETAIL.AppendChild(XD.CreateElement("ServiceProductIdentifier"));
                        ServiceProductIdentifier.InnerText = "PIB3";
                        XmlNode SERVICESEXTENSION = SERVICE_PRODUCT_DETAIL.AppendChild(XD.CreateElement("EXTENSION"));
                        XmlNode ServiceProductOperationType = SERVICESEXTENSION.AppendChild(XD.CreateElement("ServiceProductOperationType"));
                        ServiceProductOperationType.InnerText = "Create";
                        XmlNode ServiceProductNotesDescription = SERVICESEXTENSION.AppendChild(XD.CreateElement("ServiceProductNotesDescription"));
                        XmlNode ServiceProductReportReturnType = SERVICESEXTENSION.AppendChild(XD.CreateElement("ServiceProductReportReturnType"));
                        XmlNode ServiceProductImageReturnType = SERVICESEXTENSION.AppendChild(XD.CreateElement("ServiceProductImageReturnType"));
                        XmlNode ServiceProductEmailDeliveryAdrs = SERVICESEXTENSION.AppendChild(XD.CreateElement("ServiceProductEmailDeliveryAdrs"));
                        XmlNode SubjectLienRecordedDateRangeStartDate = SERVICESEXTENSION.AppendChild(XD.CreateElement("SubjectLienRecordedDateRangeStartDate"));
                        XmlNode SubjectLienRecordedDateRangeEndDate = SERVICESEXTENSION.AppendChild(XD.CreateElement("SubjectLienRecordedDateRangeEndDate"));
                        XmlNode NumberSubjectPropertiesType = SERVICESEXTENSION.AppendChild(XD.CreateElement("NumberSubjectPropertiesType"));
                        NumberSubjectPropertiesType.InnerText = "25";

                        XmlNode SERVICE_PRODUCT_REQUEST_EXTENSION = SERVICE_PRODUCT_REQUEST.AppendChild(XD.CreateElement("EXTENSION"));

                        XmlNode SERVICE_PRODUCT_NAMES = SERVICE_PRODUCT_REQUEST.AppendChild(XD.CreateElement("SERVICE_PRODUCT_NAMES"));
                        XmlNode SERVICE_PRODUCT_NAME = SERVICE_PRODUCT_REQUEST.AppendChild(XD.CreateElement("SERVICE_PRODUCT_NAME"));
                        XmlAttribute SERVICE_PRODUCT_NAMEchildatt = SERVICE_PRODUCT_NAME.Attributes.Append(XD.CreateAttribute("SequenceNumber"));
                        SERVICE_PRODUCT_NAMEchildatt.InnerText = "";

                        XmlNode SERVICE_PRODUCT_NAME_DETAIL = SERVICE_PRODUCT_NAME.AppendChild(XD.CreateElement("SERVICE_PRODUCT_NAME_DETAIL"));
                        XmlNode ServiceProductNameDescription = SERVICE_PRODUCT_NAME_DETAIL.AppendChild(XD.CreateElement("ServiceProductNameDescription"));
                        XmlNode ServiceProductNameIdentifier = SERVICE_PRODUCT_NAME_DETAIL.AppendChild(XD.CreateElement("ServiceProductNameIdentifier"));

                        XmlNode SERVICE_PRODUCT_NAME_DETAIL_EXTENSION = SERVICE_PRODUCT_NAME.AppendChild(XD.CreateElement("EXTENSION"));

                        XmlNode SERVICE_PRODUCT_NAME_EXTENSION = SERVICE_PRODUCT_NAME.AppendChild(XD.CreateElement("EXTENSION"));

                        #endregion
                        if (!Directory.Exists(strInput))
                        {
                            DirectoryInfo di = Directory.CreateDirectory(strInput);
                        }

                        string filename = strInput + orderNumber.Trim() + ".xml";
                        XD.Save(filename);

                        postXMLData("https://xmldata.datatree.com/XmlPost/PlaceOrder", filename, orderNumber);
                        readxml(orderNumber, parcelNumber, ownerName, address, state, county);


                        DataSet ds = new DataSet();
                        //string insert = "insert into tbl_output (orderno, county, state, address, OutputParcel ,AlternateAPN, lastupdateddate)  values ('" + orderNumber + "', '" + county + "' ,'" + state + "','" + address + "','" + outputparcel + "','" + alternateAPN + "','" + DateTime.Now.ToString() + "')";
                        string insert = "insert into tbl_output (orderno, county, state, address, OutputParcel ,AlternateAPN, lastupdateddate,username)  values ('" + orderNumber + "', '" + county + "' ,'" + state + "','" + address + "','" + parcelno + "','" + alternateAPN + "','" + DateTime.Now.ToString() + "','STARS')";
                        ds = tcon.ExecuteQuery(insert);

                        dsFetch = tcon.ExecuteQuery("select OutputParcel,AlternateAPN from tbl_output where orderno = '" + orderNumber + "' order by lastupdateddate desc limit 1 ");
                        dsFetch = tcon.ExecuteQuery("select OutputParcel,AlternateAPN from tbl_output where orderno = '" + orderNumber + "' order by lastupdateddate desc limit 1 ");
                        if (dsFetch.Tables[0].Rows.Count > 0)
                        {
                            if (dsFetch.Tables[0].Rows[0]["OutputParcel"].ToString() != "")
                            {
                                HttpContext.Current.Session["titleparcel"] = dsFetch.Tables[0].Rows[0]["OutputParcel"].ToString();
                                readoutput(HttpContext.Current.Session["titleparcel"].ToString());
                                return;
                            }
                            else if (dsFetch.Tables[0].Rows[0]["AlternateAPN"].ToString() != "")
                            {
                                HttpContext.Current.Session["titleparcel"] = dsFetch.Tables[0].Rows[0]["AlternateAPN"].ToString();
                                readoutput1(HttpContext.Current.Session["titleparcel"].ToString());
                                return;
                            }
                            else if (dsFetch.Tables[0].Rows[0]["AlternateAPN"].ToString() == "" && dsFetch.Tables[0].Rows[0]["OutputParcel"].ToString() == "")
                            {
                               // DataSet ds = new DataSet();
                                string bindquery = "select orderno,parcelno,ownername,address,county,state from multiparcels where orderno = '" + orderNumber + "' group by parcelno";
                                dst = tcon.ExecuteQuery(bindquery);
                                if (dst.Tables[0].Rows.Count > 0)
                                {
                                    GlobalClass.TitleFlex_Search = "Yes";
                                    HttpContext.Current.Session["TitleFlex_Search"] = "Yes";
                                    return;
                                }

                                else
                                {
                                    HttpContext.Current.Session["titleparcel"] = "";
                                    return;
                                }

                            }
                        }
                    }
                }
            }

            else
            {
                if (address.Trim() != "" && parcelNumber.Trim() == "" && ownername.Trim() == "")
                {
                    DataSet dsoutput = new DataSet();
                    dsoutput = Fetch_Output_Address_Data(state, county, address);
                    var totaldays = 0;
                    if (dsoutput.Tables[0].Rows.Count > 0)
                    {
                        DateTime lastupdateddate = Convert.ToDateTime(dsoutput.Tables[0].Rows[0]["LASTUPDATED_DATE"].ToString());
                        DateTime currentdate = Convert.ToDateTime(dsoutput.Tables[0].Rows[0]["CURRENTT_DATE"].ToString());
                        TimeSpan dayss = lastupdateddate - currentdate;
                        totaldays = dayss.Days;
                        dsFetch = tcon.ExecuteQuery("select OutputParcel,AlternateAPN from tbl_output where address = '" + address + "' order by lastupdateddate desc limit 1 ");
                        if (dsFetch.Tables[0].Rows.Count > 0)
                        {
                            if (dsFetch.Tables[0].Rows[0]["OutputParcel"].ToString() != "")
                            {
                                HttpContext.Current.Session["titleparcel"] = dsFetch.Tables[0].Rows[0]["OutputParcel"].ToString();
                                readoutput(HttpContext.Current.Session["titleparcel"].ToString());
                                return;
                            }
                            else if (dsFetch.Tables[0].Rows[0]["AlternateAPN"].ToString() != "")
                            {
                                HttpContext.Current.Session["titleparcel"] = dsFetch.Tables[0].Rows[0]["AlternateAPN"].ToString();
                                readoutput1(HttpContext.Current.Session["titleparcel"].ToString());
                                return;
                            }
                            else if (dsFetch.Tables[0].Rows[0]["AlternateAPN"].ToString() == "" && dsFetch.Tables[0].Rows[0]["OutputParcel"].ToString() == "")
                            {
                               // DataSet ds = new DataSet();
                                string bindquery = "select orderno,parcelno,ownername,address,county,state from multiparcels where address = '" + address + "' group by parcelno";
                                dst = tcon.ExecuteQuery(bindquery);
                                if (dst.Tables[0].Rows.Count > 0)
                                {
                                    GlobalClass.TitleFlex_Search = "Yes";
                                    HttpContext.Current.Session["TitleFlex_Search"] = "Yes";
                                    return;
                                }

                                else
                                {
                                    HttpContext.Current.Session["titleparcel"] = "";
                                    return;
                                }

                            }
                        }

                        else
                        {
                            dsFetch = tcon.ExecuteQuery("select OutputParcel,AlternateAPN from tbl_output where address = '" + address + "' order by lastupdateddate desc limit 1 ");
                        }
                    }

                    if (totaldays > 29 || dsoutput.Tables[0].Rows.Count == 0)
                    {
                        insert_titleflex(orderNumber, DateTime.Now.ToString(), address, county, "", state, "", ownerName, "", "", parcelNumber, "STARS");
                        XmlDocument XD = new XmlDocument();
                        XmlNode MESSAGE = XD.AppendChild(XD.CreateElement("MESSAGE"));
                        XmlNode DEAL_SETS = MESSAGE.AppendChild(XD.CreateElement("DEAL_SETS"));
                        XmlNode DEAL_SET = DEAL_SETS.AppendChild(XD.CreateElement("DEAL_SET"));
                        XmlAttribute DEAL_SETChildAtt = DEAL_SET.Attributes.Append(XD.CreateAttribute("SequenceNumber"));
                        DEAL_SETChildAtt.InnerText = "";
                        XmlNode DEALS = DEAL_SET.AppendChild(XD.CreateElement("DEALS"));
                        XmlNode DEAL = DEALS.AppendChild(XD.CreateElement("DEAL"));
                        XmlAttribute DEAL_ChildAtt = DEAL.Attributes.Append(XD.CreateAttribute("SequenceNumber"));
                        XmlAttribute DEAL_ChildAtt1 = DEAL.Attributes.Append(XD.CreateAttribute("MISMOReferenceModelIdentifier"));
                        XmlAttribute DEAL_ChildAtt2 = DEAL.Attributes.Append(XD.CreateAttribute("MISMOLogicalDataDictionaryIdentifier"));
                        DEAL_ChildAtt.InnerText = "1";
                        DEAL_ChildAtt1.InnerText = "";
                        DEAL_ChildAtt2.InnerText = "";

                        #region parties
                        XmlNode PARTIES = DEAL.AppendChild(XD.CreateElement("PARTIES"));

                        #region PARTY1
                        XmlNode PARTY1 = PARTIES.AppendChild(XD.CreateElement("PARTY"));
                        XmlAttribute DPARTY1ChildAtt = PARTY1.Attributes.Append(XD.CreateAttribute("SequenceNumber"));
                        DPARTY1ChildAtt.InnerText = "1";

                        // INDIVIDUAL

                        XmlNode INDIVIDUAL1 = PARTY1.AppendChild(XD.CreateElement("INDIVIDUAL"));
                        XmlNode NAME1 = INDIVIDUAL1.AppendChild(XD.CreateElement("NAME"));

                        if (state == "KY" && county == "Fayette")
                        {
                            string[] ownersplit = ownerName.Trim().Split(' ');
                            if (ownersplit.Count() == 2)
                            {
                                XmlNode EducationalAchievementsDescription1 = NAME1.AppendChild(XD.CreateElement("EducationalAchievementsDescription"));

                                XmlNode FirstName1 = NAME1.AppendChild(XD.CreateElement("FirstName"));
                                FirstName1.InnerText = ownersplit[1].Trim();

                                XmlNode MiddleName1 = NAME1.AppendChild(XD.CreateElement("MiddleName"));
                                ///  MiddleName1.InnerText = txtmiddlename.Text.Trim();

                                XmlNode LastName1 = NAME1.AppendChild(XD.CreateElement("LastName"));
                                LastName1.InnerText = ownersplit[0].Trim();
                            }
                            else if (ownersplit.Count() == 3)
                            {
                                XmlNode EducationalAchievementsDescription1 = NAME1.AppendChild(XD.CreateElement("EducationalAchievementsDescription"));
                                XmlNode FirstName1 = NAME1.AppendChild(XD.CreateElement("FirstName"));
                                FirstName1.InnerText = ownersplit[2].Trim();

                                XmlNode MiddleName1 = NAME1.AppendChild(XD.CreateElement("MiddleName"));
                                MiddleName1.InnerText = ownersplit[1].Trim();

                                XmlNode LastName1 = NAME1.AppendChild(XD.CreateElement("LastName"));
                                LastName1.InnerText = ownersplit[0].Trim();
                            }
                            else if (ownersplit.Count() == 1)
                            {
                                XmlNode EducationalAchievementsDescription1 = NAME1.AppendChild(XD.CreateElement("EducationalAchievementsDescription"));
                                XmlNode FirstName1 = NAME1.AppendChild(XD.CreateElement("FirstName"));
                                // FirstName1.InnerText = ownersplit[2].Trim();

                                XmlNode MiddleName1 = NAME1.AppendChild(XD.CreateElement("MiddleName"));
                                //  MiddleName1.InnerText = ownersplit[1].Trim();

                                XmlNode LastName1 = NAME1.AppendChild(XD.CreateElement("LastName"));
                                LastName1.InnerText = ownersplit[0].Trim();
                            }
                        }
                        else
                        {
                            XmlNode EducationalAchievementsDescription1 = NAME1.AppendChild(XD.CreateElement("EducationalAchievementsDescription"));

                            XmlNode FirstName1 = NAME1.AppendChild(XD.CreateElement("FirstName"));
                            // FirstName1.InnerText = txtfirstname.Text.Trim();

                            XmlNode MiddleName1 = NAME1.AppendChild(XD.CreateElement("MiddleName"));
                            ///  MiddleName1.InnerText = txtmiddlename.Text.Trim();

                            XmlNode LastName1 = NAME1.AppendChild(XD.CreateElement("LastName"));
                            LastName1.InnerText = ownerName.Trim();
                        }

                        XmlNode FullName1 = NAME1.AppendChild(XD.CreateElement("FullName"));
                        XmlNode PrefixName1 = NAME1.AppendChild(XD.CreateElement("PrefixName"));
                        XmlNode SuffixName1 = NAME1.AppendChild(XD.CreateElement("SuffixName"));
                        XmlNode EXTENSION1 = NAME1.AppendChild(XD.CreateElement("EXTENSION"));
                        XmlNode IND1EXTENSION = INDIVIDUAL1.AppendChild(XD.CreateElement("EXTENSION"));

                        //Adress

                        XmlNode ADDRESSES1 = PARTY1.AppendChild(XD.CreateElement("ADDRESSES"));
                        XmlNode ADDRESS1 = ADDRESSES1.AppendChild(XD.CreateElement("ADDRESS"));

                        XmlAttribute ADDRESS1ChildAtt = ADDRESS1.Attributes.Append(XD.CreateAttribute("SequenceNumber"));
                        ADDRESS1ChildAtt.InnerText = "";

                        XmlNode AddressType1 = ADDRESS1.AppendChild(XD.CreateElement("AddressType"));
                        AddressType1.InnerText = "Primary";
                        XmlNode AddressLineText1 = ADDRESS1.AppendChild(XD.CreateElement("AddressLineText"));
                        XmlAttribute AddressLineText1_ChildAtt1 = DEAL.Attributes.Append(XD.CreateAttribute("lang"));
                        XmlAttribute AddressLineText1_ChildAtt2 = DEAL.Attributes.Append(XD.CreateAttribute("SensitiveIndicator"));
                        AddressLineText1_ChildAtt1.InnerText = "";
                        AddressLineText1_ChildAtt2.InnerText = "";
                        AddressLineText1.InnerText = address.Trim();

                        XmlNode AddressTypeOtherDescription1 = ADDRESS1.AppendChild(XD.CreateElement("AddressTypeOtherDescription"));
                        XmlNode AddressUnitDesignatorType1 = ADDRESS1.AppendChild(XD.CreateElement("AddressUnitDesignatorType"));
                        AddressUnitDesignatorType1.InnerText = "LOT";
                        XmlNode AddressUnitDesignatorTypeOtherDescription1 = ADDRESS1.AppendChild(XD.CreateElement("AddressUnitDesignatorTypeOtherDescription"));
                        XmlNode AddressUnitIdentifier1 = ADDRESS1.AppendChild(XD.CreateElement("AddressUnitIdentifier"));
                        XmlNode CountryName1 = ADDRESS1.AppendChild(XD.CreateElement("CountryName"));
                        XmlNode CountryCode1 = ADDRESS1.AppendChild(XD.CreateElement("CountryCode"));
                        XmlNode StateName1 = ADDRESS1.AppendChild(XD.CreateElement("StateName"));
                        XmlNode StateCode1 = ADDRESS1.AppendChild(XD.CreateElement("StateCode"));
                        StateCode1.InnerText = state.Trim();
                        XmlNode CountyName1 = ADDRESS1.AppendChild(XD.CreateElement("CountyName"));
                        CountyName1.InnerText = county.Trim();
                        XmlNode CountyCode1 = ADDRESS1.AppendChild(XD.CreateElement("CountyCode"));
                        XmlNode AddressLineText12 = ADDRESS1.AppendChild(XD.CreateElement("AddressLineText"));
                        XmlNode AddressAdditionalLineText1 = ADDRESS1.AppendChild(XD.CreateElement("AddressAdditionalLineText"));
                        XmlNode CityName1 = ADDRESS1.AppendChild(XD.CreateElement("CityName"));
                        //CityName1.InnerText = txtcity.Text.Trim();
                        XmlNode PlusFourZipCode1 = ADDRESS1.AppendChild(XD.CreateElement("PlusFourZipCode"));
                        XmlNode PostalCode1 = ADDRESS1.AppendChild(XD.CreateElement("PostalCode"));
                        //  PostalCode1.InnerText = txtzip.Text.Trim();


                        //Extension APN

                        XmlNode exten = ADDRESS1.AppendChild(XD.CreateElement("EXTENSION"));

                        //  XmlNode propid = exten.AppendChild(XD.CreateElement("PropertyID"));

                        XmlNode legal = exten.AppendChild(XD.CreateElement("LEGAL_DESCRIPTIONS"));
                        XmlAttribute legal_ChildAtt1 = legal.Attributes.Append(XD.CreateAttribute("title"));
                        XmlAttribute legal_ChildAtt2 = legal.Attributes.Append(XD.CreateAttribute("role"));
                        XmlAttribute legal_ChildAtt3 = legal.Attributes.Append(XD.CreateAttribute("label"));
                        XmlAttribute legal_ChildAtt4 = legal.Attributes.Append(XD.CreateAttribute("type"));

                        XmlNode legal1 = legal.AppendChild(XD.CreateElement("LEGAL_DESCRIPTION"));
                        XmlAttribute legal1_ChildAtt1 = legal1.Attributes.Append(XD.CreateAttribute("title"));
                        XmlAttribute legal1_ChildAtt2 = legal1.Attributes.Append(XD.CreateAttribute("role"));
                        XmlAttribute legal1_ChildAtt3 = legal1.Attributes.Append(XD.CreateAttribute("label"));
                        XmlAttribute legal1_ChildAtt4 = legal1.Attributes.Append(XD.CreateAttribute("type"));

                        XmlNode parcelid1 = legal1.AppendChild(XD.CreateElement("PARCEL_IDENTIFICATIONS"));

                        XmlNode parcelid2 = parcelid1.AppendChild(XD.CreateElement("PARCEL_IDENTIFICATION"));

                        XmlNode ParcelIdentificationType = parcelid2.AppendChild(XD.CreateElement("ParcelIdentificationType"));
                        ParcelIdentificationType.InnerText = "ParcelIdentificationNumber";

                        XmlNode parceldescr = parcelid2.AppendChild(XD.CreateElement("ParcelIdentificationTypeOtherDescription"));
                        XmlAttribute legal1parceldescr_ChildAtt1 = parceldescr.Attributes.Append(XD.CreateAttribute("lang"));
                        XmlAttribute egal1lparceldescr_ChildAtt2 = parceldescr.Attributes.Append(XD.CreateAttribute("SensitiveIndicator"));

                        XmlNode Parcelidenti = parcelid2.AppendChild(XD.CreateElement("ParcelIdentifier"));
                        XmlAttribute Parcelidenti_ChildAtt1 = Parcelidenti.Attributes.Append(XD.CreateAttribute("SensitiveIndicator"));
                        XmlAttribute Parcelidenti_ChildAtt2 = Parcelidenti.Attributes.Append(XD.CreateAttribute("IdentifierEffectiveDate"));
                        XmlAttribute Parcelidenti_ChildAtt3 = Parcelidenti.Attributes.Append(XD.CreateAttribute("IdentifierOwnerURI"));
                        Parcelidenti.InnerText = parcelNumber.Trim();


                        // Roles

                        XmlNode ROLES1 = PARTY1.AppendChild(XD.CreateElement("ROLES"));
                        XmlNode ROLE1 = ROLES1.AppendChild(XD.CreateElement("ROLE"));

                        XmlNode PROPERTY_OWNER = ROLE1.AppendChild(XD.CreateElement("PROPERTY_OWNER"));
                        XmlNode PROPERTY_OWNER_EXTENSION = PROPERTY_OWNER.AppendChild(XD.CreateElement("EXTENSION"));

                        XmlNode ROLE_DETAIL1 = ROLE1.AppendChild(XD.CreateElement("ROLE_DETAIL"));
                        XmlNode ROLE_DETAIL1_PartyRoleType = ROLE_DETAIL1.AppendChild(XD.CreateElement("PartyRoleType"));
                        ROLE_DETAIL1_PartyRoleType.InnerText = "PropertyOwner";

                        XmlNode ROLE_DETAIL1_EXTENSION = ROLE1.AppendChild(XD.CreateElement("EXTENSION"));


                        //XmlNode parcel = ROLE_DETAIL1_EXTENSION.AppendChild(XD.CreateElement("PARCEL_IDENTIFICATION"));
                        //XmlNode parceltype = parcel.AppendChild(XD.CreateElement("ParcelIdentificationType"));
                        //parceltype.InnerText = "ParcelIdentificationNumber";

                        //XmlNode parcelid = parcel.AppendChild(XD.CreateElement("ParcelIdentifier"));
                        //parcelid.InnerText = txtparcel.Text.Trim();


                        #endregion

                        #region PARTY2
                        XmlNode PARTY2 = PARTIES.AppendChild(XD.CreateElement("PARTY"));
                        XmlAttribute DPARTY2ChildAtt = PARTY2.Attributes.Append(XD.CreateAttribute("SequenceNumber"));
                        DPARTY2ChildAtt.InnerText = "2";

                        XmlNode INDIVIDUAL2 = PARTY2.AppendChild(XD.CreateElement("INDIVIDUAL"));
                        XmlNode NAME2 = INDIVIDUAL2.AppendChild(XD.CreateElement("NAME"));

                        XmlNode EducationalAchievementsDescription2 = NAME2.AppendChild(XD.CreateElement("EducationalAchievementsDescription"));
                        XmlNode FirstName2 = NAME2.AppendChild(XD.CreateElement("FirstName"));
                        XmlNode MiddleName2 = NAME2.AppendChild(XD.CreateElement("MiddleName"));
                        XmlNode LastName2 = NAME2.AppendChild(XD.CreateElement("LastName"));
                        XmlNode FullName2 = NAME2.AppendChild(XD.CreateElement("FullName"));
                        XmlNode PrefixName2 = NAME2.AppendChild(XD.CreateElement("PrefixName"));
                        XmlNode SuffixName2 = NAME2.AppendChild(XD.CreateElement("SuffixName"));
                        XmlNode EXTENSION2 = NAME2.AppendChild(XD.CreateElement("EXTENSION"));

                        XmlNode IND2EXTENSION = INDIVIDUAL2.AppendChild(XD.CreateElement("EXTENSION"));


                        //Adress

                        XmlNode ADDRESSES2 = PARTY2.AppendChild(XD.CreateElement("ADDRESSES"));
                        XmlNode ADDRESS2 = ADDRESSES2.AppendChild(XD.CreateElement("ADDRESS"));

                        XmlAttribute ADDRESS2ChildAtt = ADDRESS2.Attributes.Append(XD.CreateAttribute("SequenceNumber"));
                        ADDRESS2ChildAtt.InnerText = "1";

                        XmlNode AddressType2 = ADDRESS2.AppendChild(XD.CreateElement("AddressType"));
                        AddressType2.InnerText = "Primary";
                        XmlNode AddressTypeOtherDescription2 = ADDRESS2.AppendChild(XD.CreateElement("AddressTypeOtherDescription"));
                        XmlNode AddressUnitDesignatorType2 = ADDRESS2.AppendChild(XD.CreateElement("AddressUnitDesignatorType"));
                        AddressUnitDesignatorType2.InnerText = "LOT";
                        XmlNode AddressUnitDesignatorTypeOtherDescription2 = ADDRESS2.AppendChild(XD.CreateElement("AddressUnitDesignatorTypeOtherDescription"));
                        XmlNode AddressUnitIdentifier2 = ADDRESS2.AppendChild(XD.CreateElement("AddressUnitIdentifier"));
                        XmlNode CountryName2 = ADDRESS2.AppendChild(XD.CreateElement("CountryName"));
                        XmlNode CountryCode2 = ADDRESS2.AppendChild(XD.CreateElement("CountryCode"));
                        XmlNode StateName2 = ADDRESS2.AppendChild(XD.CreateElement("StateName"));
                        XmlNode StateCode2 = ADDRESS2.AppendChild(XD.CreateElement("StateCode"));
                        XmlNode CountyName2 = ADDRESS2.AppendChild(XD.CreateElement("CountyName"));
                        XmlNode CountyCode2 = ADDRESS2.AppendChild(XD.CreateElement("CountyCode"));
                        XmlNode AddressLineText22 = ADDRESS2.AppendChild(XD.CreateElement("AddressLineText"));
                        XmlNode AddressAdditionalLineText2 = ADDRESS2.AppendChild(XD.CreateElement("AddressAdditionalLineText"));
                        XmlNode CityName2 = ADDRESS2.AppendChild(XD.CreateElement("CityName"));
                        XmlNode PlusFourZipCode2 = ADDRESS2.AppendChild(XD.CreateElement("PlusFourZipCode"));
                        XmlNode PostalCode2 = ADDRESS2.AppendChild(XD.CreateElement("PostalCode"));



                        // Roles

                        XmlNode ROLES2 = PARTY2.AppendChild(XD.CreateElement("ROLES"));
                        XmlNode ROLE2 = ROLES2.AppendChild(XD.CreateElement("ROLE"));

                        XmlNode SUBMITTING_PARTY = ROLE2.AppendChild(XD.CreateElement("SUBMITTING_PARTY"));
                        XmlNode SubmittingPartySequenceNumber = SUBMITTING_PARTY.AppendChild(XD.CreateElement("SubmittingPartySequenceNumber"));
                        SubmittingPartySequenceNumber.InnerText = "1";
                        XmlNode SubmittingPartyTransactionIdentifier = SUBMITTING_PARTY.AppendChild(XD.CreateElement("SubmittingPartyTransactionIdentifier"));
                        XmlNode SubmittingPartyEXTENSION = SUBMITTING_PARTY.AppendChild(XD.CreateElement("EXTENSION"));
                        XmlNode SubmittingPartyLoginAccountIdentifier = SubmittingPartyEXTENSION.AppendChild(XD.CreateElement("LoginAccountIdentifier"));
                        XmlNode SubmittingPartyLoginAccountPassword = SubmittingPartyEXTENSION.AppendChild(XD.CreateElement("LoginAccountPassword"));
                        XmlNode ROLE_DETAIL2 = ROLE2.AppendChild(XD.CreateElement("ROLE_DETAIL"));
                        XmlNode ROLE_DETAIL2_PartyRoleType = ROLE_DETAIL2.AppendChild(XD.CreateElement("PartyRoleType"));
                        ROLE_DETAIL2_PartyRoleType.InnerText = "SubmittingParty";

                        XmlNode ROLE_DETAIL2_EXTENSION = ROLE2.AppendChild(XD.CreateElement("EXTENSION"));


                        #endregion

                        #region PARTY3
                        XmlNode PARTY3 = PARTIES.AppendChild(XD.CreateElement("PARTY"));
                        XmlAttribute DPARTY3ChildAtt = PARTY3.Attributes.Append(XD.CreateAttribute("SequenceNumber"));
                        DPARTY3ChildAtt.InnerText = "3";

                        XmlNode INDIVIDUAL3 = PARTY3.AppendChild(XD.CreateElement("INDIVIDUAL"));
                        XmlNode NAME3 = INDIVIDUAL3.AppendChild(XD.CreateElement("NAME"));

                        XmlNode EducationalAchievementsDescription3 = NAME3.AppendChild(XD.CreateElement("EducationalAchievementsDescription"));
                        XmlNode FirstName3 = NAME3.AppendChild(XD.CreateElement("FirstName"));
                        XmlNode MiddleName3 = NAME3.AppendChild(XD.CreateElement("MiddleName"));
                        XmlNode LastName3 = NAME3.AppendChild(XD.CreateElement("LastName"));
                        XmlNode FullName3 = NAME3.AppendChild(XD.CreateElement("FullName"));
                        XmlNode PrefixName3 = NAME3.AppendChild(XD.CreateElement("PrefixName"));
                        XmlNode SuffixName3 = NAME3.AppendChild(XD.CreateElement("SuffixName"));
                        XmlNode EXTENSION3 = NAME3.AppendChild(XD.CreateElement("EXTENSION"));

                        XmlNode IND3EXTENSION = INDIVIDUAL3.AppendChild(XD.CreateElement("EXTENSION"));

                        //Adress

                        XmlNode ADDRESSES3 = PARTY3.AppendChild(XD.CreateElement("ADDRESSES"));
                        XmlNode ADDRESS3 = ADDRESSES3.AppendChild(XD.CreateElement("ADDRESS"));

                        XmlAttribute ADDRESS3ChildAtt = ADDRESS3.Attributes.Append(XD.CreateAttribute("SequenceNumber"));
                        ADDRESS3ChildAtt.InnerText = "1";

                        XmlNode AddressType3 = ADDRESS3.AppendChild(XD.CreateElement("AddressType"));
                        AddressType3.InnerText = "Primary";
                        XmlNode AddressTypeOtherDescription3 = ADDRESS3.AppendChild(XD.CreateElement("AddressTypeOtherDescription"));
                        XmlNode AddressUnitDesignatorType3 = ADDRESS3.AppendChild(XD.CreateElement("AddressUnitDesignatorType"));
                        AddressUnitDesignatorType3.InnerText = "LOT";
                        XmlNode AddressUnitDesignatorTypeOtherDescription3 = ADDRESS3.AppendChild(XD.CreateElement("AddressUnitDesignatorTypeOtherDescription"));
                        XmlNode AddressUnitIdentifier3 = ADDRESS3.AppendChild(XD.CreateElement("AddressUnitIdentifier"));
                        XmlNode CountryName3 = ADDRESS3.AppendChild(XD.CreateElement("CountryName"));
                        XmlNode CountryCode3 = ADDRESS3.AppendChild(XD.CreateElement("CountryCode"));
                        XmlNode StateName3 = ADDRESS3.AppendChild(XD.CreateElement("StateName"));
                        XmlNode StateCode3 = ADDRESS3.AppendChild(XD.CreateElement("StateCode"));
                        XmlNode CountyName3 = ADDRESS3.AppendChild(XD.CreateElement("CountyName"));
                        XmlNode CountyCode3 = ADDRESS3.AppendChild(XD.CreateElement("CountyCode"));
                        XmlNode AddressLineText32 = ADDRESS3.AppendChild(XD.CreateElement("AddressLineText"));
                        XmlNode AddressAdditionalLineText3 = ADDRESS3.AppendChild(XD.CreateElement("AddressAdditionalLineText"));
                        XmlNode CityName3 = ADDRESS3.AppendChild(XD.CreateElement("CityName"));
                        XmlNode PlusFourZipCode3 = ADDRESS3.AppendChild(XD.CreateElement("PlusFourZipCode"));
                        XmlNode PostalCode3 = ADDRESS3.AppendChild(XD.CreateElement("PostalCode"));

                        // Roles

                        XmlNode ROLES3 = PARTY3.AppendChild(XD.CreateElement("ROLES"));
                        XmlNode ROLE3 = ROLES3.AppendChild(XD.CreateElement("ROLE"));
                        XmlNode REQUESTING_PARTY = ROLE3.AppendChild(XD.CreateElement("REQUESTING_PARTY"));
                        XmlNode InternalAccountIdentifier = REQUESTING_PARTY.AppendChild(XD.CreateElement("InternalAccountIdentifier"));
                        XmlNode RequestedByName = REQUESTING_PARTY.AppendChild(XD.CreateElement("RequestedByName"));
                        XmlNode RequestingPartyBranchIdentifier = REQUESTING_PARTY.AppendChild(XD.CreateElement("RequestingPartyBranchIdentifier"));
                        XmlNode RequestingPartySequenceNumber = REQUESTING_PARTY.AppendChild(XD.CreateElement("RequestingPartySequenceNumber"));
                        RequestingPartySequenceNumber.InnerText = "1";
                        XmlNode RequestingPartyEXTENSION = REQUESTING_PARTY.AppendChild(XD.CreateElement("EXTENSION"));
                        XmlNode RequestingPartyLoginAccountIdentifier = RequestingPartyEXTENSION.AppendChild(XD.CreateElement("LoginAccountIdentifier"));
                        RequestingPartyLoginAccountIdentifier.InnerText = "StringInfo";
                        XmlNode RequestingPartyLoginAccountPassword = RequestingPartyEXTENSION.AppendChild(XD.CreateElement("LoginAccountPassword"));
                        RequestingPartyLoginAccountPassword.InnerText = "StringXML1@";

                        XmlNode ROLE_DETAIL3 = ROLE3.AppendChild(XD.CreateElement("ROLE_DETAIL"));
                        XmlNode ROLE_DETAIL3_PartyRoleType = ROLE_DETAIL3.AppendChild(XD.CreateElement("PartyRoleType"));
                        ROLE_DETAIL3_PartyRoleType.InnerText = "SubmittingParty";

                        XmlNode ROLE_DETAIL3_EXTENSION = ROLE3.AppendChild(XD.CreateElement("EXTENSION"));

                        #endregion

                        #region PARTY4

                        XmlNode PARTY4 = PARTIES.AppendChild(XD.CreateElement("PARTY"));
                        XmlAttribute DPARTY4ChildAtt = PARTY4.Attributes.Append(XD.CreateAttribute("SequenceNumber"));
                        DPARTY4ChildAtt.InnerText = "4";

                        XmlNode INDIVIDUAL4 = PARTY4.AppendChild(XD.CreateElement("INDIVIDUAL"));
                        XmlNode NAME4 = INDIVIDUAL4.AppendChild(XD.CreateElement("NAME"));

                        XmlNode EducationalAchievementsDescription4 = NAME4.AppendChild(XD.CreateElement("EducationalAchievementsDescription"));
                        XmlNode FirstName4 = NAME4.AppendChild(XD.CreateElement("FirstName"));
                        XmlNode MiddleName4 = NAME4.AppendChild(XD.CreateElement("MiddleName"));
                        XmlNode LastName4 = NAME4.AppendChild(XD.CreateElement("LastName"));
                        XmlNode FullName4 = NAME4.AppendChild(XD.CreateElement("FullName"));
                        XmlNode PrefixName4 = NAME4.AppendChild(XD.CreateElement("PrefixName"));
                        XmlNode SuffixName4 = NAME4.AppendChild(XD.CreateElement("SuffixName"));
                        XmlNode EXTENSION4 = NAME4.AppendChild(XD.CreateElement("EXTENSION"));

                        XmlNode IND4EXTENSION = INDIVIDUAL4.AppendChild(XD.CreateElement("EXTENSION"));

                        //Adress

                        XmlNode ADDRESSES4 = PARTY4.AppendChild(XD.CreateElement("ADDRESSES"));
                        XmlNode ADDRESS4 = ADDRESSES4.AppendChild(XD.CreateElement("ADDRESS"));

                        XmlAttribute ADDRESS4ChildAtt = ADDRESS4.Attributes.Append(XD.CreateAttribute("SequenceNumber"));
                        ADDRESS4ChildAtt.InnerText = "1";

                        XmlNode AddressType4 = ADDRESS4.AppendChild(XD.CreateElement("AddressType"));
                        AddressType4.InnerText = "Primary";
                        XmlNode AddressTypeOtherDescription4 = ADDRESS4.AppendChild(XD.CreateElement("AddressTypeOtherDescription"));
                        XmlNode AddressUnitDesignatorType4 = ADDRESS4.AppendChild(XD.CreateElement("AddressUnitDesignatorType"));
                        AddressUnitDesignatorType4.InnerText = "LOT";
                        XmlNode AddressUnitDesignatorTypeOtherDescription4 = ADDRESS4.AppendChild(XD.CreateElement("AddressUnitDesignatorTypeOtherDescription"));
                        XmlNode AddressUnitIdentifier4 = ADDRESS4.AppendChild(XD.CreateElement("AddressUnitIdentifier"));
                        XmlNode CountryName4 = ADDRESS4.AppendChild(XD.CreateElement("CountryName"));
                        XmlNode CountryCode4 = ADDRESS4.AppendChild(XD.CreateElement("CountryCode"));
                        XmlNode StateName4 = ADDRESS4.AppendChild(XD.CreateElement("StateName"));
                        XmlNode StateCode4 = ADDRESS4.AppendChild(XD.CreateElement("StateCode"));
                        XmlNode CountyName4 = ADDRESS4.AppendChild(XD.CreateElement("CountyName"));
                        XmlNode CountyCode4 = ADDRESS4.AppendChild(XD.CreateElement("CountyCode"));
                        XmlNode AddressLineText42 = ADDRESS4.AppendChild(XD.CreateElement("AddressLineText"));
                        XmlNode AddressAdditionalLineText4 = ADDRESS4.AppendChild(XD.CreateElement("AddressAdditionalLineText"));
                        XmlNode CityName4 = ADDRESS4.AppendChild(XD.CreateElement("CityName"));
                        XmlNode PlusFourZipCode4 = ADDRESS4.AppendChild(XD.CreateElement("PlusFourZipCode"));
                        XmlNode PostalCode4 = ADDRESS4.AppendChild(XD.CreateElement("PostalCode"));


                        // Roles

                        XmlNode ROLES4 = PARTY4.AppendChild(XD.CreateElement("ROLES"));
                        XmlNode ROLE4 = ROLES4.AppendChild(XD.CreateElement("ROLE"));
                        XmlNode RETURN_TO = ROLE4.AppendChild(XD.CreateElement("RETURN_TO"));
                        XmlNode PREFERRED_RESPONSES = RETURN_TO.AppendChild(XD.CreateElement("PREFERRED_RESPONSES"));

                        XmlNode PREFERRED_RESPONSES1 = PREFERRED_RESPONSES.AppendChild(XD.CreateElement("PREFERRED_RESPONSE"));
                        XmlAttribute PREFERRED_RESPONSES1ChildAtt = PREFERRED_RESPONSES1.Attributes.Append(XD.CreateAttribute("SequenceNumber"));
                        PREFERRED_RESPONSES1ChildAtt.InnerText = "";

                        XmlNode PreferredResponseFormatType = PREFERRED_RESPONSES1.AppendChild(XD.CreateElement("PreferredResponseFormatType"));
                        PreferredResponseFormatType.InnerText = "XML";
                        XmlNode PreferredResponseMethodType = PREFERRED_RESPONSES1.AppendChild(XD.CreateElement("PreferredResponseMethodType"));
                        PreferredResponseMethodType.InnerText = "HTTP";

                        XmlNode ROLE_DETAIL4 = ROLE4.AppendChild(XD.CreateElement("ROLE_DETAIL"));
                        XmlNode ROLE_DETAIL4_PartyRoleType = ROLE_DETAIL4.AppendChild(XD.CreateElement("PartyRoleType"));
                        ROLE_DETAIL4_PartyRoleType.InnerText = "RespondToParty";

                        XmlNode ROLE_DETAIL4_EXTENSION = ROLE4.AppendChild(XD.CreateElement("EXTENSION"));

                        #endregion

                        #endregion

                        #region Services
                        XmlNode SERVICES = DEAL.AppendChild(XD.CreateElement("SERVICES"));
                        XmlNode SERVICE = SERVICES.AppendChild(XD.CreateElement("SERVICE"));
                        XmlNode SERVICE_PRODUCT = SERVICE.AppendChild(XD.CreateElement("SERVICE_PRODUCT"));
                        XmlNode SERVICE_PRODUCT_REQUEST = SERVICE_PRODUCT.AppendChild(XD.CreateElement("SERVICE_PRODUCT_REQUEST"));

                        XmlNode SERVICE_PRODUCT_DETAIL = SERVICE_PRODUCT_REQUEST.AppendChild(XD.CreateElement("SERVICE_PRODUCT_DETAIL"));
                        XmlNode ServiceProductDescription = SERVICE_PRODUCT_DETAIL.AppendChild(XD.CreateElement("ServiceProductDescription"));
                        ServiceProductDescription.InnerText = "Property Information";
                        XmlNode ServiceProductIdentifier = SERVICE_PRODUCT_DETAIL.AppendChild(XD.CreateElement("ServiceProductIdentifier"));
                        ServiceProductIdentifier.InnerText = "PIB3";
                        XmlNode SERVICESEXTENSION = SERVICE_PRODUCT_DETAIL.AppendChild(XD.CreateElement("EXTENSION"));
                        XmlNode ServiceProductOperationType = SERVICESEXTENSION.AppendChild(XD.CreateElement("ServiceProductOperationType"));
                        ServiceProductOperationType.InnerText = "Create";
                        XmlNode ServiceProductNotesDescription = SERVICESEXTENSION.AppendChild(XD.CreateElement("ServiceProductNotesDescription"));
                        XmlNode ServiceProductReportReturnType = SERVICESEXTENSION.AppendChild(XD.CreateElement("ServiceProductReportReturnType"));
                        XmlNode ServiceProductImageReturnType = SERVICESEXTENSION.AppendChild(XD.CreateElement("ServiceProductImageReturnType"));
                        XmlNode ServiceProductEmailDeliveryAdrs = SERVICESEXTENSION.AppendChild(XD.CreateElement("ServiceProductEmailDeliveryAdrs"));
                        XmlNode SubjectLienRecordedDateRangeStartDate = SERVICESEXTENSION.AppendChild(XD.CreateElement("SubjectLienRecordedDateRangeStartDate"));
                        XmlNode SubjectLienRecordedDateRangeEndDate = SERVICESEXTENSION.AppendChild(XD.CreateElement("SubjectLienRecordedDateRangeEndDate"));
                        XmlNode NumberSubjectPropertiesType = SERVICESEXTENSION.AppendChild(XD.CreateElement("NumberSubjectPropertiesType"));
                        NumberSubjectPropertiesType.InnerText = "25";

                        XmlNode SERVICE_PRODUCT_REQUEST_EXTENSION = SERVICE_PRODUCT_REQUEST.AppendChild(XD.CreateElement("EXTENSION"));

                        XmlNode SERVICE_PRODUCT_NAMES = SERVICE_PRODUCT_REQUEST.AppendChild(XD.CreateElement("SERVICE_PRODUCT_NAMES"));
                        XmlNode SERVICE_PRODUCT_NAME = SERVICE_PRODUCT_REQUEST.AppendChild(XD.CreateElement("SERVICE_PRODUCT_NAME"));
                        XmlAttribute SERVICE_PRODUCT_NAMEchildatt = SERVICE_PRODUCT_NAME.Attributes.Append(XD.CreateAttribute("SequenceNumber"));
                        SERVICE_PRODUCT_NAMEchildatt.InnerText = "";

                        XmlNode SERVICE_PRODUCT_NAME_DETAIL = SERVICE_PRODUCT_NAME.AppendChild(XD.CreateElement("SERVICE_PRODUCT_NAME_DETAIL"));
                        XmlNode ServiceProductNameDescription = SERVICE_PRODUCT_NAME_DETAIL.AppendChild(XD.CreateElement("ServiceProductNameDescription"));
                        XmlNode ServiceProductNameIdentifier = SERVICE_PRODUCT_NAME_DETAIL.AppendChild(XD.CreateElement("ServiceProductNameIdentifier"));

                        XmlNode SERVICE_PRODUCT_NAME_DETAIL_EXTENSION = SERVICE_PRODUCT_NAME.AppendChild(XD.CreateElement("EXTENSION"));

                        XmlNode SERVICE_PRODUCT_NAME_EXTENSION = SERVICE_PRODUCT_NAME.AppendChild(XD.CreateElement("EXTENSION"));

                        #endregion
                        if (!Directory.Exists(strInput))
                        {
                            DirectoryInfo di = Directory.CreateDirectory(strInput);
                        }

                        string filename = strInput + orderNumber.Trim() + ".xml";
                        XD.Save(filename);

                        postXMLData("https://xmldata.datatree.com/XmlPost/PlaceOrder", filename, orderNumber);
                        readxml(orderNumber, parcelNumber, ownerName, address, state, county);


                        DataSet ds = new DataSet();
                        //string insert = "insert into tbl_output (orderno, county, state, address, OutputParcel ,AlternateAPN, lastupdateddate)  values ('" + orderNumber + "', '" + county + "' ,'" + state + "','" + address + "','" + outputparcel + "','" + alternateAPN + "','" + DateTime.Now.ToString() + "')";
                        string insert = "insert into tbl_output (orderno, county, state, address, OutputParcel ,AlternateAPN, lastupdateddate,username)  values ('" + orderNumber + "', '" + county + "' ,'" + state + "','" + address + "','" + parcelno + "','" + alternateAPN + "','" + DateTime.Now.ToString() + "','STARS')";
                        ds = tcon.ExecuteQuery(insert);

                        dsFetch = tcon.ExecuteQuery("select OutputParcel,AlternateAPN from tbl_output where orderno = '" + orderNumber + "' order by lastupdateddate desc limit 1 ");
                        if (dsFetch.Tables[0].Rows.Count > 0)
                        {
                            if (dsFetch.Tables[0].Rows[0]["OutputParcel"].ToString() != "")
                            {
                                HttpContext.Current.Session["titleparcel"] = dsFetch.Tables[0].Rows[0]["OutputParcel"].ToString();
                                readoutput(HttpContext.Current.Session["titleparcel"].ToString());
                                return;
                            }
                            else if (dsFetch.Tables[0].Rows[0]["AlternateAPN"].ToString() != "")
                            {
                                HttpContext.Current.Session["titleparcel"] = dsFetch.Tables[0].Rows[0]["AlternateAPN"].ToString();
                                readoutput1(HttpContext.Current.Session["titleparcel"].ToString());
                                return;
                            }
                            else if (dsFetch.Tables[0].Rows[0]["AlternateAPN"].ToString() == "" && dsFetch.Tables[0].Rows[0]["OutputParcel"].ToString() == "")
                            {
                                //DataSet ds = new DataSet();
                                string bindquery = "select orderno,parcelno,ownername,address,county,state from multiparcels where orderno = '" + orderNumber + "' group by parcelno";
                                dst = tcon.ExecuteQuery(bindquery);
                                if (dst.Tables[0].Rows.Count > 0)
                                {
                                    GlobalClass.TitleFlex_Search = "Yes";
                                    HttpContext.Current.Session["TitleFlex_Search"] = "Yes";
                                    return;
                                }

                                else
                                {
                                    HttpContext.Current.Session["titleparcel"] = "";
                                    return;
                                }

                            }
                        }
                    }
                }

                else if (ownerName.Trim() != "" && address.Trim() == "" && parcelNumber.Trim() == "")
                {
                    DataSet dsoutput = new DataSet();
                    dsoutput = Fetch_Output_Data1(state, county, ownerName);
                    var totaldays = 0;

                    if (dsoutput.Tables[0].Rows.Count > 0)
                    {
                        DateTime lastupdateddate = Convert.ToDateTime(dsoutput.Tables[0].Rows[0]["LASTUPDATED_DATE"].ToString());
                        DateTime currentdate = Convert.ToDateTime(dsoutput.Tables[0].Rows[0]["CURRENTT_DATE"].ToString());

                        TimeSpan dayss = lastupdateddate - currentdate;
                        totaldays = dayss.Days;

                        dsFetch = tcon.ExecuteQuery("select OutputParcel,AlternateAPN from tbl_output where OwnerName = '" + ownerName + "' order by lastupdateddate desc limit 1 ");
                        if (dsFetch.Tables[0].Rows.Count > 0)
                        {
                            if (dsFetch.Tables[0].Rows[0]["OutputParcel"].ToString() != "")
                            {
                                HttpContext.Current.Session["titleparcel"] = dsFetch.Tables[0].Rows[0]["OutputParcel"].ToString();
                                readoutput(HttpContext.Current.Session["titleparcel"].ToString());
                                return;
                            }
                            else if (dsFetch.Tables[0].Rows[0]["AlternateAPN"].ToString() != "")
                            {
                                HttpContext.Current.Session["titleparcel"] = dsFetch.Tables[0].Rows[0]["AlternateAPN"].ToString();
                                readoutput1(HttpContext.Current.Session["titleparcel"].ToString());
                                return;
                            }
                            else if (dsFetch.Tables[0].Rows[0]["AlternateAPN"].ToString() == "" && dsFetch.Tables[0].Rows[0]["OutputParcel"].ToString() == "")
                            {
                               // DataSet ds = new DataSet();
                                string bindquery = "select orderno,parcelno,ownername,address,county,state from multiparcels where ownername = '" + ownerName + "' group by parcelno";
                                dst = tcon.ExecuteQuery(bindquery);
                                if (dst.Tables[0].Rows.Count > 0)
                                {
                                    GlobalClass.TitleFlex_Search = "Yes";
                                    HttpContext.Current.Session["TitleFlex_Search"] = "Yes";
                                    return;
                                }

                                else
                                {
                                    HttpContext.Current.Session["titleparcel"] = "";
                                    return;
                                }

                            }
                        }
                    }

                    if (totaldays > 29 || dsoutput.Tables[0].Rows.Count == 0)
                    {
                        insert_titleflex(orderNumber, DateTime.Now.ToString(), address, county, "", state, "", ownerName, "", "", parcelNumber, "STARS");
                        XmlDocument XD = new XmlDocument();
                        XmlNode MESSAGE = XD.AppendChild(XD.CreateElement("MESSAGE"));
                        XmlNode DEAL_SETS = MESSAGE.AppendChild(XD.CreateElement("DEAL_SETS"));
                        XmlNode DEAL_SET = DEAL_SETS.AppendChild(XD.CreateElement("DEAL_SET"));
                        XmlAttribute DEAL_SETChildAtt = DEAL_SET.Attributes.Append(XD.CreateAttribute("SequenceNumber"));
                        DEAL_SETChildAtt.InnerText = "";
                        XmlNode DEALS = DEAL_SET.AppendChild(XD.CreateElement("DEALS"));
                        XmlNode DEAL = DEALS.AppendChild(XD.CreateElement("DEAL"));
                        XmlAttribute DEAL_ChildAtt = DEAL.Attributes.Append(XD.CreateAttribute("SequenceNumber"));
                        XmlAttribute DEAL_ChildAtt1 = DEAL.Attributes.Append(XD.CreateAttribute("MISMOReferenceModelIdentifier"));
                        XmlAttribute DEAL_ChildAtt2 = DEAL.Attributes.Append(XD.CreateAttribute("MISMOLogicalDataDictionaryIdentifier"));
                        DEAL_ChildAtt.InnerText = "1";
                        DEAL_ChildAtt1.InnerText = "";
                        DEAL_ChildAtt2.InnerText = "";

                        #region parties
                        XmlNode PARTIES = DEAL.AppendChild(XD.CreateElement("PARTIES"));

                        #region PARTY1
                        XmlNode PARTY1 = PARTIES.AppendChild(XD.CreateElement("PARTY"));
                        XmlAttribute DPARTY1ChildAtt = PARTY1.Attributes.Append(XD.CreateAttribute("SequenceNumber"));
                        DPARTY1ChildAtt.InnerText = "1";

                        // INDIVIDUAL

                        XmlNode INDIVIDUAL1 = PARTY1.AppendChild(XD.CreateElement("INDIVIDUAL"));
                        XmlNode NAME1 = INDIVIDUAL1.AppendChild(XD.CreateElement("NAME"));

                        if (state == "KY" && county == "Fayette")
                        {
                            string[] ownersplit = ownerName.Trim().Split(' ');
                            if (ownersplit.Count() == 2)
                            {
                                XmlNode EducationalAchievementsDescription1 = NAME1.AppendChild(XD.CreateElement("EducationalAchievementsDescription"));

                                XmlNode FirstName1 = NAME1.AppendChild(XD.CreateElement("FirstName"));
                                FirstName1.InnerText = ownersplit[1].Trim();

                                XmlNode MiddleName1 = NAME1.AppendChild(XD.CreateElement("MiddleName"));
                                ///  MiddleName1.InnerText = txtmiddlename.Text.Trim();

                                XmlNode LastName1 = NAME1.AppendChild(XD.CreateElement("LastName"));
                                LastName1.InnerText = ownersplit[0].Trim();
                            }
                            else if (ownersplit.Count() == 3)
                            {
                                XmlNode EducationalAchievementsDescription1 = NAME1.AppendChild(XD.CreateElement("EducationalAchievementsDescription"));
                                XmlNode FirstName1 = NAME1.AppendChild(XD.CreateElement("FirstName"));
                                FirstName1.InnerText = ownersplit[2].Trim();

                                XmlNode MiddleName1 = NAME1.AppendChild(XD.CreateElement("MiddleName"));
                                MiddleName1.InnerText = ownersplit[1].Trim();

                                XmlNode LastName1 = NAME1.AppendChild(XD.CreateElement("LastName"));
                                LastName1.InnerText = ownersplit[0].Trim();
                            }
                            else if (ownersplit.Count() == 1)
                            {
                                XmlNode EducationalAchievementsDescription1 = NAME1.AppendChild(XD.CreateElement("EducationalAchievementsDescription"));
                                XmlNode FirstName1 = NAME1.AppendChild(XD.CreateElement("FirstName"));
                                // FirstName1.InnerText = ownersplit[2].Trim();

                                XmlNode MiddleName1 = NAME1.AppendChild(XD.CreateElement("MiddleName"));
                                //  MiddleName1.InnerText = ownersplit[1].Trim();

                                XmlNode LastName1 = NAME1.AppendChild(XD.CreateElement("LastName"));
                                LastName1.InnerText = ownersplit[0].Trim();
                            }
                        }
                        else
                        {
                            XmlNode EducationalAchievementsDescription1 = NAME1.AppendChild(XD.CreateElement("EducationalAchievementsDescription"));

                            XmlNode FirstName1 = NAME1.AppendChild(XD.CreateElement("FirstName"));
                            // FirstName1.InnerText = txtfirstname.Text.Trim();

                            XmlNode MiddleName1 = NAME1.AppendChild(XD.CreateElement("MiddleName"));
                            ///  MiddleName1.InnerText = txtmiddlename.Text.Trim();

                            XmlNode LastName1 = NAME1.AppendChild(XD.CreateElement("LastName"));
                            LastName1.InnerText = ownerName.Trim();
                        }

                        XmlNode FullName1 = NAME1.AppendChild(XD.CreateElement("FullName"));
                        XmlNode PrefixName1 = NAME1.AppendChild(XD.CreateElement("PrefixName"));
                        XmlNode SuffixName1 = NAME1.AppendChild(XD.CreateElement("SuffixName"));
                        XmlNode EXTENSION1 = NAME1.AppendChild(XD.CreateElement("EXTENSION"));
                        XmlNode IND1EXTENSION = INDIVIDUAL1.AppendChild(XD.CreateElement("EXTENSION"));

                        //Adress

                        XmlNode ADDRESSES1 = PARTY1.AppendChild(XD.CreateElement("ADDRESSES"));
                        XmlNode ADDRESS1 = ADDRESSES1.AppendChild(XD.CreateElement("ADDRESS"));

                        XmlAttribute ADDRESS1ChildAtt = ADDRESS1.Attributes.Append(XD.CreateAttribute("SequenceNumber"));
                        ADDRESS1ChildAtt.InnerText = "";

                        XmlNode AddressType1 = ADDRESS1.AppendChild(XD.CreateElement("AddressType"));
                        AddressType1.InnerText = "Primary";
                        XmlNode AddressLineText1 = ADDRESS1.AppendChild(XD.CreateElement("AddressLineText"));
                        XmlAttribute AddressLineText1_ChildAtt1 = DEAL.Attributes.Append(XD.CreateAttribute("lang"));
                        XmlAttribute AddressLineText1_ChildAtt2 = DEAL.Attributes.Append(XD.CreateAttribute("SensitiveIndicator"));
                        AddressLineText1_ChildAtt1.InnerText = "";
                        AddressLineText1_ChildAtt2.InnerText = "";
                        AddressLineText1.InnerText = address.Trim();

                        XmlNode AddressTypeOtherDescription1 = ADDRESS1.AppendChild(XD.CreateElement("AddressTypeOtherDescription"));
                        XmlNode AddressUnitDesignatorType1 = ADDRESS1.AppendChild(XD.CreateElement("AddressUnitDesignatorType"));
                        AddressUnitDesignatorType1.InnerText = "LOT";
                        XmlNode AddressUnitDesignatorTypeOtherDescription1 = ADDRESS1.AppendChild(XD.CreateElement("AddressUnitDesignatorTypeOtherDescription"));
                        XmlNode AddressUnitIdentifier1 = ADDRESS1.AppendChild(XD.CreateElement("AddressUnitIdentifier"));
                        XmlNode CountryName1 = ADDRESS1.AppendChild(XD.CreateElement("CountryName"));
                        XmlNode CountryCode1 = ADDRESS1.AppendChild(XD.CreateElement("CountryCode"));
                        XmlNode StateName1 = ADDRESS1.AppendChild(XD.CreateElement("StateName"));
                        XmlNode StateCode1 = ADDRESS1.AppendChild(XD.CreateElement("StateCode"));
                        StateCode1.InnerText = state.Trim();
                        XmlNode CountyName1 = ADDRESS1.AppendChild(XD.CreateElement("CountyName"));
                        CountyName1.InnerText = county.Trim();
                        XmlNode CountyCode1 = ADDRESS1.AppendChild(XD.CreateElement("CountyCode"));
                        XmlNode AddressLineText12 = ADDRESS1.AppendChild(XD.CreateElement("AddressLineText"));
                        XmlNode AddressAdditionalLineText1 = ADDRESS1.AppendChild(XD.CreateElement("AddressAdditionalLineText"));
                        XmlNode CityName1 = ADDRESS1.AppendChild(XD.CreateElement("CityName"));
                        //CityName1.InnerText = txtcity.Text.Trim();
                        XmlNode PlusFourZipCode1 = ADDRESS1.AppendChild(XD.CreateElement("PlusFourZipCode"));
                        XmlNode PostalCode1 = ADDRESS1.AppendChild(XD.CreateElement("PostalCode"));
                        //  PostalCode1.InnerText = txtzip.Text.Trim();


                        //Extension APN

                        XmlNode exten = ADDRESS1.AppendChild(XD.CreateElement("EXTENSION"));

                        //  XmlNode propid = exten.AppendChild(XD.CreateElement("PropertyID"));

                        XmlNode legal = exten.AppendChild(XD.CreateElement("LEGAL_DESCRIPTIONS"));
                        XmlAttribute legal_ChildAtt1 = legal.Attributes.Append(XD.CreateAttribute("title"));
                        XmlAttribute legal_ChildAtt2 = legal.Attributes.Append(XD.CreateAttribute("role"));
                        XmlAttribute legal_ChildAtt3 = legal.Attributes.Append(XD.CreateAttribute("label"));
                        XmlAttribute legal_ChildAtt4 = legal.Attributes.Append(XD.CreateAttribute("type"));

                        XmlNode legal1 = legal.AppendChild(XD.CreateElement("LEGAL_DESCRIPTION"));
                        XmlAttribute legal1_ChildAtt1 = legal1.Attributes.Append(XD.CreateAttribute("title"));
                        XmlAttribute legal1_ChildAtt2 = legal1.Attributes.Append(XD.CreateAttribute("role"));
                        XmlAttribute legal1_ChildAtt3 = legal1.Attributes.Append(XD.CreateAttribute("label"));
                        XmlAttribute legal1_ChildAtt4 = legal1.Attributes.Append(XD.CreateAttribute("type"));

                        XmlNode parcelid1 = legal1.AppendChild(XD.CreateElement("PARCEL_IDENTIFICATIONS"));

                        XmlNode parcelid2 = parcelid1.AppendChild(XD.CreateElement("PARCEL_IDENTIFICATION"));

                        XmlNode ParcelIdentificationType = parcelid2.AppendChild(XD.CreateElement("ParcelIdentificationType"));
                        ParcelIdentificationType.InnerText = "ParcelIdentificationNumber";

                        XmlNode parceldescr = parcelid2.AppendChild(XD.CreateElement("ParcelIdentificationTypeOtherDescription"));
                        XmlAttribute legal1parceldescr_ChildAtt1 = parceldescr.Attributes.Append(XD.CreateAttribute("lang"));
                        XmlAttribute egal1lparceldescr_ChildAtt2 = parceldescr.Attributes.Append(XD.CreateAttribute("SensitiveIndicator"));

                        XmlNode Parcelidenti = parcelid2.AppendChild(XD.CreateElement("ParcelIdentifier"));
                        XmlAttribute Parcelidenti_ChildAtt1 = Parcelidenti.Attributes.Append(XD.CreateAttribute("SensitiveIndicator"));
                        XmlAttribute Parcelidenti_ChildAtt2 = Parcelidenti.Attributes.Append(XD.CreateAttribute("IdentifierEffectiveDate"));
                        XmlAttribute Parcelidenti_ChildAtt3 = Parcelidenti.Attributes.Append(XD.CreateAttribute("IdentifierOwnerURI"));
                        Parcelidenti.InnerText = parcelNumber.Trim();


                        // Roles

                        XmlNode ROLES1 = PARTY1.AppendChild(XD.CreateElement("ROLES"));
                        XmlNode ROLE1 = ROLES1.AppendChild(XD.CreateElement("ROLE"));

                        XmlNode PROPERTY_OWNER = ROLE1.AppendChild(XD.CreateElement("PROPERTY_OWNER"));
                        XmlNode PROPERTY_OWNER_EXTENSION = PROPERTY_OWNER.AppendChild(XD.CreateElement("EXTENSION"));

                        XmlNode ROLE_DETAIL1 = ROLE1.AppendChild(XD.CreateElement("ROLE_DETAIL"));
                        XmlNode ROLE_DETAIL1_PartyRoleType = ROLE_DETAIL1.AppendChild(XD.CreateElement("PartyRoleType"));
                        ROLE_DETAIL1_PartyRoleType.InnerText = "PropertyOwner";

                        XmlNode ROLE_DETAIL1_EXTENSION = ROLE1.AppendChild(XD.CreateElement("EXTENSION"));


                        //XmlNode parcel = ROLE_DETAIL1_EXTENSION.AppendChild(XD.CreateElement("PARCEL_IDENTIFICATION"));
                        //XmlNode parceltype = parcel.AppendChild(XD.CreateElement("ParcelIdentificationType"));
                        //parceltype.InnerText = "ParcelIdentificationNumber";

                        //XmlNode parcelid = parcel.AppendChild(XD.CreateElement("ParcelIdentifier"));
                        //parcelid.InnerText = txtparcel.Text.Trim();


                        #endregion

                        #region PARTY2
                        XmlNode PARTY2 = PARTIES.AppendChild(XD.CreateElement("PARTY"));
                        XmlAttribute DPARTY2ChildAtt = PARTY2.Attributes.Append(XD.CreateAttribute("SequenceNumber"));
                        DPARTY2ChildAtt.InnerText = "2";

                        XmlNode INDIVIDUAL2 = PARTY2.AppendChild(XD.CreateElement("INDIVIDUAL"));
                        XmlNode NAME2 = INDIVIDUAL2.AppendChild(XD.CreateElement("NAME"));

                        XmlNode EducationalAchievementsDescription2 = NAME2.AppendChild(XD.CreateElement("EducationalAchievementsDescription"));
                        XmlNode FirstName2 = NAME2.AppendChild(XD.CreateElement("FirstName"));
                        XmlNode MiddleName2 = NAME2.AppendChild(XD.CreateElement("MiddleName"));
                        XmlNode LastName2 = NAME2.AppendChild(XD.CreateElement("LastName"));
                        XmlNode FullName2 = NAME2.AppendChild(XD.CreateElement("FullName"));
                        XmlNode PrefixName2 = NAME2.AppendChild(XD.CreateElement("PrefixName"));
                        XmlNode SuffixName2 = NAME2.AppendChild(XD.CreateElement("SuffixName"));
                        XmlNode EXTENSION2 = NAME2.AppendChild(XD.CreateElement("EXTENSION"));

                        XmlNode IND2EXTENSION = INDIVIDUAL2.AppendChild(XD.CreateElement("EXTENSION"));


                        //Adress

                        XmlNode ADDRESSES2 = PARTY2.AppendChild(XD.CreateElement("ADDRESSES"));
                        XmlNode ADDRESS2 = ADDRESSES2.AppendChild(XD.CreateElement("ADDRESS"));

                        XmlAttribute ADDRESS2ChildAtt = ADDRESS2.Attributes.Append(XD.CreateAttribute("SequenceNumber"));
                        ADDRESS2ChildAtt.InnerText = "1";

                        XmlNode AddressType2 = ADDRESS2.AppendChild(XD.CreateElement("AddressType"));
                        AddressType2.InnerText = "Primary";
                        XmlNode AddressTypeOtherDescription2 = ADDRESS2.AppendChild(XD.CreateElement("AddressTypeOtherDescription"));
                        XmlNode AddressUnitDesignatorType2 = ADDRESS2.AppendChild(XD.CreateElement("AddressUnitDesignatorType"));
                        AddressUnitDesignatorType2.InnerText = "LOT";
                        XmlNode AddressUnitDesignatorTypeOtherDescription2 = ADDRESS2.AppendChild(XD.CreateElement("AddressUnitDesignatorTypeOtherDescription"));
                        XmlNode AddressUnitIdentifier2 = ADDRESS2.AppendChild(XD.CreateElement("AddressUnitIdentifier"));
                        XmlNode CountryName2 = ADDRESS2.AppendChild(XD.CreateElement("CountryName"));
                        XmlNode CountryCode2 = ADDRESS2.AppendChild(XD.CreateElement("CountryCode"));
                        XmlNode StateName2 = ADDRESS2.AppendChild(XD.CreateElement("StateName"));
                        XmlNode StateCode2 = ADDRESS2.AppendChild(XD.CreateElement("StateCode"));
                        XmlNode CountyName2 = ADDRESS2.AppendChild(XD.CreateElement("CountyName"));
                        XmlNode CountyCode2 = ADDRESS2.AppendChild(XD.CreateElement("CountyCode"));
                        XmlNode AddressLineText22 = ADDRESS2.AppendChild(XD.CreateElement("AddressLineText"));
                        XmlNode AddressAdditionalLineText2 = ADDRESS2.AppendChild(XD.CreateElement("AddressAdditionalLineText"));
                        XmlNode CityName2 = ADDRESS2.AppendChild(XD.CreateElement("CityName"));
                        XmlNode PlusFourZipCode2 = ADDRESS2.AppendChild(XD.CreateElement("PlusFourZipCode"));
                        XmlNode PostalCode2 = ADDRESS2.AppendChild(XD.CreateElement("PostalCode"));



                        // Roles

                        XmlNode ROLES2 = PARTY2.AppendChild(XD.CreateElement("ROLES"));
                        XmlNode ROLE2 = ROLES2.AppendChild(XD.CreateElement("ROLE"));

                        XmlNode SUBMITTING_PARTY = ROLE2.AppendChild(XD.CreateElement("SUBMITTING_PARTY"));
                        XmlNode SubmittingPartySequenceNumber = SUBMITTING_PARTY.AppendChild(XD.CreateElement("SubmittingPartySequenceNumber"));
                        SubmittingPartySequenceNumber.InnerText = "1";
                        XmlNode SubmittingPartyTransactionIdentifier = SUBMITTING_PARTY.AppendChild(XD.CreateElement("SubmittingPartyTransactionIdentifier"));
                        XmlNode SubmittingPartyEXTENSION = SUBMITTING_PARTY.AppendChild(XD.CreateElement("EXTENSION"));
                        XmlNode SubmittingPartyLoginAccountIdentifier = SubmittingPartyEXTENSION.AppendChild(XD.CreateElement("LoginAccountIdentifier"));
                        XmlNode SubmittingPartyLoginAccountPassword = SubmittingPartyEXTENSION.AppendChild(XD.CreateElement("LoginAccountPassword"));
                        XmlNode ROLE_DETAIL2 = ROLE2.AppendChild(XD.CreateElement("ROLE_DETAIL"));
                        XmlNode ROLE_DETAIL2_PartyRoleType = ROLE_DETAIL2.AppendChild(XD.CreateElement("PartyRoleType"));
                        ROLE_DETAIL2_PartyRoleType.InnerText = "SubmittingParty";

                        XmlNode ROLE_DETAIL2_EXTENSION = ROLE2.AppendChild(XD.CreateElement("EXTENSION"));


                        #endregion

                        #region PARTY3
                        XmlNode PARTY3 = PARTIES.AppendChild(XD.CreateElement("PARTY"));
                        XmlAttribute DPARTY3ChildAtt = PARTY3.Attributes.Append(XD.CreateAttribute("SequenceNumber"));
                        DPARTY3ChildAtt.InnerText = "3";

                        XmlNode INDIVIDUAL3 = PARTY3.AppendChild(XD.CreateElement("INDIVIDUAL"));
                        XmlNode NAME3 = INDIVIDUAL3.AppendChild(XD.CreateElement("NAME"));

                        XmlNode EducationalAchievementsDescription3 = NAME3.AppendChild(XD.CreateElement("EducationalAchievementsDescription"));
                        XmlNode FirstName3 = NAME3.AppendChild(XD.CreateElement("FirstName"));
                        XmlNode MiddleName3 = NAME3.AppendChild(XD.CreateElement("MiddleName"));
                        XmlNode LastName3 = NAME3.AppendChild(XD.CreateElement("LastName"));
                        XmlNode FullName3 = NAME3.AppendChild(XD.CreateElement("FullName"));
                        XmlNode PrefixName3 = NAME3.AppendChild(XD.CreateElement("PrefixName"));
                        XmlNode SuffixName3 = NAME3.AppendChild(XD.CreateElement("SuffixName"));
                        XmlNode EXTENSION3 = NAME3.AppendChild(XD.CreateElement("EXTENSION"));

                        XmlNode IND3EXTENSION = INDIVIDUAL3.AppendChild(XD.CreateElement("EXTENSION"));

                        //Adress

                        XmlNode ADDRESSES3 = PARTY3.AppendChild(XD.CreateElement("ADDRESSES"));
                        XmlNode ADDRESS3 = ADDRESSES3.AppendChild(XD.CreateElement("ADDRESS"));

                        XmlAttribute ADDRESS3ChildAtt = ADDRESS3.Attributes.Append(XD.CreateAttribute("SequenceNumber"));
                        ADDRESS3ChildAtt.InnerText = "1";

                        XmlNode AddressType3 = ADDRESS3.AppendChild(XD.CreateElement("AddressType"));
                        AddressType3.InnerText = "Primary";
                        XmlNode AddressTypeOtherDescription3 = ADDRESS3.AppendChild(XD.CreateElement("AddressTypeOtherDescription"));
                        XmlNode AddressUnitDesignatorType3 = ADDRESS3.AppendChild(XD.CreateElement("AddressUnitDesignatorType"));
                        AddressUnitDesignatorType3.InnerText = "LOT";
                        XmlNode AddressUnitDesignatorTypeOtherDescription3 = ADDRESS3.AppendChild(XD.CreateElement("AddressUnitDesignatorTypeOtherDescription"));
                        XmlNode AddressUnitIdentifier3 = ADDRESS3.AppendChild(XD.CreateElement("AddressUnitIdentifier"));
                        XmlNode CountryName3 = ADDRESS3.AppendChild(XD.CreateElement("CountryName"));
                        XmlNode CountryCode3 = ADDRESS3.AppendChild(XD.CreateElement("CountryCode"));
                        XmlNode StateName3 = ADDRESS3.AppendChild(XD.CreateElement("StateName"));
                        XmlNode StateCode3 = ADDRESS3.AppendChild(XD.CreateElement("StateCode"));
                        XmlNode CountyName3 = ADDRESS3.AppendChild(XD.CreateElement("CountyName"));
                        XmlNode CountyCode3 = ADDRESS3.AppendChild(XD.CreateElement("CountyCode"));
                        XmlNode AddressLineText32 = ADDRESS3.AppendChild(XD.CreateElement("AddressLineText"));
                        XmlNode AddressAdditionalLineText3 = ADDRESS3.AppendChild(XD.CreateElement("AddressAdditionalLineText"));
                        XmlNode CityName3 = ADDRESS3.AppendChild(XD.CreateElement("CityName"));
                        XmlNode PlusFourZipCode3 = ADDRESS3.AppendChild(XD.CreateElement("PlusFourZipCode"));
                        XmlNode PostalCode3 = ADDRESS3.AppendChild(XD.CreateElement("PostalCode"));

                        // Roles

                        XmlNode ROLES3 = PARTY3.AppendChild(XD.CreateElement("ROLES"));
                        XmlNode ROLE3 = ROLES3.AppendChild(XD.CreateElement("ROLE"));
                        XmlNode REQUESTING_PARTY = ROLE3.AppendChild(XD.CreateElement("REQUESTING_PARTY"));
                        XmlNode InternalAccountIdentifier = REQUESTING_PARTY.AppendChild(XD.CreateElement("InternalAccountIdentifier"));
                        XmlNode RequestedByName = REQUESTING_PARTY.AppendChild(XD.CreateElement("RequestedByName"));
                        XmlNode RequestingPartyBranchIdentifier = REQUESTING_PARTY.AppendChild(XD.CreateElement("RequestingPartyBranchIdentifier"));
                        XmlNode RequestingPartySequenceNumber = REQUESTING_PARTY.AppendChild(XD.CreateElement("RequestingPartySequenceNumber"));
                        RequestingPartySequenceNumber.InnerText = "1";
                        XmlNode RequestingPartyEXTENSION = REQUESTING_PARTY.AppendChild(XD.CreateElement("EXTENSION"));
                        XmlNode RequestingPartyLoginAccountIdentifier = RequestingPartyEXTENSION.AppendChild(XD.CreateElement("LoginAccountIdentifier"));
                        RequestingPartyLoginAccountIdentifier.InnerText = "StringInfo";
                        XmlNode RequestingPartyLoginAccountPassword = RequestingPartyEXTENSION.AppendChild(XD.CreateElement("LoginAccountPassword"));
                        RequestingPartyLoginAccountPassword.InnerText = "StringXML1@";

                        XmlNode ROLE_DETAIL3 = ROLE3.AppendChild(XD.CreateElement("ROLE_DETAIL"));
                        XmlNode ROLE_DETAIL3_PartyRoleType = ROLE_DETAIL3.AppendChild(XD.CreateElement("PartyRoleType"));
                        ROLE_DETAIL3_PartyRoleType.InnerText = "SubmittingParty";

                        XmlNode ROLE_DETAIL3_EXTENSION = ROLE3.AppendChild(XD.CreateElement("EXTENSION"));

                        #endregion

                        #region PARTY4

                        XmlNode PARTY4 = PARTIES.AppendChild(XD.CreateElement("PARTY"));
                        XmlAttribute DPARTY4ChildAtt = PARTY4.Attributes.Append(XD.CreateAttribute("SequenceNumber"));
                        DPARTY4ChildAtt.InnerText = "4";

                        XmlNode INDIVIDUAL4 = PARTY4.AppendChild(XD.CreateElement("INDIVIDUAL"));
                        XmlNode NAME4 = INDIVIDUAL4.AppendChild(XD.CreateElement("NAME"));

                        XmlNode EducationalAchievementsDescription4 = NAME4.AppendChild(XD.CreateElement("EducationalAchievementsDescription"));
                        XmlNode FirstName4 = NAME4.AppendChild(XD.CreateElement("FirstName"));
                        XmlNode MiddleName4 = NAME4.AppendChild(XD.CreateElement("MiddleName"));
                        XmlNode LastName4 = NAME4.AppendChild(XD.CreateElement("LastName"));
                        XmlNode FullName4 = NAME4.AppendChild(XD.CreateElement("FullName"));
                        XmlNode PrefixName4 = NAME4.AppendChild(XD.CreateElement("PrefixName"));
                        XmlNode SuffixName4 = NAME4.AppendChild(XD.CreateElement("SuffixName"));
                        XmlNode EXTENSION4 = NAME4.AppendChild(XD.CreateElement("EXTENSION"));

                        XmlNode IND4EXTENSION = INDIVIDUAL4.AppendChild(XD.CreateElement("EXTENSION"));

                        //Adress

                        XmlNode ADDRESSES4 = PARTY4.AppendChild(XD.CreateElement("ADDRESSES"));
                        XmlNode ADDRESS4 = ADDRESSES4.AppendChild(XD.CreateElement("ADDRESS"));

                        XmlAttribute ADDRESS4ChildAtt = ADDRESS4.Attributes.Append(XD.CreateAttribute("SequenceNumber"));
                        ADDRESS4ChildAtt.InnerText = "1";

                        XmlNode AddressType4 = ADDRESS4.AppendChild(XD.CreateElement("AddressType"));
                        AddressType4.InnerText = "Primary";
                        XmlNode AddressTypeOtherDescription4 = ADDRESS4.AppendChild(XD.CreateElement("AddressTypeOtherDescription"));
                        XmlNode AddressUnitDesignatorType4 = ADDRESS4.AppendChild(XD.CreateElement("AddressUnitDesignatorType"));
                        AddressUnitDesignatorType4.InnerText = "LOT";
                        XmlNode AddressUnitDesignatorTypeOtherDescription4 = ADDRESS4.AppendChild(XD.CreateElement("AddressUnitDesignatorTypeOtherDescription"));
                        XmlNode AddressUnitIdentifier4 = ADDRESS4.AppendChild(XD.CreateElement("AddressUnitIdentifier"));
                        XmlNode CountryName4 = ADDRESS4.AppendChild(XD.CreateElement("CountryName"));
                        XmlNode CountryCode4 = ADDRESS4.AppendChild(XD.CreateElement("CountryCode"));
                        XmlNode StateName4 = ADDRESS4.AppendChild(XD.CreateElement("StateName"));
                        XmlNode StateCode4 = ADDRESS4.AppendChild(XD.CreateElement("StateCode"));
                        XmlNode CountyName4 = ADDRESS4.AppendChild(XD.CreateElement("CountyName"));
                        XmlNode CountyCode4 = ADDRESS4.AppendChild(XD.CreateElement("CountyCode"));
                        XmlNode AddressLineText42 = ADDRESS4.AppendChild(XD.CreateElement("AddressLineText"));
                        XmlNode AddressAdditionalLineText4 = ADDRESS4.AppendChild(XD.CreateElement("AddressAdditionalLineText"));
                        XmlNode CityName4 = ADDRESS4.AppendChild(XD.CreateElement("CityName"));
                        XmlNode PlusFourZipCode4 = ADDRESS4.AppendChild(XD.CreateElement("PlusFourZipCode"));
                        XmlNode PostalCode4 = ADDRESS4.AppendChild(XD.CreateElement("PostalCode"));


                        // Roles

                        XmlNode ROLES4 = PARTY4.AppendChild(XD.CreateElement("ROLES"));
                        XmlNode ROLE4 = ROLES4.AppendChild(XD.CreateElement("ROLE"));
                        XmlNode RETURN_TO = ROLE4.AppendChild(XD.CreateElement("RETURN_TO"));
                        XmlNode PREFERRED_RESPONSES = RETURN_TO.AppendChild(XD.CreateElement("PREFERRED_RESPONSES"));

                        XmlNode PREFERRED_RESPONSES1 = PREFERRED_RESPONSES.AppendChild(XD.CreateElement("PREFERRED_RESPONSE"));
                        XmlAttribute PREFERRED_RESPONSES1ChildAtt = PREFERRED_RESPONSES1.Attributes.Append(XD.CreateAttribute("SequenceNumber"));
                        PREFERRED_RESPONSES1ChildAtt.InnerText = "";

                        XmlNode PreferredResponseFormatType = PREFERRED_RESPONSES1.AppendChild(XD.CreateElement("PreferredResponseFormatType"));
                        PreferredResponseFormatType.InnerText = "XML";
                        XmlNode PreferredResponseMethodType = PREFERRED_RESPONSES1.AppendChild(XD.CreateElement("PreferredResponseMethodType"));
                        PreferredResponseMethodType.InnerText = "HTTP";

                        XmlNode ROLE_DETAIL4 = ROLE4.AppendChild(XD.CreateElement("ROLE_DETAIL"));
                        XmlNode ROLE_DETAIL4_PartyRoleType = ROLE_DETAIL4.AppendChild(XD.CreateElement("PartyRoleType"));
                        ROLE_DETAIL4_PartyRoleType.InnerText = "RespondToParty";

                        XmlNode ROLE_DETAIL4_EXTENSION = ROLE4.AppendChild(XD.CreateElement("EXTENSION"));

                        #endregion

                        #endregion

                        #region Services
                        XmlNode SERVICES = DEAL.AppendChild(XD.CreateElement("SERVICES"));
                        XmlNode SERVICE = SERVICES.AppendChild(XD.CreateElement("SERVICE"));
                        XmlNode SERVICE_PRODUCT = SERVICE.AppendChild(XD.CreateElement("SERVICE_PRODUCT"));
                        XmlNode SERVICE_PRODUCT_REQUEST = SERVICE_PRODUCT.AppendChild(XD.CreateElement("SERVICE_PRODUCT_REQUEST"));

                        XmlNode SERVICE_PRODUCT_DETAIL = SERVICE_PRODUCT_REQUEST.AppendChild(XD.CreateElement("SERVICE_PRODUCT_DETAIL"));
                        XmlNode ServiceProductDescription = SERVICE_PRODUCT_DETAIL.AppendChild(XD.CreateElement("ServiceProductDescription"));
                        ServiceProductDescription.InnerText = "Property Information";
                        XmlNode ServiceProductIdentifier = SERVICE_PRODUCT_DETAIL.AppendChild(XD.CreateElement("ServiceProductIdentifier"));
                        ServiceProductIdentifier.InnerText = "PIB3";
                        XmlNode SERVICESEXTENSION = SERVICE_PRODUCT_DETAIL.AppendChild(XD.CreateElement("EXTENSION"));
                        XmlNode ServiceProductOperationType = SERVICESEXTENSION.AppendChild(XD.CreateElement("ServiceProductOperationType"));
                        ServiceProductOperationType.InnerText = "Create";
                        XmlNode ServiceProductNotesDescription = SERVICESEXTENSION.AppendChild(XD.CreateElement("ServiceProductNotesDescription"));
                        XmlNode ServiceProductReportReturnType = SERVICESEXTENSION.AppendChild(XD.CreateElement("ServiceProductReportReturnType"));
                        XmlNode ServiceProductImageReturnType = SERVICESEXTENSION.AppendChild(XD.CreateElement("ServiceProductImageReturnType"));
                        XmlNode ServiceProductEmailDeliveryAdrs = SERVICESEXTENSION.AppendChild(XD.CreateElement("ServiceProductEmailDeliveryAdrs"));
                        XmlNode SubjectLienRecordedDateRangeStartDate = SERVICESEXTENSION.AppendChild(XD.CreateElement("SubjectLienRecordedDateRangeStartDate"));
                        XmlNode SubjectLienRecordedDateRangeEndDate = SERVICESEXTENSION.AppendChild(XD.CreateElement("SubjectLienRecordedDateRangeEndDate"));
                        XmlNode NumberSubjectPropertiesType = SERVICESEXTENSION.AppendChild(XD.CreateElement("NumberSubjectPropertiesType"));
                        NumberSubjectPropertiesType.InnerText = "25";

                        XmlNode SERVICE_PRODUCT_REQUEST_EXTENSION = SERVICE_PRODUCT_REQUEST.AppendChild(XD.CreateElement("EXTENSION"));

                        XmlNode SERVICE_PRODUCT_NAMES = SERVICE_PRODUCT_REQUEST.AppendChild(XD.CreateElement("SERVICE_PRODUCT_NAMES"));
                        XmlNode SERVICE_PRODUCT_NAME = SERVICE_PRODUCT_REQUEST.AppendChild(XD.CreateElement("SERVICE_PRODUCT_NAME"));
                        XmlAttribute SERVICE_PRODUCT_NAMEchildatt = SERVICE_PRODUCT_NAME.Attributes.Append(XD.CreateAttribute("SequenceNumber"));
                        SERVICE_PRODUCT_NAMEchildatt.InnerText = "";

                        XmlNode SERVICE_PRODUCT_NAME_DETAIL = SERVICE_PRODUCT_NAME.AppendChild(XD.CreateElement("SERVICE_PRODUCT_NAME_DETAIL"));
                        XmlNode ServiceProductNameDescription = SERVICE_PRODUCT_NAME_DETAIL.AppendChild(XD.CreateElement("ServiceProductNameDescription"));
                        XmlNode ServiceProductNameIdentifier = SERVICE_PRODUCT_NAME_DETAIL.AppendChild(XD.CreateElement("ServiceProductNameIdentifier"));

                        XmlNode SERVICE_PRODUCT_NAME_DETAIL_EXTENSION = SERVICE_PRODUCT_NAME.AppendChild(XD.CreateElement("EXTENSION"));

                        XmlNode SERVICE_PRODUCT_NAME_EXTENSION = SERVICE_PRODUCT_NAME.AppendChild(XD.CreateElement("EXTENSION"));

                        #endregion
                        if (!Directory.Exists(strInput))
                        {
                            DirectoryInfo di = Directory.CreateDirectory(strInput);
                        }

                        string filename = strInput + orderNumber.Trim() + ".xml";
                        XD.Save(filename);

                        postXMLData("https://xmldata.datatree.com/XmlPost/PlaceOrder", filename, orderNumber);
                        readxml(orderNumber, parcelNumber, ownerName, address, state, county);


                        DataSet ds = new DataSet();
                        //  string insert = "insert into tbl_output (orderno, county, state, OwnerName, OutputParcel ,AlternateAPN, lastupdateddate,username)  values ('" + orderNumber + "', '" + county + "' ,'" + state + "','" + ownerName + "','" + outputparcel + "','" + alternateAPN + "','" + DateTime.Now.ToString() + "','STARS')";
                        string insert = "insert into tbl_output (orderno, county, state, OwnerName, OutputParcel ,AlternateAPN, lastupdateddate,username)  values ('" + orderNumber + "', '" + county + "' ,'" + state + "','" + ownerName + "','" + parcelno + "','" + alternateAPN + "','" + DateTime.Now.ToString() + "','STARS')";
                        ds = tcon.ExecuteQuery(insert);

                        dsFetch = tcon.ExecuteQuery("select OutputParcel,AlternateAPN from tbl_output where orderno = '" + orderNumber + "' order by lastupdateddate desc limit 1 ");
                        if (dsFetch.Tables[0].Rows.Count > 0)
                        {
                            if (dsFetch.Tables[0].Rows[0]["OutputParcel"].ToString() != "")
                            {
                                HttpContext.Current.Session["titleparcel"] = dsFetch.Tables[0].Rows[0]["OutputParcel"].ToString();
                                readoutput(HttpContext.Current.Session["titleparcel"].ToString());
                                return;
                            }
                            else if (dsFetch.Tables[0].Rows[0]["AlternateAPN"].ToString() != "")
                            {
                                HttpContext.Current.Session["titleparcel"] = dsFetch.Tables[0].Rows[0]["AlternateAPN"].ToString();
                                readoutput1(HttpContext.Current.Session["titleparcel"].ToString());
                                return;
                            }
                            else if (dsFetch.Tables[0].Rows[0]["AlternateAPN"].ToString() == "" && dsFetch.Tables[0].Rows[0]["OutputParcel"].ToString() == "")
                            {
                               // DataSet ds = new DataSet();
                                string bindquery = "select orderno,parcelno,ownername,address,county,state from multiparcels where orderno = '" + orderNumber + "' group by parcelno";
                                dst = tcon.ExecuteQuery(bindquery);
                                if (dst.Tables[0].Rows.Count > 0)
                                {
                                    GlobalClass.TitleFlex_Search = "Yes";
                                    HttpContext.Current.Session["TitleFlex_Search"] = "Yes";
                                    return;
                                }

                                else
                                {
                                    HttpContext.Current.Session["titleparcel"] = "";
                                    return;
                                }

                            }
                        }
                    }
                }

                else if (parcelNumber.Trim() != "" && ownername.Trim() == "" && address.Trim() == "")
                {
                    DataSet dsoutput = new DataSet();
                    dsoutput = Fetch_Output_Data2(state, county, parcelNumber);
                    var totaldays = 0;

                    if (dsoutput.Tables[0].Rows.Count > 0)
                    {
                        DateTime lastupdateddate = Convert.ToDateTime(dsoutput.Tables[0].Rows[0]["LASTUPDATED_DATE"].ToString());
                        DateTime currentdate = Convert.ToDateTime(dsoutput.Tables[0].Rows[0]["CURRENTT_DATE"].ToString());

                        TimeSpan dayss = lastupdateddate - currentdate;
                        totaldays = dayss.Days;

                        dsFetch = tcon.ExecuteQuery("select OutputParcel,AlternateAPN from tbl_output where apn = '" + parcelNumber + "' order by lastupdateddate desc limit 1 ");
                        if (dsFetch.Tables[0].Rows.Count > 0)
                        {
                            if (dsFetch.Tables[0].Rows[0]["OutputParcel"].ToString() != "")
                            {
                                HttpContext.Current.Session["titleparcel"] = dsFetch.Tables[0].Rows[0]["OutputParcel"].ToString();
                                readoutput(HttpContext.Current.Session["titleparcel"].ToString());
                                return;
                            }
                            else if (dsFetch.Tables[0].Rows[0]["AlternateAPN"].ToString() != "")
                            {
                                HttpContext.Current.Session["titleparcel"] = dsFetch.Tables[0].Rows[0]["AlternateAPN"].ToString();
                                readoutput1(HttpContext.Current.Session["titleparcel"].ToString());
                                return;
                            }
                            else if (dsFetch.Tables[0].Rows[0]["AlternateAPN"].ToString() == "" && dsFetch.Tables[0].Rows[0]["OutputParcel"].ToString() == "")
                            {
                                //DataSet ds = new DataSet();
                                string bindquery = "select orderno,parcelno,ownername,address,county,state from multiparcels where orderno = '" + orderNumber + "' group by parcelno";
                                dst = tcon.ExecuteQuery(bindquery);
                                if (dst.Tables[0].Rows.Count > 0)
                                {
                                    GlobalClass.TitleFlex_Search = "Yes";
                                    HttpContext.Current.Session["TitleFlex_Search"] = "Yes";
                                    return;
                                }

                                else
                                {
                                    HttpContext.Current.Session["titleparcel"] = "";
                                    return;
                                }

                            }
                        }
                    }

                    if (totaldays > 29 || dsoutput.Tables[0].Rows.Count == 0)
                    {
                        insert_titleflex(orderNumber, DateTime.Now.ToString(), address, county, "", state, "", ownerName, "", "", parcelNumber, "STARS");
                        XmlDocument XD = new XmlDocument();
                        XmlNode MESSAGE = XD.AppendChild(XD.CreateElement("MESSAGE"));
                        XmlNode DEAL_SETS = MESSAGE.AppendChild(XD.CreateElement("DEAL_SETS"));
                        XmlNode DEAL_SET = DEAL_SETS.AppendChild(XD.CreateElement("DEAL_SET"));
                        XmlAttribute DEAL_SETChildAtt = DEAL_SET.Attributes.Append(XD.CreateAttribute("SequenceNumber"));
                        DEAL_SETChildAtt.InnerText = "";
                        XmlNode DEALS = DEAL_SET.AppendChild(XD.CreateElement("DEALS"));
                        XmlNode DEAL = DEALS.AppendChild(XD.CreateElement("DEAL"));
                        XmlAttribute DEAL_ChildAtt = DEAL.Attributes.Append(XD.CreateAttribute("SequenceNumber"));
                        XmlAttribute DEAL_ChildAtt1 = DEAL.Attributes.Append(XD.CreateAttribute("MISMOReferenceModelIdentifier"));
                        XmlAttribute DEAL_ChildAtt2 = DEAL.Attributes.Append(XD.CreateAttribute("MISMOLogicalDataDictionaryIdentifier"));
                        DEAL_ChildAtt.InnerText = "1";
                        DEAL_ChildAtt1.InnerText = "";
                        DEAL_ChildAtt2.InnerText = "";

                        #region parties
                        XmlNode PARTIES = DEAL.AppendChild(XD.CreateElement("PARTIES"));

                        #region PARTY1
                        XmlNode PARTY1 = PARTIES.AppendChild(XD.CreateElement("PARTY"));
                        XmlAttribute DPARTY1ChildAtt = PARTY1.Attributes.Append(XD.CreateAttribute("SequenceNumber"));
                        DPARTY1ChildAtt.InnerText = "1";

                        // INDIVIDUAL

                        XmlNode INDIVIDUAL1 = PARTY1.AppendChild(XD.CreateElement("INDIVIDUAL"));
                        XmlNode NAME1 = INDIVIDUAL1.AppendChild(XD.CreateElement("NAME"));

                        if (state == "KY" && county == "Fayette")
                        {
                            string[] ownersplit = ownerName.Trim().Split(' ');
                            if (ownersplit.Count() == 2)
                            {
                                XmlNode EducationalAchievementsDescription1 = NAME1.AppendChild(XD.CreateElement("EducationalAchievementsDescription"));

                                XmlNode FirstName1 = NAME1.AppendChild(XD.CreateElement("FirstName"));
                                FirstName1.InnerText = ownersplit[1].Trim();

                                XmlNode MiddleName1 = NAME1.AppendChild(XD.CreateElement("MiddleName"));
                                ///  MiddleName1.InnerText = txtmiddlename.Text.Trim();

                                XmlNode LastName1 = NAME1.AppendChild(XD.CreateElement("LastName"));
                                LastName1.InnerText = ownersplit[0].Trim();
                            }
                            else if (ownersplit.Count() == 3)
                            {
                                XmlNode EducationalAchievementsDescription1 = NAME1.AppendChild(XD.CreateElement("EducationalAchievementsDescription"));
                                XmlNode FirstName1 = NAME1.AppendChild(XD.CreateElement("FirstName"));
                                FirstName1.InnerText = ownersplit[2].Trim();

                                XmlNode MiddleName1 = NAME1.AppendChild(XD.CreateElement("MiddleName"));
                                MiddleName1.InnerText = ownersplit[1].Trim();

                                XmlNode LastName1 = NAME1.AppendChild(XD.CreateElement("LastName"));
                                LastName1.InnerText = ownersplit[0].Trim();
                            }
                            else if (ownersplit.Count() == 1)
                            {
                                XmlNode EducationalAchievementsDescription1 = NAME1.AppendChild(XD.CreateElement("EducationalAchievementsDescription"));
                                XmlNode FirstName1 = NAME1.AppendChild(XD.CreateElement("FirstName"));
                                // FirstName1.InnerText = ownersplit[2].Trim();

                                XmlNode MiddleName1 = NAME1.AppendChild(XD.CreateElement("MiddleName"));
                                //  MiddleName1.InnerText = ownersplit[1].Trim();

                                XmlNode LastName1 = NAME1.AppendChild(XD.CreateElement("LastName"));
                                LastName1.InnerText = ownersplit[0].Trim();
                            }
                        }
                        else
                        {
                            XmlNode EducationalAchievementsDescription1 = NAME1.AppendChild(XD.CreateElement("EducationalAchievementsDescription"));

                            XmlNode FirstName1 = NAME1.AppendChild(XD.CreateElement("FirstName"));
                            // FirstName1.InnerText = txtfirstname.Text.Trim();

                            XmlNode MiddleName1 = NAME1.AppendChild(XD.CreateElement("MiddleName"));
                            ///  MiddleName1.InnerText = txtmiddlename.Text.Trim();

                            XmlNode LastName1 = NAME1.AppendChild(XD.CreateElement("LastName"));
                            LastName1.InnerText = ownerName.Trim();
                        }

                        XmlNode FullName1 = NAME1.AppendChild(XD.CreateElement("FullName"));
                        XmlNode PrefixName1 = NAME1.AppendChild(XD.CreateElement("PrefixName"));
                        XmlNode SuffixName1 = NAME1.AppendChild(XD.CreateElement("SuffixName"));
                        XmlNode EXTENSION1 = NAME1.AppendChild(XD.CreateElement("EXTENSION"));
                        XmlNode IND1EXTENSION = INDIVIDUAL1.AppendChild(XD.CreateElement("EXTENSION"));

                        //Adress

                        XmlNode ADDRESSES1 = PARTY1.AppendChild(XD.CreateElement("ADDRESSES"));
                        XmlNode ADDRESS1 = ADDRESSES1.AppendChild(XD.CreateElement("ADDRESS"));

                        XmlAttribute ADDRESS1ChildAtt = ADDRESS1.Attributes.Append(XD.CreateAttribute("SequenceNumber"));
                        ADDRESS1ChildAtt.InnerText = "";

                        XmlNode AddressType1 = ADDRESS1.AppendChild(XD.CreateElement("AddressType"));
                        AddressType1.InnerText = "Primary";
                        XmlNode AddressLineText1 = ADDRESS1.AppendChild(XD.CreateElement("AddressLineText"));
                        XmlAttribute AddressLineText1_ChildAtt1 = DEAL.Attributes.Append(XD.CreateAttribute("lang"));
                        XmlAttribute AddressLineText1_ChildAtt2 = DEAL.Attributes.Append(XD.CreateAttribute("SensitiveIndicator"));
                        AddressLineText1_ChildAtt1.InnerText = "";
                        AddressLineText1_ChildAtt2.InnerText = "";
                        AddressLineText1.InnerText = address.Trim();

                        XmlNode AddressTypeOtherDescription1 = ADDRESS1.AppendChild(XD.CreateElement("AddressTypeOtherDescription"));
                        XmlNode AddressUnitDesignatorType1 = ADDRESS1.AppendChild(XD.CreateElement("AddressUnitDesignatorType"));
                        AddressUnitDesignatorType1.InnerText = "LOT";
                        XmlNode AddressUnitDesignatorTypeOtherDescription1 = ADDRESS1.AppendChild(XD.CreateElement("AddressUnitDesignatorTypeOtherDescription"));
                        XmlNode AddressUnitIdentifier1 = ADDRESS1.AppendChild(XD.CreateElement("AddressUnitIdentifier"));
                        XmlNode CountryName1 = ADDRESS1.AppendChild(XD.CreateElement("CountryName"));
                        XmlNode CountryCode1 = ADDRESS1.AppendChild(XD.CreateElement("CountryCode"));
                        XmlNode StateName1 = ADDRESS1.AppendChild(XD.CreateElement("StateName"));
                        XmlNode StateCode1 = ADDRESS1.AppendChild(XD.CreateElement("StateCode"));
                        StateCode1.InnerText = state.Trim();
                        XmlNode CountyName1 = ADDRESS1.AppendChild(XD.CreateElement("CountyName"));
                        CountyName1.InnerText = county.Trim();
                        XmlNode CountyCode1 = ADDRESS1.AppendChild(XD.CreateElement("CountyCode"));
                        XmlNode AddressLineText12 = ADDRESS1.AppendChild(XD.CreateElement("AddressLineText"));
                        XmlNode AddressAdditionalLineText1 = ADDRESS1.AppendChild(XD.CreateElement("AddressAdditionalLineText"));
                        XmlNode CityName1 = ADDRESS1.AppendChild(XD.CreateElement("CityName"));
                        //CityName1.InnerText = txtcity.Text.Trim();
                        XmlNode PlusFourZipCode1 = ADDRESS1.AppendChild(XD.CreateElement("PlusFourZipCode"));
                        XmlNode PostalCode1 = ADDRESS1.AppendChild(XD.CreateElement("PostalCode"));
                        //  PostalCode1.InnerText = txtzip.Text.Trim();


                        //Extension APN

                        XmlNode exten = ADDRESS1.AppendChild(XD.CreateElement("EXTENSION"));

                        //  XmlNode propid = exten.AppendChild(XD.CreateElement("PropertyID"));

                        XmlNode legal = exten.AppendChild(XD.CreateElement("LEGAL_DESCRIPTIONS"));
                        XmlAttribute legal_ChildAtt1 = legal.Attributes.Append(XD.CreateAttribute("title"));
                        XmlAttribute legal_ChildAtt2 = legal.Attributes.Append(XD.CreateAttribute("role"));
                        XmlAttribute legal_ChildAtt3 = legal.Attributes.Append(XD.CreateAttribute("label"));
                        XmlAttribute legal_ChildAtt4 = legal.Attributes.Append(XD.CreateAttribute("type"));

                        XmlNode legal1 = legal.AppendChild(XD.CreateElement("LEGAL_DESCRIPTION"));
                        XmlAttribute legal1_ChildAtt1 = legal1.Attributes.Append(XD.CreateAttribute("title"));
                        XmlAttribute legal1_ChildAtt2 = legal1.Attributes.Append(XD.CreateAttribute("role"));
                        XmlAttribute legal1_ChildAtt3 = legal1.Attributes.Append(XD.CreateAttribute("label"));
                        XmlAttribute legal1_ChildAtt4 = legal1.Attributes.Append(XD.CreateAttribute("type"));

                        XmlNode parcelid1 = legal1.AppendChild(XD.CreateElement("PARCEL_IDENTIFICATIONS"));

                        XmlNode parcelid2 = parcelid1.AppendChild(XD.CreateElement("PARCEL_IDENTIFICATION"));

                        XmlNode ParcelIdentificationType = parcelid2.AppendChild(XD.CreateElement("ParcelIdentificationType"));
                        ParcelIdentificationType.InnerText = "ParcelIdentificationNumber";

                        XmlNode parceldescr = parcelid2.AppendChild(XD.CreateElement("ParcelIdentificationTypeOtherDescription"));
                        XmlAttribute legal1parceldescr_ChildAtt1 = parceldescr.Attributes.Append(XD.CreateAttribute("lang"));
                        XmlAttribute egal1lparceldescr_ChildAtt2 = parceldescr.Attributes.Append(XD.CreateAttribute("SensitiveIndicator"));

                        XmlNode Parcelidenti = parcelid2.AppendChild(XD.CreateElement("ParcelIdentifier"));
                        XmlAttribute Parcelidenti_ChildAtt1 = Parcelidenti.Attributes.Append(XD.CreateAttribute("SensitiveIndicator"));
                        XmlAttribute Parcelidenti_ChildAtt2 = Parcelidenti.Attributes.Append(XD.CreateAttribute("IdentifierEffectiveDate"));
                        XmlAttribute Parcelidenti_ChildAtt3 = Parcelidenti.Attributes.Append(XD.CreateAttribute("IdentifierOwnerURI"));
                        Parcelidenti.InnerText = parcelNumber.Trim();


                        // Roles

                        XmlNode ROLES1 = PARTY1.AppendChild(XD.CreateElement("ROLES"));
                        XmlNode ROLE1 = ROLES1.AppendChild(XD.CreateElement("ROLE"));

                        XmlNode PROPERTY_OWNER = ROLE1.AppendChild(XD.CreateElement("PROPERTY_OWNER"));
                        XmlNode PROPERTY_OWNER_EXTENSION = PROPERTY_OWNER.AppendChild(XD.CreateElement("EXTENSION"));

                        XmlNode ROLE_DETAIL1 = ROLE1.AppendChild(XD.CreateElement("ROLE_DETAIL"));
                        XmlNode ROLE_DETAIL1_PartyRoleType = ROLE_DETAIL1.AppendChild(XD.CreateElement("PartyRoleType"));
                        ROLE_DETAIL1_PartyRoleType.InnerText = "PropertyOwner";

                        XmlNode ROLE_DETAIL1_EXTENSION = ROLE1.AppendChild(XD.CreateElement("EXTENSION"));


                        //XmlNode parcel = ROLE_DETAIL1_EXTENSION.AppendChild(XD.CreateElement("PARCEL_IDENTIFICATION"));
                        //XmlNode parceltype = parcel.AppendChild(XD.CreateElement("ParcelIdentificationType"));
                        //parceltype.InnerText = "ParcelIdentificationNumber";

                        //XmlNode parcelid = parcel.AppendChild(XD.CreateElement("ParcelIdentifier"));
                        //parcelid.InnerText = txtparcel.Text.Trim();


                        #endregion

                        #region PARTY2
                        XmlNode PARTY2 = PARTIES.AppendChild(XD.CreateElement("PARTY"));
                        XmlAttribute DPARTY2ChildAtt = PARTY2.Attributes.Append(XD.CreateAttribute("SequenceNumber"));
                        DPARTY2ChildAtt.InnerText = "2";

                        XmlNode INDIVIDUAL2 = PARTY2.AppendChild(XD.CreateElement("INDIVIDUAL"));
                        XmlNode NAME2 = INDIVIDUAL2.AppendChild(XD.CreateElement("NAME"));

                        XmlNode EducationalAchievementsDescription2 = NAME2.AppendChild(XD.CreateElement("EducationalAchievementsDescription"));
                        XmlNode FirstName2 = NAME2.AppendChild(XD.CreateElement("FirstName"));
                        XmlNode MiddleName2 = NAME2.AppendChild(XD.CreateElement("MiddleName"));
                        XmlNode LastName2 = NAME2.AppendChild(XD.CreateElement("LastName"));
                        XmlNode FullName2 = NAME2.AppendChild(XD.CreateElement("FullName"));
                        XmlNode PrefixName2 = NAME2.AppendChild(XD.CreateElement("PrefixName"));
                        XmlNode SuffixName2 = NAME2.AppendChild(XD.CreateElement("SuffixName"));
                        XmlNode EXTENSION2 = NAME2.AppendChild(XD.CreateElement("EXTENSION"));

                        XmlNode IND2EXTENSION = INDIVIDUAL2.AppendChild(XD.CreateElement("EXTENSION"));


                        //Adress

                        XmlNode ADDRESSES2 = PARTY2.AppendChild(XD.CreateElement("ADDRESSES"));
                        XmlNode ADDRESS2 = ADDRESSES2.AppendChild(XD.CreateElement("ADDRESS"));

                        XmlAttribute ADDRESS2ChildAtt = ADDRESS2.Attributes.Append(XD.CreateAttribute("SequenceNumber"));
                        ADDRESS2ChildAtt.InnerText = "1";

                        XmlNode AddressType2 = ADDRESS2.AppendChild(XD.CreateElement("AddressType"));
                        AddressType2.InnerText = "Primary";
                        XmlNode AddressTypeOtherDescription2 = ADDRESS2.AppendChild(XD.CreateElement("AddressTypeOtherDescription"));
                        XmlNode AddressUnitDesignatorType2 = ADDRESS2.AppendChild(XD.CreateElement("AddressUnitDesignatorType"));
                        AddressUnitDesignatorType2.InnerText = "LOT";
                        XmlNode AddressUnitDesignatorTypeOtherDescription2 = ADDRESS2.AppendChild(XD.CreateElement("AddressUnitDesignatorTypeOtherDescription"));
                        XmlNode AddressUnitIdentifier2 = ADDRESS2.AppendChild(XD.CreateElement("AddressUnitIdentifier"));
                        XmlNode CountryName2 = ADDRESS2.AppendChild(XD.CreateElement("CountryName"));
                        XmlNode CountryCode2 = ADDRESS2.AppendChild(XD.CreateElement("CountryCode"));
                        XmlNode StateName2 = ADDRESS2.AppendChild(XD.CreateElement("StateName"));
                        XmlNode StateCode2 = ADDRESS2.AppendChild(XD.CreateElement("StateCode"));
                        XmlNode CountyName2 = ADDRESS2.AppendChild(XD.CreateElement("CountyName"));
                        XmlNode CountyCode2 = ADDRESS2.AppendChild(XD.CreateElement("CountyCode"));
                        XmlNode AddressLineText22 = ADDRESS2.AppendChild(XD.CreateElement("AddressLineText"));
                        XmlNode AddressAdditionalLineText2 = ADDRESS2.AppendChild(XD.CreateElement("AddressAdditionalLineText"));
                        XmlNode CityName2 = ADDRESS2.AppendChild(XD.CreateElement("CityName"));
                        XmlNode PlusFourZipCode2 = ADDRESS2.AppendChild(XD.CreateElement("PlusFourZipCode"));
                        XmlNode PostalCode2 = ADDRESS2.AppendChild(XD.CreateElement("PostalCode"));



                        // Roles

                        XmlNode ROLES2 = PARTY2.AppendChild(XD.CreateElement("ROLES"));
                        XmlNode ROLE2 = ROLES2.AppendChild(XD.CreateElement("ROLE"));

                        XmlNode SUBMITTING_PARTY = ROLE2.AppendChild(XD.CreateElement("SUBMITTING_PARTY"));
                        XmlNode SubmittingPartySequenceNumber = SUBMITTING_PARTY.AppendChild(XD.CreateElement("SubmittingPartySequenceNumber"));
                        SubmittingPartySequenceNumber.InnerText = "1";
                        XmlNode SubmittingPartyTransactionIdentifier = SUBMITTING_PARTY.AppendChild(XD.CreateElement("SubmittingPartyTransactionIdentifier"));
                        XmlNode SubmittingPartyEXTENSION = SUBMITTING_PARTY.AppendChild(XD.CreateElement("EXTENSION"));
                        XmlNode SubmittingPartyLoginAccountIdentifier = SubmittingPartyEXTENSION.AppendChild(XD.CreateElement("LoginAccountIdentifier"));
                        XmlNode SubmittingPartyLoginAccountPassword = SubmittingPartyEXTENSION.AppendChild(XD.CreateElement("LoginAccountPassword"));
                        XmlNode ROLE_DETAIL2 = ROLE2.AppendChild(XD.CreateElement("ROLE_DETAIL"));
                        XmlNode ROLE_DETAIL2_PartyRoleType = ROLE_DETAIL2.AppendChild(XD.CreateElement("PartyRoleType"));
                        ROLE_DETAIL2_PartyRoleType.InnerText = "SubmittingParty";

                        XmlNode ROLE_DETAIL2_EXTENSION = ROLE2.AppendChild(XD.CreateElement("EXTENSION"));


                        #endregion

                        #region PARTY3
                        XmlNode PARTY3 = PARTIES.AppendChild(XD.CreateElement("PARTY"));
                        XmlAttribute DPARTY3ChildAtt = PARTY3.Attributes.Append(XD.CreateAttribute("SequenceNumber"));
                        DPARTY3ChildAtt.InnerText = "3";

                        XmlNode INDIVIDUAL3 = PARTY3.AppendChild(XD.CreateElement("INDIVIDUAL"));
                        XmlNode NAME3 = INDIVIDUAL3.AppendChild(XD.CreateElement("NAME"));

                        XmlNode EducationalAchievementsDescription3 = NAME3.AppendChild(XD.CreateElement("EducationalAchievementsDescription"));
                        XmlNode FirstName3 = NAME3.AppendChild(XD.CreateElement("FirstName"));
                        XmlNode MiddleName3 = NAME3.AppendChild(XD.CreateElement("MiddleName"));
                        XmlNode LastName3 = NAME3.AppendChild(XD.CreateElement("LastName"));
                        XmlNode FullName3 = NAME3.AppendChild(XD.CreateElement("FullName"));
                        XmlNode PrefixName3 = NAME3.AppendChild(XD.CreateElement("PrefixName"));
                        XmlNode SuffixName3 = NAME3.AppendChild(XD.CreateElement("SuffixName"));
                        XmlNode EXTENSION3 = NAME3.AppendChild(XD.CreateElement("EXTENSION"));

                        XmlNode IND3EXTENSION = INDIVIDUAL3.AppendChild(XD.CreateElement("EXTENSION"));

                        //Adress

                        XmlNode ADDRESSES3 = PARTY3.AppendChild(XD.CreateElement("ADDRESSES"));
                        XmlNode ADDRESS3 = ADDRESSES3.AppendChild(XD.CreateElement("ADDRESS"));

                        XmlAttribute ADDRESS3ChildAtt = ADDRESS3.Attributes.Append(XD.CreateAttribute("SequenceNumber"));
                        ADDRESS3ChildAtt.InnerText = "1";

                        XmlNode AddressType3 = ADDRESS3.AppendChild(XD.CreateElement("AddressType"));
                        AddressType3.InnerText = "Primary";
                        XmlNode AddressTypeOtherDescription3 = ADDRESS3.AppendChild(XD.CreateElement("AddressTypeOtherDescription"));
                        XmlNode AddressUnitDesignatorType3 = ADDRESS3.AppendChild(XD.CreateElement("AddressUnitDesignatorType"));
                        AddressUnitDesignatorType3.InnerText = "LOT";
                        XmlNode AddressUnitDesignatorTypeOtherDescription3 = ADDRESS3.AppendChild(XD.CreateElement("AddressUnitDesignatorTypeOtherDescription"));
                        XmlNode AddressUnitIdentifier3 = ADDRESS3.AppendChild(XD.CreateElement("AddressUnitIdentifier"));
                        XmlNode CountryName3 = ADDRESS3.AppendChild(XD.CreateElement("CountryName"));
                        XmlNode CountryCode3 = ADDRESS3.AppendChild(XD.CreateElement("CountryCode"));
                        XmlNode StateName3 = ADDRESS3.AppendChild(XD.CreateElement("StateName"));
                        XmlNode StateCode3 = ADDRESS3.AppendChild(XD.CreateElement("StateCode"));
                        XmlNode CountyName3 = ADDRESS3.AppendChild(XD.CreateElement("CountyName"));
                        XmlNode CountyCode3 = ADDRESS3.AppendChild(XD.CreateElement("CountyCode"));
                        XmlNode AddressLineText32 = ADDRESS3.AppendChild(XD.CreateElement("AddressLineText"));
                        XmlNode AddressAdditionalLineText3 = ADDRESS3.AppendChild(XD.CreateElement("AddressAdditionalLineText"));
                        XmlNode CityName3 = ADDRESS3.AppendChild(XD.CreateElement("CityName"));
                        XmlNode PlusFourZipCode3 = ADDRESS3.AppendChild(XD.CreateElement("PlusFourZipCode"));
                        XmlNode PostalCode3 = ADDRESS3.AppendChild(XD.CreateElement("PostalCode"));

                        // Roles

                        XmlNode ROLES3 = PARTY3.AppendChild(XD.CreateElement("ROLES"));
                        XmlNode ROLE3 = ROLES3.AppendChild(XD.CreateElement("ROLE"));
                        XmlNode REQUESTING_PARTY = ROLE3.AppendChild(XD.CreateElement("REQUESTING_PARTY"));
                        XmlNode InternalAccountIdentifier = REQUESTING_PARTY.AppendChild(XD.CreateElement("InternalAccountIdentifier"));
                        XmlNode RequestedByName = REQUESTING_PARTY.AppendChild(XD.CreateElement("RequestedByName"));
                        XmlNode RequestingPartyBranchIdentifier = REQUESTING_PARTY.AppendChild(XD.CreateElement("RequestingPartyBranchIdentifier"));
                        XmlNode RequestingPartySequenceNumber = REQUESTING_PARTY.AppendChild(XD.CreateElement("RequestingPartySequenceNumber"));
                        RequestingPartySequenceNumber.InnerText = "1";
                        XmlNode RequestingPartyEXTENSION = REQUESTING_PARTY.AppendChild(XD.CreateElement("EXTENSION"));
                        XmlNode RequestingPartyLoginAccountIdentifier = RequestingPartyEXTENSION.AppendChild(XD.CreateElement("LoginAccountIdentifier"));
                        RequestingPartyLoginAccountIdentifier.InnerText = "StringInfo";
                        XmlNode RequestingPartyLoginAccountPassword = RequestingPartyEXTENSION.AppendChild(XD.CreateElement("LoginAccountPassword"));
                        RequestingPartyLoginAccountPassword.InnerText = "StringXML1@";

                        XmlNode ROLE_DETAIL3 = ROLE3.AppendChild(XD.CreateElement("ROLE_DETAIL"));
                        XmlNode ROLE_DETAIL3_PartyRoleType = ROLE_DETAIL3.AppendChild(XD.CreateElement("PartyRoleType"));
                        ROLE_DETAIL3_PartyRoleType.InnerText = "SubmittingParty";

                        XmlNode ROLE_DETAIL3_EXTENSION = ROLE3.AppendChild(XD.CreateElement("EXTENSION"));

                        #endregion

                        #region PARTY4

                        XmlNode PARTY4 = PARTIES.AppendChild(XD.CreateElement("PARTY"));
                        XmlAttribute DPARTY4ChildAtt = PARTY4.Attributes.Append(XD.CreateAttribute("SequenceNumber"));
                        DPARTY4ChildAtt.InnerText = "4";

                        XmlNode INDIVIDUAL4 = PARTY4.AppendChild(XD.CreateElement("INDIVIDUAL"));
                        XmlNode NAME4 = INDIVIDUAL4.AppendChild(XD.CreateElement("NAME"));

                        XmlNode EducationalAchievementsDescription4 = NAME4.AppendChild(XD.CreateElement("EducationalAchievementsDescription"));
                        XmlNode FirstName4 = NAME4.AppendChild(XD.CreateElement("FirstName"));
                        XmlNode MiddleName4 = NAME4.AppendChild(XD.CreateElement("MiddleName"));
                        XmlNode LastName4 = NAME4.AppendChild(XD.CreateElement("LastName"));
                        XmlNode FullName4 = NAME4.AppendChild(XD.CreateElement("FullName"));
                        XmlNode PrefixName4 = NAME4.AppendChild(XD.CreateElement("PrefixName"));
                        XmlNode SuffixName4 = NAME4.AppendChild(XD.CreateElement("SuffixName"));
                        XmlNode EXTENSION4 = NAME4.AppendChild(XD.CreateElement("EXTENSION"));

                        XmlNode IND4EXTENSION = INDIVIDUAL4.AppendChild(XD.CreateElement("EXTENSION"));

                        //Adress

                        XmlNode ADDRESSES4 = PARTY4.AppendChild(XD.CreateElement("ADDRESSES"));
                        XmlNode ADDRESS4 = ADDRESSES4.AppendChild(XD.CreateElement("ADDRESS"));

                        XmlAttribute ADDRESS4ChildAtt = ADDRESS4.Attributes.Append(XD.CreateAttribute("SequenceNumber"));
                        ADDRESS4ChildAtt.InnerText = "1";

                        XmlNode AddressType4 = ADDRESS4.AppendChild(XD.CreateElement("AddressType"));
                        AddressType4.InnerText = "Primary";
                        XmlNode AddressTypeOtherDescription4 = ADDRESS4.AppendChild(XD.CreateElement("AddressTypeOtherDescription"));
                        XmlNode AddressUnitDesignatorType4 = ADDRESS4.AppendChild(XD.CreateElement("AddressUnitDesignatorType"));
                        AddressUnitDesignatorType4.InnerText = "LOT";
                        XmlNode AddressUnitDesignatorTypeOtherDescription4 = ADDRESS4.AppendChild(XD.CreateElement("AddressUnitDesignatorTypeOtherDescription"));
                        XmlNode AddressUnitIdentifier4 = ADDRESS4.AppendChild(XD.CreateElement("AddressUnitIdentifier"));
                        XmlNode CountryName4 = ADDRESS4.AppendChild(XD.CreateElement("CountryName"));
                        XmlNode CountryCode4 = ADDRESS4.AppendChild(XD.CreateElement("CountryCode"));
                        XmlNode StateName4 = ADDRESS4.AppendChild(XD.CreateElement("StateName"));
                        XmlNode StateCode4 = ADDRESS4.AppendChild(XD.CreateElement("StateCode"));
                        XmlNode CountyName4 = ADDRESS4.AppendChild(XD.CreateElement("CountyName"));
                        XmlNode CountyCode4 = ADDRESS4.AppendChild(XD.CreateElement("CountyCode"));
                        XmlNode AddressLineText42 = ADDRESS4.AppendChild(XD.CreateElement("AddressLineText"));
                        XmlNode AddressAdditionalLineText4 = ADDRESS4.AppendChild(XD.CreateElement("AddressAdditionalLineText"));
                        XmlNode CityName4 = ADDRESS4.AppendChild(XD.CreateElement("CityName"));
                        XmlNode PlusFourZipCode4 = ADDRESS4.AppendChild(XD.CreateElement("PlusFourZipCode"));
                        XmlNode PostalCode4 = ADDRESS4.AppendChild(XD.CreateElement("PostalCode"));


                        // Roles

                        XmlNode ROLES4 = PARTY4.AppendChild(XD.CreateElement("ROLES"));
                        XmlNode ROLE4 = ROLES4.AppendChild(XD.CreateElement("ROLE"));
                        XmlNode RETURN_TO = ROLE4.AppendChild(XD.CreateElement("RETURN_TO"));
                        XmlNode PREFERRED_RESPONSES = RETURN_TO.AppendChild(XD.CreateElement("PREFERRED_RESPONSES"));

                        XmlNode PREFERRED_RESPONSES1 = PREFERRED_RESPONSES.AppendChild(XD.CreateElement("PREFERRED_RESPONSE"));
                        XmlAttribute PREFERRED_RESPONSES1ChildAtt = PREFERRED_RESPONSES1.Attributes.Append(XD.CreateAttribute("SequenceNumber"));
                        PREFERRED_RESPONSES1ChildAtt.InnerText = "";

                        XmlNode PreferredResponseFormatType = PREFERRED_RESPONSES1.AppendChild(XD.CreateElement("PreferredResponseFormatType"));
                        PreferredResponseFormatType.InnerText = "XML";
                        XmlNode PreferredResponseMethodType = PREFERRED_RESPONSES1.AppendChild(XD.CreateElement("PreferredResponseMethodType"));
                        PreferredResponseMethodType.InnerText = "HTTP";

                        XmlNode ROLE_DETAIL4 = ROLE4.AppendChild(XD.CreateElement("ROLE_DETAIL"));
                        XmlNode ROLE_DETAIL4_PartyRoleType = ROLE_DETAIL4.AppendChild(XD.CreateElement("PartyRoleType"));
                        ROLE_DETAIL4_PartyRoleType.InnerText = "RespondToParty";

                        XmlNode ROLE_DETAIL4_EXTENSION = ROLE4.AppendChild(XD.CreateElement("EXTENSION"));

                        #endregion

                        #endregion

                        #region Services
                        XmlNode SERVICES = DEAL.AppendChild(XD.CreateElement("SERVICES"));
                        XmlNode SERVICE = SERVICES.AppendChild(XD.CreateElement("SERVICE"));
                        XmlNode SERVICE_PRODUCT = SERVICE.AppendChild(XD.CreateElement("SERVICE_PRODUCT"));
                        XmlNode SERVICE_PRODUCT_REQUEST = SERVICE_PRODUCT.AppendChild(XD.CreateElement("SERVICE_PRODUCT_REQUEST"));

                        XmlNode SERVICE_PRODUCT_DETAIL = SERVICE_PRODUCT_REQUEST.AppendChild(XD.CreateElement("SERVICE_PRODUCT_DETAIL"));
                        XmlNode ServiceProductDescription = SERVICE_PRODUCT_DETAIL.AppendChild(XD.CreateElement("ServiceProductDescription"));
                        ServiceProductDescription.InnerText = "Property Information";
                        XmlNode ServiceProductIdentifier = SERVICE_PRODUCT_DETAIL.AppendChild(XD.CreateElement("ServiceProductIdentifier"));
                        ServiceProductIdentifier.InnerText = "PIB3";
                        XmlNode SERVICESEXTENSION = SERVICE_PRODUCT_DETAIL.AppendChild(XD.CreateElement("EXTENSION"));
                        XmlNode ServiceProductOperationType = SERVICESEXTENSION.AppendChild(XD.CreateElement("ServiceProductOperationType"));
                        ServiceProductOperationType.InnerText = "Create";
                        XmlNode ServiceProductNotesDescription = SERVICESEXTENSION.AppendChild(XD.CreateElement("ServiceProductNotesDescription"));
                        XmlNode ServiceProductReportReturnType = SERVICESEXTENSION.AppendChild(XD.CreateElement("ServiceProductReportReturnType"));
                        XmlNode ServiceProductImageReturnType = SERVICESEXTENSION.AppendChild(XD.CreateElement("ServiceProductImageReturnType"));
                        XmlNode ServiceProductEmailDeliveryAdrs = SERVICESEXTENSION.AppendChild(XD.CreateElement("ServiceProductEmailDeliveryAdrs"));
                        XmlNode SubjectLienRecordedDateRangeStartDate = SERVICESEXTENSION.AppendChild(XD.CreateElement("SubjectLienRecordedDateRangeStartDate"));
                        XmlNode SubjectLienRecordedDateRangeEndDate = SERVICESEXTENSION.AppendChild(XD.CreateElement("SubjectLienRecordedDateRangeEndDate"));
                        XmlNode NumberSubjectPropertiesType = SERVICESEXTENSION.AppendChild(XD.CreateElement("NumberSubjectPropertiesType"));
                        NumberSubjectPropertiesType.InnerText = "25";

                        XmlNode SERVICE_PRODUCT_REQUEST_EXTENSION = SERVICE_PRODUCT_REQUEST.AppendChild(XD.CreateElement("EXTENSION"));

                        XmlNode SERVICE_PRODUCT_NAMES = SERVICE_PRODUCT_REQUEST.AppendChild(XD.CreateElement("SERVICE_PRODUCT_NAMES"));
                        XmlNode SERVICE_PRODUCT_NAME = SERVICE_PRODUCT_REQUEST.AppendChild(XD.CreateElement("SERVICE_PRODUCT_NAME"));
                        XmlAttribute SERVICE_PRODUCT_NAMEchildatt = SERVICE_PRODUCT_NAME.Attributes.Append(XD.CreateAttribute("SequenceNumber"));
                        SERVICE_PRODUCT_NAMEchildatt.InnerText = "";

                        XmlNode SERVICE_PRODUCT_NAME_DETAIL = SERVICE_PRODUCT_NAME.AppendChild(XD.CreateElement("SERVICE_PRODUCT_NAME_DETAIL"));
                        XmlNode ServiceProductNameDescription = SERVICE_PRODUCT_NAME_DETAIL.AppendChild(XD.CreateElement("ServiceProductNameDescription"));
                        XmlNode ServiceProductNameIdentifier = SERVICE_PRODUCT_NAME_DETAIL.AppendChild(XD.CreateElement("ServiceProductNameIdentifier"));

                        XmlNode SERVICE_PRODUCT_NAME_DETAIL_EXTENSION = SERVICE_PRODUCT_NAME.AppendChild(XD.CreateElement("EXTENSION"));

                        XmlNode SERVICE_PRODUCT_NAME_EXTENSION = SERVICE_PRODUCT_NAME.AppendChild(XD.CreateElement("EXTENSION"));

                        #endregion
                        if (!Directory.Exists(strInput))
                        {
                            DirectoryInfo di = Directory.CreateDirectory(strInput);
                        }

                        string filename = strInput + orderNumber.Trim() + ".xml";
                        XD.Save(filename);

                        postXMLData("https://xmldata.datatree.com/XmlPost/PlaceOrder", filename, orderNumber);
                        readxml(orderNumber, parcelNumber, ownerName, address, state, county);


                        DataSet ds = new DataSet();
                       // string insert = "insert into tbl_output (orderno, county, state, apn, OutputParcel ,AlternateAPN, lastupdateddate)  values ('" + orderNumber + "', '" + county + "' ,'" + state + "','" + parcelNumber + "','" + outputparcel + "','" + alternateAPN + "','" + DateTime.Now.ToString() + "')";
                        string insert = "insert into tbl_output (orderno, county, state, apn, OutputParcel ,AlternateAPN, lastupdateddate,username)  values ('" + orderNumber + "', '" + county + "' ,'" + state + "','" + parcelNumber + "','" + parcelno + "','" + alternateAPN + "','" + DateTime.Now.ToString() + "','STARS')";
                        ds = tcon.ExecuteQuery(insert);

                        dsFetch = tcon.ExecuteQuery("select OutputParcel,AlternateAPN from tbl_output where orderno = '" + orderNumber + "' order by lastupdateddate desc limit 1 ");
                        if (dsFetch.Tables[0].Rows.Count > 0)
                        {
                            if (dsFetch.Tables[0].Rows[0]["OutputParcel"].ToString() != "")
                            {
                                HttpContext.Current.Session["titleparcel"] = dsFetch.Tables[0].Rows[0]["OutputParcel"].ToString();
                                readoutput(HttpContext.Current.Session["titleparcel"].ToString());
                                return;
                            }
                            else if (dsFetch.Tables[0].Rows[0]["AlternateAPN"].ToString() != "")
                            {
                                HttpContext.Current.Session["titleparcel"] = dsFetch.Tables[0].Rows[0]["AlternateAPN"].ToString();
                                readoutput1(HttpContext.Current.Session["titleparcel"].ToString());
                                return;
                            }
                            else if (dsFetch.Tables[0].Rows[0]["AlternateAPN"].ToString() == "" && dsFetch.Tables[0].Rows[0]["OutputParcel"].ToString() == "")
                            {                              
                                string bindquery = "select orderno,parcelno,ownername,address,county,state from multiparcels where orderno = '" + orderNumber + "' group by parcelno";
                                ds = tcon.ExecuteQuery(bindquery);
                                if (ds.Tables[0].Rows.Count > 0)
                                {
                                    GlobalClass.TitleFlex_Search = "Yes";
                                    HttpContext.Current.Session["TitleFlex_Search"] = "Yes";
                                    return;
                                }

                                else
                                {
                                    HttpContext.Current.Session["titleparcel"] = "";
                                    return;
                                }

                            }
                        }
                    }
                }
            }
        }

        public string readoutput(string outputparcel)
        {
            string result = "";
            if (outputparcel != "")
            {
                result = outputparcel;
            }

            else
            {
                result = "None";
            }
            return result;
        }

        public string readoutput1(string AlternateAPN)
        {
            string result = "";
            if (AlternateAPN != "")
            {
                result = AlternateAPN;
            }
            else
            {
                result = "None";
            }
            return result;
        }


        public string postXMLData(string destinationUrl, string requestXml, string orderNumber)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(destinationUrl);
            string strXml;
            using (StreamReader sr = new StreamReader(requestXml))
            {
                strXml = sr.ReadToEnd();
            }
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(strXml);
            byte[] bytes;
            bytes = System.Text.Encoding.ASCII.GetBytes(doc.InnerXml);
            request.ContentType = "text/xml; encoding='utf-8'";
            request.ContentLength = bytes.Length;
            request.Method = "POST";

            Stream requestStream = request.GetRequestStream();
            requestStream.Write(bytes, 0, bytes.Length);
            requestStream.Close();
            HttpWebResponse response;
            response = (HttpWebResponse)request.GetResponse();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream responseStream = response.GetResponseStream();
                string responseStr = new StreamReader(responseStream).ReadToEnd();
                XmlDocument xdoc = new XmlDocument();
                xdoc.LoadXml(responseStr);

                if (!Directory.Exists(strOutput))
                {
                    DirectoryInfo di = Directory.CreateDirectory(strOutput);
                }

                string strXml1 = strOutput + orderNumber + ".xml";
                xdoc.Save(strXml1);


                XmlDocument doc1 = new XmlDocument();
                doc1.Load(strXml1);
                XmlNodeList node = doc1.GetElementsByTagName("StatusDescription");
                string successinnertext = "";
                foreach (XmlNode nd in node)
                {
                    successinnertext = nd.InnerText;
                }

            }
            return null;
        }

        string paddress = "", ownername = "", parcelno = "", exemptiondet = "", totalassvalue = "", taxamountrate = "", cityname = "", countyname = "", statename = "";
        string legal = "", assessedyear = "", taxyear = "", propertytax = "", landvalue = "", improvementvalue = "", statusmsg = "", yearBuiltValue = "", alternateAPN = "", tra = "";
        public string readxml(string orderNumber, string parcelNumber, string ownerName, string straddress, string strstate, string strcounty)
        {
            string strXmlread = strOutput + orderNumber.Trim() + ".xml";

            XmlDocument docread = new XmlDocument();
            docread.Load(strXmlread);
            XmlNodeList node = docread.GetElementsByTagName("StatusDescription");
            string successinnertext = "";
            foreach (XmlNode nd in node)
            {
                successinnertext = nd.InnerText;
                break;
            }

            XmlNodeList nodep1type = docread.GetElementsByTagName("PARTY");
            //string paddress = "", ownername = "", parcelno = "", exemptiondet = "", totalassvalue = "", taxamountrate = "", cityname = "", countyname = "", statename = "";
            //string legal = "", assessedyear = "", taxyear = "", propertytax = "", landvalue = "", improvementvalue = "", statusmsg = "", yearBuiltValue = "", alternateAPN = "", tra = "";

            XmlNodeList names = docread.GetElementsByTagName("FullName");
            foreach (XmlNode name in names)
            {
                ownername = name.InnerText;
                //txtownername.Text = name.InnerText;
                break;
            }


            XmlNodeList sales = docread.GetElementsByTagName("SALES_HISTORIES");

            foreach (XmlNode sale in sales)
            {
                foreach (XmlNode chd in sale.ChildNodes)
                {
                    foreach (XmlNode exten in chd.ChildNodes)
                    {
                        //string sst = exten["TRANSACTION_HISTORY"].InnerXml;
                    }
                }
            }

            //address
            XmlNodeList addresses = docread.GetElementsByTagName("AddressLineText");

            foreach (XmlNode address in addresses)
            {

                paddress = address.InnerText;
                if (paddress == "")
                {
                    for (int i = 0; i < addresses.Count; i++)
                    {
                        if (paddress == "")
                        {
                            paddress = addresses[i].InnerText;
                        }
                    }
                }


                XmlNodeList cities = docread.GetElementsByTagName("CityName");

                foreach (XmlNode city in cities)
                {

                    cityname = city.InnerText;
                    if (cityname == "")
                    {
                        for (int i = 0; i < cities.Count; i++)
                        {
                            if (cityname == "")
                            {
                                cityname = cities[i].InnerText;
                                // break;
                            }
                        }
                    }
                    break;

                }


                XmlNodeList counties = docread.GetElementsByTagName("CountyName");

                foreach (XmlNode county in counties)
                {

                    countyname = county.InnerText;
                    if (countyname == "")
                    {
                        for (int i = 0; i < counties.Count; i++)
                        {
                            if (countyname == "")
                            {
                                countyname = counties[i].InnerText;
                                //break;
                            }
                        }
                    }
                    // break;
                }

                XmlNodeList states = docread.GetElementsByTagName("StateCode");

                foreach (XmlNode state in states)
                {
                    statename = state.InnerText;
                    if (statename == "")
                    {
                        for (int i = 0; i < states.Count; i++)
                        {
                            if (statename == "")
                            {
                                statename = states[i].InnerText;
                                // break;
                            }
                        }
                    }
                    break;
                }
                if (cityname.ToLower() == countyname.ToLower())
                {
                    paddress = paddress + ",\r\n" + countyname + ",\r\n" + statename;
                }
                else
                {
                    paddress = paddress + ",\r\n" + cityname + ",\r\n" + countyname + ",\r\n" + statename;
                }
                //txtpaddress.Text = paddress;
                break;

            }

            //parcelid
            XmlNodeList parcels = docread.GetElementsByTagName("ParcelIdentifier");

            foreach (XmlNode parcel in parcels)
            {

                parcelno = parcel.InnerText;
                // txtparcelid.Text = parcel.InnerText;
                break;
            }

            //exemptions
            XmlNodeList exemptions = docread.GetElementsByTagName("PROPERTY_TAX_EXEMPTIONS");

            foreach (XmlNode exemption in exemptions)
            {
                exemptiondet = exemption.InnerText;
                if (exemptiondet.Trim() == "")
                {
                    for (int i = 0; i < exemptions.Count; i++)
                    {
                        if (exemptiondet == "")
                        {
                            exemptiondet = exemptions[i].InnerText;
                            // break;
                        }
                    }
                }
                break;
            }

            //totalassessedvalue
            XmlNodeList tavs = docread.GetElementsByTagName("PropertyTaxTotalAssessedValueAmount");

            foreach (XmlNode tav in tavs)
            {
                totalassvalue = tav.InnerText;
                if (totalassvalue.Trim() == "")
                {
                    for (int i = 0; i < tavs.Count; i++)
                    {
                        if (totalassvalue == "")
                        {
                            totalassvalue = tavs[i].InnerText;
                            //break;
                        }
                    }
                }
                break;
            }

            //totaltaxvalue
            XmlNodeList taxamounts = docread.GetElementsByTagName("PropertyTaxTotalTaxAmount");
            foreach (XmlNode taxamount in taxamounts)
            {
                taxamountrate = taxamount.InnerText;
                if (taxamountrate.Trim() == "")
                {
                    for (int i = 0; i < taxamounts.Count; i++)
                    {
                        if (taxamountrate == "")
                        {
                            taxamountrate = taxamounts[i].InnerText;
                            //break;
                        }
                    }
                }
                break;
            }


            //Legal Description
            XmlNodeList leagal = docread.GetElementsByTagName("UnparsedLegalDescription");
            foreach (XmlNode lea in leagal)
            {
                legal = lea.InnerText;
                if (legal.Trim() == "")
                {
                    for (int i = 0; i < leagal.Count; i++)
                    {
                        if (legal == "")
                        {
                            legal = leagal[i].InnerText;
                            //break;
                        }
                    }
                }
                break;
            }


            //Assement Year
            XmlNodeList year = docread.GetElementsByTagName("PropertyTaxAssessmentEndYear");
            foreach (XmlNode ye in year)
            {
                assessedyear = ye.InnerText;
                if (assessedyear.Trim() == "")
                {
                    for (int i = 0; i < year.Count; i++)
                    {
                        if (assessedyear == "")
                        {
                            assessedyear = year[i].InnerText;
                            //break;
                        }
                    }
                }
                break;
            }

            //Tax Year
            XmlNodeList taxy = docread.GetElementsByTagName("PropertyTaxYearIdentifier");
            foreach (XmlNode tax in taxy)
            {
                taxyear = tax.InnerText;
                if (taxyear.Trim() == "")
                {
                    for (int i = 0; i < taxy.Count; i++)
                    {
                        if (taxyear == "")
                        {
                            taxyear = taxy[i].InnerText;
                            // break;
                        }
                    }
                }
                break;
            }


            //Property Tax
            XmlNodeList propertytax1 = docread.GetElementsByTagName("PropertyTaxTotalTaxAmount");
            foreach (XmlNode pro in propertytax1)
            {
                propertytax = pro.InnerText;
                if (propertytax.Trim() == "")
                {
                    for (int i = 0; i < propertytax1.Count; i++)
                    {
                        if (propertytax == "")
                        {
                            propertytax = propertytax1[i].InnerText;
                            //break;
                        }
                    }
                }
                break;
            }


            //Land value
            XmlNodeList land = docread.GetElementsByTagName("PropertyTaxLandValueAmount");
            foreach (XmlNode la in land)
            {
                landvalue = la.InnerText;
                if (landvalue.Trim() == "")
                {
                    for (int i = 0; i < land.Count; i++)
                    {
                        if (landvalue == "")
                        {
                            landvalue = land[i].InnerText;
                            // break;
                        }
                    }
                }
                break;
            }


            //Improvement value
            XmlNodeList improvement = docread.GetElementsByTagName("PropertyTaxImprovementValueAmount");
            foreach (XmlNode imp in improvement)
            {
                improvementvalue = imp.InnerText;
                if (improvementvalue.Trim() == "")
                {
                    for (int i = 0; i < improvement.Count; i++)
                    {
                        if (improvementvalue == "")
                        {
                            improvementvalue = improvement[i].InnerText;
                            // break;
                        }
                    }
                }
                break;
            }

            //Status Code
            XmlNodeList statuscode = docread.GetElementsByTagName("StatusCode");
            foreach (XmlNode suc in statuscode)
            {
                statusmsg = suc.InnerText;
                break;
            }

            //Year Built...
            XmlNodeList yearBuilt = docread.GetElementsByTagName("PropertyStructureBuiltYear");
            foreach (XmlNode yrb in yearBuilt)
            {
                yearBuiltValue = yrb.InnerText;
                if (yearBuiltValue.Trim() == "")
                {
                    for (int i = 0; i < yearBuilt.Count; i++)
                    {
                        if (yearBuiltValue == "")
                        {
                            yearBuiltValue = yearBuilt[i].InnerText;
                            //break;
                        }
                    }
                }
                break;
            }

            //alternateAPN
            XmlNodeList alterAPN = docread.GetElementsByTagName("ParcelIdentifier");
            foreach (XmlNode alt in alterAPN)
            {
                alternateAPN = alt.InnerText;
                if (alternateAPN.Trim() == "")
                {
                    for (int i = 0; i < alterAPN.Count; i++)
                    {
                        if (alternateAPN == "")
                        {
                            alternateAPN = alterAPN[i].InnerText;
                            //break;
                        }
                    }
                }
                if (alternateAPN.Trim() != "")
                {
                    string stralternateAPN = "";
                    for (int i = 0; i < alterAPN.Count; i++)
                    {
                        stralternateAPN = alterAPN[i].InnerText;
                        if (alternateAPN != stralternateAPN)
                        {
                            alternateAPN = stralternateAPN;
                        }
                    }
                }
                break;
            }

            //for IN marion parcel id
            if (strstate == "IN" && strcounty == "Marion")
            {
                foreach (XmlNode parcel in parcels)
                {
                    parcelno = parcel.InnerText;
                }
            }

            //Effect Year Built...
            XmlNodeList EffectyearBuilt = docread.GetElementsByTagName("PropertyEffectiveBuiltYear");
            foreach (XmlNode eyrb in EffectyearBuilt)
            {
                yearBuiltValue = eyrb.InnerText;
                if (yearBuiltValue.Trim() == "")
                {
                    for (int i = 0; i < EffectyearBuilt.Count; i++)
                    {
                        if (yearBuiltValue == "")
                        {
                            yearBuiltValue = EffectyearBuilt[i].InnerText;
                            //break;
                        }
                    }
                }
                break;
            }

            //TRA
            XmlNodeList tRA = docread.GetElementsByTagName("PropertyTaxCountyRateAreaIdentifier");
            foreach (XmlNode tr in tRA)
            {
                tra = tr.InnerText;
                if (tra.Trim() == "")
                {
                    for (int i = 0; i < tRA.Count; i++)
                    {
                        if (tra == "")
                        {
                            tra = tRA[i].InnerText;
                            //break;
                        }
                    }
                }
                break;
            }




            //Francis
            XmlNodeList multi = docread.GetElementsByTagName("StatusDescription");

            XmlNodeList parcelid = docread.GetElementsByTagName("ParcelIdentifier");

            XmlNodeList multinames = docread.GetElementsByTagName("FullName");

            XmlNodeList xAddress = docread.GetElementsByTagName("AddressLineText");


            List<string> multiName = new List<string>();
            List<string> multiaddress = new List<string>();
            List<string> multiadd = new List<string>();
            for (int i = 0; i < multinames.Count; i++)
            {
                string owner_Name = multinames[i].InnerText;
                if (owner_Name != "")
                {
                    multiName.Add(owner_Name);
                }
            }
            for (int i = 0; i < xAddress.Count; i++)
            {
                string address = xAddress[i].InnerText;
                if (address != "")
                {
                    multiaddress.Add(address);
                }
            }

            //for kern/san luis obispo/Riverside/Napa/Marin   multi[0].InnerText == "MULTIPLE PROPERTIES FOUND" && strstate == "CA" && strcounty == "Kern"|| multi[0].InnerText == "MULTIPLE PROPERTIES FOUND" && strstate == "CA" && strcounty == "Napa"|| multi[0].InnerText == "MULTIPLE PROPERTIES FOUND" && strstate == "CA" && strcounty == "San Luis Obispo" || multi[0].InnerText == "MULTIPLE PROPERTIES FOUND" && strstate == "CA" && strcounty == "RiverSide" || multi[0].InnerText == "MULTIPLE PROPERTIES FOUND" && strstate == "CA" && strcounty == "Marin"
            if (multi[0].InnerText == "MULTIPLE PROPERTIES FOUND")
            {
                alternateAPN = "";
                InsertTitleFlexMultiParcel(strXmlread);
                if (nameTitle.Count > 0)
                {
                    for (int T = 0; T < nameTitle.Count; T++)
                    {
                        DataSet ds = new DataSet();
                        string multiquery = "insert into multiparcels (orderno, address, county, city, state, zipcode, parcelno,ownername)  values ('" + orderNumber + "','" + multiaddress[T] + "','" + countyname + "','" + cityname + "','" + statename + "',' ','" + parcelid[T].InnerText + "','" + multiName[T] + "')";

                        ds = tcon.ExecuteQuery(multiquery);
                        string TitleFlex = addrTitle[T] + "~" + nameTitle[T] + "~" + countyname + "~" + cityname + "~" + statename;
                        insert_date(orderNumber, parcelTitle[T], 262, TitleFlex, 1, DateTime.Now);

                       //// DataSet ds = new DataSet();
                       //  multiquery = "insert into multiparcels (orderno, address, county, city, state, zipcode, parcelno,ownername)  values ('" + orderNumber + "','" + multiaddress[T] + "','" + countyname + "','" + cityname + "','" + statename + "',' ','" + parcelid[T].InnerText + "','" + multiName[T] + "')";
                       // ds = newcon.ExecuteQuery(multiquery);
                    }
                    GlobalClass.TitleFlex_Search = "Yes";
                    HttpContext.Current.Session["TitleFlex_Search"] = "Yes";
                }

            }

            else
            {
                titleparcel = parcelno;
                HttpContext.Current.Session["titleparcel"] = parcelno;
                //Parcel empty
                if (alternateAPN != "" && tra != "")
                {
                    HttpContext.Current.Session["titleflex_alternateAPN"] = tra.Trim() + alternateAPN;
                }
                global_parcelNo = parcelno;
            }
            //else
            //{
            //    int k = 0;
            //    for (int i = 0; i < multi.Count; i++)
            //    {
            //        if (multi[i].InnerText.Length > 0)
            //        {
            //            string multiParcel = multi[i].InnerText;

            //            DataSet dsbind = new DataSet();
            //            if (multiaddress.Count == multiName.Count)
            //            {
            //                if (multiParcel == "MULTIPLE PROPERTIES FOUND")
            //                {
            //                    //for (int j = 0; j < parcelid.Count; j++)
            //                    //{
            //                    //    if (parcelid[j].InnerText.Length > 0)
            //                    //    {
            //                    //        string TitleFlex = multiaddress[k] + "~" + multiName[k] + "~" + countyname + "~" + cityname + "~" + statename;
            //                    //        insert_date(orderNumber, parcelid[j].InnerText, 262, TitleFlex, 1, DateTime.Now);
            //                    //        k++;
            //                    //        DataSet ds = new DataSet();
            //                    //        string multiquery = "insert into multiparcels (orderno, address, county, city, state, zipcode, parcelno,ownername)  values ('" + orderNumber + "','" + multiaddress[k] + "','" + countyname + "','" + cityname + "','" + statename + "',' ','" + parcelid[j].InnerText + "','" + multiName[k] + "')";
            //                    //        ds = newcon.ExecuteQuery(multiquery);
            //                    //        //k++;
            //                    //        //string bindquery = "select orderno,parcelno,ownername,address,county,state from multiparcels where orderno = '" + orderNumber + "' group by parcelno";
            //                    //        //dsbind = newcon.ExecuteQuery(bindquery);
            //                    //        GlobalClass.TitleFlex_Search = "Yes";
            //                    //        HttpContext.Current.Session["TitleFlex_Search"] = "Yes";
            //                    //    }
            //                    //}
            //                    //if (dsbind.Tables[0].Rows.Count != 0)
            //                    //{

            //                    //}
            //                }
            //                else
            //                {
            //                    if (multiName.Count != 0)
            //                    {
            //                        string TitleFlex = paddress.Replace("\r\n", " ") + "~" + ownername.Replace("\r\n", " ") + "~" + countyname + "~" + cityname + "~" + statename;
            //                        insert_date(orderNumber, parcelno, 262, TitleFlex, 1, DateTime.Now);

            //                        string TitleFlex_Details = ownername.Replace("\r\n", " ") + "~" + paddress.Replace("\r\n", " ") + "~" + legal.Replace("\r\n", " ") + "~" + yearBuiltValue;
            //                        TitleFlexAssess = TitleFlex_Details;
            //                        HttpContext.Current.Session["TitleFlexAssess"] = TitleFlex_Details;

            //                        //Parcel empty
            //                        if (alternateAPN != "" && tra != "")
            //                        {
            //                            HttpContext.Current.Session["titleflex_alternateAPN"] = tra.Trim() + alternateAPN;
            //                        }

            //                        //LegalDiscription~Property_Tax~Assessed_Year~Land_Value~Improve_Value~Total_Assessed~Exemption~Tax_Year~Total_Tax~Year_Built~Alternate_APN~TRA

            //                        //newcon.ExecuteQuery("delete from data_value_master where order_no = '" + orderNumber + "'");
            //                        //string multiquery = "insert into multiparcels (orderno, address, county, city, state, zipcode, parcelno,ownername)  values ('" + orderNumber + "','" + paddress.Replace("\r\n", " ") + "','" + strcounty + "','" + cityname + "','" + strstate + "',' ','" + parcelno + "','" + ownername.Replace("\r\n", " ") + "')";
            //                        //ds = newcon.ExecuteQuery(multiquery);
            //                        //OwnerName~Address~Legal_Discription~TRA
            //                    }
            //                    titleparcel = parcelno;
            //                    HttpContext.Current.Session["titleparcel"] = parcelno;
            //                    //Parcel empty
            //                    if (alternateAPN != "" && tra != "")
            //                    {
            //                        HttpContext.Current.Session["titleflex_alternateAPN"] = tra.Trim() + alternateAPN;
            //                    }
            //                    global_parcelNo = parcelno;
            //                }
            //            }
            //            else
            //            {
            //                multiadd = multiaddress.Distinct().ToList();
            //                if (multiadd.Count != 0)
            //                {
            //                    string TitleFlex = paddress.Replace("\r\n", " ") + "~" + ownername.Replace("\r\n", " ") + "~" + countyname + "~" + cityname + "~" + statename;
            //                    insert_date(orderNumber, parcelno, 262, TitleFlex, 1, DateTime.Now);

            //                }
            //            }
            //        }
            //    }
            //}

            return parcelno;
        }


        public DataSet Fetch_Output_Data(string orderNumber, string state, string county)
        {
            mParam = new MySqlParameter[3];
            mParam[0] = new MySqlParameter("?$orderno", orderNumber);
            mParam[0].MySqlDbType = MySqlDbType.VarChar;


            mParam[1] = new MySqlParameter("?$state", state);
            mParam[1].MySqlDbType = MySqlDbType.VarChar;


            mParam[2] = new MySqlParameter("?$county", county);
            mParam[2].MySqlDbType = MySqlDbType.VarChar;

            return tcon.Executedataset("Sp_output", true, mParam);
        }

        public void InsertAmrockTax(string OrderNo, string TaxId, string Instamount1, string Instamount2, string Instamount3, string Instamount4, string Instamountpaid1, string Instamountpaid2, string Instamountpaid3, string Instamountpaid4, string InstPaidDue1, string InstPaidDue2, string InstPaidDue3, string InstPaidDue4, string IsDelinquent)
        {
            mParam = new MySqlParameter[15];

            mParam[0] = new MySqlParameter("?$OrderNo", OrderNo);
            mParam[0].MySqlDbType = MySqlDbType.VarChar;
            mParam[0].IsNullable = false;

            mParam[1] = new MySqlParameter("?$TaxId", TaxId);
            mParam[1].MySqlDbType = MySqlDbType.VarChar;

            mParam[2] = new MySqlParameter("?$Instamount1", Instamount1);
            mParam[2].MySqlDbType = MySqlDbType.VarChar;

            mParam[3] = new MySqlParameter("?$Instamount2", Instamount2);
            mParam[3].MySqlDbType = MySqlDbType.VarChar;

            mParam[4] = new MySqlParameter("?$Instamount3", Instamount3);
            mParam[4].MySqlDbType = MySqlDbType.VarChar;

            mParam[5] = new MySqlParameter("?$Instamount4", Instamount4);
            mParam[5].MySqlDbType = MySqlDbType.VarChar;

            mParam[6] = new MySqlParameter("?$Instamountpaid1", Instamountpaid1);
            mParam[6].MySqlDbType = MySqlDbType.VarChar;

            mParam[7] = new MySqlParameter("?$Instamountpaid2", Instamountpaid2);
            mParam[7].MySqlDbType = MySqlDbType.VarChar;

            mParam[8] = new MySqlParameter("?$Instamountpaid3", Instamountpaid3);
            mParam[8].MySqlDbType = MySqlDbType.VarChar;

            mParam[9] = new MySqlParameter("?$Instamountpaid4", Instamountpaid4);
            mParam[9].MySqlDbType = MySqlDbType.VarChar;

            mParam[10] = new MySqlParameter("?$InstPaidDue1", InstPaidDue1);
            mParam[10].MySqlDbType = MySqlDbType.VarChar;

            mParam[11] = new MySqlParameter("?$InstPaidDue2", InstPaidDue2);
            mParam[11].MySqlDbType = MySqlDbType.VarChar;

            mParam[12] = new MySqlParameter("?$InstPaidDue3", InstPaidDue3);
            mParam[12].MySqlDbType = MySqlDbType.VarChar;

            mParam[13] = new MySqlParameter("?$InstPaidDue4", InstPaidDue4);
            mParam[13].MySqlDbType = MySqlDbType.VarChar;

            mParam[14] = new MySqlParameter("?$IsDelinquent", IsDelinquent);
            mParam[14].MySqlDbType = MySqlDbType.VarChar;

            newcon.ExecuteSPNonQuery("sp_insert_scrapdetails", true, mParam);

        }

        public DataSet Fetch_Output_Address_Data(string state, string county, string address)
        {
            mParam = new MySqlParameter[3];

            mParam[0] = new MySqlParameter("?$state", state);
            mParam[0].MySqlDbType = MySqlDbType.VarChar;

            mParam[1] = new MySqlParameter("?$county", county);
            mParam[1].MySqlDbType = MySqlDbType.VarChar;

            mParam[2] = new MySqlParameter("?$address", address);
            mParam[2].MySqlDbType = MySqlDbType.VarChar;

            return tcon.Executedataset("Sp_starsaddresssearch", true, mParam);
        }

        public DataSet Fetch_Output_Data1(string state, string county, string ownername)
        {
            mParam = new MySqlParameter[3];

            mParam[0] = new MySqlParameter("?$state", state);
            mParam[0].MySqlDbType = MySqlDbType.VarChar;

            mParam[1] = new MySqlParameter("?$county", county);
            mParam[1].MySqlDbType = MySqlDbType.VarChar;

            mParam[2] = new MySqlParameter("?$ownername", ownername);
            mParam[2].MySqlDbType = MySqlDbType.VarChar;

            return tcon.Executedataset("Sp_starsnamesearch", true, mParam);
        }

        public DataSet Fetch_Output_Data2(string state, string county, string parcelnumber)
        {
            mParam = new MySqlParameter[3];

            mParam[0] = new MySqlParameter("?$state", state);
            mParam[0].MySqlDbType = MySqlDbType.VarChar;

            mParam[1] = new MySqlParameter("?$county", county);
            mParam[1].MySqlDbType = MySqlDbType.VarChar;

            mParam[2] = new MySqlParameter("?$apn", parcelnumber);
            mParam[2].MySqlDbType = MySqlDbType.VarChar;

            return tcon.Executedataset("Sp_starsparcelsearch", true, mParam);
        }


        public void InsertTitleFlexMultiParcel(string path)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(path);
            XmlElement root = doc.DocumentElement;
            //Fetch the specific Nodes by Attribute value.
            XmlNodeList nodeList = root.GetElementsByTagName("STATUS");
            string fullNAME = "", fullAddress = "", parcelno = "";

            //Loop through the selected Nodes.
            foreach (XmlNode node in nodeList)
            {
                foreach (XmlNode node1 in node.ChildNodes)
                {
                    if (node1.Name == "EXTENSION")
                    {
                        foreach (XmlNode node2 in node1.ChildNodes)
                        {
                            if (node2.Name == "PROPERTIES")
                            {
                                foreach (XmlNode node3 in node2.ChildNodes)
                                {
                                    if (node3.Name == "PROPERTY")
                                    {
                                        foreach (XmlNode node4 in node3.ChildNodes)
                                        {
                                            //owner name & address..........
                                            if (node4.Name == "ADDRESS")
                                            {
                                                foreach (XmlNode node5 in node4.ChildNodes)
                                                {
                                                    if (node5.Name == "AddressLineText")
                                                    {
                                                        fullAddress = node5.InnerText;
                                                        addrTitle.Add(fullAddress);
                                                    }
                                                    if (node5.Name == "EXTENSION")
                                                    {
                                                        foreach (XmlNode node6 in node5.ChildNodes)
                                                        {

                                                            if (node6.Name == "PARTY")
                                                            {
                                                                foreach (XmlNode node8 in node6.ChildNodes)
                                                                {

                                                                    if (node8.Name == "INDIVIDUAL")
                                                                    {
                                                                        foreach (XmlNode node10 in node8.ChildNodes)
                                                                        {
                                                                            if (node10.Name == "NAME")
                                                                            {
                                                                                foreach (XmlNode node11 in node10.ChildNodes)
                                                                                {
                                                                                    if (node11.Name == "FullName")
                                                                                    {
                                                                                        fullNAME = node11.InnerText;
                                                                                        nameTitle.Add(fullNAME);
                                                                                    }
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            //parcel no......

                                            if (node4.Name == "LEGAL_DESCRIPTIONS")
                                            {
                                                foreach (XmlNode node5 in node4.ChildNodes)
                                                {
                                                    if (node5.Name == "LEGAL_DESCRIPTION")
                                                    {
                                                        foreach (XmlNode node6 in node5.ChildNodes)
                                                        {
                                                            if (node6.Name == "PARCEL_IDENTIFICATIONS")
                                                            {
                                                                foreach (XmlNode node7 in node6.ChildNodes)
                                                                {
                                                                    if (node7.Name == "PARCEL_IDENTIFICATION")
                                                                    {
                                                                        foreach (XmlNode node8 in node7.ChildNodes)
                                                                        {
                                                                            if (node8.Name == "ParcelIdentifier")
                                                                            {
                                                                                parcelno = node8.InnerText;
                                                                                parcelTitle.Add(parcelno);
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                    }
                }
            }
        }
        public void AutoDownloadFile(string orderno, string parcelno, string county, string state, string fileName)
        {

            string outputPath = ReturnPath(state, county);
            outputPath = outputPath + orderno + "\\" + parcelno + "\\";
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            //string pathUser = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            //string pathDownload = Path.Combine(pathUser, "Downloads\\");
            string pathDownload = ConfigurationManager.AppSettings["AutoPdf"];
            pathDownload = pathDownload + fileName;
            File.Copy(pathDownload, outputPath + fileName, true);
            if (File.Exists(pathDownload))
            {
                File.Delete(pathDownload);
            }
        }

        public void insert_TakenTime(string orderno, string State, string County, string StartTime, string AssessmentTime, string TaxTime, string CitytaxTime, string LastEndTime)
        {
            string AssessTakenTime = "", TaxTakentime = "", CityTaxtakentime = "", TotaltakenTime = "";
            DBconnection dbconn = new DBconnection();
            try
            {
                if (AssessmentTime != "" && AssessmentTime != null)
                {
                    var Assresult = TimeSpan.Parse(AssessmentTime) - TimeSpan.Parse(StartTime);
                    AssessTakenTime = Convert.ToString(Assresult);
                }
                else
                {
                    AssessTakenTime = "00:00:00";
                    AssessmentTime = "00:00:00";
                }
            }
            catch { }
            try
            {
                if (TaxTime != "" && TaxTime != null)
                {
                    var Taxresult = TimeSpan.Parse(TaxTime) - TimeSpan.Parse(AssessmentTime);
                    TaxTakentime = Convert.ToString(Taxresult);
                }
                else
                {
                    TaxTakentime = "00:00:00";
                    TaxTime = "00:00:00";
                }
            }
            catch { }
            try
            {
                if (CitytaxTime != "" && CitytaxTime != null)
                {
                    var Cityresult = TimeSpan.Parse(CitytaxTime) - TimeSpan.Parse(TaxTime);
                    CityTaxtakentime = Convert.ToString(Cityresult);
                }
                else
                {
                    CityTaxtakentime = "00:00:00";
                    CitytaxTime = "00:00:00";
                }
            }
            catch { }
            try
            {
                if (LastEndTime != "" && LastEndTime != null)
                {
                    var Totalresult = TimeSpan.Parse(LastEndTime) - TimeSpan.Parse(StartTime);
                    TotaltakenTime = Convert.ToString(Totalresult);
                }
                else
                {
                    TotaltakenTime = "00:00:00";
                    LastEndTime = "00:00:00";
                }
            }
            catch { }


            dbconn.ExecuteQuery("insert into ScrapingTakenTime(Orderno, State, County, StartTime, AssessTime, TaxTime, CityTaxTime, EndTime, AssessTakenTime, TaxtakenTime, CityTakentime, TotalTakentime) values ('" + orderno + "','" + State + "','" + County + "','" + StartTime + "','" + AssessmentTime + "','" + TaxTime + "','" + CitytaxTime + "','" + LastEndTime + "','" + AssessTakenTime + "','" + TaxTakentime + "','" + CityTaxtakentime + "','" + TotaltakenTime + "')");

        }

        public string getfiles(string filename)
        {
            string Path = ConfigurationManager.AppSettings["AutoPdf"];
            var files = new DirectoryInfo(Path).GetFiles("*.*");

            DateTime lastupdated = DateTime.MinValue;
            foreach (FileInfo file in files)
            {
                if (file.LastWriteTime > lastupdated)
                {
                    lastupdated = file.LastWriteTime;
                    filename = file.Name;
                }

            }
            return filename;
        }

        public string filePath(string orderno, string parcelno)
        {
            string outputPath = ReturnPath(sname, cname);
            outputPath = outputPath + orderno + "\\" + parcelno + "\\";
            return outputPath;
        }
        public DataTable readdatafromcloudAB(string Query)
        {
            DataTable dt = ReturnDtAPIAB(Query);
            DataTable dTable = new DataTable();
            if (dt.Rows.Count > 0)
            {
                string data_text_id = dt.Rows[0]["Data_Field_Text_Id"].ToString();
                string order_no = dt.Rows[0]["order_no"].ToString();
                DataTable dtfield = ReturnDtAPIAB("select Data_Fields_Text from data_field_master where id='" + data_text_id + "'");
                string columnName = "order_no" + "~" + "parcel_no" + "~" + dtfield.Rows[0]["Data_Fields_Text"].ToString();
                string[] columnArray = columnName.Split('~');

                foreach (string cName in columnArray)
                {
                    dTable.Columns.Add(cName);
                }
                DataRow dr = dTable.NewRow();
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dTable.Rows.Add();
                }

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string rowvals = order_no + "~" + dt.Rows[i]["parcel_no"].ToString() + "~" + dt.Rows[i]["Data_Field_value"].ToString();
                    string[] rowValue = rowvals.Split('~');
                    for (int k = 0; k < rowValue.Count(); k++)
                    {
                        dTable.Rows[i][k] = rowValue[k];
                    }
                }
            }

            return dTable;

        }
        public DataTable ReturnDtAPI(string qry)
        {
            string data = DtConvert.ReadDataFROMCloud(qry);
            IList<Testbind> UserList = JsonConvert.DeserializeObject<IList<Testbind>>(data);
            DataTable dt = DtConvert.ToDataTable(UserList);
            return dt;
        }
        public DataTable ReturnDtAPIAB(string qry)
        {
            string data = DtConvert.ReadDataFROMCloudAB(qry);
            IList<Testbind> UserList = JsonConvert.DeserializeObject<IList<Testbind>>(data);
            DataTable dt = DtConvert.ToDataTable(UserList);
            return dt;
        }

        public static void WriteLog(string strLog)
        {
            StreamWriter log;
            FileStream fileStream = null;
            DirectoryInfo logDirInfo = null;
            FileInfo logFileInfo;

            string logFilePath = "F:\\Error_Log\\";
            logFilePath = logFilePath + "Log-" + System.DateTime.Today.ToString("MM-dd-yyyy") + "." + "txt";
            logFileInfo = new FileInfo(logFilePath);
            logDirInfo = new DirectoryInfo(logFileInfo.DirectoryName);
            if (!logDirInfo.Exists) logDirInfo.Create();
            if (!logFileInfo.Exists)
            {
                fileStream = logFileInfo.Create();
            }
            else
            {
                fileStream = new FileStream(logFilePath, FileMode.Append);
            }
            log = new StreamWriter(fileStream);
            log.WriteLine(strLog);
            log.Close();
        }

        public void InsertSearchTax(string OrderNo, string Land, string Improvements, string ExemptionHomeowners, string ExemptionAdditional, string FirstInstallment, string FirstDueDate, string FirstTaxesOutDate, string FirstPaid, string FirstDue, string SecondInstallment, string SecondDueDate, string SecondTaxesOutDate, string SecondPaid, string SecondDue, string assyear, int TaxTypeID, int Year, string TaxingEntity, string TaxIDNumber, string TaxTypeName, string TaxIDNumberFurtherDescribed)
        {
            mParam = new MySqlParameter[22];

            mParam[0] = new MySqlParameter("?$OrderNo", OrderNo);
            mParam[0].MySqlDbType = MySqlDbType.VarChar;
            mParam[0].IsNullable = false;

            mParam[1] = new MySqlParameter("?$Land", Land);
            mParam[1].MySqlDbType = MySqlDbType.VarChar;

            mParam[2] = new MySqlParameter("?$Improvements", Improvements);
            mParam[2].MySqlDbType = MySqlDbType.VarChar;

            mParam[3] = new MySqlParameter("?$ExemptionHomeowners", ExemptionHomeowners);
            mParam[3].MySqlDbType = MySqlDbType.VarChar;

            mParam[4] = new MySqlParameter("?$ExemptionAdditional", ExemptionAdditional);
            mParam[4].MySqlDbType = MySqlDbType.VarChar;

            mParam[5] = new MySqlParameter("?$FirstInstallment", FirstInstallment);
            mParam[5].MySqlDbType = MySqlDbType.VarChar;

            mParam[6] = new MySqlParameter("?$FirstDueDate", FirstDueDate);
            mParam[6].MySqlDbType = MySqlDbType.VarChar;

            mParam[7] = new MySqlParameter("?$FirstTaxesOutDate", FirstTaxesOutDate);
            mParam[7].MySqlDbType = MySqlDbType.VarChar;

            mParam[8] = new MySqlParameter("?$FirstPaid", FirstPaid);
            mParam[8].MySqlDbType = MySqlDbType.VarChar;

            mParam[9] = new MySqlParameter("?$FirstDue", FirstDue);
            mParam[9].MySqlDbType = MySqlDbType.VarChar;

            mParam[10] = new MySqlParameter("?$SecondInstallment", SecondInstallment);
            mParam[10].MySqlDbType = MySqlDbType.VarChar;

            mParam[11] = new MySqlParameter("?$SecondDueDate", SecondDueDate);
            mParam[11].MySqlDbType = MySqlDbType.VarChar;

            mParam[12] = new MySqlParameter("?$SecondTaxesOutDate", SecondTaxesOutDate);
            mParam[12].MySqlDbType = MySqlDbType.VarChar;

            mParam[13] = new MySqlParameter("?$SecondPaid", SecondPaid);
            mParam[13].MySqlDbType = MySqlDbType.VarChar;

            mParam[14] = new MySqlParameter("?$SecondDue", SecondDue);
            mParam[14].MySqlDbType = MySqlDbType.VarChar;

            mParam[15] = new MySqlParameter("?$assyear", assyear);
            mParam[15].MySqlDbType = MySqlDbType.VarChar;

            mParam[16] = new MySqlParameter("?$TaxTypeID", TaxTypeID);
            mParam[16].MySqlDbType = MySqlDbType.Int32;

            mParam[17] = new MySqlParameter("?$Year", Year);
            mParam[17].MySqlDbType = MySqlDbType.Int32;

            mParam[18] = new MySqlParameter("?$TaxingEntity", TaxingEntity);
            mParam[18].MySqlDbType = MySqlDbType.VarChar;

            mParam[19] = new MySqlParameter("?$TaxIDNumber", TaxIDNumber);
            mParam[19].MySqlDbType = MySqlDbType.VarChar;

            mParam[20] = new MySqlParameter("?$TaxTypeName", TaxTypeName);
            mParam[20].MySqlDbType = MySqlDbType.VarChar;

            mParam[21] = new MySqlParameter("?$TaxIDNumberFurtherDescribed", TaxIDNumberFurtherDescribed);
            mParam[21].MySqlDbType = MySqlDbType.VarChar;

            newcon.ExecuteSPNonQuery("sp_InsertSearchTax", true, mParam);

        }
        public DataSet GetOrderCount_placer(string fdate, string tdate)
        {
            mParam = new MySqlParameter[2];
            mParam[0] = new MySqlParameter("?$fdate", fdate);
            mParam[0].MySqlDbType = MySqlDbType.VarChar;

            mParam[1] = new MySqlParameter("?$tdate", tdate);
            mParam[1].MySqlDbType = MySqlDbType.VarChar;
            return newcon.Executedataset("Sp_getordercount_placer", true, mParam);

        }
        public void insert_titleflex(string OrderNo, string Date, string Address, string County, string City, string State, string ZipCode, string FirstName, string MiddleName, string LastName, string ParcelId, string UserName)
        {
            mParam = new MySqlParameter[12];
            mParam[0] = new MySqlParameter("?$OrderNo", OrderNo);
            mParam[0].MySqlDbType = MySqlDbType.VarChar;
            mParam[0].IsNullable = false;

            mParam[1] = new MySqlParameter("?$Date", Date);
            mParam[1].MySqlDbType = MySqlDbType.VarChar;

            mParam[2] = new MySqlParameter("?$Address", Address);
            mParam[2].MySqlDbType = MySqlDbType.VarChar;

            mParam[3] = new MySqlParameter("?$County", County);
            mParam[3].MySqlDbType = MySqlDbType.VarChar;

            mParam[4] = new MySqlParameter("?$City", City);
            mParam[4].MySqlDbType = MySqlDbType.VarChar;

            mParam[5] = new MySqlParameter("?$State", State);
            mParam[5].MySqlDbType = MySqlDbType.VarChar;

            mParam[6] = new MySqlParameter("?$ZipCode", ZipCode);
            mParam[6].MySqlDbType = MySqlDbType.VarChar;

            mParam[7] = new MySqlParameter("?$FirstName", FirstName);
            mParam[7].MySqlDbType = MySqlDbType.VarChar;

            mParam[8] = new MySqlParameter("?$MiddleName", MiddleName);
            mParam[8].MySqlDbType = MySqlDbType.VarChar;

            mParam[9] = new MySqlParameter("?$LastName", LastName);
            mParam[9].MySqlDbType = MySqlDbType.VarChar;

            mParam[10] = new MySqlParameter("?$ParcelId", ParcelId);
            mParam[10].MySqlDbType = MySqlDbType.VarChar;

            mParam[11] = new MySqlParameter("?$UserName", UserName);
            mParam[11].MySqlDbType = MySqlDbType.VarChar;


            tcon.ExecuteSPNonQuery("sp_InsertTitleflex", true, mParam);

        }

    }
}