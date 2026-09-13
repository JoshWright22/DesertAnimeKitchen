using UnityEngine;
using UnityEngine.InputSystem;

namespace DessertFactory
{
    public class BuildController : MonoBehaviour
    {
        Factory factory;
        GameContent content;
        Camera cam;
        Hud hud;

        SpriteRenderer ghost;
        SpriteRenderer ghostArrow;

        public BuildingDef Selected { get; private set; }
        public Direction Facing { get; private set; } = Direction.Right;
        public Vector2Int HoveredTile { get; private set; }
        public bool PointerOverWorld { get; private set; }
        public string PlaceError { get; private set; }

        public void Init(Factory factory, GameContent content, Camera cam, Hud hud)
        {
            this.factory = factory;
            this.content = content;
            this.cam = cam;
            this.hud = hud;

            ghost = new GameObject("Build Ghost").AddComponent<SpriteRenderer>();
            ghost.transform.SetParent(transform, false);
            ghost.sortingOrder = 20;

            ghostArrow = new GameObject("Ghost Arrow").AddComponent<SpriteRenderer>();
            ghostArrow.transform.SetParent(ghost.transform, false);
            ghostArrow.transform.localScale = Vector3.one * 0.3f;
            ghostArrow.sprite = SpriteFactory.Arrow();
            ghostArrow.sortingOrder = 21;

            ghost.gameObject.SetActive(false);
        }

        public void Select(BuildingDef def)
        {
            Selected = def;
            RefreshGhostSprite();
        }

        void Update()
        {
            var keyboard = Keyboard.current;
            var mouse = Mouse.current;
            if (keyboard == null || mouse == null)
                return;

            HandleHotkeys(keyboard);

            var screenPos = mouse.position.ReadValue();
            PointerOverWorld = !hud.IsOverUi(screenPos);
            HoveredTile = DesertMap.WorldToTile(cam.ScreenToWorldPoint(screenPos));

            UpdateGhost();

            if (!PointerOverWorld)
                return;

            if (Selected != null)
            {
                if (mouse.leftButton.isPressed && factory.GetBuilding(HoveredTile) == null)
                    factory.Place(Selected, HoveredTile, Facing);
            }
            else if (mouse.leftButton.wasPressedThisFrame && factory.GetBuilding(HoveredTile) is CookGirl cook)
            {
                cook.NextRecipe();
            }

            if (mouse.rightButton.isPressed)
                factory.Remove(HoveredTile);
        }

        void HandleHotkeys(Keyboard keyboard)
        {
            for (int i = 0; i < content.buildings.Count && i < 9; i++)
            {
                if (keyboard[Key.Digit1 + i].wasPressedThisFrame)
                    Select(Selected == content.buildings[i] ? null : content.buildings[i]);
            }

            if (keyboard.escapeKey.wasPressedThisFrame || keyboard.qKey.wasPressedThisFrame)
                Select(null);

            if (keyboard.rKey.wasPressedThisFrame)
            {
                Facing = Facing.RotateClockwise();
                RefreshGhostSprite();
            }
        }

        void RefreshGhostSprite()
        {
            if (Selected == null)
                return;

            if (Selected.kind == BuildingKind.Conveyor)
            {
                ghost.sprite = SpriteFactory.Belt(Selected.outfitColor);
                ghost.transform.rotation = Quaternion.Euler(0, 0, Facing.ToAngle());
                ghostArrow.gameObject.SetActive(false);
            }
            else
            {
                ghost.sprite = SpriteFactory.Girl(Selected.outfitColor, Selected.hairColor);
                ghost.transform.rotation = Quaternion.identity;
                ghostArrow.gameObject.SetActive(true);
                ghostArrow.transform.localPosition = (Vector3)(Vector2)Facing.ToOffset() * 0.42f;
                ghostArrow.transform.localRotation = Quaternion.Euler(0, 0, Facing.ToAngle());
            }
        }

        void UpdateGhost()
        {
            bool show = Selected != null && PointerOverWorld;
            ghost.gameObject.SetActive(show);
            PlaceError = null;
            if (!show)
                return;

            ghost.transform.position = DesertMap.TileToWorld(HoveredTile);
            bool ok = factory.CanPlace(Selected, HoveredTile, out string reason);
            PlaceError = reason;
            ghost.color = ok ? new Color(0.6f, 1f, 0.6f, 0.7f) : new Color(1f, 0.4f, 0.4f, 0.6f);
            ghostArrow.color = ghost.color;
        }
    }
}
