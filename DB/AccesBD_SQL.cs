/**
 * 	Fichier : AccessBD_SQL.cs 
 * 
 * 	Version : 1.0.0 
 * 		- Definition des échanges de base avec la base de données pour tout les types d'éntités.
 * 		- Récupération des valeurs d'attributs.
 * 
 * 	Auteurs : Théo BOURDIN, Alexandre BOURSIER & Nolan POTIER
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

        static String info = "Server=" + System.Environment.MachineName + ";Database=Maxxess;Integrated Security=true;";
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
            String req = null;
            List<Client> resultat = new List<Client>();

            req = "SELECT * FROM TIERS;";

            SqlDataReader reader = Connexion.execute_Select(req);
            while (reader.Read())
            {
                resultat.Add(new Client(reader.GetString(0), reader.GetString(2), reader.GetString(1), reader.GetString(3), reader.GetString(4), reader.GetString(6), reader.GetString(7), reader.GetString(86)));
            }
            Connexion.close();
            return resultat;
        }

        public List<Facture> getFactureByName(String nom)
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
                queryString = "SELECT DISTINCT PI_NUMEROPIECE, PI_DATEPIECE, PI_TOTALTTC, PI_AUXILIAIRE, PI_LIBELLETIERS, RD_MODEREGLE, L_DATECREATION  FROM PIECES P, REGLEDETAIL R, LIGNES L WHERE PI_TYPEPIECE = 'VTC' and P.PI_NUMEROPIECE = R.RD_NUMEROPIECE and L.L_TYPEPIECE = 'VTC' and L.L_NUMEROPIECE = P.PI_NUMEROPIECE and PI_DATEPIECE between '" + start.ToShortDateString() + "' and '" + end.ToShortDateString() + "';";
                using (SqlCommand command = new SqlCommand(queryString, connection))
                {

                    //Command 1
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Facture f = new Facture(reader.GetInt32(0), reader.GetString(4), Convert.ToDouble((Decimal)reader.GetSqlDecimal(2)), reader.GetDateTime(6), reader.GetString(5), TypePiece.Ticket, getClientById(reader.GetString(3)), getRemiseTicket(reader.GetInt32(0)));
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

            String req = "SELECT * FROM ECRITURE WHERE E_JOURNAL = 'VEN' and E_DATECOMPTABLE between '" + start.ToShortDateString() + "' and '" + end.ToShortDateString() + "'and E_MODEP = '" + mode + "' AND E_LIBELLE LIKE '%FAC%' ;";
            

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
            }
        

            //Obtenir les tickets
            req = "SELECT DISTINCT PI_NUMEROPIECE, PI_DATEPIECE, PI_TOTALTTC, PI_AUXILIAIRE, PI_LIBELLETIERS, RD_MODEREGLE FROM PIECES P, REGLEDETAIL R WHERE PI_TYPEPIECE = 'VTC' and P.PI_NUMEROPIECE = R.RD_NUMEROPIECE and PI_DATEPIECE between '" + start.ToShortDateString() + "' and '" + end.ToShortDateString() + "' and RD_MODEREGLE = '" + mode + "'  ;";
            SqlDataReader readerOld = Connexion.execute_Select(req);


            while (readerOld.Read())
            {
                req = "SELECT L_DATECREATION FROM LIGNES WHERE L_TYPEPIECE = 'VTC' and L_NUMEROPIECE =" + readerOld.GetInt32(0) + ";";
                SqlDataReader reader2 = Connexion.execute_Select(req);
                DateTime date;
                if (reader2.Read())
                {
                    date = reader2.GetDateTime(0);
                    Facture f = new Facture(readerOld.GetInt32(0), readerOld.GetString(4), Convert.ToDouble((Decimal)readerOld.GetSqlDecimal(2)), date, readerOld.GetString(5), TypePiece.Ticket, getClientById(readerOld.GetString(3)), getRemiseTicket(readerOld.GetInt32(0)));
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

            Connexion.close();
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

        public List<Facture> getAllFactures()
        {
            List<Facture> factures = new List<Facture>();
            using (SqlConnection connection = new SqlConnection(info))
            {
                connection.Open();

                //Obtenir les factures
                //
                var queryString = "SELECT E.E_REFERENCE, E.E_LIBELLE, E.E_MODEP, L.L_DATECREATION, T.T_AUXILIAIRE, T.T_NATUREAUXI, T.T_LIBELLE, T.T_ADRESSE1, T.T_ADRESSE2, T.T_CODEPOSTAL, T.T_VILLE, T.T_CIVILITE  FROM ECRITURE E, LIGNES L, TIERS T WHERE E.E_JOURNAL = 'VEN' and E.E_NUMLIGNE=1 and E.E_LIBELLE LIKE '%FAC%' and L.L_TYPEPIECE='FAC' and L.L_NUMEROLIGNE=1 and L.L_NUMEROPIECE=E.E_REFERENCE and T.T_AUXILIAIRE = E.E_AUXILIAIRE;";
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

                //Obtenir les tickets
                //
                queryString = "SELECT DISTINCT PI_NUMEROPIECE, PI_DATEPIECE, PI_TOTALTTC, PI_AUXILIAIRE, PI_LIBELLETIERS, RD_MODEREGLE, L_DATECREATION  FROM PIECES P, REGLEDETAIL R, LIGNES L WHERE PI_TYPEPIECE = 'VTC' and P.PI_NUMEROPIECE = R.RD_NUMEROPIECE and L.L_TYPEPIECE = 'VTC' and L.L_NUMEROPIECE = P.PI_NUMEROPIECE;";
                using (SqlCommand command = new SqlCommand(queryString, connection))
                {

                    //Command 1
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Facture f = new Facture(reader.GetInt32(0), reader.GetString(4), Convert.ToDouble((Decimal)reader.GetSqlDecimal(2)), reader.GetDateTime(6), reader.GetString(5), TypePiece.Ticket, getClientById(reader.GetString(3)), getRemiseTicket(reader.GetInt32(0)));
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
           
                SqlCommand req = new SqlCommand(
               "INSERT INTO CHEQUE_FIDELITE (montant, beneficiaire, t_auxiliaire,  date_deb_val, date_fin_val, magasin, reference, bloque, avoir) " +
               "VALUES(@montant, @beneficiaire, @t_auxiliaire, @date_deb_val, @date_fin_val, @magasin, @reference, @bloque, @avoir)", Connexion.Connection);
               
                req.Parameters.Add("@montant", SqlDbType.Decimal).Value = cheque.Montant;
                req.Parameters.Add("@beneficiaire", SqlDbType.NChar, cheque.Beneficiaire.Length).Value = cheque.Beneficiaire;
                req.Parameters.Add("@t_auxiliaire", SqlDbType.NChar, cheque.Client.ID.Length).Value = cheque.Client.ID;
                req.Parameters.Add("@date_deb_val", SqlDbType.DateTime).Value = cheque.DateDebutValidite;
                req.Parameters.Add("@date_fin_val", SqlDbType.DateTime).Value = cheque.DateFinValidite;
                req.Parameters.Add("@magasin", SqlDbType.NChar, cheque.Magasin.Length).Value = cheque.Magasin;
                req.Parameters.Add("@reference", SqlDbType.NChar, cheque.Reference.Length).Value = cheque.Reference;
                req.Parameters.Add("@bloque", SqlDbType.Bit).Value = false;
                req.Parameters.Add("@avoir", SqlDbType.Bit).Value = false;

                Connexion.execute_Request(req);

                String max = "SELECT MAX(id) FROM CHEQUE_FIDELITE;";
                SqlDataReader reader = Connexion.execute_Select(max);
                if (reader.Read())
                {
                    result = reader.GetInt32(0);
                    cheque.ID = Convert.ToInt32(result);
                }

            Connexion.close();
            return result;
        }
     
        public List<KeyValuePair<String, String>> getModeReglement()
        {
            List<KeyValuePair<string, string>> result = new List<KeyValuePair<string, string>>();

            String req = "SELECT * FROM MODEREGLE";
            
            SqlDataReader reader = Connexion.execute_Select(req);
            while (reader.Read())
            {
                KeyValuePair<string,string> target = new KeyValuePair<string,string>(reader.GetString(0),reader.GetString(1)); 
                result.Add(target);
            }
            Connexion.close();

            return result;
        }

    }
}
