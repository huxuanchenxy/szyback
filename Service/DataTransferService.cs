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
using SZY.Platform.WebApi.Data;
using SZY.Platform.WebApi.Helper;

namespace SZY.Platform.WebApi.Service
{
    public class DataTransferService : IHostedService, IDisposable
    {
        private readonly Logger _logger;
        private readonly IConfiguration _configuration;
        //private static volatile bool isWork = false;
        //private static volatile bool isTransfer = false;
        private static object obj = new Object();
        //private static ConcurrentQueue<JObject> ItemsBackup = new ConcurrentQueue<JObject>();
        private static ConcurrentQueue<JObject> ItemsNow = new ConcurrentQueue<JObject>();
        private static MosquittoMqttClientService _mqttservice;
        private readonly IG40InfoRepo<G40Info> _repo;
        private static Timer _timer;
        private static int collectingTime = 30;
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
        public DataTransferService(IConfiguration configuration, IG40InfoRepo<G40Info> repo)
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
            _repo = repo;
            collectingTime = int.Parse(_configuration["MQTTSet:CarCountTimeSpan"]);
        }

        public Task StartAsync(System.Threading.CancellationToken cancellationToken)
        {
            _mqttservice = ServiceLocator.Services.GetService<MosquittoMqttClientService>();
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(collectingTime));

            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            _logger.Warning("开始计算车流量");
            _logger.Warning("队列里的数据:");
            //_logger.Warning(JsonConvert.SerializeObject(ItemsNow));
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

