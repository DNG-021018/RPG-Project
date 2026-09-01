using System;
using UnityEngine;

namespace RPG.Combat.Character
{
    public class CharacterAnimationEvents : MonoBehaviour
    {
        public event Action OnRollEnd;
        public event Action OnBackstepEnd;

        public void AnimEvent_RollEnd()
        {
            OnRollEnd?.Invoke();
        }

        public void AnimEvent_BackstepEnd()
        {
            OnBackstepEnd?.Invoke();
        }
    }
}