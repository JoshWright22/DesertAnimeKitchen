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
        CameraController cameraController;

        SpriteRenderer ghost;
        SpriteRenderer ghostArrow;
        bool gridToggled;

        public BuildingDef Selected { get; private set; }
        public Direction Facing { get; private set; } = Direction.Right;
        public Vector2Int HoveredCell { get; private set; }
        public Vector2Int PlacementOrigin { get; private set; }
        public bool PointerOverWorld { get; private set; }
        public string PlaceError { get; private set; }

        public void Init(Factory factory, GameContent content, Camera cam, Hud hud)
        {
            this.factory = factory;
            this.content = content;
            this.cam = cam;
            this.hud = hud;
            cameraController = cam.GetComponent<CameraController>();

            ghost = new GameObject("Build Ghost").AddComponent<SpriteRenderer>();
            ghost.transform.SetParent(transform, false);
            ghost.sortingOrder = 20;

            ghostArrow = new GameObject("Ghost Arrow").AddComponent<SpriteRenderer>();
            ghostArrow.transform.SetParent(transform, false);
            ghostArrow.transform.localScale = Vector3.one * 0.3f;
            ghostArrow.sprite = SpriteFactory.Arrow();
            ghostArrow.sortingOrder = 21;

            ghost.gameObject.SetActive(false);
            ghostArrow.gameObject.SetActive(false);
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
            PointerOverWorld = !hud.IsPointerOverUi();
            HoveredCell = factory.Map.WorldToCell(cam.ScreenToWorldPoint(screenPos));

            factory.Map.ShowGridLines = gridToggled || Selected != null;
            UpdateGhost();

            if (!PointerOverWorld)
                return;

            if (Selected != null)
            {
                if (mouse.leftButton.isPressed && factory.CanPlace(Selected, PlacementOrigin, Facing, out _))
                    factory.Place(Selected, PlacementOrigin, Facing);
            }
            else if (mouse.leftButton.wasPressedThisFrame && factory.GetBuilding(HoveredCell) is CookGirl cook)
            {
                cook.NextRecipe();
            }

            // right drag pans the camera, a plain right click removes
            if (mouse.rightButton.wasReleasedThisFrame && (cameraController == null || !cameraController.DraggedThisPress))
                factory.Remove(HoveredCell);
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

            if (keyboard.gKey.wasPressedThisFrame)
                gridToggled = !gridToggled;

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
                ghost.sprite = Selected.sprite != null ? Selected.sprite : SpriteFactory.Belt(Selected.outfitColor);
                ghost.transform.rotation = Quaternion.Euler(0, 0, Facing.ToAngle());
            }
            else
            {
                ghost.sprite = Selected.sprite != null ? Selected.sprite : SpriteFactory.Girl(Selected.outfitColor, Selected.hairColor);
                ghost.transform.rotation = Quaternion.identity;
            }

            ghostArrow.transform.rotation = Quaternion.Euler(0, 0, Facing.ToAngle());
        }

        void UpdateGhost()
        {
            bool show = Selected != null && PointerOverWorld;
            ghost.gameObject.SetActive(show);
            ghostArrow.gameObject.SetActive(show && Selected.kind != BuildingKind.Conveyor);
            PlaceError = null;
            if (!show)
                return;

            // keep the cursor roughly in the middle of bigger footprints
            var size = Building.RotatedSize(Selected.size, Facing);
            PlacementOrigin = HoveredCell - new Vector2Int((size.x - 1) / 2, (size.y - 1) / 2);

            var map = factory.Map;
            var cellSize = map.Grid.cellSize;
            ghost.transform.position = map.FootprintCenter(PlacementOrigin, size);
            ghost.transform.localScale = Selected.kind == BuildingKind.Conveyor
                ? cellSize
                : new Vector3(size.x * cellSize.x, size.y * cellSize.y, 1f);

            var output = Building.GetOutputCell(PlacementOrigin, size, Facing);
            ghostArrow.transform.position = Vector3.Lerp(map.CellToWorld(output - Facing.ToOffset()), map.CellToWorld(output), 0.42f);

            bool ok = factory.CanPlace(Selected, PlacementOrigin, Facing, out string reason);
            PlaceError = reason;
            ghost.color = ok ? new Color(0.6f, 1f, 0.6f, 0.7f) : new Color(1f, 0.4f, 0.4f, 0.6f);
            ghostArrow.color = ghost.color;
        }
    }
}
