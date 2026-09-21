using UnityEngine;
using UnityEngine.InputSystem;

namespace DessertFactory
{
    public class BuildController : MonoBehaviour
    {
        [SerializeField] Factory factory;
        [SerializeField] Hud hud;
        [SerializeField] Camera cam;
        [SerializeField] CameraController cameraController;
        [SerializeField] SpriteRenderer ghost;
        [SerializeField] SpriteRenderer ghostArrow;

        GameContent content;

        public BuildingDef Selected { get; private set; }
        public Direction Facing { get; private set; } = Direction.Right;
        public Vector2Int HoveredCell { get; private set; }
        public Vector2Int PlacementOrigin { get; private set; }
        public bool PointerOverWorld { get; private set; }
        public string PlaceError { get; private set; }

        void Awake()
        {
            if (ghostArrow.sprite == null)
                ghostArrow.sprite = SpriteFactory.Arrow();
        }

        // Content can be made at startup, so it gets handed over instead of assigned
        public void Init(GameContent content)
        {
            this.content = content;
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
            if (mouse.rightButton.wasReleasedThisFrame && !cameraController.DraggedThisPress)
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

            var prefab = Selected.prefab;
            ghost.sprite = prefab.SpriteFor(Selected);
            if (ghost.sprite == null)
                prefab.ShowPlaceholder(ghost);
            ghost.transform.rotation = prefab.TurnsBody ? Quaternion.Euler(0, 0, Facing.ToAngle()) : Quaternion.identity;
            ghostArrow.transform.rotation = Quaternion.Euler(0, 0, Facing.ToAngle());
        }

        void UpdateGhost()
        {
            bool show = Selected != null && PointerOverWorld;
            ghost.gameObject.SetActive(show);
            ghostArrow.gameObject.SetActive(show && Selected.prefab.HasOutputArrow);
            PlaceError = null;
            if (!show)
                return;

            // keep the cursor roughly in the middle of bigger footprints
            var size = Building.RotatedSize(Selected.size, Facing);
            PlacementOrigin = HoveredCell - new Vector2Int((size.x - 1) / 2, (size.y - 1) / 2);

            var map = factory.Map;
            ghost.transform.position = map.FootprintCenter(PlacementOrigin, size);
            ghost.transform.localScale = Building.FitScale(ghost.sprite, Selected.prefab.TurnsBody ? Selected.size : size, map.Grid.cellSize);

            var output = Building.GetOutputCell(PlacementOrigin, size, Facing);
            ghostArrow.transform.position = Vector3.Lerp(map.CellToWorld(output - Facing.ToOffset()), map.CellToWorld(output), 0.42f);

            bool ok = factory.CanPlace(Selected, PlacementOrigin, Facing, out string reason);
            PlaceError = reason;
            ghost.color = ok ? new Color(0.6f, 1f, 0.6f, 0.7f) : new Color(1f, 0.4f, 0.4f, 0.6f);
            ghostArrow.color = ghost.color;
        }
    }
}
