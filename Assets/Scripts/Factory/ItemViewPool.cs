using System.Collections.Generic;
using UnityEngine;

namespace DessertFactory
{
    public class ItemViewPool : MonoBehaviour
    {
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
                var go = new GameObject("Item");
                go.transform.SetParent(transform, false);
                go.transform.localScale = Vector3.one * 0.35f;
                view = go.AddComponent<SpriteRenderer>();
                view.sprite = SpriteFactory.Circle();
                view.sortingOrder = 10;
            }

            view.color = item.color;
            return view;
        }

        public void Release(SpriteRenderer view)
        {
            view.gameObject.SetActive(false);
            free.Push(view);
        }
    }
}
