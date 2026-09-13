using UnityEngine;

namespace DessertFactory
{
    [CreateAssetMenu(menuName = "Dessert Factory/Item")]
    public class ItemDef : ScriptableObject
    {
        public string displayName;
        public Color color = Color.white;

        // Desserts get sold when they reach a stall, everything else just goes into storage
        public bool isDessert;
        public int sellPrice;
    }
}
