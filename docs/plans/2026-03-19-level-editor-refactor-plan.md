# Unity 关卡编辑器重构 Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** 将当前基于网格笔刷的 Unity 关卡编辑器重构为一套以统一关卡数据为核心、可覆盖网页编辑器核心能力的模块化编辑器。

**Architecture:** 继续以 `LevelConfig` 作为关卡根资产，但将其扩展为统一关卡定义；`LevelEditorWindow` 只保留壳层职责，地图、波次路径、模板、校验、测试入口分别拆到独立模块。编辑器负责生产数据和校验，测试时直接把当前关卡交给 Unity Runtime。

**Tech Stack:** Unity EditorWindow, ScriptableObject, Serializable C# data model, EditMode tests, existing `GameMain` / `GridRenderer` runtime entry

---

### Task 1: 扩展统一关卡数据模型

**Files:**
- Create: `Assets/Scripts/Config/PortalDefinition.cs`
- Create: `Assets/Scripts/Config/PathNodeDefinition.cs`
- Create: `Assets/Scripts/Config/WaveDefinition.cs`
- Create: `Assets/Scripts/Config/EnemyTemplateData.cs`
- Create: `Assets/Scripts/Config/OperatorTemplateData.cs`
- Create: `Assets/Tests/EditMode/ArknightsLite.Editor.Tests.asmdef`
- Create: `Assets/Tests/EditMode/LevelEditor/LevelConfigSchemaTests.cs`
- Modify: `Assets/Scripts/Config/LevelConfig.cs`

**Step 1: Write the failing test**

```csharp
using NUnit.Framework;
using UnityEngine;
using ArknightsLite.Config;

namespace ArknightsLite.Editor.Tests.LevelEditor {
    public class LevelConfigSchemaTests {
        [Test]
        public void LevelConfig_NewCollections_AreInitialized() {
            var config = ScriptableObject.CreateInstance<LevelConfig>();

            Assert.NotNull(config.portals);
            Assert.NotNull(config.waves);
            Assert.NotNull(config.enemies);
            Assert.NotNull(config.operators);
        }
    }
}
```

**Step 2: Run test to verify it fails**

Run:

```bash
"<UNITY_EDITOR_EXE>" -batchmode -projectPath "d:\Program Files\MyProject\TDUandU" -runTests -testPlatform EditMode -testResults "Logs\EditModeTests.xml" -testFilter "ArknightsLite.Editor.Tests.LevelEditor.LevelConfigSchemaTests" -quit
```

Expected: FAIL because `LevelConfig` does not yet define `portals`, `waves`, `enemies`, `operators`.

**Step 3: Write minimal implementation**

```csharp
[Serializable]
public class PortalDefinition {
    public string id;
    public Vector2Int inPos;
    public Vector2Int outPos;
    public float delay;
    public string color;
}

[Serializable]
public class PathNodeDefinition {
    public int x;
    public int y;
    public float wait;
}

[Serializable]
public class WaveDefinition {
    public string waveId;
    public float time;
    public string enemyId;
    public int count;
    public float interval;
    public string spawnId;
    public string targetId;
    public List<PathNodeDefinition> path = new();
}
```

并在 `LevelConfig` 中增加：

```csharp
public List<PortalDefinition> portals = new();
public List<WaveDefinition> waves = new();
public List<EnemyTemplateData> enemies = new();
public List<OperatorTemplateData> operators = new();
```

**Step 4: Run test to verify it passes**

Run the same command as Step 2.  
Expected: PASS.

**Step 5: Commit**

```bash
git add Assets/Scripts/Config/LevelConfig.cs Assets/Scripts/Config/PortalDefinition.cs Assets/Scripts/Config/PathNodeDefinition.cs Assets/Scripts/Config/WaveDefinition.cs Assets/Scripts/Config/EnemyTemplateData.cs Assets/Scripts/Config/OperatorTemplateData.cs Assets/Tests/EditMode/ArknightsLite.Editor.Tests.asmdef Assets/Tests/EditMode/LevelEditor/LevelConfigSchemaTests.cs
git commit -m "feat: extend level config into unified level schema"
```

### Task 2: 建立编辑器校验服务

