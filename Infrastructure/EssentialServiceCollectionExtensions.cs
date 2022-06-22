using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MSS.API.Common.Utility;
using SZY.Platform.WebApi.Service;
using System;
using Microsoft.Extensions.Hosting;
using MQTTnet.Client.Options;
using SZY.Platform.WebApi.Options;

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
            services.AddSingleton<MosquittoMqttClientService>();
            services.AddSingleton<IHostedService>(serviceProvider =>
            {
                return serviceProvider.GetService<MosquittoMqttClientService>();
            });

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
            services.AddTransient<IConstructionPlanMonthChartService, ConstructionPlanMonthChartService>();

            services.AddTransient<IMaintenanceService, MaintenanceService>();


            services.AddMqttClientServiceWithConfig(aspOptionBuilder =>
            {
                aspOptionBuilder
                .WithCredentials("admin", "public")
                .WithClientId("zdh" + Guid.NewGuid().ToString("D"))
                .WithTcpServer("47.101.220.2", 1883);
            });

            services.AddSingleton<MosquittoMqttClientService>();
            services.AddSingleton<IHostedService>(serviceProvider =>
            {
                return serviceProvider.GetService<MosquittoMqttClientService>();
            });
            return services;
        }
    }
}
