// src/CampusGate.Api/Configuration/GradingOptions.cs
using System.ComponentModel.DataAnnotations;

namespace CampusGate.Api.Configuration;

public sealed class GradingOptions
{
    public const string SectionName = "Grading";

    [Range(1, 365)]
    public int MaxPastDays { get; init; }

    [Range(1, 100)]
    public int MaxGrade { get; init; }
}