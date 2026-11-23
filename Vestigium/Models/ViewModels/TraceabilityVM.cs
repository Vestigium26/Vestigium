using Vestigium.Models;

namespace Vestigium.Models.ViewModels
{
    public class TraceabilityVM
    {
        public int MaterialId { get; set; }


        public string PartNumber { get; set; }
            = string.Empty;


        public string Description { get; set; }
            = string.Empty;


        public int CurrentInventory { get; set; }


        public string Rack { get; set; }
            = string.Empty;


        public List<Movement> Movements { get; set; }
            = new();


        public List<MaterialLocation> Locations { get; set; }
            = new();
    }
}