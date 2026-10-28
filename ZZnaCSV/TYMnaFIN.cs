using Hydra;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System;
using System.Data.SqlClient;
using System.Data;
using cdn_api;
using TYMnaFIN;

[assembly: CallbackAssemblyDescription(Name: "ONE-025 TYM na FIN",
                                           Description: "Rozwiązywanie rezerw",
                                           Author: "OneSupport",
                                           Version: "1.0",
                                           SystemVersion: "2021.0.2",
                                           Created: "16-05-2023")]


namespace TextBank
{
    [SubscribeProcedure(Procedures.ListaRejestrVAT, "Rozwiązywanie rezerw")]
    public class ZSnaFS_Class : Callback
    {
        private int XLWersjaAPI = 20202;
        private int XLTrybWsadowy = 1;
        private int XLWinieta = -1;
        private int XLUtworzWlasnaSesje = 1;
        private int sesja = -1;
        private string XLSQLServer = "10.33.21.10";
        private string XLSQLDatabase = "TOMMA_TEST";//"ERPXL_PoloMarket"
        private string XLUser = "Maciej"; //
        private string XLPass = "NE!w0rk12";//"Dupa2003!";
        private ClaWindow RapText;
        private ClaWindow RapButton;
        private ClaWindow zakladkaTabWewnetrzne;
        private ClaWindow filtrBtn;
        private ClaWindow listaPoz;
        private ClaWindow akronim;
        FS fs;
        int listID;
        private bool bSpacja = false;
        long iNumer;
        long iTyp;
        long iFirma;
        long iLp;
        long iKntNumer;
        string kntNumer;
        public override void Init()
        {

            AddSubscription(true, 0, Events.OpenWindow, new TakeEventDelegate(OnOpenWindow));
            AddSubscription(false, 0, Events.ResizeWindow, new TakeEventDelegate(Resize));
        }



        private bool OnOpenWindow(Procedures ProcId, int ControlId, Events Event)
        {

            ClaWindow Parent = GetWindow();
            Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            Rectangle WndBnd = GetWindow().Bounds;
            zakladkaTabWewnetrzne = GetWindow().AllChildren["?Tab_WgNumeru"];
            listaPoz = GetWindow().AllChildren["?BrowseQueue"];
            listID = listaPoz.Id;
            RapButton = zakladkaTabWewnetrzne.Children.Add(ControlTypes.button);
            RapButton.Visible = true;
            filtrBtn = zakladkaTabWewnetrzne.AllChildren["?BtnOCR"];   //pobiera współrzędne przycisku zapisz
            RapButton.Bounds = new Rectangle(filtrBtn.Bounds.X-45, filtrBtn.Bounds.Y, 22, 15);
            RapButton.TextRaw = "ROZ";
            //fs = new FS((int)TraNag.TrN_GIDNumer, (int)TraNag.TrN_GIDFirma, (short)TraNag.TrN_GIDLp, (int)TraNag.TrN_GIDTyp, (int)TraNag.TrN_KntNumer);
            RapButton.Enabled = true;//rezerwa(fs);

            //akronim = Parent.AllChildren["?cKop_Knt:Akronim"];

            //AddSubscription(true, akronim.Id, Events.Accepted, new TakeEventDelegate(OnSave));
            //AddSubscription(true, listaPoz.Id, Events.NewSelection, new TakeEventDelegate(OnSave));


            //RapText.TextRaw = pobierzTresc(fs);


            
            //RapText.OnAfterOpenWindow += new TakeEventDelegate(zaczytaj_OnAfterAccepted);
            RapButton.OnAfterMouseDown += new TakeEventDelegate(BtnOnMouseDown);
            //RapButton.OnBeforeAccepted += new TakeEventDelegate(zaczytaj_OnBeforeAccepted);


            //RapText.ReadOnlyRaw = "1";
            //RapText.BoxedRaw = "0";
            //RapText.NoFrameRaw = "1";
            //RapText.FontNameRaw = "Tahoma";



            return true;
        }

