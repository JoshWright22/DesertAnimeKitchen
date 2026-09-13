using System.Collections.Generic;
using UnityEngine;

namespace DessertFactory
{
    public enum BuildingKind
    {
        Conveyor,
        Harvester,
        Cook,
        Stall
    }

    [CreateAssetMenu(menuName = "Dessert Factory/Building")]
    public class BuildingDef : ScriptableObject
    {
        public string displayName;
        [TextArea] public string description;
        public BuildingKind kind;
        public int price = 10;

        [Header("Look")]
        public Color outfitColor = Color.white;
        public Color hairColor = Color.black;

        [Header("Work")]
        [Tooltip("Harvesters: seconds per item. Conveyors: seconds to cross one tile.")]
        public float workTime = 1f;
        public bool needsDeposit;
        public List<RecipeDef> recipes = new List<RecipeDef>();
    }
}
