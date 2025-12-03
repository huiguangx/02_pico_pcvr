# 网络震动指令使用指南

## 功能概述

通过 HTTP 接口从服务器接收震动指令，并自动触发 PICO 手柄震动。

## 快速开始

### 1. 添加组件到场景

1. 打开 Unity 场景（`Assets/Scenes/PicoXr.unity`）
2. 找到 **XR Origin** 或任意 GameObject
3. 添加 **Haptic Message Receiver** 组件
   - 菜单：`Add Component → Data Tracking → Haptic Message Receiver`

### 2. 配置参数（Inspector）

#### 服务器配置
```
Message Api Url: https://localhost:5000/msg
Poll Interval: 0.1  (每 0.1 秒轮询一次)
Enable Message Receiving: ✓  (启用消息接收)
```

#### 调试选项
```
Verbose Logging: ✓  (显示详细日志)
```

### 3. 运行测试

#### 方法 A：使用内置测试功能

1. 在 Inspector 中右键点击 `Haptic Message Receiver` 组件
2. 选择：
   - **测试：震动右手柄** - 测试右手震动
   - **测试：震动左手柄** - 测试左手震动
   - **测试：震动双手柄** - 测试双手震动

#### 方法 B：从服务器发送指令

确保服务器 `/msg` 接口返回正确格式的 JSON。

---

## 服务器 API 格式

### 接口地址
```
GET https://localhost:5000/msg
```

### 响应格式

#### 1. 震动指令

**右手震动：**
```json
{
  "id": "vibrate",
  "data": {
    "side": "right",
    "intensity": 0.8,
    "duration": 0.3
  }
}
```

**左手震动：**
```json
{
  "id": "vibrate",
  "data": {
    "side": "left",
    "intensity": 0.5,
    "duration": 0.2
  }
}
```

**双手震动：**
```json
{
  "id": "vibrate",
  "data": {
    "side": "both",
    "intensity": 1.0,
    "duration": 0.5
  }
}
```

#### 2. 无消息（空响应）

如果没有新消息，返回空字符串或空 JSON：
```json
{}
```

---

## 参数说明

### `side` - 震动方向
- `"left"` - 左手柄
- `"right"` - 右手柄
- `"both"` - 双手柄

### `intensity` - 震动强度
- 类型：`float`
- 范围：`0.0` - `1.0`
- 示例：
  - `0.3` - 轻微震动
  - `0.5` - 中等震动
  - `0.8` - 强烈震动
  - `1.0` - 最大震动

### `duration` - 持续时间（秒）
- 类型：`float`
- 范围：`0.01` - `10.0` 秒
- 示例：
  - `0.1` - 短促震动（100ms）
  - `0.3` - 标准震动（300ms）
  - `0.5` - 长震动（500ms）
  - `1.0` - 很长震动（1 秒）

---

## 后端实现示例

### Node.js / Express

```javascript
const express = require('express');
const app = express();

// 消息队列（存储待发送的震动指令）
let messageQueue = [];

// 客户端轮询消息
app.get('/msg', (req, res) => {
  if (messageQueue.length > 0) {
    const message = messageQueue.shift(); // 取出第一条消息
    res.json(message);
  } else {
    res.json({}); // 无消息
  }
});

// 添加震动指令（其他服务调用）
app.post('/addVibration', express.json(), (req, res) => {
  const { side, intensity, duration } = req.body;

  messageQueue.push({
    id: "vibrate",
    data: { side, intensity, duration }
  });

  res.json({ success: true });
});

app.listen(5000, () => {
  console.log('Server running on https://localhost:5000');
});
```

**触发震动：**
```bash
curl -X POST https://localhost:5000/addVibration \
  -H "Content-Type: application/json" \
  -d '{"side":"right","intensity":0.8,"duration":0.3}'
```

### Python / Flask

```python
from flask import Flask, jsonify, request

app = Flask(__name__)

# 消息队列
message_queue = []

@app.route('/msg', methods=['GET'])
def get_message():
    if message_queue:
        message = message_queue.pop(0)
        return jsonify(message)
    else:
        return jsonify({})

@app.route('/addVibration', methods=['POST'])
def add_vibration():
    data = request.json
    message_queue.append({
        "id": "vibrate",
        "data": {
            "side": data['side'],
            "intensity": data['intensity'],
            "duration": data['duration']
        }
    })
    return jsonify({"success": True})

if __name__ == '__main__':
    app.run(host='0.0.0.0', port=5000, ssl_context='adhoc')
```

**触发震动：**
```python
import requests

requests.post('https://localhost:5000/addVibration', json={
    "side": "right",
    "intensity": 0.8,
    "duration": 0.3
})
```

---

## 使用场景示例

### 场景 1：游戏事件触发震动

当玩家受到攻击时：
```json
{
  "id": "vibrate",
  "data": {
    "side": "both",
    "intensity": 0.9,
    "duration": 0.2
  }
}
```

### 场景 2：UI 交互反馈

用户按下按钮时：
```json
{
  "id": "vibrate",
  "data": {
    "side": "right",
    "intensity": 0.3,
    "duration": 0.05
  }
}
```

