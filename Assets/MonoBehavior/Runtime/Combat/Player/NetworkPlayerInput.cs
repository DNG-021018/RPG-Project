using RPG.Combat.Character;

namespace RPG.Combat.Player.Input
{
    public class NetworkPlayerInput : ICharacterComponents, IPlayerInput
    {
        public PlayerInputData CurrentInput { get; private set; }

        public void ApplyReceivedInput(PlayerInputData data)
        {
            CurrentInput = data;
        }

        public void OnCoreInit(CharacterCore characterCore) { }
        public void OnCoreAwake() { }
        public void OnCoreEnable() { }
        public void OnCoreStart() { }
        public void OnCoreUpdate() { }
        public void OnCoreFixedUpdate() { }
        public void OnCoreLateUpdate() { }
        public void OnCoreDisable() { }
        public void OnCoreDestroy() { }
    }
}