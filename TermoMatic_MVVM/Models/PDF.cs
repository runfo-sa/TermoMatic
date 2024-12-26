using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Font;
using iText.IO.Font.Constants;
using iText.Layout.Borders;
using iText.Kernel.Pdf.Canvas;
using System.IO;
using System.Data;
using iText.Kernel.Colors;
using iText.Layout.Renderer;
using iText.Kernel.Geom;

namespace TermoMatic
{
    class PDF
    {
        public static Table ConvertirDataTableEnTable(DataTable dt)
        {
            // Crear un array con los anchos de las columnas basado en el número de columnas en el DataTable
            float[] columnWidths = new float[dt.Columns.Count];

            columnWidths[0] = 1;

            for (int i = 1; i < columnWidths.Length; i++)
            {
                columnWidths[i] = 0.8f; // Ajusta esto según sea necesario para anchos de columna específicos
            }

            // Crear la tabla de iText7 con el número de columnas adecuado
            Table pdfTable = new Table(UnitValue.CreatePercentArray(columnWidths)).UseAllAvailableWidth();
            //pdfTable.SetWidth(UnitValue.CreatePercentValue(100));

            // Agregar encabezados de columna a la tabla
            foreach (DataColumn column in dt.Columns)
            {
                Cell headerCell = new Cell()
                    .Add(new Paragraph(column.ColumnName).SetFontSize(8))
                    .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
                    //.SetTextAlignment(TextAlignment.CENTER)
                    .SetRotationAngle(1.570799);
                pdfTable.AddHeaderCell(headerCell);
            }

            // Agregar filas de datos a la tabla
            foreach (DataRow row in dt.Rows)
            {
                foreach (DataColumn column in dt.Columns)
                {
                    Cell cell = new Cell()
                        .Add(new Paragraph(row[column].ToString())
                            .SetFontSize(8))
                        //.SetTextAlignment(TextAlignment.LEFT)
                        .SetRotationAngle(1.570799);
                    pdfTable.AddCell(cell);
                }
            }

            return pdfTable;
        }

        public static List<Table> ConvertirDataTableEnListaTable(DataTable dt, int columnasPorTabla)
        {
            List<Table> tablas = new List<Table>();
            int totalColumns = dt.Columns.Count;

            for (int startCol = 1; startCol < totalColumns; startCol += columnasPorTabla - 1)
            {
                int endCol = Math.Min(startCol + columnasPorTabla, totalColumns + 1);
                float[] columnWidths = new float[endCol - startCol];
                columnWidths[0] = 1;

                for (int i = 1; i < columnWidths.Length; i++)
                {
                    columnWidths[i] = 1f; // Ajusta esto según sea necesario para anchos de columna específicos
                }

                Table pdfTable = new Table(UnitValue.CreatePercentArray(columnWidths)).UseAllAvailableWidth();
                if (startCol < endCol - 1)
                {
                    Cell colHora = new Cell()
                        .Add(new Paragraph(dt.Columns[0].ColumnName)
                            .SetFontSize(6))
                        .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
                        .SetTextAlignment(TextAlignment.CENTER);
                        //.SetVerticalAlignment(VerticalAlignment.MIDDLE)
                        //.SetRotationAngle(Math.PI / 2);
                    pdfTable.AddHeaderCell(colHora);
                }

                // Agregar encabezados de columna a la tabla
                for (int col = startCol; col < endCol - 1; col++)
                {
                    Cell headerCell = new Cell()
                        .Add(new Paragraph(dt.Columns[col].ColumnName)
                            .SetFontSize(6))
                        .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
                        .SetTextAlignment(TextAlignment.CENTER);
                        //.SetVerticalAlignment(VerticalAlignment.MIDDLE)
                        //.SetRotationAngle(Math.PI / 2);
                    pdfTable.AddHeaderCell(headerCell);
                }


                // Agregar filas de datos a la tabla
                foreach (DataRow row in dt.Rows)
                {
                    if (startCol < endCol - 1)
                    {
                        Cell celHora = new Cell().Add(new Paragraph(row[0].ToString()).SetFontSize(6)).SetTextAlignment(TextAlignment.LEFT);
                        pdfTable.AddCell(celHora);
                    }

                    for (int col = startCol; col < endCol - 1; col++)
                    {
                        Cell cell = new Cell()
                            .Add(new Paragraph(row[col].ToString())
                                .SetFontSize(6))
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetVerticalAlignment(VerticalAlignment.MIDDLE);
                        pdfTable.AddCell(cell);
                    }
                }

                if (!pdfTable.IsEmpty())
                    tablas.Add(pdfTable);
            }

            return tablas;
        }

        public static void CrearPDF(string rutaPdf, List<Table> tablas, DateTime FechaRegistro)
        {
            PdfWriter pdfWriter = new(rutaPdf);
            PdfDocument pdfDocument = new(pdfWriter);
            Document doc = new(pdfDocument, PageSize.A4);

            doc.SetMargins(144, 36, 36, 36);

            //PdfFont boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            //float fontSize = 10f;

            //doc.Add(new Paragraph($"REPORTE TEMPERATURAS\t{FechaRegistro:dd/MM/yyyy}").SetFont(boldFont).SetFontSize(fontSize));

            for (int i = 0; i < tablas.Count; i++)
            {
                doc.Add(tablas[i]);

                // Agregar un salto de página solo si no es la última tabla
                if (i < tablas.Count - 1)
                {
                    doc.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));
                    //doc.Add(new Paragraph($"REPORTE TEMPERATURAS\t{FechaRegistro:dd/MM/yyyy}").SetFont(boldFont).SetFontSize(fontSize));
                }
            }


