using System.Collections.Generic;
using UnityEngine;

namespace DessertFactory
{
    [CreateAssetMenu(menuName = "Dessert Factory/Building")]
    public class BuildingDef : ScriptableObject
    {
        public string displayName;
        [TextArea] public string description;
        public Building prefab;
        public int price = 10;
        [Tooltip("Footprint in cells when facing up")]
        public Vector2Int size = Vector2Int.one;

        [Header("Look")]
        [Tooltip("Swaps out the prefab's sprite, so two buildings can share one prefab and still look different")]
        public Sprite sprite;

        [Header("Work")]
        [Tooltip("Miners: seconds per item. Conveyors: seconds to cross one tile.")]
        public float workTime = 1f;
        public bool needsDeposit;
        public List<RecipeDef> recipes = new List<RecipeDef>();
    }
}
