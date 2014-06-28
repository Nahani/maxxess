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
using System.Windows.Navigation;
using System.Windows.Shapes;
using DB;
using System.Collections.ObjectModel;

namespace Maxxess
{
    enum SearchBy { Nom, NumClient };
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SearchBy searchBy;
        private List<Facture> factures;
        private ObservableCollection<Facture> facturesCollection;
        public MainWindow()
        {
            facturesCollection =  new ObservableCollection<Facture>();

            factures = App.access.getAllFactures();
            factures.Sort((x, y) => DateTime.Compare(x.Date, y.Date));
            factures.Reverse();
            foreach (Facture f in factures)
            {
                facturesCollection.Add(f);
            }
            InitializeComponent();
            Bt_Nom.Background = Brushes.Red;
            searchBy = SearchBy.Nom;
            bt_CB.Visibility = Visibility.Hidden;
            bt_Cheque.Visibility = Visibility.Hidden;
            bt_AllFactures.Background = Brushes.LightGreen;
        }

        public ObservableCollection<Facture> FacturesCollection
        { get { return facturesCollection; } }

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
                        facturesCollection.Clear();
                        factures = App.access.getFactureByName(txt_search.Text);
                        foreach (Facture f in factures)
                        {
                            facturesCollection.Add(f);
                        }
                        break;
                    case SearchBy.NumClient:
                        facturesCollection.Clear();
                        factures = App.access.getFactureByNumClient(txt_search.Text);
                        foreach (Facture f in factures)
                        {
                            facturesCollection.Add(f);
                        }
                        break;
                }
            }
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

            Facture item = (Facture)listViewFactures.ItemContainerGenerator.ItemFromContainer(dep);

            if (item.ChequeAssocieGenere)
            {
                System.Windows.Forms.MessageBox.Show("Le chèque cadeau a déjà été généré.",
                       "Chèque fidélité Maxxess",
                       System.Windows.Forms.MessageBoxButtons.OK,
                       System.Windows.Forms.MessageBoxIcon.Question,
                       System.Windows.Forms.MessageBoxDefaultButton.Button2);
            }
            else
            {
                FactureChequeWindow window = new FactureChequeWindow(item, this);
                window.Show();
            }
        
        }

        private void bt_FactureJour_Click(object sender, RoutedEventArgs e)
        {
            facturesCollection.Clear();
            factures = App.access.getFacturesOfDay();
            factures.Sort((x, y) => DateTime.Compare(x.Date, y.Date));
            factures.Reverse();
            double totalJour = 0;
            foreach (Facture f in factures)
            {
                totalJour += f.Total;
                facturesCollection.Add(f);
            }
            lb_TotalJour.Content = "Montant Total: " + totalJour + "€";
            bt_CB.IsEnabled = true;
            bt_CB.Visibility = Visibility.Visible; 
            bt_Cheque.IsEnabled = true;
            bt_Cheque.Visibility = Visibility.Visible;

            BrushConverter bc = new BrushConverter();
            bt_CB.Background = (Brush)bc.ConvertFrom("#FFDDDDDD");
            bt_Cheque.Background = (Brush)bc.ConvertFrom("#FFDDDDDD");

            bt_FactureJour.Background = Brushes.LightGreen;
            bt_AllFactures.Background = (Brush)bc.ConvertFrom("#FFDDDDDD");

        }

        private void bt_AllFactures_Click(object sender, RoutedEventArgs e)
        {
            facturesCollection.Clear();
            factures = App.access.getAllFactures();
            factures.Sort((x, y) => DateTime.Compare(x.Date, y.Date));
            factures.Reverse();
            foreach (Facture f in factures)
            {
                facturesCollection.Add(f);
            }
            lb_TotalJour.Content = "";
            bt_CB.IsEnabled = false;
            bt_CB.Visibility = Visibility.Hidden;
            bt_Cheque.IsEnabled = false;
            bt_Cheque.Visibility = Visibility.Hidden;

            BrushConverter bc = new BrushConverter();
            bt_AllFactures.Background = Brushes.LightGreen;
            bt_FactureJour.Background = (Brush)bc.ConvertFrom("#FFDDDDDD");
        }

        private void bt_CB_Click(object sender, RoutedEventArgs e)
        {
            if (bt_CB.Background != Brushes.CornflowerBlue) { 
                facturesCollection.Clear();
                factures = App.access.getFacturesOfDayByMode("CB");
                factures.Sort((x, y) => DateTime.Compare(x.Date, y.Date));
                factures.Reverse();
                double totalJour = 0;
                foreach (Facture f in factures)
                {
                    totalJour += f.Total;
                    facturesCollection.Add(f);
                }
                lb_TotalJour.Content = "Montant Total: " + totalJour + "€";
                bt_CB.Background = Brushes.CornflowerBlue;
                BrushConverter bc = new BrushConverter();
                bt_Cheque.Background = (Brush)bc.ConvertFrom("#FFDDDDDD");
            }
            else
            {
                BrushConverter bc = new BrushConverter();
                bt_CB.Background = (Brush)bc.ConvertFrom("#FFDDDDDD");
                bt_FactureJour_Click(sender, e);

            }
        }

        private void bt_Cheque_Click(object sender, RoutedEventArgs e)
        {
            if (bt_Cheque.Background != Brushes.CornflowerBlue)
            {
                facturesCollection.Clear();
                factures = App.access.getFacturesOfDayByMode("CHQ");
                factures.Sort((x, y) => DateTime.Compare(x.Date, y.Date));
                factures.Reverse();
                double totalJour = 0;
                foreach (Facture f in factures)
                {
                    totalJour += f.Total;
                    facturesCollection.Add(f);
                }
                lb_TotalJour.Content = "Montant Total: " + totalJour + "€";
                bt_Cheque.Background = Brushes.CornflowerBlue;
                BrushConverter bc = new BrushConverter();
                bt_CB.Background = (Brush)bc.ConvertFrom("#FFDDDDDD");
            }
            else
            {
                BrushConverter bc = new BrushConverter();
                bt_Cheque.Background = (Brush)bc.ConvertFrom("#FFDDDDDD");
                bt_FactureJour_Click(sender, e);

            }
        }

        private void bt_Clients_Click(object sender, RoutedEventArgs e)
        {
            Clients view = new Clients();
            App.Current.MainWindow = view;
            this.Close();
            view.Show();
        }


    }
}