**Files:**
- Create: `Assets/Scripts/Editor/LevelEditor/Services/LevelValidationResult.cs`
- Create: `Assets/Scripts/Editor/LevelEditor/Services/LevelValidationService.cs`
- Create: `Assets/Tests/EditMode/LevelEditor/LevelValidationServiceTests.cs`
- Modify: `Assets/Scripts/Config/LevelConfig.cs`

**Step 1: Write the failing test**

```csharp
using NUnit.Framework;
using UnityEngine;
using ArknightsLite.Config;
using ArknightsLite.Editor.LevelEditor.Services;

namespace ArknightsLite.Editor.Tests.LevelEditor {
    public class LevelValidationServiceTests {
        [Test]
        public void Validate_Fails_WhenWaveReferencesUnknownEnemy() {
            var config = ScriptableObject.CreateInstance<LevelConfig>();
            config.waves.Add(new WaveDefinition { waveId = "w1", enemyId = "missing_enemy" });

            var result = LevelValidationService.Validate(config);

            Assert.IsFalse(result.IsValid);
            Assert.That(result.Errors, Has.Some.Contains("missing_enemy"));
        }
    }
}
```

**Step 2: Run test to verify it fails**

Run:

```bash
"<UNITY_EDITOR_EXE>" -batchmode -projectPath "d:\Program Files\MyProject\TDUandU" -runTests -testPlatform EditMode -testResults "Logs\EditModeTests.xml" -testFilter "ArknightsLite.Editor.Tests.LevelEditor.LevelValidationServiceTests" -quit
```

Expected: FAIL because `LevelValidationService` does not exist yet.

**Step 3: Write minimal implementation**

```csharp
public sealed class LevelValidationResult {
    public bool IsValid => Errors.Count == 0;
    public readonly List<string> Errors = new();
}

public static class LevelValidationService {
    public static LevelValidationResult Validate(LevelConfig config) {
        var result = new LevelValidationResult();
        var enemyIds = new HashSet<string>(config.enemies.Select(e => e.id));

        foreach (var wave in config.waves) {
            if (!string.IsNullOrEmpty(wave.enemyId) && !enemyIds.Contains(wave.enemyId)) {
                result.Errors.Add($"Wave {wave.waveId} references missing enemy '{wave.enemyId}'.");
            }
        }

        return result;
    }
}
```

随后逐步补齐：

1. 起点 / 终点存在性校验
2. 传送门绑定完整性校验
3. 路径起终点与地图一致性校验
4. 模板引用校验

**Step 4: Run test to verify it passes**

Run the same command as Step 2.  
Expected: PASS.

**Step 5: Commit**

```bash
git add Assets/Scripts/Editor/LevelEditor/Services/LevelValidationResult.cs Assets/Scripts/Editor/LevelEditor/Services/LevelValidationService.cs Assets/Tests/EditMode/LevelEditor/LevelValidationServiceTests.cs Assets/Scripts/Config/LevelConfig.cs
git commit -m "feat: add level editor validation service"
```

### Task 3: 将 LevelEditorWindow 拆成壳层和会话层

**Files:**
- Create: `Assets/Scripts/Editor/LevelEditor/Core/LevelEditorMode.cs`
- Create: `Assets/Scripts/Editor/LevelEditor/Core/LevelEditorSession.cs`
- Create: `Assets/Scripts/Editor/LevelEditor/Services/LevelSceneBuilder.cs`
- Create: `Assets/Scripts/Editor/LevelEditor/Services/LevelSceneLoader.cs`
- Create: `Assets/Tests/EditMode/LevelEditor/LevelEditorSessionTests.cs`
- Modify: `Assets/Scripts/Editor/LevelEditorWindow.cs`

**Step 1: Write the failing test**

```csharp
using NUnit.Framework;
using ArknightsLite.Editor.LevelEditor.Core;

namespace ArknightsLite.Editor.Tests.LevelEditor {
    public class LevelEditorSessionTests {
        [Test]
        public void StartEditing_SetsCurrentModeToMap() {
            var session = new LevelEditorSession();

            session.StartEditing();

            Assert.AreEqual(LevelEditorMode.Map, session.Mode);
        }
    }
}
```

**Step 2: Run test to verify it fails**

