using System;
using System.Collections.Generic;

namespace DessertFactory
{
    // Shared storage for the whole factory, there's no player inventory
    public class Stockpile
    {
        readonly Dictionary<ItemDef, int> items = new Dictionary<ItemDef, int>();

        public int Coins { get; private set; }
        public int DessertsSold { get; private set; }
        public IReadOnlyDictionary<ItemDef, int> Items => items;

        public event Action Changed;

        public Stockpile(int startingCoins)
        {
            Coins = startingCoins;
        }

        public void Add(ItemDef item, int amount = 1)
        {
            items.TryGetValue(item, out int current);
            items[item] = current + amount;
            Changed?.Invoke();
        }

        public void Sell(ItemDef dessert)
        {
            Coins += dessert.sellPrice;
            DessertsSold++;
            Changed?.Invoke();
        }

        public void AddCoins(int amount)
        {
            Coins += amount;
            Changed?.Invoke();
        }

        public bool TrySpend(int amount)
        {
            if (Coins < amount)
                return false;
            Coins -= amount;
            Changed?.Invoke();
            return true;
        }
    }
}
