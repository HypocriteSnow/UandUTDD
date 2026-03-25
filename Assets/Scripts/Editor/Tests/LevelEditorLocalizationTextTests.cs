using ArknightsLite.Editor.LevelEditor.Core;
using NUnit.Framework;
using UnityEngine;

namespace ArknightsLite.Editor.Tests.LevelEditor {
    public class LevelEditorLocalizationTextTests {
        [Test]
        public void WindowTexts_UseApprovedSimplifiedChineseTerminology() {
            Assert.AreEqual("关卡编辑器", LevelEditorText.Window.Title);
            Assert.AreEqual("Workspace", LevelEditorText.Window.WorkspaceSectionTitle);
            Assert.AreEqual("Workspace 名称", LevelEditorText.Window.WorkspaceNameLabel);
            Assert.AreEqual("导出 LevelConfig", LevelEditorText.Window.ExportLevelConfigButton);
            Assert.AreEqual("编辑控制", LevelEditorText.Window.EditingSectionTitle);
            Assert.AreEqual("笔刷工具", LevelEditorText.Window.BrushSectionTitle);

            CollectionAssert.AreEqual(
                new[] { "地图", "传送门", "路径", "波次" },
                LevelEditorText.Window.WorkspaceModuleTabs);
            CollectionAssert.AreEqual(
                new[] { "地块类型", "路径" },
                LevelEditorText.Window.BrushToolTabs);
            Assert.AreEqual("地块类型", LevelEditorText.Window.BrushTypeLabel);
            Assert.AreEqual("出生点 (5)", LevelEditorText.Window.SpawnButton);
            Assert.AreEqual("目标点 (6)", LevelEditorText.Window.GoalButton);
        }

        [Test]
        public void PanelAndInspectorTexts_UseSimplifiedChineseTerminology() {
            Assert.AreEqual("关卡运行时参数", LevelEditorText.RuntimePanel.SectionTitle);
            Assert.AreEqual("传送门", LevelEditorText.PortalPanel.SectionTitle);
            Assert.AreEqual("路径", LevelEditorText.PathPanel.SectionTitle);
            Assert.AreEqual("波次", LevelEditorText.WavePanel.SectionTitle);
            Assert.AreEqual("格子信息", LevelEditorText.TileInspector.SectionTitle);
            Assert.AreEqual("格子数据", LevelEditorText.TileInspector.DataSectionTitle);
        }

        [Test]
        public void OverlayAndSummaryTexts_AreLocalized() {
            StringAssert.Contains("工具: 出生点", LevelEditorText.Window.SpawnOverlay);
            StringAssert.Contains("工具: 目标点", LevelEditorText.Window.GoalOverlay);
            StringAssert.Contains("当前 Workspace:", LevelEditorText.Window.CurrentWorkspaceLabel("Demo"));
            StringAssert.Contains("地图尺寸:", LevelEditorText.Window.MapSizeLabel(10, 8));
            StringAssert.Contains("格子尺寸:", LevelEditorText.Window.CellSizeValueLabel(1.5f));
        }

        public static void RunFromCommandLine() {
            var tests = new LevelEditorLocalizationTextTests();
            tests.WindowTexts_UseApprovedSimplifiedChineseTerminology();
            tests.PanelAndInspectorTexts_UseSimplifiedChineseTerminology();
            tests.OverlayAndSummaryTexts_AreLocalized();
            Debug.Log("[LevelEditorTests] LevelEditorLocalizationTextTests passed.");
        }
    }
}
