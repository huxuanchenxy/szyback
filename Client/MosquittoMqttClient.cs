using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using MQTTnet.Protocol;
using Newtonsoft.Json;
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

        private List<TransportCarCameraToTunnel> cameralist;

        private ITransportCameraRepo<TransportCarCameraToTunnel> _repo;

        public MosquittoMqttClient()
        {

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
                    .WithTcpServer("47.101.220.2", 1883)
                    //.WithWebSocketServer("ws://47.101.220.2:8083/mqtt")
                    .WithClientId("transportzdh" + Guid.NewGuid().ToString("D"))
                    .WithCredentials("admin", "public")
                    .Build();
            client = new MqttFactory().CreateMqttClient();
            client.UseApplicationMessageReceivedHandler(OnMessage);
        }

        /// <summary>
        /// 摄像头映射偏移量
        /// </summary>
        private void GenarateCamera()
        {
            cameralist = new List<TransportCarCameraToTunnel>();
            cameralist.Add(new TransportCarCameraToTunnel() { camera = "K4+940", offset = 0, roadpart = 2, direction = 1 });
            cameralist.Add(new TransportCarCameraToTunnel() { camera = "K4+1940", offset = 1000, roadpart = 2, direction = 1 });


        }


       





        public async Task PublishMessageAsync(string topic, string payload, bool retainFlag = false, int qos = 0)
        {
            Console.WriteLine("Enviando mensagem para o broker: ");
            Console.WriteLine(payload);

            Console.Write("Topico: ");
            Console.WriteLine(topic);

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

            await client.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic("transport/car").Build());

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

                var sss = _repo.GetPageList();
                if (topic.Contains("transport/car"))
                {
                    //Console.WriteLine("Nova mensagem recebida do broker: ");
                    //Console.WriteLine(jsonPayload);
                    _logger.Warning(jsonPayload);
                    //var payload = JsonSerializer.Deserialize<TransportCarRoot>(jsonPayload);
                    var obj = JsonConvert.DeserializeObject<TransportCarRoot>(jsonPayload);
                    if (obj.result != null)
                    {
                        var camera = obj.result.camera;
                        if (camera != null)
                        {
                            var curcamera = cameralist.Where(c => c.camera == obj.result.camera).FirstOrDefault();
                            int curoffset = curcamera.offset;
                            int curdirection = curcamera.direction;
                            int curroadpart = curcamera.roadpart;
                            obj.result.direction = curdirection;
                            obj.result.roadpart = curroadpart;
                            if (obj.result.carinfo != null && obj.result.carinfo.Count > 0)
                            {
                                for(int i = 0;i<obj.result.carinfo.Count;i++)
                                {
                                    obj.result.carinfo[i].distance += curoffset;
                                }
                            }

                            await PublishMessageAsync(@"transport/car/front", JsonConvert.SerializeObject(obj), false, 0);
                            _logger.Warning("ConvertToFront");
                            _logger.Warning(JsonConvert.SerializeObject(obj));
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


        public Task StopClientAsync()
        {
            throw new NotImplementedException();
        }
    }
}
