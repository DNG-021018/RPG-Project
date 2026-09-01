using System;
using RPG.Combat.Player.Input;
using UnityEngine;

namespace RPG.Combat.Character
{
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
        [SerializeField] private float heavyLandingImpactSpeed = 10f;

        [Header("Roll / Backstep")]
        [SerializeField] private float rollSpeed = 7f;
        [SerializeField] private float sprintRollSpeedMultiplier = 1.3f;
        [SerializeField] private float backstepSpeed = 4f;
        [SerializeField] private float rollDuration = 0.4f;
        [SerializeField] private float backstepDuration = 0.35f;
        [SerializeField] private float rollCooldown = 0.5f;
        [SerializeField] private float moveInputThreshold = 0.1f;

        private CharacterCore cc;
        private IPlayerInput input;
        private Transform selfTransform;
        private Transform cameraTransform;

        private readonly Helpers.Logger logger = new();

        private float verticalVelocity;
        private bool isRolling;
        private float rollTimer;
        private float currentRollSpeed;
        private bool isBackstepping;
        private float backstepTimer;
        private float rollCooldownTimer;
        private Vector3 rollDirection;
        private bool wasGrounded;

        public Vector2 LocomotionBlend { get; private set; }
        public bool IsGrounded { get; private set; }
        public bool IsRolling => isRolling;
        public bool IsBackstepping => isBackstepping;
        public bool IsRollOnCooldown => rollCooldownTimer > 0f;
        public bool CanRoll => !isRolling && !isBackstepping && rollCooldownTimer <= 0f;
        public bool IsCrouching { get; private set; }

        public bool IsJumping { get; private set; }
        public bool IsFalling { get; private set; }
        public bool JustLandedHeavy { get; private set; }
        public bool JustLandedLight { get; private set; }

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

            if (rollCooldownTimer > 0f) rollCooldownTimer -= Time.deltaTime;

            PlayerInputData data = input.CurrentInput;
            IsGrounded = cc.Controller.isGrounded;
            /// IsCrouching = data.CrouchHeld;

            JustLandedHeavy = false;
            JustLandedLight = false;

            if (!wasGrounded && IsGrounded)
            {
                float impactSpeed = Mathf.Abs(verticalVelocity);
                if (impactSpeed >= heavyLandingImpactSpeed) JustLandedHeavy = true;
                else JustLandedLight = true;
            }
            wasGrounded = IsGrounded;

            if (isRolling)
            {
                TickRoll();
                return;
            }

            if (isBackstepping)
            {
                TickBackstep();
                return;
            }

            if (data.RollPressed && IsGrounded && CanRoll)
            {
                if (HasMoveInput(data.Move)) StartRoll(data);
                else StartBackstep();
                return;
            }

            Vector3 moveDirWorld = CameraRelativeDirection(data.Move);
            float speed = data.SprintHeld ? runSpeed : walkSpeed;
            if (data.CrouchHeld) speed *= crouchSpeedMultiplier;
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

            IsJumping = !IsGrounded && verticalVelocity > 0f;
            IsFalling = !IsGrounded && verticalVelocity <= 0f;
        }

        public void OnCoreAwake() { }
        public void OnCoreEnable() { }
        public void OnCoreFixedUpdate() { }
        public void OnCoreLateUpdate() { }
        public void OnCoreDisable() { }
        public void OnCoreDestroy() { }

        private bool HasMoveInput(Vector2 move) => move.sqrMagnitude > moveInputThreshold * moveInputThreshold;

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
                verticalVelocity = -2f;
            }
            verticalVelocity += gravity * Time.deltaTime;
        }

        private void StartRoll(PlayerInputData data)
        {
            Vector3 dir = CameraRelativeDirection(data.Move);
            rollDirection = dir.sqrMagnitude > 0.0001f ? dir.normalized : selfTransform.forward;
            isRolling = true;
            rollTimer = rollDuration;
            currentRollSpeed = data.SprintHeld ? rollSpeed * sprintRollSpeedMultiplier : rollSpeed;
        }

        private void TickRoll()
        {
            ApplyGravity();
            Vector3 motion = rollDirection * currentRollSpeed + Vector3.up * verticalVelocity;
            cc.Controller.Move(motion * Time.deltaTime);

            rollTimer -= Time.deltaTime;
            if (rollTimer <= 0f)
            {
                EndRoll();
            }
        }

        private void StartBackstep()
        {
            isBackstepping = true;
            backstepTimer = backstepDuration;
        }

        private void TickBackstep()
        {
            ApplyGravity();
            Vector3 motion = -selfTransform.forward * backstepSpeed + Vector3.up * verticalVelocity;
            cc.Controller.Move(motion * Time.deltaTime);

            backstepTimer -= Time.deltaTime;
            if (backstepTimer <= 0f)
            {
                EndBackstep();
            }
        }

        public void OnRollAnimationEnd()
        {
            if (!isRolling) return;
            EndRoll();
        }

        public void OnBackstepAnimationEnd()
        {
            if (!isBackstepping) return;
            EndBackstep();
        }

        private void EndRoll()
        {
            isRolling = false;
            rollCooldownTimer = rollCooldown;
        }

        private void EndBackstep()
        {
            isBackstepping = false;
            rollCooldownTimer = rollCooldown;
        }
    }
}
