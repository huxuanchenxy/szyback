using AlibabaCloud.SDK.Dysmsapi20170525.Models;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using SZY.Platform.WebApi.Controllers;
using SZY.Platform.WebApi.Model;
using Tea;

namespace SZY.Platform.WebApi.Helper
{
    public sealed class AliyunHelper
    {
        private AliyunHelper() { }


        /**
         * 使用AK&SK初始化账号Client
         * @param accessKeyId
         * @param accessKeySecret
         * @return Client
         * @throws Exception
         */
        public static AlibabaCloud.SDK.Dysmsapi20170525.Client CreateClient(string accessKeyId, string accessKeySecret)
        {
            AlibabaCloud.OpenApiClient.Models.Config config = new AlibabaCloud.OpenApiClient.Models.Config
            {
                // 必填，您的 AccessKey ID
                AccessKeyId = accessKeyId,
                // 必填，您的 AccessKey Secret
                AccessKeySecret = accessKeySecret,
            };
            // 访问的域名
            config.Endpoint = "dysmsapi.aliyuncs.com";
            return new AlibabaCloud.SDK.Dysmsapi20170525.Client(config);
        }

        public static JingGaiRet2 SendSMS(JingGai2Alarm parm,string phone)
        {

            JingGaiRet2 ret = new JingGaiRet2();
            ret.success = true;
        // 工程代码泄露可能会导致AccessKey泄露，并威胁账号下所有资源的安全性。以下代码示例仅供参考，建议使用更安全的 STS 方式，更多鉴权访问方式请参见：https://help.aliyun.com/document_detail/378671.html
        //请检查模板内容与模板参数是否匹配

        //井盖报警: 设备为 ${ clientname} ,报警参数为 ${ alarmtitle}
        //    的值${ value}
        //    超出范围，参考值为${ setvalue}
            AlibabaCloud.SDK.Dysmsapi20170525.Client client = CreateClient("LTAI5tQ8wqr5YBJ1ubkYrVvm", "CetIXMe7xIB4lyKP3v5Uc6hRmUAQDJ");
            AlibabaCloud.SDK.Dysmsapi20170525.Models.SendSmsRequest sendSmsRequest = new AlibabaCloud.SDK.Dysmsapi20170525.Models.SendSmsRequest
            {
                PhoneNumbers = phone,
                SignName = "上海电气自动化研究所",
                //TemplateCode = "SMS_277385659",
                //TemplateParam = "{\"code\":\"123456\"}"
                TemplateCode = "SMS_460695774",
                TemplateParam = "{\"clientname\":\""+parm.client_name+"\",\"alarmtitle\":\""+ parm.alarm_settings_title+"\",\"value\":\""+ parm.value+"\",\"setvalue\":\""+parm.alarm_settings_value+"\"}"
            };
            try
            {
                // 复制代码运行请自行打印 API 的返回值
                var aliyunret = client.SendSmsWithOptions(sendSmsRequest, new AlibabaCloud.TeaUtil.Models.RuntimeOptions());
                ret.obj = JsonConvert.SerializeObject(aliyunret);
                
            }
            catch (TeaException error)
            {
                // 如有需要，请打印 error
                AlibabaCloud.TeaUtil.Common.AssertAsString(error.Message);
                ret.success = false;
                ret.obj = error.Message;
            }
            catch (Exception _error)
            {
                TeaException error = new TeaException(new Dictionary<string, object>
                {
                    { "message", _error.Message }
                });
                // 如有需要，请打印 error
                AlibabaCloud.TeaUtil.Common.AssertAsString(error.Message);
                ret.success = false;
                ret.obj = error.Message;
            }
            return ret;
        }

    }
}