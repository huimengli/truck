using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// �޸���ɫ��͸����
/// </summary>
public class ChangeRGBA : MonoBehaviour
{
    [Header("��ɫͨ��")]
    [SerializeField, Range(0f, 1f)]
    public float r = 1f;
    [SerializeField, Range(0f, 1f)]
    public float g = 1f;
    [SerializeField, Range(0f, 1f)]
    public float b = 1f;

    [Header("͸����")]
    [SerializeField, Range(0f, 1f)]
    public float a = 1f;

    private Renderer rend;
    private Material mat;
    private Color lastColor; // ��¼�ϴ���ɫ״̬

    /// <summary>
    /// ����������ɫ(RGBA)
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
    /// ������ɫ(RGB)
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
    /// ������ɫ(RGBA)
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
    /// ����͸����
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
        // ����ɫֵ�����仯ʱ���²���
        if (ColorChanged())
        {
            UpdateMaterialColor();
        }
    }

    /// <summary>
    /// ��ʼ������ʵ��
    /// </summary>
    private void InitializeMaterials()
    {
        rend = GetComponent<Renderer>();
        mat = rend.material;

        // �����Ӷ��󴴽���������ʵ��
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
    /// ���µ�ǰ������ɫ
    /// </summary>
    public void UpdateMaterialColor()
    {
        // ��������ɫ
        Color newColor = new Color(r, g, b, a);

        // ������������ɫ
        mat.color = newColor;
        UpdateBlendMode(mat);

        // ������������ɫ
        foreach (Transform child in transform)
        {
            ApplyColorToChild(child, newColor);
        }

        lastColor = newColor; // ���浱ǰ��ɫ״̬
    }

    /// <summary>
    /// �ж���ɫ�Ƿ����仯
    /// </summary>
    private bool ColorChanged()
    {
        return lastColor.r != r ||
               lastColor.g != g ||
               lastColor.b != b ||
               lastColor.a != a;
    }

    /// <summary>
    /// �ݹ�������������ɫ
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
    /// ���²��ʻ��ģʽ
    /// </summary>
    private void UpdateBlendMode(Material material)
    {
        bool isTransparent = a < 1f;

        material.SetInt("_SrcBlend", (int)(isTransparent ? BlendMode.SrcAlpha : BlendMode.One));
        material.SetInt("_DstBlend", (int)(isTransparent ? BlendMode.OneMinusSrcAlpha : BlendMode.Zero));
        material.SetInt("_ZWrite", isTransparent ? 0 : 1);
        material.renderQueue = isTransparent ? 3000 : -1;

        // ���ƹؼ���״̬
        material.SetKeyword("_ALPHATEST_ON", !isTransparent);
        material.SetKeyword("_ALPHABLEND_ON", isTransparent);
    }

    /// <summary>
    /// ���ӻ�����
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
/// ��չ����������ȫ�Ĺؼ��ֿ���
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
