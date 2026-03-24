#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ArknightsLite.Config;
using ArknightsLite.Model;

/// <summary>
/// 格子编辑组件 - 仅在编辑模式下使用
/// 职责：连接场景 GameObject 和 LevelConfig，实现双向同步
/// 注意：此文件虽然用于编辑器，但必须在 Runtime 文件夹中，因为它是 MonoBehaviour 组件
/// </summary>
[ExecuteInEditMode]
public class TileAuthoring : MonoBehaviour {
    private const string SpawnMarkerVisualName = "SpawnMarkerVisual";
    private const string GoalMarkerVisualName = "GoalMarkerVisual";
    private const string PortalEntranceVisualName = "PortalEntranceVisual";
    private const string PortalExitVisualName = "PortalExitVisual";
    private static Material s_spawnMarkerVisualMaterial;
    private static Material s_goalMarkerVisualMaterial;
    private static Material s_portalEntranceVisualMaterial;
    private static Material s_portalExitVisualMaterial;

    [Header("坐标信息（只读）")]
    [SerializeField] private int _x;
    [SerializeField] private int _z;
    [SerializeField] private float _cellSize = 1f;
    
    [Header("格子配置")]
    [Tooltip("格子类型")]
    public TileType tileType = TileType.Forbidden;
    
    [Tooltip("高度等级（0=地面，1=高台）")]
    [Range(0, 3)]
    public int heightLevel = 0;
    
    [Tooltip("是否可通行")]
    public bool walkable = false;
    
    [Tooltip("可部署标签")]
    public string deployTag = "All";
    
    [Header("配置引用（自动设置）")]
    [HideInInspector]
    public LevelConfig config;
    
    [HideInInspector]
    public GridVisualConfig visualConfig;

    [SerializeField] private bool _hasSpawnMarker;
    [SerializeField] private string _spawnMarkerId = string.Empty;
    [SerializeField] private bool _hasGoalMarker;
    [SerializeField] private string _goalMarkerId = string.Empty;
    [SerializeField] private List<string> _semanticLabels = new List<string>();
    
    
    // ==================== 初始化 ====================
    
    /// <summary>
    /// 初始化格子（由 LevelEditorWindow 调用）
    /// </summary>
    public void Initialize(int x, int z, LevelConfig levelConfig, GridVisualConfig gridVisualConfig) {
        _x = x;
        _z = z;
        config = levelConfig;
        visualConfig = gridVisualConfig;
        _cellSize = levelConfig != null ? levelConfig.cellSize : 1f;
        ApplySemanticMarkers(false, string.Empty, false, string.Empty);
        
        // 从配置加载初始数据
        LoadFromConfig();
        UpdateVisual();
    }

    /// <summary>
    /// 以工作区白模数据初始化格子
    /// </summary>
    public void Initialize(int x, int z, TileData initialData, float cellSize, GridVisualConfig gridVisualConfig) {
        _x = x;
        _z = z;
        config = null;
        visualConfig = gridVisualConfig;
        _cellSize = cellSize;

        ApplySemanticMarkers(false, string.Empty, false, string.Empty);
        ApplyTileData(initialData, cellSize);
    }
    
    /// <summary>
    /// 从配置加载数据
    /// </summary>
    private void LoadFromConfig() {
        if (config == null) return;
        
        var data = config.GetTileData(_x, _z);
        if (data != null) {
            tileType = data.tileType;
            heightLevel = data.heightLevel;
            walkable = data.walkable;
            deployTag = data.deployTag;
        }
    }
    
    
    // ==================== Inspector 同步 ====================
    
    /// <summary>
    /// 当 Inspector 值改变时触发
    /// </summary>
    private void OnValidate() {
        if (config == null) return;
        
        // 移除自动同步，由 CustomEditor 处理
        // SyncToConfig();
        // UpdateVisual();
    }
    
    /// <summary>
    /// 手动触发同步（供 Editor 调用）
    /// </summary>
    public void ForceSync() {
        if (config != null) {
            SyncToConfig();
        }
        UpdateVisual();
    }

    public void ApplyTileData(TileData data, float cellSize) {
        _cellSize = cellSize;

        var resolvedData = data ?? new TileData {
            x = _x,
            z = _z,
            tileType = TileType.Ground,
            heightLevel = 0,
            walkable = true,
            deployTag = "All"
        };

        tileType = resolvedData.tileType;
        heightLevel = resolvedData.heightLevel;
        walkable = resolvedData.walkable;
        deployTag = string.IsNullOrWhiteSpace(resolvedData.deployTag) ? "All" : resolvedData.deployTag;
        UpdateVisual();
    }

