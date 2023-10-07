using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public AliyunSMS sms;
    public TencentCOS cos;
    public FileChoose fileData;
    public string ID;   //用户登录ID

    private string chooseFilePath;
    private string chooseFileName;
    private string downloadFileName;
    public Button refreshbtn;

    List<string> namelist = new List<string>();     //存储目录
    //public GameObject Item;
    public GameObject ListParent;

    public ItemsPool itemsPool; //item资源池

    public GameObject tips;
    public Text txt_tips;

    public GameObject process;

    private void Awake()
    {
        //cos.AccessFile(@"/16675333963"); //访问文件夹
        //cos.CreateFile(@"/166"); //创建文件夹
        //cos.UploadObj(PutSucessCallBack, @"16675333963/测试文件1.txt", @"D:\Documents\Desktop\测试文件1.txt");   //上传文件夹
        //cos.GetObjectWithUnityWebRequest(GetSucessCallBack, ID, "作业7(1).docx", @"C:\Users\10753\Downloads\作业7(1).docx");

        EventCenter.AddListener<string>(EventDefine.DelectFile, DelectFile);
        EventCenter.AddListener<string>(EventDefine.DownLoadFile, DownLoadFile);
        
        ID = "12345678910/";    //测试
    }
    private void OnDestroy()
    {
        EventCenter.RemoveListener<string>(EventDefine.DelectFile, DelectFile);
        EventCenter.RemoveListener<string>(EventDefine.DownLoadFile, DownLoadFile);
    }


    private void OnInit()
    {
        //ID = "16675333963/";    //测试
        //regUserName(ID);        //注册账号 需要注销
    }

    //登录时调用 进行用户标识
    public void logUserName(string userName)
    {
        ID = userName + "/";
        OnInit();
        string str;
        if (userName=="13410725789")
        {
            str = " (づ′▽`)づ抱~ 宝子莹";
        }
        else
        {
            str = "Welcome to login：" + userName + " ...";
        }
        EventCenter.Broadcast<string>(EventDefine.Console, str);
    }

    //注册时调用 进行创建账户
    public void regUserName(string userName)
    {
        cos.CreateFile(userName + "/");
    }

    private void Update()
    {
        //测试功能
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //EventCenter.Broadcast<string>(EventDefine.Console, "Download File juruo.txt is OK!");
            //ShowTipWin("账号密码有误，请重新输入！");
        }
    }
    //点击删除按钮
    private void DelectFile(string fineName)
    {
        //删除文件
        cos.DelectObj(DelectSucessCallBack, ID, fineName);

        string str = "File deleted successfully：" + fineName;
        EventCenter.Broadcast<string>(EventDefine.Console, str);
        //刷新列表
        RefreshBtnDown();
    }
    //点击下载按钮
    private void DownLoadFile(string fileName)
    {
        downloadFileName = fileName;
        //下载文件
        string SaveStr = fileData.ChooseFileSavePath().ToString();
        //string fileStr = "test.txt";
        SaveStr = SaveStr + "/" + fileName;
        print("保存路径： " + SaveStr);
        cos.GetObjectWithUnityWebRequest(GetSucessCallBack, ID, fileName, SaveStr);

        string str = downloadFileName+ " File downloading..." ;
        EventCenter.Broadcast<string>(EventDefine.Console, str);

        string str2 = "保存路径： " + SaveStr;
        EventCenter.Broadcast<string>(EventDefine.Console, str2);

        //开关进度条
        process.gameObject.SetActive(true);
        StartCoroutine(closeProcess());
    }



    //点击上传按钮
    public void PutFileBtnDown()
    {
        if (fileData.ishaveFile)
        {
            chooseFilePath = fileData.ChoosePatch.text.ToString();
            chooseFileName = fileData.ChooseFileName;
            //cos.UploadObj(PutSucessCallBack, @"/16675333963/366.txt", @"D:\Documents\Desktop\测试文件.txt");
            cos.UploadObj(PutSucessCallBack, ID + chooseFileName, chooseFilePath);
            string str = chooseFileName +" File uploading... ";
            EventCenter.Broadcast<string>(EventDefine.Console, str);
            
            //开关进度条
            process.gameObject.SetActive(true);
            StartCoroutine(closeProcess());
            RefreshBtnDown();
        }
    }
    //点击刷新按钮
    public void RefreshBtnDown()
    {
        //播放动画
        refreshbtn.gameObject.GetComponent<Animation>().Play("refresh");
        //延迟1秒调用刷新
        StartCoroutine(startRefresh());
    }
    private IEnumerator startRefresh()
    {
        yield return new WaitForSeconds(1f);
        //调用刷新列表 /更新目录
        cos.UpdateCatalogue(ID);
        //关闭 List目录
        itemsPool.ReturnItem();
        //重新实例化 List
        InstanceFileList();
        string str = "Refreshing list...";
        EventCenter.Broadcast<string>(EventDefine.Console, str);
    }
    //实例化文件列表
    public void InstanceFileList()
    {
        //获取目录
        namelist = cos.GetFileList(ID);
        foreach (var item in namelist)
        {
            print(item);
            //GameObject go = Instantiate(Item);
            ////修改Item文字内容 
            //go.GetComponent<Items>().SetTextString(item);
            ////修改Item物体位置
            //go.transform.parent = ListParent.transform;

            GameObject go = itemsPool.GetItem();
            go.GetComponent<Items>().SetTextString(item);
            go.transform.parent = ListParent.transform;
        }
    }

    private void PutSucessCallBack()
    {
        //string str = "File uploaded successfully：" + chooseFileName;
        //EventCenter.Broadcast<string>(EventDefine.Console, str);
        Debug.Log("上传成功");
    }
    private void GetSucessCallBack()
    {
        //string str = "File download successfully：" + downloadFileName;
        //EventCenter.Broadcast<string>(EventDefine.Console, str);
        Debug.Log("正在下载");
    }
    private void DelectSucessCallBack()
    {
        Debug.Log("删除成功");
    }

    //显示tips栏目
    public void ShowTipWin(string str)
    {
        //设置位true，让其自动播放动画 过会隐藏
        tips.gameObject.SetActive(true);
        tips.gameObject.GetComponent<Animator>().Play("In");

        //修改文字内容
        txt_tips.text = str;
    }
    
    //进度条
    private IEnumerator closeProcess()
    {
        yield return new WaitForSeconds(4f);
        process.gameObject.SetActive(false);
    }
}

    
