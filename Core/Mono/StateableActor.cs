using System.Collections.Generic;
using UnityEngine;

namespace PofyTools
{

    public abstract class StateableActor : MonoBehaviour, IStateable, ISubscribable, IInitializable, ITransformable
    {

        #region Variables

#if UNITY_EDITOR

        private List<string> _stateList = new List<string>();
#endif

        private List<IState> _stateStack;

        #endregion

        #region IInitializable

        protected bool _isInitialized = false;

        public virtual bool Initialize()
        {
            if (!this.IsInitialized)
            {
                ConstructAvailableStates();
                this._stateStack = new List<IState>();

                this._isInitialized = !this._hasLateInitialize;
                return true;
            }
            return false;
        }

        public virtual bool IsInitialized
        {
            get
            {
                return this._isInitialized;
            }
        }

        protected bool _hasLateInitialize = false;

        public virtual bool LateInitialize()
        {
            if (!this.IsInitialized)
            {
                this._isInitialized = true;
                return true;
            }
            return false;
        }

        #endregion

        #region ISubscribable

        protected bool _isSubscribed = false;

        public virtual bool Subscribe()
        {
            if (!this.IsSubscribed)
            {
                this._isSubscribed = true;
                return true;
            }
            return false;
        }

        public virtual bool Unsubscribe()
        {
            if (this.IsSubscribed)
            {
                this._isSubscribed = false;
                return true;
            }
            return false;
        }

        public bool IsSubscribed
        {
            get
            {
                return this._isSubscribed;
            }

        }

        #endregion

        #region IStateable
        private bool _sort = false;
        public void AddState(IState state)
        {
            if (state == null)
            {
                if (this._stateStack.Count == 0)
                {
                    //this.enabled = false;
                    Debug.LogError("<size=24> " + this.name + ": Trying to add a NULL State Object!</size>");
                }
                return;
            }

            if (!state.IgnoreStacking)
                RemoveState(state);

            else if (this._stateStack.Contains(state))
            {
                //Debug.LogWarning(this.name + ": State already active! Ignoring...");
                return;
            }

            state.EnterState();

            if (state.HasUpdate)
            {
                this._stateStack.Add(state);
                //this._stateStack.Sort((x, y) => x.Priority.CompareTo(y.Priority));
                this._sort = true;
                //this.enabled = true;
            }
            else
                state.ExitState();
        }

        public void RemoveState(IState state)
        {
            if (state == null)
            {
                Debug.LogError("<size=24> " + this.name + ": Trying to add a NULL State Object!</size>");
                return;
            }

            if (!state.IsActive)
            {
                //Debug.LogWarningFormat("{0}: State {1} inactive!", this.name, state.GetType().ToString());
                return;
            }

            state.Deactivate();
            if (this._stateStack.Contains(state))
            {
                state.ExitState();
            }
        }

        public void RemoveAllStates(bool endPermanent = false, int priority = 0)
        {
            int count = this._stateStack.Count;
            IState state = null;
            for (int i = count - 1; i >= 0; --i)
            {
                state = this._stateStack[i];
                if (state.IsActive && (!state.IsPermanent || endPermanent))
                {
                    if (state.Priority >= priority)
                    {
                        //this._stateStack.RemoveAt(i);
                        state.ExitState();
                    }
                }
            }
        }

        public void PurgeStateStack()
        {
            foreach (var state in this._stateStack)
            {
                state.Deactivate();
            }

            this._stateStack.Clear();
        }

        //public void SetToState(IState state)
        //{
        //    RemoveAllStates();
        //    if (state != null)
        //        AddState(state);
        //}


        #endregion

        #region Mono

        protected virtual void Awake()
        {
            Initialize();
        }

        // Use this for initialization
        protected virtual void Start()
        {
            LateInitialize();
            Subscribe();
        }

        // Update is called once per frame
        protected virtual void Update()
        {

#if UNITY_EDITOR

            this._stateList.Clear();
            foreach (var logstate in this._stateStack)
            {
                this._stateList.Add(logstate.ToString() + ":" + logstate.IsActive);
            }
#endif

            IState state = null;

            for (int i = this._stateStack.Count - 1; i >= 0 && i < this._stateStack.Count; --i)
            {

                state = this._stateStack[i];

                if (state.IsActive && state.UpdateState())
                {
                    state.ExitState();
                }
            }
        }

