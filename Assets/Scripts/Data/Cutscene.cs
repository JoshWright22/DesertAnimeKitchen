using System;
using System.Collections.Generic;
using UnityEngine;

namespace DessertFactory
{
    [CreateAssetMenu(menuName = "Dessert Factory/Cutscene")]
    public class Cutscene : ScriptableObject
    {
        [Serializable]
        public class Line
        {
            [Tooltip("Leave empty for narration")]
            public CharacterDef speaker;
            [Tooltip("Leave empty to use her default portrait")]
            public Sprite portrait;
            [TextArea(2, 5)] public string text;
        }

        public string title;
        [Tooltip("The factory just gets dimmed if this is empty")]
        public Sprite background;
        public List<Line> lines = new List<Line>();
    }
}
