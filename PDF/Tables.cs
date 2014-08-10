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

namespace HelloMigraDoc
{
  public class Tables
  {
    public static void DefineTables(Document document)
    {
      Paragraph paragraph = document.LastSection.AddParagraph("Table Overview", "Heading1");
      paragraph.AddBookmark("Tables");

      DemonstrateSimpleTable(document);
      DemonstrateAlignment(document);
      DemonstrateCellMerge(document);
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
        aModeReglementCell.AddParagraph(aModeReglement);

        Cell aDateCell = aRow.Cells[3];
        aDateCell.AddParagraph(aDate);

    }

    public static void addTotalRow(Table iTable, String total, String label = null)
    {
        Row aTotalRow = iTable.AddRow();
        aTotalRow.Format.Alignment = ParagraphAlignment.Right;
        aTotalRow.Format.Font.Bold = true;
        aTotalRow.Cells[0].AddParagraph("Total " + label +"  : " + total);
        aTotalRow.Cells[0].MergeRight = 3;
    }

    public static void DemonstrateSimpleTable(Document document)
    {
      document.LastSection.AddParagraph("Simple Tables", "Heading2");

      Table table = new Table();
      table.Borders.Width = 0.75;

      Column column = table.AddColumn();
      column.Format.Alignment = ParagraphAlignment.Center;
      column = table.AddColumn();
      column.Format.Alignment = ParagraphAlignment.Center;

      column = table.AddColumn();
      column.Format.Alignment = ParagraphAlignment.Center;
      column = table.AddColumn();
      column.Format.Alignment = ParagraphAlignment.Center;

      Row row = table.AddRow();
      row.Shading.Color = Colors.PaleGoldenrod;
      Cell cell = row.Cells[0];
      cell.AddParagraph("N°");
      cell = row.Cells[1];
      cell.AddParagraph("Montant");
      cell = row.Cells[2];
      cell.AddParagraph("Mode règlement");
      cell = row.Cells[3];
      cell.AddParagraph("Date");

      addRow(table, "1", "200", "CB", "20/10/2014");
      addRow(table, "2", "200", "CB", "20/10/2014");
      addRow(table, "3", "200", "CB", "20/10/2014");
      addRow(table, "4", "200", "CB", "20/10/2014");
      addRow(table, "5", "200", "CB", "20/10/2014");

      addTotalRow(table,"1000", "CB");

      addRow(table, "1", "200", "CHEQUE", "20/10/2014");
      addRow(table, "2", "200", "CHEQUE", "20/10/2014");
      addRow(table, "3", "200", "CHEQUE", "20/10/2014");
      addRow(table, "4", "200", "CHEQUE", "20/10/2014");
      addRow(table, "5", "200", "CHEQUE", "20/10/2014");

      addTotalRow(table,"1000", "CHEQUE");

      addTotalRow(table, "2000");

      table.SetEdge(0, 0, 4, 7, Edge.Box, BorderStyle.Single, 1.5, Colors.Black);

      document.LastSection.Add(table);
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
