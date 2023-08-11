﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using MQTTnet.Protocol;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using SZY.Platform.WebApi.Data;
using SZY.Platform.WebApi.Model;
using SZY.Platform.WebApi.Service;

namespace SZY.Platform.WebApi.Client
{
    public class MosquittoMqttClient : IMosquittoMqttClient
    {
        const string TOPICO_LOG_STATUS = "HOME/LOG/STATUS";
        const string TOPICO_GATEWAY_ENTRADA = "HOME/GATEWAY/ENTRADA";
        const string TOPICO_GATEWAY_SAIDA = "HOME/GATEWAY/SAIDA";
        const string TOPICO_DESODORIZACAO = "HOME/DESODORIZACAO";
        const string TOPICO_INTERFONE = "HOME/INTERFONE";
        const string TOPICO_PETS = "HOME/PETS";

        const string DEVICE_ID = "CONTROLLER_SMS";

        private IMqttClientOptions Options;
        private readonly string menu = "Controller SMS API \r\nDigite a opcao abaixo: \r\nOP1 - Informar Temperatura \r\nOP2 - Desodorizar Ambiente \r\nOP3 - Abrir Portaria \r\nOP4 - Alimentar Pets";

        private IMqttClient client;
        private readonly Logger _logger;
        private readonly Logger _logger2;

        private List<TransportCarCameraToTunnel> cameralist;

        private ITransportCameraRepo<TransportCarCameraToTunnel> _repo;
        private readonly IConfiguration _configuration;

        public MosquittoMqttClient(IConfiguration configuration)
        {
            _configuration = configuration;
            _logger = new LoggerConfiguration()
#if DEBUG
        .MinimumLevel.Debug()
#else
        .MinimumLevel.Information()
#endif
        //.MinimumLevel.Override("Microsoft", LogEventLevel.)
        //.Enrich.FromLogContext()
        .WriteTo.RollingFile(@"c:\\SZYLogs\transportcar.txt")
        .CreateLogger();

            GenarateCamera();



            Options = new MqttClientOptionsBuilder()
                    .WithTcpServer(_configuration["MQTTSet:Ip"].ToString(), 1883)
                    //.WithWebSocketServer("ws://47.101.220.2:8083/mqtt")
                    .WithClientId("transportzdh" + Guid.NewGuid().ToString("D"))
                    .WithCredentials("admin", "public")
                    .Build();
            client = new MqttFactory().CreateMqttClient();
            client.UseApplicationMessageReceivedHandler(OnMessage3);


            _logger2 = new LoggerConfiguration()
#if DEBUG
        .MinimumLevel.Debug()
#else
        .MinimumLevel.Information()
#endif
        .WriteTo.File(@"c:\\SZYLogs\MosquittoMqttClient.txt", rollingInterval: RollingInterval.Day)
        .CreateLogger();
        }

        public bool CheckConnect()
        {
            return client.IsConnected;
        }

