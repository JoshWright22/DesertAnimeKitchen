using System.Collections.Generic;
using UnityEngine;

namespace DessertFactory
{
    [CreateAssetMenu(menuName = "Dessert Factory/Game Content")]
    public class GameContent : ScriptableObject
    {
        public List<ItemDef> items = new List<ItemDef>();
        public List<DepositDef> deposits = new List<DepositDef>();
        public List<RecipeDef> recipes = new List<RecipeDef>();
        public List<BuildingDef> buildings = new List<BuildingDef>();
        public List<CharacterDef> characters = new List<CharacterDef>();
    }
}
