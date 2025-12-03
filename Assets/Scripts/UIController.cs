using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using DataTracking; // 添加DataTracking命名空间

/// <summary>
/// UI控制器 - 完全通过代码生成 UI，支持 XR 射线交互
/// 使用方法：在场景中创建一个空 GameObject，挂载此脚本即可
/// </summary>
public class UIController : MonoBehaviour
{
    [Header("Canvas 配置")]
    [Tooltip("UI距离相机的距离")]
    public float distanceFromCamera = 5f;

    [Tooltip("Canvas 宽度")]
    public float canvasWidth = 300f;

    [Tooltip("Canvas 高度")]
    public float canvasHeight = 300f;

    [Tooltip("Canvas 缩放（调整整体大小）")]
    public float canvasScale = 0.005f;

    [Header("Button 配置")]
    [Tooltip("按钮宽度（0 = 自动填充容器宽度）")]
    public float buttonWidth = 0f;

    [Tooltip("按钮高度")]
    public float buttonHeight = 100f;

    [Tooltip("按钮之间的间距")]
    public float buttonSpacing = 20f;

    [Header("其他配置")]
    [Tooltip("是否在启动时显示窗口")]
    public bool showOnStart = true;

    // 内部引用
    private Canvas canvas;
    private GameObject modalWindow;
    private Text titleText;
    private Transform buttonsContainer;
    private List<Button> buttons = new List<Button>();
    private Camera mainCamera;

    // 用于检测参数变化
    private float lastCanvasWidth;
    private float lastCanvasHeight;
    private float lastCanvasScale;
    private float lastDistanceFromCamera;
    private float lastButtonWidth;
    private float lastButtonHeight;
    private float lastButtonSpacing;

    // 添加输入框相关字段
    private InputField serverUrlInputField;
    private Button confirmButton;
    private Text statusText;
    private DataTracking.DataTracking dataTracking;

    private void Awake()
    {
        Debug.Log("🔍 UIController Awake() 开始");
        mainCamera = Camera.main;

        if (mainCamera == null)
        {
            Debug.LogError("❌ 找不到 Main Camera！");
        }
        else
        {
            Debug.Log($"✅ 找到 Main Camera: {mainCamera.name}");
        }

        EnsureEventSystem();
    }

    private void Start()
    {
        Debug.Log("🔍 UIController Start() 开始");
        CreateUI();

        // 初始化参数缓存
        lastCanvasWidth = canvasWidth;
        lastCanvasHeight = canvasHeight;
        lastCanvasScale = canvasScale;
        lastDistanceFromCamera = distanceFromCamera;
        lastButtonWidth = buttonWidth;
        lastButtonHeight = buttonHeight;
        lastButtonSpacing = buttonSpacing;

        if (!showOnStart)
        {
            HideModal();
        }
        
        // 获取DataTracking实例
        dataTracking = FindObjectOfType<DataTracking.DataTracking>();
        
        // 初始化输入框
        InitializeServerUrlInput();
    }

    // 初始化服务器URL输入框
    private void InitializeServerUrlInput()
    {
        if (dataTracking != null && serverUrlInputField != null)
        {
            serverUrlInputField.text = dataTracking.serverUrl;
        }
    }

    private void Update()
    {
        // 检测 Canvas 参数变化
        if (canvas != null)
        {
            bool needUpdateCanvas = false;
            bool needUpdatePosition = false;
            bool needUpdateButtons = false;

            // 检测 Canvas 尺寸变化
            if (lastCanvasWidth != canvasWidth || lastCanvasHeight != canvasHeight)
            {
                needUpdateCanvas = true;
                lastCanvasWidth = canvasWidth;
                lastCanvasHeight = canvasHeight;
            }

            // 检测 Canvas 缩放变化
            if (lastCanvasScale != canvasScale)
            {
                canvas.transform.localScale = Vector3.one * canvasScale;
                lastCanvasScale = canvasScale;
                Debug.Log($"🔄 Canvas 缩放已更新: {canvasScale}");
            }

            // 检测距离变化
            if (lastDistanceFromCamera != distanceFromCamera)
            {
                needUpdatePosition = true;
                lastDistanceFromCamera = distanceFromCamera;
            }

            // 检测按钮参数变化
            if (lastButtonWidth != buttonWidth || lastButtonHeight != buttonHeight || lastButtonSpacing != buttonSpacing)
            {
                needUpdateButtons = true;
                lastButtonWidth = buttonWidth;
                lastButtonHeight = buttonHeight;
                lastButtonSpacing = buttonSpacing;
            }

            // 执行更新
            if (needUpdateCanvas)
            {
                UpdateCanvasSize();
            }

            if (needUpdatePosition)
            {
                UpdateUIPosition();
            }

            if (needUpdateButtons)
            {
                UpdateButtons();
            }
        }
    }

