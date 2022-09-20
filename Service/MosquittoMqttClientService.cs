using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using MQTTnet.Client.Options;
using System.Threading;
using System.Threading.Tasks;
using SZY.Platform.WebApi.Client;
using SZY.Platform.WebApi.Data;
using SZY.Platform.WebApi.Model;

namespace SZY.Platform.WebApi.Service
{

    public interface IMosquittoMqttClientService
    {
        
    }
    public class MosquittoMqttClientService : IHostedService, IMosquittoMqttClientService
    {
        private readonly MosquittoMqttClient client;
        private ITransportCameraRepo<TransportCarCameraToTunnel> _repo;
        private readonly IConfiguration _configuration;

        public MosquittoMqttClientService(IMqttClientOptions options, ITransportCameraRepo<TransportCarCameraToTunnel> repo, IConfiguration configuration)
        {
            _configuration = configuration;
            client = new MosquittoMqttClient(_configuration);
            _repo = repo;
        }

        //public MosquittoMqttClientService()
        //{
        //    client = new MosquittoMqttClient(options);
        //}

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return client.StartClientAsync(_repo);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return client.StopClientAsync();
        }
    }
}