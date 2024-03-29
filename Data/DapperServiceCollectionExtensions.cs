﻿using Dapper.FluentMap;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SZY.Platform.WebApi.Model;
using System;

namespace SZY.Platform.WebApi.Data
{
    public static class DapperServiceCollectionExtensions
    {
        public static IServiceCollection AddDapper(this IServiceCollection services, IConfiguration configuration)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            var optionsSection = configuration.GetSection("Dapper");
            var options = new DapperOptions();
            optionsSection.Bind(options);
            services.AddSingleton<DapperOptions>(options);

            services.AddTransient<IConstructionPlanRepo<ConstructionPlan>, ConstructionPlanRepo>();
            services.AddTransient<IConstructionPlanImportRepo<ConstructionPlanYear>, ConstructionPlanImportRepo>();
            services.AddTransient<IConstructionPlanMonthDetailRepo<ConstructionPlanMonthDetail>, ConstructionPlanMonthDetailRepo>();
            services.AddTransient<IWorkTaskRepo<TaskViewModel>, WorkTaskRepo>();
            services.AddTransient<IWfprocessRepo<Wfprocess>, WfprocessRepo>();
            services.AddTransient<IConstructionPlanMonthChartRepo<ConstructionPlanMonthChart>, ConstructionPlanMonthChartRepo>();
            services.AddTransient<IMaintenanceRepo<MaintenanceItem>, MaintenanceRepo>();
            services.AddTransient<ITransportCameraRepo<TransportCarCameraToTunnel>, TransportCameraRepo>();
            services.AddTransient<IJingGai2AlarmRepo<JingGai2Alarm>, JingGai2AlarmRepo>();
            services.AddTransient<IBusAlarmRepo<BusAlarm>, BusAlarmRepo>();
            services.AddTransient<ISimulationInfoRepo<SimulationInfo>, SimulationInfoRepo>();
            services.AddTransient<ISimulationInfoSumRepo<SimulationInfoSum>, SimulationInfoSumRepo>();
            services.AddTransient<IG40InfoRepo<G40Info>, G40InfoRepo>();


            //配置列名映射
            FluentMapper.Initialize(config =>
            {
                config.AddMap(new ConstructionPlanMap());
                config.AddMap(new ConstructionPlanYearMap());
                config.AddMap(new ConstructionPlanMonthMap());
                config.AddMap(new ConstructionPlanMonthDetailMap());
                config.AddMap(new ConstructionPlanImportCommonMap());
                config.AddMap(new WfprocessMap());
                config.AddMap(new ConstructionPlanMonthChartMap());

                config.AddMap(new MaintenanceItemMap());
                config.AddMap(new MaintenanceModuleItemMap());
                config.AddMap(new MaintenanceModuleItemValueMap());
                config.AddMap(new MaintenanceModuleMap());
                config.AddMap(new MaintenanceListMap());
                config.AddMap(new MaintenancePlanDetailMap());
                config.AddMap(new MaintenanceModuleItemAllMap());

                config.AddMap(new PMModuleMap());
                config.AddMap(new PMEntityMap()); 
                config.AddMap(new PMEntityMonthDetailMap());
                config.AddMap(new EqpHistoryMap());
                config.AddMap(new JingGaiMap());
                config.AddMap(new JingGaiDeviceMap());
                config.AddMap(new JingGaiAlarmMap());

                config.AddMap(new TransportCarCameraToTunnelMap());
                config.AddMap(new JingGai2AlarmMap());
                config.AddMap(new BusAlarmMap());
                config.AddMap(new JingGai2Map());
                config.AddMap(new SimulationInfoMap());
                config.AddMap(new SimulationInfoSumMap());
                config.AddMap(new G40InfoMap());


            });
            return services;
        }
    }
}
