#region MigraDoc - Creating Documents on the Fly
//
// Authors:
//   PDFsharp Team (mailto:PDFsharpSupport@pdfsharp.de)
//
// Copyright (c) 2001-2009 empira Software GmbH, Cologne (Germany)
//
// http://www.pdfsharp.com
// http://www.migradoc.com
// http://sourceforge.net/projects/pdfsharp
//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included
// in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

using System;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using System.Collections.Generic;
using DB;
using PDF;

namespace HelloMigraDoc
{
    public class Tables
    {
        public static void DefineTables(Document document, DateTime? targetedDate)
        {
            AccesBD_SQL instanceDB = AccesBD_SQL.Instance;

            generateFactureAndTickets(document, targetedDate);

            List<Facture> factures = instanceDB.getAvoirsOfDay(targetedDate);

            if (factures != null && factures.Count > 0)
            {
                document.LastSection.AddParagraph("\n\nAvoirs client\n\n");
                generateAvoirs(document, targetedDate, factures);
            }
        }

        public static void addRow(Table iTable, String aNumber, String anAmount,
           String aModeReglement, String aClient, String aPiece, String aDate)
        {
            Row aRow = iTable.AddRow();

            Cell aNumberCell = aRow.Cells[0];
            aNumberCell.AddParagraph(aNumber);

            Cell anAmountCell = aRow.Cells[3];
            anAmountCell.AddParagraph(anAmount);

            Cell aModeReglementCell = aRow.Cells[4];
            aModeReglement = resolveLabel(aModeReglement);
            aModeReglementCell.AddParagraph(aModeReglement);

            Cell aClientCell = aRow.Cells[2];
            aClientCell.AddParagraph(aClient);

            Cell aPieceCell = aRow.Cells[1];
            aPieceCell.AddParagraph(aPiece);
        }

        public static void addTotalRow(Table iTable, String total, String label = null)
        {
            Row aTotalRow = iTable.AddRow();
            aTotalRow.Shading.Color = Colors.Gainsboro;
            if (label == null)
            {
                label = "tout mode de règlement confondu";
                aTotalRow.Shading.Color = Colors.RoyalBlue;
            }
            else
            {
                label = resolveLabel(label).ToLower();
            }
            aTotalRow.Format.Alignment = ParagraphAlignment.Right;
            aTotalRow.Format.Font.Bold = true;
            aTotalRow.Cells[0].AddParagraph("Total (" + label + ")  : " + String.Format("{0:0.00}", total) + "€");
            aTotalRow.Cells[0].MergeRight = 4;
        }

        private static string resolveLabel(string label)
        {
            switch (label.Trim().ToUpper())
            {
                case "CHQ":
                    label = "Chèque";
                    break;
                case "ESP":
                    label = "Espèces";
                    break;
                case "DIV":
                    label = "Divers";
                    break;
                case "CB":
                    label = "Carte bancaire";
                    break;
                case "PRE":
                    label = "Prélèvement";
                    break;
                default: break;
            }
            return label;
        }

        private static Table initialize(Document document)
        {
            Table table = document.LastSection.AddTable();
            table.Format.Alignment = ParagraphAlignment.Center;
            table.Borders.Width = 0.75;

            Column column = table.AddColumn();
            column.Width = 50;
            column.Format.Alignment = ParagraphAlignment.Center;
            column = table.AddColumn();
            column.Width = 80;
            column.Format.Alignment = ParagraphAlignment.Center;

            column = table.AddColumn();
            column.Width = 180;
            column.Format.Alignment = ParagraphAlignment.Center;
            column = table.AddColumn();
            column.Width = 70;
            column.Format.Alignment = ParagraphAlignment.Center;

            column = table.AddColumn();
            column.Width = 100;
            column.Format.Alignment = ParagraphAlignment.Center;

            Row row = table.AddRow();
            row.Format.Alignment = ParagraphAlignment.Center;
            row.Height = 20;
            row.Shading.Color = Colors.PaleGoldenrod;
            Cell cell = row.Cells[0];
            cell.VerticalAlignment = VerticalAlignment.Center;
            cell.AddParagraph("N°");
            cell = row.Cells[3];
            cell.VerticalAlignment = VerticalAlignment.Center;
            cell.AddParagraph("Montant");
            cell = row.Cells[4];
            cell.VerticalAlignment = VerticalAlignment.Center;
            cell.AddParagraph("Mode de règlement");
            cell = row.Cells[2];
            cell.VerticalAlignment = VerticalAlignment.Center;
            cell.AddParagraph("Client");
            cell = row.Cells[1];
            cell.VerticalAlignment = VerticalAlignment.Center;
            cell.AddParagraph("Type");

            return table;
        }

