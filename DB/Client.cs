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
        private int p;
        private string p_2;
        private string p_3;
        private string p_4;
        private string p_5;
        private string p_6;
        private string p_7;

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

        public Client(int p, string p_2, string p_3, string p_4, string p_5, string p_6, string p_7)
        {
            // TODO: Complete member initialization
            this.p = p;
            this.p_2 = p_2;
            this.p_3 = p_3;
            this.p_4 = p_4;
            this.p_5 = p_5;
            this.p_6 = p_6;
            this.p_7 = p_7;
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
