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

public class WeeklyProductionData
{
    public string? Name { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalCount { get; set; }
    public int TotalQualifiedCount { get; set; }
    public int TotalDefectiveCount { get; set; }
    public double TotalQualifiedRate { get; set; }
}