        public static void generateAvoirs(Document document, DateTime? targetedDate, List<Facture> factures)
        {
            Table table = initialize(document);

            double totalSum = 0.0;

            DateTime date = DateTime.Now;
            if (targetedDate.Value != null)
                date = targetedDate.Value;

            factures.Sort((x, y) => DateTime.Compare(x.Date, y.Date));

            if (factures != null && factures.Count > 0)
            {
                double sum = 0.0;
                foreach (Facture f in factures)
                {
                    sum += f.Total;
                    addRow(table, Convert.ToString(f.IdFacure), f.TotalEuros, f.ModeReglement, f.Client.Nom, f.Type.ToString(), date.ToString("dd/MM/yyyy", PDFUtils.francais));
                }
                totalSum += sum;
                addTotalRow(table, Convert.ToString(sum), "Avoir client");
            }
        }

        public static void generateFactureAndTickets(Document document, DateTime? targetedDate)
        {

            Table table = initialize(document);

            AccesBD_SQL instanceDB = AccesBD_SQL.Instance;
            List<KeyValuePair<String, String>> modeReglements = instanceDB.getModeReglement();
            double totalSum = 0.0;
            //List<KeyValuePair<Facture, double>> prelevements = new List<KeyValuePair<Facture, double>>();
            DateTime date = DateTime.Now;
            if (targetedDate.Value != null)
                date = targetedDate.Value;

            foreach (KeyValuePair<String, String> reglement in modeReglements)
            {
                List<Facture> factures = instanceDB.getFacturesOfDayByMode(reglement.Key, targetedDate);
                factures.Sort((x, y) => DateTime.Compare(x.Date, y.Date));


                if (factures != null && factures.Count > 0)
                {
                    double sum = 0.0;
                    foreach (Facture f in factures)
                    {
                        sum += f.Total;
                        addRow(table, Convert.ToString(f.IdFacure), f.TotalEuros, f.ModeReglement, f.Client.Nom, f.Type.ToString(), date.ToString("dd/MM/yyyy", PDFUtils.francais));
                        /*double totalPrelevement = instanceDB.getIfPrelevement(f.IdFacure);
                        if (totalPrelevement > 0.0)
                            prelevements.Add(new KeyValuePair<Facture, double>(f, totalPrelevement));*/

                    }
                    totalSum += sum;
                    addTotalRow(table, Convert.ToString(sum), reglement.Value);
                }

            }
            /*double sumPre = 0.0;
            foreach (KeyValuePair<Facture, double> p in prelevements)
            {

                sumPre += p.Value;
                Facture f = p.Key;
                addRow(table, Convert.ToString(f.IdFacure), p.Value.ToString() + " €", "Prélèvement", f.Client.Nom, f.Type.ToString(), date.ToString("dd/MM/yyyy", PDFUtils.francais));

            }
            if (sumPre > 0.0)
                addTotalRow(table, Convert.ToString(sumPre), "Prélèvements");
            totalSum += sumPre;*/
            addTotalRow(table, Convert.ToString(totalSum));

            //table.SetEdge(0, 0, 4, 1, Edge.Box, BorderStyle.Single, 0.75, Colors.Black);

        }

    }
}
