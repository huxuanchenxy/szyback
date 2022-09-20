
using Dapper.FluentMap.Mapping;
using SZY.Platform.WebApi.Data;
using System.Collections.Generic;

// Coded by admin 2019/11/9 13:42:16
namespace SZY.Platform.WebApi.Model
{
    public class WXMiniprogram
    {
        /// <summary>
        /// 
        /// </summary>
        public string appid { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string pagepath { get; set; }
    }

    public class WXFirst
    {
        /// <summary>
        /// 恭喜你购买成功！
        /// </summary>
        public string value { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string color { get; set; }
    }

    public class WXKeyword1
    {
        /// <summary>
        /// 巧克力
        /// </summary>
        public string value { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string color { get; set; }
    }

    public class WXKeyword2
    {
        /// <summary>
        /// 39.8元
        /// </summary>
        public string value { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string color { get; set; }
    }

    public class WXKeyword3
    {
        /// <summary>
        /// 2014年9月22日
        /// </summary>
        public string value { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string color { get; set; }
    }

    public class WXRemark
    {
        /// <summary>
        /// 欢迎再次购买！
        /// </summary>
        public string value { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string color { get; set; }
    }

    public class WXData
    {
        /// <summary>
        /// 
        /// </summary>
        public WXFirst first { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public WXKeyword1 keyword1 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public WXKeyword2 keyword2 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public WXKeyword3 keyword3 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public WXRemark remark { get; set; }
    }

    public class WXRoot
    {
        /// <summary>
        /// 
        /// </summary>
        public string touser { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string template_id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string url { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public WXMiniprogram miniprogram { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string client_msg_id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public WXData data { get; set; }
    }

}

