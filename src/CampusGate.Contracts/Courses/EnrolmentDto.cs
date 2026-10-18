// src/CampusGate.Contracts/Courses/EnrolmentDto.cs
namespace CampusGate.Contracts.Courses;

public sealed record EnrolmentDto(Guid Id, string CourseCode, string StudentNumber, DateTimeOffset EnrolledAt);