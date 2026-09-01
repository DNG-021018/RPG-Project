using System;
using RPG.Combat.Player.Input;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RPG.Combat.Character
{
    [Serializable]
    public class PlayerCameraLook : ICharacterComponents
    {
        [Header("Camera")]
        [Tooltip("Để trống thì tự FindObjectOfType<CinemachineCamera> lúc Start. Nên tự kéo " +
                 "vào cho chắc và đỡ tốn 1 lần tìm kiếm trong scene.")]
        [SerializeField] private CinemachineCamera virtualCamera;
        [SerializeField] private bool bindFollowLookAtOnStart = true;

        [Header("Sensitivity")]
        [Tooltip("Hệ số nhân TỔNG THỂ, nhân thêm lên trên scale đã đặt sẵn trong " +
                 "PlayerInputControls.inputactions (0.05 cho chuột, 300 cho gamepad). Để 1 " +
                 "nếu scale gốc đã vừa ý, chỉnh số này cho nhanh thay vì mở lại .inputactions.")]
        [SerializeField] private float yawSensitivity = 1f;
        [SerializeField] private float pitchSensitivity = 1f;
        [SerializeField] private bool invertY = false;

        [Header("Cursor")]
        [Tooltip("Khóa và ẩn con trỏ chuột trong suốt lúc nhân vật này active, để chuột " +
                 "không thoát ra khỏi cửa sổ game giữa chừng.")]
        [SerializeField] private bool lockCursorWhilePlaying = true;

        private readonly Helpers.Logger logger = new();

        private CharacterCore cc;
        private IPlayerInput input;
        private CinemachineOrbitalFollow orbitalFollow;
        private CinemachinePanTilt panTilt;

        public void OnCoreInit(CharacterCore characterCore)
        {
            cc = characterCore;
            input = cc.TryGetCore<IPlayerInput>();
        }

        public void OnCoreEnable()
        {
            SetCursorLocked(lockCursorWhilePlaying);
        }

        public void OnCoreStart()
        {
#if UNITY_2023_1_OR_NEWER
            if (virtualCamera == null) virtualCamera = UnityEngine.Object.FindFirstObjectByType<CinemachineCamera>();
#else
            if (virtualCamera == null) virtualCamera = UnityEngine.Object.FindObjectOfType<CinemachineCamera>();
#endif

            if (virtualCamera == null)
            {
                logger.LogWarning(this, "Không tìm thấy CinemachineCamera nào trong scene - look sẽ không hoạt động.");
                return;
            }

            orbitalFollow = virtualCamera.GetComponent<CinemachineOrbitalFollow>();
            panTilt = virtualCamera.GetComponent<CinemachinePanTilt>();

            if (orbitalFollow == null && panTilt == null)
            {
                logger.LogWarning(this, virtualCamera.name +
                    " không có CinemachineOrbitalFollow lẫn CinemachinePanTilt - không có axis nào để feed input look vào.");
            }

            if (bindFollowLookAtOnStart)
            {
                virtualCamera.Follow = cc.transform;
                virtualCamera.LookAt = cc.transform;
            }
        }

        public void OnCoreUpdate()
        {
            TryReclaimCursor();

            if (input == null) return;

            Vector2 look = input.CurrentInput.Look;
            if (look.sqrMagnitude <= 0f) return;

            float yawDelta = look.x * yawSensitivity;
            float pitchDelta = (invertY ? -look.y : look.y) * pitchSensitivity;

            if (orbitalFollow != null)
            {
                ApplyDelta(ref orbitalFollow.HorizontalAxis, yawDelta);
                ApplyDelta(ref orbitalFollow.VerticalAxis, pitchDelta);
            }

            if (panTilt != null)
            {
                ApplyDelta(ref panTilt.PanAxis, yawDelta);
                ApplyDelta(ref panTilt.TiltAxis, pitchDelta);
            }
        }

        public void OnCoreDisable()
        {
            SetCursorLocked(false);
        }

        public void OnCoreAwake() { }
        public void OnCoreFixedUpdate() { }
        public void OnCoreLateUpdate() { }
        public void OnCoreDestroy() { }

        private static void ApplyDelta(ref InputAxis axis, float delta)
        {
            axis.Value = axis.ClampValue(axis.Value + delta);
        }

        private void SetCursorLocked(bool locked)
        {
            if (!lockCursorWhilePlaying) return;
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }

        private void TryReclaimCursor()
        {
            if (!lockCursorWhilePlaying || Cursor.lockState == CursorLockMode.Locked) return;
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                SetCursorLocked(true);
            }
        }
    }
}
