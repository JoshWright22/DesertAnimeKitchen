using System.Collections.Generic;
using UnityEngine;

namespace DessertFactory
{
    // Placeholder art drawn in code until we have real sprites
    public static class SpriteFactory
    {
        const int Size = 32;

        // texels per cell in the grid line textures
        public const int GridTexels = Size;

        static readonly Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();

        public static Sprite Square()
        {
            return Cached("square", () =>
            {
                var tex = NewTexture(4);
                Fill(tex, Color.white);
                return Finish(tex);
            });
        }

        public static Sprite Circle()
        {
            return Cached("circle", () =>
            {
                var tex = NewTexture(Size);
                DrawCircle(tex, 15.5f, 15.5f, 15f, Color.white);
                return Finish(tex);
            });
        }

        // One cell with lines on the left and bottom, meant for tiled drawing
        public static Sprite GridCell()
        {
            return Cached("gridcell", () =>
            {
                var tex = NewTexture(Size);
                var line = new Color(0.2f, 0.12f, 0.05f, 0.35f);
                for (int i = 0; i < Size; i++)
                {
                    tex.SetPixel(i, 0, line);
                    tex.SetPixel(0, i, line);
                }
                tex.Apply();
                return Sprite.Create(tex, new Rect(0, 0, Size, Size), new Vector2(0.5f, 0.5f), Size, 0, SpriteMeshType.FullRect);
            });
        }

        // Border around the whole cell, tinted per deposit
        public static Sprite CellOutline()
        {
            return Cached("celloutline", () =>
            {
                var tex = NewTexture(Size);
                for (int i = 0; i < Size; i++)
                {
                    for (int w = 0; w < 2; w++)
                    {
                        tex.SetPixel(i, w, Color.white);
                        tex.SetPixel(i, Size - 1 - w, Color.white);
                        tex.SetPixel(w, i, Color.white);
                        tex.SetPixel(Size - 1 - w, i, Color.white);
                    }
                }
                return Finish(tex);
            });
        }

        public static Sprite Arrow()
        {
            return Cached("arrow", () =>
            {
                var tex = NewTexture(Size);
                for (int y = 4; y < 28; y++)
                {
                    int halfWidth = (28 - y) / 2;
                    for (int x = 16 - halfWidth; x < 16 + halfWidth; x++)
                        tex.SetPixel(x, y, Color.white);
                }
                return Finish(tex);
            });
        }

        public static Sprite Belt()
        {
            return Cached("belt", () =>
            {
                var baseColor = new Color(0.35f, 0.33f, 0.32f);
                var tex = NewTexture(Size);
                var light = Color.Lerp(baseColor, Color.white, 0.35f);
                var edge = Color.Lerp(baseColor, Color.black, 0.4f);

                for (int y = 0; y < Size; y++)
                {
                    for (int x = 0; x < Size; x++)
                    {
                        if (x < 3 || x > Size - 4)
                        {
                            tex.SetPixel(x, y, edge);
                            continue;
                        }

                        int d = Mathf.FloorToInt(Mathf.Abs(x - 15.5f));
                        bool chevron = (y + d) % 10 < 2;
                        tex.SetPixel(x, y, chevron ? light : baseColor);
                    }
                }
                return Finish(tex);
            });
        }

        // With domain reload turned off the cache survives between play sessions,
        // but the sprites in it get destroyed when play mode ends
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ClearCache()
        {
            cache.Clear();
        }

        static Sprite Cached(string key, System.Func<Sprite> create)
        {
            if (!cache.TryGetValue(key, out var sprite) || sprite == null)
            {
                sprite = create();
                cache[key] = sprite;
            }
            return sprite;
        }

        static Texture2D NewTexture(int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            Fill(tex, Color.clear);
            return tex;
        }

        static Sprite Finish(Texture2D tex)
        {
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), tex.width);
        }

        static void Fill(Texture2D tex, Color color)
        {
            var pixels = new Color[tex.width * tex.height];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = color;
            tex.SetPixels(pixels);
        }

        static void DrawCircle(Texture2D tex, float cx, float cy, float r, Color color)
        {
            for (int y = 0; y < tex.height; y++)
            {
                for (int x = 0; x < tex.width; x++)
                {
                    float dx = x - cx;
                    float dy = y - cy;
                    if (dx * dx + dy * dy <= r * r)
                        tex.SetPixel(x, y, color);
                }
            }
        }
    }
}
