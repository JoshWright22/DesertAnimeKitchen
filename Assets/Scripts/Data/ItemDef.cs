using UnityEngine;

namespace DessertFactory
{
    [CreateAssetMenu(menuName = "Dessert Factory/Item")]
    public class ItemDef : ScriptableObject
    {
        public string displayName;
        public Color color = Color.white;
        [Tooltip("Optional, a tinted dot is drawn if empty")]
        public Sprite icon;
        [Tooltip("Glint frames played over the icon now and then, in order")]
        public Sprite[] sparkle = new Sprite[0];
        [Tooltip("Seconds between glints")]
        public float sparkleEvery = 1.6f;
        public float sparkleFrameTime = 0.09f;

        // Anything that reaches a stall sells. The more girls it went through on the way, the more it's worth.
        [Tooltip("Coins at the stall. Raw ore is cheap, each recipe is worth more than everything that went into it.")]
        public int sellPrice;
        [Tooltip("Stars for the gacha, given on top of the coins")]
        public int stars;

        public bool Sparkles => icon != null && sparkle.Length > 0;

        // The icon most of the time, with a quick run through the glint frames once every sparkleEvery seconds
        public Sprite IconAt(float time)
        {
            if (!Sparkles)
                return icon;
            int frame = Mathf.FloorToInt(Mathf.Repeat(time, sparkleEvery) / sparkleFrameTime);
            return frame < sparkle.Length ? sparkle[frame] : icon;
        }
    }
}