Run:

```bash
"<UNITY_EDITOR_EXE>" -batchmode -projectPath "d:\Program Files\MyProject\TDUandU" -runTests -testPlatform EditMode -testResults "Logs\EditModeTests.xml" -testFilter "ArknightsLite.Editor.Tests.LevelEditor.LevelEditorSessionTests" -quit
```

Expected: FAIL because session types do not exist yet.

**Step 3: Write minimal implementation**

```csharp
public enum LevelEditorMode {
    Map,
    Waves,
    Operators,
    Enemies,
    Json
}

public sealed class LevelEditorSession {
    public LevelEditorMode Mode { get; private set; }
    public LevelConfig CurrentLevel { get; private set; }

    public void StartEditing(LevelConfig config = null) {
        CurrentLevel = config;
        Mode = LevelEditorMode.Map;
    }
}
```

然后重写 `LevelEditorWindow`：

1. 只保留窗口入口、Tab 切换、会话调度
2. 将 `BuildScene()` 提取到 `LevelSceneBuilder`
3. 将 `LoadFromScene()` 提取到 `LevelSceneLoader`
4. 删除窗口中对场景细节和反射流程的直接堆叠

**Step 4: Run test to verify it passes**

Run the same command as Step 2.  
Expected: PASS.

**Step 5: Commit**

```bash
git add Assets/Scripts/Editor/LevelEditor/Core/LevelEditorMode.cs Assets/Scripts/Editor/LevelEditor/Core/LevelEditorSession.cs Assets/Scripts/Editor/LevelEditor/Services/LevelSceneBuilder.cs Assets/Scripts/Editor/LevelEditor/Services/LevelSceneLoader.cs Assets/Scripts/Editor/LevelEditorWindow.cs Assets/Tests/EditMode/LevelEditor/LevelEditorSessionTests.cs
git commit -m "refactor: split level editor shell from scene services"
```

### Task 4: 抽离地图编辑与场景交互控制器

**Files:**
- Create: `Assets/Scripts/Editor/LevelEditor/Map/MapCanvasController.cs`
- Create: `Assets/Scripts/Editor/LevelEditor/Map/TilePaintCommand.cs`
- Create: `Assets/Tests/EditMode/LevelEditor/MapCanvasControllerTests.cs`
- Modify: `Assets/Scripts/Runtime/EditorTools/TileAuthoring.cs`
- Modify: `Assets/Scripts/Editor/TileAuthoringEditor.cs`
- Modify: `Assets/Scripts/Editor/LevelEditorWindow.cs`

**Step 1: Write the failing test**

```csharp
using NUnit.Framework;
using UnityEngine;
using ArknightsLite.Config;
using ArknightsLite.Editor.LevelEditor.Map;
using ArknightsLite.Model;

namespace ArknightsLite.Editor.Tests.LevelEditor {
    public class MapCanvasControllerTests {
        [Test]
        public void PaintTile_WritesTileTypeIntoLevelConfig() {
            var config = ScriptableObject.CreateInstance<LevelConfig>();
            var controller = new MapCanvasController(config);

            controller.PaintTile(2, 3, TileType.HighGround, 1);

            var tile = config.GetTileData(2, 3);
            Assert.AreEqual(TileType.HighGround, tile.tileType);
            Assert.AreEqual(1, tile.heightLevel);
        }
    }
}
```

**Step 2: Run test to verify it fails**

Run:

```bash
"<UNITY_EDITOR_EXE>" -batchmode -projectPath "d:\Program Files\MyProject\TDUandU" -runTests -testPlatform EditMode -testResults "Logs\EditModeTests.xml" -testFilter "ArknightsLite.Editor.Tests.LevelEditor.MapCanvasControllerTests" -quit
```

Expected: FAIL because `MapCanvasController` does not exist yet.

**Step 3: Write minimal implementation**

```csharp
public sealed class MapCanvasController {
    private readonly LevelConfig _config;

    public MapCanvasController(LevelConfig config) {
        _config = config;
    }

    public void PaintTile(int x, int z, TileType tileType, int heightLevel) {
        _config.SetTileData(x, z, new TileData {
            x = x,
            z = z,
            tileType = tileType,
            heightLevel = heightLevel,
            walkable = tileType != TileType.Forbidden && tileType != TileType.Hole,
            deployTag = "All"
        });
    }
}
```

