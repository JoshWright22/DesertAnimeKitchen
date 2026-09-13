using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DessertFactory.EditorTools
{
    // Saves the placeholder content out as real assets so it can be tweaked in the inspector.
    // Running it again updates the existing assets in place, so scene references stay intact.
    public static class TestContentBuilder
    {
        const string Root = "Assets/TestContent";

        [MenuItem("Dessert Factory/Build Test Content")]
        public static void Build()
        {
            var source = GameContent.CreateDefault();
            var saved = new Dictionary<Object, Object>();

            foreach (var item in source.items)
                saved[item] = Persist(item, "Items");

            foreach (var deposit in source.deposits)
            {
                deposit.item = Get(saved, deposit.item);
                saved[deposit] = Persist(deposit, "Deposits");
            }

            foreach (var recipe in source.recipes)
            {
                recipe.inputs = recipe.inputs.Select(a => new ItemAmount(Get(saved, a.item), a.amount)).ToList();
                recipe.outputs = recipe.outputs.Select(a => new ItemAmount(Get(saved, a.item), a.amount)).ToList();
                saved[recipe] = Persist(recipe, "Recipes");
            }

            foreach (var building in source.buildings)
            {
                building.recipes = building.recipes.Select(r => Get(saved, r)).ToList();
                saved[building] = Persist(building, "Buildings");
            }

            source.items = source.items.Select(i => Get(saved, i)).ToList();
            source.deposits = source.deposits.Select(d => Get(saved, d)).ToList();
            source.recipes = source.recipes.Select(r => Get(saved, r)).ToList();
            source.buildings = source.buildings.Select(b => Get(saved, b)).ToList();
            source.name = "TestGameContent";
            var content = Persist(source, "");

            var layout = ScriptableObject.CreateInstance<FactoryLayout>();
            layout.name = "TestLayout";
            FillTestLayout(layout, content);
            Persist(layout, "");

            AssetDatabase.SaveAssets();
            Debug.Log($"Test content written to {Root}");
        }

        // A small sorbet line: sugar and syrup get prepped, water is dug right next to
        // the pastry girl, and the sorbet goes off to a stall. Dates get stockpiled on the side.
        static void FillTestLayout(FactoryLayout layout, GameContent content)
        {
            DepositDef Deposit(string itemName) => content.deposits.First(d => d.item.displayName == itemName);
            BuildingDef Building(string buildingName) => content.buildings.First(b => b.displayName == buildingName);

            var belt = Building("Conveyor Belt");
            var digger = Building("Digger Girl");
            var prep = Building("Prep Girl");
            var pastry = Building("Pastry Girl");
            var stall = Building("Dessert Stall");

            layout.deposits.Clear();
            layout.buildings.Clear();

            void Area(string itemName, int x, int y, int w, int h)
            {
                layout.deposits.Add(new FactoryLayout.DepositArea { deposit = Deposit(itemName), area = new RectInt(x, y, w, h) });
            }

            void Put(BuildingDef def, int x, int y, Direction facing, int recipe = 0)
            {
                layout.buildings.Add(new FactoryLayout.Placement { building = def, cell = new Vector2Int(x, y), facing = facing, recipe = recipe });
            }

            void Belts(int fromX, int toX, int y, Direction facing)
            {
                for (int x = fromX; x <= toX; x++)
                    Put(belt, x, y, facing);
            }

            Area("Sugar Sand", 2, 10, 3, 3);
            Area("Cactus Fruit", 2, 5, 3, 3);
            Area("Oasis Water", 20, 12, 3, 3);
            Area("Dates", 2, 16, 3, 3);

            // sugar
            Put(digger, 3, 11, Direction.Right);
            Put(digger, 4, 12, Direction.Down);
            Belts(4, 6, 11, Direction.Right);
            Put(prep, 7, 11, Direction.Right, recipe: 0);
            Belts(8, 19, 11, Direction.Right);

            // cactus syrup, comes around the bottom and up into the pastry girl
            Put(digger, 3, 6, Direction.Right);
            Put(digger, 4, 7, Direction.Down);
            Belts(4, 6, 6, Direction.Right);
            Put(prep, 7, 6, Direction.Right, recipe: 2);
            Belts(8, 19, 6, Direction.Right);
            for (int y = 6; y <= 9; y++)
                Put(belt, 20, y, Direction.Up);

            // water straight from the ground into her
            Put(digger, 21, 12, Direction.Down);

            Put(pastry, 20, 10, Direction.Right, recipe: 1);
            Put(belt, 22, 10, Direction.Right);
            Put(stall, 23, 9, Direction.Right);

            // dates just get stored
            Put(digger, 3, 17, Direction.Right);
            Belts(4, 9, 17, Direction.Right);
            Put(stall, 10, 17, Direction.Right);
        }

        static T Get<T>(Dictionary<Object, Object> saved, T original) where T : Object
        {
            return original != null && saved.TryGetValue(original, out var asset) ? (T)asset : original;
        }

        static T Persist<T>(T obj, string folder) where T : ScriptableObject
        {
            string dir = string.IsNullOrEmpty(folder) ? Root : $"{Root}/{folder}";
            EnsureFolder(dir);
            string path = $"{dir}/{obj.name}.asset";

            var existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing == null)
            {
                AssetDatabase.CreateAsset(obj, path);
                return obj;
            }

            string assetName = existing.name;
            EditorUtility.CopySerialized(obj, existing);
            existing.name = assetName;
            EditorUtility.SetDirty(existing);
            return existing;
        }

        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
                return;

            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }
    }
}