            doc.Close();

            PDF.CrearCabecera(rutaPdf, FechaRegistro);
        }

        private static void CrearCabecera(string rutaPdf, DateTime FechaRegistro)
        {
            string rutaTemp = rutaPdf + ".temp";
            PdfDocument pdfDoc = new(new PdfReader(rutaPdf), new PdfWriter(rutaTemp));

            // Obtener el número total de páginas del documento
            int totalPages = pdfDoc.GetNumberOfPages();

            for (int pageNum = 1; pageNum <= totalPages; pageNum++)
            {
                // Obtener la página actual
                PdfPage page = pdfDoc.GetPage(pageNum);

                // Crear el Canvas para dibujar en la página actual
                PdfCanvas canvas = new(page.NewContentStreamBefore(), page.GetResources(), pdfDoc);

                // Crear la tabla para el encabezado
                float[] columnWidths = [1, 2, 1]; // Ancho de las columnas
                Table table = new(UnitValue.CreatePercentArray(columnWidths));
                table.SetWidth(UnitValue.CreatePercentValue(100));

                // Crear la fuente en negrita
                PdfFont boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

                // Definir el tamaño de la fuente
                float fontSize = 10f;

                // Crear las celdas del encabezado
                Cell cell1 = new Cell(2, 1)
                    .Add(new Paragraph("OFFAL EXP S.A.\nEstablecimiento\nOficial N° 4407")
                        .SetFont(boldFont)
                        .SetFontSize(fontSize))
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                    .SetBorder(new SolidBorder(1)); // Borde sólido de 1 punto

                Cell cell2 = new Cell(1, 1)
                    .Add(new Paragraph("SISTEMA DE GESTION DE CALIDAD E INOCUIDAD")
                        .SetFont(boldFont)
                        .SetFontSize(fontSize))
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                    .SetBorder(new SolidBorder(1)); // Borde sólido de 1 punto

                Cell cell3 = new Cell(1, 1)
                    .Add(new Paragraph("REG-MAQ-002\nEmisión: 01-01-2024")
                        .SetFont(boldFont)
                        .SetFontSize(fontSize))
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                    .SetBorder(new SolidBorder(1)); // Borde sólido de 1 punto

                Cell cell4 = new Cell(1, 1)
                    .Add(new Paragraph("TERMORREGISTROS")
                        .SetFont(boldFont)
                        .SetFontSize(fontSize))
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                    .SetBorder(new SolidBorder(1)); // Borde sólido de 1 punto

                Cell cell5 = new Cell(1, 1)
                    .Add(new Paragraph($"Revisión 02-24\nPágina {pageNum} de {totalPages}")
                        .SetFont(boldFont)
                        .SetFontSize(fontSize))
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                    .SetBorder(new SolidBorder(1)); // Borde sólido de 1 punto

                // Agregar las celdas a la tabla
                table.AddCell(cell1);
                table.AddCell(cell2);
                table.AddCell(cell3);
                table.AddCell(cell4);
                table.AddCell(cell5);

                // Añadir la tabla al Canvas
                Canvas layoutCanvas = new(canvas, page.GetPageSize());
                layoutCanvas.Add(table.SetFixedPosition(1, 36, page.GetPageSize().GetTop() - 110, page.GetPageSize().GetWidth() - 72)); // Ajusta la posición según tus necesidades

                layoutCanvas.Add(new Paragraph($"REPORTE TEMPERATURAS\t{FechaRegistro:dd/MM/yyyy}").SetFont(boldFont).SetFontSize(fontSize).SetFixedPosition(1, 36, page.GetPageSize().GetTop() - 140, page.GetPageSize().GetWidth() - 72));
                    //doc.Add(new Paragraph($"REPORTE TEMPERATURAS\t{FechaRegistro:dd/MM/yyyy}").SetFont(boldFont).SetFontSize(fontSize));
            }

            // Cerrar el documento
            pdfDoc.Close();

            // Eliminar el archivo original y renombrar el archivo temporal
            File.Delete(rutaPdf);
            File.Move(rutaTemp, rutaPdf);
        }

        public static List<Table> SplitTable(Table table, float availableWidth)
        {
            List<Table> subTables = new List<Table>();
            Table currentTable = new Table(UnitValue.CreatePercentArray(table.GetNumberOfColumns()));
            currentTable.SetWidth(UnitValue.CreatePercentValue(100));

            foreach (IElement element in table.GetChildren())
            {
                if (element is Cell cell)
                {
                    currentTable.AddCell(cell.Clone(true));
                    if (currentTable.GetWidth().GetValue() > availableWidth)
                    {
                        subTables.Add(currentTable);
                        currentTable = new Table(UnitValue.CreatePercentArray(table.GetNumberOfColumns()));
                        currentTable.SetWidth(UnitValue.CreatePercentValue(100));
                    }
                }
            }

            if (currentTable.GetChildren().Count > 0)
            {
                subTables.Add(currentTable);
            }

            return subTables;
        }
    }
}
