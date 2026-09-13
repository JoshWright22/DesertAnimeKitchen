using System.Collections.Generic;
using UnityEngine;

namespace DessertFactory
{
    [CreateAssetMenu(menuName = "Dessert Factory/Game Content")]
    public class GameContent : ScriptableObject
    {
        public List<ItemDef> items = new List<ItemDef>();
        public List<DepositDef> deposits = new List<DepositDef>();
        public List<RecipeDef> recipes = new List<RecipeDef>();
        public List<BuildingDef> buildings = new List<BuildingDef>();

        // Placeholder content so the game runs without any assets set up.
        // Once real assets exist, make a GameContent asset and assign it on GameBootstrap.
        public static GameContent CreateDefault()
        {
            var content = CreateInstance<GameContent>();

            var sugarSand = content.AddItem("Sugar Sand", new Color(0.98f, 0.95f, 0.85f));
            var dates = content.AddItem("Dates", new Color(0.45f, 0.25f, 0.12f));
            var cactusFruit = content.AddItem("Cactus Fruit", new Color(0.85f, 0.25f, 0.5f));
            var millet = content.AddItem("Millet", new Color(0.9f, 0.75f, 0.3f));
            var water = content.AddItem("Oasis Water", new Color(0.3f, 0.6f, 0.95f));

            var sugar = content.AddItem("Sugar", Color.white);
            var flour = content.AddItem("Flour", new Color(0.95f, 0.92f, 0.8f));
            var syrup = content.AddItem("Cactus Syrup", new Color(0.7f, 0.1f, 0.35f));
            var dough = content.AddItem("Dough", new Color(0.93f, 0.8f, 0.6f));

            var dateCake = content.AddItem("Date Cake", new Color(0.6f, 0.35f, 0.2f), 30);
            var sorbet = content.AddItem("Cactus Sorbet", new Color(1f, 0.5f, 0.75f), 22);
            var cookies = content.AddItem("Sugar Cookies", new Color(0.95f, 0.8f, 0.45f), 14);

            content.AddDeposit(sugarSand, new Color(0.93f, 0.9f, 0.82f), 5, 5f);
            content.AddDeposit(dates, new Color(0.5f, 0.32f, 0.18f), 4, 3.5f);
            content.AddDeposit(cactusFruit, new Color(0.45f, 0.62f, 0.35f), 4, 3.5f);
            content.AddDeposit(millet, new Color(0.82f, 0.68f, 0.3f), 4, 4.5f);
            content.AddDeposit(water, new Color(0.35f, 0.6f, 0.8f), 3, 3f, 5000);

            var prepRecipes = new List<RecipeDef>
            {
                content.AddRecipe("Sugar", 1.5f, new[] { Amount(sugarSand, 2) }, new[] { Amount(sugar, 1) }),
                content.AddRecipe("Flour", 1.5f, new[] { Amount(millet, 2) }, new[] { Amount(flour, 1) }),
                content.AddRecipe("Cactus Syrup", 2f, new[] { Amount(cactusFruit, 2) }, new[] { Amount(syrup, 1) }),
                content.AddRecipe("Dough", 2f, new[] { Amount(flour, 2), Amount(water, 1) }, new[] { Amount(dough, 1) }),
            };

            var pastryRecipes = new List<RecipeDef>
            {
                content.AddRecipe("Sugar Cookies", 3f, new[] { Amount(dough, 1), Amount(sugar, 2) }, new[] { Amount(cookies, 2) }),
                content.AddRecipe("Cactus Sorbet", 4f, new[] { Amount(syrup, 1), Amount(water, 1), Amount(sugar, 1) }, new[] { Amount(sorbet, 1) }),
                content.AddRecipe("Date Cake", 5f, new[] { Amount(dough, 1), Amount(dates, 2), Amount(sugar, 1) }, new[] { Amount(dateCake, 1) }),
            };

            var belt = content.AddBuilding("Conveyor Belt", BuildingKind.Conveyor, 1,
                "Moves ingredients along. Drag to lay a line.");
            belt.workTime = 0.6f;
            belt.outfitColor = new Color(0.35f, 0.33f, 0.32f);

            var digger = content.AddBuilding("Digger Girl", BuildingKind.Harvester, 20,
                "Digs up whatever is buried under her and passes it forward. Place on a deposit.");
            digger.workTime = 1.2f;
            digger.needsDeposit = true;
            digger.outfitColor = new Color(0.9f, 0.55f, 0.2f);
            digger.hairColor = new Color(0.35f, 0.2f, 0.1f);

            var prep = content.AddBuilding("Prep Girl", BuildingKind.Cook, 35,
                "Turns raw ingredients into sugar, flour, syrup and dough. Click her to change recipe.");
            prep.recipes = prepRecipes;
            prep.outfitColor = new Color(0.4f, 0.7f, 0.6f);
            prep.hairColor = new Color(0.95f, 0.85f, 0.5f);

            var pastry = content.AddBuilding("Pastry Girl", BuildingKind.Cook, 60,
                "Bakes finished desserts. Click her to change recipe.");
            pastry.recipes = pastryRecipes;
            pastry.size = new Vector2Int(2, 2);
            pastry.outfitColor = new Color(0.95f, 0.6f, 0.75f);
            pastry.hairColor = new Color(0.55f, 0.35f, 0.8f);

            var stall = content.AddBuilding("Dessert Stall", BuildingKind.Stall, 40,
                "Sells desserts for coins. Anything else sent here is kept in storage.");
            stall.size = new Vector2Int(2, 2);
            stall.outfitColor = new Color(0.8f, 0.3f, 0.3f);
            stall.hairColor = new Color(0.15f, 0.15f, 0.2f);

            return content;
        }

        static ItemAmount Amount(ItemDef item, int amount) => new ItemAmount(item, amount);

        ItemDef AddItem(string name, Color color, int sellPrice = 0)
        {
            var item = CreateInstance<ItemDef>();
            item.name = name;
            item.displayName = name;
            item.color = color;
            item.isDessert = sellPrice > 0;
            item.sellPrice = sellPrice;
            items.Add(item);
            return item;
        }

        void AddDeposit(ItemDef item, Color groundColor, int patches, float radius, int amountPerTile = 300)
        {
            var deposit = CreateInstance<DepositDef>();
            deposit.name = item.displayName + " Deposit";
            deposit.item = item;
            deposit.groundColor = groundColor;
            deposit.patchCount = patches;
            deposit.patchRadius = radius;
            deposit.amountPerTile = amountPerTile;
            deposits.Add(deposit);
        }

        RecipeDef AddRecipe(string name, float time, ItemAmount[] inputs, ItemAmount[] outputs)
        {
            var recipe = CreateInstance<RecipeDef>();
            recipe.name = name;
            recipe.displayName = name;
            recipe.craftTime = time;
            recipe.inputs.AddRange(inputs);
            recipe.outputs.AddRange(outputs);
            recipes.Add(recipe);
            return recipe;
        }

        BuildingDef AddBuilding(string name, BuildingKind kind, int price, string description)
        {
            var building = CreateInstance<BuildingDef>();
            building.name = name;
            building.displayName = name;
            building.kind = kind;
            building.price = price;
            building.description = description;
            buildings.Add(building);
            return building;
        }
    }
}
