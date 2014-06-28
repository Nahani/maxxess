using DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using PDF;

namespace Maxxess
{
    /// <summary>
    /// Logique d'interaction pour FactureCheque.xaml
    /// </summary>
    public partial class FactureChequeWindow : Window
    {
        private Facture facture;
        AccesBD_SQL access;
        ChequeFidelite aChequeFidelite;

        public FactureChequeWindow()
        {
            InitializeComponent();
        }

        public FactureChequeWindow(Facture facture)
        {
            this.facture = facture;
            InitializeComponent();
            txt_Name.Text = facture.Client.Nom;
            lb_valeur.Content = facture.ChequeCadeau + "€";
            access = AccesBD_SQL.Instance;

            aChequeFidelite = new ChequeFidelite(facture.ChequeCadeau, facture.Client.Nom, facture.Client,
               DateTime.Now, DateTime.Now.AddMonths(3), "MAXXESS NICE");
        }

        private void bt_generer_Click(object sender, RoutedEventArgs e)
        {
            access.insertChequeFidelite(aChequeFidelite);
            PDFUtils.storePDF(aChequeFidelite);
        }
    }
}
