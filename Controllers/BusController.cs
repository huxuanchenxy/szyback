using System;
using System.Collections.Generic;
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
using MSS.API.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using Serilog.Core;
using SZY.Platform.WebApi.Helper;
using SZY.Platform.WebApi.Model;

namespace SZY.Platform.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BusController : ControllerBase
    {

        private readonly Logger _logger;
        private IHttpContextAccessor _accessor;

        public BusController(IHttpContextAccessor accessor)
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
        }


        [HttpPost("AiAlarm")]
        public async Task<ActionResult<ApiResult>> PostAiAlarm(BusRoot parm)
        {
            ApiResult ret = new ApiResult { code = Code.Success };
            try
            {
                var ip = _accessor.HttpContext.Connection.RemoteIpAddress.ToString();
                _logger.Warning("PostAiAlarm 来自:" + ip + "的请求   obj: " + JsonConvert.SerializeObject(parm));

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