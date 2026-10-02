using System.Collections.Generic;
using UnityEngine;

namespace DessertFactory
{
    // Spend stars to pull more copies of a girl, or a recipe for one you've got. You can only place as many of a girl
    // as you've pulled, so the gacha is what lets the factory grow. Pulls are cheap and quick so you're rolling
    // a few times a day, and every pull after the first costs a little more.
    // docs/ECONOMY.md has the pacing these numbers are tuned for.
    public class Gacha : MonoBehaviour
    {
        // What came out: copies of a girl, or a recipe for one you already have
        public struct Pull
        {
            public CharacterDef girl;
            public RecipeDef recipe;
            public int copies;
            public bool firstTime;
        }

        [SerializeField] Factory factory;
        [SerializeField] CutscenePlayer cutscenes;
        [Tooltip("The first paid pull")]
        [SerializeField] int rollCost = 30;
        [Tooltip("Each paid pull costs this many times the last one")]
        [SerializeField] float costGrowth = 1.15f;
        [Tooltip("How many of her a girl pull lets you place")]
        [SerializeField] int copiesPerPull = 2;
        [Tooltip("Who the very first pull always gives, so the tutorial can count on her")]
        [SerializeField] CharacterDef firstPull;
        [Tooltip("Still haven't met everyone by this pull? Then it's someone new")]
        [SerializeField] int newGirlBy = 3;
        [Tooltip("Chance a paid pull is a recipe for one of your girls instead of more girls, while there are any left to learn")]
        [Range(0f, 1f)]
        [SerializeField] float recipeChance = 0.35f;

        readonly List<CharacterDef> pool = new List<CharacterDef>();

        // rounded to ten so the button reads like a price
        public int RollCost => Pulls == 0 ? 0 : Mathf.RoundToInt(rollCost * Mathf.Pow(costGrowth, Pulls - 1) / 10f) * 10;
        public int Pulls { get; private set; }
        public bool CanRoll => pool.Count > 0 && factory.Stockpile.Stars >= RollCost && !cutscenes.Playing;

        // Has to run before the starting layout is placed so her buildings are already limited
        public void Init(GameContent content)
        {
            foreach (var girl in content.characters)
            {
                if (girl == null || girl.building == null)
                    continue;
                pool.Add(girl);
                factory.Stockpile.AddCopies(girl.building, girl.startingCopies);
            }
        }

        // Null girl and recipe if you couldn't afford it
        public Pull Roll()
        {
            var pull = new Pull();
            var stock = factory.Stockpile;
            if (!CanRoll || !stock.TrySpendStars(RollCost))
                return pull;

            // the tutorial's free pull is always a girl
            var recipes = Unlearned();
            if (Pulls > 0 && recipes.Count > 0 && Random.value < recipeChance)
            {
                pull.recipe = recipes[Random.Range(0, recipes.Count)];
                pull.girl = pool.Find(girl => girl.building.recipes.Contains(pull.recipe));
                stock.Learn(pull.recipe);
            }
            else
            {
                pull.girl = Pulls == 0 && pool.Contains(firstPull) ? firstPull : Pick(Pulls + 1 >= newGirlBy);
                pull.firstTime = !stock.HasMet(pull.girl.building);
                pull.copies = copiesPerPull;
                stock.AddCopies(pull.girl.building, copiesPerPull);
            }
            Pulls++;
            return pull;
        }

        // only recipes for girls you've got, so a recipe pull is never for nobody
        List<RecipeDef> Unlearned()
        {
            var recipes = new List<RecipeDef>();
            foreach (var girl in pool)
            {
                if (!factory.Stockpile.HasMet(girl.building))
                    continue;
                foreach (var recipe in girl.building.recipes)
                {
                    if (!factory.Stockpile.Knows(recipe))
                        recipes.Add(recipe);
                }
            }
            return recipes;
        }

        CharacterDef Pick(bool onlyNewGirls)
        {
            var candidates = pool;
            if (onlyNewGirls)
            {
                var unmet = pool.FindAll(girl => !factory.Stockpile.HasMet(girl.building));
                if (unmet.Count > 0)
                    candidates = unmet;
            }

            int total = 0;
            foreach (var girl in candidates)
                total += Mathf.Max(0, girl.rollWeight);

            int roll = Random.Range(0, total);
            foreach (var girl in candidates)
            {
                roll -= Mathf.Max(0, girl.rollWeight);
                if (roll < 0)
                    return girl;
            }
            return candidates[candidates.Count - 1];
        }
    }
}
