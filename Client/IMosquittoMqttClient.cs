using System.Threading.Tasks;

namespace SZY.Platform.WebApi.Client
{
    public interface IMosquittoMqttClient
    {
        Task StartClientAsync();
        Task StopClientAsync();
    }
}
