// src/CampusGate.Api/Stores/InMemoryCourseStore.cs
namespace CampusGate.Api.Stores;

/// <summary>Registered as a SINGLETON: shared by all requests, so every method takes the lock.</summary>
public sealed class InMemoryCourseStore : ICourseStore
{
    private static readonly StoredCourse[] Seed =
    [
        new("CS101", "Introduction to Programming", "science", 120),
        new("MATH201", "Linear Algebra", "science", 80),
        new("HIS110", "European History", "arts", 60),
    ];

    private readonly Lock _gate = new();                                                    // ①
    private readonly Dictionary<string, StoredCourse> _courses = new(StringComparer.OrdinalIgnoreCase); // ②
    private readonly List<StoredEnrolment> _enrolments = [];
    private readonly TimeProvider _time;                                                    // ③

    public InMemoryCourseStore(TimeProvider time)
    {
        _time = time;
        foreach (var course in Seed)
        {
            _courses[course.Code] = course;
        }
    }

    public PagedResult<StoredCourse> List(int page, int pageSize)
    {
        lock (_gate)
        {
            var ordered = _courses.Values.OrderBy(c => c.Code, StringComparer.Ordinal).ToList();
            var skip = (long)(page - 1) * pageSize;                                         // ④
            var items = skip >= ordered.Count
                ? []
                : ordered.Skip((int)skip).Take(pageSize).ToList();
            return new PagedResult<StoredCourse>(items, ordered.Count);
        }
    }

    public StoredCourse? Find(string code)
    {
        lock (_gate)
        {
            return _courses.GetValueOrDefault(code);
        }
    }

    public UpsertResult Upsert(StoredCourse course)
    {
        lock (_gate)
        {
            var existed = _courses.ContainsKey(course.Code);
            _courses[course.Code] = course;
            return existed ? UpsertResult.Updated : UpsertResult.Created;
        }
    }

    public bool Delete(string code)
    {
        lock (_gate)
        {
            if (!_courses.Remove(code, out var removed))
            {
                return false;
            }

            _enrolments.RemoveAll(e => e.CourseCode == removed.Code);
            return true;
        }
    }

    public EnrolOutcome Enrol(string courseCode, string studentNumber)
    {
        lock (_gate)                                                                        // ⑤
        {
            if (!_courses.TryGetValue(courseCode, out var course))
            {
                return new EnrolOutcome(EnrolResult.CourseNotFound, null);
            }

            var inCourse = _enrolments.Where(e => e.CourseCode == course.Code).ToList();

            if (inCourse.Any(e => e.StudentNumber == studentNumber))
            {
                return new EnrolOutcome(EnrolResult.AlreadyEnrolled, null);
            }

            if (inCourse.Count >= course.Capacity)
            {
                return new EnrolOutcome(EnrolResult.CourseFull, null);
            }

            var enrolment = new StoredEnrolment(Guid.NewGuid(), course.Code, studentNumber, _time.GetUtcNow());
            _enrolments.Add(enrolment);
            return new EnrolOutcome(EnrolResult.Enrolled, enrolment);
        }
    }

    public StoredEnrolment? FindEnrolment(string courseCode, Guid enrolmentId)
    {
        lock (_gate)
        {
            return _enrolments.FirstOrDefault(e =>
                e.Id == enrolmentId &&
                string.Equals(e.CourseCode, courseCode, StringComparison.OrdinalIgnoreCase));
        }
    }
}