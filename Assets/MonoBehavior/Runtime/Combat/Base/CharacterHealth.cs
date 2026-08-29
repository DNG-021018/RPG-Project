using System;
using RPG.Combat.CombatType.Interface;
using RPG.Combat.CombatType.Struct;
using UnityEngine;

namespace RPG.Combat.Character
{
    [Serializable]
    public class CharacterHealth : ICharacterComponents, IDamageable
    {
        public bool IsDead => _isDead;
        private bool _isDead = false;
        CharacterCore cc;

        public void OnCoreInit(CharacterCore characterCore)
        {
            cc = characterCore;
        }

        public void OnCoreAwake()
        {

        }

        public void OnCoreFixedUpdate()
        {

        }

        public void OnCoreLateUpdate()
        {

        }

        public void OnCoreDestroy()
        {

        }

        public void OnCoreDisable()
        {

        }

        public void OnCoreEnable()
        {

        }

        public void OnCoreStart()
        {

        }

        public void TakeDamage(DamageInfo info)
        {

        }

        public void OnCoreUpdate()
        {

        }
    }
}
