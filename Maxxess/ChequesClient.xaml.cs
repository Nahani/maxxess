using DB;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Maxxess
{
    /// <summary>
    /// Logique d'interaction pour ChequesClient.xaml
    /// </summary>
    /// 
   
    public partial class ChequesClientView : Window
    {
        private Client client;
        private List<ChequeFidelite> cheques;
        private ObservableCollection<ChequeFidelite> chequesCollection;

        public List<ChequeFidelite> Cheques
        {
            get { return cheques; }
            set { cheques = value; }
        }

        public ObservableCollection<ChequeFidelite> ChequesCollection
        {
            get { return chequesCollection; }
            set { chequesCollection = value; }
        }
        public ChequesClientView(Client client)
        {
            this.Client = client;
            try
            {
                cheques = App.access.getChequesFideliteByClient(Client);
            }
            catch(Exception e)
            {
                cheques = new List<ChequeFidelite>();
                System.Windows.Forms.MessageBox.Show(client.ID + e.Message,
                         "Chèque fidélité Maxxess",
                         MessageBoxButtons.OK,
                         MessageBoxIcon.Question,
                         MessageBoxDefaultButton.Button2);

            }
            
            chequesCollection = new ObservableCollection<ChequeFidelite>();
            foreach (ChequeFidelite c in cheques)
            {
                chequesCollection.Add(c);
            }
            InitializeComponent();
        }

        public Client Client
        {
            get { return client; }
            set { client = value; }
        }

        private void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DependencyObject dep = (DependencyObject)e.OriginalSource;

            while ((dep != null) && !(dep is System.Windows.Controls.ListViewItem))
            {
                dep = VisualTreeHelper.GetParent(dep);
            }

            if (dep == null)
                return;

            ChequeFidelite item = (ChequeFidelite)listViewCheques.ItemContainerGenerator.ItemFromContainer(dep);

            if (item.Bloque)
            {
               System.Windows.Forms.MessageBox.Show("Ce chèque cadeau est actuellement bloqué. Vous ne pouvez ni le regénérer, ni le bloquer à nouveau.",
                         "Chèque fidélité Maxxess",
                         MessageBoxButtons.OK,
                         MessageBoxIcon.Question,
                         MessageBoxDefaultButton.Button2);
            }
            else
            {
                ChequeFideliteView window = new ChequeFideliteView(item,this);
                window.Show();
            }
        }
    }
}
