using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TrafficControlService.Actors;
using TrafficControlService.DomainServices;
using TrafficControlService.Repositories;

namespace TrafficControlService
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ISpeedingViolationCalculator>(
                new DefaultSpeedingViolationCalculator("A12", 10, 100, 5));

            services.AddSingleton<IVehicleStateRepository, DaprVehicleStateRepository>();

            var daprGrpcPort = Environment.GetEnvironmentVariable("DAPR_GRPC_PORT") ?? "50001";
            var daprHttpPort = Environment.GetEnvironmentVariable("DAPR_HTTP_PORT") ?? "3500";
            services.AddDaprClient(
                builder => builder
                    .UseGrpcEndpoint($"http://localhost:{daprGrpcPort}")
                    .UseHttpEndpoint($"http://localhost:{daprHttpPort}"));

            services.AddControllers();

            services.AddActors(options =>
            {
                options.Actors.RegisterActor<VehicleActor>();
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapActorsHandlers();
            });
        }
    }
}
