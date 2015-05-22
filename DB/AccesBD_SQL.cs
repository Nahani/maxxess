/**
 * 	Fichier : AccessBD_SQL.cs 
 * 
 * 	Version : 1.0.0 
 * 		- Definition des échanges de base avec la base de données pour tout les types d'éntités.
 * 		- Récupération des valeurs d'attributs.
 * 
 * 	Auteurs : Alexandre BOURSIER & Nolan POTIER
 * 	
 * 	Résumé : Implémentation de l'interface AccessBD pour une base de données SQL. 
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;

namespace DB
{
    public class AccesBD_SQL : AccesBD
    {

        private static AccesBD_SQL instance;

        static String info = "Server=.\\SQLEXPRESS;Database=Maxxess;Integrated Security=true;";
        //static String info = "Server=SERVER_MAXXESS\\SQLEXPRESS;Database=A_V_L_V_;User Id=sa;Password=cegid.2005;";

        private AccesBD_SQL() { }

        public static AccesBD_SQL Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new AccesBD_SQL();
                }
                return instance;
            }
        }

        public int deleteOutdatedChequeFidelites()
        {
            int result = 0;

            using (SqlConnection connection = new SqlConnection(info))
            {
                connection.Open();

                //Obtenir les factures
                //
                var queryString = "DELETE FROM CHEQUE_FIDELITE WHERE DATE_FIN_VAL < GETDATE()";
                using (SqlCommand command = new SqlCommand(queryString, connection))
                {

                    //Command
                    result = command.ExecuteNonQuery();
                    
                }
            }
            return result;
        }

        public Client getClientById(String id)
        {
            
            Client targeted_user = null;
            using (SqlConnection connection = new SqlConnection(info))
            {
                connection.Open();

                //Obtenir les factures
                //
                var queryString = "SELECT * FROM TIERS WHERE T_AUXILIAIRE='" + id + "'";
                using (SqlCommand command = new SqlCommand(queryString, connection))
                {

                    //Command
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            targeted_user = new Client(reader.GetString(0), reader.GetString(2), reader.GetString(1), reader.GetString(3), reader.GetString(4), reader.GetString(6), reader.GetString(7), reader.GetString(86));
                        }
                    }
                }
            }
            return targeted_user;
        }

        public List<Client> getClientByName(String nom)
        {
            String req = null;
            List<Client> targeted_user = new List<Client>();
             
            req = "SELECT * FROM TIERS WHERE T_LIBELLE LIKE '%" + nom + "%'";
            
            SqlDataReader reader = Connexion.execute_Select(req);
            while (reader.Read())
            {
                targeted_user.Add(new Client(reader.GetString(0), reader.GetString(2), reader.GetString(1), reader.GetString(3), reader.GetString(4), reader.GetString(6), reader.GetString(7), reader.GetString(86)));
            }
            Connexion.close();
            return targeted_user;
        }

        public List<Client> getAllClients()
        {
            List<Client> resultat = new List<Client>();
            using (SqlConnection connection = new SqlConnection(info))
            {
                connection.Open();

                //Obtenir les clients
                //
                var queryString = "SELECT * FROM TIERS;";
                using (SqlCommand command = new SqlCommand(queryString, connection))
                {

                    //Command
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            resultat.Add(new Client(reader.GetString(0), reader.GetString(2), reader.GetString(1), reader.GetString(3), reader.GetString(4), reader.GetString(6), reader.GetString(7), reader.GetString(86)));
                        }
                    }
                }
            }
            return resultat;
        }

        public List<Facture> getFactureByName(String nom)
        {
            if (nom.Equals(""))
            {
                return getAllFactures(DateTime.Now);
            }
            else
            {
           
                List<Facture> result = new List<Facture>();
                List<Client> clients = getClientByName(nom);
                foreach(Client client in clients ) 
                {
                    String req = "SELECT * FROM ECRITURE WHERE E_JOURNAL = 'VEN' and E_AUXILIAIRE = '" + client.ID + "' and E_NUMLIGNE=1 AND E_LIBELLE LIKE '%FAC%';";
                    SqlDataReader reader = Connexion.execute_Select(req);
                

                    while (reader.Read())
                    {
                        req = "SELECT L_DATECREATION FROM LIGNES WHERE L_TYPEPIECE = 'FAC' and L_NUMEROPIECE =" + Convert.ToInt32(reader.GetString(7)) + ";";
                        SqlDataReader reader2 = Connexion.execute_Select(req);
                        DateTime date;
                        if (reader2.Read())
                        {
                            /*req = "SELECT SUM(E_DEBIT) FROM ECRITURE WHERE E_REFERENCE ='" + reader.GetString(7) + "' and E_LIBELLE NOT LIKE '%Remise%' and E_LIBELLE LIKE '%FAC%';";
                            SqlDataReader reader3 = Connexion.execute_Select(req);
                            Double total = 0.0;
                            if (reader3.Read())
                            {
                                total = Convert.ToDouble((Decimal)reader3.GetSqlDecimal(0));
                            }*/
                            date = reader2.GetDateTime(0);
                            Facture f = new Facture(Convert.ToInt32(reader.GetString(7)), reader.GetString(6), Convert.ToDouble((Decimal)reader.GetSqlDecimal(8)), date, reader.GetString(37), TypePiece.Facture, client, getRemiseFacture(Convert.ToInt32(reader.GetString(7))));
                            f.ChequeAssocieGenere = chequeFideliteAssocieExists(f);
                            if (f.ChequeAssocieGenere)
                            {
                                f.ChequeAssocieBloque = chequeFideliteAssocieIsBloque(f);
                                f.ChequeAssocie = getChequeFideliteByFacture(f);
                                if (chequeFideliteAssocieIsUsed(f))
                                {
                                    f.IsUsed = true;
                                    f.ChequeAssocieBloque = true;
                                }
                            }
                        
                            f.Avoir = false;
                            result.Add(f);
                        
                        }
                
                }

                //Obtenir les avoirs
                //
                req = "SELECT * FROM ECRITURE WHERE E_JOURNAL = 'VEN' and E_AUXILIAIRE = '" + client.ID + "' and E_NUMLIGNE=1 AND E_LIBELLE LIKE '%AVC%';";
                reader = Connexion.execute_Select(req);
                while (reader.Read())
                {
                    req = "SELECT L_DATECREATION FROM LIGNES WHERE L_TYPEPIECE = 'AVC' and L_NUMEROPIECE =" + Convert.ToInt32(reader.GetString(7)) + ";";
                    SqlDataReader reader2 = Connexion.execute_Select(req);
                    DateTime date;
                    if (reader2.Read())
                    {
                        date = reader2.GetDateTime(0);
                        Facture f = new Facture(Convert.ToInt32(reader.GetString(7)), reader.GetString(6), Convert.ToDouble((Decimal)reader.GetSqlDecimal(9)),
                        date, reader.GetString(37), TypePiece.Avoir, getClientById(reader.GetString(5)), 0.0);
                        f.ChequeAssocieGenere = false;                       
                        f.Avoir = true;
                        result.Add(f);
                    }
                }

                //Obtenir les tickets
                req = "SELECT DISTINCT PI_NUMEROPIECE, PI_DATEPIECE, PI_TOTALTTC, PI_AUXILIAIRE, PI_LIBELLETIERS, RD_MODEREGLE FROM PIECES P, REGLEDETAIL R WHERE PI_TYPEPIECE = 'VTC' and P.PI_NUMEROPIECE = R.RD_NUMEROPIECE and PI_AUXILIAIRE = '" + client.ID + "';";
                reader = Connexion.execute_Select(req);


                while (reader.Read())
                {
                    req = "SELECT L_DATECREATION FROM LIGNES WHERE L_TYPEPIECE = 'VTC' and L_NUMEROPIECE =" + reader.GetInt32(0) + ";";
                    SqlDataReader reader2 = Connexion.execute_Select(req);
                    DateTime date;
                    if (reader2.Read())
                    {
                        date = reader2.GetDateTime(0);
                        Facture f = new Facture(reader.GetInt32(0), reader.GetString(4), Convert.ToDouble((Decimal)reader.GetSqlDecimal(2)), date, reader.GetString(5), TypePiece.Ticket, getClientById(reader.GetString(3)), getRemiseTicket(reader.GetInt32(0)));
                        f.ChequeAssocieGenere = chequeFideliteAssocieExists(f);

                        if (f.ChequeAssocieGenere)
                        {
                            f.ChequeAssocieBloque = chequeFideliteAssocieIsBloque(f);
                            f.ChequeAssocie = getChequeFideliteByFacture(f);
                            if (chequeFideliteAssocieIsUsed(f))
                            {
                                f.IsUsed = true;
                                f.ChequeAssocieBloque = true;
                            }
                        }
                        f.Avoir = false;
                        result.Add(f);
                    }

                }

                Connexion.close();
            }
                
            return result;
            }

        }

        public List<Facture> getFactureByDate(DateTime start, DateTime end)
        {
            String req = "SELECT * FROM ECRITURE WHERE E_JOURNAL = 'VEN' and E_DATECOMPTABLE between '" + start.ToShortDateString() + "' and '" + end.ToShortDateString() + "' and E_NUMLIGNE=1 AND E_LIBELLE LIKE '%FAC%';";
            SqlDataReader reader = Connexion.execute_Select(req);

            List<Facture> result = new List<Facture>();
            while (reader.Read())
            {
                req = "SELECT L_DATECREATION FROM LIGNES WHERE L_TYPEPIECE = 'FAC' and L_NUMEROPIECE =" + Convert.ToInt32(reader.GetString(7)) + ";";
                SqlDataReader reader2 = Connexion.execute_Select(req);
                DateTime date;
                if (reader2.Read())
                {
                    /*req = "SELECT SUM(E_DEBIT) FROM ECRITURE WHERE E_REFERENCE ='" + reader.GetString(7) + "' and E_LIBELLE NOT LIKE '%Remise%' and E_LIBELLE LIKE '%FAC%';";
                    SqlDataReader reader3 = Connexion.execute_Select(req);
                    Double total = 0.0;
                    if (reader3.Read())
                    {
                        total = Convert.ToDouble((Decimal)reader3.GetSqlDecimal(0));
                    }*/
                    date = reader2.GetDateTime(0);
                    Facture f = new Facture(Convert.ToInt32(reader.GetString(7)), reader.GetString(6), Convert.ToDouble((Decimal)reader.GetSqlDecimal(8)), date, reader.GetString(37), TypePiece.Facture, getClientById(reader.GetString(5)), getRemiseFacture(Convert.ToInt32(reader.GetString(7))));
                    f.ChequeAssocieGenere = chequeFideliteAssocieExists(f);
                    if (f.ChequeAssocieGenere)
                    {
                        f.ChequeAssocieBloque = chequeFideliteAssocieIsBloque(f);
                        f.ChequeAssocie = getChequeFideliteByFacture(f);
                        if (chequeFideliteAssocieIsUsed(f))
                        {
                            f.IsUsed = true;
                            f.ChequeAssocieBloque = true;
                        }
                    }
                    
                    f.Avoir = false;
                    result.Add(f);

                }

            }

            //Obtenir les avoirs
            //
            req = "SELECT * FROM ECRITURE WHERE E_JOURNAL = 'VEN' and E_DATECOMPTABLE between '" + start.ToShortDateString() + "' and '" + end.ToShortDateString() + "' and E_NUMLIGNE=1 AND E_LIBELLE LIKE '%AVC%';";
            reader = Connexion.execute_Select(req);
            while (reader.Read())
            {
                req = "SELECT L_DATECREATION FROM LIGNES WHERE L_TYPEPIECE = 'AVC' and L_NUMEROPIECE =" + Convert.ToInt32(reader.GetString(7)) + ";";
                SqlDataReader reader2 = Connexion.execute_Select(req);
                DateTime date;
                if (reader2.Read())
                {
                    date = reader2.GetDateTime(0);
                    Facture f = new Facture(Convert.ToInt32(reader.GetString(7)), reader.GetString(6), Convert.ToDouble((Decimal)reader.GetSqlDecimal(9)),
                    date, reader.GetString(37), TypePiece.Avoir, getClientById(reader.GetString(5)), 0.0);
                    f.ChequeAssocieGenere = false;
                    f.Avoir = true;
                    result.Add(f);
                }
            }


            //Obtenir les tickets
            req = "SELECT DISTINCT PI_NUMEROPIECE, PI_DATEPIECE, PI_TOTALTTC, PI_AUXILIAIRE, PI_LIBELLETIERS, RD_MODEREGLE FROM PIECES P, REGLEDETAIL R WHERE PI_TYPEPIECE = 'VTC' and P.PI_NUMEROPIECE = R.RD_NUMEROPIECE and PI_DATEPIECE between '" + start.ToShortDateString() + "' and '" + end.ToShortDateString() + "';";
            reader = Connexion.execute_Select(req);


            while (reader.Read())
            {
                req = "SELECT L_DATECREATION FROM LIGNES WHERE L_TYPEPIECE = 'VTC' and L_NUMEROPIECE =" + reader.GetInt32(0) + ";";
                SqlDataReader reader2 = Connexion.execute_Select(req);
                DateTime date;
                if (reader2.Read())
                {
                    date = reader2.GetDateTime(0);
                    Facture f = new Facture(reader.GetInt32(0), reader.GetString(4), Convert.ToDouble((Decimal)reader.GetSqlDecimal(2)), date, reader.GetString(5), TypePiece.Ticket, getClientById(reader.GetString(3)), getRemiseTicket(reader.GetInt32(0)));
                    f.ChequeAssocieGenere = chequeFideliteAssocieExists(f);
                    if (f.ChequeAssocieGenere)
                    {
                        f.ChequeAssocieBloque = chequeFideliteAssocieIsBloque(f);
                        f.ChequeAssocie = getChequeFideliteByFacture(f);
                        if (chequeFideliteAssocieIsUsed(f))
                        {
                            f.IsUsed = true;
                            f.ChequeAssocieBloque = true;
                        }
                    }
                    f.Avoir = false;
                    result.Add(f);
                }

            }

            Connexion.close();
            return result;

        }

        public List<Facture> getFactureByDateAndClient(String nom, DateTime start, DateTime end)
        {
            List<Facture> result = new List<Facture>();
            List<Client> clients = getClientByName(nom);
            foreach (Client client in clients)
            {
                String req = "SELECT * FROM ECRITURE WHERE E_JOURNAL = 'VEN' and E_AUXILIAIRE = '" + client.ID + "' and E_DATECOMPTABLE between '" + start.ToShortDateString() + "' and '" + end.ToShortDateString() + "' and E_NUMLIGNE=1 AND E_LIBELLE LIKE '%FAC%';";
                SqlDataReader reader = Connexion.execute_Select(req);


                while (reader.Read())
                {
                    req = "SELECT L_DATECREATION FROM LIGNES WHERE L_TYPEPIECE = 'FAC' and L_NUMEROPIECE =" + Convert.ToInt32(reader.GetString(7)) + ";";
                    SqlDataReader reader2 = Connexion.execute_Select(req);
                    DateTime date;
                    if (reader2.Read())
                    {
                       /* req = "SELECT SUM(E_DEBIT) FROM ECRITURE WHERE E_REFERENCE ='" + reader.GetString(7) + "' and E_LIBELLE NOT LIKE '%Remise%' and E_LIBELLE LIKE '%FAC%';";
                        SqlDataReader reader3 = Connexion.execute_Select(req);
                        Double total = 0.0;
                        if (reader3.Read())
                        {
                            total = Convert.ToDouble((Decimal)reader3.GetSqlDecimal(0));
                        }*/
                        date = reader2.GetDateTime(0);
                        Facture f = new Facture(Convert.ToInt32(reader.GetString(7)), reader.GetString(6), Convert.ToDouble((Decimal)reader.GetSqlDecimal(8)), date, reader.GetString(37), TypePiece.Facture, getClientById(reader.GetString(5)), getRemiseFacture(Convert.ToInt32(reader.GetString(7))));
                        f.ChequeAssocieGenere = chequeFideliteAssocieExists(f);
                        if (f.ChequeAssocieGenere)
                        {
                            f.ChequeAssocieBloque = chequeFideliteAssocieIsBloque(f);
                            f.ChequeAssocie = getChequeFideliteByFacture(f);
                            if (chequeFideliteAssocieIsUsed(f))
                            {
                                f.IsUsed = true;
                                f.ChequeAssocieBloque = true;
                            }
                        }
                        
                        f.Avoir = false;
                        result.Add(f);
                    }

                }

                //Obtenir les avoirs
                //
                req = "SELECT * FROM ECRITURE WHERE E_JOURNAL = 'VEN' and E_AUXILIAIRE = '" + client.ID + "' and E_DATECOMPTABLE between '" + start.ToShortDateString() + "' and '" + end.ToShortDateString() + "' and E_NUMLIGNE=1 AND E_LIBELLE LIKE '%AVC%';";
                reader = Connexion.execute_Select(req);
                while (reader.Read())
                {
                    req = "SELECT L_DATECREATION FROM LIGNES WHERE L_TYPEPIECE = 'AVC' and L_NUMEROPIECE =" + Convert.ToInt32(reader.GetString(7)) + ";";
                    SqlDataReader reader2 = Connexion.execute_Select(req);
                    DateTime date;
                    if (reader2.Read())
                    {
                        date = reader2.GetDateTime(0);
                        Facture f = new Facture(Convert.ToInt32(reader.GetString(7)), reader.GetString(6), Convert.ToDouble((Decimal)reader.GetSqlDecimal(8)),
                        date, reader.GetString(37), TypePiece.Avoir, getClientById(reader.GetString(5)), 0.0);
                        f.ChequeAssocieGenere = false;
                        f.Avoir = true;
                        result.Add(f);
                    }
                }


                //Obtenir les tickets
                req = "SELECT DISTINCT PI_NUMEROPIECE, PI_DATEPIECE, PI_TOTALTTC, PI_AUXILIAIRE, PI_LIBELLETIERS, RD_MODEREGLE FROM PIECES P, REGLEDETAIL R WHERE PI_TYPEPIECE = 'VTC' and P.PI_NUMEROPIECE = R.RD_NUMEROPIECE and PI_DATEPIECE between '" + start.ToShortDateString() + "' and '" + end.ToShortDateString() + "' and PI_AUXILIAIRE = '" + client.ID + "';";
                reader = Connexion.execute_Select(req);


                while (reader.Read())
                {
                    req = "SELECT L_DATECREATION FROM LIGNES WHERE L_TYPEPIECE = 'VTC' and L_NUMEROPIECE =" + reader.GetInt32(0) + ";";
                    SqlDataReader reader2 = Connexion.execute_Select(req);
                    DateTime date;
                    if (reader2.Read())
                    {
                        date = reader2.GetDateTime(0);
                        Facture f = new Facture(reader.GetInt32(0), reader.GetString(4), Convert.ToDouble((Decimal)reader.GetSqlDecimal(2)), date, reader.GetString(5), TypePiece.Ticket, getClientById(reader.GetString(3)), getRemiseTicket(reader.GetInt32(0)));
                        f.ChequeAssocieGenere = chequeFideliteAssocieExists(f);
                        if (f.ChequeAssocieGenere)
                        {
                            f.ChequeAssocieBloque = chequeFideliteAssocieIsBloque(f);
                            f.ChequeAssocie = getChequeFideliteByFacture(f);
                            if (chequeFideliteAssocieIsUsed(f))
                            {
                                f.IsUsed = true;
                                f.ChequeAssocieBloque = true;
                            }
                        }
                        f.Avoir = false;
                        result.Add(f);
                    }

                }
            }

            Connexion.close();
            return result;

        }

        public Double getRemiseFacture(int idFacture)
        {
            Double res=0;
            String req = "SELECT * FROM LIGNES WHERE L_NUMEROPIECE =" + idFacture + " and L_TYPEPIECE = 'FAC' and L_TAUXREMISE=5;";
            SqlDataReader reader = Connexion.execute_Select(req);


            while (reader.Read())
            {
                
                    res += (Convert.ToDouble((Decimal)reader.GetSqlDecimal(6))*0.95);
                
            }
            Connexion.close();
            return res;
        }

        public Double getRemiseTicket(int idFacture)
        {
            Double res = 0;
            using (SqlConnection connection = new SqlConnection(info))
            {
                connection.Open();

                //Obtenir les factures
                //
                var queryString = "SELECT * FROM LIGNES WHERE L_NUMEROPIECE =" + idFacture + " and L_TYPEPIECE = 'VTC' and L_TAUXREMISE=5;";
                using (SqlCommand command = new SqlCommand(queryString, connection))
                {

                    //Command
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            res += (Convert.ToDouble((Decimal)reader.GetSqlDecimal(6)) * 0.95);
                        }
                    }
                }
            }         
            return res;
        }

        public List<Facture> getFactureByNumClient(String id)
        {
            List<Facture> result = new List<Facture>();

            String req = "SELECT * FROM ECRITURE WHERE E_JOURNAL = 'VEN' and E_AUXILIAIRE = '" + id + "' and E_NUMLIGNE=1 AND E_LIBELLE LIKE '%FAC%';";
            SqlDataReader reader = Connexion.execute_Select(req);

            while (reader.Read())
            {
                req = "SELECT L_DATECREATION FROM LIGNES WHERE L_TYPEPIECE = 'FAC' and L_NUMEROPIECE =" + Convert.ToInt32(reader.GetString(7)) + ";";
                SqlDataReader reader2 = Connexion.execute_Select(req);
                DateTime date;
                if (reader2.Read())
                {
                   /* req = "SELECT SUM(E_DEBIT) FROM ECRITURE WHERE E_REFERENCE ='" + reader.GetString(7) + "' and E_LIBELLE NOT LIKE '%Remise%' and E_LIBELLE LIKE '%FAC%';";
                    SqlDataReader reader3 = Connexion.execute_Select(req);
                    Double total = 0.0;
                    if (reader3.Read())
                    {
                        total = Convert.ToDouble((Decimal)reader3.GetSqlDecimal(0));
                    }*/
                    date = reader2.GetDateTime(0);
                    Facture f = new Facture(Convert.ToInt32(reader.GetString(7)), reader.GetString(6), Convert.ToDouble((Decimal)reader.GetSqlDecimal(8)), date, reader.GetString(37), TypePiece.Facture, getClientById(id), getRemiseFacture(Convert.ToInt32(reader.GetString(7))));
                    f.ChequeAssocieGenere = chequeFideliteAssocieExists(f);
                    if (f.ChequeAssocieGenere)
                    {
                        f.ChequeAssocieBloque = chequeFideliteAssocieIsBloque(f);
                        f.ChequeAssocie = getChequeFideliteByFacture(f);
                        if (chequeFideliteAssocieIsUsed(f))
                        {
                            f.IsUsed = true;
                            f.ChequeAssocieBloque = true;
                        }
                    }                    
                    f.Avoir = false;
                    result.Add(f);
                }
                
            }

            //Obtenir les avoirs
            //
            req = "SELECT * FROM ECRITURE WHERE E_JOURNAL = 'VEN' and E_AUXILIAIRE = '" + id + "' and E_NUMLIGNE=1 AND E_LIBELLE LIKE '%AVC%';";
            reader = Connexion.execute_Select(req);
            while (reader.Read())
            {
                req = "SELECT L_DATECREATION FROM LIGNES WHERE L_TYPEPIECE = 'AVC' and L_NUMEROPIECE =" + Convert.ToInt32(reader.GetString(7)) + ";";
                SqlDataReader reader2 = Connexion.execute_Select(req);
                DateTime date;
                if (reader2.Read())
                {
                    date = reader2.GetDateTime(0);
                    Facture f = new Facture(Convert.ToInt32(reader.GetString(7)), reader.GetString(6), Convert.ToDouble((Decimal)reader.GetSqlDecimal(9)),
                    date, reader.GetString(37), TypePiece.Avoir, getClientById(reader.GetString(5)), 0.0);
                    f.ChequeAssocieGenere = false;
                    f.Avoir = true;
                    result.Add(f);
                }
            }

            //Obtenir les tickets
            req = "SELECT DISTINCT PI_NUMEROPIECE, PI_DATEPIECE, PI_TOTALTTC, PI_AUXILIAIRE, PI_LIBELLETIERS, RD_MODEREGLE FROM PIECES P, REGLEDETAIL R WHERE PI_TYPEPIECE = 'VTC' and P.PI_NUMEROPIECE = R.RD_NUMEROPIECE and PI_AUXILIAIRE = '" + id + "';";
            reader = Connexion.execute_Select(req);


            while (reader.Read())
            {
                req = "SELECT L_DATECREATION FROM LIGNES WHERE L_TYPEPIECE = 'VTC' and L_NUMEROPIECE =" + reader.GetInt32(0) + ";";
                SqlDataReader reader2 = Connexion.execute_Select(req);
                DateTime date;
                if (reader2.Read())
                {
                    date = reader2.GetDateTime(0);
                    Facture f = new Facture(reader.GetInt32(0), reader.GetString(4), Convert.ToDouble((Decimal)reader.GetSqlDecimal(2)), date, reader.GetString(5), TypePiece.Ticket, getClientById(reader.GetString(3)), getRemiseTicket(reader.GetInt32(0)));
                    f.ChequeAssocieGenere = chequeFideliteAssocieExists(f);
                    if (f.ChequeAssocieGenere)
                    {
                        f.ChequeAssocieBloque = chequeFideliteAssocieIsBloque(f);
                        f.ChequeAssocie = getChequeFideliteByFacture(f);
                        if (chequeFideliteAssocieIsUsed(f))
                        {
                            f.IsUsed = true;
                            f.ChequeAssocieBloque = true;
                        }
                    }
                    f.Avoir = false;
                    result.Add(f);
                }

            }


            Connexion.close();
            
            return result;

        }

        public List<Facture> getFacturesOfDay()
        {
            DateTime start = DateTime.Now;
            DateTime end = DateTime.Now.AddDays(1);
            List<Facture> factures = new List<Facture>();



             using (SqlConnection connection = new SqlConnection(info))
            {
                connection.Open();

                //Obtenir les factures
                //
                var queryString = "SELECT E.E_REFERENCE, E.E_LIBELLE, E.E_MODEP, L.L_DATECREATION, T.T_AUXILIAIRE, T.T_NATUREAUXI, T.T_LIBELLE, T.T_ADRESSE1, T.T_ADRESSE2, T.T_CODEPOSTAL, T.T_VILLE, T.T_CIVILITE  FROM ECRITURE E, LIGNES L, TIERS T WHERE E.E_JOURNAL = 'VEN' and E.E_NUMLIGNE=1 and E.E_LIBELLE LIKE '%FAC%' and L.L_TYPEPIECE='FAC' and L.L_NUMEROLIGNE=1 and L.L_NUMEROPIECE=E.E_REFERENCE and T.T_AUXILIAIRE = E.E_AUXILIAIRE and E_DATECOMPTABLE between '" + start.ToShortDateString() + "' and '" + end.ToShortDateString() + "';";
                using (SqlCommand command = new SqlCommand(queryString, connection))
                {
                    
                    //Command 1
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            factures.Add(new Facture(Convert.ToInt32(reader.GetString(0)), reader.GetString(1), reader.GetString(2), reader.GetDateTime(3), new Client(reader.GetString(4),reader.GetString(6), reader.GetString(5),reader.GetString(7), reader.GetString(8), reader.GetString(9), reader.GetString(10), reader.GetString(11))));
                        }
                    }
                    
                    foreach(Facture f in factures)
                    {
                        //Obtenir le total
                        command.CommandText= "SELECT SUM(E_DEBIT) FROM ECRITURE E WHERE E.E_JOURNAL = 'VEN' and E_LIBELLE NOT LIKE '%Remise%' and E.E_LIBELLE LIKE '%FAC%' and E.E_REFERENCE= " + f.IdFacure +  " GROUP BY E.E_REFERENCE;";
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                f.Total = Convert.ToDouble((Decimal)reader.GetSqlDecimal(0));
                            }
                        }

                        //Obtenir la remise
                        Double remise = 0.0;
                        command.CommandText = "SELECT * FROM LIGNES WHERE L_NUMEROPIECE =" + f.IdFacure + " and L_TYPEPIECE = 'FAC' and L_TAUXREMISE=5;";
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {                
                                remise += (Convert.ToDouble((Decimal)reader.GetSqlDecimal(6))*0.95);
                            }
                            f.TotalRemise = remise;
                        }

                        //Check ifChequeAssocierGenere
                        command.CommandText = "SELECT COUNT(*) FROM CHEQUE_FIDELITE WHERE REFERENCE = 'f_" + f.IdFacure + "'";
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                f.ChequeAssocieGenere = (reader.GetInt32(0) == 0 ? false : true);
                            }
                        }


                        if (f.ChequeAssocieGenere)
                        {
                            //Check if cheque blocked
                            command.CommandText = "SELECT COUNT(*) FROM CHEQUE_FIDELITE WHERE REFERENCE = 'f_" + f.IdFacure + "' AND BLOQUE = 1";
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    f.ChequeAssocieBloque = (reader.GetInt32(0) == 0 ? false : true);
                                }
                            }


                            //Get cheque
                            command.CommandText = "SELECT * FROM CHEQUE_FIDELITE WHERE REFERENCE = 'f_" + f.IdFacure + "'";

                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                if (reader.Read())
                                {

                                    f.ChequeAssocie = new ChequeFidelite(reader.GetInt32(0), Convert.ToDouble((Decimal)reader.GetSqlDecimal(1)), reader.GetString(2), f.Client,
                                        reader.GetDateTime(4), reader.GetDateTime(5), reader.GetString(6), reader.GetBoolean(7), reader.GetString(8), reader.GetBoolean(9), chequeFideliteIsUsed(reader.GetInt32(0)));
                                }
                            }


                            //Get if used
                            command.CommandText = "SELECT L_NUMEROPIECE FROM LIGNES WHERE L_ARTICLE = 'CHQFID' AND L_LIBELLE LIKE '%CHQFD" + f.ChequeAssocie.ID + "%';";
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                bool chequeUsed = false;
                                if (reader.Read())
                                {
                                    chequeUsed = true;
                                }

                                if (chequeUsed)
                                {
                                    f.IsUsed = true;
                                    f.ChequeAssocieBloque = true;
                                }
                            }
                        }
                           

                        f.Avoir = false;

                    }
                }

            

            //Obtenir les avoirs
            //
           /* req = "SELECT * FROM ECRITURE WHERE E_JOURNAL = 'VEN' and E_DATECOMPTABLE between '" + start.ToShortDateString() + "' and '" + end.ToShortDateString() + "' and E_NUMLIGNE=1 AND E_LIBELLE LIKE '%AVC%';";
            reader = Connexion.execute_Select(req);
            while (reader.Read())
            {
                req = "SELECT L_DATECREATION FROM LIGNES WHERE L_TYPEPIECE = 'AVC' and L_NUMEROPIECE =" + Convert.ToInt32(reader.GetString(7)) + ";";
                SqlDataReader reader2 = Connexion.execute_Select(req);
                DateTime date;
                if (reader2.Read())
                {
                    date = reader2.GetDateTime(0);
                    Facture f = new Facture(Convert.ToInt32(reader.GetString(7)), reader.GetString(6), Convert.ToDouble((Decimal)reader.GetSqlDecimal(9)),
                    date, reader.GetString(37), TypePiece.Avoir, getClientById(reader.GetString(5)), 0.0);
                    f.ChequeAssocieGenere = false;
                    f.Avoir = true;
                    result.Add(f);
                }
            }*/


                //Obtenir les tickets
                //
                queryString = "SELECT DISTINCT PI_NUMEROPIECE, PI_DATEPIECE, PI_TOTALTTC, PI_AUXILIAIRE, PI_LIBELLETIERS, RD_MODEREGLE, RD_DATECREATION  FROM PIECES P, REGLEDETAIL R WHERE PI_TYPEPIECE = 'VTC' and P.PI_NUMEROPIECE = R.RD_NUMEROPIECE  and P.PI_CAISSE = R.RD_CAISSE and RD_TYPEPIECE = 'VTC' and PI_DATEPIECE between '" + start.ToShortDateString() + "' and '" + end.ToShortDateString() + "';";
                using (SqlCommand command = new SqlCommand(queryString, connection))
                {

                    //Command 1
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Facture f = new Facture(int.Parse(reader.GetString(7)[2] + reader.GetInt32(0).ToString()), reader.GetString(4), Convert.ToDouble((Decimal)reader.GetSqlDecimal(2)), reader.GetDateTime(6), reader.GetString(5), TypePiece.Ticket, getClientById(reader.GetString(3)), getRemiseTicket(reader.GetInt32(0)));
                            f.ChequeAssocieGenere = chequeFideliteAssocieExists(f);
                            if (f.ChequeAssocieGenere)
                            {
                                f.ChequeAssocieBloque = chequeFideliteAssocieIsBloque(f);
                                f.ChequeAssocie = getChequeFideliteByFacture(f);
                                if (chequeFideliteAssocieIsUsed(f))
                                {
                                    f.IsUsed = true;
                                    f.ChequeAssocieBloque = true;
                                }
                            }
                            f.Avoir = false;
                            factures.Add(f);
                        }
                    }
                }

            }
            
            return factures;
        }

        public double getIfPrelevement(int idFacture)
        {
            String req = "SELECT E_DEBIT FROM ECRITURE WHERE E_JOURNAL = 'VEN' and E_REFERENCE=" + idFacture + " and E_MODEP= 'PRE' and E_LIBELLE LIKE '%FAC%';";
            SqlDataReader reader = Connexion.execute_Select(req);
            double res = 0;

            while (reader.Read())
            {
                res += Convert.ToDouble((Decimal)reader.GetSqlDecimal(0));
            }

            return res;
        }

        public List<Facture> getFacturesOfDayByMode(String mode, DateTime? target = null)
        {
            DateTime start = DateTime.Now;
            if (target != null)
            {
                start = target.Value;
            }

            DateTime end = start;

            //String req = "SELECT * FROM ECRITURE WHERE E_JOURNAL = 'VEN' and E_DATECOMPTABLE between '" + start.ToShortDateString() + "' and '" + end.ToShortDateString() + "'and E_MODEP = '" + mode + "' AND E_LIBELLE LIKE '%FAC%' ;";
            

            List<Facture> factures = new List<Facture>();
            using (SqlConnection connection = new SqlConnection(info))
            {
                connection.Open();
                var queryString = "SELECT E.E_REFERENCE, E.E_LIBELLE, E.E_MODEP, L.L_DATECREATION, T.T_AUXILIAIRE, T.T_NATUREAUXI, T.T_LIBELLE, T.T_ADRESSE1, T.T_ADRESSE2, T.T_CODEPOSTAL, T.T_VILLE, T.T_CIVILITE, E_DEBIT  FROM ECRITURE E, LIGNES L, TIERS T WHERE E.E_JOURNAL = 'VEN' and E.E_LIBELLE LIKE '%FAC%' and L.L_TYPEPIECE='FAC' and L.L_NUMEROLIGNE=1 and L.L_NUMEROPIECE=E.E_REFERENCE and T.T_AUXILIAIRE = E.E_AUXILIAIRE and E_DATECOMPTABLE between '" + start.ToShortDateString() + "' and '" + end.ToShortDateString() + "'and E_MODEP = '" + mode + "';";
                using (SqlCommand command = new SqlCommand(queryString, connection))
                {

                    //Command 1
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Facture f = new Facture(Convert.ToInt32(reader.GetString(0)), reader.GetString(1), reader.GetString(2), reader.GetDateTime(3), new Client(reader.GetString(4), reader.GetString(6), reader.GetString(5), reader.GetString(7), reader.GetString(8), reader.GetString(9), reader.GetString(10), reader.GetString(11)));
                            f.Total = Convert.ToDouble((Decimal)reader.GetSqlDecimal(12));
                            factures.Add(f);
                        }
                    }

                    foreach (Facture f in factures)
                    {
                        
                        //Obtenir la remise
                        Double remise = 0.0;
                        command.CommandText = "SELECT * FROM LIGNES WHERE L_NUMEROPIECE =" + f.IdFacure + " and L_TYPEPIECE = 'FAC' and L_TAUXREMISE=5;";
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                remise += (Convert.ToDouble((Decimal)reader.GetSqlDecimal(6)) * 0.95);
                            }
                            f.TotalRemise = remise;
                        }

                        //Check ifChequeAssocierGenere
                        command.CommandText = "SELECT COUNT(*) FROM CHEQUE_FIDELITE WHERE REFERENCE = 'f_" + f.IdFacure + "'";
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                f.ChequeAssocieGenere = (reader.GetInt32(0) == 0 ? false : true);
                            }
                        }


                        if (f.ChequeAssocieGenere)
                        {
                            //Check if cheque blocked
                            command.CommandText = "SELECT COUNT(*) FROM CHEQUE_FIDELITE WHERE REFERENCE = 'f_" + f.IdFacure + "' AND BLOQUE = 1";
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    f.ChequeAssocieBloque = (reader.GetInt32(0) == 0 ? false : true);
                                }
                            }


                            //Get cheque
                            command.CommandText = "SELECT * FROM CHEQUE_FIDELITE WHERE REFERENCE = 'f_" + f.IdFacure + "'";

                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                if (reader.Read())
                                {

                                    f.ChequeAssocie = new ChequeFidelite(reader.GetInt32(0), Convert.ToDouble((Decimal)reader.GetSqlDecimal(1)), reader.GetString(2), f.Client,
                                        reader.GetDateTime(4), reader.GetDateTime(5), reader.GetString(6), reader.GetBoolean(7), reader.GetString(8), reader.GetBoolean(9), chequeFideliteIsUsed(reader.GetInt32(0)));
                                }
                            }


                            //Get if used
                            command.CommandText = "SELECT L_NUMEROPIECE FROM LIGNES WHERE L_ARTICLE = 'CHQFID' AND L_LIBELLE LIKE '%CHQFD" + f.ChequeAssocie.ID + "%';";
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                bool chequeUsed = false;
                                if (reader.Read())
                                {
                                    chequeUsed = true;
                                }

                                if (chequeUsed)
                                {
                                    f.IsUsed = true;
                                    f.ChequeAssocieBloque = true;
                                }
                            }
                        }


                        f.Avoir = false;

                    }
                }

                //Obtenir les tickets
                //
                queryString = "SELECT DISTINCT PI_NUMEROPIECE, PI_DATEPIECE, PI_TOTALTTC, PI_AUXILIAIRE, PI_LIBELLETIERS, RD_MODEREGLE, RD_DATECREATION, PI_CAISSE  FROM PIECES P, REGLEDETAIL R WHERE PI_TYPEPIECE = 'VTC' and P.PI_NUMEROPIECE = R.RD_NUMEROPIECE  and P.PI_CAISSE = R.RD_CAISSE and RD_TYPEPIECE = 'VTC' and PI_DATEPIECE between '" + start.ToShortDateString() + "' and '" + end.ToShortDateString() + "' and RD_MODEREGLE = '" + mode + "' ;";
                using (SqlCommand command = new SqlCommand(queryString, connection))
                {

                    //Command 1
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Facture f = new Facture(int.Parse(reader.GetString(7)[2] + reader.GetInt32(0).ToString()), reader.GetString(4), Convert.ToDouble((Decimal)reader.GetSqlDecimal(2)), reader.GetDateTime(6), reader.GetString(5), TypePiece.Ticket, getClientById(reader.GetString(3)), getRemiseTicket(reader.GetInt32(0)));
                            f.ChequeAssocieGenere = chequeFideliteAssocieExists(f);
                            if (f.ChequeAssocieGenere)
                            {
                                f.ChequeAssocieBloque = chequeFideliteAssocieIsBloque(f);
                                f.ChequeAssocie = getChequeFideliteByFacture(f);
                                if (chequeFideliteAssocieIsUsed(f))
                                {
                                    f.IsUsed = true;
                                    f.ChequeAssocieBloque = true;
                                }
                            }
                            f.Avoir = false;
                            factures.Add(f);
                        }
                    }
                }
            }

            if (mode.Equals("CB"))
                factures.AddRange(getFacturesOfDayByMode("CBV", target));
            else if (mode.Equals("ESP"))
                factures.AddRange(getFacturesOfDayByMode("EEU", target));
        

            
            return factures;
        }

       
        public List<Facture> getAvoirsOfDay(DateTime? target = null)
        {
            DateTime start = DateTime.Now;
            if (target != null)
            {
                start = target.Value;
            }

            DateTime end = start;

            List<Facture> result = new List<Facture>();

            //Obtenir les avoirs
            //
            String req = "SELECT * FROM ECRITURE WHERE E_JOURNAL = 'VEN' and E_DATECOMPTABLE between '" + start.ToShortDateString() + "' and '" + end.ToShortDateString() + "' and E_NUMLIGNE=1 AND E_LIBELLE LIKE '%AVC%';";
            SqlDataReader reader = Connexion.execute_Select(req);
            while (reader.Read())
            {
                req = "SELECT L_DATECREATION FROM LIGNES WHERE L_TYPEPIECE = 'AVC' and L_NUMEROPIECE =" + Convert.ToInt32(reader.GetString(7)) + ";";
                SqlDataReader reader2 = Connexion.execute_Select(req);
                DateTime date;
                if (reader2.Read())
                {
                    date = reader2.GetDateTime(0);
                    Facture f = new Facture(Convert.ToInt32(reader.GetString(7)), reader.GetString(6), Convert.ToDouble((Decimal)reader.GetSqlDecimal(9)),
                    date, reader.GetString(37), TypePiece.Avoir, getClientById(reader.GetString(5)), 0.0);
                    f.ChequeAssocieGenere = false;
                    f.Avoir = true;
                    result.Add(f);
                }
            }

            return result;
        }
      

        public List<Facture> getFacturesOfDayByMode2(String mode, DateTime? target = null)
        {
            DateTime start = DateTime.Now;
            if (target != null)
            {
                start = target.Value;
            }

            DateTime end = start;

            String req = "SELECT * FROM ECRITURE WHERE E_JOURNAL = 'VEN' and E_DATECOMPTABLE between '" + start.ToShortDateString() + "' and '" + end.ToShortDateString() + "'and E_MODEP = '" + mode + "' AND E_LIBELLE LIKE '%FAC%' ;";
            SqlDataReader reader = Connexion.execute_Select(req);

            List<Facture> result = new List<Facture>();

            while (reader.Read())
            {

                req = "SELECT L_DATECREATION FROM LIGNES WHERE L_TYPEPIECE = 'FAC' and L_NUMEROPIECE =" + Convert.ToInt32(reader.GetString(7)) + ";";
                SqlDataReader reader2 = Connexion.execute_Select(req);
                DateTime date;
                if (reader2.Read())
                {
                    /*req = "SELECT SUM(E_DEBIT) FROM ECRITURE WHERE E_REFERENCE ='" + reader.GetString(7) + "' and E_LIBELLE NOT LIKE '%Remise%' and E_LIBELLE LIKE '%FAC%';";
                    SqlDataReader reader3 = Connexion.execute_Select(req);
                    Double total = 0.0;
                    if (reader3.Read())
                    {
                        total = Convert.ToDouble((Decimal)reader3.GetSqlDecimal(0));
                    }*/
                    date = reader2.GetDateTime(0);
                    Facture f = new Facture(Convert.ToInt32(reader.GetString(7)), reader.GetString(6), Convert.ToDouble((Decimal)reader.GetSqlDecimal(8)), date, reader.GetString(37), TypePiece.Facture, getClientById(reader.GetString(5)), getRemiseFacture(Convert.ToInt32(reader.GetString(7))));
                    f.ChequeAssocieGenere = chequeFideliteAssocieExists(f);
                    if (f.ChequeAssocieGenere)
                    {
                        f.ChequeAssocieBloque = chequeFideliteAssocieIsBloque(f);
                        f.ChequeAssocie = getChequeFideliteByFacture(f);
                        if (chequeFideliteAssocieIsUsed(f))
                        {
                            f.IsUsed = true;
                            f.ChequeAssocieBloque = true;
                        }
                    }                    
                    f.Avoir = false;
                    result.Add(f);
                }                
                
            }

            //Obtenir les avoirs
            //
            req = "SELECT * FROM ECRITURE WHERE E_JOURNAL = 'VEN' and E_DATECOMPTABLE between '" + start.ToShortDateString() + "' and '" + end.ToShortDateString() + "' and E_NUMLIGNE=1 AND E_LIBELLE LIKE '%AVC%' and E_MODEP = '" + mode + "';";
            reader = Connexion.execute_Select(req);
            while (reader.Read())
            {
                req = "SELECT L_DATECREATION FROM LIGNES WHERE L_TYPEPIECE = 'AVC' and L_NUMEROPIECE =" + Convert.ToInt32(reader.GetString(7)) + ";";
                SqlDataReader reader2 = Connexion.execute_Select(req);
                DateTime date;
                if (reader2.Read())
                {
                    date = reader2.GetDateTime(0);
                    Facture f = new Facture(Convert.ToInt32(reader.GetString(7)), reader.GetString(6), Convert.ToDouble((Decimal)reader.GetSqlDecimal(9)),
                    date, reader.GetString(37), TypePiece.Avoir, getClientById(reader.GetString(5)), 0.0);
                    f.ChequeAssocieGenere = false;
                    f.Avoir = true;
                    result.Add(f);
                }
            }

            //Obtenir les tickets
            req = "SELECT DISTINCT PI_NUMEROPIECE, PI_DATEPIECE, PI_TOTALTTC, PI_AUXILIAIRE, PI_LIBELLETIERS, RD_MODEREGLE FROM PIECES P, REGLEDETAIL R WHERE PI_TYPEPIECE = 'VTC' and P.PI_NUMEROPIECE = R.RD_NUMEROPIECE and PI_DATEPIECE between '" + start.ToShortDateString() + "' and '" + end.ToShortDateString() + "' and RD_MODEREGLE = '"+ mode + "'  ;";
            reader = Connexion.execute_Select(req);


            while (reader.Read())
            {
                req = "SELECT L_DATECREATION FROM LIGNES WHERE L_TYPEPIECE = 'VTC' and L_NUMEROPIECE =" + reader.GetInt32(0) + ";";
                SqlDataReader reader2 = Connexion.execute_Select(req);
                DateTime date;
                if (reader2.Read())
                {
                    date = reader2.GetDateTime(0);
                    Facture f = new Facture(reader.GetInt32(0), reader.GetString(4), Convert.ToDouble((Decimal)reader.GetSqlDecimal(2)), date, reader.GetString(5), TypePiece.Ticket, getClientById(reader.GetString(3)), getRemiseTicket(reader.GetInt32(0)));
                    f.ChequeAssocieGenere = chequeFideliteAssocieExists(f);
                    if (f.ChequeAssocieGenere)
                    {
                        f.ChequeAssocieBloque = chequeFideliteAssocieIsBloque(f);
                        f.ChequeAssocie = getChequeFideliteByFacture(f);
                        if (chequeFideliteAssocieIsUsed(f))
                        {
                            f.IsUsed = true;
                            f.ChequeAssocieBloque = true;
                        }
                    }
                    f.Avoir = false;
                    result.Add(f);
                }

            }

            Connexion.close();
            return result;
        }

        public Boolean chequeFideliteAssocieIsUsed(Facture f)
        {
            bool result = false;
            using (SqlConnection connection = new SqlConnection(info))
            {
                connection.Open();

                var queryString = "SELECT L_NUMEROPIECE FROM LIGNES WHERE L_ARTICLE = 'CHQFID' AND L_LIBELLE LIKE '%CHQFD" + f.ChequeAssocie.ID + "%';";
                using (SqlCommand command = new SqlCommand(queryString, connection))
                {

                    //Command 1
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            result = true;
                        }
                    }
                }
            }
           
            return result;
        }

        public Boolean chequeFideliteIsUsed(int idCheque)
        {
            bool result = false;
            using (SqlConnection connection = new SqlConnection(info))
            {
                connection.Open();
                var queryString = "SELECT L_NUMEROPIECE FROM LIGNES WHERE L_ARTICLE = 'CHQFID' AND L_LIBELLE LIKE '%CHQFD" + idCheque + "%';";
                using (SqlCommand command = new SqlCommand(queryString, connection))
                {

                    //Command 1
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            result = true;
                        }
                    }
                }
            }
            
            return result;
        }

        public int getAllFacturesCount()
        {
            int result = -1;
            using (SqlConnection connection = new SqlConnection(info))
            {
                connection.Open();

                //Obtenir les factures
                //
                var queryString = "SELECT count(*) FROM ECRITURE E, LIGNES L, TIERS T WHERE E.E_JOURNAL = 'VEN' and E.E_NUMLIGNE=1 and E.E_LIBELLE LIKE '%FAC%' and L.L_TYPEPIECE='FAC' and L.L_NUMEROLIGNE=1 and L.L_NUMEROPIECE=E.E_REFERENCE and T.T_AUXILIAIRE = E.E_AUXILIAIRE;";
                using (SqlCommand command = new SqlCommand(queryString, connection))
                {

                    //Command 1
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            result = reader.GetInt32(0);
                        }
                    }
                }
            }
            return result;
        }

        public List<Facture> getAllFactures(DateTime date)
        {
            List<Facture> factures = new List<Facture>();
            using (SqlConnection connection = new SqlConnection(info))
            {
                connection.Open();

                //Obtenir les factures
                //
                var queryString = @"SELECT CASE
WHEN T7.isUsed IS NULL THEN 'False' else 'True'
  END as HasCheque,
       T5.id,
       T5.montant,
       t5.beneficiaire,
       t5.date_deb_val,
       t5.date_fin_val,
       t5.magasin,
       t5.reference,
       CASE
       WHEN t5.avoir = 1 THEN 'True' else 'False'
  END as avoir,
       CASE
    WHEN T5.HasCheque IS NULL THEN 'False' else 'True'
  END as HasCheque,
  CASE
       WHEN t5.bloque = 1 THEN 'True' else 'False'
  END as bloque,
       CASE
    WHEN T3.REMISE IS NULL THEN 0 else T3.REMISE
  END as Remise,
       T1.TOTAL,
       T1.E_REFERENCE,
       T2.E_LIBELLE,
       T2.E_MODEP,
       T2.L_DATECREATION,
       T2.T_AUXILIAIRE,
       T2.T_NATUREAUXI,
       T2.T_LIBELLE,
       T2.T_ADRESSE1,
       T2.T_ADRESSE2,
       T2.T_CODEPOSTAL,
       T2.T_VILLE,
       T2.T_CIVILITE
FROM
  (SELECT SUM(E_DEBIT) AS TOTAL,
          E.E_REFERENCE
   FROM ECRITURE E
   WHERE E.E_JOURNAL = 'VEN'
     AND E_LIBELLE NOT LIKE '%Remise%'
     AND E.E_LIBELLE LIKE '%FAC%'
     AND E.E_NUMECHE <> 0
     AND E.E_REFERENCE IN
       (SELECT E.E_REFERENCE
        FROM ECRITURE E,
                      LIGNES L,
                             TIERS T
        WHERE E.E_JOURNAL = 'VEN'
          AND E.E_NUMLIGNE=1
          AND E.E_LIBELLE LIKE '%FAC%'
          AND L.L_TYPEPIECE='FAC'
          AND L.L_NUMEROLIGNE=1
          AND L.L_NUMEROPIECE=E.E_REFERENCE
          AND T.T_AUXILIAIRE = E.E_AUXILIAIRE)
   GROUP BY E.E_REFERENCE) AS T1
JOIN
  (SELECT E.E_REFERENCE,
          E.E_LIBELLE,
          E.E_MODEP,
          L.L_DATECREATION,
          T.T_AUXILIAIRE,
          T.T_NATUREAUXI,
          T.T_LIBELLE,
          T.T_ADRESSE1,
          T.T_ADRESSE2,
          T.T_CODEPOSTAL,
          T.T_VILLE,
          T.T_CIVILITE
   FROM ECRITURE E,
                 LIGNES L,
                        TIERS T
   WHERE E.E_JOURNAL = 'VEN'
     AND E.E_NUMLIGNE=1
     AND E.E_LIBELLE LIKE '%FAC%'
     AND L.L_TYPEPIECE='FAC'
     AND L.L_NUMEROLIGNE=1
     AND L.L_NUMEROPIECE=E.E_REFERENCE
     AND T.T_AUXILIAIRE = E.E_AUXILIAIRE  AND DATEPART(yy, L.L_DATECREATION) = " + date.Year + " AND DATEPART(mm, L.L_DATECREATION) = " + date.Month + @") AS T2 ON T1.E_REFERENCE=T2.E_REFERENCE
LEFT JOIN
  (SELECT SUM(L_PUTTC) AS REMISE,
          L_NUMEROPIECE
   FROM LIGNES
   WHERE L_NUMEROPIECE IN
       (SELECT E.E_REFERENCE
        FROM ECRITURE E,
                      LIGNES L,
                             TIERS T
        WHERE E.E_JOURNAL = 'VEN'
          AND E.E_NUMLIGNE=1
          AND E.E_LIBELLE LIKE '%FAC%'
          AND E_LIBELLE NOT LIKE '%Remise%'
          AND L.L_TYPEPIECE='FAC'
          AND L.L_NUMEROLIGNE=1
          AND L.L_NUMEROPIECE=E.E_REFERENCE
          AND T.T_AUXILIAIRE = E.E_AUXILIAIRE)
     AND L_TYPEPIECE = 'FAC'
     AND L_TAUXREMISE=5
   GROUP BY L_NUMEROPIECE) AS T3 ON T3.L_NUMEROPIECE = T2.E_REFERENCE
LEFT JOIN
  (SELECT CASE
              WHEN T4.NB > 0 THEN 'True'
              ELSE 'False'
          END AS HasCheque,
          t4.reference,
          T4.REF,
          T4.bloque,
          T4.id,
          T4.montant,
          T4.beneficiaire,
          T4.date_deb_val,
          t4.date_fin_val,
          t4.magasin,
          t4.avoir
   FROM
     (SELECT COUNT(reference) AS NB,
             substring(REFERENCE, 3,10) AS REF,
             reference,
             bloque,
             id,
             montant,
             beneficiaire,
             date_deb_val,
             date_fin_val,
             magasin,
             avoir
      FROM CHEQUE_FIDELITE
      WHERE REFERENCE IN
          (SELECT 'f_' + E.E_REFERENCE
           FROM ECRITURE E,
                         LIGNES L,
                                TIERS T
           WHERE E.E_JOURNAL = 'VEN'
             AND E.E_NUMLIGNE=1
             AND E.E_LIBELLE LIKE '%FAC%'
             AND L.L_TYPEPIECE='FAC'
             AND L.L_NUMEROLIGNE=1
             AND L.L_NUMEROPIECE=E.E_REFERENCE
             AND T.T_AUXILIAIRE = E.E_AUXILIAIRE
           GROUP BY E.E_REFERENCE)
      GROUP BY reference,
               bloque,
               id,
               montant,
               beneficiaire,
               date_deb_val,
               date_fin_val,
               magasin,
               avoir) AS T4) AS T5 ON T5.REF = T1.E_REFERENCE
			   LEFT JOIN (SELECT CASE
              WHEN T6.L_NUMEROPIECE > 0 THEN 'True'
              ELSE 'False' END AS isUsed, T6.L_NUMEROPIECE
          FROM (SELECT DISTINCT L_NUMEROPIECE FROM LIGNES where L_ARTICLE = 'CHQFID') AS T6) AS T7 on T7.L_NUMEROPIECE = T1.E_REFERENCE;";
                using (SqlCommand command = new SqlCommand(queryString, connection))
                {
                    
                    //Command 1
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Facture f = new Facture(Convert.ToInt32(reader.GetString(13)), reader.GetString(14), reader.GetString(15), reader.GetDateTime(16), Convert.ToDouble((Decimal)reader.GetSqlDecimal(12)),Convert.ToDouble((Decimal)reader.GetSqlDecimal(11)), new Client(reader.GetString(17), reader.GetString(19), reader.GetString(18), reader.GetString(20), reader.GetString(21), reader.GetString(22), reader.GetString(23), reader.GetString(24)));
                            if (Convert.ToBoolean(reader.GetString(9)))
                            {
                                f.ChequeAssocieGenere = true;
                                f.ChequeAssocie = new ChequeFidelite(reader.GetInt32(1), Convert.ToDouble((Decimal)reader.GetSqlDecimal(2)), reader.GetString(3), f.Client, reader.GetDateTime(4), reader.GetDateTime(5), reader.GetString(6), Convert.ToBoolean(reader.GetString(10)), reader.GetString(7), Convert.ToBoolean(reader.GetString(8)), Convert.ToBoolean(reader.GetString(0)));
                            }
                            f.Avoir = false;
                            factures.Add(f);
                        }
                    }                   
                  
                }

                //Obtenir les tickets
                //
                queryString = "SELECT DISTINCT PI_NUMEROPIECE, PI_DATEPIECE, PI_TOTALTTC, PI_AUXILIAIRE, PI_LIBELLETIERS, RD_MODEREGLE, RD_DATECREATION, PI_CAISSE  FROM PIECES P, REGLEDETAIL R WHERE PI_TYPEPIECE = 'VTC' and P.PI_NUMEROPIECE = R.RD_NUMEROPIECE  and P.PI_CAISSE = R.RD_CAISSE and RD_TYPEPIECE = 'VTC' AND DATEPART(yy, RD_DATECREATION) = " + date.Year + " AND DATEPART(mm, RD_DATECREATION) = " + date.Month +";";
                using (SqlCommand command = new SqlCommand(queryString, connection))
                {

                    //Command 1
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Facture f = new Facture(int.Parse(reader.GetString(7)[2] +  reader.GetInt32(0).ToString()),reader.GetString(4), Convert.ToDouble((Decimal)reader.GetSqlDecimal(2)), reader.GetDateTime(6), reader.GetString(5), TypePiece.Ticket, getClientById(reader.GetString(3)), getRemiseTicket(reader.GetInt32(0)));
                            f.ChequeAssocieGenere = chequeFideliteAssocieExists(f);
                            if (f.ChequeAssocieGenere)
                            {
                                f.ChequeAssocieBloque = chequeFideliteAssocieIsBloque(f);
                                f.ChequeAssocie = getChequeFideliteByFacture(f);
                                if (chequeFideliteAssocieIsUsed(f))
                                {
                                    f.IsUsed = true;
                                    f.ChequeAssocieBloque = true;
                                }
                            }
                            f.Avoir = false;
                            factures.Add(f);
                        }
                    }
                }   

            }

            return factures;

        }

        public List<Facture> getAllFactures2()
        {
            String req = "SELECT * FROM ECRITURE WHERE E_JOURNAL = 'VEN' and E_NUMLIGNE=1 AND E_LIBELLE LIKE '%FAC%';";
            SqlDataReader reader = Connexion.execute_Select(req);
            List<Facture> result = new List<Facture>();
            while (reader.Read())
            {
                req = "SELECT L_DATECREATION FROM LIGNES WHERE L_TYPEPIECE = 'FAC' and L_NUMEROPIECE =" + Convert.ToInt32(reader.GetString(7)) + ";";
                SqlDataReader reader2 = Connexion.execute_Select(req);
                DateTime date;
                if (reader2.Read())
                {
                    /*req = "SELECT SUM(E_DEBIT) FROM ECRITURE WHERE E_REFERENCE ='" + reader.GetString(7) + "' and E_LIBELLE NOT LIKE '%Remise%' and E_LIBELLE LIKE '%FAC%';";
                    SqlDataReader reader3 = Connexion.execute_Select(req);
                    Double total = 0.0;
                    if (reader3.Read())
                    {
                        total = Convert.ToDouble((Decimal)reader3.GetSqlDecimal(0));
                    }*/
                    date = reader2.GetDateTime(0);
                    Facture f = new Facture(Convert.ToInt32(reader.GetString(7)), reader.GetString(6), Convert.ToDouble((Decimal)reader.GetSqlDecimal(8)),
                    date, reader.GetString(37), TypePiece.Facture, getClientById(reader.GetString(5)), getRemiseFacture(Convert.ToInt32(reader.GetString(7))));
                    f.ChequeAssocieGenere = chequeFideliteAssocieExists(f);
                    if (f.ChequeAssocieGenere)
                    {
                        f.ChequeAssocieBloque = chequeFideliteAssocieIsBloque(f);
                        f.ChequeAssocie = getChequeFideliteByFacture(f);
                        if (chequeFideliteAssocieIsUsed(f))
                        {
                            f.IsUsed = true;
                            f.ChequeAssocieBloque = true;
                        }
                    }
                    
                    f.Avoir = false;
                    result.Add(f);
                    reader2.Close();
                }
              
            }

            //Obtenir les avoirs
            //
            req = "SELECT * FROM ECRITURE WHERE E_JOURNAL = 'VEN' and E_NUMLIGNE=1 AND E_LIBELLE LIKE '%AVC%';";
            reader = Connexion.execute_Select(req);
            while (reader.Read())
            {
                req = "SELECT L_DATECREATION FROM LIGNES WHERE L_TYPEPIECE = 'AVC' and L_NUMEROPIECE =" + Convert.ToInt32(reader.GetString(7)) + ";";
                SqlDataReader reader2 = Connexion.execute_Select(req);
                DateTime date;
                if (reader2.Read())
                {
                    date = reader2.GetDateTime(0);
                    Facture f = new Facture(Convert.ToInt32(reader.GetString(7)), reader.GetString(6), Convert.ToDouble((Decimal)reader.GetSqlDecimal(9)),
                    date, reader.GetString(37), TypePiece.Avoir, getClientById(reader.GetString(5)), 0.0);
                    f.ChequeAssocieGenere = false;
                   
                    f.Avoir = true;
                    result.Add(f);
                }
            }

            //Obtenir les tickets
            req = "SELECT DISTINCT PI_NUMEROPIECE, PI_DATEPIECE, PI_TOTALTTC, PI_AUXILIAIRE, PI_LIBELLETIERS, RD_MODEREGLE FROM PIECES P, REGLEDETAIL R WHERE PI_TYPEPIECE = 'VTC' and P.PI_NUMEROPIECE = R.RD_NUMEROPIECE;";
            reader = Connexion.execute_Select(req);
            
            while (reader.Read())
            {
                req = "SELECT L_DATECREATION FROM LIGNES WHERE L_TYPEPIECE = 'VTC' and L_NUMEROPIECE =" + reader.GetInt32(0) + ";";
                SqlDataReader reader2 = Connexion.execute_Select(req);
                DateTime date;
                if (reader2.Read())
                {
                    date = reader2.GetDateTime(0);
                    Facture f = new Facture(reader.GetInt32(0), reader.GetString(4), Convert.ToDouble((Decimal)reader.GetSqlDecimal(2)), date, reader.GetString(5), TypePiece.Ticket, getClientById(reader.GetString(3)), getRemiseTicket(reader.GetInt32(0)));
                    f.ChequeAssocieGenere = chequeFideliteAssocieExists(f);
                    if (f.ChequeAssocieGenere)
                    {
                        f.ChequeAssocieBloque = chequeFideliteAssocieIsBloque(f);
                        f.ChequeAssocie = getChequeFideliteByFacture(f);
                        if (chequeFideliteAssocieIsUsed(f))
                        {
                            f.IsUsed = true;
                            f.ChequeAssocieBloque = true;
                        }
                    }
                    f.Avoir = false;
                    result.Add(f);
                }                              
                
            }
                
            Connexion.close();
            return result;
        }
        public ChequeFidelite getChequeFideliteById(String id){
            String req = null;
            ChequeFidelite targeted_cheque = null;
            
            req = "SELECT * FROM CHEQUE_FIDELITE WHERE id='" + id + "'";
           
            SqlDataReader reader = Connexion.execute_Select(req);
            if (reader.Read())
            {
                Client associated_client = getClientById(reader.GetString(3));
                targeted_cheque = new ChequeFidelite(reader.GetInt32(0), Convert.ToDouble((Decimal)reader.GetSqlDecimal(1)), reader.GetString(2), associated_client,
                    reader.GetDateTime(4), reader.GetDateTime(5), reader.GetString(6), reader.GetBoolean(7), reader.GetString(8), reader.GetBoolean(9), chequeFideliteIsUsed(reader.GetInt32(0)));
            }
            Connexion.close();
            return targeted_cheque;
        }

        public ChequeFidelite getChequeFideliteByFacture(Facture aFacture)
        {
            String type = null;
            ChequeFidelite targeted_cheque = null;
            if (aFacture.Type == TypePiece.Facture)
            {
                type = "f_";
            }
            else
            {
                type = "t_";
            }

            using (SqlConnection connection = new SqlConnection(info))
            {
                connection.Open();
                var queryString = "SELECT * FROM CHEQUE_FIDELITE WHERE REFERENCE = '" + type + aFacture.IdFacure + "'";
                using (SqlCommand command = new SqlCommand(queryString, connection))
                {

                    //Command 1
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            Client associated_client = getClientById(reader.GetString(3));
                            targeted_cheque = new ChequeFidelite(reader.GetInt32(0), Convert.ToDouble((Decimal)reader.GetSqlDecimal(1)), reader.GetString(2), associated_client,
                                reader.GetDateTime(4), reader.GetDateTime(5), reader.GetString(6), reader.GetBoolean(7), reader.GetString(8), reader.GetBoolean(9), chequeFideliteIsUsed(reader.GetInt32(0)));
                        }
                    }
                }
            }            
            return targeted_cheque;
        }
    
        public List<ChequeFidelite> getChequesFideliteByClient(Client client)
        {
            List<ChequeFidelite> result = new List<ChequeFidelite>();
            using (SqlConnection connection = new SqlConnection(info))
            {
                connection.Open();
                var queryString = "SELECT * FROM CHEQUE_FIDELITE WHERE t_auxiliaire='" + client.ID + "';";
                using (SqlCommand command = new SqlCommand(queryString, connection))
                {

                    //Command 1
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(new ChequeFidelite(reader.GetInt32(0), Convert.ToDouble((Decimal)reader.GetSqlDecimal(1)), reader.GetString(2), client,
                   reader.GetDateTime(4), reader.GetDateTime(5), reader.GetString(6), reader.GetBoolean(7), reader.GetString(8), reader.GetBoolean(9), chequeFideliteIsUsed(reader.GetInt32(0))));
                        }
                    }
                }
            }
           
            return result;
        }

        public Facture getFacture(int idFacture)
        {
            String req = "SELECT * FROM ECRITURE WHERE E_JOURNAL = 'VEN' and E_NUMEROPIECE = "+ idFacture +"and E_NUMLIGNE=1;";
            SqlDataReader reader = Connexion.execute_Select(req);
            SqlDataReader reader2;
            Facture result = null;
            while (reader.Read())
            {
                req = "SELECT * FROM TIERS WHERE T_AUXILIAIRE='" + reader.GetString(5) + "'";
                reader2 = Connexion.execute_Select(req);
                if (reader2.Read())
                {
                    result = new Facture(Convert.ToInt32(reader.GetString(7)), reader.GetString(6), Convert.ToDouble((Decimal)reader.GetSqlDecimal(8)), reader.GetDateTime(1), reader.GetString(37), TypePiece.Facture, new Client(reader2.GetString(0), reader2.GetString(2), reader2.GetString(1), reader2.GetString(3), reader2.GetString(4), reader2.GetString(6), reader2.GetString(7), reader2.GetString(86)), getRemiseFacture(Convert.ToInt32(reader.GetString(7))));
                }

            }

            Connexion.close();
            return result;
        }

        public Boolean chequeFideliteAssocieExists(Facture aFacture)
        {
            Boolean result = false;
            String type;
            if (aFacture.Type == TypePiece.Facture)
            {
                type = "f_";
            }
            else
            {
                type = "t_";
            }
            using (SqlConnection connection = new SqlConnection(info))
            {
                connection.Open();
                var queryString = "SELECT COUNT(*) FROM CHEQUE_FIDELITE WHERE REFERENCE = '" + type + aFacture.IdFacure + "'";
                using (SqlCommand command = new SqlCommand(queryString, connection))
                {

                    //Command 1
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            result = (reader.GetInt32(0) == 0 ? false : true);
                        }
                    }
                }
            }            
            return result;
        }

        public Boolean chequeFideliteAssocieIsBloque(Facture aFacture)
        {
            Boolean result = false;
            String type;
            if (aFacture.Type == TypePiece.Facture)
            {
                type = "f_";
            }
            else
            {
                type = "t_";
            }
            using (SqlConnection connection = new SqlConnection(info))
            {
                connection.Open();
                var queryString = "SELECT COUNT(*) FROM CHEQUE_FIDELITE WHERE REFERENCE = '" + type + aFacture.IdFacure + "' AND BLOQUE = 1";
                using (SqlCommand command = new SqlCommand(queryString, connection))
                {

                    //Command 1
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            result = (reader.GetInt32(0) == 0 ? false : true);
                        }
                    }
                }
            }            
            return result;
        }


        public bool bloquerChequeFidelite(ChequeFidelite cheque)
        {
            bool result = false;

            SqlCommand req = new SqlCommand(
           "UPDATE CHEQUE_FIDELITE SET BLOQUE = 1 WHERE ID = '" + cheque.ID + "'", Connexion.Connection);

            result = Connexion.execute_Request(req);

            Connexion.close();
            return result;
        }


        public int insertChequeFidelite(ChequeFidelite cheque)
        {
            int result = -1;

            using (SqlConnection connection = new SqlConnection(info))
            {
                connection.Open();
                var queryString = "INSERT INTO CHEQUE_FIDELITE (montant, beneficiaire, t_auxiliaire,  date_deb_val, date_fin_val, magasin, reference, bloque, avoir) " +
               "VALUES(@montant, @beneficiaire, @t_auxiliaire, @date_deb_val, @date_fin_val, @magasin, @reference, @bloque, @avoir);";
                using (SqlCommand command = new SqlCommand(queryString, connection))
                {
                    command.Parameters.Add("@montant", SqlDbType.Decimal).Value = cheque.Montant;
                    command.Parameters.Add("@beneficiaire", SqlDbType.NChar, cheque.Beneficiaire.Length).Value = cheque.Beneficiaire;
                    command.Parameters.Add("@t_auxiliaire", SqlDbType.NChar, cheque.Client.ID.Length).Value = cheque.Client.ID;
                    command.Parameters.Add("@date_deb_val", SqlDbType.DateTime).Value = cheque.DateDebutValidite;
                    command.Parameters.Add("@date_fin_val", SqlDbType.DateTime).Value = cheque.DateFinValidite;
                    command.Parameters.Add("@magasin", SqlDbType.NChar, cheque.Magasin.Length).Value = cheque.Magasin;
                    command.Parameters.Add("@reference", SqlDbType.NChar, cheque.Reference.Length).Value = cheque.Reference;
                    command.Parameters.Add("@bloque", SqlDbType.Bit).Value = false;
                    command.Parameters.Add("@avoir", SqlDbType.Bit).Value = false;

                    command.ExecuteNonQuery();

                    command.CommandText = "SELECT MAX(id) FROM CHEQUE_FIDELITE;";

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            result = reader.GetInt32(0);
                            cheque.ID = Convert.ToInt32(result);
                        }
                    }

                }
            }

            return result;
        }
     
        public List<KeyValuePair<String, String>> getModeReglement()
        {
            List<KeyValuePair<string, string>> result = new List<KeyValuePair<string, string>>();

            result.Add(new KeyValuePair<string, string>("CB", "Carte Bancaire"));
            result.Add(new KeyValuePair<string, string>("CCD", "Chèque Cadeaux"));
            result.Add(new KeyValuePair<string, string>("CHQ", "Chèque"));
            result.Add(new KeyValuePair<string, string>("DIV", "Rétrocession"));
            result.Add(new KeyValuePair<string, string>("ESP", "Espèces"));
            result.Add(new KeyValuePair<string, string>("BOR", "Partenaire"));
            result.Add(new KeyValuePair<string, string>("VIR", "PNF"));
            result.Add(new KeyValuePair<string, string>("PRE", "PNF"));
            //result.Add(new KeyValuePair<string, string>("CBV", "CB"));
            //result.Add(new KeyValuePair<string, string>("EEU", "Espèces"));
            //result.Add(new KeyValuePair<string, string>("CHI", "Chèque"));
            return result;
          
        }

    }
}