    /// <summary>
    /// 更新 Canvas 尺寸
    /// </summary>
    private void UpdateCanvasSize()
    {
        if (canvas != null)
        {
            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            canvasRect.sizeDelta = new Vector2(canvasWidth, canvasHeight);
            Debug.Log($"🔄 Canvas 尺寸已更新: {canvasWidth} x {canvasHeight}");
        }
    }

    /// <summary>
    /// 更新 UI 位置
    /// </summary>
    private void UpdateUIPosition()
    {
        if (canvas != null && mainCamera != null)
        {
            Vector3 cameraPos = mainCamera.transform.position;
            Vector3 cameraForward = mainCamera.transform.forward;
            canvas.transform.position = cameraPos + cameraForward * distanceFromCamera;
            canvas.transform.LookAt(cameraPos);
            canvas.transform.Rotate(0, 180, 0);
            Debug.Log($"🔄 UI 位置已更新，距离: {distanceFromCamera}");
        }
    }

    /// <summary>
    /// 更新所有按钮的尺寸和布局
    /// </summary>
    private void UpdateButtons()
    {
        if (buttonsContainer == null) return;

        // 更新布局组件
        VerticalLayoutGroup layout = buttonsContainer.GetComponent<VerticalLayoutGroup>();
        if (layout != null)
        {
            layout.spacing = buttonSpacing;
            layout.childControlWidth = (buttonWidth == 0);
            layout.childForceExpandWidth = (buttonWidth == 0);
        }

        // 更新每个按钮的尺寸
        foreach (Button btn in buttons)
        {
            if (btn != null)
            {
                RectTransform rect = btn.GetComponent<RectTransform>();
                if (buttonWidth > 0)
                {
                    rect.sizeDelta = new Vector2(buttonWidth, buttonHeight);
                }
                else
                {
                    rect.sizeDelta = new Vector2(0, buttonHeight);
                }
            }
        }

        Debug.Log($"🔄 按钮已更新 - 宽度: {buttonWidth}, 高度: {buttonHeight}, 间距: {buttonSpacing}");
    }

    /// <summary>
    /// 确保场景中有 EventSystem
    /// </summary>
    private void EnsureEventSystem()
    {
        EventSystem eventSystem = FindObjectOfType<EventSystem>();
        if (eventSystem == null)
        {
            Debug.Log("🔍 创建 EventSystem");
            GameObject esObj = new GameObject("EventSystem");
            esObj.AddComponent<EventSystem>();
            esObj.AddComponent<StandaloneInputModule>();
            Debug.Log("✅ EventSystem 已创建");
        }
        else
        {
            Debug.Log($"✅ EventSystem 已存在: {eventSystem.name}");
        }
    }

    /// <summary>
    /// 创建整个 UI 系统
    /// </summary>
    private void CreateUI()
    {
        Debug.Log("🔍 开始创建 UI");

        // 1. 创建 Canvas
        CreateCanvas();

        // 2. 创建模态窗口
        CreateModalWindow();

        // 3. 创建标题
        CreateTitle();

        // 4. 创建按钮容器
        CreateButtonsContainer();

        // 5. 添加服务器URL输入框
        CreateServerUrlInputField();

        // 6. 添加默认按钮
        AddDefaultButtons();

        Debug.Log("✅ UI 系统创建完成");
    }

