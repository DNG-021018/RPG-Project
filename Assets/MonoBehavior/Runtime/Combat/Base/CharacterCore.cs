using System;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Combat.Character
{
    [RequireComponent(typeof(CharacterController))]
    public class CharacterCore : MonoBehaviour
    {
        public CharacterController Controller { get; private set; }

        private readonly List<ICharacterComponents> _cores = new();
        private readonly Dictionary<Type, ICharacterComponents> _coreDict = new();

        readonly Helpers.Logger logger = new();

        private void Awake()
        {
            Controller = GetComponent<CharacterController>();

            SetupCores();
            foreach (ICharacterComponents core in _cores)
            {
                core.OnCoreInit(this);
                core.OnCoreAwake();
            }
        }

        private void Register(ICharacterComponents core)
        {
            if (core == null) return;

            Type type = core.GetType();
            if (_coreDict.ContainsKey(type))
            {
                logger.LogError(this, "Core of type " + type.Name + " is already registered.");
                return;
            }

            _cores.Add(core);
            _coreDict[type] = core;
        }

        protected virtual void SetupCores()
        {
            Register(new CharacterHealth());
            Register(new CharacterMovement());
            Register(new CharacterCombat());
            Register(new CharacterAnimatorController());
        }

        public T TryGetCore<T>() where T : class, ICharacterComponents
        {
            if (_coreDict.TryGetValue(typeof(T), out var core))
            {
                return core as T;
            }

            return null;
        }

        void OnEnable()
        {
            foreach (ICharacterComponents characterCore in _cores)
            {
                characterCore.OnCoreEnable();
            }
        }

        void Start()
        {
            foreach (ICharacterComponents characterCore in _cores)
            {
                characterCore.OnCoreStart();
            }
        }

        void Update()
        {
            foreach (ICharacterComponents characterCore in _cores)
            {
                characterCore.OnCoreUpdate();
            }
        }

        void FixedUpdate()
        {
            foreach (ICharacterComponents characterCore in _cores)
            {
                characterCore.OnCoreFixedUpdate();
            }
        }

        void LateUpdate()
        {
            foreach (ICharacterComponents characterCore in _cores)
            {
                characterCore.OnCoreLateUpdate();
            }
        }

        void OnDisable()
        {
            foreach (ICharacterComponents characterCore in _cores)
            {
                characterCore.OnCoreDisable();
            }
        }

        void OnDestroy()
        {
            foreach (ICharacterComponents characterCore in _cores)
            {
                characterCore.OnCoreDestroy();
            }
        }
    }
}
