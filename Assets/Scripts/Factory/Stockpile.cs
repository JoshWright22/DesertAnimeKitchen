using System;
using System.Collections.Generic;

namespace DessertFactory
{
    // Shared storage for the whole factory, there's no player inventory
    public class Stockpile
    {
        readonly Dictionary<ItemDef, int> items = new Dictionary<ItemDef, int>();
        // copies owned of each girl's building, anything not in here isn't limited
        readonly Dictionary<BuildingDef, int> workers = new Dictionary<BuildingDef, int>();

        public int Coins { get; private set; }
        public int Stars { get; private set; }
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
            Stars += dessert.stars;
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

        public bool TrySpendStars(int amount)
        {
            if (Stars < amount)
                return false;
            Stars -= amount;
            Changed?.Invoke();
            return true;
        }

        public bool IsLimited(BuildingDef def) => workers.ContainsKey(def);

        public int Owned(BuildingDef def)
        {
            workers.TryGetValue(def, out int owned);
            return owned;
        }

        // Adding 0 still marks the building as limited
        public void AddWorkers(BuildingDef def, int amount = 1)
        {
            workers[def] = Owned(def) + amount;
            Changed?.Invoke();
        }
    }
}
