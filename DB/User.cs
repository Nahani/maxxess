/**
 * 	Fichier : User.cs 
 * 
 * 	Version : 1.0.0 
 * 		- Definition des échanges de base avec la base de données : ADD & DELETE ;
 * 		- Récupération des valeurs d'attributs.
 * 
 * 	Auteurs : Théo BOURDIN, Alexandre BOURSIER & Nolan POTIER
 * 	
 * 	Résumé : Entité lien avec la base de données PICASA définissant les utilisateurs cibles. 
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Data;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization;

namespace DB
{
    [Serializable]
    public class User
    {

        private String first_name;

        public String First_name
        {
            get { return first_name; }
            set { first_name = value; }
        }

        private String last_name;

        public String Last_name
        {
            get { return last_name; }
            set { last_name = value; }
        }

        private String login;

        public String Login
        {
            get { return login; }
            set { login = value; }
        }

        private String password;

        public String Password
        {
            get { return password; }
            set { password = value; }
        }

        private String mail;

        public String Mail
        {
            get { return mail; }
            set { mail = value; }
        }

        private bool level;

        public bool Level
        {
            get { return level; }
            set { level = value; }
        }

        /*
         * Constructeur normal d'un utilisateur
         * 
         * @param first_name  : Nom de l'utilisateur cible
         * @param last_name   : Prénom de l'utilisateur cible 
         * @param login       : Login de l'utilisateur cible
         * @param password    : Mot de Passe de l'utilisateur cible
         * @param mail        : Adresse email de l'utilisateur cible
         * @param level       : Niveau d'autoritsation de l'utilisateur cible :
         *                  - false : utilisateur classique 
         *                  - true  : administrateur
         * 
         */
        public User(String first_name, String last_name, String login, String password, String mail, bool level = false)
        {
            this.first_name = first_name;
            this.last_name = last_name;
            this.login = login;
            this.password = password;
            this.mail = mail;
            this.level = level;
        }

        public override String ToString()
        {
            return "{Utilisateur = " + first_name + ", " + last_name + ", " + login + ", " + password + ", " + mail + ", " + level + "}";
        }
    }
}
