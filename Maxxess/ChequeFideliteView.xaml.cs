using DB;
using PDF;
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

namespace Maxxess
{
    /// <summary>
    /// Logique d'interaction pour ChequeFideliteView.xaml
    /// </summary>
    public partial class ChequeFideliteView : Window
    {
        private ChequeFidelite cheque;
        public ChequeFideliteView(ChequeFidelite cheque)
        {
            this.cheque = cheque;
            InitializeComponent();
        }

        private void bt_Regenerer_Click(object sender, RoutedEventArgs e)
        {
            PDFUtils.storePDF(cheque);
            this.Close();
        }
    }
}
