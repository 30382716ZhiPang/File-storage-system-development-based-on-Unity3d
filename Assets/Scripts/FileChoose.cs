using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 通过文件夹选择文件 直接选择.obj文件 并记录路径
/// </summary>
public class FileChoose : MonoBehaviour
{
    public Text ChoosePatch;
    [HideInInspector]
    public string ChooseFileName;
    [HideInInspector]
    public bool ishaveFile=false;

    public void ChooseFileBtnDown()
    {
        OpenDialogFile openFileName = new OpenDialogFile();
        openFileName.structSize = Marshal.SizeOf(openFileName);
        //openFileName.filter = "obj文件(*.obj; *.mtl)\0*.obj; *.mtl\0";//指定筛选的文件格式，指定格式写在\0 \0中间
        openFileName.filter = null;//指定筛选的文件格式，指定格式写在\0 \0中间
        openFileName.file = new string(new char[256]);
        openFileName.maxFile = openFileName.file.Length;
        openFileName.fileTitle = new string(new char[64]);
        openFileName.maxFileTitle = openFileName.fileTitle.Length;
        openFileName.initialDir = Application.streamingAssetsPath.Replace('/', '\\');//默认路径
        openFileName.title = "模型文件选择窗口";
        openFileName.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000008;
        openFileName.dlgOwner = GetActiveWindow();    //将窗口置于最前面
        if (DllOpenFileDialog.GetSaveFileName(openFileName))
        {
            ishaveFile = true;
            ChoosePatch.text = openFileName.file.ToString();
            ChooseFileName = openFileName.fileTitle.ToString();
            Debug.Log("filePath" + openFileName.file);//文件路径
            Debug.Log("fileName" + openFileName.fileTitle);//文件名
            //Settings.ObjPath = openFileName.file;
            //GetComponent<LoadModel>().LoadModelEvent();
        }

    }
    public string ChooseFileSavePath()
    {
        //使用如下：
        OpenDialogDir ofn2 = new OpenDialogDir();
        ofn2.pszDisplayName = new string(new char[2048]); ;     // 存放目录路径缓冲区  
        ofn2.lpszTitle = "选择保存路径";// 标题  
        //ofn2.ulFlags = BIF_NEWDIALOGSTYLE | BIF_EDITBOX; // 新的样式,带编辑框  
        //IntPtr pidlPtr = IntPtr.Zero;
        IntPtr pidlPtr = DllOpenFileDialog.SHBrowseForFolder(ofn2);

        char[] charArray = new char[2048];
        for (int i = 0; i < 2048; i++)
            charArray[i] = '\0';

        DllOpenFileDialog.SHGetPathFromIDList(pidlPtr, charArray);
        string fullDirPath = new String(charArray);
        fullDirPath = fullDirPath.Substring(0, fullDirPath.IndexOf('\0'));

        Debug.Log(fullDirPath);//这个就是选择的目录路径
        return fullDirPath;
    }


    #region 选择文件GUI
    //void OnGUI()
    //{
    //    //选择某一文件
    //    if (GUI.Button(new Rect(10, 10, 100, 50), "ChooseFile"))
    //    {
    //        OpenDialogFile openFileName = new OpenDialogFile();
    //        openFileName.structSize = Marshal.SizeOf(openFileName);
    //        //openFileName.filter = "obj文件(*.obj; *.mtl)\0*.obj; *.mtl\0";//指定筛选的文件格式，指定格式写在\0 \0中间
    //        openFileName.filter = null ;//指定筛选的文件格式，指定格式写在\0 \0中间
    //        openFileName.file = new string(new char[256]);
    //        openFileName.maxFile = openFileName.file.Length;
    //        openFileName.fileTitle = new string(new char[64]);
    //        openFileName.maxFileTitle = openFileName.fileTitle.Length;
    //        openFileName.initialDir = Application.streamingAssetsPath.Replace('/', '\\');//默认路径
    //        openFileName.title = "模型文件选择窗口";
    //        openFileName.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000008;
    //        openFileName.dlgOwner = GetActiveWindow();    //将窗口置于最前面
    //        if (DllOpenFileDialog.GetSaveFileName(openFileName))
    //        {
    //            Debug.Log("filePath" + openFileName.file);//文件路径
    //            Debug.Log("fileName" + openFileName.fileTitle);//文件名
    //            //Settings.ObjPath = openFileName.file;
    //            //GetComponent<LoadModel>().LoadModelEvent();
    //        }
    //    }
    //    //选择某一文件夹
    //    if (GUI.Button(new Rect(10, 100, 100, 50), "ChooseDirectory"))
    //    {
    //        OpenDialogDir openDir = new OpenDialogDir();
    //        openDir.pszDisplayName = new string(new char[2000]);
    //        openDir.lpszTitle = "资源文件夹选择";
    //        openDir.ulFlags = 1;// BIF_NEWDIALOGSTYLE | BIF_EDITBOX;
    //        IntPtr pidl = DllOpenFileDialog.SHBrowseForFolder(openDir);

    //        char[] path = new char[2000];
    //        for (int i = 0; i < 2000; i++)
    //            path[i] = '\0';
    //        if (DllOpenFileDialog.SHGetPathFromIDList(pidl, path))
    //        {
    //            string str = new string(path);
    //            string DirPath = str.Substring(0, str.IndexOf('\0'));
    //            Debug.Log("路径" + DirPath);
    //        }
    //    }
    //}
    #endregion

    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();
}
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
public class OpenDialogFile
{
    public int structSize = 0;
    public IntPtr dlgOwner = IntPtr.Zero;
    public IntPtr instance = IntPtr.Zero;
    public String filter = null;
    public String customFilter = null;
    public int maxCustFilter = 0;
    public int filterIndex = 0;
    public String file = null;
    public int maxFile = 0;
    public String fileTitle = null;
    public int maxFileTitle = 0;
    public String initialDir = null;
    public String title = null;
    public int flags = 0;
    public short fileOffset = 0;
    public short fileExtension = 0;
    public String defExt = null;
    public IntPtr custData = IntPtr.Zero;
    public IntPtr hook = IntPtr.Zero;
    public String templateName = null;
    public IntPtr reservedPtr = IntPtr.Zero;
    public int reservedInt = 0;
    public int flagsEx = 0;
}
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
public class OpenDialogDir
{
    public IntPtr hwndOwner = IntPtr.Zero;
    public IntPtr pidlRoot = IntPtr.Zero;
    public String pszDisplayName = "123";
    public String lpszTitle = null;
    public UInt32 ulFlags = 0;
    public IntPtr lpfn = IntPtr.Zero;
    public IntPtr lParam = IntPtr.Zero;
    public int iImage = 0;
}
public class DllOpenFileDialog
{
    [DllImport("Comdlg32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
    public static extern bool GetOpenFileName([In, Out] OpenDialogFile ofn);

    [DllImport("Comdlg32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
    public static extern bool GetSaveFileName([In, Out] OpenDialogFile ofn);

    [DllImport("shell32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
    public static extern IntPtr SHBrowseForFolder([In, Out] OpenDialogDir ofn);

    [DllImport("shell32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
    public static extern bool SHGetPathFromIDList([In] IntPtr pidl, [In, Out] char[] fileName);
}