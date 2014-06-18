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
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ObservableCollection<Facture> facturesCollection;
        public MainWindow()
        {
            facturesCollection =  new ObservableCollection<Facture>();

            List<Facture> factures = App.access.getAllFactures();
            foreach (Facture f in factures)
            {
                facturesCollection.Add(f);
            }
            InitializeComponent();
        }

        public ObservableCollection<Facture> FacturesCollection
        { get { return facturesCollection; } }


    }
}
