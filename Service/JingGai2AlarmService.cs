using MSS.API.Common;
using MSS.API.Common.Utility;
using SZY.Platform.WebApi.Data;
using SZY.Platform.WebApi.Model;
using System;
using System.Net;
using System.Threading.Tasks;
using SZY.Platform.WebApi.Helper;
using SZY.Platform.WebApi.Controllers;


// Coded By admin 2019/11/9 13:46:57
namespace SZY.Platform.WebApi.Service
{
    public interface IJingGai2AlarmService
    {
        Task<ApiResult> Save(JingGai2Alarm obj);
        Task<JingGai2AlarmPageView> GetPageList(JingGai2AlarmParm parm);
        Task<Jinggai2AlarmPhonePageView> GetPageList2();
        Task<ApiResult> Save2(OpenApiJingGai2Data obj);
        Task<ApiResult> Save3(OpenApiDeviceObj json);
    }

    public class JingGai2AlarmService : IJingGai2AlarmService
    {
        private readonly IJingGai2AlarmRepo<JingGai2Alarm> _repo;
        private readonly IAuthHelper _authhelper;
        private readonly int _userID;

        public JingGai2AlarmService(IJingGai2AlarmRepo<JingGai2Alarm> repo, IAuthHelper authhelper)
        {
            _repo = repo;
            _authhelper = authhelper;
            _userID = _authhelper.GetUserId();
        }

        public async Task<JingGai2AlarmPageView> GetPageList(JingGai2AlarmParm parm)
        {
            JingGai2AlarmPageView ret = new JingGai2AlarmPageView();
            try
            {
                ret = await _repo.GetPageList(parm);
            }
            catch (Exception ex)
            {
                
            }
            return ret;
        }


        public async Task<Jinggai2AlarmPhonePageView> GetPageList2()
        {
            Jinggai2AlarmPhonePageView ret = new Jinggai2AlarmPhonePageView();
            try
            {
                ret = await _repo.GetPageList2();
            }
            catch (Exception ex)
            {

            }
            return ret;
        }
        public async Task<ApiResult> Save(JingGai2Alarm obj)
        {
            ApiResult ret = new ApiResult();
            try
            {
                //DateTime dt = DateTime.Now;
                //obj.UpdatedTime = dt;
                //obj.CreatedTime = dt;
                //obj.UpdatedBy = _userID;
                //obj.CreatedBy = _userID;
                obj.date1 = DateTime.Now;
                ret.data = await _repo.Save(obj);
                ret.code = Code.Success;
                return ret;
            }
            catch (Exception ex)
            {
                ret.code = Code.Failure;
                ret.msg = ex.Message;
                return ret;
            }
        }

        public async Task<ApiResult> Save2(OpenApiJingGai2Data json)
        {
            ApiResult ret = new ApiResult();
            try
            {
                //DateTime dt = DateTime.Now;
                //obj.UpdatedTime = dt;
                //obj.CreatedTime = dt;
                //obj.UpdatedBy = _userID;
                //obj.CreatedBy = _userID;
                //obj.date1 = DateTime.Now;
                //ret.data = await _repo.Save(obj);
                if (json != null)
                {
                    string serial_no = json.serial_no;
                    string client_id = json.client_id;
                    string model_type = json.model_type;
                    if (json.payload != null && json.payload.Count > 0)
                    {
                        foreach (var p in json.payload)
                        {
                            string identifier = p.identifier;
                            string value = p.value.ToString();
                            DateTime upload_time = AliyunHelper.GetDateTimeMilliseconds(p.upload_time);
                            bool is_alarm = p.is_alarm;
                            JingGai2 et = new JingGai2() { serial_no = serial_no,client_id = client_id,model_type = model_type,identifier = identifier,value = value,date1 = upload_time,is_alarm = is_alarm };
                            await _repo.Save2(et);
                        }
                    }
                }
                ret.code = Code.Success;
                return ret;
            }
            catch (Exception ex)
            {
                ret.code = Code.Failure;
                ret.msg = ex.Message;
                return ret;
            }
        }


        public async Task<ApiResult> Save3(OpenApiDeviceObj json)
        {
            ApiResult ret = new ApiResult();
            try
            {
                if (json != null)
                {
                    if (json.data != null && json.data.data!= null && json.data.data.Count > 0)
                    {
                        foreach (var p in json.data.data)
                        {
                            var device_name = p.device_name;
                            var model_type = p.model_type;
                            var client_id = p.client_id;
                            var addr = p.addr;
                            var longitude = p.longitude;
                            var latitude = p.latitude;
                            var active_at = p.active_at;
                            var last_upload_at = p.last_upload_at;
                            var status = p.status == "online"? "在线":"离线";
                            var groups = p.groups;
                            JingGaiDevice etobj = new JingGaiDevice() {  date1 = AliyunHelper.GetDateTimeMilliseconds(last_upload_at).ToString("yyyy-MM-dd hh:mm:ss"), device_id = client_id, device_name = device_name,device_type = model_type, install_addr = addr, install_time = active_at.ToString(), lat = latitude.ToString(), lng = longitude.ToString(), status = status  };
                            await _repo.SaveJingGaiDevice(etobj);
                        }
                    }
                }
                ret.code = Code.Success;
                return ret;
            }
            catch (Exception ex)
            {
                ret.code = Code.Failure;
                ret.msg = ex.Message;
                return ret;
            }
        }
    }
}



