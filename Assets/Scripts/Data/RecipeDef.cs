using System.Collections.Generic;
using UnityEngine;

namespace DessertFactory
{
    [CreateAssetMenu(menuName = "Dessert Factory/Recipe")]
    public class RecipeDef : ScriptableObject
    {
        public string displayName;
        public List<ItemAmount> inputs = new List<ItemAmount>();
        public List<ItemAmount> outputs = new List<ItemAmount>();
        public float craftTime = 2f;

        public int InputAmount(ItemDef item)
        {
            foreach (var input in inputs)
            {
                if (input.item == item)
                    return input.amount;
            }
            return 0;
        }
    }
}
