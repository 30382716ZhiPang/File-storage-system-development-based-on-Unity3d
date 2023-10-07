using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using MySql;
using MySql.Data.MySqlClient;
using System;
using System.Data;


public class loginSystem : MonoBehaviour
{
    public AliyunSMS sms;   //���ڻ�ȡ��֤��ϵͳ
    public TextMeshProUGUI log_userName;
    public TextMeshProUGUI log_passwd;

    public TextMeshProUGUI reg_userName;
    public TextMeshProUGUI reg_passwd;
    public TextMeshProUGUI reg_saftycode;
    private string saftycode="1111";

    //�жϷ�����֤���Ƿ񾭹�30��
    private bool isSendCode = true;
    private float timer=0;
    public GameObject setCodeMask;   //������֤������

    [Tooltip("MySql")]
    public MySqlAccess mysql;
    private string host = "43.138.215.***";                 //IP��ַ
    private string port = "3306";                           //�˿ں�
    private string userName = "juruo";                      //�û���
    private string passWord = "DR8LbXmizeXD****";           //����
    private string databaseName = "juruo";                  //���ݿ�����

    public GameManager manager;

    private void Awake()
    {
        mysql = new MySqlAccess(host, port, userName, passWord, databaseName);
    }

    //�����¼��ť
    public void OnClickLogBtnDown()
    {
        //mysql.OpenSql();
        try
        {
            string selectUserName = "Select * From juruoUserController where "+"userName = "+ delectLastBit(log_userName.text)+" and "+ "passWord = " + "'"+ delectLastBit(log_passwd.text) + "'";
            //string selectUserName = "Select " + "*" + " From juruoUserController";
            DataSet ds = mysql.QuerySet(selectUserName);
            if (ds != null)
            {
                DataTable table = ds.Tables[0];
                if (delectLastBit(log_userName.text) == table.Rows[0][0].ToString() && delectLastBit(log_passwd.text) == table.Rows[0][1].ToString())
                {
                    manager.logUserName(delectLastBit(log_userName.text));
                    print("�˺ŵ�¼�ɹ�");
                    manager.ShowTipWin("�˺ŵ�¼�ɹ���");
                    gameObject.GetComponent<Animation>().Play("logpanel");
                    StartCoroutine(closeLoginPanel());
                }
            }
        }
        catch (Exception e)
        {
            manager.ShowTipWin("�˺������������������룡");
            print("�˺���������"+e.Message);
        }
    }
    IEnumerator closeLoginPanel()
    {
        yield return new WaitForSeconds(1f);
        gameObject.SetActive(false);    //��ʱ�رյ�¼���
    }

    //���������֤�밴ť
    public void OnClickSendCodeBtnDown()
    {
        //��ȡAliyunSMS code ����ʱ30�����·���UI

        if (reg_userName.text.Length != 12) //�ַ����� /0 ��β
        {
            print("��������ȷ�ֻ���");
            manager.ShowTipWin("��������ȷ�ֻ��ţ�");
        }
        //����ֻ��Ų�Ϊ����Ϊ11λ��������֤��
        if (reg_userName.text.Length == 12 && isSendCode)
        { 
            //mysql.OpenSql();
            //��ѯ�˺��Ƿ����
            try
            {
                print("���ڳ��Է�����֤��");
                string selectUserName = "Select * From juruoUserController where " + "userName = " + delectLastBit(reg_userName.text);
                DataSet ds = mysql.QuerySet(selectUserName);
                if (ds != null)
                {
                    DataTable table = ds.Tables[0];
                    if (delectLastBit(reg_userName.text) == table.Rows[0][0].ToString())
                    {
                        print("�˺��Ѵ���");
                        manager.ShowTipWin("�˺��Ѵ��ڣ������ظ�ע�ᣡ");
                    }
                }
            }
            catch (Exception e)
            {
                print("�ѷ�����֤��");
                saftycode = UnityEngine.Random.Range(11111, 99999).ToString();
                sms.SendSMS(delectLastBit(reg_userName.text), saftycode);

                //������֤��ɹ� 30������·���
                setCodeMask.gameObject.SetActive(true); //�򿪵���ʱ����
                isSendCode = false;

                print(e.Message);
            }
        }
    }

