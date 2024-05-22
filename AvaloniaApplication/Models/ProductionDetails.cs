using System;

namespace AvaloniaApplication.Models;

public class ProductionDetails
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public int OperatorId { get; set; }
    public int MachineId { get; set; }
    public string? IsQualified { get; set; }
    public DateTime ProductionTime { get; set; }
}