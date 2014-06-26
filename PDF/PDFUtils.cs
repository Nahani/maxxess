﻿using System;
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
        private static string filename = "Maxxess.pdf";
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

            File.Copy(Path.Combine("../../", filename),
             Path.Combine(Directory.GetCurrentDirectory(), filename), true);

            try
            {
                document = PdfReader.Open(filename, "invalid password");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            document = PdfReader.Open(filename, PdfDocumentOpenMode.Modify);
            document = PdfReader.Open(filename, "user", PdfDocumentOpenMode.ReadOnly);

            bool hasOwnerAccess = document.SecuritySettings.HasOwnerPermissions;
            document = PdfReader.Open(filename, "owner");
            hasOwnerAccess = document.SecuritySettings.HasOwnerPermissions;

            gfx = XGraphics.FromPdfPage(document.Pages[0]);

            CultureInfo francais = CultureInfo.GetCultureInfo("fr-FR");

            float amount = 7;//(int)aChequeFidelite.Montant;
            string civilite = aChequeFidelite.Client.Civilite;
            string nomcomplet = aChequeFidelite.Beneficiaire.Replace(",","");
            string prenom = nomcomplet.Split(new char[] {' '})[0];
            string nom = nomcomplet.Split(new char[] { ' ' })[1];
            nomcomplet = civilite + " " + prenom + " " + nom.ToUpper();
            string compte = aChequeFidelite.Client.ID;
            //DateTime dateReception = aChequeFidelite.DateReception;
            DateTime dateDebut = aChequeFidelite.DateDebutValidite;
            DateTime dateFin = aChequeFidelite.DateFinValidite;
            string magasin = aChequeFidelite.Magasin;

            XFont classical = new XFont("Times New Roman", 12);
            XFont small = new XFont("Times New Roman", 9);
            XFont big_date = new XFont("Times New Roman", 11, XFontStyle.Bold);

            int amount_size_text = 55;
            if (amount >= 100)
            {
                amount_size_text = 45;
            }

            XFont big_amount = new XFont("Arial Black", amount_size_text, XFontStyle.Bold);
            XFont big_amount_2 = new XFont("Arial Black", amount_size_text / 2.7, XFontStyle.Bold);

            // Montant en toutes lettres header
            gfx.DrawString(CurrencyConverter.convert(amount),
              classical, XBrushes.Red, 123, 49);

            // Bénéficiaire header
            gfx.DrawString(nomcomplet,
              classical, XBrushes.Red, 69, 83);

            // Date header
            gfx.DrawString(dateDebut.AddDays(-1).ToString("dd MMMM yyyy", francais) + " à Nice" /*+ UppercaseFirst(magasin)*/,
              classical, XBrushes.Red, 272, 83);

            // N° de compte header
            gfx.DrawString(compte,
              classical, XBrushes.Red, 103, 101);

            // Début validité header
            gfx.DrawString(dateDebut.ToString("dd MMMM yyyy", francais),
              classical, XBrushes.Red, 68, 119);

            // Fin validité header
            gfx.DrawString(dateFin.ToString("dd MMMM yyyy", francais),
              classical, XBrushes.Red, 172, 119);

            // Magasin header
            gfx.DrawString(magasin.ToUpperInvariant(),
              small, XBrushes.Red, new XRect(461, 53, 100, 0), XStringFormats.Center);

            // Montant header
            if (amount >= 100)
            {
                gfx.DrawString("-" + amount,
                  big_amount, XBrushes.Black, new XRect(445, 108, 100, 0));
            }
            else if (amount < 10)
            {
                gfx.DrawString("-" + amount,
                 big_amount, XBrushes.Black, new XRect(475, 110, 100, 0));
            }
            else
            {
                gfx.DrawString("-" + amount,
                 big_amount, XBrushes.Black, new XRect(455, 110, 100, 0));
            }

            // Numéro de chèque
            gfx.DrawString(aChequeFidelite.ID,
                 classical, XBrushes.Black, new XRect(475, 200, 100, 0));
         

            // Date second
            gfx.DrawString(dateFin.ToString("dd MMMM yyyy", francais),
             big_date, XBrushes.Red, new XRect(228, 235, 100, 0), XStringFormats.Center);

            // Client second
            gfx.DrawString(nomcomplet,
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

            document.Save(filename);
            Process.Start(filename);
        }
    }
}
