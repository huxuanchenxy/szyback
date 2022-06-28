using System.Threading.Tasks;
using SZY.Platform.WebApi.Data;
using SZY.Platform.WebApi.Model;

namespace SZY.Platform.WebApi.Client
{
    public interface IMosquittoMqttClient
    {
        Task StartClientAsync(ITransportCameraRepo<TransportCarCameraToTunnel> repo);
        Task StopClientAsync();
    }
}
