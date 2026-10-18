// src/CampusGate.Api/Controllers/CoursesController.cs
using System.ComponentModel.DataAnnotations;
using CampusGate.Api.Stores;
using CampusGate.Contracts.Common;
using CampusGate.Contracts.Courses;
using Microsoft.AspNetCore.Mvc;

namespace CampusGate.Api.Controllers;

[ApiController]
[Route("api/v1/courses")]                                                                  // ①
public sealed class CoursesController(ICourseStore store) : ControllerBase
{
    public const int MaxPageSize = 100;

    [HttpGet]
    [ProducesResponseType<PagedResponse<CourseDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public ActionResult<PagedResponse<CourseDto>> List([FromQuery] PageRequest paging)
    {
        var pageSize = Math.Min(paging.PageSize, MaxPageSize);                              // ②
        var result = store.List(paging.Page, pageSize);
        var items = result.Items.Select(ToDto).ToList();
        return Ok(new PagedResponse<CourseDto>(items, paging.Page, pageSize, result.TotalCount));
    }

    [HttpGet("{code}")]
    [ProducesResponseType<CourseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public ActionResult<CourseDto> GetByCode(
        [FromRoute, RegularExpression(CourseCodeFormat.Pattern)] string code)               // ③
    {
        var course = store.Find(code);
        return course is null ? NotFound() : Ok(ToDto(course));
    }

    [HttpPut("{code}")]
    [Consumes("application/json")]                                                          // ④
    [ProducesResponseType<CourseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<CourseDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public ActionResult<CourseDto> Upsert(
        [FromRoute, RegularExpression(CourseCodeFormat.Pattern)] string code,
        [FromBody] UpsertCourseRequest request)
    {
        var course = new StoredCourse(code.ToUpperInvariant(), request.Title, request.Faculty, request.Capacity);
        var result = store.Upsert(course);
        var dto = ToDto(course);

        return result == UpsertResult.Created
            ? CreatedAtAction(nameof(GetByCode), new { code = dto.Code }, dto)              // ⑤
            : Ok(dto);
    }

    [HttpDelete("{code}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public IActionResult Delete([FromRoute, RegularExpression(CourseCodeFormat.Pattern)] string code) =>
        store.Delete(code) ? NoContent() : NotFound();                                      // ⑥

    private static CourseDto ToDto(StoredCourse c) => new(c.Code, c.Title, c.Faculty, c.Capacity);
}