using System.Collections.Generic;
using UnityEngine;

namespace DessertFactory
{
    public class Factory : MonoBehaviour
    {
        public DesertMap Map { get; private set; }
        public Stockpile Stockpile { get; private set; }
        public ItemViewPool ItemViews { get; private set; }

        readonly Dictionary<Vector2Int, Building> buildings = new Dictionary<Vector2Int, Building>();
        readonly List<Building> tickOrder = new List<Building>();

        public void Init(DesertMap map, Stockpile stockpile)
        {
            Map = map;
            Stockpile = stockpile;
            ItemViews = new GameObject("Item Views").AddComponent<ItemViewPool>();
            ItemViews.transform.SetParent(transform, false);
        }

        void Update()
        {
            float dt = Time.deltaTime;
            for (int i = 0; i < tickOrder.Count; i++)
                tickOrder[i].Tick(dt);
        }

        public Building GetBuilding(Vector2Int tile)
        {
            buildings.TryGetValue(tile, out var building);
            return building;
        }

        public bool CanPlace(BuildingDef def, Vector2Int tile, out string reason)
        {
            reason = null;
            if (!Map.InBounds(tile))
                reason = "Out of bounds";
            else if (buildings.ContainsKey(tile))
                reason = "Something is already here";
            else if (def.needsDeposit && Map.GetDeposit(tile) == null)
                reason = "Needs to go on a deposit";
            else if (Stockpile.Coins < def.price)
                reason = "Not enough coins";
            return reason == null;
        }

        public Building Place(BuildingDef def, Vector2Int tile, Direction facing)
        {
            if (!CanPlace(def, tile, out _) || !Stockpile.TrySpend(def.price))
                return null;

            var go = new GameObject($"{def.displayName} {tile}");
            go.transform.SetParent(transform, false);

            Building building;
            switch (def.kind)
            {
                case BuildingKind.Conveyor: building = go.AddComponent<Conveyor>(); break;
                case BuildingKind.Harvester: building = go.AddComponent<HarvesterGirl>(); break;
                case BuildingKind.Cook: building = go.AddComponent<CookGirl>(); break;
                default: building = go.AddComponent<DessertStall>(); break;
            }

            building.Init(this, def, tile, facing);
            buildings[tile] = building;
            tickOrder.Add(building);
            return building;
        }

        public void Remove(Vector2Int tile)
        {
            if (!buildings.TryGetValue(tile, out var building))
                return;

            building.OnRemoved();
            buildings.Remove(tile);
            tickOrder.Remove(building);
            Stockpile.AddCoins(building.Def.price);
            Destroy(building.gameObject);
        }
    }
}
