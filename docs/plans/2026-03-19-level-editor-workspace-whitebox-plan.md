# Level Editor Workspace/Whitebox Refactor Implementation Plan

> 状态：历史阶段计划。
>
> 当前关于地块类型、统一笔刷、默认禁用地图、出生点/目标点显式编号、覆盖规则、颜色规则、路径独立边界的有效定义，已转移到以下文档：
>
> - `docs/plans/2026-03-24-level-editor-workspace-productized-design.md`
> - `docs/plans/2026-03-24-level-editor-workspace-productized-plan.md`
>
> 后续开发不要再以本文档中的旧白模假设作为当前实现依据。

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** 将当前 Unity 关卡编辑器重构为“编辑器工作区驱动白模编辑，并最终导出 `<当前关卡名称>_LevelConfig`”的工作流。

**Architecture:** 以 `LevelEditorWorkspace` 作为编辑期真源，白模场景/根节点作为地图与交互工作面，`LevelConfig` 退化为导出产物。窗口负责关卡工作流与模块调度，不再要求用户先手动拖入 `LevelConfig` 才能开始编辑。

**Tech Stack:** Unity EditorWindow, ScriptableObject, Editor-only services, Serializable C# data model, EditMode tests via `-executeMethod`, existing whitebox tile authoring flow

---

### Task 1: 建立编辑器工作区数据模型

**Files:**
- Create: `Assets/Scripts/Editor/LevelEditor/Core/LevelEditorWorkspace.cs`
- Create: `Assets/Scripts/Editor/LevelEditor/Core/LevelRuntimeParameters.cs`
- Create: `Assets/Scripts/Editor/LevelEditor/Core/LevelEditorWorkspaceRepository.cs`
- Create: `Assets/Scripts/Editor/Tests/LevelEditorWorkspaceTests.cs`
- Modify: `Assets/Scripts/Editor/LevelEditor/Core/LevelEditorSession.cs`

**Step 1: Write the failing test**

```csharp
using NUnit.Framework;
using ArknightsLite.Editor.LevelEditor.Core;

namespace ArknightsLite.Editor.Tests.LevelEditor {
    public class LevelEditorWorkspaceTests {
        [Test]
        public void CreateNewWorkspace_UsesLevelNameAndInitializesRuntimeParameters() {
            var workspace = LevelEditorWorkspace.CreateNew("Tutorial_01");

            Assert.AreEqual("Tutorial_01", workspace.LevelName);
            Assert.AreEqual(10, workspace.MapWidth);
            Assert.AreEqual(10, workspace.MapDepth);
            Assert.AreEqual(20, workspace.Runtime.InitialDp);
            Assert.AreEqual(3, workspace.Runtime.BaseHealth);
        }
    }
}
```

**Step 2: Run test to verify it fails**

Run:

```bash
& 'C:\Program Files\Unity\Hub\Editor\2022.3.62f1\Editor\Unity.exe' -batchmode -projectPath 'd:\Program Files\MyProject\TDUandU' -executeMethod 'ArknightsLite.Editor.Tests.LevelEditor.LevelEditorWorkspaceTests.RunFromCommandLine' -logFile 'd:\Program Files\MyProject\TDUandU\Logs\workspace-test.log' -quit
```

Expected: FAIL because `LevelEditorWorkspace` does not exist yet.

**Step 3: Write minimal implementation**

Implement:

1. `LevelRuntimeParameters`
   - `InitialDp = 20`
   - `BaseHealth = 3`
   - `DpRecoveryInterval = 1f`
   - `DpRecoveryAmount = 1`
2. `LevelEditorWorkspace`
   - `LevelName`
   - `MapWidth = 10`
   - `MapDepth = 10`
   - `CellSize = 1f`
   - `DefaultTileType = TileType.Ground`
   - `Runtime`
   - lists for tiles, portals, waves, enemies, operators
   - static `CreateNew(string levelName)`
