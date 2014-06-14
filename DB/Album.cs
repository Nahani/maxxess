/**
 * 	Fichier : Album.cs 
 * 
 * 	Version : 1.0.0 
 * 		- Definition des échanges de base avec la base de données : ADD & DELETE ;
 * 		- Récupération des valeurs d'attributs.
 * 
 * 	Auteurs : Théo BOURDIN, Alexandre BOURSIER & Nolan POTIER
 * 	
 * 	Résumé : Entité lien avec la base de données PICASA définissant un album contenant un ensemble d'images appartenant à un utilisateur. 
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Runtime.Serialization;
using System.IO;

namespace DB
{
    [Serializable]
    public class Album
    {
        // Nom de l'album cible
        String name;

        public String Name
        {
            get { return name; }
            set { name = value; }
        }

        // Utilisateur propriétaire de l'album cible
        int idUser;

        public int IdUser
        {
            get { return idUser; }
            set { idUser = value; }
        }

        /*
         * Constructeur normal d'un album
         * 
         * @param name  : Nom de l'album cible
         * @param idUser   : Identifiant de l'utilisateur propriétaire de l'albumcible 
         * 
         */
        public Album(String name, int idUser)
        {
            this.name = name;
            this.idUser = idUser;
        }

        public override String ToString()
        {
            return "{Album = " + name + ", " + idUser + "}";
        }
    }
}
