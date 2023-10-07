using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Items : MonoBehaviour
{
    public Text txt_item;

    //����text�ı�����
    public void SetTextString(string str)
    {
        txt_item.text = str;
    }
    //��ȡtext�ı�����
    public string GetTextString()
    {
        return txt_item.text;
    }
    
    //���ɾ����ť ����text�ı�����
    public void onDelectBtnDown()
    {
        EventCenter.Broadcast<string>(EventDefine.DelectFile, txt_item.text);
    }
    //������ذ�ť ����text�ı�����
    public void onDownloadBtnDown()
    {
        EventCenter.Broadcast<string>(EventDefine.DownLoadFile, txt_item.text);
    }
}
