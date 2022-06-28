
using Dapper.FluentMap.Mapping;
using SZY.Platform.WebApi.Data;
using System.Collections.Generic;

// Coded by admin 2019/11/9 13:42:16
namespace SZY.Platform.WebApi.Model
{
    public class TansportCarParm : BaseQueryParm
    {

    }
    public class TansportCarView
    {
        public List<TansportCar> rows { get; set; }
        public int total { get; set; }
    }

    public class TansportCar : BaseEntity
    {
        
    }


    public class TransportCarCameraToTunnelView
    {
        public List<TransportCarCameraToTunnel> rows { get; set; }
        public int total { get; set; }
    }

    public class TransportCarCameraToTunnel : BaseEntity
    {
        public int id { get; set; }
        public int offset { get; set; }
        public string camera { get; set; }
        public int roadpart { get; set; }
        public int direction { get; set; }

    }
    public class TransportCarRoot
    {
        /// <summary>
        /// 
        /// </summary>
        public int code { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public long timestamp { get; set; }

        /// <summary>
        /// 成功
        /// </summary>
        public string msg { get; set; }

        /// <summary>
        /// result
        /// </summary>
        public TransportCarResult result { get; set; }

    }


    public class TransportCarResult
    {
        /// <summary>
        /// 
        /// </summary>
        public int roadpart { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int roadpartx { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int roadparty { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int direction { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string camera { get; set; }

        /// <summary>
        /// carinfo
        /// </summary>
        public List<TransportCarinfo> carinfo { get; set; }

    }

    public class TransportCarinfo
    {
        /// <summary>
        /// 
        /// </summary>
        public string num { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string carid { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int cartype { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int carcolor { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int roadlane { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int distance { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int curx { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int cury { get; set; }

    }

    public class TransportCarCameraToTunnelMap : EntityMap<TransportCarCameraToTunnel>
    {
        public TransportCarCameraToTunnelMap()
        {
            Map(o => o.id).ToColumn("id");
            Map(o => o.camera).ToColumn("camera");
            Map(o => o.direction).ToColumn("direction");
            Map(o => o.offset).ToColumn("offset");
            Map(o => o.roadpart).ToColumn("roadpart");
        }
    }

}