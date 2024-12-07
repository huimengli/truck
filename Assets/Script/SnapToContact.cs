using UnityEngine;

public class SnapToContact : MonoBehaviour
{
    /// <summary>
    /// 预制件上的ReceptorPoints
    /// </summary>
    private Transform[] receptorPoints;

    /// <summary>
    /// 负载平台上的ContactPoints
    /// </summary>
    public Transform[] contactPoints;

    /// <summary>
    /// 吸附的最大距离
    /// </summary>
    public float snapDistance = 0.5f;

    private bool isDragging = false;

    void Update()
    {
        if (isDragging)
        {
            // 拖动逻辑
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = transform.position.z;
            transform.position = mousePos;

            // 检查是否所有ReceptorPoints都靠近一个ContactPoint
            bool allCanSnap = true;
            foreach (var receptor in receptorPoints)
            {
                bool canSnap = false;
                foreach (var contact in contactPoints)
                {
                    if (Vector3.Distance(receptor.position, contact.position) <= snapDistance)
                    {
                        canSnap = true;
                        break;
                    }
                }

                if (!canSnap)
                {
                    allCanSnap = false;
                    break;
                }
            }

            // 如果所有ReceptorPoints都可以吸附，执行吸附逻辑
            if (allCanSnap)
            {
                foreach (var receptor in receptorPoints)
                {
                    Transform closestContact = null;
                    float closestDistance = snapDistance;
                    foreach (var contact in contactPoints)
                    {
                        float distance = Vector3.Distance(receptor.position, contact.position);
                        if (distance <= closestDistance)
                        {
                            closestDistance = distance;
                            closestContact = contact;
                        }
                    }

                    if (closestContact != null)
                    {
                        receptor.position = closestContact.position;
                    }
                }
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }
    }

    void OnMouseDown()
    {
        isDragging = true;
    }
}