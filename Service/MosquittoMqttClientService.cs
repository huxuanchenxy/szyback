using Microsoft.Extensions.Hosting;
using MQTTnet.Client.Options;
using System.Threading;
using System.Threading.Tasks;
using SZY.Platform.WebApi.Client;

namespace SZY.Platform.WebApi.Service
{
    public class MosquittoMqttClientService : IHostedService
    {
        private readonly MosquittoMqttClient client;

        public MosquittoMqttClientService(IMqttClientOptions options)
        {
            client = new MosquittoMqttClient(options);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return client.StartClientAsync();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return client.StopClientAsync();
        }
    }
}