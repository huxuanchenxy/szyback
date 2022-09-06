using Dapper;
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

        
    }
}



