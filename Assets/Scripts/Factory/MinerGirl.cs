using UnityEngine;

namespace DessertFactory
{
    public class MinerGirl : Building
    {
        float timer;
        int mined;
        ItemDef holding;
        string status = "Starting";

        // The land she's standing on, or null if she was put down off it
        public DepositDef Digging => FindMineCell(out var cell) ? Factory.Map.GetDeposit(cell) : null;

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
                status = "Nothing to mine here";
                return;
            }

            status = "Mining";
            timer += deltaTime;
            if (timer >= Def.workTime)
            {
                timer = 0f;
                // lands with more than one ore hand them out in turn
                var deposit = Factory.Map.GetDeposit(cell);
                holding = deposit.items[mined++ % deposit.items.Count];
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

        public override string GetStatus()
        {
            if (!FindMineCell(out var cell))
                return status;
            return $"{status} ({Factory.Map.GetDeposit(cell).displayName})";
        }
    }
}
