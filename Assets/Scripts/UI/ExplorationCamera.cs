using UnityEngine;
using UnityEngine.InputSystem;

namespace GladiusAI
{
    // Camara de recorrido para evaluar la ambientacion de un nivel: mover con WASD / flechas o arrastrando
    // (mouse o un dedo), zoom con la rueda o pellizcando. Va en la Main Camera ortografica.
    [RequireComponent(typeof(Camera))]
    public class ExplorationCamera : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 8f;
        [SerializeField] private float dragSpeed = 0.02f;
        [SerializeField] private float zoomStep = 0.5f;
        [SerializeField] private float minSize = 2f;
        [SerializeField] private float maxSize = 14f;
        [SerializeField] private Vector2 limitX = new Vector2(-20f, 20f);
        [SerializeField] private Vector2 limitZ = new Vector2(-25f, 15f);

        private Camera cam;

        private void Awake() => cam = GetComponent<Camera>();

        private void Update()
        {
            Vector2 move = ReadKeyboard() * moveSpeed * Time.unscaledDeltaTime;
            Vector2 drag = ReadDrag();
            Pan(move - drag * dragSpeed * cam.orthographicSize);
            Zoom(ReadZoom());
        }

        private static Vector2 ReadKeyboard()
        {
            var kb = Keyboard.current;
            if (kb == null) return Vector2.zero;

            float x = (kb.dKey.isPressed || kb.rightArrowKey.isPressed ? 1f : 0f) - (kb.aKey.isPressed || kb.leftArrowKey.isPressed ? 1f : 0f);
            float y = (kb.wKey.isPressed || kb.upArrowKey.isPressed ? 1f : 0f) - (kb.sKey.isPressed || kb.downArrowKey.isPressed ? 1f : 0f);
            return new Vector2(x, y);
        }

        private static Vector2 ReadDrag()
        {
            var touch = Touchscreen.current;
            if (touch != null && touch.touches.Count > 0 && touch.touches[0].isInProgress && !touch.touches[1].isInProgress)
                return touch.touches[0].delta.ReadValue();

            var mouse = Mouse.current;
            if (mouse != null && mouse.leftButton.isPressed)
                return mouse.delta.ReadValue();

            return Vector2.zero;
        }

        private float ReadZoom()
        {
            var mouse = Mouse.current;
            if (mouse != null && mouse.scroll.ReadValue().y != 0f)
                return -Mathf.Sign(mouse.scroll.ReadValue().y) * zoomStep;

            var touch = Touchscreen.current;
            if (touch != null && touch.touches[0].isInProgress && touch.touches[1].isInProgress)
            {
                Vector2 a = touch.touches[0].position.ReadValue();
                Vector2 b = touch.touches[1].position.ReadValue();
                Vector2 da = touch.touches[0].delta.ReadValue();
                Vector2 db = touch.touches[1].delta.ReadValue();
                float before = ((a - da) - (b - db)).magnitude;
                float after = (a - b).magnitude;
                return (before - after) * 0.01f;
            }

            return 0f;
        }

        // Se mueve sobre el piso, no hacia arriba: el forward de la camara se aplana en el plano XZ.
        private void Pan(Vector2 input)
        {
            Vector3 right = transform.right;
            Vector3 forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
            Vector3 pos = transform.position + right * input.x + forward * input.y;
            pos.x = Mathf.Clamp(pos.x, limitX.x, limitX.y);
            pos.z = Mathf.Clamp(pos.z, limitZ.x, limitZ.y);
            transform.position = pos;
        }

        private void Zoom(float delta)
            => cam.orthographicSize = Mathf.Clamp(cam.orthographicSize + delta, minSize, maxSize);
    }
}
