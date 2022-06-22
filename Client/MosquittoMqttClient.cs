using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using MQTTnet.Protocol;
using System;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;

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

        public MosquittoMqttClient(IMqttClientOptions options)
        {
            Options = new MqttClientOptionsBuilder()
                    .WithTcpServer("47.101.220.2", 1883)
                    //.WithWebSocketServer("ws://47.101.220.2:8083/mqtt")
                    .WithClientId("transportzdh" + Guid.NewGuid().ToString("D"))
                    .WithCredentials("admin", "public")
                    .Build();
            client = new MqttFactory().CreateMqttClient();
            client.UseApplicationMessageReceivedHandler(OnMessage);
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


        public async Task StartClientAsync()
        {
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

                if (topic.Contains("transport/car"))
                {
                    Console.WriteLine("Nova mensagem recebida do broker: ");
                    Console.WriteLine(jsonPayload);

                    //var payload = JsonSerializer.Deserialize<PayloadMessage>(jsonPayload);

                    //if (!string.IsNullOrEmpty(payload.message))
                    //{

                    //}
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
