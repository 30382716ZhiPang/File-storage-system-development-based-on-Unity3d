using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public AliyunSMS sms;
    public TencentCOS cos;
    public FileChoose fileData;
    public string ID;   //�û���¼ID

    private string chooseFilePath;
    private string chooseFileName;
    private string downloadFileName;
    public Button refreshbtn;

    List<string> namelist = new List<string>();     //�洢Ŀ¼
    //public GameObject Item;
    public GameObject ListParent;

    public ItemsPool itemsPool; //item��Դ��

    public GameObject tips;
    public Text txt_tips;

    public GameObject process;

    private void Awake()
    {
        //cos.AccessFile(@"/16675333963"); //�����ļ���
        //cos.CreateFile(@"/166"); //�����ļ���
        //cos.UploadObj(PutSucessCallBack, @"16675333963/�����ļ�1.txt", @"D:\Documents\Desktop\�����ļ�1.txt");   //�ϴ��ļ���
        //cos.GetObjectWithUnityWebRequest(GetSucessCallBack, ID, "��ҵ7(1).docx", @"C:\Users\10753\Downloads\��ҵ7(1).docx");

        EventCenter.AddListener<string>(EventDefine.DelectFile, DelectFile);
        EventCenter.AddListener<string>(EventDefine.DownLoadFile, DownLoadFile);
        
        ID = "12345678910/";    //����
    }
    private void OnDestroy()
    {
        EventCenter.RemoveListener<string>(EventDefine.DelectFile, DelectFile);
        EventCenter.RemoveListener<string>(EventDefine.DownLoadFile, DownLoadFile);
    }


    private void OnInit()
    {
        //ID = "16675333963/";    //����
        //regUserName(ID);        //ע���˺� ��Ҫע��
    }

    //��¼ʱ���� �����û���ʶ
    public void logUserName(string userName)
    {
        ID = userName + "/";
        OnInit();
        string str;
        if (userName=="13410725789")
        {
            str = " (�š䨌`)�ű�~ ����Ө";
        }
        else
        {
            str = "Welcome to login��" + userName + " ...";
        }
        EventCenter.Broadcast<string>(EventDefine.Console, str);
    }

    //ע��ʱ���� ���д����˻�
    public void regUserName(string userName)
    {
        cos.CreateFile(userName + "/");
    }

    private void Update()
    {
        //���Թ���
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //EventCenter.Broadcast<string>(EventDefine.Console, "Download File juruo.txt is OK!");
            //ShowTipWin("�˺������������������룡");
        }
    }
    //���ɾ����ť
    private void DelectFile(string fineName)
    {
        //ɾ���ļ�
        cos.DelectObj(DelectSucessCallBack, ID, fineName);

        string str = "File deleted successfully��" + fineName;
        EventCenter.Broadcast<string>(EventDefine.Console, str);
        //ˢ���б�
        RefreshBtnDown();
    }
    //������ذ�ť
    private void DownLoadFile(string fileName)
    {
        downloadFileName = fileName;
        //�����ļ�
        string SaveStr = fileData.ChooseFileSavePath().ToString();
        //string fileStr = "test.txt";
        SaveStr = SaveStr + "/" + fileName;
        print("����·���� " + SaveStr);
        cos.GetObjectWithUnityWebRequest(GetSucessCallBack, ID, fileName, SaveStr);

        string str = downloadFileName+ " File downloading..." ;
        EventCenter.Broadcast<string>(EventDefine.Console, str);

        string str2 = "����·���� " + SaveStr;
        EventCenter.Broadcast<string>(EventDefine.Console, str2);

        //���ؽ�����
        process.gameObject.SetActive(true);
        StartCoroutine(closeProcess());
    }



    //����ϴ���ť
    public void PutFileBtnDown()
    {
        if (fileData.ishaveFile)
        {
            chooseFilePath = fileData.ChoosePatch.text.ToString();
            chooseFileName = fileData.ChooseFileName;
            //cos.UploadObj(PutSucessCallBack, @"/16675333963/366.txt", @"D:\Documents\Desktop\�����ļ�.txt");
            cos.UploadObj(PutSucessCallBack, ID + chooseFileName, chooseFilePath);
            string str = chooseFileName +" File uploading... ";
            EventCenter.Broadcast<string>(EventDefine.Console, str);
            
            //���ؽ�����
            process.gameObject.SetActive(true);
            StartCoroutine(closeProcess());
            RefreshBtnDown();
        }
    }
    //���ˢ�°�ť
    public void RefreshBtnDown()
    {
        //���Ŷ���
        refreshbtn.gameObject.GetComponent<Animation>().Play("refresh");
        //�ӳ�1�����ˢ��
        StartCoroutine(startRefresh());
    }
    private IEnumerator startRefresh()
    {
        yield return new WaitForSeconds(1f);
        //����ˢ���б� /����Ŀ¼
        cos.UpdateCatalogue(ID);
        //�ر� ListĿ¼
        itemsPool.ReturnItem();
        //����ʵ���� List
        InstanceFileList();
        string str = "Refreshing list...";
        EventCenter.Broadcast<string>(EventDefine.Console, str);
    }
    //ʵ�����ļ��б�
    public void InstanceFileList()
    {
        //��ȡĿ¼
        namelist = cos.GetFileList(ID);
        foreach (var item in namelist)
        {
            print(item);
            //GameObject go = Instantiate(Item);
            ////�޸�Item�������� 
            //go.GetComponent<Items>().SetTextString(item);
            ////�޸�Item����λ��
            //go.transform.parent = ListParent.transform;

            GameObject go = itemsPool.GetItem();
            go.GetComponent<Items>().SetTextString(item);
            go.transform.parent = ListParent.transform;
        }
    }

    private void PutSucessCallBack()
    {
        //string str = "File uploaded successfully��" + chooseFileName;
        //EventCenter.Broadcast<string>(EventDefine.Console, str);
        Debug.Log("�ϴ��ɹ�");
    }
    private void GetSucessCallBack()
    {
        //string str = "File download successfully��" + downloadFileName;
        //EventCenter.Broadcast<string>(EventDefine.Console, str);
        Debug.Log("��������");
    }
    private void DelectSucessCallBack()
    {
        Debug.Log("ɾ���ɹ�");
    }

    //��ʾtips��Ŀ
    public void ShowTipWin(string str)
    {
        //����λtrue�������Զ����Ŷ��� ��������
        tips.gameObject.SetActive(true);
        tips.gameObject.GetComponent<Animator>().Play("In");

        //�޸���������
        txt_tips.text = str;
    }
    
    //������
    private IEnumerator closeProcess()
    {
        yield return new WaitForSeconds(4f);
        process.gameObject.SetActive(false);
    }
}

    
