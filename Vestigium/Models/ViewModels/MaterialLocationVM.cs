using Vestigium.Models;

namespace Vestigium.Models.ViewModels
{
    public class MaterialLocationVM
    {
        public int MaterialId { get; set; }

        public string PartNumber { get; set; }
            = string.Empty;

        public string Description { get; set; }
            = string.Empty;

        public List<MaterialLocation> Locations { get; set; }
            = new();
    }
}