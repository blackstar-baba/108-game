using UnityEngine;

namespace Liangshan
{
    /// <summary>Keyboard and mouse input. The project intentionally uses the classic Input Manager.</summary>
    public static class InputReader
    {
        public static bool Enabled = true;
        public static float MouseSensitivity = 1f;

        public static Vector2 Move
        {
            get
            {
                if (!Enabled) return Vector2.zero;
                float x = Input.GetAxisRaw("Horizontal");
                float y = Input.GetAxisRaw("Vertical");
                Vector2 move = new Vector2(x, y);
                return move.sqrMagnitude > 1f ? move.normalized : move;
            }
        }

        public static Vector2 MouseDelta
        {
            get
            {
                if (!Enabled) return Vector2.zero;
                return new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")) * MouseSensitivity;
            }
        }

        public static float Scroll => Enabled ? Input.mouseScrollDelta.y : 0f;
        public static bool AttackDown => Enabled && Input.GetMouseButtonDown(0);
        public static bool AttackHeld => Enabled && Input.GetMouseButton(0);
        public static bool HeavyDown => Enabled && Input.GetMouseButtonDown(1);
        public static bool DodgeDown => Enabled && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.LeftShift));
        public static bool SkillDown => Enabled && Input.GetKeyDown(KeyCode.E);
        public static bool UltimateDown => Enabled && Input.GetKeyDown(KeyCode.R);
        public static bool PauseDown => Input.GetKeyDown(KeyCode.Escape);
        public static bool CodexDown => Input.GetKeyDown(KeyCode.Tab);
        public static bool LockOnDown => Enabled && Input.GetKeyDown(KeyCode.Tab);
        public static bool AnyNumberDown(out int number)
        {
            for (int i = 0; i < 9; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i) || Input.GetKeyDown(KeyCode.Keypad1 + i))
                {
                    number = i + 1;
                    return true;
                }
            }
            number = 0;
            return false;
        }

        public static bool ConfirmDown => Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0);
    }
}
