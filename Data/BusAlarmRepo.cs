using Dapper;
using SZY.Platform.WebApi.Model;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;


// Coded By admin 2019/11/9 13:45:19
namespace SZY.Platform.WebApi.Data
{
    public interface IBusAlarmRepo<T> where T : BaseEntity
    {
        Task<BusAlarm> Save(BusAlarm obj);
    }

    public class BusAlarmRepo : BaseRepo, IBusAlarmRepo<BusAlarm>
    {
        public BusAlarmRepo(DapperOptions options) : base(options) { }

        public async Task<BusAlarm> Save(BusAlarm obj)
        {
            return await WithConnection(async c =>
            {
                IDbTransaction trans = c.BeginTransaction();
                try
                {

                    string sql = $@" INSERT INTO `aisense`.`bus_alarm`(`camera_id`, `site_id`, `camera_lng`, `camera_lat`, `camera_url`, `camera_name`, `time`, `alarm_picture`, `alarm_type`, `alarm_objects`, `alarm_des`) 
VALUES (@camera_id, @site_id, @camera_lng, @camera_lat, @camera_url, @camera_name, @time, @alarm_picture, @alarm_type, @alarm_objects, @alarm_des);
                    ";
                    sql += "SELECT LAST_INSERT_ID() ";
                    int newid = await c.QueryFirstOrDefaultAsync<int>(sql, obj, trans);
                    obj.id = newid;
                    trans.Commit();
                    return obj;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw new Exception(ex.ToString());
                }
            });
        }


    }
}



