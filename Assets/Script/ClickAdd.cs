using Export.Attribute;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 点击添加对象
/// </summary>
public class ClickAdd : MonoBehaviour
{
    /// <summary>
    /// 添加的对象
    /// </summary>
    [SerializeField]
    public GameObject AddPrefab;

    /// <summary>
    /// 图片显示对象
    /// </summary>
    [SerializeField]
    [ReadOnly]
    public Image image;

    /// <summary>
    /// 渲染用的RenderTexture
    /// </summary>
    [SerializeField]
    [ReadOnly]
    public RenderTexture renderTexture;

    /// <summary>
    /// 用来渲染的Camera
    /// </summary>
    [SerializeField]
    [ReadOnly]
    private Camera renderCamera;

    private GameObject model;

    private void Awake()
    {
        if (AddPrefab == null || renderTexture == null || image == null)
        {
            return;
        }

        // 查找共享相机，避免每次都创建新的相机
        renderCamera = Camera.main; // 使用主相机来减少开销

        if (renderCamera == null)
        {
            // 如果主相机不存在，则创建一个临时相机来渲染
            renderCamera = new GameObject("RenderCamera").AddComponent<Camera>();
        }

        renderCamera.targetTexture = renderTexture;  // 设置渲染目标为RenderTexture
        renderCamera.clearFlags = CameraClearFlags.SolidColor; // 仅清除颜色
        renderCamera.backgroundColor = Color.clear; // 背景设为透明

        if (model == null)
        {
            // 实例化你的3D对象Prefab（只实例化一次）
            model = Instantiate(AddPrefab);
            model.SetActive(false); // 初始状态不渲染，等待显示时才启用

            // 将模型的位置放在摄像机前面，确保摄像机能够看到它
            model.transform.position = new Vector3(0, 0, 5);

            // 设置摄像机视图来适应模型
            renderCamera.transform.position = new Vector3(0, 0, -5);  // 摄像机朝向模型
            renderCamera.transform.LookAt(model.transform);  // 让摄像机看向模型
        }

        // 在UI上显示RenderTexture
        if (image != null)
        {
            // 将RenderTexture转换为Sprite
            image.material.mainTexture = renderTexture;
        }
    }

    // 当你需要显示模型时调用此方法
    public void ShowModel()
    {
        if (model != null)
        {
            model.SetActive(true); // 激活模型，进行渲染
            renderCamera.enabled = true; // 启用相机进行渲染
        }
    }

    // 当你不再需要显示模型时调用此方法
    public void HideModel()
    {
        if (model != null)
        {
            model.SetActive(false); // 禁用模型，停止渲染
            renderCamera.enabled = false; // 禁用相机
        }
    }
}
