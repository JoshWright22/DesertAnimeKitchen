using UnityEngine;

namespace DessertFactory
{
    [CreateAssetMenu(menuName = "Dessert Factory/Deposit")]
    public class DepositDef : ScriptableObject
    {
        public ItemDef item;
        public Color groundColor = Color.white;
        public int patchCount = 4;
        public float patchRadius = 4f;
        public int amountPerTile = 300;
    }
}
