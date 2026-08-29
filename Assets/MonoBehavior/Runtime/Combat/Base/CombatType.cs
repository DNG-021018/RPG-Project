using RPG.Combat.CombatType.Enums;
using RPG.Combat.CombatType.Struct;
using UnityEngine;

namespace RPG.Combat.CombatType
{
    #region Interfaces
    namespace Interface
    {
        public interface IDamageable
        {
            bool IsDead { get; }
            void TakeDamage(DamageInfo info);
        }

        public interface IKnockbackable
        {
            void Knockback(Vector3 direction, float force);
        }

        public interface IRiposteable
        {
            void Riposte(HitReactionType reactionType);
        }

        // public interface IFinisherBehaviour
        // {
        //     void PlayFinisher(CharacterCore attacker, CharacterCore target);
        // }
    }
    #endregion

    #region Structs
    namespace Struct
    {
        public struct DamageInfo
        {
            public float Amount;
            public Vector3 HitPoint;
            public Vector3 HitDirection;
            public GameObject Source;
            public bool IsCritical;
            public HitReactionType ReactionType;
        }
    }
    #endregion

    #region Enums
    namespace Enums
    {
        public enum AttackType
        {
            Light,
            Heavy,
            Critical,
            // Finisher,
        }

        public enum HitReactionType
        {
            Light,
            Heavy,
            Critical,
            Knockback,
            // Finisher
        }
    }
    #endregion
}
