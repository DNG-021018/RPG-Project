using RPG.Combat.Character;

namespace RPG.Combat.Player.Input
{
    /// <summary>
    /// Placeholder cho nguồn input đến từ mạng. Class này cố tình KHÔNG phụ thuộc vào bất kỳ
    /// thư viện netcode cụ thể nào (Netcode for GameObjects, Mirror, Fish-Net...) - nó chỉ cần
    /// được "nạp" một PlayerInputData mỗi tick, bất kể dữ liệu đó đến bằng cách nào
    /// (một RPC, một NetworkVariable, một packet tự định nghĩa...).
    ///
    /// Luồng điển hình khi bạn thêm networking:
    ///   - Client sở hữu nhân vật vẫn chạy LocalPlayerInput như bình thường, và gửi
    ///     CurrentInput của nó lên server mỗi tick (qua Command/ServerRpc...).
    ///   - Server (và các client khác, nếu bạn replicate input thay vì chỉ replicate state)
    ///     nạp PlayerInputData nhận được vào một instance của class này qua ApplyReceivedInput.
    ///   - CharacterCore, movement, combat... vẫn đọc qua IPlayerInput y hệt như với
    ///     LocalPlayerInput - không cần rẽ nhánh gì ở phía dưới cả.
    /// </summary>
    public class NetworkPlayerInput : ICharacterComponents, IPlayerInput
    {
        public PlayerInputData CurrentInput { get; private set; }

        /// Gọi hàm này từ code nhận dữ liệu mạng (RPC handler, callback đồng bộ state, v.v.)
        /// khi bạn đã chọn được thư viện networking.
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