随后：

1. 让 `LevelEditorWindow` 把 SceneView 事件转发给控制器
2. 让 `TileAuthoring` 只负责可视桥接，不再承担过多同步策略
3. 让 `TileAuthoringEditor` 专注单格属性编辑

**Step 4: Run test to verify it passes**

Run the same command as Step 2.  
Expected: PASS.

**Step 5: Commit**

```bash
git add Assets/Scripts/Editor/LevelEditor/Map/MapCanvasController.cs Assets/Scripts/Editor/LevelEditor/Map/TilePaintCommand.cs Assets/Scripts/Runtime/EditorTools/TileAuthoring.cs Assets/Scripts/Editor/TileAuthoringEditor.cs Assets/Scripts/Editor/LevelEditorWindow.cs Assets/Tests/EditMode/LevelEditor/MapCanvasControllerTests.cs
git commit -m "refactor: extract map canvas controller from level editor window"
```

### Task 5: 增加传送门、波次与路径编辑模块

**Files:**
- Create: `Assets/Scripts/Editor/LevelEditor/Panels/PortalEditorPanel.cs`
- Create: `Assets/Scripts/Editor/LevelEditor/Panels/WaveEditorPanel.cs`
- Create: `Assets/Scripts/Editor/LevelEditor/Panels/PathEditorPanel.cs`
- Create: `Assets/Scripts/Editor/LevelEditor/Services/PathAutoFillService.cs`
- Create: `Assets/Tests/EditMode/LevelEditor/PathAutoFillServiceTests.cs`
- Modify: `Assets/Scripts/Editor/LevelEditorWindow.cs`
- Modify: `Assets/Scripts/Config/LevelConfig.cs`

**Step 1: Write the failing test**

```csharp
using NUnit.Framework;
using UnityEngine;
using ArknightsLite.Config;
using ArknightsLite.Editor.LevelEditor.Services;

namespace ArknightsLite.Editor.Tests.LevelEditor {
    public class PathAutoFillServiceTests {
        [Test]
        public void BuildPath_ReturnsNonEmptyPath_WhenSpawnAndGoalAreConnected() {
            var config = ScriptableObject.CreateInstance<LevelConfig>();
            config.mapWidth = 5;
            config.mapDepth = 5;
            config.spawnPoints.Add(new Vector2Int(0, 0));
            config.goalPoint = new Vector2Int(4, 4);

            var path = PathAutoFillService.BuildPath(config, config.spawnPoints[0], config.goalPoint);

            Assert.That(path.Count, Is.GreaterThan(0));
        }
    }
}
```

**Step 2: Run test to verify it fails**

Run:

```bash
"<UNITY_EDITOR_EXE>" -batchmode -projectPath "d:\Program Files\MyProject\TDUandU" -runTests -testPlatform EditMode -testResults "Logs\EditModeTests.xml" -testFilter "ArknightsLite.Editor.Tests.LevelEditor.PathAutoFillServiceTests" -quit
```

Expected: FAIL because `PathAutoFillService` does not exist yet.

**Step 3: Write minimal implementation**

```csharp
public static class PathAutoFillService {
    public static List<PathNodeDefinition> BuildPath(LevelConfig config, Vector2Int spawn, Vector2Int goal) {
        var path = new List<PathNodeDefinition>();
        var current = spawn;

        while (current.x != goal.x || current.y != goal.y) {
            if (current.x < goal.x) current.x++;
            else if (current.y < goal.y) current.y++;

            path.Add(new PathNodeDefinition { x = current.x, y = current.y, wait = 0f });
        }

        return path;
    }
}
```

然后逐步补齐：

1. 传送门入口 / 出口配置 UI
2. 波次列表与敌人引用 UI
3. 路径手动编辑与等待时间编辑
4. 自动寻路与地图连通校验

**Step 4: Run test to verify it passes**

Run the same command as Step 2.  
Expected: PASS.

**Step 5: Commit**

