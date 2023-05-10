﻿using MSS.API.Common;
using MSS.API.Common.Utility;
using SZY.Platform.WebApi.Data;
using SZY.Platform.WebApi.Model;
using System;
using System.Net;
using System.Threading.Tasks;


// Coded By admin 2019/11/9 13:46:57
namespace SZY.Platform.WebApi.Service
{
    public interface IJingGai2AlarmService
    {
        Task<ApiResult> Save(JingGai2Alarm obj);
        Task<JingGai2AlarmPageView> GetPageList(JingGai2AlarmParm parm);
        Task<Jinggai2AlarmPhonePageView> GetPageList2();
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



    }
}



