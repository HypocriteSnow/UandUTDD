# 基础架构层 - 03. 配置管理系统 (ConfigManager)

## 1. 系统概述
游戏中的核心数据（干员属性、关卡地图、敌人波次）必须与代码分离，实现**数据驱动**。`ConfigManager` 负责加载、解析和提供这些数据。

## 2. 核心职责
1. **统一加载**：屏蔽底层加载方式（Resources / AssetBundle / Addressables）。
2. **数据解析**：支持 JSON 反序列化为 C# 对象，或直接读取 ScriptableObject。
3. **缓存管理**：避免重复读取磁盘文件。

## 3. 接口规范

### 3.1 数据结构
推荐使用 `ScriptableObject` 作为 Unity 编辑器内的数据容器，或纯 C# 类配合 JSON。

```csharp
// 示例：干员配置
[System.Serializable]
public class OperatorConfig {
    public string ID;
    public string Name;
    public int BaseHP;
    public int BaseAtk;
    public int Cost;
}
```

### 3.2 ConfigManager API
```csharp
public class ConfigManager : MonoSingleton<ConfigManager> {
    // 加载配置（泛型）
    public T LoadConfig<T>(string path) where T : class;
    
    // 获取已缓存的配置
    public T GetConfig<T>(string id);
    
    // 预加载列表
    public void PreloadConfigs(List<string> paths);
}
```

## 4. 开发注意事项
- **只读原则**：运行时获取的配置对象应当是只读的，严禁在业务逻辑中修改配置数据。
- **异步预留**：虽然 MVP 阶段可以使用同步加载（`Resources.Load`），但接口设计应考虑未来切换为异步加载（`Addressables`）的可能性。
- **路径规范**：统一配置文件存放路径，如 `Resources/Configs/Operators/`。
