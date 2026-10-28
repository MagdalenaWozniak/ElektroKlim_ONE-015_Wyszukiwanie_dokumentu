using cdn_api;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static TYMnaFIN.Opis;
using static TYMnaFIN.Plat;
using static TYMnaFIN.TYM;
using static TYMnaFIN.VAT;

namespace TYMnaFIN
{
    public partial class TYM_Form : Form
    {
        string connection_string;
        FS dok;
        List<ListaTYM> TYMy;
        string date1 = "1800-12-28";

        private int XLWersjaAPI = 20202;
        private int XLTrybWsadowy = 1;
        private int XLWinieta = -1;
        private int XLUtworzWlasnaSesje = 1;
        private int sesja = -1;
        private string XLSQLServer = "10.33.21.10";
        private string XLSQLDatabase = "TOMMA_TEST";//"ERPXL_PoloMarket"
        private string XLUser = "Maciej"; //
        private string XLPass = "NE!w0rk12";//"Dupa2003!";

        public TYM_Form(string connectionString, FS fs, string SQLDatabase, string kntNumer)
        {
            InitializeComponent();

            connection_string = connectionString;
            dok = fs;
            XLSQLDatabase = SQLDatabase;
            string tymyQuery = "SELECT TrN_GIDTyp, TrN_GIDNumer, cdn.NumerDokumentu(TrN_GIDTyp, TrN_SpiTyp, TrN_TrNTyp, TrN_TrNNumer, TrN_TrNRok, TrN_TrNSeria, TrN_TrNMiesiac),  CAST(TrN_NettoP as varchar(10))+' '+TrN_Waluta, "
                                    + " CAST(DateAdd(day, TrN_Data2 - 4, '1801-01-01') as date) From cdn.TraNag join cdn.Atrybuty on TrN_GIDNumer = Atr_ObiNumer and TrN_GIDTyp = Atr_ObiTyp "
                                    + " and TrN_GIDFirma = Atr_ObiFirma join cdn.AtrybutyKlasy on Atr_AtkId = AtK_ID where AtK_Nazwa = 'Rezerwa rozliczona' and  Atr_Wartosc = 'NIE' and TrN_KntNumer = " + kntNumer;
            TYMy = SelectListRowColumn(tymyQuery, "cdn.TraNag");
            string[] rodzaje;
            rodzaje = new string[TYMy.Count];
            int x = 0;
            foreach (ListaTYM box in TYMy)
            {
                rodzaje[x] = box.Nazwa + "\t \t" + box.Data.ToShortDateString() + " \t" + box.Kwota;
                x++;
            }
            tym.Items.AddRange(rodzaje);
        }

        private void TYM_Form_Load(object sender, EventArgs e)
        {

        }

