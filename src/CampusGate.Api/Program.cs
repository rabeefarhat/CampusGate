// src/CampusGate.Api/Program.cs  (composition root)
using System.Diagnostics;
using CampusGate.Api.Configuration;
using CampusGate.Api.ErrorHandling;
using CampusGate.Api.Middleware;
using CampusGate.Api.Stores;

var builder = WebApplication.CreateBuilder(args);

// ---------- Services (DI registrations) ----------
builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddProblemDetails(options =>                                               // ①
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Extensions["traceId"] =
            Activity.Current?.Id ?? context.HttpContext.TraceIdentifier;
        if (context.HttpContext.Items.TryGetValue(CorrelationIdMiddleware.ItemKey, out var correlationId))
        {
            context.ProblemDetails.Extensions["correlationId"] = correlationId;
        }
    });
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();                             // ②

builder.Services.AddOptions<GradingOptions>()                                               // ③
    .BindConfiguration(GradingOptions.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddSingleton(TimeProvider.System);                                         // ④
builder.Services.AddSingleton<ICourseStore, InMemoryCourseStore>();                         // ⑤
builder.Services.AddHealthChecks();

var app = builder.Build();

// ---------- Pipeline (ORDER IS BEHAVIOUR) ----------
app.UseMiddleware<CorrelationIdMiddleware>();   // ⑥ first: every response, even a 500, gets the id
app.UseExceptionHandler();                      // ⑦ catches anything thrown further in
app.UseStatusCodePages();                       // ⑧ empty 4xx/5xx bodies become ProblemDetails

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();                           // OpenAPI document only in Development
}

app.UseHttpsRedirection();
app.UseAuthorization();                         // slot for L08: UseAuthentication goes right before this

app.MapHealthChecks("/health");                 // ⑨
app.MapControllers();

app.Run();