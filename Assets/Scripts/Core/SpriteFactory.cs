using System.Collections.Generic;
using UnityEngine;

namespace DessertFactory
{
    // Placeholder art drawn in code until we have real sprites
    public static class SpriteFactory
    {
        const int Size = 32;

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

        public static Sprite Belt(Color baseColor)
        {
            return Cached("belt" + baseColor, () =>
            {
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

        public static Sprite Girl(Color outfit, Color hair)
        {
            return Cached("girl" + outfit + hair, () =>
            {
                var tex = NewTexture(Size);
                var skin = new Color(1f, 0.87f, 0.77f);
                var floor = Color.Lerp(outfit, Color.black, 0.55f);
                floor.a = 0.6f;

                // work spot
                DrawRect(tex, 1, 1, 30, 30, floor);

                // dress
                for (int y = 3; y < 13; y++)
                {
                    int halfWidth = 4 + (13 - y) / 2;
                    for (int x = 16 - halfWidth; x < 16 + halfWidth; x++)
                        tex.SetPixel(x, y, outfit);
                }

                // twin tails, hair, then face on top leaving the bangs
                DrawCircle(tex, 6.5f, 16f, 3.5f, hair);
                DrawCircle(tex, 25.5f, 16f, 3.5f, hair);
                DrawCircle(tex, 16f, 20f, 8.5f, hair);
                for (int y = 12; y <= 21; y++)
                {
                    for (int x = 9; x < 23; x++)
                    {
                        float dx = x - 15.5f;
                        float dy = y - 18f;
                        if (dx * dx + dy * dy <= 6.5f * 6.5f)
                            tex.SetPixel(x, y, skin);
                    }
                }

                var eye = new Color(0.15f, 0.1f, 0.25f);
                DrawRect(tex, 12, 16, 2, 3, eye);
                DrawRect(tex, 18, 16, 2, 3, eye);
                tex.SetPixel(15, 13, new Color(0.9f, 0.4f, 0.45f));
                tex.SetPixel(16, 13, new Color(0.9f, 0.4f, 0.45f));

                return Finish(tex);
            });
        }

        static Sprite Cached(string key, System.Func<Sprite> create)
        {
            if (!cache.TryGetValue(key, out var sprite))
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

        static void DrawRect(Texture2D tex, int x, int y, int w, int h, Color color)
        {
            for (int py = y; py < y + h; py++)
                for (int px = x; px < x + w; px++)
                    tex.SetPixel(px, py, color);
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
