using Microsoft.AspNetCore.Http;

namespace Vestigium.Models.ViewModels
{
    public class BulkBomImportViewModel
    {
        public int ProductId { get; set; }

        public string ProductNumber { get; set; } =
            string.Empty;


        // Entrada

        public string? BomText { get; set; }

        public IFormFile? File { get; set; }


        // Configuracion

        public bool UpdateExisting { get; set; }

        // Seguridad

        public bool RequiresPassword { get; set; }
        public string? Password { get; set; }

        // Resultados

        public int TotalLines { get; set; }

        public int AddedCount { get; set; }

        public int UpdatedCount { get; set; }

        public int DuplicateCount { get; set; }

        public int InvalidCount { get; set; }

        public List<string> NotFound { get; set; } =
            new();
    }
}