using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Quartz;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SZY.Platform.WebApi.Infrastructure;
using SZY.Platform.WebApi.Model;
using Microsoft.Extensions.DependencyInjection;
using SZY.Platform.WebApi.Helper;

namespace SZY.Platform.WebApi.Service
{
    public class FundJob3 : IJob//创建IJob的实现类，并实现Excute方法。
    {
        private readonly Logger _logger;
        private readonly IConfiguration _configuration;
        private static Timer _timer;
        private static int carspeed;
        private static int times;
        private static int sleep;
        private static object obj = new Object();
        private static MosquittoMqttClientService _mqttservice;
        public FundJob3(IConfiguration configuration)
        {
            _logger = new LoggerConfiguration()
#if DEBUG
        .MinimumLevel.Debug()
#else
        .MinimumLevel.Information()
#endif
        //.MinimumLevel.Override("Microsoft", LogEventLevel.)
        //.Enrich.FromLogContext()
        .WriteTo.RollingFile(@"c:\\SZYLogs\FakeCarJob3.txt")
        .CreateLogger();
            _configuration = configuration;
            carspeed = int.Parse(_configuration["FackCar:carspeed"].ToString());
            times = int.Parse(_configuration["FackCar:times"].ToString());
            sleep = int.Parse(_configuration["FackCar:sleep"].ToString());
            _mqttservice = ServiceLocator.Services.GetService<MosquittoMqttClientService>();
        }

        public void Dispose()
        {
            _timer.Dispose();
        }
        public async Task Execute(IJobExecutionContext context)
        {
            
            _logger.Warning("Fake车辆生成开始");
            try
            {
                //int times = int.Parse(_configuration["FackCar:times"].ToString());
                //int sleep = int.Parse(_configuration["FackCar:sleep"].ToString());
                int roadpart = int.Parse(_configuration["FackCar:roadpart"].ToString());
                string camera = _configuration["FackCar:camera"];
                int direction = int.Parse(_configuration["FackCar:direction"].ToString());
                int carcount = int.Parse(_configuration["FackCar:carcount"].ToString());
                int firstcarloc = int.Parse(_configuration["FackCar:firstcarloc"].ToString());
                int offset1 = int.Parse(_configuration["FackCar:offset1"].ToString());
                int offset2 = int.Parse(_configuration["FackCar:offset2"].ToString());
                int carcountrndMax = int.Parse(_configuration["FackCar:carcountrndMax"].ToString());
                await Publish_FakeCar_Message( roadpart, direction, carcount, firstcarloc, offset1, offset2, carcountrndMax);
                
            }
            catch (Exception ex)
            {
                _logger.Error("Fake车辆生成错误:" + ex.ToString());
            }


        }

