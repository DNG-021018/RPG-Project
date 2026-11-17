using UnityEngine;

namespace RPG.Helpers.AnimatorKey
{
    public static class AnimatorParameter
    {
        public class Player
        {
            public static readonly int BlendXHash = Animator.StringToHash("BlendX");
            public static readonly int BlendYHash = Animator.StringToHash("BlendY");
            public static readonly int IsCouchHash = Animator.StringToHash("isCouch");
            public static readonly int IsRollingHash = Animator.StringToHash("isRolling");
            public static readonly int IsBackstepHash = Animator.StringToHash("isBackstep");
            public static readonly int IsJumpHash = Animator.StringToHash("isJump");
            public static readonly int IsFallingHash = Animator.StringToHash("isFalling");
            public static readonly int IsHeavyFallingHash = Animator.StringToHash("isHeavyFalling");
            public static readonly int IsLightFallingHash = Animator.StringToHash("isLightFalling");
        }
    }
}
