using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// 修改颜色和透明度
/// </summary>
public class ChangeRGBA : MonoBehaviour
{
    [Header("颜色通道")]
    [SerializeField, Range(0f, 1f)]
    public float r = 1f;
    [SerializeField, Range(0f, 1f)]
    public float g = 1f;
    [SerializeField, Range(0f, 1f)]
    public float b = 1f;

    [Header("透明度")]
    [SerializeField, Range(0f, 1f)]
    public float a = 1f;

    private Renderer rend;
    private Material mat;
    private Color lastColor; // 记录上次颜色状态

    /// <summary>
    /// 设置完整颜色(RGBA)
    /// </summary>
    /// <param name="color"></param>
    public void SetColor(Color color)
    {
        r = color.r;
        g = color.g;
        b = color.b;
        a = color.a;
        UpdateMaterialColor();
    }

    /// <summary>
    /// 设置颜色(RGB)
    /// </summary>
    /// <param name="red"></param>
    /// <param name="green"></param>
    /// <param name="blue"></param>
    public void SetColor(float red,float green,float blue)
    {
        r = Mathf.Clamp01(red);
        g = Mathf.Clamp01(green);
        b = Mathf.Clamp01(blue);
        UpdateMaterialColor();
    }

    /// <summary>
    /// 设置颜色(RGBA)
    /// </summary>
    /// <param name="red"></param>
    /// <param name="green"></param>
    /// <param name="blue"></param>
    /// <param name="alpha"></param>
    public void SetColor(float red, float green, float blue, float alpha)
    {
        r = Mathf.Clamp01(red);
        g = Mathf.Clamp01(green);
        b = Mathf.Clamp01(blue);
        a = Mathf.Clamp01(alpha);
        UpdateMaterialColor();
    }

    /// <summary>
    /// 设置透明度
    /// </summary>
    /// <param name="alpha"></param>
    public void SetOpacity(float alpha)
    {
        a = Mathf.Clamp01(alpha);
        UpdateMaterialColor();
    }

    void Awake()
    {
        InitializeMaterials();
        UpdateMaterialColor();
    }

    void Update()
    {
        // 当颜色值发生变化时更新材质
        if (ColorChanged())
        {
            UpdateMaterialColor();
        }
    }

    /// <summary>
    /// 初始化材质实例
    /// </summary>
    private void InitializeMaterials()
    {
        rend = GetComponent<Renderer>();
        mat = rend.material;

        // 遍历子对象创建独立材质实例
        foreach (Transform child in GetComponentsInChildren<Transform>())
        {
            var childRenderer = child.GetComponent<Renderer>();
            if (childRenderer != null)
            {
                childRenderer.material = new Material(childRenderer.material);
            }
        }
    }

    /// <summary>
    /// 更新当前材质颜色
    /// </summary>
    public void UpdateMaterialColor()
    {
        // 生成新颜色
        Color newColor = new Color(r, g, b, a);

        // 设置主物体颜色
        mat.color = newColor;
        UpdateBlendMode(mat);

        // 设置子物体颜色
        foreach (Transform child in transform)
        {
            ApplyColorToChild(child, newColor);
        }

        lastColor = newColor; // 保存当前颜色状态
    }

    /// <summary>
    /// 判断颜色是否发生变化
    /// </summary>
    private bool ColorChanged()
    {
        return lastColor.r != r ||
               lastColor.g != g ||
               lastColor.b != b ||
               lastColor.a != a;
    }

    /// <summary>
    /// 递归设置子物体颜色
    /// </summary>
    private void ApplyColorToChild(Transform parent, Color color)
    {
        foreach (Transform child in parent)
        {
            var renderer = child.GetComponent<Renderer>();
            if (renderer && child.GetComponent<ChangeRGBA>() == null)
            {
                renderer.material.color = color;
                UpdateBlendMode(renderer.material);
            }
            ApplyColorToChild(child, color);
        }
    }

    /// <summary>
    /// 更新材质混合模式
    /// </summary>
    private void UpdateBlendMode(Material material)
    {
        bool isTransparent = a < 1f;

        material.SetInt("_SrcBlend", (int)(isTransparent ? BlendMode.SrcAlpha : BlendMode.One));
        material.SetInt("_DstBlend", (int)(isTransparent ? BlendMode.OneMinusSrcAlpha : BlendMode.Zero));
        material.SetInt("_ZWrite", isTransparent ? 0 : 1);
        material.renderQueue = isTransparent ? 3000 : -1;

        // 控制关键字状态
        material.SetKeyword("_ALPHATEST_ON", !isTransparent);
        material.SetKeyword("_ALPHABLEND_ON", isTransparent);
    }

    /// <summary>
    /// 可视化调试
    /// </summary>
    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying && rend != null)
        {
            rend.sharedMaterial.color = new Color(r, g, b, a);
        }
    }
}

/// <summary>
/// 扩展方法：更安全的关键字控制
/// </summary>
public static class MaterialExtensions
{
    public static void SetKeyword(this Material material, string keyword, bool state)
    {
        if (state)
            material.EnableKeyword(keyword);
        else
            material.DisableKeyword(keyword);
    }
}
