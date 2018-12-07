using UnityEngine;

namespace PofyTools
{
    public delegate void TimerDelegate(Timer timer);

    public class Timer : IInitializable
    {
        protected float _timerDuration;
        protected float _initialTimerDuration;
        protected float _nextTimestamp;

        protected string _id;
        public string Id
        {
            get { return this._id; }
        }

        #region Constructors
        public Timer(string id) : this(id, 0f) { }

        public Timer(string id, float countDownDuration)
        {
            this._id = id;
            this._timerDuration = countDownDuration;
            this._initialTimerDuration = countDownDuration;
        }

        #endregion

        #region Event
        protected TimerDelegate _onEvent = null;

        protected void IdleEventListener(Timer timer)
        {
        }

        /// <summary>
        /// Adds the event listener.
        /// </summary>
        /// <param name="listener">Listener.</param>
        public void AddEventListener(TimerDelegate listener)
        {
            this._onEvent += listener;
        }

        /// <summary>
        /// Removes the event listener.
        /// </summary>
        /// <param name="listener">Listener.</param>
        public void RemoveEventListener(TimerDelegate listener)
        {
            this._onEvent -= listener;
        }

        /// <summary>
        /// Removes all event listeners.
        /// </summary>
        public void RemoveAllEventListeners()
        {
            this._onEvent = IdleEventListener;
        }
        #endregion

        #region Initialize
        /// <summary>
        /// Gets a value indicating whether this <see cref="PofyTools.Timer"/> is initialized.
        /// </summary>
        /// <value><c>true</c> if is initialized; otherwise, <c>false</c>.</value>
        public bool IsInitialized
        {
            get;
            protected set;
        }

        /// <summary>
        /// Initialize this instance.
        /// </summary>

        public virtual bool Initialize()
        {
            if (!this.IsInitialized)
            {
                this._onEvent = IdleEventListener;
                this.IsInitialized = true;
                return true;
            }

            return false;
        }

        #endregion

        #region Count Up

        private float _counterTimestamp;

        public void StartCounter()
        {
            this._counterTimestamp = Time.time;
        }

        /// <summary>
        /// Time in seconds since counter started via StartCounter().
        /// </summary>
        public float Counter
        {
            get
            {
                return Time.time - this._counterTimestamp;
            }
        }

        #endregion

        #region Count Down
        public void SetTimer(float duration)
        {
            this._timerDuration = duration;
            this._nextTimestamp = Time.time + duration;
        }

        /// <summary>
        /// Gets the next timestamp in seconds.
        /// </summary>
        /// <value>The next timestamp in seconds.</value>
        public float NextTimestamp
        {
            get
            {
                return this._nextTimestamp;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="PofyTools.Timer"/> has reached next timestamp.
        /// </summary>
        /// <value><c>true</c> if is ready; otherwise, <c>false</c>.</value>
        public bool IsReady
        {
            get
            {
                return (Time.time >= this._nextTimestamp);
            }
        }

        public void SetReady()
        {
            this._nextTimestamp = -1;
        }

        /// <summary>
        /// Gets the time left for cooldown in seconds. Time left is difference between next timestamp and Time.time maxed at 0.
        /// </summary>
        /// <value>The time left.</value>
        public float TimeLeft
        {
            get
            {
                return Mathf.Max(0, this._nextTimestamp - Time.time);
            }
        }

        /// <summary>
        /// Gets current normalized time between timer started and timer should end
        /// </summary>
        public float NormalizedTime
        {
            get
            {
                if (this.IsReady)
                    return 1f;//clamp                
                return Mathf.InverseLerp(this.TimerStarted, this._nextTimestamp, Time.time);
                //return (Time.time - TimerStarted) / (this._nextTimestamp - TimerStarted);
            }
        }

        /// <summary>
        /// Get thetime the timer started timing
        /// </summary>
        public float TimerStarted { get { return this._nextTimestamp - this._timerDuration; } }



        /// <summary>
        /// Tries the execute, firing event and reseting the cooldown.
        /// </summary>
        /// <returns><c>true</c>, if execution was successful, <c>false</c> otherwise.</returns>
        /// <param name="force">If set to <c>true</c> force execution.</param>
        public bool TryExecute(bool force = false, bool autoReset = true)
        {
            if (this.IsReady || force)
            {
                FireEvent(autoReset);
                return true;
            }
            return false;
        }

        protected void FireEvent(bool autoResetOnEvent = true)
        {
            this._onEvent?.Invoke(this);
            if (autoResetOnEvent)
                ResetTimer();
            else
            {
                this._nextTimestamp = float.PositiveInfinity;
            }
        }

        public virtual void ResetTimer()
        {
            SetTimer(this._timerDuration);
        }

        #endregion

    }
}
