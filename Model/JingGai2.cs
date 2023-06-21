
using Dapper.FluentMap.Mapping;
using SZY.Platform.WebApi.Data;
using System.Collections.Generic;
using System;

// Coded by admin 2019/11/9 13:42:16
namespace SZY.Platform.WebApi.Model
{
    public class JingGai2Parm : BaseQueryParm
    {

    }
    public class JingGai2PageView
    {
        public List<JingGai> rows { get; set; }
        public int total { get; set; }
    }

    public class JingGai2 : BaseEntity
    {
        public int id { get; set; }
        public string serial_no { get; set; }
        public string client_id { get; set; }
        public string model_type { get; set; }
        public string identifier { get; set; }
        public string value { get; set; }
        public bool is_alarm { get; set; }
        public DateTime date1 { get; set; }
    }

    public class JingGai2Map : EntityMap<JingGai2>
    {
        public JingGai2Map()
        {
            Map(o => o.id).ToColumn("id");
            Map(o => o.serial_no).ToColumn("serial_no");
            Map(o => o.client_id).ToColumn("client_id");
            Map(o => o.model_type).ToColumn("model_type");
            Map(o => o.identifier).ToColumn("identifier");
            Map(o => o.value).ToColumn("value");
            Map(o => o.is_alarm).ToColumn("is_alarm");
            Map(o => o.date1).ToColumn("date1");
            
        }
    }

    public class OpenApiJingGai2Data : JingGaiJson
    {
        public new List<OpenApiJingGai2DataPayload> payload { get; set; }
    }
    public class JingGaiJson
    {
        public string serial_no { get; set; }
        public string client_id { get; set; }
        public string model_type { get; set; }
        public string push_type { get; set; }
        public dynamic payload { get; set; }
    }

    public class OpenApiJingGai2Alarm : JingGaiJson
    {
        public new List<OpenApiJingGai2AlarmPayload> payload { get; set; }
    }

    public class OpenApiJingGai2AlarmPayload
    {
        public string alarm_type { get; set; }
        public string alarm_level { get; set; }
        public string identifier { get; set; }
        public dynamic value { get; set; }
        public string alarm_time { get; set; }
        public List<OpenApiJingGai2AlarmPayloadSettings> alarm_settings { get; set; }
    }


    public class OpenApiJingGai2DataPayload
    {

        public string identifier { get; set; }

        public string value { get; set; }

        public long upload_time { get; set; }

        public bool is_alarm { get; set; }
    }

    public class OpenApiJingGai2AlarmPayloadSettings
    {
        public string title { get; set; }
        public string identifier { get; set; }
        public string alarm_type { get; set; }
        public string alarm_level { get; set; }
        public string compare { get; set; }
        public dynamic value { get; set; }
    }
}