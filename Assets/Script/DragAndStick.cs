using Export.Attribute;
using Export.Tools;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.EventSystems;
using Export.AddFunc;
using UnityEngine.UIElements;
using System.Collections.Generic;
using Export.BehaviourEX;

/// <summary>
/// 拖拽功能实现
/// </summary>
public class DragAndStick : DragAndStickBehaviour
{
    /// <summary>
    /// UI生成的物体,全局唯一
    /// </summary>
    public static GameObject GenerateObject = null;

    /// <summary>
    /// 游戏调用代码
    /// </summary>
    [ReadOnly]
    public GameConfig config;

    /// <summary>
    /// 用于筛选接受点的正则表达式。
    /// </summary>
    [SerializeField]
    [ReadOnly]
    public string receptorPointName;

    /// <summary>
    /// 用于读取接受点名称的正则表达式对象。
    /// </summary>
    private Regex readName;

    /// <summary>
    /// 吸附失败后是否还显示
    /// </summary>
    public bool stickFailDisplay;

    /// <summary>
    /// 需要忽略的的碰撞体
    /// </summary>
    private List<Collider> ignoreColliders = new List<Collider>();

    /// <summary>
    /// 当前物体的高度
    /// </summary>
    [ReadOnly]
    [SerializeField]
    public float height = 0f;

    /// <summary>
    /// 拖动时的透明度
    /// </summary>
    [SerializeField]
    [Range(0f, 1f)]
    public float DragOpacity = 0.8f;

    /// <summary>
    /// 当前物体的高度
    /// </summary>
    public float Height
    {
        get
        {
            return height;
        }
    }

    /// <summary>
    /// 初始化
    /// </summary>
    private void Awake()
    {
        config = FindObjectOfType<GameConfig>();
        receptorPointName = config.ReceptorPointName + type.GetValue();

        // 计算当前物体的高度
        Mesh mesh = GetComponent<MeshFilter>().sharedMesh;
        float originalHeight = mesh.bounds.size.y;
        float scaledHeight = originalHeight * transform.localScale.y;
        height = scaledHeight;

        // 调用基类初始化逻辑
        Init();

        // 添加碰撞体
        ignoreColliders.AddRange(GetComponents<Collider>());
        ignoreColliders.AddRange(GetComponentsInChildren<Collider>());
    }

    /// <summary>
    /// 初始化方法，筛选符合条件的接受点。
    /// </summary>
    void Start()
    {
        readName = new Regex(receptorPointName); // 初始化正则表达式

        // 筛选子对象中名称符合正则表达式的接受点
        receptorPoints = transform.GetComponentsInChildren<Transform>()
            .Where(e => e != null && readName.IsMatch(e.gameObject.name))
            .ToArray();
    }

    /// <summary>
    /// 每帧更新。
    /// </summary>
    void Update()
    {
        Upgrade(); // 调用基类的更新逻辑

        // 更新完成后,如果吸附成功则修改颜色
        if (isSticked)
        {
            // 如果物体没有 ChangeRGBA 组件，则添加
            if (GetComponent<ChangeRGBA>() == null)
            {
                gameObject.AddComponent<ChangeRGBA>();
            }
            // 物品显示为正常颜色
            ChangeRGBA change = GetComponent<ChangeRGBA>();
            change.SetColor(1,1,1,1);
        }
    }

