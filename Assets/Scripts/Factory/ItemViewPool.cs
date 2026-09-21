using System.Collections.Generic;
using UnityEngine;

namespace DessertFactory
{
    public class ItemViewPool : MonoBehaviour
    {
        [SerializeField] SpriteRenderer viewPrefab;

        readonly Stack<SpriteRenderer> free = new Stack<SpriteRenderer>();

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
            return view;
        }

        public void Release(SpriteRenderer view)
        {
            view.gameObject.SetActive(false);
            free.Push(view);
        }
    }
}
