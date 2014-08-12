﻿/**
 * 	Fichier : AccessBD.cs 
 * 
 * 
 * 	Auteurs : Théo BOURDIN, Alexandre BOURSIER & Nolan POTIER
 * 	
 * 	Résumé : Interface regroupant toutes les méthodes d'accés à la base de données.
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DB
{
    public interface AccesBD
    {

        Client getClientById(String id);
        List<Client> getClientByName(String name);

        List<Client> getAllClients();

        List<Facture> getAllFactures();
        Facture getFacture(int idFacture);

        ChequeFidelite getChequeFideliteById(String id);
        List<ChequeFidelite> getChequesFideliteByClient(Client client);
        int insertChequeFidelite(ChequeFidelite cheque);

        List<Facture> getFactureByName(String nom);

        List<Facture> getFactureByNumClient(String id);

        List<Facture> getFacturesOfDay();

        List<Facture> getFacturesOfDayByMode(String mode);

        bool bloquerChequeFidelite(ChequeFidelite cheque);

        Boolean chequeFideliteAssocieExists(Facture aFacture);

   

    }
}
