using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using SZY.Platform.WebApi.Controllers;
using SZY.Platform.WebApi.Infrastructure;
using SZY.Platform.WebApi.Model;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Options;
using Serilog.Core;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace SZY.Platform.WebApi.Service
{
    public class DataTransferService: IHostedService, IDisposable
    {
        private readonly Logger _logger;
        private readonly IConfiguration _configuration;
        //private static volatile bool isWork = false;
        //private static volatile bool isTransfer = false;
        private static object obj = new Object();
        //private static ConcurrentQueue<JObject> ItemsBackup = new ConcurrentQueue<JObject>();
        private static ConcurrentQueue<JObject> ItemsNow = new ConcurrentQueue<JObject>();
        //private static Dictionary<string, List<BusRoot>> TempItems1 = new Dictionary<string, List<BusRoot>>();
        //private static Dictionary<string, List<BusRoot>> TempItems2 = new Dictionary<string, List<BusRoot>>();
        //public static Dictionary<string, RoadPart> RoadCfg = new Dictionary<string, RoadPart>();
        //public static Dictionary<string, List<RoadPartLane>> RoadCfg;// = new Dictionary<string, List<RoadPartLane>>();
        //public static Dictionary<string, List<MarkArea>> MarkAreas; 
        private static MosquittoMqttClientService _mqttservice;
        //private static IBusInfoService _busInfoService;
        //private static SimulationConfig scfg;
        private static Timer _timer;
        private static int collectingTime = 20;
        public static int GetCollectingTime() { return collectingTime; }
        public static bool EnqueueTask(JObject task) 
        {
            lock (obj) 
            {
                 ItemsNow.Enqueue(task);
            }
            return true;
        }
        //public DataTransferService(IOptions<SimulationConfig> cfgOption)
        public DataTransferService(IConfiguration configuration)
        {
            _logger = new LoggerConfiguration()
#if DEBUG
        .MinimumLevel.Debug()
#else
        .MinimumLevel.Information()
#endif
        //.MinimumLevel.Override("Microsoft", LogEventLevel.)
        //.Enrich.FromLogContext()
        .WriteTo.File(@"c:\\SZYLogs\DataTransferService.txt", rollingInterval: RollingInterval.Day)
        .CreateLogger();
            _configuration = configuration;
        }

        public Task StartAsync(System.Threading.CancellationToken cancellationToken)
        {
            //_mqttservice = ServiceLocator.Services.GetService<MosquittoMqttClientService>();
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(collectingTime));

            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            _logger.Warning("开始计算车流量");
            _logger.Warning("队列里的数据:");
            _logger.Warning(JsonConvert.SerializeObject(ItemsNow));
            if (ItemsNow.Count == 0)
                return;
            
            JObject[] tempItems;
            try
            {
                lock (obj)
                {
                    tempItems = ItemsNow.ToArray();
                    ItemsNow.Clear();
                }
                if (tempItems.Length == 0)
                    return;
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
            }



        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("StopAsync");

            return Task.CompletedTask;
        }


        public void Dispose()
        {
            _timer.Dispose();
        }

    }
    
}
