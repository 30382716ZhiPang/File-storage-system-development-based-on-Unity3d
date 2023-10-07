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
    //��ʼ����Դ��
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
    //��ȡ��Դ
    public GameObject GetItem()
    {
        foreach (GameObject go in ItemLiset)
        {
            //�����Դ��������Դδ��ʹ�ã�����Դ����
            if (go.activeInHierarchy == false)
            {
                go.SetActive(true);
                return go;
            }
        }
        //���Ϊ�� ���������
        GameObject go2 = GameObject.Instantiate(ItemPrefab);
        ItemLiset.Add(go2);
        //go2.transform.parent = this.transform;
        poolCount++;
        return go2;
    }
    //�ر���Դ
    public void ReturnItem()
    {
        foreach (GameObject go in ItemLiset)
        {
            //�����Դ��������Դ��ʹ�ã�����Դȡ��
            if (go.activeInHierarchy)
            {
                go.SetActive(false);
            }
        }
    }

}