        private void button_Click(object sender, EventArgs e)
        {
            if (Convert.ToString(tym.SelectedItem) == "")
            {
                MessageBox.Show("Proszę wybrać rezerwę ", "Komunikat", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            }
            else
            {
                try
                {

                    XLLoginInfo_20202 nowyUzytkownik = new XLLoginInfo_20202();
                    nowyUzytkownik.Wersja = XLWersjaAPI;
                    nowyUzytkownik.Winieta = XLWinieta;
                    nowyUzytkownik.TrybWsadowy = XLTrybWsadowy;
                    nowyUzytkownik.UtworzWlasnaSesje = XLUtworzWlasnaSesje;
                    nowyUzytkownik.SerwerKlucza = XLSQLServer;
                    nowyUzytkownik.Baza = XLSQLDatabase;
                    nowyUzytkownik.OpeIdent = XLUser;
                    nowyUzytkownik.OpeHaslo = XLPass;
                    nowyUzytkownik.PortHTTPSerweraKlucza = 1;
                    nowyUzytkownik.RodzajSerweraKlucza = 0;
                    nowyUzytkownik.SerwerKlucza = "COMARCHBD::5000024502";
                    nowyUzytkownik.ProgramID = "ONE-025 TYM na FIN";

                    int blad = cdn_api.cdn_api.XLLogin(nowyUzytkownik, ref sesja);
                    if (blad != 0)
                    {
                        MessageBox.Show("Błąd logowania: " + blad, "Komunikat", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }


                    // MessageBox.Show("Data2: " + date2);
                    // MessageBox.Show("Rok: " + (dataTable.Rows[i][5]).ToString().Substring(0, 4));
                    // MessageBox.Show("Miesiąc: " + dataTable.Rows[i][5].ToString().Substring(5, 2));




                    string pozQuery = "SELECT * FROM cdn.TraVat where TrV_GIDNumer=" + TYMy[tym.SelectedIndex].GidNumer + " and TrV_GIDTyp=" + TYMy[tym.SelectedIndex].GidTyp;
                    List<ListaVat> Vaty = SelectListVat(pozQuery, "cdn.TraVat");
                    

                    pozQuery = "SELECT * FROM cdn.TraPlat where TrP_GIDNumer=" + TYMy[tym.SelectedIndex].GidNumer + " and TrP_GIDTyp=" + TYMy[tym.SelectedIndex].GidTyp;
                    List<ListaPlat> Platy = SelectListPlat(pozQuery, "cdn.TraPlat");

                    pozQuery = "SELECT [OWE_GIDLp] ,[OWE_Procent], OWS_TypWymiaru, OWS_WMRID, OWS_Element FROM [CDN].[OpisWymElem] join [CDN].[OpisWymSElem] on OWE_GIDNumer = OWS_GIDNumer and OWE_GIDTyp = OWS_GIDTyp "
                                + " and OWE_GIDLp = OWS_GIDLp WHERE OWE_GIDNumer = " + TYMy[tym.SelectedIndex].GidNumer + "and OWE_GIDTyp = " + TYMy[tym.SelectedIndex].GidTyp + " ORDER BY OWE_GIDLp";
                    List<ListaOpis> Linie = SelectListOpis(pozQuery, "cdn.OpisWymElem");
                    

                    string date2 = Convert.ToString(DateTime.Now.ToString("yyyy-MM-dd"));
                    string query5 = "SELECT DATEDIFF(day, '" + date1 + "','" + date2 + "'); ";

                    XLDokumentNagInfo_20202 nowyDokument = new XLDokumentNagInfo_20202();
                    nowyDokument.Wersja = XLWersjaAPI;
                    nowyDokument.GIDFirma = 0;
                    nowyDokument.GIDLp = 0;
                    nowyDokument.GIDNumer = 0;
                    nowyDokument.GIDTyp = 0;
                    nowyDokument.KntFirma = Convert.ToInt32(Platy[0].KntFirma);
                    nowyDokument.KntLp = Convert.ToInt32(Platy[0].KntLp);
                    nowyDokument.KntNumer = Convert.ToInt32(Platy[0].KntNumer);
                    nowyDokument.KntTyp = Convert.ToInt32(Platy[0].KntTyp);
                    nowyDokument.Typ = 1; //FZ
                    nowyDokument.Avista = 1;
                    nowyDokument.Tryb = 2;
                    nowyDokument.Korekta = 1;
                    nowyDokument.Rok = DateTime.Now.Year;
                    nowyDokument.Miesiac = DateTime.Now.Month;
                    nowyDokument.Seria = "TYM";
                    nowyDokument.Rejestr = "ZAK";
                    nowyDokument.DokumentObcy = TYMy[tym.SelectedIndex].Nazwa;
                    //nowyDokument.IDOddzial = Convert.ToInt32(dataTable.Rows[i][2]);
                    nowyDokument.DataSpr = Convert.ToInt32(ConnectToSQL(connection_string, query5));
                    nowyDokument.Data = Convert.ToInt32(ConnectToSQL(connection_string, query5));
                    nowyDokument.DataMag = Convert.ToInt32(ConnectToSQL(connection_string, query5));
                    nowyDokument.DataVat = Convert.ToInt32(ConnectToSQL(connection_string, query5));
                    int dokumentId = 0;
                    blad = cdn_api.cdn_api.XLNowyDokument(sesja, ref dokumentId, nowyDokument);
                    if (blad != 0)
                    {

                        MessageBox.Show("Błąd XLNowuDokument w wierszu : " + blad, "Komunikat", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                    }

                    foreach (ListaVat vat_poz in Vaty)
                    {
                        XLVatInfo_20202 vat = new XLVatInfo_20202();

                        vat.Wersja = XLWersjaAPI;
                        vat.StawkaPod = Convert.ToInt32(Convert.ToDouble(vat_poz.StawkaPod) * 100);
                        vat.Wartosc = "-"+vat_poz.NettoP;
                        vat.WartoscVat = "-"+vat_poz.VatP;
                        vat.DeklRok = Convert.ToInt32(vat_poz.DeklRok);
                        vat.DeklMiesiac = Convert.ToInt32(vat_poz.DeklMiesiac);
                        vat.IgnorujVat = 1;
                        vat.OdliczeniaVat = 2;
                        blad = cdn_api.cdn_api.XLDodajVAT(dokumentId, vat);
                        if (blad != 0)
                        {

                            MessageBox.Show("Błąd vatu : " + blad, "Komunikat", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                        }
                    }



                    

                    XLZamkniecieDokumentuInfo_20202 zamkniecie = new XLZamkniecieDokumentuInfo_20202();
                    zamkniecie.Wersja = XLWersjaAPI;
                    zamkniecie.Tryb = 0;
                    blad = cdn_api.cdn_api.XLZamknijDokument(dokumentId, zamkniecie);
                    if (blad != 0)
                    {

                        MessageBox.Show("Błąd zamknięcie dokumentu: " + blad, "Komunikat", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                    }

                    int opisID = 0;
                    blad = dodajOpis(ref opisID, nowyDokument );
                    if (blad != 0)
                    {
                        MessageBox.Show("Błąd Dodawania opisu w wierszu :" + blad, "Komunikat", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                    }
                    int Lp = 1;

                    for (int i = 0; i < Linie.Count; i++)
                    {
                       


                        blad = dodajLinie(Linie, ref i, opisID, Lp);
                        if (blad != 0)
                        {
                            MessageBox.Show("Błąd Dodawania linii : " + blad, "Komunikat", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                        }
                        Lp++;


                       
                    }
                    XLZamknijOpisInfo_20202 zamOpis = new XLZamknijOpisInfo_20202();
                    zamOpis.Wersja = XLWersjaAPI;
                    cdn_api.cdn_api.XLZamknijOpis(opisID, zamOpis);

                    XLAtrybutInfo_20202 atr = new XLAtrybutInfo_20202();
                    atr.Wersja = XLWersjaAPI;
                    atr.GIDFirma = nowyDokument.GIDFirma;
                    atr.GIDLp = nowyDokument.GIDLp;
                    atr.GIDNumer = nowyDokument.GIDNumer;
                    atr.GIDSubLp = 0;
                    atr.GIDTyp = nowyDokument.GIDTyp;
                    atr.Klasa = "Dokument FIN";
                    atr.Wartosc =ConnectToSQL(connection_string, "SELECT cdn.NumerDokumentu(TrN_GIDTyp, TrN_SpiTyp, TrN_TrNTyp, TrN_TrNNumer, TrN_TrNRok, TrN_TrNSeria, TrN_TrNMiesiac) FROM cdn.TraNag where TrN_GIDTyp=" 
                                + dok.GIDTyp + " and TrN_GIDNumer=" + dok.GIDNumer); 
                    blad = cdn_api.cdn_api.XLDodajAtrybut(sesja, atr);
                    if (blad != 0)
                    {
                        MessageBox.Show("Błąd atrybutu : " + blad, "Komunikat", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                    }

                    cdn_api.cdn_api.XLLogout(sesja);

                    string updateQuery = "update k set Atr_Wartosc='TAK' FROM cdn.Atrybuty as k join cdn.AtrybutyKlasy on Atr_AtkId = AtK_ID where AtK_Nazwa = 'Rezerwa rozliczona' and Atr_ObiTyp =" + TYMy[tym.SelectedIndex].GidTyp
                                        + " AND Atr_ObiNumer = " + TYMy[tym.SelectedIndex].GidNumer;
                    Update(connection_string, updateQuery); 

                    
                    string numerDokQuery = "SELECT cdn.NumerDokumentu(TrN_GIDTyp, TrN_SpiTyp, TrN_TrNTyp, TrN_TrNNumer, TrN_TrNRok, TrN_TrNSeria, TrN_TrNMiesiac) FROM cdn.TraNag where TrN_GIDTyp=" + nowyDokument.GIDTyp
                                            + " and TrN_GIDNumer=" + nowyDokument.GIDNumer;

                    MessageBox.Show("Zakończono rozwiązywanie rezerwy.\nDodano dokument "+ConnectToSQL(connection_string, numerDokQuery), "Komunikat", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Application.Exit();
                }
                catch(Exception en)
                {
                    MessageBox.Show("Błąd dodawania korekty: " + en.Message, "Komunikat", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }


            }
        }




        int dodajOpis(ref int opisID, XLDokumentNagInfo_20202 dok)
        {
            XLNowyOpisInfo_20202 opis = new XLNowyOpisInfo_20202();
            opis.GIDFirma = dok.GIDFirma;
            opis.GIDLp = dok.GIDLp;
            opis.GIDNumer = dok.GIDNumer;
            opis.GIDTyp = dok.GIDTyp;
            opis.Wersja = XLWersjaAPI;
            opis.Tryb = 2;
            return cdn_api.cdn_api.XLNowyOpis(sesja, ref opisID, opis);


        }

        int dodajLinie(List<ListaOpis> Linie, ref int i, int opisID, int lp)
        {
            XLDodajLinieOpisuInfo_20202 linia = new XLDodajLinieOpisuInfo_20202();
            linia.Wersja = XLWersjaAPI;
            linia.Tryb = 2;
            linia.Lp = 0;
            linia.Procent = Linie[i].Procent;
            int blad = cdn_api.cdn_api.XLDodajLinieOpisu(opisID, linia);

            int k = i;
            do
            {
                i = k;
                XLDodajWymiarOpisuInfo_20202 kat = new XLDodajWymiarOpisuInfo_20202();
                kat.Wersja = XLWersjaAPI;
                kat.Tryb = 2;
                kat.Typ = Linie[i].Typ;
                kat.Wymiar = Linie[i].WymiarID;
                if (Linie[i].WymiarID == 23)
                    kat.Element = Linie[i].Element;
                int bladWym = cdn_api.cdn_api.XLDodajWymiarOpisu(opisID, kat);
                if (bladWym != 0)
                {
                    MessageBox.Show("Błąd Dodawania wymiaru " + Linie[i].WymiarID + ": " + bladWym, "Komunikat", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                    k = i + 1;
            }
            while (i + 1 < Linie.Count && Linie[k].Lp == lp);




            XLZamknijLinieOpisuInfo_20202 zamLin = new XLZamknijLinieOpisuInfo_20202();
            zamLin.Wersja = XLWersjaAPI;
            cdn_api.cdn_api.XLZamknijLinieOpisu(opisID, zamLin);

            return blad;

        }

        private List<ListaTYM> SelectListRowColumn(string query, string srcTableName)
        {
            List<string> str = new List<string>();
            List<ListaTYM> myList = new List<ListaTYM>();
            string sttr = "";
            try
            {
                using (SqlConnection connection = new SqlConnection(connection_string))
                {
                    SqlDataAdapter adapter = new SqlDataAdapter();
                    adapter.SelectCommand = new SqlCommand(query, connection);
                    DataSet dataSet = new DataSet();
                    adapter.Fill(dataSet, srcTableName);
                    foreach (DataRow row in (InternalDataCollectionBase)dataSet.Tables[0].Rows)
                    {
                        foreach (DataColumn column in (InternalDataCollectionBase)dataSet.Tables[0].Columns)
                        {
                            str.Add(row[column].ToString());

                            //MessageBox.Show(column + ": " + row[column].ToString());
                        }
                        myList.Add(new ListaTYM() { GidTyp = Convert.ToInt32(str[0]), GidNumer = Convert.ToInt32(str[1]), Nazwa = str[2], Kwota = str[3], Data = Convert.ToDateTime(str[4]) });
                        str.Clear();

                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(sttr = sttr + "Bład SQL:" + ex.ToString() + "\n");
            }
            return myList;
        }


        private string InsertInto(string sql)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connection_string))
                {
                    SqlDataAdapter adapter = new SqlDataAdapter();
                    connection.Open();
                    adapter.InsertCommand = new SqlCommand(sql, connection);
                    adapter.InsertCommand.ExecuteNonQuery();
                    //MessageBox.Show("Row inserted !! ");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Komunikat", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return sql;
        }

        public string ConnectToSQL(string connection_string, string query)
        {
            string str = "";
            try
            {
                using (SqlConnection connection = new SqlConnection(connection_string))
                {
                    SqlDataAdapter adapter = new SqlDataAdapter();
                    adapter.SelectCommand = new SqlCommand(query, connection);
                    DataSet dataSet = new DataSet();
                    adapter.Fill(dataSet, "Query");
                    if (dataSet.Tables.Count == 0)
                    {
                        return "";
                    }
                    foreach (DataRow row in (InternalDataCollectionBase)dataSet.Tables[0].Rows)
                    {
                        str = row[0].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                str = str + "Bład SQL:" + ex.ToString() + "\n";
            }
            return str;
        }


        private List<ListaVat> SelectListVat(string query, string srcTableName)
        {
            List<string> str = new List<string>();
            List<ListaVat> myList = new List<ListaVat>();
            string sttr = "";
            try
            {
                using (SqlConnection connection = new SqlConnection(connection_string))
                {
                    SqlDataAdapter adapter = new SqlDataAdapter();
                    adapter.SelectCommand = new SqlCommand(query, connection);
                    DataSet dataSet = new DataSet();
                    adapter.Fill(dataSet, srcTableName);
                    foreach (DataRow row in (InternalDataCollectionBase)dataSet.Tables[0].Rows)
                    {
                        foreach (DataColumn column in (InternalDataCollectionBase)dataSet.Tables[0].Columns)
                        {
                            str.Add(row[column].ToString());

                            //MessageBox.Show(column + ": " + row[column].ToString());
                        }
                        myList.Add(new ListaVat() {GidTyp=str[0],GidFirma=str[1] ,GidNumer=str[2], GidLp=str[3], StawkaPod =str[4] , FlagaVat=str[5], Zrodlowa=str[6] , NettoR = str[7] , NettoP=str[8] , VatR=str[9] ,
                                                   VatP = str[10], ExpoNorm=str[11], DeklRok=str[12] , DeklMiesiac=str[13] , RodzajZakupu=str[14] , OdliczeniaVat=str[15] , Rozliczac=str[16], WartoscWal=str[17] ,
                                                   Zaliczkowy =str[18] , Struktura=str[19] , RozliczacP = str[20], SrtTyp=str[21] , SrtFirma=str[22] , SrtNumer=str[23] , SrtLp=str[24] , ULRID=str[25] , 
                                                   RKNID =str[26] , RKStrona=str[27] ,OgraniczenieOdl=str[28] , Opis=str[29] , SHENumer=str[30], SHELp=str[31], KatDatyOP=str[32], DataOP=str[33], DataPO=str[34] ,
                                                   VATNalOd=str[35], KntNumer=str[36], KnANumer=str[37], KorektaDanych=str[38], RozliczacVIU=str[38] });
                        str.Clear();

                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(sttr = sttr + "Bład SQL:" + ex.ToString() + "\n");
            }
            return myList;
        }

        private List<ListaOpis> SelectListOpis(string query, string srcTableName)
        {
            List<string> str = new List<string>();
            List<ListaOpis> myList = new List<ListaOpis>();
            string sttr = "";
            try
            {
                using (SqlConnection connection = new SqlConnection(connection_string))
                {
                    SqlDataAdapter adapter = new SqlDataAdapter();
                    adapter.SelectCommand = new SqlCommand(query, connection);
                    DataSet dataSet = new DataSet();
                    adapter.Fill(dataSet, srcTableName);
                    foreach (DataRow row in (InternalDataCollectionBase)dataSet.Tables[0].Rows)
                    {
                        foreach (DataColumn column in (InternalDataCollectionBase)dataSet.Tables[0].Columns)
                        {
                            str.Add(row[column].ToString());

                            //MessageBox.Show(column + ": " + row[column].ToString());
                        }
                        myList.Add(new ListaOpis()
                        {
                            Lp=Convert.ToInt32(str[0]), Procent=(str[1]), Typ=Convert.ToInt32(str[2]), WymiarID=Convert.ToInt32(str[3]), Element=str[4]
                            
                        });
                        str.Clear();

                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(sttr = sttr + "Bład SQL:" + ex.ToString() + "\n");
            }
            return myList;
        }


        public static string Update(string connection_string, string sql)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connection_string))
                {
                    SqlDataAdapter adapter = new SqlDataAdapter();
                    connection.Open();
                    adapter.UpdateCommand = new SqlCommand(sql, connection);
                    adapter.UpdateCommand.ExecuteNonQuery();
                    //MessageBox.Show("Row updated !! ");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Komunikat", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return sql;
        }


        private List<ListaPlat> SelectListPlat(string query, string srcTableName)
        {
            List<string> str = new List<string>();
            List<ListaPlat> myList = new List<ListaPlat>();
            string sttr = "";
            try
            {
                using (SqlConnection connection = new SqlConnection(connection_string))
                {
                    SqlDataAdapter adapter = new SqlDataAdapter();
                    adapter.SelectCommand = new SqlCommand(query, connection);
                    DataSet dataSet = new DataSet();
                    adapter.Fill(dataSet, srcTableName);
                    foreach (DataRow row in (InternalDataCollectionBase)dataSet.Tables[0].Rows)
                    {
                        foreach (DataColumn column in (InternalDataCollectionBase)dataSet.Tables[0].Columns)
                        {
                            str.Add(row[column].ToString());

                            //MessageBox.Show(column + ": " + row[column].ToString());
                        }
                        myList.Add(new ListaPlat()
                        {
                            GIDTyp = str[0],
                            GIDFirma = str[1],
                            GIDNumer = str[2],
                            GIDLp = str[3],
                            KntTyp = str[4],
                            KntFirma = str[5],
                            KntNumer = str[6],
                            KntLp = str[7],
                            AkwTyp = str[8],
                            AkwFirma = str[9],
                            AkwNumer = str[10],
                            AkwLp = str[11],
                            Waluta=str[12],
                            NrKursu=str[13],
                            KursM=str[14],
                            KursL=str[15],
                            Typ=str[16],
                            Termin=str[17],
                            FormaNr=str[18],
                            FormaNazwa=str[19],
                            Kwota=str[20],
                            Pozostaje=str[21],
                            Rozliczona=str[22],
                            Status=str[23],
                            OpeWTyp=str[24],
                            OpeWFirma=str[25],
                            OpeWNumer=str[26],
                            OpeWLp=str[27],
                            Notatki=str[28],
                            SpiTyp=str[29],
                            SpiFirma=str[30],
                            SpiNumer=str[31],
                            SpiLp=str[32],
                            KARTyp=str[33],
                            KARFirma=str[34],
                            KARNumer=str[35],
                            KARLp=str[36],
                            NrKonta=str[37],
                            SpodziewanyTermin=str[38],
                            SEPA=str[39],
                            ExpId=str[40],
                            ExpLin=str[41],
                            KwotaSys=str[42],
                            PozostajeSys=str[43],
                            KnANumer=str[44],
                            CzasModyfikacji=str[45],
                            Rodzaj=str[46],
                            ZewnetrznyId=str[47],
                            CDCMsgId=str[48],
                            CDCOrgInstrId=str[49],
                            CDCExpTimestamp=str[50],
                            OpeCDCTyp=str[51],
                            OpeCDCFirma=str[52],
                            OpeCDCNumer=str[53],
                            OpeCDCLp=str[54],
                            CDCStatusKod=str[55],
                            CDCStatusOpis=str[56],
                            CDCStatusTimestamp=str[57],
                            MaksymalnyTermin=str[58],
                            SplitPayment=str[59],
                            SPKwotaVAT=str[60],
                            SPNip=str[61],
                            SPDokument=str[62],
                            DataWysylki=str[63],
                            JestLimitCelowy=str[64],
                            TerminPoProlongacie=str[65],
                            ProlZrdTyp=str[66],
                            ProlZrdFirma=str[67],
                            ProlZrdNumer=str[68],
                            ProlZrdLp=str[69],
                            PrleLP=str[70],
                            RachBank=str[71],
                            ZrdTyp=str[72],
                            ZrdFirma=str[73],
                            ZrdNumer=str[74],
                            ZrdLp=str[75],
                            KorektaDanych=str[76],
                            BbonId=str[77],
                            EndToEndId=str[78]
                        });
                        str.Clear();

                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(sttr = sttr + "Bład SQL:" + ex.ToString() + "\n");
            }
            return myList;
        }
    }
}
