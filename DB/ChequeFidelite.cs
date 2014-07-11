using System;
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
        private Boolean bloque;
        private Boolean avoir;
        private String reference;

        public ChequeFidelite(Double montant, String beneficiaire, Client client, DateTime dateDebutValidite, DateTime dateFinValidite, String magasin, String reference)
        {
            this.id = 0;
            this.montant = montant;
            this.beneficiaire = beneficiaire;
            this.client = client;
            this.dateDebutValidite = dateDebutValidite;
            this.dateFinValidite = dateFinValidite;
            this.magasin = magasin;
            this.reference = reference;
            this.bloque = false;
        }

        public ChequeFidelite(int id, Double montant, String beneficiaire, Client client, DateTime dateDebutValidite,
            DateTime dateFinValidite, String magasin, Boolean bloque, String reference,  Boolean avoir)
        {
            this.id = id;
            this.montant = montant;
            this.beneficiaire = beneficiaire;
            this.client = client;
            this.dateDebutValidite = dateDebutValidite;
            this.dateFinValidite = dateFinValidite;
            this.magasin = magasin;
            this.reference = reference;
            this.bloque = bloque;
            this.avoir = avoir;
        }

        public String Type
        {
            get { return reference.StartsWith("f") ? "Facture" : "Ticket"; }
        }

        public String isBloque
        {
            get { return bloque ? "Oui" : "Non"; }
        }

        public Boolean Bloque
        {
            get { return bloque; }
            set { bloque = value; }
        }

        public String hasAvoir
        {
            get { return avoir ? "Oui" : "Non"; }
        }

        public Boolean Avoir
        {
            get { return avoir; }
            set { avoir = value; }
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

        public String Reference
        {
            get { return reference; }
            set { reference = value; }
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
