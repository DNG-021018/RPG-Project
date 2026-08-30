using System;
using RPG.Combat.Player.Input;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RPG.Combat.Character
{
    /// <summary>
    /// Điều khiển "look" bằng Cinemachine 3: mỗi frame đọc Look từ IPlayerInput rồi cộng dồn
    /// vào các InputAxis tương ứng trên CinemachineCamera. Bản thân các component vị trí/xoay
    /// của Cinemachine (OrbitalFollow, PanTilt...) KHÔNG tự đọc input - theo docs, chúng "does
    /// not read user input itself" và cần được "driven by ... some other means that you
    /// devise" - nên feed input theo đường IPlayerInput như thế này là đúng ý đồ package,
    /// đồng thời vẫn giữ nguyên tắc "chỉ LocalPlayerInput biết tới thiết bị input thật" của
    /// dự án (xem LocalPlayerInput.cs).
    ///
    /// Tự nhận diện 1 trong 2 kiểu rig phổ biến của Cinemachine 3, gắn cái nào trên
    /// CinemachineCamera thì dùng cái đó (có thể dùng cả 2 cùng lúc nếu bạn ghép rig lạ,
    /// nhưng bình thường chỉ có 1):
    ///   - CinemachineOrbitalFollow (Body): camera orbit quanh nhân vật, HorizontalAxis/
    ///     VerticalAxis chính là góc nhìn. Để Aim trống ("Do nothing") vì OrbitalFollow + có
    ///     LookAt sẽ tự hard-look-at nhân vật. Hợp với kiểu action-camera orbit (Genshin,
    ///     Dark Souls...), khớp với cách PlayerMovement đang dùng cameraTransform.forward.
    ///   - CinemachinePanTilt (Aim): dùng khi Body là CinemachineThirdPersonFollow (rig kiểu
    ///     đứng trên vai nhân vật) - PanAxis/TiltAxis xoay thẳng camera theo input.
    ///
    /// Chỉ nên đăng ký cho nhân vật cục bộ (xem PlayerCore.SetupCores): camera là tài nguyên
    /// dùng chung của cả scene, nhân vật điều khiển từ network không nên tự giành nó.
    /// </summary>
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

            // KHÔNG nhân Time.deltaTime ở đây: Look đã được scale sẵn theo từng thiết bị ngay
            // trong .inputactions - delta chuột theo pixel (đã độc lập frame rate) x0.05, còn
            // giá trị analog stick (một "tốc độ", cần giữ qua nhiều frame để tiếp tục xoay)
            // x300. Nhân thêm deltaTime ở đây sẽ làm sai cả 2 (chuột xoay chậm theo frame rate,
            // gamepad xoay quá nhanh/không đúng vì bị nhân deltaTime 2 lần về mặt ý nghĩa).
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
            // ClampValue tôn trọng Range/Wrap bạn đã cấu hình sẵn trên component trong
            // Inspector (ví dụ giới hạn góc tilt -30..70 độ), không cần hardcode lại ở đây.
            axis.Value = axis.ClampValue(axis.Value + delta);
        }

        private void SetCursorLocked(bool locked)
        {
            if (!lockCursorWhilePlaying) return;
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }

        // Chuột có thể bị nhả khóa ngoài ý muốn (cửa sổ mất focus, Alt-Tab, một số nền tảng tự
        // nhả khi bấm Esc...). Bấm chuột trái lại trong lúc đang chơi sẽ khóa/ẩn lại ngay thay
        // vì bắt người chơi thoát/vào lại game.
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
