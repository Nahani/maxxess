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
    /// Logique d'interaction pour Clients.xaml
    /// </summary>
    /// 
  
    public partial class Clients : Window
    {
        private SearchBy searchBy;
        private List<Client> clients;
        private ObservableCollection<Client> clientsCollection;

        public ObservableCollection<Client> ClientsCollection
        {
            get { return clientsCollection; }
        }
        public Clients()
        {
            clientsCollection = new ObservableCollection<Client>();



            clients = App.access.getAllClients();
            
            foreach (Client c in clients)
            {
                clientsCollection.Add(c);
            }
            InitializeComponent();
            Bt_Nom.Background = Brushes.Red;
            searchBy = SearchBy.Nom;
            
        }

        private void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }

        private void txt_search_GotFocus(object sender, RoutedEventArgs e)
        {
            txt_search.Text = "";
        }

        private void Bt_numClient_Click(object sender, RoutedEventArgs e)
        {
            BrushConverter bc = new BrushConverter();
            Bt_Nom.Background = (Brush)bc.ConvertFrom("#FFDDDDDD");
            Bt_numClient.Background = Brushes.Red;
            searchBy = SearchBy.NumClient;
        }

        private void Bt_Nom_Click(object sender, RoutedEventArgs e)
        {
            BrushConverter bc = new BrushConverter();
            Bt_Nom.Background = Brushes.Red;
            Bt_numClient.Background = (Brush)bc.ConvertFrom("#FFDDDDDD");
            searchBy = SearchBy.Nom;
        }

        private void txt_search_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {


                switch (searchBy)
                {

                    case SearchBy.Nom:
                        clientsCollection.Clear();
                        clients = App.access.getClientByName(txt_search.Text);
                        foreach (Client c in clients)
                        {
                            clientsCollection.Add(c);
                        }
                        break;
                    case SearchBy.NumClient:
                        clientsCollection.Clear();
                        
                        clientsCollection.Add(App.access.getClientById(txt_search.Text));
                        
                        break;
                }
            }
        }

        private void bt_factures_Click(object sender, RoutedEventArgs e)
        {
            MainWindow view = new MainWindow();
            App.Current.MainWindow = view;
            this.Close();
            view.Show();
        }
    }
}
