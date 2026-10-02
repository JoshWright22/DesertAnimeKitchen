using UnityEngine;

namespace DessertFactory
{
    public class DessertStall : Building
    {
        int sold;

        public override bool TryInsert(ItemDef item, Vector2Int fromCell)
        {
            Factory.Stockpile.Sell(item);
            sold++;
            return true;
        }

        public override string GetStatus()
        {
            return $"Sold {sold}";
        }
    }
}
