using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 支撑点类型
/// </summary>
public enum SupportPointType
{
    /// <summary>
    /// A类型
    /// </summary>
    TYPE_A = 1,

    /// <summary>
    /// B类型
    /// </summary>
    TYPE_B = 2,

    /// <summary>
    /// C类型
    /// </summary>
    TYPE_C = 3,
}

/// <summary>
/// 支撑点类型追加
/// </summary>
public static class SupportPointTypeAdd
{
    /// <summary>
    /// 获取值
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static string GetValue(this SupportPointType type)
    {
        var ret = "";
        switch (type)
        {
            case SupportPointType.TYPE_A:
                ret = "A";
                break;
            case SupportPointType.TYPE_B:
                ret = "B";
                break;
            case SupportPointType.TYPE_C:
                ret = "C";
                break;
            default:
                break;
        }
        return ret;
    }
}

/// <summary>
/// 支撑点状态
/// </summary>
public enum SupportPointStatus
{
    /// <summary>
    /// 错误状态
    /// </summary>
    NONE = -1,

    /// <summary>
    /// 接受点
    /// </summary>
    RECEPTOR = 0,

    /// <summary>
    /// 支撑点
    /// </summary>
    CONTACT = 1,
}

/// <summary>
/// 支撑点
/// </summary>
public class SupportPoint
{
    /// <summary>
    /// 支撑点类型
    /// </summary>
    public SupportPointType type;

    /// <summary>
    /// 支撑点状态
    /// </summary>
    public SupportPointStatus status;

    /// <summary>
    /// 点位置
    /// </summary>
    public Transform position;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="type"></param>
    /// <param name="status"></param>
    /// <param name="position"></param>
    public SupportPoint(SupportPointType type, SupportPointStatus status, Transform position)
    {
        this.type = type;
        this.status = status;
        this.position = position;
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="type"></param>
    /// <param name="position"></param>
    public SupportPoint(SupportPointType type, Transform position) : this(type, SupportPointStatus.NONE, position)
    {

    }
}