    public void ApplySemanticMarkers(bool hasSpawnMarker, string spawnMarkerId, bool hasGoalMarker, string goalMarkerId) {
        _hasSpawnMarker = hasSpawnMarker;
        _spawnMarkerId = hasSpawnMarker ? spawnMarkerId ?? string.Empty : string.Empty;
        _hasGoalMarker = hasGoalMarker;
        _goalMarkerId = hasGoalMarker ? goalMarkerId ?? string.Empty : string.Empty;
        _semanticLabels = new List<string>();
        if (_hasSpawnMarker) {
            _semanticLabels.Add(_spawnMarkerId);
        }

        if (_hasGoalMarker) {
            _semanticLabels.Add(_goalMarkerId);
        }
        UpdateVisual();
    }

    public void ApplySemanticMarkers(IEnumerable<string> semanticLabels) {
        _semanticLabels = new List<string>();
        _hasSpawnMarker = false;
        _spawnMarkerId = string.Empty;
        _hasGoalMarker = false;
        _goalMarkerId = string.Empty;

        if (semanticLabels != null) {
            foreach (string semanticLabel in semanticLabels) {
                if (string.IsNullOrWhiteSpace(semanticLabel) || _semanticLabels.Contains(semanticLabel)) {
                    continue;
                }

                _semanticLabels.Add(semanticLabel);
                if (!_hasSpawnMarker && IsSpawnSemanticLabel(semanticLabel)) {
                    _hasSpawnMarker = true;
                    _spawnMarkerId = semanticLabel;
                }

                if (!_hasGoalMarker && IsGoalSemanticLabel(semanticLabel)) {
                    _hasGoalMarker = true;
                    _goalMarkerId = semanticLabel;
                }
            }
        }

        UpdateVisual();
    }
    
    /// <summary>
    /// 同步数据到配置文件
    /// </summary>
    private void SyncToConfig() {
        if (config == null) return;
        
        var data = new TileData {
            x = _x,
            z = _z,
            tileType = tileType,
            heightLevel = heightLevel,
            walkable = walkable,
            deployTag = deployTag
        };
        
        config.SetTileData(_x, _z, data);
        EditorUtility.SetDirty(config);
    }
    
    
    // ==================== 视觉更新 ====================
    
    /// <summary>
    /// 更新格子的视觉表现
    /// </summary>
    public void UpdateVisual() {
        UpdateMaterial();
        UpdatePosition();
        UpdateSemanticMarkerVisuals();
    }
    
    /// <summary>
    /// 更新材质
    /// </summary>
    private void UpdateMaterial() {
        if (visualConfig == null) return;
        
        var renderer = GetComponent<MeshRenderer>();
        if (renderer == null) return;
        
        // 优先级：起点/终点 > 格子类型
        Material targetMaterial = null;
        
        if (HasSpawnMarker && visualConfig.spawnPointMaterial != null) {
            targetMaterial = visualConfig.spawnPointMaterial;
        }
        else if (HasGoalMarker && visualConfig.goalPointMaterial != null) {
            targetMaterial = visualConfig.goalPointMaterial;
        }
        
        // 如果不是特殊点，使用类型材质
        if (targetMaterial == null) {
            targetMaterial = visualConfig.GetMaterialForType(tileType);
        }
        
        if (targetMaterial != null) {
            renderer.sharedMaterial = targetMaterial;
        }
    }
    
    /// <summary>
    /// 更新位置（根据高度等级）
    /// </summary>
    private void UpdatePosition() {
        Vector3 pos = transform.position;
        float resolvedCellSize = config != null ? config.cellSize : _cellSize;
        pos.x = _x * resolvedCellSize;
        pos.y = heightLevel * resolvedCellSize + 0.1f; // 略微抬高避免 Z-Fighting
        pos.z = _z * resolvedCellSize;
        transform.position = pos;
    }
    
    
    // ==================== 公共接口 ====================
    