        private async Task<List<carinfo>> GenerCar(int singlecarcount, int firststart, int offset1, int offset2, int singlecarcountRndMax)
        {
            List<carinfo> ret = new List<carinfo>();
            for (int i = 1; i <= 3; i++)//模拟三根车道
            {
                int roadlane = i;
                string caridfront = _configuration["FackCar:carfront"].ToString() + DateTime.Now.ToString("HHmmssfff") + i;
                Random ran5 = new Random();
                int curroadlanecarcount = ran5.Next(singlecarcount, singlecarcountRndMax);

                Random ran1 = new Random();
                int firstcarstart = ran1.Next(0, firststart);
                int distance = 0;
                for (int j = 1; j <= curroadlanecarcount; j++)
                {
                    string curcarid = caridfront + j.ToString();
                    string curnum = i.ToString() + j.ToString();
                    //模拟第一次该车辆的起点位置

                    if (j == 1)
                    {
                        distance = firstcarstart;
                    }
                    else
                    {
                        Random ran2 = new Random();
                        int flowdistance = ran2.Next(offset1, offset2);//下一辆车的偏移距离
                        distance = distance + flowdistance;
                    }

                    Random ran3 = new Random();
                    int cartype = ran3.Next(1, 3);


                    Random ran4 = new Random();
                    int carcolor = ran4.Next(1, 7);
                    ret.Add(new carinfo()
                    {
                        num = int.Parse(curnum),
                        carid = curcarid,
                        cartype = cartype,
                        carcolor = carcolor,
                        distance = distance,
                        roadlane = roadlane
                    });
                }
            }
            return ret;
        }
        private async Task Publish_FakeCar_Message(int rp, int dirc, int carcount, int firstcarloccation, int offset1, int offset2, int carcountRndMax)
        {


            List<carinfo> carinfolist = await GenerCar(carcount, firstcarloccation, offset1, offset2, carcountRndMax);
            transportret ret = new transportret();
            ret.fakeindex = 0;
            ret.code = 0;
            ret.msg = "成功";
            ret.result = new transportresult() { roadpart = rp, direction = dirc, carinfo = carinfolist, camera = _configuration["FackCar:camera"] };
            _timer = new Timer(DoWork, ret, TimeSpan.Zero, TimeSpan.FromSeconds(sleep));
            //for (int i = 0; i < times; i++)//时间间隔
            //{
            //    //Thread.Sleep(sleep);
            //    transportret ret = new transportret();
            //    ret.code = 0;
            //    DateTime startTime = TimeZoneInfo.ConvertTime(new DateTime(1970, 1, 1, 8, 0, 0, 0), TimeZoneInfo.Local);
            //    long t = (DateTime.Now.Ticks - startTime.Ticks) / 10000;   //除10000调整为13位 
            //    ret.timestamp = t.ToString();
            //    ret.msg = "成功";

            //    if (i != 0)//第二次开始，要让车辆动起来，则需要改变distance
            //    {
            //        for (int j = 0; j < carinfolist.Count; j++)
            //        {
            //            carinfolist[j].distance = carinfolist[j].distance + carspeed;
            //        }
            //    }
            //    ret.result = new transportresult() { roadpart = rp, direction = dirc, carinfo = carinfolist, camera = _configuration["FackCar:camera"] };
            //    string payloadstr = JsonConvert.SerializeObject(ret);

            //    await _mqttservice.getClient().PublishMessageAsync(_configuration["MQTTSet:FackSend"], JsonConvert.SerializeObject(payloadstr), false, 0);
            //    _logger.Warning("发送给 " + _configuration["MQTTSet:FackSend"].ToString() + " : " + payloadstr);
            //}

        }

        private async void DoWork(object state)
        {
            try
            {
                lock (obj)
                {
                    transportret ret = (transportret)state;
                    if (ret != null && ret.result != null)
                    {
                        var carinfolist = ret.result.carinfo;
                        _logger.Warning("生成下一帧" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        ret.timestamp = AliyunHelper.TimeStampToDateTime(DateTime.Now).ToString();
                        ret.time = DateTime.Now;
                        if (ret.fakeindex != 0)//第二次开始，要让车辆动起来，则需要改变distance
                        {
                            for (int j = 0; j < carinfolist.Count; j++)
                            {
                                carinfolist[j].distance = carinfolist[j].distance + carspeed;
                            }
                        }
                        ret.result.carinfo = carinfolist;
                        string payloadstr = JsonConvert.SerializeObject(ret);
                        _mqttservice.getClient().PublishMessageAsync(_configuration["MQTTSet:FackSend"], payloadstr, false, 0);
                        _logger.Warning("已发送给 " + _configuration["MQTTSet:FackSend"].ToString() + " : " + payloadstr);
                        ret.fakeindex++;
                        if (ret.fakeindex >= times)
                        {
                            _logger.Warning("Fake车辆生成结束");
                            _timer.Dispose();
                        }
                    }
                    
                }
            }
            catch (Exception ex)
            {
                _logger.Error("发送到mqtt出错:" + ex.ToString());
            }
        }

    }
}
