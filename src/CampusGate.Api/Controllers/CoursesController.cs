// src/CampusGate.Api/Controllers/CoursesController.cs
using CampusGate.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace CampusGate.Api.Controllers;

[ApiController]                                                    // ①
[Route("api/courses")]                                             // ②
public sealed class CoursesController : ControllerBase             // ③
{
    private static readonly IReadOnlyList<CourseDto> Courses =     // ④
    [
        new("CS101", "Introduction to Programming", "science", 120),
        new("MATH201", "Linear Algebra", "science", 80),
        new("HIS110", "European History", "arts", 60),
    ];

    [HttpGet]                                                      // ⑤
    [ProducesResponseType<IReadOnlyList<CourseDto>>(StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<CourseDto>> GetAll() => Ok(Courses);

    [HttpGet("{code}")]                                            // ⑥
    [ProducesResponseType<CourseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<CourseDto> GetByCode(string code)
    {
        var course = Courses.FirstOrDefault(c =>
            string.Equals(c.Code, code, StringComparison.OrdinalIgnoreCase)); // ⑦
        return course is null ? NotFound() : Ok(course);                     // ⑧
    }
}