3. extend `LevelEditorSession`
   - add `CurrentWorkspace`
   - add `SetWorkspace(LevelEditorWorkspace workspace)`

**Step 4: Run test to verify it passes**

Run the same command as Step 2.

Expected: PASS with pass marker in log.

**Step 5: Commit**

```bash
git add Assets/Scripts/Editor/LevelEditor/Core/LevelEditorWorkspace.cs Assets/Scripts/Editor/LevelEditor/Core/LevelRuntimeParameters.cs Assets/Scripts/Editor/LevelEditor/Core/LevelEditorWorkspaceRepository.cs Assets/Scripts/Editor/LevelEditor/Core/LevelEditorSession.cs Assets/Scripts/Editor/Tests/LevelEditorWorkspaceTests.cs
git commit -m "feat: add level editor workspace model"
```

### Task 2: 建立白模生成与刷新服务

**Files:**
- Create: `Assets/Scripts/Editor/LevelEditor/Services/WhiteboxGenerationService.cs`
- Create: `Assets/Scripts/Editor/LevelEditor/Services/WhiteboxRoot.cs`
- Create: `Assets/Scripts/Editor/Tests/WhiteboxGenerationServiceTests.cs`
- Modify: `Assets/Scripts/Editor/LevelEditorWindow.cs`

**Step 1: Write the failing test**

```csharp
using NUnit.Framework;
using ArknightsLite.Editor.LevelEditor.Core;
using ArknightsLite.Editor.LevelEditor.Services;

namespace ArknightsLite.Editor.Tests.LevelEditor {
    public class WhiteboxGenerationServiceTests {
        [Test]
        public void EnsureWhitebox_CreatesExpectedTileCountFromWorkspaceSize() {
            var workspace = LevelEditorWorkspace.CreateNew("Tutorial_01");
            workspace.MapWidth = 4;
            workspace.MapDepth = 3;

            var result = WhiteboxGenerationService.BuildPreview(workspace);

            Assert.AreEqual(12, result.TileCount);
        }
    }
}
```

**Step 2: Run test to verify it fails**

Run:

```bash
& 'C:\Program Files\Unity\Hub\Editor\2022.3.62f1\Editor\Unity.exe' -batchmode -projectPath 'd:\Program Files\MyProject\TDUandU' -executeMethod 'ArknightsLite.Editor.Tests.LevelEditor.WhiteboxGenerationServiceTests.RunFromCommandLine' -logFile 'd:\Program Files\MyProject\TDUandU\Logs\whitebox-test.log' -quit
```

Expected: FAIL because `WhiteboxGenerationService` does not exist yet.

**Step 3: Write minimal implementation**

Implement:

1. `WhiteboxGenerationService.BuildPreview(LevelEditorWorkspace workspace)`
   - returns tile count metadata for tests
2. `WhiteboxGenerationService.GenerateIntoOpenScene(...)`
   - creates/refreshes a whitebox root
   - creates tile objects based on `MapWidth * MapDepth`
3. `WhiteboxRoot`
   - stores `LevelName`
   - stores whitebox dimensions
4. update `LevelEditorWindow`
   - add button: “生成/刷新白模”
   - use current workspace size instead of requiring `LevelConfig`

**Step 4: Run test to verify it passes**

Run the same command as Step 2.

Expected: PASS.

**Step 5: Commit**

```bash
git add Assets/Scripts/Editor/LevelEditor/Services/WhiteboxGenerationService.cs Assets/Scripts/Editor/LevelEditor/Services/WhiteboxRoot.cs Assets/Scripts/Editor/LevelEditorWindow.cs Assets/Scripts/Editor/Tests/WhiteboxGenerationServiceTests.cs
git commit -m "feat: add workspace-driven whitebox generation"
```

### Task 3: 让地图笔刷直接作用于工作区和白模

