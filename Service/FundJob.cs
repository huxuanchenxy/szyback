using Microsoft.Extensions.Caching.Distributed;
using MSS.API.Common;
using MSS.API.Common.Utility;
using SZY.Platform.WebApi.Data;
using SZY.Platform.WebApi.Model;
using System;
using System.Net;
using System.Threading.Tasks;
using MSS.API.Common.DistributedEx;
using System.Collections.Generic;
using System.Linq;
using Quartz;
using Serilog.Core;
using Serilog;
using MQTTnet;
using MQTTnet.Client.Options;
using System.Threading;
using Newtonsoft.Json;
using MQTTnet.Client;
using SZY.Platform.WebApi.Client;
using SZY.Platform.WebApi.Service;

namespace SZY.Platform.WebApi.Service
{

    //    {"A01":110000,"res":"123"}，//马上打开1路继电器
    //{ "A01":100000,"res":"123"}，//马上关闭1路继电器
    //{ "A01":310005,"res":"123"}，//延时5秒打开1路继电器
    //{ "A01":210005,"res":"123"}，//延时5秒关闭1路继电器
    //{ "A01":110000,"A02":210010,"A03":110000,"A04":210010,"A05":110000,"A06":210010,"A07":110000,"A08":210010,"A09":110000,"A10":210010,”res”:”123”},//多路继电器
    public class FundJob : IJob//创建IJob的实现类，并实现Excute方法。
    {
        private readonly Logger _logger;
        public FundJob()
        {
            _logger = new LoggerConfiguration()
#if DEBUG
        .MinimumLevel.Debug()
#else
        .MinimumLevel.Information()
#endif
        //.MinimumLevel.Override("Microsoft", LogEventLevel.)
        //.Enrich.FromLogContext()
        .WriteTo.RollingFile(@"c:\\SZYLogs\watercontrol.txt")
        .CreateLogger();


        }
        public async Task Execute(IJobExecutionContext context)
        {
            //return Task.Run(() =>
            //{
            //Console.WriteLine(DateTime.Now);

            //_logger.Warning(DateTime.Now.ToString("yyyy-mm-dd HH:mm:ss"));
            //string payloadstr = "{\"A01\":110000,\"res\":\"123\"}";
            //client.PublishMessageAsync("set4GMQTT000801", payloadstr, false, 0);



            var mqttFactory = new MqttFactory();
            string payloadstr = "{\"A02BBB\":9999,\"res\":\"123\"}";
            using (var mqttClient = mqttFactory.CreateMqttClient())
            {
                var mqttClientOptions = new MqttClientOptionsBuilder()
                    //.WithTcpServer("broker.hivemq.com")
                    .WithWebSocketServer("ws://47.101.220.2:8083/mqtt")
                    .Build();

                await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

                var applicationMessage = new MqttApplicationMessageBuilder()
                    .WithTopic("/set/4GMQTT000801")
                    .WithPayload(payloadstr)
                    .Build();

                if (mqttClient != null)
                {
                    await mqttClient.PublishAsync(applicationMessage, CancellationToken.None);
                }
                _logger.Warning("/set/4GMQTT000801/shutdown");
                _logger.Warning(payloadstr);
                //Console.WriteLine("MQTT application message is published.");
            }
            //});
        }
    }
}
