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
        Campaign campaign;

        public BuildingDef Selected { get; private set; }
        public Direction Facing { get; private set; } = Direction.Right;
        public Vector2Int HoveredCell { get; private set; }
        public Vector2 PointerWorld { get; private set; }
        public Vector2Int PlacementOrigin { get; private set; }
        public bool PointerOverWorld { get; private set; }

        void Awake()
        {
            if (ghostArrow.sprite == null)
                ghostArrow.sprite = SpriteFactory.Arrow();
        }

        // Handed over by the bootstrap so every system reads the same content
        public void Init(GameContent content, Campaign campaign)
        {
            this.content = content;
            this.campaign = campaign;
        }

        public void Select(BuildingDef def)
        {
            // girls you haven't pulled yet can't be picked up at all
            if (def != null && (campaign.IsLocked(def) || factory.Stockpile.IsGachaGirl(def) && !factory.Stockpile.HasMet(def)))
                return;
            if (def != null)
                hud.HideRecipes();
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
            if (Selected != null && campaign.IsLocked(Selected))
                Select(null);

            var screenPos = mouse.position.ReadValue();
            PointerOverWorld = !hud.IsPointerOverUi();
            PointerWorld = cam.ScreenToWorldPoint(screenPos);
            HoveredCell = factory.Map.WorldToCell(PointerWorld);

            UpdateGhost();

            if (!PointerOverWorld)
                return;

            // X or Delete picks up whatever's under the cursor, even while you're holding something
            if (keyboard.xKey.isPressed || keyboard.deleteKey.isPressed)
            {
                factory.Remove(HoveredCell);
            }
            else if (Selected != null)
            {
                if (mouse.leftButton.isPressed)
                    factory.Place(Selected, PlacementOrigin, Facing, overwrite: CurrentOverwrite(mouse));
            }
            else if (mouse.leftButton.wasPressedThisFrame)
            {
                // click a cook to see her recipe, click anywhere else to put it away
                if (factory.GetBuilding(HoveredCell) is CookGirl cook)
                    hud.ShowRecipes(cook);
                else
                    hud.HideRecipes();
            }

            // right drag pans the camera. A plain right click turns what you're placing, or removes when you aren't placing.
            if (mouse.rightButton.wasReleasedThisFrame && !cameraController.DraggedThisPress)
            {
                if (Selected != null)
                    Rotate();
                else
                    factory.Remove(HoveredCell);
            }
        }

        // A fresh click can put anything down on top of anything, holding it down only reroutes belts
        static Factory.Overwrite CurrentOverwrite(Mouse mouse)
        {
            return mouse.leftButton.wasPressedThisFrame ? Factory.Overwrite.Anything : Factory.Overwrite.Belts;
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
                Rotate();
        }

        void Rotate()
        {
            Facing = Facing.RotateClockwise();
            RefreshGhostSprite();
        }

        void RefreshGhostSprite()
        {
            if (Selected == null)
                return;

            var prefab = Selected.prefab;
            ghost.sprite = prefab.SpriteFor(Selected, Facing);
            if (ghost.sprite == null)
                prefab.ShowPlaceholder(ghost);
            ghost.transform.rotation = prefab.TurnsBody ? Quaternion.Euler(0, 0, Facing.ToAngle()) : Quaternion.identity;
            ghostArrow.transform.rotation = Quaternion.Euler(0, 0, Facing.ToAngle());
        }

        void UpdateGhost()
        {
            bool show = Selected != null && PointerOverWorld;
            ghost.gameObject.SetActive(show);
            ghostArrow.gameObject.SetActive(show && Selected.prefab.ShowsFacing);
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

            bool ok = factory.CanPlace(Selected, PlacementOrigin, Facing, out _, overwrite: Factory.Overwrite.Anything);
            if (!ok)
                ghost.color = new Color(1f, 0.4f, 0.4f, 0.6f);
            else if (factory.WouldReplace)
                ghost.color = new Color(1f, 0.85f, 0.4f, 0.7f);
            else
                ghost.color = new Color(0.6f, 1f, 0.6f, 0.7f);
            ghostArrow.color = ghost.color;
        }
    }
}
