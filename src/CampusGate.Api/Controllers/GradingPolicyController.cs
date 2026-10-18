// src/CampusGate.Api/Controllers/GradingPolicyController.cs
using CampusGate.Api.Configuration;
using CampusGate.Contracts.Grading;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CampusGate.Api.Controllers;

[ApiController]
[Route("api/v1/grading-policy")]
public sealed class GradingPolicyController(IOptions<GradingOptions> options) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<GradingPolicyDto>(StatusCodes.Status200OK)]
    public ActionResult<GradingPolicyDto> Get() =>
        Ok(new GradingPolicyDto(options.Value.MaxGrade, options.Value.MaxPastDays));
}