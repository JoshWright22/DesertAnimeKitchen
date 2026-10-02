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

        [Header("Look")]
        [SerializeField] Sprite vertical;
        [SerializeField] Sprite horizontal;
        [Tooltip("Belt ends, named after the side that's capped off")]
        [SerializeField] Sprite endUp;
        [SerializeField] Sprite endDown;
        [SerializeField] Sprite endLeft;
        [SerializeField] Sprite endRight;

        readonly List<BeltItem> items = new List<BeltItem>();

        // the art has no arrows, so the ghost shows which way it'll run
        public override bool ShowsFacing => true;

        public override Sprite SpriteFor(BuildingDef def, Direction facing)
        {
            return facing == Direction.Up || facing == Direction.Down ? vertical : horizontal;
        }

        // Caps the front if the line stops here, otherwise the back if nothing feeds in from behind
        public override void NeighboursChanged()
        {
            var behind = Origin - Facing.ToOffset();
            bool leadsOn = Factory.GetBuilding(OutputCell) is Conveyor;
            bool fedFromBehind = Factory.GetBuilding(behind) is Conveyor back && back.OutputCell == Origin;

            if (!leadsOn)
                Body.sprite = EndFacing(Facing);
            else if (!fedFromBehind)
                Body.sprite = EndFacing(Facing.Opposite());
            else
                Body.sprite = SpriteFor(Def, Facing);
        }

        Sprite EndFacing(Direction side)
        {
            switch (side)
            {
                case Direction.Up: return endUp;
                case Direction.Right: return endRight;
                case Direction.Down: return endDown;
                default: return endLeft;
            }
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

        // whatever was riding on it falls off into the sand
        public override void OnRemoved()
        {
            foreach (var beltItem in items)
                Factory.ItemViews.Release(beltItem.view);
            items.Clear();
        }

        // The item drawn closest to a point, for the hover tag
        public ItemDef ItemNear(Vector2 point)
        {
            ItemDef nearest = null;
            float best = float.MaxValue;
            foreach (var beltItem in items)
            {
                float distance = ((Vector2)beltItem.view.transform.position - point).sqrMagnitude;
                if (distance < best)
                {
                    best = distance;
                    nearest = beltItem.item;
                }
            }
            return nearest;
        }

        public override string GetStatus()
        {
            return items.Count == 0 ? "Empty" : $"Carrying {items.Count}";
        }
    }
}