    /// <summary>
    /// 获取 X 坐标
    /// </summary>
    private void UpdateSemanticMarkerVisuals() {
        float resolvedCellSize = config != null ? config.cellSize : _cellSize;
        EnsureMarkerVisual(
            SpawnMarkerVisualName,
            HasSpawnMarker,
            PrimitiveType.Sphere,
            new Vector3(resolvedCellSize * 0.28f, resolvedCellSize * 0.28f, resolvedCellSize * 0.28f),
            new Vector3(0f, resolvedCellSize * 0.42f, 0f),
            GetSpawnMarkerVisualMaterial()
        );
        EnsureMarkerVisual(
            GoalMarkerVisualName,
            HasGoalMarker,
            PrimitiveType.Cube,
            new Vector3(resolvedCellSize * 0.48f, resolvedCellSize * 0.14f, resolvedCellSize * 0.48f),
            new Vector3(0f, resolvedCellSize * 0.22f, 0f),
            GetGoalMarkerVisualMaterial()
        );
        EnsureMarkerVisual(
            PortalEntranceVisualName,
            HasPortalEntranceMarker,
            PrimitiveType.Cylinder,
            new Vector3(resolvedCellSize * 0.2f, resolvedCellSize * 0.05f, resolvedCellSize * 0.2f),
            new Vector3(0f, resolvedCellSize * 0.34f, 0f),
            GetPortalEntranceVisualMaterial()
        );
        EnsureMarkerVisual(
            PortalExitVisualName,
            HasPortalExitMarker,
            PrimitiveType.Cylinder,
            new Vector3(resolvedCellSize * 0.2f, resolvedCellSize * 0.05f, resolvedCellSize * 0.2f),
            new Vector3(0f, resolvedCellSize * 0.56f, 0f),
            GetPortalExitVisualMaterial()
        );
    }

    private void EnsureMarkerVisual(string markerName, bool shouldExist, PrimitiveType primitiveType, Vector3 worldScale, Vector3 worldOffset, Material material) {
        Transform marker = transform.Find(markerName);
        if (!shouldExist) {
            if (marker != null) {
                DestroyImmediate(marker.gameObject);
            }

            return;
        }

        if (marker == null) {
            var markerObject = GameObject.CreatePrimitive(primitiveType);
            markerObject.name = markerName;
            markerObject.transform.SetParent(transform, false);
            marker = markerObject.transform;

            var collider = markerObject.GetComponent<Collider>();
            if (collider != null) {
                DestroyImmediate(collider);
            }
        }

        marker.localPosition = ToLocalOffset(worldOffset);
        marker.localRotation = Quaternion.identity;
        marker.localScale = ToLocalScale(worldScale);

        var renderer = marker.GetComponent<MeshRenderer>();
        if (renderer != null && material != null) {
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }
    }

    private Vector3 ToLocalOffset(Vector3 worldOffset) {
        Vector3 parentScale = transform.localScale;
        return new Vector3(
            SafeDivide(worldOffset.x, parentScale.x),
            SafeDivide(worldOffset.y, parentScale.y),
            SafeDivide(worldOffset.z, parentScale.z)
        );
    }

    private Vector3 ToLocalScale(Vector3 worldScale) {
        Vector3 parentScale = transform.localScale;
        return new Vector3(
            SafeDivide(worldScale.x, parentScale.x),
            SafeDivide(worldScale.y, parentScale.y),
            SafeDivide(worldScale.z, parentScale.z)
        );
    }

    private static float SafeDivide(float value, float divisor) {
        return Mathf.Approximately(divisor, 0f) ? value : value / divisor;
    }

    private static Material GetSpawnMarkerVisualMaterial() {
        if (s_spawnMarkerVisualMaterial == null) {
            s_spawnMarkerVisualMaterial = CreateMarkerVisualMaterial(new Color(0.95f, 0.25f, 0.25f));
        }

        return s_spawnMarkerVisualMaterial;
    }

    private static Material GetGoalMarkerVisualMaterial() {
        if (s_goalMarkerVisualMaterial == null) {
            s_goalMarkerVisualMaterial = CreateMarkerVisualMaterial(new Color(0.2f, 0.55f, 1f));
        }

        return s_goalMarkerVisualMaterial;
    }

    private static Material GetPortalEntranceVisualMaterial() {
        if (s_portalEntranceVisualMaterial == null) {
            s_portalEntranceVisualMaterial = CreateMarkerVisualMaterial(new Color(1f, 0.75f, 0.2f));
        }

        return s_portalEntranceVisualMaterial;
    }

    private static Material GetPortalExitVisualMaterial() {
        if (s_portalExitVisualMaterial == null) {
            s_portalExitVisualMaterial = CreateMarkerVisualMaterial(new Color(0.45f, 0.95f, 0.75f));
        }

        return s_portalExitVisualMaterial;
    }

    private static Material CreateMarkerVisualMaterial(Color color) {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) {
            shader = Shader.Find("Standard");
        }

        if (shader == null) {
            shader = Shader.Find("Sprites/Default");
        }

        var material = new Material(shader) {
            hideFlags = HideFlags.HideAndDontSave
        };

