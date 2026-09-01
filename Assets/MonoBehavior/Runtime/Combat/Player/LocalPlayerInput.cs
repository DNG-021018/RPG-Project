using RPG.Combat.Character;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RPG.Combat.Player.Input
{
    public class LocalPlayerInput : ICharacterComponents, IPlayerInput, PlayerInputControls.IPlayerActions
    {
        public PlayerInputData CurrentInput => _currentInput;
        private PlayerInputData _currentInput;

        private PlayerInputControls _controls;
        private uint _tick;

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
            // _currentInput.CrouchHeld = context.ReadValueAsButton();
        }

        public void OnRoll(InputAction.CallbackContext context)
        {
            if (context.performed) _rollQueued = true;
        }
    }
}