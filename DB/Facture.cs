using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DB
{
    public enum TypePiece { Facture, Ticket, Avoir };
    public class Facture
    {
        private int idFacture;
        private String libelle;
        private double total;              
        private Client client;
        private double chequeCadeau;
        private DateTime date;
        private Boolean chequeAssocieGenere;
        private Boolean chequeAssocieBloque;
        private ChequeFidelite chequeAssocie;
        private Boolean hasAvoir;
        private String modeReglement;
        private TypePiece type;
        private Double totalRemise;

        public Double TotalRemise
        {
            get { return totalRemise; }
            set
            {
                totalRemise = value;  //Calculer le montant du chèque cadeau
                this.chequeCadeau = (double)Math.Floor(0.04 * totalRemise * 10.0) / 10.0;
            }
        }
        private Boolean chequeAssocieUsed;

        
        public Facture(int idFacture, String libelle, double total, DateTime date, String modeReglement, TypePiece type, Client client, Double remise)
        {
            this.IdFacure = idFacture;
            this.Libelle = libelle;
            this.Total = total;
            this.Client = client;
            this.Date = date;
            this.modeReglement = modeReglement;
            this.type = type;
            this.totalRemise = remise;
            this.chequeAssocieUsed = false;
            
            //Calculer le montant du chèque cadeau
            this.chequeCadeau = (double)Math.Floor(0.04 * totalRemise * 10.0) / 10.0;
        }

        public Facture(int idFacture, String libelle, String modeReglement, DateTime date, Client client)
        {
            this.idFacture = idFacture;
            this.libelle = libelle;
            this.modeReglement = modeReglement;
            this.Date = date;
            this.Client = client;
        }

        public Facture(int idFacture, String libelle, String modeReglement, DateTime date, double total, double remise, Client client)
        {
            this.idFacture = idFacture;
            this.libelle = libelle;
            this.modeReglement = modeReglement;
            this.Date = date;
            this.Client = client;
            this.Total = total;
            this.TotalRemise = remise;
        }

        public String isChequeAssocieGenere
        {
            get { return chequeAssocieGenere ? "Oui" : "Non"; }
        }

        public Boolean ChequeAssocieGenere
        {
            get { return chequeAssocieGenere; }
            set { chequeAssocieGenere = value; }
        }

        public Boolean IsUsed
        {
            get { return chequeAssocieUsed; }
            set { chequeAssocieUsed = value; }
        }

        public String HasAvoir
        {
            get { return hasAvoir ? "Oui" : "Non"; }
        }

        public string chequeID
        {
            get { return chequeAssocieGenere ? Convert.ToString(chequeAssocie.ID) : "-"; }
        }

        public ChequeFidelite ChequeAssocie
        {
            get { return chequeAssocie; }
            set { chequeAssocie = value; }
        }

        public Boolean Avoir
        {
            get { return hasAvoir; }
            set { hasAvoir = value; }
        }

        public String isChequeAssocieBloque
        {
            get {
                if (!chequeAssocieGenere)
                    return " - ";
                else return chequeAssocieBloque ? "Oui" : "Non";
            }
        }

        public Boolean ChequeAssocieBloque
        {
            get { return chequeAssocieBloque; }
            set { chequeAssocieBloque = value; }
        }

        public int IdFacure
        {
            get { return idFacture; }
            set { idFacture = value; }
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

        public double ChequeCadeau
        {
            get { return chequeCadeau; }
            set { chequeCadeau = value; }
        }

        public DateTime Date
        {
            get { return date; }
            set { date = value; }
        }

        public String DateShort
        {
            get { return date.ToShortDateString(); }
        }

        public String TotalEuros
        {
            get { return String.Format("{0:0.00}",total) + " €"; }
        }

        public String ChequeCadeauEuros
        {
            get { return String.Format("{0:0.00}",chequeCadeau) + " €"; }
        }

        public String ModeReglement
        {
            get { return modeReglement; }
            set { modeReglement = value; }
        }

        public TypePiece Type
        {
            get { return type; }
            set { type = value; }
        }


    }
}
