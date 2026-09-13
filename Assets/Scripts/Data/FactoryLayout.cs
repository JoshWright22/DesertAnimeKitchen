using System;
using System.Collections.Generic;
using UnityEngine;

namespace DessertFactory
{
    // A hand built starting setup, handy for test scenes and tutorials
    [CreateAssetMenu(menuName = "Dessert Factory/Factory Layout")]
    public class FactoryLayout : ScriptableObject
    {
        [Serializable]
        public class DepositArea
        {
            public DepositDef deposit;
            public RectInt area;
        }

        [Serializable]
        public class Placement
        {
            public BuildingDef building;
            [Tooltip("Bottom left cell of the building")]
            public Vector2Int cell;
            public Direction facing;
            public int recipe;
        }

        public List<DepositArea> deposits = new List<DepositArea>();
        public List<Placement> buildings = new List<Placement>();

        public void Apply(DesertMap map, Factory factory)
        {
            foreach (var d in deposits)
            {
                if (d.deposit != null)
                    map.PaintDeposit(d.deposit, d.area);
            }

            foreach (var p in buildings)
            {
                if (p.building == null)
                    continue;

                if (!factory.CanPlace(p.building, p.cell, p.facing, out string reason, free: true))
                {
                    Debug.LogWarning($"{name}: can't place {p.building.displayName} at {p.cell} ({reason})");
                    continue;
                }

                var building = factory.Place(p.building, p.cell, p.facing, free: true);
                if (building is CookGirl cook)
                    cook.SetRecipe(p.recipe);
            }
        }
    }
}
