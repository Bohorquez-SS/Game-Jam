using UnityEngine;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
#endif

namespace GNight
{
    /// <summary>Entrada unificada: Input Manager clásico o Input System nuevo.</summary>
    public static class Inp
    {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        static Keyboard K => Keyboard.current;
        static Mouse M => Mouse.current;
        static bool Down(KeyControl k) => k != null && k.wasPressedThisFrame;
        static bool Held(KeyControl k) => k != null && k.isPressed;
        public static Vector2 Move()
        {
            if (K == null) return Vector2.zero;
            return new Vector2((Held(K.dKey) ? 1 : 0) - (Held(K.aKey) ? 1 : 0), (Held(K.wKey) ? 1 : 0) - (Held(K.sKey) ? 1 : 0));
        }
        public static Vector2 Look() => M == null ? Vector2.zero : M.delta.ReadValue() * 0.05f;
        public static bool Run() => K != null && Held(K.leftShiftKey);
        public static bool Crouch() => K != null && (Held(K.leftCtrlKey) || Held(K.cKey));
        public static bool Interact() => K != null && Down(K.eKey);
        public static bool Phone() => K != null && Down(K.tabKey);
        public static bool Esc() => K != null && Down(K.escapeKey);
        public static bool Flashlight() => K != null && Down(K.fKey);
        public static bool Back() => K != null && Down(K.backspaceKey);
        public static bool Enter() => K != null && (Down(K.enterKey) || Down(K.numpadEnterKey));
        public static bool Left() => K != null && Down(K.leftArrowKey);
        public static bool Right() => K != null && Down(K.rightArrowKey);
        public static int Digit()
        {
            if (K == null) return -1;
            KeyControl[] row = { K.digit0Key, K.digit1Key, K.digit2Key, K.digit3Key, K.digit4Key, K.digit5Key, K.digit6Key, K.digit7Key, K.digit8Key, K.digit9Key };
            KeyControl[] pad = { K.numpad0Key, K.numpad1Key, K.numpad2Key, K.numpad3Key, K.numpad4Key, K.numpad5Key, K.numpad6Key, K.numpad7Key, K.numpad8Key, K.numpad9Key };
            for (int i = 0; i < 10; i++) if (Down(row[i]) || Down(pad[i])) return i;
            return -1;
        }
        public static Vector2 MousePos() => M == null ? Vector2.zero : M.position.ReadValue();
        public static bool Click() => M != null && M.leftButton.wasPressedThisFrame;
        public static bool ClickHeld() => M != null && M.leftButton.isPressed;
        public static bool RightClick() => M != null && M.rightButton.wasPressedThisFrame;
        public static bool RightHeld() => M != null && M.rightButton.isPressed;
        public static float Wheel() => M == null ? 0f : Mathf.Clamp(M.scroll.ReadValue().y / 120f, -1f, 1f);
        public static bool Q() => K != null && Down(K.qKey);
        public static bool Space() => K != null && Down(K.spaceKey);
#else
        public static Vector2 Move()
        {
            return new Vector2((Input.GetKey(KeyCode.D) ? 1 : 0) - (Input.GetKey(KeyCode.A) ? 1 : 0),
                               (Input.GetKey(KeyCode.W) ? 1 : 0) - (Input.GetKey(KeyCode.S) ? 1 : 0));
        }
        public static Vector2 Look() => new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
        public static bool Run() => Input.GetKey(KeyCode.LeftShift);
        public static bool Crouch() => Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.C);
        public static bool Interact() => Input.GetKeyDown(KeyCode.E);
        public static bool Phone() => Input.GetKeyDown(KeyCode.Tab);
        public static bool Esc() => Input.GetKeyDown(KeyCode.Escape);
        public static bool Flashlight() => Input.GetKeyDown(KeyCode.F);
        public static bool Back() => Input.GetKeyDown(KeyCode.Backspace);
        public static bool Enter() => Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter);
        public static bool Left() => Input.GetKeyDown(KeyCode.LeftArrow);
        public static bool Right() => Input.GetKeyDown(KeyCode.RightArrow);
        public static int Digit()
        {
            for (int i = 0; i < 10; i++)
                if (Input.GetKeyDown(KeyCode.Alpha0 + i) || Input.GetKeyDown(KeyCode.Keypad0 + i)) return i;
            return -1;
        }
        public static Vector2 MousePos() => Input.mousePosition;
        public static bool Click() => Input.GetMouseButtonDown(0);
        public static bool ClickHeld() => Input.GetMouseButton(0);
        public static bool RightClick() => Input.GetMouseButtonDown(1);
        public static bool RightHeld() => Input.GetMouseButton(1);
        public static float Wheel() => Input.mouseScrollDelta.y;
        public static bool Q() => Input.GetKeyDown(KeyCode.Q);
        public static bool Space() => Input.GetKeyDown(KeyCode.Space);
#endif
    }
}
