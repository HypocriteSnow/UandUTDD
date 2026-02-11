namespace ArknightsLite.Model {
    
    /// <summary>
    /// 格子类型枚举
    /// </summary>
    public enum TileType {
        /// <summary>地面 - 可部署近战干员</summary>
        Ground,
        
        /// <summary>高台 - 可部署远程干员</summary>
        HighGround,
        
        /// <summary>禁区 - 不可部署任何单位</summary>
        Forbidden,
        
        /// <summary>坑洞 - 地面单位不可通行，空军可通过</summary>
        Hole
    }
}