```bash
git add Assets/Scripts/Editor/LevelEditor/Panels/PortalEditorPanel.cs Assets/Scripts/Editor/LevelEditor/Panels/WaveEditorPanel.cs Assets/Scripts/Editor/LevelEditor/Panels/PathEditorPanel.cs Assets/Scripts/Editor/LevelEditor/Services/PathAutoFillService.cs Assets/Scripts/Editor/LevelEditorWindow.cs Assets/Scripts/Config/LevelConfig.cs Assets/Tests/EditMode/LevelEditor/PathAutoFillServiceTests.cs
git commit -m "feat: add portal, wave, and path editor modules"
```

### Task 6: 增加模板编辑、JSON 调试与测试入口

**Files:**
- Create: `Assets/Scripts/Editor/LevelEditor/Panels/EnemyCatalogPanel.cs`
- Create: `Assets/Scripts/Editor/LevelEditor/Panels/OperatorCatalogPanel.cs`
- Create: `Assets/Scripts/Editor/LevelEditor/Panels/JsonDebugPanel.cs`
- Create: `Assets/Scripts/Editor/LevelEditor/Services/LevelJsonImportExportService.cs`
- Create: `Assets/Scripts/Editor/LevelEditor/Services/PlayEntryService.cs`
- Create: `Assets/Tests/EditMode/LevelEditor/LevelJsonImportExportServiceTests.cs`
- Modify: `Assets/Scripts/Editor/LevelEditorWindow.cs`

**Step 1: Write the failing test**

```csharp
using NUnit.Framework;
using UnityEngine;
using ArknightsLite.Config;
using ArknightsLite.Editor.LevelEditor.Services;

namespace ArknightsLite.Editor.Tests.LevelEditor {
    public class LevelJsonImportExportServiceTests {
        [Test]
        public void ExportAndImport_RetainsWaveAndTemplateCounts() {
            var config = ScriptableObject.CreateInstance<LevelConfig>();
            config.waves.Add(new WaveDefinition { waveId = "w1" });
            config.enemies.Add(new EnemyTemplateData { id = "enemy_1" });

            string json = LevelJsonImportExportService.Export(config);
            var imported = ScriptableObject.CreateInstance<LevelConfig>();
            LevelJsonImportExportService.ImportInto(imported, json);

            Assert.AreEqual(1, imported.waves.Count);
            Assert.AreEqual(1, imported.enemies.Count);
        }
    }
}
```

**Step 2: Run test to verify it fails**

Run:

```bash
"<UNITY_EDITOR_EXE>" -batchmode -projectPath "d:\Program Files\MyProject\TDUandU" -runTests -testPlatform EditMode -testResults "Logs\EditModeTests.xml" -testFilter "ArknightsLite.Editor.Tests.LevelEditor.LevelJsonImportExportServiceTests" -quit
```

Expected: FAIL because `LevelJsonImportExportService` does not exist yet.

**Step 3: Write minimal implementation**

```csharp
public static class LevelJsonImportExportService {
    public static string Export(LevelConfig config) {
        return JsonUtility.ToJson(config, true);
    }

    public static void ImportInto(LevelConfig target, string json) {
        JsonUtility.FromJsonOverwrite(json, target);
    }
}
```

然后补齐：

1. 敌人模板与干员模板编辑面板
2. JSON 文本视图与导入导出按钮
3. `PlayEntryService`，负责保存当前关卡并触发 `EditorApplication.isPlaying = true`
4. Runtime 读取当前关卡的桥接逻辑

**Step 4: Run test to verify it passes**

Run the same command as Step 2.  
Expected: PASS.

**Step 5: Commit**

```bash
git add Assets/Scripts/Editor/LevelEditor/Panels/EnemyCatalogPanel.cs Assets/Scripts/Editor/LevelEditor/Panels/OperatorCatalogPanel.cs Assets/Scripts/Editor/LevelEditor/Panels/JsonDebugPanel.cs Assets/Scripts/Editor/LevelEditor/Services/LevelJsonImportExportService.cs Assets/Scripts/Editor/LevelEditor/Services/PlayEntryService.cs Assets/Scripts/Editor/LevelEditorWindow.cs Assets/Tests/EditMode/LevelEditor/LevelJsonImportExportServiceTests.cs
git commit -m "feat: add template panels, json debug, and play entry"
```
