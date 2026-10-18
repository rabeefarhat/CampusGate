// src/CampusGate.Api/Controllers/DiagnosticsController.cs
using Microsoft.AspNetCore.Mvc;

namespace CampusGate.Api.Controllers;

/// <summary>⚠️ DEV ONLY: lets us trigger an unhandled exception. Returns 404 in every other environment.</summary>
[ApiController]
[Route("api/v1/diagnostics")]
[ApiExplorerSettings(IgnoreApi = true)]
public sealed class DiagnosticsController(IHostEnvironment environment) : ControllerBase
{
    [HttpGet("boom")]
    public IActionResult Boom()
    {
        if (!environment.IsDevelopment())
        {
            return NotFound();
        }

        throw new InvalidOperationException(
            "Simulated failure with internal detail: table dbo.Enrolments on server sql-internal-01.");
    }
}