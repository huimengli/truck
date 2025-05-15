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
    /// Ԥ�����б�
    /// </summary>
    public GameObject[] Prefabs;

    /// <summary>
    /// UI��ʾ����
    /// </summary>
    public GameObject UIObject;

    /// <summary>
    /// ���UI����ļ��
    /// </summary>
    public Vector2 changePosition;

    /// <summary>
    /// Ԥ�Ƽ����ɵ�UI����
    /// </summary>
    [ReadOnly]
    public Sprite[] PrefabSprites;

    /// <summary>
    /// UI��ʾ���ɶ����б�
    /// </summary>
    [ReadOnly]
    public List<GameObject> UIObjectList = new List<GameObject>();

    /// <summary>
    /// ���ɵĶ����б�
    /// </summary>
    [ReadOnly]
    public List<GameObject> GenerateObjects = new List<GameObject>();

    /// <summary>
    /// Ԥ�Ƽ��ֵ�
    /// </summary>
    private Dictionary<string, GameObject> PrefabDict = new Dictionary<string, GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        // ���UI�����С
        RectTransform rectTransform = UIObject.GetComponent<RectTransform>();
        Rect rect = rectTransform.rect;
        Debug.Log($"weidth: {rect.width}, height: {rect.height}");
        Debug.Log($"x: {rectTransform.anchoredPosition.x}, y: {rectTransform.anchoredPosition.y}");

        // ��ʼ��Ԥ�Ƽ��б�
        PrefabSprites = new Sprite[Prefabs.Length];
        for (int i = 0; i < Prefabs.Length; i++)
        {
            // ���ɾ���
            PrefabSprites[i] = ConvertPrefabToSprite(Prefabs[i], new Vector2(rect.width, rect.height));
            // ����UI����
            GameObject obj = Instantiate(UIObject, transform);
            // ���UI����û��ͼƬ����������һ��
            if (obj.GetComponent<SpriteRenderer>() == null)
            {
                obj.AddComponent<SpriteRenderer>();
            }
            obj.GetComponent<SpriteRenderer>().sprite = PrefabSprites[i];
            UIObjectList.Add(obj);
            // ����UI�����λ��
            RectTransform objRect = obj.GetComponent<RectTransform>();
            objRect.anchoredPosition = new Vector2(
                rectTransform.anchoredPosition.x + i * changePosition.x,
                rectTransform.anchoredPosition.y + i * changePosition.y
            );
            // ���û�е�����,�����һ��
            if (obj.GetComponent<ButtonEX>() == null)
            {
                obj.AddComponent<ButtonEX>();
            }
            // ���UUID���
            if (obj.GetComponent<UUIDBehavior>() == null)
            {
                obj.AddComponent<UUIDBehavior>();
                UUIDBehavior uid = obj.GetComponent<UUIDBehavior>();
                // ���Ԥ�Ƽ��ֵ�
                PrefabDict[uid.UUID] = Prefabs[i];
            }
            // ��ӵ���¼�
            obj.GetComponent<ButtonEX>().mouseDown.AddListener(() =>
            {
                GameObject prefab = PrefabDict[obj.GetComponent<UUIDBehavior>().UUID];
                GameObject newObj = AddGameObject(prefab);
                GenerateObjects.Add(newObj);
                // ʹ����ק���
                DragAndStick dragAndStick = newObj.GetComponent<DragAndStick>();
                if (dragAndStick != null)
                {
                    dragAndStick.OnMouseDown();
                }
            });
            // ����ɿ�����¼�
            obj.GetComponent<ButtonEX>().mouseUp.AddListener(() =>
            {
                string UUID = obj.GetComponent<UUIDBehavior>().UUID;
                GameObject prefab = PrefabDict[UUID];
                int index = PrefabDict.KeysToList().IndexOf(UUID);
                // ʹ����ק���
                DragAndStick dragAndStick = PrefabDict[UUID].GetComponent<DragAndStick>();
                if (dragAndStick != null)
                {
                    dragAndStick.OnMouseUp();
                }
            });
        }

        // ��ʼUI�������ò���ʾ
        UIObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// ���Ԥ����
    /// </summary>
    /// <param name="prefab"></param>
    /// <returns></returns>
    public GameObject AddGameObject(GameObject prefab)
    {
        // ������Ļ����
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        Vector3 intersectionPoint = transform.position;
        // ���߼��
        if (Physics.Raycast(ray, out hit))
        {
            intersectionPoint = hit.point;
        }
        // ����Ԥ����
        GameObject obj = Instantiate(prefab, intersectionPoint, Quaternion.identity);

        // �ж������Ƿ��� DragAndStick ���
        if (obj.GetComponent<DragAndStick>() == null)
        {
            // ��� DragAndStick ���
            obj.AddComponent<DragAndStick>();
        }

        // ����
        return obj;
    }

    public static Sprite ConvertPrefabToSprite(GameObject prefab, Vector2 imageSize)
    {
        // ����һ����ʱ�������Ⱦ����
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

        // ������Ⱦ����
        tempCamera.targetTexture = rt;
        GameObject instance = Object.Instantiate(prefab);
        instance.SetActive(true);

        // ִ����Ⱦ
        tempCamera.Render();
        RenderTexture.active = rt;

        // ת����������
        Texture2D texture = new Texture2D(
            rt.width,
            rt.height,
            TextureFormat.ARGB32,
            false
        );
        texture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        texture.Apply();

        // ��������
        Sprite sprite = Sprite.Create(
            texture,
            new Rect(0, 0, rt.width, rt.height),
            Vector2.zero
        );

        // ������Դ
        RenderTexture.active = null;
        tempCamera.targetTexture = null;
        Object.DestroyImmediate(rt);
        Object.DestroyImmediate(tempCamera.gameObject);
        Object.DestroyImmediate(instance);

        // ���ؾ���
        return sprite;
    }
}