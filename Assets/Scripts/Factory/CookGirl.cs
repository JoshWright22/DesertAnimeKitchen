using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace DessertFactory
{
    public class CookGirl : Building
    {
        // how many batches worth of ingredients she'll hold onto
        const int BufferBatches = 2;

        readonly Dictionary<ItemDef, int> inputs = new Dictionary<ItemDef, int>();
        readonly Queue<ItemDef> finished = new Queue<ItemDef>();

        int recipeIndex;
        float progress;
        bool cooking;

        public RecipeDef Recipe => Def.recipes.Count > 0 ? Def.recipes[recipeIndex] : null;

        public void NextRecipe()
        {
            if (Def.recipes.Count == 0)
                return;

            ReturnEverything();
            recipeIndex = (recipeIndex + 1) % Def.recipes.Count;
        }

        public override bool TryInsert(ItemDef item, Vector2Int fromTile)
        {
            if (Recipe == null || fromTile == FrontTile)
                return false;

            int needed = Recipe.InputAmount(item);
            if (needed == 0)
                return false;

            inputs.TryGetValue(item, out int have);
            if (have >= needed * BufferBatches)
                return false;

            inputs[item] = have + 1;
            return true;
        }

        public override void Tick(float deltaTime)
        {
            while (finished.Count > 0 && TryPushForward(finished.Peek()))
                finished.Dequeue();

            // don't start another batch while the last one is stuck
            if (Recipe == null || finished.Count > 0)
                return;

            if (!cooking && HasIngredients())
            {
                foreach (var input in Recipe.inputs)
                    inputs[input.item] -= input.amount;
                cooking = true;
                progress = 0f;
            }

            if (!cooking)
                return;

            progress += deltaTime;
            if (progress >= Recipe.craftTime)
            {
                foreach (var output in Recipe.outputs)
                {
                    for (int i = 0; i < output.amount; i++)
                        finished.Enqueue(output.item);
                }
                cooking = false;
            }
        }

        bool HasIngredients()
        {
            foreach (var input in Recipe.inputs)
            {
                inputs.TryGetValue(input.item, out int have);
                if (have < input.amount)
                    return false;
            }
            return true;
        }

        void ReturnEverything()
        {
            foreach (var pair in inputs)
            {
                if (pair.Value > 0)
                    Factory.Stockpile.Add(pair.Key, pair.Value);
            }
            inputs.Clear();

            while (finished.Count > 0)
                Factory.Stockpile.Add(finished.Dequeue());

            if (cooking)
            {
                foreach (var input in Recipe.inputs)
                    Factory.Stockpile.Add(input.item, input.amount);
            }
            cooking = false;
            progress = 0f;
        }

        public override void OnRemoved()
        {
            ReturnEverything();
        }

        public override string GetStatus()
        {
            if (Recipe == null)
                return "No recipes";

            var sb = new StringBuilder();
            sb.Append("Making ").Append(Recipe.displayName);

            if (finished.Count > 0)
                sb.Append(" - output blocked");
            else if (cooking)
                sb.Append($" - {Mathf.RoundToInt(progress / Recipe.craftTime * 100f)}%");

            sb.Append("\nNeeds: ");
            for (int i = 0; i < Recipe.inputs.Count; i++)
            {
                var input = Recipe.inputs[i];
                inputs.TryGetValue(input.item, out int have);
                if (i > 0)
                    sb.Append(", ");
                sb.Append($"{input.item.displayName} {have}/{input.amount}");
            }
            return sb.ToString();
        }
    }
}
