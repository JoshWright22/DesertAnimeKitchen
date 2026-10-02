using System;
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
        [Serializable]
        public class Expression
        {
            [Tooltip("Put #tag at the end of a Yarn line to use it, like #cringe")]
            public string tag;
            public Sprite sprite;
        }

        [Tooltip("Shown in cutscenes when a line doesn't pick a portrait")]
        public Sprite portrait;
        [Tooltip("Other faces she pulls while talking")]
        public List<Expression> expressions = new List<Expression>();
        [Tooltip("Shown instead of her name until the Yarn variable below is true")]
        public string alias;
        [Tooltip("Yarn bool variable, like $knows_jennifer")]
        public string revealedBy;

        [Header("Gacha")]
        [Tooltip("Higher is more common")]
        public int rollWeight = 10;
        [Tooltip("Copies you have from the start, on top of any the starting layout places")]
        public int startingCopies;

        // The face for a line's tags, or her normal portrait if none of them match
        public Sprite PortraitFor(IEnumerable<string> tags)
        {
            foreach (var tag in tags)
            {
                var match = expressions.Find(e => string.Equals(e.tag, tag.TrimStart('#'), StringComparison.OrdinalIgnoreCase));
                if (match != null && match.sprite != null)
                    return match.sprite;
            }
            return portrait;
        }
    }
}
