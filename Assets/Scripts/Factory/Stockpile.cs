using System;
using System.Collections.Generic;

namespace DessertFactory
{
    // The factory's money, stars and girls, and a tally of everything the stall has sold. There's no player inventory.
    public class Stockpile
    {
        readonly Dictionary<ItemDef, int> sold = new Dictionary<ItemDef, int>();
        // copies pulled of each gacha girl, you can place that many of her. Anything not in here, like belts, isn't limited.
        readonly Dictionary<BuildingDef, int> girls = new Dictionary<BuildingDef, int>();
        // the girls only cook what corporate has shipped the recipe for
        readonly HashSet<RecipeDef> recipes = new HashSet<RecipeDef>();

        public int Coins { get; private set; }
        public int Stars { get; private set; }
        public int DessertsSold { get; private set; }
        public IReadOnlyDictionary<ItemDef, int> Sold => sold;
        public int RecipesKnown => recipes.Count;

        // What corporate judges you on: everything sold, minus wages and other running costs, minus the loan
        public int Revenue { get; private set; }
        public int Expenses { get; private set; }
        public int Debt { get; }
        public int Profit => Revenue - Expenses - Debt;

        public event Action Changed;

        // The loan is the cash you start with, and it's owed back
        public Stockpile(int loan)
        {
            Coins = loan;
            Debt = loan;
        }

        public void Sell(ItemDef item)
        {
            Coins += item.sellPrice;
            Revenue += item.sellPrice;
            Stars += item.stars;
            DessertsSold++;
            sold.TryGetValue(item, out int count);
            sold[item] = count + 1;
            Changed?.Invoke();
        }

        public void AddCoins(int amount)
        {
            Coins += amount;
            Changed?.Invoke();
        }

        public void AddStars(int amount)
        {
            Stars += amount;
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

        // Wages and the like get paid whether you can afford them or not
        public void Pay(int amount)
        {
            Coins -= amount;
            Expenses += amount;
            Changed?.Invoke();
        }

        public bool TrySpendStars(int amount)
        {
            if (Stars < amount)
                return false;
            Stars -= amount;
            Changed?.Invoke();
            return true;
        }

        public bool IsGachaGirl(BuildingDef def) => girls.ContainsKey(def);

        public int Owned(BuildingDef def)
        {
            girls.TryGetValue(def, out int owned);
            return owned;
        }

        public bool HasMet(BuildingDef def) => Owned(def) > 0;

        // Adding 0 still marks her as a gacha girl. Her first copy brings her starting recipes with her.
        public void AddCopies(BuildingDef def, int amount)
        {
            if (!HasMet(def) && amount > 0)
            {
                for (int i = 0; i < def.startingRecipes && i < def.recipes.Count; i++)
                    recipes.Add(def.recipes[i]);
            }
            girls[def] = Owned(def) + amount;
            Changed?.Invoke();
        }

        public bool Knows(RecipeDef recipe) => recipes.Contains(recipe);

        public void Learn(RecipeDef recipe)
        {
            if (recipes.Add(recipe))
                Changed?.Invoke();
        }
    }
}
