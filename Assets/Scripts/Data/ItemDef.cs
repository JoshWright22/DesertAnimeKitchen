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

        // Desserts get sold when they reach a stall, everything else just goes into storage
        public bool isDessert;
        public int sellPrice;
        [Tooltip("Stars for the gacha, given on top of the coins")]
        public int stars;
    }
}
