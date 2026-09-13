using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace DessertFactory
{
    // Owns the grid: ground and deposit tilemaps plus cell <-> world conversion
    [RequireComponent(typeof(Grid))]
    public class DesertMap : MonoBehaviour
    {
        [SerializeField] Tilemap groundLayer;
        [SerializeField] Tilemap depositLayer;
        [Tooltip("Sprite used for every ground/deposit tile. Tinted per cell. Leave empty for a plain square.")]
        [SerializeField] Sprite tileSprite;

        public int Width { get; private set; }
        public int Height { get; private set; }
        public Grid Grid { get; private set; }

        readonly List<DepositDef> depositTypes = new List<DepositDef>();
        int[] depositIndex;
        int[] depositAmount;
        Color[] sandColors;
        Tile tile;
        SpriteRenderer gridLines;

        public bool ShowGridLines
        {
            get => gridLines != null && gridLines.enabled;
            set { if (gridLines != null) gridLines.enabled = value; }
        }

        public void Generate(int width, int height, List<DepositDef> deposits, int seed, bool scatterDeposits)
        {
            Width = width;
            Height = height;
            Grid = GetComponent<Grid>();
            depositTypes.Clear();
            depositTypes.AddRange(deposits);
            depositIndex = new int[width * height];
            depositAmount = new int[width * height];
            sandColors = new Color[width * height];

            SetUpLayers();

            var rng = new System.Random(seed);
            float noiseOffset = rng.Next(0, 10000);

            var sandLight = new Color(0.93f, 0.8f, 0.55f);
            var sandDark = new Color(0.82f, 0.66f, 0.42f);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // two octaves so the dunes don't look too smooth
                    float n = Mathf.PerlinNoise(noiseOffset + x * 0.05f, noiseOffset + y * 0.05f) * 0.7f
                              + Mathf.PerlinNoise(noiseOffset + x * 0.25f, noiseOffset + y * 0.25f) * 0.3f;
                    sandColors[y * width + x] = Color.Lerp(sandDark, sandLight, n);
                    depositIndex[y * width + x] = -1;
                }
            }

            if (scatterDeposits)
            {
                var center = new Vector2(width / 2f, height / 2f);
                for (int d = 0; d < deposits.Count; d++)
                {
                    for (int p = 0; p < deposits[d].patchCount; p++)
                    {
                        // first patch of every type lands near the middle so the start is always playable
                        float spread = p == 0 ? 14f : Mathf.Min(width, height) * 0.45f;
                        var patchCenter = center + new Vector2(
                            (float)(rng.NextDouble() * 2 - 1) * spread,
                            (float)(rng.NextDouble() * 2 - 1) * spread);
                        PaintPatch(d, patchCenter, deposits[d], noiseOffset + d * 31.7f + p * 7.3f);
                    }
                }
            }

            DrawGround();
            DrawAllDeposits();
            CreateGridLines();
        }

        void SetUpLayers()
        {
            if (groundLayer == null)
                groundLayer = CreateLayer("Ground", -10);
            if (depositLayer == null)
                depositLayer = CreateLayer("Deposits", -9);

            tile = ScriptableObject.CreateInstance<Tile>();
            tile.sprite = tileSprite != null ? tileSprite : SpriteFactory.Square();
            tile.flags = TileFlags.None;
        }

        Tilemap CreateLayer(string layerName, int sortingOrder)
        {
            var go = new GameObject(layerName);
            go.transform.SetParent(transform, false);
            var tilemap = go.AddComponent<Tilemap>();
            go.AddComponent<TilemapRenderer>().sortingOrder = sortingOrder;
            return tilemap;
        }

        void PaintPatch(int type, Vector2 patchCenter, DepositDef deposit, float noiseSeed)
        {
            int r = Mathf.CeilToInt(deposit.patchRadius * 1.5f);
            for (int y = (int)patchCenter.y - r; y <= (int)patchCenter.y + r; y++)
            {
                for (int x = (int)patchCenter.x - r; x <= (int)patchCenter.x + r; x++)
                {
                    if (!InBounds(new Vector2Int(x, y)))
                        continue;

                    int i = y * Width + x;
                    if (depositIndex[i] != -1)
                        continue;

                    float wobble = 0.6f + Mathf.PerlinNoise(noiseSeed + x * 0.3f, noiseSeed + y * 0.3f) * 0.8f;
                    float dist = Vector2.Distance(new Vector2(x, y), patchCenter);
                    if (dist <= deposit.patchRadius * wobble)
                    {
                        depositIndex[i] = type;
                        depositAmount[i] = deposit.amountPerTile;
                    }
                }
            }
        }

        // Used by hand made layouts to put a deposit exactly where they want it
        public void PaintDeposit(DepositDef deposit, RectInt area)
        {
            int type = depositTypes.IndexOf(deposit);
            if (type < 0)
            {
                depositTypes.Add(deposit);
                type = depositTypes.Count - 1;
            }

            foreach (var pos in area.allPositionsWithin)
            {
                if (!InBounds(pos))
                    continue;
                int i = pos.y * Width + pos.x;
                depositIndex[i] = type;
                depositAmount[i] = deposit.amountPerTile;
                RefreshDepositCell(pos);
            }
        }

        void DrawGround()
        {
            groundLayer.ClearAllTiles();
            var tiles = new TileBase[Width * Height];
            for (int i = 0; i < tiles.Length; i++)
                tiles[i] = tile;
            groundLayer.SetTilesBlock(new BoundsInt(0, 0, 0, Width, Height, 1), tiles);

            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                    groundLayer.SetColor(new Vector3Int(x, y, 0), sandColors[y * Width + x]);
        }

        void DrawAllDeposits()
        {
            depositLayer.ClearAllTiles();
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                    RefreshDepositCell(new Vector2Int(x, y));
        }

        void RefreshDepositCell(Vector2Int cell)
        {
            var pos = new Vector3Int(cell.x, cell.y, 0);
            int i = cell.y * Width + cell.x;
            if (depositIndex[i] < 0)
            {
                depositLayer.SetTile(pos, null);
                return;
            }

            // speckle the deposit a bit so it reads as stuff in the sand
            var color = depositTypes[depositIndex[i]].groundColor;
            if ((cell.x * 7 + cell.y * 13) % 5 == 0)
                color = Color.Lerp(color, sandColors[i], 0.5f);

            depositLayer.SetTile(pos, tile);
            depositLayer.SetColor(pos, color);
        }

        void CreateGridLines()
        {
            if (gridLines == null)
            {
                gridLines = new GameObject("Grid Lines").AddComponent<SpriteRenderer>();
                gridLines.transform.SetParent(transform, false);
                gridLines.sprite = SpriteFactory.GridCell();
                gridLines.drawMode = SpriteDrawMode.Tiled;
                gridLines.sortingOrder = -5;
            }

            var cellSize = (Vector2)Grid.cellSize;
            gridLines.size = new Vector2(Width, Height);
            gridLines.transform.localScale = new Vector3(cellSize.x, cellSize.y, 1f);
            gridLines.transform.localPosition = new Vector3(Width * cellSize.x / 2f, Height * cellSize.y / 2f, 0f);
            gridLines.enabled = false;
        }

        public bool InBounds(Vector2Int cell)
        {
            return cell.x >= 0 && cell.y >= 0 && cell.x < Width && cell.y < Height;
        }

        public DepositDef GetDeposit(Vector2Int cell)
        {
            if (!InBounds(cell))
                return null;
            int index = depositIndex[cell.y * Width + cell.x];
            return index < 0 ? null : depositTypes[index];
        }

        public int GetAmount(Vector2Int cell)
        {
            return InBounds(cell) ? depositAmount[cell.y * Width + cell.x] : 0;
        }

        public bool TryDig(Vector2Int cell, out ItemDef item)
        {
            item = null;
            var deposit = GetDeposit(cell);
            if (deposit == null)
                return false;

            int i = cell.y * Width + cell.x;
            item = deposit.item;
            depositAmount[i]--;

            if (depositAmount[i] <= 0)
            {
                depositIndex[i] = -1;
                RefreshDepositCell(cell);
            }
            return true;
        }

        public Vector2Int WorldToCell(Vector3 world)
        {
            var cell = Grid.WorldToCell(world);
            return new Vector2Int(cell.x, cell.y);
        }

        public Vector3 CellToWorld(Vector2Int cell)
        {
            return Grid.GetCellCenterWorld(new Vector3Int(cell.x, cell.y, 0));
        }

        public Vector3 FootprintCenter(Vector2Int origin, Vector2Int size)
        {
            return Vector3.Lerp(CellToWorld(origin), CellToWorld(origin + size - Vector2Int.one), 0.5f);
        }

        public Vector2 WorldSize => new Vector2(Width * Grid.cellSize.x, Height * Grid.cellSize.y);
    }
}
