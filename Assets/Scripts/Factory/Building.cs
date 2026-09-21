using System.Collections.Generic;
using UnityEngine;

namespace DessertFactory
{
    public abstract class Building : MonoBehaviour
    {
        [SerializeField] SpriteRenderer body;
        [Tooltip("Marker on the front edge showing where she hands things off. Leave empty if there's no front.")]
        [SerializeField] SpriteRenderer outputArrow;
        [Tooltip("Rotate the body to match the way it was placed, like belts")]
        [SerializeField] bool turnBody;

        public BuildingDef Def { get; private set; }
        public Vector2Int Origin { get; private set; }
        public Vector2Int Size { get; private set; }
        public Direction Facing { get; private set; }

        protected Factory Factory { get; private set; }

        public bool TurnsBody => turnBody;
        public bool HasOutputArrow => outputArrow != null;

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

            body.sprite = SpriteFor(def);
            if (body.sprite == null)
                ShowPlaceholder(body);
            if (turnBody)
                body.transform.localRotation = Quaternion.Euler(0, 0, facing.ToAngle());
            body.transform.localScale = FitScale(body.sprite, turnBody ? def.size : Size, factory.Map.Grid.cellSize);

            if (outputArrow != null)
            {
                if (outputArrow.sprite == null)
                    outputArrow.sprite = SpriteFactory.Arrow();
                var lastCell = factory.Map.CellToWorld(OutputCell - facing.ToOffset());
                outputArrow.transform.position = Vector3.Lerp(lastCell, factory.Map.CellToWorld(OutputCell), 0.42f);
                outputArrow.transform.rotation = Quaternion.Euler(0, 0, facing.ToAngle());
            }
        }

        public Sprite SpriteFor(BuildingDef def) => def.sprite != null ? def.sprite : body.sprite;

        // Only used until the building has real art
        public virtual void ShowPlaceholder(SpriteRenderer target)
        {
            target.sprite = SpriteFactory.Square();
            target.color = new Color(0.5f, 0.5f, 0.55f);
        }

        // Sizes the sprite to the footprint without squashing it, whatever resolution the art is
        public static Vector3 FitScale(Sprite sprite, Vector2Int cells, Vector3 cellSize)
        {
            var spriteSize = sprite.bounds.size;
            float scale = Mathf.Min(cells.x * cellSize.x / spriteSize.x, cells.y * cellSize.y / spriteSize.y);
            return new Vector3(scale, scale, 1f);
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
