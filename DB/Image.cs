/**
 * 	Fichier : Image.cs 
 * 
 * 	Version : 1.0.0 
 * 		- Definition des échanges de base avec la base de données : ADD & DELETE ;
 * 		- Récupération des valeurs d'attributs.
 * 
 * 	Auteurs : Théo BOURDIN, Alexandre BOURSIER & Nolan POTIER
 * 	
 * 	Résumé : Entité lien avec la base de données PICASA définissant les images d'un album appartenant à un utilisateur cible. 
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

namespace DB
{
    [Serializable]
    public class Img
    {
        // Identifiant unique clé primaire de l'image cible
        private int id;

        public int Id
        {
            get { return id; }
            set { id = value; }
        }

        // Identifiant de l'album contenant propriété clé étrangère
        private int idAlbum;

        public int IdAlbum
        {
            get { return idAlbum; }
            set { idAlbum = value; }
        }

        // Nom de l'image courante
        private String name;

        public String Name
        {
            get { return name; }
            set { name = value; }
        }

        // Image cible
        private byte[] image;

        public byte[] Image
        {
            get { return image; }
            set { image = value; }
        }

        /* 
         * Constructeur normal d'une image
         * 
         * @param idAlbum   : le nom de l'album contenant
         * @param name      : le nom de l'image cible
         * @param img       : l'image cible sous forme d'un tableau de bytes
         * 
         */
        public Img(int idAlbum, String name, byte[] img)
        {
            ;
            this.name = name;
            this.idAlbum = idAlbum;
            this.image = img;
        }


        public override String ToString()
        {
            return "{Image : \n n° = " + id + ", utilisateur = " + idAlbum + ", nom = " + name + ", taille = " + image.Length + "}";
        }

    }
}
