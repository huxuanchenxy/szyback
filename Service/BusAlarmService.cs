using MSS.API.Common;
using MSS.API.Common.Utility;
using SZY.Platform.WebApi.Data;
using SZY.Platform.WebApi.Model;
using System;
using System.Net;
using System.Threading.Tasks;
using SZY.Platform.WebApi.Controllers;
using Newtonsoft.Json;


// Coded By admin 2019/11/9 13:46:57
namespace SZY.Platform.WebApi.Service
{
    public interface IBusAlarmService
    {
        Task<ApiResult> Save(BusRoot obj);
    }

    public class BusAlarmService : IBusAlarmService
    {
        private readonly IBusAlarmRepo<BusAlarm> _repo;
        //private readonly IAuthHelper _authhelper;
        //private readonly int _userID;

        public BusAlarmService(IBusAlarmRepo<BusAlarm> repo)
        {
            _repo = repo;
            //_userID = _authhelper.GetUserId();
        }
        public async Task<ApiResult> Save(BusRoot obj)
        {
            ApiResult ret = new ApiResult();
            ret.code = Code.Success;
            try
            {
                if (obj != null)
                {
                    if (obj.alarm_data != null && obj.alarm_data.Count > 0)
                    {
                        
                        string camera_id = obj.camera_id;
                        string site_id = obj.site_id;
                        double camera_lng = obj.camera_lng;
                        double camera_lat = obj.camera_lat;
                        string camera_url = obj.camera_url;
                        string camera_name = obj.camera_name;
                        string alarm_picture = obj.alarm_picture;
                        DateTime time = Convert.ToDateTime(obj.time);
                        foreach (var alarmobj in obj.alarm_data)
                        {
                            BusAlarm ent = new BusAlarm();
                            int alarm_type = alarmobj.alarm_type;
                            string alarm_obj = JsonConvert.SerializeObject(alarmobj.alarm_objects);
                            ent.camera_id = camera_id;
                            ent.site_id = site_id;
                            ent.camera_lng = camera_lng;
                            ent.camera_lat = camera_lat;
                            ent.camera_url = camera_url;
                            ent.camera_name = camera_name;
                            ent.alarm_type = alarm_type;
                            ent.alarm_objects = alarm_obj;
                            ent.alarm_picture = alarm_picture;
                            ent.alarm_des = "";
                            ent.time = time;
                            switch (alarm_type)
                            {
                                case 1:
                                    ent.alarm_des = "有人闯入";
                                    break;
                                case 2:
                                    ent.alarm_des = "发现明火";
                                    break;
                            }
                            await _repo.Save(ent);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ret.code = Code.Failure;
                ret.msg = ex.Message;
                
            }
            return ret;
        }



    }
}



