using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DB
{
    public enum TypePiece { Facture, Ticket };
    public class Facture
    {
        private int idFacure;
        private String libelle;
        private double total;              
        private Client client;
        private double chequeCadeau;
        private DateTime date;
        private Boolean chequeAssocieGenere;
        private String modeReglement;
        private TypePiece type;

        
        public Facture(int idFacture, String libelle, double total, DateTime date, String modeReglement, TypePiece type, Client client)
        {
            this.IdFacure = idFacture;
            this.Libelle = libelle;
            this.Total = total;
            this.Client = client;
            this.Date = date;
            this.modeReglement = modeReglement;
            this.type = type;
            //Calculer le montant du chèque cadeau
            this.ChequeCadeau = total * 0.04;
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
            get { return total + " €"; }
        }

        public String ChequeCadeauEuros
        {
            get { return chequeCadeau + " €"; }
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
