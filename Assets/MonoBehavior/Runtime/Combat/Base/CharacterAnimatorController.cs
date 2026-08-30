using System;
using UnityEngine;

namespace RPG.Combat.Character
{
    /// <summary>
    /// Feed dữ liệu locomotion vào Animator Controller "One Hand":
    ///   - BlendX / BlendY: tham số của blend tree "2D Freeform Directional", dùng chung cho
    ///     cả state Main Locomotion lẫn Crouch Locomotion (cả 2 đọc cùng tên param nên chỉ cần
    ///     set 1 chỗ, Animator tự áp cho state nào đang active).
    ///   - isCouch: đúng tên (kể cả chính tả) tham số bool đã tạo trong Animator, điều khiển
    ///     transition Main Locomotion <-> Crouch Locomotion. KHÔNG đổi thành "isCrouch" vì
    ///     Animator.SetBool so tên chuỗi chính xác - đổi tên ở đây mà không đổi trong Animator
    ///     sẽ khiến crouch không bao giờ kích hoạt.
    ///
    /// LocomotionBlend từ PlayerMovement đã có magnitude đúng khớp bán kính blend tree (0 = idle,
    /// 0.5 = walk, 1 = run - xem PlayerMovement.OnCoreUpdate) nên feed thẳng, không cần quy đổi.
    ///
    /// Chỉ có tác dụng khi nhân vật có PlayerMovement (tức PlayerCore) - NPC dùng CharacterCore
    /// trần sẽ chưa có nguồn LocomotionBlend nên animator giữ nguyên giá trị mặc định, tới khi
    /// có AI movement riêng thì nên tách phần đọc dữ liệu này ra một interface chung (ví dụ
    /// ILocomotionSource) thay vì TryGetCore&lt;PlayerMovement&gt; thẳng như hiện tại.
    /// </summary>
    [Serializable]
    public class CharacterAnimatorController : ICharacterComponents
    {
        private static readonly int BlendXHash = Animator.StringToHash("BlendX");
        private static readonly int BlendYHash = Animator.StringToHash("BlendY");
        private static readonly int IsCouchHash = Animator.StringToHash("isCouch");

        [Header("Animator")]
        [Tooltip("Để trống thì tự GetComponentInChildren<Animator> lúc Start. Component này " +
                 "hiện được tạo bằng 'new' trong CharacterCore.SetupCores (không phải " +
                 "[SerializeReference]) nên field này chưa hiện ra Inspector được - để trống là ổn.")]
        [SerializeField] private Animator animator;

        [Tooltip("Thời gian làm mượt khi BlendX/BlendY đổi giá trị, giống damping trong Animator.")]
        [SerializeField] private float blendDampTime = 0.15f;

        private readonly Helpers.Logger logger = new();

        private CharacterCore cc;
        private PlayerMovement movement;

        public void OnCoreInit(CharacterCore characterCore)
        {
            cc = characterCore;
        }

        public void OnCoreStart()
        {
            if (animator == null) animator = cc.GetComponentInChildren<Animator>();
            if (animator == null)
            {
                logger.LogWarning(this, "Không tìm thấy Animator trên " + cc.name + " (hoặc con của nó) - sẽ không animate được.");
            }

            movement = cc.TryGetCore<PlayerMovement>();
        }

        public void OnCoreUpdate()
        {
            if (animator == null || movement == null) return;

            Vector2 blend = movement.LocomotionBlend;
            animator.SetFloat(BlendXHash, blend.x, blendDampTime, Time.deltaTime);
            animator.SetFloat(BlendYHash, blend.y, blendDampTime, Time.deltaTime);
            animator.SetBool(IsCouchHash, movement.IsCrouching);
        }

        public void OnCoreAwake() { }
        public void OnCoreEnable() { }
        public void OnCoreFixedUpdate() { }
        public void OnCoreLateUpdate() { }
        public void OnCoreDisable() { }
        public void OnCoreDestroy() { }
    }
}