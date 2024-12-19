using UnityEngine;

/// <summary>
/// 修改透明度
/// </summary>
public class ChangeOpacity : MonoBehaviour
{
    /// <summary>
    /// 透明度
    /// </summary>
    [SerializeField]
    [Range(0f, 1f)]
    public float opacity = 1f;

    private Renderer rend;
    private Material mat;

    void Awake()
    {
        rend = GetComponent<Renderer>();
        mat = rend.material;  // 获取材质并保存在本地，以避免多次获取

        // 设置当前对象的透明度
        SetOpacity(opacity);

        // 遍历子对象并设置透明度
        SetOpacityForChildren(transform, opacity);
    }

    void Update()
    {
        // 可以根据其他逻辑修改透明度并调用 SetOpacity
        // 例如，如果你想让透明度随时间变化：
        // opacity = Mathf.PingPong(Time.time, 1f);  // 透明度在 0 到 1 之间波动
        // SetOpacity(opacity);
    }

    /// <summary>
    /// 设置当前对象的透明度
    /// </summary>
    /// <param name="opacity">透明度值</param>
    public void SetOpacity(float opacity)
    {
        Color color = mat.color;
        color.a = opacity;
        mat.color = color;

        // 设置材质的透明度相关属性
        if (opacity < 1f)
        {
            // 透明
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.renderQueue = 3000;
        }
        else
        {
            // 不透明
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
            mat.SetInt("_ZWrite", 1);
            mat.EnableKeyword("_ALPHATEST_ON");
            mat.DisableKeyword("_ALPHABLEND_ON");
            mat.renderQueue = -1;
        }
    }

    /// <summary>
    /// 设置子对象的透明度（递归遍历子对象）
    /// </summary>
    /// <param name="parentTransform">父对象的 Transform</param>
    /// <param name="opacity">透明度值</param>
    private void SetOpacityForChildren(Transform parentTransform, float opacity)
    {
        // 遍历当前对象的所有子对象
        foreach (Transform child in parentTransform)
        {
            // 如果子对象没有附加 ChangeOpacity 组件，则设置透明度
            if (child.GetComponent<ChangeOpacity>() == null)
            {
                Renderer childRenderer = child.GetComponent<Renderer>();
                if (childRenderer != null)
                {
                    // 获取子对象的材质
                    Material childMat = childRenderer.material;
                    Color color = childMat.color;
                    color.a = opacity;
                    childMat.color = color;

                    // 设置子对象的透明度相关属性
                    if (opacity < 1f)
                    {
                        childMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                        childMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                        childMat.SetInt("_ZWrite", 0);
                        childMat.DisableKeyword("_ALPHATEST_ON");
                        childMat.EnableKeyword("_ALPHABLEND_ON");
                        childMat.renderQueue = 3000;
                    }
                    else
                    {
                        childMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                        childMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                        childMat.SetInt("_ZWrite", 1);
                        childMat.EnableKeyword("_ALPHATEST_ON");
                        childMat.DisableKeyword("_ALPHABLEND_ON");
                        childMat.renderQueue = -1;
                    }
                }
            }

            // 递归调用：设置子对象的所有孙子对象的透明度
            SetOpacityForChildren(child, opacity);
        }
    }
}
