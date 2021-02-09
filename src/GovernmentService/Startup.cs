using System;
using System.Collections.Generic;
using Dapr.Client;
using GovernmentService.Helpers;
using GovernmentService.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GovernmentService
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
            RegisterVehicleInfoRepository(services);

            services.AddSingleton<IFineCalculator, HardCodedFineCalculator>();

            services.AddControllers().AddDapr();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseCloudEvents();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapSubscribeHandler();
                endpoints.MapControllers();
            });
        }
        
        private void RegisterVehicleInfoRepository(IServiceCollection services)
        {
            // specify secret-store to use based on hosting environment
            string runningInK8s =
                (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") ?? "false").ToLowerInvariant();
            string secretStoreName = runningInK8s == "true" ? "kubernetes" : "secret-store-file";

            // get connection string
            var daprClient = new DaprClientBuilder().Build();
            var apiKeySecret = daprClient.GetSecretAsync(secretStoreName, "db-connectionstring",
                new Dictionary<string, string> { { "namespace", "dapr-trafficcontrol" } }).Result;
            string connectionstring = apiKeySecret["db-connectionstring"];

            // register repository
            services.AddScoped<IVehicleInfoRepository, InMemoryVehicleInfoRepository>(
                _ => new InMemoryVehicleInfoRepository(connectionstring));
        }        
    }
}
