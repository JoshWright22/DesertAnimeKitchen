using System.Collections.Generic;
using UnityEngine;

namespace DessertFactory
{
    public class Factory : MonoBehaviour
    {
        public DesertMap Map { get; private set; }
        public Stockpile Stockpile { get; private set; }
        public ItemViewPool ItemViews { get; private set; }

        // every cell a building covers points back at it
        readonly Dictionary<Vector2Int, Building> occupied = new Dictionary<Vector2Int, Building>();
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

        public Building GetBuilding(Vector2Int cell)
        {
            occupied.TryGetValue(cell, out var building);
            return building;
        }

        public bool CanPlace(BuildingDef def, Vector2Int origin, Direction facing, out string reason, bool free = false)
        {
            reason = null;
            var size = Building.RotatedSize(def.size, facing);
            bool onDeposit = false;

            for (int y = 0; y < size.y && reason == null; y++)
            {
                for (int x = 0; x < size.x; x++)
                {
                    var cell = new Vector2Int(origin.x + x, origin.y + y);
                    if (!Map.InBounds(cell))
                    {
                        reason = "Out of bounds";
                        break;
                    }
                    if (occupied.ContainsKey(cell))
                    {
                        reason = "Something is in the way";
                        break;
                    }
                    if (Map.GetDeposit(cell) != null)
                        onDeposit = true;
                }
            }

            if (reason == null && def.needsDeposit && !onDeposit)
                reason = "Needs to go on a deposit";
            if (reason == null && !free && Stockpile.Coins < def.price)
                reason = "Not enough coins";
            return reason == null;
        }

        public Building Place(BuildingDef def, Vector2Int origin, Direction facing, bool free = false)
        {
            if (!CanPlace(def, origin, facing, out _, free))
                return null;
            if (!free && !Stockpile.TrySpend(def.price))
                return null;

            var go = new GameObject($"{def.displayName} {origin}");
            go.transform.SetParent(transform, false);

            Building building;
            switch (def.kind)
            {
                case BuildingKind.Conveyor: building = go.AddComponent<Conveyor>(); break;
                case BuildingKind.Miner: building = go.AddComponent<MinerGirl>(); break;
                case BuildingKind.Cook: building = go.AddComponent<CookGirl>(); break;
                default: building = go.AddComponent<DessertStall>(); break;
            }

            building.Init(this, def, origin, facing);
            foreach (var cell in building.Cells)
                occupied[cell] = building;
            tickOrder.Add(building);
            return building;
        }

        public void Remove(Vector2Int cell)
        {
            var building = GetBuilding(cell);
            if (building == null)
                return;

            building.OnRemoved();
            foreach (var c in building.Cells)
                occupied.Remove(c);
            tickOrder.Remove(building);
            Stockpile.AddCoins(building.Def.price);
            Destroy(building.gameObject);
        }
    }
}
