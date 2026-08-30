using RPG.Combat.Character;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RPG.Combat.Player.Input
{
    /// <summary>
    /// Đọc phần cứng cục bộ (bàn phím/chuột/gamepad) qua PlayerInputControls đã generate,
    /// và biến nó thành một PlayerInputData snapshot mỗi tick. Đây là class DUY NHẤT trong dự
    /// án cần biết tới PlayerInputControls / Input System package - mọi hệ thống khác chỉ đọc
    /// IPlayerInput.CurrentInput.
    ///
    /// Chỉ nên được đăng ký cho nhân vật mà người chơi cục bộ thực sự điều khiển. Khi thêm
    /// networking, những nhân vật khác nên đăng ký một IPlayerInput implementation khác
    /// (ví dụ NetworkPlayerInput, được nạp dữ liệu từ state đồng bộ qua mạng) thay vì class
    /// này. Xem PlayerCore.SetupCores để biết quyết định đó được đưa ra ở đâu.
    /// </summary>
    public class LocalPlayerInput : ICharacterComponents, IPlayerInput, PlayerInputControls.IPlayerActions
    {
        public PlayerInputData CurrentInput => _currentInput;
        private PlayerInputData _currentInput;

        private PlayerInputControls _controls;
        private uint _tick;

        // Các action rời rạc kiểu "bấm 1 lần" được giữ lại (latch) ở đây cho tới khi
        // OnCoreUpdate tiêu thụ, để không bao giờ bị lỡ mất giữa lúc input event bắn ra
        // và lúc tick đọc nó.
        private bool _jumpQueued;
        private bool _rollQueued;

        public void OnCoreInit(CharacterCore characterCore)
        {
            _controls = new PlayerInputControls();
            _controls.Player.SetCallbacks(this);
        }

        public void OnCoreEnable() => _controls?.Enable();
        public void OnCoreDisable() => _controls?.Disable();
        public void OnCoreDestroy() => _controls?.Dispose();

        public void OnCoreUpdate()
        {
            _currentInput.JumpPressed = _jumpQueued;
            _currentInput.RollPressed = _rollQueued;
            _currentInput.Tick = _tick++;

            _jumpQueued = false;
            _rollQueued = false;
        }

        public void OnCoreAwake() { }
        public void OnCoreStart() { }
        public void OnCoreFixedUpdate() { }
        public void OnCoreLateUpdate() { }

        // ----- PlayerInputControls.IPlayerActions -----

        public void OnMove(InputAction.CallbackContext context)
        {
            _currentInput.Move = context.ReadValue<Vector2>();
        }

        public void OnLook(InputAction.CallbackContext context)
        {
            _currentInput.Look = context.ReadValue<Vector2>();
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if (context.performed) _jumpQueued = true;
        }

        public void OnSprint(InputAction.CallbackContext context)
        {
            _currentInput.SprintHeld = context.ReadValueAsButton();
        }

        public void OnCrouch(InputAction.CallbackContext context)
        {
            _currentInput.CrouchHeld = context.ReadValueAsButton();
        }

        public void OnRoll(InputAction.CallbackContext context)
        {
            if (context.performed) _rollQueued = true;
        }
    }
}