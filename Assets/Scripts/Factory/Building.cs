using System.Collections.Generic;
using UnityEngine;

namespace DessertFactory
{
    public abstract class Building : MonoBehaviour
    {
        public BuildingDef Def { get; private set; }
        public Vector2Int Origin { get; private set; }
        public Vector2Int Size { get; private set; }
        public Direction Facing { get; private set; }

        protected Factory Factory { get; private set; }

        // The cell just past our front edge, where finished items get handed off
        public Vector2Int OutputCell => GetOutputCell(Origin, Size, Facing);

        public IEnumerable<Vector2Int> Cells
        {
            get
            {
                for (int y = 0; y < Size.y; y++)
                    for (int x = 0; x < Size.x; x++)
                        yield return new Vector2Int(Origin.x + x, Origin.y + y);
            }
        }

        public static Vector2Int RotatedSize(Vector2Int size, Direction facing)
        {
            return facing == Direction.Left || facing == Direction.Right ? new Vector2Int(size.y, size.x) : size;
        }

        public static Vector2Int GetOutputCell(Vector2Int origin, Vector2Int size, Direction facing)
        {
            switch (facing)
            {
                case Direction.Up: return new Vector2Int(origin.x + (size.x - 1) / 2, origin.y + size.y);
                case Direction.Right: return new Vector2Int(origin.x + size.x, origin.y + (size.y - 1) / 2);
                case Direction.Down: return new Vector2Int(origin.x + (size.x - 1) / 2, origin.y - 1);
                default: return new Vector2Int(origin.x - 1, origin.y + (size.y - 1) / 2);
            }
        }

        public void Init(Factory factory, BuildingDef def, Vector2Int origin, Direction facing)
        {
            Factory = factory;
            Def = def;
            Origin = origin;
            Facing = facing;
            Size = RotatedSize(def.size, facing);
            transform.position = factory.Map.FootprintCenter(origin, Size);
            BuildVisuals();
        }

        protected virtual void BuildVisuals()
        {
            var cellSize = Factory.Map.Grid.cellSize;

            var body = new GameObject("Body").AddComponent<SpriteRenderer>();
            body.transform.SetParent(transform, false);
            body.transform.localScale = new Vector3(Size.x * cellSize.x, Size.y * cellSize.y, 1f);
            body.sprite = Def.sprite != null ? Def.sprite : SpriteFactory.Square();
            if (Def.sprite == null)
                body.color = new Color(0.5f, 0.5f, 0.55f);
            body.sortingOrder = 5;

            // little marker on the front edge showing where she hands things off
            var lastCell = Factory.Map.CellToWorld(OutputCell - Facing.ToOffset());
            var arrow = new GameObject("Output").AddComponent<SpriteRenderer>();
            arrow.transform.SetParent(transform, false);
            arrow.transform.position = Vector3.Lerp(lastCell, Factory.Map.CellToWorld(OutputCell), 0.42f);
            arrow.transform.rotation = Quaternion.Euler(0, 0, Facing.ToAngle());
            arrow.transform.localScale = Vector3.one * 0.3f;
            arrow.sprite = SpriteFactory.Arrow();
            arrow.color = new Color(1f, 1f, 1f, 0.8f);
            arrow.sortingOrder = 6;
        }

        public virtual void Tick(float deltaTime)
        {
        }

        // Called by neighbours handing us an item from one of their cells. Return false to refuse it.
        public abstract bool TryInsert(ItemDef item, Vector2Int fromCell);

        public virtual void OnRemoved()
        {
        }

        public virtual string GetStatus()
        {
            return string.Empty;
        }

        protected bool TryPushForward(ItemDef item)
        {
            var target = Factory.GetBuilding(OutputCell);
            return target != null && target != this && target.TryInsert(item, OutputCell - Facing.ToOffset());
        }
    }
}
