using UnityEngine;
using RPG.Combat.Player.Input;
using System;

namespace RPG.Combat.Character
{
    public class PlayerCore : CharacterCore
    {
        [SerializeField] private bool isLocalPlayer = true;

        [SerializeReference] private LocalPlayerInput localPlayerInput = new();
        [SerializeReference] private PlayerMovement movement = new();
        [SerializeReference] private PlayerCameraLook cameraLook = new();
        [SerializeReference] private PlayerAnimatorController animatorController = new();
        [SerializeReference] private NetworkPlayerInput networkPlayerInput = new();

        protected override void SetupCores()
        {
            base.SetupCores();

            if (isLocalPlayer)
            {
                Register(localPlayerInput);
                Register(cameraLook);
            }
            else
            {
                Register(networkPlayerInput);
            }

            Register(movement);
            Register(animatorController);
        }
    }
}
