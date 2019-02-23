using UnityEngine;

namespace PofyTools
{
    public delegate void MilestoneDelegate(Milestone milestone);

    public class Milestone
    {
        private Range _currentRange;
        private Range _initialDistanceRange;
        private Range _distanceRange;

        //private float _initialDistance;

        #region Constructors
        /// <summary>
        /// Distance less than 0 is set to 0.
        /// </summary>
        /// <param name="distance"></param>
        /// <param name="startPoint"></param>
        public Milestone(Range distanceRange, float startPoint = 0f)
        {
            distanceRange.max = Mathf.Max(0, distanceRange.max);
            distanceRange.min = Mathf.Max(0, distanceRange.min);

            this._initialDistanceRange = this._distanceRange = distanceRange;
            this._currentRange = new Range(min: startPoint, max: this._initialDistanceRange.max + startPoint);
            //this._initialDistance = distance;
        }
        #endregion


        #region API

        public float CurrentMilestone => this._currentRange.max;

        public Range CurrentRange => this._currentRange;

        public bool MilestoneReached(float point)
        {
            bool result = point >= this._currentRange.max;
            return result;
        }

        public float DistanceFromMilestone(float point)
        {
            return this._currentRange.max - point;
        }

        public void ResetMilestone(float startPoint)
        {
            SetMilestone(this._distanceRange, startPoint);
        }

        public void SetMilestone(Range distanceRange, float startPoint)
        {
            distanceRange.max = Mathf.Max(0, distanceRange.max);
            distanceRange.min = Mathf.Max(0, distanceRange.min);

            this._distanceRange = distanceRange;

            this._currentRange = new Range(min: startPoint, max: startPoint + distanceRange.Random);
        }
        #endregion


        #region Event
        protected MilestoneDelegate _onEvent = null;

        protected void IdleEventListener(Milestone milestone)
        {
        }

        /// <summary>
        /// Adds the event listener.
        /// </summary>
        /// <param name="listener">Listener.</param>
        public void AddEventListener(MilestoneDelegate listener)
        {
            this._onEvent += listener;
        }

        /// <summary>
        /// Removes the event listener.
        /// </summary>
        /// <param name="listener">Listener.</param>
        public void RemoveEventListener(MilestoneDelegate listener)
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

        /// <summary>
        /// Tries the execute, firing event and reseting the cooldown.
        /// </summary>
        /// <returns><c>true</c>, if execution was successful, <c>false</c> otherwise.</returns>
        /// <param name="force">If set to <c>true</c> force execution.</param>
        public bool TryExecute(float point, bool force = false, bool autoReset = true)
        {
            if (MilestoneReached(point) || force)
            {
                FireEvent(point, autoReset);
                return true;
            }
            return false;
        }

        protected void FireEvent(float point, bool autoResetOnEvent = true)
        {
            this._onEvent?.Invoke(this);
            if (autoResetOnEvent)
                ResetMilestone(point);
            else
            {
                SetMilestone(new Range(float.PositiveInfinity, float.PositiveInfinity), point);
            }
        }



        #endregion
    }

}
