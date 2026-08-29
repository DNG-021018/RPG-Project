using UnityEngine;

namespace RPG.Combat.Character
{
    public interface ICharacterComponents
    {
        public void OnCoreInit(CharacterCore characterCore);
        public void OnCoreAwake();
        public void OnCoreEnable();
        public void OnCoreStart();
        public void OnCoreUpdate();
        public void OnCoreFixedUpdate();
        public void OnCoreLateUpdate();
        public void OnCoreDisable();
        public void OnCoreDestroy();
    }
}
