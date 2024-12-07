using UnityEngine;

public class SnapToContact : MonoBehaviour
{
    /// <summary>
    /// 预制件上的ReceptorPoints，表示需要与接触点（ContactPoints）对齐的点。
    /// </summary>
    private Transform[] receptorPoints;

    /// <summary>
    /// 负载平台上的ContactPoints，表示目标吸附点。
    /// </summary>
    public Transform[] contactPoints;

    /// <summary>
    /// 吸附的最大距离。
    /// </summary>
    public float snapDistance = 0.5f;

    /// <summary>
    /// 吸附时的平滑速度。
    /// </summary>
    public float snapSpeed = 5f;

    /// <summary>
    /// 是否正在拖动物体。
    /// </summary>
    private bool isDragging = false;

    /// <summary>
    /// 鼠标与物体的初始偏移量。
    /// </summary>
    private Vector3 dragOffset;

    void Start()
    {
        // 初始化ReceptorPoints
        receptorPoints = GetComponentsInChildren<Transform>();
    }

    void Update()
    {
        if (isDragging)
        {
            DragObject();
        }
    }

    /// <summary>
    /// 拖动逻辑。
    /// </summary>
    void DragObject()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = transform.position.z; // 锁定Z轴
        transform.position = mousePos + dragOffset;
    }

    /// <summary>
    /// 检查吸附条件并强制吸附。
    /// </summary>
    void SnapToClosestPoints()
    {
        // 验证是否所有ReceptorPoints都可以吸附
        bool allCanSnap = true;
        for (int i = 0; i < receptorPoints.Length; i++)
        {
            // 确保索引匹配点对点吸附
            if (i >= contactPoints.Length)
            {
                allCanSnap = false;
                break;
            }

            float distance = Vector3.Distance(receptorPoints[i].position, contactPoints[i].position);
            if (distance > snapDistance)
            {
                allCanSnap = false;
                break;
            }
        }

        // 如果所有点都满足吸附条件，整体对齐到ContactPoints的中心
        if (allCanSnap)
        {
            Vector3 receptorCenter = CalculateCenter(receptorPoints);
            Vector3 contactCenter = CalculateCenter(contactPoints);

            // 使用插值平滑对齐中心点
            transform.position = Vector3.Lerp(transform.position, transform.position + (contactCenter - receptorCenter), Time.deltaTime * snapSpeed);
        }
    }

    /// <summary>
    /// 计算Transform数组的中心点。
    /// </summary>
    /// <param name="transforms">Transform数组。</param>
    /// <returns>中心点位置。</returns>
    private Vector3 CalculateCenter(Transform[] transforms)
    {
        Vector3 center = Vector3.zero;
        foreach (var t in transforms)
        {
            center += t.position;
        }
        return center / transforms.Length;
    }

    void OnMouseDown()
    {
        isDragging = true;

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = transform.position.z;
        dragOffset = transform.position - mousePos;
    }

    void OnMouseUp()
    {
        isDragging = false;

        // 松开鼠标时强制执行吸附逻辑
        SnapToClosestPoints();
    }
}
