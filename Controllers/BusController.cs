using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MSS.API.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using Serilog.Core;
using SZY.Platform.WebApi.Helper;
using SZY.Platform.WebApi.Model;
using SZY.Platform.WebApi.Service;

namespace SZY.Platform.WebApi.Controllers
{
    [Route("api/[controller]")]//8036
    [ApiController]
    public class BusController : ControllerBase
    {

        private readonly Logger _logger;
        private IHttpContextAccessor _accessor;
        private readonly IConfiguration _configuration;
        private readonly IBusAlarmService _service;

        public BusController(IHttpContextAccessor accessor, IConfiguration configuration, IBusAlarmService service)
        {
            _accessor = accessor;
            _logger = new LoggerConfiguration()
#if DEBUG
        .MinimumLevel.Debug()
#else
        .MinimumLevel.Information()
#endif
        //.MinimumLevel.Override("Microsoft", LogEventLevel.)
        //.Enrich.FromLogContext()
        .WriteTo.RollingFile(@"c:\\BusLogs\Buslog.txt")
        .CreateLogger();
            _configuration = configuration;
            _service = service;
        }


        [HttpPost("AiAlarm")]
        public async Task<ActionResult<ApiResult>> PostAiAlarm(BusRoot parm)
        {
            ApiResult ret = new ApiResult { code = Code.Success };
            try
            {
                var ip = _accessor.HttpContext.Connection.RemoteIpAddress.ToString();
                _logger.Warning("PostAiAlarm 来自:" + ip + "的请求   obj: " + JsonConvert.SerializeObject(parm));
                if (parm != null)
                {
                    await _service.Save(parm);
                    Base64toImg(parm.alarm_picture);
                }
            }
            catch (System.Exception ex)
            {
                _logger.Warning("PostAiAlarm" + ex.Message.ToString());
                ret.msg = string.Format(
                    "异常信息:{0}",
                    ex.Message);
            }
            return ret;
        }

        [NonAction]
        private void Base64toImg(string base64str)
        {
            //站点文件目录
            string fileDir = _configuration["CheZai:Pic"]+ DateTime.Now.Year.ToString() + "\\" + DateTime.Now.Month.ToString() + "\\";
            //文件名称
            string fileName = "chezai" + DateTime.Now.ToString("yyyyMMddHHmmssff");
            //保存文件所在站点位置
            string filePath = Path.Combine(fileDir, fileName);

            if (!System.IO.Directory.Exists(fileDir))
                System.IO.Directory.CreateDirectory(fileDir);



            //将Base64String转为图片并保存
            byte[] arr2 = Convert.FromBase64String(base64str);
            using (MemoryStream ms2 = new MemoryStream(arr2))
            {
                //System.Drawing.Bitmap bmp2 = new System.Drawing.Bitmap(ms2);
                ////bmp2.Save(filePath + ".jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
                ////bmp2.Save(filePath + ".bmp", System.Drawing.Imaging.ImageFormat.Bmp);
                ////bmp2.Save(filePath + ".gif", System.Drawing.Imaging.ImageFormat.Gif);
                //bmp2.Save(filePath + ".png", System.Drawing.Imaging.ImageFormat.Png);

                Bitmap bmpTemp = new Bitmap(ms2);
                Bitmap bmp = new Bitmap(bmpTemp);
                bmpTemp.Dispose();
                bmp.Save(filePath + ".png", System.Drawing.Imaging.ImageFormat.Png);
            }
        }




    }

    public class Alarm_objectsItem
    {
        public int @class { get; set; }

        public string class_name { get; set; }

        public double confidence { get; set; }

        public int x { get; set; }

        public int y { get; set; }

        public int width { get; set; }

        public int height { get; set; }

    }



    public class Alarm_dataItem
    {
        public int alarm_type { get; set; }

        public List<Alarm_objectsItem> alarm_objects { get; set; }

    }



    public class BusRoot
    {

        public string camera_id { get; set; }

        public string site_id { get; set; }

        public double camera_lng { get; set; }

        public double camera_lat { get; set; }

        public string camera_url { get; set; }

        public string camera_name { get; set; }

        public string time { get; set; }

        public string alarm_picture { get; set; }

        public List<Alarm_dataItem> alarm_data { get; set; }

    }


}