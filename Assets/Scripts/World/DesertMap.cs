using System.Collections.Generic;
using UnityEngine;

namespace DessertFactory
{
    public class DesertMap : MonoBehaviour
    {
        public int Width { get; private set; }
        public int Height { get; private set; }

        List<DepositDef> depositTypes;
        int[] depositIndex;
        int[] depositAmount;
        Texture2D groundTexture;
        Color[] sandColors;

        public void Generate(int width, int height, List<DepositDef> deposits, int seed)
        {
            Width = width;
            Height = height;
            depositTypes = deposits;
            depositIndex = new int[width * height];
            depositAmount = new int[width * height];
            sandColors = new Color[width * height];

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

            BuildGroundSprite();
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

        void BuildGroundSprite()
        {
            groundTexture = new Texture2D(Width, Height, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };

            var pixels = new Color[Width * Height];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = TileColor(i);
            groundTexture.SetPixels(pixels);
            groundTexture.Apply();

            var sr = gameObject.AddComponent<SpriteRenderer>();
            sr.sprite = Sprite.Create(groundTexture, new Rect(0, 0, Width, Height), Vector2.zero, 1f);
            sr.sortingOrder = -10;

            // tile (x, y) is centered on world (x, y)
            transform.position = new Vector3(-0.5f, -0.5f, 0f);
        }

        Color TileColor(int i)
        {
            if (depositIndex[i] < 0)
                return sandColors[i];

            // speckle the deposit a bit so it reads as stuff in the sand
            var color = depositTypes[depositIndex[i]].groundColor;
            int x = i % Width;
            int y = i / Width;
            return (x * 7 + y * 13) % 5 == 0 ? Color.Lerp(color, sandColors[i], 0.5f) : color;
        }

        public bool InBounds(Vector2Int tile)
        {
            return tile.x >= 0 && tile.y >= 0 && tile.x < Width && tile.y < Height;
        }

        public DepositDef GetDeposit(Vector2Int tile)
        {
            if (!InBounds(tile))
                return null;
            int index = depositIndex[tile.y * Width + tile.x];
            return index < 0 ? null : depositTypes[index];
        }

        public int GetAmount(Vector2Int tile)
        {
            return InBounds(tile) ? depositAmount[tile.y * Width + tile.x] : 0;
        }

        public bool TryDig(Vector2Int tile, out ItemDef item)
        {
            item = null;
            var deposit = GetDeposit(tile);
            if (deposit == null)
                return false;

            int i = tile.y * Width + tile.x;
            item = deposit.item;
            depositAmount[i]--;

            if (depositAmount[i] <= 0)
            {
                depositIndex[i] = -1;
                groundTexture.SetPixel(tile.x, tile.y, sandColors[i]);
                groundTexture.Apply();
            }
            return true;
        }

        public static Vector2Int WorldToTile(Vector3 world)
        {
            return new Vector2Int(Mathf.RoundToInt(world.x), Mathf.RoundToInt(world.y));
        }

        public static Vector3 TileToWorld(Vector2Int tile)
        {
            return new Vector3(tile.x, tile.y, 0f);
        }
    }
}
