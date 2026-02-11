namespace ArknightsLite.Config {
    using UnityEngine;
    using ArknightsLite.Model;
    using System;

    /// <summary>
    /// 网格视觉配置 - 定义不同格子类型的视觉表现
    /// </summary>
    [CreateAssetMenu(fileName = "GridVisualConfig", menuName = "ArknightsLite/Grid Visual Config", order = 2)]
    public class GridVisualConfig : ScriptableObject {
        
        [Header("格子预制体")]
        [Tooltip("基础格子预制体（包含 MeshRenderer 的 Cube）")]
        public GameObject tilePrefab;
        
        [Header("格子材质配置")]
        [Tooltip("不同类型格子的材质")]
        public TileVisualData[] tileVisuals;
        
        [Header("特殊标记材质")]
        [Tooltip("起点标记材质")]
        public Material spawnPointMaterial;
        
        [Tooltip("终点标记材质")]
        public Material goalPointMaterial;
        
        [Tooltip("占据状态材质")]
        public Material occupiedMaterial;
        
        
        /// <summary>
        /// 根据类型获取材质
        /// </summary>
        public Material GetMaterialForType(TileType type) {
            foreach (var visual in tileVisuals) {
                if (visual.tileType == type) {
                    return visual.material;
                }
            }
            return null;
        }
    }
    
    /// <summary>
    /// 格子视觉数据
    /// </summary>
    [Serializable]
    public class TileVisualData {
        public TileType tileType;
        public Material material;
    }
}
