using System;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Combat.Character
{
    [RequireComponent(typeof(CharacterController))]
    public class CharacterCore : MonoBehaviour
    {
        public CharacterController Controller { get; private set; }

        [SerializeReference] private readonly List<ICharacterComponents> _cores = new();
        private readonly Dictionary<Type, ICharacterComponents> _coreDict = new();

        protected readonly Helpers.Logger logger = new();

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

        protected void Register(ICharacterComponents core)
        {
            if (core == null) return;

            Type concreteType = core.GetType();
            if (_coreDict.ContainsKey(concreteType))
            {
                logger.LogWarning(this, "Core of type " + concreteType.Name + " is already registered.");
                return;
            }

            _cores.Add(core);
            _coreDict[concreteType] = core;

            foreach (Type interfaceType in concreteType.GetInterfaces())
            {
                if (interfaceType == typeof(ICharacterComponents)) continue;
                if (_coreDict.ContainsKey(interfaceType)) continue;
                _coreDict[interfaceType] = core;
            }
        }

        protected virtual void SetupCores()
        {
            Register(new CharacterHealth());
            Register(new CharacterCombat());
            Register(new CharacterAnimatorController());
        }

        public T TryGetCore<T>() where T : class
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