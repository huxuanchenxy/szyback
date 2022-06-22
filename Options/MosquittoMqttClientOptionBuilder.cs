using MQTTnet.Client.Options;
using System;

namespace SZY.Platform.WebApi.Options
{
    public class MosquittoMqttClientOptionBuilder : MqttClientOptionsBuilder
    {
        public IServiceProvider ServiceProvider { get; }

        public MosquittoMqttClientOptionBuilder(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }        
    }
}
