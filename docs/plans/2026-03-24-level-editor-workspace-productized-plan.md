# Level Editor Workspace Terrain/Semantic Productization Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** 将 Unity 关卡编辑器的地块、笔刷、出生点/目标点、传送点、颜色与导出规则统一到最新产品化设计，并移除 legacy fallback。

**Architecture:** 保持 `LevelEditorWorkspace` 为编辑期真源，白模为唯一地图编辑载体。每个格子同一时刻只保留一种主类型，路径保持独立模块，不并入地块类型体系。本轮重点是地形与语义点的一体化交互，而不是路径系统重做。

**Tech Stack:** Unity EditorWindow, SceneView whitebox authoring, ScriptableObject persistence, Editor-only services, EditMode tests via `-executeMethod`

---

### Task 1: 用测试锁定新的地图主类型规则

**Files:**
- Modify: `Assets/Scripts/Editor/Tests/LevelEditorWorkspaceTests.cs`
- Modify: `Assets/Scripts/Editor/LevelEditor/Core/LevelEditorWorkspace.cs`

**Step 1: Write the failing test**

```csharp
[Test]
public void CreateNewWorkspace_DefaultsMapToForbiddenTerrain() {
    var workspace = LevelEditorWorkspace.CreateNew("Tutorial_01");

    Assert.AreEqual(TileType.Forbidden, workspace.DefaultTileType);
    Assert.IsTrue(workspace.GetTileOverride(0, 0).tileType == TileType.Forbidden);
}
```

**Step 2: Run test to verify it fails**

Run:

```bash
& 'C:\Program Files\Unity\Hub\Editor\2022.3.62f1\Editor\Unity.exe' -batchmode -projectPath 'd:\Program Files\MyProject\TDUandU' -executeMethod 'ArknightsLite.Editor.Tests.LevelEditor.LevelEditorWorkspaceTests.RunFromCommandLine' -logFile 'd:\Program Files\MyProject\TDUandU\Logs\workspace-default-terrain.log' -quit
```

Expected: FAIL because the workspace still defaults to ground or still depends on old tile assumptions.

**Step 3: Write minimal implementation**

Implement:

1. set `LevelEditorWorkspace.DefaultTileType = TileType.Forbidden`
2. ensure new workspaces initialize all empty tiles as forbidden
3. keep runtime parameters and naming defaults unchanged

**Step 4: Run test to verify it passes**

Run the same command as Step 2.

Expected: PASS.

**Step 5: Commit**

```bash
git add Assets/Scripts/Editor/Tests/LevelEditorWorkspaceTests.cs Assets/Scripts/Editor/LevelEditor/Core/LevelEditorWorkspace.cs
git commit -m "feat: default new workspace tiles to forbidden"
```

### Task 2: 用测试锁定“后刷覆盖前刷”的主类型规则

**Files:**
- Modify: `Assets/Scripts/Editor/Tests/WorkspaceMapControllerTests.cs`
- Modify: `Assets/Scripts/Editor/LevelEditor/Map/WorkspaceMapController.cs`
- Modify: `Assets/Scripts/Editor/LevelEditor/Core/LevelEditorWorkspace.cs`

**Step 1: Write the failing test**

```csharp
[Test]
public void PaintGround_OverwritesExistingSpawnMarkerOnSameTile() {
    var workspace = LevelEditorWorkspace.CreateNew("Tutorial_01");
    var controller = new WorkspaceMapController(workspace);

    controller.PlaceSpawnMarker(2, 3);
    controller.PaintTile(2, 3, TileType.Ground, 0);

    Assert.IsFalse(workspace.IsSpawnMarker("R1", 2, 3));
    Assert.AreEqual(TileType.Ground, workspace.GetTileOverride(2, 3).tileType);
}
```

**Step 2: Run test to verify it fails**

Run:

```bash
& 'C:\Program Files\Unity\Hub\Editor\2022.3.62f1\Editor\Unity.exe' -batchmode -projectPath 'd:\Program Files\MyProject\TDUandU' -executeMethod 'ArknightsLite.Editor.Tests.LevelEditor.WorkspaceMapControllerTests.RunFromCommandLine' -logFile 'd:\Program Files\MyProject\TDUandU\Logs\workspace-overwrite.log' -quit
```

Expected: FAIL because semantic points and terrain still behave like separate layers or do not clear each other.

**Step 3: Write minimal implementation**