        protected virtual void FixedUpdate()
        {
            IState state = null;

            for (int i = this._stateStack.Count - 1; i >= 0 && i < this._stateStack.Count; --i)
            {
                state = this._stateStack[i];

                if (state.IsActive && state.FixedUpdateState())
                {
                    //this._stateStack.RemoveAt(i);
                    state.ExitState();

                }
            }
        }

        protected virtual void LateUpdate()
        {
            IState state = null;

            for (int i = this._stateStack.Count - 1; i >= 0 && i < this._stateStack.Count; --i)
            {
                state = this._stateStack[i];

                if (state.IsActive && state.LateUpdateState())
                {
                    //this._stateStack.RemoveAt(i);
                    state.ExitState();
                }
            }

            RemoveInactiveStates();
        }

        private void RemoveInactiveStates()
        {
            var removal = this._stateStack.RemoveAll(x => !x.IsActive) > 0;
            if (this._sort)
            {
                this._stateStack.Sort((x, y) => x.Priority.CompareTo(y.Priority));
                this._sort = false;
            }

        }

        protected virtual void OnDestroy()
        {
            PurgeStateStack();
            Unsubscribe();
        }

        #endregion

        #region States

        public abstract void ConstructAvailableStates();

        #endregion

        #region IList

        public int IndexOf(IState state)
        {
            return this._stateStack.IndexOf(state);
        }

        public IState this[int index]
        {
            get { return this._stateStack[index]; }
            set { this._stateStack[index] = value; }
        }

        //public bool Contains(IState item) { return this._stateStack.Contains(item); }
        //public int Count { get { return this._stateStack.Count; } }
        //public IEnumerator<IState> GetEnumerator() { return this._stateStack.GetEnumerator(); }

        #endregion
    }

    public class StateObject<T> : IState where T : IStateable
    {
        public T ControlledObject
        {
            get;
            protected set;
        }

        public bool HasUpdate
        {
            get;
            protected set;
        }

        public bool IsInitialized
        {
            get;
            protected set;
        }

        public bool IsActive
        {
            get;
            protected set;
        }

        public void Deactivate()
        {
            this.IsActive = false;
        }

        public bool IgnoreStacking
        {
            get;
            protected set;
        }

        public bool IsPermanent
        {
            get;
            protected set;
        }

        public int Priority
        {
            get; set;
        }

        #region Constructor

        public StateObject()
        {
        }

        public StateObject(T controlledObject)
        {
            this.ControlledObject = controlledObject;
            InitializeState();
        }

        #endregion

        #region IState implementation

        public virtual void InitializeState()
        {
            this.IsInitialized = true;
            if (this[0] == null)
            {
                Debug.LogError(ToString() + " has no controlled object");
            }
        }

        public virtual void EnterState()
        {
            this.IsActive = true;
            //this.onEnter (this);
        }

        public virtual bool UpdateState()
        {
            //return true on exit condition
            return false;
        }

        public virtual bool FixedUpdateState()
        {
            //do fixed stuff
            return false;
        }

        public virtual bool LateUpdateState()
        {
            //do late state
            return false;
        }

        public virtual void ExitState()
        {

            this.IsActive = false;
            //this.onExit (this);
        }

        #endregion

        public T this[int arg]
        {
            get
            {
                return this.ControlledObject;
            }
        }
    }

    public class TimedStateObject<T> : StateObject<T> where T : IStateable
    {
        protected Range _timeRange;
        protected AnimationCurve _curve;

        public TimedStateObject(T controlledObject, Range timeRange, AnimationCurve curve)
        {
            this.ControlledObject = controlledObject;
            this._timeRange = timeRange;
            this._curve = curve;

            InitializeState();
        }

        public TimedStateObject(T controlledObject, float duration, AnimationCurve curve)
            : this(controlledObject, new Range(duration), curve)
        {
        }

        public TimedStateObject(T controlledObject, float duration)
            : this(controlledObject, new Range(duration), null)
        {
        }

        public TimedStateObject(T controlledObject)
            : this(controlledObject, new Range(1), null)
        {
        }

        public virtual void InitializeState(float duration, AnimationCurve curve)
        {
            this.HasUpdate = true;

            base.InitializeState();
        }

        public override void InitializeState()
        {
            this.HasUpdate = true;
            base.InitializeState();
        }
    }

    public class TimerStateObject<T> : StateObject<T> where T : IStateable
    {
        public Timer timer = null;

        public TimerStateObject(T controlledObject, float timerDuration) : this(controlledObject, new Timer("timer", timerDuration))
        {
        }

