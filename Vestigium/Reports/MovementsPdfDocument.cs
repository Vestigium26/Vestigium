using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Vestigium.Models.Enums;
using Vestigium.ReportModels;

namespace Vestigium.Reports;

public class MovementsPdfDocument : IDocument
{
    private readonly MovementReportVM _model;
    private readonly string _logoPath;

    public MovementsPdfDocument(
        MovementReportVM model,
        string logoPath)
    {
        _model = model;
        _logoPath = logoPath;
    }

    public DocumentMetadata GetMetadata()
    {
        return DocumentMetadata.Default;
    }

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4.Landscape());

            page.Margin(20);

            page.DefaultTextStyle(x =>
                x.FontSize(9));

            // HEADER

            page.Header()
                .BorderBottom(1)
                .BorderColor("#DEE2E6")
                .PaddingBottom(10)
                .Row(row =>
                {
                    row.ConstantItem(280)
    .Height(80)
    .AlignMiddle()
    .Image(_logoPath)
    .FitHeight();
                    /*row.ConstantItem(220)
                        .AlignMiddle()
                        .Text("REICHERT");*/

                    // Cuando quieras volver a usar el logo:
                    /*
                    row.ConstantItem(220)
                        .Image(_logoPath)
                        .FitArea();
                    */

                    row.RelativeItem()
                        .Column(col =>
                        {
                            col.Item()
                                .Text("VESTIGIUM")
                                .FontSize(22)
                                .Bold();

                            col.Item()
                                .Text("Reporte de Movimientos de Inventario")
                                .FontSize(14);

                            col.Item()
                .Text(
        _model.EndDate.HasValue
            ? $"Período consultado: {_model.Date:dd/MM/yyyy} - {_model.EndDate.Value:dd/MM/yyyy}"
            : $"Fecha consultada: {_model.Date:dd/MM/yyyy}");

                        });
                });

            // CONTENIDO

            page.Content()
                .PaddingVertical(10)
                .Column(col =>
                {
                    // DATOS DEL REPORTE

                    col.Item()
                        .Background("#F8F9FA")
                        .Padding(10)
                        .Column(info =>
                        {
                            info.Item()
                                .Text(
                                    $"Generado por: {_model.GeneratedBy}");

                            info.Item()
                                .Text(
                                    $"Fecha de emisión: {_model.GeneratedAt:dd/MM/yyyy HH:mm:ss}");
                        });

                    col.Item()
                        .PaddingTop(10);

                    // KPI

                    col.Item()
                        .Background("#F8F9FA")
                        .Padding(10)
                        .Row(row =>
                        {
                            row.RelativeItem()
                                .Background("#FFFFFF")
                                .Padding(8)
                                .Text(
                                    $"Movimientos: {_model.TotalMovements}");

                            row.RelativeItem()
                                .Background("#FFFFFF")
                                .Padding(8)
                                .Text(
                                    $"Entradas: {_model.TotalEntries}");

                            row.RelativeItem()
                                .Background("#FFFFFF")
                                .Padding(8)
                                .Text(
                                    $"Transferencias: {_model.TotalTransfers}");

                            row.RelativeItem()
                                .Background("#FFFFFF")
                                .Padding(8)
                                .Text(
                                    $"Salidas: {_model.TotalOutputs}");

                            row.RelativeItem()
                                .Background("#FFFFFF")
                                .Padding(8)
                                .Text(
                                    $"Piezas Movidas: {_model.TotalMovedPieces}");
                        });

                    col.Item()
                        .PaddingTop(15);

                    // TABLA

                    col.Item()
                        .Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2);     // Fecha
                                columns.RelativeColumn(2);     // Folio
                                columns.RelativeColumn(2);     // Parte
                                columns.RelativeColumn(3);     // Descripción
                                columns.RelativeColumn(1.5f);  // Tipo
                                columns.RelativeColumn(1);     // Cantidad
                                columns.RelativeColumn(2);     // Origen
                                columns.RelativeColumn(2);     // Destino
                                columns.RelativeColumn(2);     // Registró
                                columns.RelativeColumn(2);     // Recibió
                                columns.RelativeColumn(3);     // Comentarios
                            });

                            // HEADER TABLA

                            table.Header(header =>
                            {
                                string headerColor = "#0D6EFD";

                                void Header(string title)
                                {
                                    header.Cell()
                                        .Background(headerColor)
                                        .PaddingVertical(6)
                                        .PaddingHorizontal(5)
                                        .Text(title)
                                        .FontColor(Colors.White)
                                        .SemiBold();
                                }

                                Header("Fecha/Hora");
                                Header("Folio");
                                Header("No. Parte");
                                Header("Descripción");
                                Header("Tipo");
                                Header("Cantidad");
                                Header("Origen");
                                Header("Destino");
                                Header("Registró");
                                Header("Recibió");
                                Header("Comentarios");
                            });

                            bool alternate = false;

                            foreach (var item in _model.Movements)
                            {
                                string rowColor =
                                    alternate
                                        ? "#F8F9FA"
                                        : "#FFFFFF";

                                alternate = !alternate;

                                void Cell(string value)
                                {
                                    table.Cell()
                                        .Background(rowColor)
                                        .PaddingVertical(4)
                                        .PaddingHorizontal(4)
                                        .BorderBottom(0.5f)
                                        .BorderColor("#E9ECEF")
                                        .Text(value);
                                }

                                Cell(
                                    item.MovementDate
                                        .ToString("dd/MM/yyyy HH:mm"));

                                Cell(
                                    item.ReportNumber);

                                Cell(
                                    item.Material.PartNumber);

                                Cell(
                                    item.Material.Description);

                                Cell(
                                    item.MovementType switch
                                    {
                                        MovementType.Entry =>
                                            "Entrada",

                                        MovementType.Transfer =>
                                            "Transferencia",

                                        _ =>
                                            "Salida"
                                    });

                                Cell(
                                    item.Quantity.ToString());

                                Cell(
                                    item.OriginLocation);

                                Cell(
                                    item.DestinationLocation);

                                Cell(
                                    item.RegisteredByName ?? "-");

                                Cell(
                                    item.ReceivedByName ?? "-");

                                Cell(
                                    item.Comments ?? "-");
                            }
                        });
                });

            // FOOTER

            page.Footer()
                .BorderTop(1)
                .BorderColor("#DEE2E6")
                .PaddingTop(5)
                .Row(row =>
                {
                    row.RelativeItem()
                        .Text(
                            "Vestigium Inventory Management System | Documento de uso interno");

                    row.ConstantItem(100)
                        .AlignRight()
                        .Text(text =>
                        {
                            text.CurrentPageNumber();

                            text.Span(" / ");

                            text.TotalPages();
                        });
                });
        });
    }
}