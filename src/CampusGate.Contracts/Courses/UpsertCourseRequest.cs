// src/CampusGate.Contracts/Courses/UpsertCourseRequest.cs
using System.ComponentModel.DataAnnotations;

namespace CampusGate.Contracts.Courses;

/// <summary>Body of PUT /api/v1/courses/{code}. The code comes from the URL, never the body.</summary>
public sealed class UpsertCourseRequest
{
    [Required, StringLength(100, MinimumLength = 3)]
    public string Title { get; init; } = string.Empty;

    [Required, RegularExpression("^(science|arts)$")]
    public string Faculty { get; init; } = string.Empty;

    [Range(1, 500)]
    public int Capacity { get; init; }
}