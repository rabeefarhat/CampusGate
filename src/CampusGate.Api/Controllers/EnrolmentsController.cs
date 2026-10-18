// src/CampusGate.Api/Controllers/EnrolmentsController.cs
using System.ComponentModel.DataAnnotations;
using CampusGate.Api.Stores;
using CampusGate.Contracts.Courses;
using Microsoft.AspNetCore.Mvc;

namespace CampusGate.Api.Controllers;

[ApiController]
[Route("api/v1/courses/{code}/enrolments")]                                                 // ①
public sealed class EnrolmentsController(ICourseStore store, ILogger<EnrolmentsController> logger) : ControllerBase
{
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType<EnrolmentDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public ActionResult<EnrolmentDto> Create(
        [FromRoute, RegularExpression(CourseCodeFormat.Pattern)] string code,
        [FromBody] CreateEnrolmentRequest request)
    {
        var outcome = store.Enrol(code, request.StudentNumber);

        switch (outcome.Result)                                                             // ②
        {
            case EnrolResult.CourseNotFound:
                return NotFound();
            case EnrolResult.AlreadyEnrolled:
                return ConflictProblem("already_enrolled", "The student is already enrolled in this course.");
            case EnrolResult.CourseFull:
                return ConflictProblem("course_full", "The course has reached its capacity.");
        }

        var enrolment = outcome.Enrolment!;                                                 // ③
        logger.LogInformation("Enrolment {EnrolmentId} created in course {CourseCode}",      // ④
            enrolment.Id, enrolment.CourseCode);

        return CreatedAtAction(nameof(GetById),
            new { code = enrolment.CourseCode, id = enrolment.Id }, ToDto(enrolment));
    }

    [HttpGet("{id:guid}")]                                                                  // ⑤
    [ProducesResponseType<EnrolmentDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public ActionResult<EnrolmentDto> GetById(
        [FromRoute, RegularExpression(CourseCodeFormat.Pattern)] string code,
        [FromRoute] Guid id)
    {
        var enrolment = store.FindEnrolment(code, id);
        return enrolment is null ? NotFound() : Ok(ToDto(enrolment));
    }

    private ObjectResult ConflictProblem(string errorCode, string title)
    {
        var problem = ProblemDetailsFactory.CreateProblemDetails(                           // ⑥
            HttpContext,
            statusCode: StatusCodes.Status409Conflict,
            title: title,
            type: $"https://campusgate.example/problems/{errorCode.Replace('_', '-')}");
        problem.Extensions["errorCode"] = errorCode;

        return new ObjectResult(problem)
        {
            StatusCode = StatusCodes.Status409Conflict,
            ContentTypes = { "application/problem+json" }
        };
    }

    private static EnrolmentDto ToDto(StoredEnrolment e) => new(e.Id, e.CourseCode, e.StudentNumber, e.EnrolledAt);
}