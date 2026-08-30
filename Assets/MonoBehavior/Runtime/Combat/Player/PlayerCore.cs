using UnityEngine;
using RPG.Combat.Player.Input;
using System;

namespace RPG.Combat.Character
{
    /// <summary>
    /// Ví dụ cách nhân vật do người chơi điều khiển chọn nguồn input của mình.
    /// Thay thế đoạn kiểm tra isLocalPlayer bên dưới bằng ownership check thật của thư viện
    /// networking bạn chọn sau này (ví dụ NetworkObject.IsOwner với Netcode for GameObjects,
    /// hoặc isLocalPlayer với Mirror) - không có chỗ nào khác trong dự án cần sửa, vì mọi nơi
    /// khác chỉ hỏi IPlayerInput, chưa bao giờ hỏi thẳng LocalPlayerInput.
    ///
    /// cameraLook (điều khiển Cinemachine + khóa/ẩn chuột) chỉ đăng ký cho nhân vật cục bộ vì
    /// camera là tài nguyên dùng chung của cả scene - nhân vật network không nên tự giành nó.
    /// </summary>
    public class PlayerCore : CharacterCore
    {
        [SerializeField] private bool isLocalPlayer = true;

        [SerializeReference] private LocalPlayerInput localPlayerInput = new();
        [SerializeReference] private PlayerMovement movement = new();
        [SerializeReference] private PlayerCameraLook cameraLook = new();

        protected override void SetupCores()
        {
            base.SetupCores();

            if (isLocalPlayer)
            {
                logger.Log(this, "Đây là nhân vật cục bộ, đăng ký LocalPlayerInput và PlayerCameraLook.");
                Register(localPlayerInput);
                Register(cameraLook);
            }
            else
            {
                Register(new NetworkPlayerInput());
            }

            Register(movement);
        }
    }
}
