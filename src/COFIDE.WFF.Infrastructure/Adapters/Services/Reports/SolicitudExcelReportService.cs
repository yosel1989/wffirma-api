using ClosedXML.Excel;
using COFIDE.WFF.Application.Features.Solicitudes.DTOs;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;

namespace COFIDE.WFF.Infrastructure.Adapters.Services.Reports
{
    public sealed class SolicitudExcelReportService : ISolicitudExcelReportService
    {
        public byte[] GenerarReporteSolicitudes(List<SolicitudPaginadoDto> solicitudes)
        {
            using var workbook = new XLWorkbook();

            var worksheet = workbook.Worksheets.Add("Solicitudes");

            CrearCabecera(worksheet);
            LlenarDatos(worksheet, solicitudes);
            AplicarFormato(worksheet, solicitudes.Count);

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);

            return stream.ToArray();
        }

        private static void CrearCabecera(IXLWorksheet worksheet)
        {
            worksheet.Cell(1, 1).Value = "ID Solicitud";
            worksheet.Cell(1, 2).Value = "Código Solicitud";
            worksheet.Cell(1, 3).Value = "Tipo Solicitud";
            worksheet.Cell(1, 4).Value = "Sub Tipo Solicitud";
            worksheet.Cell(1, 5).Value = "Urgente";
            worksheet.Cell(1, 6).Value = "Solicitante";
            worksheet.Cell(1, 7).Value = "Estado";
            worksheet.Cell(1, 8).Value = "Fecha Creación";
            worksheet.Cell(1, 9).Value = "Aprobadores Pendientes";
            worksheet.Cell(1, 10).Value = "Aprobador Actual";
        }

        private static void LlenarDatos(
            IXLWorksheet worksheet,
            List<SolicitudPaginadoDto> solicitudes)
        {
            var row = 1;

            foreach (var item in solicitudes)
            {
                row++;

                worksheet.Cell(row, 1).Value = item.IdSolicitud;
                worksheet.Cell(row, 2).Value = item.CodigoSolicitud ?? string.Empty;
                worksheet.Cell(row, 3).Value = item.TipoSolicitud ?? string.Empty;
                worksheet.Cell(row, 4).Value = item.SubTipoSolicitud ?? string.Empty;
                worksheet.Cell(row, 5).Value = item.Urgente ?? string.Empty;
                worksheet.Cell(row, 6).Value = item.Solicitante ?? string.Empty;
                worksheet.Cell(row, 7).Value = item.Estado ?? string.Empty;

                if (item.FechaCreacion.HasValue)
                {
                    worksheet.Cell(row, 8).Value = item.FechaCreacion.Value;
                    worksheet.Cell(row, 8).Style.DateFormat.Format = "dd/MM/yyyy HH:mm";
                }
                else
                {
                    worksheet.Cell(row, 8).Value = string.Empty;
                }

                worksheet.Cell(row, 9).Value = item.AprobadoresPendientes;
                worksheet.Cell(row, 10).Value = item.AprobadorActual ?? string.Empty;
            }
        }

        private static void AplicarFormato(
            IXLWorksheet worksheet,
            int cantidadRegistros)
        {
            var ultimaFila = cantidadRegistros + 1;
            const int ultimaColumna = 10;

            var headerRange = worksheet.Range(1, 1, 1, ultimaColumna);

            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            if (cantidadRegistros > 0)
            {
                var dataRange = worksheet.Range(2, 1, ultimaFila, ultimaColumna);

                dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                dataRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            }

            worksheet.SheetView.FreezeRows(1);

            var rangeUsed = worksheet.RangeUsed();

            if (rangeUsed is not null)
                rangeUsed.SetAutoFilter();

            worksheet.Columns().AdjustToContents();

            worksheet.Column(1).Width = 15;
            worksheet.Column(2).Width = 25;
            worksheet.Column(3).Width = 25;
            worksheet.Column(4).Width = 25;
            worksheet.Column(5).Width = 15;
            worksheet.Column(6).Width = 35;
            worksheet.Column(7).Width = 20;
            worksheet.Column(8).Width = 22;
            worksheet.Column(9).Width = 25;
            worksheet.Column(10).Width = 35;
        }
    }
}