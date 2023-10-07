using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemsPool : MonoBehaviour
{
    public int poolCount = 5;
    public GameObject ItemPrefab;
    private List<GameObject> ItemLiset = new List<GameObject>();
    private void Start()
    {
        InitPool();
    }
    //初始化资源池
    void InitPool()
    {
        for (int i = 0; i < poolCount; i++)
        {
            GameObject go = GameObject.Instantiate(ItemPrefab);
            ItemLiset.Add(go);
            go.SetActive(false);
            //go.transform.parent = this.transform;
        }
    }
    //获取资源
    public GameObject GetItem()
    {
        foreach (GameObject go in ItemLiset)
        {
            //如果资源池内有资源未被使用，则将资源返回
            if (go.activeInHierarchy == false)
            {
                go.SetActive(true);
                return go;
            }
        }
        //如果为空 则继续新增
        GameObject go2 = GameObject.Instantiate(ItemPrefab);
        ItemLiset.Add(go2);
        //go2.transform.parent = this.transform;
        poolCount++;
        return go2;
    }
    //关闭资源
    public void ReturnItem()
    {
        foreach (GameObject go in ItemLiset)
        {
            //如果资源池内有资源被使用，则将资源取回
            if (go.activeInHierarchy)
            {
                go.SetActive(false);
            }
        }
    }

}
