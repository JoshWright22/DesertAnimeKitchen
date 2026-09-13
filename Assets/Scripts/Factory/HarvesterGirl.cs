using UnityEngine;

namespace DessertFactory
{
    public class HarvesterGirl : Building
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

            if (!FindDigCell(out var cell))
            {
                status = "Nothing left to dig";
                return;
            }

            status = "Digging";
            timer += deltaTime;
            if (timer >= Def.workTime)
            {
                timer = 0f;
                Factory.Map.TryDig(cell, out holding);
            }
        }

        // Bigger diggers work through every cell under them
        bool FindDigCell(out Vector2Int cell)
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
            if (!FindDigCell(out var cell))
                return status;
            return $"{status} ({Factory.Map.GetDeposit(cell).item.displayName}, {Factory.Map.GetAmount(cell)} left here)";
        }
    }
}
