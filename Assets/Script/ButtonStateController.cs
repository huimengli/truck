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
        //������Panelʱ��ǿ�ư�ť����Selected״̬
        if (linkedPanel.activeSelf)
        {
            targetBotton.Select();//����Select״̬
        }
    }
}
