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
using PDF;

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
           
            try
            {
                factures = App.access.getAllFactures();
            }
            catch(Exception e)
            {
                factures = new List<Facture>();
                System.Windows.Forms.MessageBox.Show(e.Message,
                       "Chèque fidélité Maxxess",
                       System.Windows.Forms.MessageBoxButtons.OK,
                       System.Windows.Forms.MessageBoxIcon.Question,
                       System.Windows.Forms.MessageBoxDefaultButton.Button2);
            }
            
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
                        DateTime? start = date_picker_start.SelectedDate;
                        DateTime? end = date_picker_end.SelectedDate;
                        if (start != null && end != null)
                        {
                            try
                            {
                                factures = App.access.getFactureByDateAndClient(txt_search.Text, start.Value, end.Value);
                            }
                            catch (Exception ex)
                            {
                                factures = new List<Facture>();
                                System.Windows.Forms.MessageBox.Show(ex.Message,
                                       "Chèque fidélité Maxxess",
                                       System.Windows.Forms.MessageBoxButtons.OK,
                                       System.Windows.Forms.MessageBoxIcon.Question,
                                       System.Windows.Forms.MessageBoxDefaultButton.Button2);
                            }
                        }
                        else
                        {
                            factures = App.access.getFactureByName(txt_search.Text);
                        }
                        factures.Sort((x, y) => DateTime.Compare(x.Date, y.Date));
                        factures.Reverse();
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

            if (item.ChequeCadeau <= 0.0)
            {
                System.Windows.Forms.MessageBox.Show("Le chèque cadeau ne peut être généré sans crédit.",
                      "Chèque fidélité Maxxess",
                      System.Windows.Forms.MessageBoxButtons.OK,
                      System.Windows.Forms.MessageBoxIcon.Question,
                      System.Windows.Forms.MessageBoxDefaultButton.Button2);
            }
            else if (item.Avoir)
            {
                System.Windows.Forms.MessageBox.Show("Un avoir a été généré pour cette facture. Le chèque-cadeau ne peut alors été généré.",
                       "Chèque fidélité Maxxess",
                       System.Windows.Forms.MessageBoxButtons.OK,
                       System.Windows.Forms.MessageBoxIcon.Question,
                       System.Windows.Forms.MessageBoxDefaultButton.Button2);
            } else if (item.ChequeAssocieBloque)
            {
                System.Windows.Forms.MessageBox.Show("Le chèque cadeau est actuellement bloqué",
                       "Chèque fidélité Maxxess",
                       System.Windows.Forms.MessageBoxButtons.OK,
                       System.Windows.Forms.MessageBoxIcon.Question,
                       System.Windows.Forms.MessageBoxDefaultButton.Button2);
            } else if (item.ChequeAssocieGenere)
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
            ventes_button.Visibility = Visibility.Visible;
            facturesCollection.Clear();
            //factures = App.access.getFacturesOfDay();
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
            date_picker_start.Visibility = Visibility.Hidden;
            date_picker_start.IsEnabled = false;
            date_picker_end.Visibility = Visibility.Hidden;
            date_picker_end.IsEnabled = false;
            bt_filtrer_date.Visibility = Visibility.Hidden;
            bt_filtrer_date.IsEnabled = false;

            BrushConverter bc = new BrushConverter();
            bt_CB.Background = (Brush)bc.ConvertFrom("#FFDDDDDD");
            bt_Cheque.Background = (Brush)bc.ConvertFrom("#FFDDDDDD");
            bt_FactureJour.Background = Brushes.LightGreen;
            bt_AllFactures.Background = (Brush)bc.ConvertFrom("#FFDDDDDD");

        }

        private void bt_AllFactures_Click(object sender, RoutedEventArgs e)
        {
            ventes_button.Visibility = Visibility.Hidden;
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
            date_picker_start.Visibility = Visibility.Visible;
            date_picker_start.IsEnabled = true;
            date_picker_end.Visibility = Visibility.Visible;
            date_picker_end.IsEnabled = true;
            bt_filtrer_date.Visibility = Visibility.Visible;
            bt_filtrer_date.IsEnabled = true;

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

        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (bt_FactureJour.Background != Brushes.LightGreen)
            {
                facturesCollection.Clear();
                factures = App.access.getAllFactures();
                factures.Sort((x, y) => DateTime.Compare(x.Date, y.Date));
                factures.Reverse();
                foreach (Facture f in factures)
                {
                    facturesCollection.Add(f);
                }
            }
            else
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
            }
        }

        private void Image_MouseEnter(object sender, MouseEventArgs e)
        {
            this.Cursor = Cursors.Hand;
        }

        private void Image_MouseLeave(object sender, MouseEventArgs e)
        {
            this.Cursor = Cursors.Arrow;
        }

        private void bt_filtrer_date_Click(object sender, RoutedEventArgs e)
        {
            DateTime? start = date_picker_start.SelectedDate;
            DateTime? end = date_picker_end.SelectedDate;
            if (start != null && end != null)
            {
                try
                {
                    factures = App.access.getFactureByDate(start.Value, end.Value);
                }
                catch (Exception ex)
                {
                    factures = new List<Facture>();
                    System.Windows.Forms.MessageBox.Show(ex.Message,
                           "Chèque fidélité Maxxess",
                           System.Windows.Forms.MessageBoxButtons.OK,
                           System.Windows.Forms.MessageBoxIcon.Question,
                           System.Windows.Forms.MessageBoxDefaultButton.Button2);
                }

                factures.Sort((x, y) => DateTime.Compare(x.Date, y.Date));
                factures.Reverse();
                facturesCollection.Clear();
                foreach (Facture f in factures)
                {
                    facturesCollection.Add(f);
                }
            }
            
           
        }

        private void vente_Click(object sender, RoutedEventArgs e)
        {
            List<Facture> CBs = App.access.getFacturesOfDayByMode("CB");
            CBs.Sort((x, y) => DateTime.Compare(x.Date, y.Date));
 
            List<Facture> cheques = App.access.getFacturesOfDayByMode("CHQ");
            cheques.Sort((x, y) => DateTime.Compare(x.Date, y.Date));

            List<Facture> especes = App.access.getFacturesOfDayByMode("ESP");
            especes.Sort((x, y) => DateTime.Compare(x.Date, y.Date));

            List<Facture> div = App.access.getFacturesOfDayByMode("DIV");
            div.Sort((x, y) => DateTime.Compare(x.Date, y.Date));

            if (CBs.Count == 0 && cheques.Count == 0 && especes.Count == 0 && div.Count == 0)
            {
                System.Windows.Forms.MessageBox.Show("Aucune vente n'a été réalisée aujourd'hui",
                          "Ventes journalières Maxxess",
                          System.Windows.Forms.MessageBoxButtons.OK,
                          System.Windows.Forms.MessageBoxIcon.Exclamation,
                          System.Windows.Forms.MessageBoxDefaultButton.Button2);
            }
            else
            {
                PDFUtils.generateJourneeDeVente(DateTime.Now, CBs, cheques, especes, div);
            }

         }


    }
}
