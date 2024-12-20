using Export.Attribute;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

/// <summary>
/// 拖拽功能实现
/// </summary>
public class DragAndStick : DragAndStickBehaviour
{
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
    /// 初始化
    /// </summary>
    private void Awake()
    {
        config = FindObjectOfType<GameConfig>();
        receptorPointName = config.ReceptorPointName + type.GetValue();

        // 调用基类初始化逻辑
        Init();
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
    }
}