Implement:

1. when painting normal terrain, clear spawn/goal/portal semantics on that tile
2. when placing spawn/goal/portal semantics, clear incompatible previous type on that tile
3. keep the rule: one tile, one final main type

**Step 4: Run test to verify it passes**

Run the same command as Step 2.

Expected: PASS.

**Step 5: Commit**

```bash
git add Assets/Scripts/Editor/Tests/WorkspaceMapControllerTests.cs Assets/Scripts/Editor/LevelEditor/Map/WorkspaceMapController.cs Assets/Scripts/Editor/LevelEditor/Core/LevelEditorWorkspace.cs
git commit -m "feat: enforce overwrite behavior for terrain and semantics"
```

### Task 3: 统一笔刷工具为单一地块类型选择区

**Files:**
- Modify: `Assets/Scripts/Editor/LevelEditorWindow.cs`
- Modify: `Assets/Scripts/Editor/Tests/LevelEditorWindowWorkflowTests.cs`
- Modify: `Assets/Scripts/Editor/LevelEditor/Core/LevelEditorText.cs`

**Step 1: Write the failing test**

```csharp
[Test]
public void BrushPanel_ExposesUnifiedTileTypeChoicesWithoutTopLevelSemanticTabs() {
    string[] toolTabs = ArknightsLite.Editor.LevelEditor.Core.LevelEditorText.Window.BrushToolTabs;

    CollectionAssert.DoesNotContain(toolTabs, "出生点");
    CollectionAssert.DoesNotContain(toolTabs, "目标点");
}
```

**Step 2: Run test to verify it fails**

Run:

```bash
& 'C:\Program Files\Unity\Hub\Editor\2022.3.62f1\Editor\Unity.exe' -batchmode -projectPath 'd:\Program Files\MyProject\TDUandU' -executeMethod 'ArknightsLite.Editor.Tests.LevelEditor.LevelEditorWindowWorkflowTests.RunFromCommandLine' -logFile 'd:\Program Files\MyProject\TDUandU\Logs\editor-brush-layout.log' -quit
```

Expected: FAIL because the window still uses the old split tool layout.

**Step 3: Write minimal implementation**

Implement:

1. remove the old top-level semantic brush tabs from the brush area
2. create one unified tile-type button matrix for:
   - 禁用
   - 地面
   - 高台
   - 坑洞
   - 出生点
   - 目标点
   - 传送入口
   - 传送出口
3. keep `路径` outside this button matrix
4. keep all user-facing labels in simplified Chinese

**Step 4: Run test to verify it passes**

Run the same command as Step 2.

Expected: PASS.

**Step 5: Commit**

```bash
git add Assets/Scripts/Editor/LevelEditorWindow.cs Assets/Scripts/Editor/Tests/LevelEditorWindowWorkflowTests.cs Assets/Scripts/Editor/LevelEditor/Core/LevelEditorText.cs
git commit -m "feat: unify brush panel tile type selection"
```

### Task 4: 锁定显式编号和稳定编号分配

**Files:**
- Modify: `Assets/Scripts/Editor/Tests/WorkspaceSemanticMarkerTests.cs`
- Modify: `Assets/Scripts/Editor/LevelEditor/Core/LevelEditorWorkspace.cs`
- Modify: `Assets/Scripts/Editor/LevelEditor/Map/WorkspaceMapController.cs`

**Step 1: Write the failing test**

```csharp
[Test]
public void RecreateSpawnMarker_UsesNextStableIdInsteadOfReorderingExistingIds() {
    var workspace = LevelEditorWorkspace.CreateNew("Tutorial_01");
    var first = workspace.AddSpawnMarker(new Vector2Int(1, 1));
    workspace.RemoveSemanticAt(first.Position.x, first.Position.y);

    var recreated = workspace.AddSpawnMarker(new Vector2Int(2, 2));

    Assert.AreEqual("R2", recreated.Id);
}
```

**Step 2: Run test to verify it fails**

Run:

```bash
& 'C:\Program Files\Unity\Hub\Editor\2022.3.62f1\Editor\Unity.exe' -batchmode -projectPath 'd:\Program Files\MyProject\TDUandU' -executeMethod 'ArknightsLite.Editor.Tests.LevelEditor.WorkspaceSemanticMarkerTests.RunFromCommandLine' -logFile 'd:\Program Files\MyProject\TDUandU\Logs\semantic-id-stability.log' -quit
```

