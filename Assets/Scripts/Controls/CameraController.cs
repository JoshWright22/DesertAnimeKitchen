using UnityEngine;
using UnityEngine.InputSystem;

namespace DessertFactory
{
    [RequireComponent(typeof(Camera))]
    public class CameraController : MonoBehaviour
    {
        public float panSpeed = 1.2f;
        public float zoomStep = 1.15f;
        public float minZoom = 3f;
        public float maxZoom = 40f;

        Camera cam;
        Vector2 mapSize;
        Vector3 dragOrigin;

        public void Init(Vector2 size)
        {
            mapSize = size;
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
                float scroll = mouse.scroll.ReadValue().y;
                if (scroll != 0f)
                    ZoomTowards(mouse.position.ReadValue(), scroll > 0 ? 1f / zoomStep : zoomStep);

                if (mouse.middleButton.wasPressedThisFrame)
                    dragOrigin = cam.ScreenToWorldPoint(mouse.position.ReadValue());
                if (mouse.middleButton.isPressed)
                {
                    var now = cam.ScreenToWorldPoint(mouse.position.ReadValue());
                    transform.position += dragOrigin - now;
                }
            }

            ClampToMap();
        }

        void ZoomTowards(Vector2 screenPoint, float factor)
        {
            var before = cam.ScreenToWorldPoint(screenPoint);
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize * factor, minZoom, maxZoom);
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
    }
}
