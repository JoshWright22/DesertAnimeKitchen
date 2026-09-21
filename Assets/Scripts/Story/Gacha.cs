using System.Collections.Generic;
using UnityEngine;

namespace DessertFactory
{
    // Spend stars to pull a girl. Each pull is one more copy you can place, and plays her next story scene.
    public class Gacha : MonoBehaviour
    {
        [SerializeField] Factory factory;
        [SerializeField] CutscenePlayer cutscenes;
        [SerializeField] int rollCost = 10;

        readonly List<CharacterDef> pool = new List<CharacterDef>();
        readonly Dictionary<CharacterDef, int> chapters = new Dictionary<CharacterDef, int>();

        public int RollCost => rollCost;
        public bool CanRoll => pool.Count > 0 && factory.Stockpile.Stars >= rollCost && !cutscenes.Playing;

        // Has to run before the starting layout is placed so her buildings are already limited
        public void Init(GameContent content)
        {
            foreach (var girl in content.characters)
            {
                if (girl == null || girl.building == null)
                    continue;
                pool.Add(girl);
                factory.Stockpile.AddWorkers(girl.building, girl.startingCopies);
            }
        }

        public CharacterDef Roll(out bool firstTime)
        {
            firstTime = false;
            if (!CanRoll || !factory.Stockpile.TrySpendStars(rollCost))
                return null;

            var girl = Pick();
            firstTime = factory.Stockpile.Owned(girl.building) == 0;
            factory.Stockpile.AddWorkers(girl.building);
            return girl;
        }

        public void PlayNextScene(CharacterDef girl)
        {
            chapters.TryGetValue(girl, out int chapter);
            chapters[girl] = chapter + 1;
            if (chapter < girl.story.Count && girl.story[chapter] != null)
                cutscenes.Play(girl.story[chapter]);
        }

        CharacterDef Pick()
        {
            int total = 0;
            foreach (var girl in pool)
                total += Mathf.Max(0, girl.rollWeight);

            int roll = Random.Range(0, total);
            foreach (var girl in pool)
            {
                roll -= Mathf.Max(0, girl.rollWeight);
                if (roll < 0)
                    return girl;
            }
            return pool[pool.Count - 1];
        }
    }
}
