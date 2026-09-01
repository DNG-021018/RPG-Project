using RPG.Combat.Character;
using UnityEngine;

namespace RPG
{
    public class PlayerAnimatorController : CharacterAnimatorController
    {
        private static readonly int BlendXHash = Animator.StringToHash("BlendX");
        private static readonly int BlendYHash = Animator.StringToHash("BlendY");
        private static readonly int IsCouchHash = Animator.StringToHash("isCouch");
        private static readonly int IsRollingHash = Animator.StringToHash("isRolling");
        private static readonly int IsBackstepHash = Animator.StringToHash("isBackstep");
        private static readonly int IsJumpHash = Animator.StringToHash("isJump");
        private static readonly int IsFallingHash = Animator.StringToHash("isFalling");
        private static readonly int IsHeavyFallingHash = Animator.StringToHash("isHeavyFalling");
        private static readonly int IsLightFallingHash = Animator.StringToHash("isLightFalling");

        [SerializeField] private float blendDampTime = 0.15f;
        [HideInInspector] private PlayerMovement movement;

        private CharacterAnimationEvents animEvents;

        public override void OnCoreInit(CharacterCore characterCore)
        {
            base.OnCoreInit(characterCore);
            movement = cc.TryGetCore<PlayerMovement>();
            if (movement == null)
            {
                logger.LogWarning(this, "Không tìm thấy PlayerMovement trên " + cc.name + " - sẽ không animate được.");
            }
        }

        public override void OnCoreStart()
        {
            base.OnCoreStart();

            if (animator == null) return;

            animEvents = animator.GetComponent<CharacterAnimationEvents>();
            if (animEvents == null) animEvents = animator.gameObject.AddComponent<CharacterAnimationEvents>();
            animEvents.OnRollEnd += HandleRollAnimationEnd;
            animEvents.OnBackstepEnd += HandleBackstepAnimationEnd;
        }

        public override void OnCoreUpdate()
        {
            base.OnCoreUpdate();
            if (animator == null || movement == null) return;

            Vector2 blend = movement.LocomotionBlend;
            animator.SetFloat(BlendXHash, blend.x, blendDampTime, Time.deltaTime);
            animator.SetFloat(BlendYHash, blend.y, blendDampTime, Time.deltaTime);
            animator.SetBool(IsCouchHash, movement.IsCrouching);
            animator.SetBool(IsRollingHash, movement.IsRolling);
            animator.SetBool(IsBackstepHash, movement.IsBackstepping);
            animator.SetBool(IsJumpHash, movement.IsJumping);
            animator.SetBool(IsFallingHash, movement.IsFalling);
            animator.SetBool(IsHeavyFallingHash, movement.JustLandedHeavy);
            animator.SetBool(IsLightFallingHash, movement.JustLandedLight);
        }

        public override void OnCoreDestroy()
        {
            base.OnCoreDestroy();
            if (animEvents != null)
            {
                animEvents.OnRollEnd -= HandleRollAnimationEnd;
                animEvents.OnBackstepEnd -= HandleBackstepAnimationEnd;
            }
        }

        private void HandleRollAnimationEnd()
        {
            movement?.OnRollAnimationEnd();
        }

        private void HandleBackstepAnimationEnd()
        {
            movement?.OnBackstepAnimationEnd();
        }
    }
}