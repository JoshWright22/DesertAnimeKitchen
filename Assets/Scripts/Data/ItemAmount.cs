using System;

namespace DessertFactory
{
    [Serializable]
    public struct ItemAmount
    {
        public ItemDef item;
        public int amount;

        public ItemAmount(ItemDef item, int amount)
        {
            this.item = item;
            this.amount = amount;
        }
    }
}