### 场景 3：距离感应震动

物体靠近时，逐渐增强震动：
```javascript
// 根据距离计算强度
const distance = 5.0; // 米
const intensity = Math.max(0, 1 - distance / 10);

messageQueue.push({
  id: "vibrate",
  data: {
    side: "both",
    intensity: intensity,
    duration: 0.1
  }
});
```

---

## 调试与监控

### 1. 启用详细日志

在 Inspector 中勾选 **Verbose Logging**，查看：
- 📨 收到的原始消息 JSON
- 📳 触发的震动参数
- ⚠️ 错误和警告信息

### 2. Console 输出示例

**成功接收震动指令：**
```
📨 收到消息: {"id":"vibrate","data":{"side":"right","intensity":0.8,"duration":0.3}}
📳 右手柄震动: 强度=0.80, 时长=0.30秒
✅ PCVR 震动已发送到: PICO 4 Controller - Right
```

**接收非震动消息：**
```
📬 收到其他消息: game_state_update
```

**服务器连接失败：**
```
⚠️ 消息接收失败: Connection refused
```

### 3. 网络监控

使用 Wireshark 或浏览器开发工具监控 HTTP 请求：
```
GET https://localhost:5000/msg
Response: {"id":"vibrate","data":{"side":"right","intensity":0.8,"duration":0.3}}
```

---

## 性能优化

### 轮询间隔建议

| 场景 | 推荐间隔 | 说明 |
|-----|---------|------|
| 实时游戏 | `0.05s` - `0.1s` | 低延迟，高响应 |
| 一般应用 | `0.2s` - `0.5s` | 平衡性能和响应 |
| 后台同步 | `1.0s` - `5.0s` | 省电，低频更新 |

### 网络优化

**使用长轮询（Long Polling）：**
服务器端等待新消息再返回，减少无效请求。

```python
@app.route('/msg', methods=['GET'])
def get_message_long_poll():
    timeout = 30  # 30 秒超时
    start_time = time.time()

    while time.time() - start_time < timeout:
        if message_queue:
            return jsonify(message_queue.pop(0))
        time.sleep(0.1)  # 避免 CPU 占用

    return jsonify({})  # 超时返回空
```

**使用 WebSocket（高级）：**
实时双向通信，零延迟。

---

## 常见问题

### Q1: 震动不工作？

**检查清单：**
- ✅ 确保 `Enable Message Receiving` 已勾选
- ✅ 检查服务器 URL 是否正确
- ✅ 查看 Console 是否有错误信息
- ✅ 测试使用 Context Menu 的测试功能
- ✅ 确保 Android 有 VIBRATE 权限（Native 模式）

### Q2: 消息延迟太高？

**解决方案：**
- 降低 `Poll Interval`（如 `0.05s`）
- 使用长轮询或 WebSocket
- 检查网络延迟（`ping` 服务器）

### Q3: PCVR 模式震动不工作？

**原因：**
- PCVR 模式震动支持有限，依赖于 Unity XR API
- 某些设备/驱动不支持

**解决方案：**
- 确保 PICO Connect 更新到最新版本
- 尝试在 Native APK 模式下测试

### Q4: JSON 解析失败？

**检查：**
- 确保服务器返回的是**有效的 JSON**
- 字段名区分大小写：`id`, `data`, `side`, `intensity`, `duration`
- 使用在线工具验证 JSON 格式（如 jsonlint.com）

---

## 扩展功能

### 支持更多消息类型

在 `ProcessMessage` 方法中添加：

```csharp
if (message.id == "vibrate")
{
    TriggerVibration(message.data);
}
else if (message.id == "play_sound")
{
    // 播放音效
    PlaySound(message.data.soundName);
}
else if (message.id == "show_notification")
{
    // 显示通知
    ShowNotification(message.data.text);
}
```

### 自定义震动频率

修改 `TriggerVibration` 方法，从服务器接收频率参数：

```csharp
// 在 VibrationData 中添加频率字段
[System.Serializable]
public class VibrationData
{
    public string side;
    public float intensity;
    public float duration;
    public int frequency = 200;  // 默认 200Hz
}

// 在触发震动时使用
PXR_Input.SendHapticImpulse(vibrateType, intensity, durationMs, data.frequency);
```

---

## 总结

### 实现步骤

1. ✅ 添加 `HapticMessageReceiver` 组件到场景
2. ✅ 配置服务器 URL
3. ✅ 实现后端 `/msg` 接口
4. ✅ 发送震动指令 JSON
5. ✅ 测试并调试

### 支持的运行模式

| 模式 | 震动支持 |
|-----|---------|
| Native APK | ✅ 完全支持 |
| PCVR Streaming | ✅ 基础支持 |
| Unity Editor | ❌ 不支持 |

### 关键文件

- **脚本**：`Assets/Scripts/HapticMessageReceiver.cs`
- **场景**：`Assets/Scenes/PicoXr.unity`
- **文档**：`NETWORK_HAPTIC_GUIDE.md`

---

**祝你开发顺利！** 🎮✨
