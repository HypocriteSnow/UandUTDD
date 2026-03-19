namespace ArknightsLite.Editor.LevelEditor.Core {
    using System;
    using System.Collections.Generic;
    using ArknightsLite.Config;
    using ArknightsLite.Model;

    [Serializable]
    public sealed class LevelEditorWorkspace {
        public string LevelName = string.Empty;
        public int MapWidth = 10;
        public int MapDepth = 10;
        public float CellSize = 1f;
        public TileType DefaultTileType = TileType.Ground;
        public LevelRuntimeParameters Runtime = new LevelRuntimeParameters();
        public List<TileData> TileOverrides = new List<TileData>();
        public List<PortalDefinition> Portals = new List<PortalDefinition>();
        public List<WaveDefinition> Waves = new List<WaveDefinition>();
        public List<EnemyTemplateData> Enemies = new List<EnemyTemplateData>();
        public List<OperatorTemplateData> Operators = new List<OperatorTemplateData>();

        public static LevelEditorWorkspace CreateNew(string levelName) {
            return new LevelEditorWorkspace {
                LevelName = string.IsNullOrWhiteSpace(levelName) ? "NewLevel" : levelName,
                Runtime = new LevelRuntimeParameters()
            };
        }

        public TileData GetTileOverride(int x, int z) {
            var existing = TileOverrides.Find(tile => tile.x == x && tile.z == z);
            return existing ?? CreateDefaultTile(x, z);
        }

        public void SetTileOverride(int x, int z, TileData tileData) {
            var existing = TileOverrides.Find(tile => tile.x == x && tile.z == z);
            var normalized = tileData ?? CreateDefaultTile(x, z);
            normalized.x = x;
            normalized.z = z;

            bool isDefaultTile = normalized.tileType == DefaultTileType && normalized.heightLevel == 0;
            if (isDefaultTile) {
                if (existing != null) {
                    TileOverrides.Remove(existing);
                }

                return;
            }

            if (existing != null) {
                existing.tileType = normalized.tileType;
                existing.heightLevel = normalized.heightLevel;
                existing.walkable = normalized.walkable;
                existing.deployTag = normalized.deployTag;
                return;
            }

            TileOverrides.Add(new TileData {
                x = x,
                z = z,
                tileType = normalized.tileType,
                heightLevel = normalized.heightLevel,
                walkable = normalized.walkable,
                deployTag = normalized.deployTag
            });
        }

        private TileData CreateDefaultTile(int x, int z) {
            return new TileData {
                x = x,
                z = z,
                tileType = DefaultTileType,
                heightLevel = 0,
                walkable = DefaultTileType != TileType.Forbidden && DefaultTileType != TileType.Hole,
                deployTag = "All"
            };
        }
    }
}
