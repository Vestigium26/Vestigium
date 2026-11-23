using Vestigium.Models;
using Vestigium.Models.Enums;

namespace Vestigium.Models.ViewModels
{
    public class MovementHistoryVM
    {
        public string? PartNumber { get; set; }

        public MovementType? MovementType { get; set; }

        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }

        public List<Movement> Movements { get; set; }
            = new();
        public string? EmployeeSearch { get; set; }

        //Paginacion

        public int CurrentPage { get; set; }

        public int TotalPages { get; set; }

        public int TotalRecords { get; set; }

        public int PageSize { get; set; } = 50;

        //KPIs

        public int TotalMovements { get; set; }

        public int TotalEntries { get; set; }

        public int TotalTransfers { get; set; }

        public int TotalOutputs { get; set; }

        public int TotalMovedPieces { get; set; }

    }
}