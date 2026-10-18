// src/CampusGate.Contracts/Courses/CourseCodeFormat.cs
namespace CampusGate.Contracts.Courses;

/// <summary>Allow-list format for course codes: 2–4 letters + 3 digits (e.g. CS101).</summary>
public static class CourseCodeFormat
{
    public const string Pattern = "^[A-Za-z]{2,4}[0-9]{3}$";
}