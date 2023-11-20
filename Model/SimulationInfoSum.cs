
using Dapper.FluentMap.Mapping;
using SZY.Platform.WebApi.Data;
using System.Collections.Generic;

namespace SZY.Platform.WebApi.Model
{
    public class SimulationInfoSumParm : BaseQueryParm
    {
        public string time { get; set; }
        public int item_type { get; set; }
    }
    public class SimulationInfoSumPageView
    {
        public List<SimulationInfoSum> rows { get; set; }
        public int total { get; set; }
    }

    public class SimulationInfoSum : BaseEntity
    {
        public long Id { get; set; }
        public int Roadpart { get; set; }
        public string Camera { get; set; }
        public int AreaId { get; set; }
        public int ItemType { get; set; }
        public int ItemCount { get; set; }
        public int Speed { get; set; }
        public double QueueLength { get; set; }
        public System.DateTime Time { get; set; }
        public int ProjId { get; set; }
        public System.DateTime AddTime { get; set; }
    }

    public class SimulationInfoSumMap : EntityMap<SimulationInfoSum>
    {
        public SimulationInfoSumMap()
        {
            Map(o => o.Id).ToColumn("id");
            Map(o => o.Roadpart).ToColumn("roadpart");
            Map(o => o.Camera).ToColumn("camera");
            Map(o => o.AreaId).ToColumn("area_id");
            Map(o => o.ItemType).ToColumn("item_type");
            Map(o => o.ItemCount).ToColumn("Item_count");
            Map(o => o.Speed).ToColumn("speed");
            Map(o => o.QueueLength).ToColumn("queue_length");
            Map(o => o.Time).ToColumn("time");
            Map(o => o.ProjId).ToColumn("proj_id");
            Map(o => o.AddTime).ToColumn("add_time");
        }
    }

}