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

        public ObservableCollection<ChequeFidelite> ChequesCollection
        {
            get { return chequesCollection; }
        }
        public ChequesClientView(Client client)
        {
            this.Client = client;
            cheques = App.access.getChequesFideliteByClient(Client);
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

            while ((dep != null) && !(dep is ListViewItem))
            {
                dep = VisualTreeHelper.GetParent(dep);
            }

            if (dep == null)
                return;

            ChequeFidelite item = (ChequeFidelite)listViewCheques.ItemContainerGenerator.ItemFromContainer(dep);
            
            ChequeFideliteView window = new ChequeFideliteView(item);
            window.Show();
        }
    }
}