                Dictionary<string, List<carinfo>> dic = new Dictionary<string, List<carinfo>>();//摄像头,车数组，为了合并同一个摄像头的车辆数据，以便去重用
                Dictionary<string, transportresult> dic2 = new Dictionary<string, transportresult>();//存摄像头到 roadpart 和 direction的对应，并且存当前摄像头的3分钟之内的车的数量
                foreach (var item in tempItems)
                {
                    try
                    {
                        var curobj = JsonConvert.DeserializeObject<transportret>(item.ToString());
                        //_logger.Warning("当前行:");
                        //_logger.Warning(JsonConvert.SerializeObject(curobj));


                        //先去重，去除3分钟之内重复数的车辆
                        if (curobj != null && curobj.result != null && curobj.result.carinfo != null)
                        {
                            for (int k = 0; k < curobj.result.carinfo.Count; k++)
                            {
                                var thisrowcarid = curobj.result.carinfo[k].carid == null ? "" : curobj.result.carinfo[k].carid;
                                curobj.result.carinfo[k].caridnum = "cc" + curobj.result.carinfo[k].num + thisrowcarid;
                            }
                            curobj.time = AliyunHelper.GetDateTimeMilliseconds(long.Parse(curobj.timestamp));
                            _logger.Warning(JsonConvert.SerializeObject(curobj));
                            var roadpart = curobj.result.roadpart;
                            var camera = curobj.result.camera;
                            if (!dic.ContainsKey(camera))
                            {
                                dic.Add(camera, curobj.result.carinfo);
                            }
                            else
                            {
                                var prelist = dic[camera];
                                dic[camera] = prelist.Concat<carinfo>(curobj.result.carinfo).Distinct(new carinfoComparer()).ToList();
                            }
                            if (!dic2.ContainsKey(camera))
                            {
                                dic2.Add(camera, new transportresult() { direction = curobj.result.direction, roadpart = curobj.result.roadpart });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Warning("当前行有问题:" + ex.ToString());
                    }
                }
                foreach (var dd in dic)
                {
                    var curdata = new G40Info() { Camera = dd.Key, RoadPart = dic2[dd.Key].roadpart, Carcount = dd.Value.Count, Time = DateTime.Now, Timespan = collectingTime };
                    try
                    {
                        _repo.Save(curdata);
                        _logger.Warning("成功插入:" + curdata);
                    }
                    catch (Exception ex)
                    {
                        _logger.Warning("当前行存数据库有问题:" + ex.ToString());
                    }

                    try
                    {
                        _mqttservice.getClient().PublishMessageAsync(_configuration["MQTTSet:CarCountAdd"] + "/" + _configuration["MQTTSet:CarCountAddCamera"], JsonConvert.SerializeObject(curdata), false, 0);
                        _logger.Warning("已成功发送每3分钟mqtt:" + _configuration["MQTTSet:CarCountAdd"] + "/" + _configuration["MQTTSet:CarCountAddCamera"] + " 内容是:" + JsonConvert.SerializeObject(curdata));
                    }
                    catch (Exception ex)
                    {
                        _logger.Warning("当前行发送每3分钟到mqtt" + _configuration["MQTTSet:CarCountAdd"] + "/" + _configuration["MQTTSet:CarCountAddCamera"] + "有问题:" + ex.ToString());
                    }

                    try
                    {

                        var curtime = _configuration["MQTTSet:CarCountAddLastHourDate"];
                        var endtime = curtime == null ? DateTime.Now : Convert.ToDateTime(curtime + " " + DateTime.Now.ToString("HH:mm:ss"));
                        var starttime = endtime.AddHours(-1);
                        _logger.Warning("计算范围 starttime :" + starttime + " endtime:" + endtime);
                        var sqlret = _repo.GetLastHour(new G40InfoParm() { StartTime = starttime, endTime = endtime, camera = dd.Key });
                        var lasthourcarscount = 0;
                        if (sqlret != null && sqlret.Result != null && sqlret.Result.rows != null)
                        {
                            var cursqlrow = sqlret.Result.rows[0];
                            lasthourcarscount = cursqlrow.Carcount;
                        }
                        var curdata1 = new G40Info() { Camera = dd.Key, RoadPart = dic2[dd.Key].roadpart, Carcount = lasthourcarscount, Time = DateTime.Now, Timespan = collectingTime };
                        _logger.Warning("已成功发送过去1小时mqtt:" + _configuration["MQTTSet:CarCountAddLastHour"] + "/" + _configuration["MQTTSet:CarCountAddCamera"] + " 内容是:" + JsonConvert.SerializeObject(curdata1));
                    }
                    catch (Exception ex)
                    {
                        _logger.Warning("当前行发送到过去1小时mqtt" + _configuration["MQTTSet:CarCountAddLastHour"] + "/" + _configuration["MQTTSet:CarCountAddCamera"] + "有问题:" + ex.ToString());
                    }

                    try
                    {

                        var curtime = _configuration["MQTTSet:CarCountAddLastHourDate"];
                        var endtime = curtime == null ? DateTime.Now : Convert.ToDateTime(curtime + " " + DateTime.Now.ToString("HH:mm:ss"));
                        var starttime = Convert.ToDateTime(endtime.ToString("yyyy-MM-dd 00:00:00"));
                        _logger.Warning("计算范围 starttime :" + starttime + " endtime:" + endtime);
                        var sqlret = _repo.GetLastHour(new G40InfoParm() { StartTime = starttime, endTime = endtime, camera = dd.Key });
                        var lasthourcarscount = 0;
                        if (sqlret != null && sqlret.Result != null && sqlret.Result.rows != null)
                        {
                            var cursqlrow = sqlret.Result.rows[0];
                            lasthourcarscount = cursqlrow.Carcount;
                        }
                        var curdata1 = new G40Info() { Camera = dd.Key, RoadPart = dic2[dd.Key].roadpart, Carcount = lasthourcarscount, Time = DateTime.Now, Timespan = collectingTime };
                        _logger.Warning("已成功发送从0点累计到现在mqtt:" + _configuration["MQTTSet:CarCountAddToday"] + "/" + _configuration["MQTTSet:CarCountAddCamera"] + " 内容是:" + JsonConvert.SerializeObject(curdata1));
                    }
                    catch (Exception ex)
                    {
                        _logger.Warning("当前行发送从0点累计到现在mqtt" + _configuration["MQTTSet:CarCountAddToday"] + "/" + _configuration["MQTTSet:CarCountAddCamera"] + "有问题:" + ex.ToString());
                    }
                }


                //string str = JsonConvert.SerializeObject(ItemsNow);
                //

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
            }



        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("StopAsync");
            _logger.Warning("StopAsync");
            return Task.CompletedTask;
        }


        public void Dispose()
        {
            _timer.Dispose();
        }

    }

    public class carinfoComparer : IEqualityComparer<carinfo>
    {
        public bool Equals(carinfo x, carinfo y)
        {
            // Check if the carid of both objects are equal
            //return x.carid == y.carid;
            return x.caridnum == y.caridnum;
        }

        public int GetHashCode(carinfo car)
        {
            // Use the carid's hash code for determining the hash code
            return car.caridnum.GetHashCode();
        }
    }

}
