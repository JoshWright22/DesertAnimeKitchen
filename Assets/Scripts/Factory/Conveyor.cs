using System.Collections.Generic;
using UnityEngine;

namespace DessertFactory
{
    public class Conveyor : Building
    {
        const float Spacing = 0.5f;

        class BeltItem
        {
            public ItemDef item;
            public float progress;
            public Vector3 start;
            public SpriteRenderer view;
        }

        readonly List<BeltItem> items = new List<BeltItem>();

        protected override void BuildVisuals()
        {
            var body = new GameObject("Belt").AddComponent<SpriteRenderer>();
            body.transform.SetParent(transform, false);
            body.transform.localRotation = Quaternion.Euler(0, 0, Facing.ToAngle());
            body.transform.localScale = Factory.Map.Grid.cellSize;
            body.sprite = Def.sprite != null ? Def.sprite : SpriteFactory.Belt(Def.outfitColor);
            body.sortingOrder = 2;
        }

        public override bool TryInsert(ItemDef item, Vector2Int fromCell)
        {
            if (fromCell == OutputCell)
                return false;
            if (items.Count > 0 && items[items.Count - 1].progress < Spacing)
                return false;

            items.Add(new BeltItem
            {
                item = item,
                progress = 0f,
                start = Vector3.Lerp(transform.position, Factory.Map.CellToWorld(fromCell), 0.5f),
                view = Factory.ItemViews.Get(item)
            });
            return true;
        }

        public override void Tick(float deltaTime)
        {
            float speed = 1f / Mathf.Max(0.05f, Def.workTime);
            var center = transform.position;
            var end = Vector3.Lerp(center, Factory.Map.CellToWorld(OutputCell), 0.5f);

            for (int i = 0; i < items.Count; i++)
            {
                var beltItem = items[i];
                float limit = i == 0 ? 1f : items[i - 1].progress - Spacing;
                beltItem.progress = Mathf.Max(beltItem.progress, Mathf.Min(beltItem.progress + speed * deltaTime, limit));

                // go to the middle first so items coming in from the side turn the corner
                var t = beltItem.progress;
                beltItem.view.transform.position = t < 0.5f
                    ? Vector3.Lerp(beltItem.start, center, t * 2f)
                    : Vector3.Lerp(center, end, (t - 0.5f) * 2f);
            }

            if (items.Count > 0 && items[0].progress >= 1f && TryPushForward(items[0].item))
            {
                Factory.ItemViews.Release(items[0].view);
                items.RemoveAt(0);
            }
        }

        public override void OnRemoved()
        {
            foreach (var beltItem in items)
            {
                Factory.Stockpile.Add(beltItem.item);
                Factory.ItemViews.Release(beltItem.view);
            }
            items.Clear();
        }

        public override string GetStatus()
        {
            return items.Count == 0 ? "Empty" : $"Carrying {items.Count}";
        }
    }
}
