using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace PofyTools
{
    public abstract class StateComponent : MonoBehaviour, IState
    {
        #region Stateable Actor
        [SerializeField]
        protected StateableActor _controlledObject;
        public StateableActor ControlledObject { get { return this._controlledObject; } }

        public StateableActor this[int arg]
        {
            get
            {
                return this._controlledObject;
            }
        }
        #endregion

        #region IState

        [SerializeField]        protected bool _hasUpdate;
        public bool HasUpdate { get { return this._hasUpdate; } }

        [SerializeField]        protected bool _isPermanent;
        public bool IsPermanent { get { return this._isPermanent; } }

        [SerializeField]        protected int _priority;
        public int Priority { get { return this._priority; } }

        [SerializeField] protected bool _ignoreStacking;
        public bool RequiresReactivation { get { return this._ignoreStacking; } }

        [SerializeField]        protected bool _isActive;
        public bool IsActive { get { return this._isActive; } }

        [SerializeField]        protected bool _isInitialized;
        public bool IsInitialized { get { return this._isInitialized; } }

        public void InitializeState(StateableActor actor)
        {
            this._controlledObject = actor;

            InitializeState();
        }

        public void InitializeState()
        {
            this._isInitialized = true;

            if (this[0] == null)
                Debug.LogError(this.ToString() + " has no controlled object");
        }

        public void EnterState()
        {
            this._isActive = true;
        }

        public virtual bool UpdateState() { return false; }

        public virtual bool FixedUpdateState() { return false; }

        public virtual bool LateUpdateState() { return false; }

        public virtual void Deactivate() { this._isActive = false; }

        public virtual void ExitState() { this._isActive = false; }

        #endregion
    }
}