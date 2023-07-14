using Quartz;
using Serilog;
using Serilog.Core;
using System.Threading.Tasks;
using OfficeOpenXml;
using System.IO;
using System;
using Microsoft.Extensions.Configuration;
using SZY.Platform.WebApi.Data;
using SZY.Platform.WebApi.Model;
using Newtonsoft.Json;

namespace SZY.Platform.WebApi.Service
{

    //    {"A01":110000,"res":"123"}，//马上打开1路继电器
    //{ "A01":100000,"res":"123"}，//马上关闭1路继电器
    //{ "A01":310005,"res":"123"}，//延时5秒打开1路继电器
    //{ "A01":210005,"res":"123"}，//延时5秒关闭1路继电器
    //{ "A01":110000,"A02":210010,"A03":110000,"A04":210010,"A05":110000,"A06":210010,"A07":110000,"A08":210010,"A09":110000,"A10":210010,”res”:”123”},//多路继电器
    public class FundJob2 : IJob//创建IJob的实现类，并实现Excute方法。
    {
        private readonly Logger _logger;
        private readonly IConfiguration _configuration;
        private readonly ISimulationInfoRepo<SimulationInfo> _repo;
        public FundJob2(IConfiguration configuration, ISimulationInfoRepo<SimulationInfo> repo)
        {
            _logger = new LoggerConfiguration()
#if DEBUG
        .MinimumLevel.Debug()
#else
        .MinimumLevel.Information()
#endif
        //.MinimumLevel.Override("Microsoft", LogEventLevel.)
        //.Enrich.FromLogContext()
        .WriteTo.RollingFile(@"c:\\SZYLogs\FundJob2.txt")
        .CreateLogger();
            _configuration = configuration;
            _repo = repo;
        }
        public async Task Execute(IJobExecutionContext context)
        {
            //return Task.Run(() =>
            //{
            //Console.WriteLine(DateTime.Now);

            //_logger.Warning(DateTime.Now.ToString("yyyy-mm-dd HH:mm:ss"));
            //string payloadstr = "{\"A01\":110000,\"res\":\"123\"}";
            //client.PublishMessageAsync("set4GMQTT000801", payloadstr, false, 0);
            //});
            _logger.Warning("仿真excel生成开始");
            try
            {
                var data = await _repo.GetPageList(new SimulationInfoParm() { page = 1, rows = 10000, sort = "id", order = "asc",time = "2023-07-12" });
                _logger.Warning(JsonConvert.SerializeObject(data));
                var sourceFile = _configuration["FangZhen:Path"] + @"file.xlsx";
                var destinationFile = _configuration["FangZhen:Path"] + @"destinationFile" + DateTime.Now.ToString("yyyyMMddHHmm") +".xlsx";

                // 拷贝文件
                File.Copy(sourceFile, destinationFile);

                var file = new FileInfo(destinationFile);
                using (var package = new ExcelPackage(file))
                {
                    // 添加工作表
                    var worksheet = package.Workbook.Worksheets["Sheet1"];

                    // 添加数据
                    worksheet.Cells["A1"].Value = "Hello1" + DateTime.Now;
                    worksheet.Cells["B1"].Value = "World1!" + DateTime.Now;

                    // 保存文件
                    package.Save();
                }
                _logger.Warning("仿真excel生成结束"+ destinationFile);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
            }
            
            
        }
    }
}
