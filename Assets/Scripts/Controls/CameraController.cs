using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace DessertFactory
{
    [RequireComponent(typeof(Camera))]
    public class CameraController : MonoBehaviour
    {
        public float panSpeed = 1.2f;
        public float zoomStep = 1.15f;
        public float minZoom = 3f;
        [Tooltip("Hard limit, zooming out also stops once grid lines would get thinner than a pixel")]
        public float maxZoom = 40f;
        [Tooltip("How far in pixels the mouse has to move before a right click counts as a drag")]
        public float dragThreshold = 6f;

        Camera cam;
        Vector2 mapSize;
        float cellHeight = 1f;

        bool dragging;
        Vector2 pressScreenPos;
        Vector3 dragWorldOrigin;

        // True while the right button is held and has moved far enough to be a drag
        public bool IsDragging => dragging;

        // Stays true on the frame the button is released so clicks can tell they were really drags
        public bool DraggedThisPress { get; private set; }

        public void Init(Vector2 size, float cellWorldHeight)
        {
            mapSize = size;
            cellHeight = cellWorldHeight;
        }

        // Grid lines are one texel of a SpriteFactory.GridTexels sized cell, so a cell has to
        // cover at least that many screen pixels or the lines start breaking up
        float MaxZoom
        {
            get
            {
                float limit = cellHeight * Screen.height / (2f * SpriteFactory.GridTexels);
                return Mathf.Max(minZoom, Mathf.Min(maxZoom, limit));
            }
        }

        void Awake()
        {
            cam = GetComponent<Camera>();
        }

        void Update()
        {
            var keyboard = Keyboard.current;
            var mouse = Mouse.current;

            if (keyboard != null)
            {
                var move = Vector2.zero;
                if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) move.y += 1;
                if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) move.y -= 1;
                if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) move.x -= 1;
                if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) move.x += 1;

                float speed = panSpeed * cam.orthographicSize * (keyboard.shiftKey.isPressed ? 2.5f : 1f);
                transform.position += (Vector3)(move.normalized * speed * Time.unscaledDeltaTime);
            }

            if (mouse != null)
            {
                HandleDrag(mouse);

                float scroll = mouse.scroll.ReadValue().y;
                if (scroll != 0f && !PointerOverUi())
                    ZoomTowards(mouse.position.ReadValue(), scroll > 0 ? 1f / zoomStep : zoomStep);
            }

            // window size can change, so keep checking
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, MaxZoom);
            ClampToMap();
        }

        void HandleDrag(Mouse mouse)
        {
            var screenPos = mouse.position.ReadValue();

            if (mouse.rightButton.wasPressedThisFrame)
            {
                DraggedThisPress = false;
                if (!PointerOverUi())
                {
                    pressScreenPos = screenPos;
                    dragWorldOrigin = cam.ScreenToWorldPoint(screenPos);
                }
                else
                {
                    // started on the UI, don't let this press pan the map
                    DraggedThisPress = true;
                }
            }

            if (mouse.rightButton.isPressed && !DraggedThisPress && Vector2.Distance(screenPos, pressScreenPos) > dragThreshold)
            {
                dragging = true;
                DraggedThisPress = true;
            }

            if (dragging)
            {
                // keep the point we grabbed under the cursor
                var now = cam.ScreenToWorldPoint(screenPos);
                transform.position += dragWorldOrigin - now;
            }

            if (!mouse.rightButton.isPressed)
                dragging = false;
        }

        void ZoomTowards(Vector2 screenPoint, float factor)
        {
            var before = cam.ScreenToWorldPoint(screenPoint);
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize * factor, minZoom, MaxZoom);
            var after = cam.ScreenToWorldPoint(screenPoint);
            transform.position += before - after;
        }

        void ClampToMap()
        {
            var pos = transform.position;
            pos.x = Mathf.Clamp(pos.x, 0f, mapSize.x);
            pos.y = Mathf.Clamp(pos.y, 0f, mapSize.y);
            transform.position = pos;
        }

        static bool PointerOverUi()
        {
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        }
    }
}
