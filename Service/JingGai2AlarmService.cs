using MSS.API.Common;
using MSS.API.Common.Utility;
using SZY.Platform.WebApi.Data;
using SZY.Platform.WebApi.Model;
using System;
using System.Net;
using System.Threading.Tasks;
using SZY.Platform.WebApi.Helper;


// Coded By admin 2019/11/9 13:46:57
namespace SZY.Platform.WebApi.Service
{
    public interface IJingGai2AlarmService
    {
        Task<ApiResult> Save(JingGai2Alarm obj);
        Task<JingGai2AlarmPageView> GetPageList(JingGai2AlarmParm parm);
        Task<Jinggai2AlarmPhonePageView> GetPageList2();
        Task<ApiResult> Save2(OpenApiJingGai2Data obj);
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



    }
}



