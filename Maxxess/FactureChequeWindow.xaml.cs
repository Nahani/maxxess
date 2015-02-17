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
using System.Windows.Forms;
using System.Collections.ObjectModel;

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
        private MainWindow mainWindow;

        public FactureChequeWindow()
        {
            InitializeComponent();
        }

        public FactureChequeWindow(Facture facture, MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            this.facture = facture;
            InitializeComponent();
            txt_Name.Text = facture.Client.Nom;
            lb_valeur.Content = String.Format("{0:0.00}", facture.ChequeCadeau) + "€";
            access = AccesBD_SQL.Instance;

            String type;
            if (facture.Type == TypePiece.Facture)
            {
                type = "f_";
            }
            else
            {
                type = "t_";
            }

            aChequeFidelite = new ChequeFidelite(facture.ChequeCadeau, facture.Client.Nom, facture.Client,
               DateTime.Now, DateTime.Now.AddMonths(3), "MAXXESS NICE", type +facture.IdFacure, false);
        }

        private void bt_generer_Click(object sender, RoutedEventArgs e)
        {
            try {
                aChequeFidelite.ID = access.insertChequeFidelite(aChequeFidelite);
            }
            catch
            {
                System.Windows.Forms.MessageBox.Show("Erreur insere BDD", "Erreur",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button2);
            }
            
            PDFUtils.storePDF(aChequeFidelite);

            System.Windows.Forms.MessageBox.Show("Le chèque cadeau a été généré avec succès.",
                        "Chèque fidélité Maxxess",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button2);

            this.Close();

            ObservableCollection<Facture> facturesCollection = new ObservableCollection<Facture>();

            List<Facture> factures = App.access.getAllFactures(MainWindow.filterMonth, MainWindow.filterYear);
            factures.Sort((x, y) => DateTime.Compare(x.Date, y.Date));
            factures.Reverse();
            foreach (Facture f in factures)
            {
                facturesCollection.Add(f);
            }

            mainWindow.listViewFactures.ItemsSource = factures;
            mainWindow.listViewFactures.Items.Refresh();

        }
    }
}
