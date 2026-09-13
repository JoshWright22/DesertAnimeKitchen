using UnityEngine;

namespace DessertFactory
{
    [CreateAssetMenu(menuName = "Dessert Factory/Deposit")]
    public class DepositDef : ScriptableObject
    {
        public ItemDef item;
        public Color groundColor = Color.white;
        [Tooltip("Outline drawn around each cell of this deposit so it stands out on the grid")]
        public Color gridColor = Color.white;
        public int patchCount = 4;
        public float patchRadius = 4f;
        public int amountPerTile = 300;
    }
}
