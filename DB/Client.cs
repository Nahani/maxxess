using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DB
{
    public class Client
    {
        private String id;
        private String nom;
        private String nature;
        private String adresse1;
        private String adresse2;
        private String codePostal;
        private String ville;

        public Client(String id, String nom, String nature, String adresse1, String adresse2, String codePostal, String ville)
        {
            this.id = id;
            this.nom = nom;
            this.nature = nature;
            this.adresse1 = adresse1;
            this.adresse2 = adresse2;
            this.codePostal = codePostal;
            this.ville = ville;
        }

        public String ID
        {
            get { return id; }
            set { id = value; }
        }

        public String Nom
        {
            get { return nom; }
            set { nom = value; }
        }

        public String Nature
        {
            get { return nature; }
            set { nature = value; }
        }

        public String Adresse1
        {
            get { return adresse1; }
            set { adresse1 = value; }
        }

        public String Adresse2
        {
            get { return adresse2; }
            set { adresse2 = value; }
        }

        public String CodePostal
        {
            get { return codePostal; }
            set { codePostal = value; }
        }

        public String Ville
        {
            get { return ville; }
            set { ville = value; }
        }
    }
}
