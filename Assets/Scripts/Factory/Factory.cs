using System.Collections.Generic;
using UnityEngine;

namespace DessertFactory
{
    public class Factory : MonoBehaviour
    {
        [SerializeField] DesertMap map;
        [SerializeField] ItemViewPool itemViews;
        [Tooltip("Corporate's starting loan. It's your cash on day one and it counts against your profit.")]
        [SerializeField] int loan = 30000;

        public DesertMap Map => map;
        public Stockpile Stockpile { get; private set; }
        public ItemViewPool ItemViews => itemViews;
        public IReadOnlyList<Building> Buildings => tickOrder;

        // The girls go home after hours and everything stops where it is
        public bool Working { get; set; } = true;

        // every cell a building covers points back at it
        readonly Dictionary<Vector2Int, Building> occupied = new Dictionary<Vector2Int, Building>();
        readonly List<Building> tickOrder = new List<Building>();
        readonly Dictionary<BuildingDef, int> placedCounts = new Dictionary<BuildingDef, int>();
        // what the last CanPlace would have to pick up to make room
        readonly List<Building> inTheWay = new List<Building>();

        // What placing on top of something else is allowed to replace
        public enum Overwrite
        {
            Nothing,
            // dragging a belt line can reroute old belts without eating the girls along the way
            Belts,
            Anything
        }

        static readonly Vector2Int[] Around = { Vector2Int.zero, Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        void Awake()
        {
            Stockpile = new Stockpile(loan);
        }

        void Update()
        {
            if (!Working)
                return;

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

        public bool CanPlace(BuildingDef def, Vector2Int origin, Direction facing, out string reason, bool free = false, Overwrite overwrite = Overwrite.Nothing)
        {
            reason = null;
            inTheWay.Clear();
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
                    if (occupied.TryGetValue(cell, out var other))
                    {
                        if (!CanReplace(other, def, origin, facing, overwrite))
                        {
                            reason = "Something is in the way";
                            break;
                        }
                        if (!inTheWay.Contains(other))
                            inTheWay.Add(other);
                    }
                    if (Map.GetDeposit(cell) != null)
                        onDeposit = true;
                }
            }

            if (reason == null && def.needsDeposit && !onDeposit)
                reason = "Needs to go on a deposit";
            // whatever gets picked up is refunded first, and a girl being moved doesn't need a second copy
            int refund = 0;
            int sameGirl = 0;
            foreach (var other in inTheWay)
            {
                refund += other.Def.price;
                if (other.Def == def)
                    sameGirl++;
            }
            if (reason == null && !free && Stockpile.Coins + refund < def.price)
                reason = "Not enough coins";
            if (reason == null && !free && Stockpile.IsGachaGirl(def) && CountPlaced(def) - sameGirl >= Stockpile.Owned(def))
                reason = "No more of her, roll the gacha for more";
            return reason == null;
        }

        public Building Place(BuildingDef def, Vector2Int origin, Direction facing, bool free = false, Overwrite overwrite = Overwrite.Nothing)
        {
            if (!CanPlace(def, origin, facing, out _, free, overwrite))
                return null;
            foreach (var other in inTheWay.ToArray())
                Remove(other);
            if (!free && !Stockpile.TrySpend(def.price))
                return null;
            // girls the starting layout hands out count as owned
            if (free && Stockpile.IsGachaGirl(def))
                Stockpile.AddCopies(def, 1);

            var building = Instantiate(def.prefab, transform);
            building.name = $"{def.displayName} {origin}";
            building.Init(this, def, origin, facing);
            foreach (var cell in building.Cells)
            {
                occupied[cell] = building;
                Map.ClearCactus(cell);
            }
            tickOrder.Add(building);
            placedCounts[def] = CountPlaced(def) + 1;
            TellNeighbours(building);
            return building;
        }

        // True when the placement would pick something up first
        public bool WouldReplace => inTheWay.Count > 0;

        static bool CanReplace(Building other, BuildingDef def, Vector2Int origin, Direction facing, Overwrite overwrite)
        {
            // the exact same thing in the exact same spot, nothing to do
            if (other.Def == def && other.Origin == origin && other.Facing == facing)
                return false;
            return overwrite == Overwrite.Anything
                || (overwrite == Overwrite.Belts && other is Conveyor && def.prefab is Conveyor);
        }

        public void Remove(Vector2Int cell)
        {
            var building = GetBuilding(cell);
            if (building != null)
                Remove(building);
        }

        void Remove(Building building)
        {
            building.OnRemoved();
            foreach (var c in building.Cells)
                occupied.Remove(c);
            tickOrder.Remove(building);
            placedCounts[building.Def]--;
            Stockpile.AddCoins(building.Def.price);
            TellNeighbours(building);
            Destroy(building.gameObject);
        }

        // Lets the building and everything touching it update their look, like belts capping their ends
        void TellNeighbours(Building building)
        {
            var told = new HashSet<Building>();
            foreach (var cell in building.Cells)
            {
                foreach (var offset in Around)
                {
                    var other = GetBuilding(cell + offset);
                    if (other != null && told.Add(other))
                        other.NeighboursChanged();
                }
            }
        }
    }
}