        /// <summary>
        /// 摄像头映射偏移量
        /// </summary>
        private void GenarateCamera()
        {
            //找到这个打开services.AddTransient<IMosquittoMqttClient, MosquittoMqttClient>();
            //services.AddTransient<IMosquittoMqttClientService, MosquittoMqttClientService>();
            //services.AddMqttClientServiceWithConfig(aspOptionBuilder =>
            //{
            //    aspOptionBuilder
            //    .WithCredentials("admin", "public")
            //    .WithClientId("zdh" + Guid.NewGuid().ToString("D"))
            //    .WithTcpServer(_configuration["MQTTSet:Ip"], 1883);
            //});
            int k28 = int.Parse(_configuration["MQTTSet:K2right"].ToString());
            int k35 = int.Parse(_configuration["MQTTSet:K2left"].ToString());
            int pxlong = int.Parse(_configuration["MQTTSet:Pxlong"].ToString());
            cameralist = new List<TransportCarCameraToTunnel>();
            //从k28+020到610,760-20 总共740m
            cameralist.Add(new TransportCarCameraToTunnel() { camera = "K28+020", offset = 20, offsetsingle  = 80,offsetsinglepx = 80*pxlong/k28,  roadpart = 28, direction = 1 });//后面一个摄像头减去当前摄像头的距离
            cameralist.Add(new TransportCarCameraToTunnel() { camera = "K28+100", offset = 100, offsetsingle = 75, offsetsinglepx = 75 * pxlong / k28, roadpart = 28, direction = 1 });
            cameralist.Add(new TransportCarCameraToTunnel() { camera = "K28+175", offset = 175, offsetsingle = 75, offsetsinglepx = 75 * pxlong / k28, roadpart = 28, direction = 1 });
            cameralist.Add(new TransportCarCameraToTunnel() { camera = "K28+250", offset = 250, offsetsingle = 70, offsetsinglepx = 70 * pxlong / k28, roadpart = 28, direction = 1 });
            cameralist.Add(new TransportCarCameraToTunnel() { camera = "K28+320", offset = 320, offsetsingle = 70, offsetsinglepx = 70 * pxlong / k28, roadpart = 28, direction = 1 });
            cameralist.Add(new TransportCarCameraToTunnel() { camera = "K28+390", offset = 390, offsetsingle = 150, offsetsinglepx = 150 * pxlong / k28, roadpart = 28, direction = 1 });
            cameralist.Add(new TransportCarCameraToTunnel() { camera = "K28+540", offset = 540, offsetsingle = 70, offsetsinglepx = 70 * pxlong / k28, roadpart = 28, direction = 1 });
            cameralist.Add(new TransportCarCameraToTunnel() { camera = "K28+610", offset = 610, offsetsingle = 150, offsetsinglepx = 150 * pxlong / k28, roadpart = 28, direction = 1 });
            cameralist.Add(new TransportCarCameraToTunnel() { camera = "K28+760", offset = 760, offsetsingle = 100, offsetsinglepx = 100 * pxlong / k28, roadpart = 28, direction = 1 });

            //从k35+047到670=745-47=698m
            cameralist.Add(new TransportCarCameraToTunnel() { camera = "K35+047", offset = 0, offsetsingle = 47, offsetsinglepx = 47 * pxlong / k35, roadpart = 35, direction = 2 });
            cameralist.Add(new TransportCarCameraToTunnel() { camera = "K35+120", offset = 47, offsetsingle = 73, offsetsinglepx = 73 * pxlong / k35, roadpart = 35, direction = 2 });
            cameralist.Add(new TransportCarCameraToTunnel() { camera = "K35+190", offset = 120, offsetsingle = 70, offsetsinglepx = 70 * pxlong / k35, roadpart = 35, direction = 2 });
            cameralist.Add(new TransportCarCameraToTunnel() { camera = "K35+310", offset = 190, offsetsingle = 120, offsetsinglepx = 120 * pxlong / k35, roadpart = 35, direction = 2 });
            cameralist.Add(new TransportCarCameraToTunnel() { camera = "K35+385", offset = 310, offsetsingle = 75, offsetsinglepx = 75 * pxlong / k35, roadpart = 35, direction = 2 });
            cameralist.Add(new TransportCarCameraToTunnel() { camera = "K35+447", offset = 385, offsetsingle = 62, offsetsinglepx = 62 * pxlong / k35, roadpart = 35, direction = 2 });
            cameralist.Add(new TransportCarCameraToTunnel() { camera = "K35+520", offset = 447, offsetsingle = 73, offsetsinglepx = 73 * pxlong / k35, roadpart = 35, direction = 2 });
            cameralist.Add(new TransportCarCameraToTunnel() { camera = "K35+670", offset = 520, offsetsingle = 150, offsetsinglepx = 150 * pxlong / k35, roadpart = 35, direction = 2 });
            cameralist.Add(new TransportCarCameraToTunnel() { camera = "K35+745", offset = 670, offsetsingle = 150, offsetsinglepx = 150 * pxlong / k35, roadpart = 35, direction = 2 });
        }