**Files:**
- Create: `Assets/Scripts/Editor/LevelEditor/Map/WorkspaceMapController.cs`
- Create: `Assets/Scripts/Editor/Tests/WorkspaceMapControllerTests.cs`
- Modify: `Assets/Scripts/Runtime/EditorTools/TileAuthoring.cs`
- Modify: `Assets/Scripts/Editor/TileAuthoringEditor.cs`
- Modify: `Assets/Scripts/Editor/LevelEditorWindow.cs`

**Step 1: Write the failing test**

```csharp
using NUnit.Framework;
using ArknightsLite.Editor.LevelEditor.Core;
using ArknightsLite.Editor.LevelEditor.Map;
using ArknightsLite.Model;

namespace ArknightsLite.Editor.Tests.LevelEditor {
    public class WorkspaceMapControllerTests {
        [Test]
        public void PaintTile_WritesOverrideIntoWorkspace() {
            var workspace = LevelEditorWorkspace.CreateNew("Tutorial_01");
            var controller = new WorkspaceMapController(workspace);

            controller.PaintTile(2, 1, TileType.HighGround, 1);

            var tile = workspace.GetTileOverride(2, 1);
            Assert.AreEqual(TileType.HighGround, tile.tileType);
            Assert.AreEqual(1, tile.heightLevel);
        }
    }
}
```

**Step 2: Run test to verify it fails**

Run:

```bash
& 'C:\Program Files\Unity\Hub\Editor\2022.3.62f1\Editor\Unity.exe' -batchmode -projectPath 'd:\Program Files\MyProject\TDUandU' -executeMethod 'ArknightsLite.Editor.Tests.LevelEditor.WorkspaceMapControllerTests.RunFromCommandLine' -logFile 'd:\Program Files\MyProject\TDUandU\Logs\workspace-map-test.log' -quit
```

Expected: FAIL because `WorkspaceMapController` and workspace tile APIs do not exist yet.

**Step 3: Write minimal implementation**

Implement:

1. workspace tile override API
   - `GetTileOverride`
   - `SetTileOverride`
2. `WorkspaceMapController`
   - applies brush result into workspace
   - requests visual refresh on touched whitebox tile
3. update `TileAuthoring`
   - stop treating `LevelConfig` as required source
   - support workspace-backed initialization
4. update `LevelEditorWindow`
   - route SceneView paint events into `WorkspaceMapController`

**Step 4: Run test to verify it passes**

Run the same command as Step 2.

Expected: PASS.

**Step 5: Commit**

```bash
git add Assets/Scripts/Editor/LevelEditor/Map/WorkspaceMapController.cs Assets/Scripts/Editor/Tests/WorkspaceMapControllerTests.cs Assets/Scripts/Runtime/EditorTools/TileAuthoring.cs Assets/Scripts/Editor/TileAuthoringEditor.cs Assets/Scripts/Editor/LevelEditorWindow.cs
git commit -m "refactor: drive whitebox painting from workspace"
```

### Task 4: 建立导出 `关卡名_LevelConfig` 的流水线

**Files:**
- Create: `Assets/Scripts/Editor/LevelEditor/Services/LevelConfigExportService.cs`
- Create: `Assets/Scripts/Editor/Tests/LevelConfigExportServiceTests.cs`
- Modify: `Assets/Scripts/Config/LevelConfig.cs`
- Modify: `Assets/Scripts/Editor/LevelEditorWindow.cs`

**Step 1: Write the failing test**

```csharp
using NUnit.Framework;
using ArknightsLite.Editor.LevelEditor.Core;
using ArknightsLite.Editor.LevelEditor.Services;

namespace ArknightsLite.Editor.Tests.LevelEditor {
    public class LevelConfigExportServiceTests {
        [Test]
        public void BuildAssetName_UsesLevelNameSuffixLevelConfig() {
            var workspace = LevelEditorWorkspace.CreateNew("Tutorial_01");

            string assetName = LevelConfigExportService.BuildAssetName(workspace);

            Assert.AreEqual("Tutorial_01_LevelConfig", assetName);
        }
    }
}
```

**Step 2: Run test to verify it fails**

Run:

