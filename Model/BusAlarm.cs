
using Dapper.FluentMap.Mapping;
using SZY.Platform.WebApi.Data;
using System.Collections.Generic;
using System;
using RestSharp;

// Coded by admin 2019/11/9 13:42:16
namespace SZY.Platform.WebApi.Model
{
    public class BusAlarmParm : BaseQueryParm
    {

    }
    public class BusAlarmPageView
    {
        public List<BusAlarm> rows { get; set; }
        public int total { get; set; }
    }

    public class BusAlarm : BaseEntity
    {
        public int id { get; set; }
        public string camera_id { get; set; }
        public string site_id { get; set; }
        public double camera_lng { get; set; }
        public double camera_lat { get; set; }
        public string camera_url { get; set; }
        public string camera_name { get; set; }
        public DateTime time { get; set; }
        public string alarm_picture { get; set; }
        public int alarm_type { get; set; }
        public string alarm_objects { get; set; }
        public string alarm_des { get; set; }
    }

    public class BusAlarmMap : EntityMap<BusAlarm>
    {
        public BusAlarmMap()
        {
            Map(o => o.id).ToColumn("id");
            Map(o => o.camera_id).ToColumn("camera_id");
            Map(o => o.site_id).ToColumn("site_id");
            Map(o => o.camera_lng).ToColumn("camera_lng");
            Map(o => o.camera_lat).ToColumn("camera_lat");
            Map(o => o.camera_url).ToColumn("camera_url");
            Map(o => o.camera_name).ToColumn("camera_name");
            Map(o => o.time).ToColumn("time");
            Map(o => o.alarm_picture).ToColumn("alarm_picture");
            Map(o => o.alarm_type).ToColumn("alarm_type");
            Map(o => o.alarm_objects).ToColumn("alarm_objects");
            Map(o => o.alarm_des).ToColumn("alarm_des");
        }
    }

}