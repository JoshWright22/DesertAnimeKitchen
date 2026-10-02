using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace DessertFactory
{
    // Owns the grid: sand and land tilemaps plus cell <-> world conversion
    [RequireComponent(typeof(Grid))]
    public class DesertMap : MonoBehaviour
    {
        [SerializeField] Tilemap groundLayer;
        [Tooltip("Deposits, and cacti out on the open sand")]
        [SerializeField] Tilemap landLayer;
        [SerializeField] SpriteRenderer gridLines;

        [Header("Desert")]
        [SerializeField] Sprite sand;
        [SerializeField] Sprite sandDune;
        [SerializeField, Range(0f, 1f)] float duneChance = 0.05f;
        [SerializeField] Sprite cactus;
        [SerializeField, Range(0f, 1f)] float cactusChance = 0.012f;

        public int Width { get; private set; }
        public int Height { get; private set; }
        public Grid Grid { get; private set; }

        readonly List<DepositDef> depositTypes = new List<DepositDef>();
        readonly Dictionary<Sprite, Tile> tiles = new Dictionary<Sprite, Tile>();
        int[] depositIndex;
        bool[] cacti;

        void Awake()
        {
            Grid = GetComponent<Grid>();
        }

        public void Generate(int width, int height, List<DepositDef> deposits, int seed, bool scatterDeposits)
        {
            Width = width;
            Height = height;
            depositTypes.Clear();
            depositTypes.AddRange(deposits);
            depositIndex = new int[width * height];
            cacti = new bool[width * height];
            for (int i = 0; i < depositIndex.Length; i++)
                depositIndex[i] = -1;

            var rng = new System.Random(seed);
            float noiseOffset = rng.Next(0, 10000);

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

            for (int i = 0; i < cacti.Length; i++)
                cacti[i] = depositIndex[i] < 0 && rng.NextDouble() < cactusChance;

            DrawGround(rng);
            DrawAllDeposits();
            FitGridLines();
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
                        depositIndex[i] = type;
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
                cacti[i] = false;
            }

            area.xMin--;
            area.yMin--;
            area.xMax++;
            area.yMax++;
            foreach (var pos in area.allPositionsWithin)
                RefreshCell(pos);
        }

        // Building over a cactus clears it away
        public void ClearCactus(Vector2Int cell)
        {
            if (!InBounds(cell) || !cacti[cell.y * Width + cell.x])
                return;
            cacti[cell.y * Width + cell.x] = false;
            RefreshCell(cell);
        }

        void DrawGround(System.Random rng)
        {
            groundLayer.ClearAllTiles();
            var ground = new TileBase[Width * Height];
            for (int i = 0; i < ground.Length; i++)
                ground[i] = TileFor(rng.NextDouble() < duneChance ? sandDune : sand);
            groundLayer.SetTilesBlock(new BoundsInt(0, 0, 0, Width, Height, 1), ground);
        }

        void DrawAllDeposits()
        {
            landLayer.ClearAllTiles();
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                    RefreshCell(new Vector2Int(x, y));
        }

        void RefreshCell(Vector2Int cell)
        {
            if (!InBounds(cell))
                return;

            var pos = new Vector3Int(cell.x, cell.y, 0);
            var deposit = GetDeposit(cell);
            if (deposit == null)
            {
                landLayer.SetTile(pos, cacti[cell.y * Width + cell.x] ? TileFor(cactus) : null);
                return;
            }

            landLayer.SetTile(pos, TileFor(deposit.tiles[EdgeIndex(cell, deposit)]));
        }

        // Picks the edge piece from the 3x3 set by which sides border something else
        int EdgeIndex(Vector2Int cell, DepositDef deposit)
        {
            bool up = GetDeposit(cell + Vector2Int.up) == deposit;
            bool down = GetDeposit(cell + Vector2Int.down) == deposit;
            bool left = GetDeposit(cell + Vector2Int.left) == deposit;
            bool right = GetDeposit(cell + Vector2Int.right) == deposit;

            int row = !up ? 0 : !down ? 2 : 1;
            int column = !left ? 0 : !right ? 2 : 1;
            return row * 3 + column;
        }

        Tile TileFor(Sprite sprite)
        {
            if (!tiles.TryGetValue(sprite, out var tile) || tile == null)
            {
                tile = ScriptableObject.CreateInstance<Tile>();
                tile.sprite = sprite;
                tiles[sprite] = tile;
            }
            return tile;
        }

        void FitGridLines()
        {
            if (gridLines.sprite == null)
                gridLines.sprite = SpriteFactory.GridCell();

            var cellSize = (Vector2)Grid.cellSize;
            gridLines.size = new Vector2(Width, Height);
            gridLines.transform.localScale = new Vector3(cellSize.x, cellSize.y, 1f);
            gridLines.transform.localPosition = new Vector3(Width * cellSize.x / 2f, Height * cellSize.y / 2f, 0f);
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
