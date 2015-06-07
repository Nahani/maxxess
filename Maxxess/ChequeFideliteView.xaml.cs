using DB;
using PDF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Windows;
using System.Collections.ObjectModel;

namespace Maxxess
{
    /// <summary>
    /// Logique d'interaction pour ChequeFideliteView.xaml
    /// </summary>
    public partial class ChequeFideliteView : Window
    {
        private ChequeFidelite cheque;
        AccesBD_SQL access;
        private ChequesClientView chequesClientView;

        public ChequeFideliteView(ChequeFidelite item, ChequesClientView chequesClientView)
        {
            this.cheque = item;
            this.chequesClientView = chequesClientView;
            access = AccesBD_SQL.Instance;
            InitializeComponent();
        }

        private void bt_Regenerer_Click(object sender, RoutedEventArgs e)
        {
            access.regenereCheque(cheque);
            PDFUtils.storePDF(cheque);
            this.Close();

            chequesClientView.Cheques = App.access.getChequesFideliteByClient(cheque.Client);
            ObservableCollection<ChequeFidelite> collection = new ObservableCollection<ChequeFidelite>();
            foreach (ChequeFidelite c in chequesClientView.Cheques)
            {
                collection.Add(c);
            }
            chequesClientView.listViewCheques.ItemsSource = collection;
            chequesClientView.listViewCheques.Items.Refresh();
        }

        private void bt_Bloquer_Click(object sender, RoutedEventArgs e)
        {

            DialogResult result = System.Windows.Forms.MessageBox.Show("Voulez-vous vraiment bloquer ce chèque fidélité ? Attention cette action est irreversible.",
                "Chèque fidélité Maxxess",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);

            if (result == System.Windows.Forms.DialogResult.Yes)
            {
                access.bloquerChequeFidelite(cheque);
                System.Windows.Forms.MessageBox.Show("Le chèque cadeau a été bloqué avec succès.",
                "Chèque fidélité Maxxess",
                MessageBoxButtons.OK,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);
                this.Close();

                
                chequesClientView.Cheques = App.access.getChequesFideliteByClient(cheque.Client);
                ObservableCollection<ChequeFidelite> collection = new ObservableCollection<ChequeFidelite>();
                foreach (ChequeFidelite c in chequesClientView.Cheques)
                {
                    collection.Add(c);
                }
                chequesClientView.listViewCheques.ItemsSource = collection;
                chequesClientView.listViewCheques.Items.Refresh();
            }

        }
    }
}
