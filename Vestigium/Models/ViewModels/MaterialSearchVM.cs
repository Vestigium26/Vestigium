namespace Vestigium.Models.ViewModels
{
    public class MaterialSearchVM
    {
        // ==========================================
        // FILTROS
        // ==========================================

        public string? SearchText { get; set; }

        public string? Rack { get; set; }

        public string? PartNumber { get; set; }

        public int? ProductId { get; set; }

        public string? QuantityOrder { get; set; }


        // ==========================================
        // RESULTADOS
        // ==========================================

        public List<MaterialInventoryItemVM> Materials { get; set; }
            = new();


        // ==========================================
        // PAGINACIÓN
        // ==========================================

        public int CurrentPage { get; set; }

        public int TotalPages { get; set; }

        public int TotalRecords { get; set; }

        public int PageSize { get; set; } = 50;


        // ==========================================
        // INDICADORES
        // ==========================================

        public int GlobalMaterials { get; set; }

        public int TotalInventory { get; set; }

        public int CriticalMaterials { get; set; }

        public int CurrentPageInventory { get; set; }


        public double CurrentPagePercentage =>
            TotalInventory == 0
                ? 0
                : (CurrentPageInventory * 100.0) /
                  TotalInventory;
    }
}