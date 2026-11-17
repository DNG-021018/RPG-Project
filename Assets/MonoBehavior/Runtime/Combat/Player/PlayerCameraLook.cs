using System;
using Unity.Cinemachine;
using UnityEngine;

namespace RPG.Combat.Character
{
    [Serializable]
    public class PlayerCameraLook : ICharacterComponents
    {
        [Header("Camera")]
        [SerializeField] private CinemachineCamera virtualCamera;
        [SerializeField] private bool bindFollowLookAtOnStart = true;

        private readonly Helpers.Logger logger = new();

        private CharacterCore cc;

        public void OnCoreInit(CharacterCore characterCore)
        {
            cc = characterCore;
        }

        public void OnCoreEnable() { }

        public void OnCoreStart()
        {
#if UNITY_EDITOR && UNITY_2023_1_OR_NEWER
            if (virtualCamera == null) virtualCamera = UnityEngine.Object.FindFirstObjectByType<CinemachineCamera>();
            if (virtualCamera == null) virtualCamera = UnityEngine.Object.FindFirstObjectByType<CinemachineCamera>();
#else
            if (virtualCamera == null) virtualCamera = UnityEngine.Object.FindObjectOfType<CinemachineCamera>();
#endif

            if (virtualCamera == null)
            {
                logger.LogWarning(this, "Không tìm thấy CinemachineCamera nào trong scene - không thể gán Follow/LookAt.");
                return;
            }

            if (bindFollowLookAtOnStart)
            {
                virtualCamera.Follow = cc.transform;
                virtualCamera.LookAt = cc.transform;
            }
        }

        public void OnCoreUpdate() { }

        public void OnCoreDisable() { }

        public void OnCoreAwake() { }
        public void OnCoreFixedUpdate() { }
        public void OnCoreLateUpdate() { }
        public void OnCoreDestroy() { }
    }
}
