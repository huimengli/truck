using Export.Attribute;
using Export.AddFunc;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 拖拽与吸附行为的基类。
/// 提供拖拽、吸附点检测、以及吸附动作的实现。
/// </summary>
public class DragAndStickBehaviour : MonoBehaviour
{
    /// <summary>
    /// 所有可能的吸附点（全局静态列表）。
    /// 这些点将作为所有拖拽物体的潜在吸附目标。
    /// </summary>
    public static List<Transform> _points = new List<Transform>();

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
    /// 拖拽物体的高度。
    /// 用于调整物体吸附时的位置，使其与吸附点对齐。
    /// </summary>
    public float height = 1f;

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
    /// 每帧更新拖拽与吸附逻辑。
    /// </summary>
    protected void Upgrade()
    {
        if (isDragging)
        {
            DragObject(); // 如果正在拖拽，执行拖拽逻辑
        }
        else if (isSticking && !isSticked)
        {
            StickToObject(); // 如果未完成吸附，执行吸附逻辑
        }
    }

    /// <summary>
    /// 执行拖拽逻辑。
    /// 根据鼠标位置更新物体位置，并判断是否可以吸附。
    /// </summary>
    void DragObject()
    {
        // 计算鼠标在世界空间中的位置
        float distanceToCamera = Vector3.Distance(transform.position, Camera.main.transform.position);
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, distanceToCamera));
        transform.position = new Vector3(mousePos.x, mousePos.y, transform.position.z); // 保持Z轴不变
        //transform.position = mousePos;

        // 检查是否满足吸附条件
        bool canStick = CheckIfCanStick();

        if (canStick)
        {
            // 如果可以吸附，显示虚影并更新虚影位置
            if (shadow == null)
            {
                shadow = Instantiate(shadowPrefab, transform.position,transform.rotation); // 创建虚影
                if (shadow.GetComponent<ChangeOpacity>()==null)
                {
                    shadow.AddComponent<ChangeOpacity>();
                }
            }
            shadow.SetActive(true);
            var centerPoint = CalculateCenter(closestPointMap.ValuesToArray()); // 更新虚影位置到吸附点中心
            shadow.transform.position = new Vector3(centerPoint.x, centerPoint.y + height / 2, centerPoint.z);
            
            ChangeOpacity opacity = shadow.GetComponent<ChangeOpacity>();
            opacity.SetOpacity(shadowOpacity);
            Debug.Log(opacity.opacity);
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
    private bool CheckIfCanStick()
    {
        closestPointMap.Clear(); // 清空当前记录的最近吸附点
        bool canStick = true;

        foreach (var receptor in receptorPoints)
        {
            Transform closest = null;
            float minDistance = stickDistance;

            foreach (var point in _points)
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
    void StickToObject()
    {
        Vector3 targetPosition = CalculateCenter(closestPointMap.ValuesToArray()); // 计算吸附点中心
        targetPosition = new Vector3(targetPosition.x, targetPosition.y + height/2, targetPosition.z);
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
        }
    }

    /// <summary>
    /// 计算一组Transform的中心点。
    /// </summary>
    /// <param name="points">Transform数组。</param>
    /// <returns>中心点位置。</returns>
    private Vector3 CalculateCenter(Transform[] points)
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
    void OnMouseDown()
    {
        isDragging = true; // 启用拖拽
        isSticked = false; // 重置吸附状态
    }

    /// <summary>
    /// 当鼠标松开时，停止拖拽并开始吸附。
    /// </summary>
    void OnMouseUp()
    {
        isDragging = false; // 停止拖拽
        if (shadow != null && shadow.activeSelf)
        {
            isSticking = true; // 如果虚影可见，开始吸附
        }
    }
}
