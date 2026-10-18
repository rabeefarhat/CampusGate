// src/CampusGate.Contracts/Courses/CourseDto.cs
namespace CampusGate.Contracts.Courses;

public sealed record CourseDto(string Code, string Title, string Faculty, int Capacity);