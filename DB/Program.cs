using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DB
{
    class Program
    {
        static AccesBD_SQL access = AccesBD_SQL.Instance;
        static void Main(string[] args)
        {

            Client c = access.getClientById("001");
            Console.WriteLine(c.Adresse1);

           /* c = access.getClientByName("Polo");
            Console.WriteLine(c.Adresse1);*/


            List<Facture> factures = access.getAllFactures();
            foreach (Facture f in factures)
            {
                Console.WriteLine(f.Client.Adresse1 + " - " + f.IdFacure + " - " + f.Libelle + " - " + String.Format("{0:0.00}",f.Total));
            }

            Facture facture1 = access.getFacture(1);
            Console.WriteLine(facture1.Total);


            ChequeFidelite cf = new ChequeFidelite(01903920, 140.093, "Nolan Potier le conquérant", c, DateTime.Now, DateTime.Now, "BAGNOLE SUR SEIZE");
            access.insertChequeFidelite(cf);

            Console.WriteLine(c.ID);

            Console.WriteLine(access.getChequesFideliteByClient(c).Count);

        }

    }
}