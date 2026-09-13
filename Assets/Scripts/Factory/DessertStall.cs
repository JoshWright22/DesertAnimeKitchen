using UnityEngine;

namespace DessertFactory
{
    public class DessertStall : Building
    {
        int sold;

        public override bool TryInsert(ItemDef item, Vector2Int fromCell)
        {
            if (item.isDessert)
            {
                Factory.Stockpile.Sell(item);
                sold++;
            }
            else
            {
                Factory.Stockpile.Add(item);
            }
            return true;
        }

        public override string GetStatus()
        {
            return $"Sold {sold} desserts";
        }
    }
}
