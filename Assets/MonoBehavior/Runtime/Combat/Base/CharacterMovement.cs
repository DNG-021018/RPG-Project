using System;
using UnityEngine;

namespace RPG.Combat.Character
{
    [Serializable]
    public class CharacterMovement : ICharacterComponents
    {
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

        public void OnCoreUpdate()
        {
        }
    }
}
