using System;
using System.Diagnostics;
using System.IO;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using PdfSharp.Pdf.Security;
using DB;
using System.Globalization;

namespace PDF
{
    public class PDFUtils
    {
        private static XGraphics gfx;
        //private static string filename = "Maxxess.pdf";
        private static PdfDocument document;

        public static string UppercaseFirst(string s)
        {
            // Check for empty string.
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            // Return char and concat substring.
            return char.ToUpper(s[0]) + s.Substring(1).ToLowerInvariant();
        }

        public static void storePDF(ChequeFidelite aChequeFidelite)
        {

            //File.Copy(Path.Combine("../../", filename),
            //Path.Combine(Directory.GetCurrentDirectory(), filename), true);

            Stream stream = new MemoryStream(ResourcePDF.Maxxess);

            try
            {
                document = PdfReader.Open(stream);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            document = PdfReader.Open(stream);
            document = PdfReader.Open(stream);

            bool hasOwnerAccess = document.SecuritySettings.HasOwnerPermissions;
            document = PdfReader.Open(stream);
            hasOwnerAccess = document.SecuritySettings.HasOwnerPermissions;

            gfx = XGraphics.FromPdfPage(document.Pages[0]);

            CultureInfo francais = CultureInfo.GetCultureInfo("fr-FR");

            float amount = (float)aChequeFidelite.Montant;
            string civilite = aChequeFidelite.Client.Civilite;
            string nomcomplet = aChequeFidelite.Beneficiaire;
            
            string compte = aChequeFidelite.Client.ID;
            //DateTime dateReception = aChequeFidelite.DateReception;
            DateTime dateDebut = aChequeFidelite.DateDebutValidite;
            DateTime dateFin = aChequeFidelite.DateFinValidite;
            string magasin = aChequeFidelite.Magasin;
            /*
            float amount = 1.0f;
            string civilite = "civil";
            string nomcomplet = "nomcomplet";
            string prenom = "prenom";
            string nom = "nom";
            nomcomplet = civilite + prenom + " " + nom.ToUpper();
            string compte = "compte";
            //DateTime dateReception = aChequeFidelite.DateReception;
            DateTime dateDebut = DateTime.Now;
            DateTime dateFin = DateTime.Now;
            string magasin = "magasin";*/

            XFont classical = new XFont("Times New Roman", 12);
            XFont small = new XFont("Times New Roman", 9);
            XFont big_date = new XFont("Times New Roman", 11, XFontStyle.Bold);

            int amount_size_text = 45;
            int amount_length = amount.ToString().Length;
            switch (amount_length)
            {
                case 1: amount_size_text = 45; break;
                case 2: amount_size_text = 45; break;
                case 3: amount_size_text = 40; break;
                case 4: amount_size_text = 35; break;
                case 5: amount_size_text = 30; break;
                case 6: amount_size_text = 25; break;
                case 7: amount_size_text = 20; break;
                default: amount_size_text = 30; break;
            }

            XFont big_amount = new XFont("Arial Black", amount_size_text, XFontStyle.Bold);
            XFont big_amount_2 = new XFont("Arial Black", amount_size_text / 2.7, XFontStyle.Bold);

            // Montant en toutes lettres header
            gfx.DrawString(CurrencyConverter.convertNumber((int)Math.Floor(amount), (int)Math.Round((amount - (int)amount)*100.0)),
              classical, XBrushes.Red, 123, 49);

            // Bénéficiaire header
            gfx.DrawString(nomcomplet,
              classical, XBrushes.Red, 69, 83);

            // Date header
            gfx.DrawString(dateDebut.ToString("dd MMMM yyyy", francais) + " à Nice" /*+ UppercaseFirst(magasin)*/,
              classical, XBrushes.Red, 272, 83);

            // N° de compte header
            gfx.DrawString(compte,
              classical, XBrushes.Red, new XRect(103, 101, 100, 0));

            // Début validité header
            gfx.DrawString(dateDebut.AddDays(1).ToString("dd MMMM yyyy", francais),
              classical, XBrushes.Red, 68, 119);

            // Fin validité header
            gfx.DrawString(dateFin.AddDays(1).ToString("dd MMMM yyyy", francais),
              classical, XBrushes.Red, 172, 119);

            // Magasin header
            gfx.DrawString(magasin.ToUpperInvariant(),
              small, XBrushes.Red, new XRect(461, 53, 100, 0), XStringFormats.Center);

            XRect rect = new XRect(445, 67, 105, 42);
            XStringFormat format = new XStringFormat();
            format.Alignment = XStringAlignment.Far;
            format.LineAlignment = XLineAlignment.Center;

            gfx.DrawString("-" + amount, big_amount, XBrushes.Black, rect, format);

            // Montant header
            /*if (amount >= 100)
            {
                gfx.DrawString("-" + amount,
                  big_amount, XBrushes.Black, new XRect(425, 108, 15, 0));
            }
            else if (amount < 10)
            {
                gfx.DrawString("-" + amount,
                 big_amount, XBrushes.Black, new XRect(455, 110, 15, 0));
            }
            else
            {
                gfx.DrawString("-" + amount,
                 big_amount, XBrushes.Black, new XRect(435, 110, 15, 0));
            }*/

            // Numéro de chèque
            gfx.DrawString(Convert.ToString(aChequeFidelite.ID),
                 classical, XBrushes.Black, new XRect(475, 200, 100, 0));
         

            // Date second
            gfx.DrawString(dateDebut.AddDays(1).ToString("dd MMMM yyyy", francais),
             big_date, XBrushes.Red, new XRect(228, 235, 100, 0), XStringFormats.Center);

            // Client second
            gfx.DrawString(nomcomplet +",",
             classical, XBrushes.Black, 38, 428);

            // Montant chiffre second
            if (amount < 10)
            {
                gfx.DrawString(amount + "€",
                    big_amount_2, XBrushes.Red, new XRect(151, 461, 100, 0));
            }
            else
            {
                gfx.DrawString(amount + "€",
                    big_amount_2, XBrushes.Red, new XRect(146, 461, 100, 0));
            }

            PdfDocumentSecurityLevel level = document.SecuritySettings.DocumentSecurityLevel;

            string path = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),"Cheques/");

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            string newFileName = "cheque_" + aChequeFidelite.ID + "_" + aChequeFidelite.DateDebutValidite.ToString("dd_mm_yyyy", francais) + ".pdf";
            document.Save(Path.Combine(path, newFileName));
            Process.Start(Path.Combine(path, newFileName));
        }
    }

}
