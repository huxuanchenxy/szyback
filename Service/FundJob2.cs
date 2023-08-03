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
using System.Collections.Generic;
using System.Linq;
using OfficeOpenXml.Style;
using System.Drawing;
using SZY.Platform.WebApi.Helper;

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
                //var yesterday = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
                var yesterday = "";
                if (_configuration["FangZhen:Yesterday"].ToString() != "")
                {
                    yesterday = _configuration["FangZhen:Yesterday"].ToString();
                }
                else
                {
                    yesterday = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
                }
                _logger.Warning("FangZhen:Yesterday : "+ yesterday);
                var data = await _repo.GetPageList(new SimulationInfoParm() { page = 1, rows = 100000, sort = "id", order = "asc",time = yesterday });
                //_logger.Warning(JsonConvert.SerializeObject(data));

                
                var sourceFile = _configuration["FangZhen:SourcePath"] + _configuration["FangZhen:SourceFileName"];
                var destinationFile = _configuration["FangZhen:TargetPath"] + _configuration["FangZhen:TargetFileName"] + DateTime.Now.ToString("yyyyMMddHHmm") +".xlsx";

                // 拷贝文件
                File.Copy(sourceFile, destinationFile);

                var file = new FileInfo(destinationFile);
                using (var package = new ExcelPackage(file))
                {
                    // 添加工作表
                    var worksheet = package.Workbook.Worksheets["Sheet1"];
                    

                    List<DateObj> list = new List<DateObj>();
                    var timeconfig = _configuration["FangZhen:TimeConfig"].Split(",");
                    string lastalph = _configuration["FangZhen:TimeExcelStart"];
                    for (int i = 0; i < timeconfig.Length; i++)
                    {
                        //int asciiCode = int.Parse(_configuration["FangZhen:TimeExcelStart"]) + i;//g开始
                        string alph = AliyunHelper.GetNextColumnName(lastalph);
                        
                        //_logger.Warning(alph);
                        var cellname = alph + _configuration["FangZhen:TimeExcelStartRow"].ToString();
                        //_logger.Warning(cellname);
                        
                        worksheet.Cells[cellname].Value = timeconfig[i];
                        
                        
                        worksheet.Cells[cellname].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.FromArgb(0, 0, 0));
                        var curtimearr = timeconfig[i].Split("-");
                        var stime = Convert.ToDateTime(yesterday + " " + curtimearr[0] + ":00");
                        var etime = Convert.ToDateTime(yesterday + " " + curtimearr[1] + ":59");
                        //for (int j = 3; j <= 6; j++)
                        //{
                        //    int lane = j - 3 + 1;
                        //    var curcellname = alph + j;
                        //    list.Add(new DateObj() { camera = 1,roadpart = 1,lane = lane, cellName = curcellname, startTime = stime, endTime = etime });
                        //}
                        //for (int j = 7; j <= 9; j++)
                        //{
                        //    int lane = j - 7 + 1;
                        //    var curcellname = alph + j;
                        //    list.Add(new DateObj() { camera = 2, roadpart = 2, lane = lane, cellName = curcellname, startTime = stime, endTime = etime });
                        //}

                        //for (int j = 25; j <= 26; j++)
                        //{
                        //    int lane = j - 25 + 1;
                        //    var curcellname = alph + j;
                        //    list.Add(new DateObj() { camera = 3, roadpart = 3, lane = lane, cellName = curcellname, startTime = stime, endTime = etime });
                        //}
                        //for (int j = 27; j <= 28; j++)
                        //{
                        //    int lane = j - 27 + 1;
                        //    var curcellname = alph + j;
                        //    list.Add(new DateObj() { camera = 4, roadpart = 4, lane = lane, cellName = curcellname, startTime = stime, endTime = etime });
                        //}
                        var excelconfig = _configuration["FangZhen:ExcelLaneConfig"].Split(",");
                        foreach (var curconfigarr in excelconfig)
                        {
                            var curconfig = curconfigarr.Split("-");
                            int jcamera = int.Parse(curconfig[0]);
                            int jrp  = int.Parse(curconfig[1]);
                            int jstart = int.Parse(curconfig[2]);
                            int jmax = int.Parse(curconfig[3]);
                            for (int j = jstart; j <= jmax; j++)
                            {
                                int lane = j - jstart + 1;
                                var curcellname = alph + j;//alph如G  j如27
                                list.Add(new DateObj() { camera = jcamera, roadpart = jrp, lane = lane, cellName = curcellname, startTime = stime, endTime = etime });
                                worksheet.Cells[curcellname].Value = 0;
                                worksheet.Cells[curcellname].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.FromArgb(0, 0, 0));
                            }
                        }
                        lastalph = alph;
                    }
                    //_logger.Warning("list");
                    //_logger.Warning(JsonConvert.SerializeObject(list));
                    foreach (var curd in data.rows)
                    {
                        //_logger.Warning("curd");
                        //_logger.Warning(JsonConvert.SerializeObject(curd));
                        var summ = curd.Summarize;

                        //_logger.Warning(summ);
                        SqlSum dd = JsonConvert.DeserializeObject<SqlSum>(summ);
                        //_logger.Warning("dd");
                        //_logger.Warning(JsonConvert.SerializeObject(dd));
                        //_logger.Warning("dd.laneinfo");
                        //_logger.Warning(JsonConvert.SerializeObject(dd.laneinfo));
                        if (dd.laneinfo != null && dd.laneinfo.Count > 0)
                        {
                            foreach (var lanelist in dd.laneinfo)
                            {
                                //_logger.Warning("lanelist");
                                //_logger.Warning(JsonConvert.SerializeObject(lanelist));
                                //_logger.Warning("curd.Camera");
                                //_logger.Warning(JsonConvert.SerializeObject(curd.Camera));
                                //_logger.Warning("curd.RoadPart");
                                //_logger.Warning(JsonConvert.SerializeObject(curd.RoadPart));
                                //_logger.Warning("lanelist.lane");
                                //_logger.Warning(JsonConvert.SerializeObject(lanelist.lane));
                                //_logger.Warning("curd.Time");
                                //_logger.Warning(JsonConvert.SerializeObject(curd.Time));
                                try
                                {
                                    var excelcellname = list.Where(c => c.camera.ToString() == curd.Camera && c.roadpart.ToString() == curd.RoadPart.ToString() && c.lane == lanelist.lane && (curd.Time >= c.startTime && curd.Time < c.endTime)).FirstOrDefault();
                                    //var excelcellname = list.Where(c => c.camera.ToString() == curd.Camera && c.roadpart.ToString() == curd.RoadPart.ToString() && c.lane == lanelist.lane).FirstOrDefault();
                                    //_logger.Warning("excelcellname");
                                    //_logger.Warning(JsonConvert.SerializeObject(excelcellname));
                                    if (excelcellname != null)
                                    {
                                        //_logger.Warning("worksheet.Cells[excelcellname.cellName].Value");
                                        //_logger.Warning(JsonConvert.SerializeObject(worksheet.Cells[excelcellname.cellName].Value));
                                        if (worksheet.Cells[excelcellname.cellName].Value == null)
                                        {
                                            worksheet.Cells[excelcellname.cellName].Value = lanelist.count.ToString();
                                        }
                                        else
                                        {
                                            worksheet.Cells[excelcellname.cellName].Value = int.Parse(worksheet.Cells[excelcellname.cellName].Value.ToString().Trim()) + lanelist.count;
                                        }
                                        worksheet.Cells[excelcellname.cellName].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.FromArgb(0, 0, 0));

                                    }
                                }
                                catch (Exception ex)
                                {
                                    _logger.Warning("数据出错:" + ex.ToString());
                                }
                                
                            }
                        }
                    }

                    //_logger.Warning(JsonConvert.SerializeObject(list));

                    //// 添加数据
                    //worksheet.Cells["A1"].Value = "Hello1" + DateTime.Now;
                    //worksheet.Cells["B1"].Value = "World1!" + DateTime.Now;

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

    public class DateObj
    {
        public int camera { get; set; }
        public int roadpart { get; set; }
        public int lane { get; set; }
        public string cellName { get; set; }
        public DateTime startTime { get; set; }
        public DateTime endTime { get; set; }
    }

    public class SqlSum
    {
        public string time { get; set; }
        public int? roadpart { get; set; }
        public int? camera { get; set; }
        public int count { get; set; }
        public List<laneinfo> laneinfo { get; set; }
    }

    public class laneinfo
    {
        public int lane { get; set; }
        public int count { get; set; }
    }


}
