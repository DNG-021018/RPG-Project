using System;
using UnityEngine;

namespace RPG.Combat.Player.Input
{
    public interface IPlayerInput
    {
        PlayerInputData CurrentInput { get; }
    }

    [Serializable]
    public struct PlayerInputData
    {
        public Vector2 Move;
        public Vector2 Look;

        public bool JumpPressed;
        public bool RollPressed;
        public bool SprintHeld;
        public bool CrouchHeld;
        public uint Tick;
    }
}