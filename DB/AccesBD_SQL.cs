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
            String req = null;
            Client targeted_user = null;
            
            req = "SELECT * FROM TIERS WHERE T_AUXILIAIRE='" + id + "'";
           
            SqlDataReader reader = Connexion.execute_Select(req);
            if (reader.Read())
            {
                targeted_user = new Client(reader.GetString(0), reader.GetString(2), reader.GetString(1), reader.GetString(3), reader.GetString(4), reader.GetString(6), reader.GetString(7), reader.GetString(86));
            }
            Connexion.close();
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
                        date = reader2.GetDateTime(0);
                        Facture f = new Facture(Convert.ToInt32(reader.GetString(7)), reader.GetString(6), Convert.ToDouble((Decimal)reader.GetSqlDecimal(8)), date, reader.GetString(37), TypePiece.Facture, client,getRemiseFacture(Convert.ToInt32(reader.GetString(7))));
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
                        date, reader.GetString(37), TypePiece.Facture, getClientById(reader.GetString(5)), 0.0);
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
                    date, reader.GetString(37), TypePiece.Facture, getClientById(reader.GetString(5)), 0.0);
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
                        date = reader2.GetDateTime(0);
                        Facture f = new Facture(Convert.ToInt32(reader.GetString(7)), reader.GetString(6), Convert.ToDouble((Decimal)reader.GetSqlDecimal(9)), date, reader.GetString(37), TypePiece.Facture, getClientById(reader.GetString(5)), getRemiseFacture(Convert.ToInt32(reader.GetString(7))));
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
                        date, reader.GetString(37), TypePiece.Facture, getClientById(reader.GetString(5)), 0.0);
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
            String req = "SELECT * FROM LIGNES WHERE L_NUMEROPIECE =" + idFacture + " and L_TYPEPIECE = 'VTC' and L_TAUXREMISE=5;";
            SqlDataReader reader = Connexion.execute_Select(req);


            while (reader.Read())
            {
                
               res += (Convert.ToDouble((Decimal)reader.GetSqlDecimal(6))*0.95);
                
            }
            Connexion.close();
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
                    date, reader.GetString(37), TypePiece.Facture, getClientById(reader.GetString(5)), 0.0);
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
                    date, reader.GetString(37), TypePiece.Facture, getClientById(reader.GetString(5)), 0.0);
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

        public List<Facture> getFacturesOfDayByMode(String mode, DateTime? target = null)
        {
            DateTime start = DateTime.Now;
            if (target != null)
            {
                start = target.Value;
            }

            DateTime end = start.AddDays(1);

            String req = "SELECT * FROM ECRITURE WHERE E_JOURNAL = 'VEN' and E_DATECOMPTABLE between '" + start.ToShortDateString() + "' and '" + end.ToShortDateString() + "' and E_NUMLIGNE=1 and E_MODEP = '" + mode + "' AND E_LIBELLE LIKE '%FAC%' ;";
            SqlDataReader reader = Connexion.execute_Select(req);

            List<Facture> result = new List<Facture>();

            while (reader.Read())
            {

                req = "SELECT L_DATECREATION FROM LIGNES WHERE L_TYPEPIECE = 'FAC' and L_NUMEROPIECE =" + Convert.ToInt32(reader.GetString(7)) + ";";
                SqlDataReader reader2 = Connexion.execute_Select(req);
                DateTime date;
                if (reader2.Read())
                {
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
                    date, reader.GetString(37), TypePiece.Facture, getClientById(reader.GetString(5)), 0.0);
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
            String req = "SELECT L_NUMEROPIECE FROM LIGNES WHERE L_ARTICLE = 'CHQFID' AND L_LIBELLE LIKE '%CHQFD" + f.ChequeAssocie.ID + "%';";
            SqlDataReader reader = Connexion.execute_Select(req);
            bool result = false;
            if (reader.Read())
            {
                result = true;
            }
            Connexion.close();
            return result;
        }

        public Boolean chequeFideliteIsUsed(int idCheque)
        {
            String req = "SELECT L_NUMEROPIECE FROM LIGNES WHERE L_ARTICLE = 'CHQFID' AND L_LIBELLE LIKE '%CHQFD" + idCheque + "%';";
            SqlDataReader reader = Connexion.execute_Select(req);
            bool result = false;
            if (reader.Read())
            {
                result = true;
            }
            Connexion.close();
            return result;
        }

        public List<Facture> getAllFactures()
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
                    date, reader.GetString(37), TypePiece.Facture, getClientById(reader.GetString(5)), 0.0);
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
            String req = null, type = null;
            ChequeFidelite targeted_cheque = null;
            if (aFacture.Type == TypePiece.Facture)
            {
                type = "f_";
            }
            else
            {
                type = "t_";
            }

            req = "SELECT * FROM CHEQUE_FIDELITE WHERE REFERENCE = '" + type + aFacture.IdFacure + "'";

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
    
        public List<ChequeFidelite> getChequesFideliteByClient(Client client)
        {
            String req = "SELECT * FROM CHEQUE_FIDELITE WHERE t_auxiliaire='" + client.ID + "';";
            SqlDataReader reader = Connexion.execute_Select(req);
            List<ChequeFidelite> result = new List<ChequeFidelite>();
            while (reader.Read())
                result.Add(new ChequeFidelite(reader.GetInt32(0), Convert.ToDouble((Decimal)reader.GetSqlDecimal(1)), reader.GetString(2), client,
                    reader.GetDateTime(4), reader.GetDateTime(5), reader.GetString(6), reader.GetBoolean(7), reader.GetString(8), reader.GetBoolean(9), chequeFideliteIsUsed(reader.GetInt32(0))));
            Connexion.close();
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
            String req = "SELECT COUNT(*) FROM CHEQUE_FIDELITE WHERE REFERENCE = '"+ type + aFacture.IdFacure + "'";
            SqlDataReader reader = Connexion.execute_Select(req);
            if (reader.Read())
            {
                result = (reader.GetInt32(0) == 0 ? false : true);
            }
            Connexion.close();
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
            String req = "SELECT COUNT(*) FROM CHEQUE_FIDELITE WHERE REFERENCE = '" + type + aFacture.IdFacure + "' AND BLOQUE = 1";
            SqlDataReader reader = Connexion.execute_Select(req);
            if (reader.Read())
            {
                result = (reader.GetInt32(0) == 0 ? false : true);
            }
            Connexion.close();
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
     

        /* 
         * Récupérer l'identifiant de l'image cible
         * 
         * @param idAlbum   : le nom de l'album contenant
         * @param name      : le nom de l'image cible
         * 
         * @return l'id de l'image si elle existe, -1 sinon
         * 
         */
        public int Get_Id_Img(int idAlbum, String name)
        {
            int idImage = -1;
            String req = "SELECT id FROM IMAGE WHERE name='" + name + "' AND idAlbum='" + idAlbum + "';";
            SqlDataReader reader = Connexion.execute_Select(req);
            while (reader.Read())
            {
                // Récupérer la colonne 0 (id) de la table formée par la requête précitée 
                idImage = reader.GetInt32(0);
            }
            Connexion.close();
            return idImage;
        }

        /* 
         * Récupérer le nom de l'image cible
         * 
         * @param id        : l'identifiant de l'image cible
         * @param idAlbum   : le nom de l'album contenant
         * 
         * @return le nom de l'image si elle existe, null sinon
         * 
         */
        public String Get_Name_Img(int idAlbum, int id)
        {
            String name = null;
            String req = "SELECT name FROM IMAGE WHERE id='" + id + "' AND idAlbum='" + idAlbum + "';";
            SqlDataReader reader = Connexion.execute_Select(req);
            while (reader.Read())
            {
                // Récupérer la colonne 0 (name) de la table formée par la requête précitée 
                name = reader.GetString(0);
            }
            Connexion.close();
            return name;
        }

        /*
	         * Obtenir une liste d'albums appartenant n'apparenant pas à l'utilisateur cible
         *
         * @param idUser    : l'identifiant de l'utilisateur propriétaire des albums cibles
	         *
         * @return la liste d'albums s'ils ont bien été récupérés, null le cas échéant
         *
         */
        public List<Album> Get_Albums_From_Other_Users(int idUser)
        {
            String req = "SELECT name,idUser FROM ALBUM WHERE idUser <> '" + idUser + "';";
            SqlDataReader reader = Connexion.execute_Select(req);
            List<Album> result = new List<Album>();
            while (reader.Read())
                result.Add(new Album(reader.GetString(0), reader.GetInt32(1)));
            Connexion.close();
            return result;
        }

        /* 
         * Récupérer une image
         * 
         * @param id        : l'identifiant de l'image cible
         * @param idAlbum   : le nom de l'album contenant
         * 
         * @return l'image si elle existe, null sinon
         * 
         */
        public byte[] Get_Image(int id, int idAlbum)
        {
            byte[] blob = null;
            String req = "SELECT size,image FROM IMAGE WHERE idAlbum = '" + idAlbum + "' AND id='" + id + "';";
            SqlDataReader reader = Connexion.execute_Select(req);

            if (reader.Read())
            {
                int size = reader.GetInt32(0);
                blob = new byte[size];
                reader.GetBytes(1, 0, blob, 0, size);
            }
            Connexion.close();
            return blob;
        }

        /* 
         * Récupérer toutes les images d'un album
         * 
         * @param idAlbum   : le nom de l'album contenant
         * 
         * @return les images si l'album n'esgt pas vide, null sinon
         * 
         */
        public List<byte[]> Get_Image_From_Albums(int idAlbum)
        {
            List<byte[]> blobs = new List<byte[]>();
            String req = "SELECT size,image FROM IMAGE WHERE idAlbum = '" + idAlbum + "';";
            SqlDataReader reader = Connexion.execute_Select(req);

            while (reader.Read())
            {
                int size = reader.GetInt32(0);
                byte[] blob = new byte[size];
                reader.GetBytes(1, 0, blob, 0, size);
                blobs.Add(blob);
            }
            Connexion.close();
            return blobs;
        }

        /* 
         * Récupérer tous les identifiants des images d'un album
         * 
         * @param idAlbum   : le nom de l'album contenant
         * 
         * @return les images si l'album n'esgt pas vide, null sinon
         * 
         */
        public List<int> Get_Image_ID_From_Albums(int idAlbum)
        {
            List<int> ids = new List<int>(); ;
            String req = "SELECT id FROM IMAGE WHERE idAlbum = '" + idAlbum + "';";
            SqlDataReader reader = Connexion.execute_Select(req);

            while (reader.Read())
            {
                ids.Add(reader.GetInt32(0));
            }
            Connexion.close();
            return ids;
        }

        /*
         * Ajouter une image dans la Base De Données
         * 
         * @return true si l'image a bien été ajoutée, false le cas échéant
         * 
         */
        public bool Add_Img(Img im)
        {
            bool flag = false;
            if (Get_Id_Img(im.IdAlbum, im.Name) == -1)
            {
                SqlCommand req = new SqlCommand(
               "INSERT INTO IMAGE (idAlbum, name,  size, image) " +
               "VALUES(@idAlbum, @name, @size, @image)", Connexion.Connection);
                req.Parameters.Add("@idAlbum", SqlDbType.Int).Value
                = im.IdAlbum;
                req.Parameters.Add("@name", SqlDbType.NChar, im.Name.Length).Value
               = im.Name;
                req.Parameters.Add("@size", SqlDbType.Int).Value = im.Image.Length;
                req.Parameters.Add("@image", SqlDbType.Image, im.Image.Length).Value
                = im.Image;

                flag = Connexion.execute_Request(req);
            }
            return flag;
        }

        /*
        * Supprimer une image de la Base De Données
        * 
        * @param id        : l'identifiant de l'image cible
        * @param idAlbum   : l'identifiant de l'album contenant
        * 
        * @return true si l'image a bien été supprimée, false le cas échéant
        * 
        */
        public bool Delete_Img(int id, int idAlbum)
        {
            bool flag = false;
            String req = "DELETE FROM IMAGE WHERE id = '" + id + "' AND idAlbum='" + idAlbum + "';";
            flag = Connexion.execute_Request(req);
            return flag;
        }

        /*
         * Obtenir un album selon son login ou son identifiant
         * 
         * @param name : le nom de l'album cible
         * @param idUser    : l'identifiant de l'utilisateur propriétaire de l'album cible
         * 
         * @return l'album s'il a bien été récupéré, null le cas échéant
         * 
         */
        public Album Get_Album(String name, int idUser)
        {
            String req = "SELECT nom, id FROM ALBUM WHERE name='" + name + "' AND idUser='" + idUser + "';";
            SqlDataReader reader = Connexion.execute_Select(req);
            Album a = null;
            if (reader.Read())
                a = new Album(reader.GetString(0), reader.GetInt32(1));
            Connexion.close();
            return a;
        }

        /*
         * Obtenir une liste d'albums appartenant à un utilisateur cible
         * 
         * @param idUser    : l'identifiant de l'utilisateur propriétaire des albums cibles
         * 
         * @return la liste d'albums s'ils ont bien été récupérés, null le cas échéant
         * 
         */
        public List<Album> Get_Albums_From_User(int idUser)
        {
            String req = "SELECT name FROM ALBUM WHERE idUser='" + idUser + "';";
            SqlDataReader reader = Connexion.execute_Select(req);
            List<Album> result = new List<Album>();
            while (reader.Read())
                result.Add(new Album(reader.GetString(0), idUser));
            Connexion.close();
            return result;
        }


        /* 
         * Récupérer l'identifiant de l'l'album
         * 
         * @param name    : le nom de l'album cible
         * @param idProp  : l'identifiant de l'utilisateur cible
         * 
         * @return l'identifiant de l'album si il existe, -1 le cas échéant
         * 
         */
        public int Get_Id_Album(String name, int idProp)
        {
            int idUser = -1;
            String req = "SELECT id FROM ALBUM WHERE name='" + name + "' AND idUser = '" + idProp + "';";
            SqlDataReader reader = Connexion.execute_Select(req);
            while (reader.Read())
            {
                // Récupérer la colonne 0 (id) de la table formée par la requête précitée 
                idUser = reader.GetInt32(0);
            }
            Connexion.close();
            return idUser;

        }

        /*
         * Vérifier si un album utilise déjà l'identifiant cible
         * 
         * @param id    : l'identifiant de l'album recherché 
         * 
         * @return true si l'album cible existe, false le cas échéant
         * 
         */
        public bool Exists_Album(int id)
        {
            String req = "SELECT * FROM ALBUM WHERE id='" + id + "'";
            SqlDataReader reader = Connexion.execute_Select(req);
            bool exists = false;
            if (reader.Read())
            {
                exists = true;
            }
            Connexion.close();
            return exists;
        }

        /*
         * Ajouter un album dans la Base De Données
         * 
         * @return true si l'album a bien été ajouté, false le cas échéant
         * 
         */
        public bool Add_Album(Album al)
        {
            /*
            bool flag = false;
            String req = "INSERT INTO ALBUM (name, date, idUser) VALUES ('" + al.Name + "','" + Convert.ToString(DateTime.Now) + "','" + al.IdUser + "');";
            flag = Connexion.execute_Request(req);
            return flag;
             */

            bool flag = false;
            if (Get_Id_Album(al.Name, al.IdUser) == -1)
            {
                SqlCommand req = new SqlCommand(
               "INSERT INTO ALBUM (name, date, idUser) " +
               "VALUES(@name, @date, @idUser)", Connexion.Connection);
                req.Parameters.Add("@name", SqlDbType.NChar, al.Name.Length).Value
                = al.Name;
                req.Parameters.Add("@date", SqlDbType.Date).Value
               = DateTime.Now;
                req.Parameters.Add("@idUser", SqlDbType.Int).Value = al.IdUser;


                flag = Connexion.execute_Request(req);
            }
            return flag;
        }


        /*
         * Supprimer un album de la Base De Données
         * 
         * @param idProp  : l'identifiant du propriétaire cible
         * @param name  : le nom de l'album cible
         * 
         * @return true si l'album a bien été supprimé, false le cas échéant
         * 
         */
        public bool Delete_Album(int idProp, String name)
        {
            bool flag = false;
            String req = "DELETE FROM ALBUM WHERE idUser = '" + idProp + "' AND name = '" + name + "';";
            flag = Connexion.execute_Request(req);
            return flag;
        }

        /*
         * Récupérer l'identifiant de l'utilisateur
         * 
         * @param login    : le login de l'utilisateur cible
         * 
         * @return l'identifiant de l'utlisateur si il existe, -1 le cas échéant
         * 
         */
        public int Get_Id_User(String login)
        {
            int idUser = -1;
            String req = "SELECT id FROM USERS WHERE login='" + login + "';";
            SqlDataReader reader = Connexion.execute_Select(req);
            while (reader.Read())
            {
                // Récupérer la colonne 0 (id) de la table formée par la requête précitée 
                idUser = reader.GetInt32(0);
            }
            Connexion.close();
            return idUser;

        }

        /*
         * Vérifier si un login est déjà existant
         * 
         * @param login    : le login de l'utilisateur cible
         * 
         * @return true si l'utilisateur cible existe, false le cas échéant
         * 
         */
        public bool Exists_User(String login)
        {
            String req = "SELECT * FROM USERS WHERE login='" + login + "'";
            SqlDataReader reader = Connexion.execute_Select(req);
            bool exists = false;
            if (reader.Read())
            {
                exists = true;
            }
            Connexion.close();
            return exists;
        }

        /*
         * Ajouter un utilisateur dans la Base De Données
         * 
         * @return true si l'utilisateur a bien été ajouté, false le cas échéant
         * 
         */
        public bool Add_User(User us)
        {
            bool flag = false;
            if (!Exists_User(us.Login))
            {
                String req = "INSERT INTO USERS (first_name, last_name, login, password, mail, lvl) VALUES ('" + us.First_name + "','" + us.Last_name + "','" + us.Login + "','" + MD5_Actions.GetMd5Hash(MD5.Create(), us.Password) + "','" + us.Mail + "','" + Convert.ToInt32(us.Level) + "');";
                flag = Connexion.execute_Request(req);
            }
            return flag;
        }

        /*
         * Supprimer un utilisateur de la Base De Données
         * 
         * @param login : le login de l'utilisateur cible
         * 
         * @return true si l'utilisateur a bien été supprimé, false le cas échéant
         * 
         */
        public bool Delete_User(String login)
        {
            bool flag = false;
            String req = "DELETE FROM USERS WHERE login = '" + login + "';";
            flag = Connexion.execute_Request(req);
            return flag;
        }

        /*
         * Obtenir un utilisateur selon son login ou son identifiant
         * 
         * @param login : le login de l'utilisateur cible
         * @param id    : l'identifiant de l'utilisateur cible
         * 
         * @return l'utilisateur s'il a bien été récupéré, null le cas échéant
         * 
         */
        public User Get_User(String login = null, int id = 0)
        {
            String req = null;
            User targeted_user = null;
            if (!id.Equals(0))
            {
                req = "SELECT * FROM USERS WHERE id='" + id + "'";
            }
            else if (!login.Equals(null))
            {
                req = "SELECT * FROM USERS WHERE login='" + login + "'";
            }
            SqlDataReader reader = Connexion.execute_Select(req);
            if (reader.Read())
            {
                targeted_user = new User(reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetString(4), reader.GetString(5), true);
            }
            Connexion.close();
            return targeted_user;
        }

        /*
         * Vérifier la validité d'un mot de passe
         * 
         * @param login         : le login de l'utilisateur présumé
         * @param password      : le mot de passe de l'utilisateur présumé éponyme
         * 
         * @return true si le mot de passe est correct, false le cas échéant
         * 
         */
        public bool Check_password(String login, String password)
        {
            String req = "SELECT password FROM USERS WHERE login='" + login + "'";
            SqlDataReader reader = Connexion.execute_Select(req);
            bool flag = false;
            if (reader.Read())
            {
                if (MD5_Actions.VerifyMd5Hash(MD5.Create(), password, reader.GetString(0)))
                {
                    flag = true;
                }
            }
            Connexion.close();
            return flag;
        }

    }
}
