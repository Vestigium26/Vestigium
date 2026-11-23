namespace Vestigium.Models.ViewModels
{
    public class ScrapMrbInventoryVM
    {
        public string PartNumber { get; set; }
        = string.Empty;

        public string Description { get; set; }
            = string.Empty;

        public int ScrapQuantity { get; set; }

        public int MrbQuantity { get; set; }

        public string? RegisteredByName { get; set; }

        public string? UserName { get; set; }

        public string? ReceivedByName { get; set; }

        public string? ReceivedByEmployeeNumber { get; set; }

        public DateTime? LastMovementDate { get; set; }

        public string? LastMovementBy { get; set; }
    }
}
