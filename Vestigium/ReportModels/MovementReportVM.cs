using Vestigium.Models;

namespace Vestigium.ReportModels;

public class MovementReportVM
{
    public DateTime Date { get; set; }

    public DateTime? EndDate { get; set; }

    public string GeneratedBy { get; set; } = string.Empty;

    public DateTime GeneratedAt { get; set; }

    public int TotalMovements { get; set; }

    public int TotalEntries { get; set; }

    public int TotalTransfers { get; set; }

    public int TotalOutputs { get; set; }

    public List<Movement> Movements { get; set; } = new();

    public int TotalMovedPieces { get; set; }
}
