using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DB
{
    class Article
    {
        String id;
        String libelle;
        String famille;
        String sousFamille;

        public Article(String id, String libelle, String famille, String sousFamille)
        {
            this.id = id;
            this.libelle = libelle;
            this.famille = famille;
            this.sousFamille = sousFamille;
        }

        public String ID
        {
            get { return id; }
            set { id = value; }
        }

        public String Libelle
        {
            get { return libelle; }
            set { libelle = value; }
        }

        public String Famille
        {
            get { return famille; }
            set { famille = value; }
        }

        public String SousFamille
        {
            get { return sousFamille; }
            set { sousFamille = value; }
        }

    }
}
