using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MSS.API.Common.Utility;
using SZY.Platform.WebApi.Service;
using System;
using Microsoft.Extensions.Hosting;
using MQTTnet.Client.Options;
using SZY.Platform.WebApi.Options;
using SZY.Platform.WebApi.Client;
using Microsoft.Extensions.Configuration;

namespace SZY.Platform.WebApi.Infrastructure
{
    public static class EssentialServiceCollectionExtensions
    {

        private static IServiceCollection AddMqttClientServiceWithConfig(this IServiceCollection services, Action<MosquittoMqttClientOptionBuilder> configure)
        {
            services.AddSingleton<IMqttClientOptions>(serviceProvider =>
            {
                var optionBuilder = new MosquittoMqttClientOptionBuilder(serviceProvider);
                configure(optionBuilder);
                return optionBuilder.Build();
            });
            //services.AddSingleton<MosquittoMqttClientService>();
            //services.AddSingleton<IHostedService>(serviceProvider =>
            //{
            //    ServiceLocator.SetServices(serviceProvider);
            //    return serviceProvider.GetService<MosquittoMqttClientService>();
            //});

            return services;
        }
        public static IServiceCollection AddEssentialService(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddTransient<IAuthHelper, AuthHelper>();
            services.AddTransient<IConstructionPlanService, ConstructionPlanService>();
            services.AddTransient<IConstructionPlanImportService, ConstructionPlanImportService>();
            services.AddTransient<IConstructionPlanMonthDetailService, ConstructionPlanMonthDetailService>();
            services.AddTransient<IWorkTaskService, WorkTaskService>();
            services.AddTransient<IWfprocessService, WfprocessService>();
            services.AddTransient<IJingGai2AlarmService, JingGai2AlarmService>();
            services.AddTransient<IConstructionPlanMonthChartService, ConstructionPlanMonthChartService>();

            services.AddTransient<IMaintenanceService, MaintenanceService>();
            services.AddTransient<IBusAlarmService, BusAlarmService>();
            //services.AddTransient<IBusInfoService, BusInfoService>();
            //services.AddTransient<ISimulationInfoService, SimulationInfoService>();
            //services.AddHostedService<DataTransferService>();
            var builder = new ConfigurationBuilder()
    //.SetBasePath("path here") //<--You would need to set the path
    .AddJsonFile("appsettings.json"); //or what ever file you have the settings

            IConfiguration _configuration = builder.Build();

            //services.AddMqttClientServiceWithConfig(aspOptionBuilder =>
            //{
            //    aspOptionBuilder
            //    .WithCredentials("admin", "public")
            //    .WithClientId("zdh" + Guid.NewGuid().ToString("D"))
            //    .WithTcpServer(_configuration["MQTTSet:Ip"], 1883);
            //});

            return services;
        }
    }

    public static class ServiceLocator
    {
        public static IServiceProvider Services { get; private set; }

        public static void SetServices(IServiceProvider services)
        {
            Services = services;
        }
    }
}
