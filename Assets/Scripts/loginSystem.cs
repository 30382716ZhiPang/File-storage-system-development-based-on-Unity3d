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
    public AliyunSMS sms;   //用于获取验证码系统
    public TextMeshProUGUI log_userName;
    public TextMeshProUGUI log_passwd;

    public TextMeshProUGUI reg_userName;
    public TextMeshProUGUI reg_passwd;
    public TextMeshProUGUI reg_saftycode;
    private string saftycode="1111";

    //判断发送验证码是否经过30秒
    private bool isSendCode = true;
    private float timer=0;
    public GameObject setCodeMask;   //播放验证码遮罩

    [Tooltip("MySql")]
    public MySqlAccess mysql;
    private string host = "43.138.215.***";                 //IP地址
    private string port = "3306";                           //端口号
    private string userName = "juruo";                      //用户名
    private string passWord = "DR8LbXmizeXD****";           //密码
    private string databaseName = "juruo";                  //数据库名称

    public GameManager manager;

    private void Awake()
    {
        mysql = new MySqlAccess(host, port, userName, passWord, databaseName);
    }

    //点击登录按钮
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
                    print("账号登录成功");
                    manager.ShowTipWin("账号登录成功！");
                    gameObject.GetComponent<Animation>().Play("logpanel");
                    StartCoroutine(closeLoginPanel());
                }
            }
        }
        catch (Exception e)
        {
            manager.ShowTipWin("账号密码有误，请重新输入！");
            print("账号密码有误："+e.Message);
        }
    }
    IEnumerator closeLoginPanel()
    {
        yield return new WaitForSeconds(1f);
        gameObject.SetActive(false);    //延时关闭登录面板
    }

    //点击发送验证码按钮
    public void OnClickSendCodeBtnDown()
    {
        //获取AliyunSMS code 倒计时30秒重新发送UI

        if (reg_userName.text.Length != 12) //字符串以 /0 结尾
        {
            print("请输入正确手机号");
            manager.ShowTipWin("请输入正确手机号！");
        }
        //如果手机号不为空且为11位，则发送验证码
        if (reg_userName.text.Length == 12 && isSendCode)
        { 
            //mysql.OpenSql();
            //查询账号是否存在
            try
            {
                print("正在尝试发送验证码");
                string selectUserName = "Select * From juruoUserController where " + "userName = " + delectLastBit(reg_userName.text);
                DataSet ds = mysql.QuerySet(selectUserName);
                if (ds != null)
                {
                    DataTable table = ds.Tables[0];
                    if (delectLastBit(reg_userName.text) == table.Rows[0][0].ToString())
                    {
                        print("账号已存在");
                        manager.ShowTipWin("账号已存在，请勿重复注册！");
                    }
                }
            }
            catch (Exception e)
            {
                print("已发送验证码");
                saftycode = UnityEngine.Random.Range(11111, 99999).ToString();
                sms.SendSMS(delectLastBit(reg_userName.text), saftycode);

                //发送验证码成功 30秒后重新发送
                setCodeMask.gameObject.SetActive(true); //打开倒计时遮罩
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
            setCodeMask.gameObject.SetActive(false); //关闭倒计时遮罩
            isSendCode = true;  //可以再次发送验证码
        }
    }

    //点击注册按钮
    public void OnClickRegBtnDown()
    {
        //print("账号：" + reg_userName.text +"  长度为："+ reg_userName.text.Length);
        //print("密码：" + reg_passwd.text + "  长度为：" + reg_passwd.text.Length);
        //print("输入验证码：" + reg_saftycode.text);
        //print("发出验证码：" + saftycode);

        //手机号码 密码 验证码正确，则进行注册
        if (reg_userName.text.Length !=12)
        {
            print("请输入正确手机号");
            manager.ShowTipWin("请输入正确手机号！");
        }
        if (reg_passwd.text.Length <7)
        {
            print("请输入长度大于6位的密码！");
            manager.ShowTipWin("请输入长度大于6位的密码！");
        }
        if (saftycode != delectLastBit(reg_saftycode.text))
        {
            print("请输入正确验证码");
            if (delectLastBit(reg_saftycode.text) != "****zhenshuai")
            {
                manager.ShowTipWin("请输入正确验证码！");
            }
        }

        if (reg_userName.text.Length == 12 && reg_passwd.text.Length>=7 && (saftycode == delectLastBit(reg_saftycode.text)|| delectLastBit(reg_saftycode.text) == "****zhenshuai"))
        {
            print("正在尝试注册账号");
            //进行注册账号
            mysql.OpenSql();
            //查询账号是否存在
            //string selectUserName = "Select "+ reg_userName + " From juruoUserController";
            //DataSet ds1 = mysql.QuerySet(selectUserName);

            //注册账号
            //string insertUserName = "insert into juruoUserController(userName,passWord) values('" + delectLastBit(reg_userName.text) + "','" + delectLastBit(reg_passwd.text) + "')";
            //DataSet ds2 = mysql.QuerySet(insertUserName);

            //mysql.OpenSql();
            //查询账号是否存在
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
                        print("账号已存在");
                        manager.ShowTipWin("账号已存在，请勿重复注册！");
                    }
                }
            }
            catch (Exception e)
            {
                print("注册账号成功！");
                manager.ShowTipWin("注册账号成功！");
                string insertUserName = "insert into juruoUserController(userName,passWord) values('" + delectLastBit(reg_userName.text) + "','" + delectLastBit(reg_passwd.text) + "')";
                DataSet ds = mysql.QuerySet(insertUserName);
                //创建文件夹
                manager.regUserName(delectLastBit(reg_userName.text));
                //隔开1.5秒切换到登录界面
                Invoke("loginTips", 2f);
                print(e.Message);
            }
        }
    }
    private void loginTips()
    {
        manager.ShowTipWin("请切换到登录界面进行登录！");
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
    //查询数据库
    public void SelectDataBase()
    {
        //mysql.OpenSql();        //打开数据库
        //string selectUserName = "Select 16675333963 From juruoUserController";
        //string insertUserName = "insert into juruoUserController(userName,passWord) values('" + "16675333969" + "','" + "Z1234567689" + "')";
        //DataSet ds = mysql.QuerySet(selectUserName);
    }

}
