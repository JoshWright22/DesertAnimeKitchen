using System.Collections.Generic;
using UnityEngine;

namespace DessertFactory
{
    // A patch of land you can mine forever. The tiles are the ore, so you can tell what's under a girl by looking.
    [CreateAssetMenu(menuName = "Dessert Factory/Deposit")]
    public class DepositDef : ScriptableObject
    {
        public string displayName;
        [Tooltip("Mined in turn, so a land with two ores gives both")]
        public List<ItemDef> items = new List<ItemDef>();
        [Tooltip("3x3 set, left to right then top to bottom. The middle one fills the inside, the rest are the sandy edges and corners.")]
        public Sprite[] tiles = new Sprite[9];
        public int patchCount = 4;
        public float patchRadius = 4f;
    }
}
