using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelController : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject targetPanel;//需要在Inspector中绑定Panel对象

    void Start()
    {
        targetPanel.SetActive(false);//初始化状态下隐藏指定Panel
    }

    public void TogglePanel()
    {
        targetPanel.SetActive(!targetPanel.activeSelf);//切换显示状态
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
