using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client.Options;
using MSS.API.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Quartz;
using RestSharp;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using SZY.Platform.WebApi.Client;
using SZY.Platform.WebApi.Model;
using SZY.Platform.WebApi.Service;

namespace SZY.Platform.WebApi.Controllers
{
    //[Route("api/v1/[controller]")]
    [ApiController]
    public class WfController : ControllerBase
    {
        private readonly ISchedulerFactory _schedulerFactory;
        private IScheduler _scheduler;
        private QuartzStart _quart;
        private IHttpContextAccessor _accessor;
        private readonly IWorkTaskService _service;
        private readonly Microsoft.Extensions.Logging.ILogger _logger;
        private MosquittoMqttClientService _mqttclientservice;
        private readonly Logger _logger1;
        private readonly Logger _logger2;
        private readonly Logger _logger3;
        private readonly IJingGai2AlarmService _jg2service;
        private readonly IConfiguration _configuration;

        public WfController(IWorkTaskService service, ISchedulerFactory schedulerFactory, QuartzStart quart, IHttpContextAccessor accessor, ILogger<WfController> logger, MosquittoMqttClientService mqttclientservice, IJingGai2AlarmService jg2service, IConfiguration configuration)
        {
            _service = service;
            _jg2service = jg2service;
            this._schedulerFactory = schedulerFactory;
            _quart = quart;
            _accessor = accessor;
            _logger = logger;
            _mqttclientservice = mqttclientservice;
            _configuration = configuration;

            _logger1 = new LoggerConfiguration()
#if DEBUG
        .MinimumLevel.Debug()
#else
        .MinimumLevel.Information()
#endif
        //.MinimumLevel.Override("Microsoft", LogEventLevel.)
        //.Enrich.FromLogContext()
        .WriteTo.RollingFile(@"c:\\SZYLogs\watercontrol.txt")
        .CreateLogger();

            _logger2 = new LoggerConfiguration()
#if DEBUG
        .MinimumLevel.Debug()
#else
        .MinimumLevel.Information()
#endif
        //.MinimumLevel.Override("Microsoft", LogEventLevel.)
        //.Enrich.FromLogContext()
        .WriteTo.RollingFile(@"c:\\JingGaiLogs\log.txt")
        .CreateLogger();


            _logger3 = new LoggerConfiguration()
#if DEBUG
        .MinimumLevel.Debug()
#else
        .MinimumLevel.Information()
#endif
        //.MinimumLevel.Override("Microsoft", LogEventLevel.)
        //.Enrich.FromLogContext()
        .WriteTo.RollingFile(@"c:\\JingGaiLogs\WXlog.txt")
        .CreateLogger();
        }




        [HttpGet("TransportCar")]
        public async Task<ActionResult<ApiResult>> TransportCar([FromQuery] CarQueryParm parm)
        {
            ApiResult retapi = new ApiResult { code = Code.Failure };
            try
            {
                await Publish_FakeCar_Message(_logger, parm.times, parm.sleep, parm.roadpart, parm.direction, parm.carcount, parm.carspeed, parm.firstcarloc, parm.offset1, parm.offset2);
                retapi.code = Code.Success;
            }
            catch (System.Exception ex)
            {
                retapi.msg = string.Format(
                    "异常信息:{0}",
                    ex.Message);
            }
            return retapi;
        }

        [HttpGet("ReceiveTransportCar")]
        public async Task<ActionResult<ApiResult>> ReceiveTransportCar([FromQuery] string parm)
        {
            ApiResult ret = new ApiResult { code = Code.Failure };
            try
            {
                //DateTime startTime = TimeZoneInfo.ConvertTime(new DateTime(1970, 1, 1, 8, 0, 0, 0), TimeZoneInfo.Local);
                //long t = (DateTime.Now.Ticks - startTime.Ticks) / 10000;   //除10000调整为13位 

                //client = new MqttFactory().CreateMqttClient();
                //client.UseApplicationMessageReceivedHandler(OnMessage);
                //await client.StartClientAsync();
                //await StartClientAsync();
                //await _mqttclientservice.StartAsync(CancellationToken.None);
            }
            catch (System.Exception ex)
            {
                ret.msg = string.Format(
                    "异常信息:{0}",
                    ex.Message);
            }
            return ret;
        }



