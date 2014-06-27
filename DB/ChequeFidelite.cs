﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DB
{
    public class ChequeFidelite
    {
        private int id;
        private Double montant;
        private String beneficiaire;
        private Client client;
        private DateTime dateDebutValidite;
        private DateTime dateFinValidite;
        private String magasin;

        public ChequeFidelite(Double montant, String beneficiaire, Client client, DateTime dateDebutValidite, DateTime dateFinValidite, String magasin)
        {
            this.id = 0;
            this.montant = montant;
            this.beneficiaire = beneficiaire;
            this.client = client;
            this.dateDebutValidite = dateDebutValidite;
            this.dateFinValidite = dateFinValidite;
            this.magasin = magasin;
        }

        public ChequeFidelite(int id, Double montant, String beneficiaire, Client client, DateTime dateDebutValidite, DateTime dateFinValidite, String magasin)
        {
            this.id = id;
            this.montant = montant;
            this.beneficiaire = beneficiaire;
            this.client = client;
            this.dateDebutValidite = dateDebutValidite;
            this.dateFinValidite = dateFinValidite;
            this.magasin = magasin;
        }

        public int ID
        {
            get { return id; }
            set { id = value; }
        }

        public Double Montant
        {
            get { return montant; }
            set { montant = value; }
        }

        public String Beneficiaire
        {
            get { return beneficiaire; }
            set { beneficiaire = value; }
        }

        public Client Client
        {
            get { return client; }
            set { client = value; }
        }

        public DateTime DateDebutValidite
        {
            get { return dateDebutValidite; }
            set { dateDebutValidite = value; }
        }

        public DateTime DateFinValidite
        {
            get { return dateFinValidite; }
            set { dateFinValidite = value; }
        }

        public String Validite
        {
            get { return "Du " + DateDebutValidite.ToShortDateString() + " au " + DateFinValidite.ToShortDateString(); }
        }

        public String MontantEuros
        {
            get { return Montant + "€"; }
        }

        public String Magasin
        {
            get { return magasin; }
            set { magasin = value; }
        }
    }
}