    /// <summary>
    /// 创建 Canvas
    /// </summary>
    private void CreateCanvas()
    {
        Debug.Log("🔍 创建 Canvas");

        GameObject canvasObj = new GameObject("UICanvas");
        canvasObj.transform.SetParent(transform);

        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        // 设置 Canvas 位置（在相机前方）
        if (mainCamera != null)
        {
            Vector3 cameraPos = mainCamera.transform.position;
            Vector3 cameraForward = mainCamera.transform.forward;
            canvasObj.transform.position = cameraPos + cameraForward * distanceFromCamera;
            canvasObj.transform.LookAt(cameraPos);
            canvasObj.transform.Rotate(0, 180, 0);
            Debug.Log($"✅ Canvas 位置: {canvasObj.transform.position}");
        }

        // 设置 Canvas 尺寸和缩放
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(canvasWidth, canvasHeight);
        canvasObj.transform.localScale = Vector3.one * canvasScale;

        // 添加 CanvasScaler
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 10;

        // 尝试添加 TrackedDeviceGraphicRaycaster（用于 XR）
        var raycasterType = System.Type.GetType("UnityEngine.XR.Interaction.Toolkit.UI.TrackedDeviceGraphicRaycaster, Unity.XR.Interaction.Toolkit");
        if (raycasterType != null)
        {
            canvasObj.AddComponent(raycasterType);
            Debug.Log("✅ 添加了 TrackedDeviceGraphicRaycaster");
        }
        else
        {
            canvasObj.AddComponent<GraphicRaycaster>();
            Debug.LogWarning("⚠️ 使用标准 GraphicRaycaster");
        }

        Debug.Log($"✅ Canvas 创建完成");
    }

    /// <summary>
    /// 创建模态窗口背景
    /// </summary>
    private void CreateModalWindow()
    {
        Debug.Log("🔍 创建 ModalWindow");

        modalWindow = new GameObject("ModalWindow");
        modalWindow.transform.SetParent(canvas.transform, false);

        RectTransform rect = modalWindow.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
        rect.anchoredPosition = Vector2.zero;

        Image bgImage = modalWindow.AddComponent<Image>();
        bgImage.color = new Color(0.15f, 0.15f, 0.15f, 1f); // 深灰色不透明

        Debug.Log($"✅ ModalWindow 创建完成，Active: {modalWindow.activeSelf}");
    }

    /// <summary>
    /// 创建标题栏
    /// </summary>
    private void CreateTitle()
    {
        Debug.Log("🔍 创建 Title");

        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(modalWindow.transform, false);

        RectTransform rect = titleObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(0.5f, 1);
        rect.sizeDelta = new Vector2(0, 100);
        rect.anchoredPosition = Vector2.zero;

        Image bgImage = titleObj.AddComponent<Image>();
        bgImage.color = new Color(0.1f, 0.1f, 0.1f, 1f);

        // 创建标题文本
        GameObject textObj = new GameObject("TitleText");
        textObj.transform.SetParent(titleObj.transform, false);

        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;

        titleText = textObj.AddComponent<Text>();
        titleText.text = "VR UI Test Window";
        titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleText.fontSize = 48;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = Color.white;
        titleText.fontStyle = FontStyle.Bold;

        Debug.Log($"✅ Title 创建完成，文本: {titleText.text}，字体: {titleText.font?.name}");
    }

    /// <summary>
    /// 创建按钮容器
    /// </summary>
    private void CreateButtonsContainer()
    {
        Debug.Log("🔍 创建 ButtonsContainer");

        if (modalWindow == null)
        {
            Debug.LogError("❌ modalWindow 为空！");
            return;
        }

        GameObject containerObj = new GameObject("ButtonsContainer");
        containerObj.transform.SetParent(modalWindow.transform, false);

        RectTransform rect = containerObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = new Vector2(40, 40);   // 左、下边距
        rect.offsetMax = new Vector2(-40, -120); // 右、上边距（为标题留空间）

        buttonsContainer = containerObj.transform;

        // 添加垂直布局
        VerticalLayoutGroup layout = containerObj.AddComponent<VerticalLayoutGroup>();
        layout.spacing = buttonSpacing;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = (buttonWidth == 0); // 如果 buttonWidth = 0，自动填充宽度
        layout.childControlHeight = false;
        layout.childForceExpandWidth = (buttonWidth == 0);
        layout.childForceExpandHeight = false;

        Debug.Log($"✅ ButtonsContainer 创建完成，Parent: {containerObj.transform.parent.name}");
    }

