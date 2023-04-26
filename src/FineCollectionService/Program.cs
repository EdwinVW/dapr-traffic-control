var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IFineCalculator, HardCodedFineCalculator>();
builder.Services.AddSingleton<VehicleRegistrationService>(_ => new VehicleRegistrationService(DaprClient.CreateInvokeHttpClient("vehicleregistrationservice")));
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
app.MapSubscribeHandler();

app.Run();
