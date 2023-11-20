
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
        public string camera_lng { get; set; }
        public string camera_lat { get; set; }
        public string camera_url { get; set; }
        public string camera_name { get; set; }
        public DateTime time { get; set; }
        public string alarm_picture { get; set; }
        public int? alarm_type { get; set; }
        public string alarm_objects { get; set; }
        public string alarm_des { get; set; }
        public string pic_url { get; set; }
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
            Map(o => o.pic_url).ToColumn("pic_url");
        }
    }

    public class Alarm_dataItem
    {
        /// <summary>
        /// 
        /// </summary>
        public List<int> height { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<string> sn_int { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<int> width { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int? sn_id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int? ch_id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<string> confidence { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<string> class_name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int alarm_type { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<string> @event { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<int> @class { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public List<int> x { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public List<int> y { get; set; }
}

public class BusRoot2
{
    /// <summary>
    /// 
    /// </summary>
    public List<Alarm_dataItem> alarm_data { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string description { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string site_id { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string time { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public int? camera_url { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string camera_lat { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string camera_id { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public int? camera_name { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string alarm_picture { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string camera_lng { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public int? ch_id { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public int? sn_id { get; set; }
}

}