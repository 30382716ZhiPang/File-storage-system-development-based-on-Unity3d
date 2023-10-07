using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using QCloud.CosApi.Api;
using QCloud.CosApi.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using UnityEngine;
using System.Threading;
using System.Collections;
using UnityEngine.Networking;
using System.Net;

public class TencentCOS : MonoBehaviour
{
    const int APP_ID = 1258767577;
    const string SECRET_ID = "AKIDfCYt89zdEoBNSL1Zp25GH5i7NXfM****";
    const string SECRET_KEY = "1Odr176va3Pg1Qi4USXcXgeR4Z0I****";

    private CosCloud cos;
    private string bucketName = "juruo"; //桶名

    private Thread thread;
    private string remotePath, localPath;   //上传路径 文件路径
    private Action PutSuccess;  //上传成功回调
    private Action GetSuccess;  //下载成功回调

    [HideInInspector]
    public const string DownloadPath = "https://juruo-1258767577.cos.ap-guangzhou.myqcloud.com/"; //+文件夹+文件名

    private string SavePath = "";//默认在下载位置
    //public float Process;       //上传进度

    private UnityWebRequestAsyncOperation asyncOperation;   //异步下载资源

    private void Awake()
    {
        //创建cos对象
        cos = new CosCloud(APP_ID, SECRET_ID, SECRET_KEY);

        cos.SetRegion("gz");
    }
    private void Update()
    {
        if (asyncOperation!=null&&asyncOperation.isDone)
        {
            //读取字节 腾讯云COS暂未提供C#的引用 无法访问进度
            byte[] bytes = asyncOperation.webRequest.downloadHandler.data;
            //异步加载完则设置为空
            asyncOperation = null;
            try
            {
                File.WriteAllBytes(SavePath, bytes);
            }
            catch (Exception e)
            {
                string str ="无权限下载至："+ SavePath;
                EventCenter.Broadcast<string>(EventDefine.Console, str);

                EventCenter.Broadcast<string>(EventDefine.Console, "请进行更换下载路径，如D盘等...");
                print("无权限放置至："+ SavePath);
            }
        }
    }

    //上传文件
    public void UploadObj(Action action,string remotePath,string localPath)
    {
        this.remotePath = remotePath;
        this.localPath = localPath;     //将形参路径赋值到全局路径 方便线程访问
        PutSuccess = action;
        thread = new Thread(PutObjectFromLocalThread);
        thread.Start();
    }
    private void PutObjectFromLocalThread()
    {
        var result = "";
        //上传文件（不论文件是否分片，均使用本接口）
        Stopwatch sw = new Stopwatch();
        sw.Start();
        var uploadParasDic = new Dictionary<string, string>();
        uploadParasDic.Add(CosParameters.PARA_BIZ_ATTR, "");
        uploadParasDic.Add(CosParameters.PARA_INSERT_ONLY, "0");

        result = cos.UploadFile(bucketName, remotePath, localPath, uploadParasDic, true, 20);
        sw.Stop();
        PutSuccess();   //回调
        print("上传文件:" + result);

        //关闭线程
        thread.Abort();
    }


    //WWW下载文件 貌似过时了 放这里先
    private IEnumerator GetObjectWithWWW(string id,string fileName)
    {
        string path = DownloadPath + id + fileName;
        WWW www = new WWW(path);
        yield return www;
    }

    //使用 UnityWebRequest下载文件
    public void GetObjectWithUnityWebRequest(Action action,string id,string fileName,string savePath)
    {
        GetSuccess = action;
        SavePath = savePath;
        StartCoroutine(GetObjectWithUnityWebRequest(id+fileName));
    }
    private IEnumerator GetObjectWithUnityWebRequest(string filePath)
    {
        string path = DownloadPath + filePath;
        UnityWebRequest unityWebRequest = UnityWebRequest.Get(path);
        //异步加载资源
        asyncOperation = unityWebRequest.SendWebRequest();
        GetSuccess();
        //加载资源成功返回协程
        yield return asyncOperation;
    }
    //删除文件
    public void DelectObj(Action action,string id,string fileName)
    {
        var result = "";
        result = cos.DeleteFile(bucketName, id+fileName);
        print("删除文件:" + result);
    }


//访问文件
public void AccessFile(string folder)
    {
        //CreateFile(folder);
        var result = "";
        result = cos.GetFileStat(bucketName, folder);
        print("获取文件属性:" + result);
    }

