// src/CampusGate.Contracts/Courses/CreateEnrolmentRequest.cs
using System.ComponentModel.DataAnnotations;

namespace CampusGate.Contracts.Courses;

/// <summary>Body of POST /api/v1/courses/{code}/enrolments.</summary>
public sealed class CreateEnrolmentRequest
{
    // Shape check only: 8 digits starting with 2. The Luhn check digit is a domain rule (L04).
    [Required, RegularExpression("^2[0-9]{7}$")]
    public string StudentNumber { get; init; } = string.Empty;
}