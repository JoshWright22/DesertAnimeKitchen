using UnityEngine;

namespace DessertFactory
{
    public class MinerGirl : Building
    {
        float timer;
        ItemDef holding;
        string status = "Starting";

        public override void Tick(float deltaTime)
        {
            if (holding != null)
            {
                if (!TryPushForward(holding))
                {
                    status = "Waiting for space in front";
                    return;
                }
                holding = null;
            }

            if (!FindMineCell(out var cell))
            {
                status = "Nothing left to mine";
                return;
            }

            status = "Mining";
            timer += deltaTime;
            if (timer >= Def.workTime)
            {
                timer = 0f;
                Factory.Map.TryMine(cell, out holding);
            }
        }

        // Bigger miners work through every cell under them
        bool FindMineCell(out Vector2Int cell)
        {
            foreach (var c in Cells)
            {
                if (Factory.Map.GetDeposit(c) != null)
                {
                    cell = c;
                    return true;
                }
            }
            cell = default;
            return false;
        }

        public override bool TryInsert(ItemDef item, Vector2Int fromCell)
        {
            return false;
        }

        public override void OnRemoved()
        {
            if (holding != null)
                Factory.Stockpile.Add(holding);
        }

        public override string GetStatus()
        {
            if (!FindMineCell(out var cell))
                return status;
            return $"{status} ({Factory.Map.GetDeposit(cell).item.displayName}, {Factory.Map.GetAmount(cell)} left here)";
        }
    }
}
