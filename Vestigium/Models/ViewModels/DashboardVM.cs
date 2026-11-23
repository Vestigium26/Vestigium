using Vestigium.Models;

namespace Vestigium.Models.ViewModels
{
    public class DashboardVM
    {
        public string FullName { get; set; } = string.Empty;

        public string UserName { get; set; } = string.Empty;

        public DateTime CurrentDateTime { get; set; }

        public int TotalMaterials { get; set; }

        public int TotalInventory { get; set; }

        public int ScrapQuantity { get; set; }

        public int MrbQuantity { get; set; }

        public int TotalEntries { get; set; }

        public int TotalTransfers { get; set; }

        public int TotalScrap { get; set; }

        public int TotalMrb { get; set; }

        //Nuevo
        public string Area { get; set; } = string.Empty;
        public int LowInventoryCount { get; set; }

        public List<Movement> RecentMovements { get; set; } = new();


        public List<Material> LowInventoryMaterials { get; set; }
            = new();

        public Movement? LastMovement { get; set; }

        public int ActiveLocations { get; set; }
    }
}