    /// <summary>
    /// 创建服务器URL输入框
    /// </summary>
    private void CreateServerUrlInputField()
    {
        if (buttonsContainer == null) return;

        Debug.Log("🔍 创建 Server URL InputField");

        // 创建输入框容器
        GameObject inputContainer = new GameObject("ServerUrlInputContainer");
        inputContainer.transform.SetParent(buttonsContainer, false);

        RectTransform containerRect = inputContainer.AddComponent<RectTransform>();
        containerRect.sizeDelta = new Vector2(0, 120);

        // 创建输入框
        GameObject inputFieldObj = new GameObject("ServerUrlInputField");
        inputFieldObj.transform.SetParent(inputContainer.transform, false);

        RectTransform inputRect = inputFieldObj.AddComponent<RectTransform>();
        inputRect.anchorMin = Vector2.zero;
        inputRect.anchorMax = new Vector2(0.7f, 1f);
        inputRect.pivot = new Vector2(0, 0.5f);
        inputRect.offsetMin = new Vector2(0, 10);
        inputRect.offsetMax = new Vector2(-10, -10);

        serverUrlInputField = inputFieldObj.AddComponent<InputField>();
        serverUrlInputField.text = "https://localhost:5000/poseData";

        Image inputBg = inputFieldObj.AddComponent<Image>();
        inputBg.color = new Color(0.2f, 0.2f, 0.2f, 1f);

        serverUrlInputField.targetGraphic = inputBg;
        serverUrlInputField.placeholder = CreatePlaceholder("输入服务器地址...");

        Text inputText = CreateTextComponent(inputFieldObj, "ServerUrlInputText");
        inputText.alignment = TextAnchor.MiddleLeft;
        serverUrlInputField.textComponent = inputText;

        // 创建确认按钮
        GameObject confirmBtnObj = new GameObject("ConfirmButton");
        confirmBtnObj.transform.SetParent(inputContainer.transform, false);

        RectTransform confirmRect = confirmBtnObj.AddComponent<RectTransform>();
        confirmRect.anchorMin = new Vector2(0.7f, 0);
        confirmRect.anchorMax = Vector2.one;
        confirmRect.pivot = new Vector2(0.5f, 0.5f);
        confirmRect.offsetMin = new Vector2(10, 10);
        confirmRect.offsetMax = new Vector2(0, -10);

        confirmButton = confirmBtnObj.AddComponent<Button>();

        Image confirmBg = confirmBtnObj.AddComponent<Image>();
        confirmBg.color = new Color(0.2f, 0.6f, 1f, 1f);
        confirmButton.targetGraphic = confirmBg;

        Text confirmText = CreateTextComponent(confirmBtnObj, "ConfirmButtonText");
        confirmText.text = "确认";
        confirmText.alignment = TextAnchor.MiddleCenter;

        confirmButton.onClick.AddListener(OnConfirmServerUrl);

        // 创建状态文本
        GameObject statusObj = new GameObject("StatusText");
        statusObj.transform.SetParent(inputContainer.transform, false);

        RectTransform statusRect = statusObj.AddComponent<RectTransform>();
        statusRect.anchorMin = new Vector2(0, 0);
        statusRect.anchorMax = new Vector2(1, 0);
        statusRect.pivot = new Vector2(0.5f, 0);
        statusRect.offsetMin = new Vector2(0, -30);
        statusRect.offsetMax = new Vector2(0, -10);

        statusText = CreateTextComponent(statusObj, "StatusText");
        statusText.fontSize = 20;
        statusText.alignment = TextAnchor.MiddleCenter;
        statusText.color = Color.green;

        Debug.Log("✅ Server URL InputField 创建完成");
    }

