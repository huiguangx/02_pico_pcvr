# UIController 使用指南

## 概述

`UIController` 完全通过代码生成 UI，无需在场景中手动配置任何 UI 元素。只需添加脚本即可自动创建完整的模态窗口和按钮。

---

## 快速开始

### 1. 添加 UIController 到场景

在 Unity Hierarchy 中：
1. 创建空 GameObject（右键 → Create Empty）
2. 重命名为 "UIManager"
3. Add Component → UIController

**就这么简单！** UI 会在运行时自动创建。

### 2. 配置参数（可选）

在 Inspector 中可以调整以下参数：

| 参数 | 说明 | 默认值 |
|------|------|--------|
| **UI Position** | UI 在世界空间中的位置 | (0, 1.5, 2) |
| **UI Scale** | UI 的缩放大小 | 0.001 |
| **Window Width** | 窗口宽度（像素） | 500 |
| **Window Height** | 窗口高度（像素） | 400 |
| **Button Spacing** | 按钮之间的间距 | 10 |
| **Show On Start** | 是否在启动时显示窗口 | true |

---

## 配置 XR 射线交互

UIController 会自动添加 `TrackedDeviceGraphicRaycaster` 组件，但你仍需要在手柄上配置射线交互器。

### 方法 1：使用 XR Interaction Toolkit 预制体（推荐）

如果你的场景中使用了 XR Interaction Toolkit 的 XR Origin 预制体：

1. 在项目中找到 **XR Origin (XR Rig)** 预制体
2. 确保它包含 **XR Ray Interactor** 组件在 LeftHand 和 RightHand Controller 上
3. 如果没有，继续看方法 2

### 方法 2：手动添加射线交互器

选择 **XR Origin → Camera Offset → LeftHand Controller**：

1. Add Component → **XR Ray Interactor**
   - Line Type: Straight Line
   - Max Raycast Distance: 10
   - **Enable Interaction with UI GameObjects**: ✓ 勾选

2. Add Component → **XR Interactor Line Visual**
   - Line Width: 0.02

3. Add Component → **Line Renderer**
   - Width: 0.02

对 **RightHand Controller** 重复相同步骤。

---

## 代码使用示例

### 基础使用

```csharp
// 获取 UIController 引用
UIController uiController = FindObjectOfType<UIController>();

// 显示窗口
uiController.ShowModal("我的窗口标题");

// 隐藏窗口
uiController.HideModal();

// 切换显示状态
uiController.ToggleModal();
```

### 添加自定义按钮

在 `UIController` 脚本中修改 `AddDefaultButtons()` 方法：

```csharp
private void AddDefaultButtons()
{
    // 添加你自己的按钮
    AddButton("开始游戏", OnStartGameClicked, new Color(0.3f, 0.69f, 0.31f)); // 绿色
    AddButton("设置", OnSettingsClicked, new Color(0.13f, 0.59f, 0.95f)); // 蓝色
    AddButton("退出", OnExitClicked, new Color(0.96f, 0.26f, 0.21f)); // 红色
}

// 对应的事件处理方法
private void OnStartGameClicked()
{
    Debug.Log("开始游戏");
    HideModal();
    // 你的游戏启动逻辑
}

private void OnSettingsClicked()
{
    Debug.Log("打开设置");
    // 你的设置界面逻辑
}

private void OnExitClicked()
{
    Debug.Log("退出游戏");
    Application.Quit();
}
```

### 动态添加按钮

你也可以在运行时动态添加按钮：

```csharp
void Start()
{
    // 清除默认按钮
    uiController.ClearButtons();

    // 添加新按钮
    uiController.AddButton("选项 1", () => {
        Debug.Log("选项 1 被点击");
    });

    uiController.AddButton("选项 2", () => {
        Debug.Log("选项 2 被点击");
    }, Color.red); // 可选：自定义颜色
}
```

### 高级：从其他脚本控制 UI

```csharp
public class GameManager : MonoBehaviour
{
    private UIController uiController;

    void Start()
    {
        uiController = FindObjectOfType<UIController>();
    }

    public void ShowPauseMenu()
    {
        uiController.ClearButtons();
        uiController.AddButton("继续游戏", OnResume);
        uiController.AddButton("重新开始", OnRestart);
        uiController.AddButton("退出", OnQuit);
        uiController.ShowModal("暂停");
    }

    private void OnResume() { /* ... */ }
    private void OnRestart() { /* ... */ }
    private void OnQuit() { /* ... */ }
}
```

---

## 自定义样式

### 修改窗口颜色

在 `UIController.cs` 的 `CreateModalWindow()` 方法中修改：

```csharp
bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.95f); // 深灰色
```

### 修改标题栏颜色

在 `CreateTitle()` 方法中修改：

```csharp
bgImage.color = new Color(0.12f, 0.12f, 0.12f, 1f); // 更深的灰色
```

### 修改按钮默认颜色

在 `AddButton()` 方法中修改：

```csharp
Color normalColor = buttonColor ?? new Color(0.13f, 0.59f, 0.95f); // 默认蓝色
```

---

## 测试

### 在 Unity Editor 中测试

1. 进入 Play 模式
2. 如果安装了 XR Device Simulator：
   - 按住 Ctrl + 鼠标移动来模拟手柄射线
   - 左键点击按钮
3. 检查 Console 中的日志输出

### 在 PICO 设备上测试

1. Build 并安装到 PICO 设备
2. 手柄会显示射线
3. 将射线对准按钮
4. 按下 Trigger 触发点击

---

## 常见问题

### Q: UI 没有显示？
A: 检查 UIController 的 `showOnStart` 是否勾选。或手动调用 `uiController.ShowModal()`。

### Q: 射线无法点击按钮？
A: 确保：
1. 手柄上有 **XR Ray Interactor** 组件
2. XR Ray Interactor 的 **Enable Interaction with UI GameObjects** 已勾选
3. Canvas 上有 **TrackedDeviceGraphicRaycaster**（UIController 会自动添加）

### Q: 如何修改 UI 位置？
A: 在 Inspector 中修改 UIController 的 **UI Position** 参数。

### Q: 按钮点击没有反应？
A: 检查 Console 中是否有日志输出。如果有日志但没有执行你的逻辑，检查事件绑定是否正确。

### Q: 如何添加更多按钮？
A: 修改 `AddDefaultButtons()` 方法，或使用 `AddButton()` API 动态添加。

---

## API 参考

### 公共方法

| 方法 | 说明 |
|------|------|
| `ShowModal(string title)` | 显示模态窗口 |
| `HideModal()` | 隐藏模态窗口 |
| `ToggleModal()` | 切换窗口显示状态 |
| `AddButton(string text, UnityAction onClick, Color? color)` | 添加按钮 |
| `ClearButtons()` | 清除所有按钮 |

### 自定义方法（在 UIController.cs 中添加）

你可以根据需要添加更多方法来扩展功能。例如：

```csharp
// 添加内容文本
public void SetContent(string content) { /* ... */ }

// 添加关闭按钮
public void AddCloseButton() { /* ... */ }

// 添加输入框
public void AddInputField() { /* ... */ }
```

---

## 总结

使用 `UIController` 非常简单：

1. **在场景中添加一个空 GameObject，挂上 UIController 脚本**
2. **在手柄上配置 XR Ray Interactor**（如果还没有）
3. **运行游戏，UI 自动生成！**

所有 UI 元素都通过代码创建，完全不需要在场景中手动拖拽配置。
