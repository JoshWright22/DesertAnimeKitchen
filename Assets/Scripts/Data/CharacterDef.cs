using System.Collections.Generic;
using UnityEngine;

namespace DessertFactory
{
    // A girl you can pull from the gacha. Every copy you pull lets you place one more of her.
    [CreateAssetMenu(menuName = "Dessert Factory/Character")]
    public class CharacterDef : ScriptableObject
    {
        public string displayName;
        public Color nameColor = Color.white;
        [Tooltip("The building she works as. Give it her chibi as its sprite.")]
        public BuildingDef building;
        [Tooltip("Shown in cutscenes when a line doesn't pick a portrait")]
        public Sprite portrait;

        [Header("Gacha")]
        [Tooltip("Higher is more common")]
        public int rollWeight = 10;
        [Tooltip("Copies you have from the start, on top of any the starting layout places")]
        public int startingCopies;

        [Tooltip("One scene plays each time you pull her, in order. The first one introduces her.")]
        public List<Cutscene> story = new List<Cutscene>();
    }
}
