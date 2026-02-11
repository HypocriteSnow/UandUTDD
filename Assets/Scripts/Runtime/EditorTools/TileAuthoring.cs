#if UNITY_EDITOR
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
    
    [Header("坐标信息（只读）")]
    [SerializeField] private int _x;
    [SerializeField] private int _z;
    
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
    
    
    // ==================== 初始化 ====================
    
    /// <summary>
    /// 初始化格子（由 LevelEditorWindow 调用）
    /// </summary>
    public void Initialize(int x, int z, LevelConfig levelConfig, GridVisualConfig gridVisualConfig) {
        _x = x;
        _z = z;
        config = levelConfig;
        visualConfig = gridVisualConfig;
        
        // 从配置加载初始数据
        LoadFromConfig();
        UpdateVisual();
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
        if (config == null) return;
        SyncToConfig();
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
        
        if (config != null) {
            if (config.IsSpawnPoint(_x, _z) && visualConfig.spawnPointMaterial != null) {
                targetMaterial = visualConfig.spawnPointMaterial;
            }
            else if (config.IsGoalPoint(_x, _z) && visualConfig.goalPointMaterial != null) {
                targetMaterial = visualConfig.goalPointMaterial;
            }
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
        if (config == null) return;
        
        Vector3 pos = transform.position;
        pos.x = _x * config.cellSize;
        pos.y = heightLevel * config.cellSize + 0.1f; // 略微抬高避免 Z-Fighting
        pos.z = _z * config.cellSize;
        transform.position = pos;
    }
    
    
    // ==================== 公共接口 ====================
    
    /// <summary>
    /// 获取 X 坐标
    /// </summary>
    public int X => _x;
    
    /// <summary>
    /// 获取 Z 坐标
    /// </summary>
    public int Z => _z;
    
    /// <summary>
    /// 设置格子类型（用于笔刷工具）
    /// </summary>
    public void SetTileType(TileType newType) {
        if (tileType == newType) return;
        
        tileType = newType;
        
        // 根据类型自动设置通行性
        walkable = (newType != TileType.Forbidden && newType != TileType.Hole);
        
        SyncToConfig();
        UpdateVisual();
    }
    
    /// <summary>
    /// 设置高度等级（用于笔刷工具）
    /// </summary>
    public void SetHeightLevel(int newHeight) {
        if (heightLevel == newHeight) return;
        
        heightLevel = Mathf.Clamp(newHeight, 0, 3);
        SyncToConfig();
        UpdateVisual();
    }
    
    
    // ==================== 调试信息 ====================
    
    private void OnDrawGizmosSelected() {
        // 显示坐标信息
        Gizmos.color = Color.yellow;
        Vector3 labelPos = transform.position + Vector3.up * 0.5f;
        
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(labelPos, $"({_x}, {_z})\n{tileType}");
        #endif
    }
}
#endif
