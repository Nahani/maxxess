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
      public static void DefineTables(Document document, List<Facture> CBs, List<Facture> cheques, List<Facture> especes, List<Facture> div)
    {
      DemonstrateSimpleTable(document, CBs, cheques, especes, div);
    }

    public static void addRow(Table iTable, String aNumber, String anAmount,
       String aModeReglement, String aDate)
    {
        Row aRow = iTable.AddRow();

        Cell aNumberCell = aRow.Cells[0];
        aNumberCell.AddParagraph(aNumber);

        Cell anAmountCell = aRow.Cells[1];
        anAmountCell.AddParagraph(anAmount);

        Cell aModeReglementCell = aRow.Cells[2];
        aModeReglement = resolveLabel(aModeReglement);
        aModeReglementCell.AddParagraph(aModeReglement);

        Cell aDateCell = aRow.Cells[3];
        aDateCell.AddParagraph(aDate);

    }

    public static void addTotalRow(Table iTable, String total, String label = null)
    {
        Row aTotalRow = iTable.AddRow();
        aTotalRow.Shading.Color = Colors.Gainsboro;
        if (label == null)
        {
            aTotalRow.Shading.Color = Colors.RoyalBlue;
        }
        else
        {          
            label = resolveLabel(label).ToLower();
        }
        aTotalRow.Format.Alignment = ParagraphAlignment.Right;
        aTotalRow.Format.Font.Bold = true;
        aTotalRow.Cells[0].AddParagraph("Total (" + label + ")  : " + String.Format("{0:0.00}", total) + "�");
        aTotalRow.Cells[0].MergeRight = 3;
    }

    private static string resolveLabel(string label){
        switch (label)
            {
                case "CHQ":
                    label = "Ch�que";
                    break;
                case "ESP":
                    label = "Esp�ces";
                    break;
                case "DIV":
                    label = "Div";
                    break;
                case "CB":
                    label = "Carte de cr�dit";
                    break;
                default: break;
            }
        return label;
    }

    public static void DemonstrateSimpleTable(Document document, List<Facture> CBs, List<Facture> cheques, List<Facture> especes, List<Facture> div)
    {
    
      Table table = document.LastSection.AddTable();
      table.Format.Alignment = ParagraphAlignment.Center;
      table.Borders.Width = 0.75;

      Column column = table.AddColumn();
      column.Width = 50;
      column.Format.Alignment = ParagraphAlignment.Center;
      column = table.AddColumn();
      column.Width = 130;
      column.Format.Alignment = ParagraphAlignment.Center;

      column = table.AddColumn();
      column.Width = 130;
      column.Format.Alignment = ParagraphAlignment.Center;
      column = table.AddColumn();
      column.Width = 130;
      column.Format.Alignment = ParagraphAlignment.Center;

      Row row = table.AddRow();
      row.Format.Alignment = ParagraphAlignment.Center;
      row.Height = 20;
      row.Shading.Color = Colors.PaleGoldenrod;
      Cell cell = row.Cells[0];
      cell.VerticalAlignment = VerticalAlignment.Center;
      cell.AddParagraph("N�");
      cell = row.Cells[1];
      cell.VerticalAlignment = VerticalAlignment.Center;
      cell.AddParagraph("Montant");
      cell = row.Cells[2];
      cell.VerticalAlignment = VerticalAlignment.Center;
      cell.AddParagraph("Mode de r�glement");
      cell = row.Cells[3];
      cell.VerticalAlignment = VerticalAlignment.Center;
      cell.AddParagraph("Date");

      double sum_cb = 0, sum_cheques = 0, sum_especes = 0, sum_div = 0;
      string mode = null;

      if (CBs != null && CBs.Count > 0)
      {
          foreach (Facture f in CBs)
          {
              if (mode == null)
              {
                  mode = f.ModeReglement;
              }
              sum_cb += f.Total;
              addRow(table, Convert.ToString(f.IdFacure), f.TotalEuros, f.ModeReglement, f.Date.ToString("dd/MM/yyyy", PDFUtils.francais));
          }
          addTotalRow(table, Convert.ToString(sum_cb), mode);
      }

      if (cheques != null && cheques.Count > 0)
      {
          mode = null;
          foreach (Facture f in cheques)
          {
              if (mode == null)
              {
                  mode = f.ModeReglement;
              }
              sum_cheques += f.Total;
              addRow(table, Convert.ToString(f.IdFacure), f.TotalEuros, f.ModeReglement, f.Date.ToString("dd/MM/yyyy", PDFUtils.francais));
          }
          addTotalRow(table, Convert.ToString(sum_cheques), mode);
      }

      if (especes != null && especes.Count > 0)
      {
          mode = null;
          foreach (Facture f in especes)
          {
              if (mode == null)
              {
                  mode = f.ModeReglement;
              }
              sum_especes += f.Total;
              addRow(table, Convert.ToString(f.IdFacure), f.TotalEuros, f.ModeReglement, f.Date.ToString("dd/MM/yyyy", PDFUtils.francais));
          }
          addTotalRow(table, Convert.ToString(sum_especes), mode);
      }

      if (div != null && div.Count > 0)
      {
          mode = null;
          foreach (Facture f in div)
          {
              if (mode == null)
              {
                  mode = f.ModeReglement;
              }
              sum_div += f.Total;
              addRow(table, Convert.ToString(f.IdFacure), f.TotalEuros, f.ModeReglement, f.Date.ToString("dd/MM/yyyy", PDFUtils.francais));
          }
          addTotalRow(table, Convert.ToString(sum_div), mode);
      }

      addTotalRow(table, Convert.ToString(sum_cb + sum_cheques + sum_div + sum_especes));

      //table.SetEdge(0, 0, 4, 1, Edge.Box, BorderStyle.Single, 0.75, Colors.Black);

    }

    public static void DemonstrateAlignment(Document document)
    {
      document.LastSection.AddParagraph("Cell Alignment", "Heading2");

      Table table = document.LastSection.AddTable();
      table.Borders.Visible = true;
      table.Format.Shading.Color = Colors.LavenderBlush;
      table.Shading.Color = Colors.Salmon;
      table.TopPadding = 5;
      table.BottomPadding = 5;

      Column column = table.AddColumn();
      column.Format.Alignment = ParagraphAlignment.Left;

      column = table.AddColumn();
      column.Format.Alignment = ParagraphAlignment.Center;
      
      column = table.AddColumn();
      column.Format.Alignment = ParagraphAlignment.Right;

      table.Rows.Height = 35;

      Row row = table.AddRow();
      row.VerticalAlignment = VerticalAlignment.Top;
      row.Cells[0].AddParagraph("Text");
      row.Cells[1].AddParagraph("Text");
      row.Cells[2].AddParagraph("Text");

      row = table.AddRow();
      row.VerticalAlignment = VerticalAlignment.Center;
      row.Cells[0].AddParagraph("Text");
      row.Cells[1].AddParagraph("Text");
      row.Cells[2].AddParagraph("Text");

      row = table.AddRow();
      row.VerticalAlignment = VerticalAlignment.Bottom;
      row.Cells[0].AddParagraph("Text");
      row.Cells[1].AddParagraph("Text");
      row.Cells[2].AddParagraph("Text");
    }

    public static void DemonstrateCellMerge(Document document)
    {
      document.LastSection.AddParagraph("Cell Merge", "Heading2");

      Table table = document.LastSection.AddTable();
      table.Borders.Visible = true;
      table.TopPadding = 5;
      table.BottomPadding = 5;

      Column column = table.AddColumn();
      column.Format.Alignment = ParagraphAlignment.Left;

      column = table.AddColumn();
      column.Format.Alignment = ParagraphAlignment.Center;

      column = table.AddColumn();
      column.Format.Alignment = ParagraphAlignment.Right;

      table.Rows.Height = 35;

      Row row = table.AddRow();
      row.Cells[0].AddParagraph("Merge Right");
      row.Cells[0].MergeRight = 1;

      row = table.AddRow();
      row.VerticalAlignment = VerticalAlignment.Bottom;
      row.Cells[0].MergeDown = 1;
      row.Cells[0].VerticalAlignment = VerticalAlignment.Bottom;
      row.Cells[0].AddParagraph("Merge Down");

      table.AddRow();
    }
  }
}