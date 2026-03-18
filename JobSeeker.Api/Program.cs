using JobSeeker.Application.Contracts;
using JobSeeker.Application.Interfaces;
using JobSeeker.Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:4200", "http://localhost:4000")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
builder.Services.AddJobSearchInfrastructure(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("Frontend");

app.MapGet("/api/health", () => Results.Ok(new { status = "ok" }))
    .WithName("GetHealth")
    .WithOpenApi();

app.MapPost("/api/search", SearchJobsAsync)
    .Accepts<IFormFile>("multipart/form-data")
    .Produces<JobSearchResponse>(StatusCodes.Status200OK)
    .Produces<JobSearchResponse>(StatusCodes.Status400BadRequest)
    .ProducesProblem(StatusCodes.Status500InternalServerError)
    .DisableAntiforgery()
    .WithName("SearchJobs")
    .WithOpenApi();

app.Run();

static async Task<Results<Ok<JobSearchResponse>, BadRequest<JobSearchResponse>, ProblemHttpResult>> SearchJobsAsync(
    IFormFile? resume,
    IJobSearchService jobSearchService,
    ILoggerFactory loggerFactory,
    CancellationToken cancellationToken)
{
    if (resume is null)
    {
        return TypedResults.BadRequest(new JobSearchResponse(
            "error",
            "Please upload a PDF or DOCX resume before starting the search.",
            0,
            0,
            0,
            [],
            []));
    }

    try
    {
        await using var content = resume.OpenReadStream();
        var response = await jobSearchService.SearchAsync(content, resume.FileName, resume.ContentType, cancellationToken);
        return TypedResults.Ok(response);
    }
    catch (InvalidDataException exception)
    {
        return TypedResults.BadRequest(new JobSearchResponse(
            "error",
            exception.Message,
            0,
            0,
            0,
            [],
            []));
    }
    catch (Exception exception)
    {
        loggerFactory.CreateLogger("SearchJobs").LogError(exception, "Job search failed unexpectedly.");
        return TypedResults.Problem("The job search failed unexpectedly. Please try again.");
    }
}