Expected: FAIL because ID allocation is not yet explicitly locked to the approved rule.

**Step 3: Write minimal implementation**

Implement:

1. use `max(existing numeric suffix) + 1` for `R*`, `B*`, `IN* / OUT*`
2. do not reindex existing points after delete
3. keep displayed IDs identical between scene, panels, and export

**Step 4: Run test to verify it passes**

Run the same command as Step 2.

Expected: PASS.

**Step 5: Commit**

```bash
git add Assets/Scripts/Editor/Tests/WorkspaceSemanticMarkerTests.cs Assets/Scripts/Editor/LevelEditor/Core/LevelEditorWorkspace.cs Assets/Scripts/Editor/LevelEditor/Map/WorkspaceMapController.cs
git commit -m "feat: stabilize semantic marker id allocation"
```

### Task 5: 让白模颜色直接表达最终主类型

**Files:**
- Modify: `Assets/Scripts/Runtime/EditorTools/TileAuthoring.cs`
- Modify: `Assets/Scripts/Editor/TileAuthoringEditor.cs`
- Modify: `Assets/Scripts/Editor/LevelEditor/Services/WhiteboxGenerationService.cs`
- Modify: `Assets/Scripts/Editor/Tests/WhiteboxGenerationServiceTests.cs`

**Step 1: Write the failing test**

```csharp
[Test]
public void SpawnMarkerTile_UsesSemanticGroundVisualInsteadOfKeepingBaseTerrainColor() {
    var workspace = LevelEditorWorkspace.CreateNew("Tutorial_01");
    workspace.AddSpawnMarker(new Vector2Int(1, 1));

    var result = WhiteboxGenerationService.BuildPreview(workspace);

    Assert.Greater(result.TileCount, 0);
    // Extend this test to assert the semantic tile visual metadata once preview metadata supports it.
}
```

**Step 2: Run test to verify it fails**

Run:

```bash
& 'C:\Program Files\Unity\Hub\Editor\2022.3.62f1\Editor\Unity.exe' -batchmode -projectPath 'd:\Program Files\MyProject\TDUandU' -executeMethod 'ArknightsLite.Editor.Tests.LevelEditor.WhiteboxGenerationServiceTests.RunFromCommandLine' -logFile 'd:\Program Files\MyProject\TDUandU\Logs\whitebox-visuals.log' -quit
```

Expected: FAIL or remain incomplete until semantic tile colors are represented in authoring/preview data.

**Step 3: Write minimal implementation**

Implement:

1. drive tile base color from final main type, not only from base terrain
2. keep spawn/goal/portal helper markers
3. make the underlying tile color also change for spawn/goal/portal tiles
4. align type-to-color mapping with the approved web editor palette

**Step 4: Run test to verify it passes**

Run the same command as Step 2.

Expected: PASS.

**Step 5: Commit**

```bash
git add Assets/Scripts/Runtime/EditorTools/TileAuthoring.cs Assets/Scripts/Editor/TileAuthoringEditor.cs Assets/Scripts/Editor/LevelEditor/Services/WhiteboxGenerationService.cs Assets/Scripts/Editor/Tests/WhiteboxGenerationServiceTests.cs
git commit -m "feat: render semantic tiles with explicit ground colors"
```

### Task 6: 移除导出和校验中的 legacy fallback

**Files:**
- Modify: `Assets/Scripts/Editor/LevelEditor/Services/LevelConfigExportService.cs`
- Modify: `Assets/Scripts/Editor/LevelEditor/Services/LevelValidationService.cs`
- Modify: `Assets/Scripts/Editor/Tests/LevelConfigExportServiceTests.cs`
- Modify: `Assets/Scripts/Editor/Tests/LevelEditorValidationWorkflowTests.cs`

**Step 1: Write the failing test**

```csharp
[Test]
public void Export_FailsWhenWaveReferencesRemovedSpawnMarker() {
    var workspace = LevelEditorWorkspace.CreateNew("Tutorial_01");
    var spawn = workspace.AddSpawnMarker(new Vector2Int(1, 1));
    var goal = workspace.AddGoalMarker(new Vector2Int(5, 5));
    var wave = LevelEditorWorkspace.CreateDefaultWave("wave_01");
    wave.spawnId = spawn.Id;
    wave.targetId = goal.Id;
    workspace.Waves.Add(wave);

    workspace.RemoveSemanticAt(1, 1);

    var result = LevelValidationService.ValidateWorkspace(workspace);

    Assert.IsFalse(result.IsValid);
}
```

