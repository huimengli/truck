using Export.AddFunc;
using Export.Class;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 点池(全局静态字典)
/// </summary>
public static class PointPool
{
    /// <summary>
    /// 点池
    /// </summary>
    public static DictionaryEX<SupportPointType, DictionaryEX<string, List<Transform>>> _points = new DictionaryEX<SupportPointType, DictionaryEX<string, List<Transform>>> {
        { SupportPointType.TYPE_A,new DictionaryEX<string, List<Transform>>() },
        { SupportPointType.TYPE_B,new DictionaryEX<string, List<Transform>>() },
        { SupportPointType.TYPE_C,new DictionaryEX<string, List<Transform>>() },
    };

    /// <summary>
    /// 用过的吸附点
    /// </summary>
    public static DictionaryEX<SupportPointType, DictionaryEX<string, List<Transform>>> _used = new DictionaryEX<SupportPointType, DictionaryEX<string, List<Transform>>>
    {
        { SupportPointType.TYPE_A,new DictionaryEX<string, List<Transform>>() },
        { SupportPointType.TYPE_B,new DictionaryEX<string, List<Transform>>() },
        { SupportPointType.TYPE_C,new DictionaryEX<string, List<Transform>>() },
    };

    /// <summary>
    /// 获取用过的吸附点列表
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static List<Transform> getUseds(SupportPointType type)
    {
        return _used[type].ValuesToList().Join();
    }

    /// <summary>
    /// 获取点
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static List<Transform> getPoints(SupportPointType type)
    {
        var used = getUseds(type);
        return _points[type].ValuesToList().Join().Where(p => !used.Contains(p)).ToList();
    }
}
