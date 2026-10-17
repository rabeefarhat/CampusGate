// src/CampusGate.Api/Models/CourseDto.cs
namespace CampusGate.Api.Models;

public sealed record CourseDto(string Code, string Title, string Faculty, int Capacity);