    //获取文件列表(启用)
    public List<string> GetFileList(string folder)
    {
        var result = "";
        var foldlistparasdic = new Dictionary<string, string>();
        foldlistparasdic.Add(CosParameters.PARA_NUM, "100");
        result = cos.GetFolderList(bucketName, folder, foldlistparasdic);
        //print("获取文件列表:" + result);

        //解析Json目录
        List<string> namelist = new List<string>();
        JObject json = JObject.Parse(result);
        json = (JObject)json["data"];
        //print(json["infos"].ToString());
        JArray array = JArray.Parse(json["infos"].ToString());
        //print(array[0]["name"]);
        foreach (var item in array)
        {
            //遍历目录
            namelist.Add(item["name"].ToString());
            //print(item["name"]);
        }
        return namelist;
    }
    //目录列表（暂停使用）
    public void GetDirectoryList(string folder)
    {
        var result = "";
        var folderlistParasDic = new Dictionary<string, string>();
        folderlistParasDic.Add(CosParameters.PARA_NUM, "100");
        folderlistParasDic.Add(CosParameters.PARA_ORDER, "0");
        folderlistParasDic.Add(CosParameters.PARA_PATTERN, FolderPattern.PATTERN_BOTH);
        result = cos.GetFolderList(bucketName, folder, folderlistParasDic);
        print("查询目录列表:" + result);
    }
    //更新目录
    public void UpdateCatalogue(string folder)
    {
        var result = "";
        var updateParasDic = new Dictionary<string, string>();
        updateParasDic.Add(CosParameters.PARA_BIZ_ATTR, "new attribute");
        result = cos.UpdateFolder(bucketName, folder, updateParasDic);
        print("目录更新:" + result);
    }

    public void CreateFile(string folder)
    {
        //建议注册账号的时候调用
        var result = "";
        result = cos.CreateFolder(bucketName, folder);
        print("创建文件目录：" + result);
    }

    //获取上传进度 COS暂不支持C#
    public void PutObjectProcess(string loacalPath,string fileName)
    {
        //try
        //{
        //    // 存储桶名称，此处填入格式必须为 bucketname-APPID, 其中 APPID 获取参考 https://console.cloud.tencent.com/developer
        //    string bucket = "examplebucket-1250000000";
        //    string key = "exampleobject"; //对象键
        //    string srcPath = @"temp-source-file";//本地文件绝对路径

        //    PutObjectRequest request = new PutObjectRequest(bucket, key, srcPath);
        //    //设置进度回调
        //    request.SetCosProgressCallback(delegate (long completed, long total)
        //    {
        //        Console.WriteLine(String.Format("progress = {0:##.##}%", completed * 100.0 / total));
        //    });
        //    //执行请求
        //    PutObjectResult result = cosXml.PutObject(request);
        //    //对象的 eTag
        //    string eTag = result.eTag;
        //}
        //catch (COSXML.CosException.CosClientException clientEx)
        //{
        //    //请求失败
        //    Console.WriteLine("CosClientException: " + clientEx);
        //}
        //catch (COSXML.CosException.CosServerException serverEx)
        //{
        //    //请求失败
        //    Console.WriteLine("CosServerException: " + serverEx.GetInfo());
        //}
    }

    //static void Main(string[] args)
    //{
    //    try
    //    {
    //        var result = "";

    //        const string bucketName = "kitmansh";
    //        const string localPath = @"C:\\testdata\a";
    //        const string remotePath = "/sdktest/a";
    //        const string folder = "/sdktest/";

    //        //创建文件夹
    //        result = cos.CreateFolder(bucketName, folder);
    //        Console.WriteLine("创建文件目录：" + result);


    //        //获取文件夹属性
    //        result = cos.GetFolderStat(bucketName, folder);
    //        Console.WriteLine("查询文件夹属性:" + result);


    //        //获取文件属性
    //        result = cos.GetFileStat(bucketName, remotePath);
    //        Console.WriteLine("获取文件属性:" + result);


    //        //设置可选参数
    //        var optionParasDic = new Dictionary<string, string>();
    //        optionParasDic.Add(CosParameters.PARA_BIZ_ATTR, "new attribute");
    //        optionParasDic.Add(CosParameters.PARA_AUTHORITY, AUTHORITY.AUTHORITY_PRIVATEPUBLIC);
    //        optionParasDic.Add(CosParameters.PARA_CACHE_CONTROL, "no");
    //        optionParasDic.Add(CosParameters.PARA_CONTENT_TYPE, "application/text");
    //        optionParasDic.Add(CosParameters.PARA_CONTENT_DISPOSITION, "inline filename=\"QC-7677.pdf\"");
    //        optionParasDic.Add(CosParameters.PARA_CONTENT_LANGUAGE, "en");
    //        optionParasDic.Add("x-cos-meta-test", "test");

    //        //更新文件
    //        result = cos.UpdateFile(bucketName, remotePath, optionParasDic);
    //        Console.WriteLine("更新文件属性" + result);

    //        //获取文件属性
    //        result = cos.GetFileStat(bucketName, remotePath);
    //        Console.WriteLine("获取文件属性:" + result);


    //        //删除文件夹
    //        result = cos.DeleteFolder(bucketName, folder);
    //        Console.WriteLine("删除文件夹:" + result);

    //    }
    //    catch (Exception e)
    //    {
    //        Console.WriteLine(e.Message);
    //    }
    //    Console.ReadKey();
    //}
}