        if (material.HasProperty("_BaseColor")) {
            material.SetColor("_BaseColor", color);
        }

        if (material.HasProperty("_Color")) {
            material.color = color;
        }

        if (material.HasProperty("_EmissionColor")) {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", color * 0.5f);
        }

        return material;
    }

    public int X => _x;
    
    /// <summary>
    /// 获取 Z 坐标
    /// </summary>
    public int Z => _z;

    public bool HasSpawnMarker => _hasSpawnMarker || (config != null && config.IsSpawnPoint(_x, _z));

    public bool HasGoalMarker => _hasGoalMarker || (config != null && config.IsGoalPoint(_x, _z));

    public bool HasPortalEntranceMarker => ContainsSemanticLabelPrefix("IN");

    public bool HasPortalExitMarker => ContainsSemanticLabelPrefix("OUT");

    public string SpawnMarkerId => string.IsNullOrWhiteSpace(_spawnMarkerId) ? "spawn_01" : _spawnMarkerId;

    public string GoalMarkerId => string.IsNullOrWhiteSpace(_goalMarkerId) ? "goal_01" : _goalMarkerId;

    public string SemanticLabel => GetSemanticLabel();
    
    /// <summary>
    /// 设置格子类型（用于笔刷工具）
    /// </summary>
    public void SetTileType(TileType newType) {
        if (tileType == newType) return;
        
        tileType = newType;
        
        // 根据类型自动设置通行性
        walkable = (newType != TileType.Forbidden && newType != TileType.Hole);
        
        if (config != null) {
            SyncToConfig();
        }
        UpdateVisual();
    }
    
    /// <summary>
    /// 设置高度等级（用于笔刷工具）
    /// </summary>
    public void SetHeightLevel(int newHeight) {
        if (heightLevel == newHeight) return;
        
        heightLevel = Mathf.Clamp(newHeight, 0, 3);
        if (config != null) {
            SyncToConfig();
        }
        UpdateVisual();
    }
    
    
    // ==================== 调试信息 ====================
    
    private void OnDrawGizmos() {
        string semanticLabel = GetSemanticLabel();
        if (string.IsNullOrWhiteSpace(semanticLabel)) {
            return;
        }

        Color color = HasSpawnMarker && HasGoalMarker
            ? Color.magenta
            : (HasSpawnMarker ? new Color(0.9f, 0.2f, 0.2f) : (HasGoalMarker ? new Color(0.2f, 0.5f, 1f) : Color.magenta));

        Gizmos.color = color;
        Gizmos.DrawWireCube(transform.position + Vector3.up * 0.2f, new Vector3(_cellSize * 0.6f, 0.3f, _cellSize * 0.6f));

        #if UNITY_EDITOR
        var style = new GUIStyle(EditorStyles.boldLabel);
        style.normal.textColor = color;
        Handles.Label(transform.position + Vector3.up * 0.6f, semanticLabel, style);
        #endif
    }

    private void OnDrawGizmosSelected() {
        // 显示坐标信息
        Gizmos.color = Color.yellow;
        Vector3 labelPos = transform.position + Vector3.up * 0.5f;
        
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(labelPos, $"({_x}, {_z})\n{tileType}");
        #endif
    }

    private string GetSemanticLabel() {
        if (_semanticLabels != null && _semanticLabels.Count > 0) {
            return string.Join(" / ", _semanticLabels);
        }

        if (HasSpawnMarker && HasGoalMarker) {
            return $"{SpawnMarkerId} / {GoalMarkerId}";
        }

        if (HasSpawnMarker) {
            return SpawnMarkerId;
        }

        if (HasGoalMarker) {
            return GoalMarkerId;
        }

        return string.Empty;
    }

    private static bool IsSpawnSemanticLabel(string semanticLabel) {
        return semanticLabel.StartsWith("R", StringComparison.OrdinalIgnoreCase)
            || string.Equals(semanticLabel, "spawn_01", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsGoalSemanticLabel(string semanticLabel) {
        return semanticLabel.StartsWith("B", StringComparison.OrdinalIgnoreCase)
            || string.Equals(semanticLabel, "goal_01", StringComparison.OrdinalIgnoreCase);
    }

    private bool ContainsSemanticLabelPrefix(string prefix) {
        if (_semanticLabels == null || string.IsNullOrWhiteSpace(prefix)) {
            return false;
        }

        foreach (string semanticLabel in _semanticLabels) {
            if (!string.IsNullOrWhiteSpace(semanticLabel)
                && semanticLabel.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) {
                return true;
            }
        }

        return false;
    }
}
#endif
