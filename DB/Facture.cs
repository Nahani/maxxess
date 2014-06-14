using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DB
{
    public class Facture
    {
        private int idFacure;
        private String libelle;
        private double total;              
        private Client client;

        public Facture(int idFacture, String libelle, double total, Client client)
        {
            this.IdFacure = idFacture;
            this.Libelle = libelle;
            this.Total = total;
            this.Client = client;
        }
        public int IdFacure
        {
            get { return idFacure; }
            set { idFacure = value; }
        }

        public String Libelle
        {
            get { return libelle; }
            set { libelle = value; }
        }

        public double Total
        {
            get { return total; }
            set { total = value; }
        }

        public Client Client
        {
            get { return client; }
            set { client = value; }
        }


    }
}
