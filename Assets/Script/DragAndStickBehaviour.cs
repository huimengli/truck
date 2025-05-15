using Export.Attribute;
using Export.AddFunc;
using System.Collections.Generic;
using UnityEngine;
using Export.BehaviourEX;
using System.Linq;

/// <summary>
/// ��ק��������Ϊ�Ļ��ࡣ
/// �ṩ��ק���������⡢�Լ�����������ʵ�֡�
/// </summary>
public class DragAndStickBehaviour : UUIDBehavior
{
    /// <summary>
    /// ������
    /// </summary>
    public SupportPointType type;

    /// <summary>
    /// ������
    /// </summary>
    public Dictionary<string, List<Transform>> _points;

    /// <summary>
    /// �ù��ĵ�����
    /// </summary>
    public Dictionary<string, List<Transform>> _used;

    /// <summary>
    /// ���õĵ�����
    /// </summary>
    public List<Transform> Points
    {
        get
        {
            return PointPool.getPoints(type);
        }
    }

    /// <summary>
    /// ������ʾ��Ӱ��Ԥ���塣
    /// ����ק�����У���Ӱ��ʾǱ������λ�á�
    /// </summary>
    public GameObject shadowPrefab;

    /// <summary>
    /// ��Ӱ������ʾ͸����
    /// </summary>
    [Range(0,1)]
    public float shadowOpacity = 0.4f;

    /// <summary>
    /// ��ǰ�����ϵĽ��ܵ����顣
    /// ���ڼ����������ľ��롣
    /// </summary>
    [ReadOnly]
    public Transform[] receptorPoints;

    /// <summary>
    /// ����������롣
    /// �������������ܵ�֮��ľ���С�ڸ�ֵ�������������
    /// </summary>
    public float stickDistance = 0.5f;

    /// <summary>
    /// �������ƶ��ٶȡ�
    /// ��������ӵ�ǰλ�õ���������ƶ��ٶȡ�
    /// </summary>
    public float moveSpeed = 5f;

    /// <summary>
    /// ��ǰ�����Ƿ����ڱ���ק��
    /// </summary>
    [ReadOnly]
    public bool isDragging = false;

    /// <summary>
    /// ��ǰ�����Ƿ�����������
    /// </summary>
    [ReadOnly]
    public bool isSticking = false;

    /// <summary>
    /// ��ǰ�����Ƿ��Ѿ����������
    /// </summary>
    [ReadOnly]
    public bool isSticked = false;

    /// <summary>
    /// ������ʾ��Ӱ��ʵ����
    /// </summary>
    protected GameObject shadow;

    /// <summary>
    /// ���ڼ�¼ÿ�����ܵ�����������㡣
    /// ȷ������������ܵ�һһ��Ӧ��
    /// </summary>
    protected Dictionary<Transform, Transform> closestPointMap = new Dictionary<Transform, Transform>();

    /// <summary>
    /// �ڲ���ʼ��
    /// </summary>
    protected void Init()
    {
        _points = PointPool._points[type];
        _used = PointPool._used[type];
    }

    /// <summary>
    /// ÿ֡������ק�������߼���
    /// </summary>
    protected void Upgrade()
    {
        if (isDragging)
        {
            DragObject(); // ���������ק��ִ����ק�߼�
        }
        else if (isSticking && !isSticked)
        {
            StickToObject(); // ���δ���������ִ�������߼�
        }

        // �������
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
    /// ִ����ק�߼���
    /// �������λ�ø�������λ�ã����ж��Ƿ����������
    /// �����������,����Ч������
    /// </summary>
    protected virtual void DragObject()
    {
        // �������������ռ��е�λ��
        float distanceToCamera = Vector3.Distance(transform.position, Camera.main.transform.position);
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, distanceToCamera));
        transform.position = new Vector3(mousePos.x, transform.position.y, mousePos.y); // ����Y�᲻��

        // ������������룬���޸� Y ��λ��
        float scroll = Input.GetAxis("Mouse ScrollWheel"); // ��ȡ���ֹ���ֵ
        if (scroll != 0)
        {
            transform.position += new Vector3(0, scroll * 0.5f, 0); // �޸� Y �ᣬ0.5f Ϊ�������ٶ�ϵ���������е���
        }

