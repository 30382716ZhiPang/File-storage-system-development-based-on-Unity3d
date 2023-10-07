using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Items : MonoBehaviour
{
    public Text txt_item;

    //设置text文本内容
    public void SetTextString(string str)
    {
        txt_item.text = str;
    }
    //获取text文本内容
    public string GetTextString()
    {
        return txt_item.text;
    }
    
    //点击删除按钮 返回text文本内容
    public void onDelectBtnDown()
    {
        EventCenter.Broadcast<string>(EventDefine.DelectFile, txt_item.text);
    }
    //点击下载按钮 返回text文本内容
    public void onDownloadBtnDown()
    {
        EventCenter.Broadcast<string>(EventDefine.DownLoadFile, txt_item.text);
    }
}
