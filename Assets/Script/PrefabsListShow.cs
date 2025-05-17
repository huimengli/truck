using System;
using System.Collections;
using System.Collections.Generic;
using Export.AddFunc;
using Export.Attribute;
using Export.BehaviourEX;
using LT_Code.UnityExtra;
using UnityEngine;
using UnityEngine.UI;

public class PrefabsListShow : MonoBehaviour
{
    /// <summary>
    /// 预制体列表
    /// </summary>
    public GameObject[] Prefabs;

    /// <summary>
    /// UI显示对象
    /// </summary>
    public GameObject UIObject;

    /// <summary>
    /// 多个UI对象的间隔
    /// </summary>
    public Vector2 changePosition;

    /// <summary>
    /// 预制件生成的UI精灵
    /// </summary>
    [ReadOnly]
    public Sprite[] PrefabSprites;

    /// <summary>
    /// UI显示生成对象列表
    /// </summary>
    [ReadOnly]
    public List<GameObject> UIObjectList = new List<GameObject>();

    /// <summary>
    /// 生成的对象列表
    /// </summary>
    [ReadOnly]
    public List<GameObject> GenerateObjects = new List<GameObject>();

    /// <summary>
    /// 预制件字典
    /// </summary>
    private Dictionary<string, GameObject> PrefabDict = new Dictionary<string, GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        // 检测UI对象大小
        RectTransform rectTransform = UIObject.GetComponent<RectTransform>();
        Rect rect = rectTransform.rect;
        Debug.Log($"weidth: {rect.width}, height: {rect.height}");
        Debug.Log($"x: {rectTransform.anchoredPosition.x}, y: {rectTransform.anchoredPosition.y}");

        // 初始化预制件列表
        PrefabSprites = new Sprite[Prefabs.Length];
        for (int i = 0; i < Prefabs.Length; i++)
        {
            // 生成精灵
            PrefabSprites[i] = ConvertPrefabToSprite(Prefabs[i], new Vector2(rect.width, rect.height));
            // 生成UI对象
            GameObject obj = Instantiate(UIObject, transform);
            // 如果UI对象没有图片组件，则添加一个
            if (obj.GetComponent<SpriteRenderer>() == null)
            {
                obj.AddComponent<SpriteRenderer>();
            }
            obj.GetComponent<SpriteRenderer>().sprite = PrefabSprites[i];
            UIObjectList.Add(obj);
            // 设置UI对象的位置
            RectTransform objRect = obj.GetComponent<RectTransform>();
            objRect.anchoredPosition = new Vector2(
                rectTransform.anchoredPosition.x + i * changePosition.x,
                rectTransform.anchoredPosition.y + i * changePosition.y
            );
            // 如果没有点击组件,则添加一个
            if (obj.GetComponent<ButtonEX>() == null)
            {
                obj.AddComponent<ButtonEX>();
            }
            // 添加UUID组件
            if (obj.GetComponent<UUIDBehavior>() == null)
            {
                obj.AddComponent<UUIDBehavior>();
                UUIDBehavior uid = obj.GetComponent<UUIDBehavior>();
                // 添加预制件字典
                PrefabDict[uid.UUID] = Prefabs[i];
            }
            // 添加点击事件
            obj.GetComponent<ButtonEX>().mouseDown.AddListener(() =>
            {
                GameObject prefab = PrefabDict[obj.GetComponent<UUIDBehavior>().UUID];
                GameObject newObj = AddGameObject(prefab);
                // 如果唯一物体不为空,则删除
                Debug.Log(DragAndStick.GenerateObject);
                if (DragAndStick.GenerateObject != null)
                {
                    DragAndStick.GenerateObject.SetActive(false);
                    DestroyImmediate(DragAndStick.GenerateObject);
                }
                DragAndStick.GenerateObject = newObj;
                GenerateObjects.Add(newObj);
                // 使用拖拽组件
                DragAndStick dragAndStick = newObj.GetComponent<DragAndStick>();
                if (dragAndStick != null)
                {
                    dragAndStick.OnMouseDown();
                }
            });
            //// 添加松开鼠标事件
            //obj.GetComponent<ButtonEX>().mouseUp.AddListener(() =>
            //{
            //    string UUID = obj.GetComponent<UUIDBehavior>().UUID;
            //    GameObject prefab = PrefabDict[UUID];
            //    int index = PrefabDict.KeysToList().IndexOf(UUID);
            //    // 使用拖拽组件
            //    DragAndStick dragAndStick = PrefabDict[UUID].GetComponent<DragAndStick>();
            //    if (dragAndStick != null)
            //    {
            //        dragAndStick.OnMouseUp();
            //    }
            //});
        }

        // 初始UI对象设置不显示
        UIObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// 添加预制体
    /// </summary>
    /// <param name="prefab"></param>
    /// <returns></returns>
    public GameObject AddGameObject(GameObject prefab)
    {
        // 创建屏幕射线
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        Vector3 intersectionPoint = transform.position;
        // 射线检测
        if (Physics.Raycast(ray, out hit))
        {
            intersectionPoint = hit.point;
        }
        // 生成预制体
        GameObject obj = Instantiate(prefab, intersectionPoint, Quaternion.identity);

        // 删除预制件下面的碰撞体
        Collider collider = obj.GetComponent<Collider>();
        Collider[] childColliders = obj.GetComponentsInChildren<Collider>();
        foreach (var c in childColliders)
        {
            if (collider != c)
            {
                Destroy(c);
            }
        }

        // 判断物体是否含有 DragAndStick 组件
        if (obj.GetComponent<DragAndStick>() == null)
        {
            // 添加 DragAndStick 组件
            obj.AddComponent<DragAndStick>();
        }

        // 返回
        return obj;
    }

    public static Sprite ConvertPrefabToSprite(GameObject prefab, Vector2 imageSize)
    {
        // 创建一个临时相机和渲染纹理
        var tempCamera = new GameObject("TempCamera").AddComponent<Camera>();
        tempCamera.cullingMask = 0;// 1 << LayerMask.NameToLayer("UI");
        tempCamera.clearFlags = CameraClearFlags.SolidColor;
        tempCamera.backgroundColor = Color.clear;

        RenderTexture rt = new RenderTexture(
            (int)imageSize.x,
            (int)imageSize.y,
            24,
            RenderTextureFormat.ARGB32
        );

        // 设置渲染环境
        tempCamera.targetTexture = rt;
        GameObject instance = Instantiate(prefab);
        instance.SetActive(true);

        // 执行渲染
        tempCamera.Render();
        RenderTexture.active = rt;

        // 转换纹理数据
        Texture2D texture = new Texture2D(
            rt.width,
            rt.height,
            TextureFormat.ARGB32,
            false
        );
        texture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        texture.Apply();

        // 创建精灵
        Sprite sprite = Sprite.Create(
            texture,
            new Rect(0, 0, rt.width, rt.height),
            Vector2.zero
        );

        // 清理资源
        RenderTexture.active = null;
        tempCamera.targetTexture = null;
        DestroyImmediate(rt);
        DestroyImmediate(tempCamera.gameObject);
        DestroyImmediate(instance);

        // 返回精灵
        return sprite;
    }
}