using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelController : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject targetPanel;//��Ҫ��Inspector�а�Panel����

    void Start()
    {
        targetPanel.SetActive(false);//��ʼ��״̬������ָ��Panel
    }

    public void TogglePanel()
    {
        targetPanel.SetActive(!targetPanel.activeSelf);//�л���ʾ״̬
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
