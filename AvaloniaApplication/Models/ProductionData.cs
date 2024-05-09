using System;

namespace AvaloniaApplication.Models;

public class ProductionData
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public int QualifiedCount { get; set; }
    public int DefectiveCount { get; set; }
    public double QualifiedRate { get; set; }
    public int TotalCount { get; set; }
    public DateTime Date { get; set; }
}