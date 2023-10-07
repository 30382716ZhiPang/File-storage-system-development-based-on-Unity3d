using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class consoleSystem : MonoBehaviour
{
    public Text console;
    private float width = 740;
    private float hight = 400;
    private void Awake()
    {
        EventCenter.AddListener<string>(EventDefine.Console, InputText);
    }
    private void OnDestroy()
    {
        EventCenter.RemoveListener<string>(EventDefine.Console, InputText);
    }

    private void InputText(string str)
    {
        console.text = str + "\n" + console.text;
        console.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(width, hight += 40);
    }
}
