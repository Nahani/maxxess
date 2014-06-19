﻿using System;
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
        private double chequeCadeau;
        private DateTime date;
               

       

        public Facture(int idFacture, String libelle, double total, DateTime date, Client client)
        {
            this.IdFacure = idFacture;
            this.Libelle = libelle;
            this.Total = total;
            this.Client = client;
            this.Date = date;

            //Calculer le montant du chèque cadeau
            this.ChequeCadeau = total * 0.04;
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


    }
}