```bash
& 'C:\Program Files\Unity\Hub\Editor\2022.3.62f1\Editor\Unity.exe' -batchmode -projectPath 'd:\Program Files\MyProject\TDUandU' -executeMethod 'ArknightsLite.Editor.Tests.LevelEditor.LevelConfigExportServiceTests.RunFromCommandLine' -logFile 'd:\Program Files\MyProject\TDUandU\Logs\export-test.log' -quit
```

Expected: FAIL because `LevelConfigExportService` does not exist yet.

**Step 3: Write minimal implementation**

Implement:

1. `BuildAssetName(LevelEditorWorkspace workspace)`
   - returns `<LevelName>_LevelConfig`
2. `Export(LevelEditorWorkspace workspace)`
   - create/update `Assets/Resources/Levels/Configs/<LevelName>_LevelConfig.asset`
   - copy workspace map data into `LevelConfig`
   - copy runtime parameters into `LevelConfig`
   - copy portals/waves/enemies/operators into `LevelConfig`
3. add export button into window
   - “导出 LevelConfig”

**Step 4: Run test to verify it passes**

Run the same command as Step 2.

Expected: PASS.

**Step 5: Commit**

```bash
git add Assets/Scripts/Editor/LevelEditor/Services/LevelConfigExportService.cs Assets/Scripts/Editor/Tests/LevelConfigExportServiceTests.cs Assets/Scripts/Config/LevelConfig.cs Assets/Scripts/Editor/LevelEditorWindow.cs
git commit -m "feat: export workspace into named level config asset"
```

### Task 5: 将全局关卡参数移入编辑器工作流

**Files:**
- Create: `Assets/Scripts/Editor/LevelEditor/Panels/LevelRuntimePanel.cs`
- Create: `Assets/Scripts/Editor/Tests/LevelRuntimePanelDataTests.cs`
- Modify: `Assets/Scripts/Editor/LevelEditorWindow.cs`
- Modify: `Assets/Scripts/Editor/LevelEditor/Core/LevelEditorWorkspace.cs`

**Step 1: Write the failing test**

```csharp
using NUnit.Framework;
using ArknightsLite.Editor.LevelEditor.Core;

namespace ArknightsLite.Editor.Tests.LevelEditor {
    public class LevelRuntimePanelDataTests {
        [Test]
        public void Workspace_AllowsEditingRuntimeParameters() {
            var workspace = LevelEditorWorkspace.CreateNew("Tutorial_01");

            workspace.Runtime.InitialDp = 35;
            workspace.Runtime.BaseHealth = 5;
            workspace.Runtime.DpRecoveryInterval = 0.5f;
            workspace.Runtime.DpRecoveryAmount = 2;

            Assert.AreEqual(35, workspace.Runtime.InitialDp);
            Assert.AreEqual(5, workspace.Runtime.BaseHealth);
            Assert.AreEqual(0.5f, workspace.Runtime.DpRecoveryInterval);
            Assert.AreEqual(2, workspace.Runtime.DpRecoveryAmount);
        }
    }
}
```

**Step 2: Run test to verify it fails**

Run:

```bash
& 'C:\Program Files\Unity\Hub\Editor\2022.3.62f1\Editor\Unity.exe' -batchmode -projectPath 'd:\Program Files\MyProject\TDUandU' -executeMethod 'ArknightsLite.Editor.Tests.LevelEditor.LevelRuntimePanelDataTests.RunFromCommandLine' -logFile 'd:\Program Files\MyProject\TDUandU\Logs\runtime-panel-test.log' -quit
```

Expected: FAIL if runtime parameters are not fully surfaced in workspace/panel flow.

**Step 3: Write minimal implementation**

Implement:

1. `LevelRuntimePanel`
   - edit `InitialDp`
   - edit `BaseHealth`
   - edit `DpRecoveryInterval`
   - edit `DpRecoveryAmount`
2. integrate into `LevelEditorWindow`
   - display before whitebox-specific tools
