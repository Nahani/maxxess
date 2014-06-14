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

    }
}
