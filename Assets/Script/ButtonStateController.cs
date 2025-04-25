using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class ButtonStateController : MonoBehaviour
{
    // Start is called before the first frame update
    public Button targetBotton;
    public GameObject linkedPanel;


    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //当激活Panel时，强制按钮进入Selected状态
        if (linkedPanel.activeSelf)
        {
            targetBotton.Select();//触发Select状态
        }
    }
}
