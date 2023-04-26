var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IVehicleInfoRepository, InMemoryVehicleInfoRepository>();
builder.Services.AddControllers().AddDapr();
builder.Services.AddHealthChecks();

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
