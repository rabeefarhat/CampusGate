// src/CampusGate.Contracts/Grading/GradingPolicyDto.cs
namespace CampusGate.Contracts.Grading;

public sealed record GradingPolicyDto(int MaxGrade, int MaxPastDays);