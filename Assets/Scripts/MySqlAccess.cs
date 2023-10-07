using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MySql.Data;
using MySql.Data.MySqlClient;
using System;
using System.Data;

public class MySqlAccess : MonoBehaviour
{
    //连接类对象
    private static MySqlConnection mySqlConnection;
    //IP地址
    private static string host;
    //端口号
    private static string port;
    //用户名
    private static string userName;
    //密码
    private static string password;
    //数据库名称
    private static string databaseName;
    /// <summary>
    /// 初始化
    /// </summary>
    public MySqlAccess(string _host, string _port, string _userName, string _password, string _databaseName)
    {
        host = _host;
        port = _port;
        userName = _userName;
        password = _password;
        databaseName = _databaseName;
        OpenSql();
    }
    /// <summary>
    /// 打开数据库
    /// </summary>
    public void OpenSql()
    {
        try
        {
            string mySqlString = string.Format("Database={0};Data Source={1};User Id={2};Password={3};port={4}; charset = utf8"
                , databaseName, host, userName, password, port);
            mySqlConnection = new MySqlConnection(mySqlString);
            mySqlConnection.Open();
        }
        catch (Exception e)
        {
            EventCenter.Broadcast<string>(EventDefine.Console, "服务器连接失败，请重新检查MySql服务是否打开。");
            throw new Exception("服务器连接失败，请重新检查MySql服务是否打开。" + e.Message.ToString());
        }
    }

    /// <summary>
    /// 关闭数据库
    /// </summary>
    public void CloseSql()
    {
        if (mySqlConnection != null)
        {
            mySqlConnection.Close();
            mySqlConnection.Dispose();
            mySqlConnection = null;
        }
    }
    /// <summary>
    /// 执行SQL语句
    /// </summary>
    public DataSet QuerySet(string sqlString)
    {
        if (mySqlConnection.State == ConnectionState.Open)
        {
            DataSet ds = new DataSet();
            try
            {
                MySqlDataAdapter mySqlAsapter = new MySqlDataAdapter(sqlString, mySqlConnection);
                mySqlAsapter.Fill(ds);
            }
            catch (Exception e)
            {
                throw new Exception("SQL:" + sqlString + "/n" + e.Message.ToString());
            }
            return ds;
        }
        return null;
    }

}
