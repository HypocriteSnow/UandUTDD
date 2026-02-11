namespace ArknightsLite.Core {
    using UnityEngine;

    /// <summary>
    /// 游戏核心事件定义
    /// 遵循强类型事件规范，使用 struct 避免 GC
    /// </summary>
    public static class GameEvents {
        
        /// <summary>
        /// 网格变化事件 - 当格子的占据状态改变时触发
        /// </summary>
        public struct GridChangedEvent {
            public int X;
            public int Y;
            public object Occupier; // 避免循环依赖，使用 object
        }

        /// <summary>
        /// 格子可部署状态变化事件
        /// </summary>
        public struct TileDeployableChangedEvent {
            public int X;
            public int Y;
            public bool IsDeployable;
        }
    }
}