        public TimerStateObject(T controlledObject, Timer timer) : base(controlledObject)
        {
            this.timer = timer;
        }

        public override void InitializeState()
        {
            this.HasUpdate = true;
            base.InitializeState();
        }
    }

    #region Utility States
    public class BackButtonListenerState : StateObject<IStateable>
    {
        public UpdateDelegate onBackButton;

        public BackButtonListenerState(IStateable controlledObject)
            : base(controlledObject)
        {
        }

        public void VoidIdle()
        {
        }

        public override void InitializeState()
        {
            this.onBackButton = VoidIdle;
            this.HasUpdate = true;
            base.InitializeState();
        }

        public override bool LateUpdateState()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                this.onBackButton.Invoke();
            }
            return false;
        }

    }

    public class DelegateStack : StateObject<IStateable>
    {
        public UpdateDelegate updater;

        void VoidIdle()
        {
        }

        public DelegateStack(IStateable controlledObject)
            : base(controlledObject)
        {
        }

        public override void InitializeState()
        {
            this.HasUpdate = true;
            this.updater = VoidIdle;
        }

        public override bool UpdateState()
        {
            this.updater();
            return false;
        }
    }

    #endregion

    public interface IState
    {
        void InitializeState();

        void EnterState();

        bool UpdateState();

        bool FixedUpdateState();

        bool LateUpdateState();

        void ExitState();

        bool HasUpdate { get; }

        bool IsPermanent { get; }

        int Priority { get; }

        bool IgnoreStacking { get; }

        bool IsActive
        {
            get;
        }

        void Deactivate();
    }

    public interface IStateable
    {
        void ConstructAvailableStates();

        void AddState(IState state);

        void RemoveState(IState state);

        void RemoveAllStates(bool endPermanent = false, int priority = 0);

        void PurgeStateStack();

    }

    public delegate void IStateDelegate(IState state);

    public abstract class BaseStateable : IStateable
    {
        private List<IState> _stateStack = new List<IState>();

        public abstract void ConstructAvailableStates();

        #region IStateable

        public void AddState(IState state)
        {
            if (state == null)
                return;

            if (!this._stateStack.Contains(state))
            {
                state.EnterState();

                if (state.HasUpdate)
                {
                    this._stateStack.Add(state);
                    this._stateStack.Sort((x, y) => x.Priority.CompareTo(y.Priority));
                    return;
                }

                state.ExitState();
            }
        }

        public void RemoveState(IState state)
        {
            if (this._stateStack.Remove(state))
            {
                state.ExitState();
            }
        }

        public void RemoveAllStates(bool endPermanent = false, int priority = 0)
        {
            if (this._stateStack != null)
            {
                int count = this._stateStack.Count;
                IState state = null;
                for (int i = count - 1; i >= 0; --i)
                {
                    state = this._stateStack[i];
                    if (!state.IsPermanent || endPermanent)
                    {
                        if (state.Priority >= priority)
                        {
                            this._stateStack.RemoveAt(i);
                            state.ExitState();
                        }
                    }
                }
            }
        }

        public void PurgeStateStack()
        {
            if (this._stateStack != null)
            {
                foreach (var state in this._stateStack)
                {
                    state.Deactivate();
                }
                this._stateStack.Clear();
            }
        }

        public void SetToState(IState state)
        {
            RemoveAllStates();
            if (state != null)
                AddState(state);
        }

        #endregion

        #region Mono
        // Update is called once per frame
        public virtual void Update()
        {
            IState state = null;

            for (int i = this._stateStack.Count - 1; i >= 0 && i < this._stateStack.Count; --i)
            {

                state = this._stateStack[i];

                if (state.UpdateState())
                {
                    this._stateStack.RemoveAt(i);
                    state.ExitState();
                }
            }
        }

        public virtual void FixedUpdate()
        {
            IState state = null;

            for (int i = this._stateStack.Count - 1; i >= 0 && i < this._stateStack.Count; --i)
            {
                state = this._stateStack[i];

                if (state.FixedUpdateState())
                {
                    this._stateStack.RemoveAt(i);
                    state.ExitState();

                }
            }
        }

        public virtual void LateUpdate()
        {
            IState state = null;

            for (int i = this._stateStack.Count - 1; i >= 0 && i < this._stateStack.Count; --i)
            {
                state = this._stateStack[i];

                if (state.LateUpdateState())
                {
                    this._stateStack.RemoveAt(i);
                    state.ExitState();
                }
            }
        }

        #endregion
    }
}