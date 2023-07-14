

using Dapper.FluentMap.Mapping;
using SZY.Platform.WebApi.Data;
using System.Collections.Generic;

// Coded by admin 2019/11/9 13:42:16
namespace SZY.Platform.WebApi.Model
{
    public class SimulationInfoParm : BaseQueryParm
    {
        public string time { get; set; }
    }
    public class SimulationInfoPageView
    {
        public List<SimulationInfo> rows { get; set; }
        public int total { get; set; }
    }

    public class SimulationInfo : BaseEntity
    {
        public int Id { get; set; }
        public int RoadPart { get; set; }
        public string Camera { get; set; }
        public System.DateTime Time { get; set; }
        public string Summarize { get; set; }
        public string Content { get; set; }
    }

    public class SimulationInfoMap : EntityMap<SimulationInfo>
    {
        public SimulationInfoMap()
        {
            Map(o => o.Id).ToColumn("id");
            Map(o => o.RoadPart).ToColumn("road_part");
            Map(o => o.Camera).ToColumn("camera");
            Map(o => o.Time).ToColumn("time");
            Map(o => o.Summarize).ToColumn("summarize");
            Map(o => o.Content).ToColumn("content");
        }
    }

}