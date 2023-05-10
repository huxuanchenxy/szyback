
using Dapper.FluentMap.Mapping;
using SZY.Platform.WebApi.Data;
using System.Collections.Generic;
using System;

// Coded by admin 2019/11/9 13:42:16
namespace SZY.Platform.WebApi.Model
{
    public class JingGai2AlarmParm : BaseQueryParm
    {
        public string client_id { get; set; }
    }
    public class JingGai2AlarmPageView
    {
        public List<JingGai2Alarm> rows { get; set; }
        public int total { get; set; }
    }

    public class Jinggai2AlarmPhonePageView
    {
        public List<Jinggai2AlarmPhone> rows { get; set; }
        public int total { get; set; }
    }

    public class Jinggai2AlarmPhone : BaseEntity
    {
        public string client_id { get; set; }
        public string phone { get; set; }
    }

    public class JingGai2Alarm : BaseEntity
    {
        public int id { get; set; }
        public string client_id { get; set; }
        public string client_group { get; set; }
        public string client_name { get; set; }
        public string model_type { get; set; }
        public string alarm_type { get; set; }
        public string alarm_level { get; set; }
        public string identifier { get; set; }
        public string value { get; set; }
        public string alarm_time { get; set; }
        public string alarm_settings_title { get; set; }
        public string alarm_settings_identifier { get; set; }
        public string alarm_settings_alarm_type { get; set; }
        public string alarm_settings_alarm_level { get; set; }
        public string alarm_settings_compare { get; set; }
        public string alarm_settings_value { get; set; }
        public DateTime date1 { get; set; }

    }

    public class JingGai2AlarmMap : EntityMap<JingGai2Alarm>
    {
        public JingGai2AlarmMap()
        {
            Map(o => o.id).ToColumn("id");
            Map(o => o.client_id).ToColumn("client_id");
            Map(o => o.client_group).ToColumn("client_group");
            Map(o => o.client_name).ToColumn("client_name");
            Map(o => o.model_type).ToColumn("model_type");
            Map(o => o.alarm_type).ToColumn("alarm_type");
            Map(o => o.alarm_level).ToColumn("alarm_level");
            Map(o => o.identifier).ToColumn("identifier");
            Map(o => o.value).ToColumn("value");
            Map(o => o.alarm_time).ToColumn("alarm_time");
            Map(o => o.alarm_settings_title).ToColumn("alarm_settings_title");
            Map(o => o.alarm_settings_identifier).ToColumn("alarm_settings_identifier");
            Map(o => o.alarm_settings_alarm_type).ToColumn("alarm_settings_alarm_type");
            Map(o => o.alarm_settings_alarm_level).ToColumn("alarm_settings_alarm_level");
            Map(o => o.alarm_settings_compare).ToColumn("alarm_settings_compare");
            Map(o => o.alarm_settings_value).ToColumn("alarm_settings_value");
            Map(o => o.date1).ToColumn("date1");

        }
    }

}