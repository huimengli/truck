using Export.Attribute;
using Export.AddFunc;
using System.Collections.Generic;
using UnityEngine;
using Export.BehaviourEX;
using System.Linq;

/// <summary>
/// 拖拽与吸附行为的基类。
/// 提供拖拽、吸附点检测、以及吸附动作的实现。
/// </summary>
public class DragAndStickBehaviour : UUIDBehavior
{
    /// <summary>
    /// 点类型
    /// </summary>
    public SupportPointType type;

    /// <summary>
    /// 点数据
    /// </summary>
    public Dictionary<string, List<Transform>> _points;

    /// <summary>
    /// 用过的点数据
    /// </summary>
    public Dictionary<string, List<Transform>> _used;

    /// <summary>
    /// 可用的点数据
    /// </summary>
    public List<Transform> Points
    {
        get
        {
            return PointPool.getPoints(type);
        }
    }

    /// <summary>
    /// 用于显示虚影的预制体。
    /// 在拖拽过程中，虚影显示潜在吸附位置。
    /// </summary>
    public GameObject shadowPrefab;

    /// <summary>
    /// 虚影对象显示透明度
    /// </summary>
    [Range(0,1)]
    public float shadowOpacity = 0.4f;

    /// <summary>
    /// 当前对象上的接受点数组。
    /// 用于检测与吸附点的距离。
    /// </summary>
    [ReadOnly]
    public Transform[] receptorPoints;

    /// <summary>
    /// 最大吸附距离。
    /// 如果吸附点与接受点之间的距离小于该值，则可以吸附。
    /// </summary>
    public float stickDistance = 0.5f;

    /// <summary>
    /// 吸附的移动速度。
    /// 控制物体从当前位置到吸附点的移动速度。
    /// </summary>
    public float moveSpeed = 5f;

    /// <summary>
    /// 当前物体是否正在被拖拽。
    /// </summary>
    [ReadOnly]
    public bool isDragging = false;

    /// <summary>
    /// 当前物体是否正在吸附。
    /// </summary>
    [ReadOnly]
    public bool isSticking = false;

    /// <summary>
    /// 当前物体是否已经完成吸附。
    /// </summary>
    [ReadOnly]
    public bool isSticked = false;

    /// <summary>
    /// 用于显示虚影的实例。
    /// </summary>
    protected GameObject shadow;

    /// <summary>
    /// 用于记录每个接受点最近的吸附点。
    /// 确保吸附点与接受点一一对应。
    /// </summary>
    protected Dictionary<Transform, Transform> closestPointMap = new Dictionary<Transform, Transform>();

    /// <summary>
    /// 内部初始化
    /// </summary>
    protected void Init()
    {
        _points = PointPool._points[type];
        _used = PointPool._used[type];
    }

    /// <summary>
    /// 每帧更新拖拽与吸附逻辑。
    /// </summary>
    protected void Upgrade()
    {
        // 添加右键点击触发物品旋转功能
        if (isDragging && Input.GetMouseButtonUp(1))
        {
            transform.rotation *= Quaternion.Euler(0, 90, 0); // 在当前旋转基础上增加90度
        }

        if (isDragging)
        {
            DragObject(); // 如果正在拖拽，执行拖拽逻辑
        }
        else if (isSticking && !isSticked)
        {
            StickToObject(); // 如果未完成吸附，执行吸附逻辑
        }

        // 吸附完成
        if (isSticked)
        {
            _used.AddOrSet(UUID, closestPointMap.ValuesToList());
        }
        else
        {
            _used.Remove(UUID);
        }
    }

