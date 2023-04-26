var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ISpeedingViolationCalculator>(new SpeedingViolationCalculator("A12", 10, 100, 5));
builder.Services.AddControllers().AddDapr();
builder.Services.AddHealthChecks();
builder.Services.AddDaprClient();

builder.Logging.AddConsole();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
app.UseCloudEvents();

app.MapControllers();
app.MapHealthChecks("/healthz");

app.Run();
