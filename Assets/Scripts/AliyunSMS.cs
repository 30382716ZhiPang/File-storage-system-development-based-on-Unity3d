using System;
using System.Collections.Generic;
using Aliyun.Acs.Core;
using Aliyun.Acs.Core.Profile;
using Aliyun.Acs.Core.Exceptions;
using Aliyun.Acs.Core.Http;
using Aliyun.Acs.Core.Auth;
using UnityEngine;

public class AliyunSMS : MonoBehaviour
{
    private void Awake()
    {
        //SendSMS("1667533****",rang.ToString());
    }
    public void SendSMS(string sendPhoneNumber,string code)
    {
        //AlibabaCloudCredentialsProvider provider = new AccessKeyCredentialProvider("<your-access-key-id>", "<your-access-key-secret>");

        /* use STS Token
        AlibabaCloudCredentialsProvider provider = new StsCredentialProvider("<your-access-key-id>", "<your-access-key-secret>", "<your-sts-token>");
        */

        //IClientProfile profile = DefaultProfile.GetProfile("cn-hangzhou", provider);
        //DefaultAcsClient client = new DefaultAcsClient(profile, provider);


        IClientProfile profile = DefaultProfile.GetProfile("cn-hangzhou", "LTAI5tANuDbmeFqqC5Ey****", "kf6V5FFcYrVPJ7CbPuIGNIbmPD****");
        DefaultAcsClient client = new DefaultAcsClient(profile);

        CommonRequest request = new CommonRequest();

        request.Method = MethodType.POST;
        request.Domain = "dysmsapi.aliyuncs.com";
        request.Version = "2017-05-25";
        request.Action = "SendSms";
        // request.Protocol = ProtocolType.HTTP;
        request.AddQueryParameters("PhoneNumbers", sendPhoneNumber);
        request.AddQueryParameters("SignName", "ÉXÉm");
        request.AddQueryParameters("TemplateCode", "SMS_260255129");
        //request.AddQueryParameters("TemplateParam", "{\"code\":\"1234\"}");
        request.AddQueryParameters("TemplateParam", "{'code':'"+ code+ "'}");
        try
        {
            CommonResponse response = client.GetCommonResponse(request);
            print(System.Text.Encoding.Default.GetString(response.HttpResponse.Content));
        }
        catch (ServerException e)
        {
            print(e);
        }
        catch (ClientException e)
        {
            print(e);
        }
    }
}
