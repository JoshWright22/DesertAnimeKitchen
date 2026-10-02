using System.Collections.Generic;
using UnityEngine;

namespace DessertFactory
{
    public class ItemViewPool : MonoBehaviour
    {
        [SerializeField] SpriteRenderer viewPrefab;

        readonly Stack<SpriteRenderer> free = new Stack<SpriteRenderer>();
        // views out on the belts that glint, each with its own head start so a belt doesn't flash all at once
        readonly Dictionary<SpriteRenderer, (ItemDef item, float offset)> sparkling = new Dictionary<SpriteRenderer, (ItemDef, float)>();

        public SpriteRenderer Get(ItemDef item)
        {
            SpriteRenderer view;
            if (free.Count > 0)
            {
                view = free.Pop();
                view.gameObject.SetActive(true);
            }
            else
            {
                view = Instantiate(viewPrefab, transform);
            }

            // icons already have their colors baked in
            view.sprite = item.icon != null ? item.icon : SpriteFactory.Circle();
            view.color = item.icon != null ? Color.white : item.color;
            if (item.Sparkles)
                sparkling[view] = (item, Random.value * item.sparkleEvery);
            return view;
        }

        public void Release(SpriteRenderer view)
        {
            sparkling.Remove(view);
            view.gameObject.SetActive(false);
            free.Push(view);
        }

        void Update()
        {
            foreach (var pair in sparkling)
                pair.Key.sprite = pair.Value.item.IconAt(Time.time + pair.Value.offset);
        }
    }
}
