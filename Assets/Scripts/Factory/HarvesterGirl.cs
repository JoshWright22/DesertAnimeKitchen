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

            if (Factory.Map.GetDeposit(Tile) == null)
            {
                status = "Nothing left to dig";
                return;
            }

            status = "Digging";
            timer += deltaTime;
            if (timer >= Def.workTime)
            {
                timer = 0f;
                Factory.Map.TryDig(Tile, out holding);
            }
        }

        public override bool TryInsert(ItemDef item, Vector2Int fromTile)
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
            var deposit = Factory.Map.GetDeposit(Tile);
            if (deposit == null)
                return status;
            return $"{status} ({deposit.item.displayName}, {Factory.Map.GetAmount(Tile)} left)";
        }
    }
}