3. ensure export service writes these fields into `LevelConfig`

**Step 4: Run test to verify it passes**

Run the same command as Step 2.

Expected: PASS.

**Step 5: Commit**

```bash
git add Assets/Scripts/Editor/LevelEditor/Panels/LevelRuntimePanel.cs Assets/Scripts/Editor/Tests/LevelRuntimePanelDataTests.cs Assets/Scripts/Editor/LevelEditorWindow.cs Assets/Scripts/Editor/LevelEditor/Core/LevelEditorWorkspace.cs
git commit -m "feat: edit runtime level parameters from workspace"
```

### Task 6: 逐步补齐传送门、路径、波次模块并接入导出

**Files:**
- Modify: `Assets/Scripts/Editor/LevelEditor/Panels/PortalEditorPanel.cs`
- Modify: `Assets/Scripts/Editor/LevelEditor/Panels/WaveEditorPanel.cs`
- Modify: `Assets/Scripts/Editor/LevelEditor/Panels/PathEditorPanel.cs`
- Modify: `Assets/Scripts/Editor/LevelEditor/Services/PathAutoFillService.cs`
- Modify: `Assets/Scripts/Editor/LevelEditor/Services/LevelConfigExportService.cs`
- Create: `Assets/Scripts/Editor/Tests/WorkspaceWaveExportTests.cs`

**Step 1: Write the failing test**

```csharp
using NUnit.Framework;
using ArknightsLite.Editor.LevelEditor.Core;
using ArknightsLite.Editor.LevelEditor.Services;

namespace ArknightsLite.Editor.Tests.LevelEditor {
    public class WorkspaceWaveExportTests {
        [Test]
        public void Export_CopiesWavePathAndReferencesIntoLevelConfig() {
            var workspace = LevelEditorWorkspace.CreateNew("Tutorial_01");
            workspace.Waves.Add(LevelEditorWorkspace.CreateDefaultWave("wave_01"));
            workspace.Waves[0].enemyId = "enemy_01";
            workspace.Waves[0].path.Add(new ArknightsLite.Config.PathNodeDefinition { x = 1, y = 2, wait = 0f });

            var config = LevelConfigExportService.BuildTransientConfig(workspace);

            Assert.AreEqual(1, config.waves.Count);
            Assert.AreEqual("enemy_01", config.waves[0].enemyId);
            Assert.AreEqual(1, config.waves[0].path.Count);
        }
    }
}
```

**Step 2: Run test to verify it fails**

Run:

```bash
& 'C:\Program Files\Unity\Hub\Editor\2022.3.62f1\Editor\Unity.exe' -batchmode -projectPath 'd:\Program Files\MyProject\TDUandU' -executeMethod 'ArknightsLite.Editor.Tests.LevelEditor.WorkspaceWaveExportTests.RunFromCommandLine' -logFile 'd:\Program Files\MyProject\TDUandU\Logs\workspace-wave-export-test.log' -quit
```

Expected: FAIL until workspace/export pipeline fully carries wave data.

**Step 3: Write minimal implementation**

Implement:

1. workspace-facing portal/path/wave editing
2. path auto-fill against whitebox passability
3. export of portals/waves/path data into `LevelConfig`
4. keep panel code isolated from window shell

**Step 4: Run test to verify it passes**

Run the same command as Step 2.

Expected: PASS.

**Step 5: Commit**

```bash
git add Assets/Scripts/Editor/LevelEditor/Panels/PortalEditorPanel.cs Assets/Scripts/Editor/LevelEditor/Panels/WaveEditorPanel.cs Assets/Scripts/Editor/LevelEditor/Panels/PathEditorPanel.cs Assets/Scripts/Editor/LevelEditor/Services/PathAutoFillService.cs Assets/Scripts/Editor/LevelEditor/Services/LevelConfigExportService.cs Assets/Scripts/Editor/Tests/WorkspaceWaveExportTests.cs
git commit -m "feat: connect workspace wave modules to export pipeline"
```
