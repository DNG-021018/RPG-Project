using System;
using RPG.Combat.Player.Input;
using UnityEngine;

namespace RPG.Combat.Character
{
    /// <summary>
    /// Điều khiển nhân vật bằng CharacterController (không dùng Collider/Rigidbody).
    /// Đổi từ MonoBehaviour sang plain class implement ICharacterComponents, giống
    /// CharacterHealth/CharacterCombat, để chạy chung vòng đời (Update, FixedUpdate...) do
    /// CharacterCore điều phối thay vì tự quản lý GetComponent/Update riêng.
    ///
    /// Toàn bộ logic chạy trong OnCoreUpdate (không phải OnCoreFixedUpdate): CharacterController
    /// không nằm trong physics simulation của Rigidbody nên gọi Move() theo Time.deltaTime ở
    /// Update là chuẩn, đồng thời cùng pha với LocalPlayerInput (cũng đọc input ở Update) nên
    /// không lệch/nuốt input giữa 2 core.
    ///
    /// Kiểu facing: soulslike free-camera - camera (PlayerCameraLook) orbit độc lập quanh nhân
    /// vật theo input Look, còn nhân vật chỉ xoay mặt theo HƯỚNG DI CHUYỂN (không phải hướng
    /// camera). Đứng yên thì giữ nguyên hướng mặt hiện tại, không tự xoay theo camera. Khi
    /// thêm lock-on target sau này, chỉ cần đổi hướng target của Slerp bên dưới (moveDirWorld)
    /// thành hướng tới mục tiêu bị khóa, và có thể bật lại kiểu blend strafe (dùng hướng di
    /// chuyển trong local-space của facing-tới-target) cho lúc đang khóa mục tiêu.
    /// </summary>
    [Serializable]
    public class PlayerMovement : ICharacterComponents
    {
        [Header("Speeds")]
        [SerializeField] private float walkSpeed = 2.5f;
        [SerializeField] private float runSpeed = 5f;
        [SerializeField] private float crouchSpeedMultiplier = 0.5f;
        [SerializeField] private float rotationSpeed = 12f;

        [Header("Vertical")]
        [SerializeField] private float gravity = -20f;
        [SerializeField] private float jumpHeight = 1.2f;

        [Header("Roll")]
        [SerializeField] private float rollSpeed = 7f;
        [SerializeField] private float rollDuration = 0.4f;

        private CharacterCore cc;
        private IPlayerInput input;
        private Transform selfTransform;
        private Transform cameraTransform;

        private float verticalVelocity;
        private bool isRolling;
        private float rollTimer;
        private Vector3 rollDirection;

        /// Hướng di chuyển trong không gian cục bộ của nhân vật, nhân với hệ số tốc độ
        /// (0 = đứng yên, 0.5 = đi bộ, 1 = chạy) - feed thẳng vào BlendX/BlendY của Animator.
        /// Khớp với 2 vòng bán kính (0.5 / 1.0) trong blend tree "2D Freeform Directional".
        /// Vì nhân vật luôn xoay để hướng theo moveDirWorld (xem OnCoreUpdate), giá trị này khi
        /// đang chạy ổn định sẽ hội tụ về gần (0, speedScale) - tức gần như luôn "forward" -
        /// chỉ lệch nhẹ sang X trong vài frame đang xoay (turn lean), đúng cảm giác soulslike.
        public Vector2 LocomotionBlend { get; private set; }
        public bool IsGrounded { get; private set; }
        public bool IsRolling => isRolling;
        public bool IsCrouching { get; private set; }

        public void OnCoreInit(CharacterCore characterCore)
        {
            cc = characterCore;
            selfTransform = cc.transform;
            input = cc.TryGetCore<IPlayerInput>();
        }

        public void OnCoreStart()
        {
            if (Camera.main != null) cameraTransform = Camera.main.transform;
        }

        public void OnCoreUpdate()
        {
            if (input == null || cc.Controller == null) return;
            if (cameraTransform == null && Camera.main != null) cameraTransform = Camera.main.transform;

            PlayerInputData data = input.CurrentInput;
            IsGrounded = cc.Controller.isGrounded;
            IsCrouching = data.CrouchHeld;

            if (isRolling)
            {
                TickRoll();
                return;
            }

            if (data.RollPressed && IsGrounded)
            {
                StartRoll(data);
                return;
            }

            Vector3 moveDirWorld = CameraRelativeDirection(data.Move);
            float speed = data.SprintHeld ? runSpeed : walkSpeed;
            if (data.CrouchHeld) speed *= crouchSpeedMultiplier;

            // Xoay nhân vật theo hướng ĐANG DI CHUYỂN, không theo camera. Camera (PlayerCameraLook)
            // free-look độc lập quanh nhân vật; đứng yên (không có input di chuyển) thì giữ
            // nguyên hướng mặt hiện tại thay vì tự xoay theo camera.
            if (moveDirWorld.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(moveDirWorld, Vector3.up);
                selfTransform.rotation = Quaternion.Slerp(selfTransform.rotation, targetRot, rotationSpeed * Time.deltaTime);
            }

            ApplyGravity();
            if (data.JumpPressed && IsGrounded)
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }

            Vector3 motion = moveDirWorld * speed + Vector3.up * verticalVelocity;
            cc.Controller.Move(motion * Time.deltaTime);

            float speedScale = moveDirWorld.sqrMagnitude < 0.0001f ? 0f : (data.SprintHeld ? 1f : 0.5f);
            Vector3 localDir = moveDirWorld.sqrMagnitude > 0.0001f
                ? selfTransform.InverseTransformDirection(moveDirWorld.normalized)
                : Vector3.zero;
            LocomotionBlend = new Vector2(localDir.x, localDir.z) * speedScale;
        }

        public void OnCoreAwake() { }
        public void OnCoreEnable() { }
        public void OnCoreFixedUpdate() { }
        public void OnCoreLateUpdate() { }
        public void OnCoreDisable() { }
        public void OnCoreDestroy() { }

        private Vector3 CameraRelativeDirection(Vector2 rawMove)
        {
            Vector3 fwd = cameraTransform != null ? FlattenY(cameraTransform.forward) : selfTransform.forward;
            Vector3 right = cameraTransform != null ? FlattenY(cameraTransform.right) : selfTransform.right;
            Vector3 dir = fwd * rawMove.y + right * rawMove.x;
            return dir.sqrMagnitude > 1f ? dir.normalized : dir;
        }

        private static Vector3 FlattenY(Vector3 v)
        {
            v.y = 0f;
            return v.sqrMagnitude > 0.0001f ? v.normalized : Vector3.zero;
        }

        private void ApplyGravity()
        {
            if (IsGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = -2f; // ép nhẹ xuống đất để isGrounded ổn định
            }
            verticalVelocity += gravity * Time.deltaTime;
        }

        private void StartRoll(PlayerInputData data)
        {
            Vector3 dir = data.Move.sqrMagnitude > 0.01f ? CameraRelativeDirection(data.Move) : selfTransform.forward;
            rollDirection = dir.sqrMagnitude > 0.0001f ? dir.normalized : selfTransform.forward;
            isRolling = true;
            rollTimer = rollDuration;
        }

        private void TickRoll()
        {
            ApplyGravity();
            Vector3 motion = rollDirection * rollSpeed + Vector3.up * verticalVelocity;
            cc.Controller.Move(motion * Time.deltaTime);

            rollTimer -= Time.deltaTime;
            if (rollTimer <= 0f) isRolling = false;
        }
    }
}