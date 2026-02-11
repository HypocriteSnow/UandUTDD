namespace ArknightsLite.Model {
    
    /// <summary>
    /// 格子数据类 - 存储单个格子的静态与动态属性
    /// 纯数据类，不包含逻辑
    /// </summary>
    public class Tile {
        // ===== 静态属性 (初始化后不变) =====
        
        /// <summary>逻辑坐标 X（水平方向）</summary>
        public int X { get; private set; }
        
        /// <summary>逻辑坐标 Z（纵深方向）</summary>
        public int Z { get; private set; }
        
        /// <summary>格子类型</summary>
        public TileType Type { get; private set; }
        
        /// <summary>高度等级（Y轴高度：0=地面，1=高台，可扩展）</summary>
        public int HeightLevel { get; private set; }
        
        /// <summary>基础通行性（不考虑占据者）</summary>
        public bool BaseWalkable { get; private set; }
        
        /// <summary>可部署标签 - 用于限制哪些类型的干员可以部署</summary>
        public string DeployableTag { get; private set; }
        
        
        // ===== 动态属性 (运行时变化) =====
        
        /// <summary>当前占据者（干员、敌人、障碍物等）</summary>
        public object Occupier { get; private set; }
        
        
        // ===== 计算属性 =====
        
        /// <summary>
        /// 当前是否可通行
        /// 规则：基础通行性 && 没有阻挡型占据者
        /// </summary>
        public bool IsWalkable {
            get {
                if (!BaseWalkable) return false;
                // 扩展点：未来可根据 Occupier 的 IsBlocker 属性判断
                return Occupier == null;
            }
        }
        
        /// <summary>
        /// 当前是否可部署
        /// 规则：无占据者 && 格子类型允许部署
        /// </summary>
        public bool IsDeployable {
            get {
                if (Occupier != null) return false;
                return Type == TileType.Ground || Type == TileType.HighGround;
            }
        }
        
        
        // ===== 构造函数 =====
        
        public Tile(int x, int z, TileType type, int heightLevel = 0, bool walkable = true, string deployTag = "All") {
            X = x;
            Z = z;
            Type = type;
            HeightLevel = heightLevel;
            BaseWalkable = walkable;
            DeployableTag = deployTag;
            Occupier = null;
        }
        
        
        // ===== 公共方法 =====
        
        /// <summary>
        /// 设置占据者（内部使用，由 GridModel 调用）
        /// </summary>
        internal void SetOccupier(object occupier) {
            Occupier = occupier;
        }
        
        /// <summary>
        /// 清除占据者
        /// </summary>
        internal void ClearOccupier() {
            Occupier = null;
        }
    }
}