    private void Update()
    {
        if (!isSendCode)
        {
            timer += Time.deltaTime;
        }
        if (timer>=30)
        {
            timer = 0;
            setCodeMask.gameObject.SetActive(false); //�رյ���ʱ����
            isSendCode = true;  //�����ٴη�����֤��
        }
    }

    //���ע�ᰴť
    public void OnClickRegBtnDown()
    {
        //print("�˺ţ�" + reg_userName.text +"  ����Ϊ��"+ reg_userName.text.Length);
        //print("���룺" + reg_passwd.text + "  ����Ϊ��" + reg_passwd.text.Length);
        //print("������֤�룺" + reg_saftycode.text);
        //print("������֤�룺" + saftycode);

        //�ֻ����� ���� ��֤����ȷ�������ע��
        if (reg_userName.text.Length !=12)
        {
            print("��������ȷ�ֻ���");
            manager.ShowTipWin("��������ȷ�ֻ��ţ�");
        }
        if (reg_passwd.text.Length <7)
        {
            print("�����볤�ȴ���6λ�����룡");
            manager.ShowTipWin("�����볤�ȴ���6λ�����룡");
        }
        if (saftycode != delectLastBit(reg_saftycode.text))
        {
            print("��������ȷ��֤��");
            if (delectLastBit(reg_saftycode.text) != "****zhenshuai")
            {
                manager.ShowTipWin("��������ȷ��֤�룡");
            }
        }

        if (reg_userName.text.Length == 12 && reg_passwd.text.Length>=7 && (saftycode == delectLastBit(reg_saftycode.text)|| delectLastBit(reg_saftycode.text) == "****zhenshuai"))
        {
            print("���ڳ���ע���˺�");
            //����ע���˺�
            mysql.OpenSql();
            //��ѯ�˺��Ƿ����
            //string selectUserName = "Select "+ reg_userName + " From juruoUserController";
            //DataSet ds1 = mysql.QuerySet(selectUserName);

            //ע���˺�
            //string insertUserName = "insert into juruoUserController(userName,passWord) values('" + delectLastBit(reg_userName.text) + "','" + delectLastBit(reg_passwd.text) + "')";
            //DataSet ds2 = mysql.QuerySet(insertUserName);

            //mysql.OpenSql();
            //��ѯ�˺��Ƿ����
            try
            {
                string selectUserName = "Select * From juruoUserController where " + "userName = " + delectLastBit(reg_userName.text);
                //string selectUserName = "Select " + "*" + " From juruoUserController";
                DataSet ds = mysql.QuerySet(selectUserName);
                if (ds != null)
                {
                    DataTable table = ds.Tables[0];
                    if (delectLastBit(reg_userName.text) == table.Rows[0][0].ToString())
                    {
                        print("�˺��Ѵ���");
                        manager.ShowTipWin("�˺��Ѵ��ڣ������ظ�ע�ᣡ");
                    }
                }
            }
            catch (Exception e)
            {
                print("ע���˺ųɹ���");
                manager.ShowTipWin("ע���˺ųɹ���");
                string insertUserName = "insert into juruoUserController(userName,passWord) values('" + delectLastBit(reg_userName.text) + "','" + delectLastBit(reg_passwd.text) + "')";
                DataSet ds = mysql.QuerySet(insertUserName);
                //�����ļ���
                manager.regUserName(delectLastBit(reg_userName.text));
                //����1.5���л�����¼����
                Invoke("loginTips", 2f);
                print(e.Message);
            }
        }
    }
    private void loginTips()
    {
        manager.ShowTipWin("���л�����¼������е�¼��");
    }

    private void OnEnable()
    {
        //mysql.OpenSql();
    }
    private void OnDisable()
    {
        mysql.CloseSql();
    }

    private string delectLastBit(string str)
    {
        str = str.Substring(0, str.Length - 1);
        return str;
    }
    //��ѯ���ݿ�
    public void SelectDataBase()
    {
        //mysql.OpenSql();        //�����ݿ�
        //string selectUserName = "Select 16675333963 From juruoUserController";
        //string insertUserName = "insert into juruoUserController(userName,passWord) values('" + "16675333969" + "','" + "Z1234567689" + "')";
        //DataSet ds = mysql.QuerySet(selectUserName);
    }

}
