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
    private enum VisualType {
        Forbidden,
        Ground,
        HighGround,
        Hole,
        Spawn,
        Goal,
        PortalEntrance,
        PortalExit
    }

    private const string SpawnMarkerVisualName = "SpawnMarkerVisual";
    private const string GoalMarkerVisualName = "GoalMarkerVisual";
    private const string PortalEntranceVisualName = "PortalEntranceVisual";
    private const string PortalExitVisualName = "PortalExitVisual";
    private static Material s_forbiddenTileMaterial;
    private static Material s_groundTileMaterial;
    private static Material s_highGroundTileMaterial;
    private static Material s_holeTileMaterial;
    private static Material s_spawnTileMaterial;
    private static Material s_goalTileMaterial;
    private static Material s_portalEntranceTileMaterial;
    private static Material s_portalExitTileMaterial;
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
    public GridVisualConfig visualConfig;

    [SerializeField] private bool _hasSpawnMarker;
    [SerializeField] private string _spawnMarkerId = string.Empty;
    [SerializeField] private bool _hasGoalMarker;
    [SerializeField] private string _goalMarkerId = string.Empty;
    [SerializeField] private List<string> _semanticLabels = new List<string>();
    
    
    // ==================== 初始化 ====================
    
    /// <summary>
    /// 以工作区白模数据初始化格子
    /// </summary>
    public void Initialize(int x, int z, TileData initialData, float cellSize, GridVisualConfig gridVisualConfig) {
        _x = x;
        _z = z;
        visualConfig = gridVisualConfig;
        _cellSize = cellSize;

        ApplySemanticMarkers(false, string.Empty, false, string.Empty);
        ApplyTileData(initialData, cellSize);
    }
    
    /// <summary>
    /// 手动触发同步（供 Editor 调用）
    /// </summary>
    public void ForceSync() {
        UpdateVisual();
    }

    public void ApplyTileData(TileData data, float cellSize) {
        _cellSize = cellSize;

        var resolvedData = data ?? new TileData {
            x = _x,
            z = _z,
            tileType = TileType.Forbidden,
            heightLevel = 0,
            walkable = false,
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
        var renderer = GetComponent<MeshRenderer>();
        if (renderer == null) {
            return;
        }

        VisualType visualType = ResolveVisualType();
        Material targetMaterial = ResolveConfiguredTileMaterial(visualType);
        if (targetMaterial == null) {
            targetMaterial = GetFallbackTileMaterial(visualType);
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
        pos.x = _x * _cellSize;
        pos.y = heightLevel * _cellSize + 0.1f; // 略微抬高避免 Z-Fighting
        pos.z = _z * _cellSize;
        transform.position = pos;
    }
    
    
    // ==================== 公共接口 ====================
    
    /// <summary>
    /// 获取 X 坐标
    /// </summary>
    private void UpdateSemanticMarkerVisuals() {
        EnsureMarkerVisual(
            SpawnMarkerVisualName,
            HasSpawnMarker,
            PrimitiveType.Sphere,
            new Vector3(_cellSize * 0.28f, _cellSize * 0.28f, _cellSize * 0.28f),
            new Vector3(0f, _cellSize * 0.42f, 0f),
            GetSpawnMarkerVisualMaterial()
        );
        EnsureMarkerVisual(
            GoalMarkerVisualName,
            HasGoalMarker,
            PrimitiveType.Cube,
            new Vector3(_cellSize * 0.48f, _cellSize * 0.14f, _cellSize * 0.48f),
            new Vector3(0f, _cellSize * 0.22f, 0f),
            GetGoalMarkerVisualMaterial()
        );
        EnsureMarkerVisual(
            PortalEntranceVisualName,
            HasPortalEntranceMarker,
            PrimitiveType.Cylinder,
            new Vector3(_cellSize * 0.2f, _cellSize * 0.05f, _cellSize * 0.2f),
            new Vector3(0f, _cellSize * 0.34f, 0f),
            GetPortalEntranceVisualMaterial()
        );
        EnsureMarkerVisual(
            PortalExitVisualName,
            HasPortalExitMarker,
            PrimitiveType.Cylinder,
            new Vector3(_cellSize * 0.2f, _cellSize * 0.05f, _cellSize * 0.2f),
            new Vector3(0f, _cellSize * 0.56f, 0f),
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

    private Material ResolveConfiguredTileMaterial(VisualType visualType) {
        if (visualConfig == null) {
            return null;
        }

        switch (visualType) {
            case VisualType.Spawn:
                return visualConfig.spawnPointMaterial;
            case VisualType.Goal:
                return visualConfig.goalPointMaterial;
            case VisualType.Ground:
                return visualConfig.GetMaterialForType(TileType.Ground);
            case VisualType.HighGround:
                return visualConfig.GetMaterialForType(TileType.HighGround);
            case VisualType.Forbidden:
                return visualConfig.GetMaterialForType(TileType.Forbidden);
            case VisualType.Hole:
                return visualConfig.GetMaterialForType(TileType.Hole);
            default:
                return null;
        }
    }

    private static Material GetFallbackTileMaterial(VisualType visualType) {
        switch (visualType) {
            case VisualType.Spawn:
                if (s_spawnTileMaterial == null) {
                    s_spawnTileMaterial = CreateMarkerVisualMaterial(new Color(1f, 0.2f, 0.2f));
                }

                return s_spawnTileMaterial;
            case VisualType.Goal:
                if (s_goalTileMaterial == null) {
                    s_goalTileMaterial = CreateMarkerVisualMaterial(new Color(0.2f, 0.59f, 1f));
                }

                return s_goalTileMaterial;
            case VisualType.PortalEntrance:
                if (s_portalEntranceTileMaterial == null) {
                    s_portalEntranceTileMaterial = CreateMarkerVisualMaterial(new Color(1f, 0.75f, 0.2f));
                }

                return s_portalEntranceTileMaterial;
            case VisualType.PortalExit:
                if (s_portalExitTileMaterial == null) {
                    s_portalExitTileMaterial = CreateMarkerVisualMaterial(new Color(0.45f, 0.95f, 0.75f));
                }

                return s_portalExitTileMaterial;
            case VisualType.HighGround:
                if (s_highGroundTileMaterial == null) {
                    s_highGroundTileMaterial = CreateMarkerVisualMaterial(new Color(0.16f, 0.31f, 0.16f));
                }

                return s_highGroundTileMaterial;
            case VisualType.Hole:
                if (s_holeTileMaterial == null) {
                    s_holeTileMaterial = CreateMarkerVisualMaterial(new Color(0.2f, 0.2f, 0.2f));
                }

                return s_holeTileMaterial;
            case VisualType.Ground:
                if (s_groundTileMaterial == null) {
                    s_groundTileMaterial = CreateMarkerVisualMaterial(new Color(0.39f, 0.78f, 0.39f));
                }

                return s_groundTileMaterial;
            default:
                if (s_forbiddenTileMaterial == null) {
                    s_forbiddenTileMaterial = CreateMarkerVisualMaterial(new Color(1f, 1f, 1f));
                }

                return s_forbiddenTileMaterial;
        }
    }

    private VisualType ResolveVisualType() {
        if (HasSpawnMarker) {
            return VisualType.Spawn;
        }

        if (HasGoalMarker) {
            return VisualType.Goal;
        }

        if (HasPortalEntranceMarker) {
            return VisualType.PortalEntrance;
        }

        if (HasPortalExitMarker) {
            return VisualType.PortalExit;
        }

        switch (tileType) {
            case TileType.HighGround:
                return VisualType.HighGround;
            case TileType.Hole:
                return VisualType.Hole;
            case TileType.Forbidden:
                return VisualType.Forbidden;
            default:
                return VisualType.Ground;
        }
    }

    private static string GetVisualTypeLabel(VisualType visualType) {
        switch (visualType) {
            case VisualType.Spawn:
                return "出生点";
            case VisualType.Goal:
                return "目标点";
            case VisualType.PortalEntrance:
                return "传送入口";
            case VisualType.PortalExit:
                return "传送出口";
            case VisualType.HighGround:
                return "高台";
            case VisualType.Hole:
                return "坑洞";
            case VisualType.Forbidden:
                return "禁用";
            default:
                return "地面";
        }
    }

    public int X => _x;
    
    /// <summary>
    /// 获取 Z 坐标
    /// </summary>
    public int Z => _z;

    public bool HasSpawnMarker => _hasSpawnMarker;

    public bool HasGoalMarker => _hasGoalMarker;

    public bool HasPortalEntranceMarker => ContainsSemanticLabelPrefix("IN");

    public bool HasPortalExitMarker => ContainsSemanticLabelPrefix("OUT");

    public string SpawnMarkerId => _spawnMarkerId;

    public string GoalMarkerId => _goalMarkerId;

    public string SemanticLabel => GetSemanticLabel();

    public string VisualTypeName => GetVisualTypeLabel(ResolveVisualType());
    
    /// <summary>
    /// 设置格子类型（用于笔刷工具）
    /// </summary>
    public void SetTileType(TileType newType) {
        if (tileType == newType) return;
        
        tileType = newType;
        
        // 根据类型自动设置通行性
        walkable = (newType != TileType.Forbidden && newType != TileType.Hole);
        UpdateVisual();
    }
    
    /// <summary>
    /// 设置高度等级（用于笔刷工具）
    /// </summary>
    public void SetHeightLevel(int newHeight) {
        if (heightLevel == newHeight) return;
        
        heightLevel = Mathf.Clamp(newHeight, 0, 3);
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
        return semanticLabel.StartsWith("R", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsGoalSemanticLabel(string semanticLabel) {
        return semanticLabel.StartsWith("B", StringComparison.OrdinalIgnoreCase);
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