    // 创建占位符文本
    private Text CreatePlaceholder(string placeholderText)
    {
        GameObject placeholderObj = new GameObject("Placeholder");
        placeholderObj.transform.SetParent(serverUrlInputField.transform, false);

        RectTransform rect = placeholderObj.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
        rect.anchoredPosition = Vector2.zero;

        Text placeholder = placeholderObj.AddComponent<Text>();
        placeholder.text = placeholderText;
        placeholder.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        placeholder.fontSize = 36;
        placeholder.alignment = TextAnchor.MiddleLeft;
        placeholder.color = new Color(0.7f, 0.7f, 0.7f, 0.5f);

        return placeholder;
    }

    // 创建文本组件
    private Text CreateTextComponent(GameObject parent, string name)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent.transform, false);

        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;

        Text text = textObj.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 36;
        text.color = Color.white;

        return text;
    }

    /// <summary>
    /// 添加默认按钮
    /// </summary>
    private void AddDefaultButtons()
    {
        Debug.Log("🔍 添加默认按钮");
        AddButton("CONFIRM", OnConfirmClicked, new Color(0.2f, 0.6f, 1f));
        AddButton("CANCEL", OnCancelClicked, new Color(0.7f, 0.7f, 0.7f));
        AddButton("APPLY", OnApplyClicked, new Color(0.3f, 0.7f, 0.3f));
    }

    /// <summary>
    /// 动态添加按钮
    /// </summary>
    public Button AddButton(string buttonText, UnityAction onClick, Color? buttonColor = null)
    {
        if (buttonsContainer == null)
        {
            Debug.LogError("❌ buttonsContainer 为空！");
            return null;
        }

        Debug.Log($"🔍 创建按钮: {buttonText}");

        GameObject buttonObj = new GameObject($"Button_{buttonText}");
        buttonObj.transform.SetParent(buttonsContainer, false);

        RectTransform rect = buttonObj.AddComponent<RectTransform>();

        // 根据 buttonWidth 设置按钮尺寸
        if (buttonWidth > 0)
        {
            rect.sizeDelta = new Vector2(buttonWidth, buttonHeight); // 固定宽高
        }
        else
        {
            rect.sizeDelta = new Vector2(0, buttonHeight); // 只设置高度，宽度由布局控制
        }

        // 先创建 Image 组件
        Image bgImage = buttonObj.AddComponent<Image>();

        Button button = buttonObj.AddComponent<Button>();

        Color normalColor = buttonColor ?? new Color(0.2f, 0.6f, 1f);

        // 设置按钮的 targetGraphic（非常重要！）
        button.targetGraphic = bgImage;

        // 显式设置 Transition 为 ColorTint（确保 hover 效果生效）
        button.transition = Selectable.Transition.ColorTint;

        ColorBlock colors = button.colors;
        colors.normalColor = normalColor;
        colors.highlightedColor = Color.Lerp(normalColor, Color.white, 0.4f); // hover 时变浅（混合白色）
        colors.pressedColor = normalColor * 0.7f;                              // 点击时变深
        colors.selectedColor = normalColor;
        colors.fadeDuration = 0.15f; // 平滑过渡
        button.colors = colors;

        bgImage.color = normalColor;

        Debug.Log($"🔍 按钮 {buttonText} - targetGraphic: {button.targetGraphic != null}, transition: {button.transition}, 尺寸: {rect.sizeDelta}");

        // 添加 EventTrigger 来处理 hover 事件（额外的视觉反馈）
        EventTrigger trigger = buttonObj.AddComponent<EventTrigger>();

        // PointerEnter 事件
        EventTrigger.Entry enterEntry = new EventTrigger.Entry();
        enterEntry.eventID = EventTriggerType.PointerEnter;
        enterEntry.callback.AddListener((data) => OnButtonHoverEnter(buttonObj, buttonText));
        trigger.triggers.Add(enterEntry);

        // PointerExit 事件
        EventTrigger.Entry exitEntry = new EventTrigger.Entry();
        exitEntry.eventID = EventTriggerType.PointerExit;
        exitEntry.callback.AddListener((data) => OnButtonHoverExit(buttonObj, buttonText));
        trigger.triggers.Add(exitEntry);

        // 创建按钮文本
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);

        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;

        Text text = textObj.AddComponent<Text>();
        text.text = buttonText;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 36;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.fontStyle = FontStyle.Bold;

        button.onClick.AddListener(onClick);
        buttons.Add(button);

        Debug.Log($"✅ 按钮创建完成: {buttonText}");

        return button;
    }

    /// <summary>
    /// 按钮 Hover 进入事件
    /// </summary>
    private void OnButtonHoverEnter(GameObject buttonObj, string buttonText)
    {
        Debug.Log($"🎯 Hover 进入: {buttonText}");
        // 可以在这里添加额外的视觉效果，比如缩放动画
        // buttonObj.transform.localScale = Vector3.one * 1.05f;
    }

    /// <summary>
    /// 按钮 Hover 退出事件
    /// </summary>
    private void OnButtonHoverExit(GameObject buttonObj, string buttonText)
    {
        Debug.Log($"🎯 Hover 退出: {buttonText}");
        // buttonObj.transform.localScale = Vector3.one;
    }

    // 确认服务器URL按钮点击事件
    private void OnConfirmServerUrl()
    {
        if (dataTracking != null && serverUrlInputField != null)
        {
            string newUrl = serverUrlInputField.text.Trim();
            
            if (!string.IsNullOrEmpty(newUrl))
            {
                // 验证URL格式
                if (IsValidUrl(newUrl))
                {
                    // 更新DataTracking中的serverUrl
                    dataTracking.serverUrl = newUrl;
                    
                    // 保存到PlayerPrefs以便下次启动时使用
                    PlayerPrefs.SetString("ServerUrl", newUrl);
                    PlayerPrefs.Save();

                    // 更新状态文本
                    if (statusText != null)
                    {
                        statusText.text = "服务器地址已更新";
                        statusText.color = Color.green;
                    }

                    Debug.Log($"服务器地址已更新为: {newUrl}");
                }
                else
                {
                    // URL格式无效
                    if (statusText != null)
                    {
                        statusText.text = "URL格式无效";
                        statusText.color = Color.red;
                    }
                }
            }
            else
            {
                // URL为空
                if (statusText != null)
                {
                    statusText.text = "URL不能为空";
                    statusText.color = Color.red;
                }
            }
        }
    }

    // 验证URL格式
    private bool IsValidUrl(string url)
    {
        if (string.IsNullOrEmpty(url)) return false;

        // 如果URL不包含协议，则自动添加https://
        if (!url.StartsWith("http://") && !url.StartsWith("https://"))
        {
            url = "https://" + url;
        }

        try
        {
            var uri = new System.Uri(url);
            return uri.Scheme == System.Uri.UriSchemeHttp || uri.Scheme == System.Uri.UriSchemeHttps;
        }
        catch
        {
            return false;
        }
    }

    public void ShowModal(string title = "VR UI Test Window")
    {
        if (modalWindow != null)
        {
            modalWindow.SetActive(true);
            if (titleText != null)
            {
                titleText.text = title;
            }
            Debug.Log($"✅ 显示模态窗口: {title}");
        }
    }

    public void HideModal()
    {
        if (modalWindow != null)
        {
            modalWindow.SetActive(false);
            Debug.Log("✅ 隐藏模态窗口");
        }
    }

    private void OnConfirmClicked()
    {
        Debug.Log("✅✅✅ BUTTON CONFIRM 按钮被点击！");
        // HideModal();
    }

    private void OnCancelClicked()
    {
        Debug.Log("❌❌❌ BUTTON CANCEL 按钮被点击！");
        // HideModal();
    }

    private void OnApplyClicked()
    {
        Debug.Log("✔️✔️✔️ BUTTON APPLY 按钮被点击！");
    }

    private void OnDestroy()
    {
        foreach (Button btn in buttons)
        {
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
            }
        }
        
        if (confirmButton != null)
        {
            confirmButton.onClick.RemoveListener(OnConfirmServerUrl);
        }
    }
}