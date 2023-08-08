using Quartz;
using Serilog;
using Serilog.Core;
using System.Threading.Tasks;
using OfficeOpenXml;
using System.IO;
using System;
using Microsoft.Extensions.Configuration;
using SZY.Platform.WebApi.Data;
using SZY.Platform.WebApi.Model;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using OfficeOpenXml.Style;
using System.Drawing;
using SZY.Platform.WebApi.Helper;
using MQTTnet;
using MQTTnet.Client.Options;
using System.Threading;

namespace SZY.Platform.WebApi.Service
{
    public class FundJob3 : IJob//创建IJob的实现类，并实现Excute方法。
    {
        private readonly Logger _logger;
        private readonly IConfiguration _configuration;
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
        }
        public async Task Execute(IJobExecutionContext context)
        {
            
            _logger.Warning("Fake车辆生成开始");
            try
            {
                int times = int.Parse(_configuration["FackCar:times"].ToString());
                int sleep = int.Parse(_configuration["FackCar:sleep"].ToString());
                int roadpart = int.Parse(_configuration["FackCar:roadpart"].ToString());
                string camera = _configuration["FackCar:camera"];
                int direction = int.Parse(_configuration["FackCar:direction"].ToString());
                int carcount = int.Parse(_configuration["FackCar:carcount"].ToString());
                int carspeed = int.Parse(_configuration["FackCar:carspeed"].ToString());
                int firstcarloc = int.Parse(_configuration["FackCar:firstcarloc"].ToString());
                int offset1 = int.Parse(_configuration["FackCar:offset1"].ToString());
                int offset2 = int.Parse(_configuration["FackCar:offset2"].ToString());
                int carcountrndMax = int.Parse(_configuration["FackCar:carcountrndMax"].ToString());
                await Publish_FakeCar_Message(times, sleep, roadpart, direction, carcount, carspeed, firstcarloc, offset1, offset2, carcountrndMax);
                _logger.Warning("Fake车辆生成结束");
            }
            catch (Exception ex)
            {
                _logger.Error("Fake车辆生成错误:" + ex.ToString());
            }
            
            
        }

        private async Task<List<carinfo>> GenerCar(int singlecarcount, int firststart, int offset1, int offset2,int singlecarcountRndMax)
        {
            List<carinfo> ret = new List<carinfo>();
            for (int i = 1; i <= 3; i++)//模拟三根车道
            {
                int roadlane = i;
                string caridfront = _configuration["FackCar:carfront"].ToString() + DateTime.Now.Hour.ToString()+ DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString() + i;
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
        private async Task Publish_FakeCar_Message(int times, int sleep, int rp, int dirc, int carcount, int cspeed, int firstcarloccation, int offset1, int offset2,int carcountRndMax)
        {

            var mqttFactory = new MqttFactory();

            using (var mqttClient = mqttFactory.CreateMqttClient())
            {
                var mqttClientOptions = new MqttClientOptionsBuilder()
                    //.WithTcpServer("broker.hivemq.com")
                    .WithWebSocketServer("ws://" + _configuration["MQTTSet:Ip"].ToString() + ":8083/mqtt")
                    .Build();

                await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

                List<carinfo> carinfolist = await GenerCar(carcount, firstcarloccation, offset1, offset2,carcountRndMax);
                int carspeed = cspeed;
                for (int i = 0; i < times; i++)//时间间隔
                {
                    Thread.Sleep(sleep);
                    transportret ret = new transportret();
                    ret.code = 0;
                    //DateTime dtStart = TimeZoneInfo.ConvertTimeFromUtc(new DateTime(1970, 1, 1, 0, 0, 0), TimeZoneInfo.Local);
                    //long timeStamp = Convert.ToInt32((DateTime.Now - dtStart).TotalMilliseconds);
                    DateTime startTime = TimeZoneInfo.ConvertTime(new DateTime(1970, 1, 1, 8, 0, 0, 0), TimeZoneInfo.Local);
                    long t = (DateTime.Now.Ticks - startTime.Ticks) / 10000;   //除10000调整为13位 
                    //TimeSpan ts = new TimeSpan(DateTime.Now.Ticks);
                    //ret.timestamp = ts.TotalMilliseconds.ToString();
                    ret.timestamp = t.ToString();
                    ret.msg = "成功";

                    if (i != 0)//第二次开始，要让车辆动起来，则需要改变distance
                    {
                        for (int j = 0; j < carinfolist.Count; j++)
                        {
                            carinfolist[j].distance = carinfolist[j].distance + carspeed;
                        }
                    }
                    ret.result = new transportresult() { roadpart = rp, direction = dirc, carinfo = carinfolist, camera = _configuration["FackCar:camera"] };
                    string payload = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:ms");

                    string payloadstr = JsonConvert.SerializeObject(ret);

                    _logger.Warning("发送给 "+ _configuration["MQTTSet:FackSend"].ToString() + " : " + payloadstr);
                    var applicationMessage = new MqttApplicationMessageBuilder()
                    .WithTopic(_configuration["MQTTSet:FackSend"].ToString())
                    .WithPayload(payloadstr)
                                        .Build();

                    await mqttClient.PublishAsync(applicationMessage, CancellationToken.None);
                }
                //var applicationMessage = new MqttApplicationMessageBuilder()
                //    .WithTopic("transport/car")
                //    .WithPayload(DateTime.Now.ToString())
                //    .Build();

                //await mqttClient.PublishAsync(applicationMessage, CancellationToken.None);

                //Console.WriteLine("MQTT application message is published.");
            }
        }
    }
}
