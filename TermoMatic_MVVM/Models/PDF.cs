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
                columnWidths[i] = 0.9f; // Ajusta esto según sea necesario para anchos de columna específicos
            }

            // Crear la tabla de iText7 con el número de columnas adecuado
            Table pdfTable = new Table(UnitValue.CreatePercentArray(columnWidths));
            pdfTable.SetWidth(UnitValue.CreatePercentValue(100));

            // Agregar encabezados de columna a la tabla
            foreach (DataColumn column in dt.Columns)
            {
                Cell headerCell = new Cell()
                    .Add(new Paragraph(column.ColumnName)
                        .SetFontSize(11))
                    .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
                    .SetTextAlignment(TextAlignment.CENTER);
                pdfTable.AddHeaderCell(headerCell);
            }

            // Agregar filas de datos a la tabla
            foreach (DataRow row in dt.Rows)
            {
                foreach (DataColumn column in dt.Columns)
                {
                    Cell cell = new Cell()
                        .Add(new Paragraph(row[column].ToString())
                            .SetFontSize(10))
                        .SetTextAlignment(TextAlignment.LEFT);
                    pdfTable.AddCell(cell);
                }
            }

            return pdfTable;
        }

        public static void CrearPDF(string rutaPdf, Table tabla, DateTime FechaRegistro)
        {
            PdfWriter pdfWriter = new(rutaPdf);
            PdfDocument pdfDocument = new(pdfWriter);
            Document doc = new(pdfDocument);

            doc.SetMargins(144, 36, 36, 36);

            PdfFont boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            float fontSize = 10f;

            doc.Add(new Paragraph($"REPORTE TEMPERATURAS\t{FechaRegistro:dd/MM/yyyy}")
                        .SetFont(boldFont)
                        .SetFontSize(fontSize));

            doc.Add(tabla);

            doc.Close();

            PDF.CrearCabecera(rutaPdf);
        }

        private static void CrearCabecera(string rutaPdf)
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
                    .Add(new Paragraph("OFFAL EXP S.A.\nEstablecimiento Oficial\nN° 4407")
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
                    .Add(new Paragraph("REG-MAQ-002\nEmisión: 12-08-2022")
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
                    .Add(new Paragraph($"Revisión 01\nVersión 2022\nPágina {pageNum} de {totalPages}")
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
            }

            // Cerrar el documento
            pdfDoc.Close();

            // Eliminar el archivo original y renombrar el archivo temporal
            File.Delete(rutaPdf);
            File.Move(rutaTemp, rutaPdf);
        }
    }
}
