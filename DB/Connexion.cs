/**
 * 	Fichier : Connexion.cs 
 * 
 * 	Version : 1.0.0 
 * 		- Definition des fonctions de base de connexion : CONNEXION & DECONNEXION ;
 * 		- Séparation des méthodes d'éxecution et de sélection : EXECUTE & SELECT.
 * 
 * 	Auteurs : Théo BOURDIN, Alexandre BOURSIER & Nolan POTIER
 * 	
 * 	Résumé : Classe en lien avec la base de données PICASA définissant les modalités d'échange avec cette dernière
 * 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;

namespace DB
{
    public class Connexion
    {
        // Information relatives à la connexion à la base de données éponyme
        static String info = "Server=" + System.Environment.MachineName +"\\SQLEXPRESS;Database=MAXXESS;Integrated Security=true;";

        // Objet de connexion en lien direct avec la base de données
        static SqlConnection connection;

        /*
         * Récupérer/Modifer l'objet lien de connexion avec la base de données
         * 
         */
        public static SqlConnection Connection
        {
            get { return connection; }
            set { connection = value; }
        }

        /*
         * Ouvrir la connexion avec la base de données
         * 
         */
        static public void open()
        {
            connection = new SqlConnection(info);
            try
            {
                connection.Open();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error :" + e.Message);
            }
        }

        /*
         * Fermer la connexion avec la base de données
         * 
         */
        static public void close()
        {
            connection.Close();
        }

        /*
         * Executer la requête cible
         * 
         * @param str   : la chaîne de requête courante
         *
         * @return true si l'opération s'est bien passée, faux le cas échéant
         *
         */
        static public bool execute_Request(String str)
        {
            open();
            int result = new SqlCommand(str, connection).ExecuteNonQuery();
            close();
            return Convert.ToBoolean(result);
        }

        /*
         * Executer la requête cible
         * 
         * @param str   : la chaîne de requête courante
         *
         * @return true si l'opération s'est bien passée, faux le cas échéant
         *
         */
        static public bool execute_Request(SqlCommand str)
        {
            str.Connection.Open();
            int result = str.ExecuteNonQuery();
            str.Connection.Close();
            return Convert.ToBoolean(result);
        }

        /*
         * Retourner le résultat de l'éxecution la requête cible
         * 
         * @param str   : la chaîne de requête courante
         *
         * @return result, l'objet de lecture pointant sur le résultat de l'éxecution de la requête cible
         *
         */
        static public SqlDataReader execute_Select(String str)
        {
            open();
            SqlDataReader result = new SqlCommand(str, connection).ExecuteReader(CommandBehavior.SequentialAccess);
            return result;
        }
    }
}