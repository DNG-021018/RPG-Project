using System;
using UnityEngine;

namespace RPG.Combat.Character
{
    [Serializable]
    public abstract class CharacterAnimatorController : ICharacterComponents
    {
        [Header("Animator")]
        [SerializeField] protected Animator animator;

        protected CharacterCore cc;

        protected readonly Helpers.Logger logger = new();

        public virtual void OnCoreInit(CharacterCore characterCore)
        {
            cc = characterCore;
        }

        public virtual void OnCoreStart()
        {
            if (animator == null) animator = cc.GetComponentInChildren<Animator>();
        }

        public virtual void OnCoreAwake() { }
        public virtual void OnCoreUpdate() { }
        public virtual void OnCoreEnable() { }
        public virtual void OnCoreFixedUpdate() { }
        public virtual void OnCoreLateUpdate() { }
        public virtual void OnCoreDisable() { }
        public virtual void OnCoreDestroy() { }
    }
}