using Export.AddFunc;
using Export.Attribute;
using Export.BehaviourEX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 添加DropAndStick的点
/// </summary>
public class AddDASPoint : UUIDBehavior
{
    /// <summary>
    /// 游戏调用代码
    /// </summary>
    [ReadOnly]
    public GameConfig config;

    /// <summary>
    /// 追加点位名称
    /// </summary>
    [SerializeField]
    [ReadOnly]
    public string addPointsName;

    /// <summary>
    /// 拖拽脚本
    /// (用于判断点是否添加到点表中)
    /// </summary>
    public DragAndStick dragScript;

    /// <summary>
    /// 读取名称
    /// </summary>
    private Regex readName;

    /// <summary>
    /// 支撑点类型
    /// </summary>
    public SupportPointType type;

    /// <summary>
    /// 追加点
    /// </summary>
    [SerializeField]
    [ReadOnly]
    public Transform[] points;

    /// <summary>
    /// 追加点
    /// </summary>
    public List<Transform> Points
    {
        get
        {
            return new List<Transform>(points);
        }
    }

    private void Awake()
    {
        config = FindObjectOfType<GameConfig>();
        addPointsName = config.ContactPointName + type.GetValue();
    }

    private void Start()
    {
        readName = new Regex(addPointsName);

        points = transform.GetComponentsInChildren<Transform>();
        points = points.Where(t => t != null && readName.IsMatch(t.name)).ToArray();

        if (dragScript == null)
        {
            PointPool._points[type].Add(UUID, Points);
        }
    }

    private void Update()
    {
        if (dragScript != null)
        {
            if (dragScript.isSticked)
            {
                PointPool._points[type].AddOrSet(UUID, Points);
            }
            else
            {
                PointPool._points[type].Remove(UUID);
            }
        }
    }
}
