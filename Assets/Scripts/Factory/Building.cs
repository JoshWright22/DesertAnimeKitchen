using UnityEngine;

namespace DessertFactory
{
    public abstract class Building : MonoBehaviour
    {
        public BuildingDef Def { get; private set; }
        public Vector2Int Tile { get; private set; }
        public Direction Facing { get; private set; }

        protected Factory Factory { get; private set; }
        protected Vector2Int FrontTile => Tile + Facing.ToOffset();

        public void Init(Factory factory, BuildingDef def, Vector2Int tile, Direction facing)
        {
            Factory = factory;
            Def = def;
            Tile = tile;
            Facing = facing;
            transform.position = DesertMap.TileToWorld(tile);
            BuildVisuals();
        }

        protected virtual void BuildVisuals()
        {
            var body = new GameObject("Body").AddComponent<SpriteRenderer>();
            body.transform.SetParent(transform, false);
            body.sprite = SpriteFactory.Girl(Def.outfitColor, Def.hairColor);
            body.sortingOrder = 5;

            // little marker showing which way she hands things off
            var arrow = new GameObject("Output").AddComponent<SpriteRenderer>();
            arrow.transform.SetParent(transform, false);
            arrow.transform.localPosition = (Vector3)(Vector2)Facing.ToOffset() * 0.42f;
            arrow.transform.localRotation = Quaternion.Euler(0, 0, Facing.ToAngle());
            arrow.transform.localScale = Vector3.one * 0.3f;
            arrow.sprite = SpriteFactory.Arrow();
            arrow.color = new Color(1f, 1f, 1f, 0.8f);
            arrow.sortingOrder = 6;
        }

        public virtual void Tick(float deltaTime)
        {
        }

        // Called by neighbours handing us an item. Return false to refuse it.
        public abstract bool TryInsert(ItemDef item, Vector2Int fromTile);

        public virtual void OnRemoved()
        {
        }

        public virtual string GetStatus()
        {
            return string.Empty;
        }

        protected bool TryPushForward(ItemDef item)
        {
            var target = Factory.GetBuilding(FrontTile);
            return target != null && target.TryInsert(item, Tile);
        }
    }
}
