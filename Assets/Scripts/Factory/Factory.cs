using System.Collections.Generic;
using UnityEngine;

namespace DessertFactory
{
    public class Factory : MonoBehaviour
    {
        [SerializeField] DesertMap map;
        [SerializeField] ItemViewPool itemViews;
        [SerializeField] int startingCoins = 300;

        public DesertMap Map => map;
        public Stockpile Stockpile { get; private set; }
        public ItemViewPool ItemViews => itemViews;

        // every cell a building covers points back at it
        readonly Dictionary<Vector2Int, Building> occupied = new Dictionary<Vector2Int, Building>();
        readonly List<Building> tickOrder = new List<Building>();
        readonly Dictionary<BuildingDef, int> placedCounts = new Dictionary<BuildingDef, int>();

        void Awake()
        {
            Stockpile = new Stockpile(startingCoins);
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

        public int CountPlaced(BuildingDef def)
        {
            placedCounts.TryGetValue(def, out int count);
            return count;
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
            if (reason == null && !free && Stockpile.IsLimited(def) && CountPlaced(def) >= Stockpile.Owned(def))
                reason = "No more of her, pull another from the gacha";
            return reason == null;
        }

        public Building Place(BuildingDef def, Vector2Int origin, Direction facing, bool free = false)
        {
            if (!CanPlace(def, origin, facing, out _, free))
                return null;
            if (!free && !Stockpile.TrySpend(def.price))
                return null;
            // girls the starting layout hands out count as owned
            if (free && Stockpile.IsLimited(def))
                Stockpile.AddWorkers(def);

            var building = Instantiate(def.prefab, transform);
            building.name = $"{def.displayName} {origin}";
            building.Init(this, def, origin, facing);
            foreach (var cell in building.Cells)
                occupied[cell] = building;
            tickOrder.Add(building);
            placedCounts[def] = CountPlaced(def) + 1;
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
            placedCounts[building.Def]--;
            Stockpile.AddCoins(building.Def.price);
            Destroy(building.gameObject);
        }
    }
}
