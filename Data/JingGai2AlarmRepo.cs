﻿using Dapper;
using SZY.Platform.WebApi.Model;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


// Coded By admin 2019/11/9 13:45:19
namespace SZY.Platform.WebApi.Data
{
    public interface IJingGai2AlarmRepo<T> where T : BaseEntity
    {
        
        Task<JingGai2Alarm> Save(JingGai2Alarm obj);
        Task<JingGai2AlarmPageView> GetPageList(JingGai2AlarmParm parm);
        Task<Jinggai2AlarmPhonePageView> GetPageList2();
        Task<JingGai2> Save2(JingGai2 obj);
        Task<JingGaiDevice> SaveJingGaiDevice(JingGaiDevice obj);
    }

    public class JingGai2AlarmRepo : BaseRepo, IJingGai2AlarmRepo<JingGai2Alarm>
    {
        public JingGai2AlarmRepo(DapperOptions options) : base(options) { }

        public async Task<JingGai2Alarm> Save(JingGai2Alarm obj)
        {
            return await WithConnection(async c =>
            {
                string sql = $@" INSERT INTO `aisense`.`jinggai2_alarm`(`id`, `client_id`, `model_type`, `alarm_type`, `alarm_level`, `identifier`, `value`, `alarm_time`, `alarm_settings_title`, `alarm_settings_identifier`, `alarm_settings_alarm_type`, `alarm_settings_alarm_level`, `alarm_settings_compare`, `alarm_settings_value`,`date1`,`client_group`,`client_name`) VALUES (@id,@client_id,@model_type,@alarm_type,@alarm_level,@identifier,@value,@alarm_time,@alarm_settings_title,@alarm_settings_identifier,@alarm_settings_alarm_type,@alarm_settings_alarm_level,@alarm_settings_compare,@alarm_settings_value,@date1,@client_group,@client_name);

                    ";
                sql += "SELECT LAST_INSERT_ID() ";
                int newid = await c.QueryFirstOrDefaultAsync<int>(sql, obj);
                obj.id = newid;
                return obj;
            });
        }

        public async Task<JingGai2> Save2(JingGai2 obj)
        {
            return await WithConnection(async c =>
            {
                string sql = $@" INSERT INTO `aisense`.`jinggai2`(`serial_no`, `client_id`, `model_type`, `identifier`, `value`, `is_alarm`, `date1`) VALUES ( @serial_no,@client_id,@model_type,@identifier,@value,@is_alarm,@date1);

                    ";
                sql += "SELECT LAST_INSERT_ID() ";
                int newid = await c.QueryFirstOrDefaultAsync<int>(sql, obj);
                obj.id = newid;
                return obj;
            });
        }

        public async Task<JingGai2AlarmPageView> GetPageList(JingGai2AlarmParm parm)
        {
            return await WithConnection(async c =>
            {

                StringBuilder sql = new StringBuilder();
                sql.Append($@"  SELECT * from jinggai2_alarm WHERE client_id = "+ parm.client_id +" order by id desc limit 1 ; ");
                var ets = await c.QueryAsync<JingGai2Alarm>(sql.ToString());

                JingGai2AlarmPageView ret = new JingGai2AlarmPageView();
                ret.rows = ets.ToList();
                return ret;
            });
        }


        public async Task<Jinggai2AlarmPhonePageView> GetPageList2()
        {
            return await WithConnection(async c =>
            {

                StringBuilder sql = new StringBuilder();
                sql.Append($@"  SELECT * from jinggai2_alarm_phone ; ");
                var ets = await c.QueryAsync<Jinggai2AlarmPhone>(sql.ToString());

                Jinggai2AlarmPhonePageView ret = new Jinggai2AlarmPhonePageView();
                ret.rows = ets.ToList();
                return ret;
            });
        }

        public async Task<JingGaiDevice> SaveJingGaiDevice(JingGaiDevice obj)
        {
            return await WithConnection(async c =>
            {
                string sqltemp = $@" SELECT * FROM jinggai_device WHERE device_id = @device_id ";
                var tempdata = await c.QueryFirstOrDefaultAsync<JingGaiDevice>(sqltemp, obj);
                if (tempdata != null)
                {
                    string sqlupdate = $@" UPDATE `aisense`.`jinggai_device` SET `device_type` = @device_type, `device_name` = @device_name, `install_addr` = @install_addr, `install_time` = @install_time, `lng` = @lng, `lat` = @lat, `date1` = @date1,`status` = @status WHERE `device_id` = @device_id;";
                    await c.ExecuteAsync(sqlupdate, obj);
                    return obj;
                }
                else
                {
                    string sql = $@" INSERT INTO `aisense`.`jinggai_device`( `device_id`, `device_type`, `device_name`, `install_addr`, `install_time`, `lng`, `lat`, `date1`, `status`) VALUES (@device_id,@device_type,@device_name,@install_addr,@install_time,@lng,@lat,@date1,@status);
                    ";
                    sql += "SELECT LAST_INSERT_ID() ";
                    int newid = await c.QueryFirstOrDefaultAsync<int>(sql, obj);
                    obj.id = newid;
                    return obj;
                }

            });
        }


    }
}