    /// <summary>
    /// 执行拖拽逻辑。
    /// 根据鼠标位置更新物体位置，并判断是否可以吸附。
    /// 这个方法能用,但是效果不好
    /// </summary>
    protected virtual void DragObject()
    {
        // 计算鼠标在世界空间中的位置
        float distanceToCamera = Vector3.Distance(transform.position, Camera.main.transform.position);
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, distanceToCamera));
        transform.position = new Vector3(mousePos.x, transform.position.y, mousePos.y); // 保持Y轴不变

        // 检测鼠标滚轮输入，并修改 Y 轴位置
        float scroll = Input.GetAxis("Mouse ScrollWheel"); // 获取滚轮滚动值
        if (scroll != 0)
        {
            transform.position += new Vector3(0, scroll * 0.5f, 0); // 修改 Y 轴，0.5f 为滚动的速度系数，可自行调整
        }

        // 检查是否满足吸附条件
        bool canStick = CheckIfCanStick();

        if (canStick)
        {
            // 如果可以吸附，显示虚影并更新虚影位置
            if (shadow == null)
            {
                shadow = Instantiate(shadowPrefab, transform.position,transform.rotation); // 创建虚影
                // 如果阴影对象没有透明度修改模块则添加
                if (shadow.GetComponent<ChangeOpacity>()==null)
                {
                    shadow.AddComponent<ChangeOpacity>();
                }
                // 如果阴影对象有拖动模块则删除
                DragAndStick drag = shadow.GetComponent<DragAndStick>();
                if (drag!=null)
                {
                    Destroy(drag);
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
            shadow.transform.position = targetPosition;

            ChangeOpacity opacity = shadow.GetComponent<ChangeOpacity>();
            opacity.SetOpacity(shadowOpacity);
        }
        else if (shadow != null)
        {
            shadow.SetActive(false); // 如果不能吸附，隐藏虚影
        }
    }

    /// <summary>
    /// 检查是否满足吸附条件。
    /// 遍历所有接受点，找到与其距离最近的吸附点，并记录。
    /// </summary>
    /// <returns>是否满足吸附条件。</returns>
    protected bool CheckIfCanStick()
    {
        closestPointMap.Clear(); // 清空当前记录的最近吸附点
        bool canStick = true;

        foreach (var receptor in receptorPoints)
        {
            Transform closest = null;
            float minDistance = stickDistance;

            foreach (var point in Points)
            {
                float distance = Vector3.Distance(receptor.position, point.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closest = point; // 更新最近吸附点
                }
            }

            if (closest == null)
            {
                canStick = false; // 如果某个接受点没有满足条件的吸附点，则不能吸附
                break;
            }
            closestPointMap[receptor] = closest; // 记录接受点与最近吸附点的对应关系
        }

        return canStick;
    }

    /// <summary>
    /// 执行吸附逻辑。
    /// 将物体移动到吸附点中心位置，直到吸附完成。
    /// </summary>
    protected virtual void StickToObject()
    {
        Vector3 currentPosition = CalculateCenter(receptorPoints.ToArray()); // 计算当前吸附的中心
        Vector3 targetPosition = CalculateCenter(closestPointMap.ValuesToArray()); // 计算吸附点中心
        //targetPosition = new Vector3(targetPosition.x, targetPosition.y + height/2, targetPosition.z);
        targetPosition = new Vector3(
            targetPosition.x - currentPosition.x + transform.position.x,
            targetPosition.y - currentPosition.y + transform.position.y,
            targetPosition.z - currentPosition.z + transform.position.z
        );
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime); // 平滑移动到目标位置

        if (Vector3.Distance(transform.position, targetPosition) <= 0.01f)
        {
            isSticking = false;
            isSticked = true;

            if (shadow != null)
            {
                Destroy(shadow); // 吸附完成后销毁虚影
                shadow = null;
            }

            // 移动物体到吸附点位置
            transform.position = targetPosition; // 确保物体位置与吸附点一致
        }
    }

    /// <summary>
    /// 计算一组Transform的中心点。
    /// </summary>
    /// <param name="points">Transform数组。</param>
    /// <returns>中心点位置。</returns>
    protected Vector3 CalculateCenter(Transform[] points)
    {
        Vector3 center = Vector3.zero;
        foreach (var point in points)
        {
            center += point.position;
        }
        return center / points.Length; // 返回平均位置作为中心点
    }

    /// <summary>
    /// 当鼠标按下时，开始拖拽。
    /// </summary>
    public void OnMouseDown()
    {
        isDragging = true; // 启用拖拽
        isSticked = false; // 重置吸附状态
    }

    /// <summary>
    /// 当鼠标松开时，停止拖拽并开始吸附。
    /// </summary>
    public void OnMouseUp()
    {
        isDragging = false; // 停止拖拽
        if (shadow != null)
        {
            isSticking = shadow.activeSelf; // 如果虚影可见，开始吸附
        }
    }

    protected void OnDestroy()
    {
        if (shadow != null)
        {
            Destroy(shadow); // 销毁虚影
            shadow = null;
        }
        // 清除吸附点数据
        if (_used.ContainsKey(UUID))
        {
            _used.Remove(UUID);
        }
    }
}
