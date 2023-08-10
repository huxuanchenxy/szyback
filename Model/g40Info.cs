
using Dapper.FluentMap.Mapping;
using SZY.Platform.WebApi.Data;
using System.Collections.Generic;


namespace SZY.Platform.WebApi.Model
{
    public class G40InfoParm : BaseQueryParm
    {
        public System.DateTime StartTime { get; set; }
        public System.DateTime endTime { get; set; }
        public string camera { get; set; }
    }
    public class G40InfoPageView
    {
        public List<G40Info> rows { get; set; }
        public int total { get; set; }
    }

    public class G40Info : BaseEntity
    {
        public int Id { get; set; }
        public int RoadPart { get; set; }
        public string Camera { get; set; }
        public System.DateTime Time { get; set; }
        public int Carcount { get; set; }
        public int Timespan { get; set; }
    }

    public class CarQueryParm
    {
        public int times { get; set; }
        public int sleep { get; set; }
        public int roadpart { get; set; }
        public int direction { get; set; }
        public int carcount { get; set; }
        public int carspeed { get; set; }
        public int firstcarloc { get; set; }
        public int offset1 { get; set; }
        public int offset2 { get; set; }
    }
    public class carinfo
    {
        public int num { get; set; }
        public string carid { get; set; }
        public string caridnum { get; set; }
        public int cartype { get; set; }
        public int carcolor { get; set; }
        public int distance { get; set; }
        public string curx { get; set; }
        public string cury { get; set; }
        public int roadlane { get; set; }
    }

    public class transportret
    {
        public int code { get; set; }
        public string timestamp { get; set; }
        public string msg { get; set; }
        public transportresult result { get; set; }
    }

    public class transportresult
    {
        public int roadpart { get; set; }
        public string roadpartx { get; set; }
        public string roadparty { get; set; }
        public int direction { get; set; }
        public string camera { get; set; }
        public List<carinfo> carinfo { get; set; }

        //public int carcount { get; set; }//g40info用
        //public DateTime time { get; set; }///*g40info用*/
    }

    public class G40InfoMap : EntityMap<G40Info>
    {
        public G40InfoMap()
        {
            Map(o => o.Id).ToColumn("id");
            Map(o => o.RoadPart).ToColumn("road_part");
            Map(o => o.Camera).ToColumn("camera");
            Map(o => o.Time).ToColumn("time");
            Map(o => o.Carcount).ToColumn("carcount");
            Map(o => o.Timespan).ToColumn("timespan");
        }
    }

}