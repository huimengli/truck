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

        // ���������ʵ����
        rend.material = new Material(rend.material);
        mat = rend.material;

        // �����崦����Ƕ���Ӽ���
        foreach (var childRenderer in GetComponentsInChildren<Renderer>(true))
        {
            if (childRenderer.GetComponent<ChangeRGBA>() != null) continue;
            childRenderer.material = new Material(childRenderer.material);

            // ����Ӧ�ó�ʼ��ɫ
            childRenderer.material.color = new Color(r, g, b, a);
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
        Color targetColor = new Color(r, g, b, a);

        // ͨ����������
        material.SetColor("_Color", targetColor);
        material.SetColor("_BaseColor", targetColor); // ����HDRP/URP

        // ��׼Shader���⴦��
        if (material.shader.name.Contains("Standard"))
        {
            int mode = isTransparent ? 2 : 0; // 0=Opaque, 1=Cutout, 2=Fade, 3=Transparent
            material.SetFloat("_Mode", mode);
            material.SetOverrideTag("RenderType", isTransparent ? "Transparent" : "Opaque");

            // ��ʽ���û�ϲ���
            material.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", isTransparent ? 0 : 1);

            // ������������
            material.SetTexture("_MainTex", material.mainTexture);
        }
        else
        {
            // ͨ��Shader����
            material.SetInt("_SrcBlend", (int)(isTransparent ? BlendMode.SrcAlpha : BlendMode.One));
            material.SetInt("_DstBlend", (int)(isTransparent ? BlendMode.OneMinusSrcAlpha : BlendMode.Zero));
            material.SetInt("_ZWrite", isTransparent ? 0 : 1);
        }

        // ǿ�Ƹ�����Ⱦ����
        material.renderQueue = isTransparent ?
            (int)RenderQueue.Transparent :
            (int)RenderQueue.Geometry;

        // ˫����Ⱦ���ƣ���Ҫ����
        material.SetInt("_Cull", isTransparent ? (int)CullMode.Off : (int)CullMode.Back);

        // �ؼ���״̬����
        material.SetKeyword("_ALPHAPREMULTIPLY_ON", false);
        material.SetKeyword("_ALPHABLEND_ON", isTransparent);
        material.SetKeyword("_ALPHATEST_ON", false);

        // ǿ�Ƹ��²���
        material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
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

    /// <summary>
    /// �༭ģʽ�µĵ���
    /// </summary>
    void OnValidate()
    {
        if (!Application.isPlaying)
        {
            // ��ȫ��ȡ��Ⱦ��
            if (rend == null) rend = GetComponent<Renderer>();
            if (rend == null) return;

            // ������ʱ����ʵ��������Ⱦԭʼ����
            var tempMat = new Material(rend.sharedMaterial);
            tempMat.color = new Color(r, g, b, a);
            rend.sharedMaterial = tempMat;
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
