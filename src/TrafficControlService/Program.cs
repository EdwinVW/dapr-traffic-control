var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ISpeedingViolationCalculator>(new SpeedingViolationCalculator("A12", 10, 100, 5));
builder.Services.AddControllers().AddDapr();
builder.Services.AddHealthChecks();
builder.Services.AddDaprClient();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder => builder.WithOrigins("http://localhost:5000")
                                               .WithMethods("POST")
                                               .AllowAnyHeader());
});

builder.Logging.AddConsole();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
app.UseCloudEvents();
app.UseCors();

app.MapControllers();
app.MapHealthChecks("/healthz");

app.Run();