        [HttpPost("JingGaiData")]
        public async Task<ActionResult<JingGaiRet>> JingGaiData(JingGaiObj parm)
        {
            JingGaiRet ret = new JingGaiRet { success = true };
            try
            {
                var ip = _accessor.HttpContext.Connection.RemoteIpAddress.ToString();
                _logger.LogWarning("JingGaiData 来自:" + ip + "的请求 deviceid: " + parm.device_id + " obj: " + JsonConvert.SerializeObject(parm));
                //ret = await _service.QueryReadyActivityInstance(parm);
                JingGai data = new JingGai()
                {
                    device_id = parm.device_id,
                    device_type = parm.device_type,
                    fv = parm.data.fv,
                    soc = parm.data.soc,
                    sor = parm.data.sor,
                    rtd = parm.data.rtd,
                    rad = parm.data.rad,
                    rqd = parm.data.rqd,
                    sol = parm.data.sol,
                    lng = parm.data.lng,
                    lat = parm.data.lat,
                    sensor_water_level = parm.data.sensor_water_level,
                    sensor_water_depth = parm.data.sensor_water_depth,
                    sensor_temperature = parm.data.sensor_temperature,
                    sensor_humidity = parm.data.sensor_humidity,
                    sensor_smoke = parm.data.sensor_smoke,
                    sensor_ch4 = parm.data.sensor_ch4,
                    sensor_toxic = parm.data.sensor_toxic,
                    sensor_water_alarm = parm.data.sensor_water_alarm,
                    sensor_water_warn = parm.data.sensor_water_warn,
                    sensor_ph = parm.data.sensor_ph,
                    sensor_ch4_conc = parm.data.sensor_ch4_conc,
                    sensor_toxic_conc = parm.data.sensor_toxic_conc,
                    date1 = DateTime.Now.ToString()
                };
                await _service.AddJingGai(data);
                _logger.LogWarning("JingGaiData 插入成功");
            }
            catch (System.Exception ex)
            {
                _logger.LogWarning("JingGaiData" + ex.Message.ToString());
                ret.errmsg = string.Format(
                    "异常信息:{0}",
                    ex.Message);
            }
            return ret;
        }


        [HttpPost("JingGaiDevice")]
        public async Task<ActionResult<JingGaiRet>> JingGaiDevice(JingGaiDevice parm)
        {
            JingGaiRet ret = new JingGaiRet { success = true };
            try
            {
                var ip = _accessor.HttpContext.Connection.RemoteIpAddress.ToString();
                _logger.LogWarning("JingGaiDevice 来自:" + ip + "的请求 deviceid: " + parm.device_id + " obj: " + JsonConvert.SerializeObject(parm));
                //ret = await _service.QueryReadyActivityInstance(parm);
                JingGaiDevice data = new JingGaiDevice()
                {
                    device_id = parm.device_id,
                    device_type = parm.device_type,
                    device_name = parm.device_name,
                    install_addr = parm.install_addr,
                    install_time = parm.install_time,
                    lng = parm.lng,
                    lat = parm.lat,
                    date1 = DateTime.Now.ToString()
                };
                await _service.AddJingGaiDevice(data);
                _logger.LogWarning("JingGaiDevice db插入成功");
            }
            catch (System.Exception ex)
            {
                _logger.LogWarning("JingGaiDevice" + ex.Message.ToString());
                ret.errmsg = string.Format(
                    "异常信息:{0}",
                    ex.Message);
            }
            return ret;
        }


