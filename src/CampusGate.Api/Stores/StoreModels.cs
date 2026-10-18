// src/CampusGate.Api/Stores/StoreModels.cs
namespace CampusGate.Api.Stores;
public class StoreModels       
{   
    // This class is intentionally left empty to serve as a container for the record types.
}
public sealed record StoredCourse(string Code, string Title, string Faculty, int Capacity);

public sealed record StoredEnrolment(Guid Id, string CourseCode, string StudentNumber, DateTimeOffset EnrolledAt);

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int TotalCount);

public enum UpsertResult { Created, Updated }

public enum EnrolResult { Enrolled, CourseNotFound, AlreadyEnrolled, CourseFull }

public sealed record EnrolOutcome(EnrolResult Result, StoredEnrolment? Enrolment);