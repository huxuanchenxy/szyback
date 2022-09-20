using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using Serilog.Core;
using SZY.Platform.WebApi.Helper;
using SZY.Platform.WebApi.Model;

namespace SZY.Platform.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WxController : ControllerBase
    {

        private readonly Logger _logger3;

        public WxController()
        {
            _logger3 = new LoggerConfiguration()
#if DEBUG
        .MinimumLevel.Debug()
#else
        .MinimumLevel.Information()
#endif
        //.MinimumLevel.Override("Microsoft", LogEventLevel.)
        //.Enrich.FromLogContext()
        .WriteTo.RollingFile(@"c:\\JingGaiLogs\WXlog.txt")
        .CreateLogger();
        }
        /// <summary>
        /// 生成签名
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        [NonAction]
        private string MakeSign(params string[] args)
        {
            //字典排序
            Array.Sort(args);
            string tmpStr = string.Join("", args);
            //字符加密
            var sha1 = EncryptHelper.Sha1Encrypt(tmpStr);
            return sha1;
        }
        /// <summary>
        /// 生成消息签名
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        [NonAction]
        private string MakeMsgSign(params string[] args)
        {
            //字典排序
            Array.Sort(args, new CharSort());
            string tmpStr = string.Join("", args);
            //字符加密
            var sha1 = EncryptHelper.Sha1Encrypt(tmpStr);
            return sha1;
        }
        /// <summary>
        /// 微信回调统一接口
        /// </summary>
        /// <returns></returns>
        [HttpGet("Service"), HttpPost]
        public string Service()
        {
            //获取配置文件中的数据
            var token = "jinggai2022";
            var encodingAESKey = "9b1d01dd259426fe6f09b1244a971294";
            var appId = "wxfc879eb996df5996";

            bool isGet = string.Equals(HttpContext.Request.Method, HttpMethod.Get.Method, StringComparison.OrdinalIgnoreCase);
            bool isPost = string.Equals(HttpContext.Request.Method, HttpMethod.Post.Method, StringComparison.OrdinalIgnoreCase);
            if (!isGet && !isPost)
            {
                return "";
            }

            bool isEncrypt = false;
            try
            {
                var query = HttpContext.Request.QueryString.ToString();
                string msg_signature = "", nonce = "", timestamp = "", encrypt_type = "", signature = "", echostr = "";

                if (!string.IsNullOrEmpty(query))//需要验证签名
                {
                    var collection = HttpUtility.ParseQueryString(query);
                    msg_signature = collection["msg_signature"]?.Trim();
                    nonce = collection["nonce"]?.Trim();
                    timestamp = collection["timestamp"]?.Trim();
                    encrypt_type = collection["encrypt_type"]?.Trim();
                    signature = collection["signature"]?.Trim();
                    echostr = collection["echostr"]?.Trim();

                    if (!string.IsNullOrEmpty(encrypt_type))//有使用加密
                    {
                        if (!string.Equals(encrypt_type, "aes", StringComparison.OrdinalIgnoreCase))//只支持AES加密方式
                        {
                            return "";
                        }
                        isEncrypt = true;
                    }
                }

                //先验证签名
                if (!string.IsNullOrEmpty(signature))
                {
                    //字符加密
                    var sha1 = MakeSign(nonce, timestamp, token);
                    if (!sha1.Equals(signature, StringComparison.OrdinalIgnoreCase))//验证不通过
                    {
                        return "";
                    }

                    if (isGet)//是否Get请求，如果true,那么就认为是修改服务器回调配置信息
                    {
                        return echostr;
                    }
                }
                else
                {
                    return "";//没有签名，请求直接返回
                }

                var body = new StreamReader(HttpContext.Request.Body).ReadToEnd();

                if (isEncrypt)
                {
                    XDocument doc = XDocument.Parse(body);
                    var encrypt = doc.Element("xml").Element("Encrypt");

                    //验证消息签名
                    if (!string.IsNullOrEmpty(msg_signature))
                    {
                        //消息加密
                        var sha1 = MakeMsgSign(nonce, timestamp, encrypt.Value, token);
                        if (!sha1.Equals(msg_signature, StringComparison.OrdinalIgnoreCase))//验证不通过
                        {
                            return "";
                        }
                    }

                    body = EncryptHelper.AESDecrypt(encrypt.Value, encodingAESKey);//解密
                }

                if (!string.IsNullOrEmpty(body))
                {
                    //
                    //在这里根据body中的MsgType和Even来区分消息，然后来处理不同的业务逻辑
                    //
                    //

                    //result是上面逻辑处理完成之后的待返回结果，如返回文本消息：
                    var result = @"<xml>
                                      <ToUserName><![CDATA[toUser]]></ToUserName>
                                      <FromUserName><![CDATA[fromUser]]></FromUserName>
                                      <CreateTime>12345678</CreateTime>
                                      <MsgType><![CDATA[text]]></MsgType>
                                      <Content><![CDATA[你好]]></Content>
                                    </xml>";
                    if (!string.IsNullOrEmpty(result))
                    {
                        if (isEncrypt)
                        {
                            result = EncryptHelper.AESEncrypt(result, encodingAESKey, appId);
                            var _msg_signature = MakeMsgSign(nonce, timestamp, result, token);
                            result = $@"<xml>
                                                    <Encrypt><![CDATA[{result}]]></Encrypt>
                                                    <MsgSignature>{_msg_signature}</MsgSignature>
                                                    <TimeStamp>{timestamp}</TimeStamp>
                                                    <Nonce>{nonce}</Nonce>
                                                </xml>";
                        }
                        return result;
                    }

                    //如果这里我们的处理逻辑需要花费较长时间，可以这里先返回空(""),然后使用异步去处理业务逻辑，
                    //异步处理完后，调用微信的客服消息接口通知微信服务器
                }
            }
            catch (Exception ex)
            {
                //记录异常日志
            }

            return "";
        }

        //给微信公众号发送通知
        [HttpGet("SendWeChatMsg")]
        public string SendWeChatMsg()
        {
            string OpenID = "oXx6O6e8y3swFXrWtY9CjK9HF0kM";
            //Appid
            string appid = "wx2ad82f5d4f69022e";//wxfc879eb996df5996  测试号
            //secret
            string secret = "99edc8041bfc01e5d17863426fd8aaf3";//9b1d01dd259426fe6f09b1244a971294 测试号密码

            string ret = string.Empty;

            try
            {

                if (OpenID != "")
                {
                    string url = "https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid=" + appid + "&secret=" + secret + "";
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                    request.Method = "GET";
                    request.ContentType = "text/html;charset=UTF-8";
                    string jsonData = "";
                    using (HttpWebResponse response1 = (HttpWebResponse)request.GetResponse())
                    {
                        using (StreamReader sr = new StreamReader(response1.GetResponseStream(), Encoding.UTF8))
                        {
                            jsonData = sr.ReadToEnd();
                            sr.Close();
                        }
                        response1.Close();
                    }
                    if (jsonData != "")
                    {
                        string jsonString = jsonData;
                        JObject json = JObject.Parse(jsonString);
                        string access_token = json["access_token"].ToString();
                        ret = access_token;
                        string str = "https://api.weixin.qq.com/cgi-bin/message/template/send?access_token=" + access_token;

                        //json参数，传入对应的值                                                                           
                        //string jsonParam = "{\"touser\": \"" + OpenID + "\",\"template_id\":\"" + "VKhqNtMKtiI_Mi-p_P7YnFZR70BLwGsW8ywx9uFlXz0" + "\",\"url\":\"" + "http://weixin.qq.com/download" + "\",\"data\": {\"cols1\": { \"value\": \"1111\"}," +
                        //    "\"cols2\": { \"value\": \"2222\"}," +
                        //      "\"cols3\": { \"value\": \"333\"}," +
                        //        "\"cols4\": { \"value\": \"" + DateTime.Now.ToString() + "\"}}}";

                        //string jsonParam = "{\"touser\": \"" + OpenID + "\",\"template_id\":\"" + "6ngf_w5yemfU0lZrDW2USzqWZQDGPg4oOXfp4LlnDaY" + "\",\"url\":\"" + "http://weixin.qq.com/download" + "\",\"data\": {\"cols1\": { \"value\": \"1111\"}," +
                        //    "\"cols2\": { \"value\": \"2222\"}," +
                        //      "\"cols3\": { \"value\": \"333\"}}}";

                        //string jsonParam = "{\"touser\": \"" + OpenID + "\", 	\"template_id\": \"QB6AJFWAZK4UZTgY-UPm1xTS45y4_To4wCsluPv6fzM\", 	\"url\": \"http://weixin.qq.com/download\", 	\"miniprogram\": { 		\"appid\": \"xiaochengxuappid12345\", 		\"pagepath\": \"index?foo=bar\" 	}, \"data\": { 		\"first\": \"" + DateTime.Now.ToString() + "\" 	} }";

                        WXRoot wxroot = new WXRoot() { touser = OpenID, template_id = "QB6AJFWAZK4UZTgY-UPm1xTS45y4_To4wCsluPv6fzM", url = "http://weixin.qq.com/download", miniprogram = new WXMiniprogram() { appid = "wx73fa94f9c3741408" }, data = new WXData() { first = new WXFirst() { value = "恭喜你购买成功！" + DateTime.Now.ToString(), color = "#440033" }, keyword1 = new WXKeyword1() { value = "aasss" } } };
                        string jsonParam = JsonConvert.SerializeObject(wxroot);

                        HttpWebRequest requests = (HttpWebRequest)WebRequest.Create(str);
                        requests.Method = "POST";
                        requests.Timeout = 20000;
                        requests.ContentType = "application/json;charset=UTF-8";
                        byte[] byteData = Encoding.UTF8.GetBytes(jsonParam);
                        int length = byteData.Length;
                        requests.ContentLength = length;

                        using (Stream writer = requests.GetRequestStream())
                        {
                            writer.Write(byteData, 0, length);
                            writer.Close();
                        }

                        string jsonStrings = string.Empty;
                        using (HttpWebResponse responses = (HttpWebResponse)requests.GetResponse())
                        {
                            using (Stream streams = responses.GetResponseStream())
                            {
                                using (StreamReader readers = new StreamReader(streams, System.Text.Encoding.UTF8))
                                {
                                    jsonStrings = readers.ReadToEnd();
                                    _logger3.Warning("微信请求成功：" + jsonStrings);
                                    responses.Close();
                                    streams.Close();
                                    readers.Close();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger3.Warning("微信请求报错:" + ex.Message.ToString());
            }
            return ret;
        }


    }
}