        [HttpPost("JingGaiAlarm")]
        public async Task<ActionResult<JingGaiRet>> JingGaiAlarm(JingGaiAlarmObj parm)
        {
            var ip = _accessor.HttpContext.Connection.RemoteIpAddress.ToString();
            _logger.LogWarning("JingGaiAlarm 来自:" + ip + "的请求 deviceid: " + parm.device_id + " obj: " + JsonConvert.SerializeObject(parm));
            JingGaiRet ret = new JingGaiRet { success = true };
            try
            {
                await Publish_Application_Message();
                List<JingGaiAlarm> jgalarmlist = new List<JingGaiAlarm>();
                jgalarmlist.Add(new Model.JingGaiAlarm() { alarm_item = "soc", alarm_item_name = "剩余电量" });
                jgalarmlist.Add(new Model.JingGaiAlarm() { alarm_item = "rtd", alarm_item_name = "实时温度" });
                jgalarmlist.Add(new Model.JingGaiAlarm() { alarm_item = "rad", alarm_item_name = "倾斜角度" });
                jgalarmlist.Add(new Model.JingGaiAlarm() { alarm_item = "rqd", alarm_item_name = "震动幅度" });
                jgalarmlist.Add(new Model.JingGaiAlarm() { alarm_item = "sol", alarm_item_name = "锁状态" });
                jgalarmlist.Add(new Model.JingGaiAlarm() { alarm_item = "sensor_water_level", alarm_item_name = "水面距离" });
                jgalarmlist.Add(new Model.JingGaiAlarm() { alarm_item = "sensor_water_depth", alarm_item_name = "水面深度" });
                jgalarmlist.Add(new Model.JingGaiAlarm() { alarm_item = "sensor_temperature", alarm_item_name = "温度" });
                jgalarmlist.Add(new Model.JingGaiAlarm() { alarm_item = "sensor_humidity", alarm_item_name = "湿度" });
                jgalarmlist.Add(new Model.JingGaiAlarm() { alarm_item = "sensor_smoke", alarm_item_name = "烟雾报警" });
                jgalarmlist.Add(new Model.JingGaiAlarm() { alarm_item = "sensor_ch4", alarm_item_name = "可燃气体报警" });
                jgalarmlist.Add(new Model.JingGaiAlarm() { alarm_item = "sensor_water_alarm", alarm_item_name = "水位报警" });
                jgalarmlist.Add(new Model.JingGaiAlarm() { alarm_item = "sensor_toxic", alarm_item_name = "有毒气体报警" });
                jgalarmlist.Add(new Model.JingGaiAlarm() { alarm_item = "sensor_water_warn", alarm_item_name = "水位预警" });
                jgalarmlist.Add(new Model.JingGaiAlarm() { alarm_item = "sensor_ph", alarm_item_name = "酸碱度" });
                jgalarmlist.Add(new Model.JingGaiAlarm() { alarm_item = "sensor_toxic_conc", alarm_item_name = "有毒气体浓度" });
                jgalarmlist.Add(new Model.JingGaiAlarm() { alarm_item = "sensor_ch4_conc", alarm_item_name = "可燃气体浓度" });

                JingGaiDevicePageView device = await _service.GetPageJingGaiDevice(new JingGaiDeviceParm() { });
                if (parm != null)
                {
                    string device_id = parm.device_id;
                    string device_type = parm.device_type;
                    string device_name = string.Empty;
                    string device_addr = string.Empty;
                    if (device.rows != null && device.rows.Count > 0)
                    {
                        List<JingGaiDevice> list = device.rows;
                        JingGaiDevice deviceobj = list.Where(c => c.device_id == device_id).FirstOrDefault();
                        device_name = deviceobj.device_name;
                        device_addr = deviceobj.install_addr;
                    }
                    if (parm.alarm_data != null && parm.alarm_data.Count > 0)
                    {
                        for (int i = 0; i < parm.alarm_data.Count; i++)
                        {
                            string alarm_item = parm.alarm_data[i].alarm_item;
                            string alarm_value = parm.alarm_data[i].alarm_value;
                            var temp = jgalarmlist.Where(c => c.alarm_item == alarm_item).FirstOrDefault();
                            string alarm_item_name = string.Empty;
                            if (temp != null)
                            {
                                alarm_item_name = temp.alarm_item_name;
                            }

                            var threshold_conf = parm.alarm_data[i].threshold_conf;
                            if (threshold_conf != null && threshold_conf.Count > 0)
                            {
                                for (int j = 0; j < threshold_conf.Count; j++)
                                {
                                    string compare_type = threshold_conf[j].compare_type;
                                    string threshold = threshold_conf[j].threshold;

                                    JingGaiAlarm obj = new JingGaiAlarm()
                                    {
                                        device_id = device_id,
                                        device_type = int.Parse(device_type),
                                        device_name = device_name,
                                        device_addr = device_addr,
                                        alarm_item = alarm_item,
                                        alarm_value = alarm_value,
                                        alarm_item_name = alarm_item_name,
                                        compare_type = compare_type,
                                        threshold = threshold,
                                        date1 = DateTime.Now.ToString()
                                    };
                                    await _service.AddJingGaiAlarm(obj);
                                    _logger.LogWarning("JingGaiAlarm db插入成功1");
                                }
                            }
                            else
                            {
                                JingGaiAlarm obj = new JingGaiAlarm()
                                {
                                    device_id = device_id
                                    ,
                                    device_type = int.Parse(device_type)
                                    ,
                                    device_name = device_name
                                    ,
                                    device_addr = device_addr
                                    ,
                                    alarm_item = alarm_item
                                    ,
                                    alarm_value = alarm_value
                                    ,
                                    alarm_item_name = alarm_item_name
                                    ,
                                    date1 = DateTime.Now.ToString()
                                };
                                await _service.AddJingGaiAlarm(obj);
                                _logger.LogWarning("JingGaiAlarm db插入成功2");
                            }
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogWarning("JingGaiAlarm" + ex.Message.ToString());
                ret.errmsg = string.Format(
                    "异常信息:{0}",
                    ex.Message);
            }
            return ret;
        }


        [HttpPost("JingGaiApi")]
        public async Task<ActionResult<JingGaiRet2>> JingGaiApi(JObject json)
        {
            JingGaiRet2 ret = new JingGaiRet2 { success = true };
            try
            {
                //AppID: kygjIYuuImUQzfiD
                //AppSecret: ZpP2XVr6ILoByCWcgKxcTu3SRP5II0LE
            var ip = _accessor.HttpContext.Connection.RemoteIpAddress.ToString();
                _logger2.Warning("JingGaiApi 来自:" + ip + "的请求 " + json);

                JingGaiJson jsonobj = JsonConvert.DeserializeObject<JingGaiJson>(json.ToString());

                //alarm_data 报警数据推送
                //property 属性推送
                //device_create 设备添加推送
                //device_update 设备编辑推送
                //device_delete 设备删除推送
                string topic = string.Empty;
                switch (jsonobj.push_type)
                {
                    case "alarm_data":
                        topic = "/jinggai/alarm_data";
                        OpenApiJingGai2Alarm json1 = JsonConvert.DeserializeObject<OpenApiJingGai2Alarm>(json.ToString());
                        try
                        {
                            JingGai2AlarmToDB(json1);
                            _logger2.Warning("JingGai2AlarmToDB 成功 ");
                        }
                        catch (Exception ex)
                        {
                            _logger2.Warning("JingGai2AlarmToDB 失败: " + ex.Message.ToString());
                        }
                        break;
                    case "property":
                        topic = "/jinggai/property";
                        break;
                    case "device_create":
                        topic = "/jinggai/device_create";
                        break;
                    case "device_update":
                        topic = "/jinggai/device_update";
                        break;
                    case "device_delete":
                        topic = "/jinggai/device_delete";
                        break;
                }
                if (string.IsNullOrEmpty(topic))
                {
                    ret.err_msg = "empty pushtype";
                    ret.err_code = -1;
                    ret.success = false;
                    return ret;
                }

                var mqttFactory = new MqttFactory();
                
                string payloadstr = json.ToString();
                using (var mqttClient = mqttFactory.CreateMqttClient())
                {
                    var mqttClientOptions = new MqttClientOptionsBuilder()
                        //.WithTcpServer("broker.hivemq.com")
                        .WithWebSocketServer("ws://"+_configuration["MQTTSet:Ip"].ToString()+":8083/mqtt")
                        .Build();

                    await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

                    var applicationMessage = new MqttApplicationMessageBuilder()
                        .WithTopic(topic)
                        .WithPayload(payloadstr)
                        .Build();

                    if (mqttClient != null)
                    {
                        await mqttClient.PublishAsync(applicationMessage, CancellationToken.None);
                    }
                    _logger2.Warning("已发送至:" + topic);
                    _logger2.Warning(json.ToString());
                    //Console.WriteLine("MQTT application message is published.");
                }
            }
            catch (System.Exception ex)
            {
                _logger2.Warning("JingGaiApi " + ex.Message.ToString());
                ret.success = false;
                ret.err_code = -1;
                ret.err_msg = string.Format(
                    "异常信息:{0}",
                    ex.Message);
            }
            return ret;
        }

        private async void JingGai2AlarmToDB(OpenApiJingGai2Alarm json)
        {
            if (json != null)
            {
                if (json.payload != null && json.payload.Count > 0)
                {
                    foreach (var p in json.payload)
                    {
                        if (p.alarm_settings != null && p.alarm_settings.Count > 0)
                        {
                            var et = await GetJingGai2DeviceDetail(json.client_id);
                            if (et.data != null)
                            {
                                foreach (var s in p.alarm_settings)
                                {
                                    JingGai2Alarm obj = new JingGai2Alarm();
                                    obj.client_id = json.client_id;
                                    obj.client_name = et.data.device_name;
                                    var curgroup = et.data.groups;
                                    //obj.client_group = et.data.device_name.Substring(0, 3);//取100@的3位
                                    obj.client_group = curgroup != null && curgroup.Count > 0 ? curgroup[0].group_name : "";
                                    obj.model_type = json.model_type;
                                    obj.alarm_type = p.alarm_type;
                                    obj.alarm_level = p.alarm_level;
                                    obj.identifier = p.identifier;
                                    obj.value = Convert.ToString(p.value);
                                    obj.alarm_time = p.alarm_time;
                                    obj.alarm_settings_title = s.title;
                                    obj.alarm_settings_identifier = s.identifier;
                                    obj.alarm_settings_alarm_type = s.alarm_type;
                                    obj.alarm_settings_alarm_level = s.alarm_level;
                                    obj.alarm_settings_compare = s.compare;
                                    obj.alarm_settings_value = Convert.ToString(s.value);
                                    await _jg2service.Save(obj);
                                }
                            }
                            
                        }
                    }
                }
            }
        }


        [HttpPost("JingGaiOpenApiDevice")]
        public async Task<ActionResult<OpenApiDeviceObj>> JingGaiOpenApiDevice(int page_index,int page_size,string search_key)
        {
            OpenApiDeviceObj ret = new OpenApiDeviceObj { success = true };
            try
            {
                //AppID: kygjIYuuImUQzfiD
                //AppSecret: ZpP2XVr6ILoByCWcgKxcTu3SRP5II0LE
                var client = new RestClient("https://things.cdjyl.com.cn:12000/api/v1/open_api/device?page_index="+ page_index + "&page_size="+ page_size + "&search_key="+ search_key);
                client.Timeout = -1;
                string timestamp = CurrentTimeStamp(true).ToString();
                string rand = CreateRandCdkeys(16);
                string appid = "kygjIYuuImUQzfiD";
                string str = "app_id="+appid+"&nonce="+ rand + "&timestamp="+ timestamp;
                string sign = HmacSHA256(str, "ZpP2XVr6ILoByCWcgKxcTu3SRP5II0LE");
                _logger2.Warning("JingGaiOpenApiDevice请求字符串:" + str);
                _logger2.Warning("JingGaiOpenApiDevice签名:" + sign);
                var request = new RestRequest(Method.GET);
                request.AddHeader("X-App-ID", appid);
                request.AddHeader("X-Timestamp", timestamp);//"1662098451"
                request.AddHeader("X-Nonce", rand);//"NMEtgrGslbA4QRD0"
                request.AddHeader("X-Sign", sign);
                IRestResponse response = await client.ExecuteAsync(request);
                string content = response.Content;

                ret = JsonConvert.DeserializeObject<OpenApiDeviceObj>(content);

            }
            catch (System.Exception ex)
            {
                _logger2.Warning("JingGaiOpenApiDevice " + ex.Message.ToString());
                ret.success = false;
                ret.err_code = -1;
                ret.err_msg = string.Format(
                    "异常信息:{0}",
                    ex.Message);
            }
            return ret;
        }

        private async Task<OpenApiDeviceDetail> GetJingGai2DeviceDetail(string client_id)
        {
            OpenApiDeviceDetail ret = null;
            var client = new RestClient("https://things.cdjyl.com.cn:12000/api/v1/open_api/device/detail?client_id="+ client_id);
            client.Timeout = -1;
            string timestamp = CurrentTimeStamp(true).ToString();
            string rand = CreateRandCdkeys(16);
            string appid = "kygjIYuuImUQzfiD";
            string str = "app_id=" + appid + "&nonce=" + rand + "&timestamp=" + timestamp;
            string sign = HmacSHA256(str, "ZpP2XVr6ILoByCWcgKxcTu3SRP5II0LE");
            _logger2.Warning("GetJingGai2DeviceDetail请求字符串:" + str);
            _logger2.Warning("GetJingGai2DeviceDetail签名:" + sign);
            var request = new RestRequest(Method.GET);
            request.AddHeader("X-App-ID", appid);
            request.AddHeader("X-Timestamp", timestamp);//"1662098451"
            request.AddHeader("X-Nonce", rand);//"NMEtgrGslbA4QRD0"
            request.AddHeader("X-Sign", sign);
            IRestResponse response = await client.ExecuteAsync(request);
            string content = response.Content;

            ret = JsonConvert.DeserializeObject<OpenApiDeviceDetail>(content);
            return ret;
        }

        public static long CurrentTimeStamp(bool isMinseconds = false)
         {
             var ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            long times = Convert.ToInt64(isMinseconds ? ts.TotalMilliseconds : ts.TotalSeconds);
             return times;
        }


        public static string CreateRandCdkeys(int x)
        {
            string[] codeSerial = new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };
            Random rand = new Random();
            int temp = -1;
            string cdKey = string.Empty;
            for (int i = 0; i < 16; i++)
            {
                if (temp != -1)
                {
                    rand = new Random(x + i * temp * unchecked((int)DateTime.Now.Ticks));
                }
                int randIndex = rand.Next(0, 35);
                temp = randIndex;
                cdKey += codeSerial[randIndex];
            }
            return cdKey;
        }

        private string HmacSHA256(string secret, string signKey)
        {
            //HMACSHA1加密
            HMACSHA256 hmacsha1 = new HMACSHA256();
            hmacsha1.Key = System.Text.Encoding.UTF8.GetBytes(signKey);

            byte[] dataBuffer = System.Text.Encoding.UTF8.GetBytes(secret);
            byte[] hashBytes = hmacsha1.ComputeHash(dataBuffer);
            String result = BitConverter.ToString(hashBytes);
            result = result.Replace("-", "").ToLower();
            return result;
        }


        [HttpPost("SunWaterOpen")]
        public async Task<ActionResult<ApiResult>> SunWaterOpen()
        {
            ApiResult ret = new ApiResult { code = 0, msg = "success" };
            try
            {
                var mqttFactory = new MqttFactory();
                string payloadstr = "{\"A02\":110000,\"res\":\"123\"}";
                using (var mqttClient = mqttFactory.CreateMqttClient())
                {
                    var mqttClientOptions = new MqttClientOptionsBuilder()
                        //.WithTcpServer("broker.hivemq.com")
                        .WithWebSocketServer("ws://" + _configuration["MQTTSet:Ip"].ToString() + ":8083/mqtt")
                        .Build();

                    await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

                    var applicationMessage = new MqttApplicationMessageBuilder()
                        .WithTopic("/set/4GMQTT000801")
                        .WithPayload(payloadstr)
                        .Build();

                    if (mqttClient != null)
                    {
                        await mqttClient.PublishAsync(applicationMessage, CancellationToken.None);
                    }
                    _logger1.Warning("/set/4GMQTT000801/manulopen");
                    _logger1.Warning(payloadstr);
                    //Console.WriteLine("MQTT application message is published.");
                }
            }
            catch (System.Exception ex)
            {
                ret.msg = string.Format(
                    "异常信息:{0}",
                    ex.Message);
            }
            return ret;
        }


        [HttpPost("SunWaterShutDown")]
        public async Task<ActionResult<ApiResult>> SunWaterShutDown()
        {
            ApiResult ret = new ApiResult { code = 0, msg = "success" };
            try
            {
                var mqttFactory = new MqttFactory();
                string payloadstr = "{\"A02\":100000,\"res\":\"123\"}";
                using (var mqttClient = mqttFactory.CreateMqttClient())
                {
                    var mqttClientOptions = new MqttClientOptionsBuilder()
                        //.WithTcpServer("broker.hivemq.com")
                        .WithWebSocketServer("ws://" + _configuration["MQTTSet:Ip"].ToString() + ":8083/mqtt")
                        .Build();

                    await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

                    var applicationMessage = new MqttApplicationMessageBuilder()
                        .WithTopic("/set/4GMQTT000801")
                        .WithPayload(payloadstr)
                        .Build();

                    if (mqttClient != null)
                    {
                        await mqttClient.PublishAsync(applicationMessage, CancellationToken.None);
                    }
                    _logger1.Warning("/set/4GMQTT000801/manulshutdown");
                    _logger1.Warning(payloadstr);
                    //Console.WriteLine("MQTT application message is published.");
                }
            }
            catch (System.Exception ex)
            {
                ret.msg = string.Format(
                    "异常信息:{0}",
                    ex.Message);
            }
            return ret;
        }
        private async Task Publish_Application_Message()
        {
            /*
             * This sample pushes a simple application message including a topic and a payload.
             *
             * Always use builders where they exist. Builders (in this project) are designed to be
             * backward compatible. Creating an _MqttApplicationMessage_ via its constructor is also
             * supported but the class might change often in future releases where the builder does not
             * or at least provides backward compatibility where possible.
             */

            var mqttFactory = new MqttFactory();

            using (var mqttClient = mqttFactory.CreateMqttClient())
            {
                var mqttClientOptions = new MqttClientOptionsBuilder()
                    //.WithTcpServer("broker.hivemq.com")
                    .WithWebSocketServer("ws://" + _configuration["MQTTSet:Ip"].ToString() + ":8083/mqtt")
                    .Build();

                await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

                var applicationMessage = new MqttApplicationMessageBuilder()
                    .WithTopic("alarm")
                    .WithPayload(DateTime.Now.ToString())
                    .Build();

                await mqttClient.PublishAsync(applicationMessage, CancellationToken.None);

                //Console.WriteLine("MQTT application message is published.");
            }
        }

        //[
        //    {"carid":"沪a8888","cartype":1,"carcolor":2,"distance":100,"curx":36.3131600729,"cury":36.3131600729,"roadlane":3},
        //    {"carid":"沪a7777","cartype":1,"carcolor":2,"distance":200,"curx":36.3131600729,"cury":36.3131600729,"roadlane":1},

        //   { "carid":"沪a9999","cartype":1,"carcolor":2,"distance":300,"curx":36.3131600729,"cury":36.3131600729,"roadlane":2}
        //]   

        private async Task<List<carinfo>> GenerCar(int singlecarcount, int firststart, int offset1, int offset2)
        {
            List<carinfo> ret = new List<carinfo>();
            for (int i = 1; i <= 3; i++)//模拟三根车道
            {
                int roadlane = i;
                string caridfront = "沪A" + i;
                int curroadlanecarcount = singlecarcount;

                Random ran1 = new Random();
                int firstcarstart = ran1.Next(0, firststart);
                int distance = 0;
                for (int j = 1; j <= curroadlanecarcount; j++)
                {
                    string curcarid = caridfront + j.ToString();
                    string curnum = i.ToString() + j.ToString();
                    //模拟第一次该车辆的起点位置

                    if (j == 1)
                    {
                        distance = firstcarstart;
                    }
                    else
                    {
                        Random ran2 = new Random();
                        int flowdistance = ran2.Next(offset1, offset2);//下一辆车的偏移距离
                        distance = distance + flowdistance;
                    }

                    Random ran3 = new Random();
                    int cartype = ran3.Next(1, 3);


                    Random ran4 = new Random();
                    int carcolor = ran4.Next(1, 7);
                    ret.Add(new carinfo()
                    {
                        num = int.Parse(curnum),
                        carid = curcarid,
                        cartype = cartype,
                        carcolor = carcolor,
                        distance = distance,
                        roadlane = roadlane
                    });
                }
            }
            return ret;
        }
        private async Task Publish_FakeCar_Message(Microsoft.Extensions.Logging.ILogger _logger, int times, int sleep, int rp, int dirc, int carcount, int cspeed, int firstcarloccation, int offset1, int offset2)
        {

            var mqttFactory = new MqttFactory();

            using (var mqttClient = mqttFactory.CreateMqttClient())
            {
                var mqttClientOptions = new MqttClientOptionsBuilder()
                    //.WithTcpServer("broker.hivemq.com")
                    .WithWebSocketServer("ws://" + _configuration["MQTTSet:Ip"].ToString() + ":8083/mqtt")
                    .Build();

                await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

                List<carinfo> carinfolist = await GenerCar(carcount, firstcarloccation, offset1, offset2);
                int carspeed = cspeed;
                for (int i = 0; i < times; i++)//时间间隔
                {
                    Thread.Sleep(sleep);
                    transportret ret = new transportret();
                    ret.code = 0;
                    //DateTime dtStart = TimeZoneInfo.ConvertTimeFromUtc(new DateTime(1970, 1, 1, 0, 0, 0), TimeZoneInfo.Local);
                    //long timeStamp = Convert.ToInt32((DateTime.Now - dtStart).TotalMilliseconds);
                    DateTime startTime = TimeZoneInfo.ConvertTime(new DateTime(1970, 1, 1, 8, 0, 0, 0), TimeZoneInfo.Local);
                    long t = (DateTime.Now.Ticks - startTime.Ticks) / 10000;   //除10000调整为13位 
                    //TimeSpan ts = new TimeSpan(DateTime.Now.Ticks);
                    //ret.timestamp = ts.TotalMilliseconds.ToString();
                    ret.timestamp = t.ToString();
                    ret.msg = "成功";

                    if (i != 0)//第二次开始，要让车辆动起来，则需要改变distance
                    {
                        for (int j = 0; j < carinfolist.Count; j++)
                        {
                            carinfolist[j].distance = carinfolist[j].distance + carspeed;
                        }
                    }
                    ret.result = new transportresult() { roadpart = rp, direction = dirc, carinfo = carinfolist, camera = "K4+940" };
                    string payload = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:ms");

                    string payloadstr = JsonConvert.SerializeObject(ret);

                    _logger.LogWarning(payloadstr);
                    var applicationMessage = new MqttApplicationMessageBuilder()
                    .WithTopic("transport/car/frontmock")
                    .WithPayload(payloadstr)
                                        .Build();

                    await mqttClient.PublishAsync(applicationMessage, CancellationToken.None);
                }
                //var applicationMessage = new MqttApplicationMessageBuilder()
                //    .WithTopic("transport/car")
                //    .WithPayload(DateTime.Now.ToString())
                //    .Build();

                //await mqttClient.PublishAsync(applicationMessage, CancellationToken.None);

                //Console.WriteLine("MQTT application message is published.");
            }
        }



    }


    public class CarQueryParm
    {
        public int times { get; set; }
        public int sleep { get; set; }
        public int roadpart { get; set; }
        public int direction { get; set; }
        public int carcount { get; set; }
        public int carspeed { get; set; }
        public int firstcarloc { get; set; }
        public int offset1 { get; set; }
        public int offset2 { get; set; }
    }
    public class carinfo
    {
        public int num { get; set; }
        public string carid { get; set; }
        public int cartype { get; set; }
        public int carcolor { get; set; }
        public int distance { get; set; }
        public string curx { get; set; }
        public string cury { get; set; }
        public int roadlane { get; set; }
    }

    public class transportret
    {
        public int code { get; set; }
        public string timestamp { get; set; }
        public string msg { get; set; }
        public transportresult result { get; set; }
    }

    public class transportresult
    {
        public int roadpart { get; set; }
        public string roadpartx { get; set; }
        public string roadparty { get; set; }
        public int direction { get; set; }
        public string camera { get; set; }
        public List<carinfo> carinfo { get; set; }
    }

    public class JingGaiRet
    {
        public bool success { get; set; }
        public string errmsg { get; set; }
    }

    public class JingGaiRet2
    {
        public bool success { get; set; }
        public int err_code { get; set; }
        public string err_msg { get; set; }

        public object obj { get; set; }
    }

    public class OpenApiDeviceObj : JingGaiRet2
    {
        public OpenApiDeviceData data { get; set; }
    }

    public class OpenApiDeviceData
    {
        public List<OpenApiDeviceEntity> data { get; set; }
        public int total { get; set; }
    }

    public class OpenApiDeviceDetail : JingGaiRet2
    {
        public OpenApiDeviceEntity data { get; set; }
    }

    public class OpenApiDeviceEntity
    {
        public int id { get; set; }
        public string device_name { get; set; }
        public string model_type { get; set; }
        public string client_id { get; set; }
        public string addr { get; set; }
        public float longitude { get; set; }
        public float latitude { get; set; }
        public bool is_alarm { get; set; }
        public Int64 active_at { get; set; }
        public Int64 last_upload_at { get; set; }
        public string status { get; set; }
        public List<OpenApiDeviceGroup> groups { get; set; }
    }

    public class OpenApiDeviceGroup
    {
        public int id { get; set; }
        public string group_name { get; set; }
    }

    public class OpenApiJingGai2Alarm: JingGaiJson
    {
        public new List<OpenApiJingGai2AlarmPayload> payload { get; set; }
    }

    public class OpenApiJingGai2AlarmPayload
    {
        public string alarm_type { get; set; }
        public string alarm_level { get; set; }
        public string identifier { get; set; }
        public dynamic value { get; set; }
        public string alarm_time { get; set; }
        public List<OpenApiJingGai2AlarmPayloadSettings> alarm_settings { get; set; }
    }

    public class OpenApiJingGai2AlarmPayloadSettings
    {
        public string title { get; set; }
        public string identifier { get; set; }
        public string alarm_type { get; set; }
        public string alarm_level { get; set; }
        public string compare { get; set; }
        public dynamic value { get; set; }
    }

    public class JingGaiJson
    {
        public string client_id { get; set; }
        public string model_type { get; set; }
        public string push_type { get; set; }
        public dynamic payload { get; set; }
    }



//如果好用，请收藏地址，帮忙分享。
public class JingGaiData
    {
        /// <summary>
        /// 固件版本
        /// </summary>
        public string fv { get; set; }
        /// <summary>
        /// 剩余电量
        /// </summary>
        public string soc { get; set; }
        /// <summary>
        /// 运行状态 0维护 1正常
        /// </summary>
        public string sor { get; set; }
        /// <summary>
        /// 实时温度
        /// </summary>
        public string rtd { get; set; }
        /// <summary>
        /// 倾斜角度
        /// </summary>
        public string rad { get; set; }
        /// <summary>
        /// 震动幅度
        /// </summary>
        public string rqd { get; set; }
        /// <summary>
        /// 锁状态 0开 1关 E故障 X未知
        /// </summary>
        public string sol { get; set; }
        /// <summary>
        /// 经度
        /// </summary>
        public string lng { get; set; }
        /// <summary>
        /// 纬度
        /// </summary>
        public string lat { get; set; }
        /// <summary>
        /// 水面距离 单位厘米
        /// </summary>
        public string sensor_water_level { get; set; }
        /// <summary>
        /// 水面深度 单位厘米
        /// </summary>
        public string sensor_water_depth { get; set; }
        /// <summary>
        /// 温度
        /// </summary>
        public string sensor_temperature { get; set; }
        /// <summary>
        /// 湿度
        /// </summary>
        public string sensor_humidity { get; set; }
        /// <summary>
        /// 烟雾报警 0正常 1报警
        /// </summary>
        public string sensor_smoke { get; set; }
        /// <summary>
        /// 可燃气体报警 0正常 1报警
        /// </summary>
        public string sensor_ch4 { get; set; }
        /// <summary>
        /// 有毒气体报警 0正常 1报警
        /// </summary>
        public string sensor_toxic { get; set; }
        /// <summary>
        /// 水位报警 0正常 1报警
        /// </summary>
        public string sensor_water_alarm { get; set; }
        /// <summary>
        /// 水位预警 0正常 1报警
        /// </summary>
        public string sensor_water_warn { get; set; }
        /// <summary>
        /// 酸碱度
        /// </summary>
        public string sensor_ph { get; set; }
        /// <summary>
        /// 可燃气体浓度
        /// </summary>
        public string sensor_ch4_conc { get; set; }
        /// <summary>
        /// 有毒气体浓度
        /// </summary>
        public string sensor_toxic_conc { get; set; }
    }

    public class JingGaiObj
    {
        /// <summary>
        /// 设备ID
        /// </summary>
        public string device_id { get; set; }
        /// <summary>
        /// 设备类型 1:井盖
        /// </summary>
        public int device_type { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public JingGaiData data { get; set; }
    }

    public class ThresholdConf
    {
        public string compare_type { get; set; }
        public string threshold { get; set; }

    }

    public class JingGaiAlarmData
    {
        public string alarm_item { get; set; }
        public string alarm_value { get; set; }
        public List<ThresholdConf> threshold_conf { get; set; }
    }

    public class JingGaiAlarmObj
    {
        public string device_id { get; set; }
        public string device_type { get; set; }
        public List<JingGaiAlarmData> alarm_data { get; set; }
    }

}
