// src/CampusGate.Api/Stores/ICourseStore.cs
namespace CampusGate.Api.Stores;

public interface ICourseStore
{
    PagedResult<StoredCourse> List(int page, int pageSize);
    StoredCourse? Find(string code);
    UpsertResult Upsert(StoredCourse course);
    bool Delete(string code);
    EnrolOutcome Enrol(string courseCode, string studentNumber);
    StoredEnrolment? FindEnrolment(string courseCode, Guid enrolmentId);
}