**Step 2: Run test to verify it fails**

Run:

```bash
& 'C:\Program Files\Unity\Hub\Editor\2022.3.62f1\Editor\Unity.exe' -batchmode -projectPath 'd:\Program Files\MyProject\TDUandU' -executeMethod 'ArknightsLite.Editor.Tests.LevelEditor.LevelEditorValidationWorkflowTests.RunFromCommandLine' -logFile 'd:\Program Files\MyProject\TDUandU\Logs\validation-no-fallback.log' -quit
```

Expected: FAIL because validation/export still tolerate missing explicit references or rely on fallback behavior.

**Step 3: Write minimal implementation**

Implement:

1. remove implicit spawn/goal fallback in export flow
2. validate `spawnId` and `targetId` against existing map semantics
3. block export when explicit references are missing
4. keep `路径` current behavior intact, but do not allow it to hide missing spawn/goal references

**Step 4: Run test to verify it passes**

Run the same command as Step 2.

Expected: PASS.

**Step 5: Commit**

```bash
git add Assets/Scripts/Editor/LevelEditor/Services/LevelConfigExportService.cs Assets/Scripts/Editor/LevelEditor/Services/LevelValidationService.cs Assets/Scripts/Editor/Tests/LevelConfigExportServiceTests.cs Assets/Scripts/Editor/Tests/LevelEditorValidationWorkflowTests.cs
git commit -m "feat: remove legacy export fallback for map semantics"
```

### Task 7: 回归测试并补齐文档闭环

**Files:**
- Modify: `Assets/Scripts/Editor/Tests/LevelEditorWorkspaceTests.cs`
- Modify: `Assets/Scripts/Editor/Tests/WorkspaceMapControllerTests.cs`
- Modify: `Assets/Scripts/Editor/Tests/WorkspaceSemanticMarkerTests.cs`
- Modify: `Assets/Scripts/Editor/Tests/WhiteboxGenerationServiceTests.cs`
- Modify: `Assets/Scripts/Editor/Tests/LevelConfigExportServiceTests.cs`
- Create: `docs/plans/2026-03-24-level-editor-workspace-terrain-semantic-rollout-notes.md`

**Step 1: Write the failing test**

```csharp
[Test]
public void NewWorkspace_CanBePaintedWithForbiddenGroundSpawnGoalAndStillExportAfterSaveReload() {
    // Arrange a real user flow:
    // 1. create workspace
    // 2. paint ground path on top of forbidden defaults
    // 3. place spawn/goal
    // 4. overwrite one semantic tile back to terrain
    // 5. save and reload
    // 6. validate/export
}
```

**Step 2: Run test to verify it fails**

Run all touched suites individually, starting with:

```bash
& 'C:\Program Files\Unity\Hub\Editor\2022.3.62f1\Editor\Unity.exe' -batchmode -projectPath 'd:\Program Files\MyProject\TDUandU' -executeMethod 'ArknightsLite.Editor.Tests.LevelEditor.LevelEditorWorkspaceTests.RunFromCommandLine' -logFile 'd:\Program Files\MyProject\TDUandU\Logs\terrain-semantic-regression.log' -quit
```

Expected: At least one suite fails until the new interaction model is fully closed.

**Step 3: Write minimal implementation**

Implement:

1. any remaining persistence glue for the new main-type rules
2. any remaining scene refresh glue
3. rollout notes for the new brush/terrain/semantic workflow

**Step 4: Run test to verify it passes**

Re-run all touched suites individually and verify pass markers in logs.

Expected: PASS across all updated suites.

**Step 5: Commit**

```bash
git add Assets/Scripts/Editor/Tests/LevelEditorWorkspaceTests.cs Assets/Scripts/Editor/Tests/WorkspaceMapControllerTests.cs Assets/Scripts/Editor/Tests/WorkspaceSemanticMarkerTests.cs Assets/Scripts/Editor/Tests/WhiteboxGenerationServiceTests.cs Assets/Scripts/Editor/Tests/LevelConfigExportServiceTests.cs docs/plans/2026-03-24-level-editor-workspace-terrain-semantic-rollout-notes.md
git commit -m "test: cover unified terrain and semantic editing flow"
```
