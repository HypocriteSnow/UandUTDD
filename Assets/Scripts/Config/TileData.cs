namespace ArknightsLite.Config {
    using System;
    using ArknightsLite.Model;

    /// <summary>
    /// 单个格子的配置数据
    /// 可序列化，用于在 Inspector 中编辑
    /// </summary>
    [Serializable]
    public class TileData {
        /// <summary>X 坐标（水平方向）</summary>
        public int x;
        
        /// <summary>Z 坐标（纵深方向，俯视图的"竖向"）</summary>
        public int z;
        
        /// <summary>格子类型</summary>
        public TileType tileType = TileType.Ground;
        
        /// <summary>Y 轴高度等级（0=地面，1=高台，可扩展）</summary>
        public int heightLevel = 0;
        
        /// <summary>是否可通行</summary>
        public bool walkable = true;
        
        /// <summary>可部署标签</summary>
        public string deployTag = "All";
    }
}
