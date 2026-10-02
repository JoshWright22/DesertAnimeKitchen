using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace DessertFactory
{
    // Works out every production line from the real assets: the girls one full-speed line needs, what it makes a day,
    // its wages and its stars. Run it after touching a price, wage or craft time to see what changed.
    // docs/ECONOMY.md explains what each dessert is meant to be good at.
    public static class PipelineReport
    {
        // 9 am to 5 pm at 15 real seconds an hour, the workday set on the Campaign in the scene
        const float DaySeconds = 8 * 15f;

        class Line
        {
            public ItemDef item;
            public float perDay;
            public readonly Dictionary<BuildingDef, int> girls = new Dictionary<BuildingDef, int>();
            public int coins, wages, stars;
            public string lands;
            public int GirlCount => girls.Values.Sum();
        }

        [MenuItem("Dessert Factory/Pipeline Report")]
        static void Report()
        {
            var buildings = Load<BuildingDef>();
            var miner = buildings.Find(b => b.needsDeposit);
            var cookFor = new Dictionary<RecipeDef, BuildingDef>();
            var recipeFor = new Dictionary<ItemDef, RecipeDef>();
            foreach (var building in buildings)
            {
                foreach (var recipe in building.recipes)
                {
                    cookFor[recipe] = building;
                    recipeFor[recipe.outputs[0].item] = recipe;
                }
            }
            var landFor = new Dictionary<ItemDef, DepositDef>();
            foreach (var deposit in Load<DepositDef>())
            {
                foreach (var item in deposit.items)
                    landFor[item] = deposit;
            }

            var lines = new List<Line>();
            foreach (var item in Load<ItemDef>())
            {
                if (!recipeFor.ContainsKey(item) && !landFor.ContainsKey(item))
                {
                    Debug.LogWarning($"{item.displayName}: nothing makes it and no land has it");
                    continue;
                }

                // as fast as one girl at the end of the line can go
                float rate = recipeFor.TryGetValue(item, out var last) ? last.outputs[0].amount / last.craftTime : 1f / miner.workTime;
                var load = new Dictionary<RecipeDef, float>();
                var raw = new Dictionary<ItemDef, float>();
                Need(item, rate, recipeFor, load, raw);

                var line = new Line { item = item, perDay = rate * DaySeconds };
                foreach (var pair in load)
                    Add(line.girls, cookFor[pair.Key], Mathf.CeilToInt(pair.Value - 0.001f));
                foreach (var pair in raw)
                    Add(line.girls, miner, Mathf.CeilToInt(pair.Value * miner.workTime - 0.001f));
                line.coins = Mathf.RoundToInt(line.perDay * item.sellPrice);
                line.stars = Mathf.RoundToInt(line.perDay * item.stars);
                line.wages = line.girls.Sum(pair => pair.Key.wage * pair.Value);
                line.lands = string.Join(", ", raw.Keys.Select(ore => landFor[ore].displayName));
                lines.Add(line);
            }

            var sb = new StringBuilder("Pipeline report, one full-speed line each, per working day\n");
            foreach (var line in lines.OrderByDescending(l => (l.coins - l.wages) / (float)l.GirlCount))
            {
                string girls = string.Join(" ", line.girls.Select(pair => $"{pair.Value} {pair.Key.displayName}"));
                sb.AppendLine($"{line.item.displayName}: {girls} | {line.perDay:0} a day | {line.coins:N0} coins - {line.wages:N0} wages = " +
                              $"{line.coins - line.wages:N0} ({(line.coins - line.wages) / line.GirlCount:N0} a girl) | " +
                              $"{line.stars} stars ({line.stars / line.GirlCount} a girl) | {line.lands}");
            }
            Debug.Log(sb.ToString());
        }

        // what a rate of this item takes: each recipe's busy girls and each ore a second
        static void Need(ItemDef item, float rate, Dictionary<ItemDef, RecipeDef> recipeFor, Dictionary<RecipeDef, float> load, Dictionary<ItemDef, float> raw)
        {
            if (!recipeFor.TryGetValue(item, out var recipe))
            {
                raw.TryGetValue(item, out float have);
                raw[item] = have + rate;
                return;
            }

            float batches = rate / recipe.outputs[0].amount;
            load.TryGetValue(recipe, out float busy);
            load[recipe] = busy + batches * recipe.craftTime;
            foreach (var input in recipe.inputs)
                Need(input.item, batches * input.amount, recipeFor, load, raw);
        }

        static void Add(Dictionary<BuildingDef, int> girls, BuildingDef girl, int count)
        {
            girls.TryGetValue(girl, out int have);
            girls[girl] = have + count;
        }

        static List<T> Load<T>() where T : Object
        {
            return AssetDatabase.FindAssets($"t:{typeof(T).Name}")
                .Select(guid => AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid)))
                .ToList();
        }
    }
}