        public bool Resize(Procedures ProcId, int ControlId, Events Event)
        {
            filtrBtn = zakladkaTabWewnetrzne.AllChildren["?BtnOCR"];                            //pobiera współrzędne przycisku zapisz
            RapButton.Bounds = new Rectangle(filtrBtn.Bounds.X-45, filtrBtn.Bounds.Y, 22, 15);
            //fs = new FS((int)TraNag.TrN_GIDNumer, (int)TraNag.TrN_GIDFirma, (short)TraNag.TrN_GIDLp, (int)TraNag.TrN_GIDTyp, (int)TraNag.TrN_KntNumer);
            //RapButton.Enabled = rezerwa(fs);
            // RapButton.Bounds = new Rectangle(filtrBtn.Bounds.X , filtrBtn.Bounds.Y-20, 17, 15);
            return true;
        }

        bool zaczytaj_OnBeforeAccepted(Procedures ProcedureId, int ControlId, Events Event)
        {
            bSpacja = true;
            Runtime.WindowController.PostEvent(listaPoz.Id, Events.NewSelection);
            iNumer = TraNag.TrN_GIDNumer;
            iFirma = TraNag.TrN_GIDFirma;
            iLp = TraNag.TrN_GIDLp;
            iTyp = TraNag.TrN_GIDTyp;
            iKntNumer = TraNag.TrN_KntNumer;

            MessageBox.Show("fs.numer: " + iNumer + "\nfs.typ: " + iTyp + "\nfs.firma: " + iFirma + "\nfs.KntNumer: " + iKntNumer);
            //fs = new FS((int)TraNag.TrN_GIDNumer, (int)TraNag.TrN_GIDFirma, (short)TraNag.TrN_GIDLp, (int)TraNag.TrN_GIDTyp, (int)TraNag.TrN_KntNumer);

            return true;
        }