    /// <summary>
    /// 修改拖着物品逻辑
    /// 根据鼠标未知更新物体未知,并且显示是否可以吸附
    /// </summary>
    protected override void DragObject()
    {
        // 创建屏幕射线
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        Vector3 intersectionPoint = transform.position;
        // 射线检测
        RaycastHit[] hits = Physics.RaycastAll(ray, Mathf.Infinity);
        if (hits.Length > 1)
        {
            foreach (var hit in hits.OrderBy(h => h.distance))
            {
                if (!ignoreColliders.Contains(hit.collider))
                {
                    intersectionPoint = hit.point; // 获取射线与物体的交点
                    break;
                }
            }
        }

        // 更新物体位置
        transform.position = new Vector3(
            intersectionPoint.x,
            intersectionPoint.y + Height / 2,
            intersectionPoint.z
        );

        // 检查是否满足吸附条件
        bool canStick = CheckIfCanStick();

        if (canStick)
        {
            // 如果可以吸附，显示虚影并更新虚影位置
            if (shadow == null)
            {
                shadow = Instantiate(shadowPrefab, transform.position, transform.rotation); // 创建虚影
                // 如果阴影对象没有透明度修改模块则添加
                if (shadow.GetComponent<ChangeRGBA>() == null)
                {
                    shadow.AddComponent<ChangeRGBA>();
                }
                // 如果阴影有碰撞体则删除
                if (shadow.GetComponents<Collider>().Length > 0)
                {
                    foreach (var collider in new List<Collider[]>()
                    {
                        shadow.GetComponents<Collider>(),
                        shadow.GetComponentsInChildren<Collider>()
                    }.Map(colliders =>
                    {
                        List<Collider> list = new List<Collider>();
                        list.AddRange(colliders);
                        return list;
                    }).Join())
                    {
                        Destroy(collider);
                    }
                }
                // 如果阴影对象有拖动模块则删除
                DragAndStick drag = shadow.GetComponent<DragAndStick>();
                if (drag != null)
                {
                    Destroy(drag);
                }
                // 如果阴影对象有添加点模块则删除
                AddDASPoint addDASPoint = shadow.GetComponent<AddDASPoint>();
                if (addDASPoint != null)
                {
                    Destroy(addDASPoint);
                }
            }
            else
            {
                shadow.transform.rotation = transform.rotation; // 修改旋转角度
            }
            shadow.SetActive(true);

            Vector3 currentPosition = CalculateCenter(receptorPoints.ToArray()); // 计算当前吸附的中心
            Vector3 targetPosition = CalculateCenter(closestPointMap.ValuesToArray()); // 计算吸附点中心
            targetPosition = new Vector3(
                targetPosition.x - currentPosition.x + transform.position.x,
                targetPosition.y - currentPosition.y + transform.position.y,
                targetPosition.z - currentPosition.z + transform.position.z
            );
            shadow.transform.position = targetPosition; // 更新虚影位置

            // 修改虚影的透明度
            ChangeRGBA shadowRGBA = shadow.GetComponent<ChangeRGBA>();
            shadowRGBA.SetColor(1, 1, 1, shadowOpacity);

            // 如果物体没有 ChangeRGBA 组件，则添加
            if (GetComponent<ChangeRGBA>() == null)
            {
                gameObject.AddComponent<ChangeRGBA>();
            }
            // 物品显示为绿色
            ChangeRGBA change = GetComponent<ChangeRGBA>();
            change.SetColor(0.5f, 1f, 0.5f, DragOpacity); // 绿色
        }
        else
        {
            // 如果不满足吸附条件，隐藏虚影
            if (shadow != null)
            {
                shadow.SetActive(false);
            }
            // 如果物体没有 ChangeRGBA 组件，则添加
            if (GetComponent<ChangeRGBA>() == null)
            {
                gameObject.AddComponent<ChangeRGBA>();
            }
            // 物品显示为红色
            ChangeRGBA change = GetComponent<ChangeRGBA>();
            change.SetColor(1f, 0.5f, 0.5f, DragOpacity); // 红色
        }
    }

    public new void OnMouseDown()
    {
        base.OnMouseDown();
        Debug.Log($"{UUID} OnMouseDown");
    }

    /// <summary>
    /// 重写鼠标弹起事件
    /// </summary>
    public new void OnMouseUp()
    {
        base.OnMouseUp();
        Debug.Log($"{UUID} OnMouseUp");

        // 删除唯一组件
        if (GenerateObject != null)
        {
            if (isSticking==false)
            {
                DestroyImmediate(GenerateObject);
            }
            GenerateObject = null;
        }else
        {
            if (isSticking==false)
            {
                // 如果吸附失败则删除当前物体
                Destroy(gameObject);
            }
        }
    }
}
