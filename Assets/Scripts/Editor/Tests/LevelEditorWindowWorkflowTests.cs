using System.Reflection;
using ArknightsLite.Editor.LevelEditor.Core;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace ArknightsLite.Editor.Tests.LevelEditor {
    public class LevelEditorWindowWorkflowTests {
        [Test]
        public void CreateWorkspace_CreatesTrackedWorkspaceAsset() {
            var window = ScriptableObject.CreateInstance<LevelEditorWindow>();

            try {
                SetPrivateField(window, "_newLevelName", "Episode_01");
                InvokePrivate(window, "CreateWorkspace");

                var session = GetSession(window);
                Assert.IsNotNull(session.CurrentWorkspaceAsset);
                Assert.AreEqual("Episode_01", session.CurrentWorkspace.LevelName);
                Assert.AreEqual("Episode_01", session.CurrentWorkspace.ExportName);
                Assert.IsFalse(string.IsNullOrWhiteSpace(AssetDatabase.GetAssetPath(session.CurrentWorkspaceAsset)));
            } finally {
                CleanupTrackedAsset(window);
                Object.DestroyImmediate(window);
            }
        }

        [Test]
        public void CreateWorkspace_UsesConfiguredBaseMapParameters() {
            var window = ScriptableObject.CreateInstance<LevelEditorWindow>();

            try {
                SetPrivateField(window, "_newLevelName", "Episode_01");
                SetPrivateField(window, "_newMapWidth", 14);
                SetPrivateField(window, "_newMapDepth", 8);
                SetPrivateField(window, "_newCellSize", 1.5f);

                InvokePrivate(window, "CreateWorkspace");

                var session = GetSession(window);
                Assert.AreEqual(14, session.CurrentWorkspace.MapWidth);
                Assert.AreEqual(8, session.CurrentWorkspace.MapDepth);
                Assert.AreEqual(1.5f, session.CurrentWorkspace.CellSize);
            } finally {
                CleanupTrackedAsset(window);
                Object.DestroyImmediate(window);
            }
        }

        [Test]
        public void SaveWorkspace_PersistsUpdatedExportNameBackToAsset() {
            var window = ScriptableObject.CreateInstance<LevelEditorWindow>();

            try {
                SetPrivateField(window, "_newLevelName", "Episode_02");
                InvokePrivate(window, "CreateWorkspace");

                var session = GetSession(window);
                session.CurrentWorkspace.ExportName = "Episode_02_Export";

                InvokePrivate(window, "SaveWorkspace");

                string assetPath = AssetDatabase.GetAssetPath(session.CurrentWorkspaceAsset);
                var restored = LevelEditorWorkspaceRepository.LoadAsset(assetPath);
                Assert.AreEqual("Episode_02_Export", restored.Workspace.ExportName);
            } finally {
                CleanupTrackedAsset(window);
                Object.DestroyImmediate(window);
            }
        }

        [Test]
        public void LoadWorkspaceAsset_BindsLoadedWorkspaceToCurrentSession() {
            string assetPath = null;
            var window = ScriptableObject.CreateInstance<LevelEditorWindow>();

            try {
                var workspace = LevelEditorWorkspace.CreateNew("Episode_03");
                workspace.ExportName = "Episode_03_Export";
                assetPath = LevelEditorWorkspaceRepository.SaveAsNewAsset(workspace);

                InvokePrivate(window, "LoadWorkspaceAsset", assetPath);

                var session = GetSession(window);
                Assert.IsNotNull(session.CurrentWorkspaceAsset);
                Assert.AreEqual(assetPath, AssetDatabase.GetAssetPath(session.CurrentWorkspaceAsset));
                Assert.AreEqual("Episode_03", session.CurrentWorkspace.LevelName);
                Assert.AreEqual("Episode_03_Export", session.CurrentWorkspace.ExportName);
            } finally {
                if (!string.IsNullOrWhiteSpace(assetPath)) {
                    AssetDatabase.DeleteAsset(assetPath);
                }

                Object.DestroyImmediate(window);
            }
        }

        [Test]
        public void SyncWorkspaceNaming_CopiesLevelNameIntoExportName() {
            var window = ScriptableObject.CreateInstance<LevelEditorWindow>();

            try {
                SetPrivateField(window, "_newLevelName", "Episode_04");
                InvokePrivate(window, "CreateWorkspace");

                var session = GetSession(window);
                session.CurrentWorkspace.LevelName = "Episode_04_Renamed";
                session.CurrentWorkspace.ExportName = "CustomExport";

                InvokePrivate(window, "SyncWorkspaceNaming");

                Assert.AreEqual("Episode_04_Renamed", session.CurrentWorkspace.ExportName);
            } finally {
                CleanupTrackedAsset(window);
                Object.DestroyImmediate(window);
            }
        }

        public static void RunFromCommandLine() {
            new LevelEditorWindowWorkflowTests().CreateWorkspace_CreatesTrackedWorkspaceAsset();
            new LevelEditorWindowWorkflowTests().CreateWorkspace_UsesConfiguredBaseMapParameters();
            new LevelEditorWindowWorkflowTests().SaveWorkspace_PersistsUpdatedExportNameBackToAsset();
            new LevelEditorWindowWorkflowTests().LoadWorkspaceAsset_BindsLoadedWorkspaceToCurrentSession();
            new LevelEditorWindowWorkflowTests().SyncWorkspaceNaming_CopiesLevelNameIntoExportName();
            Debug.Log("[LevelEditorTests] LevelEditorWindowWorkflowTests passed.");
        }

        private static LevelEditorSession GetSession(LevelEditorWindow window) {
            return GetPrivateField<LevelEditorSession>(window, "_session");
        }

        private static void CleanupTrackedAsset(LevelEditorWindow window) {
            var session = GetSession(window);
            if (session?.CurrentWorkspaceAsset == null) {
                return;
            }

            string assetPath = AssetDatabase.GetAssetPath(session.CurrentWorkspaceAsset);
            if (!string.IsNullOrWhiteSpace(assetPath)) {
                AssetDatabase.DeleteAsset(assetPath);
            }
        }

        private static T GetPrivateField<T>(object target, string fieldName) {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, $"Expected private field '{fieldName}'.");
            return (T)field.GetValue(target);
        }

        private static void SetPrivateField(object target, string fieldName, object value) {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, $"Expected private field '{fieldName}'.");
            field.SetValue(target, value);
        }

        private static object InvokePrivate(object target, string methodName, params object[] args) {
            MethodInfo method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(method, $"Expected private method '{methodName}'.");
            return method.Invoke(target, args);
        }
    }
}