        public bool OnSave(Procedures ProcId, int ControlId, Events Event)
        {
            
            //fs = new FS((int)TraNag.TrN_GIDNumer, (int)TraNag.TrN_GIDFirma, (short)TraNag.TrN_GIDLp, (int)TraNag.TrN_GIDTyp, (int)TraNag.TrN_KntNumer);
            //RapButton.Enabled = rezerwa(fs);
            // RapButton.Bounds = new Rectangle(filtrBtn.Bounds.X , filtrBtn.Bounds.Y-20, 17, 15);
            return true;
        }

        
        public bool rezerwa(FS fs)
        {
            try
            {

                //MessageBox.Show(Runtime.ConfigurationDictionary.Baza.ToString());
                string connectionString = "";
                if (Runtime.ConfigurationDictionary.Baza.ToString().Substring(0, 16) == "ERPXL_Tomma_test")
                {
                    connectionString = @"Data Source = COMARCHBD; Initial Catalog = ERPXL_Tomma_test; User ID = sa; Password=fr$g4i9sSa032V,_";
                    XLSQLDatabase = "TOMMA_TEST";
                }
                else if (Runtime.ConfigurationDictionary.Baza.ToString().Substring(0, 15) == "ERPXL_TOMMA-ERP")
                {
                    connectionString = @"Data Source = COMARCHBD; Initial Catalog = ERPXL_TOMMA-ERP; User ID = sa; Password=fr$g4i9sSa032V,_";
                    XLSQLDatabase = "TOMMA-ERP";
                }
                else if (Runtime.ConfigurationDictionary.Baza.ToString().Substring(0, 18) == "ERPXL_BONUS_2_test")
                {
                    connectionString = @"Data Source = COMARCHBD; Initial Catalog = ERPXL_BONUS_2_test; User ID = sa; Password=fr$g4i9sSa032V,_";
                    XLSQLDatabase = "BONUS 2-TEST";
                }
                else if (Runtime.ConfigurationDictionary.Baza.ToString().Substring(0, 13) == "ERPXL_BONUS_2")
                {
                    connectionString = @"Data Source = COMARCHBD; Initial Catalog = ERPXL_BONUS_2; User ID = sa; Password=fr$g4i9sSa032V,_";
                    XLSQLDatabase = "BONUS 2";
                }




                    string kntNumerQuery = "SELECT TrN_KntNumer from cdn.TraNag where TrN_GIDNumer=" + fs.GIDNumer + " and TrN_GIDTyp=" + fs.GIDTyp;
                    kntNumer = ConnectToSQL(connectionString, kntNumerQuery);
                    string czyRezerwaQuery = "if exists(SELECT * From cdn.TraNag join cdn.Atrybuty on TrN_GIDNumer = Atr_ObiNumer and TrN_GIDTyp = Atr_ObiTyp and TrN_GIDFirma = Atr_ObiFirma "
                                            + " join cdn.AtrybutyKlasy on Atr_AtkId = AtK_ID where AtK_Nazwa = 'Rezerwa rozliczona' "
                                        + " and Atr_Wartosc = 'NIE' and TrN_KntNumer = " + kntNumer + ") SELECT 'TAK' else SELECT 'NIE' ";


                    string czyRezerwa = ConnectToSQL(connectionString, czyRezerwaQuery);

                    //MessageBox.Show(kntNumer);
                    if (czyRezerwa == "TAK")
                    {
                        string czyBylQuery = "if exists (SELECT * FROM cdn.Atrybuty WHERE ATR_Wartosc=(SELECT cdn.NumerDokumentu(TrN_GIDTyp, TrN_SpiTyp, TrN_TrNTyp, TrN_TrNNumer, TrN_TrNRok, TrN_TrNSeria, TrN_TrNMiesiac) "
                                            + " FROM cdn.TraNag where TrN_GIDTyp=" + fs.GIDTyp + " and TrN_GIDNumer = " + fs.GIDNumer + ") ) SELECT 'TAK' else SELECT 'NIE'";
                        string czyByl = ConnectToSQL(connectionString, czyBylQuery);
                        if(czyByl=="NIE")
                            return true;
                        else
                        {
                            MessageBox.Show("Dla tego dokumentu już stworzono korektę dla faktury TYM", "Komunikat", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return false;
                        }

                    }
                    else
                    {
                        MessageBox.Show("Dla tego kontrahenta nie ma dostępnych rezerw", "Komunikat", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return false;
                    }
               
            }

            catch (Exception e)
            {
                 MessageBox.Show("Błąd widoczności przycisku 'TYM': " + e.ToString(), "Komunikat", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
        }
        

        public bool BtnOnMouseDown(Procedures ProcId, int ControlId, Events Event)
        {

            try
            {
                //FS fs = new FS((int)iNumer, (int)iFirma, (short)iLp, (int)iTyp, (int)iKntNumer);
                //MessageBox.Show("fs.numer: " + fs.GIDNumer + "\nfs.typ: " + fs.GIDTyp + "\nfs.firma: " + fs.GIDFirma + "\nfs.KntNumer: " + fs.KntNumer);
                ADODB._Recordset recordset = Runtime.WindowController.GetQueueMarked((int)ProcId, listID, GetCallbackThread());

                if (!ZbieranieFS(recordset))
                {
                    if (rezerwa(fs))
                    {
                        string connectionString = "";
                        if (Runtime.ConfigurationDictionary.Baza.ToString().Substring(0, 16) == "ERPXL_Tomma_test")
                        {
                            connectionString = @"Data Source = COMARCHBD; Initial Catalog = ERPXL_Tomma_test; User ID = sa; Password=fr$g4i9sSa032V,_";
                            XLSQLDatabase = "TOMMA_TEST";
                        }
                        else if (Runtime.ConfigurationDictionary.Baza.ToString().Substring(0, 15) == "ERPXL_TOMMA-ERP")
                        {
                            connectionString = @"Data Source = COMARCHBD; Initial Catalog = ERPXL_TOMMA-ERP; User ID = sa; Password=fr$g4i9sSa032V,_";
                            XLSQLDatabase = "TOMMA";
                        }
                        else if (Runtime.ConfigurationDictionary.Baza.ToString().Substring(0, 18) == "ERPXL_BONUS_2_test")
                        {
                            connectionString = @"Data Source = COMARCHBD; Initial Catalog = ERPXL_BONUS_2_test; User ID = sa; Password=fr$g4i9sSa032V,_";
                            XLSQLDatabase = "BONUS 2-TEST";
                        }



                        Application.Run(new TYM_Form(connectionString, fs, XLSQLDatabase, kntNumer));
                    }
                    

                }

                return true;





            }
            catch (Exception e)
            {
                MessageBox.Show("Błąd: " + e.ToString(), "Komunikat", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }



            //return true;
        }


        DateTime zmianaDaty(int clarionDate, string connection_string)
        {
            string nowaData="select dateadd(s, "+clarionDate+", '19900101')";
            return Convert.ToDateTime(ConnectToSQL(connection_string, nowaData));
        }

        bool ZbieranieFS(ADODB._Recordset recordset)
        {

            try
            {
                StringBuilder dokumenty = new StringBuilder();
                if (recordset != null)
                {
                    int count = recordset.RecordCount;
                    if (count > 1)
                    {
                        MessageBox.Show("Zaznaczono więcej niż jeden dokument. Operacja anulowana.", "Komunikat", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        return true;
                    }

                    if (count != 0)
                    {
                        recordset.MoveFirst();

                        while (recordset.EOF == false)
                        {
                            ADODB.Fields fields = recordset.Fields;

                            //MessageBox.Show("Typ :"+Convert.ToString(fields[0].Value), "komunikat", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            //MessageBox.Show("Firma:" + Convert.ToString(fields[1].Value), "komunikat", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            //MessageBox.Show("Numer :" + Convert.ToString(fields[2].Value), "komunikat", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            //MessageBox.Show("Lp :" + Convert.ToString(fields[3].Value), "komunikat", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            string[] strlist = Convert.ToString(fields[0].Value).Split('#');



                            fs=(new FS(Convert.ToInt32(strlist[2]), Convert.ToInt32(strlist[1]),0, Convert.ToInt32(strlist[0])));

                            if (strlist[0] != "1521")
                            {
                                MessageBox.Show("Nie wszystkie zaznaczone dokumenty to FZ. Operacja anulowana.", "Komunikat", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                return true;
                            }

                            recordset.MoveNext();
                        }
                    }
                    else
                    {
                        MessageBox.Show("Nie zaznaczono dokumentów. Operacja anulowana.", "Komunikat", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        return true;
                    }
                }
                else
                {
                    MessageBox.Show("Nie zaznaczono dokumentów. Operacja anulowana.", "Komunikat", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Błąd pobierania zaznaczonych dokumentów --> " + ex.Message, "Komunikat", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return true;
            }
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

        public string Update(string sql, string connection_string)
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

        public static FS SelectListRowColumn(string query, string srcTableName, string connection_string)
        {
            List<string> str = new List<string>();
            FS faktura= new FS();
            //List<TwrCeny> myList = new List<TwrCeny>();
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

                            MessageBox.Show(column + ": " + row[column].ToString());
                        }
                        //faktura = new FZ(Convert.ToInt32(str[2]), Convert.ToInt32(str[1]), Convert.ToInt16(str[2]), Convert.ToInt32(str[0]), Convert.ToInt32(str[0]));
                        str.Clear();

                    }
                }

            }
            catch (Exception ex)
            {
                 MessageBox.Show(sttr = sttr + "Bład SQL:" + ex.ToString() + "\n");
            }
            return faktura;
        }

        public string InsertInto(string sql, string connection_string)
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

       



        public override void Cleanup()
        {

        }

    }
}
