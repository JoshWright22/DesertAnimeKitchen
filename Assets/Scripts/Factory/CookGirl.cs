using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace DessertFactory
{
    public class CookGirl : Building
    {
        readonly Dictionary<ItemDef, int> inputs = new Dictionary<ItemDef, int>();
        readonly Queue<ItemDef> finished = new Queue<ItemDef>();

        int recipeIndex;
        float progress;
        bool cooking;

        public RecipeDef Recipe => Def.recipes.Count > 0 ? Def.recipes[recipeIndex] : null;
        public int RecipeIndex => recipeIndex;
        public bool Cooking => cooking;
        public float Progress => cooking && Recipe != null ? progress / Recipe.craftTime : 0f;
        public bool Blocked => finished.Count > 0;

        // Until you pick a recipe for her she makes whatever the first ingredient to show up is for
        public bool PickedByHand { get; private set; }

        public bool Knows(RecipeDef recipe) => Factory.Stockpile.Knows(recipe);

        // how many of her recipes corporate has shipped so far
        public int KnownRecipes => Def.recipes.FindAll(Knows).Count;

        public int Has(ItemDef item)
        {
            inputs.TryGetValue(item, out int have);
            return have;
        }

        public void SetRecipe(int index)
        {
            if (Def.recipes.Count == 0)
                return;
            index = (index % Def.recipes.Count + Def.recipes.Count) % Def.recipes.Count;
            if (!Knows(Def.recipes[index]))
                return;

            DropEverything();
            recipeIndex = index;
            PickedByHand = true;
        }

        public override bool TryInsert(ItemDef item, Vector2Int fromCell)
        {
            if (Recipe == null || fromCell == OutputCell)
                return false;

            int needed = Recipe.InputAmount(item);
            if (needed == 0 && !PickedByHand && IsEmpty())
                needed = SwitchToRecipeFor(item);

            // she only holds one batch worth, anything she can't use or has no room for waits on the belt
            int have = Has(item);
            if (have >= needed)
                return false;

            inputs[item] = have + 1;
            return true;
        }

        int SwitchToRecipeFor(ItemDef item)
        {
            for (int i = 0; i < Def.recipes.Count; i++)
            {
                if (!Knows(Def.recipes[i]))
                    continue;
                int amount = Def.recipes[i].InputAmount(item);
                if (amount > 0)
                {
                    recipeIndex = i;
                    return amount;
                }
            }
            return 0;
        }

        bool IsEmpty()
        {
            if (cooking)
                return false;
            foreach (var pair in inputs)
            {
                if (pair.Value > 0)
                    return false;
            }
            return true;
        }

        public override void Tick(float deltaTime)
        {
            while (finished.Count > 0 && TryPushForward(finished.Peek()))
                finished.Dequeue();

            // don't start another batch while the front is backed up
            if (Recipe == null || Blocked)
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
                if (Has(input.item) < input.amount)
                    return false;
            }
            return true;
        }

        // switching recipes or picking her up throws out whatever she had on the go
        void DropEverything()
        {
            inputs.Clear();
            finished.Clear();
            cooking = false;
            progress = 0f;
        }

        public override string GetStatus()
        {
            if (Recipe == null)
                return "No recipes";

            var sb = new StringBuilder();
            sb.Append("Making ").Append(Recipe.displayName);

            if (Blocked)
                sb.Append(" - output blocked");
            else if (cooking)
                sb.Append($" - {Mathf.RoundToInt(Progress * 100f)}%");

            sb.Append("\nNeeds: ");
            for (int i = 0; i < Recipe.inputs.Count; i++)
            {
                var input = Recipe.inputs[i];
                if (i > 0)
                    sb.Append(", ");
                sb.Append($"{input.item.displayName} {Has(input.item)}/{input.amount}");
            }
            return sb.ToString();
        }
    }
}