        public async Task PublishMessageAsync(string topic, string payload, bool retainFlag = false, int qos = 0)
        {
            //Console.WriteLine("Enviando mensagem para o broker: ");
            //Console.WriteLine(payload);

            //Console.Write("Topico: ");
            //Console.WriteLine(topic);

            await client.PublishAsync(new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .WithQualityOfServiceLevel((MqttQualityOfServiceLevel)qos)
                .WithRetainFlag(retainFlag)
                .Build());
        }


        public async Task StartClientAsync(ITransportCameraRepo<TransportCarCameraToTunnel> repo)
        {
            _repo = repo;
            await client.ConnectAsync(Options, CancellationToken.None);

            // anuncia status online
            //PayloadStatus payload = new PayloadStatus();
            //payload.device = DEVICE_ID;
            //payload.status = "Online";
            //payload.connectedOn = DateTime.Now.ToString();

            await client.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic(_configuration["MQTTSet:TongJiSend"]).Build());

            if (!client.IsConnected)
            {
                await client.ReconnectAsync();
            }
        }

        public virtual async void OnMessage(MqttApplicationMessageReceivedEventArgs eventArgs)
        {
            try
            {
                var jsonPayload = Encoding.UTF8.GetString(eventArgs.ApplicationMessage.Payload);
                var topic = eventArgs.ApplicationMessage.Topic;

                int pxlong = 1;
                int cmlong = 1;
                //var sss = _repo.GetPageList();
                if (topic.Contains("transport/car"))
                {
                    //Console.WriteLine("Nova mensagem recebida do broker: ");
                    //Console.WriteLine(jsonPayload);
                    _logger.Warning("transport/car");
                    _logger.Warning(jsonPayload);
                    //var payload = JsonSerializer.Deserialize<TransportCarRoot>(jsonPayload);
                    var obj = JsonConvert.DeserializeObject<TransportCarRoot>(jsonPayload);
                    if (obj.result != null)
                    {
                        var camera = obj.result.camera;
                        if (camera != null)
                        {
                            var curcamera = cameralist.Where(c => c.camera == obj.result.camera).FirstOrDefault();
                            if (curcamera != null)
                            {
                                int curoffset = curcamera.offset * pxlong / cmlong;
                                int curdirection = curcamera.direction;
                                int curroadpart = curcamera.roadpart;
                                obj.result.direction = curdirection;
                                obj.result.roadpart = curroadpart;
                                if (obj.result.carinfo != null && obj.result.carinfo.Count > 0)
                                {
                                    for (int i = 0; i < obj.result.carinfo.Count; i++)
                                    {
                                        obj.result.carinfo[i].distance += curoffset;
                                    }
                                }

                                await PublishMessageAsync(@"transport/car/front", JsonConvert.SerializeObject(obj), false, 0);
                                _logger.Warning("transport/car/front");
                                _logger.Warning(JsonConvert.SerializeObject(obj));
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro ao tentar ler a msnsagem: " + ex.Message);
                //throw;
            }
        }


        //长江隧桥8+8离线数据
        public virtual async void OnMessage2(MqttApplicationMessageReceivedEventArgs eventArgs)
        {
            try
            {
                var jsonPayload = Encoding.UTF8.GetString(eventArgs.ApplicationMessage.Payload);
                var topic = eventArgs.ApplicationMessage.Topic;

                int pxlong = int.Parse(_configuration["MQTTSet:Pxlong"].ToString());//模拟一个屏幕的像素
                int k28 = int.Parse(_configuration["MQTTSet:K2right"].ToString()); //k28同济选择的最后一个摄像头，实际占的米
                int k35 = int.Parse(_configuration["MQTTSet:K2left"].ToString()); 
                float tongjiscreen = float.Parse(_configuration["MQTTSet:Tongjiscreen"].ToString()); //同济算法单个屏幕的长度
                //var sss = _repo.GetPageList();
                if (topic.Contains(_configuration["MQTTSet:TongJiSend"].ToString()))
                {
                    //Console.WriteLine("Nova mensagem recebida do broker: ");
                    //Console.WriteLine(jsonPayload);
                    _logger.Warning(_configuration["MQTTSet:TongJiSend"].ToString());
                    _logger.Warning(jsonPayload);
                    //var payload = JsonSerializer.Deserialize<TransportCarRoot>(jsonPayload);
                    var obj = JsonConvert.DeserializeObject<TransportCarRoot>(jsonPayload);
                    if (obj.result != null)
                    {
                        var camera = obj.result.camera;
                        if (camera != null)
                        {
                            var curcamera = cameralist.Where(c => c.camera == obj.result.camera).FirstOrDefault();
                            if (curcamera != null)
                            {
                                //int curoffset = curcamera.offset * pxlong / cmlong;//北横通道的换算
                                int curdirection = curcamera.direction;
                                int curroadpart = curcamera.roadpart;
                                int curoffsetsingle = curcamera.offsetsingle;
                                int curoffsetsinglepx = curcamera.offsetsinglepx;
                                obj.result.direction = curdirection;
                                obj.result.roadpart = curroadpart;
                                
                                if (obj.result.carinfo != null && obj.result.carinfo.Count > 0)
                                {
                                    for (int i = 0; i < obj.result.carinfo.Count; i++)
                                    {
                                        float curdistance = 0f;
                                        //if (curdirection == 1)
                                        //{
                                        //    curdistance = (obj.result.carinfo[i].distance / tongjiscreen * curoffsetsingle + curcamera.offset) * pxlong / k28;//先算同济单个屏幕里的比例，然后换算到实际摄像头的米数距离，最后转换成像素距离。//这个是总屏幕里的位置
                                        //}
                                        //else if (curdirection == 2)
                                        //{
                                        //    curdistance = (curoffsetsingle - obj.result.carinfo[i].distance / tongjiscreen * curoffsetsingle + curcamera.offset) * pxlong / k35;
                                        //}
                                        curdistance = obj.result.carinfo[i].distance / tongjiscreen * curoffsetsinglepx;//实际当前摄像头像素

                                        obj.result.carinfo[i].distance = (int)curdistance;
                                    }
                                }

                                await PublishMessageAsync(_configuration["MQTTSet:LocalReceive"].ToString(), JsonConvert.SerializeObject(obj), false, 0);
                                _logger.Warning(_configuration["MQTTSet:LocalReceive"].ToString());
                                _logger.Warning(JsonConvert.SerializeObject(obj));
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro ao tentar ler a msnsagem: " + ex.Message);
                //throw;
            }
        }

        //长江隧桥处理好的算法数据再做处理
        public virtual async void OnMessage3(MqttApplicationMessageReceivedEventArgs eventArgs)
        {
            try
            {
                _logger2.Warning("开始接收mqtt数据");
                var jsonPayload = Encoding.UTF8.GetString(eventArgs.ApplicationMessage.Payload);
                var topic = eventArgs.ApplicationMessage.Topic;
                _logger2.Warning("topic是:" + topic);
                if (topic.Contains(_configuration["MQTTSet:TongJiSend"].ToString()))
                {
                    JObject jb = JsonConvert.DeserializeObject<JObject>(jsonPayload);
                    DataTransferService.EnqueueTask(jb);
                    _logger2.Warning("已经推入 内存消息队列");
                    _logger2.Warning(jsonPayload);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("OnMessage3: " + ex.Message);
                //throw;
            }
        }

        public Task StopClientAsync()
        {
            return client.DisconnectAsync();
            //throw new NotImplementedException();
        }
    }
}