        // ����Ƿ�������������
        bool canStick = CheckIfCanStick();

        if (canStick)
        {
            // ���������������ʾ��Ӱ��������Ӱλ��
            if (shadow == null)
            {
                shadow = Instantiate(shadowPrefab, transform.position,transform.rotation); // ������Ӱ
                // �����Ӱ����û��͸�����޸�ģ�������
                if (shadow.GetComponent<ChangeOpacity>()==null)
                {
                    shadow.AddComponent<ChangeOpacity>();
                }
                // �����Ӱ�������϶�ģ����ɾ��
                DragAndStick drag = shadow.GetComponent<DragAndStick>();
                if (drag!=null)
                {
                    Destroy(drag);
                }
            }
            else
            {
                shadow.transform.rotation = transform.rotation; // �޸���ת�Ƕ�
            }
            shadow.SetActive(true);

            Vector3 currentPosition = CalculateCenter(receptorPoints.ToArray()); // ���㵱ǰ����������
            Vector3 targetPosition = CalculateCenter(closestPointMap.ValuesToArray()); // ��������������
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
            shadow.SetActive(false); // �������������������Ӱ
        }
    }

    /// <summary>
    /// ����Ƿ���������������
    /// �������н��ܵ㣬�ҵ������������������㣬����¼��
    /// </summary>
    /// <returns>�Ƿ���������������</returns>
    protected bool CheckIfCanStick()
    {
        closestPointMap.Clear(); // ��յ�ǰ��¼�����������
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
                    closest = point; // �������������
                }
            }

            if (closest == null)
            {
                canStick = false; // ���ĳ�����ܵ�û�����������������㣬��������
                break;
            }
            closestPointMap[receptor] = closest; // ��¼���ܵ������������Ķ�Ӧ��ϵ
        }

        return canStick;
    }

    /// <summary>
    /// ִ�������߼���
    /// �������ƶ�������������λ�ã�ֱ��������ɡ�
    /// </summary>
    protected virtual void StickToObject()
    {
        Vector3 currentPosition = CalculateCenter(receptorPoints.ToArray()); // ���㵱ǰ����������
        Vector3 targetPosition = CalculateCenter(closestPointMap.ValuesToArray()); // ��������������
        //targetPosition = new Vector3(targetPosition.x, targetPosition.y + height/2, targetPosition.z);
        targetPosition = new Vector3(
            targetPosition.x - currentPosition.x + transform.position.x,
            targetPosition.y - currentPosition.y + transform.position.y,
            targetPosition.z - currentPosition.z + transform.position.z
        );
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime); // ƽ���ƶ���Ŀ��λ��

        if (Vector3.Distance(transform.position, targetPosition) <= 0.01f)
        {
            isSticking = false;
            isSticked = true;

            if (shadow != null)
            {
                Destroy(shadow); // ������ɺ�������Ӱ
                shadow = null;
            }

            // �ƶ����嵽������λ��
            transform.position = targetPosition; // ȷ������λ����������һ��
        }
    }

    /// <summary>
    /// ����һ��Transform�����ĵ㡣
    /// </summary>
    /// <param name="points">Transform���顣</param>
    /// <returns>���ĵ�λ�á�</returns>
    protected Vector3 CalculateCenter(Transform[] points)
    {
        Vector3 center = Vector3.zero;
        foreach (var point in points)
        {
            center += point.position;
        }
        return center / points.Length; // ����ƽ��λ����Ϊ���ĵ�
    }

    /// <summary>
    /// ����갴��ʱ����ʼ��ק��
    /// </summary>
    public void OnMouseDown()
    {
        isDragging = true; // ������ק
        isSticked = false; // ��������״̬
    }

    /// <summary>
    /// ������ɿ�ʱ��ֹͣ��ק����ʼ������
    /// </summary>
    public void OnMouseUp()
    {
        isDragging = false; // ֹͣ��ק
        if (shadow != null && shadow.activeSelf)
        {
            isSticking = true; // �����Ӱ�ɼ�����ʼ����
        }
    }
}
