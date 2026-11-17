using System;
using UnityEngine;
using RPG.Combat.Character;

namespace RPG
{
    using aniPlayer = Helpers.AnimatorKey.AnimatorParameter.Player;

    [Serializable]
    public class PlayerAnimatorController : CharacterAnimatorController
    {
        [SerializeField] private float blendDampTime = 0.15f;
        [HideInInspector] private PlayerMovement movement;

        private CharacterAnimationEvents animEvents;

        public override void OnCoreInit(CharacterCore characterCore)
        {
            base.OnCoreInit(characterCore);
            movement = cc.TryGetCore<PlayerMovement>();

            animEvents = animator.GetComponent<CharacterAnimationEvents>();
            if (animEvents == null)
            {
                animEvents = animator.gameObject.AddComponent<CharacterAnimationEvents>();
            }
        }

        public override void OnCoreEnable()
        {
            base.OnCoreEnable();

            animEvents.OnRollEnd += HandleRollAnimationEnd;
            animEvents.OnBackstepEnd += HandleBackstepAnimationEnd;
        }

        public override void OnCoreStart()
        {
            base.OnCoreStart();
        }

        public override void OnCoreUpdate()
        {
            base.OnCoreUpdate();
            if (animator == null || movement == null) return;

            Vector2 blend = movement.LocomotionBlend;
            animator.SetFloat(aniPlayer.BlendXHash, blend.x, blendDampTime, Time.deltaTime);
            animator.SetFloat(aniPlayer.BlendYHash, blend.y, blendDampTime, Time.deltaTime);
            animator.SetBool(aniPlayer.IsCouchHash, movement.IsCrouching);
            animator.SetBool(aniPlayer.IsRollingHash, movement.IsRolling);
            animator.SetBool(aniPlayer.IsBackstepHash, movement.IsBackstepping);
            animator.SetBool(aniPlayer.IsJumpHash, movement.IsJumping);
            animator.SetBool(aniPlayer.IsFallingHash, movement.IsFalling);
            animator.SetBool(aniPlayer.IsHeavyFallingHash, movement.JustLandedHeavy);
            animator.SetBool(aniPlayer.IsLightFallingHash, movement.JustLandedLight);
        }

        public override void OnCoreDisable()
        {
            base.OnCoreDisable();
            if (animEvents != null)
            {
                animEvents.OnRollEnd -= HandleRollAnimationEnd;
                animEvents.OnBackstepEnd -= HandleBackstepAnimationEnd;
            }
        }

        private void HandleRollAnimationEnd()
        {
            if (movement != null) movement.OnRollAnimationEnd();
        }

        private void HandleBackstepAnimationEnd()
        {
            if (movement != null) movement.OnBackstepAnimationEnd();
        }
    }
}
