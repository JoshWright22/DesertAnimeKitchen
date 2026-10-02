using UnityEngine;
using UnityEngine.UI;

namespace DessertFactory
{
    // The hands are drawn into the face's own little texture, so they sit on the same pixel grid as the art
    [RequireComponent(typeof(RawImage))]
    public class PixelClock : MonoBehaviour
    {
        [Tooltip("Needs Read/Write turned on in its import settings")]
        [SerializeField] Texture2D face;
        [SerializeField] Color hourColor = new Color(0.29f, 0.16f, 0.07f);
        [SerializeField] Color minuteColor = new Color(0.57f, 0.31f, 0.07f);
        [Tooltip("In pixels from the middle")]
        [SerializeField] int hourLength = 5;
        [SerializeField] int minuteLength = 8;
        [Tooltip("The minute hand jumps in steps this big, like a cheap office clock")]
        [SerializeField] int minuteStep = 5;

        Texture2D canvas;
        Color32[] blank;
        int shownStep = -1;

        void Awake()
        {
            blank = face.GetPixels32();
            canvas = new Texture2D(face.width, face.height, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            GetComponent<RawImage>().texture = canvas;
        }

        void OnDestroy()
        {
            Destroy(canvas);
        }

        public void Show(float hour)
        {
            int step = Mathf.FloorToInt(hour * 60f / minuteStep);
            if (step == shownStep)
                return;
            shownStep = step;

            float minutes = step * minuteStep;
            var pixels = (Color32[])blank.Clone();
            int center = face.width / 2;
            // minute hand under the hour hand, so the short one is always readable
            DrawHand(pixels, center, minutes % 60f / 60f, minuteLength, minuteColor);
            DrawHand(pixels, center, minutes / 60f % 12f / 12f, hourLength, hourColor);
            pixels[center * face.width + center] = hourColor;

            canvas.SetPixels32(pixels);
            canvas.Apply();
        }

        // Plain Bresenham line out from the middle, turned by how far round the dial it is
        void DrawHand(Color32[] pixels, int center, float turn, int length, Color32 color)
        {
            float angle = turn * Mathf.PI * 2f;
            int x1 = center + Mathf.RoundToInt(Mathf.Sin(angle) * length);
            int y1 = center + Mathf.RoundToInt(Mathf.Cos(angle) * length);

            int x = center, y = center;
            int dx = Mathf.Abs(x1 - x), dy = -Mathf.Abs(y1 - y);
            int sx = x < x1 ? 1 : -1, sy = y < y1 ? 1 : -1;
            int err = dx + dy;
            while (true)
            {
                pixels[y * face.width + x] = color;
                if (x == x1 && y == y1)
                    break;
                int e2 = 2 * err;
                if (e2 >= dy)
                {
                    err += dy;
                    x += sx;
                }
                if (e2 <= dx)
                {
                    err += dx;
                    y += sy;
                }
            }
        }
    }
}
