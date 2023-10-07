using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MySql.Data;
using MySql.Data.MySqlClient;
using System;
using System.Data;

public class MySqlAccess : MonoBehaviour
{
    //���������
    private static MySqlConnection mySqlConnection;
    //IP��ַ
    private static string host;
    //�˿ں�
    private static string port;
    //�û���
    private static string userName;
    //����
    private static string password;
    //���ݿ�����
    private static string databaseName;
    /// <summary>
    /// ��ʼ��
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
    /// �����ݿ�
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
            EventCenter.Broadcast<string>(EventDefine.Console, "����������ʧ�ܣ������¼��MySql�����Ƿ�򿪡�");
            throw new Exception("����������ʧ�ܣ������¼��MySql�����Ƿ�򿪡�" + e.Message.ToString());
        }
    }

    /// <summary>
    /// �ر����ݿ�
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
    /// ִ��SQL���
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
