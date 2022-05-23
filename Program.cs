using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Serilog;
using Serilog.Events;
using System.IO;
using System.Net;

namespace SZY.Platform.WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args)
        {
            int port = int.Parse(args[0]);//3861
            return WebHost.CreateDefaultBuilder(args)

                .UseContentRoot(Directory.GetCurrentDirectory())
                 .UseIISIntegration()
                .UseKestrel(options =>
                {
                    options.Listen(IPAddress.Any, port);
                    //options.Listen(IPAddress.Any, 443, listenOptions =>
                    //{
                    //    listenOptions.UseHttps("server.pfx", "password");
                    //});
                })
              .UseStartup<Startup>().UseSerilog((context, configuration) => configuration
                .Enrich.FromLogContext()
                .MinimumLevel.Warning() //这个根据level来，比如到error那information级别的就不出来了
                                            //.WriteTo.Console()
                                            //.WriteTo.File(Path.Combine("Logs", @"log.txt"), rollingInterval: RollingInterval.Day)
                .WriteTo.Logger(l => l.Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Information).WriteTo.RollingFile(@"Logs/Information-{Date}.log"))
                .WriteTo.Logger(l => l.Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Warning).WriteTo.RollingFile(@"Logs/Warning-{Date}.log"))
                .WriteTo.Logger(l => l.Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Debug).WriteTo.RollingFile(@"Logs/Debug-{Date}.log"))
                .WriteTo.Logger(l => l.Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Error).WriteTo.RollingFile(@"Logs/Error-{Date}.log")),
                preserveStaticLogger: true)
                .Build();
        }
    }
}
